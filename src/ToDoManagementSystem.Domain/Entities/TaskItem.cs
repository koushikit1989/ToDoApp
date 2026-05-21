using ToDoManagementSystem.Domain.Common;
using ToDoManagementSystem.Domain.Enums;
using DomainTaskStatus = ToDoManagementSystem.Domain.Enums.TaskStatus;

namespace ToDoManagementSystem.Domain.Entities;

/// <summary>Task domain entity with soft-delete support.</summary>
public class TaskItem : BaseEntity
{
    /// <summary>Owner user identifier.</summary>
    public Guid UserId { get; set; }

    /// <summary>Short title of the task.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional detailed description.</summary>
    public string? Description { get; set; }

    /// <summary>Task priority level.</summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>Current lifecycle status.</summary>
    public DomainTaskStatus Status { get; set; } = DomainTaskStatus.Pending;

    /// <summary>Date by which the task should be completed (UTC).</summary>
    public DateTime DueDate { get; set; }

    /// <summary>Soft-delete flag — never hard-delete rows from the Tasks table.</summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>Navigation: owner user.</summary>
    public User User { get; set; } = null!;
}
