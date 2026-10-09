# 02 — Separation of Concerns

**Duration**: 20 minutes

---

## What Is Separation of Concerns?

Separation of Concerns (SoC) is the principle that **each part of a program should handle one responsibility and one responsibility only**. When concerns are mixed, changing one thing risks breaking another. When concerns are separated, each piece can change independently.

Think of a restaurant kitchen. Three roles exist:

- **Shopping** — someone buys the ingredients.
- **Cooking** — someone prepares the food.
- **Serving** — someone delivers the food to the customer.

One person could do all three. But as the restaurant grows, this breaks down. The cook is on the phone with the supplier while the customer waits. The server tries to cook and burns the food. Each role has different skills, different tools, and different failure modes.

Separating them means each person focuses on one job. The cook does not worry about supplier contracts. The server does not worry about ingredient prices. Change the supplier, and the cook does not notice.

Software works the same way. Each layer has one job.

---

## Concerns in Our Application

| Concern | Responsibility | Example |
|---|---|---|
| **HTTP / API** | Receive requests, validate input format, return HTTP responses | `EmployeesController` handles routing, status codes, JSON serialization |
| **Application / Use Case** | Coordinate the business operation: "create an employee," "calculate salary" | `EmployeeService` calls the repository, applies rules, returns a result |
| **Domain / Business** | Define entities and enforce business rules | `Employee` entity, rules like "salary must be positive" |
| **Persistence / Infrastructure** | Store and retrieve data | `AppDbContext`, `EmployeeRepository`, EF Core configuration |

Each concern answers a different question:

- HTTP: "How do I talk to the outside world?"
- Application: "What steps does this use case require?"
- Domain: "What are the rules of this business?"
- Persistence: "Where and how is data stored?"

---

## A Concrete Example

Consider the "Create Employee" operation. Here is what each concern handles:

**HTTP / API** receives the HTTP request, checks that the JSON is well-formed, and returns a `201 Created` response with the correct `Location` header. It does not decide whether the salary is valid. It does not know how data is stored.

**Application / Use Case** receives a command: "create an employee with these details." It coordinates the steps: validate the data, check business rules, save the employee, return the result. It does not know whether the data is stored in SQL Server, a file, or a remote API.

**Domain / Business** defines the `Employee` entity and its rules. A salary cannot be negative. An email cannot be empty. These rules exist regardless of how the employee was created (web, API, background job, import).

**Persistence / Infrastructure** saves the `Employee` to the database using EF Core. It knows about tables, connections, and transactions. It does not know about HTTP status codes or business rules.

```
HTTP Request
    -> Controller (HTTP concern: validate JSON, return 201)
        -> EmployeeService (Application: coordinate "create employee")
            -> Employee entity (Domain: salary > 0, email not empty)
            -> EmployeeRepository (Infrastructure: save to SQL Server)
    <- HTTP Response
```

---

## Before and After

### Before — Everything in the Controller

```csharp
[HttpPost]
public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
{
    var employee = new Employee
    {
        Name = dto.Name,
        Email = dto.Email,
        Salary = dto.Salary,
        DepartmentId = dto.DepartmentId
    };

    _context.Employees.Add(employee);
    await _context.SaveChangesAsync();

    var response = new EmployeeDto
    {
        Id = employee.Id,
        Name = employee.Name,
        Email = employee.Email,
        Salary = employee.Salary,
        DepartmentId = employee.DepartmentId
    };

    return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
}
```

The controller maps the DTO, creates the entity, saves to the database, maps back to a DTO, and returns a response. Five responsibilities in one method.

### After — Each Concern in Its Place

**Controller** — HTTP only:

```csharp
[HttpPost]
public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
{
    var result = await _employeeService.CreateAsync(dto);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}
```

**Service** — Application coordination:

```csharp
public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
{
    var employee = new Employee
    {
        Name = dto.Name,
        Email = dto.Email,
        Salary = dto.Salary,
        DepartmentId = dto.DepartmentId
    };

    await _repository.AddAsync(employee);

    return new EmployeeDto
    {
        Id = employee.Id,
        Name = employee.Name,
        Email = employee.Email,
        Salary = employee.Salary,
        DepartmentId = employee.DepartmentId
    };
}
```

**Repository** — Persistence only:

```csharp
public async Task AddAsync(Employee employee)
{
    _context.Employees.Add(employee);
    await _context.SaveChangesAsync();
}
```

The controller is 3 lines. The service handles the use case. The repository handles storage. Each file can change without touching the others.

---

## SoC vs. Dependency Rule

These two concepts are related but answer different questions.

**Separation of Concerns** asks: "Who does what?"

- The controller handles HTTP.
- The service handles the use case.
- The repository handles persistence.

**The Dependency Rule** asks: "Who knows about whom?"

- The controller knows about the service.
- The service knows about the repository interface.
- The repository knows about EF Core.
- The domain knows about nothing external.

SoC separates responsibilities into layers. The Dependency Rule controls the direction of references between those layers. Together they ensure that inner layers (domain, application) do not depend on outer layers (HTTP, database).

You can have separation without the dependency rule (each layer exists but they all reference each other). The Dependency Rule is what makes the separation meaningful.

---

## Common Mistakes

1. **Business rules in the controller.** If you write `if (dto.Salary < 0) return BadRequest()` in the controller, that rule only works for HTTP calls. A background job or API client calling the service directly bypasses the rule. Business rules belong in the service or domain layer.

2. **HTTP concerns in the service.** If your service method returns `ActionResult<T>` or knows about status codes, it is doing the controller's job. Services return data or throw exceptions. Controllers translate those into HTTP responses.

3. **EF Core types in the application layer.** If your service references `DbSet<T>`, `DbContext`, or EF Core attributes, the application layer now depends on the database technology. Use plain entities and repository interfaces in the application layer.

4. **Domain entities with persistence logic.** If your `Employee` class has methods like `SaveToDatabase()` or `MapToDbRow()`, it knows about storage. Domain entities should contain only business data and business rules.

---

## Knowledge Check

1. In the "After" example, which layer would change if you replaced SQL Server with MongoDB? Which layers would stay the same?

2. A developer adds this code to the controller: `if (employee.Salary > 100000) return BadRequest("Salary too high");`. Which SoC principle does this violate, and what is the risk?

3. Explain the difference between "Separation of Concerns" and "The Dependency Rule" in one sentence each.
