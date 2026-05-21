using MediatR;
using ToDoManagementSystem.Application.DTOs.Tasks;

namespace ToDoManagementSystem.Application.Features.Tasks.Queries;

/// <summary>Query to retrieve a single task by its ID.</summary>
public class GetTaskByIdQuery : IRequest<TaskResponse>
{
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
}
