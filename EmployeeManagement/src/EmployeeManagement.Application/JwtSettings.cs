// ============================================================================
// JwtSettings — configuration for JWT token generation and validation
// ============================================================================
//
// This class maps the "JwtSettings" section from appsettings.json.
// It contains non-secret defaults (issuer, audience, token lifetime).
// The signing key is read separately from an environment variable because
// it is a secret that must never be committed to source control.
//
// Why separate the signing key from appsettings.json?
//   - appsettings.json is committed to source control.
//   - The signing key is equivalent to a password: anyone who has it can
//     forge valid tokens for any user.
//   - Environment variables (or User Secrets in development) keep the key
//     out of the repository.
// ============================================================================

namespace EmployeeManagement.Application;

/// <summary>
/// Configuration settings for JWT token generation and validation.
/// Bound from the "JwtSettings" section in appsettings.json.
/// </summary>
public class JwtSettings
{
    /// <summary>The token issuer. Typically the API's name or URL.</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>The token audience. Typically the client application's name.</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>Token lifetime in minutes. Default is 60 minutes.</summary>
    public int ExpiryInMinutes { get; set; } = 60;
}
