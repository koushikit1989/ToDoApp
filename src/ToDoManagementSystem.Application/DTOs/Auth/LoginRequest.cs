namespace ToDoManagementSystem.Application.DTOs.Auth;

/// <summary>Payload for authenticating a user.</summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
