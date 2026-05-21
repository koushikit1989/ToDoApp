using AutoMapper;
using FluentAssertions;
using Moq;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Application.Features.Tasks.Commands;
using ToDoManagementSystem.Application.Features.Tasks.Handlers;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Application.Mappings;
using ToDoManagementSystem.Domain.Entities;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Features.Tasks;

public class CreateTaskCommandHandlerTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly IMapper _mapper;

    public CreateTaskCommandHandlerTests()
    {
        MapperConfiguration config = new(cfg => cfg.AddProfile<TaskMappingProfile>());
        _mapper = config.CreateMapper();
    }

    private CreateTaskCommandHandler CreateHandler() =>
        new(_taskRepoMock.Object, _unitOfWorkMock.Object, _mapper);

    [Fact]
    public async Task Handle_ValidRequest_ReturnsCreatedTask()
    {
        // Arrange
        CreateTaskCommand command = new()
        {
            UserId = Guid.NewGuid(),
            Title = "Write unit tests",
            Priority = 2,
            DueDate = DateTime.UtcNow.AddDays(3)
        };

        _taskRepoMock.Setup(r => r.AddAsync(It.IsAny<TaskItem>(), default)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        CreateTaskCommandHandler handler = CreateHandler();

        // Act
        TaskResponse result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Write unit tests");
        result.Priority.Should().Be("Medium");
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_CreatesTask_WithCorrectUserId()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        CreateTaskCommand command = new()
        {
            UserId = userId,
            Title = "My Task",
            Priority = 3,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        TaskItem? capturedTask = null;
        _taskRepoMock.Setup(r => r.AddAsync(It.IsAny<TaskItem>(), default))
            .Callback<TaskItem, CancellationToken>((t, _) => capturedTask = t)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        CreateTaskCommandHandler handler = CreateHandler();

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedTask.Should().NotBeNull();
        capturedTask!.UserId.Should().Be(userId);
        capturedTask.Title.Should().Be("My Task");
    }
}
