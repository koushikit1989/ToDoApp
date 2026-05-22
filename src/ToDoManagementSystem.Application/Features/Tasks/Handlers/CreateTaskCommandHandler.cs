using AutoMapper;
using MediatR;
using Serilog;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Application.Features.Tasks.Commands;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Enums;

namespace ToDoManagementSystem.Application.Features.Tasks.Handlers;

/// <summary>Creates a new task for the authenticated user.</summary>
public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskResponse>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateTaskCommandHandler(ITaskRepository taskRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>Persists a new TaskItem and returns its DTO representation.</summary>
    public async Task<TaskResponse> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        TaskItem task = new()
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Title,
            Description = request.Description,
            Priority = (TaskPriority)request.Priority,
            Status = DomainTaskStatus.Pending,
            DueDate = request.DueDate,
            CreatedDate = DateTime.UtcNow,
            ProjectId = request.ProjectId
        };

        await _taskRepository.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Log.ForContext<CreateTaskCommandHandler>()
           .Information("Task created: {TaskId} for user: {UserId}", task.Id, request.UserId);

        return _mapper.Map<TaskResponse>(task);
    }
}
