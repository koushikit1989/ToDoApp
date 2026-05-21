using MediatR;
using ToDoManagementSystem.Application.DTOs.Auth;

namespace ToDoManagementSystem.Application.Features.Authentication.Commands;

/// <summary>Command to exchange a refresh token for a new JWT pair.</summary>
public class RefreshTokenCommand : IRequest<LoginResponse>
{
    public string RefreshToken { get; set; } = string.Empty;
}
