using System.ComponentModel.DataAnnotations;
using DeploymentManagementSystem.Data.DataAnnotations;

namespace DeploymentManagementSystem.Data.Models
{
    [EndDateAfterStartDate(startDatePropertyName: "CreationDate", endDatePropertyName: "DueDate", ErrorMessage = "Due date must be later than creation date")]
    public class Task
    {
        public int Id {  get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Maximum 100 characters allowed")]
        public string Name { get; set; }

        [StringLength(500, MinimumLength = 6, ErrorMessage = "Description, if specified, should be from 6 to 500 symbols")]
        public string? Description { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }
        public DateTime? DueDate { get; set; }

        public int TaskStatusId {  get; set; }

        [Required]
        public int? TaskTypeId { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public string? AssignedUserId { get; set; }

        [Required]
        public string CreatorUserId { get; set; }

        public TaskStatus? TaskStatus { get; set; }
        public TaskType? TaskType { get; set; }
        public Project? Project { get; set; }
        public ApplicationUser? AssignedUser { get; set; }
        public ApplicationUser? CreatorUser { get; set; }
    }
}
