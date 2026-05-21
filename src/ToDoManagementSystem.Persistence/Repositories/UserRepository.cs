using Microsoft.EntityFrameworkCore;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Persistence.Context;

namespace ToDoManagementSystem.Persistence.Repositories;

/// <summary>User-specific repository with email lookup support.</summary>
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    /// <summary>Finds a user by email (case-insensitive), including refresh tokens.</summary>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await _dbSet
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    /// <summary>Finds a user that has a matching (non-null) password reset token field.</summary>
    public async Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken ct = default) =>
        await _dbSet
            .FirstOrDefaultAsync(u => u.PasswordResetToken != null && u.PasswordResetExpiry > DateTime.UtcNow, ct);

    /// <summary>Returns true if any user has the given email address registered.</summary>
    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        await _dbSet.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

    /// <summary>Returns all users including their refresh tokens.</summary>
    public override async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default) =>
        await _dbSet.Include(u => u.RefreshTokens).ToListAsync(ct);

    /// <summary>Explicitly tracks a new refresh token as Added via the DbSet.</summary>
    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default) =>
        await _context.Set<RefreshToken>().AddAsync(refreshToken, ct);
}
