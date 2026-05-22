using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoManagementSystem.Domain.Entities;

namespace ToDoManagementSystem.Persistence.Configurations;

/// <summary>EF Core Fluent API configuration for the Projects table.</summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProjectName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(p => p.ProjectName)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.Property(p => p.ProjectCode)
            .HasMaxLength(50);

        builder.Property(p => p.Description);

        builder.Property(p => p.StartDate);
        builder.Property(p => p.EndDate);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedDate)
            .IsRequired();

        // Global query filter: soft-deleted projects are never returned
        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasOne(p => p.Creator)
            .WithMany()
            .HasForeignKey(p => p.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
