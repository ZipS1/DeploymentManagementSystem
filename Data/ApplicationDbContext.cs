using DeploymentManagementSystem.Data.Configurations;
using DeploymentManagementSystem.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeploymentManagementSystem.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<Models.Environment> Environments { get; set; }
        public DbSet<WorkLog> WorkLogs { get; set; }
        public DbSet<Models.Task> Tasks { get; set; }
        public DbSet<TaskType> TaskTypes { get; set; }
        public DbSet<Models.TaskStatus> TaskStatuses { get; set; }
        public DbSet<TaskStatusTransition> TaskStatusTransitions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new ProjectConfiguration());
            builder.ApplyConfiguration(new EnvironmentConfiguraton());
            builder.ApplyConfiguration(new WorkLogConfiguration());
            builder.ApplyConfiguration(new TaskConfiguration());
            builder.ApplyConfiguration(new TaskTypeConfiguration());
            builder.ApplyConfiguration(new TaskStatusConfiguration());
            builder.ApplyConfiguration(new TaskStatusTransitionConfiguration());
        }
    }
}
