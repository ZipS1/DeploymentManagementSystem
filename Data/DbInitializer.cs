using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeploymentManagementSystem.Data
{
    public static class DbInitializer
    {
        public static async Task<bool> EnsureDbCreatedAndSeededAsync(DbContextOptions<ApplicationDbContext> options)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>(options);

            using var context = new ApplicationDbContext(builder.Options);
            if (await context.Database.EnsureCreatedAsync())
            {
                await SeedAsync(context);
                return true;
            }
            return false;
        }

        private static async Task SeedAsync(ApplicationDbContext context)
        {
            var adminRole = new IdentityRole("Admin");
            await context.Roles.AddAsync(adminRole);

            var rootUser = new ApplicationUser
            {
                UserName = "root@mail.com",
                Email = "root@mail.com",
                NormalizedEmail = "ROOT@MAIL.COM",
                NormalizedUserName = "ROOT@MAIL.COM",
                EmailConfirmed = true
            };

            var passwordHasher = new PasswordHasher<ApplicationUser>();
            rootUser.PasswordHash = passwordHasher.HashPassword(rootUser, "root");

            await context.Users.AddAsync(rootUser);
            await context.SaveChangesAsync();

            await context.UserRoles.AddAsync(
                new IdentityUserRole<string>
                {
                    RoleId = adminRole.Id,
                    UserId = rootUser.Id
                }
            );

            await context.SaveChangesAsync();
        }
    }
}