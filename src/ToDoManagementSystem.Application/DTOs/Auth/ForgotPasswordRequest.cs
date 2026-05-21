namespace ToDoManagementSystem.Application.DTOs.Auth;

/// <summary>Payload for initiating the password-reset flow.</summary>
public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}
