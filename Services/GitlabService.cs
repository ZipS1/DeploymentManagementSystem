using System;
using System.Text;
using System.Text.Json;
using DeploymentManagementSystem.Data;
using DeploymentManagementSystem.Services.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DeploymentManagementSystem.Services
{
    public class GitlabService
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;
        private readonly ILogger<GitlabService> _logger;

        public GitlabService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<GitlabService> logger)
        {
            this.dbFactory = dbFactory;
            _logger = logger;
        }

        public async Task<bool> InitializeGitlabConnection(int projectId, string userPAT)
        {
            try
            {
                using var context = dbFactory.CreateDbContext();
                var project = await context.Projects.SingleOrDefaultAsync(p => p.Id == projectId);
                if (project == null)
                {
                    return false;
                }

                var dto = await FetchProjectInfo(project.GitlabUrl!, userPAT);
                project.GitlabProjectId = dto.ProjectId;
                project.GitlabDefaultBranch = dto.DefaultBranch;

                project.IsGitlabConnected = true;
                context.Update(project);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize GitLab connection for projectId={ProjectId}", projectId);
                return false;
            }
        }

        private async Task<GitlabProjectDTO> FetchProjectInfo(string url, string userPAT)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                throw new ArgumentException("Invalid URL", nameof(url));

            var projectPath = uri.AbsolutePath.Trim('/');
            if (string.IsNullOrWhiteSpace(projectPath))
                throw new ArgumentException("URL does not contain a valid project path", nameof(url));

            var encodedPath = Uri.EscapeDataString(projectPath);
            var apiUrl = $"{uri.Scheme}://{uri.Host}/api/v4/projects/{encodedPath}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", userPAT);

            var response = await client.GetAsync(apiUrl);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"GitLab API returned {(int)response.StatusCode}: {response.ReasonPhrase}");

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("id", out var idElement))
                throw new InvalidOperationException("GitLab API response does not contain 'id' property");

            if (!doc.RootElement.TryGetProperty("default_branch", out var defaultBranch))
                throw new InvalidOperationException("GitLab API response does not contain 'id' property");

            return new GitlabProjectDTO() { ProjectId = idElement.GetInt32(), DefaultBranch = defaultBranch.GetString() };
        }

        public async Task<GitlabTaskDTO> CreateBranchAndMRForTask(GitlabProjectDTO projectDTO, string taskRefName, string userPAT)
        {
            if (!Uri.TryCreate(projectDTO.Url, UriKind.Absolute, out var uri))
                throw new ArgumentException("Invalid URL", nameof(projectDTO.Url));

            var gitlabApiBase = $"{uri.Scheme}://{uri.Host}/api/v4";
            var projectId = projectDTO.ProjectId;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", userPAT);

            // 1. Create the branch
            var branchUrl = $"{gitlabApiBase}/projects/{projectId}/repository/branches?branch={Uri.EscapeDataString(taskRefName)}&ref={Uri.EscapeDataString(projectDTO.DefaultBranch)}";
            var branchResponse = await client.PostAsync(branchUrl, null);
            branchResponse.EnsureSuccessStatusCode();

            // 2. Create the merge request
            var mrUrl = $"{gitlabApiBase}/projects/{projectId}/merge_requests";
            var mrPayload = new
            {
                source_branch = taskRefName,
                target_branch = projectDTO.DefaultBranch,
                title = $"Automated MR for {taskRefName}"
            };
            var mrContent = new StringContent(JsonSerializer.Serialize(mrPayload), Encoding.UTF8, "application/json");
            var mrResponse = await client.PostAsync(mrUrl, mrContent);
            mrResponse.EnsureSuccessStatusCode();

            var json = await mrResponse.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("iid", out var iidElement))
                throw new InvalidOperationException("GitLab API response does not contain 'id' property");

            return new GitlabTaskDTO { MergeRequestID = iidElement.GetInt32() };
        }

        public async Task<bool> MergeMRForTask(GitlabTaskDTO taskDTO, GitlabProjectDTO projectDTO, string userPAT)
        {
            if (!Uri.TryCreate(projectDTO.Url, UriKind.Absolute, out var uri))
                throw new ArgumentException("Invalid URL", nameof(projectDTO.Url));

            var gitlabApiBase = $"{uri.Scheme}://{uri.Host}/api/v4";
            var projectId = projectDTO.ProjectId;
            var mergeRequestId = taskDTO.MergeRequestID;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", userPAT);

            var mergeUrl = $"{gitlabApiBase}/projects/{projectId}/merge_requests/{mergeRequestId}/merge";

            var mergePayload = new
            {
                auto_merge = true,
            };
            var mergeContent = new StringContent(JsonSerializer.Serialize(mergePayload), Encoding.UTF8, "application/json");

            var response = await client.PutAsync(mergeUrl, mergeContent);

            if (response.IsSuccessStatusCode)
                return true;

            return false;
        }
    }
}
