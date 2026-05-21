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

public class UpdateTaskCommandHandlerTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly IMapper _mapper;

    public UpdateTaskCommandHandlerTests()
    {
        MapperConfiguration config = new(cfg => cfg.AddProfile<TaskMappingProfile>());
        _mapper = config.CreateMapper();
    }

    private UpdateTaskCommandHandler CreateHandler() =>
        new(_taskRepoMock.Object, _unitOfWorkMock.Object, _mapper);

    private static TaskItem MakeTask(Guid? userId = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId ?? Guid.NewGuid(),
        Title = "Original Title",
        Priority = TaskPriority.Low,
        Status = DomainTaskStatus.Pending,
        DueDate = DateTime.UtcNow.AddDays(5),
        CreatedDate = DateTime.UtcNow
    };

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsUpdatedTask()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskItem task = MakeTask(userId);
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id, default)).ReturnsAsync(task);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        UpdateTaskCommand command = new()
        {
            TaskId = task.Id,
            UserId = userId,
            Title = "Updated Title",
            Priority = (int)TaskPriority.High
        };

        // Act
        TaskResponse result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.Title.Should().Be("Updated Title");
        result.Priority.Should().Be("High");
    }

    [Fact]
    public async Task Handle_PartialUpdate_OnlyChangesProvidedFields()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskItem task = MakeTask(userId);
        string originalTitle = task.Title;
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id, default)).ReturnsAsync(task);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        UpdateTaskCommand command = new()
        {
            TaskId = task.Id,
            UserId = userId,
            Priority = (int)TaskPriority.High
            // Title not provided — should stay the same
        };

        // Act
        TaskResponse result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.Title.Should().Be(originalTitle);
        result.Priority.Should().Be("High");
    }

    [Fact]
    public async Task Handle_TaskNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _taskRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((TaskItem?)null);

        UpdateTaskCommand command = new() { TaskId = Guid.NewGuid(), UserId = Guid.NewGuid(), Title = "X" };

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

        UpdateTaskCommand command = new()
        {
            TaskId = task.Id,
            UserId = Guid.NewGuid(), // different user
            Title = "Hack"
        };

        // Act
        Func<Task> act = () => CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidUpdate_CallsRepositoryUpdate()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskItem task = MakeTask(userId);
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id, default)).ReturnsAsync(task);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        UpdateTaskCommand command = new() { TaskId = task.Id, UserId = userId, Title = "New" };

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        _taskRepoMock.Verify(r => r.Update(task), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
