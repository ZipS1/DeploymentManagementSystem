using DeploymentManagementSystem.Data.Heplers;
using DeploymentManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeploymentManagementSystem.Data.Configurations
{
    public class TaskTypeConfiguration : IEntityTypeConfiguration<TaskType>
    {
        private const string ModelName = "task_type";
        private const string TableName = $"cd_{ModelName}";

        public void Configure(EntityTypeBuilder<TaskType> builder)
        {
            builder.ToTable(TableName)
                .HasKey(p => p.Id)
                .HasName($"pk_{TableName}_id");

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName($"{ModelName}_id")
                .HasColumnType(ColumnType.Int)
                .HasComment("Идентфикатор типа задачи");

            builder.Property(p => p.Name)
                .IsRequired()
                .HasColumnName($"c_{ModelName}_name")
                .HasColumnType(ColumnType.String).HasMaxLength(100)
                .HasComment("Название типа задачи");

            builder.Property(p => p.InitialTaskStatusId)
                .IsRequired()
                .HasColumnName($"f_{ModelName}_initial_task_status_id")
                .HasColumnType(ColumnType.Int)
                .HasComment("Идентификатор начального статуса типа задачи");

            builder
                .HasOne(p => p.InitialTaskStatus)
                .WithMany()
                .HasForeignKey(p => p.InitialTaskStatusId)
                .HasConstraintName($"fk_f_{ModelName}_initial_task_status_id")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.InitialTaskStatusId, $"idx_{TableName}_fk_f_{ModelName}_initial_task_status_id");
        }
    }
}
