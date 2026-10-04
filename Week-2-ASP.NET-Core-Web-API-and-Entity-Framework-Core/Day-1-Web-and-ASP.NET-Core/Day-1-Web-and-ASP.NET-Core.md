# Day 1 — Web & ASP.NET Core

## Professional Training Course (Week 2)

**Duration:** 4 hours
**Level:** Intermediate — Building on Week 1 (C# Fundamentals, Advanced C#, SQL Server, Git)
**Prerequisites:** Day 0 (.NET SDK installed), Week 1 (classes, interfaces, generics, LINQ basics)
**Framework:** .NET 8 / ASP.NET Core 8
**Running Example:** Employee Management API (in-memory data — no database today)

---

## 🎯 Day 1 Learning Objectives

By the end of this session, you will understand:

- What happens between a client sending an HTTP request and receiving an HTTP response
- How HTTP methods, headers, bodies, and status codes work
- How to design resource-oriented REST URLs instead of action-oriented URLs
- What .NET, ASP.NET Core, and ASP.NET Core Web API are, and how they relate
- How an ASP.NET Core application starts and how it processes a request
- What middleware is and why pipeline order matters
- What each important file and folder in a Web API project is responsible for
- How a URL and an HTTP method are mapped to a controller action
- How to use route parameters and route constraints correctly
- What Dependency Injection is, why it exists, and how ASP.NET Core resolves services
- The difference between Transient, Scoped, and Singleton service lifetimes

---

## Training Schedule

| Part | Topic | Duration |
|------|-------|----------|
| 1 | HTTP and REST Principles | ~45 min |
| 2 | ASP.NET Core Fundamentals | ~40 min |
| 3 | Project Structure | ~25 min |
| 4 | ☕ Short Break | 10 min |
| 5 | Controllers and Routing | ~55 min |
| 6 | Dependency Injection | ~50 min |
| 7 | Wrap-up, Review & Final Challenge | ~15 min |

---

## 🗺️ How Today Connects

Week 1 gave you the **languages and the data**:

```text
C# (Week 1)  +  SQL Server (Week 1)
```

Week 2 puts them behind an **HTTP interface** that any client can call:

```text
Mobile app / Frontend / Another service
              ↓
         HTTP Request
              ↓
      ASP.NET Core Web API   ← you are here (Week 2, Day 1)
              ↓
      Application Logic
              ↓
     Entity Framework Core   ← later this week
              ↓
         SQL Server          ← Week 1
```

Today we stay **above the database**. Data lives in memory. We only study the path a request takes:

```text
Client
   ↓
HTTP Request
   ↓
ASP.NET Core
   ↓
Middleware Pipeline
   ↓
Routing
   ↓
Controller
   ↓
Dependency Injection
   ↓
Service
   ↓
HTTP Response
   ↓
Client
```

> 💡 **Senior Developer Note:** Everything you learn this week depends on this path. Teams lose days debugging because somebody does not know *where* in this chain the request stops.

---

## 📚 Lesson Files

The lesson is organized into focused files:

| # | Topic | File |
|---|-------|------|
| 01 | HTTP and REST Principles | [01 — HTTP and REST Principles](01-HTTP-and-REST-Principles.md) |
| 02 | ASP.NET Core Fundamentals | [02 — ASP.NET Core Fundamentals](02-ASP.NET-Core-Fundamentals.md) |
| 03 | Project Structure | [03 — Project Structure](03-Project-Structure.md) |
| 04 | Controllers and Routing | [04 — Controllers and Routing](04-Controllers-and-Routing.md) |
| 05 | Dependency Injection | [05 — Dependency Injection](05-Dependency-Injection.md) |

### Recommended Learning Sequence

```text
Day 1
│
├── 01  HTTP and REST          → the language both sides speak
├── 02  ASP.NET Core           → the server that answers
├── 03  Project Structure      → where everything lives
├── 04  Controllers & Routing  → which C# code runs
└── 05  Dependency Injection   → which objects it uses
```

Read them in order. Each file assumes the previous one. Each file is self-contained: concept → example → exercise → common mistakes → knowledge check → summary.

---

## 🛠️ Practical Project Overview

You build one small **Employee Management API** during the day. It grows file by file:

| Topic | What you add | What works afterwards |
|-------|--------------|-----------------------|
| 01 — HTTP and REST | Endpoint design on paper | You can describe the API before coding it |
| 02 — Fundamentals | A new project with `dotnet new` | The template runs and answers a request |
| 03 — Structure | `Models/` and `Services/` folders + the `Employee` type | You know where each file belongs |
| 04 — Controllers | `EmployeesController` with two `GET` actions | `GET /api/employees` and `GET /api/employees/{id}` return JSON |
| 05 — Dependency Injection | `IEmployeeService` + `EmployeeService` | Same endpoints, but the controller no longer holds data logic |

The two endpoints that must work at the end of the day:

```text
GET /api/employees        → 200 + the list of employees (JSON)
GET /api/employees/{id}   → 200 + one employee, or 404 if it does not exist
```

No database, no Entity Framework Core, no authentication. In-memory data only — on purpose, so every line of code stays understandable.

---

## 📝 Exercises Overview

Exercises are spread inside the topic file they belong to. You never leave a topic without practising it.

| File | Type of practice |
|------|------------------|
| 01 | HTTP reading exercises, REST URL design, response interpretation |
| 02 | Pipeline tracing, `Program.cs` reading, run-and-observe |
| 03 | File responsibility matching, structure exercise |
| 04 | Controller writing, route prediction, **debugging challenges** (404, 405, wrong route) |
| 05 | Service extraction, registration, injection, lifetime choice, **debugging challenge** (missing registration) |

**Exercise projects (Starter + Solution):** [Exercises/](Exercises/)

---

## 🏆 Final Challenge

At the end of the day, without looking at the lesson:

1. Create a Web API project called `EmployeeManagement.Api`.
2. Add `GET /api/departments` returning a fixed in-memory list of departments.
3. Add `GET /api/departments/{id}` returning one department or `404`.
4. Put the data behind an `IDepartmentService` registered with **Scoped** and injected into the controller.
5. Explain in one paragraph: from `curl http://localhost:xxxx/api/departments/2` to the JSON in the response.

**Exercise files:** [Exercise 03 — Final Challenge](Exercises/Exercise-03-Final-Challenge/) (Starter + Solution)

If you can do that, you understood Day 1.

---

## 📌 Day Summary

```text
HTTP        → request, response, methods, status codes
   ↓
REST        → resources, resource URLs, statelessness, JSON
   ↓
ASP.NET Core→ startup, Program.cs, pipeline, middleware, configuration
   ↓
Project     → .csproj, appsettings, launchSettings, Controllers/
   ↓
Controller  → [ApiController], [Route], [HttpGet], route parameters, IActionResult
   ↓
DI          → interface, registration, constructor injection, lifetimes
```

---

## 🔜 Next: Day 2 — Web API Development

Today your API **reads** two endpoints. Tomorrow it will grow into full CRUD: creating, updating, and deleting data, using request payloads, and shaping responses with DTOs.

```text
Day 1:  understand the request path        ✅ you are here
Day 2:  build the complete CRUD API
Day 3:  connect the API to a database with Entity Framework Core
```

---

**Start with [01 — HTTP and REST Principles](01-HTTP-and-REST-Principles.md)**
