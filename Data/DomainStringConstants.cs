namespace DeploymentManagementSystem.Data.DomainStringConstants
{
    public static class RoleConstants
    {
        public const string Admin = "Admin";
        public const string NewUser = "NewUser";
        public const string ProjectManager = "ProjectManager";
        public const string Developer = "Developer";
        public const string LeadDeveloper = "Lead";
        public const string Assignee = "Assignee";

        public static string Multiple(params string[] roles) => string.Join(",", roles);
    }

    public static class TaskStatusConstants
    {
        public const string New = "New";
        public const string Assigned = "Assigned";
        public const string InProgress = "In progress";
        public const string OnReview = "On review";
        public const string NeedsRevision = "Needs revision";
        public const string ReadyToDeploy = "Ready to deploy";
        public const string DeploymentError = "Deployment error";
        public const string SuccessfullyDeployed = "Successfully deployed";
        public const string Finished = "Finished";
    }

    public static class TaskTypeConstants
    {
        public const string Analysis = "Analysis";
        public const string Bug = "Bug";
        public const string Feature = "Feature";
    }
}
