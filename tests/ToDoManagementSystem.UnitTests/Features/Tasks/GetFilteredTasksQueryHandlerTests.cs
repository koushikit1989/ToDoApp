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
using ToDoManagementSystem.Shared.Responses;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Features.Tasks;

public class GetFilteredTasksQueryHandlerTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock = new();
    private readonly IMapper _mapper;

    public GetFilteredTasksQueryHandlerTests()
    {
        MapperConfiguration config = new(cfg => cfg.AddProfile<TaskMappingProfile>());
        _mapper = config.CreateMapper();
    }

    private GetFilteredTasksQueryHandler CreateHandler() =>
        new(_taskRepoMock.Object, _mapper);

    private static TaskItem MakeTask(Guid userId, string title = "Task") => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Title = title,
        Priority = TaskPriority.Medium,
        Status = DomainTaskStatus.Pending,
        DueDate = DateTime.UtcNow.AddDays(5),
        CreatedDate = DateTime.UtcNow
    };

    [Fact]
    public async Task Handle_ReturnsFilteredResults_WithCorrectPagination()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        List<TaskItem> tasks = new() { MakeTask(userId, "A"), MakeTask(userId, "B") };
        TaskFilterRequest filter = new() { PageNumber = 1, PageSize = 20 };

        _taskRepoMock.Setup(r => r.GetFilteredAsync(userId, filter, default))
            .ReturnsAsync((tasks, 2));

        GetFilteredTasksQuery query = new() { UserId = userId, Filter = filter };

        // Act
        PagedResponse<TaskResponse> result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Handle_EmptyResults_ReturnsEmptyPagedResponse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskFilterRequest filter = new() { PageNumber = 1, PageSize = 20 };

        _taskRepoMock.Setup(r => r.GetFilteredAsync(userId, filter, default))
            .ReturnsAsync((Enumerable.Empty<TaskItem>(), 0));

        GetFilteredTasksQuery query = new() { UserId = userId, Filter = filter };

        // Act
        PagedResponse<TaskResponse> result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_PassesFilterCriteriaToRepository()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TaskFilterRequest filter = new() { Status = 1, Priority = 3, PageNumber = 2, PageSize = 10 };

        _taskRepoMock.Setup(r => r.GetFilteredAsync(userId, filter, default))
            .ReturnsAsync((Enumerable.Empty<TaskItem>(), 0));

        GetFilteredTasksQuery query = new() { UserId = userId, Filter = filter };

        // Act
        await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        _taskRepoMock.Verify(r => r.GetFilteredAsync(userId, filter, default), Times.Once);
    }
}
