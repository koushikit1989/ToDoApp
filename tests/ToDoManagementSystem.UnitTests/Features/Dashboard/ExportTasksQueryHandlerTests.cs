using AutoMapper;
using FluentAssertions;
using Moq;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Application.Features.Dashboard.Handlers;
using ToDoManagementSystem.Application.Features.Dashboard.Queries;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Application.Mappings;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Enums;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Features.Dashboard;

public class ExportTasksQueryHandlerTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock = new();
    private readonly IMapper _mapper;

    public ExportTasksQueryHandlerTests()
    {
        MapperConfiguration config = new(cfg => cfg.AddProfile<TaskMappingProfile>());
        _mapper = config.CreateMapper();
    }

    private ExportTasksQueryHandler CreateHandler() =>
        new(_taskRepoMock.Object, _mapper);

    private static TaskItem MakeTask(Guid userId, string title, DateTime dueDate) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Title = title,
        Priority = TaskPriority.Low,
        Status = DomainTaskStatus.Pending,
        DueDate = dueDate,
        CreatedDate = DateTime.UtcNow
    };

    [Fact]
    public async Task Handle_ReturnsAllUserTasks()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        List<TaskItem> tasks = new()
        {
            MakeTask(userId, "Task One", DateTime.UtcNow.AddDays(2)),
            MakeTask(userId, "Task Two", DateTime.UtcNow.AddDays(1)),
            MakeTask(userId, "Task Three", DateTime.UtcNow.AddDays(3))
        };

        _taskRepoMock.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(tasks);

        ExportTasksQuery query = new() { UserId = userId };

        // Act
        IEnumerable<TaskResponse> result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_TasksOrderedByDueDate_EarliestFirst()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        DateTime earliest = DateTime.UtcNow.AddDays(1);
        DateTime middle = DateTime.UtcNow.AddDays(3);
        DateTime latest = DateTime.UtcNow.AddDays(7);

        List<TaskItem> tasks = new()
        {
            MakeTask(userId, "Late Task", latest),
            MakeTask(userId, "Early Task", earliest),
            MakeTask(userId, "Middle Task", middle)
        };

        _taskRepoMock.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(tasks);

        ExportTasksQuery query = new() { UserId = userId };

        // Act
        List<TaskResponse> result = (await CreateHandler().Handle(query, CancellationToken.None)).ToList();

        // Assert
        result[0].Title.Should().Be("Early Task");
        result[1].Title.Should().Be("Middle Task");
        result[2].Title.Should().Be("Late Task");
    }

    [Fact]
    public async Task Handle_NoTasks_ReturnsEmpty()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _taskRepoMock.Setup(r => r.GetByUserIdAsync(userId, default))
            .ReturnsAsync(Enumerable.Empty<TaskItem>());

        ExportTasksQuery query = new() { UserId = userId };

        // Act
        IEnumerable<TaskResponse> result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
