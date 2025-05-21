using System.ComponentModel.DataAnnotations;
using DeploymentManagementSystem.Data.DataAnnotations;

namespace DeploymentManagementSystem.Data.Models
{
    [EndDateAfterStartDate(startDatePropertyName: "CreationDate", endDatePropertyName: "DueDate", ErrorMessage = "Срок выполнения должен быть позже даты создания")]
    public class Task
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Максимум 100 символов")]
        public string Name { get; set; }

        [StringLength(500, MinimumLength = 6, ErrorMessage = "Описание, если указано, должно быть от 6 до 500 символов")]
        public string? Description { get; set; }

        [StringLength(100, MinimumLength = 3, ErrorMessage = "")]
        public string? RefName { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }
        public DateTime? DueDate { get; set; }

        public int TaskStatusId { get; set; }

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
