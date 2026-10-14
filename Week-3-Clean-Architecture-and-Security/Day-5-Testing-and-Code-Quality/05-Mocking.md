# 05 — Mocking

## What a Mock Is

A mock is a test double that replaces a real dependency during a test. It simulates the behavior of the real dependency in a controlled way.

When `EmployeeService` calls `_repository.GetByIdWithDepartmentAsync(1)`, it normally calls the real `EmployeeRepository`, which queries the database. In a unit test, you replace the real repository with a mock that returns a predefined `Employee` object without touching the database.

A mock is not the real thing. It does not execute SQL queries. It does not enforce database constraints. It does not persist data. It returns what you tell it to return, and it records what was called.

---

## Why a Dependency Needs to Be Replaced During a Test

Consider `EmployeeService.CreateEmployeeAsync`. It calls `_repository.AddAsync(employee)`, which saves to the database. To test this method as a unit test:

- You do not want to start a database.
- You do not want to manage test data.
- You do not want the test to fail if the database is unavailable.
- You want the test to run in milliseconds.
- You want the test to produce the same result every time.

Replacing the repository with a mock solves all of these problems. The mock simulates the repository's behavior without any external dependencies.

---

## The Difference Between Real Implementation, Stub, Fake, and Mock

These terms describe different kinds of test doubles:

**Real implementation**: The actual production code. `EmployeeRepository` querying SQL Server.

**Stub**: A simplified implementation that returns predefined values. It does not record interactions. Example:

```csharp
public class StubEmployeeRepository : IEmployeeRepository
{
    private readonly List<Employee> _employees = new();

    public Task<IEnumerable<Employee>> GetAllAsync()
        => Task.FromResult<IEnumerable<Employee>>(_employees);

    public Task<Employee?> GetByIdAsync(int id)
        => Task.FromResult(_employees.FirstOrDefault(e => e.Id == id));

    public Task<Employee?> GetByIdWithDepartmentAsync(int id)
        => Task.FromResult(_employees.FirstOrDefault(e => e.Id == id));

    public Task<IEnumerable<Employee>> GetWithDepartmentAsync()
        => Task.FromResult<IEnumerable<Employee>>(_employees);

    public Task AddAsync(Employee employee)
    {
        employee.Id = _employees.Count + 1;
        _employees.Add(employee);
        return Task.CompletedTask;
    }
}
```

A stub provides canned answers. It is useful when you need a simple, predictable implementation.

**Fake**: A working implementation that is not suitable for production but is useful for testing. Example: EF Core's InMemory provider. It stores data in memory instead of a database. It works for basic tests but does not enforce SQL Server constraints like unique indexes or foreign keys.

**Mock**: A test double that records interactions and allows you to verify them. Example: A Moq mock that verifies `AddAsync` was called exactly once.

```csharp
mockRepository.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Once);
```

In practice, the term "mock" is often used loosely to mean any test double. The distinction matters when you need to verify interactions (mock) versus just provide data (stub).

---

## How Interfaces and Dependency Inversion Make Dependencies Replaceable

The `EmployeeService` depends on `IEmployeeRepository`, not on `EmployeeRepository`. This is Dependency Inversion, which you learned in Day 2.

```csharp
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;

    public EmployeeService(IEmployeeRepository repository)
    {
        _repository = repository;
    }
}
```

Because the service depends on an interface, you can provide any implementation of that interface:

- In production, the DI container provides `EmployeeRepository`.
- In a unit test, you provide a mock that implements `IEmployeeRepository`.

If the service depended on a concrete class:

```csharp
public EmployeeService()
{
    _repository = new EmployeeRepository(new AppDbContext(...));
}
```

You could not replace the repository in a test. The service creates its own dependency. This is why Dependency Inversion exists: it makes dependencies replaceable, which makes testing possible.

---

## How to Configure a Mock's Behavior with Moq

Moq is a popular mocking library for .NET. It creates mock objects from interfaces and allows you to configure their behavior.

### Basic Setup

```csharp
var mockRepository = new Mock<IEmployeeRepository>();

// Configure GetByIdWithDepartmentAsync to return a specific employee
mockRepository
    .Setup(r => r.GetByIdWithDepartmentAsync(1))
    .ReturnsAsync(new Employee { Id = 1, Name = "John" });

// Use the mock
IEmployeeRepository repository = mockRepository.Object;
var employee = await repository.GetByIdWithDepartmentAsync(1);
// employee.Name == "John"
```

### Setup with Any Argument

```csharp
// Return the same employee regardless of the ID
mockRepository
    .Setup(r => r.GetByIdWithDepartmentAsync(It.IsAny<int>()))
    .ReturnsAsync(new Employee { Id = 1, Name = "John" });
```

`It.IsAny<int>()` matches any integer argument.

### Setup with Callback

```csharp
// Simulate the database assigning an ID
mockRepository
    .Setup(r => r.AddAsync(It.IsAny<Employee>()))
    .Callback<Employee>(e => e.Id = 42)
    .Returns(Task.CompletedTask);
```

The `Callback` executes when `AddAsync` is called. It modifies the `Employee` object to simulate the database setting the ID.

### Setup That Throws

```csharp
// Simulate a database error
mockRepository
    .Setup(r => r.GetAllAsync())
    .ThrowsAsync(new InvalidOperationException("Database connection failed"));
```

This is useful for testing error handling.

### Verifying Interactions

```csharp
// Verify that AddAsync was called exactly once
mockRepository.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Once);

// Verify that GetByIdWithDepartmentAsync was never called
mockRepository.Verify(r => r.GetByIdWithDepartmentAsync(It.IsAny<int>()), Times.Never);
```

Verification checks that the code under test interacted with the dependency as expected.

---

## A Realistic Unit Test with Mocking

Here is a complete test that uses Moq to test `EmployeeService.CreateEmployeeAsync`:

```csharp
[Fact]
public async Task CreateEmployeeAsync_WithValidData_CallsRepositoryAndReturnsDto()
{
    // Arrange
    var mockRepository = new Mock<IEmployeeRepository>();
    
    // Configure the mock to assign an ID when AddAsync is called
    mockRepository
        .Setup(r => r.AddAsync(It.IsAny<Employee>()))
        .Callback<Employee>(e => e.Id = 42)
        .Returns(Task.CompletedTask);

    var service = new EmployeeService(mockRepository.Object);
    var dto = new CreateEmployeeDto
    {
        Name = "John Doe",
        Email = "john@example.com",
        Salary = 50000,
        DepartmentId = 1
    };

    // Act
    var result = await service.CreateEmployeeAsync(dto);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(42, result.Id);
    Assert.Equal("John Doe", result.Name);
    Assert.Equal("john@example.com", result.Email);
    Assert.Equal(50000, result.Salary);
    Assert.Equal(1, result.DepartmentId);

    // Verify the repository was called
    mockRepository.Verify(r => r.AddAsync(It.Is<Employee>(
        e => e.Name == "John Doe" && e.Email == "john@example.com")), Times.Once);
}
```

This test:

1. Creates a mock repository.
2. Configures the mock to assign ID 42 when `AddAsync` is called.
3. Creates the service with the mock repository.
4. Calls `CreateEmployeeAsync` with a DTO.
5. Asserts that the result has the expected values.
6. Verifies that the repository's `AddAsync` was called with the correct employee data.

The test runs in milliseconds. It does not require a database. It produces the same result every time.

---

## When Mocking Is Useful

Mocking is useful when:

**The real dependency is slow or unavailable.** A database, web service, or file system adds latency and requires setup. Mocking removes these dependencies.

**You need deterministic behavior.** A mock returns the same value every time. A real database might return different data depending on what other tests or processes have modified it.

**You need to test error conditions.** It is difficult to make a real database throw a specific exception. A mock can throw any exception on demand.

**You want to verify interactions.** You can verify that a method was called with specific arguments. This is useful for testing side effects.

**You want fast feedback.** Unit tests with mocks run in milliseconds. This encourages developers to run tests frequently.

---

## When Mocking Can Hide Defects or Make Tests Too Tightly Coupled

Mocking is harmful when:

**The mock does not match the real behavior.** If the mock returns data in a different format than the real repository, the test passes but the application fails in production. This is the biggest risk of mocking.

**You mock everything.** If you mock the service, the repository, the validator, the logger, and the email sender, you have no test that verifies these components work together. You need integration tests for that.

**The mock is too complex.** If configuring the mock requires 50 lines of setup, the test is fragile. Any change to the production code breaks the mock configuration. This indicates the production code has too many dependencies.

**You mock the thing you are trying to test.** Never mock the class under test. If you need to mock `EmployeeService` to test something else, the design is wrong.

**You verify implementation details instead of behavior.** Verifying that a specific method was called on a dependency couples the test to the implementation. If the implementation changes to call a different method, the test fails even though the behavior is the same.

---

## Why EF Core's DbContext Should Not Automatically Be Treated as a Mock of a Real Relational Database

EF Core's `DbContext` is not a simple wrapper around a database. It has:

- Change tracking.
- Lazy loading.
- Identity resolution (the same entity is returned for the same primary key within a context).
- Navigation property loading strategies.
- Transaction management.
- Concurrency control.

When you mock `DbContext`, you lose all of this behavior. The mock does not track changes. It does not resolve identities. It does not enforce constraints. A test that passes with a mocked `DbContext` might fail with a real database.

For unit testing, mock the repository interface (`IEmployeeRepository`), not the `DbContext`. The repository abstracts the data access. The mock simulates the repository.

For integration testing, use a real database or a test-specific database (e.g., SQL Server LocalDB, a Docker container, or the EF Core InMemory provider with awareness of its limitations).

The EF Core InMemory provider is not a perfect substitute for SQL Server. It does not enforce:

- Foreign key constraints.
- Unique indexes.
- Transaction isolation.
- Concurrent access behavior.

Use the InMemory provider for simple tests where these constraints do not matter. Use a real SQL Server test database for tests where these constraints are relevant.

---

## Preferring Simple Real Objects Over Mocks

When a real object is inexpensive, deterministic, and available, prefer it over a mock.

Example: Testing the `Employee` entity does not require mocking. The entity has no dependencies. Create a real `Employee` object and test its behavior directly.

```csharp
[Fact]
public void Employee_DefaultValues_AreCorrect()
{
    var employee = new Employee();
    
    Assert.Equal(string.Empty, employee.Name);
    Assert.Equal(string.Empty, employee.Email);
    Assert.Equal(0, employee.Salary);
}
```

No mock needed. The real object is simple and deterministic.

Example: Testing a DTO mapping function does not require mocking. Create real input objects and verify the output.

```csharp
[Fact]
public void MapToDto_WithEmployee_MapsCorrectly()
{
    var employee = new Employee
    {
        Id = 1, Name = "John", Email = "john@example.com",
        Salary = 50000, DepartmentId = 1,
        Department = new Department { Id = 1, Name = "IT" }
    };

    var dto = EmployeeMapper.ToDto(employee);

    Assert.Equal(1, dto.Id);
    Assert.Equal("John", dto.Name);
    Assert.Equal("IT", dto.DepartmentName);
}
```

No mock needed. Use mocks when the dependency is complex, slow, or non-deterministic. Use real objects when they are simple, fast, and deterministic.

---

## Common Mistakes

**Mistake 1: Mocking the class under test.**

```csharp
var mockService = new Mock<IEmployeeService>();
// ... configure mock
var controller = new EmployeesController(mockService.Object);
```

This is correct — you mock the service to test the controller. But:

```csharp
var mockService = new Mock<EmployeeService>(); // Mocking the concrete class
```

Never mock the class you are testing. Mock its dependencies.

**Mistake 2: Verifying too many interactions.**

```csharp
mockRepository.Verify(r => r.GetWithDepartmentAsync(), Times.Once);
mockRepository.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Never);
mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
// ... 10 more verifications
```

This couples the test to the implementation. If the service changes to call a different method, the test fails. Verify only the interactions that matter for the behavior being tested.

**Mistake 3: Setting up a mock and then not using it.**

```csharp
var mockRepository = new Mock<IEmployeeRepository>();
mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Employee>());
// ... but the test calls a different method that does not use GetAllAsync
```

The mock setup is wasted. It indicates the test is not testing what it intends to test.

---

## Connection to Other Topics

Mocking depends on Dependency Inversion, which you learned in Day 2. The `EmployeeService` depends on `IEmployeeRepository`, an interface. This allows you to provide a mock implementation in tests. Without Dependency Inversion, mocking would not be possible.

Mocking is essential for unit testing (Topic 03). Unit tests require isolation from external dependencies. Mocking provides that isolation.

Mocking is not needed for integration testing (Topic 06). Integration tests use real components. They verify that the components work together, not that individual components work in isolation.

In Topic 11, you will see how mocking, unit testing, and integration testing work together in a single coherent example.
