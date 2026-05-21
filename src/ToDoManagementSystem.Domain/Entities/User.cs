using ToDoManagementSystem.Domain.Common;

namespace ToDoManagementSystem.Domain.Entities;

/// <summary>Application user domain entity.</summary>
public class User : BaseEntity
{
    /// <summary>Display name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Unique email address used for login.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>BCrypt-hashed password.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Role string: "Admin" or "User".</summary>
    public string Role { get; set; } = "User";

    /// <summary>Whether the account is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Token hash stored for password reset flow.</summary>
    public string? PasswordResetToken { get; set; }

    /// <summary>Expiry of the password reset token.</summary>
    public DateTime? PasswordResetExpiry { get; set; }

    /// <summary>Navigation: tasks owned by this user.</summary>
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    /// <summary>Navigation: refresh tokens issued to this user.</summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
