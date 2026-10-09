# 06 — API Layer

## What Is the API Layer?

The API Layer is the HTTP entry point for the application. It handles routing, model binding, authentication, authorization, and HTTP responses.

The API Layer calls Application services. It does NOT implement business rules. It does NOT write database queries. It does NOT map entities to DTOs (that is the service layer's job).

The only responsibility of the API Layer is to receive an HTTP request, pass it to the Application Layer, and return an HTTP response.

## The Thin Controller

Controllers should be thin. They receive a request, call a service, and return a response. Nothing more.

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

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        return Ok(employees);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
            return NotFound();
        return Ok(employee);
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
    {
        var created = await _employeeService.CreateEmployeeAsync(dto);
        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = created.Id },
            value: created);
    }
}
```

Notice what is missing from this controller:

- No `DbContext` injection.
- No LINQ queries.
- No entity-to-DTO mapping.
- No business validation.
- No repository access.

All of those concerns live in other layers. The controller only knows about `IEmployeeService`, which is an Application Layer interface.

## What Changed Compared to Week 2?

| Aspect | Week 2 (Before) | Week 3 (Clean Architecture) |
|---|---|---|
| What is injected | `AppDbContext` | `IEmployeeService` |
| Where queries live | In the controller | In the repository |
| EF Core in controller | Yes | No |
| Business rules in controller | Yes | No |
| Entity-to-DTO mapping | Done in controller | Done in service |
| Testability | Hard (needs database) | Easy (mock the service) |
| Coupling | High (controller knows EF Core) | Low (controller knows only interface) |

The controller went from a fat class with database logic, validation, and mapping to a thin class that only handles HTTP concerns.

## Program.cs

The `Program.cs` file is the composition root. It references Infrastructure (which provides the `AddInfrastructure` extension method). This is where all layers are wired together.

```csharp
using EmployeeManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

The key line is `builder.Services.AddInfrastructure(builder.Configuration)`. That single call registers the `DbContext`, the repository, and the service. The API project does not need to know about each individual class. It just calls the extension method.

The API project is the only project that references Infrastructure directly. This is correct. The composition root is the one place where all dependencies meet.

## Request Flow

Here is what happens when a client sends a POST request to create a new employee:

```
    HTTP POST /api/employees
    Body: { "name": "Alice", "email": "alice@example.com", "departmentId": 2 }
              |
              v
    [1] ASP.NET Core receives the HTTP request
              |
              v
    [2] Router matches POST /api/employees to EmployeesController.Create
              |
              v
    [3] Model binding creates CreateEmployeeDto from the request body
              |
              v
    [4] Controller calls _employeeService.CreateEmployeeAsync(dto)
              |
              v
    [5] EmployeeService creates an Employee entity,
        calls _repository.AddAsync(employee)
              |
              v
    [6] EmployeeRepository uses AppDbContext to INSERT the row into SQL Server
              |
              v
    [7] Return path reverses: Repository -> Service -> Controller
              |
              v
    [8] Controller returns CreatedAtAction with HTTP 201 status
              |
              v
    Response: 201 Created
    Location: /api/employees/5
    Body: { "id": 5, "name": "Alice", "email": "alice@example.com", ... }
```

Each arrow crosses a layer boundary. The request moves inward (from HTTP to database). The response moves outward (from database to HTTP). Every layer only talks to the layer directly inside it.

## What the API Should NOT Do

The following concerns do NOT belong in the API Layer:

- **Direct DbContext access.** Controllers should never inject or use `AppDbContext`. All data access goes through the service and repository.
- **Business rules.** Validation like "an employee name must not exceed 100 characters" belongs in the Domain or Application Layer, not in a controller action.
- **Complex entity-to-DTO mapping.** A one-liner like `new EmployeeDto { Name = e.Name }` is fine for simple cases, but complex mapping logic belongs in the service layer.
- **Database queries.** No `await _context.Employees.ToListAsync()` inside a controller. Queries belong in repositories.
- **Cross-cutting concerns handled by middleware.** Logging, exception handling, and authentication should use middleware or filters, not be scattered across controller methods.

## Common Mistakes

**Mistake 1: Injecting DbContext into the controller.**
This defeats the purpose of Clean Architecture. The controller becomes tightly coupled to EF Core and impossible to test without a database. Always inject the service interface instead.

**Mistake 2: Putting business logic in the controller.**
If a controller action contains `if` statements that enforce business rules (like checking salary ranges or promotion eligibility), those rules belong in the Domain or Application Layer. Controllers should only check for null and return HTTP status codes.

**Mistake 3: Returning Domain entities directly from the controller.**
Returning an `Employee` entity exposes your internal domain model to the outside world. Changes to the entity (adding a field, renaming a property) become breaking API changes. Always return DTOs.

**Mistake 4: Handling exceptions with try-catch in every action.**
Use global exception handling middleware instead. Repeating try-catch blocks in every controller action is verbose and inconsistent. A single middleware can catch all unhandled exceptions and return a proper error response.

## Knowledge Check

1. The `EmployeesController` injects `IEmployeeService` instead of `AppDbContext`. What are two benefits of this change?

2. In the request flow diagram, at which step does the database INSERT actually happen? Which layer is responsible for it?

3. Why is `Program.cs` considered the "composition root"? What would happen if you tried to wire up dependencies inside the Application Layer instead?
