using AutoMapper;
using FluentAssertions;
using Moq;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Application.Features.Tasks.Handlers;
using ToDoManagementSystem.Application.Features.Tasks.Queries;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Application.Mappings;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Enums;
using ToDoManagementSystem.Domain.Exceptions;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Features.Tasks;

public class GetTaskByIdQueryHandlerTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock = new();
    private readonly IMapper _mapper;

    public GetTaskByIdQueryHandlerTests()
    {
        MapperConfiguration config = new(cfg => cfg.AddProfile<TaskMappingProfile>());
        _mapper = config.CreateMapper();
    }

    private GetTaskByIdQueryHandler CreateHandler() =>
        new(_taskRepoMock.Object, _mapper);

    private static TaskItem MakeTask(Guid? userId = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId ?? Guid.NewGuid(),
        Title = "Find Me",
        Priority = TaskPriority.High,
        Status = DomainTaskStatus.InProgress,
        DueDate = DateTime.UtcNow.AddDays(2),
        CreatedDate = DateTime.UtcNow
    };

    [Fact]
    public async Task Handle_ExistingTask_ReturnsTaskResponse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskItem task = MakeTask(userId);
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id, default)).ReturnsAsync(task);

        GetTaskByIdQuery query = new() { TaskId = task.Id, UserId = userId };

        // Act
        TaskResponse result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);
        result.Title.Should().Be("Find Me");
        result.Priority.Should().Be("High");
        result.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task Handle_TaskNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _taskRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((TaskItem?)null);

        GetTaskByIdQuery query = new() { TaskId = Guid.NewGuid(), UserId = Guid.NewGuid() };

        // Act
        Func<Task> act = () => CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_TaskBelongsToAnotherUser_ThrowsNotFoundException()
    {
        // Arrange
        TaskItem task = MakeTask(Guid.NewGuid());
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id, default)).ReturnsAsync(task);

        GetTaskByIdQuery query = new() { TaskId = task.Id, UserId = Guid.NewGuid() };

        // Act
        Func<Task> act = () => CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_OverdueTask_ReturnsIsOverdueTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskItem task = MakeTask(userId);
        task.DueDate = DateTime.UtcNow.AddDays(-1); // past due
        task.Status = DomainTaskStatus.Pending;
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id, default)).ReturnsAsync(task);

        GetTaskByIdQuery query = new() { TaskId = task.Id, UserId = userId };

        // Act
        TaskResponse result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.IsOverdue.Should().BeTrue();
    }
}
