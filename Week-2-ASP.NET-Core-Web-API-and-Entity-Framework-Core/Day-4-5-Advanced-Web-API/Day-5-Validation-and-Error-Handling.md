# Day 5 — Validation and Error Handling

**Duration:** approximately 3 hours

## The Reliability Problem

Our endpoint works. Filtering, searching, sorting, and pagination all return correct results when the client sends well-formed requests.

What happens when clients send bad data? What happens when something goes wrong on the server?

Today we answer both questions. By the end of this session, your API will:

- Reject invalid input with clear 400 responses
- Return 404 when a resource does not exist
- Handle unexpected errors centrally without exposing internal details

---

## What You'll Build Today

| Section | What | Time |
|---------|------|------|
| 1. Validation with Data Annotations | Guard DTOs and query parameters against bad input | ~35 min |
| 2. Resource Not Found (404) | Confirm expected errors return the right status codes | ~25 min |
| 3. Centralized Error Handling | Catch unexpected exceptions with Problem Details | ~35 min |
| 4. Putting It All Together | Full request lifecycle, end-to-end test checklist | ~20 min |

---

## Section 1 — Validation with Data Annotations (~35 min)

### Concept

Validation is the process of checking whether incoming data meets your API's rules before you act on it. An API without validation accepts anything: empty names, negative salaries, page numbers of zero. The database might reject some of these, but with confusing error messages — and not all of them.

Data Annotations are the built-in .NET validation mechanism. You place attributes on your DTO properties, and the framework checks them automatically.

Your controllers already have `[ApiController]`. This attribute enables a critical behavior: **automatic model state validation**. When the model state is invalid, ASP.NET Core returns a 400 response *before your action method ever runs*.

### Why This Matters

Consider what happens today without validation:

```http
POST /api/employees
Content-Type: application/json

{
  "name": "",
  "email": "not-an-email",
  "salary": -5000,
  "departmentId": 0
}
```

This request succeeds today. An employee with an empty name, invalid email, and negative salary gets created. Validation prevents this.

### Build — Validate DTOs

Open `Dtos/CreateEmployeeDto.cs` and add validation attributes:

```csharp
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Dtos;

public class CreateEmployeeDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Range(0, 1_000_000)]
    public decimal Salary { get; set; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }
}
```

Open `Dtos/UpdateEmployeeDto.cs` and apply the same rules:

```csharp
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Dtos;

public class UpdateEmployeeDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Range(0, 1_000_000)]
    public decimal Salary { get; set; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }
}
```

Each attribute enforces one rule:

| Attribute | Rule |
|-----------|------|
| `[Required]` | Property must have a value (not null, not empty string) |
| `[StringLength(100)]` | String length must be 100 characters or fewer |
| `[EmailAddress]` | Value must be a valid email format |
| `[Range(0, 1_000_000)]` | Numeric value must be between 0 and 1,000,000 |
| `[Range(1, int.MaxValue)]` | Integer must be at least 1 |

### Build — Validate Query Parameters

Open your `EmployeeQueryParameters` class (created on Day 4) and add validation to the pagination properties:

```csharp
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Dtos;

public class EmployeeQueryParameters
{
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public decimal? MinSalary { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
```

Notice that `Search`, `DepartmentId`, `MinSalary`, `SortBy`, and `SortOrder` are optional — they are nullable types with no `[Required]` attribute. That is correct: a client does not have to filter or sort. But `PageNumber` and `PageSize` must always be valid, even when the client relies on the defaults.

### Test in Swagger

Build and run the application. Open Swagger.

**Test 1 — Missing name:**

Send a POST to `/api/employees` with this body:

```json
{
  "email": "test@example.com",
  "salary": 5000,
  "departmentId": 1
}
```

**Expected result:** `400 Bad Request`

**Test 2 — Invalid email:**

```json
{
  "name": "Alice",
  "email": "not-an-email",
  "salary": 5000,
  "departmentId": 1
}
```

**Expected result:** `400 Bad Request`

**Test 3 — Negative salary:**

```json
{
  "name": "Alice",
  "email": "alice@example.com",
  "salary": -5000,
  "departmentId": 1
}
```

**Expected result:** `400 Bad Request`

**Test 4 — Invalid page number:**

```
GET /api/employees?pageNumber=0
```

**Expected result:** `400 Bad Request`

**Test 5 — Page size too large:**

```
GET /api/employees?pageSize=101
```

**Expected result:** `400 Bad Request`

**Observe the response body.** For each 400 response, Swagger shows a response body. It will look something like this:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-abc123...",
  "errors": {
    "Name": ["The Name field is required."],
    "Email": ["The Email field is not a valid e-mail address."],
    "Salary": ["The field Salary must be between 0 and 1000000."]
  }
}
```

The exact fields depend on which properties failed validation. The `errors` object groups messages by property name. This format is part of the Problem Details standard, which you will learn more about in Section 3.

### Teach the Pipeline

This is what happens when a request arrives at your API:

```
Request body or query string
    |
    v
Model binding (JSON -> DTO, query string -> parameters object)
    |
    v
Data annotation validation (checks every [Required], [Range], etc.)
    |
    +-- Invalid model state --> 400 response (action does NOT execute)
    |
    +-- Valid model state --> Controller action executes
```

The critical point: **when validation fails, the controller action does not run at all.** The framework intercepts the request and returns 400 immediately. Your code in the action method never executes. This is why you do not need to write `if (!ModelState.IsValid)` checks in every action when you have `[ApiController]`.

### Common Mistakes

**Forgetting `using System.ComponentModel.DataAnnotations;`** — The attributes live in this namespace. Without the using directive, the attributes will not resolve and you will get compiler errors.

**Adding validation to the model class instead of the DTO** — Models represent the database entity. Validation belongs on the DTO, which represents the API contract. A database column might allow null for internal reasons, but the API should reject missing values from clients.

**Using `[Required]` on value types** — `int` and `decimal` are value types. They always have a value (0 by default). `[Required]` has no effect on a non-nullable `int`. Use `[Range]` instead to enforce minimum values.

**Not testing in Swagger** — Attributes look correct on paper but may not behave as expected. Always test with actual HTTP requests.

### Senior Developer Note

Data Annotations cover the most common validation scenarios. For complex business rules — for example, "email must be unique" or "department must exist" — you will need validation logic inside the controller or service layer. Data Annotations validate *shape*, not *business rules*.

The `[ApiController]` automatic 400 behavior is specific to controller-based APIs. Minimal APIs handle validation differently.

### Knowledge Check

1. What happens when a client sends a POST request with an empty `Name` field to your API? Which component generates the response — your controller code or the framework?

2. Why does `[Required]` not work on the `Salary` property of type `decimal`? What attribute should you use instead?

3. Your `EmployeeQueryParameters` has `[Range(1, 100)]` on `PageSize`. A client sends `GET /api/employees?pageSize=0`. What HTTP status code is returned, and does your controller action execute?

<details>
<summary>Answers</summary>

1. The framework returns 400 automatically. `[ApiController]` detects the invalid model state before the action runs. Your controller code does not execute.

2. `decimal` is a value type — it always has a value (0 by default), so `[Required]` is always satisfied. Use `[Range(0, 1_000_000)]` to enforce that the salary is within an acceptable range.

3. `400 Bad Request` is returned. The controller action does not execute. `[ApiController]` intercepts the invalid model state.

</details>

---

## Section 2 — Resource Not Found (404) (~25 min)

### Concept

APIs produce two categories of errors:

**Expected errors** — The client sent a bad request, or asked for something that does not exist. These are normal outcomes. 400 (bad input) and 404 (not found) are expected errors. Your controller handles them explicitly.

**Unexpected errors** — The database is down, a null reference occurs, an external service fails. These are bugs or infrastructure problems. You handle them centrally in Section 3.

A 404 response is not a failure of your API. It is the correct answer to "give me employee #999999" when no such employee exists. Returning 200 with a null body would be wrong — it tells the client "everything is fine" when it is not.

### Build

Your `GetById` endpoint already returns 404 when the employee does not exist. Review the code:

```csharp
[HttpGet("{id:int}")]
public async Task<ActionResult<EmployeeDto>> GetById(int id)
{
    var employee = await _context.Employees
        .Include(e => e.Department)
        .FirstOrDefaultAsync(e => e.Id == id);

    if (employee == null)
        return NotFound();

    var dto = new EmployeeDto
    {
        Id = employee.Id,
        Name = employee.Name,
        Email = employee.Email,
        Salary = employee.Salary,
        DepartmentId = employee.DepartmentId,
        DepartmentName = employee.Department != null ? employee.Department.Name : null
    };

    return Ok(dto);
}
```

The pattern is:

```
Database lookup
    |
    v
Entity found? --- No --> return NotFound() (404)
    |
    Yes
    |
    v
Project to DTO and return Ok() (200)
```

Your `Update` and `Delete` endpoints follow the same pattern:

```csharp
[HttpPut("{id:int}")]
public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
{
    var employee = await _context.Employees.FindAsync(id);
    if (employee == null)
        return NotFound();

    // ... update and save
    return NoContent();
}

[HttpDelete("{id:int}")]
public async Task<IActionResult> Delete(int id)
{
    var employee = await _context.Employees.FindAsync(id);
    if (employee == null)
        return NotFound();

    // ... delete and save
    return NoContent();
}
```

Every endpoint that looks up a resource by ID must handle the "not found" case. This is not optional.

### Test in Swagger

**Test 1 — GET non-existent employee:**

```
GET /api/employees/999999
```

**Expected result:** `404 Not Found`

**Test 2 — UPDATE non-existent employee:**

```
PUT /api/employees/999999
Content-Type: application/json

{
  "name": "Ghost",
  "email": "ghost@example.com",
  "salary": 5000,
  "departmentId": 1
}
```

**Expected result:** `404 Not Found`

**Test 3 — DELETE non-existent employee:**

```
DELETE /api/employees/999999
```

**Expected result:** `404 Not Found`

**Test 4 — GET existing employee:**

Pick an ID that exists in your database (for example, ID 1):

```
GET /api/employees/1
```

**Expected result:** `200 OK` with the employee data

### Teach the Decision

Why 404 and not something else?

| What happened | Correct status | Wrong alternative | Why it is wrong |
|---------------|---------------|-------------------|-----------------|
| Employee ID does not exist | 404 | 200 with null body | 200 means success; there is no data |
| Employee ID does not exist | 404 | 500 | 500 means server error; this is not a bug |
| Employee ID does not exist | 404 | 400 | 400 means the request format was invalid; the ID format is fine |

### Common Mistakes

**Returning 200 with a null body** — This is the most common mistake. The client receives `200 OK` with a `null` body and must guess whether the resource does not exist or the network dropped the response. Always return 404.

**Returning 500 instead of 404** — If you forget the null check and try to access properties on a null object, the application throws a `NullReferenceException`. The framework catches it and returns 500. This is wrong: a missing resource is not a server error.

**Throwing an exception for not-found** — Some developers throw a `NotFoundException` when a resource is missing. This is unnecessary complexity for a simple check. Use `return NotFound()` directly.

### Senior Developer Note

For endpoints that modify resources, the check-then-act pattern has a subtle race condition: between the lookup and the update, another request might delete the entity. EF Core handles this through concurrency tokens if you configure them. For this training, the simple `FindAsync` + null check is correct.

### Knowledge Check

1. A client calls `GET /api/employees/42` and no employee with ID 42 exists. What should the API return? Why is returning `200 OK` with a null body incorrect?

2. Your `Update` endpoint calls `_context.Employees.FindAsync(id)` and gets null. What line of code should execute next?

3. Is a 404 response a "failure" of your API? Explain.

<details>
<summary>Answers</summary>

1. `404 Not Found`. Returning `200 OK` with null tells the client "the request succeeded and here is nothing." The client cannot distinguish between "the resource does not exist" and "the response body was lost in transit." 404 is explicit.

2. `return NotFound();` — check for null immediately and return 404 before accessing any properties.

3. No. A 404 is the correct, expected response when a resource does not exist. It is the API doing its job — telling the client the truth about the state of the data.

</details>

---

## Section 3 — Centralized Error Handling (~35 min)

### Concept

You now handle two categories of errors explicitly in your controllers:

- Invalid input → 400 (via `[ApiController]` validation)
- Missing resource → 404 (via `return NotFound()`)

But what about unexpected errors? A database connection fails. A null reference slips through. An external service times out.

You could wrap every action in a `try/catch` block. That approach does not scale. It duplicates code, and developers forget to add it.

ASP.NET Core provides centralized error handling. When an unhandled exception reaches the middleware pipeline, the exception handler catches it and produces a consistent error response. You configure it once in `Program.cs`.

**The most important rule: never expose stack traces, connection strings, or internal implementation details to API clients.** The centralized handler ensures this.

### Build

Open `Program.cs`. You need two additions:

**1. Register Problem Details services** — add `builder.Services.AddProblemDetails()` before `builder.Build()`:

```csharp
using EmployeeManagement.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**2. Add the exception handler middleware** — add `app.UseExceptionHandler()` after `builder.Build()`. Place it *before* `UseHttpsRedirection`, `UseAuthorization`, and `MapControllers`. Middleware runs in order, and the exception handler must be early in the pipeline to catch errors from everything downstream.

Here is the updated `Program.cs` with both changes applied. The two new lines are marked with comments to show where they go — remove the comments in your actual file:

```csharp
using EmployeeManagement.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();        // <-- NEW

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();                   // <-- NEW

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### What Changed

| Line | Purpose |
|------|---------|
| `builder.Services.AddProblemDetails()` | Registers the Problem Details services in the DI container |
| `app.UseExceptionHandler()` | Activates the middleware that catches unhandled exceptions and produces Problem Details responses |

### Teach the Error Flow

With these changes, your API now handles three categories of errors:

```
Client request
    |
    v
[ApiController] validation detects invalid data
    --> 400 Bad Request with Problem Details
    |
    v
Controller action executes
    |
    v
Resource not found?
    --> return NotFound() --> 404
    |
    v
Action completes successfully
    --> return Ok() / CreatedAtAction() / NoContent()
    |
    v
OR an unhandled exception occurs (database down, null reference, etc.)
    --> UseExceptionHandler catches it --> 500 with Problem Details
```

Expected errors are handled in the controller. Unexpected errors are handled by the middleware. You never need to write `try/catch` in every action method.

### About the Problem Details Format

Problem Details is a standard structure for HTTP error responses defined by RFC 7807. It gives clients a consistent, machine-readable error format regardless of what went wrong.

The exact response depends on the environment and configuration. In development, the response may include additional diagnostic information. In production, it returns a minimal response with no internal details.

**Do not memorize specific JSON fields.** Instead, run the application and inspect the actual responses. The framework generates them, and the output depends on your configuration.

### Important: Do Not Create a Throw-Only Endpoint

Some tutorials add a test endpoint like `GET /api/test-error` that throws an exception, just to see the error handler in action. Do not add this to your project. It serves no purpose beyond testing, and it is easy to forget to remove it before deployment.

If your project already has a development error page (for example, from the default ASP.NET Core template), the exception handler replaces its behavior for API requests. The response format may differ from what the development error page showed — this is expected and correct.

### Common Mistakes

**Catching all exceptions in every controller action** — This defeats the purpose of centralized handling. If you write `try/catch` in every action, you have the same code in twenty places. Centralized handling is one configuration in `Program.cs`.

**Exposing stack traces to clients** — Stack traces reveal your file paths, class names, and sometimes variable values. This is a security risk. The exception handler does not expose them. If you catch exceptions manually and return the exception message, you might leak sensitive information.

**Returning database error details** — A SQL timeout or constraint violation message can contain table names, column names, and connection details. Never pass raw exception messages to the client.

**Placing `UseExceptionHandler()` after `MapControllers()`** — Middleware order matters. The exception handler must come *before* the endpoints it protects. If it comes after, it cannot catch exceptions from those endpoints.

### Senior Developer Note

`AddProblemDetails()` and `UseExceptionHandler()` are built into .NET 8. They replace the need for custom exception middleware in most projects. For advanced scenarios — like mapping specific exception types to specific status codes — you can configure options on `AddProblemDetails()`, but the defaults are correct for this training.

Logging happens automatically. ASP.NET Core's logging pipeline records the exception. You do not need to add explicit logging in the exception handler. If you want structured logging with additional context, configure your logging provider — but the exception is already logged by default.

### Knowledge Check

1. Where should `app.UseExceptionHandler()` be placed in the middleware pipeline — before or after `app.MapControllers()`? Why?

2. An unhandled `NullReferenceException` occurs inside your `GetById` action. What status code does the client receive? What would happen without `UseExceptionHandler()`?

3. Why is it wrong to write `return StatusCode(500, ex.Message)` in a catch block inside a controller action?

<details>
<summary>Answers</summary>

1. Before `app.MapControllers()`. Middleware runs in the order it is added. The exception handler must be upstream of the endpoints it protects so it can catch exceptions thrown by those endpoints.

2. The client receives 500 with a Problem Details response. Without `UseExceptionHandler()`, the response would depend on the environment — in development, a detailed error page; in production, a generic 500 with no structured body. The exception handler ensures a consistent Problem Details response in both environments.

3. `ex.Message` can contain sensitive information — SQL errors, file paths, internal class names. The centralized exception handler produces a safe response that does not leak internal details. Returning `ex.Message` directly bypasses that protection.

</details>

---

## Section 4 — Putting It All Together (~20 min)

### The Complete Request Lifecycle

Here is the full picture of how your API handles requests after today's changes:

```
Client request arrives
    |
    v
Model binding
    (JSON body --> DTO, query string --> parameters object)
    |
    v
[ApiController] data annotation validation
    |
    +-- Invalid --> 400 Bad Request (action does NOT execute)
    |
    +-- Valid
        |
        v
    Controller action executes
        |
        v
    Business logic (database lookup, query, etc.)
        |
        +-- Resource not found --> return NotFound() --> 404
        |
        +-- Success --> return Ok() / CreatedAtAction() / NoContent()
        |
        +-- Unexpected exception --> UseExceptionHandler --> 500 Problem Details
```

Every request follows this path. Your job is to make sure each branch produces the correct response.

### End-to-End Test Checklist

Run the application and test each scenario in Swagger. Record the actual status code and response body for each one.

| # | Request | Expected Status |
|---|---------|----------------|
| 1 | `GET /api/employees?search=ali` | 200 — paginated response with matching items |
| 2 | `GET /api/employees?pageNumber=0` | 400 — PageNumber must be >= 1 |
| 3 | `GET /api/employees?pageSize=101` | 400 — PageSize must be <= 100 |
| 4 | `GET /api/employees/999999` | 404 — employee does not exist |
| 5 | POST with valid data (all fields correct) | 201 — employee created |
| 6 | POST with missing Name | 400 — Name is required |
| 7 | POST with invalid email format | 400 — Email is not valid |
| 8 | POST with negative salary | 400 — Salary must be >= 0 |
| 9 | PUT with valid data for existing employee | 204 — updated |
| 10 | PUT for non-existent employee | 404 |
| 11 | DELETE for non-existent employee | 404 |
| 12 | `GET /api/employees?search=ali&departmentId=2&minSalary=3000&sortBy=salary&sortOrder=desc&pageNumber=1&pageSize=10` | 200 — fully filtered, sorted, paginated response |

If all twelve tests pass, your API correctly handles validation, expected errors, and is ready for centralized error handling of unexpected failures.

### Combined Code Reference

For reference, here is how the validated `CreateEmployeeDto` and `EmployeeQueryParameters` look together:

```csharp
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Dtos;

public class CreateEmployeeDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Range(0, 1_000_000)]
    public decimal Salary { get; set; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }
}
```

```csharp
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Dtos;

public class EmployeeQueryParameters
{
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public decimal? MinSalary { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
```

And the relevant part of `Program.cs`:

```csharp
builder.Services.AddProblemDetails();

var app = builder.Build();

// ...

app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
```

---

## Summary

Today you added three layers of reliability to the Employee API:

**1. Validation with Data Annotations** — `[Required]`, `[StringLength]`, `[EmailAddress]`, and `[Range]` on DTOs and query parameters. Combined with `[ApiController]`, invalid requests receive an automatic 400 response before the controller action runs.

**2. Resource Not Found (404)** — Every endpoint that looks up a resource by ID checks for null and returns `NotFound()`. This is an expected outcome, not an error.

**3. Centralized Error Handling** — `AddProblemDetails()` and `UseExceptionHandler()` catch unexpected exceptions and return consistent, safe error responses. No stack traces, no connection strings, no `try/catch` in every action.

### What the Employee API Can Now Do

After four days of work, the Employee API supports:

- Full CRUD operations (Create, Read, Update, Delete)
- Filtering by department and salary
- Free-text search across name and email
- Sorting by name, email, or salary
- Pagination with page number and page size
- Input validation on all DTOs and query parameters
- Proper 404 responses for missing resources
- Centralized handling of unexpected errors

### Preview: What Comes Next

The Final Challenge combines everything from this week. You will build a complete, reliable API from scratch — entities, EF Core, DTOs, querying, validation, and error handling. The individual pieces are all familiar. The challenge is assembling them into a working whole.

Build the project. Run all twelve tests in the checklist. If they pass, you are ready.
