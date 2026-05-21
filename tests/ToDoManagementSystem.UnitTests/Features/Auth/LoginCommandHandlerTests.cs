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

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private LoginCommandHandler CreateHandler() =>
        new(_userRepoMock.Object, _tokenServiceMock.Object, _unitOfWorkMock.Object);

    private static User MakeUser(string password = "Test@12345") => new()
    {
        Id = Guid.NewGuid(),
        FullName = "Test User",
        Email = "test@example.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4),
        Role = "User",
        IsActive = true
    };

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        User user = MakeUser();
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), default)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(user)).Returns("access-token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");

        LoginCommand command = new() { Email = "test@example.com", Password = "Test@12345" };

        // Act
        LoginResponse result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.Email.Should().Be("test@example.com");
        result.Role.Should().Be("User");
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        User user = MakeUser("CorrectPassword");
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);

        LoginCommand command = new() { Email = "test@example.com", Password = "WrongPassword" };

        // Act
        Func<Task> act = () => CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((User?)null);

        LoginCommand command = new() { Email = "nobody@example.com", Password = "Any" };

        // Act
        Func<Task> act = () => CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_DeactivatedAccount_ThrowsUnauthorizedException()
    {
        // Arrange
        User user = MakeUser();
        user.IsActive = false;
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);

        LoginCommand command = new() { Email = "test@example.com", Password = "Test@12345" };

        // Act
        Func<Task> act = () => CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*deactivated*");
    }

    [Fact]
    public async Task Handle_ValidLogin_PersistsRefreshToken()
    {
        // Arrange
        User user = MakeUser();
        RefreshToken? captured = null;
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), default))
            .Callback<RefreshToken, CancellationToken>((rt, _) => captured = rt)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(user)).Returns("tok");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh-tok");

        LoginCommand command = new() { Email = "test@example.com", Password = "Test@12345" };

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.UserId.Should().Be(user.Id);
        captured.Token.Should().Be("refresh-tok");
        captured.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }
}
