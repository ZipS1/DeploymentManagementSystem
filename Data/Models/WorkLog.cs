using System.ComponentModel.DataAnnotations;

namespace DeploymentManagementSystem.Data.Models
{
    public class WorkLog
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Некорректное значение для затраченного времени")]
        public decimal TimeSpent { get; set; }

        public int TaskId { get; set; }
        public string UserId { get; set; }

        public Task? Task { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
