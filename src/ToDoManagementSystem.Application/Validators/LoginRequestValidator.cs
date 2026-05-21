using FluentValidation;
using ToDoManagementSystem.Application.DTOs.Auth;

namespace ToDoManagementSystem.Application.Validators;

/// <summary>Validates login request input.</summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
