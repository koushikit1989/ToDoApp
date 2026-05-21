using FluentAssertions;
using FluentValidation.Results;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Application.Validators;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Validators;

public class UpdateTaskRequestValidatorTests
{
    private readonly UpdateTaskRequestValidator _validator = new();

    [Fact]
    public void Validate_EmptyRequest_PassesValidation()
    {
        // All fields optional — empty object should pass
        UpdateTaskRequest request = new();

        ValidationResult result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_TitleTooLong_FailsValidation()
    {
        // Arrange
        UpdateTaskRequest request = new() { Title = new string('A', 301) };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_TitleAtMaxLength_PassesValidation()
    {
        // Arrange
        UpdateTaskRequest request = new() { Title = new string('A', 300) };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidPriority_FailsValidation()
    {
        // Arrange
        UpdateTaskRequest request = new() { Priority = 99 };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Priority");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Validate_ValidPriority_PassesValidation(int priority)
    {
        // Arrange
        UpdateTaskRequest request = new() { Priority = priority };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidStatus_FailsValidation()
    {
        // Arrange
        UpdateTaskRequest request = new() { Status = 5 };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Validate_ValidStatus_PassesValidation(int status)
    {
        // Arrange
        UpdateTaskRequest request = new() { Status = status };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_PastDueDate_FailsValidation()
    {
        // Arrange
        UpdateTaskRequest request = new() { DueDate = DateTime.UtcNow.AddDays(-1) };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DueDate");
    }

    [Fact]
    public void Validate_DescriptionTooLong_FailsValidation()
    {
        // Arrange
        UpdateTaskRequest request = new() { Description = new string('X', 2001) };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }
}
