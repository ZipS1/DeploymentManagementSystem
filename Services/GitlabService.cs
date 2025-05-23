using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using DeploymentManagementSystem.Data;
using DeploymentManagementSystem.Data.DomainStringConstants;
using DeploymentManagementSystem.Services.DTOs;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace DeploymentManagementSystem.Services
{
    public class GitlabService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<GitlabService> _logger;
        const int REQUEST_DELAY_SECONDS = 5;

        public GitlabService(IServiceScopeFactory scopeFactory, ILogger<GitlabService> logger)
        {
            this._scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<bool> InitializeGitlabConnection(int projectId, string userPAT)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
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

        public async Task<bool> MergeMRForTask(GitlabTaskDTO taskDTO, GitlabProjectDTO projectDTO, string userPAT, CancellationToken cancellationToken = default)
        {
            if (!Uri.TryCreate(projectDTO.Url, UriKind.Absolute, out var uri))
                throw new ArgumentException("Invalid URL", nameof(projectDTO.Url));

            var gitlabApiBase = $"{uri.Scheme}://{uri.Host}/api/v4";
            var projectId = projectDTO.ProjectId;
            var mergeRequestId = taskDTO.MergeRequestID;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", userPAT);

            var mrSucceed = await WaitUntilMRPipelineFinished(projectDTO, taskDTO, userPAT, cancellationToken);
            if (!mrSucceed)
                return false;

            var mergeUrl = $"{gitlabApiBase}/projects/{projectId}/merge_requests/{mergeRequestId}/merge";

            var mergePayload = new
            {
                auto_merge = true,
                should_remove_source_branch = true,
            };
            var mergeContent = new StringContent(JsonSerializer.Serialize(mergePayload), Encoding.UTF8, "application/json");

            var response = await client.PutAsync(mergeUrl, mergeContent);

            if (response.IsSuccessStatusCode)
                return true;

            return false;
        }

        public async Task<bool> WaitUntilMRPipelineFinished(GitlabProjectDTO projectDTO, GitlabTaskDTO taskDTO, string userPAT, CancellationToken cancellationToken = default)
        {
            if (!Uri.TryCreate(projectDTO.Url, UriKind.Absolute, out var uri))
                throw new ArgumentException("Invalid URL", nameof(projectDTO.Url));

            var gitlabApiBase = $"{uri.Scheme}://{uri.Host}/api/v4";
            var projectId = projectDTO.ProjectId;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", userPAT);

            var url = $"{gitlabApiBase}/projects/{projectId}/pipelines?ref={taskDTO.BranchNameEncoded}&per_page=1";
            var response = await client.GetAsync(url, cancellationToken);
            _logger.LogInformation($"Wait until pipeline finished pipeline response status: {response.StatusCode}");
            _logger.LogInformation($"Task DTO branch name '{taskDTO.BranchNameEncoded}' | url: '{url}'");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            string status;
            using (var doc = JsonDocument.Parse(json))
            {
                if (!doc.RootElement[0].TryGetProperty("status", out var statusElement))
                    throw new InvalidOperationException("GitLab API response does not contain 'status' property");

                status = statusElement.ToString();
            }

            while (status != PipelineStatusConstants.Success && status != PipelineStatusConstants.Failed)
            {
                _logger.LogInformation($"Waiting for merge request pipeline to finish...");
                await Task.Delay(REQUEST_DELAY_SECONDS * 1000, cancellationToken);
                response = await client.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync(cancellationToken);

                using (var doc1 = JsonDocument.Parse(json))
                {
                    if (!doc1.RootElement[0].TryGetProperty("status", out var statusElement))
                        throw new InvalidOperationException("GitLab API response does not contain 'status' property");

                    status = statusElement.ToString();
                }
            }

            return status == PipelineStatusConstants.Success;
        }

        public async Task<bool> UpdateDeploymentStatus(GitlabProjectDTO projectDTO, int taskId, string userPAT, CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Delay(REQUEST_DELAY_SECONDS * 1000, cancellationToken);
                var dto = await GetDefaultBranchPipelineStatus(projectDTO, userPAT, cancellationToken);
                _logger.LogInformation($"dto info: {dto.Status} | {dto.CommitSHA}");
                while (dto.Status == PipelineStatusConstants.Running || dto.Status == PipelineStatusConstants.Pending)
                {
                    await Task.Delay(REQUEST_DELAY_SECONDS * 1000, cancellationToken);
                    dto = await GetDefaultBranchPipelineStatus(projectDTO, userPAT, cancellationToken);
                    _logger.LogInformation($"in-loop dto info: {dto.Status} | {dto.CommitSHA}");
                }

                if (dto.Status == PipelineStatusConstants.Success)
                {
                    await SetStatusForTask(taskId, TaskStatusConstants.SuccessfullyDeployed, cancellationToken);

                } else
                {
                    await HandlePipelineFailure(projectDTO, dto.CommitSHA, taskId, userPAT, cancellationToken);
                }

                return true;
            }
            catch (Exception ex)
            {
                await SetStatusForTask(taskId, TaskStatusConstants.DeploymentError);
                _logger.LogError(ex, "Failed to track deployment status of Task.Id={taskId}", taskId);
                return false;
            }
        }

        private async Task SetStatusForTask(int taskId, string status, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Run a set status '{status}' job for task.Id {taskId}");
            using var scope = _scopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var task = await context.Tasks.SingleOrDefaultAsync(t => t.Id == taskId, cancellationToken);
            task!.TaskStatusId = (await context.TaskStatuses.SingleOrDefaultAsync(s => s.Name == status, cancellationToken))!.Id;
            await context.SaveChangesAsync(cancellationToken);
        }

        private async Task HandlePipelineFailure(GitlabProjectDTO projectDTO, string mergeCommitSha, int taskId, string userPAT, CancellationToken cancellationToken = default)
        {
            await SetStatusForTask(taskId, TaskStatusConstants.DeploymentError, cancellationToken);
            await RevertMergeCommit(projectDTO, mergeCommitSha, taskId, userPAT, cancellationToken);
        }

        private async Task RevertMergeCommit(GitlabProjectDTO projectDTO, string mergeCommitSha, int taskId, string userPAT, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Reverting merge commit of task.Id={taskId}");
            if (!Uri.TryCreate(projectDTO.Url, UriKind.Absolute, out var uri))
                throw new ArgumentException("Invalid URL", nameof(projectDTO.Url));

            var gitlabApiBase = $"{uri.Scheme}://{uri.Host}/api/v4";
            var projectId = projectDTO.ProjectId;
            var targetBranch = projectDTO.DefaultBranch;

            using var scope = _scopeFactory.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var task = await dbContext.Set<Data.Models.Task>()
                .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
            if (task == null)
                throw new InvalidOperationException($"Task with ID {taskId} not found.");

            if (string.IsNullOrWhiteSpace(task.RefName))
                throw new InvalidOperationException("Task.RefName must be set to create a branch for the revert.");

            var revertBranch = task.RefName;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", userPAT);

            // Step 1: Create the branch if it doesn't exist
            await CreateBranchIfNotExists(client, gitlabApiBase, projectId, revertBranch, targetBranch!, cancellationToken);
            _logger.LogInformation($"Branch created successfully for reverting a task.Id={taskId}. Performing a revert...");

            // Step 2: Perform the revert with correct API parameters
            var revertUrl = $"{gitlabApiBase}/projects/{projectId}/repository/commits/{mergeCommitSha}/revert";
            var revertContent = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("branch", revertBranch)
    });

            var revertResponse = await client.PostAsync(revertUrl, revertContent, cancellationToken);
            if (!revertResponse.IsSuccessStatusCode)
            {
                var error = await revertResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to revert commit: {error}");
            }

            // Step 3: Create merge request manually
            _logger.LogInformation($"Creating a new merge request for reverted task.Id={taskId}");
            var mergeRequestId = await CreateMergeRequest(client, gitlabApiBase, projectId, revertBranch, targetBranch!, mergeCommitSha, cancellationToken);

            task.MergeRequestId = mergeRequestId;
            dbContext.Update(task);
            await dbContext.SaveChangesAsync();
        }

        private async Task CreateBranchIfNotExists(HttpClient client, string gitlabApiBase, int projectId, string branchName, string sourceBranch, CancellationToken cancellationToken)
        {
            // Check if branch exists
            var branchUrl = $"{gitlabApiBase}/projects/{projectId}/repository/branches/{Uri.EscapeDataString(branchName)}";
            var branchResponse = await client.GetAsync(branchUrl, cancellationToken);

            if (branchResponse.StatusCode == HttpStatusCode.NotFound)
            {
                // Create branch
                var createBranchUrl = $"{gitlabApiBase}/projects/{projectId}/repository/branches";
                var createContent = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("branch", branchName),
            new KeyValuePair<string, string>("ref", sourceBranch)
        });

                var createResponse = await client.PostAsync(createBranchUrl, createContent, cancellationToken);
                if (!createResponse.IsSuccessStatusCode)
                {
                    var error = await createResponse.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"Failed to create branch {branchName}: {error}");
                }
            }
            else if (!branchResponse.IsSuccessStatusCode)
            {
                var error = await branchResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to check branch {branchName}: {error}");
            }
        }

        private async Task<int> CreateMergeRequest(HttpClient client, string gitlabApiBase, int projectId, string sourceBranch, string targetBranch, string mergeCommitSha, CancellationToken cancellationToken)
        {
            var mrUrl = $"{gitlabApiBase}/projects/{projectId}/merge_requests";
            var mrContent = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("source_branch", sourceBranch),
        new KeyValuePair<string, string>("target_branch", targetBranch),
        new KeyValuePair<string, string>("title", $"Revert merge commit {mergeCommitSha[..8]}"),
        new KeyValuePair<string, string>("description", $"Reverts merge commit {mergeCommitSha}")
    });

            var mrResponse = await client.PostAsync(mrUrl, mrContent, cancellationToken);
            if (!mrResponse.IsSuccessStatusCode)
            {
                var error = await mrResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to create merge request: {error}");
            }

            var mrJson = await mrResponse.Content.ReadAsStringAsync(cancellationToken);
            using var mrDoc = JsonDocument.Parse(mrJson);

            if (mrDoc.RootElement.TryGetProperty("iid", out var iidElement))
            {
                return iidElement.GetInt32();
            }

            throw new InvalidOperationException("Could not determine the merge request ID.");
        }

        private async Task<GitlabPipelineDTO> GetDefaultBranchPipelineStatus(GitlabProjectDTO projectDTO, string userPAT, CancellationToken cancellationToken = default)
        {
            if (!Uri.TryCreate(projectDTO.Url, UriKind.Absolute, out var uri))
                throw new ArgumentException("Invalid URL", nameof(projectDTO.Url));

            var gitlabApiBase = $"{uri.Scheme}://{uri.Host}/api/v4";
            var projectId = projectDTO.ProjectId;
            var defaultBranch = projectDTO.DefaultBranch;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", userPAT);

            var url = $"{gitlabApiBase}/projects/{projectId}/pipelines?ref={defaultBranch}&per_page=1";
            var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement[0].TryGetProperty("status", out var statusElement))
                throw new InvalidOperationException("GitLab API response does not contain 'status' property");

            if (!doc.RootElement[0].TryGetProperty("sha", out var shaElement))
                throw new InvalidOperationException("GitLab API response does not contain 'sha' property");

            return new GitlabPipelineDTO
            {
                Status = statusElement.ToString(),
                CommitSHA = shaElement.ToString(),
            };
        }
    }
}
