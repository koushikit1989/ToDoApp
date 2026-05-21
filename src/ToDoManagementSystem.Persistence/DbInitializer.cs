using BCrypt.Net;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Enums;
using ToDoManagementSystem.Persistence.Context;

namespace ToDoManagementSystem.Persistence;

/// <summary>Handles initial database seeding with default users and sample tasks.</summary>
public static class DbInitializer
{
    /// <summary>Seeds the database with an admin user and sample tasks if it's empty.</summary>
    public static async Task SeedAsync(AppDbContext context)
    {
        if (context.Users.Any())
            return;

        User admin = new()
        {
            Id = Guid.Parse("A7D8E2B1-4C3F-4D2A-9B5E-1A2B3C4D5E6F"),
            FullName = "Administrator",
            Email = "admin@todo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12),
            Role = "Admin",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        User user = new()
        {
            Id = Guid.Parse("B8E9F3C2-5D4A-5E3B-AC6F-2B3C4D5E6F7A"),
            FullName = "Sample User",
            Email = "user@todo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123", workFactor: 12),
            Role = "User",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await context.Users.AddRangeAsync(admin, user);

        List<TaskItem> tasks = new()
        {
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = "Welcome to To-Do System",
                Description = "This is a sample task created automatically.",
                Priority = TaskPriority.High,
                Status = DomainTaskStatus.InProgress,
                DueDate = DateTime.UtcNow.AddDays(1),
                CreatedDate = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = "Complete Project Documentation",
                Description = "Ensure all layers are documented with XML comments.",
                Priority = TaskPriority.Medium,
                Status = DomainTaskStatus.Pending,
                DueDate = DateTime.UtcNow.AddDays(3),
                CreatedDate = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = "Review Security Best Practices",
                Description = "Check JWT implementation and rate limiting.",
                Priority = TaskPriority.Low,
                Status = DomainTaskStatus.Completed,
                DueDate = DateTime.UtcNow.AddDays(-1),
                CreatedDate = DateTime.UtcNow
            }
        };

        await context.Tasks.AddRangeAsync(tasks);
        await context.SaveChangesAsync();
    }
}
