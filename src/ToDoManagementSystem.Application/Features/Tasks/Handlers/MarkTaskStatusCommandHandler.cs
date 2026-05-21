using AutoMapper;
using MediatR;
using Serilog;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Application.Features.Tasks.Commands;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Enums;
using ToDoManagementSystem.Domain.Exceptions;

namespace ToDoManagementSystem.Application.Features.Tasks.Handlers;

/// <summary>Updates only the status field of a task.</summary>
public class MarkTaskStatusCommandHandler : IRequestHandler<MarkTaskStatusCommand, TaskResponse>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MarkTaskStatusCommandHandler(ITaskRepository taskRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>Sets the task status and persists the change.</summary>
    public async Task<TaskResponse> Handle(MarkTaskStatusCommand request, CancellationToken cancellationToken)
    {
        TaskItem? task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);

        if (task is null || task.UserId != request.UserId)
            throw new NotFoundException(nameof(TaskItem), request.TaskId);

        task.Status = (DomainTaskStatus)request.Status;
        task.UpdatedDate = DateTime.UtcNow;

        _taskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Log.ForContext<MarkTaskStatusCommandHandler>()
           .Information("Task {TaskId} status changed to {Status}", request.TaskId, task.Status);

        return _mapper.Map<TaskResponse>(task);
    }
}
