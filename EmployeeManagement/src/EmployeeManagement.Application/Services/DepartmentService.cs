// ============================================================================
// DepartmentService — Application-layer use cases for Department operations
// ============================================================================
//
// This service coordinates the business operations for departments. Like
// EmployeeService, it validates application-level rules, uses Domain
// entities, and delegates persistence to the repository.
//
// Key business rule: a department with assigned employees cannot be deleted.
// This is enforced by checking HasEmployeesAsync before calling DeleteAsync.
// The database FK constraint (Restrict) provides an additional safety net.
// ============================================================================

using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Interfaces.Repositories;
using EmployeeManagement.Application.Interfaces.Services;
using EmployeeManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Application.Services;

/// <summary>
/// Implements the Department use cases by coordinating repositories and
/// Domain entities.
/// </summary>
public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(
        IDepartmentRepository departmentRepository,
        ILogger<DepartmentService> logger)
    {
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    // -----------------------------------------------------------------------
    // GetByIdAsync — retrieves a single department by ID.
    // Returns null if the department does not exist.
    // -----------------------------------------------------------------------
    public async Task<DepartmentResponseDto?> GetByIdAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(id, cancellationToken);

        if (department is null)
            return null;

        return MapToResponseDto(department);
    }

    // -----------------------------------------------------------------------
    // GetAllAsync — retrieves all departments.
    // Returns an empty list when no departments exist.
    // -----------------------------------------------------------------------
    public async Task<IReadOnlyList<DepartmentResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var departments = await _departmentRepository.GetAllAsync(cancellationToken);

        return departments.Select(MapToResponseDto).ToList();
    }

    // -----------------------------------------------------------------------
    // CreateAsync — creates a new department.
    //
    // The Domain constructor validates the name (non-empty, trimmed).
    // If the name is invalid, the constructor throws ArgumentException.
    // -----------------------------------------------------------------------
    public async Task<DepartmentResponseDto> CreateAsync(
        CreateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        // Construct the Domain entity. The constructor validates the name
        // and throws ArgumentException for invalid input.
        var department = new Department(dto.Name, dto.Description);

        // Persist the entity. After saving, the department's Id is populated.
        await _departmentRepository.AddAsync(department, cancellationToken);

        _logger.LogInformation(
            "Department created with ID {DepartmentId}, name {DepartmentName}",
            department.Id, department.Name);

        return MapToResponseDto(department);
    }

    // -----------------------------------------------------------------------
    // UpdateAsync — updates an existing department's name and description.
    //
    // Retrieves the tracked entity, applies changes through Domain methods,
    // then persists. Returns null if the department was not found.
    // -----------------------------------------------------------------------
    public async Task<DepartmentResponseDto?> UpdateAsync(
        int id, UpdateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(id, cancellationToken);

        if (department is null)
            return null;

        // Apply changes through Domain methods. UpdateName validates the new
        // name and throws ArgumentException if it is empty or whitespace.
        department.UpdateName(dto.Name);
        department.UpdateDescription(dto.Description);

        await _departmentRepository.UpdateAsync(department, cancellationToken);

        _logger.LogInformation(
            "Department {DepartmentId} updated", id);

        return MapToResponseDto(department);
    }

    // -----------------------------------------------------------------------
    // DeleteAsync — deletes a department by ID.
    //
    // Before deleting, checks whether any employees are assigned to the
    // department. If employees exist, throws ConflictException because
    // deleting the department would orphan those employee records.
    //
    // Why check in the Application layer AND have a DB FK constraint?
    //   - The Application check produces a clear, user-friendly error message.
    //   - The DB constraint prevents data corruption even if the Application
    //     check is bypassed (e.g., by a race condition or a direct DB change).
    //
    // Returns true if deleted, false if the department was not found.
    // -----------------------------------------------------------------------
    public async Task<bool> DeleteAsync(
        int id, CancellationToken cancellationToken = default)
    {
        // Check whether the department exists. If not, return false so the
        // caller can produce a 404 response.
        var department = await _departmentRepository.GetByIdAsync(id, cancellationToken);

        if (department is null)
            return false;

        // Check whether any employees are assigned to this department.
        // Uses an efficient EXISTS query (not loading all employees).
        var hasEmployees = await _departmentRepository.HasEmployeesAsync(
            id, cancellationToken);

        if (hasEmployees)
        {
            throw new ConflictException(
                $"Cannot delete department '{department.Name}' because it has employees assigned to it. " +
                "Reassign or remove the employees before deleting the department.");
        }

        // No employees assigned — safe to delete.
        var deleted = await _departmentRepository.DeleteAsync(id, cancellationToken);

        if (deleted)
        {
            _logger.LogInformation(
                "Department {DepartmentId} ({DepartmentName}) deleted",
                id, department.Name);
        }

        return deleted;
    }

    // =======================================================================
    // Private mapping helper
    // =======================================================================

    private static DepartmentResponseDto MapToResponseDto(Department department)
    {
        return new DepartmentResponseDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            IsActive = department.IsActive
        };
    }
}
