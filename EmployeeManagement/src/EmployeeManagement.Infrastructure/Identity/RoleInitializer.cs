// ============================================================================
// RoleInitializer — creates required roles on application startup
// ============================================================================
//
// This class ensures that the required roles ("Admin" and "Employee") exist
// in the database. It runs once during application startup.
//
// Why initialize roles at startup?
//   - Roles must exist before any user can be assigned to them.
//   - Without this, the first registration would fail when trying to add
//     the user to the "Employee" role.
//   - Creating roles is idempotent: calling it when roles already exist
//     has no effect.
//
// What this class does NOT do:
//   - It does not create user accounts or seed passwords.
//   - It does not create a default admin user.
//   - Admin credentials must be provisioned separately through environment
//     variables or a secure bootstrap process.
//
// How to create an admin account in development:
//   1. Register a normal account through POST /api/auth/register.
//   2. Use the database directly (or a secure admin tool) to add the user
//      to the "Admin" role in the AspNetUserRoles table.
//   - OR: Set environment variables ADMIN_USERNAME, ADMIN_EMAIL, and
//     ADMIN_PASSWORD to create a bootstrap admin on first startup.
//     This is only for development and must not be used in production.
// ============================================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Infrastructure.Identity;

/// <summary>
/// Ensures that the required application roles exist in the database.
/// Call InitializeAsync during application startup.
/// </summary>
public static class RoleInitializer
{
    // The roles required by the application.
    public const string AdminRole = "Admin";
    public const string EmployeeRole = "Employee";

    /// <summary>
    /// Creates the required roles if they do not already exist.
    /// Optionally creates a bootstrap admin user if environment variables
    /// are configured.
    /// </summary>
    public static async Task InitializeAsync(
        RoleManager<IdentityRole<int>> roleManager,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        // Create roles if they do not exist. CreateAsync is idempotent
        // when the role already exists — it returns a failed result,
        // which we check but do not treat as an error.
        if (!await roleManager.RoleExistsAsync(EmployeeRole))
        {
            var result = await roleManager.CreateAsync(new IdentityRole<int>(EmployeeRole));
            if (result.Succeeded)
                logger.LogInformation("Created role: {Role}", EmployeeRole);
        }

        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            var result = await roleManager.CreateAsync(new IdentityRole<int>(AdminRole));
            if (result.Succeeded)
                logger.LogInformation("Created role: {Role}", AdminRole);
        }

        // Optional bootstrap admin. Only created when ALL three environment
        // variables are set. This allows development environments to have
        // an admin account without manual database manipulation.
        var adminUsername = Environment.GetEnvironmentVariable("ADMIN_USERNAME");
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (!string.IsNullOrWhiteSpace(adminUsername) &&
            !string.IsNullOrWhiteSpace(adminEmail) &&
            !string.IsNullOrWhiteSpace(adminPassword))
        {
            var existingAdmin = await userManager.FindByNameAsync(adminUsername);
            if (existingAdmin is null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminUsername,
                    Email = adminEmail
                };

                var createResult = await userManager.CreateAsync(admin, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, EmployeeRole);
                    await userManager.AddToRoleAsync(admin, AdminRole);
                    logger.LogInformation(
                        "Bootstrap admin user created: {Username}", adminUsername);
                }
                else
                {
                    logger.LogError(
                        "Failed to create bootstrap admin: {Errors}",
                        string.Join("; ", createResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                // Admin already exists — ensure they have the Admin role.
                if (!await userManager.IsInRoleAsync(existingAdmin, AdminRole))
                {
                    await userManager.AddToRoleAsync(existingAdmin, AdminRole);
                    logger.LogInformation(
                        "Existing user {Username} granted Admin role", adminUsername);
                }
            }
        }
    }
}
