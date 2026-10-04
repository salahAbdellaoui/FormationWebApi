# Exercise 01 — Controllers and Routing

## Objective

Build the `Departments` endpoints of an API using an ASP.NET Core controller, attribute routing, a route parameter, and appropriate HTTP status codes.

## Folder

| Folder | What it is |
|--------|------------|
| [`Starter/`](Starter/) | A running .NET 8 Web API with the `Department` model and an **empty** controller |
| [`Solution/`](Solution/) | The finished implementation (open it only after trying) |

## Requirements

1. Give the controller a base route built with the `[controller]` token, so it answers on `/api/departments`.
2. Add a static in-memory list of at least three departments inside the controller.
3. Add `GET /api/departments` → `200` + the whole list.
4. Add `GET /api/departments/{id:int}` → `200` + one department, or `404` when the id does not exist.

The four TODOs are already marked in [`Starter/Controllers/DepartmentsController.cs`](Starter/Controllers/DepartmentsController.cs).

## Expected Result

Requests made to the **Starter** before you implement anything return `404` with an empty body — the controller has no actions yet (verified).

After the implementation, these responses are expected. The bodies below were captured from the `Solution/`:

```text
GET /api/departments      → 200
[{"id":1,"name":"Engineering","location":"Building A"},{"id":2,"name":"HR","location":"Building B"},{"id":3,"name":"Finance","location":"Building A"}]

GET /api/departments/1    → 200
{"id":1,"name":"Engineering","location":"Building A"}

GET /api/departments/99   → 404   (the id does not exist)

GET /api/departments/abc  → 404 with an empty body   ({id:int} rejects it before the action runs)
```

> The values above come from the sample data in this exercise. Your own data may differ.

## How to Run

```bash
cd Starter
dotnet restore
dotnet build
dotnet run
```

The console prints your address, for example:

```text
Now listening on: http://localhost:5228
```

Then:

```bash
curl -i http://localhost:5228/api/departments
curl -i http://localhost:5228/api/departments/1
curl -i http://localhost:5228/api/departments/99
```

Use the port printed by **your** console — it comes from this project's `Properties/launchSettings.json`.

## Your Task

Implement the four TODOs in the Starter so that all four checks above return the expected status codes. Keep the controller free of business logic — HTTP only.

## Related Lesson

[04 — Controllers and Routing](../../04-Controllers-and-Routing.md) — Exercise 1
