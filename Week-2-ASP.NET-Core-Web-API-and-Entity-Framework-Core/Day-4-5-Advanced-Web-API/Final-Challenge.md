# Final Challenge — Employee Search Screen

## The Requirement

HR needs an employee search screen. It must allow the user to:

- Search employees by name or email
- Filter by department and minimum salary
- Sort the results
- Navigate through pages of results

All of this must be handled by a single endpoint.

## Your Task

Implement and verify a complete `GET /api/employees` endpoint that supports filtering, searching, sorting, and pagination in one request.

You already have the building blocks from Day 4 exercises. Combine them into one working pipeline.

## Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `search` | `string?` | Match against name and email |
| `departmentId` | `int?` | Filter by department |
| `minSalary` | `decimal?` | Minimum salary |
| `sortBy` | `string?` | One of: `name`, `email`, `salary` |
| `sortOrder` | `string?` | `asc` or `desc` |
| `pageNumber` | `int` | Default 1, minimum 1 |
| `pageSize` | `int` | Default 10, range 1–100 |

## Expected Response Shape

```json
{
  "items": [
    {
      "id": 1,
      "name": "Ali Hassan",
      "email": "ali@example.com",
      "salary": 4500,
      "departmentId": 2,
      "departmentName": "Engineering"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3
}
```

## Acceptance Criteria

Check each item when you are done:

- [ ] `GET /api/employees` returns the paginated response shape
- [ ] `search` matches against both name and email (case-insensitive)
- [ ] `departmentId` filters by department
- [ ] `minSalary` filters employees earning at least that amount
- [ ] `sortBy` supports `name`, `email`, and `salary`
- [ ] `sortOrder` supports `asc` and `desc` (default `asc`)
- [ ] `pageNumber` defaults to 1 and rejects values below 1
- [ ] `pageSize` defaults to 10 and rejects values outside 1–100
- [ ] `totalCount` reflects the number of matching rows before pagination
- [ ] `totalPages` is calculated correctly from `totalCount` and `pageSize`

## Before You Start

- You have already built filtering, searching, sorting, and pagination separately in Exercises 1–4.
- The `EmployeeQueryParameters` class from Day 4 already has the properties and validation attributes you need.
- The `EmployeePage` response class already has the correct shape.
- Think about the order of operations: when should you count? When should you paginate?
- The pipeline is: Filter → Search → Sort → Count → Paginate → Project → Execute.

## Verification

Open Swagger and test these requests:

**Valid requests:**

```http
GET /api/employees
GET /api/employees?search=ali
GET /api/employees?departmentId=2&minSalary=3000
GET /api/employees?sortBy=salary&sortOrder=desc
GET /api/employees?pageNumber=2&pageSize=5
GET /api/employees?search=ali&departmentId=2&minSalary=3000&sortBy=salary&sortOrder=desc&pageNumber=1&pageSize=10
```

**Invalid requests (expect 400):**

```http
GET /api/employees?pageNumber=0
GET /api/employees?pageSize=101
GET /api/employees?pageSize=-5
```

**Also verify from Day 5:**

```http
GET /api/employees/999999
```

This should return `404`.

---

## Solution Review

Stop here until you have completed the challenge.

### Approach

The endpoint builds one `IQueryable<Employee>` through a chain of operations. Each step either narrows the query (filter, search) or rearranges it (sort). Pagination happens last, just before execution.

The pipeline order is:

```text
Base query (AsNoTracking, Include Department)
    ↓
Filter by departmentId
    ↓
Filter by minSalary
    ↓
Search by name or email
    ↓
Sort by chosen field
    ↓
Count total matches
    ↓
Skip and Take for the current page
    ↓
Project to EmployeeDto
    ↓
Execute with ToListAsync
    ↓
Return EmployeePage with metadata
```

### Key decisions

**1. Count before pagination.** You need `totalCount` to represent all matching rows, not just the rows on the current page. Call `CountAsync()` after all filters and search but before `Skip`/`Take`.

**2. Deterministic sort is mandatory.** Without `OrderBy`, SQL Server returns rows in an undefined order. `Skip`/`Take` on an unordered query can skip or duplicate rows between pages. Always sort before paginating.

**3. Explicit sort mapping.** Use a `switch` expression to map `"name"`, `"email"`, `"salary"` to specific `OrderBy` calls. Never pass a raw client string to a property expression.

```csharp
query = parameters.SortBy?.ToLowerInvariant() switch
{
    "email" => descending
        ? query.OrderByDescending(e => e.Email)
        : query.OrderBy(e => e.Email),
    "salary" => descending
        ? query.OrderByDescending(e => e.Salary)
        : query.OrderBy(e => e.Salary),
    _ => descending
        ? query.OrderByDescending(e => e.Name)
        : query.OrderBy(e => e.Name)
};
```

**4. Project after pagination.** The `Select` to `EmployeeDto` comes after `Skip`/`Take`. EF Core translates the full chain into one SQL query with `WHERE`, `ORDER BY`, `OFFSET`, and `FETCH`.

```csharp
var items = await query
    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
    .Take(parameters.PageSize)
    .Select(e => new EmployeeDto
    {
        Id = e.Id,
        Name = e.Name,
        Email = e.Email,
        Salary = e.Salary,
        DepartmentId = e.DepartmentId,
        DepartmentName = e.Department == null ? null : e.Department.Name
    })
    .ToListAsync();
```

**5. `totalPages` is a computed property.** Use `Math.Ceiling` with a `double` division so integer rounding does not truncate the result:

```csharp
public int TotalPages => (int)Math.Ceiling(
    TotalCount / (double)PageSize);
```

### Pipeline pitfalls

The most common mistakes in this challenge are ordering problems:

| Mistake | What goes wrong |
|---|---|
| `ToListAsync()` before all filters | Filters are applied in memory, not in SQL |
| Count after `Skip`/`Take` | `totalCount` only reflects the current page |
| No sort before `Skip`/`Take` | Unpredictable rows appear on each page |
| Search on one field only | Users cannot find employees by email |

### Validation

`[ApiController]` handles the validation boundary. With `[Range(1, int.MaxValue)]` on `PageNumber` and `[Range(1, 100)]` on `PageSize`, invalid values produce a `400 Bad Request` before the action runs. You do not need manual checks for these.

### Error handling

The endpoint uses the same patterns from Day 5:

- `404` from `NotFound()` when a single employee is not found
- `400` from `[ApiController]` validation for invalid query parameters
- Centralized exception handler for unexpected errors, configured with `app.UseExceptionHandler()` and `AddProblemDetails()`

### Final build check

```bash
dotnet build
```

If the build succeeds, test the full URL in Swagger:

```http
GET /api/employees?search=ali&departmentId=2&minSalary=3000&sortBy=salary&sortOrder=desc&pageNumber=1&pageSize=10
```

Verify the response has `items`, `pageNumber`, `pageSize`, `totalCount`, and `totalPages` with the correct values.
