// ============================================================================
// NotFoundException — thrown when a requested entity does not exist
// ============================================================================
//
// This exception signals that a use case could not find the entity it was
// looking for (e.g., an employee ID that does not exist, or a department
// that is not in the database).
//
// Why a custom exception instead of returning null?
//   - Some callers need to distinguish "not found" from "found but empty".
//   - The exception carries a message explaining what was not found and
//     which identifier was used, making error responses more informative.
//   - A later phase (global exception middleware) will catch this exception
//     and convert it to an HTTP 404 response.
//
// This exception does not reference ASP.NET Core HTTP types. It is a pure
// Application-layer concept that can be caught and handled by any consumer.
// ============================================================================

namespace EmployeeManagement.Application.Exceptions;

/// <summary>
/// Thrown when a requested entity cannot be found in the data store.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Creates a NotFoundException with a descriptive message.
    /// </summary>
    /// <param name="entityName">The type of entity that was not found (e.g., "Employee").</param>
    /// <param name="identifier">The identifier that was used to search (e.g., the ID value).</param>
    public NotFoundException(string entityName, object identifier)
        : base($"{entityName} with identifier '{identifier}' was not found.")
    {
    }

    /// <summary>
    /// Creates a NotFoundException with a custom message.
    /// </summary>
    public NotFoundException(string message)
        : base(message)
    {
    }
}
