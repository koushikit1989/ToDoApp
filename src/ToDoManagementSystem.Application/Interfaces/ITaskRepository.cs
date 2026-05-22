using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Domain.Entities;

namespace ToDoManagementSystem.Application.Interfaces;

/// <summary>Task-specific repository extending the generic CRUD interface.</summary>
public interface ITaskRepository : IRepository<TaskItem>
{
    /// <summary>Gets all non-deleted tasks belonging to a user.</summary>
    Task<IEnumerable<TaskItem>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Gets a filtered, paginated page of tasks for a user.</summary>
    Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetFilteredAsync(
        Guid userId,
        TaskFilterRequest filter,
        CancellationToken ct = default);

    /// <summary>Gets all non-deleted tasks belonging to a user with Project navigation loaded.</summary>
    Task<IEnumerable<TaskItem>> GetByUserIdWithProjectAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Gets tasks with DueDate within the next 24 hours that are not yet completed.</summary>
    Task<IEnumerable<TaskItem>> GetUpcomingDueTasksAsync(CancellationToken ct = default);

    /// <summary>Gets overdue tasks (DueDate in the past, not completed).</summary>
    Task<IEnumerable<TaskItem>> GetOverdueTasksAsync(CancellationToken ct = default);
}
