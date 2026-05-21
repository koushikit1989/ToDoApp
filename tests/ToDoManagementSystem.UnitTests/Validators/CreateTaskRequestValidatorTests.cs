using FluentAssertions;
using FluentValidation.Results;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Application.Validators;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Validators;

public class CreateTaskRequestValidatorTests
{
    private readonly CreateTaskRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidRequest_PassesAllRules()
    {
        // Arrange
        CreateTaskRequest request = new()
        {
            Title = "Valid Task Title",
            Priority = 2,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyTitle_FailsValidation()
    {
        // Arrange
        CreateTaskRequest request = new()
        {
            Title = "",
            Priority = 2,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task Validate_PastDueDate_FailsValidation()
    {
        // Arrange
        CreateTaskRequest request = new()
        {
            Title = "Valid Title",
            Priority = 2,
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DueDate");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(-1)]
    public async Task Validate_InvalidPriority_FailsValidation(int priority)
    {
        // Arrange
        CreateTaskRequest request = new()
        {
            Title = "Valid Title",
            Priority = priority,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Priority");
    }

    [Fact]
    public async Task Validate_TitleTooLong_FailsValidation()
    {
        // Arrange
        CreateTaskRequest request = new()
        {
            Title = new string('A', 301),
            Priority = 2,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task Validate_DescriptionTooLong_FailsValidation()
    {
        // Arrange
        CreateTaskRequest request = new()
        {
            Title = "Valid Title",
            Description = new string('X', 2001),
            Priority = 2,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }
}
