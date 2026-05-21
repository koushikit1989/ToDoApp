using MediatR;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Shared.Responses;

namespace ToDoManagementSystem.Application.Features.Tasks.Queries;

/// <summary>Query to retrieve a paginated list of all tasks for the current user.</summary>
public class GetAllTasksQuery : IRequest<PagedResponse<TaskResponse>>
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
