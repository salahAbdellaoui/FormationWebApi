# 06 — Global Exception Handling

**Duration**: 30 minutes

---

## Why Global Exception Handling?

Unexpected exceptions will happen. The database goes down. A network call times out. A null reference slips through. These are not "if" questions — they are "when" questions.

Without centralized exception handling, every unhandled exception bubbles up to ASP.NET Core's default handler. That default handler returns a 500 Internal Server Error, and depending on the environment, it may expose stack traces, SQL error messages, file paths, and other internal details to the client.

This is both a security risk and a poor developer experience. Clients receive cryptic error pages. Developers struggle to find the root cause among scattered try-catch blocks. And attackers gain information about your internal architecture.

Global exception handling solves all three problems in one place.

---

## The Problem

Consider what happens today when `EmployeeService.CreateEmployeeAsync` throws an unexpected exception — say, the database connection fails.

**Without exception handling:**

1. The exception propagates up through the controller action.
2. No try-catch catches it.
3. ASP.NET Core's default handler takes over.
4. The client receives an HTTP 500 response.

What the client sees depends on the environment:

**In Development**, the response might include a detailed error page with:

- The full exception message
- The complete stack trace
- File paths on the server
- Line numbers in the source code

**In Production**, the response is a generic 500 error. But depending on configuration, it might still leak:

- Database error messages (table names, column names, SQL syntax)
- Internal server paths
- Assembly versions
- Partial stack traces

Neither outcome is acceptable. Development detail is useful for the developer working locally but should never reach a client. Production responses must reveal nothing about the internal system.

---

## ASP.NET Core Exception Handling Mechanisms

ASP.NET Core provides several built-in mechanisms for handling exceptions:

### UseExceptionHandler Middleware

This is the primary exception handling middleware. It catches unhandled exceptions and lets you define a custom response. You can configure it to branch based on environment.

### Problem Details (RFC 7807)

RFC 7807 defines a standard format for error responses called "Problem Details." It is a JSON object with these fields:

| Field | Purpose |
|---|---|
| `type` | A URI that identifies the error type |
| `title` | A short, human-readable summary |
| `status` | The HTTP status code |
| `detail` | A human-readable explanation (optional) |
| `instance` | A URI that identifies the specific occurrence (optional) |

ASP.NET Core has built-in support for Problem Details through the `AddProblemDetails()` service registration and `UseExceptionHandler()` middleware.

### UseStatusCodePages

This middleware handles non-success status codes (like 404 Not Found) that do not have a response body. It is not for exceptions, but for completeness when building error handling.

---

## Implementation for .NET 8

.NET 8 provides a clean, minimal approach to global exception handling using Problem Details.

### Step 1: Register Problem Details

In `Program.cs`, add Problem Details support to the service container:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
```

`AddProblemDetails()` registers the `IProblemDetailsClient` and configures the framework to produce RFC 7807-compliant error responses.

### Step 2: Configure the Exception Handler

After building the app, add the exception handler middleware. The placement matters: it must come before other middleware that might throw exceptions.

```csharp
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        if (app.Environment.IsDevelopment())
        {
            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (exceptionHandlerFeature != null)
            {
                problemDetails.Detail = exceptionHandlerFeature.Error.Message;
                problemDetails.Instance = exceptionHandlerFeature.Error.StackTrace;
            }
        }

        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Step 3: The Simpler Approach

For most cases, .NET 8 allows an even simpler configuration. If you just call:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
```

This automatically returns Problem Details for all unhandled exceptions. In Development, the response includes the exception message and stack trace. In Production, it returns only the generic "An unexpected error occurred" message with a `traceId`.

The simpler approach is sufficient for most APIs. Use the manual configuration when you need custom behavior (different error formats, additional fields, environment-specific logic).

---

## Development vs Production

The environment determines what the error response reveals.

### Development

In Development, the error response includes detailed information to help the developer debug:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An unexpected error occurred",
  "status": 500,
  "detail": "System.NullReferenceException: Object reference not set to an instance of an object.",
  "instance": "   at EmployeeManagement.Application.Services.EmployeeService.CreateEmployeeAsync(CreateEmployeeDto dto) in C:\\Projects\\EmployeeManagement\\EmployeeManagement.Application\\Services\\EmployeeService.cs:line 42\n   at EmployeeManagement.Api.Controllers.EmployeesController.Create(CreateEmployeeDto dto) in C:\\Projects\\EmployeeManagement\\EmployeeManagement.Api\\Controllers\\EmployeesController.cs:line 39",
  "traceId": "00-abc123def456-789xyz-00"
}
```

This is useful locally. You see exactly what failed and where. You would never send this to a client in Production.

### Production

In Production, the error response is deliberately vague:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An unexpected error occurred",
  "status": 500,
  "traceId": "00-abc123def456-789xyz-00"
}
```

No stack trace. No exception message. No file paths. No SQL errors. No internal details of any kind. The `traceId` is the only useful piece — it allows the developer to look up the full error in the logs.

---

## Safe Production Error Responses

A production error response should answer exactly one question: "Something went wrong on our side." It should not answer:

- What went wrong (no exception messages)
- Where it went wrong (no file paths or line numbers)
- How it went wrong (no stack traces)
- What data was involved (no SQL queries or parameter values)

The response should contain:

| Field | Production Value | Purpose |
|---|---|---|
| `type` | RFC 7807 URI | Identifies the error category |
| `title` | "An unexpected error occurred" | Human-readable summary |
| `status` | 500 | HTTP status code |
| `traceId` | Auto-generated by ASP.NET Core | For log correlation |

That is all. The client knows the request failed. The developer can use the `traceId` to find the real error in the logs. Nobody else learns anything about the system.

---

## Logging Unexpected Failures

While the error response hides details from the client, those details must exist somewhere for debugging. That somewhere is the log.

The `UseExceptionHandler` middleware logs exceptions automatically. When an unhandled exception occurs, ASP.NET Core writes a log entry at the Error level that includes:

- The full exception type and message
- The complete stack trace
- The request path and method
- The `traceId` for correlation

You do not need to add explicit logging in the exception handler. The framework handles it.

What you should do is ensure that your logging configuration captures these entries. In `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore.Diagnostics.ExceptionHandler": "Error"
    }
  }
}
```

For production, consider sending logs to a centralized logging system (Application Insights, Seq, Serilog with a file or database sink) so that you can search and analyze errors after they occur.

---

## Expected Failures vs Unexpected Exceptions

Not all errors are the same. Your handling strategy must distinguish between two categories.

### Expected Failures

These are errors that are a normal part of the application's operation. You can anticipate them and return a specific status code.

| Situation | Status Code | Example |
|---|---|---|
| Invalid input | 400 Bad Request | Name is empty, email format is wrong |
| Resource not found | 404 Not Found | Employee with ID 42 does not exist |
| Unauthenticated | 401 Unauthorized | No JWT token provided |
| Unauthorized | 403 Forbidden | User lacks the required role |
| Conflict | 409 Conflict | Email already exists |

Expected failures should be handled explicitly. The service or controller checks the condition and returns the appropriate response. They do not need the global exception handler.

### Unexpected Exceptions

These are errors you cannot predict or prevent at the application level.

| Situation | Status Code | Example |
|---|---|---|
| Database unavailable | 500 Internal Server Error | SQL Server is down |
| Network timeout | 500 Internal Server Error | External API does not respond |
| Null reference | 500 Internal Server Error | A bug in the code |
| Out of memory | 500 Internal Server Error | System resource exhaustion |

Unexpected exceptions are what the global exception handler catches. You do not write try-catch blocks for these in every controller. You let them bubble up, and the handler returns a safe 500 response.

The distinction is important. Do not use the global exception handler for expected failures. Returning a 500 for a validation error is wrong — the client sent bad input, not a server error. Handle expected failures with explicit checks and appropriate status codes. Reserve the global handler for the unexpected.

---

## Correlation and Trace Information

Every HTTP request in ASP.NET Core receives a `traceId`. This identifier appears in:

- Log entries for that request
- Problem Details error responses
- Response headers (if configured)

The `traceId` serves one purpose: correlation. When a client reports an error, they can provide the `traceId` from the response. You search your logs for that `traceId` and find every log entry for that specific request — the controller entry, the service call, the database query, and the exception that caused the failure.

Without the `traceId`, debugging a production issue means searching through thousands of log entries and hoping you find the right ones. With the `traceId`, you narrow it down immediately.

ASP.NET Core includes the `traceId` in Problem Details responses automatically when you use `AddProblemDetails()`. No extra configuration needed.

---

## What NOT to Expose

This list is non-negotiable. These must never appear in a production error response:

| Category | Why |
|---|---|
| Stack traces | Reveal internal code structure, file paths, class names |
| SQL error messages | Reveal table names, column names, query structure |
| File paths | Reveal server directory structure |
| Connection strings | Contain database credentials |
| Assembly versions | Help attackers identify known vulnerabilities |
| Internal IP addresses | Reveal network topology |
| Sensitive personal data | Privacy violation (PII, financial data) |

The rule is simple: if it would help an attacker understand your system, it does not belong in an error response. The error response exists to tell the client "something went wrong." Everything else belongs in the logs.

---

## Common Mistakes

**Mistake 1: Exposing stack traces in Production.**
The most common and most dangerous mistake. A developer configures the exception handler, tests it in Development (where stack traces are enabled), and forgets to disable them for Production. The result: every unhandled exception leaks the full call stack to clients. Always check `app.Environment.IsDevelopment()` before including detail fields.

**Mistake 2: Catching all exceptions and returning 200 OK.**
Some developers wrap every controller action in a try-catch and return `Ok(new { error = ex.Message })`. This hides errors from monitoring tools, breaks HTTP semantics (a 200 means success), and makes it impossible for clients to distinguish success from failure using status codes. Let exceptions bubble up. The global handler exists for a reason.

**Mistake 3: Not logging exceptions.**
The error response hides details from the client, which means the only place those details exist is the log. If you do not log, you have no way to debug production issues. The `UseExceptionHandler` middleware logs automatically, but if you write a custom handler, make sure it logs with `_logger.LogError(ex, ...)`.

**Mistake 4: Using try-catch in every controller action.**
This is the opposite of global exception handling. If you have try-catch blocks in 15 controller actions, you have 15 places to maintain, 15 places that might handle errors differently, and 15 places where a developer might forget to log. One global handler replaces all of them.

**Mistake 5: Returning 500 for expected failures.**
A validation error is not a server error. A missing resource is not a server error. An unauthorized request is not a server error. These are expected failures with specific status codes (400, 404, 401). Using 500 for everything makes it impossible to distinguish "the server broke" from "you sent bad data."

---

## Knowledge Check

**Question 1**: Your API returns a 500 Internal Server Error when a client sends a request with an empty `Name` field. Is this correct? What status code should it return, and why?

**Question 2**: A developer adds `problemDetails.Detail = exception.Message;` to the exception handler without checking the environment. What is the risk in Production?

**Question 3**: A client reports a 500 error. The error response includes a `traceId` of `00-abc123-789xyz-00`. What do you do with this information?

<details>
<summary>Answers</summary>

**Answer 1**: No, 500 is incorrect. An empty `Name` is a validation error — the client sent invalid input. The correct status code is 400 Bad Request. A 500 means "the server encountered an unexpected condition." Bad input is not unexpected; it is a normal part of API operation. Validation should be handled explicitly (with FluentValidation or Data Annotations), and the response should return 400 with details about which fields are invalid.

**Answer 2**: In Production, `exception.Message` might contain a SQL error like `"Invalid column name 'Salery'"` (revealing table structure), a connection error like `"Cannot open database 'EmployeeDB' requested by login 'sa'"` (revealing database names and credentials), or a null reference with internal class names. All of these leak information that helps an attacker understand the system. The `Detail` field should only be populated in Development.

**Answer 3**: You use the `traceId` to search your application logs. Every log entry generated during that specific request has the same `traceId`. By filtering on it, you find the complete sequence of events: the controller entry, the service call, the database query (if logged), and the exception with its full stack trace. This tells you exactly what went wrong without having to reproduce the issue.

</details>
