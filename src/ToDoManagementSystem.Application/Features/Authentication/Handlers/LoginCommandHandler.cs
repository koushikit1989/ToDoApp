using MediatR;
using Serilog;
using ToDoManagementSystem.Application.DTOs.Auth;
using ToDoManagementSystem.Application.Features.Authentication.Commands;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Exceptions;

namespace ToDoManagementSystem.Application.Features.Authentication.Handlers;

/// <summary>Handles user login — validates credentials and issues a token pair.</summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>Validates credentials and returns a new token pair.</summary>
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedException("Account is deactivated.");

        string refreshTokenValue = _tokenService.GenerateRefreshToken();
        RefreshToken refreshToken = new()
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await _userRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Log.ForContext<LoginCommandHandler>()
           .Information("User logged in: {Email}", user.Email);

        return new LoginResponse
        {
            AccessToken = _tokenService.GenerateAccessToken(user),
            RefreshToken = refreshTokenValue,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(60),
            UserId = user.Id.ToString(),
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }
}
