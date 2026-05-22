using MediatR;
using ToDoManagementSystem.Application.DTOs.Projects;

namespace ToDoManagementSystem.Application.Features.Projects.Queries;

/// <summary>Query to retrieve a single project by its identifier.</summary>
public class GetProjectByIdQuery : IRequest<ProjectResponse>
{
    public Guid ProjectId { get; set; }
}
