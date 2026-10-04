# Day 1 — Exercises

Practical coding exercises for **Day 1 — Web & ASP.NET Core**.

Every exercise targets **.NET 8 / ASP.NET Core 8** and uses in-memory data (no database).

## 📋 Exercise List

| # | Exercise | Kind | README |
|---|----------|------|--------|
| 01 | Create the Web API Project | Hands-on | [README](01-Create-Web-API-Project/README.md) |
| 02 | Create `EmployeesController` | Starter + Solution | [README](02-Employees-Controller/README.md) |
| 03 | `GET /api/employees` | Starter + Solution | [README](03-Get-All-Employees/README.md) |
| 04 | `GET /api/employees/{id}` | Starter + Solution | [README](04-Get-Employee-By-Id/README.md) |
| 05 | Routing Challenge (debug) | Broken starter + fix | [README](05-Routing-Challenge/README.md) |
| 06 | Extract `IEmployeeService` | Starter + Solution | [README](06-Employee-Service/README.md) |
| 07 | Register & inject | Starter + Solution | [README](07-Dependency-Injection/README.md) |
| 08 | Final Challenge: Departments API | Starter + Solution | [README](08-Final-Challenge/README.md) |

## 🏗️ Structure

Each exercise folder contains:

- **README.md** — objective, requirements, how to run, how to test
- **Starter/** — the starting state (with TODOs, not the solution)
- **Solution/** — the completed implementation (open only after trying)

Exercises 02–04 and 06–07 build on each other. The Starter of Exercise N is the Solution of Exercise N-1.

## 📝 Suggested Order

1. **Exercise 01** — create the project, run it
2. **Exercise 02** — add the controller skeleton
3. **Exercise 03** — return the employee list
4. **Exercise 04** — add the `GET /{id}` endpoint
5. **Exercise 05** — debug a broken controller (standalone)
6. **Exercise 06** — extract the service
7. **Exercise 07** — register and inject
8. **Exercise 08** — final challenge: build a Departments API from scratch

## 🔧 Commands (verified)

```bash
cd Starter        # or Solution
dotnet restore
dotnet build
dotnet run
```

The console prints your address:

```text
Now listening on: http://localhost:5228
```

Then test with `curl`:

```bash
curl -i http://localhost:5228/api/employees
```

Use the port printed by **your** console.

---

> 💡 **Senior Developer Note:** Do not open `Solution/` before you have tried the `Starter/`. The value of these exercises is in the debugging, not in the copying.
