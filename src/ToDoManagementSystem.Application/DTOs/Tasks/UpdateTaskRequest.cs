namespace ToDoManagementSystem.Application.DTOs.Tasks;

/// <summary>Payload for updating an existing task (all fields optional).</summary>
public class UpdateTaskRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? Priority { get; set; }
    public int? Status { get; set; }
    public DateTime? DueDate { get; set; }
}
