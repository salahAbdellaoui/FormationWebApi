# 08 — Logging

**Duration**: 30 minutes

---

## Why Logging Matters

Logging is how your application communicates with you after deployment. Without logs, you are blind in production. You cannot tell what requests are coming in, what operations succeeded, what failed, or why. A bug report says "the API returned a 500." Without logs, you have no way to find out what caused it unless you can reproduce the exact scenario.

Logs serve three purposes:

1. **Debugging**: Finding the root cause of failures.
2. **Monitoring**: Understanding system behavior over time (request rates, error rates, slow operations).
3. **Auditing**: Tracking who did what and when (who created an employee, who modified a record).

ASP.NET Core has a built-in logging framework. You do not need a third-party library to get started. The built-in `ILogger<T>` interface, combined with configuration in `appsettings.json`, provides structured logging to the console, debug window, and other providers.

---

## Logging Levels

ASP.NET Core defines six log levels. Each level represents a degree of severity.

| Level | When to Use | Example |
|---|---|---|
| **Trace** | Very detailed diagnostic information, typically only useful during development | Method entry/exit, variable values, loop iterations |
| **Debug** | Internal system events that are useful for debugging but not needed in normal operation | SQL query text, intermediate calculation results, cache hit/miss |
| **Information** | Normal application flow — events that confirm the system is working as expected | "Employee created with ID 42", "User logged in", "Order processed" |
| **Warning** | Unexpected situations that are handled but deserve attention | "Retry attempt 2 of 3", "Deprecated API version called", "Rate limit approaching" |
| **Error** | Failures that need attention but do not crash the application | "Database connection failed", "Payment processing failed", "External API returned 500" |
| **Critical** | Fatal errors that require immediate attention — the application or a critical subsystem cannot continue | "Application crashed", "Data corruption detected", "Cannot bind to port" |

### Choosing the Right Level

The level determines when the log entry appears. If you configure logging to show `Warning` and above, `Information` and `Debug` entries are hidden. Setting levels correctly ensures that production logs contain actionable information without drowning in noise.

A useful guideline:

- **Information**: The happy path. Things that should happen during normal operation.
- **Warning**: Something unexpected happened, but the system handled it.
- **Error**: Something went wrong and a user or system is affected.
- **Critical**: The system is broken and needs immediate intervention.

`Trace` and `Debug` are for development. They should almost never appear in production logs.

---

## Structured Logging

Structured logging uses message templates with named placeholders instead of string concatenation.

**Bad — string concatenation:**

```csharp
_logger.LogInformation("Employee " + employee.Name + " created with ID " + employee.Id);
```

**Bad — string interpolation:**

```csharp
_logger.LogInformation($"Employee {employee.Name} created with ID {employee.Id}");
```

**Good — structured logging:**

```csharp
_logger.LogInformation("Employee {EmployeeName} created with ID {EmployeeId}", employee.Name, employee.Id);
```

### Why Structured Logging Is Better

With string concatenation and interpolation, the log message is just a string. The log provider cannot extract meaning from it. It sees:

```
Employee Alice created with ID 42
```

With structured logging, the message template and the values are separate. The log provider sees:

```
Template: "Employee {EmployeeName} created with ID {EmployeeId}"
Values: { EmployeeName: "Alice", EmployeeId: 42 }
```

This enables:

- **Indexing**: Log aggregation tools (Serilog + Seq, Application Insights, Elasticsearch) can index `EmployeeName` and `EmployeeId` as searchable fields.
- **Filtering**: You can search for all logs where `EmployeeId = 42` without parsing text.
- **Correlation**: Structured properties can link related log entries across services.

The rule is simple: never concatenate or interpolate values into log messages. Always use placeholders and pass the values as arguments.

---

## Using ILogger\<T\>

ASP.NET Core provides logging through dependency injection. The `ILogger<T>` interface is generic — the type parameter `T` is the class that owns the logger. This allows the logging framework to tag every log entry with the source class name.

### Injecting the Logger

```csharp
using EmployeeManagement.Application.Dtos;
using EmployeeManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IEmployeeService employeeService, ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
    {
        _logger.LogInformation("Retrieving all employees");

        var employees = await _employeeService.GetAllEmployeesAsync();

        _logger.LogInformation("Retrieved {EmployeeCount} employees", employees.Count());

        return Ok(employees);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        _logger.LogInformation("Retrieving employee with ID {EmployeeId}", id);

        var employee = await _employeeService.GetEmployeeByIdAsync(id);

        if (employee == null)
        {
            _logger.LogWarning("Employee with ID {EmployeeId} not found", id);
            return NotFound();
        }

        return Ok(employee);
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
    {
        _logger.LogInformation("Creating employee with name {EmployeeName}", dto.Name);

        var created = await _employeeService.CreateEmployeeAsync(dto);

        _logger.LogInformation("Employee created with ID {EmployeeId}", created.Id);

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = created.Id },
            value: created);
    }
}
```

### Understanding the Logger Calls

| Call | Level | What It Communicates |
|---|---|---|
| `LogInformation("Retrieving all employees")` | Information | A normal operation started |
| `LogInformation("Retrieved {EmployeeCount} employees", count)` | Information | The operation completed with a result |
| `LogWarning("Employee with ID {EmployeeId} not found", id)` | Warning | An expected-but-noteworthy event occurred |
| `LogInformation("Creating employee with name {EmployeeName}", name)` | Information | A create operation started |
| `LogInformation("Employee created with ID {EmployeeId}", id)` | Information | A create operation completed |

Notice that each log entry includes structured properties (`{EmployeeId}`, `{EmployeeCount}`, `{EmployeeName}`). These are indexed by the log provider and can be searched later.

---

## Logging Exceptions

When an exception occurs, log it with `LogError` or `LogCritical` and pass the exception object as the first argument.

```csharp
[HttpPost]
public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
{
    _logger.LogInformation("Creating employee with name {EmployeeName}", dto.Name);

    try
    {
        var created = await _employeeService.CreateEmployeeAsync(dto);

        _logger.LogInformation("Employee created with ID {EmployeeId}", created.Id);

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = created.Id },
            value: created);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to create employee with name {EmployeeName}", dto.Name);
        throw;
    }
}
```

### What `LogError` Captures

When you pass the exception as the first argument, the logger records:

- The exception type (`System.InvalidOperationException`)
- The exception message ("The department with ID 99 does not exist")
- The full stack trace
- Any inner exceptions
- Your custom message and structured properties

This information appears in the log alongside the exception details. It is what you use to diagnose the failure.

### The `throw` After Logging

Notice the `throw;` statement after logging. This re-throws the original exception, preserving the stack trace. The global exception handler (Topic 06) catches it and returns a safe error response to the client. The log retains the full details.

If you use `throw ex;` instead of `throw;`, the stack trace is reset to the current location, and you lose the original call chain. Always use `throw;`.

### When to Log and Swallow

Sometimes you catch an exception, log it, and do not re-throw. This is appropriate when:

- You can provide a fallback (retry with a different strategy, return cached data)
- The failure is non-critical and should not affect the response

This is dangerous when:

- You swallow the exception silently (logging but not re-throwing when you should)
- The caller assumes success because no exception propagated

The default should be: log and re-throw. Only swallow when you have a deliberate reason and the caller does not need to know about the failure.

---

## What NOT to Log

This section is critical. Logging sensitive data is one of the most common security mistakes, and it can lead to data breaches, compliance violations, and loss of user trust.

### Never Log These

| Category | Examples |
|---|---|
| Passwords | User passwords, API keys, shared secrets |
| Access tokens | JWTs, OAuth tokens, session tokens |
| Signing keys | Private keys, certificate keys, HMAC secrets |
| Connection strings | Database connection strings with embedded credentials |
| Financial data | Credit card numbers, bank account numbers |
| Personal identifiers | Social security numbers, national ID numbers, passport numbers |
| Health information | Medical records, diagnoses, treatment details |

### Why

Logs are stored in files, databases, or third-party services. They may be:

- Stored on disk without encryption
- Transmitted over networks to log aggregation services
- Accessed by operations staff who should not see user data
- Backed up to storage with different security controls than your database
- Retained for months or years, long after the data should be deleted

A password in a log file is a password exposed. Even if the log is "internal," it has a larger attack surface than your database.

### What to Log Instead

If you need to log that an authentication event occurred, log the event, not the credential:

**Bad:**
```csharp
_logger.LogInformation("User logged in with password {Password}", password);
```

**Good:**
```csharp
_logger.LogInformation("User {UserId} logged in successfully", userId);
```

**Bad:**
```csharp
_logger.LogInformation("Connecting to database with connection string {ConnectionString}", connectionString);
```

**Good:**
```csharp
_logger.LogInformation("Connecting to database {DatabaseName} on server {ServerName}", dbName, serverName);
```

Log the fact that something happened. Log identifiers and metadata. Never log the sensitive data itself.

---

## Request and Correlation Identifiers

ASP.NET Core generates a `traceId` for every incoming HTTP request. This identifier appears in:

- Every log entry produced during that request
- The Problem Details error response (Topic 06)
- The `traceparent` HTTP header (if W3C tracing is enabled)

The `traceId` lets you correlate all log entries for a single request. When a client reports an error and provides the `traceId`, you can filter your logs and see the complete sequence:

```text
[14:32:01 INF] Request starting POST /api/employees - traceId: 00-abc123-00
[14:32:01 INF] Creating employee with name Alice - traceId: 00-abc123-00
[14:32:01 INF] Executing EmployeeService.CreateEmployeeAsync - traceId: 00-abc123-00
[14:32:02 ERR] Failed to create employee - traceId: 00-abc123-00
    Exception: System.InvalidOperationException: Department not found
[14:32:02 INF] Request finished 500 - traceId: 00-abc123-00
```

Every entry shares the same `traceId`. You can follow the request from start to finish, including the error that caused the 500 response.

You do not need to add the `traceId` to your log calls manually. ASP.NET Core includes it automatically in the log scope for each request.

---

## Development vs Production Logging

The verbosity of logging should differ between environments.

### Development

In Development, you want maximum visibility. Set the default level to `Debug` or even `Trace` to see detailed framework behavior.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

### Production

In Production, you want only actionable information. `Debug` and `Trace` entries create noise and consume storage. Set the default level to `Information` or `Warning`.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### Understanding the Configuration

The `LogLevel` object maps category names to minimum levels. `Default` applies to all categories unless overridden.

| Setting | Effect |
|---|---|
| `"Default": "Information"` | All log entries at Information, Warning, Error, and Critical are captured |
| `"Microsoft.AspNetCore": "Warning"` | ASP.NET Core framework logs only show Warning and above (hides routine request processing logs) |
| `"Microsoft.EntityFrameworkCore": "Warning"` | EF Core logs only show Warning and above (hides every SQL query) |

Without the EF Core override, every database query appears in the log at `Information` level. In Production, this generates enormous volumes of log data for very little value. Set it to `Warning` unless you have a specific reason to log queries.

---

## Application Logging vs Error Responses

These are two separate channels serving two separate audiences.

| Channel | Audience | Purpose | Content |
|---|---|---|---|
| Logs | Developers, operations | Debugging, monitoring, auditing | Full details: exception messages, stack traces, variable values |
| Error responses | API clients | Informing about request failures | Safe summary: status code, title, traceId |

**Do not mix them.** The error response should never contain what is in the logs. The logs should never be simplified to match the error response.

A developer debugging a production issue reads the logs, finds the `traceId`, and sees the full exception with stack trace. The API client receives the `traceId` and a generic "An unexpected error occurred" message. Each audience gets exactly what it needs and nothing it should not have.

---

## Avoiding Excessive Logging

Logging too little is blind. Logging too much is noise. Both make it harder to find the information you need when something goes wrong.

### What Not to Log

- Every method entry and exit (unless at `Trace` level in Development)
- Every variable value
- Every database query result
- Every successful operation that does not represent a meaningful business event

### What to Log

- **Significant business events**: Employee created, order placed, user authenticated
- **Failures and exceptions**: Any error, with full exception details
- **Boundary crossings**: When a request enters the system, when it leaves
- **Unusual conditions**: Retries, fallbacks, degraded mode, deprecated API usage

### The Test

Before adding a log entry, ask: "If I read this in production at 2 AM while investigating an incident, would it help me understand what happened?" If the answer is no, remove it or lower its level to `Debug`.

---

## Common Mistakes

**Mistake 1: Logging sensitive data.**
Passwords, tokens, connection strings, and personal information end up in logs because a developer needed to debug something and took a shortcut. The log file is then committed to source control, uploaded to a log aggregation service, or accessed by someone who should not see the data. Always ask: "Does this log entry contain anything I would not want printed on a billboard?" If yes, remove it.

**Mistake 2: Using string concatenation in log messages.**
`_logger.LogInformation("Employee " + name + " created")` prevents structured logging tools from indexing the `name` property. It also creates unnecessary string allocations on every call. Use placeholders: `_logger.LogInformation("Employee {EmployeeName} created", name)`.

**Mistake 3: Logging too much or too little.**
Logging every method call generates millions of entries that make it impossible to find the one that matters. Logging only `Critical` errors means you have no visibility into normal operation or warning signs. Find the balance: log meaningful business events at `Information`, log failures at `Error`, and keep `Debug` for development.

**Mistake 4: Not logging exceptions.**
Catching an exception and returning a generic error response without logging means you have no way to diagnose the failure. The client sees "An unexpected error occurred." You see nothing. The exception exists only in the void. Always log with `_logger.LogError(ex, ...)` before returning an error response.

**Mistake 5: Not using appropriate log levels.**
Everything at `Error` level means nothing is at `Error` level — real errors drown in a sea of false alarms. Everything at `Information` means you miss important warnings. Use the levels as intended: `Information` for normal flow, `Warning` for handled anomalies, `Error` for failures, `Critical` for system-breaking events.

---

## Knowledge Check

**Question 1**: A developer writes `_logger.LogInformation($"Employee {id} deleted")`. What is wrong with this, and how should it be rewritten?

**Question 2**: You configure `"Microsoft.EntityFrameworkCore": "Warning"` in Production. What log entries are hidden, and why is this the right choice?

**Question 3**: A client reports a 500 error. The error response contains a `traceId`. Explain how you use this to find the problem, and what information you expect to find in the logs versus what the client sees.

<details>
<summary>Answers</summary>

**Answer 1**: The string interpolation (`$"Employee {id} deleted"`) evaluates the string before passing it to the logger. The logger receives a fully-formed string with no structured properties. It cannot index `id` for searching or filtering. The fix is to use a message template with a placeholder: `_logger.LogInformation("Employee {EmployeeId} deleted", id)`. The logger now receives the template and the value separately, enabling structured logging.

**Answer 2**: This setting hides all EF Core log entries below `Warning` level. In practice, this means every SQL query that EF Core executes (logged at `Information` by default) is suppressed. This is the right choice because: (a) every API call typically generates multiple SQL queries, creating enormous log volume; (b) query text is rarely useful in production; (c) if you need to diagnose a database issue, you can temporarily lower the level or use EF Core's built-in query logging during development.

**Answer 3**: You take the `traceId` and search your application logs for it. The logs show every log entry produced during that specific request, in chronological order. You see the controller entry, the service call, any database operations (if logged), and the exception with its full stack trace, message, and inner exceptions. This tells you exactly what failed and why. The client, by contrast, sees only a generic error response with the `traceId`, a 500 status code, and the title "An unexpected error occurred." The client does not see the stack trace, the exception message, or any internal details. The `traceId` is the bridge between the safe response and the detailed logs.

</details>
