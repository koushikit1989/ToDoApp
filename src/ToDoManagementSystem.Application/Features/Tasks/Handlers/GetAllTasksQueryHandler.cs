using AutoMapper;
using MediatR;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Application.Features.Tasks.Queries;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Shared.Responses;

namespace ToDoManagementSystem.Application.Features.Tasks.Handlers;

/// <summary>Returns a paginated list of all non-deleted tasks for the current user.</summary>
public class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, PagedResponse<TaskResponse>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;

    public GetAllTasksQueryHandler(ITaskRepository taskRepository, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
    }

    /// <summary>Fetches user tasks and wraps the page in a PagedResponse.</summary>
    public async Task<PagedResponse<TaskResponse>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
    {
        TaskFilterRequest filter = new()
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        (IEnumerable<TaskItem> items, int totalCount) = await _taskRepository.GetFilteredAsync(request.UserId, filter, cancellationToken);

        IEnumerable<TaskResponse> mapped = _mapper.Map<IEnumerable<TaskResponse>>(items);
        return PagedResponse<TaskResponse>.Create(mapped, totalCount, request.PageNumber, request.PageSize);
    }
}
