# 04 — Services and Use Cases

**Duration:** 30 minutes

---

## What Is an Application Service?

An application service (or use case) coordinates a business operation. It doesn't contain business rules (that's Domain) or technical details (that's Infrastructure). It orchestrates the flow.

---

## Our EmployeeService

```csharp
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;

    public EmployeeService(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
    {
        var employees = await _repository.GetWithDepartmentAsync();
        return employees.Select(MapToDto);
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _repository.GetByIdWithDepartmentAsync(id);
        return employee == null ? null : MapToDto(employee);
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
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

    private static EmployeeDto MapToDto(Employee employee) { ... }
}
```

This service does three things:

1. Receives a request (DTO or parameters)
2. Coordinates with the repository
3. Maps entities to DTOs
4. Returns a result

---

## Service vs. Controller vs. Domain

| Layer | Responsibility | Example |
|---|---|---|
| Controller | HTTP concerns: routing, status codes, request/response | Returns 404 if employee not found |
| Service | Orchestration: coordinate the use case | Create employee, save, return DTO |
| Domain | Business rules: what makes an employee valid | Salary must be positive |
| Repository | Data access: how to get/save entities | Query with Include, SaveChanges |

---

## The Create Employee Use Case

```
HTTP POST /api/employees
    |
    v
Controller.Create(CreateEmployeeDto dto)
    |
    v
EmployeeService.CreateEmployeeAsync(dto)
    |
    v
new Employee { Name = dto.Name, ... }
    |
    v
repository.AddAsync(employee)
    |
    v
DbContext.SaveChangesAsync()
    |
    v
Return EmployeeDto
    |
    v
Controller returns 201 Created
```

Each step has a clear owner:

- **Controller** handles HTTP: receives the request, returns 201 Created.
- **Service** handles the operation: creates the entity, calls the repository, maps the result.
- **Repository** handles persistence: adds to DbContext, triggers SaveChanges.

---

## When the Service Becomes Too Large

If `EmployeeService` grows to 50+ methods, it becomes a "God Service." Options:

- Split into smaller services (`EmployeeCommandService`, `EmployeeQueryService`)
- Use explicit use-case classes (`CreateEmployeeHandler`, `GetEmployeeHandler`)
- Introduce MediatR (but only when the complexity justifies it)

For this application, a single service is fine. Don't over-engineer.

---

## Alternative: Explicit Use-Case Classes

Some teams prefer each use case as a separate class:

```csharp
public class CreateEmployeeUseCase
{
    public async Task<EmployeeDto> ExecuteAsync(CreateEmployeeDto dto) { ... }
}

public class GetEmployeeByIdUseCase
{
    public async Task<EmployeeDto?> ExecuteAsync(int id) { ... }
}
```

This is an organizational choice. Some teams prefer it for large applications. Others find it creates too many files. Neither is universally right.

---

## Key Takeaway

The service coordinates. It doesn't contain business rules (Domain) or technical details (Infrastructure). Keep it focused on the use case.

---

## Common Mistakes

1. **Putting business rules in the service** — "Salary must be positive" is a domain rule, not a service concern. Put it in the entity or a domain validator.
2. **Putting HTTP concerns in the service** — Status codes, content negotiation, and request binding belong in the controller.
3. **Creating a God Service** — If a service has 50+ methods, split it. One service per bounded context or per command/query responsibility.
4. **Confusing service with repository** — The repository fetches and saves data. The service coordinates the full use case including mapping, validation, and orchestration.

---

## Knowledge Check

1. What are the four responsibilities of `EmployeeService` listed in this lesson?
2. In the Create Employee flow, which layer returns 201 Created and which layer calls `SaveChangesAsync`?
3. If `EmployeeService` grows to 50 methods, name two strategies to address the problem.
