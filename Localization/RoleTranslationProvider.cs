namespace DeploymentManagementSystem.Localization
{
    public static class RoleTranslationProvider
    {
        private static readonly Dictionary<string, string> _roleTranslations = new()
        {
            { "Admin", "Администратор" },
            { "NewUser", "Новый пользователь" },
            { "ProjectManager", "Менеджер проекта" },
            { "Developer", "Разработчик" },
            { "Lead", "Тимлид" }
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
