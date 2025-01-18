using System.ComponentModel.DataAnnotations;
using DeploymentManagementSystem.Data.DataAnnotations;

namespace DeploymentManagementSystem.Data.Models
{
    [EndDateAfterStartDate(startDatePropertyName: "StartDate", endDatePropertyName: "EndDate", ErrorMessage = "End date must be later than start date")]
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Maximum 100 characters allowed")]
        public string Name { get; set; }

        [StringLength(500, MinimumLength = 6, ErrorMessage = "Description, if specified, should be from 6 to 500 symbols")]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }


        public DateTime? EndDate { get; set; }

        [Url(ErrorMessage = "Not a valid URL")]
        public string? GitlabUrl { get; set; }

        [Required(ErrorMessage = "Project manager must be defined")]
        public string ProjectManagerId { get; set; }

        public ApplicationUser? ProjectManager { get; set; }
        public ICollection<ApplicationUser>? Participants { get; set; }
    }
}
