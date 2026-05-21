using MediatR;

namespace ToDoManagementSystem.Application.Features.Authentication.Commands;

/// <summary>Command to initiate the password-reset email flow.</summary>
public class ForgotPasswordCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;

    /// <summary>Base URL for the reset link (e.g. https://myapp.com/reset-password).</summary>
    public string BaseResetUrl { get; set; } = string.Empty;
}
