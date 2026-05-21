using ToDoManagementSystem.Domain.Entities;

namespace ToDoManagementSystem.Application.Interfaces;

/// <summary>User-specific repository extending the generic CRUD interface.</summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>Finds a user by their email address (case-insensitive).</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>Finds a user by their password reset token.</summary>
    Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken ct = default);

    /// <summary>Checks whether an email is already registered.</summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);

    /// <summary>Directly persists a new refresh token via the DbSet, ensuring Added state.</summary>
    Task AddRefreshTokenAsync(Domain.Entities.RefreshToken refreshToken, CancellationToken ct = default);
}
