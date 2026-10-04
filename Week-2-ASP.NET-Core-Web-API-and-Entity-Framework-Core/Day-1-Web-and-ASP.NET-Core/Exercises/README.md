# Week 2 Day 1 — Practical Exercises

Practical coding exercises for **Day 1 — Web & ASP.NET Core**.

Every exercise targets **.NET 8 / ASP.NET Core 8** and uses the same in-memory **Departments** resource as the lesson (no database, no EF Core, no authentication).

| Exercise | Topic | Lesson | Project |
|----------|-------|--------|---------|
| 01 | Controllers and Routing | [04 — Controllers and Routing](../04-Controllers-and-Routing.md) | [Exercise-01-Controllers-and-Routing](Exercise-01-Controllers-and-Routing/) |
| 02 | Dependency Injection | [05 — Dependency Injection](../05-Dependency-Injection.md) | [Exercise-02-Dependency-Injection](Exercise-02-Dependency-Injection/) |
| 03 | Final Challenge | [Day 1 — Final Challenge](../Day-1-Web-and-ASP.NET-Core.md) | [Exercise-03-Final-Challenge](Exercise-03-Final-Challenge/) |

## How each exercise folder is organised

```text
Exercise-0X-.../
├── README.md      ← objective, requirements, expected result, commands
├── Starter/       ← the code you start from (contains TODOs, not the solution)
└── Solution/      ← the finished implementation — check it only after trying
```

## Suggested order

1. **Exercise 01** — build the endpoints with a controller and routing.
2. **Exercise 02** — take that working controller and move the data behind a service with Dependency Injection.
3. **Exercise 03** — build the same feature from scratch, without the lesson open.

## Commands (verified for these projects)

```bash
cd Starter        # or Solution
dotnet restore
dotnet build
dotnet run
```

Then read the address the console prints:

```text
Now listening on: http://localhost:5228      ← example from Exercise 01
```

The port comes from `Properties/launchSettings.json` of the project you are running.

> 💡 **Senior Developer Note:** Do not open `Solution/` before you have tried the `Starter/`. The value of these exercises is in the debugging, not in the copying.
