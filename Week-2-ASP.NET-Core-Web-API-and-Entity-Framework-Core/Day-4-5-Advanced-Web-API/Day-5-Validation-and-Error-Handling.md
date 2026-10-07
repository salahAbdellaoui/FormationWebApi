# Day 5 — Validation and Error Handling

**Duration:** approximately 3 hours

## 1. Validate request data

Use data annotations for the existing create and update DTOs:

```csharp
using System.ComponentModel.DataAnnotations;

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

With `[ApiController]`, invalid body data produces a framework-generated `400 Bad Request` response and the action is not entered. The same `[Range]` attributes on `EmployeeQueryParameters` reject page numbers below 1 and page sizes outside 1-100.

This is the practical pipeline:

```text
Request body/query string
    ↓
Model binding
    ↓
Data-annotation validation
    ↓
400 response, or controller action
```

Do not add FluentValidation or a custom validation framework for this module.

## 2. Expected API errors

Keep expected failures in the controller:

```csharp
[HttpGet("{id:int}")]
public async Task<ActionResult<EmployeeDto>> GetById(int id)
{
    var employee = await _context.Employees
        .AsNoTracking()
        .Where(e => e.Id == id)
        .Select(e => new EmployeeDto
        {
            Id = e.Id,
            Name = e.Name,
            Email = e.Email,
            Salary = e.Salary,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department == null ? null : e.Department.Name
        })
        .SingleOrDefaultAsync();

    return employee is null ? NotFound() : Ok(employee);
}
```

Use:

| Situation | Status |
|---|---:|
| Invalid body or query values | 400 |
| Employee ID does not exist | 404 |
| Successful read | 200 |

Do not invent a `409 Conflict` rule. Add one only if a real business conflict exists in the project.

## 3. Centralized unexpected errors

Register Problem Details and the built-in exception handler in `Program.cs`:

```csharp
builder.Services.AddProblemDetails();
```

Configure the pipeline after `builder.Build()`:

```csharp
var app = builder.Build();

app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
```

Keep the existing Swagger registration and development setup. The exception handler prevents exception details from becoming an accidental public response. Log the exception through the application's normal ASP.NET Core logging pipeline; do not catch and ignore it in each controller.

Problem Details is a standard, machine-readable description of an HTTP error. The exact fields can vary by environment and framework configuration, so inspect the response produced by this application rather than promising fields that are not configured.

Expected client errors should be returned intentionally; unexpected exceptions should flow to the centralized handler:

```text
Missing employee → controller → 404
Invalid request  → ApiController validation → 400
Unexpected error → exception handler → 500 Problem Details
```

Do not return stack traces or database connection details to API clients.

## 4. Integration test checklist

Use Swagger or the existing HTTP client to verify:

1. `GET /api/employees?search=ali` returns a page response.
2. `GET /api/employees?pageNumber=0` returns `400`.
3. `GET /api/employees?pageSize=101` returns `400`.
4. `GET /api/employees/999999` returns `404` when that ID is absent.
5. A controlled development-only exception, if already available in the project, is converted by the exception handler rather than exposing a stack trace.
6. A valid POST with the required fields still returns the existing success status.

Do not add a production endpoint whose only purpose is to throw an exception.

## Senior developer notes

- Validate at the API boundary.
- Keep error responses consistent across actions.
- Do not use broad catches as a substitute for centralized handling.
- Apply pagination before executing the query.
- Avoid executing the database query multiple times unless both `CountAsync` and the page are required.
- Keep query-building logic readable and explicit.

## Day 5 final challenge

Complete the Employee API so that one caller can use:

```http
GET /api/employees?search=ali&departmentId=2&minSalary=3000&sortBy=salary&sortOrder=desc&pageNumber=1&pageSize=10
```

The endpoint must validate input, return the paginated response shape, return `404` for a missing employee, and use centralized handling for unexpected exceptions. Finish by building the project and testing each checklist item in Swagger.
