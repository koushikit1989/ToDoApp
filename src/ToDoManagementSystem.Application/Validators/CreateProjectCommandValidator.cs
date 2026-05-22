using FluentValidation;
using ToDoManagementSystem.Application.Features.Projects.Commands;

namespace ToDoManagementSystem.Application.Validators;

/// <summary>Validates CreateProjectCommand before it reaches the handler.</summary>
public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters.");

        RuleFor(x => x.ProjectCode)
            .MaximumLength(50).WithMessage("Project code must not exceed 50 characters.")
            .When(x => x.ProjectCode is not null);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}
