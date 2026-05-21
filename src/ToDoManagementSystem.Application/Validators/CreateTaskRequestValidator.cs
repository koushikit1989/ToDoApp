using FluentValidation;
using ToDoManagementSystem.Application.DTOs.Tasks;

namespace ToDoManagementSystem.Application.Validators;

/// <summary>Validates task creation request input.</summary>
public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(300).WithMessage("Title must not exceed 300 characters.");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 3).WithMessage("Priority must be 1 (Low), 2 (Medium), or 3 (High).");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Due date must be today or a future date.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);
    }
}
