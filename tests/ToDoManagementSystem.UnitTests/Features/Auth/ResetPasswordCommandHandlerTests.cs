using FluentAssertions;
using Moq;
using ToDoManagementSystem.Application.Features.Authentication.Commands;
using ToDoManagementSystem.Application.Features.Authentication.Handlers;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Exceptions;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Features.Auth;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private ResetPasswordCommandHandler CreateHandler() =>
        new(_userRepoMock.Object, _unitOfWorkMock.Object);

    private static (User user, string rawToken) MakeUserWithResetToken(DateTime? expiry = null)
    {
        string rawToken = "raw-reset-token-12345";
        User user = new()
        {
            Id = Guid.NewGuid(),
            FullName = "Reset User",
            Email = "reset@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass@1", workFactor: 4),
            Role = "User",
            IsActive = true,
            PasswordResetToken = BCrypt.Net.BCrypt.HashPassword(rawToken, workFactor: 4),
            PasswordResetExpiry = expiry ?? DateTime.UtcNow.AddHours(1)
        };
        return (user, rawToken);
    }

    [Fact]
    public async Task Handle_ValidToken_ReturnsTrue()
    {
        // Arrange
        (User user, string rawToken) = MakeUserWithResetToken();
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });
        _userRepoMock.Setup(r => r.Update(user));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        ResetPasswordCommand command = new() { Token = rawToken, NewPassword = "NewPass@123" };

        // Act
        bool result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvalidToken_ThrowsValidationException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User>());

        ResetPasswordCommand command = new() { Token = "wrong-token", NewPassword = "NewPass@123" };

        // Act
        Func<Task> act = () => CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Invalid or expired*");
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsValidationException()
    {
        // Arrange — token is valid but already expired
        (User user, string rawToken) = MakeUserWithResetToken(expiry: DateTime.UtcNow.AddSeconds(-1));
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });

        ResetPasswordCommand command = new() { Token = rawToken, NewPassword = "NewPass@123" };

        // Act
        Func<Task> act = () => CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_ValidToken_UpdatesPasswordHash()
    {
        // Arrange
        (User user, string rawToken) = MakeUserWithResetToken();
        string originalHash = user.PasswordHash;
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });
        _userRepoMock.Setup(r => r.Update(user));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        ResetPasswordCommand command = new() { Token = rawToken, NewPassword = "BrandNew@999" };

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        user.PasswordHash.Should().NotBe(originalHash);
        BCrypt.Net.BCrypt.Verify("BrandNew@999", user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidToken_ClearsResetToken()
    {
        // Arrange
        (User user, string rawToken) = MakeUserWithResetToken();
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });
        _userRepoMock.Setup(r => r.Update(user));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        ResetPasswordCommand command = new() { Token = rawToken, NewPassword = "NewPass@123" };

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetExpiry.Should().BeNull();
    }
}
