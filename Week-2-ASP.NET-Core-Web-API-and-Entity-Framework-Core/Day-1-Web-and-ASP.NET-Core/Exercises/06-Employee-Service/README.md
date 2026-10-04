# Exercise 06 — Extract `IEmployeeService`

## 🎯 Objective

Extract the employee data and logic into a service with an interface.

## 📋 Requirements

1. Create `Services/IEmployeeService.cs` with:
   - `IEnumerable<Employee> GetAll()`
   - `Employee? GetById(int id)`
2. Create `Services/EmployeeService.cs` that implements the interface
3. Move the `static List<Employee>` into `EmployeeService`
4. Refactor `EmployeesController` to use `IEmployeeService` (but don't register/inject yet)

## 🚀 Starting Point

The `Starter/` folder contains the completed Exercise 04.

## 📝 Your Task

1. Create `Services/IEmployeeService.cs`
2. Create `Services/EmployeeService.cs`
3. Move the data and logic from the controller to the service
4. Update the controller to depend on `IEmployeeService`

## ✅ Expected Behavior

The controller should compile, but **will not work yet** because the service is not registered. That's OK — Exercise 07 will fix that.

## 🔍 How to Verify

```bash
dotnet build
```

The build should succeed. The endpoints will return `500` at runtime (service not registered) — that's expected.

## 💡 Hints

- The interface defines the contract: `GetAll()` and `GetById(int id)`
- The implementation holds the data: `private static readonly List<Employee>`
- The controller should have no data, only HTTP logic
- Add `using EmployeeManagement.Api.Services;` to the controller

## 🔜 Next

**Exercise 07** — Register and inject the service.

## 📚 Related Lesson

[05 — Dependency Injection](../../05-Dependency-Injection.md)
