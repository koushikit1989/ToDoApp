using FluentAssertions;
using Moq;
using ToDoManagementSystem.Application.DTOs.Dashboard;
using ToDoManagementSystem.Application.Features.Dashboard.Handlers;
using ToDoManagementSystem.Application.Features.Dashboard.Queries;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Enums;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Features.Dashboard;

public class GetDashboardSummaryQueryHandlerTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock = new();

    private GetDashboardSummaryQueryHandler CreateHandler() =>
        new(_taskRepoMock.Object);

    private static TaskItem Make(DomainTaskStatus status, DateTime? dueDate = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        Title = "T",
        Priority = TaskPriority.Medium,
        Status = status,
        DueDate = dueDate ?? DateTime.UtcNow.AddDays(1),
        CreatedDate = DateTime.UtcNow
    };

    [Fact]
    public async Task Handle_NoTasks_ReturnsAllZeroes()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _taskRepoMock.Setup(r => r.GetByUserIdWithProjectAsync(userId, default))
            .ReturnsAsync(Enumerable.Empty<TaskItem>());

        GetDashboardSummaryQuery query = new() { UserId = userId };

        // Act
        DashboardSummaryResponse result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.TotalTasks.Should().Be(0);
        result.CompletedTasks.Should().Be(0);
        result.PendingTasks.Should().Be(0);
        result.InProgressTasks.Should().Be(0);
        result.OverdueTasks.Should().Be(0);
        result.CompletionRate.Should().Be(0);
    }

    [Fact]
    public async Task Handle_MixedStatuses_ReturnsCorrectCounts()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        List<TaskItem> tasks = new()
        {
            Make(DomainTaskStatus.Pending),
            Make(DomainTaskStatus.Pending),
            Make(DomainTaskStatus.InProgress),
            Make(DomainTaskStatus.Completed),
            Make(DomainTaskStatus.Completed)
        };
        _taskRepoMock.Setup(r => r.GetByUserIdWithProjectAsync(userId, default)).ReturnsAsync(tasks);

        GetDashboardSummaryQuery query = new() { UserId = userId };

        // Act
        DashboardSummaryResponse result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.TotalTasks.Should().Be(5);
        result.PendingTasks.Should().Be(2);
        result.InProgressTasks.Should().Be(1);
        result.CompletedTasks.Should().Be(2);
    }

    [Fact]
    public async Task Handle_OverdueTasks_CountsOnlyNonCompleted()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        DateTime past = DateTime.UtcNow.AddDays(-1);
        List<TaskItem> tasks = new()
        {
            Make(DomainTaskStatus.Pending, past),    // overdue
            Make(DomainTaskStatus.InProgress, past), // overdue
            Make(DomainTaskStatus.Completed, past)   // NOT overdue (completed)
        };
        _taskRepoMock.Setup(r => r.GetByUserIdWithProjectAsync(userId, default)).ReturnsAsync(tasks);

        GetDashboardSummaryQuery query = new() { UserId = userId };

        // Act
        DashboardSummaryResponse result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.OverdueTasks.Should().Be(2);
    }

    [Fact]
    public async Task Handle_AllCompleted_Returns100PercentCompletionRate()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        List<TaskItem> tasks = new()
        {
            Make(DomainTaskStatus.Completed),
            Make(DomainTaskStatus.Completed)
        };
        _taskRepoMock.Setup(r => r.GetByUserIdWithProjectAsync(userId, default)).ReturnsAsync(tasks);

        GetDashboardSummaryQuery query = new() { UserId = userId };

        // Act
        DashboardSummaryResponse result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.CompletionRate.Should().Be(100);
    }

    [Fact]
    public async Task Handle_HalfCompleted_Returns50PercentCompletionRate()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        List<TaskItem> tasks = new()
        {
            Make(DomainTaskStatus.Completed),
            Make(DomainTaskStatus.Pending)
        };
        _taskRepoMock.Setup(r => r.GetByUserIdWithProjectAsync(userId, default)).ReturnsAsync(tasks);

        GetDashboardSummaryQuery query = new() { UserId = userId };

        // Act
        DashboardSummaryResponse result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.CompletionRate.Should().Be(50);
    }
}
