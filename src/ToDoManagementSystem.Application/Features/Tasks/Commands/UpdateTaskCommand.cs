using MediatR;
using ToDoManagementSystem.Application.DTOs.Tasks;

namespace ToDoManagementSystem.Application.Features.Tasks.Commands;

/// <summary>Command to update an existing task.</summary>
public class UpdateTaskCommand : IRequest<TaskResponse>
{
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? Priority { get; set; }
    public int? Status { get; set; }
    public DateTime? DueDate { get; set; }
}
