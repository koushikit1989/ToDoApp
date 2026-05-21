using MediatR;
using ToDoManagementSystem.Application.DTOs.Tasks;

namespace ToDoManagementSystem.Application.Features.Tasks.Commands;

/// <summary>Command to update only the status of a task.</summary>
public class MarkTaskStatusCommand : IRequest<TaskResponse>
{
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public int Status { get; set; }
}
