// ============================================================================
// ExceptionHandlingMiddleware — centralized HTTP error mapping
// ============================================================================
//
// This middleware catches exceptions thrown by Application services and
// maps them to appropriate HTTP status codes with safe JSON responses.
//
// Without this middleware, unhandled exceptions would either:
//   - Crash the request and return a generic 500 with a stack trace
//     (in Development), or
//   - Return an empty 500 (in Production).
//
// Neither is acceptable. API callers need structured error responses, and
// the server must never leak internal details (stack traces, SQL errors,
// connection strings) to the client.
//
// This middleware handles:
//   - NotFoundException → 404 Not Found
//   - ConflictException → 409 Conflict
//   - ArgumentException → 400 Bad Request (Domain validation failures)
//   - Any other exception → 500 Internal Server Error (safe message only)
//
// The middleware is placed early in the pipeline so it wraps all subsequent
// middleware and controller actions.
// ============================================================================

using System.Net;
using System.Text.Json;
using EmployeeManagement.Application.Exceptions;

namespace EmployeeManagement.API.Middleware;

/// <summary>
/// Catches exceptions from downstream middleware and controllers and
/// converts them to structured JSON error responses with appropriate
/// HTTP status codes.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    // JSON serializer options for consistent error response formatting.
    // CamelCase property names match the rest of the API's JSON output.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Continue to the next middleware/controller in the pipeline.
            // If nothing throws, the response is returned normally.
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            // The requested entity was not found. Return 404 with the
            // exception message, which describes what was not found.
            _logger.LogWarning(ex, "Entity not found");
            await WriteErrorResponseAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ConflictException ex)
        {
            // A data conflict prevents the operation (e.g., duplicate email,
            // deleting a department with assigned employees). Return 409.
            _logger.LogWarning(ex, "Data conflict");
            await WriteErrorResponseAsync(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (ArgumentException ex)
        {
            // Domain validation failed (e.g., empty name, negative salary).
            // The Domain entity constructor or methods threw because the
            // input violated a business rule. Return 400.
            _logger.LogWarning(ex, "Domain validation failed");
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            // An unexpected error occurred. Log the full details server-side
            // for debugging, but return only a safe generic message to the
            // client. Never expose stack traces, SQL errors, or internal
            // paths to the API caller.
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please try again later.");
        }
    }

    // -----------------------------------------------------------------------
    // WriteErrorResponseAsync — writes a JSON error response to the client.
    //
    // The response body contains a simple JSON object with a "message" field.
    // This keeps the error format consistent across all error types.
    // -----------------------------------------------------------------------
    private static async Task WriteErrorResponseAsync(
        HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var errorResponse = new { message };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(errorResponse, JsonOptions));
    }
}
