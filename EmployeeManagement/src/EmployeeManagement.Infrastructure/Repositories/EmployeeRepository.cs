// ============================================================================
// EmployeeRepository — EF Core implementation of IEmployeeRepository
// ============================================================================
//
// This class implements the employee data-access contract using Entity
// Framework Core and SQL Server. It lives in the Infrastructure layer
// because it depends on EF Core, which is a technical concern.
//
// The Application layer depends on the IEmployeeRepository interface,
// not on this class. This separation allows:
//   - Unit testing the Application service with a mock repository.
//   - Swapping the database technology without changing Application code.
//   - Keeping EF Core knowledge confined to the Infrastructure layer.
//
// Design decisions:
//   - Read-only queries use AsNoTracking() for better performance.
//   - Write operations retrieve tracked entities so EF Core can detect
//     changes and generate the correct UPDATE SQL.
//   - Each write method calls SaveChangesAsync to persist immediately.
//     This simplifies the training project and avoids the complexity of
//     a Unit of Work pattern.
//   - Duplicate email violations from the database are translated to
//     ConflictException. This handles the race condition where two
//     concurrent requests pass the application-level EmailExistsAsync
//     check but the database unique index rejects the second insert.
// ============================================================================

using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Interfaces.Repositories;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Infrastructure.Repositories;

/// <summary>
/// Implements <see cref="IEmployeeRepository"/> using EF Core and SQL Server.
/// </summary>
public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmployeeRepository> _logger;

    // SQL Server error numbers for unique constraint/index violations.
    // Error 2601: Cannot insert duplicate key row with unique index.
    // Error 2627: Violation of UNIQUE KEY constraint.
    // These are SQL Server-specific. If the database provider changes,
    // this detection logic must be updated for the new provider.
    private const int SqlUniqueIndexViolation = 2601;
    private const int SqlUniqueConstraintViolation = 2627;

    // The name of the unique index on the Email column, as configured in
    // EmployeeConfiguration. Used to identify email-specific violations
    // and distinguish them from other unique constraint violations.
    private const string EmailUniqueIndexName = "IX_Employees_Email";

    public EmployeeRepository(
        ApplicationDbContext context,
        ILogger<EmployeeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // -----------------------------------------------------------------------
    // GetByIdAsync — retrieves a single employee by ID.
    //
    // Returns a TRACKED entity because the caller (EmployeeService) needs
    // to call Domain methods and save changes. Without tracking, EF Core
    // would not detect the modifications.
    // -----------------------------------------------------------------------
    public async Task<Employee?> GetByIdAsync(
        int id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees.FindAsync(
            new object[] { id }, cancellationToken);
    }

    // -----------------------------------------------------------------------
    // GetAllAsync — retrieves all employees.
    //
    // Uses AsNoTracking() because the caller only reads data and converts
    // to DTOs. No changes will be saved, so tracking is unnecessary
    // overhead. AsNoTracking improves query performance by skipping the
    // change-tracking snapshot.
    // -----------------------------------------------------------------------
    public async Task<IReadOnlyList<Employee>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    // -----------------------------------------------------------------------
    // EmailExistsAsync — checks whether an email is already in use.
    //
    // The excludeId parameter supports the update use case: when updating
    // an employee, we check for duplicates while excluding the employee's
    // own record. For create, excludeId is 0 (no exclusion).
    //
    // Uses AnyAsync for an efficient EXISTS query — SQL Server checks
    // whether at least one matching row exists and stops immediately,
    // rather than loading full records.
    //
    // This check is a convenience for user-friendly error messages.
    // The unique database index is the real concurrency guarantee.
    // -----------------------------------------------------------------------
    public async Task<bool> EmailExistsAsync(
        string email, int excludeId = 0, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AnyAsync(e => e.Email == email && e.Id != excludeId,
                cancellationToken);
    }

    // -----------------------------------------------------------------------
    // AddAsync — adds a new employee and saves to the database.
    //
    // After SaveChangesAsync, the employee entity's Id is populated by
    // SQL Server's IDENTITY mechanism. The caller can then read the
    // generated Id from the same entity object.
    //
    // If the database rejects the insert due to a duplicate email (caught
    // by the unique index), the DbUpdateException is translated to a
    // ConflictException so the caller receives a clear error.
    // -----------------------------------------------------------------------
    public async Task AddAsync(
        Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Employees.Add(employee);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // Check if this is a duplicate email violation on the unique index.
            // If so, translate to ConflictException for a clear error response.
            // If it is a different database error, re-throw without modification.
            if (IsEmailUniquenessViolation(ex))
            {
                _logger.LogWarning(ex,
                    "Duplicate email detected at database level for {Email}",
                    employee.Email);
                throw new ConflictException(
                    $"An employee with email '{employee.Email}' already exists.");
            }

            throw;
        }
    }

    // -----------------------------------------------------------------------
    // UpdateAsync — saves changes to an existing employee.
    //
    // The employee entity was retrieved by GetByIdAsync (tracked), so
    // EF Core's change tracker already knows about it. When Domain methods
    // modify the entity's properties, EF Core detects the changes.
    // SaveChangesAsync generates the UPDATE SQL for only the changed columns.
    //
    // Like AddAsync, duplicate email violations are translated to
    // ConflictException for clear error handling.
    // -----------------------------------------------------------------------
    public async Task UpdateAsync(
        Employee employee, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (IsEmailUniquenessViolation(ex))
            {
                _logger.LogWarning(ex,
                    "Duplicate email detected at database level during update for {Email}",
                    employee.Email);
                throw new ConflictException(
                    $"An employee with email '{employee.Email}' already exists.");
            }

            throw;
        }
    }

    // -----------------------------------------------------------------------
    // DeleteAsync — removes an employee by ID.
    //
    // Retrieves the entity first to confirm it exists. If found, removes
    // it and saves. Returns true if deleted, false if not found.
    // -----------------------------------------------------------------------
    public async Task<bool> DeleteAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees.FindAsync(
            new object[] { id }, cancellationToken);

        if (employee is null)
            return false;

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    // =======================================================================
    // Private helpers
    // =======================================================================

    // -----------------------------------------------------------------------
    // IsEmailUniquenessViolation — checks whether a DbUpdateException is
    // caused by a duplicate email violation on the IX_Employees_Email index.
    //
    // This is SQL Server-specific:
    //   - SqlException.Number 2601 = duplicate key row with unique index
    //   - SqlException.Number 2627 = unique constraint violation
    //   - The error message contains the index name "IX_Employees_Email"
    //
    // Why check the index name?
    //   SQL Server can throw 2601/2627 for any unique index violation.
    //   We only want to handle the email uniqueness case. Other unique
    //   constraint violations (if any are added in the future) should
    //   not be silently translated to "email already exists" errors.
    //
    // Provider-specific limitation:
    //   This detection relies on SQL Server error numbers and index names.
    //   If the database provider changes to PostgreSQL, MySQL, etc., this
    //   method must be rewritten for the new provider's error codes.
    // -----------------------------------------------------------------------
    private static bool IsEmailUniquenessViolation(DbUpdateException ex)
    {
        // Walk the inner exceptions to find the SqlException.
        // DbUpdateException wraps the database-specific exception.
        var sqlException = ex.InnerException as SqlException;

        if (sqlException is null)
            return false;

        // Check for SQL Server unique violation error numbers.
        if (sqlException.Number != SqlUniqueIndexViolation &&
            sqlException.Number != SqlUniqueConstraintViolation)
            return false;

        // Verify the violation is on the email index by checking the
        // error message. This ensures we only handle email conflicts,
        // not other unique constraint violations.
        return sqlException.Message.Contains(EmailUniqueIndexName,
            StringComparison.OrdinalIgnoreCase);
    }
}
