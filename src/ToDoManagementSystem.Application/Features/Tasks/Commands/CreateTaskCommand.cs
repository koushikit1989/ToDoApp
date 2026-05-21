using MediatR;
using ToDoManagementSystem.Application.DTOs.Tasks;

namespace ToDoManagementSystem.Application.Features.Tasks.Commands;

/// <summary>Command to create a new task for the current user.</summary>
public class CreateTaskCommand : IRequest<TaskResponse>
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; } = 2;
    public DateTime DueDate { get; set; }
}
