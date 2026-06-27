using FluentAssertions;
using Moq;
using ToDoManagementSystem.Application.Features.Authentication.Commands;
using ToDoManagementSystem.Application.Features.Authentication.Handlers;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Features.Auth;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private ForgotPasswordCommandHandler CreateHandler() =>
        new(_userRepoMock.Object, _emailServiceMock.Object, _unitOfWorkMock.Object);

    private static User MakeUser() => new()
    {
        Id = Guid.NewGuid(),
        FullName = "Jane Doe",
        Email = "jane@example.com",
        PasswordHash = "hash",
        Role = "User",
        IsActive = true
    };

    [Fact]
    public async Task Handle_ExistingEmail_ReturnsTrue()
    {
        // Arrange
        User user = MakeUser();
        _userRepoMock.Setup(r => r.GetByEmailAsync("jane@example.com", default)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.Update(user));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _emailServiceMock.Setup(e => e.SendPasswordResetEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        ForgotPasswordCommand command = new() { Email = "jane@example.com", BaseResetUrl = "https://app.example.com/reset" };

        // Act
        bool result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NonExistentEmail_StillReturnsTrue()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((User?)null);

        ForgotPasswordCommand command = new() { Email = "nobody@example.com", BaseResetUrl = "https://app.example.com/reset" };

        // Act
        bool result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert — prevents email enumeration
        result.Should().BeTrue();
        _emailServiceMock.Verify(
            e => e.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingEmail_SendsPasswordResetEmail()
    {
        // Arrange
        User user = MakeUser();
        _userRepoMock.Setup(r => r.GetByEmailAsync("jane@example.com", default)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.Update(user));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _emailServiceMock.Setup(e => e.SendPasswordResetEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        ForgotPasswordCommand command = new() { Email = "jane@example.com", BaseResetUrl = "https://app.example.com/reset" };

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            e => e.SendPasswordResetEmailAsync("jane@example.com", "Jane Doe", It.IsAny<string>(), default),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingEmail_SetsPasswordResetTokenOnUser()
    {
        // Arrange
        User user = MakeUser();
        _userRepoMock.Setup(r => r.GetByEmailAsync("jane@example.com", default)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.Update(It.IsAny<User>()));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _emailServiceMock.Setup(e => e.SendPasswordResetEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        ForgotPasswordCommand command = new() { Email = "jane@example.com", BaseResetUrl = "https://app.example.com/reset" };

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        user.PasswordResetToken.Should().NotBeNullOrEmpty();
        user.PasswordResetExpiry.Should().BeAfter(DateTime.UtcNow);
        _userRepoMock.Verify(r => r.Update(user), Times.Once);
    }
}
