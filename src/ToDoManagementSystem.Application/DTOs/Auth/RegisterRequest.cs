namespace ToDoManagementSystem.Application.DTOs.Auth;

/// <summary>Payload for registering a new user account.</summary>
public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
