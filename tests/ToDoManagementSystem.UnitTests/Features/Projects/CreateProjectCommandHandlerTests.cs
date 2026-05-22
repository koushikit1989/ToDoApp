using AutoMapper;
using FluentAssertions;
using Moq;
using ToDoManagementSystem.Application.DTOs.Projects;
using ToDoManagementSystem.Application.Features.Projects.Commands;
using ToDoManagementSystem.Application.Features.Projects.Handlers;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Application.Mappings;
using ToDoManagementSystem.Domain.Entities;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Features.Projects;

public class CreateProjectCommandHandlerTests
{
    private readonly Mock<IProjectRepository> _projectRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly IMapper _mapper;

    public CreateProjectCommandHandlerTests()
    {
        MapperConfiguration config = new(cfg => cfg.AddProfile<ProjectMappingProfile>());
        _mapper = config.CreateMapper();
    }

    private CreateProjectCommandHandler CreateHandler() =>
        new(_projectRepoMock.Object, _unitOfWorkMock.Object, _mapper);

    [Fact]
    public async Task Handle_ValidRequest_ReturnsCreatedProject()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        CreateProjectCommand command = new()
        {
            ProjectName = "Alpha Project",
            ProjectCode = "ALPHA",
            CreatedBy = userId
        };

        _projectRepoMock.Setup(r => r.ProjectNameExistsAsync("Alpha Project", null, default)).ReturnsAsync(false);
        _projectRepoMock.Setup(r => r.AddAsync(It.IsAny<Project>(), default)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        ProjectResponse result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProjectName.Should().Be("Alpha Project");
        result.ProjectCode.Should().Be("ALPHA");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DuplicateName_ThrowsValidationException()
    {
        // Arrange
        CreateProjectCommand command = new()
        {
            ProjectName = "Duplicate",
            CreatedBy = Guid.NewGuid()
        };

        _projectRepoMock.Setup(r => r.ProjectNameExistsAsync("Duplicate", null, default)).ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ToDoManagementSystem.Domain.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task Handle_SetsCreatedByCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        CreateProjectCommand command = new()
        {
            ProjectName = "My Project",
            CreatedBy = userId
        };

        Project? captured = null;
        _projectRepoMock.Setup(r => r.ProjectNameExistsAsync("My Project", null, default)).ReturnsAsync(false);
        _projectRepoMock.Setup(r => r.AddAsync(It.IsAny<Project>(), default))
            .Callback<Project, CancellationToken>((p, _) => captured = p)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.CreatedBy.Should().Be(userId);
        captured.IsDeleted.Should().BeFalse();
        captured.IsActive.Should().BeTrue();
    }
}
