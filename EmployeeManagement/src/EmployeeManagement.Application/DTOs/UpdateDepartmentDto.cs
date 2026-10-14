// ============================================================================
// UpdateDepartmentDto — data required to update an existing department
// ============================================================================
//
// This DTO carries the input data for the "update department" use case.
// The department's Id is provided in the URL route, not in the body.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Application.DTOs;

/// <summary>
/// Contains the data required to update an existing department.
/// </summary>
public class UpdateDepartmentDto
{
    /// <summary>The department's new display name. Required, max 100 characters.</summary>
    [Required(ErrorMessage = "Department name is required.")]
    [MaxLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>The department's new description. Nullable, max 500 characters.</summary>
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }
}
