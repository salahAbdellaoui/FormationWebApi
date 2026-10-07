# Days 4-5 — Advanced Web API

**Duration:** approximately 6 hours (3 hours per day)

This module continues the Employee Management API from Days 1-3. It assumes the Day 3 EF Core exercises are complete: `Employee` has `Email`, `Salary`, and `DepartmentId` fields, and the API uses `AppDbContext` with SQL Server.

---

## Learning Goals

By the end of this module, you will be able to:

- Filter and search employees with query parameters
- Sort results using a small, explicit set of fields
- Paginate a result set safely
- Validate request and query data
- Return consistent errors for expected and unexpected failures
- Combine all features into one practical endpoint

---

## The Big Picture

```text
Day 1: ASP.NET Core fundamentals
         |
         v
Day 2: Build a Web API (CRUD, DTOs, Swagger)
         |
         v
Day 3: Connect to SQL Server with EF Core
         |
         v
Day 4: Make the API queryable  <-- you are here
         |
         v
Day 5: Make the API reliable
```

After Day 3, the API returns all employees in a flat list. Day 4-5 transforms it into a practical, queryable, validated API.

---

## Day 4 — Filtering, Searching, Sorting, Pagination

[Day-4-Advanced-Querying.md](Day-4-Advanced-Querying.md)

Build the queryable endpoint in five progressive stages:

| Stage | What | Key Concept |
|-------|------|-------------|
| 1 | Filtering | `IQueryable`, conditional `Where`, `AsNoTracking` |
| 2 | Searching | `.Contains()` translated to SQL `LIKE` |
| 3 | Sorting | Explicit switch mapping, default sort |
| 4 | Pagination | `Skip`, `Take`, `CountAsync`, `EmployeePage` response |
| 5 | Combined | Pipeline order matters |

The most important lesson: keep the query as `IQueryable` until all clauses are built. Calling `ToListAsync()` too early executes the query before filtering, sorting, or pagination.

---

## Day 5 — Validation and Error Handling

[Day-5-Validation-and-Error-Handling.md](Day-5-Validation-and-Error-Handling.md)

Make the API reliable:

| Section | What | Key Concept |
|---------|------|-------------|
| 1 | Validation | Data Annotations, `[ApiController]` automatic 400 |
| 2 | Not Found | 404 for missing resources |
| 3 | Error Handling | `AddProblemDetails()`, `UseExceptionHandler()` |
| 4 | Integration | Full request lifecycle, test checklist |

---

## Final Challenge

[Final-Challenge.md](Final-Challenge.md)

A realistic requirement: build an employee search screen that combines filtering, searching, sorting, pagination, validation, and error handling. Test everything through Swagger.

---

## Exercises

[Exercises/README.md](Exercises/README.md)

Five progressive exercises (one per Day 4 stage). Each exercise has a goal, starting point, task, hints, verification URLs, and common mistake.

The [Exercises/Starter/](Exercises/Starter/) project is the completed Day 3 API. Use it as your starting point.

---

## Prerequisites

- Day 1-3 completed
- SQL Server available
- .NET 8 SDK installed
- The Day 3 project builds and runs

## Scope

This module deliberately does **not** cover: authentication, authorization, Clean Architecture, Repository Pattern, CQRS, MediatR, AutoMapper, FluentValidation, OData, GraphQL, caching, cursor pagination, or microservices. These belong to later modules.

---

## Suggested Schedule

### Day 4 (3 hours)

| Time | Activity |
|------|----------|
| 10 min | Problem introduction |
| 30 min | Stage 1 — Filtering |
| 30 min | Stage 2 — Searching |
| 30 min | Stage 3 — Sorting |
| 35 min | Stage 4 — Pagination |
| 25 min | Stage 5 — Combined query |
| 15 min | Debugging scenarios |
| 35 min | Exercises |

### Day 5 (3 hours)

| Time | Activity |
|------|----------|
| 15 min | Reliability problem introduction |
| 35 min | Validation with Data Annotations |
| 25 min | Validation exercises |
| 30 min | 404 and expected errors |
| 35 min | Centralized error handling |
| 20 min | Problem Details and security |
| 40 min | Final challenge |

---

## Build and Test

```bash
dotnet build
dotnet run
```

Open Swagger: `https://localhost:<port>/swagger`
