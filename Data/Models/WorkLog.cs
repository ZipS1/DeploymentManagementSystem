namespace DeploymentManagementSystem.Data.Models
{
    public class WorkLog
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal TimeSpent { get; set; }
        public int TaskId { get; set; }
        public string UserId { get; set; }

        public Task? Task { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
