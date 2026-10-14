// ============================================================================
// AuthResponseDto — the response returned after successful authentication
// ============================================================================
//
// This DTO carries the JWT access token and metadata back to the client.
// It intentionally excludes password hashes, internal Identity fields, and
// other sensitive data.
// ============================================================================

namespace EmployeeManagement.Application.DTOs;

/// <summary>
/// Contains the JWT token and metadata returned after a successful
/// registration or login.
/// </summary>
public class AuthResponseDto
{
    /// <summary>The JWT access token. Include in the Authorization header as "Bearer {token}".</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>The authenticated user's username.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>The authenticated user's email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>When the token expires (UTC). The client should request a new token after this time.</summary>
    public DateTime Expiration { get; set; }
}
