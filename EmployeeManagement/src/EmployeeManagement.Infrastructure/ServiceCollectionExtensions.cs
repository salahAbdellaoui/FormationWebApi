// ============================================================================
// ServiceCollectionExtensions — DI registration for the Infrastructure layer
// ============================================================================
//
// This extension method registers all Infrastructure-layer implementations
// in the dependency injection container:
//   - Repository implementations (EmployeeRepository, DepartmentRepository)
//   - Authentication services (AuthService, JwtTokenService)
//
// This is where the Clean Architecture "wiring" happens for Infrastructure.
// The Application layer defines the interfaces, and Infrastructure provides
// the implementations. The DI container connects them at startup.
//
// Identity services are registered here because they depend on the
// ApplicationDbContext (which lives in Infrastructure) and the ApplicationUser
// type (also in Infrastructure). The API's Program.cs calls AddInfrastructure
// to register everything at once.
// ============================================================================

using EmployeeManagement.Application.Interfaces.Repositories;
using EmployeeManagement.Application.Interfaces.Services;
using EmployeeManagement.Infrastructure.Authentication;
using EmployeeManagement.Infrastructure.Identity;
using EmployeeManagement.Infrastructure.Persistence;
using EmployeeManagement.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagement.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure-layer services in DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Infrastructure implementations: repositories,
    /// Identity, and authentication services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // -------------------------------------------------------------------
        // Repository registrations
        // -------------------------------------------------------------------
        // Map repository interfaces to their EF Core implementations.
        // Scoped lifetime matches the DbContext's scoped lifetime.
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();

        // -------------------------------------------------------------------
        // ASP.NET Core Identity
        // -------------------------------------------------------------------
        // AddIdentity registers the Identity services: UserManager,
        // RoleManager, SignInManager, password validators, etc.
        // It also registers the ApplicationUser and IdentityRole<int>
        // types with the ApplicationDbContext.
        //
        // Why integer keys? The existing Employee and Department entities
        // use integer primary keys. Using int for Identity users keeps
        // the key type consistent across the entire application.
        services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
        {
            // Password requirements. These are the Identity defaults with
            // minor adjustments for a training project. In production,
            // you might require special characters or increase the minimum
            // length.
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;

            // Lockout settings: after 5 failed login attempts, lock the
            // account for 15 minutes. This slows down brute-force attacks.
            options.Lockout.DefaultLockoutTimeSpan = System.TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // Username settings: allow any characters in usernames.
            // Email is not used as username in this application.
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // -------------------------------------------------------------------
        // Authentication services
        // -------------------------------------------------------------------
        // Register the application-level authentication services.
        // IAuthService handles registration and login using Identity.
        // ITokenService handles JWT token generation.
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}
