namespace ToDoManagementSystem.Application.DTOs.Tasks;

/// <summary>Payload for creating a new task.</summary>
public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; } = 2;
    public DateTime DueDate { get; set; }
    public Guid? ProjectId { get; set; }
}
