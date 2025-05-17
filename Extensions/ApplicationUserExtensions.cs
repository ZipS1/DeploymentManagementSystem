using DeploymentManagementSystem.Data;
using DeploymentManagementSystem.Localization;
using Microsoft.AspNetCore.Identity;

namespace DeploymentManagementSystem.Extensions
{
    public static class ApplicationUserExtensions
    {
        public static string GetRoleTranslation(this ApplicationUser user)
        {
            return RoleTranslationProvider.GetTranslation(user.Role);
        }
    }
}
