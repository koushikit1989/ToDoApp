using AutoMapper;
using FluentAssertions;
using Moq;
using ToDoManagementSystem.Application.DTOs.Dashboard;
using ToDoManagementSystem.Application.Features.Dashboard.Handlers;
using ToDoManagementSystem.Application.Features.Dashboard.Queries;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Application.Mappings;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Enums;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Features.Dashboard;

public class GetReportsQueryHandlerTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock = new();
    private readonly IMapper _mapper;

    public GetReportsQueryHandlerTests()
    {
        MapperConfiguration config = new(cfg => cfg.AddProfile<TaskMappingProfile>());
        _mapper = config.CreateMapper();
    }

    private GetReportsQueryHandler CreateHandler() =>
        new(_taskRepoMock.Object, _mapper);

    private static TaskItem MakeTask(TaskPriority priority = TaskPriority.Medium,
        DomainTaskStatus status = DomainTaskStatus.Pending,
        DateTime? createdDate = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        Title = "Task",
        Priority = priority,
        Status = status,
        DueDate = DateTime.UtcNow.AddDays(1),
        CreatedDate = createdDate ?? DateTime.UtcNow
    };

    [Fact]
    public async Task Handle_NoTasks_ReturnsEmptyReport()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _taskRepoMock.Setup(r => r.GetByUserIdAsync(userId, default))
            .ReturnsAsync(Enumerable.Empty<TaskItem>());

        GetReportsQuery query = new() { UserId = userId };

        // Act
        ReportResponse result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.Summary.TotalTasks.Should().Be(0);
        result.RecentTasks.Should().BeEmpty();
        result.TasksByPriority.Should().BeEmpty();
        result.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_MoreThan10Tasks_RecentTasksLimitedTo10()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        List<TaskItem> tasks = Enumerable.Range(0, 15)
            .Select(i => MakeTask(createdDate: DateTime.UtcNow.AddMinutes(-i)))
            .ToList();
        _taskRepoMock.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(tasks);

        GetReportsQuery query = new() { UserId = userId };

        // Act
        ReportResponse result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.RecentTasks.Should().HaveCount(10);
    }

    [Fact]
    public async Task Handle_RecentTasks_OrderedByCreatedDateDescending()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskItem oldest = MakeTask(createdDate: DateTime.UtcNow.AddDays(-5));
        oldest.Title = "Oldest";
        TaskItem newest = MakeTask(createdDate: DateTime.UtcNow);
        newest.Title = "Newest";
        _taskRepoMock.Setup(r => r.GetByUserIdAsync(userId, default))
            .ReturnsAsync(new List<TaskItem> { oldest, newest });

        GetReportsQuery query = new() { UserId = userId };

        // Act
        ReportResponse result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.RecentTasks.First().Title.Should().Be("Newest");
    }

    [Fact]
    public async Task Handle_TasksByPriority_GroupsCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        List<TaskItem> tasks = new()
        {
            MakeTask(TaskPriority.High),
            MakeTask(TaskPriority.High),
            MakeTask(TaskPriority.Low)
        };
        _taskRepoMock.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(tasks);

        GetReportsQuery query = new() { UserId = userId };

        // Act
        ReportResponse result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        List<TasksByPriorityResponse> byPriority = result.TasksByPriority.ToList();
        byPriority.Should().HaveCount(2);
        byPriority.Single(p => p.Priority == "High").Count.Should().Be(2);
        byPriority.Single(p => p.Priority == "Low").Count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_SummaryStats_MatchTaskList()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        List<TaskItem> tasks = new()
        {
            MakeTask(status: DomainTaskStatus.Completed),
            MakeTask(status: DomainTaskStatus.Pending),
            MakeTask(status: DomainTaskStatus.InProgress)
        };
        _taskRepoMock.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(tasks);

        GetReportsQuery query = new() { UserId = userId };

        // Act
        ReportResponse result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.Summary.TotalTasks.Should().Be(3);
        result.Summary.CompletedTasks.Should().Be(1);
        result.Summary.PendingTasks.Should().Be(1);
        result.Summary.InProgressTasks.Should().Be(1);
    }
}
