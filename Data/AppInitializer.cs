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
            var roles = new[] { "Admin", "NewUser", "ProjectManager", "Developer", "Lead" };

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
            if ((await userManager.GetUsersInRoleAsync("Admin")).Any())
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
                Role = "Admin",
            };

            await userManager.CreateAsync(rootUser);
            await userManager.AddPasswordAsync(rootUser, "admroot");
            await userManager.AddToRoleAsync(rootUser, "Admin");
        }

        private static async Task EnsureTaskStatusesCreated(AsyncServiceScope scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (await context.TaskStatuses.AnyAsync())
                return;

            var statuses = new List<Models.TaskStatus>
            {
                new Models.TaskStatus { Name = "New" },
                new Models.TaskStatus { Name = "Assigned" },
                new Models.TaskStatus { Name = "In progress" },
                new Models.TaskStatus { Name = "On review" },
                new Models.TaskStatus { Name = "Needs revision" },
                new Models.TaskStatus { Name = "Ready to deploy" },
                new Models.TaskStatus { Name = "Deployment error" },
                new Models.TaskStatus { Name = "Successfully deployed" },
                new Models.TaskStatus { Name = "Finished" },
            };

            await context.TaskStatuses.AddRangeAsync(statuses);
            await context.SaveChangesAsync();
        }

        private static async Task EnsureTaskTypesCreated(AsyncServiceScope scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            int newTaskStatusId = (await context.TaskStatuses.FirstOrDefaultAsync(s => s.Name == "New"))!.Id;

            if (await context.TaskTypes.AnyAsync())
                return;

            var types = new List<Models.TaskType>
            {
                new Models.TaskType { Name = "Analysis", InitialTaskStatusId = newTaskStatusId },
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

            int analysisTaskTypeId = (await context.TaskTypes.FirstOrDefaultAsync(t => t.Name == "Analysis"))!.Id;
            var transitions = new List<Models.TaskStatusTransition>
            {
                new Models.TaskStatusTransition {
                    TaskTypeId = analysisTaskTypeId,
                    FromTaskStatusId = statuses.FirstOrDefault(s => s.Name == "Assigned")!.Id,
                    ToTaskStatusId = statuses.FirstOrDefault(s => s.Name == "In progress")!.Id,
                    AllowedRoles = "Assignee"
                },
                new Models.TaskStatusTransition {
                    TaskTypeId = analysisTaskTypeId,
                    FromTaskStatusId = statuses.FirstOrDefault(s => s.Name == "In progress")!.Id,
                    ToTaskStatusId = statuses.FirstOrDefault(s => s.Name == "On review")!.Id,
                    AllowedRoles = "Assignee"
                },
                new Models.TaskStatusTransition {
                    TaskTypeId = analysisTaskTypeId,
                    FromTaskStatusId = statuses.FirstOrDefault(s => s.Name == "On review")!.Id,
                    ToTaskStatusId = statuses.FirstOrDefault(s => s.Name == "Needs revision")!.Id,
                    AllowedRoles = "ProjectManager,Lead"
                },
                new Models.TaskStatusTransition {
                    TaskTypeId = analysisTaskTypeId,
                    FromTaskStatusId = statuses.FirstOrDefault(s => s.Name == "On review")!.Id,
                    ToTaskStatusId = statuses.FirstOrDefault(s => s.Name == "Finished")!.Id,
                    AllowedRoles = "ProjectManager,Lead"
                },
                new Models.TaskStatusTransition {
                    TaskTypeId = analysisTaskTypeId,
                    FromTaskStatusId = statuses.FirstOrDefault(s => s.Name == "Needs revision")!.Id,
                    ToTaskStatusId = statuses.FirstOrDefault(s => s.Name == "In progress")!.Id,
                    AllowedRoles = "Assignee"
                },
            };

            await context.TaskStatusTransitions.AddRangeAsync(transitions);
            await context.SaveChangesAsync();
        }
    }
}
