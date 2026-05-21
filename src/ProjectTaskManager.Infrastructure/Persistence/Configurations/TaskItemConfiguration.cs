using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectTaskManager.Domain.Entities;

namespace ProjectTaskManager.Infrastructure.Persistence.Configurations;

public sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.CreatedAt)
            .HasColumnType("datetime2");

        builder.Property(t => t.DueDate)
            .HasColumnType("datetime2");

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(t => t.ProjectId)
            .HasDatabaseName("IX_Tasks_ProjectId");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("IX_Tasks_Status");

        builder.HasIndex(t => t.DueDate)
            .HasDatabaseName("IX_Tasks_DueDate");

        builder.HasIndex(t => new { t.ProjectId, t.Status })
            .HasDatabaseName("IX_Tasks_ProjectId_Status");
    }
}
