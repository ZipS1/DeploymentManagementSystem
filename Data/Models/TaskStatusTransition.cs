namespace DeploymentManagementSystem.Data.Models
{
    public class TaskStatusTransition
    {
        public int Id { get; set; }
        public int TaskTypeId { get; set; }
        public int FromTaskStatusId { get; set; }
        public int ToTaskStatusId { get; set; }

        public TaskType? TaskType { get; set; }
        public TaskStatus? FromTaskStatus { get; set; }
        public TaskStatus? ToTaskStatus { get; set; }
    }
}
