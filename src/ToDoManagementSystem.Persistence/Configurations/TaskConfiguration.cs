using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Enums;

namespace ToDoManagementSystem.Persistence.Configurations;

/// <summary>EF Core Fluent API configuration for the Tasks table with global soft-delete filter.</summary>
public class TaskConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(TaskPriority.Medium);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(DomainTaskStatus.Pending);

        builder.Property(t => t.DueDate)
            .IsRequired();

        builder.Property(t => t.CreatedDate)
            .IsRequired();

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Global query filter — soft-deleted tasks are never returned
        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.HasOne(t => t.User)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(t => t.ProjectId);

        builder.HasIndex(t => t.ProjectId);
    }
}
