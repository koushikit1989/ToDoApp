using FluentValidation;
using ToDoManagementSystem.Application.DTOs.Projects;

namespace ToDoManagementSystem.Application.Validators;

/// <summary>Validates project update request input.</summary>
public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.ProjectName)
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters.")
            .NotEmpty().WithMessage("Project name cannot be empty when provided.")
            .When(x => x.ProjectName is not null);

        RuleFor(x => x.ProjectCode)
            .MaximumLength(50).WithMessage("Project code must not exceed 50 characters.")
            .When(x => x.ProjectCode is not null);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}
