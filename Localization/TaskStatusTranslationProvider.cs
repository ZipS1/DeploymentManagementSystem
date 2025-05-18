namespace DeploymentManagementSystem.Localization
{
    public static class TaskStatusTranslationProvider
    {
        private static readonly Dictionary<string, string> _statusTranslations = new()
        {
            { "New", "Новая" },
            { "Assigned", "Назначена" },
            { "In progress", "В работе" },
            { "On review", "На проверке" },
            { "Needs revision", "Требует доработки" },
            { "Ready to deploy", "Готова к развертыванию" },
            { "Deployment error", "Ошибка развертывания" },
            { "Successfully deployed", "Успешно развернута" },
            { "Finished", "Завершена" }
        };

        public static string GetTranslation(string? statusName)
        {
            if (string.IsNullOrEmpty(statusName))
                return string.Empty;

            return _statusTranslations.TryGetValue(statusName, out var translation)
                ? translation
                : statusName ?? string.Empty;
        }
    }
}
