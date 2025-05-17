using DeploymentManagementSystem.Localization;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace DeploymentManagementSystem.Extensions
{
    public static class IdentityRoleExtensions
    {
        public static string GetTranslation(this IdentityRole role)
        {
            return RoleTranslationProvider.GetTranslation(role.Name);
        }
    }
}
