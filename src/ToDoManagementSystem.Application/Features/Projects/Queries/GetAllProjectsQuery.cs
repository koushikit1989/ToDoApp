using MediatR;
using ToDoManagementSystem.Application.DTOs.Projects;

namespace ToDoManagementSystem.Application.Features.Projects.Queries;

/// <summary>Query to retrieve all active projects (available for task assignment).</summary>
public class GetAllProjectsQuery : IRequest<IEnumerable<ProjectResponse>>
{
    /// <summary>When true, returns only active projects. Default: false (returns all non-deleted).</summary>
    public bool ActiveOnly { get; set; } = false;
}
