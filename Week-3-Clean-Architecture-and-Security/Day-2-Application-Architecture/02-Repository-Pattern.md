# 02 — Repository Pattern

**Duration**: 30 minutes

---

## What Is a Repository?

A repository encapsulates data access logic behind an interface. The consumer asks for domain objects. The repository handles the details of querying, mapping, and persistence.

The caller says: "Give me the employee with ID 5, including their department." The repository says: "I will figure out how to do that." The caller does not write SQL, does not write LINQ, does not manage a database connection. The caller works with domain objects and domain concepts.

A repository creates a boundary between "what data I need" and "how I get it."

---

## Our Repository in Action

### The Interface

`IEmployeeRepository` lives in `EmployeeManagement.Application/Interfaces/`:

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

Every method speaks in domain terms. There is no `IQueryable`, no `DbSet`, no `Expression<Func<...>>`. Application says: "I need to get employees. I need to add employees."

### The Implementation

`EmployeeRepository` lives in `EmployeeManagement.Infrastructure/Repositories/`:

```csharp
public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _context.Employees.AsNoTracking().ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees.FindAsync(id);
    }

    public async Task<Employee?> GetByIdWithDepartmentAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
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

Infrastructure says: "I use EF Core to fulfill those requests." All the EF Core concepts, `Include`, `AsNoTracking`, `FindAsync`, `ToListAsync`, live here. They never appear in Application.

---

## Benefits of the Repository Pattern

- **Application does not know about EF Core.** `EmployeeService` has no `using Microsoft.EntityFrameworkCore;`. No `DbSet`, no `DbContext`. If you swap EF Core for Dapper, Application does not change.

- **Testability.** You can test `EmployeeService` with a fake repository that returns known data. No database, no in-memory provider, no test setup ceremony. You write a class that implements `IEmployeeRepository`, return what you want, and assert behavior.

- **Swappable persistence.** If the team migrates from SQL Server to PostgreSQL, only `EmployeeRepository` and `AppDbContext` change. The rest of the codebase does not know.

- **Centralized query logic.** The query for "get all employees with their department" exists in one place. If five services need this data, they all call `GetWithDepartmentAsync()`. The query does not get duplicated and drifted across the codebase.

- **Hidden complexity.** `Include`, `AsNoTracking`, `FirstOrDefaultAsync` are EF Core details. The repository hides them behind `GetByIdWithDepartmentAsync`. The consumer does not need to know what eager loading means.

---

## The Critical Question: Does EF Core Already Give Us a Repository?

This is the most important section in this lesson. Read it carefully.

EF Core's `DbSet<T>` already behaves like a repository. It provides methods to query, add, update, and remove entities. `DbContext` tracks changes and calls `SaveChangesAsync` to persist. The Unit of Work pattern is already implemented by `DbContext`.

So why add a custom repository at all?

| Without Custom Repository | With Custom Repository |
|---|---|
| Service injects `DbContext` directly | Service injects `IEmployeeRepository` |
| Service uses LINQ against `DbSet` | Service calls repository methods |
| EF Core concepts leak into Application | Application only knows domain methods |
| Harder to test (need fake `DbContext`) | Easy to test (mock the interface) |
| Less code, less indirection | More files, clearer boundaries |

Both approaches work. The question is not which one is correct. The question is which trade-off the team wants to make.

---

## When the Repository Adds Value

The custom repository earns its place when:

- **Complex queries benefit from encapsulation.** A query with multiple `Include`, `Where`, `OrderBy`, and pagination logic belongs in one place. Scattering it across service methods creates duplication and inconsistency.

- **Testing without a database matters.** If the team writes unit tests for services and wants to avoid the EF Core in-memory provider (which behaves differently from a real database), a fake repository is straightforward.

- **Persistence might change.** If there is a real possibility of swapping from EF Core to Dapper or a micro-ORM, the repository isolates that change to Infrastructure.

- **Team boundaries require it.** In a larger team, the repository enforces a contract between the developers writing business logic and the developers writing data access. The interface is the handshake.

---

## When the Repository Is Unnecessary

The custom repository adds overhead without proportional benefit when:

- **Simple CRUD with basic queries.** If every repository method is just `await _context.Employees.ToListAsync()` or `await _context.Employees.FindAsync(id)`, you have added a class that delegates every call to EF Core. The repository is a pass-through wrapper.

- **Small team, small codebase.** If one developer maintains the entire application and the queries are simple, the indirection cost is higher than the isolation benefit.

- **The abstraction cost outweighs the benefit.** Every interface is a file to maintain. Every method signature is a contract to keep in sync. If the repository adds 15 methods and all of them just wrap `DbSet`, the team is paying for abstraction without receiving abstraction.

- **You are wrapping every `DbSet` method.** If the repository has `AddAsync`, `UpdateAsync`, `DeleteAsync`, `GetByIdAsync`, `GetAllAsync`, and each one is a single line that calls the corresponding `DbSet` method, the repository is adding files without adding value.

---

## An Honest Assessment

For this Employee Management application, the custom repository is primarily a teaching tool. It demonstrates the pattern. It shows how Application defines the contract and Infrastructure fulfills it. It proves that `EmployeeService` can work without knowing EF Core exists.

In a real production project with simple queries like these, a team might choose to inject `DbContext` directly into the service layer and accept the trade-off. Many successful .NET applications do exactly that. The EF Core team itself has stated that `DbContext` is a Unit of Work and `DbSet` is a Repository.

The important thing is making a **deliberate choice**, not following a pattern because a tutorial said so. If you choose to use a custom repository, you should be able to explain why. If you choose to use `DbContext` directly, you should be able to explain why. Both are valid. Neither is free.

Ask yourself: what problem does this pattern solve for my team, in my codebase, right now? If the answer is "none," the pattern is overhead. If the answer is "it lets us test without a database" or "it hides our complex queries," the pattern is earning its keep.

---

## Common Mistakes

1. **Generic repository that adds no value.** Creating `IRepository<T>` with `GetAllAsync()`, `GetByIdAsync()`, `AddAsync()`, `UpdateAsync()`, `DeleteAsync()` and implementing it with a thin wrapper around `DbSet<T>`. This is a pass-through. Every method does exactly what `DbSet` already does. You have added an abstraction layer that abstracts nothing.

2. **Repository that contains business logic.** Putting validation, rules, or orchestration inside the repository. A repository should retrieve and persist domain objects. It should not check whether an email is unique or whether a salary is within range. That logic belongs in the service layer.

3. **Repository that returns infrastructure types.** If `IEmployeeRepository` returns `IQueryable<Employee>` or `Task<List<Employee>>` with EF Core tracking behavior, the abstraction is leaking. The interface should return domain objects in a way that hides persistence details. `IEnumerable<Employee>` is a better choice than `IQueryable<Employee>` because `IQueryable` implies the caller will compose LINQ expressions, which are EF Core expressions.

4. **Repository without a matching interface.** Implementing `EmployeeRepository` as a concrete class and injecting it directly into the service. This is not a repository. This is a class that happens to do data access. The pattern requires the abstraction. Without the interface, you get none of the testability or swappability benefits.

---

## Knowledge Check

**Question 1**: Look at `EmployeeRepository.AddAsync`. It calls `_context.SaveChangesAsync()`. Why might this be a problem if you later introduce a Unit of Work pattern? What should change?

**Question 2**: A developer writes `IEmployeeRepository.GetAllAsync()` as `Task<IQueryable<Employee>>`. The service then writes `_repository.GetAllAsync().Where(e => e.Salary > 50000)`. What is wrong with this design?

**Question 3**: The team decides to remove `EmployeeRepository` entirely and inject `AppDbContext` directly into `EmployeeService`. Name two things that become harder and two things that become easier.

<details>
<summary>Answers</summary>

**Answer 1**: `AddAsync` calls `SaveChangesAsync`, which immediately persists to the database. If you introduce a Unit of Work that coordinates multiple repository operations into a single transaction, each repository's `AddAsync` should add to the context without saving. The Unit of Work's `SaveChangesAsync` commits everything at once. Calling `SaveChangesAsync` inside the repository prevents the Unit of Work from batching changes.

**Answer 2**: Returning `IQueryable` leaks the query mechanism into the Application layer. The service now composes LINQ expressions that will be translated by EF Core. Application is implicitly coupled to EF Core's query translation behavior. The repository should encapsulate the query: if the service needs employees with salary above a threshold, the repository should have a method like `GetBySalaryAboveAsync(decimal threshold)`.

**Answer 3**: Harder: (1) testing `EmployeeService` now requires a fake `DbContext` or an in-memory database instead of a simple fake repository, (2) EF Core concepts like `Include` and `AsNoTracking` leak into the service layer. Easier: (1) fewer files to maintain, no interface-implementation split to keep in sync, (2) simpler debugging with one less layer of indirection.

</details>
