using MediatR;
using Serilog;
using ToDoManagementSystem.Application.Features.Tasks.Commands;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Exceptions;

namespace ToDoManagementSystem.Application.Features.Tasks.Handlers;

/// <summary>Soft-deletes a task by setting IsDeleted = true.</summary>
public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaskCommandHandler(ITaskRepository taskRepository, IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>Marks the task as deleted without removing the database row.</summary>
    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        TaskItem? task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);

        if (task is null || task.UserId != request.UserId)
            throw new NotFoundException(nameof(TaskItem), request.TaskId);

        task.IsDeleted = true;
        task.UpdatedDate = DateTime.UtcNow;

        _taskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Log.ForContext<DeleteTaskCommandHandler>()
           .Information("Task soft-deleted: {TaskId}", request.TaskId);

        return true;
    }
}
