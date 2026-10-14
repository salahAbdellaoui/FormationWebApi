# Capstone Exercise — Employee Management Web API

**Duration:** 3–4 hours (can extend for deeper learning)

**Scope:** This exercise brings together everything from Weeks 1–3: C#, SQL Server, ASP.NET Core Web API, Entity Framework Core, Clean Architecture, application services, repositories, DTOs, authentication, authorization, API security, exception handling, validation, logging, configuration, CORS, unit testing, integration testing, mocking, SOLID, Clean Code, and refactoring.

You work as a developer responsible for improving a real enterprise Web API. The starting point is the EmployeeManagement solution from Day 1. You will analyze it, extend it, secure it, test it, and justify your design decisions.

---

## Starting Point

The existing EmployeeManagement solution has:

**Domain layer:**
- `Employee` entity (Id, Name, Email, Salary, DepartmentId, Department)
- `Department` entity (Id, Name, Employees collection)

**Application layer:**
- `IEmployeeRepository` interface (GetAllAsync, GetByIdAsync, GetByIdWithDepartmentAsync, GetWithDepartmentAsync, AddAsync)
- `IEmployeeService` interface (GetAllEmployeesAsync, GetEmployeeByIdAsync, CreateEmployeeAsync)
- `EmployeeService` implementation
- `EmployeeDto` and `CreateEmployeeDto`

**Infrastructure layer:**
- `EmployeeRepository` with EF Core
- `AppDbContext` with Departments and Employees DbSets
- `DependencyInjection` extension method

**API layer:**
- `EmployeesController` with GET all, GET by id, and POST create endpoints
- `Program.cs` with basic middleware pipeline

**What exists:**
- Clean Architecture with four projects
- Repository pattern
- DTO mapping
- Async data access with EF Core
- SQL Server as the database

**What is missing:**
- Update and Delete operations
- Authentication and authorization
- Input validation
- Global exception handling
- Structured logging
- CORS configuration
- Configuration management for secrets
- Unit tests
- Integration tests
- Any test project

---

## Phase 1 — Understand and Analyze the Existing Application

Before writing any code, understand what you are working with.

### Tasks

1. Open the EmployeeManagement solution. Build it and run it. Verify it works.

2. Trace a `POST /api/employees` request from the controller to the database. Write down every method call, every layer it passes through, and every file involved.

3. Draw the project dependency graph. Which project references which? Does any project violate the dependency rule (inner layers referencing outer layers)?

4. Answer these questions in writing. Do not skip any.

   - What responsibility does each layer have in the current implementation?
   - Where does the `EmployeeService` get its dependencies? How are they provided?
   - What happens if `CreateEmployeeAsync` receives a `CreateEmployeeDto` with an empty Name?
   - What happens if the `DepartmentId` does not exist in the database?
   - What happens if two employees are created with the same email address?
   - Which HTTP methods and status codes does the current API support?
   - Which common employee management operations are missing?

5. List at least three design problems or missing features you would address if you owned this codebase. Justify each one.

### Deliverable

A short written document (markdown or plain text) with your answers, your dependency graph, and your list of design problems. Submit this before moving to Phase 2.

---

## Phase 2 — Database and Data Access

Connect your knowledge of SQL Server and EF Core from Weeks 1–2 to the Clean Architecture solution.

### Tasks

1. **Review the data model.**
   - The `Employee` entity has a foreign key `DepartmentId` and a navigation property `Department`.
   - The `Department` entity has a collection `Employees`.
   - Verify these relationships are correctly configured in `AppDbContext`. Are explicit configurations needed?

2. **Extend the repository.**
   The current `IEmployeeRepository` only supports reading and creating. Add the methods needed to support updating and deleting employees. Consider:
   - How should an update operation work? Fetch-then-modify or attach-and-modify?
   - Should the repository call `SaveChangesAsync` or should the service control that?
   - What method is needed to check if a department exists before assigning an employee to it?

3. **Verify the schema.**
   - Are the column types appropriate? Is `Salary` the right precision for monetary values?
   - Should there be constraints on `Name` length? On `Email` format at the database level?
   - Is the primary key generation strategy appropriate?

4. **Consider query efficiency.**
   - Which queries use `AsNoTracking()`? Why?
   - When is `Include` necessary vs unnecessary?
   - If you add a "get employee by email" check, how should the query be written?

### Acceptance Criteria

- The repository supports all operations needed for CRUD (Create, Read, Update, Delete).
- All data access methods are asynchronous.
- Missing records are handled correctly (return null, not exceptions).
- EF Core entities are not exposed directly through the API.
- The Domain layer remains independent of EF Core.

### Review Questions

- Why does `GetByIdWithDepartmentAsync` use `Include` but `GetByIdAsync` does not?
- What is the trade-off between `SaveChangesAsync` in the repository vs. in the service?
- If you needed to add a `Position` property to `Employee`, which files would change and which would not?

---

## Phase 3 — API Design and Application Logic

Implement the missing API behavior. Work across all four layers.

### Tasks

1. **Add Update support.**
   - Create `UpdateEmployeeDto` in the Application layer.
   - Add `UpdateEmployeeAsync` to `IEmployeeService` and implement it in `EmployeeService`.
   - Add the corresponding repository method(s).
   - Add a `PUT /api/employees/{id}` endpoint in `EmployeesController`.
   - Return `200 Ok` with the updated DTO on success.
   - Return `404 Not Found` when the employee does not exist.

2. **Add Delete support.**
   - Add `DeleteEmployeeAsync` to `IEmployeeService` and implement it.
   - Add the corresponding repository method(s).
   - Add a `DELETE /api/employees/{id}` endpoint.
   - Return `204 No Content` on success.
   - Return `404 Not Found` when the employee does not exist.

3. **Improve Create.**
   - The current `CreateEmployeeAsync` does not validate input or check if the department exists. Add a department existence check that throws a meaningful exception when the department is not found.
   - After creating the employee, the response should include the `DepartmentName`. Re-query the employee with its department after the insert (as practiced in Day 2 exercises).

4. **Define DTO contracts.**
   - Ensure every DTO has only the properties the API needs.
   - Do not expose navigation properties or internal identifiers that the client does not need.
   - Use appropriate types (`decimal` for salary, `int` for IDs).

### Acceptance Criteria

- `POST /api/employees` — creates an employee, returns `201 Created` with the full DTO including `DepartmentName`.
- `GET /api/employees` — returns all employees with department names.
- `GET /api/employees/{id}` — returns one employee or `404`.
- `PUT /api/employees/{id}` — updates an employee, returns `200 Ok` with updated DTO, or `404`.
- `DELETE /api/employees/{id}` — deletes an employee, returns `204 No Content`, or `404`.
- All operations are async.
- Controllers contain no business logic — they handle HTTP concerns only.
- Services contain no HTTP concerns — they throw exceptions for invalid states.

### Review Questions

- Which files did you change to add Update? To add Delete?
- Could you add a new endpoint without changing the Domain layer? Why or why not?
- Where does the department existence check belong — controller, service, or repository?

---

## Phase 4 — Clean Architecture and SOLID

Review whether your implementation from Phases 2–3 follows the principles taught in Weeks 1–3.

### Tasks

1. **Dependency direction audit.**
   - Draw the project reference graph again. Does every arrow point inward?
   - Does the Domain layer reference any outer layer?
   - Does the Application layer know about Infrastructure?

2. **Dependency Inversion audit.**
   - List every dependency in `EmployeeService`. Is each one an interface defined in the Application layer?
   - Is there any place where a concrete class is used instead of an abstraction?
   - Are there any `new` calls inside methods that should be injected dependencies?

3. **Single Responsibility audit.**
   - Does `EmployeeService` do only application coordination? Or has it taken on validation, notification, or data access responsibilities that belong elsewhere?
   - Does the controller do anything that is not an HTTP concern?

4. **SOLID analysis.**
   For each principle, find at least one place where it applies in your implementation:
   - **SRP**: Are classes focused on a single responsibility?
   - **OCP**: Could you add a new type of employee (e.g., Contractor) without modifying existing code?
   - **LSP**: Can you substitute any implementation for its interface without breaking behavior?
   - **ISP**: Are any interfaces forcing implementations to provide methods they do not need?
   - **DIP**: Do high-level modules depend on abstractions rather than low-level details?

5. **Avoid over-engineering.**
   - Do you have interfaces with only one implementation where the abstraction adds no testability or flexibility benefit?
   - Is every layer justified? Could any be collapsed without losing separation of concerns?

### Deliverable

A written analysis (one paragraph per principle) explaining:
- Where your implementation follows each SOLID principle.
- At least one trade-off or decision where you chose simplicity over strict principle adherence, and why.

---

## Phase 5 — Authentication and Authorization

Secure the API using the techniques from Days 3–4.

### Tasks

1. **Add JWT authentication.**
   - Configure JWT token validation in `Program.cs` using the settings from `appsettings.json`.
   - Store the signing key in User Secrets (not in `appsettings.json`).
   - The token should include: user ID, username, role, issuer, audience, and expiration.

2. **Add an authentication endpoint.**
   - Create an `AuthController` with a `POST /api/auth/login` endpoint.
   - Accept a username and password. Validate credentials and return a JWT.
   - For this exercise, use a simple in-memory user store (a dictionary or a hardcoded list). Do not add ASP.NET Core Identity unless it was explicitly taught.

3. **Add role-based authorization.**
   - Define at least two roles: `Admin` and `HR`.
   - Protect all employee endpoints with `[Authorize]`.
   - Apply the `Admin` role requirement to the Delete endpoint only.
   - Allow both `Admin` and `HR` to Create, Update, and read employees.

4. **Add authorization policies.**
   - Create a `CanManageEmployees` policy that requires the `Admin` or `HR` role.
   - Create an `AdminOnly` policy that requires the `Admin` role.
   - Apply these policies to the appropriate endpoints instead of repeating role names.

5. **Handle unauthorized access correctly.**
   - Missing or invalid token → `401 Unauthorized`.
   - Valid token but insufficient role → `403 Forbidden`.
   - Verify both status codes are returned correctly.

### Acceptance Criteria

- Unauthenticated requests to any `/api/employees` endpoint return `401`.
- Authenticated `HR` user can Create, Read, Update employees.
- Only `Admin` users can Delete employees.
- `HR` user attempting Delete receives `403`.
- The JWT signing key is not committed to source control.
- Token expiration is enforced.

### Review Questions

- Why does the signing key belong in User Secrets or environment variables rather than `appsettings.json`?
- What is the difference between `401` and `403`? When does each apply?
- Why use policies instead of repeating `[Authorize(Roles = "...")]` on every endpoint?

---

## Phase 6 — Validation, Exception Handling, and Logging

Make the API robust and production-ready.

### Tasks

1. **Add input validation.**
   - Create FluentValidation validators for `CreateEmployeeDto` and `UpdateEmployeeDto`.
   - Validate: Name is required and not longer than 100 characters. Email is required and in valid format. Salary must be positive. DepartmentId must be greater than zero.
   - Register the validators. Ensure invalid requests return `400 Bad Request` with a structured error response listing all validation errors.

2. **Add global exception handling.**
   - Create an exception handling middleware that catches unhandled exceptions.
   - Return Problem Details (RFC 7807) format for all error responses.
   - Never expose stack traces, internal paths, or database error messages to the client.
   - Log the full exception details server-side while returning a safe message to the client.

3. **Add structured logging.**
   - Inject `ILogger<T>` into `EmployeeService` and `EmployeesController`.
   - Log at appropriate levels:
     - `Information` when an employee is created, updated, or deleted (include the employee ID).
     - `Warning` when a requested employee is not found.
     - `Error` when an unexpected exception occurs.
   - Use structured logging with named placeholders, not string interpolation.

4. **Configure CORS.**
   - Add a CORS policy that allows requests from a specific frontend origin (e.g., `http://localhost:3000`).
   - Allow the `Authorization` header and `Content-Type` header.
   - Apply the CORS policy in the middleware pipeline in the correct position.

5. **Verify the middleware pipeline order.**
   Confirm this order in `Program.cs`:
   1. Exception handler (first — wraps everything)
   2. CORS
   3. HTTPS redirection
   4. Authentication
   5. Authorization
   6. Controller mapping

### Acceptance Criteria

- Invalid input returns `400` with all validation errors listed.
- Unhandled exceptions return `500` with a Problem Details body that contains no internal information.
- Logs are written with structured data that can be searched and filtered.
- Cross-origin requests from the allowed origin succeed. Requests from other origins are blocked.
- The middleware pipeline is in the correct order.

---

## Phase 7 — Testing

Write automated tests that verify the application behavior.

### Tasks

1. **Set up test projects.**
   - Create a unit test project: `EmployeeManagement.Application.Tests`.
   - Create an integration test project: `EmployeeManagement.Api.IntegrationTests`.
   - Use xUnit as the test framework.
   - Add the necessary project references.

2. **Write unit tests for `EmployeeService`.**
   Write at least these tests:
   - `CreateEmployeeAsync` with valid data creates an employee and returns the correct DTO.
   - `CreateEmployeeAsync` with a non-existent department throws an appropriate exception.
   - `GetEmployeeByIdAsync` returns null when the employee does not exist.
   - `GetEmployeeByIdAsync` returns the correct DTO when the employee exists.
   - `GetAllEmployeesAsync` returns all employees.
   - `UpdateEmployeeAsync` returns null when the employee does not exist (if implemented).
   - `DeleteEmployeeAsync` behaves correctly when the employee does not exist (if implemented).

   Use mocking to replace `IEmployeeRepository`. Verify that the service calls the repository methods correctly. Assert on the returned DTOs, not on the entities.

3. **Write integration tests for API endpoints.**
   Use `WebApplicationFactory<Program>` to create a test server. Write at least these tests:
   - `GET /api/employees` returns `200 Ok` with a JSON array.
   - `GET /api/employees/{id}` returns `200 Ok` for an existing employee.
   - `GET /api/employees/{id}` returns `404 Not Found` for a non-existent ID.
   - `POST /api/employees` returns `201 Created` with valid data.
   - `POST /api/employees` returns `400 Bad Request` with invalid data.
   - `POST /api/employees` returns `401 Unauthorized` without a valid token.

   For integration tests that require authentication, configure the test server to bypass or mock JWT validation.

4. **Apply the Arrange–Act–Assert pattern.**
   Every test must follow this structure:
   - **Arrange**: Set up the test data, mocks, or client.
   - **Act**: Call the method or send the request.
   - **Assert**: Verify the result.

5. **Verify the tests pass.**
   Run `dotnet test` and confirm all tests pass. Fix any failures before moving on.

### Acceptance Criteria

- Unit tests do not depend on a database, file system, or network.
- Unit tests run in milliseconds each.
- Integration tests verify HTTP behavior including status codes, routing, and serialization.
- All tests pass with `dotnet test`.
- Test names describe the scenario being tested.

### Review Questions

- What is the difference between the unit test for `CreateEmployeeAsync` and the integration test for `POST /api/employees`? What does each catch that the other cannot?
- Why do unit tests mock the repository but integration tests do not?
- If you change the service logic, which tests need to change? If you change the database schema, which tests need to change?

---

## Phase 8 — Clean Code and Refactoring

Review the code you have written across all phases and improve it.

### Tasks

1. **Identify code smells.**
   Review your implementation. Look for:
   - Methods longer than 20 lines.
   - Duplication (similar code in Create and Update, for example).
   - Poorly named variables or methods.
   - Nested `if` statements that could be guard clauses.
   - Magic numbers or strings.

2. **Refactor at least two code smells.**
   For each refactor:
   - Describe the smell you found.
   - Explain what you changed.
   - Run the tests to confirm behavior is preserved.

3. **Apply Clean Code practices.**
   - Every method should do one thing.
   - Names should reveal intent.
   - Functions should be small.
   - Avoid comments that explain what the code does (the code should be self-explanatory).

4. **Review the complete solution.**
   After refactoring, verify:
   - The build succeeds with zero warnings.
   - All tests pass.
   - The API works correctly when run manually.

### Acceptance Criteria

- `dotnet build` succeeds with zero warnings and zero errors.
- All tests from Phase 7 still pass after refactoring.
- At least two code smells identified and refactored, with a written description of each.
- No method exceeds 20 lines of meaningful code.
- No copy-paste duplication between Create and Update logic.
- All manual API tests (from Phase 3 acceptance criteria) still pass.

---

## Optional Extensions

These tasks go beyond the required curriculum. Attempt them only after completing Phases 1–8.

### Extension A — Pagination and Filtering

Add filtering by department, searching by name or email, sorting, and pagination to the `GET /api/employees` endpoint. Use the patterns from Week 2, Day 4–5 exercises. Adapt them to work within the Clean Architecture layers (the query logic should be in the service or repository, not the controller).

### Extension B — Email Notification Service

Extract an `IEmailSender` interface in the Application layer. Implement it in Infrastructure. Inject it into `EmployeeService` and send a welcome email when an employee is created. Write a unit test that verifies the email sender is called with the correct parameters.

### Extension C — Audit Logging

Add a mechanism that records who created, updated, or deleted an employee and when. Store the audit data in a separate table. Write integration tests that verify audit records are created.

### Extension D — Health Checks

Add ASP.NET Core health checks that verify the database connection is available. Configure a `/health` endpoint.

---

## Evaluation Criteria

Your work will be evaluated on nine criteria. Each has a clear standard for what constitutes acceptable and strong performance.

| Criterion | Focus |
|---|---|
| **Architecture and SOLID** | Correct layer separation, inward-pointing dependencies, thoughtful design decisions |
| **CRUD Functionality** | All endpoints work with correct HTTP methods and status codes |
| **Authentication and Authorization** | JWT works, roles enforced, policies applied, secrets protected |
| **Validation and Error Handling** | Input validated before business logic, safe error responses, no internal details leaked |
| **Testing** | Unit tests verify business logic in isolation, integration tests verify HTTP behavior, all tests pass |
| **Logging, Configuration, and CORS** | Structured logs at correct levels, secrets managed safely, CORS configured |
| **Code Quality and Refactoring** | Clean names, small methods, no duplication, evidence of deliberate improvement |
| **Design Reasoning** | Written analysis demonstrates understanding of why decisions were made |

Testing carries the highest weight, as it is the primary focus of Day 5. All criteria must reach a minimum acceptable level to pass.

---

## Submission Checklist

Before submitting, verify you have:

- [ ] Written analysis document from Phase 1
- [ ] SOLID analysis from Phase 4
- [ ] Working CRUD API with all endpoints
- [ ] JWT authentication and role-based authorization
- [ ] FluentValidation validators for all input DTOs
- [ ] Global exception handling middleware
- [ ] Structured logging in service and controller
- [ ] CORS configuration
- [ ] Unit test project with tests for `EmployeeService`
- [ ] Integration test project with tests for API endpoints
- [ ] All tests passing (`dotnet test` succeeds)
- [ ] Build succeeds with zero warnings
- [ ] No secrets or signing keys committed to source control
- [ ] Refactoring log from Phase 8

---

## Important Notes

1. **Do not copy complete solutions.** The goal is to demonstrate your ability to apply what you learned. If you get stuck, review the relevant lesson topic and try again.

2. **Work in order.** Each phase builds on the previous one. Skipping phases will make later phases harder.

3. **Ask questions about the requirements.** If an acceptance criterion is unclear, clarify it before implementing. Understanding requirements is part of the exercise.

4. **Tests are not optional.** Phases 3–5 produce functionality. Phase 7 proves it works. If you skip testing, you cannot verify your implementation.

5. **The written analysis matters as much as the code.** Being able to explain your decisions is what separates a developer who follows instructions from one who understands the system.
