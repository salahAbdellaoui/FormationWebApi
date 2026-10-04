# Exercise 07 — Register & Inject

## 🎯 Objective

Register the service and inject it into the controller.

## 📋 Requirements

1. Add `using EmployeeManagement.Api.Services;` to `Program.cs`
2. Register the service: `builder.Services.AddScoped<IEmployeeService, EmployeeService>();`
3. Refactor `EmployeesController` to use `IEmployeeService` via constructor injection
4. Remove all data and logic from the controller

## 🚀 Starting Point

The `Starter/` folder contains the completed Exercise 06 (service files exist, controller still has data).

## 📝 Your Task

1. Open `Starter/Program.cs` and add the registration line
2. Open `Starter/Controllers/EmployeesController.cs` and refactor it:
   - Add `private readonly IEmployeeService _employeeService;`
   - Add constructor: `public EmployeesController(IEmployeeService employeeService)`
   - Replace `Employees` with `_employeeService.GetAll()` and `_employeeService.GetById(id)`
   - Remove the `private static readonly List<Employee>` field

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

```bash
curl http://localhost:5189/api/employees/2
```

Response:

```json
{ "id": 2, "name": "Karim", "department": "HR" }
```

## 🔍 How to Test

```bash
cd Starter
dotnet run
# In another terminal:
curl http://localhost:5189/api/employees
curl http://localhost:5189/api/employees/1
```

## 💡 Hints

- The controller should have NO data, only HTTP logic
- Constructor injection: the container provides the service instance
- If you get `500` with "Unable to resolve service", check `Program.cs` registration

## 🔜 Next

**Exercise 08** — Final Challenge: build a Departments API from scratch.

## 📚 Related Lesson

[05 — Dependency Injection](../../05-Dependency-Injection.md)
