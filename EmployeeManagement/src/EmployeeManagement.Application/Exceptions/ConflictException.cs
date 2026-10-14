// ============================================================================
// ConflictException — thrown when an operation conflicts with existing data
// ============================================================================
//
// This exception signals that a use case cannot proceed because the
// requested action conflicts with the current state of the data. Examples:
//   - Creating an employee with an email that already exists.
//   - Deleting a department that still has employees assigned to it.
//
// The message should explain what conflict occurred so that the exception
// handler can produce a meaningful error response for the API caller.
//
// Like NotFoundException, this exception does not reference HTTP types.
// A later phase will map it to an appropriate HTTP status code (409 or 400).
// ============================================================================

namespace EmployeeManagement.Application.Exceptions;

/// <summary>
/// Thrown when an operation cannot proceed due to a data conflict.
/// </summary>
public class ConflictException : Exception
{
    /// <summary>
    /// Creates a ConflictException with a descriptive message.
    /// </summary>
    public ConflictException(string message)
        : base(message)
    {
    }
}
