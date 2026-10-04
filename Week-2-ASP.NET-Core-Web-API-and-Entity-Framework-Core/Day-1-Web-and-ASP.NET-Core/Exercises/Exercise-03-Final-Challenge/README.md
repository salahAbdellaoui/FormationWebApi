# Exercise 03 — Final Challenge

## Objective

Build a small Departments API **from scratch**, applying everything from Day 1: HTTP and REST design, an ASP.NET Core project, controllers and routing, and Dependency Injection with a Scoped service.

## Folder

| Folder | What it is |
|--------|------------|
| [`Starter/`](Starter/) | A clean .NET 8 Web API skeleton — no model, no controller, no service, only TODO comments |
| [`Solution/`](Solution/) | A complete implementation (open it only after trying) |

## Requirements

1. Have a Web API project called `EmployeeManagement.Api`.
2. Add `GET /api/departments` returning a fixed in-memory list of departments.
3. Add `GET /api/departments/{id}` returning one department or `404`.
4. Put the data behind an `IDepartmentService` registered with **Scoped** and injected into the controller.
5. Be able to explain, in one paragraph, what happens from `curl .../api/departments/2` to the JSON in the response.

These are the five steps of the **Final Challenge** in the [Day 1 overview](../../Day-1-Web-and-ASP.NET-Core.md).

## Expected Result

Captured from the `Solution/`:

```text
GET /api/departments      → 200
[{"id":1,"name":"Engineering","location":"Building A"},{"id":2,"name":"HR","location":"Building B"},{"id":3,"name":"Finance","location":"Building A"}]

GET /api/departments/3    → 200
{"id":3,"name":"Finance","location":"Building A"}

GET /api/departments/99   → 404
```

The Starter returns `404` with an empty body for every request until you build the endpoints (verified).

## How to Run

Option A — as written in the lesson (create the project yourself):

```bash
dotnet new webapi -n EmployeeManagement.Api --use-controllers
```

Option B — use the provided skeleton:

```bash
cd Starter
dotnet restore
dotnet build
dotnet run
```

The console prints your address, for example:

```text
Now listening on: http://localhost:5263
```

Then:

```bash
curl -i http://localhost:5263/api/departments
curl -i http://localhost:5263/api/departments/2
curl -i http://localhost:5263/api/departments/99
```

Use the port printed by **your** console.

## Your Task

Implement steps 1–4, verify all three status codes, then write your one-paragraph explanation of the full request path before opening the `Solution/`.

## Related Lesson

[Day 1 — Overview & Final Challenge](../../Day-1-Web-and-ASP.NET-Core.md)
