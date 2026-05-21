using AutoMapper;
using FluentAssertions;
using Moq;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Application.Features.Tasks.Commands;
using ToDoManagementSystem.Application.Features.Tasks.Handlers;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Application.Mappings;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Enums;
using ToDoManagementSystem.Domain.Exceptions;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Features.Tasks;

public class MarkTaskStatusCommandHandlerTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly IMapper _mapper;

    public MarkTaskStatusCommandHandlerTests()
    {
        MapperConfiguration config = new(cfg => cfg.AddProfile<TaskMappingProfile>());
        _mapper = config.CreateMapper();
    }

    private MarkTaskStatusCommandHandler CreateHandler() =>
        new(_taskRepoMock.Object, _unitOfWorkMock.Object, _mapper);

    private static TaskItem MakeTask(Guid? userId = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId ?? Guid.NewGuid(),
        Title = "Status Task",
        Priority = TaskPriority.Medium,
        Status = DomainTaskStatus.Pending,
        DueDate = DateTime.UtcNow.AddDays(3),
        CreatedDate = DateTime.UtcNow
    };

    [Fact]
    public async Task Handle_ValidRequest_UpdatesStatus()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskItem task = MakeTask(userId);
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id, default)).ReturnsAsync(task);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        MarkTaskStatusCommand command = new()
        {
            TaskId = task.Id,
            UserId = userId,
            Status = (int)DomainTaskStatus.InProgress
        };

        // Act
        TaskResponse result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task Handle_MarkAsCompleted_ReturnsCompletedStatus()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskItem task = MakeTask(userId);
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id, default)).ReturnsAsync(task);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        MarkTaskStatusCommand command = new()
        {
            TaskId = task.Id,
            UserId = userId,
            Status = (int)DomainTaskStatus.Completed
        };

        // Act
        TaskResponse result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Completed");
        task.UpdatedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_TaskNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _taskRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((TaskItem?)null);

        MarkTaskStatusCommand command = new()
        {
            TaskId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = (int)DomainTaskStatus.Completed
        };

        // Act
        Func<Task> act = () => CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_TaskBelongsToAnotherUser_ThrowsNotFoundException()
    {
        // Arrange
        TaskItem task = MakeTask(Guid.NewGuid());
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id, default)).ReturnsAsync(task);

        MarkTaskStatusCommand command = new()
        {
            TaskId = task.Id,
            UserId = Guid.NewGuid(),
            Status = (int)DomainTaskStatus.Completed
        };

        // Act
        Func<Task> act = () => CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
