using DeploymentManagementSystem.Data.DataAnnotations;
using DeploymentManagementSystem.Data.Heplers;
using DeploymentManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeploymentManagementSystem.Data.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        private const string ModelName = "project";
        private const string TableName = $"cd_{ModelName}";

        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable(TableName)
                .HasKey(p => p.Id)
                .HasName($"pk_{TableName}_id");

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName($"{ModelName}_id")
                .HasColumnType(ColumnType.Int)
                .HasComment("Идентфикатор записи проекта");

            builder.Property(p => p.Name)
                .IsRequired()
                .HasColumnName($"c_{ModelName}_name")
                .HasColumnType(ColumnType.String).HasMaxLength(100)
                .HasComment("Название проекта");

            builder.Property(p => p.Description)
                .HasColumnName($"c_{ModelName}_description")
                .HasColumnType(ColumnType.String).HasMaxLength(500)
                .HasComment("Описание проекта");

            builder.Property(p => p.StartDate)
                .IsRequired()
                .HasColumnName($"c_{ModelName}_start_date")
                .HasColumnType(ColumnType.DateTime)
                .HasComment("Дата начала проекта");

            builder.Property(p => p.EndDate)
                .HasColumnName($"c_{ModelName}_end_date")
                .HasColumnType(ColumnType.DateTime)
                .HasComment("Дата завершения проекта");

            builder.Property(p => p.GitlabUrl)
                .HasColumnName($"c_{ModelName}_gitlab_url")
                .HasColumnType(ColumnType.String).HasMaxLength(100)
                .HasComment("URL GitLab репозитория проекта");

            builder.Property(p => p.ProjectManagerId)
                .IsRequired()
                .HasColumnName($"f_{ModelName}_project_manager_id")
                .HasColumnType(ColumnType.String)
                .HasComment("Идентификатор руководителя проекта");

            builder
                .HasOne(p => p.ProjectManager)
                .WithMany()
                .HasForeignKey(p => p.ProjectManagerId)
                .HasConstraintName($"fk_f_{ModelName}_project_manager_id")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Participants)
                   .WithMany(p => p.Projects)
                   .UsingEntity<Dictionary<string, object>>(
                    "cd_project_user",
                    j => j
                        .HasOne<ApplicationUser>()
                        .WithMany()
                        .HasForeignKey("user_id")
                        .HasConstraintName($"fk_f_{ModelName}_user_id")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Project>()
                        .WithMany()
                        .HasForeignKey("project_id")
                        .HasConstraintName($"fk_f_{ModelName}_project_id")
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("user_id", "project_id");
                        j.ToTable("cd_project_user");
                    });

            builder.HasIndex(p => p.ProjectManagerId, $"idx_{TableName}_fk_f_{ModelName}_project_manager_id");
        }
    }
}
