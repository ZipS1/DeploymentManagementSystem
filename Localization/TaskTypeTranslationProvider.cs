namespace DeploymentManagementSystem.Localization
{
    public static class TaskTypeTranslationProvider
    {
        private static readonly Dictionary<string, string> _typeTranslations = new()
        {
            { "Analysis", "Анализ" },
            { "Bug", "Ошибка" },
            { "Feature", "Функционал" },
        };

        public static string GetTranslation(string? statusName)
        {
            if (string.IsNullOrEmpty(statusName))
                return string.Empty;

            return _typeTranslations.TryGetValue(statusName, out var translation)
                ? translation
                : statusName ?? string.Empty;
        }
    }
}
