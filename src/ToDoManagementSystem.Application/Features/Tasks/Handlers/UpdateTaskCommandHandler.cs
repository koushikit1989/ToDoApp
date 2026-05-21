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

/// <summary>Updates an existing task belonging to the authenticated user.</summary>
public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskResponse>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateTaskCommandHandler(ITaskRepository taskRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>Applies partial updates to the task and saves changes.</summary>
    public async Task<TaskResponse> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        TaskItem? task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);

        if (task is null || task.UserId != request.UserId)
            throw new NotFoundException(nameof(TaskItem), request.TaskId);

        if (request.Title is not null) task.Title = request.Title;
        if (request.Description is not null) task.Description = request.Description;
        if (request.Priority.HasValue) task.Priority = (TaskPriority)request.Priority.Value;
        if (request.Status.HasValue) task.Status = (DomainTaskStatus)request.Status.Value;
        if (request.DueDate.HasValue) task.DueDate = request.DueDate.Value;
        task.UpdatedDate = DateTime.UtcNow;

        _taskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Log.ForContext<UpdateTaskCommandHandler>()
           .Information("Task updated: {TaskId}", request.TaskId);

        return _mapper.Map<TaskResponse>(task);
    }
}
