// ============================================================================
// ServiceCollectionExtensions — DI registration for the Application layer
// ============================================================================
//
// This extension method registers all Application-layer services in the
// dependency injection container. By placing registrations here instead of
// in Program.cs, we keep the composition root clean and make it easy to
// see all Application services in one place.
//
// The API's Program.cs calls builder.Services.AddApplication() to register
// these services. When a controller requests IEmployeeService, the DI
// container provides an EmployeeService instance.
//
// Service lifetime: Scoped (one instance per HTTP request). This matches
// the DbContext's scoped lifetime and ensures that repositories and
// services within the same request share the same database context.
// ============================================================================

using EmployeeManagement.Application.Interfaces.Services;
using EmployeeManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagement.Application;

/// <summary>
/// Extension methods for registering Application-layer services in DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Application services (EmployeeService, DepartmentService)
    /// as scoped dependencies.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services. Scoped means one instance per
        // HTTP request. This ensures that all services within a single
        // request share the same database context and repository instances.
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();

        return services;
    }
}
