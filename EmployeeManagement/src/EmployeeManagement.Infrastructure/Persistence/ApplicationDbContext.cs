// ============================================================================
// ApplicationDbContext — the EF Core entry point for database operations
// ============================================================================
//
// The DbContext is the bridge between the Domain entities (C# objects) and
// the SQL Server database (tables and rows). It tracks changes, generates
// SQL, and manages database connections.
//
// This context now inherits from IdentityDbContext<ApplicationUser,
// IdentityRole<int>, int> instead of plain DbContext. This adds the
// ASP.NET Core Identity tables (AspNetUsers, AspNetRoles, AspNetUserRoles,
// etc.) alongside the existing Employees and Departments tables in the
// same database.
//
// Why extend the existing context instead of creating a separate one?
//   - A single DbContext means a single database and a single migration.
//   - Transactions that span Identity and application data are simpler.
//   - The existing Employee and Department configurations are preserved
//     by calling base.OnModelCreating (which sets up Identity tables)
//     before applying our own configurations.
//
// Entity materialization note:
// EF Core materializes entities by calling their constructors. Both
// Department and Employee have parameterized constructors that validate
// their inputs. EF Core matches constructor parameters to property names
// (case-insensitive) and passes database values through the constructor.
// Properties not in the constructor (Id, IsActive) are set via their
// private setters after construction.
// ============================================================================

using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Persistence;

/// <summary>
/// The EF Core database context for the EmployeeManagement application.
/// Provides access to the Employees and Departments tables, plus the
/// ASP.NET Core Identity tables for user authentication and roles.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    // -----------------------------------------------------------------------
    // Constructor — receives DbContextOptions from the DI container.
    // -----------------------------------------------------------------------
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // -----------------------------------------------------------------------
    // DbSet properties for application entities.
    // Identity tables (Users, Roles, etc.) are provided by the base class.
    // -----------------------------------------------------------------------
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();

    // -----------------------------------------------------------------------
    // OnModelCreating — configures both Identity and application entities.
    //
    // The base.IdentityDbContext.OnModelCreating creates all Identity tables
    // (AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims, etc.)
    // with their correct relationships and indexes.
    //
    // After the base call, ApplyConfigurationsFromAssembly discovers and
    // applies our Employee and Department configurations.
    //
    // This order ensures Identity tables are created first, and our
    // application tables are added alongside them without conflicts.
    // -----------------------------------------------------------------------
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Base call sets up all Identity table configurations.
        base.OnModelCreating(modelBuilder);

        // Apply our application entity configurations (Employee, Department).
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}
