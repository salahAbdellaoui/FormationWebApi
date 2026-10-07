# Week 2 — Days 4-5: Advanced Web API

**Duration:** approximately 6 hours

This module continues the Employee Management API from Days 1-3. It assumes that the Day 3 EF Core exercise has been completed, so `Employee` has `Email`, `Salary`, and `DepartmentId` fields and the API is using `AppDbContext`.

## Learning goals

By the end of this module, you will be able to:

- filter and search employees with query parameters;
- sort results using a small, explicit set of fields;
- paginate a result set safely;
- validate request and query data;
- return consistent errors for expected and unexpected failures; and
- combine these features in a practical endpoint.

The important request flow is:

```text
HTTP request
    ↓
Query-string model binding
    ↓
Validation
    ↓
Build IQueryable
    ↓
Filter → Search → Sort → Paginate
    ↓
Execute with EF Core
    ↓
API response
```

## Prerequisites and scope

Use the existing `EmployeeManagement.Api`, controller, DTOs, Swagger setup, and EF Core configuration. Do not create another API project or repeat CRUD, DTO, routing, or Swagger lessons.

This module deliberately does not introduce authentication, authorization, Clean Architecture, repositories, CQRS, MediatR, AutoMapper, FluentValidation, OData, caching, or cursor pagination.

## Day 4 — useful employee queries

The first four topics are implemented together in [`Day-4-Advanced-Querying.md`](./Day-4-Advanced-Querying.md):

```http
GET /api/employees?departmentId=2&minSalary=3000&search=ali&sortBy=salary&sortOrder=desc&pageNumber=1&pageSize=10
```

Keep the query as `IQueryable<Employee>` until all clauses have been added. Calling `ToListAsync()` too early executes the query before the remaining filtering, sorting, or pagination has been applied.

## Day 5 — reliable API behavior

[`Day-5-Validation-and-Error-Handling.md`](./Day-5-Validation-and-Error-Handling.md) adds:

- data-annotation validation for body and query models;
- automatic `400 Bad Request` responses from `[ApiController]`;
- `404 Not Found` for a missing employee;
- built-in Problem Details for unexpected exceptions; and
- a consistent approach to testing expected failures.

## Suggested schedule

| Day | Topic | Time |
|---|---|---:|
| 4 | Filtering, searching, sorting, pagination, and exercises | 3 hours |
| 5 | Validation, error handling, integration, and final challenge | 3 hours |

Work in the order **Code → Test → Explain**. Run `dotnet build` after each coherent change and use the existing Swagger UI to exercise the endpoint.

## Final challenge

Finish one `GET /api/employees` endpoint that supports:

- `search` against name and email;
- `departmentId`;
- `minSalary`;
- `sortBy=name|email|salary`;
- `sortOrder=asc|desc`;
- `pageNumber` and `pageSize`; and
- a response containing `items`, `pageNumber`, `pageSize`, `totalCount`, and `totalPages`.

Then verify a valid request, an invalid page request, a missing employee request, and an unexpected exception through the API's documented error shape.

Exercises are in [`Exercises/README.md`](./Exercises/README.md).
