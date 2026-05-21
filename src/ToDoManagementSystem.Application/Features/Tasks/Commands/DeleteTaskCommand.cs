using MediatR;

namespace ToDoManagementSystem.Application.Features.Tasks.Commands;

/// <summary>Command to soft-delete a task (sets IsDeleted = true).</summary>
public class DeleteTaskCommand : IRequest<bool>
{
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
}
