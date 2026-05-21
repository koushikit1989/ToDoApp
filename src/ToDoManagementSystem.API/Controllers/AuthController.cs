using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoManagementSystem.Application.DTOs.Auth;
using ToDoManagementSystem.Application.Features.Authentication.Commands;
using ToDoManagementSystem.Shared.Responses;

namespace ToDoManagementSystem.API.Controllers;

/// <summary>Authentication endpoints: register, login, token refresh, password reset.</summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>Registers a new user account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        RegisterUserCommand command = new()
        {
            FullName = request.FullName,
            Email = request.Email,
            Password = request.Password
        };

        LoginResponse result = await _mediator.Send(command, ct);
        return StatusCode(201, ApiResponse<LoginResponse>.Created(result, "User registered successfully."));
    }

    /// <summary>Authenticates a user with email/password and returns JWT tokens.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        LoginCommand command = new() { Email = request.Email, Password = request.Password };
        LoginResponse result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<LoginResponse>.Ok(result, "Login successful."));
    }

    /// <summary>Exchanges a refresh token for a new JWT access/refresh token pair.</summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        RefreshTokenCommand command = new() { RefreshToken = request.RefreshToken };
        LoginResponse result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<LoginResponse>.Ok(result, "Token refreshed."));
    }

    /// <summary>Sends a password-reset email to the given address.</summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        string baseUrl = $"{Request.Scheme}://{Request.Host}/reset-password";
        ForgotPasswordCommand command = new() { Email = request.Email, BaseResetUrl = baseUrl };
        bool result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<bool>.Ok(result, "If the email is registered, a reset link has been sent."));
    }

    /// <summary>Resets the user password using a valid reset token.</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        ResetPasswordCommand command = new() { Token = request.Token, NewPassword = request.NewPassword };
        bool result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<bool>.Ok(result, "Password reset successfully."));
    }
}
