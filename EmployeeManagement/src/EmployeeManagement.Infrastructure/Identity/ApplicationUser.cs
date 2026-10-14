// ============================================================================
// ApplicationUser — the Identity user type for this application
// ============================================================================
//
// ASP.NET Core Identity manages user accounts, password hashing, lockout,
// and role membership. By default, Identity uses a string key for user IDs.
// This application uses integer keys (IdentityUser<int>) for consistency
// with the Employee and Department entities.
//
// Why ASP.NET Core Identity instead of custom authentication?
//   - Password hashing is handled by Identity using proven algorithms
//     (PBKDF2 with salt). Manual password hashing is error-prone and a
//     common source of security vulnerabilities.
//   - Identity provides built-in support for roles, lockout, two-factor
//     authentication, and email confirmation.
//   - The EF Core integration stores users in standard tables that can be
//     migrated alongside the rest of the database schema.
//
// This class lives in Infrastructure because Identity is a technical concern.
// The Domain layer has no knowledge of user accounts or authentication.
// ============================================================================

using Microsoft.AspNetCore.Identity;

namespace EmployeeManagement.Infrastructure.Identity;

/// <summary>
/// Represents a user account in the application. Extends IdentityUser&lt;int&gt;
/// to use integer primary keys consistent with other entities.
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    // IdentityUser<int> already provides:
    //   - Id (int) — unique user identifier
    //   - UserName — the login name
    //   - Email — the user's email address
    //   - PasswordHash — managed by Identity (never access directly)
    //   - LockoutEnabled, LockoutEnd — account lockout support
    //   - AccessFailedCount — tracks failed login attempts
    //
    // Additional custom properties can be added here in the future
    // (e.g., FullName,CreatedAt) without changing the authentication flow.
}
