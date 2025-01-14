using DeploymentManagementSystem.Data.Heplers;
using DeploymentManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeploymentManagementSystem.Data.Configurations
{
    public class TaskStatusTransitionConfiguration : IEntityTypeConfiguration<TaskStatusTransition>
    {
        private const string ModelName = "task_status_transition";
        private const string TableName = $"cd_{ModelName}";

        public void Configure(EntityTypeBuilder<TaskStatusTransition> builder)
        {
            builder.ToTable(TableName)
                .HasKey(p => p.Id)
                .HasName($"pk_{TableName}_id");

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName($"{ModelName}_id")
                .HasColumnType(ColumnType.Int)
                .HasComment("Идентфикатор записи перехода статуса задачи");

            builder.Property(p => p.TaskTypeId)
                .IsRequired()
                .HasColumnName($"f_{ModelName}_task_type_id")
                .HasColumnType(ColumnType.Int)
                .HasComment("Идентификатор типа задачи");

            builder.Property(p => p.FromTaskStatusId)
                .IsRequired()
                .HasColumnName($"f_{ModelName}_from_task_status_id")
                .HasColumnType(ColumnType.Int)
                .HasComment("Идентификатор начального статуса задачи");

            builder.Property(p => p.ToTaskStatusId)
                .IsRequired()
                .HasColumnName($"f_{ModelName}_to_task_status_id")
                .HasColumnType(ColumnType.Int)
                .HasComment("Идентификатор конечного статуса задачи");

            builder
                .HasOne(p => p.TaskType)
                .WithMany()
                .HasForeignKey(p => p.TaskTypeId)
                .HasConstraintName($"fk_f_{ModelName}_task_type_id")
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(p => p.FromTaskStatus)
                .WithMany()
                .HasForeignKey(p => p.FromTaskStatusId)
                .HasConstraintName($"fk_f_{ModelName}_from_task_status_id")
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(p => p.ToTaskStatus)
                .WithMany()
                .HasForeignKey(p => p.ToTaskStatusId)
                .HasConstraintName($"fk_f_{ModelName}_to_task_status_id")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => p.TaskTypeId, $"idx_{TableName}_fk_f_{ModelName}_task_type_id");
            builder.HasIndex(p => p.FromTaskStatusId, $"idx_{TableName}_fk_f_{ModelName}_from_task_status_id");
            builder.HasIndex(p => p.ToTaskStatusId, $"idx_{TableName}_fk_f_{ModelName}_to_task_status_id");
        }
    }
}
