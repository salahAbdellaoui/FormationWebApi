// ============================================================================
// DepartmentResponseDto — data returned to the API caller for a department
// ============================================================================
//
// This DTO shapes the department data that the API returns in HTTP responses.
// Separate from the Domain entity for the same reasons as EmployeeResponseDto:
// controlled serialization, decoupled response shape, no Domain leakage.
// ============================================================================

namespace EmployeeManagement.Application.DTOs;

/// <summary>
/// Represents the department data returned in API responses.
/// </summary>
public class DepartmentResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
