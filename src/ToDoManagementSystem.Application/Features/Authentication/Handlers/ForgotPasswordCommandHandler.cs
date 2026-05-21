using MediatR;
using Serilog;
using ToDoManagementSystem.Application.Features.Authentication.Commands;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;

namespace ToDoManagementSystem.Application.Features.Authentication.Handlers;

/// <summary>Generates a password-reset token and sends the reset email.</summary>
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>Stores a hashed reset token on the user and sends the email.</summary>
    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Always return true to prevent email enumeration
        if (user is null)
            return true;

        string rawToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        user.PasswordResetToken = BCrypt.Net.BCrypt.HashPassword(rawToken);
        user.PasswordResetExpiry = DateTime.UtcNow.AddHours(1);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        string resetLink = $"{request.BaseResetUrl}?token={Uri.EscapeDataString(rawToken)}";
        await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetLink, cancellationToken);

        Log.ForContext<ForgotPasswordCommandHandler>()
           .Information("Password reset requested for: {Email}", user.Email);

        return true;
    }
}
