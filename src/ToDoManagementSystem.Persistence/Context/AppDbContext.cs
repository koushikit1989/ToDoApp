using Microsoft.EntityFrameworkCore;
using ToDoManagementSystem.Domain.Entities;

namespace ToDoManagementSystem.Persistence.Context;

/// <summary>Entity Framework Core database context for the To-Do Management System.</summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>Automatically sets UpdatedDate on modified entities before saving.</summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Modified && entry.Entity is Domain.Common.BaseEntity entity)
                entity.UpdatedDate = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
