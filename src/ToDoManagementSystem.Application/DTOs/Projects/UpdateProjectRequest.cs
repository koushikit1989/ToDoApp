namespace ToDoManagementSystem.Application.DTOs.Projects;

/// <summary>Payload for updating an existing project (all fields optional).</summary>
public class UpdateProjectRequest
{
    public string? ProjectName { get; set; }
    public string? ProjectCode { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}
