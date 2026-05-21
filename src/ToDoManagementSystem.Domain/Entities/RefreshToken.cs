namespace ToDoManagementSystem.Domain.Entities;

/// <summary>JWT refresh token stored in the database.</summary>
public class RefreshToken
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Foreign key to User.</summary>
    public Guid UserId { get; set; }

    /// <summary>The opaque refresh token string.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>When this token expires.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Whether the token has been revoked.</summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Navigation: owner user.</summary>
    public User User { get; set; } = null!;
}
