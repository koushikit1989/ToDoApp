using FluentValidation;
using ToDoManagementSystem.Application.DTOs.Tasks;

namespace ToDoManagementSystem.Application.Validators;

/// <summary>Validates task update request input (all fields optional).</summary>
public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(300).WithMessage("Title must not exceed 300 characters.")
            .When(x => x.Title is not null);

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 3).WithMessage("Priority must be 1 (Low), 2 (Medium), or 3 (High).")
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.Status)
            .InclusiveBetween(0, 2).WithMessage("Status must be 0 (Pending), 1 (InProgress), or 2 (Completed).")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Due date must be today or a future date.")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);
    }
}
