using DeploymentManagementSystem.Data.DomainStringConstants;

namespace DeploymentManagementSystem.Localization
{
    public static class TaskStatusTranslationProvider
    {
        private static readonly Dictionary<string, string> _statusTranslations = new()
        {
            { TaskStatusConstants.New, "Новая" },
            { TaskStatusConstants.Assigned, "Назначена" },
            { TaskStatusConstants.InProgress, "В работе" },
            { TaskStatusConstants.OnReview, "На проверке" },
            { TaskStatusConstants.NeedsRevision, "Требует доработки" },
            { TaskStatusConstants.ReadyToDeploy, "Готова к развертыванию" },
            { TaskStatusConstants.Deploying, "Развертывается" },
            { TaskStatusConstants.DeploymentError, "Ошибка развертывания" },
            { TaskStatusConstants.SuccessfullyDeployed, "Успешно развернута" },
            { TaskStatusConstants.Finished, "Завершена" }
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
