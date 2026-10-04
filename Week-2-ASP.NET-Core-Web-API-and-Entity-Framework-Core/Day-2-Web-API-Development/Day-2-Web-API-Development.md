# Day 2 — Web API Development

## Professional Training Course (Week 2)

**Duration:** 3 hours
**Level:** Intermediate — Building on Day 1
**Prerequisites:** Day 1 (controllers, routing, DI, in-memory data)
**Framework:** .NET 8 / ASP.NET Core 8
**Running Example:** Employee Management API (extending Day 1)

---

## 🎯 Day 2 Learning Objectives

By the end of the day, you will:

- Implement full CRUD operations (POST, PUT, DELETE)
- Understand why DTOs separate API contracts from internal models
- Use model binding to extract data from route, query string, and request body
- Choose appropriate HTTP status codes for different scenarios
- Test API endpoints using Swagger UI
- Apply basic API design principles

---

## ⏱️ Training Schedule

| # | Topic | Duration |
|---|-------|----------|
| 01 | CRUD Operations | ~40 min |
| 02 | DTOs | ~30 min |
| 03 | Model Binding | ~30 min |
| ☕ | Short Break | 10 min |
| 04 | HTTP Status Codes | ~20 min |
| 05 | Swagger / OpenAPI | ~25 min |
| 06 | API Design Best Practices | ~20 min |
| 🏆 | Final Challenge & Review | ~15 min |

**Total: 3 hours.**

---

## 📚 Topic Files

| # | Topic | File |
|---|-------|------|
| 01 | CRUD Operations | [01 — CRUD Operations](01-CRUD-Operations.md) |
| 02 | DTOs | [02 — DTOs](02-DTOs.md) |
| 03 | Model Binding | [03 — Model Binding](03-Model-Binding.md) |
| 04 | HTTP Status Codes | [04 — HTTP Status Codes](04-HTTP-Status-Codes.md) |
| 05 | Swagger / OpenAPI | [05 — Swagger and OpenAPI](05-Swagger-and-OpenAPI.md) |
| 06 | API Design Best Practices | [06 — API Design Best Practices](06-API-Design-Best-Practices.md) |

---

## 🛠️ Project Progression

Day 1 ended with two GET endpoints. Day 2 extends the API:

```text
Day 1 endpoints:
   GET  /api/employees
   GET  /api/employees/{id}

Day 2 adds:
   POST   /api/employees          (create)
   PUT    /api/employees/{id}     (replace)
   DELETE /api/employees/{id}     (remove)
```

Then we refine with:
```text
   DTOs for request/response
   Model binding from body/query/route
   Appropriate status codes
   Swagger documentation
   API design principles
```

---

## 🧪 Exercises

| # | Exercise | Kind |
|---|----------|------|
| 01 | Employee CRUD | Starter + Solution |
| 02 | Employee DTOs | Starter + Solution |
| 03 | Model Binding | Starter + Solution |
| 04 | Status Code Challenge | Debug exercise |
| 05 | Swagger Practice | Hands-on |
| 06 | API Design Challenge | Debug exercise |
| 07 | Final Challenge: Departments CRUD | Starter + Solution |

**Exercise projects:** [Exercises/](Exercises/)

---

## 🔜 Next: Day 3 — Entity Framework Core

Day 3 replaces in-memory data with a real database using Entity Framework Core.

**Start with [01 — CRUD Operations](01-CRUD-Operations.md).**
