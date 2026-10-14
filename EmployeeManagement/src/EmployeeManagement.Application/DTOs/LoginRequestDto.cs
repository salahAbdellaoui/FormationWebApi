// ============================================================================
// LoginRequestDto — data required to authenticate a user
// ============================================================================
//
// This DTO carries the credentials for the login endpoint. The service
// verifies these against Identity's user store and returns a JWT on success.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Application.DTOs;

/// <summary>
/// Contains the credentials required to log in.
/// </summary>
public class LoginRequestDto
{
    /// <summary>The username to authenticate. Required.</summary>
    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; } = string.Empty;

    /// <summary>The password to verify. Required.</summary>
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}
