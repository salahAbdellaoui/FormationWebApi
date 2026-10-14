// ============================================================================
// IEmployeeRepository — contract for Employee data access
// ============================================================================
//
// This interface defines the operations that the Application layer needs to
// persist and retrieve Employee entities. The Infrastructure layer provides
// the concrete implementation using EF Core, but the Application layer
// depends only on this interface.
//
// Why an interface? The Application layer should not know whether data comes
// from SQL Server, a file, or an in-memory collection. By depending on an
// interface, we can:
//   - Swap the database technology without changing Application code.
//   - Replace the real repository with a mock during unit testing.
//   - Test business logic in isolation from the database.
//
// This interface does not expose EF Core types (DbSet, IQueryable) or
// Infrastructure-specific classes. It works only with Domain entities.
// ============================================================================

using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Interfaces.Repositories;

/// <summary>
/// Defines data-access operations for <see cref="Employee"/> entities.
/// Implemented by Infrastructure; consumed by Application services.
/// </summary>
public interface IEmployeeRepository
{
    // -----------------------------------------------------------------------
    // GetByIdAsync — retrieves a single employee by their ID.
    //
    // Returns the tracked entity (needed for calling Domain methods and
    // saving changes) or null if no employee with that ID exists.
    // The caller must check for null before using the result.
    // -----------------------------------------------------------------------
    Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // -----------------------------------------------------------------------
    // GetAllAsync — retrieves all employees.
    //
    // Returns a read-only list. AsNoTracking is used in the implementation
    // because the caller only needs to read data and convert to DTOs.
    // -----------------------------------------------------------------------
    Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken = default);

    // -----------------------------------------------------------------------
    // EmailExistsAsync — checks whether an email address is already in use.
    //
    // The excludeId parameter allows the update use case to check for
    // duplicates while ignoring the employee being updated. When creating
    // a new employee, pass 0 (or omit the parameter) to check all records.
    //
    // Why a pre-insert check AND a unique database index?
    // The application check provides a user-friendly error message before
    // attempting to save. The database index is the final safety net that
    // prevents duplicates even under concurrent requests (race conditions).
    // Neither alone is sufficient.
    // -----------------------------------------------------------------------
    Task<bool> EmailExistsAsync(string email, int excludeId = 0, CancellationToken cancellationToken = default);

    // -----------------------------------------------------------------------
    // AddAsync — persists a new employee to the database.
    //
    // The employee entity is created by Application using the Domain
    // constructor. The repository adds it to the DbContext and saves.
    // After saving, the entity's Id property is populated by EF Core.
    // -----------------------------------------------------------------------
    Task AddAsync(Employee employee, CancellationToken cancellationToken = default);

    // -----------------------------------------------------------------------
    // UpdateAsync — saves changes to an existing employee.
    //
    // The employee entity was retrieved via GetByIdAsync (tracked) and
    // modified through Domain methods. This method calls SaveChangesAsync
    // to persist the modifications.
    // -----------------------------------------------------------------------
    Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);

    // -----------------------------------------------------------------------
    // DeleteAsync — removes an employee from the database by ID.
    //
    // Returns true if the employee was found and deleted, false if no
    // employee with that ID existed. This lets the Application layer
    // distinguish between "deleted successfully" and "not found".
    // -----------------------------------------------------------------------
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
