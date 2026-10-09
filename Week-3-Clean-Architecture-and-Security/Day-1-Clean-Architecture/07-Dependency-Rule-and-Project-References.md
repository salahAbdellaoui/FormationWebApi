# 07 — Dependency Rule and Project References

**Duration**: 25 minutes

---

## The Dependency Rule

Source code dependencies can only point inward, toward the Domain layer. This is the single most important rule in Clean Architecture.

Inner layers must not know about outer layers. The Domain layer does not know Application exists. The Application layer does not know Infrastructure exists. Each inner layer is completely unaware of what sits outside it.

Why? Because the inner layers contain the most important and most stable code. Business rules change less often than database technology. If the Domain depends on EF Core, then changing EF Core forces you to change the Domain. That is backwards. The Domain should be the last thing you touch when infrastructure changes.

The Dependency Rule means:

- **Domain** depends on nothing. It is pure C#.
- **Application** depends only on Domain. It uses Domain entities but does not know about databases or HTTP.
- **Infrastructure** depends on Application and Domain. It implements interfaces defined by Application and uses entities defined by Domain.
- **API** depends on all three. It is the outermost layer and composes everything together.

If you draw an arrow from one layer to another, the arrow always points inward.

---

## Compile-Time vs. Runtime

This is the part that confuses most people. There are two different directions at play, and they are opposite.

### Compile-Time Direction (Project References)

At compile time, project references point inward:

- **Application** references **Domain** (Application uses Domain entities)
- **Infrastructure** references **Application** and **Domain** (Infrastructure implements Application interfaces and uses Domain entities)
- **API** references **Application**, **Infrastructure**, and **Domain** (API composes everything)
- **Domain** references nothing

A project reference means: "I need this code to compile." Application needs Domain types to compile. Infrastructure needs Application interfaces and Domain types to compile.

### Runtime Direction (Request Flow)

At runtime, the request flows the opposite direction — outward to inward:

```
HTTP Request
    -> API (Controller receives request)
        -> Application (Service processes use case)
            -> Infrastructure (Repository talks to database)
        <- Result returns back
    <- HTTP Response
```

The controller calls the service. The service calls the repository. The repository talks to the database. Data flows back out through the same layers.

### The Paradox

Here is what seems impossible: the Application service calls `IEmployeeRepository` at runtime. But `EmployeeRepository` lives in Infrastructure. Application does not reference Infrastructure at compile time. How can Application call code it does not reference?

The answer is **Dependency Injection**. DI bridges the gap between compile-time and runtime.

At compile time:
- Application defines `IEmployeeRepository` (an interface)
- Infrastructure implements `EmployeeRepository` (a class)
- Application has no reference to Infrastructure

At runtime:
- The DI container creates an `EmployeeRepository` instance
- The DI container injects it into `EmployeeService` as `IEmployeeRepository`
- `EmployeeService` calls the repository through the interface, without knowing the concrete type

The DI container is the bridge. It connects the inner layer's interface to the outer layer's implementation, at runtime, without creating a compile-time dependency.

---

## Project Reference Diagram

```text
Domain
  ^
  |
Application
  ^
  |
Infrastructure

API --> Application
API --> Infrastructure (composition)
Application --> Domain
Infrastructure --> Application, Domain
Domain --> (nothing)
```

Each arrow represents a project reference. The `^` symbols show that inner layers sit below and outer layers point down into them. The table below explains every arrow:

| From | To | Why |
|---|---|---|
| Application | Domain | Application uses Domain entities (`Employee`, `Department`) in its services and DTOs |
| Infrastructure | Application | Infrastructure implements Application interfaces (`IEmployeeRepository`) and registers Application services (`EmployeeService`) |
| Infrastructure | Domain | Infrastructure uses Domain entities in EF Core (`DbSet<Employee>`) and repositories |
| API | Application | API controllers inject Application interfaces (`IEmployeeService`) and use Application DTOs |
| API | Infrastructure | API calls `AddInfrastructure()` to register DI services (composition root) |
| API | Domain | API references Domain directly for entity types used in controllers and DTOs |
| Domain | (nothing) | Domain is the innermost layer. It has zero dependencies on outer layers |

---

## How Dependency Injection Resolves the Paradox

The key problem: Application needs to call Infrastructure code at runtime, but Application must not reference Infrastructure at compile time.

Step 1: Application defines the interface.

```csharp
namespace EmployeeManagement.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task AddAsync(Employee employee);
}
```

Step 2: Application service uses the interface. It does not know what implements it.

```csharp
namespace EmployeeManagement.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;

    public EmployeeService(IEmployeeRepository repository)
    {
        _repository = repository;
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
        return new EmployeeDto { Id = employee.Id, Name = employee.Name };
    }
}
```

Step 3: Infrastructure implements the interface.

```csharp
namespace EmployeeManagement.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
    }
}
```

Step 4: API wires them together in Program.cs (the composition root).

```csharp
builder.Services.AddInfrastructure(builder.Configuration);
```

Inside `AddInfrastructure`:

```csharp
services.AddScoped<IEmployeeRepository, EmployeeRepository>();
services.AddScoped<IEmployeeService, EmployeeService>();
```

The key insight: Application does not reference Infrastructure. But at runtime, the DI container provides the Infrastructure implementation (`EmployeeRepository`) when Application's `EmployeeService` asks for `IEmployeeRepository`. The compile-time dependency is absent. The runtime dependency exists. This is what Dependency Inversion means.

---

## The Actual .csproj Reference Graph

These are the real project references in the EmployeeManagement solution:

```
EmployeeManagement.Domain          -> (no project references)
EmployeeManagement.Application     -> EmployeeManagement.Domain
EmployeeManagement.Infrastructure  -> EmployeeManagement.Domain, EmployeeManagement.Application
EmployeeManagement.Api             -> EmployeeManagement.Domain, EmployeeManagement.Application, EmployeeManagement.Infrastructure
```

Notice the pattern: each project only references projects that sit inside it. The outermost project (API) references everything. The innermost project (Domain) references nothing.

This matches the Dependency Rule exactly. References point inward.

---

## Circular References: The Red Line

A circular reference occurs when project A references project B, and project B references project A. This creates a loop that the compiler cannot resolve.

Example of a broken reference graph:

```
Domain -> Application (WRONG)
Application -> Domain
```

If Domain references Application, and Application references Domain, the compiler cannot build either project. Neither can compile first because each needs the other.

The .NET build system catches this immediately. You get an error:

```
MSB3105: The item "...\EmployeeManagement.Domain.csproj" was specified in the input file
but it forms a circular dependency
```

This is a good thing. The compiler prevents you from breaking the Dependency Rule at compile time. You cannot accidentally ship code where Domain depends on Infrastructure. The build fails before it ever runs.

In practice, circular references usually happen when a developer puts a convenience method in the wrong layer. For example, if Domain needs to call a service defined in Application, that is a sign the method belongs in Domain, not Application.

---

## Common Mistakes

**Mistake 1: Domain referencing EF Core.**
A developer adds `Microsoft.EntityFrameworkCore` to the Domain project to use data annotations like `[Table]` or `[Column]`. This breaks the Dependency Rule. Domain now depends on a framework. If you change the database library, Domain must change. Data annotations belong in Infrastructure using Fluent API configuration.

**Mistake 2: Application referencing Infrastructure.**
A developer adds `using EmployeeManagement.Infrastructure;` inside an Application service to use `EmployeeRepository` directly. This bypasses the interface. The Dependency Inversion Principle is broken. Application should only use `IEmployeeRepository`, never the concrete class.

**Mistake 3: Confusing runtime flow with compile-time references.**
A developer looks at the request flow (API -> Application -> Infrastructure) and assumes the project references should follow the same direction. This is backwards. Project references point inward (toward Domain). Runtime flow goes outward-to-inward and back. The directions are opposite.

**Mistake 4: API not referencing Infrastructure.**
The API must reference Infrastructure because it calls `AddInfrastructure()` in Program.cs. Without this reference, the DI container cannot register `EmployeeRepository` as the implementation for `IEmployeeRepository`. The app compiles, but fails at runtime with "No service for type IEmployeeRepository."

**Mistake 5: Infrastructure defining its own interfaces.**
A developer defines `IEmployeeRepository` in Infrastructure and implements it there. This works but breaks Clean Architecture. The interface belongs in Application so that Application controls its own contracts. If the interface is in Infrastructure, Application depends on Infrastructure to define what it needs.

---

## Knowledge Check

1. Why must project references point inward (toward Domain) while the runtime request flow goes outward to inward? What role does Dependency Injection play in making both directions work?

2. A developer adds `using EmployeeManagement.Infrastructure.Repositories;` to `EmployeeService.cs` in the Application layer and uses `EmployeeRepository` directly instead of `IEmployeeRepository`. What rule is broken, and what happens if you try to build?

3. The API project references Infrastructure. Does this violate the Dependency Rule? Why or why not?
