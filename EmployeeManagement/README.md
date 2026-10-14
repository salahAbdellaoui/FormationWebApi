# EmployeeManagement Web API

A training Web API built with ASP.NET Core 8, Entity Framework Core, SQL Server, and Clean Architecture.

This solution is developed progressively across multiple phases. Each phase adds a new layer of functionality while preserving the architectural boundaries established in this initial structure.

---

## Purpose

The application manages employee records for an organization. Authorized users can create, read, update, and delete employees through a RESTful API. The system demonstrates professional practices including layered architecture, dependency injection, input validation, authentication, authorization, structured logging, and automated testing.

---

## Project Structure

The solution follows **Clean Architecture** with four projects. Each project has a single, well-defined responsibility.

```text
EmployeeManagement/
├── src/
│   ├── EmployeeManagement.Domain/          # Innermost layer — entities and business rules
│   │   └── Entities/                       # Employee, Department, and other domain objects
│   │
│   ├── EmployeeManagement.Application/     # Use cases — application logic and contracts
│   │   ├── Interfaces/                     # Repository and service interfaces
│   │   └── DTOs/                           # Data Transfer Objects for API communication
│   │
│   ├── EmployeeManagement.Infrastructure/  # Outer layer — technical implementations
│   │   ├── Persistence/                    # EF Core DbContext and configuration
│   │   └── Repositories/                   # Concrete repository implementations
│   │
│   └── EmployeeManagement.API/             # Outermost layer — HTTP entry point
│       └── Controllers/                    # API controllers that handle HTTP requests
│
├── tests/
│   ├── EmployeeManagement.Domain.Tests/
│   ├── EmployeeManagement.Application.Tests/
│   └── EmployeeManagement.API.Tests/
│
├── EmployeeManagement.slnx
└── README.md
```

---

## Role of Each Project

### EmployeeManagement.Domain

The innermost layer. Contains:

- **Entities** — the core business objects (Employee, Department).
- **Business rules** — validation logic and invariants that are always true.

This project has **no dependencies** on any other project or external library. It does not reference ASP.NET Core, Entity Framework Core, or any infrastructure concern. If you need to change the database, the web framework, or the JSON serializer, Domain should not change.

### EmployeeManagement.Application

The use-case layer. Contains:

- **Interfaces** — contracts for repositories and services that Infrastructure implements.
- **DTOs** — data shapes that flow between the API and the Application layer.
- **Services** — orchestration logic that coordinates repositories to fulfill use cases.

This project references only **Domain**. It defines what the application can do without deciding how it is done. The interfaces it defines are implemented by Infrastructure, but Application never knows which implementation is used.

### EmployeeManagement.Infrastructure

The technical implementation layer. Contains:

- **Persistence** — the EF Core `DbContext` that maps entities to SQL Server tables.
- **Repositories** — concrete implementations of the interfaces defined in Application.

This project references both **Application** (to implement its interfaces) and **Domain** (to work with entities). It is the only project that knows about Entity Framework Core, SQL Server, or any other data-access technology.

### EmployeeManagement.API

The HTTP entry point. Contains:

- **Controllers** — receive HTTP requests, delegate to Application services, and return HTTP responses.
- **Middleware** — cross-cutting concerns like exception handling, authentication, and CORS (added in later phases).

This project references **Application** (for interfaces and DTOs) and **Infrastructure** (to register concrete implementations in the dependency injection container). Controllers never access the database directly and never contain business logic.

---

## Domain Model

The Domain layer contains two entities that represent the core business concepts.

### Department

A Department is an organizational unit (e.g., "Engineering", "Human Resources") that groups employees.

**Properties:**

| Property | Type | Required | Description |
|---|---|---|---|
| `Id` | `int` | Auto | Unique identifier, assigned by the database |
| `Name` | `string` | Yes | Department name, trimmed, non-empty |
| `Description` | `string?` | No | Optional description of the department's role |
| `IsActive` | `bool` | Yes | Whether the department is currently in use (default: `true`) |

**Business rules enforced by the entity:**

- Name cannot be null, empty, or whitespace. The constructor and `UpdateName()` both validate this.
- Names are trimmed automatically to prevent invisible differences (e.g., " Engineering " becomes "Engineering").
- A department can be activated or deactivated through `Activate()` and `Deactivate()`.

### Employee

An Employee is a person employed by the organization, assigned to a department.

**Properties:**

| Property | Type | Required | Description |
|---|---|---|---|
| `Id` | `int` | Auto | Unique identifier, assigned by the database |
| `FirstName` | `string` | Yes | Employee's first name, trimmed |
| `LastName` | `string` | Yes | Employee's last name, trimmed |
| `Email` | `string` | Yes | Employee's email address, trimmed |
| `PhoneNumber` | `string?` | No | Optional phone number |
| `HireDate` | `DateTime` | Yes | The date the employee started |
| `Salary` | `decimal` | Yes | Must be zero or positive |
| `DepartmentId` | `int` | Yes | Must be greater than zero |
| `IsActive` | `bool` | Yes | Whether the employee is currently active (default: `true`) |

**Business rules enforced by the entity:**

- FirstName, LastName, and Email cannot be null, empty, or whitespace.
- All string fields are trimmed automatically.
- Salary cannot be negative.
- DepartmentId must be greater than zero (every employee must belong to a department).
- Personal details (name, email, phone) are updated through `UpdatePersonalDetails()`.
- Employment details (salary, department) are updated through `UpdateEmploymentDetails()`.
- The employee can be activated or deactivated through `Activate()` and `Deactivate()`.

### Why Business Rules Belong in the Domain Layer

Business rules live in the Domain entities — not in controllers, not in database configuration, not in validation attributes. This is because:

1. **Controllers can be bypassed.** A background job, a message handler, or a test can create an entity without going through an HTTP endpoint. If the validation only exists in the controller, those paths can create invalid data.

2. **Database constraints are a safety net, not the primary defense.** Database constraints (NOT NULL, CHECK) protect data integrity at rest, but they produce cryptic error messages and cannot express business concepts like "salary cannot be negative" in a domain-meaningful way.

3. **The Domain layer is always available.** No matter how the application is called — through an API, a CLI, a scheduled job, or a test — the Domain entities enforce their own rules. This makes the system reliable and the rules testable in isolation.

---

## Persistence Layer

Phase 3 adds the Infrastructure persistence layer, which connects the Domain entities to a SQL Server database through Entity Framework Core.

### ApplicationDbContext

The `ApplicationDbContext` (in `Infrastructure/Persistence/`) is the EF Core entry point. It:

- Exposes `DbSet<Employee>` and `DbSet<Department>` for querying and saving entities.
- Applies entity configurations from the `Configurations/` folder via `ApplyConfigurationsFromAssembly`.
- Receives its SQL Server connection string from the DI container, which reads it from `appsettings.json`.

### Entity Configuration Classes

Each entity has a dedicated configuration class that implements `IEntityTypeConfiguration<T>`:

- **`DepartmentConfiguration`** — maps `Department` to the `Departments` table. Defines column lengths (Name: 100, Description: 500), primary key, and identity generation.
- **`EmployeeConfiguration`** — maps `Employee` to the `Employees` table. Defines column lengths, `decimal(18,2)` for Salary, `datetime2` for HireDate, the foreign key to Department, `Restrict` delete behavior, and a unique index on Email.

Keeping configurations in separate files (instead of one large `OnModelCreating` method) makes each entity's mapping easy to find and modify.

### Connection String

The API reads the SQL Server connection string from `appsettings.json`:

```text
ConnectionStrings:DefaultConnection
```

The default value uses SQL Server LocalDB, which is suitable for local development. In production, the connection string should be overridden through environment variables or User Secrets — never by editing `appsettings.json` directly.

### Entity Materialization

The Domain entities use parameterized constructors and private property setters. EF Core can materialize these entities by:

1. Calling the constructor with database values that match parameter names (case-insensitive).
2. Setting remaining properties (Id, IsActive) via their private setters using reflection.

This means Domain validation runs during materialization. If the database contains invalid data, the application fails fast instead of silently loading corrupt entities.

### Migrations

EF Core migrations track schema changes over time. The initial migration (`InitialCreate`) creates the `Departments` and `Employees` tables with all configured columns, constraints, and indexes.

**Creating a migration** generates a C# file that describes the schema changes. It does not modify the database.

**Applying a migration** executes the schema changes against the database.

```bash
# Create a new migration (after changing entity configurations)
dotnet ef migrations add <MigrationName> \
  --project EmployeeManagement/src/EmployeeManagement.Infrastructure \
  --startup-project EmployeeManagement/src/EmployeeManagement.API \
  --output-dir Persistence/Migrations

# Apply pending migrations to the database
dotnet ef database update \
  --project EmployeeManagement/src/EmployeeManagement.Infrastructure \
  --startup-project EmployeeManagement/src/EmployeeManagement.API
```

The `dotnet-ef` tool must be installed. Verify with `dotnet tool list --global dotnet-ef`.

---

## Application and Repository Layer

Phase 4 adds the Application and Infrastructure components that implement CRUD use cases.

### Repository Interfaces

Repository interfaces (`IEmployeeRepository`, `IDepartmentRepository`) are defined in the Application layer under `Interfaces/Repositories/`. They define the data-access operations that Application services need — retrieve, add, update, delete, and existence checks — without specifying how they are implemented.

This separation means:
- The Application layer does not depend on EF Core or SQL Server.
- Unit tests can replace real repositories with mocks.
- The database technology can change without affecting Application code.

### Application Services

Application services (`EmployeeService`, `DepartmentService`) implement the use cases. They:
1. Validate application-level rules (department exists, email unique).
2. Create and modify Domain entities through their constructors and methods.
3. Delegate persistence to repositories.
4. Convert Domain entities to DTOs for the API layer.

Services use **DTOs** (Data Transfer Objects) for input and output. Request DTOs (`CreateEmployeeDto`, `UpdateEmployeeDto`, `CreateDepartmentDto`, `UpdateDepartmentDto`) carry the data needed to perform an operation. Response DTOs (`EmployeeResponseDto`, `DepartmentResponseDto`) shape the data returned to the API caller. Using separate DTOs prevents Domain entities from leaking into HTTP responses.

### Infrastructure Repositories

Repository implementations (`EmployeeRepository`, `DepartmentRepository`) live in Infrastructure and use EF Core to execute queries against SQL Server. They:
- Use `AsNoTracking()` for read-only queries (better performance).
- Retrieve tracked entities for updates (EF Core detects changes).
- Call `SaveChangesAsync` after each write operation.
- Use `AnyAsync` for efficient existence checks.

### Application-Level Error Handling

Two custom exception types represent application-level errors:
- **`NotFoundException`** — thrown when a requested entity does not exist (e.g., employee ID not found, department not found). A later phase maps this to HTTP 404.
- **`ConflictException`** — thrown when an operation conflicts with existing data (e.g., duplicate email, deleting a department with assigned employees). A later phase maps this to HTTP 409.

These exceptions do not reference HTTP types. They are pure Application-layer concepts.

### Email Uniqueness: Two Layers of Protection

Email uniqueness requires both an **application check** and a **database constraint**:

1. The application service calls `EmailExistsAsync` before saving. If the email is already in use, it throws `ConflictException` with a clear message.
2. The database has a unique index on `Email` (configured in Phase 3). If two requests race to create employees with the same email, the database rejects the second insert.

Neither layer alone is sufficient. The application check provides user-friendly errors. The database index prevents data corruption under concurrency.

### Dependency Injection

Services and repositories are registered through extension methods:
- `AddApplication()` (in the Application project) registers `IEmployeeService` and `IDepartmentService`.
- `AddInfrastructure()` (in the Infrastructure project) registers `IEmployeeRepository` and `IDepartmentRepository`.

The API's `Program.cs` calls both methods. All services use **scoped** lifetime (one instance per HTTP request), matching the DbContext's scoped lifetime.

---

## API Endpoints

Phase 4b adds RESTful HTTP controllers for Employee and Department CRUD operations.

### Employee Endpoints

| Method | Route | Purpose | Success | Error |
|---|---|---|---|---|
| GET | `/api/employee` | List all employees | 200 OK | — |
| GET | `/api/employee/{id}` | Get employee by ID | 200 OK | 404 |
| POST | `/api/employee` | Create employee | 201 Created | 400, 404, 409 |
| PUT | `/api/employee/{id}` | Update employee | 200 OK | 400, 404, 409 |
| DELETE | `/api/employee/{id}` | Delete employee | 204 No Content | 404 |

### Department Endpoints

| Method | Route | Purpose | Success | Error |
|---|---|---|---|---|
| GET | `/api/department` | List all departments | 200 OK | — |
| GET | `/api/department/{id}` | Get department by ID | 200 OK | 404 |
| POST | `/api/department` | Create department | 201 Created | 400 |
| PUT | `/api/department/{id}` | Update department | 200 OK | 400, 404 |
| DELETE | `/api/department/{id}` | Delete department | 204 No Content | 404, 409 |

### Error Responses

All errors return a JSON body with a `message` field:

```json
{ "message": "Description of what went wrong." }
```

| Status | Cause |
|---|---|
| 400 | Invalid input (failed Data Annotation validation or Domain business rule violation) |
| 404 | Requested entity not found, or referenced department does not exist |
| 409 | Data conflict (duplicate email, deleting a department with assigned employees) |
| 500 | Unexpected server error (safe message only, no internal details) |

### Exception Handling

A centralized `ExceptionHandlingMiddleware` maps Application exceptions to HTTP status codes:

- `NotFoundException` → 404
- `ConflictException` → 409
- `ArgumentException` → 400 (Domain validation failures)
- All other exceptions → 500 (generic safe message)

### Request Validation

Request DTOs use Data Annotations for API-level input validation:

- `[Required]` on mandatory string fields
- `[MaxLength]` matching database column lengths (Name: 100, Email: 255, etc.)
- `[EmailAddress]` on email fields
- `[Range(0, ...)]` on Salary (non-negative)
- `[Range(1, ...)]` on DepartmentId (must be > 0)

The `[ApiController]` attribute automatically returns 400 when validation fails.

### Running the API

```bash
dotnet run --project EmployeeManagement/src/EmployeeManagement.API
```

The API listens on HTTPS (port configured in `Properties/launchSettings.json`). Swagger UI is available at `/swagger` in the Development environment.

**Prerequisite:** The database must exist and the migration must be applied before the API can serve requests:

```bash
dotnet ef database update \
  --project EmployeeManagement/src/EmployeeManagement.Infrastructure \
  --startup-project EmployeeManagement/src/EmployeeManagement.API
```

---

## Authentication and Authorization

Phase 5 adds JWT Bearer authentication using ASP.NET Core Identity.

### Architecture

- **ASP.NET Core Identity** manages user accounts, password hashing (PBKDF2), lockout, and role membership. Passwords are never stored in plain text and are never handled manually.
- **JWT Bearer tokens** authenticate API requests. After login, the client receives a signed token that it includes in the `Authorization: Bearer <token>` header.
- **Role-based authorization** protects endpoints. Only users with the `Admin` role can access Employee and Department CRUD endpoints.

### Authentication Endpoints

| Method | Route | Purpose | Auth Required |
|---|---|---|---|
| POST | `/api/auth/register` | Create a new account | No (anonymous) |
| POST | `/api/auth/login` | Obtain a JWT token | No (anonymous) |

### Protected Endpoints

All Employee (`/api/employee`) and Department (`/api/department`) endpoints require a valid JWT with the `Admin` role. Requests without a token receive `401 Unauthorized`. Requests with a valid token but wrong role receive `403 Forbidden`.

### JWT Configuration

JWT settings are read from `appsettings.json` (non-secret defaults) and environment variables (secret signing key):

| Setting | Source | Description |
|---|---|---|
| `JwtSettings:Issuer` | `appsettings.json` | Token issuer (e.g., `EmployeeManagementApi`) |
| `JwtSettings:Audience` | `appsettings.json` | Token audience (e.g., `EmployeeManagementClient`) |
| `JwtSettings:ExpiryInMinutes` | `appsettings.json` | Token lifetime (default: 60 minutes) |
| `JwtSettings:SigningKey` | Environment variable or User Secrets | **Secret** — the HMAC-SHA256 signing key (min 32 characters) |

**Setting the signing key for development:**

```bash
# Option 1: User Secrets (recommended for local development)
dotnet user-secrets init --project EmployeeManagement/src/EmployeeManagement.API
dotnet user-secrets set "JwtSettings:SigningKey" "your-secret-key-at-least-32-characters-long" --project EmployeeManagement/src/EmployeeManagement.API

# Option 2: Environment variable
export ASPNETCORE_JWT_SIGNING_KEY="your-secret-key-at-least-32-characters-long"
```

**Never commit signing keys to source control.** The application fails to start if the signing key is missing or too short.

### Roles

| Role | Description |
|---|---|
| `Employee` | Default role assigned to all newly registered users |
| `Admin` | Full access to all CRUD endpoints. Must be assigned through a controlled process. |

Public registration **never** assigns the `Admin` role. This prevents privilege escalation.

### Role Initialization and Admin Provisioning

On startup, the application ensures the `Admin` and `Employee` roles exist in the database. This is idempotent — running it when roles already exist has no effect.

**Creating an admin in development:**

Option A — Bootstrap admin via environment variables (set before first startup):

```bash
export ADMIN_USERNAME="admin"
export ADMIN_EMAIL="admin@example.com"
export ADMIN_PASSWORD="YourStrongPassword123!"
dotnet run --project EmployeeManagement/src/EmployeeManagement.API
```

Option B — Manual database assignment after registration:

1. Register a normal account through `POST /api/auth/register`.
2. In SQL Server, insert a row into `AspNetUserRoles` linking the user to the Admin role (role ID can be found in `AspNetRoles`).

### Identity Migration

The Identity schema is added by the `AddIdentityTables` migration. This migration creates the standard ASP.NET Core Identity tables (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, etc.) alongside the existing `Employees` and `Departments` tables. It does not modify or drop existing application tables.

```bash
dotnet ef database update \
  --project EmployeeManagement/src/EmployeeManagement.Infrastructure \
  --startup-project EmployeeManagement/src/EmployeeManagement.API
```

### Using JWT in Swagger

1. Start the API and navigate to `/swagger`.
2. Click "Authorize" at the top of the Swagger UI page.
3. Obtain a token by calling `POST /api/auth/login` through Swagger.
4. Paste the token value (without the `Bearer ` prefix) into the Authorize dialog.
5. All subsequent requests will include the token in the `Authorization` header.

---

## Exception Handling, Logging, and CORS

### Centralized Exception Handling

`ExceptionHandlingMiddleware` maps Application-layer exceptions to safe HTTP responses:

| Exception | HTTP Status | When |
|---|---|---|
| `NotFoundException` | 404 | Requested entity does not exist |
| `ConflictException` | 409 | Data conflict (duplicate email, department has employees) |
| `ArgumentException` | 400 | Domain validation failure (invalid input that bypassed DTO validation) |
| Any other | 500 | Unexpected error — safe generic message only |

**Error response safety:** The middleware never returns stack traces, SQL error messages, connection strings, or internal exception details to the client. Full exception details are logged server-side at Error level for diagnostics.

**Duplicate email under concurrency:** The `EmployeeRepository` catches SQL Server `DbUpdateException` with error numbers 2601/2627 on the `IX_Employees_Email` unique index and translates it to `ConflictException`. This handles the race condition where two concurrent requests pass the application-level `EmailExistsAsync` check but the database rejects the second insert. This detection is SQL Server-specific — if the database provider changes, the detection logic must be updated.

### Structured Logging

All services use `ILogger<T>` with structured logging templates:

```text
// Good — structured: searchable and indexable
_logger.LogInformation("Employee created with ID {EmployeeId}, email {Email}", employee.Id, employee.Email);

// Bad — string interpolation: not searchable
_logger.LogInformation($"Employee created with ID {employee.Id}");
```

**Safe logging practices:**
- Passwords, JWTs, signing keys, and authorization headers are never logged.
- Exception objects are passed to `LogError` so that stack traces are preserved in server logs.
- Log levels: Information for success, Warning for expected failures (not found, conflict), Error for unexpected exceptions.

### Authentication Lockout

Login uses `SignInManager.CheckPasswordSignInAsync` which handles lockout automatically:

| Setting | Value | Effect |
|---|---|---|
| `MaxFailedAccessAttempts` | 5 | Locks account after 5 wrong passwords |
| `DefaultLockoutTimeSpan` | 15 minutes | Account unlocks automatically after 15 min |
| `AllowedForNewUsers` | true | Lockout applies to all new accounts |

On lockout, login returns the same generic error as invalid credentials. The API does not reveal whether an account is locked.

### CORS Configuration

CORS is configured through `appsettings.json`:

```json
{
  "CorsSettings": {
    "AllowedOrigins": [ "http://localhost:3000" ]
  }
}
```

- Only explicitly listed origins are allowed. Wildcard (`*`) is not used.
- Allowed methods: GET, POST, PUT, DELETE, OPTIONS.
- Allowed headers: Authorization, Content-Type.
- Credentials are allowed (required for JWT Bearer tokens from browser clients).
- The CORS middleware runs before authentication so that preflight OPTIONS requests succeed without a token.

To add a frontend origin, update the `AllowedOrigins` array. Do not add production origins until the frontend is deployed.

---

## Project Reference Direction

Clean Architecture enforces a strict dependency direction. Dependencies point **inward** — from outer layers toward the Domain.

```text
  API
  ├──→ Application
  └──→ Infrastructure
         ├──→ Application
         └──→ Domain
                (no outward references)

  Application
  └──→ Domain
```

**Rules:**

- **Domain** references nothing. It is the center.
- **Application** references only Domain.
- **Infrastructure** references Application and Domain.
- **API** references Application and Infrastructure.
- No project references a layer that depends on it (no circular dependencies).
- Domain never knows about EF Core, ASP.NET Core, or any framework.

---

## Intended Development Sequence

The application is built in phases. Each phase adds functionality while preserving the architecture.

| Phase | Focus |
|---|---|
| 1 | Solution structure and project references |
| 2 | Domain entities (Employee, Department) |
| 3 | EF Core persistence (DbContext, mappings, migrations) |
| 4 | Repositories and application services |
| 4b | API controllers and HTTP endpoints |
| 5 | JWT authentication, Identity, and role-based authorization |
| 6 | Exception handling, logging, configuration, and CORS |
| 7 | Unit tests and integration tests |
| 8 | Clean Code review and refactoring |

Each phase builds on the previous one. Do not skip phases.

---

## Testing

Phase 7 adds three test projects with 71 automated tests.

### Test Projects

| Project | Type | Tests | Purpose |
|---|---|---|---|
| `EmployeeManagement.Domain.Tests` | Unit | 39 | Domain entity business rules (Employee, Department) |
| `EmployeeManagement.Application.Tests` | Unit | 20 | Service use cases with mocked repositories |
| `EmployeeManagement.API.Tests` | Integration | 12 | HTTP endpoints, auth, authorization, middleware |

### Unit Tests vs Integration Tests

**Unit tests** (Domain.Tests, Application.Tests) verify individual components in isolation:
- Domain tests verify entity business rules without any database or framework dependency.
- Application tests verify service logic with mocked repositories (using Moq).
- Unit tests run in milliseconds and do not require a database.

**Integration tests** (API.Tests) verify the full HTTP pipeline:
- Use `WebApplicationFactory` to create an in-memory test server.
- Exercise real routing, model binding, authentication middleware, and authorization filters.
- Use SQLite in-memory as the test database (not SQL Server).
- Create real Identity users and generate real JWT tokens for authorization tests.

### Running Tests

```bash
# Run all tests
dotnet test EmployeeManagement/EmployeeManagement.slnx

# Run a specific test project
dotnet test EmployeeManagement/tests/EmployeeManagement.Domain.Tests
dotnet test EmployeeManagement/tests/EmployeeManagement.Application.Tests
dotnet test EmployeeManagement/tests/EmployeeManagement.API.Tests
```

### Test Database Strategy

Integration tests use **SQLite in-memory** instead of SQL Server. The `CustomWebApplicationFactory` replaces the SQL Server DbContext with a SQLite connection that is kept alive for the test lifetime and disposed after all tests complete.

**Limitations:**
- SQLite does not enforce `decimal(18,2)` precision (uses floating point).
- SQL Server-specific features (like the duplicate-key detection in `EmployeeRepository`) are not exercised by integration tests.
- The test database uses `EnsureCreated()` instead of migrations, so the schema is generated from the model directly.

Provider-independent tests (Domain, Application) work identically regardless of database. SQL Server-specific behavior (duplicate email race condition handling) is only verifiable against a real SQL Server instance.

### What Is Tested

- All Domain entity validation rules (constructor, updates, activate/deactivate)
- EmployeeService and DepartmentService use cases with mocked dependencies
- Registration assigns only the Employee role (not Admin)
- Invalid credentials return 401 without revealing whether the user exists
- Successful login returns a valid JWT with correct role claims
- Protected endpoints return 401 without a token
- Protected endpoints return 403 for non-Admin users
- Admin users can access protected endpoints
- Invalid model validation returns 400
- Not-found returns 404

### What Is Not Tested

- SQL Server-specific duplicate-key exception translation (requires live SQL Server)
- Account lockout after 5 failed attempts (possible but requires sequential login attempts)
- CORS preflight behavior
- Refresh tokens (not implemented)
- Concurrent request scenarios

---

## Prerequisites

Before building or running this solution, install:

- **.NET 8 SDK** (version 8.0.x or later). Verify with `dotnet --version`.
- **SQL Server** (Express, LocalDB, or Developer Edition). The connection string in `appsettings.json` uses LocalDB by default.
- **An IDE or editor** — Visual Studio 2022, JetBrains Rider, or VS Code with the C# Dev Kit extension.

---

## How to Build

From the `EmployeeManagement/` directory:

```bash
dotnet build
```

Expected result: `Build succeeded. 0 Warning(s) 0 Error(s)`.

To restore packages before building (if needed):

```bash
dotnet restore
dotnet build
```

---

## Current State

Phase 7 (Testing) is complete. The solution contains:

- Four production projects with correct Clean Architecture references.
- Two domain entities with full business rule enforcement.
- EF Core persistence with Identity tables alongside application tables.
- Repository implementations with SQL Server-specific duplicate-email detection.
- Application services with structured logging for all CRUD operations.
- `ExceptionHandlingMiddleware` with safe error responses.
- `SignInManager.CheckPasswordSignInAsync` for proper account lockout enforcement.
- JWT Bearer authentication with configurable signing key.
- Role-based authorization (Admin role required for CRUD endpoints).
- CORS configured with explicit allowed origins.
- Structured logging throughout.
- Data Annotation validation on all request DTOs.
- Swagger UI with Bearer token authorization support.
- **71 automated tests (39 unit + 20 unit + 12 integration), all passing.**

The solution does **not yet** include:

- Refresh tokens or password-reset workflows.
- Security headers (X-Content-Type-Options, etc.).

These features are added in subsequent phases.

**Note:** Integration tests use SQLite in-memory. Endpoints have not been tested against a live SQL Server database. SQL Server must be available, migrations must be applied, and a signing key must be configured for production runtime.
