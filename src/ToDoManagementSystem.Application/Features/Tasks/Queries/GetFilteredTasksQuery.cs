using MediatR;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Shared.Responses;

namespace ToDoManagementSystem.Application.Features.Tasks.Queries;

/// <summary>Query to retrieve a filtered, paginated list of tasks.</summary>
public class GetFilteredTasksQuery : IRequest<PagedResponse<TaskResponse>>
{
    public Guid UserId { get; set; }
    public TaskFilterRequest Filter { get; set; } = new();
}
