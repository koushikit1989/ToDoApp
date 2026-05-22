using MediatR;

namespace ToDoManagementSystem.Application.Features.Projects.Commands;

/// <summary>Command to soft-delete a project.</summary>
public class DeleteProjectCommand : IRequest<bool>
{
    public Guid ProjectId { get; set; }
    public Guid RequestedBy { get; set; }
}
