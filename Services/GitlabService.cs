using System.Text.Json;
using DeploymentManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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

                project.GitlabProjectId = await FetchProjectId(project.GitlabUrl!, userPAT);

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

        private async Task<int> FetchProjectId(string url, string userPAT)
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

            return idElement.GetInt32();
        }
    }
}
