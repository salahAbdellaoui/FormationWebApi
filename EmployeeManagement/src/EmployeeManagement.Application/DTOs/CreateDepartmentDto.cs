// ============================================================================
// CreateDepartmentDto — data required to create a new department
// ============================================================================
//
// This DTO carries the input data for the "create department" use case.
// Data Annotations enforce API-level validation (required name, max lengths).
//
// Notable exclusions:
//   - Id: assigned by the database.
//   - IsActive: new departments are active by default (Domain rule).
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Application.DTOs;

/// <summary>
/// Contains the data required to create a new department.
/// </summary>
public class CreateDepartmentDto
{
    /// <summary>The department's display name. Required, max 100 characters.</summary>
    [Required(ErrorMessage = "Department name is required.")]
    [MaxLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description of the department's role, max 500 characters.</summary>
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }
}
