# 03 — Unit of Work

**Duration:** 25 minutes

---

## What Is Unit of Work?

Unit of Work (UoW) is a pattern that tracks all changes made during a business operation and saves them as a single atomic unit. It ensures that either all changes succeed or none do.

---

## EF Core Already Does This

This is critical. `DbContext` already implements Unit of Work behavior:

- It tracks changes to entities (Added, Modified, Deleted)
- `SaveChangesAsync()` persists all tracked changes in a single transaction
- If SaveChanges fails, nothing is persisted

Look at the actual code from `EmployeeRepository.AddAsync`:

```csharp
public async Task AddAsync(Employee employee)
{
    _context.Employees.Add(employee);
    await _context.SaveChangesAsync();
}
```

The repository calls `SaveChangesAsync` immediately after adding. This means each repository method saves independently.

---

## The Trade-Off: Save in Repository vs. Save in Service

**Approach 1 — Repository Saves (Current Implementation):**

```csharp
// Repository
public async Task AddAsync(Employee employee)
{
    _context.Employees.Add(employee);
    await _context.SaveChangesAsync();  // Save happens here
}

// Service
public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
{
    var employee = new Employee { ... };
    await _repository.AddAsync(employee);  // Already saved
    return new EmployeeDto { ... };
}
```

**Approach 2 — Service Controls Saving:**

```csharp
// Repository
public void Add(Employee employee)
{
    _context.Employees.Add(employee);  // Track only, don't save
}

// Service
public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
{
    var employee = new Employee { ... };
    _repository.Add(employee);
    await _context.SaveChangesAsync();  // Save happens here
    return new EmployeeDto { ... };
}
```

| Approach 1: Repository Saves | Approach 2: Service Saves |
|---|---|
| Simpler service code | Service controls the transaction |
| Each operation saves independently | Can combine multiple operations |
| Harder to coordinate multi-step operations | Can save after all steps complete |
| Good for single-entity operations | Better for complex use cases |

---

## Why Not Just Add an IUnitOfWork?

Some tutorials add `IUnitOfWork` with a `SaveAsync()` method. But `DbContext` already does this. Adding `IUnitOfWork` that just wraps `SaveChangesAsync` is redundant unless you need to:

- Coordinate multiple DbContexts
- Abstract the save mechanism for testing
- Manage distributed transactions (advanced)

For this application, a custom `IUnitOfWork` would add complexity without benefit.

---

## When the Current Approach Is Fine

For single-entity operations (create one employee), saving in the repository works. The operation is atomic by nature.

---

## When You Need Service-Level Control

If a use case creates an employee AND sends a notification email AND logs the action, you want all steps to succeed or fail together. Saving at the service level gives you that control.

---

## Key Takeaway

EF Core's `DbContext` IS the Unit of Work. You don't need to wrap it unless you have a specific reason. The current implementation saves in the repository, which is fine for simple operations. For complex use cases, move the `SaveChangesAsync` to the service.

---

## Common Mistakes

1. **Adding IUnitOfWork just to wrap SaveChangesAsync** — DbContext already does this. You are adding a layer that does nothing.
2. **Calling SaveChanges in every repository method when you need coordination** — If two operations must succeed or fail together, save once at the service level.
3. **Thinking you need distributed transactions for everything** — Most operations in a typical CRUD app are single-entity. Don't over-engineer.
4. **Mixing approaches** — Don't save in the repository for some methods and in the service for others within the same use case. Pick one boundary per use case.

---

## Knowledge Check

1. What pattern does `DbContext` already implement, and what method triggers it?
2. In Approach 2 (Service Controls Saving), what changes in the repository compared to Approach 1?
3. Name one scenario where adding a custom `IUnitOfWork` on top of `DbContext` would be justified.
