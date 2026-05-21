using MediatR;

namespace ToDoManagementSystem.Application.Features.Authentication.Commands;

/// <summary>Command to complete the password-reset flow.</summary>
public class ResetPasswordCommand : IRequest<bool>
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
