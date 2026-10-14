// ============================================================================
// IDepartmentService — contract for Department application use cases
// ============================================================================
//
// This interface defines the Department-related operations that the API
// layer can invoke. Each method represents a complete business use case.
//
// Like IEmployeeService, this interface uses DTOs for input and output.
// The service handles application-level rules (e.g., preventing deletion
// of departments with assigned employees) before delegating persistence
// to the repository.
// ============================================================================

using EmployeeManagement.Application.DTOs;

namespace EmployeeManagement.Application.Interfaces.Services;

/// <summary>
/// Defines the Department use cases that the API layer can invoke.
/// </summary>
public interface IDepartmentService
{
    /// <summary>
    /// Retrieves a single department by ID.
    /// Returns null if the department does not exist.
    /// </summary>
    Task<DepartmentResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all departments as response DTOs.
    /// Returns an empty list if no departments exist.
    /// </summary>
    Task<IReadOnlyList<DepartmentResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new department from the provided DTO.
    /// Returns the created department as a response DTO.
    /// </summary>
    Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing department's name and description.
    /// Returns the updated department as a response DTO, or null if not found.
    /// </summary>
    Task<DepartmentResponseDto?> UpdateAsync(int id, UpdateDepartmentDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a department by ID.
    /// Returns true if the department was found and deleted, false if not found.
    /// </summary>
    /// <exception cref="Exceptions.ConflictException">
    /// Thrown when the department has employees assigned to it.
    /// The department must be emptied before deletion.
    /// </exception>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
