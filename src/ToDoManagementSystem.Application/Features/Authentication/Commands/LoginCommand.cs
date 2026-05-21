using MediatR;
using ToDoManagementSystem.Application.DTOs.Auth;

namespace ToDoManagementSystem.Application.Features.Authentication.Commands;

/// <summary>Command to authenticate a user with email/password credentials.</summary>
public class LoginCommand : IRequest<LoginResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
