using FluentAssertions;
using Moq;
using ToDoManagementSystem.Application.Features.Tasks.Commands;
using ToDoManagementSystem.Application.Features.Tasks.Handlers;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Enums;
using ToDoManagementSystem.Domain.Exceptions;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Features.Tasks;

public class DeleteTaskCommandHandlerTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private DeleteTaskCommandHandler CreateHandler() =>
        new(_taskRepoMock.Object, _unitOfWorkMock.Object);

    private static TaskItem MakeTask(Guid? userId = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId ?? Guid.NewGuid(),
        Title = "Task to delete",
        Priority = TaskPriority.Medium,
        Status = DomainTaskStatus.Pending,
        DueDate = DateTime.UtcNow.AddDays(1),
        CreatedDate = DateTime.UtcNow
    };

    [Fact]
    public async Task Handle_ValidRequest_ReturnsTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskItem task = MakeTask(userId);
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id, default)).ReturnsAsync(task);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        DeleteTaskCommand command = new() { TaskId = task.Id, UserId = userId };

        // Act
        bool result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidRequest_SetsIsDeletedTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskItem task = MakeTask(userId);
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id, default)).ReturnsAsync(task);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        DeleteTaskCommand command = new() { TaskId = task.Id, UserId = userId };

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        task.IsDeleted.Should().BeTrue();
        task.UpdatedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_TaskNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _taskRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((TaskItem?)null);

        DeleteTaskCommand command = new() { TaskId = Guid.NewGuid(), UserId = Guid.NewGuid() };

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

        DeleteTaskCommand command = new() { TaskId = task.Id, UserId = Guid.NewGuid() };

        // Act
        Func<Task> act = () => CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_SoftDelete_DoesNotCallRepositoryDelete()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskItem task = MakeTask(userId);
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id, default)).ReturnsAsync(task);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        DeleteTaskCommand command = new() { TaskId = task.Id, UserId = userId };

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert — soft delete uses Update, never Delete
        _taskRepoMock.Verify(r => r.Delete(It.IsAny<TaskItem>()), Times.Never);
        _taskRepoMock.Verify(r => r.Update(task), Times.Once);
    }
}
