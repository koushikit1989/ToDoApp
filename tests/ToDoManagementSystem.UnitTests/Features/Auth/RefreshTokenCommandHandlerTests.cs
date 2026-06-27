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

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private RefreshTokenCommandHandler CreateHandler() =>
        new(_userRepoMock.Object, _tokenServiceMock.Object, _unitOfWorkMock.Object);

    private static (User user, RefreshToken token) MakeUserWithToken(
        bool isRevoked = false,
        DateTime? expiresAt = null)
    {
        RefreshToken refreshToken = new()
        {
            Id = Guid.NewGuid(),
            Token = "valid-refresh-token",
            IsRevoked = isRevoked,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7)
        };

        User user = new()
        {
            Id = Guid.NewGuid(),
            FullName = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = "User",
            IsActive = true,
            RefreshTokens = new List<RefreshToken> { refreshToken }
        };

        refreshToken.UserId = user.Id;
        return (user, refreshToken);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_ReturnsNewLoginResponse()
    {
        // Arrange
        (User user, RefreshToken _) = MakeUserWithToken();
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(user)).Returns("new-access-token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("new-refresh-token");
        _userRepoMock.Setup(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), default)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        RefreshTokenCommand command = new() { RefreshToken = "valid-refresh-token" };

        // Act
        LoginResponse result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
        result.Email.Should().Be("test@example.com");
        result.UserId.Should().Be(user.Id.ToString());
    }

    [Fact]
    public async Task Handle_UnknownRefreshToken_ThrowsUnauthorizedException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User>());

        RefreshTokenCommand command = new() { RefreshToken = "unknown-token" };

        // Act
        Func<Task> act = () => CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*Invalid refresh token*");
    }

    [Fact]
    public async Task Handle_RevokedToken_ThrowsUnauthorizedException()
    {
        // Arrange
        (User user, RefreshToken _) = MakeUserWithToken(isRevoked: true);
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });

        RefreshTokenCommand command = new() { RefreshToken = "valid-refresh-token" };

        // Act
        Func<Task> act = () => CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*expired or revoked*");
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsUnauthorizedException()
    {
        // Arrange
        (User user, RefreshToken _) = MakeUserWithToken(expiresAt: DateTime.UtcNow.AddSeconds(-1));
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });

        RefreshTokenCommand command = new() { RefreshToken = "valid-refresh-token" };

        // Act
        Func<Task> act = () => CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*expired or revoked*");
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_RevokesOldToken()
    {
        // Arrange
        (User user, RefreshToken storedToken) = MakeUserWithToken();
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(user)).Returns("tok");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("new-tok");
        _userRepoMock.Setup(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), default)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        RefreshTokenCommand command = new() { RefreshToken = "valid-refresh-token" };

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        storedToken.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_PersistsNewToken()
    {
        // Arrange
        (User user, RefreshToken _) = MakeUserWithToken();
        RefreshToken? captured = null;
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(user)).Returns("tok");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("brand-new-token");
        _userRepoMock.Setup(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), default))
            .Callback<RefreshToken, CancellationToken>((rt, _) => captured = rt)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        RefreshTokenCommand command = new() { RefreshToken = "valid-refresh-token" };

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.Token.Should().Be("brand-new-token");
        captured.UserId.Should().Be(user.Id);
        captured.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }
}
