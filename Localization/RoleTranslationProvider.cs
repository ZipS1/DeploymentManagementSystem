using DeploymentManagementSystem.Data.DomainStringConstants;

namespace DeploymentManagementSystem.Localization
{
    public static class RoleTranslationProvider
    {
        private static readonly Dictionary<string, string> _roleTranslations = new()
        {
            { RoleConstants.Admin, "Администратор" },
            { RoleConstants.NewUser, "Новый пользователь" },
            { RoleConstants.ProjectManager, "Менеджер проекта" },
            { RoleConstants.Developer, "Разработчик" },
            { RoleConstants.LeadDeveloper, "Тимлид" },
            { RoleConstants.Gitlab, "Гитлаб" }
        };

        public static string GetTranslation(string? roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return string.Empty;

            return _roleTranslations.TryGetValue(roleName, out var translation)
                ? translation
                : roleName ?? string.Empty;
        }
    }
}
