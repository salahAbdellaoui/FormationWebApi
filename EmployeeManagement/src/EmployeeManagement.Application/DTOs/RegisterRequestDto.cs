// ============================================================================
// RegisterRequestDto — data required to register a new user account
// ============================================================================
//
// This DTO carries the input data for the registration endpoint. It includes
// only the fields needed to create a new Identity account.
//
// Security note: The DTO does NOT include a Role field. Allowing clients to
// specify a role during registration would let anyone assign themselves the
// Admin role. New accounts always receive the default "Employee" role.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Application.DTOs;

/// <summary>
/// Contains the data required to register a new user account.
/// </summary>
public class RegisterRequestDto
{
    /// <summary>The desired username. Required, 3–50 characters.</summary>
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    public string Username { get; set; } = string.Empty;

    /// <summary>The user's email address. Required, valid format.</summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>The user's password. Required, minimum 8 characters.</summary>
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
    public string Password { get; set; } = string.Empty;
}
