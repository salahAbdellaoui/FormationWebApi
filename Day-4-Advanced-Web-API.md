# Day 4 — Advanced Web API

## Professional Training Course

**Duration:** 4 hours  
**Level:** Intermediate — Building on Days 1-3  
**Prerequisites:** Days 1-3 (ASP.NET Core, C#, EF Core)  
**Framework:** .NET 8 / ASP.NET Core Web API

---

## ًںژ¯ Day 4 Learning Objectives

By the end of this session, you will understand:

- How to design useful API endpoints beyond basic CRUD
- How to implement filtering, searching, sorting, and pagination
- How to validate incoming requests properly
- How to return appropriate HTTP status codes
- How to handle errors consistently with global exception handling
- How to use DTOs to protect your API contract
- How to build a professional, production-style query API
- How API design affects frontend clients and scalability

---

## Training Schedule

| Part | Topic | Duration |
|------|-------|----------|
| 1 | From Basic CRUD to Real APIs | ~30 min |
| 2 | Filtering & Searching | ~45 min |
| 3 | Sorting | ~30 min |
| 4 | Pagination | ~50 min |
| 5 | Validation & HTTP Responses | ~45 min |
| 6 | Error Handling & API Quality | ~30 min |
| 7 | Complete Practical API | ~30 min |

---

# Part 1 — From Basic CRUD to Real APIs

## 1.1 The Problem

> **Instructor Note:** Ask participants — *"If we have an API that returns all employees, what happens when the company has 10 employees?"*

That works fine.

> *"What happens when it has 100,000 employees?"*

Now you have a problem.

### The Basic CRUD Approach

```http
GET /api/employees
```

Returns **every** employee. Every column. Every relationship.

This works for:

- A small demo project
- A prototype
- A table with 50 rows

This fails for:

- Real applications
- Growing data
- Multiple clients with different needs
- Mobile clients on slow networks
- Frontend tables with 10,000 rows

---

## 1.2 What Real APIs Need

A production API needs more than `GET`, `POST`, `PUT`, `DELETE`.

It needs:

```text
Filtering     â†’ "Give me only IT department employees"
Searching     â†’ "Find employees named Ahmed"
Sorting       â†’ "Sort by salary, highest first"
Pagination    â†’ "Give me page 2, 20 results per page"
Validation    â†’ "Reject invalid data before it hits the database"
Error Handling â†’ "Return consistent, useful error responses"
```

### The Progression

```text
Basic CRUD
    â†"
Filtering
    â†"
Searching
    â†"
Sorting
    â†"
Pagination
    â†"
Validation
    â†"
Consistent Errors
    â†"
Production-Ready API
```

> ًں’، **Senior Developer Lesson:** "An API is a contract, not just a collection of controller methods. Frontend developers, mobile developers, and other teams depend on your API behaving consistently."

---

## 1.3 Classroom Question

> ًں§  **Think About It:** "If the frontend wants employees from the IT department with salary above 3000, should we retrieve all employees and filter them in C#?"

**Expected answer:** No. The database should perform this filtering. We learned this in the EF Core session — filter in the database, not in memory.

```csharp
// â‌Œ BAD — loads everything, filters in memory
var all = await context.Employees.ToListAsync();
var filtered = all.Where(e => e.DepartmentId == 3 && e.Salary > 3000).ToList();

// âœ… GOOD — filters in the database
var filtered = await context.Employees
    .Where(e => e.DepartmentId == 3 && e.Salary > 3000)
    .ToListAsync();
```

---

## 1.4 Query Parameters

How do clients send filtering/sorting/pagination information to the API?

### Path Parameters

```http
GET /api/employees/15
```

Used for **specific resources**. The `15` identifies a single employee.

### Query Parameters

```http
GET /api/employees?departmentId=3&isActive=true
```

Used for **filtering, searching, sorting, pagination**. The parameters modify the query.

### Request Body

```json
{
  "name": "Ahmed",
  "salary": 5000
}
```

Used for **create/update** operations. The data is too complex for query parameters.

| Parameter Type | Example | Typical Use |
|----------------|---------|-------------|
| Route | `/employees/10` | Specific resource |
| Query | `?page=2` | Filtering, search, sort, pagination |
| Body | JSON payload | Create, update |

---

## 1.5 ASP.NET Core Model Binding

How does ASP.NET Core read query parameters?

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees(
    [FromQuery] int? departmentId,
    [FromQuery] bool? isActive,
    [FromQuery] decimal? minSalary,
    CancellationToken cancellationToken)
{
    // ...
}
```

The `[FromQuery]` attribute tells ASP.NET Core to read from the query string.

For complex models, you can use a class:

```csharp
[HttpGet]
public async Task<ActionResult<PagedResult<EmployeeDto>>> GetEmployees(
    [FromQuery] EmployeeQueryParameters parameters,
    CancellationToken cancellationToken)
{
    // ASP.NET Core binds query string values to the class properties
}
```

```text
GET /api/employees?departmentId=3&isActive=true&page=1&pageSize=20
```

Binds to:

```csharp
parameters.DepartmentId = 3
parameters.IsActive = true
parameters.Page = 1
parameters.PageSize = 20
```


---

# Part 2 — Filtering & Searching

## 2.1 Filtering

### The Requirement

> "Return only active employees from the IT department."

### The Endpoint

```http
GET /api/employees?departmentId=3&isActive=true
```

### Building the Query Progressively

> **Instructor Note:** Do NOT immediately show a huge LINQ expression. Build it step by step.

**Step 1 — Start with a base query:**

```csharp
var query = context.Employees.AsQueryable();
```

Nothing is executed. This is an `IQueryable` — a query description.

**Step 2 — Add department filter:**

```csharp
if (departmentId.HasValue)
{
    query = query.Where(e => e.DepartmentId == departmentId.Value);
}
```

**Step 3 — Add active filter:**

```csharp
if (isActive.HasValue)
{
    query = query.Where(e => e.IsActive == isActive.Value);
}
```

**Step 4 — Add salary filter:**

```csharp
if (minSalary.HasValue)
{
    query = query.Where(e => e.Salary >= minSalary.Value);
}

if (maxSalary.HasValue)
{
    query = query.Where(e => e.Salary <= maxSalary.Value);
}
```

**Step 5 — Execute:**

```csharp
var employees = await query.ToListAsync(cancellationToken);
```

### Why Build the Query This Way?

```text
Base Query (IQueryable)
   â†"
Filter 1 (if provided)
   â†"
Filter 2 (if provided)
   â†"
Filter 3 (if provided)
   â†"
Execute (only now hits the database)
```

Each `.Where()` adds a condition to the SQL. The database does the filtering.

> ًں’، **Senior Developer Lesson:** "Build the query first. Execute it only after all filters are applied. The IQueryable is a description — it becomes SQL only when you call an execution method."

---

## 2.2 Dynamic Query Construction

The full filtering logic looks like this:

```csharp
public async Task<List<EmployeeDto>> GetEmployeesAsync(
    int? departmentId,
    bool? isActive,
    decimal? minSalary,
    decimal? maxSalary,
    CancellationToken cancellationToken)
{
    var query = context.Employees.AsQueryable();

    if (departmentId.HasValue)
    {
        query = query.Where(e => e.DepartmentId == departmentId.Value);
    }

    if (isActive.HasValue)
    {
        query = query.Where(e => e.IsActive == isActive.Value);
    }

    if (minSalary.HasValue)
    {
        query = query.Where(e => e.Salary >= minSalary.Value);
    }

    if (maxSalary.HasValue)
    {
        query = query.Where(e => e.Salary <= maxSalary.Value);
    }

    return await query
        .Select(e => new EmployeeDto
        {
            Id = e.Id,
            Name = e.Name,
            Email = e.Email,
            Salary = e.Salary,
            DepartmentName = e.Department!.Name
        })
        .ToListAsync(cancellationToken);
}
```

Every filter is optional. If no parameters are provided, the API returns all employees.

---

## 2.3 Searching

### The Requirement

> "The user wants to search employees by name or email."

### The Endpoint

```http
GET /api/employees?search=ahmed
```

### The Implementation

```csharp
if (!string.IsNullOrWhiteSpace(search))
{
    query = query.Where(e =>
        e.Name.Contains(search) ||
        e.Email.Contains(search));
}
```

This generates a SQL `LIKE` query:

```sql
WHERE Name LIKE '%ahmed%' OR Email LIKE '%ahmed%'
```

### Case Sensitivity

SQL Server default collation is usually case-insensitive. So `Contains("ahmed")` matches "Ahmed", "AHMED", "ahmed".

If you need case-sensitive search, you must configure the collation explicitly.

---

## 2.4 Search Performance

> ًں§  **Think About It:** "Would `Contains()` be enough for a Google-like search engine?"

**Answer:** No. `LIKE '%term%'` (leading wildcard) cannot use a standard index. For small-to-medium datasets, this is acceptable. For millions of records, you need:

- Full-text search (SQL Server Full-Text Search)
- Search engines (Elasticsearch, Azure Cognitive Search)
- Proper indexing strategies

For our Employee API, `Contains()` is practical. But understand its limits.

> ًں’، **Senior Developer Lesson:** "Simple database search and full-text search are different problems. Know which one you are solving."

---

## 2.5 Combined Filtering + Searching

```csharp
var query = context.Employees.AsQueryable();

// Filtering
if (departmentId.HasValue)
    query = query.Where(e => e.DepartmentId == departmentId.Value);

if (isActive.HasValue)
    query = query.Where(e => e.IsActive == isActive.Value);

if (minSalary.HasValue)
    query = query.Where(e => e.Salary >= minSalary.Value);

// Searching
if (!string.IsNullOrWhiteSpace(search))
    query = query.Where(e =>
        e.Name.Contains(search) ||
        e.Email.Contains(search));

// Execute
var employees = await query.ToListAsync(cancellationToken);
```

All conditions are combined with `AND` in the generated SQL.

```sql
WHERE DepartmentId = 3
  AND IsActive = 1
  AND Salary >= 3000
  AND (Name LIKE '%ahmed%' OR Email LIKE '%ahmed%')
```


---

# Part 3 — Sorting

## 3.1 The Requirement

> "The user wants to sort employees by salary, highest first."

### The Endpoint

```http
GET /api/employees?sortBy=salary&sortDirection=desc
```

---

## 3.2 Basic Sorting

### Ascending (Default)

```csharp
query = sortBy?.ToLower() switch
{
    "name" => query.OrderBy(e => e.Name),
    "salary" => query.OrderBy(e => e.Salary),
    "email" => query.OrderBy(e => e.Email),
    _ => query.OrderBy(e => e.Id)
};
```

### Descending

```csharp
if (sortDirection?.ToLower() == "desc")
{
    query = sortBy?.ToLower() switch
    {
        "name" => query.OrderByDescending(e => e.Name),
        "salary" => query.OrderByDescending(e => e.Salary),
        "email" => query.OrderByDescending(e => e.Email),
        _ => query.OrderByDescending(e => e.Id)
    };
}
else
{
    query = sortBy?.ToLower() switch
    {
        "name" => query.OrderBy(e => e.Name),
        "salary" => query.OrderBy(e => e.Salary),
        "email" => query.OrderBy(e => e.Email),
        _ => query.OrderBy(e => e.Id)
    };
}
```

---

## 3.3 Safe Sorting with a Whitelist

> ًںں¥ **Warning:** Never accept arbitrary property names from clients and inject them into queries.

### Why This Is Dangerous

```csharp
// â‌Œ NEVER DO THIS — dynamic reflection-based sorting from user input
var property = typeof(Employee).GetProperty(sortBy);
query = query.OrderBy(e => property.GetValue(e));
```

This can:

- Expose internal property names
- Cause exceptions on invalid input
- Be exploited for information disclosure

### The Safe Approach: Whitelist

```csharp
private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
{
    "id", "name", "email", "salary", "departmentname"
};

public IQueryable<Employee> ApplySorting(IQueryable<Employee> query, string? sortBy, string? sortDirection)
{
    if (string.IsNullOrWhiteSpace(sortBy) || !AllowedSortFields.Contains(sortBy))
    {
        return query.OrderBy(e => e.Id);
    }

    bool descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

    return sortBy.ToLower() switch
    {
        "name" => descending
            ? query.OrderByDescending(e => e.Name)
            : query.OrderBy(e => e.Name),
        "salary" => descending
            ? query.OrderByDescending(e => e.Salary)
            : query.OrderBy(e => e.Salary),
        "email" => descending
            ? query.OrderByDescending(e => e.Email)
            : query.OrderBy(e => e.Email),
        _ => query.OrderBy(e => e.Id)
    };
}
```

> ًں’، **Senior Developer Lesson:** "Never trust client-provided query parameters. Always validate and map them to known, allowed values."

---

## 3.4 Instructor Interaction

> ًں§  **Think About It:** "What if the client sends `sortBy=password`? What should the API do?"
>
> **Answer:** Ignore it. Fall back to the default sort (e.g., by Id). Never throw an error that reveals what fields exist.


---

# Part 4 — Pagination

This is one of the most important sections of the entire course.

## 4.1 The Problem

> **Instructor Note:** Ask participants — *"Our API now returns 100,000 employees. What should we do?"*

You cannot return 100,000 records in a single response. The client does not need them all at once. The database does not need to send them all at once.

---

## 4.2 What Is Pagination?

Instead of returning everything, return a **page** of results.

```http
GET /api/employees?page=1&pageSize=20
```

```text
Page 1 â†’ Employees 1—"20
Page 2 â†’ Employees 21—"40
Page 3 â†’ Employees 41—"60
```

The client requests one page at a time. The server returns only that page.

---

## 4.3 Offset Pagination with Skip/Take

### The Formula

```text
Skip = (page - 1) أ— pageSize
```

### Concrete Examples

| Page | PageSize | Skip | Take |
|------|----------|------|------|
| 1 | 20 | 0 | 20 |
| 2 | 20 | 20 | 20 |
| 3 | 20 | 40 | 20 |
| 1 | 10 | 0 | 10 |
| 5 | 10 | 40 | 10 |

### Implementation

```csharp
var totalCount = await query.CountAsync(cancellationToken);

var employees = await query
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(cancellationToken);
```

**Step 1:** Count total matching records (for metadata).  
**Step 2:** Skip to the right page. Take only the page size.

---

## 4.4 Pagination Validation

> ًںں¥ **Warning:** Never trust client-provided pagination values.

### Dangerous Inputs

```http
?page=0&pageSize=-10
?page=1&pageSize=1000000
?page=-5&pageSize=20
```

### Safe Defaults

```csharp
page = Math.Max(page, 1);
pageSize = Math.Clamp(pageSize, 1, 100);
```

| Input | After Validation |
|-------|------------------|
| page=0 | page=1 |
| page=-5 | page=1 |
| pageSize=-10 | pageSize=1 |
| pageSize=1000000 | pageSize=100 |

> ًں’، **Senior Developer Lesson:** "Pagination is not only a UI feature. It is a scalability feature. Without it, a single request can bring down your database."

---

## 4.5 Pagination Response Design

Do not return only a raw list. The client needs metadata.

### The Response Model

```csharp
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}
```

### The JSON Response

```json
{
  "items": [
    { "id": 21, "name": "Ahmed", "salary": 5000 },
    { "id": 22, "name": "Sara", "salary": 4500 }
  ],
  "page": 2,
  "pageSize": 20,
  "totalCount": 145,
  "totalPages": 8
}
```

### Why Metadata Matters

- **Page:** "What page am I on?"
- **PageSize:** "How many items per page?"
- **TotalCount:** "How many total results exist?"
- **TotalPages:** "How many pages are there?"

The frontend uses this to render pagination controls (next/previous buttons, page numbers).

---

## 4.6 Implementation

```csharp
public async Task<PagedResult<EmployeeDto>> GetEmployeesAsync(
    EmployeeQueryParameters parameters,
    CancellationToken cancellationToken)
{
    var query = context.Employees.AsQueryable();

    // Filtering, searching, sorting applied here...

    // Count FIRST (before pagination)
    var totalCount = await query.CountAsync(cancellationToken);

    // Then paginate
    var items = await query
        .Skip((parameters.Page - 1) * parameters.PageSize)
        .Take(parameters.PageSize)
        .Select(e => new EmployeeDto
        {
            Id = e.Id,
            Name = e.Name,
            Email = e.Email,
            Salary = e.Salary,
            DepartmentName = e.Department!.Name
        })
        .ToListAsync(cancellationToken);

    return new PagedResult<EmployeeDto>
    {
        Items = items,
        Page = parameters.Page,
        PageSize = parameters.PageSize,
        TotalCount = totalCount,
        TotalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize)
    };
}
```

---

## 4.7 Instructor Interaction

> ًں§  **Think About It:** "Why do we count BEFORE paginating?"
>
> **Answer:** `CountAsync()` counts all matching records (with filters applied, but before Skip/Take). This gives us the total for the metadata. If we counted after pagination, we would only count the current page's records.

---

## 4.8 Pagination Order Matters

```text
Base Query (IQueryable)
   â†"
Filtering (Where)
   â†"
Searching (Where)
   â†"
Sorting (OrderBy)
   â†"
Count (CountAsync)     â†گ must be before Skip/Take
   â†"
Pagination (Skip/Take)
   â†"
Projection (Select)
   â†"
Execution (ToListAsync)
```

> âڑ ï¸ڈ **Important:** If you apply Skip/Take before Count, the Count will be wrong. Always count the filtered query, then paginate.

---

## 4.9 Offset vs Keyset Pagination

### Offset Pagination (What We Built)

```text
Skip + Take
```

**Pros:** Simple, supports "jump to page N".  
**Cons:** Can be slow on very large offsets (e.g., `SKIP 1000000`).

### Keyset / Cursor Pagination (Advanced)

```text
WHERE Id > lastSeenId
```

**Pros:** Consistent performance regardless of page depth.  
**Cons:** Cannot jump to arbitrary pages. More complex to implement.

For most APIs, offset pagination is sufficient. Keyset pagination is an optimization for specific high-volume scenarios.

> ًں’، **Senior Developer Lesson:** "There is more than one pagination strategy. Know when offset is enough and when keyset is necessary."


---

# Part 5 — Validation & HTTP Responses

## 5.1 The Problem

> **Instructor Note:** Ask participants — *"If a user sends this to create an employee, what should happen?"*

```json
{
  "name": "",
  "email": "not-an-email",
  "salary": -500
}
```

Should the database be the first place that rejects this request?

No. The API should validate **before** anything touches the database.

---

## 5.2 DTO Validation with Data Annotations

### CreateEmployeeRequest

```csharp
public class CreateEmployeeRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Range(0, 1_000_000, ErrorMessage = "Salary must be between 0 and 1,000,000.")]
    public decimal Salary { get; set; }

    [Required(ErrorMessage = "Department is required.")]
    public int DepartmentId { get; set; }
}
```

### Common Validation Attributes

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `[Required]` | Value must be provided | Name cannot be empty |
| `[MaxLength(n)]` | Maximum string length | Max 100 characters |
| `[MinLength(n)]` | Minimum string length | At least 2 characters |
| `[Range(min, max)]` | Numeric range | Salary between 0 and 1M |
| `[EmailAddress]` | Valid email format | Must be a valid email |
| `[RegularExpression]` | Custom pattern | Phone number format |

---

## 5.3 Syntactic vs Business Validation

### Syntactic Validation

> "Is the email in a valid format?"

This is checking the **shape** of the data. Data Annotations handle this well.

### Business Validation

> "Does the department exist? Is the employee email already in use?"

This is checking **business rules**. Data Annotations cannot handle this — you need code.

```csharp
// Business validation: department must exist
var departmentExists = await context.Departments
    .AnyAsync(d => d.Id == request.DepartmentId, cancellationToken);

if (!departmentExists)
{
    return BadRequest(new { Error = "Department does not exist." });
}

// Business validation: email must be unique
var emailExists = await context.Employees
    .AnyAsync(e => e.Email == request.Email, cancellationToken);

if (emailExists)
{
    return Conflict(new { Error = "An employee with this email already exists." });
}
```

> ًں’، **Senior Developer Lesson:** "Not every validation rule belongs in attributes. Data Annotations are for syntactic validation. Business rules need code."

---

## 5.4 Automatic Model Validation with [ApiController]

When you use `[ApiController]` on a controller, ASP.NET Core automatically validates the request model.

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(
        CreateEmployeeRequest request)
    {
        // If request is invalid, ASP.NET Core returns 400 automatically
        // This code only runs if validation passes
        // ...
    }
}
```

### Validation Error Response

If validation fails, ASP.NET Core returns HTTP 400:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["Name is required."],
    "Email": ["Invalid email format."],
    "Salary": ["Salary must be between 0 and 1,000,000."]
  }
}
```

---

## 5.5 FluentValidation (Professional Alternative)

For complex validation rules, teams often use FluentValidation.

```bash
dotnet add package FluentValidation.AspNetCore
```

### Example Validator

```csharp
public class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Salary)
            .InclusiveBetween(0, 1_000_000)
            .WithMessage("Salary must be between 0 and 1,000,000.");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("Department ID must be greater than 0.");
    }
}
```

### Why FluentValidation?

| Data Annotations | FluentValidation |
|-----------------|------------------|
| Attributes on DTO | Separate validator class |
| Simple rules | Complex conditional rules |
| Limited | Full power of C# |
| Tight coupling | Clean separation |

> ًں’، **Senior Developer Lesson:** "Use Data Annotations for simple cases. Use FluentValidation when rules become complex or when you want to separate validation from the DTO."

---

## 5.6 HTTP Status Codes

Status codes communicate the **result** of an operation.

| Status | Meaning | When to Use |
|--------|---------|-------------|
| 200 | OK | Successful GET, successful PUT |
| 201 | Created | Successful POST |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Invalid input, validation failure |
| 401 | Unauthorized | Missing or invalid authentication |
| 403 | Forbidden | Authenticated but not allowed |
| 404 | Not Found | Resource does not exist |
| 409 | Conflict | Business/data conflict (duplicate email) |
| 422 | Unprocessable Entity | Semantically invalid |
| 500 | Internal Server Error | Unexpected server error |

---

## 5.7 Classroom Question

> ًں§  **Think About It:** "If the user asks for employee ID 9999 and it does not exist, should we return 200 with null or 404?"
>
> **Answer:** It depends on the API contract, but resource-oriented endpoints commonly return 404. The resource does not exist — that is a meaningful status code. Returning 200 with null forces the client to check for null, which is less clear.

---

## 5.8 Returning Status Codes in ASP.NET Core

```csharp
// 200 OK
return Ok(employeeDto);

// 201 Created
return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employeeDto);

// 204 No Content
return NoContent();

// 400 Bad Request
return BadRequest(new { Error = "Invalid data" });

// 404 Not Found
return NotFound(new { Error = "Employee not found" });

// 409 Conflict
return Conflict(new { Error = "Email already exists" });
```

---

## 5.9 CreatedAtAction

When creating a resource, return 201 with a `Location` header:

```csharp
[HttpPost]
public async Task<ActionResult<EmployeeDto>> CreateEmployee(
    CreateEmployeeRequest request,
    CancellationToken cancellationToken)
{
    // ... create employee ...

    return CreatedAtAction(
        nameof(GetEmployee),
        new { id = employee.Id },
        employeeDto);
}
```

This returns:

```http
HTTP/1.1 201 Created
Location: /api/employees/42
```

The client knows where to find the newly created resource.


---

# Part 6 — Error Handling & API Quality

## 6.1 The Problem

> **Instructor Note:** Ask participants — *"What happens when the database fails? Or when an unexpected exception occurs?"*

### A Bad Approach

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
{
    try
    {
        var employee = await context.Employees.FindAsync(id);

        if (employee is null)
            return NotFound();

        return Ok(employee);
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);  // â‌Œ Wrong status code + leaks details
    }
}
```

### Why This Is Problematic

| Problem | Explanation |
|---------|-------------|
| Leaks implementation details | `ex.Message` may contain SQL connection strings, stack traces |
| Wrong status code | A database error is 500, not 400 |
| Duplicated code | Every action has the same try/catch |
| Inconsistent responses | Some errors return `BadRequest`, others return `StatusCode(500)` |
| Security risk | Exception details can reveal internal architecture |

> ًں’، **Senior Developer Lesson:** "Exceptions are for exceptional situations, not normal control flow. And they should never be exposed to clients in production."

---

## 6.2 What Should Happen

```text
Request
   â†"
Controller
   â†"
Exception occurs
   â†"
Global Exception Handler catches it
   â†"
Logs the detailed exception (server-side)
   â†"
Returns a safe, generic response to the client
   â†"
Client receives a consistent error format
```

The client gets a useful message. The server logs the details for debugging.

---

## 6.3 Global Exception Handling with ProblemDetails

### ProblemDetails — The Standard

`ProblemDetails` is an RFC 7807-compliant error response format.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred. Please try again later."
}
```

### Implementing with IExceptionHandler (.NET 8)

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred. Please try again later.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
```

### Register in Program.cs

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
```

---

## 6.4 What the Client Sees

### Database Connection Failure

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred. Please try again later."
}
```

### What the Server Logs

```
2024-01-15 10:30:45 [ERROR] GlobalExceptionHandler - Exception occurred: 
Cannot open database "EmployeeDb" requested by the login. The login failed.
System.Data.SqlClient.SqlException (0x80131904): Cannot open database...
```

The client gets a safe message. The server logs the full exception for debugging.

---

## 6.5 Common Exception Types

| Exception | Likely Cause | Appropriate Response |
|-----------|-------------|---------------------|
| `InvalidOperationException` | Business rule violation | 400 or 409 |
| `SqlException` | Database error | 500 |
| `TaskCanceledException` | Request timeout | 504 |
| `NullReferenceException` | Bug in code | 500 |
| `ValidationException` | Invalid data | 400 |

---

## 6.6 Returning Consistent Errors

> ًںں¥ **Important:** Never return `ex.ToString()` to clients.

```csharp
// â‌Œ NEVER in production
return StatusCode(500, ex.ToString());

// âœ… Safe
return StatusCode(500, new ProblemDetails
{
    Title = "Internal Server Error",
    Detail = "An unexpected error occurred."
});
```

### Why?

- Exception stack traces reveal internal architecture
- SQL error messages reveal database structure
- File paths reveal server configuration
- This is a security vulnerability

---

## 6.7 API Design — DTOs

> **Instructor Note:** Reinforce why controllers should not expose EF Core entities.

### Bad

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
{
    return Ok(await context.Employees.ToListAsync());
}
```

This exposes:

- Internal database IDs
- Navigation properties (potentially circular references)
- Properties the client does not need
- EF Core tracking overhead

### Better

```csharp
[HttpGet]
public async Task<ActionResult<PagedResult<EmployeeDto>>> GetEmployees(
    [FromQuery] EmployeeQueryParameters parameters,
    CancellationToken cancellationToken)
{
    var result = await _employeeService.GetEmployeesAsync(parameters, cancellationToken);
    return Ok(result);
}
```

The API returns exactly what the client needs.

---

## 6.8 Thin Controllers

Controllers should **coordinate** requests, not contain business logic.

### Fat Controller (Bad)

```text
Controller
 â"œâ"€â"€ validation logic
 â"œâ"€â"€ business rules
 â"œâ"€â"€ database queries
 â"œâ"€â"€ object mapping
 â"œâ"€â"€ error handling
 â"œâ"€â"€ filtering logic
 â""â"€â"€ calculations
```

### Thin Controller (Better)

```text
Controller
    â†" (delegates to)
Application Service
    â†" (uses)
Data Access (EF Core)
    â†" (queries)
SQL Server
```

> ًں’، **Senior Developer Lesson:** "A controller that knows too much is usually a design problem. Tomorrow we will learn Clean Architecture to formalize these boundaries."


---

# Part 7 — Complete Practical API

## 7.1 Employee Query Parameters

```csharp
public class EmployeeQueryParameters
{
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
```

| Property | Purpose | Example |
|----------|---------|---------|
| `Search` | Search by name or email | `search=ahmed` |
| `DepartmentId` | Filter by department | `departmentId=3` |
| `IsActive` | Filter by status | `isActive=true` |
| `MinSalary` | Minimum salary filter | `minSalary=3000` |
| `MaxSalary` | Maximum salary filter | `maxSalary=8000` |
| `SortBy` | Sort field | `sortBy=salary` |
| `SortDirection` | Sort direction | `sortDirection=desc` |
| `Page` | Page number (default 1) | `page=2` |
| `PageSize` | Items per page (default 20) | `pageSize=10` |

---

## 7.2 The Complete Query Pipeline

```text
HTTP Query Parameters
        â†"
Model Binding (ASP.NET Core)
        â†"
Validation / Normalization
        â†"
Base IQueryable<Employee>
        â†"
Filtering (Where)
        â†"
Searching (Where)
        â†"
Sorting (OrderBy)
        â†"
Count (CountAsync)
        â†"
Pagination (Skip/Take)
        â†"
Projection (Select)
        â†"
Execution (ToListAsync)
        â†"
PagedResult<EmployeeDto>
        â†"
JSON Response
```

> ًں§  **Think About It:** "Why does the order of this pipeline matter?"
>
> **Answer:** Filtering before counting ensures the count is correct. Sorting before pagination ensures consistent page contents. Projection at the end ensures we only transfer needed data.

---

## 7.3 Complete Employee Service

```csharp
public class EmployeeService
{
    private readonly ApplicationDbContext _context;

    public EmployeeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<EmployeeDto>> GetEmployeesAsync(
        EmployeeQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        // Normalize pagination
        int page = Math.Max(parameters.Page, 1);
        int pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        // Base query
        var query = _context.Employees.AsQueryable();

        // Filtering
        if (parameters.DepartmentId.HasValue)
            query = query.Where(e => e.DepartmentId == parameters.DepartmentId.Value);

        if (parameters.IsActive.HasValue)
            query = query.Where(e => e.IsActive == parameters.IsActive.Value);

        if (parameters.MinSalary.HasValue)
            query = query.Where(e => e.Salary >= parameters.MinSalary.Value);

        if (parameters.MaxSalary.HasValue)
            query = query.Where(e => e.Salary <= parameters.MaxSalary.Value);

        // Searching
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();
            query = query.Where(e =>
                e.Name.Contains(search) ||
                e.Email.Contains(search));
        }

        // Sorting
        query = ApplySorting(query, parameters.SortBy, parameters.SortDirection);

        // Count (before pagination)
        var totalCount = await query.CountAsync(cancellationToken);

        // Pagination + Projection
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                Salary = e.Salary,
                DepartmentName = e.Department!.Name
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<EmployeeDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "id", "name", "email", "salary"
    };

    private IQueryable<Employee> ApplySorting(
        IQueryable<Employee> query, string? sortBy, string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortBy) || !AllowedSortFields.Contains(sortBy))
            return query.OrderBy(e => e.Id);

        bool descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLower() switch
        {
            "name" => descending
                ? query.OrderByDescending(e => e.Name)
                : query.OrderBy(e => e.Name),
            "salary" => descending
                ? query.OrderByDescending(e => e.Salary)
                : query.OrderBy(e => e.Salary),
            "email" => descending
                ? query.OrderByDescending(e => e.Email)
                : query.OrderBy(e => e.Email),
            _ => query.OrderBy(e => e.Id)
        };
    }
}
```

---

## 7.4 Complete Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _employeeService;
    private readonly ApplicationDbContext _context;

    public EmployeesController(EmployeeService employeeService, ApplicationDbContext context)
    {
        _employeeService = employeeService;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<EmployeeDto>>> GetEmployees(
        [FromQuery] EmployeeQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetEmployeesAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(
        int id,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .Where(e => e.Id == id)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                Salary = e.Salary,
                DepartmentName = e.Department!.Name
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (employee is null)
            return NotFound(new { Error = $"Employee with ID {id} not found." });

        return Ok(employee);
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        // Business validation
        var departmentExists = await _context.Departments
            .AnyAsync(d => d.Id == request.DepartmentId, cancellationToken);

        if (!departmentExists)
            return BadRequest(new { Error = "Department does not exist." });

        var emailExists = await _context.Employees
            .AnyAsync(e => e.Email == request.Email, cancellationToken);

        if (emailExists)
            return Conflict(new { Error = "An employee with this email already exists." });

        var employee = new Employee
        {
            Name = request.Name,
            Email = request.Email,
            Salary = request.Salary,
            DepartmentId = request.DepartmentId,
            IsActive = true
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Salary = employee.Salary,
            DepartmentName = (await _context.Departments.FindAsync(request.DepartmentId))!.Name
        };

        return CreatedAtAction(
            nameof(GetEmploy

---

# Debugging Challenge

## Find the Problems

The following code contains several realistic mistakes. Find all of them.

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmployeesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var employees = await _context.Employees.ToListAsync();
            return Ok(employees);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string term)
    {
        var all = await _context.Employees.ToListAsync();
        var results = all.Where(e => e.Name.Contains(term)).ToList();
        return Ok(results);
    }

    [HttpGet("sorted")]
    public async Task<IActionResult> GetSorted(string sortBy, string direction)
    {
        var employees = await _context.Employees.ToListAsync();

        if (direction == "desc")
            employees = employees.OrderByDescending(e => e.Name).ToList();
        else
            employees = employees.OrderBy(e => e.Name).ToList();

        return Ok(employees);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return Ok(employee);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        return Ok(employee);
    }
}
```

---

### What Is Wrong?

**Problem 1 — `GetAll`:** Returns ALL employees with ALL columns. No filtering, no pagination, no projection.

**Problem 2 — `GetAll`:** Catches all exceptions and returns 400 (Bad Request). Database errors should be 500. Exception details are leaked.

**Problem 3 — `Search`:** Loads ALL employees into memory, then filters in C#. Should filter in the database with `Where()`.

**Problem 4 — `Sorted`:** Same problem — loads everything, sorts in memory.

**Problem 5 — `Sorted`:** Accepts arbitrary `sortBy` with no whitelist. Security risk.

**Problem 6 — `Create`:** Returns the EF entity directly. Exposes internal properties. Returns 200 instead of 201.

**Problem 7 — `Create`:** No validation at all.

**Problem 8 — `GetById`:** Returns 200 with null when employee not found. Should return 404.

---

### Corrected Code

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _employeeService;
    private readonly ApplicationDbContext _context;

    public EmployeesController(EmployeeService employeeService, ApplicationDbContext context)
    {
        _employeeService = employeeService;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<EmployeeDto>>> GetEmployees(
        [FromQuery] EmployeeQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetEmployeesAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(
        int id,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .Where(e => e.Id == id)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                Salary = e.Salary,
                DepartmentName = e.Department!.Name
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (employee is null)
            return NotFound();

        return Ok(employee);
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // ... create logic ...
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, dto);
    }
}
```


---

# Practical Exercises

## ًںں¢ Beginner — Exercise 1: Active Filter

### Problem

Add an `isActive` filter to the GET endpoint. When `isActive=true` is in the query string, return only active employees.

### Starting Point

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees(
    [FromQuery] bool? isActive,
    CancellationToken cancellationToken)
{
    var query = _context.Employees.AsQueryable();

    // Add your filter here

    var employees = await query
        .Select(e => new EmployeeDto { Id = e.Id, Name = e.Name })
        .ToListAsync(cancellationToken);

    return Ok(employees);
}
```

### Hints

- Check if `isActive.HasValue`
- Use `.Where(e => e.IsActive == isActive.Value)`
- The filter is optional — if not provided, return all

### Instructor Solution

```csharp
if (isActive.HasValue)
    query = query.Where(e => e.IsActive == isActive.Value);
```

### Common Mistakes

- Filtering in memory after `ToListAsync()`
- Forgetting that `bool` defaults to `false` — use `bool?`

---

## ًںں¢ Beginner — Exercise 2: Search by Name

### Problem

Add a `search` parameter. When provided, return employees whose name contains the search term.

### Starting Point

```csharp
[HttpGet("search")]
public async Task<ActionResult<IEnumerable<EmployeeDto>>> SearchEmployees(
    [FromQuery] string? search,
    CancellationToken cancellationToken)
{
    // Add your search logic
    throw new NotImplementedException();
}
```

### Instructor Solution

```csharp
var query = _context.Employees.AsQueryable();

if (!string.IsNullOrWhiteSpace(search))
    query = query.Where(e => e.Name.Contains(search));

var employees = await query
    .Select(e => new EmployeeDto { Id = e.Id, Name = e.Name })
    .ToListAsync(cancellationToken);

return Ok(employees);
```

---

## ًںں¢ Beginner — Exercise 3: Salary Range

### Problem

Add `minSalary` and `maxSalary` filters. When provided, return employees within the salary range.

### Instructor Solution

```csharp
if (minSalary.HasValue)
    query = query.Where(e => e.Salary >= minSalary.Value);

if (maxSalary.HasValue)
    query = query.Where(e => e.Salary <= maxSalary.Value);
```

---

## ًںں، Intermediate — Exercise 4: Sorting

### Problem

Add `sortBy` and `sortDirection` parameters. Support sorting by `name` and `salary`. Default to sorting by `id`.

### Starting Point

```csharp
// You have a query. Apply sorting based on parameters.
var query = _context.Employees.AsQueryable();
```

### Hints

- Use a switch expression
- Validate `sortBy` against allowed values
- Check `sortDirection` for "desc"

### Instructor Solution

```csharp
private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
{
    "name", "salary"
};

IQueryable<Employee> ApplySorting(IQueryable<Employee> query, string? sortBy, string? sortDirection)
{
    if (string.IsNullOrWhiteSpace(sortBy) || !AllowedSortFields.Contains(sortBy))
        return query.OrderBy(e => e.Id);

    bool descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

    return sortBy.ToLower() switch
    {
        "name" => descending
            ? query.OrderByDescending(e => e.Name)
            : query.OrderBy(e => e.Name),
        "salary" => descending
            ? query.OrderByDescending(e => e.Salary)
            : query.OrderBy(e => e.Salary),
        _ => query.OrderBy(e => e.Id)
    };
}
```

### Common Mistakes

- Accepting arbitrary property names
- Forgetting to handle the default case

---

## ًںں، Intermediate — Exercise 5: Pagination

### Problem

Add `page` and `pageSize` parameters. Return a `PagedResult<EmployeeDto>` with items and metadata.

### Starting Point

```csharp
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}
```

### Hints

- Validate page and pageSize (page >= 1, pageSize 1-100)
- Count BEFORE pagination
- Use Skip and Take

### Instructor Solution

```csharp
int page = Math.Max(parameters.Page, 1);
int pageSize = Math.Clamp(parameters.PageSize, 1, 100);

var totalCount = await query.CountAsync(cancellationToken);

var items = await query
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(e => new EmployeeDto { Id = e.Id, Name = e.Name })
    .ToListAsync(cancellationToken);

return new PagedResult<EmployeeDto>
{
    Items = items,
    Page = page,
    PageSize = pageSize,
    TotalCount = totalCount,
    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
};
```

---

## ًںں، Intermediate — Exercise 6: Paginated DTOs

### Problem

Combine filtering, searching, sorting, and pagination. Return paginated DTOs with department names.

### Instructor Solution

See the complete `EmployeeService` in Part 7. The key is to apply all operations in the correct order:

```text
Filtering â†’ Searching â†’ Sorting â†’ Count â†’ Pagination â†’ Projection â†’ Execute
```

---

## ًں"´ Advanced — Exercise 7: Full Pipeline

### Problem

Implement the complete query pipeline:

```http
GET /api/employees?search=ahmed&departmentId=2&isActive=true&sortBy=salary&sortDirection=desc&page=2&pageSize=10
```

### Requirements

1. Search by name or email
2. Filter by department, active status, salary range
3. Sort by allowed fields (whitelist)
4. Paginate with validated values
5. Project into DTOs
6. Return `PagedResult<EmployeeDto>`
7. Async with CancellationToken

### Starting Point

```csharp
public class EmployeeQueryParameters
{
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
```

### Instructor Solution

See the complete `EmployeeService.GetEmployeesAsync()` in Part 7.

---

## ًں"´ Advanced — Exercise 8: Safe Sorting with Whitelist

### Problem

Implement sorting that:

1. Only allows predefined fields
2. Falls back to default sort on invalid input
3. Supports ascending and descending
4. Does not expose internal property names

### Instructor Solution

```csharp
private static readonly Dictionary<string, Func<IQueryable<Employee>, bool, IQueryable<Employee>>> SortMap = new(StringComparer.OrdinalIgnoreCase)
{
    ["name"] = (q, desc) => desc ? q.OrderByDescending(e => e.Name) : q.OrderBy(e => e.Name),
    ["salary"] = (q, desc) => desc ? q.OrderByDescending(e => e.Salary) : q.OrderBy(e => e.Salary),
    ["email"] = (q, desc) => desc ? q.OrderByDescending(e => e.Email) : q.OrderBy(e => e.Email)
};

public IQueryable<Employee> ApplySorting(IQueryable<Employee> query, string? sortBy, string? sortDirection)
{
    bool descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

    if (sortBy is not null && SortMap.TryGetValue(sortBy, out var sortFunc))
        return sortFunc(query, descending);

    return query.OrderBy(e => e.Id);
}
```

---

## ًں"´ Advanced — Exercise 9: Consistent Error Handling

### Problem

Implement a global exception handler that:

1. Catches unhandled exceptions
2. Logs the full exception (server-side)
3. Returns a safe `ProblemDetails` response to the client
4. Never exposes exception details

### Starting Point

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    // Implement TryHandleAsync
}
```

### Instructor Solution

See the `GlobalExceptionHandler` implementation in Part 6.

### Explanation

- The handler logs the exception with full details
- The client receives a generic 500 response
- The server has all the information for debugging
- No sensitive data leaks to the client


---

# Instructor Questions

Throughout the session, ask these questions to engage trainees:

| Question | Expected Answer |
|----------|-----------------|
| "Where should this filtering happen?" | In the database, not in C# |
| "When should the database query execute?" | After all filters are applied (when ToListAsync is called) |
| "Why do we need pagination?" | Scalability — cannot return 100K rows |
| "What happens if pageSize is 1,000,000?" | Clamp it — enforce a maximum |
| "Should the API expose the EF entity?" | No — use DTOs |
| "Why not catch Exception in every controller?" | Duplicated, leaks details, wrong status codes |
| "What HTTP status should we return for not found?" | 404 |
| "What if the client sends an unsupported sort field?" | Fall back to default sort |
| "Why is returning exception.ToString() dangerous?" | Leaks internals, security risk |
| "What happens when the database contains millions of records?" | Pagination is mandatory |

---

# Senior Developer Lessons

> "An API is a contract, not just a collection of controller methods."

> "Do not make the database do unnecessary work, but do not make the application do database work either."

> "Pagination is not only a UI feature. It is a scalability feature."

> "Never trust client-provided query parameters."

> "A 200 response with an error message is still a bad API contract."

> "Exceptions are for exceptional situations, not normal control flow."

> "Your API response should contain what the client needs — not whatever EF Core happens to return."

> "A controller that knows too much is usually a design problem."

> "Filter in the database. Project only what you need. Paginate everything."

> "Status codes are not decorations. They are communication."

> "The best API is the one the frontend developer never has to ask questions about."

> "Validation is not optional. It is the first line of defense."


---

# Common Beginner Mistakes

## 1. Returning All Records

**What developers do:**
```csharp
var employees = await context.Employees.ToListAsync();
return Ok(employees);
```

**Why it is a problem:** Returns every row with every column. No filtering, no pagination. Kills performance.

**Better approach:** Use filtering, pagination, and projection.

---

## 2. Filtering in Memory

**What developers do:**
```csharp
var all = await context.Employees.ToListAsync();
var filtered = all.Where(e => e.Salary > 5000).ToList();
```

**Why it is a problem:** Loads all data from database, filters in C#. The database is much better at this.

**Better approach:**
```csharp
var filtered = await context.Employees
    .Where(e => e.Salary > 5000)
    .ToListAsync();
```

---

## 3. No Pagination

**What developers do:**
```csharp
return Ok(await context.Employees.ToListAsync());
```

**Why it is a problem:** As data grows, response time and memory usage grow linearly.

**Better approach:** Always paginate. Default page size of 20-50.

---

## 4. Huge Page Sizes

**What developers do:**
```csharp
pageSize = 1000000; // No validation
```

**Why it is a problem:** Defeats the purpose of pagination. Returns everything in one page.

**Better approach:** Clamp page size to a maximum (e.g., 100).

---

## 5. No Validation

**What developers do:**
```csharp
[HttpPost]
public async Task<IActionResult> Create(Employee employee)
{
    context.Employees.Add(employee);
    await context.SaveChangesAsync();
    return Ok(employee);
}
```

**Why it is a problem:** Accepts invalid data. Empty names, negative salaries, non-existent departments.

**Better approach:** Use Data Annotations or FluentValidation. Validate before database operations.

---

## 6. Returning EF Entities

**What developers do:**
```csharp
return Ok(await context.Employees.ToListAsync());
```

**Why it is a problem:** Exposes internal properties, navigation properties, potential circular references.

**Better approach:** Use DTOs. Return only what the client needs.

---

## 7. Incorrect Status Codes

**What developers do:**
```csharp
return Ok(new { Error = "Not found" }); // Should be 404
return BadRequest("Server error");      // Should be 500
```

**Why it is a problem:** Clients cannot distinguish between success, client error, and server error.

**Better approach:** Use the correct HTTP status codes.

---

## 8. Catching Exceptions Everywhere

**What developers do:**
```csharp
[HttpGet]
public async Task<IActionResult> Get()
{
    try { /* ... */ }
    catch (Exception ex) { return BadRequest(ex.Message); }
}
```

**Why it is a problem:** Duplicated code, leaks details, wrong status codes.

**Better approach:** Use global exception handling with `IExceptionHandler`.

---

## 9. Exposing Exception Details

**What developers do:**
```csharp
catch (Exception ex)
{
    return StatusCode(500, ex.ToString());
}
```

**Why it is a problem:** Security vulnerability. Reveals internal architecture, database structure, file paths.

**Better approach:** Log the exception server-side. Return a generic error to the client.

---

## 10. Accepting Arbitrary Sort Fields

**What developers do:**
```csharp
// Dynamically sorting by any property name from user input
var property = typeof(Employee).GetProperty(sortBy);
```

**Why it is a problem:** Security risk, information disclosure, potential exceptions.

**Better approach:** Use a whitelist of allowed sort fields.

---

## 11. Not Using Async EF Methods

**What developers do:**
```csharp
var employees = context.Employees.ToList(); // Synchronous
```

**Why it is a problem:** Blocks the thread. In ASP.NET Core, this reduces throughput.

**Better approach:** Always use `ToListAsync()`, `FirstOrDefaultAsync()`, etc.

---

## 12. Ignoring CancellationToken

**What developers do:**
```csharp
public async Task<IActionResult> Get()
{
    var employees = await context.Employees.ToListAsync(); // No token
}
```

**Why it is a problem:** If the client disconnects, the query continues executing unnecessarily.

**Better approach:** Pass `CancellationToken` through the entire call chain.

---

## 13. Mixing API and Database Models

**What developers do:**
```csharp
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<EmployeeProject> EmployeeProjects { get; set; }
    // ... everything
}

// Used as both API response AND database entity
```

**Why it is a problem:** Changes to the database model break the API contract.

**Better approach:** Separate DTOs for API from entities for database.

---

## 14. Inconsistent Response Structures

**What developers do:**
```csharp
// GET returns: { id, name, email }
// POST returns: { employeeId, employeeName, employeeEmail }
```

**Why it is a problem:** Frontend developers cannot rely on consistent property names.

**Better approach:** Use consistent DTOs across all endpoints.


---

# Comparison Tables

## Parameter Types

| Type | Example | Typical Use |
|------|---------|-------------|
| Route | `/employees/10` | Specific resource by ID |
| Query | `?page=2&search=ahmed` | Filtering, search, sort, pagination |
| Body | JSON payload | Create, update |

## HTTP Status Codes

| Status | Meaning | When to Use |
|--------|---------|-------------|
| 200 | OK | Successful GET, PUT |
| 201 | Created | Successful POST |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Validation failure, invalid input |
| 401 | Unauthorized | Missing/invalid authentication |
| 403 | Forbidden | Authenticated but not allowed |
| 404 | Not Found | Resource does not exist |
| 409 | Conflict | Duplicate, business conflict |
| 500 | Internal Server Error | Unexpected server error |

## Pagination Strategies

| Approach | Best For | Limitation |
|----------|----------|------------|
| Offset (Skip/Take) | Simple/general APIs | Slow on deep offsets |
| Keyset/Cursor | Large/high-volume datasets | Cannot jump to arbitrary pages |

## Validation Approaches

| Approach | Best For | Limitations |
|----------|----------|-------------|
| Data Annotations | Simple rules (`[Required]`, `[MaxLength]`) | Limited power |
| FluentValidation | Complex/conditional rules | More setup |
| Business Logic | Uniqueness, existence checks | Requires code |

## Error Response Formats

| Format | Standard | When to Use |
|--------|----------|-------------|
| ProblemDetails | RFC 7807 | Recommended for ASP.NET Core |
| Custom object | Team convention | Simpler APIs |

---

# Mermaid Diagrams

## 1. API Request Flow

```text
Client (Browser / Mobile / Postman)
        |
        v
HTTP Request
        |
        v
ASP.NET Core Middleware
        |
        v
Model Binding
        |
        v
Controller
        |
        v
Application / Service
        |
        v
EF Core
        |
        v
SQL Server
        |
        v
Response DTO
        |
        v
HTTP Response
```

## 2. Query Pipeline

```text
HTTP Query Parameters
        |
        v
Model Binding
        |
        v
Validation / Normalization
        |
        v
Base IQueryable
        |
        v
Filtering (Where)
        |
        v
Searching (Where)
        |
        v
Sorting (OrderBy)
        |
        v
Count (CountAsync)
        |
        v
Pagination (Skip/Take)
        |
        v
Projection (Select)
        |
        v
Execution (ToListAsync)
        |
        v
PagedResult DTO
```

## 3. Error Handling Flow

```text
Request
        |
        v
Controller
        |
        v
Exception
        |
        v
Global Exception Handler
        |
        +---> Logs full exception (server-side)
        |
        v
ProblemDetails
        |
        v
HTTP Response (safe message)
```

## 4. CRUD Status Codes

```text
GET /api/employees       --> 200 OK
GET /api/employees/99    --> 200 OK or 404 Not Found
POST /api/employees      --> 201 Created
PUT /api/employees/1     --> 200 OK or 204 No Content
DELETE /api/employees/1  --> 204 No Content or 404 Not Found
```

---

# Knowledge Check

## Questions

### Question 1 — Multiple Choice

Which parameter type is appropriate for pagination?

A. Route parameter  
B. Query parameter  
C. Request body  
D. Header  

---

### Question 2 — True/False

Calling `ToListAsync()` before `Where()` means the filtering happens in the database.

---

### Question 3 — Multiple Choice

What is the purpose of projection?

A. To validate data  
B. To select only the columns you need  
C. To sort the results  
D. To paginate the results  

---

### Question 4 — Multiple Choice

Why is returning EF Core entities directly often undesirable?

A. They are too slow  
B. They expose internal properties and may cause circular references  
C. EF Core does not support serialization  
D. Entities cannot contain navigation properties  

---

### Question 5 — Multiple Choice

What is the difference between 401 and 403?

A. 401 means unauthorized, 403 means forbidden  
B. They are the same  
C. 401 is for GET requests, 403 is for POST  
D. 401 means not found, 403 means server error  

---

### Question 6 — Multiple Choice

When should an API return 404?

A. When the database connection fails  
B. When the requested resource does not exist  
C. When validation fails  
D. When the user is not authenticated  

---

### Question 7 — True/False

Why should exception details not be returned to clients?

---

### Question 8 — Multiple Choice

Why should sort fields be restricted?

A. To improve performance  
B. To prevent information disclosure and security risks  
C. To simplify the code  
D. Sorting does not need restrictions  

---

### Question 9 — Code Analysis

What is wrong with this code?

```csharp
[HttpGet]
public async Task<IActionResult> Get()
{
    var employees = await context.Employees.ToListAsync();
    return Ok(employees);
}
```

---

### Question 10 — Multiple Choice

What does `PagedResult<T>` provide?

A. Only the items  
B. Items plus page metadata (page number, total count, etc.)  
C. A connection string  
D. An error message  

---

### Question 11 — True/False

Pagination should always happen before filtering.

---

### Question 12 — Code Analysis

What happens when the client sends `page=0&pageSize=-5`?

---

### Question 13 — Multiple Choice

What is the correct order for the query pipeline?

A. Pagination â†’ Filtering â†’ Sorting  
B. Filtering â†’ Sorting â†’ Count â†’ Pagination  
C. Sorting â†’ Pagination â†’ Count  
D. Count â†’ Filtering â†’ Sorting â†’ Pagination  

---

### Question 14 — Multiple Choice

Which HTTP status code is appropriate for a successful DELETE?

A. 200  
B. 201  
C. 204  
D. 301  

---

### Question 15 — Performance Scenario

Your API returns 50,000 employees. Response time is 15 seconds. What do you check first?

---

## Answers

### Answer 1

**B.** Query parameters are appropriate for pagination. Route parameters are for specific resources.

### Answer 2

**False.** Calling `ToListAsync()` first materializes ALL records. The subsequent `Where()` filters in memory.

### Answer 3

**B.** Projection selects only the columns you need, reducing data transfer and improving performance.

### Answer 4

**B.** Entities expose internal properties and navigation properties. This can cause circular references and over-exposure.

### Answer 5

**A.** 401 (Unauthorized) means authentication is missing or invalid. 403 (Forbidden) means authenticated but not authorized.

### Answer 6

**B.** 404 means the requested resource does not exist.

### Answer 7

Exception details can reveal internal architecture, database structure, file paths — a security vulnerability.

### Answer 8

**B.** Restricting sort fields prevents information disclosure and security risks.

### Answer 9

Three problems:
1. Returns ALL employees (no filtering, no pagination)
2. Returns EF entity directly (should use DTO)
3. No CancellationToken

### Answer 10

**B.** PagedResult provides items plus metadata: page number, page size, total count, total pages.

### Answer 11

**False.** Filtering should happen first (to get the correct count), then pagination.

### Answer 12

The API should normalize: `page` becomes 1, `pageSize` becomes 1. Never trust client input directly.

### Answer 13

**B.** Filtering â†’ Sorting â†’ Count â†’ Pagination.

### Answer 14

**C.** 204 No Content is appropriate for successful DELETE operations.

### Answer 15

1. Check if all 50K rows are loaded (no pagination)
2. Check for missing filtering
3. Check for missing projection (loading all columns)
4. Check for N+1 queries
5. Check database indexes


---

# Final Project

## Employee Management API — Production-Style Query Endpoint

### Requirements

Build a complete GET endpoint that supports:

```http
GET /api/employees?search=ahmed&departmentId=2&isActive=true&sortBy=salary&sortDirection=desc&page=2&pageSize=20
```

**Features:**
1. Search by name or email
2. Filter by department, active status, salary range
3. Sort by allowed fields (whitelist)
4. Paginate with validated values
5. Project into DTOs
6. Return `PagedResult<EmployeeDto>`
7. Async with CancellationToken
8. Proper HTTP status codes
9. Global error handling

### DTOs

```csharp
public class EmployeeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}

public class EmployeeQueryParameters
{
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
```

### Hints

1. Start with `AsQueryable()` — build the query progressively
2. Apply filters with `Where()` — each is optional
3. Apply search with `Contains()` — check Name and Email
4. Apply sorting with a whitelist switch expression
5. Count BEFORE pagination
6. Apply `Skip()` and `Take()`
7. Project with `Select()` into `EmployeeDto`
8. Return `PagedResult` with metadata

### Instructor Solution

See the complete `EmployeeService.GetEmployeesAsync()` in Part 7.

---

# Final Instructor Review

Review the project as a Senior Developer conducting a code review.

## API Design

| Check | Good | Bad |
|-------|------|-----|
| Endpoints are meaningful | `GET /api/employees?...` | `GET /api/getAllEmployees` |
| HTTP methods are correct | `POST` for create, `PUT` for update | `POST` for everything |
| Status codes are appropriate | `201` for create, `404` for not found | `200` for everything |

## Performance

| Check | Good | Bad |
|-------|------|-----|
| Filtering in SQL | `.Where()` before `ToListAsync()` | Load all, filter in C# |
| Pagination implemented | `Skip()` + `Take()` | Return everything |
| Projection used | `.Select()` into DTO | Return full entity |

## Code Quality

| Check | Good | Bad |
|-------|------|-----|
| Controllers are thin | Delegate to service | 200-line actions |
| Code is readable | Named methods, clear structure | One giant method |
| DRY | Service layer, shared logic | Duplicated in every action |

## Validation

| Check | Good | Bad |
|-------|------|-----|
| Input validated | Data Annotations or FluentValidation | No validation |
| Business rules checked | Department exists, email unique | Accept anything |

## Error Handling

| Check | Good | Bad |
|-------|------|-----|
| Global handler | `IExceptionHandler` | try/catch in every action |
| Consistent format | `ProblemDetails` | Random object shapes |
| Safe messages | Generic error to client | Exception details exposed |

## Maintainability

| Check | Good | Bad |
|-------|------|-----|
| DTOs separate from entities | `EmployeeDto` vs `Employee` | Same class for both |
| Query parameters in a class | `EmployeeQueryParameters` | 10 parameters in action signature |
| Sort fields whitelisted | `HashSet` or `Dictionary` | Arbitrary input accepted |

> ًں’، **Senior Developer Lesson:** "A good code review checks not just what the code does, but how it will behave when data grows, when requirements change, and when new developers join the team."


---

# Final Summary

## What We Covered Today

```text
API Request
    |
    v
Model Binding (query parameters)
    |
    v
Validation (Data Annotations / FluentValidation)
    |
    v
Filtering (Where in the database)
    |
    v
Searching (Contains in the database)
    |
    v
Sorting (OrderBy with whitelist)
    |
    v
Pagination (Skip/Take with validated values)
    |
    v
Projection (Select into DTOs)
    |
    v
EF Core (translated to SQL)
    |
    v
SQL Server
    |
    v
PagedResult DTO
    |
    v
HTTP Response (correct status code)
```

## What You Should Be Able To Do Now

- [ ] Build a professional CRUD API
- [ ] Use query parameters for filtering and searching
- [ ] Implement database-side filtering with `Where()`
- [ ] Implement text search with `Contains()`
- [ ] Implement sorting with a whitelist
- [ ] Implement pagination with `Skip()` and `Take()`
- [ ] Validate requests with Data Annotations or FluentValidation
- [ ] Use correct HTTP status codes (200, 201, 204, 400, 404, 409, 500)
- [ ] Handle exceptions globally with `IExceptionHandler`
- [ ] Return `ProblemDetails` for errors
- [ ] Return DTOs, not EF Core entities
- [ ] Use async methods with `CancellationToken`
- [ ] Think about API performance and scalability
- [ ] Review API code like a Senior Developer

---

# Next Module — Week 3: Clean Architecture & Security

Now that you have learned the technical building blocks (C#, ASP.NET Core, EF Core, Web API), you are ready to learn how to **organize** a professional application.

## Day 5 — Clean Architecture

- Architecture principles
- Separation of Concerns
- Domain Layer
- Application Layer
- Infrastructure Layer
- API Layer
- Dependency flow

## Day 6 — Application Architecture

- Dependency Inversion Principle
- Repository Pattern
- Unit of Work
- Services
- Use Cases
- DTOs and Mapping
- CQRS concepts
- MediatR preview

> "You have learned how to build the parts. Now you will learn how to organize them into a professional application that can grow, scale, and be maintained by a team."

---

*End of Day 4 — Advanced Web API*

