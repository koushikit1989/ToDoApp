using FluentAssertions;
using FluentValidation.Results;
using ToDoManagementSystem.Application.DTOs.Projects;
using ToDoManagementSystem.Application.Validators;
using Xunit;

namespace ToDoManagementSystem.UnitTests.Validators;

public class CreateProjectRequestValidatorTests
{
    private readonly CreateProjectRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_PassesValidation()
    {
        // Arrange
        CreateProjectRequest request = new()
        {
            ProjectName = "My Project",
            ProjectCode = "MP",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyProjectName_FailsValidation()
    {
        // Arrange
        CreateProjectRequest request = new() { ProjectName = "" };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProjectName");
    }

    [Fact]
    public void Validate_TooLongProjectName_FailsValidation()
    {
        // Arrange
        CreateProjectRequest request = new() { ProjectName = new string('X', 201) };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_EndDateBeforeStartDate_FailsValidation()
    {
        // Arrange
        CreateProjectRequest request = new()
        {
            ProjectName = "Test",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(5)
        };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EndDate");
    }

    [Fact]
    public void Validate_TooLongProjectCode_FailsValidation()
    {
        // Arrange
        CreateProjectRequest request = new()
        {
            ProjectName = "Valid",
            ProjectCode = new string('A', 51)
        };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
