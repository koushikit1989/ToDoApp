namespace ToDoManagementSystem.Application.DTOs.Tasks;

/// <summary>Task representation returned to API consumers.</summary>
public class TaskResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsOverdue { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
}
