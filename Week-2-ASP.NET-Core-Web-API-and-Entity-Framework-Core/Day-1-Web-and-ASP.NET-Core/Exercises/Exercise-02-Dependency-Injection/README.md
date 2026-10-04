# Exercise 02 — Dependency Injection

## Objective

Take a working controller that holds its own data and extract everything except HTTP into a service, registered with Dependency Injection and injected through the constructor.

## Folder

| Folder | What it is |
|--------|------------|
| [`Starter/`](Starter/) | A **working** `DepartmentsController` that still contains the data and the lookup logic |
| [`Solution/`](Solution/) | The refactored version: `IDepartmentService` + `DepartmentService` + a thin controller |

## Requirements

1. Create `Services/IDepartmentService.cs` with `GetAll()` and `GetById(int id)`.
2. Create `Services/DepartmentService.cs` and **move** the list and the lookups into it.
3. Register it in `Program.cs` as **Scoped**, before `builder.Build()`, including the `using` directive.
4. Inject `IDepartmentService` through the controller constructor.

The controller must finish with:

- no `List<Department>` field,
- no `FirstOrDefault` call,
- no data at all — only routing, service calls, and status codes.

## Expected Result

**Important:** the Starter already answers `200` (verified). This is a **refactoring** exercise, not a bug fix — after your change, the same endpoints must behave exactly as before:

```text
GET /api/departments      → 200 + the list
GET /api/departments/1    → 200 + {"id":1,"name":"Engineering","location":"Building A"}
GET /api/departments/99   → 404
```

Captured from the `Solution/`:

```text
[{"id":1,"name":"Engineering","location":"Building A"},{"id":2,"name":"HR","location":"Building B"},{"id":3,"name":"Finance","location":"Building A"}]
```

If a `500` appears after your change, read the error page: a missing registration reports that the service type cannot be resolved while activating the controller.

## How to Run

```bash
cd Starter
dotnet restore
dotnet build
dotnet run
```

The console prints your address, for example:

```text
Now listening on: http://localhost:5201
```

Then:

```bash
curl -i http://localhost:5201/api/departments
curl -i http://localhost:5201/api/departments/1
curl -i http://localhost:5201/api/departments/99
```

Use the port printed by **your** console.

## Your Task

Refactor the Starter following the four requirements, then re-run the three `curl` calls and confirm the status codes did not change.

## Related Lesson

[05 — Dependency Injection](../../05-Dependency-Injection.md) — Exercise 1
