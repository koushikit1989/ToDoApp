using MediatR;
using ToDoManagementSystem.Application.DTOs.Auth;

namespace ToDoManagementSystem.Application.Features.Authentication.Commands;

/// <summary>Command to register a new user and return an initial login response.</summary>
public class RegisterUserCommand : IRequest<LoginResponse>
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
