// ============================================================================
// IEmployeeService — contract for Employee application use cases
// ============================================================================
//
// This interface defines the Employee-related operations that the API layer
// can invoke. Each method represents a use case: a complete business
// operation that may involve multiple repository calls, validation checks,
// and DTO conversions.
//
// The service uses DTOs for input and output, never exposing Domain entities
// directly to the API layer. This keeps the HTTP layer decoupled from the
// Domain model and allows the service to evolve independently.
// ============================================================================

using EmployeeManagement.Application.DTOs;

namespace EmployeeManagement.Application.Interfaces.Services;

/// <summary>
/// Defines the Employee use cases that the API layer can invoke.
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// Retrieves a single employee by ID.
    /// Returns null if the employee does not exist.
    /// </summary>
    Task<EmployeeResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all employees as response DTOs.
    /// Returns an empty list if no employees exist.
    /// </summary>
    Task<IReadOnlyList<EmployeeResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new employee from the provided DTO.
    /// Validates department existence and email uniqueness before persisting.
    /// Returns the created employee as a response DTO.
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">
    /// Thrown when the referenced department does not exist.
    /// </exception>
    /// <exception cref="Exceptions.ConflictException">
    /// Thrown when the email address is already in use.
    /// </exception>
    Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing employee's personal and employment details.
    /// Validates department existence and email uniqueness before persisting.
    /// Returns the updated employee as a response DTO, or null if not found.
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">
    /// Thrown when the referenced department does not exist.
    /// </exception>
    /// <exception cref="Exceptions.ConflictException">
    /// Thrown when the email address is already in use by another employee.
    /// </exception>
    Task<EmployeeResponseDto?> UpdateAsync(int id, UpdateEmployeeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an employee by ID.
    /// Returns true if the employee was found and deleted, false if not found.
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
