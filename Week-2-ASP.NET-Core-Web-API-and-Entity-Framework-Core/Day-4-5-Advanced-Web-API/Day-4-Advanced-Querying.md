# Day 4 — Filtering, Searching, Sorting, and Pagination

**Duration:** approximately 3 hours

## 1. Query parameters

Create one focused model instead of adding many unrelated controller parameters:

```csharp
using System.ComponentModel.DataAnnotations;

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

ASP.NET Core binds `?pageNumber=2&pageSize=10` to this object. `[ApiController]` then turns invalid model state into a `400 Bad Request` before the action runs.

## 2. Filtering

Filtering narrows the database query:

```csharp
if (parameters.DepartmentId.HasValue)
{
    query = query.Where(e => e.DepartmentId == parameters.DepartmentId.Value);
}

if (parameters.MinSalary.HasValue)
{
    query = query.Where(e => e.Salary >= parameters.MinSalary.Value);
}
```

The filters can be combined:

```http
GET /api/employees?departmentId=2&minSalary=3000
```

## 3. Searching

Search only the fields the API promises to support. For this project, name and email are enough:

```csharp
if (!string.IsNullOrWhiteSpace(parameters.Search))
{
    var search = parameters.Search.Trim();
    query = query.Where(e =>
        e.Name.Contains(search) ||
        e.Email.Contains(search));
}
```

```http
GET /api/employees?search=john
```

Do not add full-text search, Elasticsearch, or a dynamic LINQ library for this lesson.

## 4. Sorting

Never pass an arbitrary client string to a property or SQL expression. An explicit switch makes the supported contract clear:

```csharp
var descending = string.Equals(
    parameters.SortOrder,
    "desc",
    StringComparison.OrdinalIgnoreCase);

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

Supported examples:

```http
GET /api/employees?sortBy=name
GET /api/employees?sortBy=salary&sortOrder=desc
```

For a production API, decide whether an unknown `sortBy` should use the documented default or return `400`. Keep that decision explicit and test it. This lesson uses name as the default.

## 5. Pagination

Count before applying `Skip` and `Take`, then apply pagination to the ordered query:

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
```

The response can use one employee-specific model:

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

Pagination prevents an endpoint from returning an unbounded response. `PageSize` has a maximum of 100 so a caller cannot request an unreasonable page.

## 6. Complete endpoint

The following action shows the intended sequence. Adapt the namespace and existing DTO names to the Day 3 project:

```csharp
[HttpGet]
public async Task<ActionResult<EmployeePage>> GetAll(
    [FromQuery] EmployeeQueryParameters parameters)
{
    IQueryable<Employee> query = _context.Employees
        .AsNoTracking()
        .Include(e => e.Department);

    if (parameters.DepartmentId.HasValue)
        query = query.Where(e => e.DepartmentId == parameters.DepartmentId.Value);

    if (parameters.MinSalary.HasValue)
        query = query.Where(e => e.Salary >= parameters.MinSalary.Value);

    if (!string.IsNullOrWhiteSpace(parameters.Search))
    {
        var search = parameters.Search.Trim();
        query = query.Where(e =>
            e.Name.Contains(search) || e.Email.Contains(search));
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

`CountAsync()` and the page query are two required results: the total number of matches and the current page. Do not call `ToListAsync()` before filtering, sorting, and pagination.

## Practice and review

Test these combinations in Swagger:

```http
GET /api/employees
GET /api/employees?departmentId=2
GET /api/employees?minSalary=3000
GET /api/employees?search=ali
GET /api/employees?sortBy=salary&sortOrder=desc
GET /api/employees?pageNumber=2&pageSize=10
GET /api/employees?search=ali&departmentId=2&minSalary=3000&sortBy=salary&sortOrder=desc&pageNumber=1&pageSize=10
```

Common mistakes:

- executing the query before adding all clauses;
- allowing an arbitrary sort property;
- forgetting a stable sort before `Skip` and `Take`;
- accepting `pageNumber=0`; and
- returning a raw list without total-count metadata.
