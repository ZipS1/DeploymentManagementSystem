using DeploymentManagementSystem.Localization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DeploymentManagementSystem.Extensions
{
    public static class StringExtensions
    {
        public static string GetRoleTranslation(this string? name) => RoleTranslationProvider.GetTranslation(name);
        public static string GetTaskTypeTranslation(this string? name) => TaskTypeTranslationProvider.GetTranslation(name);
    }
}
