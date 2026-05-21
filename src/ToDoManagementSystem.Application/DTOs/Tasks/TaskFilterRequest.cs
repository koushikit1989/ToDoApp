namespace ToDoManagementSystem.Application.DTOs.Tasks;

/// <summary>Filter and pagination parameters for task queries.</summary>
public class TaskFilterRequest
{
    public int? Status { get; set; }
    public int? Priority { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
