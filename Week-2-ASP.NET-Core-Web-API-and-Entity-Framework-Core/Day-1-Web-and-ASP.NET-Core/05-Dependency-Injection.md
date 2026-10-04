# 05 — Dependency Injection

---

## 🎯 What You Will Learn (40 min)

By the end you can:

- Explain what tight coupling is
- Define an interface and an implementation
- Register a service in `Program.cs`
- Inject it into a controller
- Explain what the container does when a controller is created

---

## The Problem

Here is the controller from Topic 04. Look at what it contains:

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private static readonly List<Employee> Employees = new()
    {
        new Employee { Id = 1, Name = "Amira", Department = "Engineering" },
        // ...
    };

    [HttpGet]
    public IActionResult GetAll() => Ok(Employees);             // data + logic

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)                        // data + logic
    {
        var employee = Employees.FirstOrDefault(e => e.Id == id);
        return employee is null ? NotFound() : Ok(employee);
    }
}
```

Three jobs in one class:

- HTTP (routing, status codes)
- Data (the list)
- Logic (finding an employee)

Now imagine the data must come from a database (later lessons), or a second controller needs the same lookup. You would edit — and duplicate — logic in every controller.

**This is the problem Dependency Injection solves.**

---

## What Is a Dependency?

A **dependency** is something a class needs in order to do its job.

```text
EmployeesController   needs   a way to read employees   → dependency
```

---

## Tight Coupling

The opposite of dependency injection: **creating your dependencies yourself**.

```csharp
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _service = new EmployeeService();
}
```

| Problem | What goes wrong |
|---------|-----------------|
| Hidden dependency | Reading the constructor does not tell you what the class really needs |
| Cannot swap the implementation | The controller is welded to `EmployeeService` |
| Responsibilities mixed | The controller decides *which* implementation to use |

**Tight coupling** = the class is glued to a specific implementation instead of a contract.

---

## Dependency Injection — The Fix

**Dependency Injection (DI)** means: the class receives the objects it depends on from the outside, instead of creating them itself.

```text
BEFORE

    EmployeesController
           │
           └── new EmployeeService()


AFTER

    EmployeesController
           │  needs IEmployeeService (a contract)
           ▼
    DI container (creates it)
           │
           └── EmployeeService
```

Three participants:

| Participant | Role |
|-------------|------|
| **Consumer** | `EmployeesController` — asks for a contract |
| **Contract** | `IEmployeeService` — what the object can do |
| **Implementation** | `EmployeeService` — how it is done |
| **Container** | ASP.NET Core's DI container — creates and delivers the objects |

---

## Interface and Implementation

### The contract

```csharp
// Services/IEmployeeService.cs
using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Services;

public interface IEmployeeService
{
    IEnumerable<Employee> GetAll();
    Employee? GetById(int id);
}
```

### The implementation

```csharp
// Services/EmployeeService.cs
using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Services;

public class EmployeeService : IEmployeeService
{
    private static readonly List<Employee> Employees = new()
    {
        new Employee { Id = 1, Name = "Amira", Department = "Engineering" },
        new Employee { Id = 2, Name = "Karim", Department = "HR" }
    };

    public IEnumerable<Employee> GetAll() => Employees;

    public Employee? GetById(int id) => Employees.FirstOrDefault(e => e.Id == id);
}
```

| | Interface `IEmployeeService` | Class `EmployeeService` |
|---|---|---|
| Answers | *What can be done?* | *How is it done?* |
| Contains | Method signatures | Data + method bodies |
| Used by | The controller | The container |

---

## Constructor Injection

The controller declares what it needs in its constructor:

```csharp
using EmployeeManagement.Api.Services;
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
    public IActionResult GetAll() => Ok(_employeeService.GetAll());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var employee = _employeeService.GetById(id);
        return employee is null ? NotFound() : Ok(employee);
    }
}
```

The controller now:

- ✅ holds **no data**,
- ✅ holds **no lookup logic**,
- ✅ does **only HTTP**.

---

## Service Registration

The container can only deliver what it has been told about. Registration lives in `Program.cs`, **before** `builder.Build()`:

```csharp
using EmployeeManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
// ...

var app = builder.Build();
// ...
```

| Half | Meaning |
|------|---------|
| `IEmployeeService` | The type the controller **asks for** |
| `EmployeeService` | The type the container **creates** |
| `AddScoped` | How long the object lives (next section) |

> ⚠️ The `using EmployeeManagement.Api.Services;` line is not decoration — without it, the build fails.

---

## How the Container Resolves a Request

```text
GET /api/employees/2
        │
        ▼
ROUTING  →  EmployeesController.GetById(2)
        │
        ▼
Container must CREATE the controller
        │
        ├─ reads the constructor: needs IEmployeeService
        ├─ looks in the registration list
        ├─ creates (or reuses) the EmployeeService
        └─ passes it to the constructor
        │
        ▼
GetById runs with a ready service  →  200 / 404
```

### When the registration is missing

Verified on .NET 8 — `IEmployeeService` was never registered:

```text
HTTP 500

Unable to resolve service for type 'EmployeeManagement.Api.Services.IEmployeeService'
while attempting to activate 'EmployeeManagement.Api.Controllers.EmployeesController'.
```

Read that message like a senior developer:

| Part | Meaning |
|------|---------|
| `Unable to resolve service for type ...IEmployeeService` | No registration for the interface |
| `while attempting to activate ...EmployeesController` | It happened while **creating the controller** |
| `HTTP 500` | The failure is on the **server** |

**Fix:** add the missing `builder.Services.AddScoped<...>(...)` line.

---

## Service Lifetimes — A Practical Introduction

| Lifetime | How many instances | When |
|----------|--------------------|------|
| **Transient** | New every time it is requested | Small, stateless objects |
| **Scoped** | One per HTTP request | Objects that hold *this request's* data |
| **Singleton** | One for the application's life | Shared, thread-safe objects |

In a web app, a **scope** is one HTTP request.

### Choosing — with reasons, not slogans

| Question | If yes | Choose |
|----------|--------|--------|
| Does the object keep data that belongs to *one* request? | Yes | **Scoped** |
| Is it tiny, cheap, and stateless? | Yes | **Transient** |
| Is it expensive to create and safe to share? | Yes | **Singleton** |
| Does it hold a mutable field that changes during a request? | Yes | **Never Singleton** |

> 💡 **Senior Developer Note:** Pick the lifetime from **what the object holds**, not from performance folklore.

---

## 🐛 Debugging Challenge — HTTP 500 on Every Request

**Situation.** Your API worked yesterday. Today every endpoint returns `500`. The Development error page shows:

```text
Unable to resolve service for type 'EmployeeManagement.Api.Services.IEmployeeService'
while attempting to activate 'EmployeeManagement.Api.Controllers.EmployeesController'.
```

1. What exactly is missing?
2. Which file do you open?
3. Which line do you add?

<details>
<summary><b>Answers</b></summary>

1. The container has no registration for `IEmployeeService`.
2. `Program.cs`.
3. `builder.Services.AddScoped<IEmployeeService, EmployeeService>();` (before `builder.Build()`), plus the `using` directive.

</details>

---

## 🧠 Knowledge Check

### Question 1

What is a dependency?

**Answer:** Something a class needs in order to work — usually another object handed to it.

---

### Question 2

Why is `new EmployeeService()` inside the controller a problem?

**Answer:** The controller creates its own dependency, so it is tied to one implementation. Injection moves that decision to registration in `Program.cs`.

---

### Question 3

Where do you register a service?

**Answer:** In `Program.cs`, in `builder.Services`, before `builder.Build()`.

---

### Question 4

Which lifetime gives one object per HTTP request?

**Answer:** Scoped.

---

### Question 5

Your endpoint returns `500` with *"Unable to resolve service for type X"*. Where do you look?

**Answer:** `Program.cs` — the registration for `X` is missing, or it registers a different type than the one being injected.

---

## ✅ Check Yourself

- [ ] I can explain dependency, tight coupling, and dependency injection in my own words
- [ ] I created an interface and an implementation for the Employee service
- [ ] I registered the service in `Program.cs` with the right `using` line
- [ ] I injected the service through the constructor and removed all data from the controller
- [ ] I can explain what the container does while creating a controller

---

## 🔜 Back to the Day

Today's five topics are one chain:

```text
HTTP & REST    →  the language both sides speak
ASP.NET Core   →  the server that answers
Project        →  where everything lives
Controllers    →  which C# code runs
DI             →  which objects it uses
```

**Next: the [Final Challenge](Exercises/08-Final-Challenge/)**
