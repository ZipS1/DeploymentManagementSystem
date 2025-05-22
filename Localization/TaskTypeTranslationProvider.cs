using DeploymentManagementSystem.Data.DomainStringConstants;

namespace DeploymentManagementSystem.Localization
{
    public static class TaskTypeTranslationProvider
    {
        private static readonly Dictionary<string, string> _typeTranslations = new()
        {
            { TaskTypeConstants.Analysis, "Анализ" },
            { TaskTypeConstants.Fix, "Исправление" },
            { TaskTypeConstants.Feature, "Функционал" },
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
