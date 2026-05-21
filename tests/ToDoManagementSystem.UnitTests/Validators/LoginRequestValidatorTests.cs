using FluentAssertions;
using FluentValidation.Results;
using ToDoManagementSystem.Application.DTOs.Auth;
using ToDoManagementSystem.Application.Validators;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_PassesValidation()
    {
        // Arrange
        LoginRequest request = new() { Email = "user@example.com", Password = "Secret123" };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_MissingEmail_FailsValidation()
    {
        // Arrange
        LoginRequest request = new() { Email = "", Password = "Secret123" };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_InvalidEmailFormat_FailsValidation()
    {
        // Arrange
        LoginRequest request = new() { Email = "not-an-email", Password = "Secret123" };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("valid email"));
    }

    [Fact]
    public void Validate_MissingPassword_FailsValidation()
    {
        // Arrange
        LoginRequest request = new() { Email = "user@example.com", Password = "" };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_MissingBothFields_ReturnsTwoErrors()
    {
        // Arrange
        LoginRequest request = new() { Email = "", Password = "" };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
