# Day 3 — Entity Framework Core

## Professional Training Course (Week 2)

**Duration:** 3 hours
**Level:** Intermediate — Building on Day 1 and Day 2
**Prerequisites:** Day 1 (controllers, routing, DI), Day 2 (CRUD, DTOs, Swagger)
**Framework:** .NET 8 / EF Core 8
**Running Example:** Employee Management API (moving from in-memory to SQL Server)

---

## Learning Objectives

By the end of the day, you will:

- Create a `DbContext` and define `DbSet` properties
- Understand the Code First approach
- Create and apply a migration
- Define a one-to-many relationship between entities
- Query data using LINQ through EF Core
- Replace the in-memory data source with a real database

---

## Training Schedule

| # | Topic | Duration |
|---|-------|----------|
| 01 | DbContext and DbSet | ~30 min |
| 02 | Code First | ~30 min |
| 03 | Migrations | ~30 min |
| | Short Break | 10 min |
| 04 | Relationships | ~30 min |
| 05 | LINQ with EF Core | ~30 min |
| | Exercises / Practical Work | ~35 min |
| | Review / Knowledge Check | ~15 min |

**Total: 3 hours.**

---

## Topic Files

| # | Topic | File |
|---|-------|------|
| 01 | DbContext and DbSet | [01 -- DbContext and DbSet](01-DbContext-and-DbSet.md) |
| 02 | Code First | [02 -- Code First](02-Code-First.md) |
| 03 | Migrations | [03 -- Migrations](03-Migrations.md) |
| 04 | Relationships | [04 -- Relationships](04-Relationships.md) |
| 05 | LINQ with EF Core | [05 -- LINQ with EF Core](05-LINQ-with-EF-Core.md) |

---

## Project Progression

Day 1 and Day 2 used in-memory data:

```text
Controller
    |
    v
EmployeeService (in-memory List)
```

Day 3 replaces the data source:

```text
Controller
    |
    v
AppDbContext
    |
    v
EF Core
    |
    v
SQL Server
```

The existing controller style and DTOs stay. Only the data source changes.

---

## Required Packages

| Package | Purpose |
|---------|---------|
| `Microsoft.EntityFrameworkCore.SqlServer` | SQL Server database provider |
| `Microsoft.EntityFrameworkCore.Tools` | CLI tools for migrations |

Both target .NET 8 / EF Core 8.

---

## Exercises

| # | Exercise | Kind |
|---|----------|------|
| 01 | Create DbContext | Hands-on |
| 02 | Code First Entities | Hands-on |
| 03 | Create and Apply Migration | Hands-on |
| 04 | Define Relationship | Hands-on |
| 05 | EF Core Queries | Hands-on |

**Exercise starter project:** [Exercises/](Exercises/)

---

## Senior Developer Notes

- `DbContext` is a unit-of-work. Do not make it a global singleton. ASP.NET Core creates one per request through DI.
- EF Core already provides `DbSet<T>` as your data-access abstraction. You do not need a Repository pattern on top of it at this stage.
- Keep database access explicit and understandable. Do not hide simple EF Core operations behind unnecessary layers.

---

## Next: Day 4

Day 4 will build on this foundation with more advanced topics.

**Start with [01 -- DbContext and DbSet](01-DbContext-and-DbSet.md).**
