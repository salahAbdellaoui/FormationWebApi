# 04 — Controllers and Routing

---

## 🎯 Learning Objectives

By the end of this topic, you will be able to:

- Explain what a controller is, what an action is, and what belongs in them
- Use `[ApiController]`, `[Route]`, and the HTTP method attributes correctly
- Predict which URL and method reach which action
- Use route parameters and route constraints
- Choose the right response helper and status code for each situation
- Debug the four routing problems every beginner meets: missing mapping, wrong route, wrong method, wrong parameter name

---

## The Problem

Topic 02 ended with this line:

```csharp
app.MapControllers();
```

It publishes "controllers", but we have never written one. So today: **which C# code runs when a request arrives, and how does the URL decide?**

```text
GET /api/employees/2     →   ???
```

The answer has two halves:

```text
ROUTING      URL + HTTP method   →   which action
CONTROLLER   that action         →   the response
```

---

## What Is a Controller?

A **controller** is a class that:

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

| Term | Meaning | Example |
|------|---------|---------|
| **Controller** | The class, name ends with `Controller` | `EmployeesController` |
| **Action** | A public method of the controller | `GetAll()` |
| **Endpoint** | One URL + one HTTP method + one action | `GET /api/employees` → `GetAll()` |
| **Route** | The URL pattern the action answers to | `api/employees/{id}` |

Where it lives: the `Controllers/` folder — the convention the template set up in Topic 03.

> 💡 **Senior Developer Note:** A controller is a **delivery mechanism**. Its job is HTTP: read the request, pick the status code, hand back the body. Business rules belong behind it (Topic 05). When a controller starts deciding prices, discounts, or permissions, it has grown too fat.

---

## `ControllerBase` and `[ApiController]`

### Why `ControllerBase`

```csharp
public class EmployeesController : ControllerBase
```

`ControllerBase` gives you the HTTP helpers you will use all week:

```text
Ok()  NotFound()  BadRequest()  Conflict()  StatusCode()  Created(...)
Request  HttpContext  ModelState
```

We do **not** inherit from `Controller` — that class adds view support (rendering HTML pages). An API returns data, not views.

### Why `[ApiController]`

`[ApiController]` switches on two behaviours that ASP.NET Core applies to every action of the class. Both were verified on .NET 8:

**1. The request body is bound automatically.**

| Action parameter | With `[ApiController]` | Without `[ApiController]` |
|------------------|------------------------|---------------------------|
| `Create(ItemDto dto)` + JSON body `{"name":"Laptop"}` | `dto.Name` = `"Laptop"` — the JSON body was read | `dto.Name` = `null` — the JSON body was ignored |

**2. Invalid requests are rejected before your action runs.**

| Request | Result with `[ApiController]` |
|---------|------------------------------|
| `POST` with a JSON body | `200`, body read correctly |
| `POST` with **no** body | `400` with a JSON error describing the problem |
| `POST` with broken JSON | `400` with a JSON error describing the problem |

Verified body for the missing-body case:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "": ["A non-empty request body is required."],
    "dto": ["The dto field is required."]
  }
}
```

> ⚠️ Without `[ApiController]`, the same missing body silently reaches your action as an empty object. You will only discover it in production, when "creation" starts storing empty records.

---

## Routing Attributes

### `[Route]` — the base path of the controller

```csharp
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
```

`[controller]` is a **route token**: ASP.NET Core replaces it with the controller name **without** the `Controller` suffix.

```text
EmployeesController   →   employees
```

So the base path is:

```text
/api/employees
```

Verified: a request to `/api/employees` reaches this controller. Routing is also **case-insensitive** — `/api/Employees` returns the same result.

### HTTP method attributes — one action per method

```csharp
[HttpGet]      // read
[HttpPost]     // create
[HttpPut]      // replace
[HttpPatch]    // update partly
[HttpDelete]   // remove
```

An attribute without a value means *"this action answers on the controller's base path"*:

```csharp
[HttpGet]                    // GET  /api/employees
[HttpGet("{id:int}")]        // GET  /api/employees/5
[HttpPost]                   // POST /api/employees
```

> 💡 **Senior Developer Note:** The URL says **which resource**, the attribute says **which method**. One URL + one method must map to exactly one action. If two actions answer the same pair, the request fails with a verified framework error: `AmbiguousMatchException: The request matched multiple endpoints.` Your controller compiles — the failure only appears at request time.

---

## Your First Endpoint

Create `Controllers/EmployeesController.cs` with in-memory data (no database today):

```csharp
using EmployeeManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private static readonly List<Employee> Employees = new()
    {
        new Employee { Id = 1, Name = "Amira Benali", Email = "amira@company.com", Department = "Engineering" },
        new Employee { Id = 2, Name = "Karim Haddad",  Email = "karim@company.com",  Department = "HR" },
        new Employee { Id = 3, Name = "Sara Naji",     Email = "sara@company.com",    Department = "Engineering" }
    };

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(Employees);
    }
}
```

(The `Employee` type is the one you created in Topic 03, Exercise 2.)

Run it and call it:

```bash
dotnet run
curl http://localhost:5189/api/employees
```

Verified response:

```json
[
  { "id": 1, "name": "Amira Benali", "email": "amira@company.com", "department": "Engineering" },
  { "id": 2, "name": "Karim Haddad", "email": "karim@company.com", "department": "HR" },
  { "id": 3, "name": "Sara Naji", "email": "sara@company.com", "department": "Engineering" }
]
```

### From URL to action, step by step

```text
GET /api/employees
     │
     ▼
[1] ROUTING
      controller route : api/[controller]   →  api/employees
      action route     : [HttpGet]          →  method GET on that path
      match found?     : yes
     │
     ▼
[2] CONTROLLER  EmployeesController.GetAll()
     │
     ▼
[3] RESPONSE    Ok(Employees)  →  200 + JSON
```

---

## Route Parameters

The URL carries data: `/api/employees/2`. That `2` must reach your action.

```csharp
[HttpGet("{id:int}")]
public IActionResult GetById(int id)
{
    var employee = Employees.FirstOrDefault(e => e.Id == id);

    if (employee is null)
    {
        return NotFound();
    }

    return Ok(employee);
}
```

```text
GET /api/employees/2
                 └┬┘
            {id:int}  →  id = 2   →  GetById(2)
```

### The one rule that is silently broken most often

> ⚠️ The **action parameter name must match the route placeholder name**.

Verified experiment — the placeholder is `{id}` but the parameter is `employeeId`:

| Request | What the action receives |
|---------|--------------------------|
| `GET .../5` | `employeeId = 0` ← the `5` never arrives |
| `GET .../5?employeeId=42` | `employeeId = 42` ← it came from the **query string** |
| `GET .../abc` | `employeeId = 0` ← no constraint, so the route matched anyway |

No error, no crash — just **wrong data**. This is debugging challenge 3 below.

### Route constraints

A constraint validates the URL segment **before** the action runs.

| Constraint | Accepts | Verified result for the wrong value |
|------------|---------|--------------------------------------|
| `{id:int}` | integers | `/api/employees/abc` → `404` (route does not match) |
| `{id:min(1)}` | integers ≥ 1 | `.../min/0` → `404`, `.../min/abc` → `404` |
| `{code:alpha}` | letters only | `.../alpha/A1` → `404` |
| `{id:guid}` | GUID values | `.../guid/notaguid` → `404` |

Why constraints matter: **a wrong URL fails at routing with a `404`, instead of reaching your action with a default value like `0`.**

```text
Without constraint:   /api/employees/abc  →  action runs with id = 0  →  you must defend yourself
With {id:int}:        /api/employees/abc  →  404, your action never runs
```

---

## `IActionResult` — Choosing the Response

`IActionResult` means: *"I will decide which status code and which body this response gets."*

Each branch of your action returns a different result:

```csharp
[HttpGet("{id:int}")]
public IActionResult GetById(int id)
{
    var employee = Employees.FirstOrDefault(e => e.Id == id);

    if (employee is null)
    {
        return NotFound();     // 404 — the client asked for something that does not exist
    }

    return Ok(employee);       // 200 — here it is
}
```

Verified behaviour of the helpers in a .NET 8 API:

| Helper | Status | Body | Example |
|--------|--------|------|---------|
| `Ok()` | `200` | empty | — |
| `Ok(value)` | `200` | JSON of `value` | `Ok(Employees)` |
| `NotFound()` | `404` | a JSON error description | `GET /api/employees/999` |
| `NotFound("custom")` | `404` | `custom` | when you need your own text |
| `BadRequest()` | `400` | a JSON error description | invalid input |
| `Conflict(object)` | `409` | JSON of the object | duplicate resource |
| `StatusCode(503)` | `503` | a JSON error description | unusual cases |

You can also return the value directly, without `IActionResult`:

```csharp
[HttpGet]
public IEnumerable<Employee> GetAll() => Employees;   // automatically 200 + JSON
```

The template's own sample action does exactly this. Choose:

- **return the value** when the answer is always `200`,
- **return `IActionResult`** when the status code depends on the data (not found, invalid, conflict …).

> 💡 **Senior Developer Note:** The status code is part of your API contract (Topic 01). `200` for everything forces every client to parse your body to discover that something failed.

---

## The Complete Picture

```text
Client:  GET /api/employees/2
              │
              ▼
         ROUTING
           api/[controller] + {id:int} + GET   →   EmployeesController.GetById(2)
              │
              ▼
         CONTROLLER
           reads id = 2, looks for the employee
              │
              ├── found   → Ok(employee)   →  200 + JSON
              └── missing → NotFound()     →  404
              │
              ▼
         RESPONSE travels back through the pipeline to the client
```

---

## 🐛 Debugging Method

Use the same order every time — never change code at random:

```text
Observe      what is the exact status code and body?
     ↓
Reproduce    the same request every time?
     ↓
Inspect      route?  method?  mapping?  parameter name?
     ↓
Hypothesis   "I think the route never matched"
     ↓
Test         prove or disprove it with one change
     ↓
Fix          change the smallest thing possible
     ↓
Retest       the original request must now work
```

---

## 🐛 Debugging Challenge 1 — Everything Returns 404

**Situation.** `EmployeesController` exists with `[HttpGet]` and `[Route("api/[controller]")]`. The URL `http://localhost:5189/api/employees` looks perfect. Every request returns `404` with an **empty body**.

**What to investigate.**

1. Does `Program.cs` contain `app.MapControllers()`?
2. Does `builder.Services.AddControllers()` still exist?
3. Is the request really going to the port the console printed?

**Root cause.** The endpoints were never published. Our verified test: with `app.MapControllers()` commented out, the application **starts normally** and answers every request with:

```http
HTTP/1.1 404 Not Found
Content-Length: 0
```

**Fix.**

```csharp
app.MapControllers();
```

**Why this works.** Controllers are plain classes. Nothing in C# tells ASP.NET Core that they answer HTTP requests. `MapControllers()` reads the route attributes and publishes endpoints — that is the bridge between "a class" and "a URL".

**Variant — a missing `AddControllers()`.** If you comment out `builder.Services.AddControllers()` instead, the application does not start at all. Verified startup error:

```text
System.InvalidOperationException: Unable to find the required services. Please add
all the required services by calling 'IServiceCollection.AddControllers' inside the
call to 'ConfigureServices(...)' in the application startup code.
```

That is your clue: **registration is missing** (Topic 02, Part 1), not routing.

---

## 🐛 Debugging Challenge 2 — The URL Looks Right, But 404

**Situation.** The route is written like this:

```csharp
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    [HttpGet("api/employees")]          // ← the developer "made sure"
    public IActionResult GetAll() => Ok(Employees);
}
```

The client calls `GET /api/employees` and receives `404`.

**What to investigate.** Build the full path: *controller route* + *action route*.

**Root cause.** Route segments are **combined**, not replaced:

```text
api/[controller]   +   api/employees
      ↓                     ↓
api/employees      +   api/employees
      =
api/employees/api/employees      ← the real URL
```

Verified: a request to `/api/employees/api/employees` returns `200`; the expected `/api/employees` returns `404`.

**Fix.**

```csharp
[HttpGet]                 // no prefix — the controller route already provides it
public IActionResult GetAll() => Ok(Employees);
```

**Why this works.** `[Route]` on the controller is the **base path**. An action route is a **relative addition** to it. Write only what is extra (`{id}`, a sub-resource name, …).

---

## 🐛 Debugging Challenge 3 — The Endpoint Answers, But With Wrong Data

**Situation.** The action runs, returns `200`… and the value is always `0`. No exception, no log.

```csharp
[HttpGet("{id:int}")]
public IActionResult GetById(int employeeId)      // ← placeholder is {id}
{
    // employeeId is 0 even for /api/employees/5
}
```

**What to investigate.** Compare the placeholder in the attribute with the parameter name in the signature.

**Root cause.** Names must match. Verified: with `{id}` in the route and `employeeId` in the method, the URL value is never handed over — the parameter keeps its default value `0`, and a `?employeeId=42` query value would be used instead.

**Fix.** Make them identical:

```csharp
[HttpGet("{id:int}")]
public IActionResult GetById(int id)
```

**Why this works.** Routing binds values **by name**. There is no positional matching, and the compiler cannot catch this because both names are individually valid.

---

## 🐛 Debugging Challenge 4 — 405 Instead of 404

**Situation.** `GET /api/employees` works. The same URL with `POST` answers:

```http
HTTP/1.1 405 Method Not Allowed
Allow: GET
```

**What to investigate.** Is this a URL problem or a method problem? Read the status code and the `Allow` header.

**Root cause.** The route **matched** (so the path is correct), but no action accepts `POST`. Today's API only implements `GET`.

**Fix.** Depends on intent:

- the client used the wrong method → the client sends `GET`;
- the endpoint should really accept `POST` → add a `[HttpPost]` action (Day 2).

**Why this works.** `405` means "you found the right door, but it only opens that way". `404` with an empty body means "there is no door here at all".

---

## 🧪 Exercise 1 — Build the Departments Endpoints

Add a `Department` type in `Models/Department.cs`:

```csharp
namespace EmployeeManagement.Api.Models;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}
```

Then create `Controllers/DepartmentsController.cs` so that:

| Method | URL | Result |
|--------|-----|--------|
| `GET` | `/api/departments` | `200` + the list |
| `GET` | `/api/departments/{id}` | `200` + one department, or `404` |

Requirements: base route with `[controller]`, a constraint on the id, in-memory data, `IActionResult`.

Test all three cases with `curl` and paste the status codes:

```bash
curl -i http://localhost:5189/api/departments
curl -i http://localhost:5189/api/departments/1
curl -i http://localhost:5189/api/departments/99
```

<details>
<summary><b>Solution</b></summary>

```csharp
using EmployeeManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private static readonly List<Department> Departments = new()
    {
        new Department { Id = 1, Name = "Engineering", Location = "Building A" },
        new Department { Id = 2, Name = "HR",          Location = "Building B" },
        new Department { Id = 3, Name = "Finance",     Location = "Building A" }
    };

    [HttpGet]
    public IActionResult GetAll() => Ok(Departments);

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var department = Departments.FirstOrDefault(d => d.Id == id);

        return department is null ? NotFound() : Ok(department);
    }
}
```

Expected: `200`, `200`, `404`.

</details>

---

## 🧪 Exercise 2 — Predict the Route (Before Running)

For each action, write the exact URL + method. Then mark which request returns `404`.

```csharp
[ApiController]
[Route("api/[controller]")]                   // A
public class PositionsController : ControllerBase
{
    [HttpGet]                                 // B
    public IActionResult GetAll() => Ok();

    [HttpGet("{id:int}")]                     // C
    public IActionResult GetById(int id) => Ok();

    [HttpGet("by-title/{title}")]             // D
    public IActionResult GetByTitle(string title) => Ok();
}
```

| Request | Reaches an action? | Which one? |
|---------|--------------------|------------|
| `GET /api/positions` | | |
| `GET /api/positions/3` | | |
| `GET /api/positions/3?title=Dev` | | |
| `GET /api/positions/by-title/Dev` | | |
| `POST /api/positions` | | |
| `GET /api/positions/abc` | | |

<details>
<summary><b>Answers</b></summary>

| Request | Reaches an action? | Which one? |
|---------|--------------------|------------|
| `GET /api/positions` | Yes | `GetAll` (B) |
| `GET /api/positions/3` | Yes | `GetById` (C), `id = 3` |
| `GET /api/positions/3?title=Dev` | Yes | `GetById` — the query value is ignored by this action |
| `GET /api/positions/by-title/Dev` | Yes | `GetByTitle` (D), `title = "Dev"` |
| `POST /api/positions` | **No** | — → `405`, `Allow: GET` |
| `GET /api/positions/abc` | **No** | — → `404`, empty body (`{id:int}` rejects it) |

*(Why not `by-title/Dev` for `GetById`? That URL has two segments after `positions`, while `{id:int}` covers only one — so only `GetByTitle` can match.)*

</details>

---

## 🧪 Exercise 3 — Find the Three Bugs

This controller compiles, but every request returns `404`. Find **three** separate problems.

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

1. **`[Route("api/employee")]`** — singular. The client calls `/api/employees` (plural, Topic 01). The real base path is `/api/employee`.
2. **`[HttpGet("api/employees")]`** — the action route is appended, so the real path is `/api/employee/api/employees`.
3. **`int employeeId` vs `{id}`** — names do not match, so the id from the URL never reaches the action.

Bonus: `Ok(Employees[0])` would throw for an empty list — always check for "not found" first.

</details>

---

## ⚠️ Common Mistakes

| # | Mistake | Symptom | Fix |
|---|---------|---------|-----|
| 1 | No `app.MapControllers()` | `404` for everything, empty body | Add the mapping |
| 2 | No `builder.Services.AddControllers()` | The app fails at startup | Add the registration |
| 3 | Repeating the prefix in `[HttpGet]` | `404` on the "obvious" URL | Action routes are relative to the controller route |
| 4 | Parameter name ≠ placeholder name | Silent `0` / wrong records | Make the names identical |
| 5 | Forgetting `{id:int}` | Non-numeric URLs reach your action | Add the constraint |
| 6 | No `[ApiController]` | Bodies not read, no automatic `400` | Add the attribute to every API controller |
| 7 | Inheriting `Controller` | View-related members you will never use | Inherit `ControllerBase` |
| 8 | Putting business rules in the controller | Untestable, bloated endpoints | Keep controllers thin (Topic 05) |
| 9 | Returning `200` for failures | Clients cannot tell success from failure | `404`, `400`, `409`, `500` |
| 10 | Changing a route casually | Existing clients break | Routes are a contract |

---

## 💡 Senior Developer Notes

- Keep controllers focused on HTTP concerns: read input, call one service, choose a status code, return.
- Design routes before writing actions — and treat them as a contract with your clients.
- One URL + one HTTP method must map to exactly one action. If it is ambiguous to you, it is ambiguous to ASP.NET Core.
- Prefer constraints over defensive code: `{id:int}` rejects bad URLs before your action runs.
- Debug with evidence: status code, body, `Allow` header, console log. Never by rewriting random lines.

---

## 🧠 Knowledge Check

### Question 1

What does the `[controller]` token in `[Route("api/[controller]")]` resolve to for `EmployeesController`?

**Answer:** `employees` — the class name without the `Controller` suffix. The full base path is `api/employees`.

---

### Question 2

A request to `/api/employees/abc` returns `404` although the action exists. Why?

**Answer:** The action route is `{id:int}`. `abc` fails the constraint, so the route does not match and the request never reaches the action.

---

### Question 3

`GET /api/employees` works, `POST /api/employees` returns `405`. What does `405` tell you?

**Answer:** The path matched (so the URL is right) but no action accepts `POST`. The `Allow` header lists the accepted methods.

---

### Question 4

Why does `[ApiController]` matter for a POST with a missing body?

**Answer:** With it, ASP.NET Core rejects the request with `400` and a JSON description before your action runs. Without it, your action receives an empty object and may save bad data.

---

### Question 5

Name the difference between `NotFound()` and `NotFound("employee does not exist")`.

**Answer:** Both return `404`. The first produces a standard JSON error description; the second returns your own text as the body.

---

### Question 6

Which should a controller contain: HTTP handling, or business rules?

**Answer:** HTTP handling — read the request, call the logic (Topic 05), choose the status code, return the body. Business rules belong behind the controller.

---

## ✅ Check Yourself

- [ ] I can explain the difference between a controller, an action, and an endpoint
- [ ] I know why we use `ControllerBase` and `[ApiController]`
- [ ] I can predict the URL of an action from its attributes
- [ ] I used a route parameter and a route constraint correctly
- [ ] I know why the parameter name must match the placeholder
- [ ] I can pick the right response helper for success, missing, and invalid
- [ ] I debugged at least one of the four routing challenges myself

---

## Summary

| Concept | What to remember |
|---------|------------------|
| Controller | Class that receives HTTP requests and returns responses |
| Action | Public method = one endpoint |
| `[ApiController]` | Body binding + automatic `400` |
| `[Route("api/[controller]")]` | Base path, `[controller]` = name minus `Controller` |
| `[HttpGet]` etc. | Which method the action accepts |
| Route parameter | `{id:int}` — name must match the action parameter |
| Route constraint | Rejects bad URLs **before** the action runs |
| `IActionResult` | Lets each branch choose its status code |
| `404` empty body | Route never matched |
| `405` + `Allow` | Route matched, method rejected |

---

**Next: [05 — Dependency Injection](05-Dependency-Injection.md)**
