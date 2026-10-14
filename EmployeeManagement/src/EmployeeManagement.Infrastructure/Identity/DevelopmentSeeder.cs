// ============================================================================
// DevelopmentSeeder — creates development-only test accounts
// ============================================================================
//
// This class creates the Admin and Employee test accounts used during local
// development. It runs ONLY when the application is in the Development
// environment, and only when the required credentials are configured.
//
// Security model:
//   - Passwords are supplied through User Secrets or environment variables,
//     never hardcoded in source code or committed configuration.
//   - The seeding is idempotent: it does not reset passwords on every
//     startup, and does not change an existing user's role unexpectedly.
//   - If required credentials are missing, no accounts are created and a
//     warning is logged (without printing secret values).
//   - This mechanism is separate from the production bootstrap-admin
//     provisioning in RoleInitializer.
//
// The existing Identity password policy requires:
//   - Minimum 8 characters
//   - At least one digit
//   - At least one uppercase letter
//   - At least one lowercase letter
//
// Requested weak passwords ("admin", "user1") do NOT satisfy this policy.
// Compliant alternatives are provided via User Secrets.
// ============================================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Infrastructure.Identity;

/// <summary>
/// Creates development-only test accounts when running in the Development
/// environment and the required credentials are configured.
/// </summary>
public static class DevelopmentSeeder
{
    /// <summary>
    /// Seeds the development test accounts (admin and user1) if their
    /// credentials are present in configuration. Call only when the
    /// application is running in the Development environment.
    ///
    /// Configuration keys (set via User Secrets or environment variables):
    ///   DevTestAccounts:Admin:Username
    ///   DevTestAccounts:Admin:Password
    ///   DevTestAccounts:Employee:Username
    ///   DevTestAccounts:Employee:Password
    /// </summary>
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        // Read the development credentials from configuration.
        // In Development, IConfiguration includes User Secrets, so
        // passwords stored there are automatically available.
        var adminUsername = configuration["DevTestAccounts:Admin:Username"];
        var adminPassword = configuration["DevTestAccounts:Admin:Password"];
        var employeeUsername = configuration["DevTestAccounts:Employee:Username"];
        var employeePassword = configuration["DevTestAccounts:Employee:Password"];

        // Seed the admin account if credentials are present.
        if (!string.IsNullOrWhiteSpace(adminUsername) &&
            !string.IsNullOrWhiteSpace(adminPassword))
        {
            await SeedUserAsync(
                userManager, adminUsername, adminPassword,
                RoleInitializer.AdminRole, logger);
        }
        else
        {
            logger.LogWarning(
                "Development admin account not seeded: " +
                "DevTestAccounts:Admin:Username or Password is not configured. " +
                "Set them via User Secrets or environment variables.");
        }

        // Seed the employee account if credentials are present.
        if (!string.IsNullOrWhiteSpace(employeeUsername) &&
            !string.IsNullOrWhiteSpace(employeePassword))
        {
            await SeedUserAsync(
                userManager, employeeUsername, employeePassword,
                RoleInitializer.EmployeeRole, logger);
        }
        else
        {
            logger.LogWarning(
                "Development employee account not seeded: " +
                "DevTestAccounts:Employee:Username or Password is not configured. " +
                "Set them via User Secrets or environment variables.");
        }
    }

    // -----------------------------------------------------------------------
    // SeedUserAsync — creates a single user with the specified role.
    //
    // Idempotent behavior:
    //   - If the user already exists, it is NOT modified.
    //   - The password is NOT reset on existing users.
    //   - The role is only assigned if the user does not already have it.
    //   - This prevents accidental password resets on every startup.
    // -----------------------------------------------------------------------
    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        string username,
        string password,
        string role,
        ILogger logger)
    {
        // Check if the user already exists. If so, do not modify it.
        var existingUser = await userManager.FindByNameAsync(username);
        if (existingUser is not null)
        {
            // Ensure the user has the correct role without resetting
            // the password or changing any other properties.
            if (!await userManager.IsInRoleAsync(existingUser, role))
            {
                await userManager.AddToRoleAsync(existingUser, role);
                logger.LogInformation(
                    "Development user {Username} assigned to role {Role}",
                    username, role);
            }
            else
            {
                logger.LogDebug(
                    "Development user {Username} already exists with role {Role}",
                    username, role);
            }

            return;
        }

        // Create the user. The email is derived from the username so that
        // RequireUniqueEmail does not cause conflicts.
        var email = $"{username}@dev.local";
        var user = new ApplicationUser
        {
            UserName = username,
            Email = email
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (createResult.Succeeded)
        {
            // Assign the specified role.
            await userManager.AddToRoleAsync(user, role);
            logger.LogInformation(
                "Development user {Username} created with role {Role}",
                username, role);
        }
        else
        {
            // Log the failure reasons. The password itself is never printed.
            var errors = string.Join(
                "; ", createResult.Errors.Select(e => e.Description));
            logger.LogError(
                "Failed to create development user {Username}: {Errors}",
                username, errors);
        }
    }
}
