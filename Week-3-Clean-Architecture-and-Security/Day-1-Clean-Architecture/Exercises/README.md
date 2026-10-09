# Day 1 — Exercises

**Duration**: 30 minutes

Complete these exercises in order. Each one builds on the previous. The solution code is in the `Code/` folder if you get stuck.

---

## Exercise 1 — Identify the Responsibilities

### Goal

Recognize which parts of a mixed-responsibility method belong to which Clean Architecture layer.

### Starting Point

A developer wrote this controller method that mixes concerns from all four layers:

```csharp
[HttpPost]
public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Name))
        return BadRequest("Name is required");

    if (dto.Salary <= 0)
        return BadRequest("Salary must be positive");

    var employee = new Employee
    {
        Name = dto.Name,
        Email = dto.Email,
        Salary = dto.Salary,
        DepartmentId = dto.DepartmentId
    };

    using var connection = new SqlConnection(_connectionString);
    await connection.ExecuteAsync(
        "INSERT INTO Employees (Name, Email, Salary, DepartmentId) VALUES (@Name, @Email, @Salary, @DepartmentId)",
        employee);

    return CreatedAtAction(nameof(GetById), new { id = employee.Id }, dto);
}
```

### Task

Annotate which lines belong to which layer. Fill in the table:

| Lines | Layer | Responsibility |
|---|---|---|
| ? | API | ? |
| ? | Application | ? |
| ? | Domain | ? |
| ? | Infrastructure | ? |

### Expected Behavior

Each line maps to exactly one layer. Lines that belong to a layer handle only that layer's responsibility.

### Hints

- API handles HTTP concerns: status codes, request/response, routing
- Application handles use case coordination: mapping DTOs to entities, calling services
- Domain handles business rules: what data is valid
- Infrastructure handles persistence: SQL queries, database connections

### Verification

Compare your answer with the separated version in topic 02 (Separation of Concerns).

### Common Mistake

Treating the DTO-to-entity mapping as an API concern. The mapping is Application. The API should only receive the DTO and return the response.

### Review Question

If the `if (dto.Salary <= 0)` check stays in the controller, what happens when a background job creates an employee without going through the API?

---

## Exercise 2 — Extract a Business Rule

### Goal

Move a salary validation from the controller into the Application service.

### Starting Point

The controller currently has:

```csharp
[HttpPost]
public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
{
    if (dto.Salary <= 0)
        return BadRequest("Salary must be positive");

    var created = await _employeeService.CreateEmployeeAsync(dto);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
}
```

The `EmployeeService.CreateEmployeeAsync` method does not validate salary.

### Task

1. Add salary validation in `EmployeeService.CreateEmployeeAsync`. If `Salary <= 0`, throw an `ArgumentException`.
2. Update the controller to catch `ArgumentException` and return `BadRequest`.

### Expected Behavior

- `POST /api/employees` with `Salary = -100` returns `400 Bad Request` with the message "Salary must be positive"
- `POST /api/employees` with `Salary = 50000` returns `201 Created`
- The validation runs regardless of how the service is called (API, background job, test)

### Hints

- The service should throw, not return an HTTP result
- The controller catches the exception and translates it to an HTTP response
- Use `ArgumentException` with a descriptive message

### Verification

```bash
curl -X POST https://localhost:<port>/api/employees \
  -H "Content-Type: application/json" \
  -d '{"name":"Test","email":"test@test.com","salary":-100,"departmentId":1}'
```

Expected: `400 Bad Request`

### Common Mistake

Returning `BadRequest` from the service. Services should not know about HTTP status codes. Throw an exception and let the controller handle it.

### Review Question

Why is it important that the salary validation runs in the service and not only in the controller? Name a scenario where the controller check would be bypassed.

---

## Exercise 3 — Create an Application Use Case

### Goal

Add an UpdateEmployee operation across all four layers.

### Starting Point

The current solution has Create, GetAll, and GetById. It does not have Update.

### Task

1. Create `UpdateEmployeeDto.cs` in `EmployeeManagement.Application/Dtos`:
   - Properties: `Name`, `Email`, `Salary`, `DepartmentId`

2. Add `UpdateEmployeeAsync` to `IEmployeeService` in Application:
   - Signature: `Task<EmployeeDto?> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto)`

3. Implement `UpdateEmployeeAsync` in `EmployeeService`:
   - Fetch the employee by ID
   - If not found, return `null`
   - Update properties from the DTO
   - Save changes (add an `UpdateAsync` method to `IEmployeeRepository` and `EmployeeRepository`)
   - Return the updated employee as `EmployeeDto`

4. Add `UpdateAsync` to `IEmployeeRepository` and implement it in `EmployeeRepository`

5. Add an `Update` endpoint in `EmployeesController`:
   - Route: `PUT /api/employees/{id}`
   - If service returns `null`, return `404 NotFound`
   - Otherwise return `200 Ok` with the updated DTO

### Expected Behavior

- `PUT /api/employees/1` with valid data returns `200 Ok` with updated employee
- `PUT /api/employees/999` returns `404 NotFound`

### Hints

- Follow the same pattern as `CreateEmployeeAsync`
- The repository needs a new method to update entities
- EF Core tracks changes automatically if you modify the entity on an existing DbContext

### Verification

```bash
curl -X PUT https://localhost:<port>/api/employees/1 \
  -H "Content-Type: application/json" \
  -d '{"name":"Updated Name","email":"updated@test.com","salary":75000,"departmentId":1}'
```

Expected: `200 Ok` with updated employee data

### Common Mistake

Creating a new `Employee` entity instead of fetching and updating the existing one. This would create a duplicate row instead of modifying the existing record.

### Review Question

Which files did you change to add this feature? Could you have added it without touching Domain? Why or why not?

---

## Exercise 4 — Implement a Persistence Dependency

### Goal

Add a method to the repository that checks if a department exists, and use it in the service to validate before creating an employee.

### Starting Point

`EmployeeService.CreateEmployeeAsync` creates an employee without checking if the department exists. If the `DepartmentId` is invalid, the database throws a foreign key constraint error.

### Task

1. Add `Task<bool> DepartmentExistsAsync(int departmentId)` to `IEmployeeRepository`

2. Implement it in `EmployeeRepository`:
   - Query the `Departments` table using EF Core
   - Return `true` if the department exists, `false` otherwise

3. Update `EmployeeService.CreateEmployeeAsync`:
   - Before creating the employee, call `_repository.DepartmentExistsAsync(dto.DepartmentId)`
   - If it returns `false`, throw an `ArgumentException` with message "Department not found"

4. Update the controller to catch this exception and return `400 Bad Request`

### Expected Behavior

- Creating an employee with a valid `DepartmentId` returns `201 Created`
- Creating an employee with `DepartmentId = 999` (non-existent) returns `400 Bad Request` with message "Department not found"

### Hints

- Use `_context.Departments.AnyAsync(d => d.Id == departmentId)` in the repository
- The service throws, the controller catches and returns HTTP response

### Verification

```bash
curl -X POST https://localhost:<port>/api/employees \
  -H "Content-Type: application/json" \
  -d '{"name":"Test","email":"test@test.com","salary":50000,"departmentId":999}'
```

Expected: `400 Bad Request` with message "Department not found"

### Common Mistake

Putting the department existence check in the controller using a direct database call. The check belongs in the service (business rule) and repository (data access).

### Review Question

Why does the department existence check belong in the service instead of the controller? What happens if you only check in the controller?

---

## Exercise 5 — Verify the Architecture

### Goal

Confirm that the solution follows Clean Architecture principles by inspecting project references, DI configuration, and request flow.

### Starting Point

The completed EmployeeManagement solution from the guided implementation (with any changes you made in exercises 1-4).

### Task

1. **Check project references**: Run `dotnet build` and verify it succeeds. Then check each `.csproj` file:
   - Domain: no project references
   - Application: references Domain only
   - Infrastructure: references Domain and Application
   - API: references Domain, Application, and Infrastructure

2. **Check DI configuration**: Open `DependencyInjection.cs` in Infrastructure. Verify that:
   - `IEmployeeRepository` is registered with `EmployeeRepository`
   - `IEmployeeService` is registered with `EmployeeService`
   - `AppDbContext` is configured with SQL Server

3. **Trace a Create request**: Starting from `POST /api/employees`, trace the request through all four layers:
   - API: which controller method handles the request?
   - Application: which service method is called? What does it do?
   - Infrastructure: which repository method is called? What EF Core operation runs?
   - Database: what SQL is executed?

4. **Draw the dependency diagram from memory**: Without looking at any files, draw the project reference graph and the runtime request flow. Compare with your answer.

### Expected Behavior

- Build succeeds with zero warnings and zero errors
- DI registrations match the interface-implementation pairs
- The request trace passes through all four layers in the correct order
- Your dependency diagram matches the actual `.csproj` references

### Hints

- `dotnet build` output shows the exact projects compiled and in what order
- The composition root is Program.cs
- Use Swagger UI to send a test request and verify the full flow

### Verification

```bash
dotnet build
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

Then use Swagger UI to create an employee and verify the response is `201 Created`.

### Common Mistake

Assuming the build passes means the architecture is correct. The build only checks that references compile. You still need to verify that the Dependency Rule is followed (inner layers do not reference outer layers).

### Review Question

If you removed the API project, could Application and Infrastructure still compile and work together? What would be missing?
