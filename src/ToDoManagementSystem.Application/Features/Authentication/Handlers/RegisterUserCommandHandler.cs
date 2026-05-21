using MediatR;
using Serilog;
using ToDoManagementSystem.Application.DTOs.Auth;
using ToDoManagementSystem.Application.Features.Authentication.Commands;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Exceptions;

namespace ToDoManagementSystem.Application.Features.Authentication.Handlers;

/// <summary>Handles user registration — hashes password, persists user, issues initial token pair.</summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>Registers a new user and returns an access/refresh token pair.</summary>
    public async Task<LoginResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        bool exists = await _userRepository.EmailExistsAsync(request.Email, cancellationToken);
        if (exists)
            throw new ValidationException("Email is already registered.");

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        User user = new()
        {
            FullName = request.FullName,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = "User",
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);

        string refreshTokenValue = _tokenService.GenerateRefreshToken();
        RefreshToken refreshToken = new()
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        user.RefreshTokens.Add(refreshToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Log.ForContext<RegisterUserCommandHandler>()
           .Information("New user registered: {Email}", user.Email);

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
