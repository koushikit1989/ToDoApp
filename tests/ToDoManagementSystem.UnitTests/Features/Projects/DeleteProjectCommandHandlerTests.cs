using FluentAssertions;
using Moq;
using ToDoManagementSystem.Application.Features.Projects.Commands;
using ToDoManagementSystem.Application.Features.Projects.Handlers;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Exceptions;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Features.Projects;

public class DeleteProjectCommandHandlerTests
{
    private readonly Mock<IProjectRepository> _projectRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private DeleteProjectCommandHandler CreateHandler() =>
        new(_projectRepoMock.Object, _unitOfWorkMock.Object);

    [Fact]
    public async Task Handle_ExistingProject_SoftDeletes()
    {
        // Arrange
        Guid projectId = Guid.NewGuid();
        Project project = new()
        {
            Id = projectId,
            ProjectName = "To Delete",
            IsDeleted = false,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, default)).ReturnsAsync(project);
        _projectRepoMock.Setup(r => r.Update(It.IsAny<Project>()));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        bool result = await CreateHandler().Handle(
            new DeleteProjectCommand { ProjectId = projectId, RequestedBy = Guid.NewGuid() },
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        project.IsDeleted.Should().BeTrue();
        project.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AlreadyDeleted_ThrowsNotFoundException()
    {
        // Arrange
        Guid projectId = Guid.NewGuid();
        Project project = new()
        {
            Id = projectId,
            ProjectName = "Gone",
            IsDeleted = true,
            CreatedDate = DateTime.UtcNow
        };

        _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, default)).ReturnsAsync(project);

        // Act
        Func<Task> act = async () => await CreateHandler().Handle(
            new DeleteProjectCommand { ProjectId = projectId, RequestedBy = Guid.NewGuid() },
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
