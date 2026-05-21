using AutoMapper;
using MediatR;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Application.Features.Dashboard.Queries;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;

namespace ToDoManagementSystem.Application.Features.Dashboard.Handlers;

/// <summary>Returns all tasks for a user ordered by due date, used for Excel export.</summary>
public class ExportTasksQueryHandler : IRequestHandler<ExportTasksQuery, IEnumerable<TaskResponse>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;

    public ExportTasksQueryHandler(ITaskRepository taskRepository, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
    }

    /// <summary>Fetches all user tasks with no page limit.</summary>
    public async Task<IEnumerable<TaskResponse>> Handle(ExportTasksQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<TaskItem> tasks = await _taskRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        return _mapper.Map<IEnumerable<TaskResponse>>(tasks.OrderBy(t => t.DueDate));
    }
}
