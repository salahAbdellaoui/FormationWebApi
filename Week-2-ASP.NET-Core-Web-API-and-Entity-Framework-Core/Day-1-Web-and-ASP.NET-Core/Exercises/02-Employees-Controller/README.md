# Exercise 02 — Create `EmployeesController`

## 🎯 Objective

Create the `EmployeesController` skeleton with the correct attributes.

## 📋 Requirements

1. Add `[ApiController]` attribute to the controller
2. Add `[Route("api/[controller]")]` attribute to the controller
3. Add a `[HttpGet]` action that returns `Ok("employees endpoint")`

## 🚀 Starting Point

The `Starter/` folder contains:

- `Models/Employee.cs` (the model)
- `Controllers/EmployeesController.cs` (empty skeleton with TODOs)
- Template files (csproj, Program.cs, appsettings, launchSettings)

## 📝 Your Task

Open `Starter/Controllers/EmployeesController.cs` and complete the TODOs.

## ✅ Expected Behavior

```bash
dotnet run
curl -i http://localhost:5189/api/employees
```

Response:

```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

"employees endpoint"
```

## 🔍 How to Test

```bash
cd Starter
dotnet run
# In another terminal:
curl -i http://localhost:5189/api/employees
```

## 💡 Hints

- `[ApiController]` enables automatic model binding and validation
- `[Route("api/[controller]")]` sets the base URL using the `[controller]` token
- `[controller]` is replaced with the class name without "Controller" → `Employees`

## 🔜 Next

**Exercise 03** — `GET /api/employees` returns the employee list.

## 📚 Related Lesson

[04 — Controllers and Routing](../../04-Controllers-and-Routing.md)
