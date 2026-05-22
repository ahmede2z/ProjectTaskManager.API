using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectTaskManager.Domain.Entities;

namespace ProjectTaskManager.Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.CreatedAt)
            .HasColumnType("datetime2");

        builder.Property(p => p.OwnerId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(p => p.OwnerId)
            .HasDatabaseName("IX_Projects_OwnerId");

        builder.HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
