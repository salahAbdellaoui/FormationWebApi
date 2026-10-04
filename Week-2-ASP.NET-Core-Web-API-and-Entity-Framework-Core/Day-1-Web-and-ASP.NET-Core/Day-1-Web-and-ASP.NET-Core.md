# Day 1 — Web & ASP.NET Core

## Professional Training Course (Week 2)

**Duration:** 3 hours
**Level:** Intermediate — Building on Week 1
**Prerequisites:** Day 0 (.NET SDK installed), Week 1 (classes, interfaces)
**Framework:** .NET 8 / ASP.NET Core 8
**Running Example:** Employee Management API (in-memory data)

---

## 🎯 Day 1 Learning Objectives

By the end of the day, you will:

- Understand what happens between an HTTP request and a response
- Create and run a .NET 8 Web API project
- Build a controller with two `GET` endpoints
- Extract logic behind an interface
- Register and inject a service
- Trace the complete request flow from client to response

---

## ⏱️ Training Schedule

| # | Topic | Duration |
|---|-------|----------|
| 01 | HTTP & REST Mental Model | ~20 min |
| 02 | ASP.NET Core Fundamentals | ~25 min |
| 03 | Project Structure | ~20 min |
| ☕ | Short Break | 10 min |
| 04 | Controllers & Routing | ~55 min |
| 05 | Dependency Injection | ~40 min |
| 🏆 | Final Challenge & Review | ~20 min |

**Total: 3 hours.**

---

## 📚 Topic Files

| # | Topic | File |
|---|-------|------|
| 01 | HTTP & REST Mental Model | [01 — HTTP and REST Mental Model](01-HTTP-and-REST-Mental-Model.md) |
| 02 | ASP.NET Core Fundamentals | [02 — ASP.NET Core Fundamentals](02-ASP.NET-Core-Fundamentals.md) |
| 03 | Project Structure | [03 — Project Structure](03-Project-Structure.md) |
| 04 | Controllers & Routing | [04 — Controllers and Routing](04-Controllers-and-Routing.md) |
| 05 | Dependency Injection | [05 — Dependency Injection](05-Dependency-Injection.md) |

---

## 🛠️ Project Progression

The project grows one layer at a time:

```text
dotnet new webapi
       ↓
EmployeesController
       ↓
GET /api/employees
       ↓
GET /api/employees/{id}
       ↓
IEmployeeService  +  EmployeeService
       ↓
DI registration  +  constructor injection
```

At every step, **write → run → curl → observe → explain**.

---

## 🧪 Exercises

| # | Exercise | Kind |
|---|----------|------|
| 01 | Create the Web API Project | Hands-on |
| 02 | Create `EmployeesController` | Starter + Solution |
| 03 | `GET /api/employees` | Starter + Solution |
| 04 | `GET /api/employees/{id}` | Starter + Solution |
| 05 | Routing Challenge (debug) | Broken starter + fix |
| 06 | Extract `IEmployeeService` | Starter + Solution |
| 07 | Register & inject | Starter + Solution |
| 08 | Final Challenge: Departments API | Starter + Solution |

**Exercise projects:** [Exercises/](Exercises/)

---

## 🔜 Next: Day 2 — Web API Development

Day 2 adds the full CRUD surface (POST / PUT / DELETE), DTOs, model binding, status-code design, and Swagger.

**Start with [01 — HTTP and REST Mental Model](01-HTTP-and-REST-Mental-Model.md).**
