using Microsoft.EntityFrameworkCore;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Enums;
using ToDoManagementSystem.Persistence.Context;

namespace ToDoManagementSystem.Persistence.Repositories;

/// <summary>Task-specific repository with filtering, search, and reminder query support.</summary>
public class TaskRepository : GenericRepository<TaskItem>, ITaskRepository
{
    public TaskRepository(AppDbContext context) : base(context) { }

    /// <summary>Returns all non-deleted tasks for a specific user.</summary>
    public async Task<IEnumerable<TaskItem>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await _dbSet.Where(t => t.UserId == userId).ToListAsync(ct);

    /// <summary>Returns a filtered, paginated set of tasks with optional search, status, priority, and date filters.</summary>
    public async Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetFilteredAsync(
        Guid userId,
        TaskFilterRequest filter,
        CancellationToken ct = default)
    {
        IQueryable<TaskItem> query = _dbSet.Where(t => t.UserId == userId);

        if (filter.Status.HasValue)
            query = query.Where(t => (int)t.Status == filter.Status.Value);

        if (filter.Priority.HasValue)
            query = query.Where(t => (int)t.Priority == filter.Priority.Value);

        if (filter.DueDateFrom.HasValue)
            query = query.Where(t => t.DueDate >= filter.DueDateFrom.Value);

        if (filter.DueDateTo.HasValue)
            query = query.Where(t => t.DueDate <= filter.DueDateTo.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            string term = filter.SearchTerm.ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(term) ||
                (t.Description != null && t.Description.ToLower().Contains(term)));
        }

        int totalCount = await query.CountAsync(ct);

        List<TaskItem> items = await query
            .OrderByDescending(t => t.CreatedDate)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    /// <summary>Returns tasks due within the next 24 hours that are not completed.</summary>
    public async Task<IEnumerable<TaskItem>> GetUpcomingDueTasksAsync(CancellationToken ct = default)
    {
        DateTime threshold = DateTime.UtcNow.AddHours(24);
        return await _dbSet
            .Where(t => t.DueDate <= threshold && t.DueDate >= DateTime.UtcNow && t.Status != DomainTaskStatus.Completed)
            .ToListAsync(ct);
    }

    /// <summary>Returns all tasks whose due date has passed and are not completed.</summary>
    public async Task<IEnumerable<TaskItem>> GetOverdueTasksAsync(CancellationToken ct = default) =>
        await _dbSet
            .Where(t => t.DueDate < DateTime.UtcNow && t.Status != DomainTaskStatus.Completed)
            .ToListAsync(ct);
}
