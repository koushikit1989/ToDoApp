using MediatR;
using ToDoManagementSystem.Application.DTOs.Projects;

namespace ToDoManagementSystem.Application.Features.Projects.Commands;

/// <summary>Command to update an existing project.</summary>
public class UpdateProjectCommand : IRequest<ProjectResponse>
{
    public Guid ProjectId { get; set; }
    public Guid RequestedBy { get; set; }
    public string? ProjectName { get; set; }
    public string? ProjectCode { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}
