using AutoMapper;
using FluentAssertions;
using Moq;
using ToDoManagementSystem.Application.DTOs.Projects;
using ToDoManagementSystem.Application.Features.Projects.Handlers;
using ToDoManagementSystem.Application.Features.Projects.Queries;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Application.Mappings;
using ToDoManagementSystem.Domain.Entities;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Features.Projects;

public class GetAllProjectsQueryHandlerTests
{
    private readonly Mock<IProjectRepository> _projectRepoMock = new();
    private readonly IMapper _mapper;

    public GetAllProjectsQueryHandlerTests()
    {
        MapperConfiguration config = new(cfg => cfg.AddProfile<ProjectMappingProfile>());
        _mapper = config.CreateMapper();
    }

    private GetAllProjectsQueryHandler CreateHandler() =>
        new(_projectRepoMock.Object, _mapper);

    [Fact]
    public async Task Handle_ActiveOnlyFalse_CallsGetProjectsWithTasks()
    {
        // Arrange
        List<Project> projects = new()
        {
            new Project { Id = Guid.NewGuid(), ProjectName = "P1", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Project { Id = Guid.NewGuid(), ProjectName = "P2", IsActive = false, CreatedDate = DateTime.UtcNow }
        };

        _projectRepoMock.Setup(r => r.GetProjectsWithTasksAsync(default)).ReturnsAsync(projects);

        // Act
        IEnumerable<ProjectResponse> result = await CreateHandler().Handle(
            new GetAllProjectsQuery { ActiveOnly = false }, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        _projectRepoMock.Verify(r => r.GetProjectsWithTasksAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ActiveOnlyTrue_CallsGetAllActive()
    {
        // Arrange
        List<Project> projects = new()
        {
            new Project { Id = Guid.NewGuid(), ProjectName = "Active Only", IsActive = true, CreatedDate = DateTime.UtcNow }
        };

        _projectRepoMock.Setup(r => r.GetAllActiveAsync(default)).ReturnsAsync(projects);

        // Act
        IEnumerable<ProjectResponse> result = await CreateHandler().Handle(
            new GetAllProjectsQuery { ActiveOnly = true }, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        _projectRepoMock.Verify(r => r.GetAllActiveAsync(default), Times.Once);
    }
}
