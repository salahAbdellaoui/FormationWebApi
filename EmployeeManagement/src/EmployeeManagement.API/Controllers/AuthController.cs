// ============================================================================
// AuthController — HTTP endpoints for registration and login
// ============================================================================
//
// This controller handles authentication requests: creating new accounts
// (register) and obtaining JWT tokens (login). Both endpoints are anonymous
// (no authentication required) because they are the entry points for
// obtaining credentials.
//
// Security notes:
//   - Registration never accepts a role from the client. All new accounts
//     receive the default "Employee" role. This prevents privilege escalation.
//   - Login returns null for invalid credentials, which the controller maps
//     to 401 Unauthorized. A generic message is used to prevent username
//     enumeration (attackers cannot determine which usernames exist).
//   - Passwords are never logged, returned in responses, or stored in
//     plain text. Identity handles all password hashing.
// ============================================================================

using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers;

/// <summary>
/// Provides HTTP endpoints for user registration and authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // -----------------------------------------------------------------------
    // POST /api/auth/register — creates a new user account.
    //
    // Returns 200 OK with the JWT token on success.
    // Returns 409 Conflict if the username or email is already taken.
    // Returns 400 Bad Request if the input fails validation.
    //
    // This endpoint is anonymous — anyone can register an account.
    // The new account receives only the "Employee" role.
    // -----------------------------------------------------------------------
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register(
        [FromBody] RegisterRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(dto, cancellationToken);
        return Ok(result);
    }

    // -----------------------------------------------------------------------
    // POST /api/auth/login — authenticates a user and returns a JWT token.
    //
    // Returns 200 OK with the JWT token on success.
    // Returns 401 Unauthorized if the credentials are invalid.
    //
    // This endpoint is anonymous — authentication is the purpose of this
    // endpoint, so it cannot require prior authentication.
    // -----------------------------------------------------------------------
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(dto, cancellationToken);

        if (result is null)
        {
            // Return a generic error message. Do not reveal whether the
            // username does not exist or the password is wrong — this
            // prevents attackers from enumerating valid usernames.
            return Unauthorized(new { message = "Invalid username or password." });
        }

        return Ok(result);
    }
}
