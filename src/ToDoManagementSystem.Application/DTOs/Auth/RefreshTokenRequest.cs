namespace ToDoManagementSystem.Application.DTOs.Auth;

/// <summary>Payload for exchanging a refresh token for a new token pair.</summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
