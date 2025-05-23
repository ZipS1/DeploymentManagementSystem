namespace DeploymentManagementSystem.Services.DTOs
{
    public class GitlabTaskDTO
    {
        public string? BranchNameEncoded { get; set; }
        public int MergeRequestID { get; set; }
    }
}
