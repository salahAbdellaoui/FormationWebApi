# 05 — LINQ with EF Core

**Duration:** ~30 min

---

## What You'll Learn

By the end of this topic, you will:

- Write LINQ queries against `DbSet<T>`
- Understand how EF Core translates LINQ to SQL
- Filter, sort, and include related data
- Update the controller to use `DbContext` instead of in-memory data

---

## How LINQ with EF Core Works

You write C# LINQ. EF Core translates it to SQL.

```text
C# LINQ Expression
    |
    v
EF Core Query Provider
    |
    v
SQL Query
    |
    v
SQL Server
    |
    v
Results mapped back to C# objects
```

Example:

```csharp
var employees = await context.Employees
    .Where(e => e.Salary > 3000)
    .ToListAsync();
```

EF Core generates:

```sql
SELECT * FROM Employees WHERE Salary > 3000
```

---

## Query Examples

### Get All Employees

```csharp
var employees = await context.Employees.ToListAsync();
```

Generates:

```sql
SELECT * FROM Employees
```

### Find by ID

```csharp
var employee = await context.Employees.FindAsync(id);
```

Generates:

```sql
SELECT * FROM Employees WHERE Id = @id
```

`FindAsync` is optimized for primary key lookups. It checks the change tracker first before querying the database.

### Filter with Where

```csharp
var employees = await context.Employees
    .Where(e => e.Salary > 3000)
    .ToListAsync();
```

Generates:

```sql
SELECT * FROM Employees WHERE Salary > 3000
```

### Order Results

```csharp
var employees = await context.Employees
    .OrderByDescending(e => e.Salary)
    .ToListAsync();
```

Generates:

```sql
SELECT * FROM Employees ORDER BY Salary DESC
```

### Combine Filter and Order

```csharp
var employees = await context.Employees
    .Where(e => e.Salary > 3000)
    .OrderByDescending(e => e.Salary)
    .ToListAsync();
```

Generates:

```sql
SELECT * FROM Employees WHERE Salary > 3000 ORDER BY Salary DESC
```

### Include Related Data

```csharp
var employees = await context.Employees
    .Include(e => e.Department)
    .ToListAsync();
```

Generates a SQL JOIN:

```sql
SELECT e.*, d.*
FROM Employees e
INNER JOIN Departments d ON e.DepartmentId = d.Id
```

Without `Include()`, the `Department` navigation property is null. `Include()` tells EF Core to load the related entity.

### Get One Employee with Department

```csharp
var employee = await context.Employees
    .Include(e => e.Department)
    .FirstOrDefaultAsync(e => e.Id == id);
```

`FirstOrDefaultAsync` returns the first match or null if not found.

---

## Update the Controller

Replace the service-based controller with one that uses `DbContext` directly.

```csharp
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Dtos;
using EmployeeManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .ToListAsync();

        var dtos = employees.Select(e => new EmployeeDto
        {
            Id = e.Id,
            Name = e.Name,
            Email = e.Email,
            Salary = e.Salary,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department?.Name
        });

        return Ok(dtos);
    }

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
            DepartmentName = employee.Department?.Name
        };

        return Ok(dto);
    }

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

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = response.Id },
            value: response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound();

        employee.Name = dto.Name;
        employee.Email = dto.Email;
        employee.Salary = dto.Salary;
        employee.DepartmentId = dto.DepartmentId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound();

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
```

**What changed from Day 2?**

| Before (Day 2) | After (Day 3) |
|----------------|---------------|
| `IEmployeeService` injected | `AppDbContext` injected |
| Methods are synchronous | Methods are `async` |
| In-memory `List<Employee>` | Database through EF Core |
| Manual ID generation | SQL Server identity handles IDs |
| `_service.GetAll()` | `_context.Employees.ToListAsync()` |
| `_service.Create(e)` | `_context.Employees.Add(e)` + `SaveChangesAsync()` |

---

## Key EF Core Methods

| Method | Purpose |
|--------|---------|
| `ToListAsync()` | Execute query, return all results |
| `FirstOrDefaultAsync(predicate)` | Execute query, return first match or null |
| `FindAsync(id)` | Find by primary key (checks tracker first) |
| `Add(entity)` | Mark entity for insertion |
| `Remove(entity)` | Mark entity for deletion |
| `SaveChangesAsync()` | Send all pending changes to the database |
| `Include(property)` | Load related data (eager loading) |

---

## The SaveChanges Pattern

```text
Add / Update / Remove entities
    |
    v
EF Core tracks changes in memory
    |
    v
SaveChangesAsync()
    |
    v
EF Core generates INSERT / UPDATE / DELETE SQL
    |
    v
SQL Server executes
```

Nothing reaches the database until you call `SaveChangesAsync()`.

---

## Common Mistakes

**Mistake 1: Forgetting `Include()`.**

Without `Include(e => e.Department)`, the `Department` property is null when you access it.

**Mistake 2: Forgetting `await`.**

All EF Core query methods are async. Forgetting `await` returns a `Task`, not data.

**Mistake 3: Calling `SaveChanges` after every operation.**

You can make multiple changes and call `SaveChangesAsync()` once:

```csharp
_context.Employees.Add(employee1);
_context.Employees.Add(employee2);
await _context.SaveChangesAsync();   // both saved in one trip
```

**Mistake 4: Not checking for null after `FirstOrDefaultAsync`.**

The entity might not exist. Always check before using it.

---

## Knowledge Check

1. What does `ToListAsync()` do?
2. What is the difference between `FirstOrDefaultAsync` and `FindAsync`?
3. What does `Include()` do?
4. When does data reach the database?
5. Why are the controller methods `async`?

---

## Next

The API now uses EF Core and SQL Server for all data operations. In the exercises, you will apply all five topics together.
