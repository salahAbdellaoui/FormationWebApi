# 06 — Integration Testing

## What an Integration Test Verifies

An integration test verifies that multiple components work together correctly. While a unit test isolates a single component and replaces its dependencies with test doubles, an integration test uses real components and verifies that they communicate correctly.

In the EmployeeManagement application, an integration test might verify that:

- A POST request to `/api/employees` creates an employee in the database and returns a 201 status with the correct JSON body.
- A GET request to `/api/employees/1` returns the employee from the database with the correct JSON serialization.
- The dependency injection container resolves `IEmployeeService` to `EmployeeService` and `IEmployeeRepository` to `EmployeeRepository` correctly.
- Model binding converts the JSON request body to a `CreateEmployeeDto` correctly.
- The middleware pipeline (authentication, authorization, exception handling) works correctly for both valid and invalid requests.

---

## How Integration Testing Differs from Unit Testing

| Aspect | Unit Test | Integration Test |
|---|---|---|
| Scope | Single method or class | Multiple components working together |
| Dependencies | Replaced with mocks/stubs | Real or partially real |
| Speed | Milliseconds | Hundreds of milliseconds to seconds |
| Isolation | Fully isolated | Shared state (database, test server) |
| What it catches | Logic errors in individual components | Configuration errors, serialization errors, middleware issues, routing errors |

A unit test might verify that `EmployeeService.CreateEmployeeAsync` maps DTOs to entities correctly. An integration test verifies that a POST request to `/api/employees` goes through routing, model binding, authentication, authorization, the controller, the service, the repository, the database, and back through serialization to the HTTP response — all correctly.

---

## Why Individual Components Can Pass Unit Tests While Their Integration Still Fails

Consider these scenarios where unit tests pass but the application fails:

**Scenario 1: Missing dependency registration.** The `EmployeeService` works correctly in a unit test with a mock repository. But in production, `IEmployeeService` is not registered in the DI container. The application throws an exception when the controller tries to resolve it. A unit test cannot catch this because it constructs the service manually.

**Scenario 2: Incorrect routing.** The `EmployeesController` has the correct logic. But the route template is wrong: `[Route("api/employee")]` instead of `[Route("api/employees")]`. Unit tests do not test routing. An integration test sends a request to the actual URL and catches the error.

**Scenario 3: Serialization mismatch.** The `EmployeeDto` has a property `DepartmentName`. But the JSON serializer is configured to use camelCase, and the client expects PascalCase. The unit test asserts the DTO directly, so it passes. The integration test checks the HTTP response body and catches the mismatch.

**Scenario 4: Middleware ordering.** The authentication middleware is registered after the authorization middleware. The application allows unauthorized requests through. Unit tests do not test the middleware pipeline. An integration test sends a request without a token and catches the error.

These scenarios show why integration tests are necessary. They test the connections between components, not just the components themselves.

---

## How to Test ASP.NET Core Endpoints with WebApplicationFactory

`WebApplicationFactory<TEntryPoint>` creates an in-memory test server that hosts your ASP.NET Core application. It starts the application, configures dependency injection, sets up the middleware pipeline, and listens for HTTP requests — all without starting a real web server.

### Basic Setup

```csharp
using Microsoft.AspNetCore.Mvc.Testing;

public class EmployeesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EmployeesEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithEmployeeList()
    {
        // Act
        var response = await _client.GetAsync("/api/employees");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8",
            response.Content.Headers.ContentType?.ToString());
    }
}
```

The `WebApplicationFactory<Program>` starts your application using the `Program` class as the entry point. The `CreateClient()` method returns an `HttpClient` that sends requests to the test server. The requests go through the full middleware pipeline: routing, authentication, authorization, model binding, serialization, and your controller logic.

### The Program Class Must Be Accessible

For `WebApplicationFactory` to work, the `Program` class must be visible to the test project. In .NET 8 with minimal hosting, the `Program` class is internal by default. Add this to `Program.cs`:

```csharp
// At the bottom of Program.cs
public partial class Program { }
```

This makes the `Program` class accessible to the test project without changing its behavior.

---

## What the Integration Test Actually Tests

When you send a request through `WebApplicationFactory`, the test exercises:

1. **Routing**: The URL `/api/employees` is matched to the correct controller and action.
2. **Middleware**: Authentication, authorization, CORS, exception handling, and other middleware execute in order.
3. **Model binding**: The JSON request body is deserialized into the correct DTO.
4. **Dependency injection**: The controller receives its dependencies from the DI container.
5. **Controller logic**: The action method executes.
6. **Service logic**: The service processes the request.
7. **Repository logic**: The repository queries or modifies data.
8. **Database**: The actual database (or test database) stores or retrieves data.
9. **Serialization**: The response object is serialized to JSON.
10. **HTTP response**: The status code, headers, and body are returned to the client.

If any of these steps fails, the integration test catches it.

---

## Test Configuration vs. Production Configuration

The test server should not use the production database. It should use a test-specific database or an in-memory alternative.

### Replacing the Database for Tests

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EmployeeManagement.Infrastructure.Data;
using EmployeeManagement.Domain.Entities;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            
            if (descriptor != null)
                services.Remove(descriptor);

            // Add an in-memory database for testing
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });

            // Build the service provider and seed test data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            
            // Seed test data
            if (!db.Departments.Any())
            {
                db.Departments.Add(new Department { Id = 1, Name = "IT" });
                db.Employees.Add(new Employee
                {
                    Id = 1, Name = "John Doe", Email = "john@example.com",
                    Salary = 50000, DepartmentId = 1
                });
                db.SaveChanges();
            }
        });
    }
}
```

This custom factory:

1. Removes the real SQL Server database configuration.
2. Replaces it with an in-memory database.
3. Seeds test data so the tests have known data to work with.

The test project then uses `TestWebApplicationFactory` instead of `WebApplicationFactory<Program>`:

```csharp
public class EmployeesEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EmployeesEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
}
```

---

## Required Packages for Integration Testing

Add this package to the API test project:

```bash
dotnet add package Microsoft.AspNetCore.Mvc.Testing
```

This package provides `WebApplicationFactory<TEntryPoint>` and the test server infrastructure. It targets .NET 8 and is compatible with the existing EmployeeManagement API project.

For the in-memory database:

```bash
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

This provides the `UseInMemoryDatabase` extension method. Note that the InMemory provider has limitations — it does not enforce relational constraints like foreign keys, unique indexes, or transaction isolation. For tests that need these constraints, use a real SQL Server test database or SQL Server LocalDB.

---

## How to Test Specific Scenarios

### Testing a GET Endpoint

```csharp
[Fact]
public async Task GetById_WithExistingEmployee_ReturnsOkWithEmployee()
{
    // Act
    var response = await _client.GetAsync("/api/employees/1");

    // Assert
    response.EnsureSuccessStatusCode();
    
    var employee = await response.Content.ReadFromJsonAsync<EmployeeDto>();
    Assert.NotNull(employee);
    Assert.Equal(1, employee.Id);
    Assert.Equal("John Doe", employee.Name);
}
```

### Testing a POST Endpoint

```csharp
[Fact]
public async Task Create_WithValidData_ReturnsCreatedWithEmployee()
{
    // Arrange
    var dto = new CreateEmployeeDto
    {
        Name = "Jane Smith",
        Email = "jane@example.com",
        Salary = 60000,
        DepartmentId = 1
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/employees", dto);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    
    var created = await response.Content.ReadFromJsonAsync<EmployeeDto>();
    Assert.NotNull(created);
    Assert.Equal("Jane Smith", created.Name);
    Assert.Equal("jane@example.com", created.Email);
}
```

### Testing a 404 Response

```csharp
[Fact]
public async Task GetById_WithNonExistingId_ReturnsNotFound()
{
    // Act
    var response = await _client.GetAsync("/api/employees/999");

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
```

### Testing Authentication

If the API requires authentication (as described in Day 3–4), the integration test must provide a valid token:

```csharp
[Fact]
public async Task GetAll_WithoutToken_ReturnsUnauthorized()
{
    // Act
    var response = await _client.GetAsync("/api/employees");

    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}

[Fact]
public async Task GetAll_WithValidToken_ReturnsOk()
{
    // Arrange
    var token = GenerateTestToken("admin");
    _client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

    // Act
    var response = await _client.GetAsync("/api/employees");

    // Assert
    response.EnsureSuccessStatusCode();
}
```

Generating a test token requires access to the same signing key used by the authentication middleware. In the test configuration, you can replace the authentication configuration to use a known test key.

---

## How to Isolate and Clean Up Test Data

Integration tests that use a real database must manage test data carefully. If tests leave data behind, they affect other tests.

**Strategy 1: Use a fresh database for each test.**

```csharp
public class EmployeesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EmployeesEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        // Each test creates a new factory with a unique database name
        var uniqueFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
            });
        });
        var client = uniqueFactory.CreateClient();
        // ...
    }
}
```

**Strategy 2: Reset the database before each test.**

```csharp
public async Task InitializeAsync()
{
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    // Seed known data
}

public async Task DisposeAsync()
{
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureDeleted();
}
```

**Strategy 3: Use transactions that roll back after each test.**

This approach starts a transaction before each test and rolls it back after the test completes. The database returns to its original state without deleting data.

Choose the strategy that matches your test requirements. For simple tests, a fresh in-memory database is sufficient. For tests that need relational constraints, use a real SQL Server test database with careful data management.

---

## When to Use a Real SQL Server Test Database

The EF Core InMemory provider is useful for simple tests, but it does not behave exactly like SQL Server. Specifically, it does not enforce:

- Foreign key constraints.
- Unique indexes.
- Concurrent access behavior.
- Transaction isolation levels.
- Database-specific functions.

If your test verifies behavior that depends on these constraints — for example, testing that duplicate email addresses are rejected by a unique index — you need a real SQL Server test database.

Options for a SQL Server test database:

- **SQL Server LocalDB**: A lightweight version of SQL Server that runs on the developer's machine. No separate server required.
- **SQL Server in Docker**: A containerized SQL Server instance that starts and stops with the tests.
- **A dedicated test database on a shared server**: Useful in CI/CD environments.

For this training, the in-memory provider is sufficient for most examples. When SQL Server-specific behavior is needed, use LocalDB or Docker.

---

## Common Mistakes

**Mistake 1: Using the production database for tests.**

Integration tests modify data. If they use the production database, they corrupt production data. Always use a separate test database.

**Mistake 2: Not seeding test data.**

If the test database is empty, GET requests return empty results. The test might pass for the wrong reason. Always seed known data before running integration tests.

**Mistake 3: Tests that depend on execution order.**

If `Test1` creates an employee and `Test2` assumes the employee exists, the tests are coupled. If `Test2` runs before `Test1`, it fails. Each test should set up its own data or use a known seeded state.

**Mistake 4: Claiming the InMemory provider behaves exactly like SQL Server.**

It does not. If your test depends on SQL Server constraints, use a real SQL Server test database.

---

## Connection to Other Topics

Integration testing complements unit testing. Unit tests verify individual components in isolation. Integration tests verify that the components work together. Both are necessary.

Integration testing depends on the Clean Architecture structure you built in Days 1–2. The `WebApplicationFactory` starts the API project, which references the Infrastructure project, which configures dependency injection for the Application and Domain layers. The integration test exercises the complete chain.

Integration testing also depends on the security practices from Days 3–4. If the API uses JWT authentication, the integration test must provide valid tokens. If the API uses authorization policies, the integration test must verify that unauthorized requests are rejected.
