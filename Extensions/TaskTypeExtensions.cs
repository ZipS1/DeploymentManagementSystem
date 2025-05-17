using DeploymentManagementSystem.Localization;

namespace DeploymentManagementSystem.Extensions
{
    public static class TaskTypeExtensions
    {
        public static string GetTranslation(this Data.Models.TaskType type)
            => TaskTypeTranslationProvider.GetTranslation(type?.Name);
    }
}
