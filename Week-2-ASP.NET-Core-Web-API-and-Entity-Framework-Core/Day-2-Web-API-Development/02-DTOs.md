# 02 — DTOs (Data Transfer Objects)

**Duration:** ~30 min

---

## 🎯 What You'll Learn

By the end of this topic, you'll be able to:

- Explain why we use DTOs instead of exposing entities directly
- Create DTOs for create, update, and response operations
- Map between entities and DTOs manually
- Control the API contract explicitly

---

## 📖 The Problem

In Topic 01, we exposed the `Employee` entity directly:

```csharp
[HttpPost]
public IActionResult Create([FromBody] Employee employee)
{
    var created = _employeeService.Create(employee);
    return CreatedAtAction(...);
}
```

**What's wrong with this?**

1. **The client can set the ID** — they could send `{"id": 999, "name": "Hacker", ...}`
2. **The API contract is implicit** — it's tied to the internal model
3. **No validation** — any JSON that matches the shape is accepted
4. **Future changes break clients** — if we add an internal field, it's now part of the API

---

## 💡 The Solution: DTOs

A **DTO (Data Transfer Object)** is a class that defines the shape of data that crosses the API boundary.

```text
Internal Model (Entity)     →    Used inside the application
      ↕
DTO                        →    Used at the API boundary
```

**Benefits:**
- Explicit API contract
- Control what clients can send/receive
- Validation happens at the boundary
- Internal changes don't break the API

---

## 🔧 Step 1: Create the DTOs

Create a new folder `Dtos/` and add three DTOs:

### CreateEmployeeDto.cs

```csharp
namespace EmployeeManagement.Api.Dtos;

public class CreateEmployeeDto
{
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}
```

**Note:** No `Id` property — the server generates it.

### UpdateEmployeeDto.cs

```csharp
namespace EmployeeManagement.Api.Dtos;

public class UpdateEmployeeDto
{
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}
```

**Note:** Same shape as Create, but semantically different (we're updating, not creating).

### EmployeeDto.cs

```csharp
namespace EmployeeManagement.Api.Dtos;

public class EmployeeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}
```

**Note:** This is what the API returns. It has all fields, but they're read-only from the client's perspective.

---

## 🔧 Step 2: Update the Service

The service still works with the internal `Employee` model. We'll map DTOs to entities in the controller.

No changes needed to `IEmployeeService` or `EmployeeService`.

---

## 🔧 Step 3: Update the Controller

### Update POST

```csharp
using EmployeeManagement.Api.Dtos;

[HttpPost]
public IActionResult Create([FromBody] CreateEmployeeDto dto)
{
    // Map DTO to entity
    var employee = new Employee
    {
        Name = dto.Name,
        Department = dto.Department
    };
    
    var created = _employeeService.Create(employee);
    
    // Map entity to response DTO
    var response = new EmployeeDto
    {
        Id = created.Id,
        Name = created.Name,
        Department = created.Department
    };
    
    return CreatedAtAction(
        actionName: nameof(GetById),
        routeValues: new { id = response.Id },
        value: response);
}
```

**What changed?**
- Parameter is now `CreateEmployeeDto` (no Id field)
- We map DTO → Entity before calling the service
- We map Entity → `EmployeeDto` for the response

### Update PUT

```csharp
[HttpPut("{id:int}")]
public IActionResult Update(int id, [FromBody] UpdateEmployeeDto dto)
{
    // Map DTO to entity
    var employee = new Employee
    {
        Name = dto.Name,
        Department = dto.Department
    };
    
    var success = _employeeService.Update(id, employee);
    if (!success)
        return NotFound();
    
    return NoContent();
}
```

### Update GET

```csharp
[HttpGet]
public IActionResult GetAll()
{
    var employees = _employeeService.GetAll();
    
    // Map entities to DTOs
    var dtos = employees.Select(e => new EmployeeDto
    {
        Id = e.Id,
        Name = e.Name,
        Department = e.Department
    });
    
    return Ok(dtos);
}

[HttpGet("{id:int}")]
public IActionResult GetById(int id)
{
    var employee = _employeeService.GetById(id);
    if (employee == null)
        return NotFound();
    
    // Map entity to DTO
    var dto = new EmployeeDto
    {
        Id = employee.Id,
        Name = employee.Name,
        Department = employee.Department
    };
    
    return Ok(dto);
}
```

---

## 🧪 Practice Exercise

**Task:** Test the updated API with DTOs.

**Steps:**
1. Start the API: `dotnet run`
2. Try to create an employee with an ID in the body:

```bash
curl -X POST http://localhost:5000/api/employees \
  -H "Content-Type: application/json" \
  -d '{"id":999,"name":"Omar","department":"Finance"}' \
  -i
```

3. Check the response — the ID should be server-generated (e.g., 4), not 999
4. Verify all GET endpoints return `EmployeeDto` objects

**Questions:**
- Can the client set the ID now? → **No** (CreateEmployeeDto has no Id property)
- What happens to extra fields in the JSON? → They're ignored by the model binder
- Why use separate DTOs for Create and Update? → Semantic clarity, future flexibility

---

## 💡 Key Takeaways

| Before (exposing entity) | After (using DTOs) |
|--------------------------|---------------------|
| Client can set Id | Id is server-generated |
| Implicit API contract | Explicit API contract |
| Internal changes break API | Internal changes are hidden |
| No validation boundary | Validation at the boundary |

**When to use DTOs:**
- Always at the API boundary
- When you need to control the API contract
- When you need to validate input
- When internal models differ from API needs

**When you might skip DTOs:**
- Very simple prototypes (but you'll pay for it later)
- Internal services (not exposed to clients)

---

## 🔜 Next

We've added DTOs, but how does ASP.NET Core know to bind JSON to our DTO properties? Next, we'll explore model binding in detail.
