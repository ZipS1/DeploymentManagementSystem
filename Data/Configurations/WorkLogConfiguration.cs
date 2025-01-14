using DeploymentManagementSystem.Data.Heplers;
using DeploymentManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeploymentManagementSystem.Data.Configurations
{
    public class WorkLogConfiguration : IEntityTypeConfiguration<WorkLog>
    {
        private const string ModelName = "work_log";
        private const string TableName = $"cd_{ModelName}";

        public void Configure(EntityTypeBuilder<WorkLog> builder)
        {
            builder.ToTable(TableName)
                .HasKey(p => p.Id)
                .HasName($"pk_{TableName}_id");

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName($"{ModelName}_id")
                .HasColumnType(ColumnType.Int)
                .HasComment("Идентфикатор записи трудозатрат");

            builder.Property(p => p.Date)
                .IsRequired()
                .HasColumnName($"c_{ModelName}_date")
                .HasColumnType(ColumnType.DateTime)
                .HasComment("Дата трудозатрат");

            builder.Property(p => p.TimeSpent)
                .IsRequired()
                .HasColumnName($"c_{ModelName}_time_spent")
                .HasColumnType(ColumnType.Decimal)
                .HasComment("Количество трудозатрат");

            builder.Property(p => p.TaskId)
                .IsRequired()
                .HasColumnName($"f_{ModelName}_task_id")
                .HasColumnType(ColumnType.Int)
                .HasComment("Идентификатор задачи");

            builder.Property(p => p.UserId)
                .IsRequired()
                .HasColumnName($"f_{ModelName}_user_id")
                .HasColumnType(ColumnType.String)
                .HasComment("Идентификатор пользователя");

            builder
                .HasOne(p => p.Task)
                .WithMany()
                .HasForeignKey(p => p.TaskId)
                .HasConstraintName($"fk_f_{ModelName}_task_id")
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .HasConstraintName($"fk_f_{ModelName}_user_id")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(p => p.TaskId, $"idx_{TableName}_fk_f_{ModelName}_task_id");
            builder.HasIndex(p => p.UserId, $"idx_{TableName}_fk_f_{ModelName}_user_id");
        }
    }
}
