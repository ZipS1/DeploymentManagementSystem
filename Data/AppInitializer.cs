using DeploymentManagementSystem.Data.DomainStringConstants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeploymentManagementSystem.Data
{
    public static class AppInitializer
    {
        public static async Task<bool> EnsureAppInitialized(AsyncServiceScope scope)
        {
            var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>(options);

            using var context = new ApplicationDbContext(builder.Options);

#if FLUSH_ON_START
            await context.Database.EnsureDeletedAsync();
#endif

            if (!context.Database.EnsureCreated())
                return false;

            await EnsureRolesCreated(scope);
            await EnsureAdminCreated(scope);
            await EnsureTaskStatusesCreated(scope);
            await EnsureTaskTypesCreated(scope);
            await EnsureTaskStatusTransitionsCreated(scope);

            return true;
        }

        private static async Task EnsureRolesCreated(AsyncServiceScope scope)
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = new[] { RoleConstants.Admin, RoleConstants.NewUser, RoleConstants.ProjectManager, RoleConstants.Developer, RoleConstants.LeadDeveloper };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task EnsureAdminCreated(AsyncServiceScope scope)
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            if ((await userManager.GetUsersInRoleAsync(RoleConstants.Admin)).Any())
                return;

            var rootUser = new ApplicationUser()
            {
                UserName = "root@mail.com",
                Email = "root@mail.com",
                NormalizedEmail = "ROOT@MAIL.COM",
                NormalizedUserName = "ROOT@MAIL.COM",
                Lastname = "root",
                Name = "root",
                Patronymic = "root",
                Role = RoleConstants.Admin,
            };

            await userManager.CreateAsync(rootUser);
            await userManager.AddPasswordAsync(rootUser, "admroot");
            await userManager.AddToRoleAsync(rootUser, RoleConstants.Admin);
        }

        private static async Task EnsureTaskStatusesCreated(AsyncServiceScope scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (await context.TaskStatuses.AnyAsync())
                return;

            var statuses = new List<Models.TaskStatus>
            {
                new Models.TaskStatus { Name = TaskStatusConstants.New },
                new Models.TaskStatus { Name = TaskStatusConstants.Assigned },
                new Models.TaskStatus { Name = TaskStatusConstants.InProgress },
                new Models.TaskStatus { Name = TaskStatusConstants.OnReview },
                new Models.TaskStatus { Name = TaskStatusConstants.NeedsRevision },
                new Models.TaskStatus { Name = TaskStatusConstants.ReadyToDeploy },
                new Models.TaskStatus { Name = TaskStatusConstants.DeploymentError },
                new Models.TaskStatus { Name = TaskStatusConstants.SuccessfullyDeployed },
                new Models.TaskStatus { Name = TaskStatusConstants.Finished },
            };

            await context.TaskStatuses.AddRangeAsync(statuses);
            await context.SaveChangesAsync();
        }

        private static async Task EnsureTaskTypesCreated(AsyncServiceScope scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            int newTaskStatusId = (await context.TaskStatuses.FirstOrDefaultAsync(s => s.Name == TaskStatusConstants.New))!.Id;

            if (await context.TaskTypes.AnyAsync())
                return;

            var types = new List<Models.TaskType>
            {
                new Models.TaskType { Name = TaskTypeConstants.Analysis, InitialTaskStatusId = newTaskStatusId },
            };

            await context.TaskTypes.AddRangeAsync(types);
            await context.SaveChangesAsync();
        }

        private static async Task EnsureTaskStatusTransitionsCreated(AsyncServiceScope scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var statuses = await context.TaskStatuses.ToListAsync();

            if (await context.TaskStatusTransitions.AnyAsync())
                return;

            int analysisTaskTypeId = (await context.TaskTypes.FirstOrDefaultAsync(t => t.Name == TaskTypeConstants.Analysis))!.Id;
            var transitions = new List<Models.TaskStatusTransition>
            {
                new Models.TaskStatusTransition {
                    TaskTypeId = analysisTaskTypeId,
                    FromTaskStatusId = statuses.FirstOrDefault(s => s.Name == TaskStatusConstants.Assigned)!.Id,
                    ToTaskStatusId = statuses.FirstOrDefault(s => s.Name == TaskStatusConstants.InProgress)!.Id,
                    AllowedRoles = RoleConstants.Assignee
                },
                new Models.TaskStatusTransition {
                    TaskTypeId = analysisTaskTypeId,
                    FromTaskStatusId = statuses.FirstOrDefault(s => s.Name == TaskStatusConstants.InProgress)!.Id,
                    ToTaskStatusId = statuses.FirstOrDefault(s => s.Name == TaskStatusConstants.OnReview)!.Id,
                    AllowedRoles = RoleConstants.Assignee
                },
                new Models.TaskStatusTransition {
                    TaskTypeId = analysisTaskTypeId,
                    FromTaskStatusId = statuses.FirstOrDefault(s => s.Name == TaskStatusConstants.OnReview)!.Id,
                    ToTaskStatusId = statuses.FirstOrDefault(s => s.Name == TaskStatusConstants.NeedsRevision)!.Id,
                    AllowedRoles = RoleConstants.Multiple(RoleConstants.ProjectManager, RoleConstants.LeadDeveloper)
                },
                new Models.TaskStatusTransition {
                    TaskTypeId = analysisTaskTypeId,
                    FromTaskStatusId = statuses.FirstOrDefault(s => s.Name == TaskStatusConstants.OnReview)!.Id,
                    ToTaskStatusId = statuses.FirstOrDefault(s => s.Name == TaskStatusConstants.Finished)!.Id,
                    AllowedRoles = RoleConstants.Multiple(RoleConstants.ProjectManager, RoleConstants.LeadDeveloper)
                },
                new Models.TaskStatusTransition {
                    TaskTypeId = analysisTaskTypeId,
                    FromTaskStatusId = statuses.FirstOrDefault(s => s.Name == TaskStatusConstants.NeedsRevision)!.Id,
                    ToTaskStatusId = statuses.FirstOrDefault(s => s.Name == TaskStatusConstants.InProgress)!.Id,
                    AllowedRoles = RoleConstants.Assignee,
                },
            };

            await context.TaskStatusTransitions.AddRangeAsync(transitions);
            await context.SaveChangesAsync();
        }
    }
}
