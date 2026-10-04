# 04 — HTTP Status Codes

**Duration:** ~20 min

---

## 🎯 What You'll Learn

By the end of this topic, you'll be able to:

- Choose the right status code for each API scenario
- Explain the difference between 2xx, 4xx, and 5xx codes
- Return status codes explicitly from your endpoints

---

## 📖 Status Code Categories

| Category | Range | Meaning |
|----------|-------|---------|
| **2xx Success** | 200-299 | The request succeeded |
| **4xx Client Error** | 400-499 | The client made a mistake |
| **5xx Server Error** | 500-599 | The server has a problem |

---

## 🔍 Status Codes You'll Use Today

### 2xx — Success

| Code | Name | When to Use | Example |
|------|------|-------------|---------|
| **200** | OK | Successful GET or PUT | `GET /api/employees` |
| **201** | Created | Resource created successfully | `POST /api/employees` |
| **204** | No Content | Success with no response body | `DELETE /api/employees/5` |

### 4xx — Client Error

| Code | Name | When to Use | Example |
|------|------|-------------|---------|
| **400** | Bad Request | Invalid request data | Missing required fields |
| **404** | Not Found | Resource doesn't exist | `GET /api/employees/999` |
| **409** | Conflict | Resource already exists | Duplicate email |
| **422** | Unprocessable Entity | Validation failed | Invalid data format |

### 5xx — Server Error

| Code | Name | When to Use | Example |
|------|------|-------------|---------|
| **500** | Internal Server Error | Unexpected server error | Null reference exception |

---

## 🔧 Returning Status Codes Explicitly

### 200 OK

```csharp
[HttpGet]
public IActionResult GetAll()
{
    var employees = _employeeService.GetAll();
    return Ok(employees);  // 200 OK
}
```

### 201 Created

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateEmployeeDto dto)
{
    var employee = new Employee { Name = dto.Name, Department = dto.Department };
    var created = _employeeService.Create(employee);
    
    return CreatedAtAction(
        actionName: nameof(GetById),
        routeValues: new { id = created.Id },
        value: created);  // 201 Created with Location header
}
```

### 204 No Content

```csharp
[HttpDelete("{id:int}")]
public IActionResult Delete(int id)
{
    var success = _employeeService.Delete(id);
    if (!success)
        return NotFound();  // 404
    
    return NoContent();  // 204
}
```

### 404 Not Found

```csharp
[HttpGet("{id:int}")]
public IActionResult GetById(int id)
{
    var employee = _employeeService.GetById(id);
    if (employee == null)
        return NotFound();  // 404
    
    return Ok(employee);  // 200
}
```

### 400 Bad Request

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateEmployeeDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Name))
        return BadRequest("Name is required");  // 400
    
    var employee = new Employee { Name = dto.Name, Department = dto.Department };
    var created = _employeeService.Create(employee);
    
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
}
```

---

## 🧪 Practice Exercise

**Task:** Add validation to the Create endpoint.

**Requirements:**
- Name must not be empty
- Department must not be empty
- Return `400 Bad Request` with an error message if validation fails

**Implementation:**

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateEmployeeDto dto)
{
    // Validation
    if (string.IsNullOrWhiteSpace(dto.Name))
        return BadRequest("Name is required");
    
    if (string.IsNullOrWhiteSpace(dto.Department))
        return BadRequest("Department is required");
    
    // Create employee
    var employee = new Employee
    {
        Name = dto.Name,
        Department = dto.Department
    };
    
    var created = _employeeService.Create(employee);
    
    return CreatedAtAction(
        nameof(GetById),
        new { id = created.Id },
        created);
}
```

**Test it:**

```bash
# Valid request
curl -X POST http://localhost:5000/api/employees \
  -H "Content-Type: application/json" \
  -d '{"name":"Omar","department":"Finance"}' \
  -i
# Expected: 201 Created

# Invalid request (missing name)
curl -X POST http://localhost:5000/api/employees \
  -H "Content-Type: application/json" \
  -d '{"department":"Finance"}' \
  -i
# Expected: 400 Bad Request with error message
```

---

## 💡 Key Takeaways

| Scenario | Status Code | Helper Method |
|----------|-------------|---------------|
| Successful GET | `200` | `Ok(value)` |
| Successful POST | `201` | `CreatedAtAction(...)` |
| Successful DELETE | `204` | `NoContent()` |
| Resource not found | `404` | `NotFound()` |
| Invalid request | `400` | `BadRequest(message)` |
| Server error | `500` | `StatusCode(500)` or let exception propagate |

**Best practices:**
- Use the most specific status code
- Include error details in 4xx responses
- Don't expose internal errors in 5xx responses (use problem details)
- Be consistent across all endpoints

---

## 🔜 Next

We've covered the status codes we need. Next, we'll explore Swagger UI to test and document our API.
