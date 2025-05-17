using DeploymentManagementSystem.Localization;

namespace DeploymentManagementSystem.Extensions
{
    public static class TaskStatusExtensions
    {
        public static string GetTranslation(this Data.Models.TaskStatus status)
            => TaskStatusTranslationProvider.GetTranslation(status?.Name);
    }
}
