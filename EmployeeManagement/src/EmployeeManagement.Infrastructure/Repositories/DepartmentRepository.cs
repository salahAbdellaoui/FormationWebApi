// ============================================================================
// DepartmentRepository — EF Core implementation of IDepartmentRepository
// ============================================================================
//
// This class implements the department data-access contract using EF Core
// and SQL Server. Like EmployeeRepository, it keeps EF Core knowledge
// confined to the Infrastructure layer.
//
// Key operations:
//   - Standard CRUD (GetById, GetAll, Add, Update, Delete).
//   - ExistsAsync — efficient existence check for department validation.
//   - HasEmployeesAsync — efficient check for assigned employees before
//     deletion. Uses AnyAsync (SQL EXISTS) instead of loading all employees.
// ============================================================================

using EmployeeManagement.Application.Interfaces.Repositories;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Repositories;

/// <summary>
/// Implements <see cref="IDepartmentRepository"/> using EF Core and SQL Server.
/// </summary>
public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _context;

    public DepartmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // -----------------------------------------------------------------------
    // GetByIdAsync — retrieves a tracked department by ID.
    // Tracked because the caller may modify it and save changes.
    // -----------------------------------------------------------------------
    public async Task<Department?> GetByIdAsync(
        int id, CancellationToken cancellationToken = default)
    {
        return await _context.Departments.FindAsync(
            new object[] { id }, cancellationToken);
    }

    // -----------------------------------------------------------------------
    // GetAllAsync — retrieves all departments without tracking.
    // AsNoTracking improves performance for read-only scenarios.
    // -----------------------------------------------------------------------
    public async Task<IReadOnlyList<Department>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    // -----------------------------------------------------------------------
    // ExistsAsync — checks whether a department with the given ID exists.
    //
    // Uses AnyAsync for an efficient SQL EXISTS query. This is faster than
    // loading the full entity when we only need to know whether it exists.
    // -----------------------------------------------------------------------
    public async Task<bool> ExistsAsync(
        int id, CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AnyAsync(d => d.Id == id, cancellationToken);
    }

    // -----------------------------------------------------------------------
    // HasEmployeesAsync — checks whether any employees belong to a department.
    //
    // This is used before deleting a department to prevent orphaning
    // employee records. Uses AnyAsync against the Employees DbSet with
    // a filter on DepartmentId. This generates a SQL EXISTS query that
    // stops as soon as it finds the first matching row.
    //
    // Why not load all employees and check the count? Loading employees
    // wastes memory and bandwidth when we only need a yes/no answer.
    // -----------------------------------------------------------------------
    public async Task<bool> HasEmployeesAsync(
        int departmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AnyAsync(e => e.DepartmentId == departmentId, cancellationToken);
    }

    // -----------------------------------------------------------------------
    // AddAsync — adds a new department and saves to the database.
    // After saving, the department's Id is populated by SQL Server IDENTITY.
    // -----------------------------------------------------------------------
    public async Task AddAsync(
        Department department, CancellationToken cancellationToken = default)
    {
        _context.Departments.Add(department);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // -----------------------------------------------------------------------
    // UpdateAsync — saves changes to an existing tracked department.
    // EF Core detects property modifications and generates UPDATE SQL.
    // -----------------------------------------------------------------------
    public async Task UpdateAsync(
        Department department, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    // -----------------------------------------------------------------------
    // DeleteAsync — removes a department by ID.
    // Returns true if found and deleted, false if not found.
    //
    // Note: The Application layer checks for assigned employees before
    // calling this method. The database FK constraint (Restrict) is the
    // final safety net — if an employee is assigned concurrently, the
    // database will reject the DELETE and EF Core will throw.
    // -----------------------------------------------------------------------
    public async Task<bool> DeleteAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments.FindAsync(
            new object[] { id }, cancellationToken);

        if (department is null)
            return false;

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
