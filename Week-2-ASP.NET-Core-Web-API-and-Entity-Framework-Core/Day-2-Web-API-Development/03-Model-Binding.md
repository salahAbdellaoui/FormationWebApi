# 03 — Model Binding

**Duration:** ~30 min

---

## 🎯 What You'll Learn

By the end of this topic, you'll be able to:

- Explain where action parameters come from (route, query, body)
- Use `[FromRoute]`, `[FromQuery]`, and `[FromBody]` attributes
- Understand how complex types are bound
- Debug binding problems

---

## 📖 How Model Binding Works

When a request arrives, ASP.NET Core needs to populate your action parameters:

```text
HTTP Request
    ↓
Model Binding
    ↓
Action Parameters
```

**Example request:**

```http
GET /api/employees/5
```

**Example action:**

```csharp
[HttpGet("{id:int}")]
public IActionResult GetById(int id)
```

**What happens?**
1. Routing extracts `id = 5` from the URL
2. Model binding converts `"5"` to `int`
3. The action receives `id = 5`

---

## 📍 Binding Sources

ASP.NET Core can bind parameters from different parts of the request:

| Source | Attribute | Example |
|--------|-----------|---------|
| Route | `[FromRoute]` | `/api/employees/5` → `id = 5` |
| Query string | `[FromQuery]` | `?department=Engineering` → `department = "Engineering"` |
| Request body | `[FromBody]` | `{"name":"Omar"}` → `Name = "Omar"` |

**Default behavior:**
- Simple types (int, string, bool) → try route, then query
- Complex types (classes) → body (for POST/PUT/PATCH)

**Explicit is better:** Use attributes to be clear.

---

## 🔧 Example 1: Route Binding

```csharp
[HttpGet("{id:int}")]
public IActionResult GetById([FromRoute] int id)
{
    var employee = _employeeService.GetById(id);
    return employee == null ? NotFound() : Ok(employee);
}
```

**Request:** `GET /api/employees/5`

**What happens:**
1. Route matches `/api/employees/{id:int}`
2. `id = 5` is extracted from the route
3. Model binding converts `"5"` to `int`
4. Action receives `id = 5`

---

## 🔧 Example 2: Query String Binding

Add a filter endpoint:

```csharp
[HttpGet("by-department")]
public IActionResult GetByDepartment([FromQuery] string department)
{
    var employees = _employeeService.GetAll()
        .Where(e => e.Department.Equals(department, StringComparison.OrdinalIgnoreCase));
    
    return Ok(employees);
}
```

**Request:** `GET /api/employees/by-department?department=Engineering`

**What happens:**
1. Route matches `/api/employees/by-department`
2. Query string `department=Engineering` is extracted
3. Action receives `department = "Engineering"`

**Test it:**

```bash
curl "http://localhost:5000/api/employees/by-department?department=Engineering"
```

**Multiple query parameters:**

```csharp
[HttpGet("search")]
public IActionResult Search([FromQuery] string? name, [FromQuery] string? department)
{
    var employees = _employeeService.GetAll();
    
    if (!string.IsNullOrEmpty(name))
        employees = employees.Where(e => e.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
    
    if (!string.IsNullOrEmpty(department))
        employees = employees.Where(e => e.Department.Equals(department, StringComparison.OrdinalIgnoreCase));
    
    return Ok(employees);
}
```

**Request:** `GET /api/employees/search?name=omar&department=finance`

---

## 🔧 Example 3: Body Binding

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateEmployeeDto dto)
{
    // dto is automatically populated from JSON body
    var employee = new Employee
    {
        Name = dto.Name,
        Department = dto.Department
    };
    
    var created = _employeeService.Create(employee);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
}
```

**Request:**

```http
POST /api/employees
Content-Type: application/json

{
  "name": "Omar",
  "department": "Finance"
}
```

**What happens:**
1. ASP.NET Core sees `[FromBody]`
2. Reads the request body as JSON
3. Deserializes it into `CreateEmployeeDto`
4. Action receives the populated DTO

---

## 🧪 Practice Exercise

**Task:** Add a new endpoint that uses multiple binding sources.

**Endpoint:** `GET /api/employees/filter`

**Parameters:**
- `department` from query string
- `minId` from query string (optional)

**Example:** `GET /api/employees/filter?department=Engineering&minId=2`

**Implementation:**

```csharp
[HttpGet("filter")]
public IActionResult Filter(
    [FromQuery] string department,
    [FromQuery] int? minId)
{
    var employees = _employeeService.GetAll();
    
    // Filter by department
    employees = employees.Where(e => 
        e.Department.Equals(department, StringComparison.OrdinalIgnoreCase));
    
    // Filter by minimum ID if provided
    if (minId.HasValue)
        employees = employees.Where(e => e.Id >= minId.Value);
    
    return Ok(employees);
}
```

**Test it:**

```bash
# Filter by department only
curl "http://localhost:5000/api/employees/filter?department=Engineering"

# Filter by department and minimum ID
curl "http://localhost:5000/api/employees/filter?department=Engineering&minId=2"
```

---

## 🐛 Debugging Binding Problems

**Problem:** Parameter is always null or default value.

**Check:**
1. **Is the attribute correct?**
   - Route parameter? → `[FromRoute]`
   - Query parameter? → `[FromQuery]`
   - JSON body? → `[FromBody]`

2. **Does the parameter name match?**
   - Query: `?department=Engineering` → parameter should be `department`
   - Route: `{id}` → parameter should be `id`
   - Body: JSON property names should match DTO property names

3. **Is the Content-Type correct?**
   - For `[FromBody]`, must be `Content-Type: application/json`

4. **Is the type compatible?**
   - Route: `{id:int}` → parameter must be `int`
   - Query: `?minId=5` → parameter must be `int` or `int?`

---

## 💡 Key Takeaways

| Scenario | Binding Source | Attribute |
|----------|----------------|-----------|
| `/api/employees/5` | Route | `[FromRoute]` |
| `?department=Engineering` | Query string | `[FromQuery]` |
| `{"name":"Omar"}` | Request body | `[FromBody]` |

**Best practices:**
- Use explicit attributes (`[FromRoute]`, `[FromQuery]`, `[FromBody]`)
- Use nullable types for optional parameters (`int?`, `string?`)
- Match parameter names to request data names
- Validate required parameters

---

## 🔜 Next

We've covered CRUD, DTOs, and model binding. Next, we'll review the HTTP status codes we've been using and add a few more.
