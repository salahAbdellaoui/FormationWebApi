// ============================================================================
// EmployeeResponseDto — data returned to the API caller for an employee
// ============================================================================
//
// This DTO shapes the employee data that the API returns in HTTP responses.
// It includes all relevant employee details plus the department ID.
//
// Why a separate response DTO instead of returning the Domain entity?
//   1. The Domain entity has private setters — the JSON serializer cannot
//      write them, which would produce confusing output.
//   2. The response shape may differ from the Domain model over time
//      (e.g., adding computed fields, removing internal properties).
//   3. It prevents accidental exposure of Domain internals.
// ============================================================================

namespace EmployeeManagement.Application.DTOs;

/// <summary>
/// Represents the employee data returned in API responses.
/// </summary>
public class EmployeeResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; }
}
