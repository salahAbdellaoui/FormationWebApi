// ============================================================================
// JwtTokenService — generates signed JWT access tokens
// ============================================================================
//
// This service creates JWT tokens that the API uses to authenticate requests.
// A JWT (JSON Web Token) is a compact, self-contained token that contains
// claims about the user (ID, username, roles) and is signed with a secret key.
//
// How JWT works:
//   1. The server creates a token with claims and signs it with a secret key.
//   2. The client stores the token and sends it with each request in the
//      Authorization header: "Bearer <token>".
//   3. The server validates the token's signature, issuer, audience, and
//      expiration on each request. If valid, the user is authenticated.
//
// Why the signing key must remain secret:
//   Anyone who has the signing key can create valid tokens for any user,
//   including Admin. This is equivalent to having the master password.
//   The key is read from an environment variable, never committed to source.
//
// Claims included in the token:
//   - sub (subject): the user's unique ID
//   - unique_name: the username
//   - email: the user's email
//   - role: one claim per role (Admin, Employee, etc.)
//   - exp: expiration time (UTC)
//   - iss/aud: issuer and audience for validation
// ============================================================================

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmployeeManagement.Application;
using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagement.Infrastructure.Authentication;

/// <summary>
/// Generates signed JWT access tokens using the configured JwtSettings.
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly string _signingKey;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(
        IConfiguration configuration,
        ILogger<JwtTokenService> logger)
    {
        _logger = logger;

        // Bind JwtSettings from the "JwtSettings" section in configuration.
        _jwtSettings = new JwtSettings
        {
            Issuer = configuration["JwtSettings:Issuer"] ?? string.Empty,
            Audience = configuration["JwtSettings:Audience"] ?? string.Empty,
            ExpiryInMinutes = int.TryParse(
                configuration["JwtSettings:ExpiryInMinutes"], out var minutes) ? minutes : 60
        };

        // Read the signing key from an environment variable. This keeps the
        // secret out of source control. In development, set it via User
        // Secrets or the ASPNETCORE_JWT_SIGNING_KEY environment variable.
        _signingKey = configuration["JwtSettings:SigningKey"]
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_JWT_SIGNING_KEY")
            ?? string.Empty;

        // Validate that the signing key is configured and sufficiently long.
        // A short key would be vulnerable to brute-force attacks. 256 bits
        // (32 bytes) is the minimum for HMAC-SHA256.
        if (string.IsNullOrWhiteSpace(_signingKey) || _signingKey.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT signing key is not configured or is too short. " +
                "Set the ASPNETCORE_JWT_SIGNING_KEY environment variable to a " +
                "secret value of at least 32 characters. " +
                "In development, use User Secrets: dotnet user-secrets set \"JwtSettings:SigningKey\" \"<your-secret>\"");
        }
    }

    // -----------------------------------------------------------------------
    // GenerateTokenAsync — creates a signed JWT for the given user.
    //
    // The token includes the user's ID, username, email, and roles as claims.
    // It is signed with HMAC-SHA256 using the configured signing key.
    // -----------------------------------------------------------------------
    public Task<AuthResponseDto> GenerateTokenAsync(
        int userId, string username, string email, IEnumerable<string> roles)
    {
        // Create a symmetric signing key from the secret string.
        // The key bytes are derived from the UTF-8 encoding of the secret.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Calculate the expiration time in UTC. Using UTC avoids issues with
        // daylight saving time and timezone differences between servers.
        var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

        // Build the list of claims included in the token.
        var claims = new List<Claim>
        {
            // The subject claim identifies the user by their unique ID.
            new(ClaimTypes.NameIdentifier, userId.ToString()),

            // The username and email claims provide user information.
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, email),
        };

        // Add a role claim for each role the user belongs to.
        // Multiple roles produce multiple role claims, which is the standard
        // way to represent multi-role membership in JWT.
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Create the JWT token with all claims, signing credentials, and
        // validation parameters (issuer, audience, expiration).
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        // Serialize the token to its compact string representation.
        // This is the string the client sends in the Authorization header.
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation(
            "JWT token generated for user {UserId} ({Username}), expires {Expiration}",
            userId, username, expiration);

        return Task.FromResult(new AuthResponseDto
        {
            Token = tokenString,
            Username = username,
            Email = email,
            Expiration = expiration
        });
    }
}
