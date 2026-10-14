// ============================================================================
// EmployeeService — Application-layer use cases for Employee operations
// ============================================================================
//
// This service coordinates the business operations for employees. It does
// NOT contain database logic (that belongs in the repository) and does NOT
// contain HTTP concerns (that belongs in the controller). Its job is to:
//
//   1. Validate application-level rules (department exists, email unique).
//   2. Create and modify Domain entities through their constructors/methods.
//   3. Delegate persistence to the repository.
//   4. Convert Domain entities to DTOs for the API layer.
//
// The service depends on IEmployeeRepository and IDepartmentRepository
// (interfaces), not on concrete repository classes. This allows the service
// to be unit tested with mock repositories.
// ============================================================================

using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Interfaces.Repositories;
using EmployeeManagement.Application.Interfaces.Services;
using EmployeeManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Application.Services;

/// <summary>
/// Implements the Employee use cases by coordinating repositories and
/// Domain entities.
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<EmployeeService> _logger;

    // -----------------------------------------------------------------------
    // Constructor — receives dependencies through injection.
    //
    // Both repositories are injected as interfaces, so this service does
    // not know whether the real implementation uses SQL Server, a file,
    // or an in-memory collection. During unit testing, mock implementations
    // can be substituted.
    //
    // ILogger provides structured logging for operational visibility.
    // All log messages use named placeholders (not string interpolation)
    // so that log aggregation tools can index and search them.
    // -----------------------------------------------------------------------
    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository,
        ILogger<EmployeeService> logger)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    // -----------------------------------------------------------------------
    // GetByIdAsync — retrieves a single employee by ID and maps to DTO.
    //
    // Returns null if the employee does not exist. The API layer will
    // convert this to a 404 response.
    // -----------------------------------------------------------------------
    public async Task<EmployeeResponseDto?> GetByIdAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);

        if (employee is null)
            return null;

        return MapToResponseDto(employee);
    }

    // -----------------------------------------------------------------------
    // GetAllAsync — retrieves all employees and maps each to a DTO.
    //
    // Returns an empty list (not null) when no employees exist. This
    // simplifies the API response — the caller always gets a JSON array.
    // -----------------------------------------------------------------------
    public async Task<IReadOnlyList<EmployeeResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);

        return employees.Select(MapToResponseDto).ToList();
    }

    // -----------------------------------------------------------------------
    // CreateAsync — creates a new employee.
    //
    // Steps:
    //   1. Confirm the referenced department exists.
    //   2. Check that the email is not already in use.
    //   3. Construct the Domain entity (which validates all fields).
    //   4. Persist through the repository.
    //   5. Return the created employee as a DTO.
    //
    // The Domain entity constructor enforces field-level rules (non-empty
    // names, positive salary, valid department ID). If the constructor
    // throws ArgumentException, it means the input data was invalid —
    // this exception propagates to the caller.
    //
    // The pre-insert email check provides a user-friendly error before
    // attempting to save. The unique database index is the final safety
    // net against race conditions.
    // -----------------------------------------------------------------------
    public async Task<EmployeeResponseDto> CreateAsync(
        CreateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        // Step 1: Confirm the department exists before creating the employee.
        // This prevents orphaned employee records that reference non-existent
        // departments. The database FK constraint would also catch this, but
        // checking first gives a clearer error message.
        var departmentExists = await _departmentRepository.ExistsAsync(
            dto.DepartmentId, cancellationToken);

        if (!departmentExists)
        {
            throw new NotFoundException("Department", dto.DepartmentId);
        }

        // Step 2: Check email uniqueness. The excludeId is 0 because we are
        // creating a new employee — there is no existing record to exclude.
        var emailExists = await _employeeRepository.EmailExistsAsync(
            dto.Email, excludeId: 0, cancellationToken);

        if (emailExists)
        {
            throw new ConflictException(
                $"An employee with email '{dto.Email}' already exists.");
        }

        // Step 3: Construct the Domain entity. The constructor validates
        // all required fields and throws ArgumentException for invalid input.
        var employee = new Employee(
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.HireDate,
            dto.Salary,
            dto.DepartmentId,
            dto.PhoneNumber);

        // Step 4: Persist the entity. After SaveChangesAsync, the employee's
        // Id property is populated by the database.
        await _employeeRepository.AddAsync(employee, cancellationToken);

        // Step 5: Return the created employee as a response DTO.
        _logger.LogInformation(
            "Employee created with ID {EmployeeId}, email {Email}, department {DepartmentId}",
            employee.Id, employee.Email, employee.DepartmentId);

        return MapToResponseDto(employee);
    }

    // -----------------------------------------------------------------------
    // UpdateAsync — updates an existing employee's details.
    //
    // Steps:
    //   1. Retrieve the existing employee (tracked).
    //   2. Confirm the requested department exists.
    //   3. Check email uniqueness, excluding this employee's own ID.
    //   4. Update through Domain methods (UpdatePersonalDetails,
    //      UpdateEmploymentDetails).
    //   5. Persist and return the updated DTO.
    //
    // The Domain methods validate the new values. If they throw
    // ArgumentException, the exception propagates to the caller.
    // -----------------------------------------------------------------------
    public async Task<EmployeeResponseDto?> UpdateAsync(
        int id, UpdateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        // Step 1: Retrieve the tracked entity. We need a tracked entity
        // because we are going to modify it and save changes.
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);

        if (employee is null)
            return null;

        // Step 2: Confirm the department exists.
        var departmentExists = await _departmentRepository.ExistsAsync(
            dto.DepartmentId, cancellationToken);

        if (!departmentExists)
        {
            throw new NotFoundException("Department", dto.DepartmentId);
        }

        // Step 3: Check email uniqueness, excluding the current employee.
        // Without excludeId, the check would find this employee's own email
        // and reject the update even when the email has not changed.
        var emailExists = await _employeeRepository.EmailExistsAsync(
            dto.Email, excludeId: id, cancellationToken);

        if (emailExists)
        {
            throw new ConflictException(
                $"An employee with email '{dto.Email}' already exists.");
        }

        // Step 4: Update through Domain methods. These methods validate the
        // new values (non-empty names, valid email, positive salary, etc.)
        // and throw ArgumentException for invalid input.
        employee.UpdatePersonalDetails(
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.PhoneNumber);

        employee.UpdateEmploymentDetails(dto.Salary, dto.DepartmentId);

        // Step 5: Persist the changes.
        await _employeeRepository.UpdateAsync(employee, cancellationToken);

        _logger.LogInformation(
            "Employee {EmployeeId} updated", id);

        return MapToResponseDto(employee);
    }

    // -----------------------------------------------------------------------
    // DeleteAsync — deletes an employee by ID.
    //
    // Delegates the existence check and deletion to the repository.
    // The repository returns true if the employee was found and deleted,
    // false if not found.
    // -----------------------------------------------------------------------
    public async Task<bool> DeleteAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var deleted = await _employeeRepository.DeleteAsync(id, cancellationToken);

        if (deleted)
        {
            _logger.LogInformation("Employee {EmployeeId} deleted", id);
        }
        else
        {
            _logger.LogWarning("Employee delete requested for non-existent ID {EmployeeId}", id);
        }

        return deleted;
    }

    // =======================================================================
    // Private mapping helper
    // =======================================================================

    // -----------------------------------------------------------------------
    // MapToResponseDto — converts a Domain Employee entity to a response DTO.
    //
    // This explicit mapping (instead of AutoMapper) keeps the transformation
    // visible and understandable. Each property assignment is a single line
    // that a trainee can read and verify.
    // -----------------------------------------------------------------------
    private static EmployeeResponseDto MapToResponseDto(Employee employee)
    {
        return new EmployeeResponseDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            HireDate = employee.HireDate,
            Salary = employee.Salary,
            DepartmentId = employee.DepartmentId,
            IsActive = employee.IsActive
        };
    }
}
