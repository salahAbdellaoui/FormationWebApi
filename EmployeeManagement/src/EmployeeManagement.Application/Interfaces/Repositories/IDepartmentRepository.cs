// ============================================================================
// IDepartmentRepository — contract for Department data access
// ============================================================================
//
// This interface defines the operations that the Application layer needs to
// persist and retrieve Department entities. Like IEmployeeRepository, it
// keeps the Application layer independent of the database technology.
//
// The interface exposes only Domain entities and primitive types. No EF Core
// types, no IQueryable, no Infrastructure-specific classes.
// ============================================================================

using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Interfaces.Repositories;

/// <summary>
/// Defines data-access operations for <see cref="Department"/> entities.
/// Implemented by Infrastructure; consumed by Application services.
/// </summary>
public interface IDepartmentRepository
{
    // -----------------------------------------------------------------------
    // GetByIdAsync — retrieves a single department by its ID.
    //
    // Returns the tracked entity (needed for calling Domain methods and
    // saving changes) or null if no department with that ID exists.
    // -----------------------------------------------------------------------
    Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // -----------------------------------------------------------------------
    // GetAllAsync — retrieves all departments.
    //
    // Uses AsNoTracking in the implementation because callers only read
    // data and convert to DTOs.
    // -----------------------------------------------------------------------
    Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken = default);

    // -----------------------------------------------------------------------
    // ExistsAsync — checks whether a department with the given ID exists.
    //
    // Used by the employee service to validate that a DepartmentId in a
    // create or update request refers to a real department.
    // -----------------------------------------------------------------------
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    // -----------------------------------------------------------------------
    // HasEmployeesAsync — checks whether any employees are assigned to a
    // department.
    //
    // Used before deleting a department. If employees are assigned, the
    // Application layer rejects the deletion with a conflict error.
    // The implementation uses an efficient existence query (AnyAsync)
    // rather than loading all employees.
    // -----------------------------------------------------------------------
    Task<bool> HasEmployeesAsync(int departmentId, CancellationToken cancellationToken = default);

    // -----------------------------------------------------------------------
    // AddAsync — persists a new department to the database.
    // After saving, the entity's Id is populated by EF Core.
    // -----------------------------------------------------------------------
    Task AddAsync(Department department, CancellationToken cancellationToken = default);

    // -----------------------------------------------------------------------
    // UpdateAsync — saves changes to an existing department.
    // The entity was retrieved via GetByIdAsync (tracked) and modified
    // through Domain methods.
    // -----------------------------------------------------------------------
    Task UpdateAsync(Department department, CancellationToken cancellationToken = default);

    // -----------------------------------------------------------------------
    // DeleteAsync — removes a department from the database by ID.
    //
    // Returns true if the department was found and deleted, false if no
    // department with that ID existed.
    //
    // Note: The Application layer checks for assigned employees before
    // calling this method. The database FK constraint (Restrict) provides
    // an additional safety net if an employee is assigned concurrently.
    // -----------------------------------------------------------------------
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
