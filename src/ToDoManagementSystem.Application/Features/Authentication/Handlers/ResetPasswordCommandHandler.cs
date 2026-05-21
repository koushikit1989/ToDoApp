using MediatR;
using Serilog;
using ToDoManagementSystem.Application.Features.Authentication.Commands;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Exceptions;

namespace ToDoManagementSystem.Application.Features.Authentication.Handlers;

/// <summary>Validates the reset token and updates the user password.</summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>Validates the reset token, updates password, and clears the token.</summary>
    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        IEnumerable<User> users = await _userRepository.GetAllAsync(cancellationToken);
        User? user = users.FirstOrDefault(u =>
            u.PasswordResetToken != null &&
            u.PasswordResetExpiry > DateTime.UtcNow &&
            BCrypt.Net.BCrypt.Verify(request.Token, u.PasswordResetToken));

        if (user is null)
            throw new ValidationException("Invalid or expired password reset token.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
        user.PasswordResetToken = null;
        user.PasswordResetExpiry = null;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Log.ForContext<ResetPasswordCommandHandler>()
           .Information("Password reset completed for: {Email}", user.Email);

        return true;
    }
}
