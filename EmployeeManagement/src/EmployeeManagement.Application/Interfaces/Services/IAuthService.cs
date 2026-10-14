// ============================================================================
// IAuthService — contract for authentication use cases
// ============================================================================
//
// This interface defines the authentication operations that the API layer
// can invoke: registration and login. The implementation uses ASP.NET Core
// Identity for user management and JWT for token generation.
//
// The interface does not expose Identity types (UserManager, IdentityResult)
// to keep the Application layer independent of the Identity framework.
// Failures are communicated through exceptions (ConflictException for
// duplicate accounts) or null returns (invalid credentials).
// ============================================================================

using EmployeeManagement.Application.DTOs;

namespace EmployeeManagement.Application.Interfaces.Services;

/// <summary>
/// Defines the authentication use cases (register and login).
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account with the default "Employee" role.
    /// Returns the authentication response containing the JWT token.
    /// </summary>
    /// <exception cref="Exceptions.ConflictException">
    /// Thrown when the username or email is already in use.
    /// </exception>
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user with username and password.
    /// Returns the authentication response containing the JWT token on success,
    /// or null if the credentials are invalid.
    /// </summary>
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
}
