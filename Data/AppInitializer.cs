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
            if (context.Database.EnsureCreated() == false)
                return false;

            await EnsureRolesCreated(scope);
            await EnsureAdminCreated(scope);

            return true;
        }

        private static async Task EnsureRolesCreated(AsyncServiceScope scope)
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = new[] { "Admin", "NewUser", "ProjectManager", "Developer", "LeadDeveloper" };

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
                Patronymic = "root"
            };

            await userManager.CreateAsync(rootUser);
            await userManager.AddPasswordAsync(rootUser, "admroot");
            await userManager.AddToRoleAsync(rootUser, "Admin");
        }
    }
}