namespace DeploymentManagementSystem.Data.Models
{
    public class Task
    {
        public int Id {  get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int TaskStatusId {  get; set; }
        public int TaskTypeId { get; set; }
        public int ProjectId { get; set; }
        public string AssignedUserId { get; set; }
        public string CreatorUserId { get; set; }

        public TaskStatus? TaskStatus { get; set; }
        public TaskType? TaskType { get; set; }
        public Project? Project { get; set; }
        public ApplicationUser? AssignedUser { get; set; }
        public ApplicationUser? CreatorUser { get; set; }
    }
}
