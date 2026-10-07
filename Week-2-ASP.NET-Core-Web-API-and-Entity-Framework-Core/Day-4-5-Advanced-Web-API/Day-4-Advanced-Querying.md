# Day 4 — Filtering, Searching, Sorting, and Pagination

**Duration:** approximately 3 hours

## What You'll Build Today

Today you will transform a simple "get all employees" endpoint into a powerful query endpoint. You will build it in five stages, each one adding a new capability:

1. **Filtering** — return only employees in a specific department or above a salary threshold
2. **Searching** — find employees by name or email with a single text box
3. **Sorting** — order results by name, email, or salary
4. **Pagination** — handle 100,000 employees without crashing the server
5. **Combined query** — all of the above working together in one endpoint

Each stage produces working code you can test in Swagger before moving to the next.

## The Problem

Open your `EmployeesController`. The `GetAll` endpoint looks like this:

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
{
    var employees = await _context.Employees
        .Include(e => e.Department)
        .ToListAsync();

    var dtos = employees.Select(e => new EmployeeDto
    {
        Id = e.Id,
        Name = e.Name,
        Email = e.Email,
        Salary = e.Salary,
        DepartmentId = e.DepartmentId,
        DepartmentName = e.Department != null ? e.Department.Name : null
    });

    return Ok(dtos);
}
```

This works fine today. You have a handful of employees. But imagine your company grows. Now you have 100,000 employees. This endpoint:

- Loads all 100,000 rows from the database into memory
- Serializes all 100,000 objects into JSON
- Sends all 100,000 objects over the network to the client

The client only wanted to see employees in the IT department earning more than $3,000, sorted by salary, showing the first 10 results. But it received everything.

Today you will fix this. The database should do the heavy lifting — not your application server, and certainly not the client.

---

## Stage 1 — Filtering (~30 min)

### What is filtering?

Filtering means asking the database to return only the rows that match certain conditions. Instead of loading everything and then removing unwanted rows in C#, you tell the database exactly what you want using `WHERE` clauses.

The database engine is optimized for this. It can use indexes. It transfers less data. It is always faster to filter at the database than in memory.

### The query parameter object

Instead of adding many individual parameters to your controller method, create a class to hold them:

```csharp
namespace EmployeeManagement.Api.Dtos;

public class EmployeeQueryParameters
{
    public int? DepartmentId { get; set; }
    public decimal? MinSalary { get; set; }
}
```

Nullable types mean "this filter is optional." When `DepartmentId` is null, you will not apply that filter.

### The first filter

Modify the `GetAll` method to accept the query parameters:

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll(
    [FromQuery] EmployeeQueryParameters parameters)
{
    IQueryable<Employee> query = _context.Employees
        .AsNoTracking()
        .Include(e => e.Department);

    if (parameters.DepartmentId.HasValue)
    {
        query = query.Where(e => e.DepartmentId == parameters.DepartmentId.Value);
    }

    var employees = await query.ToListAsync();

    var dtos = employees.Select(e => new EmployeeDto
    {
        Id = e.Id,
        Name = e.Name,
        Email = e.Email,
        Salary = e.Salary,
        DepartmentId = e.DepartmentId,
        DepartmentName = e.Department != null ? e.Department.Name : null
    });

    return Ok(dtos);
}
```

Notice several important things:

1. **`[FromQuery]`** tells ASP.NET Core to read values from the URL query string (`?departmentId=2`).
2. **`IQueryable<Employee> query`** — you store the query in a variable. This is critical. `IQueryable` represents a query that has not been executed yet. Each `.Where()` call adds another condition to the query.
3. **`AsNoTracking()`** tells EF Core not to track the returned entities in its change tracker. Since you are reading data and not modifying it, this saves memory and improves performance.
4. **`ToListAsync()` is called only once**, after all filters are applied. This is the moment the query actually runs.

### Test in Swagger

Run the application and open Swagger. Try these URLs:

```
GET /api/employees
GET /api/employees?departmentId=2
```

The first URL returns all employees. The second returns only employees in department 2.

### Adding a second filter

Now add `MinSalary`. Update the parameters class:

```csharp
public class EmployeeQueryParameters
{
    public int? DepartmentId { get; set; }
    public decimal? MinSalary { get; set; }
}
```

And add the filter in the controller:

```csharp
if (parameters.MinSalary.HasValue)
{
    query = query.Where(e => e.Salary >= parameters.MinSalary.Value);
}
```

Place this after the `DepartmentId` filter. The order of `Where` clauses does not matter for correctness, but keeping a consistent order helps readability.

### Test in Swagger

```
GET /api/employees?minSalary=3000
GET /api/employees?departmentId=2&minSalary=3000
```

The combined URL applies both filters. Only employees in department 2 with a salary of at least 3000 are returned.

### How the query is built

```
Client                          Controller
  |                                |
  |  ?departmentId=2&minSalary=3000 |
  | -----------------------------> |
  |                                |
  |                  IQueryable<Employee>
  |                       |
  |                   .Where(dept)
  |                       |
  |                  .Where(salary)
  |                       |
  |                  .ToListAsync()
  |                       |
  |                   EF Core
  |                       |
  |               translates to SQL
  |                       |
  |                   SQL Server
  |                       |
  |           SELECT ... WHERE DeptId=2
  |              AND Salary >= 3000
  |                       |
  | <--------------------------- |
  |     JSON response            |
```

The key insight: `IQueryable` is lazy. Nothing hits the database until you call `ToListAsync()`. Each `Where()` just builds up the expression tree. EF Core translates the entire tree into a single SQL query.

### Exercise

Open `Exercises/Day-4-Exercises.md` and complete Exercise 1: Filtering.

### Common mistakes

**Mistake 1: Calling `ToListAsync()` too early**

```csharp
var employees = await _context.Employees.ToListAsync();
employees = employees.Where(e => e.DepartmentId == departmentId).ToList();
```

This loads all employees into memory first, then filters in C#. If you have 100,000 employees, you load all 100,000 into memory. The filter runs on the application server instead of the database. This is the single most common performance mistake with EF Core.

**Mistake 2: Using `var` for the query**

```csharp
var query = _context.Employees.Include(e => e.Department);
query = query.Where(...);
```

This works, but using `IQueryable<Employee>` explicitly makes the type clear. When you see `IQueryable`, you know the query has not executed yet. When you see `List<Employee>`, you know it has.

**Mistake 3: Forgetting to check for null**

If you write `query.Where(e => e.DepartmentId == parameters.DepartmentId)` without checking `HasValue`, you get a filter that matches nothing when `DepartmentId` is null. Always check whether the optional parameter has a value before adding the filter.

### Senior developer note

`AsNoTracking()` is not just a performance optimization. It prevents subtle bugs. When EF Core tracks entities, it holds references to them. If you accidentally modify a tracked entity and call `SaveChanges()`, the change is persisted. With `AsNoTracking()`, the entities are detached. You cannot accidentally save changes to read-only data.

### Knowledge check

1. What type represents a database query that has not been executed yet?
2. When does EF Core send the SQL query to the database?
3. Why do you use `AsNoTracking()` for read-only queries?

---

## Stage 2 — Searching (~30 min)

### What is searching?

Filtering matches exact values: `DepartmentId == 2`. Searching finds partial matches in text: "find me everyone whose name contains 'ali'."

The client could pass separate `nameSearch` and `emailSearch` parameters, but a single `search` parameter that checks multiple fields is a better user experience. One text box, one parameter, multiple fields searched.

### Add the search parameter

Update the parameters class:

```csharp
public class EmployeeQueryParameters
{
    public int? DepartmentId { get; set; }
    public decimal? MinSalary { get; set; }
    public string? Search { get; set; }
}
```

### Add the search logic

In the controller, after the existing filters, add:

```csharp
if (!string.IsNullOrWhiteSpace(parameters.Search))
{
    var term = parameters.Search.Trim();
    query = query.Where(e =>
        e.Name.Contains(term) || e.Email.Contains(term));
}
```

The `Trim()` removes accidental leading or trailing whitespace. The `string.IsNullOrWhiteSpace` check ensures that an empty search string does not add a meaningless filter.

### Test in Swagger

```
GET /api/employees?search=ali
GET /api/employees?search=ali&departmentId=2
```

The first URL returns every employee whose name or email contains "ali". The second combines search with the department filter.

### What EF Core generates

When you write `.Contains(term)`, EF Core translates it to a SQL `LIKE` expression:

```sql
WHERE Name LIKE '%ali%' OR Email LIKE '%ali%'
```

The `%` wildcards mean "any characters before or after." This is a case-insensitive search by default in SQL Server (depending on the column collation).

### The full pipeline so far

```
Query Parameters
       |
       v
IQueryable (base query)
       |
       v
  .Where(departmentId)    -- filter
       |
       v
  .Where(minSalary)       -- filter
       |
       v
  .Where(search)          -- search
       |
       v
  .ToListAsync()          -- execute
```

Each step adds to the query. Nothing executes until `ToListAsync()`.

### Exercise

Complete Exercise 2 in `Exercises/Day-4-Exercises.md`.

### Common mistakes

**Mistake 1: Searching after materialization**

```csharp
var employees = await _context.Employees.ToListAsync();
var filtered = employees.Where(e => e.Name.Contains(term)).ToList();
```

This loads every employee into memory and then searches in C#. With 100,000 employees, you waste memory and CPU. The database can search faster than your application server.

**Mistake 2: Not trimming the search term**

If the user types `" ali "` (with spaces), `.Contains(" ali ")` will not match `"Ali"`. Always trim.

**Mistake 3: Case sensitivity surprises**

`.Contains()` in LINQ-to-Entities is translated to SQL `LIKE`. Whether this is case-sensitive depends on your database collation. SQL Server's default collation is case-insensitive. If you need guaranteed case-insensitive search, use `.ToLower()` on both sides — but know that this prevents index usage.

### Senior developer note

For this training project, `.Contains()` is the right tool. In a production system with millions of rows, `LIKE '%term%'` cannot use indexes efficiently because the wildcard is at the beginning. You would consider full-text search, trigram indexes, or an external search engine. But do not reach for those tools until you have measured a real performance problem.

### Knowledge check

1. What SQL does `.Contains(term)` translate to?
2. Why do you check `!string.IsNullOrWhiteSpace` before adding the search filter?
3. If the user types `"  john  "`, what happens without `.Trim()`?

---

## Stage 3 — Sorting (~30 min)

### Why sorting matters

Without an explicit sort, SQL Server returns rows in whatever order is convenient — usually insertion order or index order. This order is not guaranteed. If you paginate without sorting, the same employee might appear on page 1 and page 3, while another never appears at all.

### The temptation to let the client choose

You might think: "I will let the client pass any property name to sort by." Do not do this. If the client passes `sortBy=PasswordHash` or `sortBy=; DROP TABLE Employees`, you have a problem.

Instead, define an explicit list of allowed sort columns and map them in code.

### Add sort parameters

Update the parameters class:

```csharp
public class EmployeeQueryParameters
{
    public int? DepartmentId { get; set; }
    public decimal? MinSalary { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}
```

`SortBy` accepts: `"name"`, `"email"`, or `"salary"`. Anything else falls back to sorting by name.

`SortOrder` accepts: `"desc"` for descending. Anything else (or nothing) means ascending.

### Add the sorting logic

After the search block, add:

```csharp
var descending = string.Equals(
    parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

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

This switch expression maps each allowed value to the correct `OrderBy` or `OrderByDescending` call. The default case (`_`) sorts by name. An unknown `sortBy` value does not crash — it silently uses the default.

### Test in Swagger

```
GET /api/employees?sortBy=name
GET /api/employees?sortBy=salary&sortOrder=desc
GET /api/employees?sortBy=email&sortOrder=asc
GET /api/employees?sortBy=unknown
```

The last URL sorts by name (the default) because `"unknown"` is not in the allowed list.

### Why the explicit switch?

Consider what happens if you pass the client string directly to EF Core. There is no built-in way to say `.OrderBy(clientString)`. You would need expression trees, reflection, or a library. Each of those adds complexity and risk. The switch is 10 lines of code. It is obvious what it does. It is obvious what the API supports. It is impossible to exploit.

### The pipeline grows

```
IQueryable (base query)
       |
  .Where(departmentId)    -- filter
       |
  .Where(minSalary)       -- filter
       |
  .Where(search)          -- search
       |
  .OrderBy(...)           -- sort
       |
  .ToListAsync()          -- execute
```

The sort is applied after all filters but before execution. This is correct: you want to sort the filtered results, not sort everything and then filter.

### Exercise

Complete Exercise 3 in `Exercises/Day-4-Exercises.md`.

### Common mistakes

**Mistake 1: Sorting before filtering**

```csharp
query = query.OrderBy(e => e.Name);
query = query.Where(e => e.DepartmentId == departmentId);
```

This works correctly — EF Core is smart enough to produce the same SQL regardless of whether you call `OrderBy` before or after `Where`. However, it is confusing to read. Keep a consistent order: filter first, then sort, then paginate. This matches the SQL execution order and makes the code easier to reason about.

**Mistake 2: Case-sensitive sort comparison**

```csharp
if (parameters.SortOrder == "desc")
```

This fails if the client sends `"Desc"` or `"DESC"`. Use `string.Equals` with `StringComparison.OrdinalIgnoreCase`.

**Mistake 3: No default sort**

If `sortBy` is null and you do not provide a default, the query has no ordering. This is dangerous once you add pagination (Stage 4). Always provide a default sort.

### Senior developer note

Some teams return `400 Bad Request` when the client passes an unknown `sortBy`. Others silently use the default. Both are valid design decisions. Pick one, document it, and be consistent. This lesson uses the silent default approach.

### Knowledge check

1. Why should you not let the client pass any property name to sort by?
2. What happens when the client passes `sortBy=unknown` in your implementation?
3. Why do you always provide a default sort column?

---

## Stage 4 — Pagination (~35 min)

### The big problem

You have 100,000 employees. The client requests all of them. Your server:

1. Reads 100,000 rows from SQL Server
2. Allocates memory for 100,000 entity objects
3. Maps them to 100,000 DTOs
4. Serializes 100,000 DTOs to JSON
5. Sends a multi-megabyte HTTP response

The client renders the first 10 rows in a table and ignores the rest. This is wasteful and slow.

Pagination solves this: the client requests one page at a time.

### How Skip and Take work

```
100 Employees, page size 10

Page 1:  Skip 0,  Take 10   → employees 1-10
Page 2:  Skip 10, Take 10   → employees 11-20
Page 3:  Skip 20, Take 10   → employees 21-30
...
Page 10: Skip 90, Take 10   → employees 91-100

Formula: Skip = (PageNumber - 1) * PageSize
```

`Skip()` tells the database to ignore the first N rows. `Take()` tells it to return at most N rows. In SQL, this becomes `OFFSET ... FETCH NEXT ...`.

### Add pagination parameters

Update the parameters class:

```csharp
using System.ComponentModel.DataAnnotations;

public class EmployeeQueryParameters
{
    public int? DepartmentId { get; set; }
    public decimal? MinSalary { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
```

The `[Range]` attributes are critical:

- `PageNumber` must be at least 1. Page 0 does not exist.
- `PageSize` must be between 1 and 100. A maximum prevents the client from requesting all 100,000 rows in one page.

Because the controller has `[ApiController]`, invalid values automatically return `400 Bad Request` before your action code runs.

### The EmployeePage response model

The client needs to know how many total employees exist (so it can show "Page 1 of 50") and how many total pages there are. Create a response model:

```csharp
namespace EmployeeManagement.Api.Dtos;

public class EmployeePage
{
    public IReadOnlyList<EmployeeDto> Items { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

`TotalPages` is computed, not stored. It is always derived from `TotalCount` and `PageSize`.

### The paginated query

Here is the critical part. You need two pieces of information from the database:

1. The total count of matching employees (before pagination)
2. The employees on the requested page (after pagination)

```csharp
var totalCount = await query.CountAsync();

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

return Ok(new EmployeePage
{
    Items = items,
    PageNumber = parameters.PageNumber,
    PageSize = parameters.PageSize,
    TotalCount = totalCount
});
```

### Why CountAsync comes before Skip and Take

`CountAsync()` counts all rows that match the filters and search. It runs a `SELECT COUNT(*)` query with all your `WHERE` clauses but without `OFFSET/FETCH`.

If you called `CountAsync()` after `Skip` and `Take`, you would count only the rows on the current page (at most `PageSize`). That is not what you want. You need the total count across all pages.

The return type changes from `IEnumerable<EmployeeDto>` to `EmployeePage`:

```csharp
[HttpGet]
public async Task<ActionResult<EmployeePage>> GetAll(
    [FromQuery] EmployeeQueryParameters parameters)
```

### Test in Swagger

```
GET /api/employees?pageNumber=1&pageSize=5
GET /api/employees?pageNumber=2&pageSize=5
GET /api/employees?pageNumber=1&pageSize=5&departmentId=2
```

Observe the response shape:

```json
{
  "items": [
    { "id": 1, "name": "Ahmad Ali", "email": "ahmad@test.com", ... },
    { "id": 3, "name": "Ali Hassan", "email": "ali@test.com", ... }
  ],
  "pageNumber": 1,
  "pageSize": 5,
  "totalCount": 12,
  "totalPages": 3
}
```

Try `pageNumber=0` — you should get a `400 Bad Request` because of the `[Range(1, int.MaxValue)]` attribute.

Try `pageSize=500` — you should get a `400 Bad Request` because the maximum is 100.

### Exercise

Complete Exercise 4 in `Exercises/Day-4-Exercises.md`.

### Common mistakes

**Mistake 1: pageNumber = 0**

`Skip((0 - 1) * 10)` = `Skip(-10)`. EF Core throws an exception. The `[Range]` attribute prevents this by rejecting the request before it reaches your code.

**Mistake 2: No deterministic sort before Skip/Take**

```csharp
query.Skip(10).Take(10);
```

Without `OrderBy`, SQL Server can return rows in any order. Page 1 might contain employee #5 and employee #17. Page 2 might contain employee #5 again. A deterministic sort (one that includes a unique column like `Id` to break ties) ensures each row appears on exactly one page.

**Mistake 3: Count after Skip/Take**

```csharp
var items = await query.Skip(0).Take(10).ToListAsync();
var count = items.Count;
```

This gives you the count of items on the current page, not the total count. You need `query.CountAsync()` before `Skip/Take`.

**Mistake 4: PageSize too large**

Without a maximum, a client can request `pageSize=1000000`. Your server reads and serializes a million rows. Always cap the page size.

### Senior developer note

Offset pagination (`OFFSET/FETCH`) has a known problem: for very large datasets, high page numbers are slow because the database must scan and discard all rows before the offset. For example, page 10,000 with page size 10 means the database reads and discards 100,000 rows.

This is acceptable for most business applications. If you encounter this problem in production, investigate cursor-based pagination (keyset pagination). But do not build it until you need it.

### Knowledge check

1. What formula converts `pageNumber` and `pageSize` into the `Skip` value?
2. Why must `CountAsync()` run before `Skip/Take`?
3. What happens if you paginate without a deterministic sort?

---

## Stage 5 — Combined Query (~25 min)

### The complete endpoint

You have built each piece separately. Now bring them together. Here is the final `GetAll` method:

```csharp
[HttpGet]
public async Task<ActionResult<EmployeePage>> GetAll(
    [FromQuery] EmployeeQueryParameters parameters)
{
    IQueryable<Employee> query = _context.Employees
        .AsNoTracking()
        .Include(e => e.Department);

    if (parameters.DepartmentId.HasValue)
    {
        query = query.Where(e => e.DepartmentId == parameters.DepartmentId.Value);
    }

    if (parameters.MinSalary.HasValue)
    {
        query = query.Where(e => e.Salary >= parameters.MinSalary.Value);
    }

    if (!string.IsNullOrWhiteSpace(parameters.Search))
    {
        var term = parameters.Search.Trim();
        query = query.Where(e =>
            e.Name.Contains(term) || e.Email.Contains(term));
    }

    var descending = string.Equals(
        parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

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

    var totalCount = await query.CountAsync();

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

    return Ok(new EmployeePage
    {
        Items = items,
        PageNumber = parameters.PageNumber,
        PageSize = parameters.PageSize,
        TotalCount = totalCount
    });
}
```

### The pipeline order

```
Start IQueryable (AsNoTracking + Include)
       |
       v
  Filtering (Where)
       |
       v
  Searching (Where with Contains)
       |
       v
  Sorting (OrderBy / OrderByDescending)
       |
       v
  Count (CountAsync)
       |
       v
  Pagination (Skip / Take)
       |
       v
  Projection (Select)
       |
       v
  Execute (ToListAsync)
```

### Why this order matters

**Filters before sort.** Sorting 100,000 rows then filtering to 50 wastes effort. Filtering first reduces the dataset before sorting.

**Sort before pagination.** `Skip` and `Take` operate on the ordered sequence. If you paginate before sorting, you get arbitrary rows — the "first 10" in whatever order the database returns them.

**Count before Skip/Take.** You need the total count of matching rows, not the count of rows on the current page.

**Projection (Select) last.** The `Select` is part of the query that EF Core translates. Placing it after `Skip/Take` means the database only projects the columns you need for the current page. If you placed it before filtering, EF Core would still produce correct SQL, but keeping it at the end makes the code easier to read.

**ToListAsync last.** This is the single moment the query executes. Everything before it is expression building.

### Test the full URL in Swagger

```
GET /api/employees?search=ali&departmentId=2&minSalary=3000&sortBy=salary&sortOrder=desc&pageNumber=1&pageSize=10
```

This single request:
- Filters to department 2
- Filters to salary >= 3000
- Searches for "ali" in name or email
- Sorts by salary descending
- Returns page 1 with 10 results
- Includes the total count and total pages

All of this is one SQL query (plus one `COUNT` query) sent to the database. Not one row more is loaded into memory.

### Exercise

Complete Exercise 5 in `Exercises/Day-4-Exercises.md`.

### Common mistakes

**Mistake 1: Mixing up the order**

Calling `ToListAsync()` after building filters but before sorting means the sort runs in memory on the materialized list. The data is correct, but the database did the work of loading everything.

**Mistake 2: Forgetting the Include when paginating**

If you forget `.Include(e => e.Department)`, the `DepartmentName` in the DTO will always be null. The `Select` projection needs the navigation property to be loaded.

**Mistake 3: Changing the return type and forgetting to update Swagger**

After changing from `ActionResult<IEnumerable<EmployeeDto>>` to `ActionResult<EmployeePage>`, restart the application so Swagger regenerates the API documentation.

### Senior developer note

This endpoint handles the 95% case well. If your dataset grows to millions of rows and you see slow responses on high page numbers, measure where the time goes. Use SQL Server Profiler or EF Core logging to see the generated SQL. Add indexes for the columns you filter and sort by most often. The query pattern stays the same — only the infrastructure around it changes.

---

## Debugging Scenarios (~15 min)

Work through each scenario. Read the symptom, think about the cause, then check the explanation.

### Scenario 1: The filter does not work

**Symptom:** You added a `Where` clause, but the endpoint still returns all employees. The filter seems to have no effect.

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll(int? departmentId)
{
    var employees = await _context.Employees
        .Include(e => e.Department)
        .ToListAsync();

    if (departmentId.HasValue)
    {
        employees = employees
            .Where(e => e.DepartmentId == departmentId.Value)
            .ToList();
    }

    return Ok(employees);
}
```

**Think:** What is the type of `employees` after the first line? When does the database query execute?

<details>

**Explanation:**

`ToListAsync()` executes the query immediately and loads all employees into a `List<Employee>`. The subsequent `.Where()` runs in memory using LINQ-to-Objects, not LINQ-to-Entities. The filter works correctly, but it runs on the application server after loading all data.

The fix: use `IQueryable` and call `ToListAsync()` only after all filters are applied.

```csharp
IQueryable<Employee> query = _context.Employees.Include(e => e.Department);

if (departmentId.HasValue)
{
    query = query.Where(e => e.DepartmentId == departmentId.Value);
}

var employees = await query.ToListAsync();
```

</details>

### Scenario 2: pageNumber = 0 crashes

**Symptom:** A client sends `GET /api/employees?pageNumber=0&pageSize=10`. The API returns a 500 error.

**Think:** What does `Skip((0 - 1) * 10)` produce? Is this a valid argument for `Skip()`?

<details>

**Explanation:**

`Skip(-10)` throws an `ArgumentOutOfRangeException`. The fix is to validate `PageNumber` using data annotations:

```csharp
[Range(1, int.MaxValue)]
public int PageNumber { get; set; } = 1;
```

With `[ApiController]`, the framework returns `400 Bad Request` automatically before your action runs.

</details>

### Scenario 3: sortBy=DepartmentName crashes

**Symptom:** A client sends `GET /api/employees?sortBy=DepartmentName`. The API returns a 500 error or ignores the sort.

**Think:** Does your switch expression handle `"departmentname"`? What should the API do when the client requests an unsupported sort column?

<details>

**Explanation:**

Your switch expression maps `"name"`, `"email"`, and `"salary"`. `"departmentname"` hits the default case and sorts by name. The API does not crash, but the client might not realize their sort was ignored.

You have two options:
1. **Silent default** (current approach): unknown values sort by name. Simple, safe.
2. **Explicit rejection**: check `sortBy` against the allowed list and return `400` if it does not match.

Both are valid. Document your choice in the API specification.

</details>

### Scenario 4: Pagination returns duplicate and missing rows

**Symptom:** The client requests page 1, then page 2. Employee #15 appears on both pages. Employee #22 never appears on any page.

**Think:** What determines which row appears on which page? Is your sort deterministic?

<details>

**Explanation:**

Without a deterministic sort, SQL Server can return rows in any order. The order might change between the page 1 query and the page 2 query, especially if inserts or updates happen in between.

The fix: always apply a sort before `Skip/Take`, and make sure the sort is deterministic. If the primary sort column has duplicates (e.g., multiple employees named "Ali"), add a tiebreaker:

```csharp
query = query
    .OrderBy(e => e.Name)
    .ThenBy(e => e.Id);
```

`Id` is unique, so the combination of `Name` and `Id` produces a deterministic order.

</details>

---

## Summary

### What you built

You started with a simple endpoint that returned all employees. Over five stages, you added:

| Stage | Capability | Key concept |
|-------|-----------|-------------|
| 1 | Filtering | `IQueryable`, conditional `Where`, `AsNoTracking` |
| 2 | Searching | `.Contains()` → SQL `LIKE` |
| 3 | Sorting | Explicit switch mapping, default sort |
| 4 | Pagination | `Skip`, `Take`, `CountAsync`, `EmployeePage` |
| 5 | Combined | Pipeline order, single endpoint |

### The complete query pipeline

```
Start IQueryable (AsNoTracking + Include)
       |
       v
  Filtering (Where)           -- narrows the dataset
       |
       v
  Searching (Where/Contains)  -- narrows further with text match
       |
       v
  Sorting (OrderBy)           -- orders the filtered results
       |
       v
  Count (CountAsync)          -- total matching rows
       |
       v
  Pagination (Skip/Take)      -- extracts one page
       |
       v
  Projection (Select)         -- maps to DTO
       |
       v
  Execute (ToListAsync)       -- sends SQL to database
```

This order is not arbitrary. Each step feeds the next. Filters reduce the data before sorting. Sorting orders the data before pagination. Counting happens before pagination so you know the total. Projection happens last so the database only shapes the rows you actually return.

### What you learned

- `IQueryable` is lazy. The query does not execute until you call a method like `ToListAsync()`.
- Filters, searches, sorts, and pagination all build on the same `IQueryable`. They compose naturally.
- `AsNoTracking()` improves performance for read-only queries.
- Always sort before paginating. Always use a deterministic sort.
- The database should do the heavy lifting. Push as much work to SQL Server as you can.

### Preview of Day 5

Tomorrow you will add validation and error handling. What happens if the client sends an empty name when creating an employee? What if the email is malformed? What if a department does not exist? You will learn how to validate input, return meaningful error responses, and handle exceptions gracefully.
