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

public class GetAllTasksQueryHandlerTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock = new();
    private readonly IMapper _mapper;

    public GetAllTasksQueryHandlerTests()
    {
        MapperConfiguration config = new(cfg => cfg.AddProfile<TaskMappingProfile>());
        _mapper = config.CreateMapper();
    }

    private GetAllTasksQueryHandler CreateHandler() =>
        new(_taskRepoMock.Object, _mapper);

    [Fact]
    public async Task Handle_ReturnsOnlyCurrentUsersTasks()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid otherId = Guid.NewGuid();

        List<TaskItem> userTasks = new()
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Title = "Task 1", Status = DomainTaskStatus.Pending, Priority = TaskPriority.Medium, DueDate = DateTime.UtcNow.AddDays(1), CreatedDate = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), UserId = userId, Title = "Task 2", Status = DomainTaskStatus.Completed, Priority = TaskPriority.High, DueDate = DateTime.UtcNow.AddDays(2), CreatedDate = DateTime.UtcNow }
        };

        _taskRepoMock.Setup(r => r.GetFilteredAsync(userId, It.IsAny<TaskFilterRequest>(), default))
            .ReturnsAsync((userTasks, 2));

        GetAllTasksQueryHandler handler = CreateHandler();
        GetAllTasksQuery query = new() { UserId = userId, PageNumber = 1, PageSize = 20 };

        // Act
        PagedResponse<TaskResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        List<TaskItem> page = new()
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Title = "Paged Task", Status = DomainTaskStatus.Pending, Priority = TaskPriority.Low, DueDate = DateTime.UtcNow.AddDays(5), CreatedDate = DateTime.UtcNow }
        };

        _taskRepoMock.Setup(r => r.GetFilteredAsync(userId, It.IsAny<TaskFilterRequest>(), default))
            .ReturnsAsync((page, 21));

        GetAllTasksQueryHandler handler = CreateHandler();
        GetAllTasksQuery query = new() { UserId = userId, PageNumber = 2, PageSize = 20 };

        // Act
        PagedResponse<TaskResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(20);
        result.TotalCount.Should().Be(21);
        result.TotalPages.Should().Be(2);
    }
}
