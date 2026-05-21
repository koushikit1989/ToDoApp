using System.Security.Claims;
using ToDoManagementSystem.Domain.Entities;

namespace ToDoManagementSystem.Application.Interfaces;

/// <summary>JWT access and refresh token service.</summary>
public interface ITokenService
{
    /// <summary>Generates a signed JWT access token for the given user.</summary>
    string GenerateAccessToken(User user);

    /// <summary>Generates a cryptographically random refresh token string.</summary>
    string GenerateRefreshToken();

    /// <summary>Extracts claims principal from an expired JWT (used during refresh).</summary>
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
