# Exercise 03 — `GET /api/employees`

## 🎯 Objective

Implement the endpoint that returns the full list of employees.

## 📋 Requirements

1. Add a `private static readonly List<Employee>` with at least 3 employees
2. Change `GetAll()` to return `Ok(Employees)`
3. Add the `using EmployeeManagement.Api.Models;` directive

## 🚀 Starting Point

The `Starter/` folder contains the completed Exercise 02.

## 📝 Your Task

Open `Starter/Controllers/EmployeesController.cs` and complete the TODO.

## ✅ Expected Behavior

```bash
curl http://localhost:5189/api/employees
```

Response:

```json
[
  { "id": 1, "name": "Amira", "department": "Engineering" },
  { "id": 2, "name": "Karim", "department": "HR" },
  { "id": 3, "name": "Sara", "department": "Engineering" }
]
```

## 🔍 How to Test

```bash
cd Starter
dotnet run
# In another terminal:
curl http://localhost:5189/api/employees
```

## 💡 Hints

- Use `static readonly` so the list is shared across requests
- `Ok(value)` serializes the value to JSON with status 200
- The model is in `EmployeeManagement.Api.Models`

## 🔜 Next

**Exercise 04** — `GET /api/employees/{id}` returns one employee.

## 📚 Related Lesson

[04 — Controllers and Routing](../../04-Controllers-and-Routing.md)
