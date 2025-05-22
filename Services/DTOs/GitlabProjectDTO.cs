namespace DeploymentManagementSystem.Services.DTOs
{
    public class GitlabProjectDTO
    {
        public string? Url { get; set; }
        public int ProjectId { get; set; }
        public string? DefaultBranch { get; set; }
    }
}
