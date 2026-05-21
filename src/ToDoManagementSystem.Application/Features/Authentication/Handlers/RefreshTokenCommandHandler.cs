using System.Security.Claims;
using MediatR;
using Serilog;
using ToDoManagementSystem.Application.DTOs.Auth;
using ToDoManagementSystem.Application.Features.Authentication.Commands;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Exceptions;

namespace ToDoManagementSystem.Application.Features.Authentication.Handlers;

/// <summary>Handles refresh-token rotation — revokes old token, issues a new pair.</summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>Validates the refresh token, rotates it, and returns a new token pair.</summary>
    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetAllAsync(cancellationToken)
            .ContinueWith(t => t.Result.FirstOrDefault(u =>
                u.RefreshTokens.Any(rt => rt.Token == request.RefreshToken)), cancellationToken);

        if (user is null)
            throw new UnauthorizedException("Invalid refresh token.");

        RefreshToken? storedToken = user.RefreshTokens
            .FirstOrDefault(rt => rt.Token == request.RefreshToken);

        if (storedToken is null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token is expired or revoked.");

        storedToken.IsRevoked = true;

        string newRefreshTokenValue = _tokenService.GenerateRefreshToken();
        RefreshToken newRefreshToken = new()
        {
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await _userRepository.AddRefreshTokenAsync(newRefreshToken, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Log.ForContext<RefreshTokenCommandHandler>()
           .Information("Token refreshed for user: {UserId}", user.Id);

        return new LoginResponse
        {
            AccessToken = _tokenService.GenerateAccessToken(user),
            RefreshToken = newRefreshTokenValue,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(60),
            UserId = user.Id.ToString(),
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }
}
