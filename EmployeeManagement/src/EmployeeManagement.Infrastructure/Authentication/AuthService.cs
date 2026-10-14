// ============================================================================
// AuthService — authentication use cases using ASP.NET Core Identity
// ============================================================================
//
// This service implements registration and login using ASP.NET Core Identity.
//
// Why use Identity instead of manual user management?
//   - Password hashing: Identity uses PBKDF2 with a per-user salt and
//     configurable iteration count. Manual hashing (even with bcrypt) is
//     easy to implement incorrectly.
//   - Account lockout: Identity tracks failed login attempts and can
//     temporarily lock accounts to prevent brute-force attacks.
//   - Role management: Identity provides built-in role storage and lookup.
//   - Standards compliance: Identity follows established security practices.
//
// Registration flow:
//   1. Check if the username or email is already in use.
//   2. Create the user via Identity's UserManager (which hashes the password).
//   3. Assign the default "Employee" role (never let the client choose).
//   4. Generate a JWT token and return it.
//
// Login flow:
//   Uses SignInManager.CheckPasswordSignInAsync which handles:
//   - Password verification via PBKDF2 hash comparison
//   - Lockout checking (rejects locked accounts even with correct password)
//   - Failed attempt counting (increments on failure, resets on success)
//   - Lockout triggering (locks account after MaxFailedAccessAttempts)
//
//   This is the recommended Identity pattern because it ensures all lockout
//   bookkeeping happens consistently, regardless of which code path is taken.
// ============================================================================

using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Interfaces.Services;
using EmployeeManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Infrastructure.Authentication;

/// <summary>
/// Implements authentication use cases (register, login) using
/// ASP.NET Core Identity for user management and ITokenService for JWT.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    // The default role assigned to all newly registered users.
    // Admin role must be assigned through a separate, controlled process.
    private const string DefaultRole = "Employee";

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    // -----------------------------------------------------------------------
    // RegisterAsync — creates a new user account with the default role.
    //
    // The method never accepts a role from the client. This prevents a
    // malicious user from registering as Admin by manipulating the request.
    //
    // If the username or email is already taken, a ConflictException is
    // thrown so the API can return a 409 response.
    // -----------------------------------------------------------------------
    public async Task<AuthResponseDto> RegisterAsync(
        RegisterRequestDto dto, CancellationToken cancellationToken = default)
    {
        // Check if a user with this username already exists.
        var existingUser = await _userManager.FindByNameAsync(dto.Username);
        if (existingUser is not null)
        {
            _logger.LogWarning(
                "Registration conflict: username {Username} already exists",
                dto.Username);
            throw new ConflictException("A user with this username already exists.");
        }

        // Check if a user with this email already exists.
        var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existingEmail is not null)
        {
            _logger.LogWarning(
                "Registration conflict: email {Email} already exists",
                dto.Email);
            throw new ConflictException("A user with this email already exists.");
        }

        // Create the Identity user. UserManager.CreateAsync hashes the
        // password using PBKDF2 before storing it. We never see or store
        // the plain-text password.
        var user = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            // Identity rejected the password (e.g., too short, missing
            // required characters). Return a conflict with the details.
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            _logger.LogWarning(
                "Registration failed for {Username}: {Errors}",
                dto.Username, errors);
            throw new ConflictException($"Registration failed: {errors}");
        }

        // Assign the default "Employee" role. This role grants basic
        // authenticated access. Admin must be assigned separately.
        await _userManager.AddToRoleAsync(user, DefaultRole);

        _logger.LogInformation(
            "New user registered: {Username} with role {Role}",
            dto.Username, DefaultRole);

        // Generate and return a JWT token for the newly registered user.
        var roles = await _userManager.GetRolesAsync(user);
        return await _tokenService.GenerateTokenAsync(user.Id, user.UserName!, user.Email!, roles);
    }

    // -----------------------------------------------------------------------
    // LoginAsync — authenticates a user and returns a JWT token.
    //
    // Uses SignInManager.CheckPasswordSignInAsync which is the recommended
    // Identity pattern for password verification with lockout support.
    //
    // This method handles:
    //   - Password verification via PBKDF2 hash comparison
    //   - Lockout checking (rejects locked accounts even with correct password)
    //   - Failed attempt counting (increments on failure, resets on success)
    //   - Lockout triggering (locks account after MaxFailedAccessAttempts)
    //
    // Returns null if the credentials are invalid. The null response is
    // intentional: it does not reveal whether the username exists or the
    // password was wrong. This prevents username enumeration attacks.
    //
    // Lockout behavior:
    //   After 5 failed attempts (configurable via Identity options), the
    //   account is locked for 15 minutes. During lockout, even a correct
    //   password is rejected. The lockout resets automatically after the
    //   time span elapses.
    // -----------------------------------------------------------------------
    public async Task<AuthResponseDto?> LoginAsync(
        LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        // Find the user by username. If not found, return null (do not
        // reveal that the username does not exist).
        var user = await _userManager.FindByNameAsync(dto.Username);
        if (user is null)
        {
            _logger.LogWarning("Login failed: unknown username {Username}", dto.Username);
            return null;
        }

        // CheckPasswordSignInAsync performs password verification, lockout
        // checking, and failed-attempt tracking in one call.
        //
        // The second parameter (lockOnFailure: true) means Identity will
        // increment the failed-access counter when the password is wrong.
        // After MaxFailedAccessAttempts (5 by default), the account is
        // locked for DefaultLockoutTimeSpan (15 minutes by default).
        //
        // We do NOT use the SignInResult to create a cookie-based session.
        // We only use it to check whether authentication succeeded and then
        // generate our own JWT token.
        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user, dto.Password, lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            // Log the specific failure reason for diagnostics without
            // revealing it to the client.
            if (signInResult.IsLockedOut)
            {
                _logger.LogWarning(
                    "Login failed: account locked out for {Username}",
                    dto.Username);
            }
            else
            {
                _logger.LogWarning(
                    "Login failed: invalid password for {Username}",
                    dto.Username);
            }

            // Return null for ALL failure types. This prevents attackers
            // from determining whether a username exists, whether the
            // account is locked, or whether the password is wrong.
            return null;
        }

        // Password verified and account is not locked. Reset the failed
        // attempt counter so the lockout window does not accumulate.
        await _userManager.ResetAccessFailedCountAsync(user);

        _logger.LogInformation(
            "User {Username} logged in successfully",
            dto.Username);

        // Generate a JWT token with the user's roles.
        var roles = await _userManager.GetRolesAsync(user);
        return await _tokenService.GenerateTokenAsync(user.Id, user.UserName!, user.Email!, roles);
    }
}
