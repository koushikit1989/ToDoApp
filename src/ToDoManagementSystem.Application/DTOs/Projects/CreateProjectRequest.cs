namespace ToDoManagementSystem.Application.DTOs.Projects;

/// <summary>Payload for creating a new project.</summary>
public class CreateProjectRequest
{
    public string ProjectName { get; set; } = string.Empty;
    public string? ProjectCode { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
