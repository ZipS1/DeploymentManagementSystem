namespace DeploymentManagementSystem.Data.Models
{
    public class Environment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public DateTime? LastDeploymentDate { get; set; }
        public int ProjectId { get; set; }

        public Project? Project { get; set; }
    }
}
