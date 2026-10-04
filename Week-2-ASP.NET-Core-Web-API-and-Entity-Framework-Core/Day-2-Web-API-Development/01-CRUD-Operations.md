# 01 — CRUD Operations

**Duration:** ~40 min

---

## 🎯 What You'll Learn

By the end of this topic, you'll be able to:

- Implement POST, PUT, and DELETE endpoints
- Understand the HTTP method for each CRUD operation
- Return appropriate status codes for each operation
- Test CRUD endpoints with curl

---

## 📖 Context

Day 1 gave us two GET endpoints:

```csharp
GET /api/employees          → get all employees
GET /api/employees/{id}     → get one employee
```

These are "Read" operations. Today we add:

```csharp
POST   /api/employees          → Create a new employee
PUT    /api/employees/{id}     → Update (replace) an employee
DELETE /api/employees/{id}     → Delete an employee
```

---

## 🔧 Step 1: Extend the Service

The service needs new methods. Open `IEmployeeService.cs` and add:

```csharp
using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Services;

public interface IEmployeeService
{
    IEnumerable<Employee> GetAll();
    Employee? GetById(int id);
    
    // New CRUD methods
    Employee Create(Employee employee);
    bool Update(int id, Employee employee);
    bool Delete(int id);
}
```

Now implement them in `EmployeeService.cs`:

```csharp
public Employee Create(Employee employee)
{
    // Simple ID generation (in real apps, the database does this)
    employee.Id = Employees.Count > 0 ? Employees.Max(e => e.Id) + 1 : 1;
    Employees.Add(employee);
    return employee;
}

public bool Update(int id, Employee employee)
{
    var existing = Employees.FirstOrDefault(e => e.Id == id);
    if (existing == null)
        return false;
    
    existing.Name = employee.Name;
    existing.Department = employee.Department;
    return true;
}

public bool Delete(int id)
{
    var employee = Employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
        return false;
    
    Employees.Remove(employee);
    return true;
}
```

**Run:** `dotnet build` — should compile successfully.

---

## 🔧 Step 2: Add the POST Endpoint

Create a new employee:

```http
POST /api/employees
Content-Type: application/json

{
  "name": "Omar",
  "department": "Finance"
}
```

**Expected:** Returns `201 Created` with the new employee in the body.

Add the endpoint to `EmployeesController.cs`:

```csharp
[HttpPost]
public IActionResult Create([FromBody] Employee employee)
{
    var created = _employeeService.Create(employee);
    return CreatedAtAction(
        actionName: nameof(GetById),
        routeValues: new { id = created.Id },
        value: created);
}
```

**What does `CreatedAtAction` do?**
- Returns status code `201 Created`
- Adds a `Location` header pointing to the new resource
- Includes the created employee in the response body

**Test it:**

```bash
curl -X POST http://localhost:5000/api/employees \
  -H "Content-Type: application/json" \
  -d '{"name":"Omar","department":"Finance"}' \
  -i
```

You should see:
```http
HTTP/1.1 201 Created
Location: http://localhost:5000/api/employees/4
Content-Type: application/json

{"id":4,"name":"Omar","department":"Finance"}
```

---

## 🔧 Step 3: Add the PUT Endpoint

Replace an existing employee:

```http
PUT /api/employees/4
Content-Type: application/json

{
  "name": "Omar Ben Ali",
  "department": "Marketing"
}
```

**Expected:** Returns `204 No Content` (no body, just success).

Add the endpoint:

```csharp
[HttpPut("{id:int}")]
public IActionResult Update(int id, [FromBody] Employee employee)
{
    var success = _employeeService.Update(id, employee);
    if (!success)
        return NotFound();
    
    return NoContent();
}
```

**What does `NoContent` do?**
- Returns status code `204 No Content`
- No response body (the update succeeded, nothing more to say)

**Test it:**

```bash
curl -X PUT http://localhost:5000/api/employees/4 \
  -H "Content-Type: application/json" \
  -d '{"name":"Omar Ben Ali","department":"Marketing"}' \
  -i
```

You should see:
```http
HTTP/1.1 204 No Content
```

Try updating a non-existent employee (e.g., ID 999):

```bash
curl -X PUT http://localhost:5000/api/employees/999 \
  -H "Content-Type: application/json" \
  -d '{"name":"Nobody","department":"Nowhere"}' \
  -i
```

Expected: `404 Not Found`.

---

## 🔧 Step 4: Add the DELETE Endpoint

Remove an employee:

```http
DELETE /api/employees/4
```

**Expected:** Returns `204 No Content`.

Add the endpoint:

```csharp
[HttpDelete("{id:int}")]
public IActionResult Delete(int id)
{
    var success = _employeeService.Delete(id);
    if (!success)
        return NotFound();
    
    return NoContent();
}
```

**Test it:**

```bash
curl -X DELETE http://localhost:5000/api/employees/4 -i
```

Expected: `204 No Content`.

Verify it's gone:

```bash
curl http://localhost:5000/api/employees
```

Employee 4 should no longer appear.

---

## 🧪 Practice Exercise

**Task:** Create, update, and delete employees using curl. Verify each operation works correctly.

**Steps:**
1. Start the API: `dotnet run`
2. Create an employee with POST
3. Verify it exists with GET
4. Update it with PUT
5. Verify the update with GET
6. Delete it with DELETE
7. Verify it's gone with GET

**Questions:**
- What status code does POST return? → `201 Created`
- What status code does PUT return? → `204 No Content`
- What status code does DELETE return? → `204 No Content`
- What happens if you PUT or DELETE a non-existent ID? → `404 Not Found`

---

## 💡 Key Takeaways

| Operation | HTTP Method | Endpoint | Success Status | Error Status |
|-----------|-------------|----------|----------------|--------------|
| Read all | GET | `/api/employees` | `200 OK` | — |
| Read one | GET | `/api/employees/{id}` | `200 OK` | `404 Not Found` |
| Create | POST | `/api/employees` | `201 Created` | `400 Bad Request` |
| Update | PUT | `/api/employees/{id}` | `204 No Content` | `404 Not Found` |
| Delete | DELETE | `/api/employees/{id}` | `204 No Content` | `404 Not Found` |

---

## 🔜 Next

We have working CRUD endpoints, but we're exposing the internal `Employee` model directly. Next, we'll introduce DTOs to control the API contract.
