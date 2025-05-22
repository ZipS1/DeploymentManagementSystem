using System.ComponentModel.DataAnnotations;
using DeploymentManagementSystem.Data.DataAnnotations;

namespace DeploymentManagementSystem.Data.Models
{
    [EndDateAfterStartDate(startDatePropertyName: "StartDate", endDatePropertyName: "EndDate", ErrorMessage = "Дата окончания должна быть позже даты начала")]
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Максимум 100 символов")]
        public string Name { get; set; }

        [StringLength(500, MinimumLength = 6, ErrorMessage = "Описание, если указано, должно быть от 6 до 500 символов")]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Url(ErrorMessage = "Неправильный формат URL")]
        public string? GitlabUrl { get; set; }
        public int? GitlabProjectId { get; set; }
        public string? GitlabToken { get; set; }
        public bool IsGitlabConnected { get; set; } = false;


        [Required(ErrorMessage = "Руководитель проекта должен быть указан")]
        public string ProjectManagerId { get; set; }

        public ApplicationUser? ProjectManager { get; set; }
        public ICollection<ApplicationUser>? Participants { get; set; }
    }
}
