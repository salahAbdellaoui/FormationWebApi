# Exercise 08 — Final Challenge: Departments API

## 🎯 Objective

Build a complete Departments API from scratch, applying everything you learned today.

## 📋 Requirements

Without looking at the lesson, build:

1. **Model**: `Department` with `Id` and `Name`
2. **Service**: `IDepartmentService` interface + `DepartmentService` implementation
   - `IEnumerable<Department> GetAll()`
   - `Department? GetById(int id)`
   - Use in-memory data (at least 3 departments)
3. **Controller**: `DepartmentsController` with:
   - `[ApiController]` and `[Route("api/[controller]")]`
   - Constructor injection of `IDepartmentService`
   - `GET /api/departments` → 200 + list
   - `GET /api/departments/{id}` → 200 + one department, or 404
4. **Registration**: Register `IDepartmentService` as Scoped in `Program.cs`

## 🚀 Starting Point

The `Starter/` folder contains:

- A minimal Web API project (from `dotnet new webapi --use-controllers`)
- `Models/Department.cs` (already created)
- `Controllers/DepartmentsController.cs` (empty skeleton with TODOs)

You need to create everything else.

## 📝 Your Task

1. Create `Services/IDepartmentService.cs`
2. Create `Services/DepartmentService.cs` with in-memory data
3. Complete `Controllers/DepartmentsController.cs` with constructor injection
4. Update `Program.cs` to register the service
5. Test both endpoints

## ✅ Expected Behavior

```bash
curl http://localhost:5189/api/departments
```

Response:

```json
[
  { "id": 1, "name": "Engineering" },
  { "id": 2, "name": "HR" },
  { "id": 3, "name": "Finance" }
]
```

```bash
curl http://localhost:5189/api/departments/2
```

Response:

```json
{ "id": 2, "name": "HR" }
```

```bash
curl http://localhost:5189/api/departments/99
```

Response: `404 Not Found`

## 🔍 How to Test

```bash
cd Starter
dotnet run
# In another terminal:
curl http://localhost:5189/api/departments
curl http://localhost:5189/api/departments/1
curl http://localhost:5189/api/departments/99
```

## 💡 Hints

- Follow the same pattern as `EmployeesController` and `IEmployeeService`
- Use `static readonly` for the in-memory list
- Use `{id:int}` route constraint
- Return `NotFound()` when the department is `null`
- Register with `AddScoped<IDepartmentService, DepartmentService>()`

## 🎓 After Completing

Compare your solution with `Solution/` to see if you missed anything.

## 🏆 Congratulations!

You've completed Day 1. You can now:

- Create a Web API project
- Build controllers with routing
- Extract services behind interfaces
- Register and inject dependencies
- Trace the complete request flow

## 📚 Related Lesson

[Day 1 Overview](../../Day-1-Web-and-ASP.NET-Core.md)
