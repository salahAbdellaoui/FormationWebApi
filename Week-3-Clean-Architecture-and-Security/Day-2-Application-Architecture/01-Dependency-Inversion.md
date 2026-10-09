# 01 — Dependency Inversion

**Duration**: 25 minutes

---

## What Is the Dependency Inversion Principle?

The Dependency Inversion Principle (DIP) is the "D" in SOLID. It states two things:

1. High-level modules should not depend on low-level modules. Both should depend on abstractions.
2. Abstractions should not depend on details. Details should depend on abstractions.

In plain terms: the important code (business logic) should not know about the unimportant code (database, file system, external APIs). The important code defines what it needs. The unimportant code fulfills that need.

Think of it as a job posting. The Application layer writes the job description: "I need something that can save and retrieve employees." The Infrastructure layer applies for the job: "I use EF Core and SQL Server to do exactly that." The Application layer never asks who applied. It only checks whether the candidate meets the requirements.

The principle says: decide WHAT you need (interface), not WHO provides it (implementation).

---

## DIP vs. Dependency Injection vs. Interfaces

These three concepts are related but distinct. Many developers use the terms interchangeably. They should not.

| Concept | What It Is | Role |
|---|---|---|
| Dependency Inversion Principle | A design principle | Tells you to depend on abstractions |
| Dependency Injection | A technique | Supplies the implementation at runtime |
| Interface | A contract | Defines the abstraction |

DIP is the **why**. It tells you that depending on concrete implementations creates fragile code.

Interface is the **what**. It defines the abstraction that both sides agree on.

DI is the **how**. It is the mechanism that delivers the implementation without the consumer hardcoding a reference to it.

You can have interfaces without DIP (interfaces in the wrong layer). You can have DI without DIP (injecting concrete classes). You need all three working together for the principle to hold.

---

## Our Day 1 Example

The Employee Management solution from Day 1 demonstrates DIP at every boundary.

### Step 1: Application Defines the Abstraction

The Application layer defines `IEmployeeRepository`. This interface lives in `EmployeeManagement.Application/Interfaces/`:

```csharp
public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByIdWithDepartmentAsync(int id);
    Task<IEnumerable<Employee>> GetWithDepartmentAsync();
    Task AddAsync(Employee employee);
}
```

Application says: "I need the ability to store and retrieve employees. Here is exactly what I expect."

### Step 2: High-Level Module Depends on the Abstraction

`EmployeeService` lives in the Application layer. It is the high-level module. It depends on `IEmployeeRepository`, not on any concrete class:

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
}
```

`EmployeeService` knows nothing about EF Core, SQL Server, or file storage. It only knows about `IEmployeeRepository`.

### Step 3: Low-Level Module Fulfills the Contract

`EmployeeRepository` lives in the Infrastructure layer. It implements the interface that Application defined:

```csharp
public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Employee>> GetWithDepartmentAsync()
    {
        return await _context.Employees
            .Include(e => e.Department)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
    }
}
```

Infrastructure knows about EF Core. Infrastructure knows about `AppDbContext`. But Application does not know about any of that. The dependency arrow points from Infrastructure toward Application, not the other way around.

### Step 4: Composition Root Wires It Together

`DependencyInjection.cs` in Infrastructure registers the mapping between interface and implementation:

```csharp
services.AddScoped<IEmployeeRepository, EmployeeRepository>();
services.AddScoped<IEmployeeService, EmployeeService>();
```

When the runtime creates an `EmployeeService`, the DI container injects an `EmployeeRepository`. The high-level module gets its dependency satisfied without ever referencing the low-level module directly.

---

## What Would Break the Principle?

### Violation 1: Service Depends on Concrete Repository

If `EmployeeService` depended on `EmployeeRepository` instead of `IEmployeeRepository`:

```csharp
public class EmployeeService : IEmployeeService
{
    private readonly EmployeeRepository _repository;

    public EmployeeService(EmployeeRepository repository)
    {
        _repository = repository;
    }
}
```

Application now depends on Infrastructure. The project reference would have to point from Application to Infrastructure. The Dependency Rule is violated. You can no longer swap persistence mechanisms without changing Application code.

### Violation 2: Service Depends on AppDbContext Directly

If `EmployeeService` used `AppDbContext` directly:

```csharp
public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;

    public EmployeeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .AsNoTracking()
            .ToListAsync();
        return employees.Select(MapToDto);
    }
}
```

Now Application knows about EF Core. LINQ queries, `Include`, `AsNoTracking` are all EF Core concerns living in the business logic layer. Testing requires a database or a complex in-memory provider setup. Swapping to Dapper or MongoDB means rewriting the service.

The interface exists precisely to prevent this.

---

## When Is an Interface Worth It?

Not everything needs an interface. Creating an interface for every class is a common overcorrection that adds ceremony without value.

An interface earns its place when:

- **It marks a boundary between layers.** `IEmployeeRepository` separates Application from Infrastructure. This is the primary reason the interface exists.
- **You need to swap implementations.** Today you use SQL Server. Tomorrow you might need PostgreSQL, or an in-memory provider for tests. The interface makes that a configuration change, not a code change.
- **You need to test in isolation.** `EmployeeService` can be tested with a fake repository that returns known data. Without the interface, you need a real database.
- **Multiple implementations exist or are planned.** An email service might have a real implementation and a mock implementation for development.

An interface is unnecessary when:

- **It wraps a single class that never changes.** If there is only one implementation, no plan for a second, and no testing benefit, the interface adds a file that nobody reads.
- **It exists only because "that is what you do."** Patterns should solve problems, not satisfy habits.

A good test: if you deleted the interface, would anything break or become harder? If the answer is no, the interface is ceremony.

---

## The Key Insight

DIP is about who DECIDES what you depend on.

If the high-level module (Application) chooses its own abstraction ("I need a repository that returns employees"), and the low-level module (Infrastructure) conforms to that abstraction, DIP is satisfied.

If the low-level module forces its shape on the high-level module ("You will use my `DbContext`, my `DbSet`, my `SaveChanges`"), DIP is violated.

The inversion is in the direction of the decision. Normally, the higher-level code calls the lower-level code. The dependency flows downward. DIP inverts this: the lower-level code depends on the higher-level code's definition of what it needs. The dependency flows upward.

This is why Application defines `IEmployeeRepository` and Infrastructure implements it. The Application layer, which is more stable and more important, gets to set the terms.

---

## Common Mistakes

1. **Creating interfaces for everything.** Every class gets an `I` prefix interface. Most of these interfaces have one implementation and are never swapped. This adds files, adds indirection, and teaches the team that interfaces are free. They are not free. Every interface is a contract that must be maintained.

2. **Confusing DI with DIP.** "I inject my dependencies, so I follow DIP." Not necessarily. If you inject a concrete `EmployeeRepository` into `EmployeeService`, you are using DI but violating DIP. DI is the technique. DIP is the principle. They work together, but they are not the same thing.

3. **Putting the interface in the wrong layer.** If `IEmployeeRepository` lived in Infrastructure, and Application referenced Infrastructure to use it, the Dependency Rule is broken. The interface must live in the layer that consumes it, not the layer that implements it.

4. **Letting the implementation leak into the abstraction.** If `IEmployeeRepository` returns `IQueryable<Employee>` or exposes EF Core types, the abstraction is not an abstraction. It is a leaky wrapper around the implementation. The interface should speak in domain terms, not infrastructure terms.

---

## Knowledge Check

**Question 1**: In the Employee Management solution, which project defines `IEmployeeRepository`, and which project implements it? Why is it set up this way instead of the other way around?

**Question 2**: A developer adds a `using Microsoft.EntityFrameworkCore;` statement to `EmployeeService`. Is this a DIP violation? Explain why or why not.

**Question 3**: A team decides to remove `IEmployeeService` and have `EmployeesController` depend directly on `EmployeeService`. The code still works. What has been lost?

<details>
<summary>Answers</summary>

**Answer 1**: Application defines `IEmployeeRepository`. Infrastructure implements it. This is because Application is the higher-level module: it contains business logic. Business logic should not depend on infrastructure details. By defining the interface in Application, the dependency flows inward (from Infrastructure to Application), which satisfies the Dependency Rule.

**Answer 2**: Yes, this is a DIP violation. `EmployeeService` is in the Application layer. `Microsoft.EntityFrameworkCore` is an infrastructure concern. By referencing EF Core types directly, Application now depends on a low-level framework. Even if the developer only uses the import for a LINQ extension method, the boundary is compromised. The fix is to move that EF Core logic into `EmployeeRepository`, where it belongs.

**Answer 3**: The controller now depends on a concrete class instead of an abstraction. You can no longer swap the service implementation without changing the controller. You can no longer test the controller with a fake service. The Dependency Inversion Principle is violated at the API-to-Application boundary.

</details>
