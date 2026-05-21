using AutoMapper;
using FluentAssertions;
using Moq;
using ToDoManagementSystem.Application.DTOs.Auth;
using ToDoManagementSystem.Application.Features.Authentication.Commands;
using ToDoManagementSystem.Application.Features.Authentication.Handlers;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Exceptions;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Features.Auth;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private RegisterUserCommandHandler CreateHandler() =>
        new(_userRepoMock.Object, _tokenServiceMock.Object, _unitOfWorkMock.Object);

    [Fact]
    public async Task Handle_ValidRequest_ReturnsLoginResponse()
    {
        // Arrange
        RegisterUserCommand command = new()
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "P@ssw0rd!"
        };

        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>(), default)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");

        RegisterUserCommandHandler handler = CreateHandler();

        // Act
        LoginResponse result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsValidationException()
    {
        // Arrange
        RegisterUserCommand command = new()
        {
            FullName = "Test",
            Email = "existing@example.com",
            Password = "P@ssw0rd!"
        };

        _userRepoMock.Setup(r => r.EmailExistsAsync("existing@example.com", default)).ReturnsAsync(true);

        RegisterUserCommandHandler handler = CreateHandler();

        // Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*already registered*");
    }
}
