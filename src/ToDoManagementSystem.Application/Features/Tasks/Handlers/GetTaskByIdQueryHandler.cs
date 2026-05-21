using AutoMapper;
using MediatR;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Application.Features.Tasks.Queries;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Exceptions;

namespace ToDoManagementSystem.Application.Features.Tasks.Handlers;

/// <summary>Retrieves a single task by ID, scoped to the current user.</summary>
public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskResponse>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;

    public GetTaskByIdQueryHandler(ITaskRepository taskRepository, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
    }

    /// <summary>Fetches the task and maps it to a DTO.</summary>
    public async Task<TaskResponse> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        TaskItem? task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);

        if (task is null || task.UserId != request.UserId)
            throw new NotFoundException(nameof(TaskItem), request.TaskId);

        return _mapper.Map<TaskResponse>(task);
    }
}
