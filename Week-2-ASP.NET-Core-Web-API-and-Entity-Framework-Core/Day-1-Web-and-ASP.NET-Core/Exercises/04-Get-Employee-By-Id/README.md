# Exercise 04 — `GET /api/employees/{id}`

## 🎯 Objective

Add an endpoint to get a single employee by ID.

## 📋 Requirements

1. Add `[HttpGet("{id:int}")]` attribute
2. Add `GetById(int id)` method
3. Return `Ok(employee)` if found, `NotFound()` if not
4. Use the `{id:int}` route constraint

## 🚀 Starting Point

The `Starter/` folder contains the completed Exercise 03.

## 📝 Your Task

Open `Starter/Controllers/EmployeesController.cs` and complete the TODOs.

## ✅ Expected Behavior

```bash
curl http://localhost:5189/api/employees/2
```

Response:

```json
{ "id": 2, "name": "Karim", "department": "HR" }
```

```bash
curl http://localhost:5189/api/employees/99
```

Response: `404 Not Found`

## 🔍 How to Test

```bash
cd Starter
dotnet run
# In another terminal:
curl http://localhost:5189/api/employees/1
curl http://localhost:5189/api/employees/99
```

## 💡 Hints

- Use `FirstOrDefault(e => e.Id == id)` to find the employee
- Return `NotFound()` when the employee is `null`
- The `{id:int}` constraint rejects non-numeric values with `404`

## 🔜 Next

**Exercise 05** — Debug a broken controller.

## 📚 Related Lesson

[04 — Controllers and Routing](../../04-Controllers-and-Routing.md)
