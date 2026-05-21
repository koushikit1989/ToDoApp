namespace ToDoManagementSystem.Domain.Common;

/// <summary>Abstract base class providing audit properties for all domain entities.</summary>
public abstract class BaseEntity
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>UTC last-modified timestamp.</summary>
    public DateTime? UpdatedDate { get; set; }
}
