using AutoMapper;
using MediatR;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Application.Features.Tasks.Queries;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Shared.Responses;

namespace ToDoManagementSystem.Application.Features.Tasks.Handlers;

/// <summary>Returns a filtered, paginated list of tasks for the current user.</summary>
public class GetFilteredTasksQueryHandler : IRequestHandler<GetFilteredTasksQuery, PagedResponse<TaskResponse>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;

    public GetFilteredTasksQueryHandler(ITaskRepository taskRepository, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
    }

    /// <summary>Applies filter criteria and returns the matching page.</summary>
    public async Task<PagedResponse<TaskResponse>> Handle(GetFilteredTasksQuery request, CancellationToken cancellationToken)
    {
        (IEnumerable<TaskItem> items, int totalCount) = await _taskRepository.GetFilteredAsync(
            request.UserId, request.Filter, cancellationToken);

        IEnumerable<TaskResponse> mapped = _mapper.Map<IEnumerable<TaskResponse>>(items);
        return PagedResponse<TaskResponse>.Create(mapped, totalCount, request.Filter.PageNumber, request.Filter.PageSize);
    }
}
