using AutoMapper;
using FluentAssertions;
using Moq;
using ToDoManagementSystem.Application.DTOs.Projects;
using ToDoManagementSystem.Application.Features.Projects.Commands;
using ToDoManagementSystem.Application.Features.Projects.Handlers;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Application.Mappings;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Exceptions;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Features.Projects;

public class UpdateProjectCommandHandlerTests
{
    private readonly Mock<IProjectRepository> _projectRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly IMapper _mapper;

    public UpdateProjectCommandHandlerTests()
    {
        MapperConfiguration config = new(cfg => cfg.AddProfile<ProjectMappingProfile>());
        _mapper = config.CreateMapper();
    }

    private UpdateProjectCommandHandler CreateHandler() =>
        new(_projectRepoMock.Object, _unitOfWorkMock.Object, _mapper);

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsUpdatedProject()
    {
        // Arrange
        Guid projectId = Guid.NewGuid();
        Project existing = new()
        {
            Id = projectId,
            ProjectName = "Old Name",
            IsActive = true,
            IsDeleted = false,
            CreatedDate = DateTime.UtcNow
        };

        UpdateProjectCommand command = new()
        {
            ProjectId = projectId,
            RequestedBy = Guid.NewGuid(),
            ProjectName = "New Name",
            IsActive = false
        };

        _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, default)).ReturnsAsync(existing);
        _projectRepoMock.Setup(r => r.ProjectNameExistsAsync("New Name", projectId, default)).ReturnsAsync(false);
        _projectRepoMock.Setup(r => r.Update(It.IsAny<Project>()));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        ProjectResponse result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.ProjectName.Should().Be("New Name");
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ProjectNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid projectId = Guid.NewGuid();
        _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, default)).ReturnsAsync((Project?)null);

        UpdateProjectCommand command = new() { ProjectId = projectId, RequestedBy = Guid.NewGuid() };

        // Act
        Func<Task> act = async () => await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
