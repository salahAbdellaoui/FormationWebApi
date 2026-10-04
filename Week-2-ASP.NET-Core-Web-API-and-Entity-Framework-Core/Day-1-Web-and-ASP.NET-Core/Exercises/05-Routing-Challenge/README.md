# Exercise 05 — Routing Challenge (Debug)

## 🎯 Objective

Find and fix three bugs in a broken controller.

## 📋 The Problem

This controller compiles, but every request returns `404`. Your task is to find the three bugs and fix them.

## 🚀 Starting Point

The `Starter/` folder contains a buggy `EmployeesController`.

## 📝 Your Task

1. Open `Starter/Controllers/EmployeesController.cs`
2. Find the three bugs
3. Fix them
4. Test that both endpoints work

## ✅ Expected Behavior (after fixing)

```bash
curl http://localhost:5189/api/employees
# → 200 + list of employees

curl http://localhost:5189/api/employees/1
# → 200 + one employee

curl http://localhost:5189/api/employees/99
# → 404 Not Found
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

- Bug 1: The `[Route]` attribute uses the wrong name
- Bug 2: The `[HttpGet]` attribute on `GetAll` has an unexpected value
- Bug 3: The parameter name in `GetById` doesn't match the route placeholder

## 🎓 After Fixing

Compare your solution with `Solution/Controllers/EmployeesController.cs`.

## 🔜 Next

**Exercise 06** — Extract the service.

## 📚 Related Lesson

[04 — Controllers and Routing](../../04-Controllers-and-Routing.md) — Debugging Challenges
