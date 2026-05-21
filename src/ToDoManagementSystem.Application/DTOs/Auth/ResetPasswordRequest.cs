namespace ToDoManagementSystem.Application.DTOs.Auth;

/// <summary>Payload for completing the password-reset flow.</summary>
public class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
