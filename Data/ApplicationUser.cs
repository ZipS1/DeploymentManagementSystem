using DeploymentManagementSystem.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace DeploymentManagementSystem.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Project>? Projects { get; set; }
    }

}
