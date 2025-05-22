namespace DeploymentManagementSystem.Data.Models
{
    public class TaskType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public int InitialTaskStatusId { get; set; }

        public TaskStatus? InitialTaskStatus { get; set; }
    }
}
