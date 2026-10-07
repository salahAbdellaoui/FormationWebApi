# Day 4-5 Exercises — Advanced Web API

These exercises continue the Employee Management API. Work through them in order. Each exercise builds on the previous one.

Do not introduce new libraries or architectural patterns. Work in the existing project.

---

## Exercise 1 — Filtering

### Goal

Add `departmentId` and `minSalary` query parameters to the employee listing endpoint.

### Starting Point

`EmployeesController.GetAll` returns all employees as a list:

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
{
    var employees = await _context.Employees
        .AsNoTracking()
        .Include(e => e.Department)
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

    return Ok(employees);
}
```

No query parameters exist yet.

### Task

1. Add `int? departmentId` and `decimal? minSalary` parameters to the action (or create an `EmployeeQueryParameters` class).
2. Before calling `ToListAsync()`, apply each filter only when the parameter has a value:
   - `departmentId`: match exact department
   - `minSalary`: salary greater than or equal to the value
3. Keep the query as `IQueryable` until all filters are applied.

### Expected Behavior

- Requests without parameters return all employees.
- Requests with one or both parameters return only matching employees.

### Hints

- Use `.Where()` conditionally. Reassign the query variable each time.
- `IQueryable` builds the SQL expression tree. Nothing executes until `ToListAsync()`.

### Verification

```http
GET /api/employees
GET /api/employees?departmentId=2
GET /api/employees?minSalary=3000
GET /api/employees?departmentId=2&minSalary=3000
```

The last request should return only employees in department 2 with salary >= 3000.

### Common Mistake

Calling `ToListAsync()` before applying all filters. This executes the query early and applies the remaining filters in memory instead of in SQL. Every `.Where()` must come before the single `ToListAsync()` call.

---

## Exercise 2 — Searching

### Goal

Add a `search` query parameter that finds employees whose name or email contains the search text.

### Starting Point

The endpoint from Exercise 1 with `departmentId` and `minSalary` filtering working.

### Task

1. Add a `string? search` parameter.
2. If the search value is not null, empty, or whitespace:
   - Trim whitespace from the value.
   - Add a `.Where()` clause that matches `Name` **or** `Email` containing the trimmed value.
3. Skip the search step when the value is empty after trimming.

### Expected Behavior

- `GET /api/employees?search=ali` returns employees whose name or email contains "ali".
- An empty `search=` returns all employees (no filtering applied).

### Hints

- `string.IsNullOrWhiteSpace()` is the cleanest way to check.
- Use `||` inside a single `.Where()` to search both fields. EF Core translates this to SQL `OR`.

### Verification

```http
GET /api/employees?search=ali
GET /api/employees?search=ali&departmentId=2
GET /api/employees?search=
```

The last request should return all employees (empty search is ignored).

### Common Mistake

Searching only the `Name` field and ignoring `Email`. The requirement says "name or email." Also, failing to trim whitespace means a search for `" ali "` misses records that `"ali"` would find.

---

## Exercise 3 — Sorting

### Goal

Add `sortBy` and `sortOrder` query parameters with an explicit mapping to supported fields.

### Starting Point

The endpoint from Exercise 2 with filtering and searching working.

### Task

1. Add `string? sortBy` and `string? sortOrder` parameters.
2. Determine sort direction: `sortOrder` equal to `"desc"` (case-insensitive) means descending. Everything else is ascending.
3. Use a `switch` expression on `sortBy` (lowercased) to map:
   - `"name"` → order by `Name`
   - `"email"` → order by `Email`
   - `"salary"` → order by `Salary`
   - anything else → default to `Name`
4. Apply `OrderBy` or `OrderByDescending` based on the direction.

### Expected Behavior

- `GET /api/employees?sortBy=salary` sorts by salary ascending.
- `GET /api/employees?sortBy=salary&sortOrder=desc` sorts by salary descending.
- Unknown `sortBy` values default to sorting by name.

### Hints

- Use `StringComparison.OrdinalIgnoreCase` or `.ToLowerInvariant()` for the comparison.
- The `switch` expression should return the modified `IQueryable<Employee>`.

### Verification

```http
GET /api/employees?sortBy=name
GET /api/employees?sortBy=email&sortOrder=asc
GET /api/employees?sortBy=salary&sortOrder=desc
```

### Common Mistake

Letting the client pass an arbitrary property name and using it directly with reflection or dynamic LINQ. This is a security risk and can crash on invalid property names. Always use an explicit mapping.

---

## Exercise 4 — Pagination

### Goal

Add `pageNumber` and `pageSize` parameters and return a paginated response with metadata.

### Starting Point

The endpoint from Exercise 3 with filtering, searching, and sorting working.

### Task

1. Create a response class:

```csharp
public class EmployeePage
{
    public IReadOnlyList<EmployeeDto> Items { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(
        TotalCount / (double)PageSize);
}
```

2. Add `int pageNumber` (default 1) and `int pageSize` (default 10) parameters.
3. After sorting and before pagination, call `CountAsync()` to get `totalCount`.
4. Apply `.Skip((pageNumber - 1) * pageSize).Take(pageSize)` to the sorted query.
5. Project to `EmployeeDto` after `Skip`/`Take`.
6. Return `EmployeePage` with `Items`, `PageNumber`, `PageSize`, and `TotalCount`.

### Expected Behavior

- The response includes the current page of items plus `totalCount` and `totalPages`.
- Changing `pageSize` changes the number of items returned per page.

### Hints

- `CountAsync()` must run on the query **after** filters and search but **before** `Skip`/`Take`.
- `TotalPages` is a computed property. `Math.Ceiling` with a `double` cast prevents integer truncation.

### Verification

```http
GET /api/employees?pageNumber=1&pageSize=5
GET /api/employees?pageNumber=2&pageSize=5
GET /api/employees?pageNumber=1&pageSize=10
```

Page 1 with size 5 and page 2 with size 5 should return different items with no overlap.

### Common Mistake

Not applying a deterministic sort before `Skip`/`Take`. Without `OrderBy`, SQL Server does not guarantee row order. Rows can shift between pages, appearing twice or never. Always sort before paginating.

---

## Exercise 5 — Combined Employee Query

### Goal

Combine filtering, searching, sorting, and pagination into one `GET /api/employees` endpoint that handles all parameters together.

### Starting Point

Exercises 1–4 are each working individually. Now they must work together in one endpoint with the correct pipeline order.

### Task

1. Create (or update) `EmployeeQueryParameters` with all properties and validation attributes:

```csharp
public class EmployeeQueryParameters
{
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public decimal? MinSalary { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
```

2. Build the endpoint using this pipeline order:

```text
Base query → Filter → Search → Sort → Count → Skip/Take → Project → Execute
```

3. Change the return type to `ActionResult<EmployeePage>`.
4. Run `dotnet build` to verify compilation.

### Expected Behavior

- A request with all parameters returns a filtered, searched, sorted, paginated result with correct metadata.
- Invalid `pageNumber` or `pageSize` values return `400 Bad Request` automatically through `[ApiController]`.
- Requests with no parameters return page 1 of all employees.

### Hints

- `CountAsync()` and `ToListAsync()` are the only two database calls. Both run against the same filtered-and-sorted query, but only the paginated one applies `Skip`/`Take`.
- The `Select` projection to `EmployeeDto` belongs after `Take`, not before.

### Verification

**Full request with all parameters:**

```http
GET /api/employees?search=ali&departmentId=2&minSalary=3000&sortBy=salary&sortOrder=desc&pageNumber=1&pageSize=10
```

**Default request (no parameters):**

```http
GET /api/employees
```

**Invalid pagination (expect 400):**

```http
GET /api/employees?pageNumber=0
GET /api/employees?pageSize=0
GET /api/employees?pageSize=101
```

**Single employee (expect 404 when ID does not exist):**

```http
GET /api/employees/999999
```

### Common Mistake

Counting **after** pagination. If you call `CountAsync()` after `Skip`/`Take`, `totalCount` will only reflect the current page size instead of all matching rows. Count first, then paginate.
