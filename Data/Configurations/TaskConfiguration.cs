using DeploymentManagementSystem.Data.Heplers;
using DeploymentManagementSystem.Data.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DeploymentManagementSystem.Data.Configurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<Models.Task>
    {
        private const string ModelName = "task";
        private const string TableName = $"cd_{ModelName}";

        public void Configure(EntityTypeBuilder<Models.Task> builder)
        {
            builder.ToTable(TableName)
                .HasKey(p => p.Id)
                .HasName($"pk_{TableName}_id");

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName($"{ModelName}_id")
                .HasColumnType(ColumnType.Int)
                .HasComment("Идентфикатор записи задачи");

            builder.Property(p => p.Name)
                .IsRequired()
                .HasColumnName($"c_{ModelName}_name")
                .HasColumnType(ColumnType.String).HasMaxLength(100)
                .HasComment("Название задачи");

            builder.Property(p => p.Description)
                .HasColumnName($"c_{ModelName}_description")
                .HasColumnType(ColumnType.String).HasMaxLength(500)
                .HasComment("Описание задачи");

            builder.Property(p => p.CreationDate)
                .IsRequired()
                .HasColumnName($"c_{ModelName}_creation_date")
                .HasColumnType(ColumnType.DateTime)
                .HasComment("Дата создания задачи");

            builder.Property(p => p.DueDate)
                .HasColumnName($"c_{ModelName}_due_date")
                .HasColumnType(ColumnType.DateTime)
                .HasComment("Дата сдачи задачи");

            builder.Property(p => p.TaskStatusId)
                .IsRequired()
                .HasColumnName($"f_{ModelName}_task_status_id")
                .HasColumnType(ColumnType.Int)
                .HasComment("Идентификатор статуса задачи");

            builder.Property(p => p.TaskTypeId)
                .IsRequired()
                .HasColumnName($"f_{ModelName}_task_type_id")
                .HasColumnType(ColumnType.Int)
                .HasComment("Идентификатор типа задачи");

            builder.Property(p => p.ProjectId)
                .IsRequired()
                .HasColumnName($"f_{ModelName}_project_id")
                .HasColumnType(ColumnType.Int)
                .HasComment("Идентификатор проекта задачи");

            builder.Property(p => p.AssignedUserId)
                .HasColumnName($"f_{ModelName}_assigned_user_id")
                .HasColumnType(ColumnType.String)
                .HasComment("Идентификатор исполнителя задачи");

            builder.Property(p => p.CreatorUserId)
                .IsRequired()
                .HasColumnName($"f_{ModelName}_creator_user_id")
                .HasColumnType(ColumnType.String)
                .HasComment("Идентификатор создателя задачи");

            builder
                .HasOne(p => p.TaskStatus)
                .WithMany()
                .HasForeignKey(p => p.TaskStatusId)
                .HasConstraintName($"fk_f_{ModelName}_task_status_id")
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(p => p.TaskType)
                .WithMany()
                .HasForeignKey(p => p.TaskTypeId)
                .HasConstraintName($"fk_f_{ModelName}_task_type_id")
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(p => p.AssignedUser)
                .WithMany()
                .HasForeignKey(p => p.AssignedUserId)
                .HasConstraintName($"fk_f_{ModelName}_assigned_user_id")
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(p => p.CreatorUser)
                .WithMany()
                .HasForeignKey(p => p.CreatorUserId)
                .HasConstraintName($"fk_f_{ModelName}_creator_user_id")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(p => p.TaskStatusId, $"idx_{TableName}_fk_f_{ModelName}_task_status_id");
            builder.HasIndex(p => p.TaskTypeId, $"idx_{TableName}_fk_f_{ModelName}_task_type_id");
            builder.HasIndex(p => p.AssignedUserId, $"idx_{TableName}_fk_f_{ModelName}_assigned_user_id");
            builder.HasIndex(p => p.CreatorUserId, $"idx_{TableName}_fk_f_{ModelName}_creator_user_id");
        }
    }
}
