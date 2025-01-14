using DeploymentManagementSystem.Data.Heplers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeploymentManagementSystem.Data.Configurations
{
    public class EnvironmentConfiguraton : IEntityTypeConfiguration<Models.Environment>
    {
        private const string ModelName = "environment";
        private const string TableName = $"cd_{ModelName}";

        public void Configure(EntityTypeBuilder<Models.Environment> builder)
        {
            builder.ToTable(TableName)
                .HasKey(p => p.Id)
                .HasName($"pk_{TableName}_id");

            builder.Property(p => p.Id)
                    .ValueGeneratedOnAdd()
                    .HasColumnName($"{ModelName}_id")
                    .HasColumnType(ColumnType.Int)
                    .HasComment("Идентфикатор записи окружения");

            builder.Property(p => p.Name)
                    .IsRequired()
                    .HasColumnName($"c_{ModelName}_name")
                    .HasColumnType(ColumnType.String).HasMaxLength(100)
                    .HasComment("Название окружения");

            builder.Property(p => p.Url)
                    .IsRequired()
                    .HasColumnName($"c_{ModelName}_url")
                    .HasColumnType(ColumnType.String).HasMaxLength(100)
                    .HasComment("URL окружения");

            builder.Property(p => p.Status)
                    .IsRequired()
                    .HasColumnName($"c_{ModelName}_status")
                    .HasColumnType(ColumnType.String).HasMaxLength(100)
                    .HasComment("Статус окружения");

            builder.Property(p => p.Type)
                    .IsRequired()
                    .HasColumnName($"c_{ModelName}_type")
                    .HasColumnType(ColumnType.String).HasMaxLength(100)
                    .HasComment("Тип окружения");

            builder.Property(p => p.LastDeploymentDate)
                .HasColumnName($"c_{ModelName}_last_deployment_date")
                .HasColumnType(ColumnType.DateTime)
                .HasComment("Дата последнего развертывания окружения");

            builder.Property(p => p.ProjectId)
                .IsRequired()
                .HasColumnName($"f_{ModelName}_project_id")
                .HasColumnType(ColumnType.Int)
                .HasComment("Идентификатор проекта");

            builder
                .HasOne(p => p.Project)
                .WithMany()
                .HasForeignKey(p => p.ProjectId)
                .HasConstraintName($"fk_f_{ModelName}_project_id")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => p.ProjectId, $"idx_{TableName}_fk_f_{ModelName}_project_id");
        }
    }
}
