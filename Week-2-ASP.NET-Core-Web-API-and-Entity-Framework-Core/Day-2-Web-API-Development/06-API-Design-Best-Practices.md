# 06 — API Design Best Practices

**Duration:** ~20 min

---

## 🎯 What You'll Learn

By the end of this topic, you'll be able to:

- Design resource-oriented URLs
- Use HTTP methods correctly
- Write consistent and predictable APIs
- Avoid common API design mistakes

---

## 📖 What Makes a Good API?

A good API is:
- **Intuitive** — Easy to understand without reading documentation
- **Consistent** — Same patterns everywhere
- **Predictable** — Behaves as expected
- **Self-documenting** — Clear names and status codes

---

## ✅ Best Practice 1: Resource-Oriented URLs

**Bad (action-oriented):**

```http
POST /api/getEmployees
POST /api/getEmployeeById
POST /api/createEmployee
POST /api/updateEmployee
POST /api/deleteEmployee
```

**Good (resource-oriented):**

```http
GET    /api/employees
GET    /api/employees/{id}
POST   /api/employees
PUT    /api/employees/{id}
DELETE /api/employees/{id}
```

**Why?**
- The URL names the **resource** (employees)
- The HTTP method names the **action** (GET, POST, PUT, DELETE)
- Easier to understand and cache

---

## ✅ Best Practice 2: Use HTTP Methods Correctly

| Method | Purpose | Idempotent* | Example |
|--------|---------|-------------|---------|
| **GET** | Read a resource | Yes | `GET /api/employees` |
| **POST** | Create a resource | No | `POST /api/employees` |
| **PUT** | Replace a resource | Yes | `PUT /api/employees/5` |
| **PATCH** | Partially update | No | `PATCH /api/employees/5` |
| **DELETE** | Remove a resource | Yes | `DELETE /api/employees/5` |

*Idempotent = calling it multiple times has the same effect as calling it once.

**Rules:**
- GET should not change data (read-only)
- POST creates new resources
- PUT replaces the entire resource
- PATCH updates part of a resource
- DELETE removes a resource

---

## ✅ Best Practice 3: Use Plural Nouns

**Bad:**

```http
GET /api/employee
GET /api/employee/5
```

**Good:**

```http
GET /api/employees
GET /api/employees/5
```

**Why?**
- A collection contains multiple items
- `/api/employees` = the collection
- `/api/employees/5` = one item in the collection

---

## ✅ Best Practice 4: Use Lowercase and Hyphens

**Bad:**

```http
GET /api/GetEmployees
GET /api/employeeDetails
```

**Good:**

```http
GET /api/employees
GET /api/employee-details
```

**Why?**
- URLs are case-sensitive (sometimes)
- Lowercase is standard
- Hyphens are more readable than camelCase

---

## ✅ Best Practice 5: Return Appropriate Status Codes

**Bad:**

```http
POST /api/employees
→ 200 OK (even though it was created)

DELETE /api/employees/5
→ 200 OK (even though there's no body)
```

**Good:**

```http
POST /api/employees
→ 201 Created

DELETE /api/employees/5
→ 204 No Content
```

**Why?**
- Status codes communicate meaning
- Clients can rely on them
- Standard behavior across APIs

---

## ✅ Best Practice 6: Use DTOs for API Contracts

**Bad (exposing entity directly):**

```csharp
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Department { get; set; }
    public string InternalNotes { get; set; }  // internal field
}

[HttpPost]
public IActionResult Create([FromBody] Employee employee)
```

**Good (using DTOs):**

```csharp
public class CreateEmployeeDto
{
    public string Name { get; set; }
    public string Department { get; set; }
    // No Id (server-generated)
    // No InternalNotes (not exposed)
}

[HttpPost]
public IActionResult Create([FromBody] CreateEmployeeDto dto)
```

**Why?**
- Control what clients can send/receive
- Hide internal implementation
- Validate at the boundary

---

## ✅ Best Practice 7: Validate Input

**Bad (no validation):**

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateEmployeeDto dto)
{
    var employee = new Employee { Name = dto.Name, Department = dto.Department };
    _employeeService.Create(employee);
    return CreatedAtAction(...);
}
```

**Good (with validation):**

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateEmployeeDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Name))
        return BadRequest("Name is required");
    
    if (string.IsNullOrWhiteSpace(dto.Department))
        return BadRequest("Department is required");
    
    var employee = new Employee { Name = dto.Name, Department = dto.Department };
    var created = _employeeService.Create(employee);
    return CreatedAtAction(...);
}
```

**Why?**
- Prevent invalid data
- Give clear error messages
- Protect the API

---

## 🧪 Practice Exercise

**Task:** Review the API design below and identify the problems.

**Bad API:**

```http
POST /api/getEmployees
POST /api/getEmployeeById
POST /api/createEmployee
POST /api/updateEmployee
POST /api/deleteEmployee
```

**Problems:**
1. All endpoints use POST (wrong HTTP methods)
2. URLs are action-oriented, not resource-oriented
3. Inconsistent naming
4. Not using HTTP methods correctly

**Fixed API:**

```http
GET    /api/employees
GET    /api/employees/{id}
POST   /api/employees
PUT    /api/employees/{id}
DELETE /api/employees/{id}
```

---

## 💡 Key Takeaways

| Principle | Do This | Not This |
|-----------|---------|----------|
| URLs | `/api/employees` | `/api/getEmployees` |
| Methods | Use HTTP methods | All POST |
| Nouns | Plural: `/employees` | Singular: `/employee` |
| Case | Lowercase: `/employee-details` | Mixed: `/EmployeeDetails` |
| Status | Use specific codes | Always 200 |
| DTOs | Use DTOs at boundary | Expose entities |
| Validation | Validate input | Trust all input |

**Remember:**
> A good API is not just an API that works. It is an API whose behavior is predictable for its consumers.

---

## 🎉 Day 2 Complete

You've learned:
- ✅ CRUD operations (POST, PUT, DELETE)
- ✅ DTOs for API contracts
- ✅ Model binding from route, query, body
- ✅ HTTP status codes
- ✅ Swagger UI for testing
- ✅ API design best practices

**Next:** Day 3 replaces in-memory data with a real database using Entity Framework Core.
