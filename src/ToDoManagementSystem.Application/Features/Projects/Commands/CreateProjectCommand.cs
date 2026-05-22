using MediatR;
using ToDoManagementSystem.Application.DTOs.Projects;

namespace ToDoManagementSystem.Application.Features.Projects.Commands;

/// <summary>Command to create a new project.</summary>
public class CreateProjectCommand : IRequest<ProjectResponse>
{
    public string ProjectName { get; set; } = string.Empty;
    public string? ProjectCode { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid CreatedBy { get; set; }
}
