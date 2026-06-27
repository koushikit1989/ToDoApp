using AutoMapper;
using FluentAssertions;
using Moq;
using ToDoManagementSystem.Application.DTOs.Projects;
using ToDoManagementSystem.Application.Features.Projects.Handlers;
using ToDoManagementSystem.Application.Features.Projects.Queries;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Application.Mappings;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Exceptions;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Features.Projects;

public class GetProjectByIdQueryHandlerTests
{
    private readonly Mock<IProjectRepository> _projectRepoMock = new();
    private readonly IMapper _mapper;

    public GetProjectByIdQueryHandlerTests()
    {
        MapperConfiguration config = new(cfg => cfg.AddProfile<ProjectMappingProfile>());
        _mapper = config.CreateMapper();
    }

    private GetProjectByIdQueryHandler CreateHandler() =>
        new(_projectRepoMock.Object, _mapper);

    private static Project MakeProject(bool isDeleted = false) => new()
    {
        Id = Guid.NewGuid(),
        ProjectName = "Alpha Project",
        ProjectCode = "ALPHA",
        IsActive = true,
        IsDeleted = isDeleted,
        CreatedDate = DateTime.UtcNow
    };

    [Fact]
    public async Task Handle_ExistingProject_ReturnsProjectResponse()
    {
        // Arrange
        Project project = MakeProject();
        _projectRepoMock.Setup(r => r.GetByIdAsync(project.Id, default)).ReturnsAsync(project);

        GetProjectByIdQuery query = new() { ProjectId = project.Id };

        // Act
        ProjectResponse result = await CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(project.Id);
        result.ProjectName.Should().Be("Alpha Project");
    }

    [Fact]
    public async Task Handle_ProjectNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid missingId = Guid.NewGuid();
        _projectRepoMock.Setup(r => r.GetByIdAsync(missingId, default)).ReturnsAsync((Project?)null);

        GetProjectByIdQuery query = new() { ProjectId = missingId };

        // Act
        Func<Task> act = () => CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_DeletedProject_ThrowsNotFoundException()
    {
        // Arrange
        Project project = MakeProject(isDeleted: true);
        _projectRepoMock.Setup(r => r.GetByIdAsync(project.Id, default)).ReturnsAsync(project);

        GetProjectByIdQuery query = new() { ProjectId = project.Id };

        // Act
        Func<Task> act = () => CreateHandler().Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
