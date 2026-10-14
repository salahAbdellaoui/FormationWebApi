// ============================================================================
// UpdateEmployeeDto — data required to update an existing employee
// ============================================================================
//
// This DTO carries the input data for the "update employee" use case.
// It contains the same fields as CreateEmployeeDto because updating an
// employee replaces all mutable properties. The employee's Id is provided
// in the URL route, not in the DTO body.
//
// Data Annotations provide the same API-level validation as CreateEmployeeDto.
// The validation rules are consistent with the database column constraints.
//
// Notable exclusions:
//   - Id: provided in the URL route, not in the body.
//   - IsActive: activation/deactivation is a separate Domain operation.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Application.DTOs;

/// <summary>
/// Contains the data required to update an existing employee's details.
/// </summary>
public class UpdateEmployeeDto
{
    /// <summary>The employee's first (given) name. Required, max 100 characters.</summary>
    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>The employee's last (family) name. Required, max 100 characters.</summary>
    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>The employee's email address. Required, max 255 characters, basic format check.</summary>
    [Required(ErrorMessage = "Email is required.")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Optional phone number, max 30 characters.</summary>
    [Phone(ErrorMessage = "Please provide a valid phone number.")]
    [MaxLength(30, ErrorMessage = "Phone number cannot exceed 30 characters.")]
    public string? PhoneNumber { get; set; }

    /// <summary>The employee's hire date. Required.</summary>
    [Required(ErrorMessage = "Hire date is required.")]
    public DateTime HireDate { get; set; }

    /// <summary>The employee's salary. Must be zero or positive.</summary>
    [Range(0, double.MaxValue, ErrorMessage = "Salary cannot be negative.")]
    public decimal Salary { get; set; }

    /// <summary>The ID of the department the employee belongs to. Must be greater than zero.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Department ID must be greater than zero.")]
    public int DepartmentId { get; set; }
}
