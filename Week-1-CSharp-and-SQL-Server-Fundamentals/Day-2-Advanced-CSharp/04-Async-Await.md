# 04 — async / await

---

## The Problem

Your application needs to:

1. Call a database to get employee data
2. Call an external API to check something
3. Read a file from disk

These operations take time. While waiting, the application is **blocked** — it cannot do anything else.

```csharp
// Synchronous — blocks while waiting
var employees = GetEmployeesFromDatabase();  // Wait 2 seconds
var result = CallExternalApi();               // Wait 3 seconds
// Total: 5 seconds, application is frozen
```

> In a web application, blocking means other users have to wait too.

---

## What Is Asynchronous Programming?

Asynchronous code **does not block** while waiting. It starts the operation, lets other work continue, and comes back when the result is ready.

```text
Synchronous:
Start task → Wait → Wait → Wait → Done

Asynchronous:
Start task → Start task → Start task → Results ready
```

> 💡 **Important:** async/await does **not** make code faster. It makes the application **responsive** while waiting for slow operations.

---

## The Basics

### Task

A `Task` represents an operation that will complete in the future.

```csharp
Task task = SomeOperationAsync();  // Starts the operation
await task;                        // Waits for it to finish
```

### Task\<T>

When the operation returns a value:

```csharp
Task<Employee> task = GetEmployeeAsync(1);  // Returns an Employee
Employee employee = await task;             // Get the result
```

### async and await

- `async` — marks a method as asynchronous
- `await` — waits for a task to complete without blocking

```csharp
public async Task<Employee?> GetEmployeeAsync(int id)
{
    // Imagine this calls a database
    Employee? employee = await database.GetByIdAsync(id);
    return employee;
}
```

---

## A Simple Example

```csharp
public async Task<List<Employee>> GetEmployeesAsync()
{
    // Simulate a slow database call
    await Task.Delay(2000);  // Wait 2 seconds (simulated)

    return new List<Employee>
    {
        new Employee { Name = "Ali", Salary = 5000 },
        new Employee { Name = "Sara", Salary = 7000 }
    };
}
```

**Usage:**

```csharp
Console.WriteLine("Loading...");
var employees = await GetEmployeesAsync();
Console.WriteLine($"Loaded {employees.Count} employees");
```

**Output:**

```text
Loading...
(2 second pause)
Loaded 2 employees
```

---

## What Actually Happens?

```text
1. Method starts executing
2. await Task.Delay(2000) is encountered
3. Method pauses — but the thread is released (not blocked)
4. Other code can run while waiting
5. After 2 seconds, the method resumes
6. Returns the result
```

---

## When to Use async/await

Use async for operations that **wait for something external**:

| Operation | Why Async Helps |
|-----------|----------------|
| Database calls | Network latency |
| HTTP requests | External service response time |
| File I/O | Disk read/write time |
| Long computations | Freeing the UI thread |

Do **not** use async for simple CPU work (like a loop or calculation).

---

## ⚠️ Common Mistakes

### Mistake 1 — Using .Result

```csharp
// BAD — blocks the thread, can cause deadlocks
var employees = GetEmployeesAsync().Result;
```

Always use `await` instead:

```csharp
// GOOD
var employees = await GetEmployeesAsync();
```

### Mistake 2 — Using .Wait()

```csharp
// BAD — same problem as .Result
GetEmployeesAsync().Wait();
```

### Mistake 3 — Forgetting await

```csharp
// BAD — method runs but you never get the result
public void LoadData()
{
    var employees = GetEmployeesAsync();  // Missing await!
    Console.WriteLine(employees.Count);   // Error!
}
```

### Mistake 4 — Making everything async unnecessarily

```csharp
// BAD — no reason for this to be async
public async Task<int> Add(int a, int b)
{
    return a + b;  // No await, no async operation
}
```

Only use async when there is an actual asynchronous operation.

---

## async/await with LINQ

Combine async with collections:

```csharp
public async Task ProcessEmployeesAsync(List<int> employeeIds)
{
    foreach (var id in employeeIds)
    {
        var employee = await GetEmployeeAsync(id);
        Console.WriteLine(employee?.Name);
    }
}
```

> ⚠️ This processes one at a time. For parallel processing, use `Task.WhenAll`:

```csharp
public async Task ProcessEmployeesAsync(List<int> employeeIds)
{
    var tasks = employeeIds.Select(id => GetEmployeeAsync(id));
    var employees = await Task.WhenAll(tasks);

    foreach (var emp in employees)
    {
        Console.WriteLine(emp?.Name);
    }
}
```

---

## 🤔 Think

> What is the difference between these two?

```csharp
// Version A
var result = await GetEmployeeAsync(1);

// Version B
var task = GetEmployeeAsync(1);
var result = await task;
```

**Answer:** They are essentially the same. Version B is useful when you want to start multiple tasks before waiting for any of them.

---

## Real-World Pattern

```csharp
public class EmployeeService
{
    private readonly IEmployeeRepository _repository;

    public EmployeeService(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid employee ID");

        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<Employee>> GetAllActiveAsync()
    {
        var employees = await _repository.GetAllAsync();
        return employees.Where(e => e.IsActive).ToList();
    }
}
```

This is how async/await appears in real .NET applications.

---

## Summary

| Keyword | Purpose |
|---------|---------|
| `async` | Marks a method as asynchronous |
| `await` | Waits for a task without blocking |
| `Task` | An operation that completes in the future |
| `Task<T>` | An operation that returns a value |

| Mistake | Fix |
|---------|-----|
| `.Result` | Use `await` |
| `.Wait()` | Use `await` |
| Missing `await` | Add `await` |
| Async with no async operation | Remove `async` |

> 💡 **Senior Developer Note:** async/await is essential in web applications. Every database call and HTTP request should be async. It keeps your application responsive and scalable.

---

**Next: [05 — Practical Exercises](05-Practical-Exercises.md)**
