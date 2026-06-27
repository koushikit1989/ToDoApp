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

public class GetProjectDashboardQueryHandlerTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock = new();

    private GetProjectDashboardQueryHandler CreateHandler() =>
        new(_taskRepoMock.Object);

    private static Project MakeProject(string name = "Project A") => new()
    {
        Id = Guid.NewGuid(),
        ProjectName = name,
        IsActive = true,
        IsDeleted = false,
        CreatedDate = DateTime.UtcNow
    };

    private static TaskItem MakeTask(Guid userId, Project project, DomainTaskStatus status) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Title = "Task",
        Priority = TaskPriority.Medium,
        Status = status,
        DueDate = DateTime.UtcNow.AddDays(3),
        ProjectId = project.Id,
        Project = project,
        CreatedDate = DateTime.UtcNow
    };

    [Fact]
    public async Task Handle_NoProjectTasks_ReturnsEmpty()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskItem taskWithoutProject = new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Unassigned",
            Status = DomainTaskStatus.Pending,
            Priority = TaskPriority.Low,
            DueDate = DateTime.UtcNow.AddDays(1),
            ProjectId = null,
            Project = null,
            CreatedDate = DateTime.UtcNow
        };

        _taskRepoMock.Setup(r => r.GetByUserIdWithProjectAsync(userId, default))
            .ReturnsAsync(new List<TaskItem> { taskWithoutProject });

        GetProjectDashboardQuery query = new() { UserId = userId };

        // Act
        IEnumerable<ProjectTaskSummary> result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_TasksGroupedByProject_ReturnsCorrectSummaries()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Project projectA = MakeProject("Project A");
        Project projectB = MakeProject("Project B");

        List<TaskItem> tasks = new()
        {
            MakeTask(userId, projectA, DomainTaskStatus.Completed),
            MakeTask(userId, projectA, DomainTaskStatus.Pending),
            MakeTask(userId, projectB, DomainTaskStatus.InProgress)
        };

        _taskRepoMock.Setup(r => r.GetByUserIdWithProjectAsync(userId, default)).ReturnsAsync(tasks);

        GetProjectDashboardQuery query = new() { UserId = userId };

        // Act
        List<ProjectTaskSummary> result = (await CreateHandler().Handle(query, CancellationToken.None)).ToList();

        // Assert
        result.Should().HaveCount(2);

        ProjectTaskSummary summaryA = result.Single(s => s.ProjectName == "Project A");
        summaryA.TotalTasks.Should().Be(2);
        summaryA.CompletedTasks.Should().Be(1);
        summaryA.PendingTasks.Should().Be(1);
        summaryA.InProgressTasks.Should().Be(0);

        ProjectTaskSummary summaryB = result.Single(s => s.ProjectName == "Project B");
        summaryB.TotalTasks.Should().Be(1);
        summaryB.InProgressTasks.Should().Be(1);
    }

    [Fact]
    public async Task Handle_CompletionRate_CalculatedCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Project project = MakeProject("Calc Project");

        List<TaskItem> tasks = new()
        {
            MakeTask(userId, project, DomainTaskStatus.Completed),
            MakeTask(userId, project, DomainTaskStatus.Completed),
            MakeTask(userId, project, DomainTaskStatus.Pending)
        };

        _taskRepoMock.Setup(r => r.GetByUserIdWithProjectAsync(userId, default)).ReturnsAsync(tasks);

        GetProjectDashboardQuery query = new() { UserId = userId };

        // Act
        List<ProjectTaskSummary> result = (await CreateHandler().Handle(query, CancellationToken.None)).ToList();

        // Assert — 2 of 3 completed = 66.67%
        result.Single().CompletionRate.Should().BeApproximately(66.67, precision: 0.01);
    }

    [Fact]
    public async Task Handle_UnassignedTasks_AreExcluded()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Project project = MakeProject();
        TaskItem assignedTask = MakeTask(userId, project, DomainTaskStatus.Pending);
        TaskItem unassignedTask = new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "No Project",
            Status = DomainTaskStatus.Pending,
            Priority = TaskPriority.Low,
            DueDate = DateTime.UtcNow.AddDays(1),
            ProjectId = null,
            Project = null,
            CreatedDate = DateTime.UtcNow
        };

        _taskRepoMock.Setup(r => r.GetByUserIdWithProjectAsync(userId, default))
            .ReturnsAsync(new List<TaskItem> { assignedTask, unassignedTask });

        GetProjectDashboardQuery query = new() { UserId = userId };

        // Act
        List<ProjectTaskSummary> result = (await CreateHandler().Handle(query, CancellationToken.None)).ToList();

        // Assert — only the project task counts, unassigned is excluded
        result.Should().HaveCount(1);
        result.Single().TotalTasks.Should().Be(1);
    }
}
