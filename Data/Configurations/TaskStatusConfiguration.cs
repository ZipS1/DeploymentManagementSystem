using DeploymentManagementSystem.Data.Heplers;
using DeploymentManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeploymentManagementSystem.Data.Configurations
{
    public class TaskStatusConfiguration : IEntityTypeConfiguration<Models.TaskStatus>
    {
        private const string ModelName = "task_status";
        private const string TableName = $"cd_{ModelName}";

        public void Configure(EntityTypeBuilder<Models.TaskStatus> builder)
        {
            builder.ToTable(TableName)
                .HasKey(p => p.Id)
                .HasName($"pk_{TableName}_id");

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName($"{ModelName}_id")
                .HasColumnType(ColumnType.Int)
                .HasComment("Идентфикатор записи статуса задачи");

            builder.Property(p => p.Name)
                .IsRequired()
                .HasColumnName($"c_{ModelName}_name")
                .HasColumnType(ColumnType.String).HasMaxLength(100)
                .HasComment("Название статуса задачи");
        }
    }
}
