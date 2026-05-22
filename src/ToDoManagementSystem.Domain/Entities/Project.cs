using ToDoManagementSystem.Domain.Common;

namespace ToDoManagementSystem.Domain.Entities;

/// <summary>Project domain entity — groups tasks under a named initiative.</summary>
public class Project : BaseEntity
{
    /// <summary>Display name of the project (unique).</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Optional short code or abbreviation for the project.</summary>
    public string? ProjectCode { get; set; }

    /// <summary>Optional detailed description.</summary>
    public string? Description { get; set; }

    /// <summary>Optional project start date (UTC).</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Optional project end/deadline date (UTC).</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Whether the project is active and available for task assignment.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Soft-delete flag — never hard-delete project rows.</summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>ID of the user who created this project.</summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>Navigation: creator user.</summary>
    public User? Creator { get; set; }

    /// <summary>Navigation: tasks assigned to this project.</summary>
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
