// ============================================================================
// ITokenService — contract for JWT token generation
// ============================================================================
//
// This interface defines the JWT token generation contract. The implementation
// in Infrastructure creates a signed JWT with the user's claims (ID, username,
// roles). Keeping this behind an interface allows the token generation logic
// to be tested independently and swapped if needed.
// ============================================================================

using EmployeeManagement.Application.DTOs;

namespace EmployeeManagement.Application.Interfaces.Services;

/// <summary>
/// Defines the JWT token generation contract.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a signed JWT access token for the given user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="username">The user's username (included as a claim).</param>
    /// <param name="email">The user's email address (included as a claim).</param>
    /// <param name="roles">The user's roles (included as role claims).</param>
    /// <returns>An AuthResponseDto containing the token, username, email, and expiration.</returns>
    Task<AuthResponseDto> GenerateTokenAsync(int userId, string username, string email, IEnumerable<string> roles);
}
