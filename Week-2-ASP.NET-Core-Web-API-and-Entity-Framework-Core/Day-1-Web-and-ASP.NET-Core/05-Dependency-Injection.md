# 05 — Dependency Injection

---

## 🎯 Learning Objectives

By the end of this topic, you will be able to:

- Explain what a dependency is and what tight coupling costs you
- Describe Dependency Injection and constructor injection in your own words
- Define an interface and an implementation for a service
- Register a service in `Program.cs` and inject it into a controller
- Explain what the ASP.NET Core container does when a controller is created
- Choose between Transient, Scoped, and Singleton — and justify the choice
- Finish the Employee Management API with the controller → service structure

---

## The Problem

Here is the controller from Topic 04. Look at what it contains:

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private static readonly List<Employee> Employees = new()   // 1. the DATA
    {
        new Employee { Id = 1, Name = "Amira Benali", /* ... */ },
        // ...
    };

    [HttpGet]
    public IActionResult GetAll() => Ok(Employees);             // 2. the LOGIC

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)                        // 3. the HTTP work
    {
        var employee = Employees.FirstOrDefault(e => e.Id == id);
        return employee is null ? NotFound() : Ok(employee);
    }
}
```

Three jobs in one class. Now imagine:

- the data must come from a database (later lessons),
- a second controller needs the same employee lookup,
- the rules for "who counts as an employee" change.

You would end up editing — and duplicating logic in — **every controller**.

> This is exactly the problem **Dependency Injection** solves.

---

## What Is a Dependency?

A **dependency** is something a class needs in order to do its job.

```text
EmployeesController   needs   a way to read employees   → dependency
Printer               needs   paper                     → dependency
EmployeeService       needs   a data source             → dependency
```

In code, a dependency is usually another object passed to the class.

---

## Tight Coupling

The opposite of dependency injection: **creating your dependencies yourself**.

```csharp
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _service = new EmployeeService();   // ← I build it myself
}
```

| Problem | What goes wrong |
|---------|-----------------|
| Hidden dependency | Reading the constructor does not tell you what the class really needs — you must open every `new` |
| Cannot swap the implementation | The controller is welded to `EmployeeService`; another data source means editing the controller |
| Duplicated setup | Every controller repeats `new EmployeeService()` — and its configuration |
| Responsibilities mixed | The controller decides *which* implementation to use — that is a design decision, not HTTP work |

**Tight coupling** = a class is glued to a *specific* implementation instead of to a *contract*.

> 💡 **Senior Developer Note:** `new` inside a class is not evil — it becomes a problem when the object being created is a **collaborator with behaviour and configuration**, not a simple value.

---

## Dependency Injection — The Fix

**Dependency Injection (DI)** means: *the class receives the objects it depends on from the outside, instead of creating them itself.*

```text
BEFORE (tight coupling)

    EmployeesController
           │
           └── new EmployeeService()      ← controller decides & creates


AFTER (dependency injection)

    EmployeesController
           │  needs IEmployeeService (a contract)
           ▼
    DI container (ASP.NET Core creates it)
           │
           └── EmployeeService            ← the chosen implementation
```

Three participants:

| Participant | Role |
|-------------|------|
| **Consumer** | `EmployeesController` — asks for a *contract* |
| **Contract** | `IEmployeeService` — what the object can do |
| **Implementation** | `EmployeeService` — how it is done |
| **Container** | ASP.NET Core's built-in DI container — creates and delivers the objects |

The container is **not** a framework you install. `builder.Services` in `Program.cs` *is* it.

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
        new Employee { Id = 1, Name = "Amira Benali", Email = "amira@company.com", Department = "Engineering" },
        new Employee { Id = 2, Name = "Karim Haddad",  Email = "karim@company.com",  Department = "HR" },
        new Employee { Id = 3, Name = "Sara Naji",     Email = "sara@company.com",    Department = "Engineering" }
    };

    public IEnumerable<Employee> GetAll() => Employees;

    public Employee? GetById(int id) => Employees.FirstOrDefault(e => e.Id == id);
}
```

| | Interface `IEmployeeService` | Class `EmployeeService` |
|---|---|---|
| Answers | *What can be done?* | *How is it done?* |
| Contains | Method signatures | Data + method bodies |
| Depends on | Nothing | The data |
| Used by | The controller | The container |

> ⚠️ In-memory data with a `static` list is a **teaching device**: the list survives across requests only because it is `static`. Later lessons replace this service with a real database — the interface and the controller stay unchanged. That is the point of the contract.

---

## Constructor Injection

The controller declares what it needs in its constructor. The container fills it.

```csharp
using EmployeeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;      // 1. the field

    public EmployeesController(IEmployeeService employeeService)   // 2. the requirement
    {
        _employeeService = employeeService;                  // 3. the container's object
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_employeeService.GetAll());
    }

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
- ✅ does **only HTTP**: read the route, call the service, choose `200` or `404`.

---

## Service Registration

The container can only deliver what it has been told about. Registration lives in `Program.cs`, **before** `builder.Build()`:

```csharp
using EmployeeManagement.Api.Services;      // ← needed, or the types below do not compile

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();   // contract → implementation

var app = builder.Build();
// ... unchanged pipeline ...
```

| Half | Meaning |
|------|---------|
| `IEmployeeService` | The type your controller **asks for** |
| `EmployeeService` | The type the container **creates** |
| `AddScoped` | How long the object lives (next section) |

> ⚠️ The `using EmployeeManagement.Api.Services;` line is not decoration — without it, `IEmployeeService` is unknown and the build fails.

---

## How the Container Resolves a Request

```text
GET /api/employees/2
        │
        ▼
ROUTING  →  EmployeesController.GetById(2)
        │
        ▼
ASP.NET Core must CREATE the controller
        │
        ├─ reads the constructor: needs IEmployeeService
        ├─ looks in the registration list:  IEmployeeService  →  EmployeeService
        ├─ creates (or reuses, per lifetime) the EmployeeService
        └─ passes it to the constructor
        │
        ▼
GetById runs with a ready service  →  200 / 404
```

### When the registration is missing

Verified on .NET 8 — `IEmployeeService` was never registered:

```text
HTTP 500, and in the Development error page:

Unable to resolve service for type 'EmployeeManagement.Api.Services.IEmployeeService'
while attempting to activate 'EmployeeManagement.Api.Controllers.EmployeesController'.
```

Read that message like a senior developer:

| Part of the message | Meaning |
|---------------------|---------|
| `Unable to resolve service for type ...IEmployeeService` | The container has **no registration** for the interface |
| `while attempting to activate ...EmployeesController` | It happened while **creating the controller** |
| `HTTP 500` | The failure is on the **server**, not in the client's request |

**Fix:** add the missing `builder.Services.AddScoped<...>(...)` line.

> 💡 **Senior Developer Note:** With DI, most `500` errors you meet as a beginner are *registration* mistakes, not logic mistakes. Read the message first — it names the exact type.

---

## The Three Service Lifetimes

Registration has a third piece: **how long does the created object live?**

```csharp
builder.Services.AddTransient<IEmployeeService, EmployeeService>();  // every request asks → new object
builder.Services.AddScoped<IEmployeeService, EmployeeService>();     // one object per HTTP request
builder.Services.AddSingleton<IEmployeeService, EmployeeService>();  // one object for the whole application
```

In a web application, a **scope** is **one HTTP request**.

| Lifetime | How many instances | Created | Typical use |
|----------|--------------------|---------|-------------|
| **Transient** | A new one every time it is requested | Each request, each injection | Small, cheap, stateless objects |
| **Scoped** | One per HTTP request | Once per request, shared inside that request | Objects that hold *this request's* data |
| **Singleton** | One for the application's whole life | Once, at first use | Configuration, caches, objects shared by everyone |

### Verified experiment

A probe service that stamps each instance with a new GUID, registered under all three lifetimes and injected **twice** into one controller. Two consecutive requests produced (one real run — your GUIDs will differ):

```text
REQUEST 1
  transientA = 30a62bd6-…     transientB = 3e601ad9-…   ← two different instances
  scopedA    = 792bfd51-…     scopedB    = 792bfd51-…   ← ONE instance for this request
  singletonA = d8859ea7-…     singletonB = d8859ea7-…   ← ONE instance

REQUEST 2
  transientA = 58d39faf-…     transientB = 2123c092-…   ← new again
  scopedA    = 3164aaae-…     scopedB    = 3164aaae-…   ← one instance for THIS request
  singletonA = d8859ea7-…     singletonB = d8859ea7-…   ← unchanged from request 1
```

Read it carefully:

```text
Transient : new every injection, new every request
Scoped    : same object within one request — different object in the next request
Singleton : the very same object for the life of the application
```

### Choosing — with reasons, not slogans

| Question | If the answer is… | Choose |
|----------|-------------------|--------|
| Does the object keep data that belongs to *one* request? | Yes | **Scoped** |
| Is it tiny, cheap, and stateless? | Yes | **Transient** |
| Is it expensive to create and safe to share by everyone? | Yes | **Singleton** |
| Does it hold a mutable field that changes during a request? | Yes | **Never Singleton** — request 2 would read request 1's leftovers |

⚠️ There is no "best" lifetime:

- **Singleton is not "always better"** — a singleton shares its state with *every* concurrent request; that state must be safe to share, and it must never be request-specific.
- **Scoped is not "always faster"** — it is simply the right *visibility* for per-request data.
- **Transient is not "always safest"** — if creating the object is expensive (opening a connection, for example), creating thousands of them is a real cost.

> 💡 **Senior Developer Note:** Pick the lifetime from **what the object holds**, not from performance guesswork. In the following lessons you will meet a service that *must* be Scoped because it represents one unit of work — the pattern stays the same.

---

## The Finished Employee Management API

### The four files

```text
EmployeeManagement.Api/
├── Controllers/
│   └── EmployeesController.cs       HTTP only: read route → call service → pick status
├── Models/
│   └── Employee.cs                  the data type
├── Services/
│   ├── IEmployeeService.cs          the contract
│   └── EmployeeService.cs           in-memory implementation
├── Program.cs                       registration + pipeline
└── ...
```

### `Program.cs` — the final version

```csharp
using EmployeeManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
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

### The complete request flow

```text
Client
  │
  ▼
HTTP Request          GET /api/employees/2
  │
  ▼
ASP.NET Core          Kestrel receives it
  │
  ▼
Middleware Pipeline   routing matches → middleware runs → endpoint executes
  │
  ▼
Routing               api/[controller] + {id:int} + GET  →  GetById(2)
  │
  ▼
Controller            EmployeesController.GetById(int id)
  │
  ▼
Dependency Injection  container creates/reuses IEmployeeService → EmployeeService
  │
  ▼
Service               EmployeeService.GetById(2)  →  finds / does not find
  │
  ▼
HTTP Response         200 + JSON   or   404
  │
  ▼
Client
```

That is the whole day, in one picture.

---

## 🧪 Exercise 1 — Extract a Service (Departments)

**Exercise files:** [Exercise 02 — Dependency Injection](Exercises/Exercise-02-Dependency-Injection/) (Starter + Solution)

Take your `DepartmentsController` from Topic 04, Exercise 1 and move everything except HTTP out of it.

1. Create `Services/IDepartmentService.cs`:

```csharp
using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Services;

public interface IDepartmentService
{
    IEnumerable<Department> GetAll();
    Department? GetById(int id);
}
```

2. Create `Services/DepartmentService.cs` and **move the list and the lookup code** into it.
3. Register it in `Program.cs` as **Scoped**, including the `using` line.
4. Replace the controller body with a constructor and two calls.
5. Test the three URLs again: `200`, `200`, `404`.

**Success criteria:** the controller contains no `List<>`, no `FirstOrDefault`, and no data — only routing, calls, and status codes.

---

## 🧪 Exercise 2 — Choose the Lifetime

For each case, pick **Transient**, **Scoped**, or **Singleton** and give your reason in one sentence.

1. An object that calculates a bonus from two numbers. Stateless, no fields.
2. An object that holds "the id of the request currently being processed".
3. An object that keeps a reference table (countries, currency rates) loaded once and read by every request.
4. An object with a field `private string _currentRequestId;` filled during the request — registered as **Singleton**.

<details>
<summary><b>Answers</b></summary>

1. **Transient** — cheap, stateless; a new tiny object per use costs nothing.
2. **Scoped** — it belongs to exactly one request; a new scope per request guarantees isolation.
3. **Singleton** — created once, shared by everyone, and it holds no request data.
4. **Wrong choice** — a Singleton must never hold per-request state: concurrent requests would overwrite each other's `Id`. Use **Scoped**.

</details>

---

## 🧪 Exercise 3 — Debug: HTTP 500 on Every Request

Your team's API worked yesterday. Today every endpoint returns `500`. The Development error page shows:

```text
Unable to resolve service for type 'EmployeeManagement.Api.Services.IDepartmentService'
while attempting to activate 'EmployeeManagement.Api.Controllers.DepartmentsController'.
```

1. What exactly is missing?
2. Which file do you open?
3. Which line do you add?
4. What is the *second* possible cause if that line already exists?

<details>
<summary><b>Answers</b></summary>

1. The container has no registration for `IDepartmentService`.
2. `Program.cs`.
3. `builder.Services.AddScoped<IDepartmentService, DepartmentService>();` (before `builder.Build()`), plus `using EmployeeManagement.Api.Services;`.
4. The registration exists but registers the **concrete** type only — `AddScoped<DepartmentService>()` — while the controller asks for the **interface**. The container can only deliver what was registered: register the pair (contract → implementation).

</details>

---

## 🧪 Exercise 4 — Read the Code (Code Reading)

Given:

```csharp
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IRequestContext, RequestContext>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
```

One controller constructor injects all three. Answer:

1. How many `SystemClock` instances exist while the application runs, after 1000 requests?
2. How many `RequestContext` instances exist for one request?
3. Two actions of the *same* request use `IEmailSender` — how many instances do they see?

<details>
<summary><b>Answers</b></summary>

1. **One** — Singleton creates one instance for the application's life (assuming it is resolved at least once).
2. **One per request** — all injections inside that request receive the same instance.
3. Each injection gets its own instance — Transient creates one every time it is requested.

</details>

---

## ⚠️ Common Mistakes

| # | Mistake | Symptom | Fix |
|---|---------|---------|-----|
| 1 | `new MyService()` inside the controller | Hidden dependency, implementation welded in | Inject the interface |
| 2 | Forgetting the registration | `500`: *unable to resolve service* | Add it to `builder.Services` before `Build()` |
| 3 | Registering the concrete type but injecting the interface | Same `500`, same message | Register `AddScoped<IService, Service>()` |
| 4 | Registering after `builder.Build()` | The registration is not part of the built container | Register before `Build()` |
| 5 | Singleton holding request data | Data leaks between requests | Scoped, or remove the state |
| 6 | Circular dependency (`A` needs `B`, `B` needs `A`) | The container cannot build the graph | Redesign: extract a third collaborator |
| 7 | Interface with a single implementation nobody ever swaps | Ceremony without benefit | Add interfaces where they hide a real choice (data source, protocol, cost) |
| 8 | Business rules left in the controller | Fat endpoints, duplicated logic | Move them into the service |
| 9 | Choosing lifetimes by folklore | Subtle state bugs later | Decide from what the object holds |

---

## 💡 Senior Developer Notes

- Prefer dependency injection over manually constructing dependencies inside controllers.
- Controllers should depend on **contracts**, not on concrete classes.
- Keep controllers focused on HTTP concerns — routing, status codes, and calling one service.
- A service is the right home for logic that must stay the same whether it is called from a controller or from another part of the application.
- Understand lifetimes before adding complexity: the lifetime decides what an object can safely remember.
- Do not introduce architectural patterns before the fundamentals are solid — `Controller → Service → (database later)` carries you a long way.

---

## 🧠 Knowledge Check

### Question 1

What is a dependency, in one sentence?

**Answer:** Something a class needs in order to do its job — in practice, another object handed to it.

---

### Question 2

Why is `private readonly EmployeeService _service = new EmployeeService();` a problem in a controller?

**Answer:** The controller creates its own dependency, so it is tied to one implementation, the dependency is hidden, and the choice of implementation is made in the wrong place. Injection moves that decision to registration in `Program.cs`.

---

### Question 3

Where do you register a service?

**Answer:** In `Program.cs`, in `builder.Services`, before `builder.Build()`.

---

### Question 4

What is the difference between `IEmployeeService` and `EmployeeService`?

**Answer:** The interface is the contract the controller depends on; the class is the implementation the container creates and delivers.

---

### Question 5

Which lifetime gives one object per HTTP request?

**Answer:** Scoped.

---

### Question 6

A service holds a counter of "requests processed so far" and is registered as Singleton. What is the risk?

**Answer:** Every request shares — and mutates — the same object, so counts can interleave under concurrent requests, and request-specific state bleeds across requests.

---

### Question 7

Your endpoint returns `500` with *"Unable to resolve service for type X"*. Where do you look?

**Answer:** `Program.cs` — the registration for `X` is missing, or it registers a different type than the one being injected.

---

## ✅ Check Yourself

- [ ] I can explain dependency, tight coupling, and dependency injection in my own words
- [ ] I created an interface and an implementation for the Employee service
- [ ] I registered the service in `Program.cs` with the right `using` line
- [ ] I injected the service through the constructor and removed all data from the controller
- [ ] I can explain what the container does while creating a controller
- [ ] I can compare Transient, Scoped, and Singleton without slogans
- [ ] I can diagnose a missing registration from the error message

---

## Summary

| Concept | What to remember |
|---------|------------------|
| Dependency | An object a class needs to work |
| Tight coupling | The class creates its own collaborator |
| Dependency Injection | The collaborator is provided from outside |
| Interface | The contract the consumer depends on |
| Implementation | The class the container creates |
| Registration | `builder.Services.AddScoped<IService, Service>()` before `Build()` |
| Constructor injection | The standard way to receive dependencies |
| Container | ASP.NET Core's built-in DI — reads registrations and creates objects |
| Transient | New object every time it is requested |
| Scoped | One object per HTTP request |
| Singleton | One object for the application's life |
| Rule | Choose the lifetime from what the object holds |

---

## 🔜 Back to the Day

Today's five topics are one chain:

```text
HTTP + REST   →  the language both sides speak
ASP.NET Core  →  the server that answers
Project       →  where everything lives
Controllers   →  which C# code runs
DI            →  which objects it uses
```

**Next: [Day 1 Overview & Final Challenge](Day-1-Web-and-ASP.NET-Core.md)** — then try the final challenge without looking at the lesson.

