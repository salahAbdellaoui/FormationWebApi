# 04 — Controllers and Routing

---

## 🎯 What You Will Learn (55 min)

By the end you can:

- Explain what a controller is and what an action is
- Create a controller that answers `GET /api/employees`
- Add a second endpoint `GET /api/employees/{id}`
- Predict which URL reaches which action
- Debug four common routing mistakes

---

## The Problem

A request arrives:

```http
GET /api/employees/2
```

Which C# method should run? And how does the framework know?

---

## What Is a Controller?

A controller is a class that:

1. receives an HTTP request,
2. calls the logic it needs,
3. returns an HTTP response.

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    // actions (public methods) live here
}
```

| Term | Meaning |
|------|---------|
| **Controller** | The class, name ends with `Controller` |
| **Action** | A public method of the controller |
| **Endpoint** | One URL + one HTTP method → one action |

> 💡 **Senior Developer Note:** A controller is a **delivery mechanism**. Its job is HTTP — read the request, call the logic, choose a status code, return the body. Keep business logic out of it.

---

## `ControllerBase` and `[ApiController]`

### Why `ControllerBase`

`ControllerBase` gives you the HTTP helpers:

```text
Ok()  NotFound()  BadRequest()  StatusCode()
Request  HttpContext
```

Do **not** inherit `Controller` — it adds view rendering (MVC pages). APIs return data, not views.

### What `[ApiController]` does

It switches on two behaviours (verified on .NET 8):

1. **Body binding.** A complex parameter is bound from the JSON body automatically.
2. **Automatic 400.** A malformed body returns `400` before your action runs.

| Request | Result with `[ApiController]` |
|---------|------------------------------|
| POST with JSON body | `200`, body read correctly |
| POST with no body | `400` automatically |
| POST with broken JSON | `400` automatically |

---

## Route Attributes

### `[Route]` — the base path

```csharp
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
```

`[controller]` is a **token**. It is replaced with the class name without `Controller`:

```text
EmployeesController  →  employees
```

So the base path is `api/employees`.

### HTTP method attributes — one per action

```csharp
[HttpGet]      // read
[HttpPost]     // create (Day 2)
[HttpPut]      // replace (Day 2)
[HttpDelete]   // remove (Day 2)
```

An attribute with no value means *"this action answers on the controller's base path"*.

---

## Your First Endpoint

### The Model

```csharp
// Models/Employee.cs
namespace EmployeeManagement.Api.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}
```

### The Controller

```csharp
// Controllers/EmployeesController.cs
using EmployeeManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private static readonly List<Employee> Employees = new()
    {
        new Employee { Id = 1, Name = "Amira", Department = "Engineering" },
        new Employee { Id = 2, Name = "Karim", Department = "HR" }
    };

    [HttpGet]
    public IActionResult GetAll() => Ok(Employees);
}
```

### Run it

```bash
dotnet run
curl http://localhost:5189/api/employees
```

Response:

```json
[
  { "id": 1, "name": "Amira", "department": "Engineering" },
  { "id": 2, "name": "Karim", "department": "HR" }
]
```

### What just happened

```text
curl /api/employees
        ↓
Routing: api/[controller] → api/employees, GET → GetAll()
        ↓
Controller returns Ok(Employees)
        ↓
Framework serializes to JSON, status 200
        ↓
curl receives the JSON array
```

---

## Route Parameters

To read one employee:

```csharp
[HttpGet("{id:int}")]
public IActionResult GetById(int id)
{
    var employee = Employees.FirstOrDefault(e => e.Id == id);
    return employee is null ? NotFound() : Ok(employee);
}
```

```text
GET /api/employees/2
                  └┬┘
              {id:int}  →  id = 2  →  GetById(2)
```

### The One Rule That Gets Broken

> ⚠️ The **parameter name must match the placeholder name**.

If you write:

```csharp
[HttpGet("{id:int}")]
public IActionResult GetById(int employeeId)   // ← mismatch
```

The URL value never reaches your parameter. You get `0` instead of `2`, and no error. The bug is invisible.

### Route constraints

A constraint validates the URL segment **before** the action runs.

| Constraint | Verified behaviour |
|------------|--------------------|
| `{id:int}` | `GET /api/employees/abc` → `404` (the route does not match) |
| `{id:min(1)}` | `GET .../0` → `404` |

Without the constraint, `abc` reaches the action as `0` and your code has to defend itself.

---

## `IActionResult` — Choosing the Response

```csharp
[HttpGet("{id:int}")]
public IActionResult GetById(int id)
{
    var employee = Employees.FirstOrDefault(e => e.Id == id);
    return employee is null ? NotFound() : Ok(employee);
}
```

| Helper | Status | Body |
|--------|--------|------|
| `Ok(value)` | `200` | JSON of `value` |
| `NotFound()` | `404` | A JSON error description |
| `BadRequest()` | `400` | A JSON error description |
| `StatusCode(503)` | `503` | A JSON error description |

> 💡 **Senior Developer Note:** The status code is part of your API contract. A `200` for every outcome forces every client to parse your body to discover what happened.

---

## 🐛 Debugging Challenge 1 — Everything Returns 404

**Situation.** Your controller compiles. The URL `/api/employees` looks right. Every request returns `404` with an empty body.

**What to inspect.** Does `Program.cs` contain `app.MapControllers()`?

**Root cause.** Verified: without `app.MapControllers()`, the application **starts normally** but no routes are published.

**Fix.**

```csharp
app.MapControllers();
```

---

## 🐛 Debugging Challenge 2 — The URL Looks Right, But 404

**Situation.**

```csharp
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    [HttpGet("api/employees")]          // ← the developer "made sure"
    public IActionResult GetAll() => Ok(Employees);
}
```

Client calls `GET /api/employees` → `404`.

**Root cause.** Route segments are **combined**, not replaced:

```text
api/[controller]  +  api/employees
      ↓                    ↓
api/employees     +  api/employees
      =
api/employees/api/employees     ← the real URL
```

**Fix.**

```csharp
[HttpGet]     // no prefix — the controller route already provides it
```

---

## 🐛 Debugging Challenge 3 — The Endpoint Answers, But With the Wrong Value

**Situation.**

```csharp
[HttpGet("{id:int}")]
public IActionResult GetById(int employeeId)
{
    // employeeId is always 0, even for /api/employees/2
}
```

**Root cause.** Names must match. With `{id}` in the route and `employeeId` in the method, the URL value is never handed over.

**Fix.**

```csharp
[HttpGet("{id:int}")]
public IActionResult GetById(int id)
```

---

## 🐛 Debugging Challenge 4 — 405 Instead of 404

**Situation.** `GET /api/employees` works. `POST /api/employees` returns:

```http
HTTP/1.1 405 Method Not Allowed
Allow: GET
```

**Root cause.** The route **matched** (the URL is right), but no action accepts `POST`. Today's API only implements `GET`.

**Fix.** The client should use `GET`. Or, on Day 2, add a `[HttpPost]` action.

---

## 🧪 Exercise 1 — Build the Departments Endpoints

Add `Models/Department.cs`:

```csharp
namespace EmployeeManagement.Api.Models;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

Create `Controllers/DepartmentsController.cs` so that:

| Method | URL | Result |
|--------|-----|--------|
| `GET` | `/api/departments` | `200` + the list |
| `GET` | `/api/departments/{id}` | `200` + one department, or `404` |

Requirements: base route with `[controller]`, a constraint on the id, in-memory data, `IActionResult`.

---

## 🧪 Exercise 2 — Find the Three Bugs

This controller compiles. Every request returns `404`. Find the three problems.

```csharp
[ApiController]
[Route("api/employee")]
public class EmployeesController : ControllerBase
{
    private static readonly List<Employee> Employees = new() { };

    [HttpGet("api/employees")]
    public IActionResult GetAll() => Ok(Employees);

    [HttpGet("{id:int}")]
    public IActionResult GetById(int employeeId) => Ok(Employees[0]);
}
```

<details>
<summary><b>Answers</b></summary>

1. `[Route("api/employee")]` — singular. Clients expect `/api/employees` (plural).
2. `[HttpGet("api/employees")]` — combined with the controller route, the real path is `/api/employee/api/employees`.
3. `int employeeId` vs `{id}` — names do not match, so the URL value never arrives.

</details>

---

## 🧠 Knowledge Check

### Question 1

What does the `[controller]` token resolve to in `EmployeesController`?

**Answer:** `employees` — the class name without `Controller`. Base path: `api/employees`.

---

### Question 2

Why does `GET /api/employees/abc` return `404` when the action exists?

**Answer:** The action has `{id:int}`. The segment `abc` fails the constraint, so the route does not match and the action never runs.

---

### Question 3

`GET /api/employees` works. `POST /api/employees` returns `405`. What does that mean?

**Answer:** The URL matched, but no action accepts `POST`. The `Allow` header lists the accepted methods.

---

### Question 4

Name the difference between `NotFound()` and `NotFound("employee does not exist")`.

**Answer:** Both return `404`. The first produces a JSON error description; the second returns your own text as the body.

---

## ✅ Check Yourself

- [ ] I can explain the difference between a controller, an action, and an endpoint
- [ ] I know why we use `ControllerBase` and `[ApiController]`
- [ ] I can predict the URL of an action from its attributes
- [ ] I used a route parameter and a route constraint correctly
- [ ] I know why the parameter name must match the placeholder
- [ ] I can pick the right response helper for success, missing, and invalid

---

**Next: [05 — Dependency Injection](05-Dependency-Injection.md)**
