using DeploymentManagementSystem.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace DeploymentManagementSystem.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string? Lastname { get; set; }
        public string? Name { get; set; }
        public string? Patronymic { get; set; }
        public bool IsDataFullfilled { get; set; } = false;
        public string Role { get; set; } = "NewUser";

        public ICollection<Project>? Projects { get; set; }

        public string Fullname => $"{Lastname} {Name!.First()}.{Patronymic!.First()}.".Trim();
    }

}
