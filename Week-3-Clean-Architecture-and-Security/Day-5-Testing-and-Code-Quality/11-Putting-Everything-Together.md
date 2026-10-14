# 11 — Putting Everything Together

## The Goal of This Topic

Topics 02–10 covered testing, mocking, SOLID principles, Clean Code, and refactoring as separate concepts. In practice, these practices are not separate. They work together in a cycle:

1. Good design makes code easier to test.
2. Tests provide confidence during refactoring.
3. Refactoring improves the design.
4. Improved design makes testing easier.

This topic demonstrates the cycle with one coherent example from the EmployeeManagement application. You will follow a piece of code from its initial state through design improvements, testing, and refactoring — and see how each practice supports the others.

---

## Starting Point: Code That Is Difficult to Test

Imagine that the `EmployeeService` has evolved over time. New requirements have been added without improving the design. The code now looks like this:

```csharp
public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;
    private readonly SmtpClient _smtpClient;

    public EmployeeService(AppDbContext context)
    {
        _context = context;
        _smtpClient = new SmtpClient("smtp.company.com");
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required");
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email is required");
        if (dto.Salary < 0)
            throw new ArgumentException("Salary must be positive");

        // Check duplicate email
        var existing = await _context.Employees
            .FirstOrDefaultAsync(e => e.Email == dto.Email);
        if (existing != null)
            throw new InvalidOperationException("Email already exists");

        // Create employee
        var employee = new Employee
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim().ToLower(),
            Salary = dto.Salary,
            DepartmentId = dto.DepartmentId
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Send notification
        await _smtpClient.SendMailAsync(
            "hr@company.com", dto.Email, "Welcome",
            $"Welcome {dto.Name} to the team!");

        return new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Salary = employee.Salary,
            DepartmentId = employee.DepartmentId
        };
    }
}
```

### Why This Code Is Difficult to Test

To unit test `CreateEmployeeAsync`, you need:

- A real `AppDbContext` connected to a database. The service depends on a concrete class, not an interface.
- A real SMTP server. The service creates its own `SmtpClient` in the constructor.
- A database with no existing employees (to test the happy path) or with a duplicate email (to test the duplicate check).

You cannot test this code in isolation. You cannot run it without a database and an SMTP server. You cannot test the validation logic without setting up a database.

The design problems that make testing difficult are:

1. **Violation of DIP**: The service depends on `AppDbContext` (a concrete class) instead of an interface.
2. **Violation of SRP**: The service handles validation, database access, and email notification.
3. **Hidden dependency**: The `SmtpClient` is created in the constructor, not injected.
4. **Mixed concerns**: Business logic, infrastructure, and notification are in the same method.

---

## Step 1: Apply Dependency Inversion

The first step is to make the dependencies replaceable. Replace `AppDbContext` with `IEmployeeRepository` and extract the email sending to an interface.

> **Note:** The improved code below calls `_repository.GetByEmailAsync(dto.Email)`. This method does not exist in the current `IEmployeeRepository` interface (which has `GetAllAsync`, `GetByIdAsync`, `GetByIdWithDepartmentAsync`, `GetWithDepartmentAsync`, and `AddAsync`). Adding a `GetByEmailAsync` method would be a necessary extension to support the duplicate email check. The examples in this topic show the target design, not the current state of the codebase.

```csharp
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IEmailSender _emailSender;

    public EmployeeService(IEmployeeRepository repository, IEmailSender emailSender)
    {
        _repository = repository;
        _emailSender = emailSender;
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required");
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email is required");
        if (dto.Salary < 0)
            throw new ArgumentException("Salary must be positive");

        // Check duplicate email
        var existing = await _repository.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new InvalidOperationException("Email already exists");

        // Create employee
        var employee = new Employee
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim().ToLower(),
            Salary = dto.Salary,
            DepartmentId = dto.DepartmentId
        };
        await _repository.AddAsync(employee);

        // Send notification
        await _emailSender.SendAsync(
            "hr@company.com", dto.Email, "Welcome",
            $"Welcome {dto.Name} to the team!");

        return new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Salary = employee.Salary,
            DepartmentId = employee.DepartmentId
        };
    }
}
```

### What Changed

- The service depends on `IEmployeeRepository` instead of `AppDbContext`.
- The email sender is injected as `IEmailSender` instead of being created internally.
- The repository interface is defined in the Application layer. The implementation is in Infrastructure.
- The DI container wires the dependencies.

### What This Enables

Now you can write a unit test:

```csharp
[Fact]
public async Task CreateEmployeeAsync_WithValidData_ReturnsEmployeeDto()
{
    // Arrange
    var mockRepository = new Mock<IEmployeeRepository>();
    mockRepository
        .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
        .ReturnsAsync((Employee?)null);
    mockRepository
        .Setup(r => r.AddAsync(It.IsAny<Employee>()))
        .Callback<Employee>(e => e.Id = 1);

    var mockEmailSender = new Mock<IEmailSender>();

    var service = new EmployeeService(mockRepository.Object, mockEmailSender.Object);
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
    Assert.Equal(1, result.Id);
    Assert.Equal("John Doe", result.Name);
    Assert.Equal("john@example.com", result.Email);
}
```

The test runs in milliseconds. It does not require a database or an SMTP server. The design change (DIP) made testing possible.

---

## Step 2: Apply Single Responsibility

The service still handles validation, creation, and email notification. Extract the validation and notification into separate classes.

```csharp
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;

    public EmployeeService(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        ValidateCreateDto(dto);
        await EnsureEmailIsUniqueAsync(dto.Email);

        var employee = new Employee
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim().ToLower(),
            Salary = dto.Salary,
            DepartmentId = dto.DepartmentId
        };
        await _repository.AddAsync(employee);

        return MapToDto(employee);
    }

    private static void ValidateCreateDto(CreateEmployeeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required");
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email is required");
        if (dto.Salary < 0)
            throw new ArgumentException("Salary must be positive");
    }

    private async Task EnsureEmailIsUniqueAsync(string email)
    {
        var existing = await _repository.GetByEmailAsync(email);
        if (existing != null)
            throw new InvalidOperationException("Email already exists");
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Salary = employee.Salary,
            DepartmentId = employee.DepartmentId
        };
    }
}
```

The email notification moves to a separate handler (an event handler or a decorator):

```csharp
public class EmployeeCreatedNotificationHandler
{
    private readonly IEmailSender _emailSender;

    public async Task HandleAsync(EmployeeCreatedEvent @event)
    {
        await _emailSender.SendAsync(
            "hr@company.com", @event.Email, "Welcome",
            $"Welcome {@event.Name} to the team!");
    }
}
```

### What This Enables

The service now has one responsibility: creating employees. The notification handler has one responsibility: sending emails. Each class is easier to understand, easier to test, and easier to change.

The unit test for `CreateEmployeeAsync` no longer needs to verify email sending. It focuses on the employee creation logic. The notification handler has its own test.

---

## Step 3: Add Tests

With the improved design, you can write focused unit tests:

```csharp
[Fact]
public async Task CreateEmployeeAsync_WithValidData_ReturnsEmployeeDto()
{
    var mockRepository = new Mock<IEmployeeRepository>();
    mockRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((Employee?)null);
    mockRepository.Setup(r => r.AddAsync(It.IsAny<Employee>())).Callback<Employee>(e => e.Id = 1);

    var service = new EmployeeService(mockRepository.Object);
    var dto = new CreateEmployeeDto { Name = "John Doe", Email = "john@example.com", Salary = 50000, DepartmentId = 1 };

    var result = await service.CreateEmployeeAsync(dto);

    Assert.Equal(1, result.Id);
    Assert.Equal("John Doe", result.Name);
}

[Fact]
public async Task CreateEmployeeAsync_WithDuplicateEmail_ThrowsException()
{
    var mockRepository = new Mock<IEmployeeRepository>();
    mockRepository.Setup(r => r.GetByEmailAsync("john@example.com")).ReturnsAsync(new Employee { Id = 1 });

    var service = new EmployeeService(mockRepository.Object);
    var dto = new CreateEmployeeDto { Name = "John", Email = "john@example.com", Salary = 50000, DepartmentId = 1 };

    await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateEmployeeAsync(dto));
}

[Fact]
public async Task CreateEmployeeAsync_WithNegativeSalary_ThrowsException()
{
    var mockRepository = new Mock<IEmployeeRepository>();
    var service = new EmployeeService(mockRepository.Object);
    var dto = new CreateEmployeeDto { Name = "John", Email = "john@example.com", Salary = -1000, DepartmentId = 1 };

    await Assert.ThrowsAsync<ArgumentException>(() => service.CreateEmployeeAsync(dto));
}
```

These tests verify the business logic in isolation. They run in milliseconds. They catch bugs before they reach production.

---

## Step 4: What an Integration Test Would Verify Separately

The unit tests verify the service logic. But they do not verify:

- The API endpoint routes correctly.
- The DI container resolves `IEmployeeService` to `EmployeeService`.
- The JSON serialization produces the correct response body.
- The authentication middleware rejects unauthorized requests.
- The database saves the employee correctly.

An integration test verifies these:

```csharp
[Fact]
public async Task CreateEmployee_WithValidData_Returns201WithEmployee()
{
    var client = _factory.CreateClient();
    var dto = new CreateEmployeeDto { Name = "John Doe", Email = "john@example.com", Salary = 50000, DepartmentId = 1 };

    var response = await client.PostAsJsonAsync("/api/employees", dto);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    
    var created = await response.Content.ReadFromJsonAsync<EmployeeDto>();
    Assert.NotNull(created);
    Assert.Equal("John Doe", created.Name);
}
```

The integration test exercises the complete request flow. The unit test verifies the business logic. Together, they cover both the components and the connections.

---

## Step 5: How the Tests Provide Confidence During Further Refactoring

Imagine a new requirement: employee names must be at least 2 characters long. You modify the validation:

```csharp
private static void ValidateCreateDto(CreateEmployeeDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length < 2)
        throw new ArgumentException("Name must be at least 2 characters");
    if (string.IsNullOrWhiteSpace(dto.Email))
        throw new ArgumentException("Email is required");
    if (dto.Salary < 0)
        throw new ArgumentException("Salary must be positive");
}
```

You run the tests. The test `CreateEmployeeAsync_WithNegativeSalary_ThrowsException` still passes. You add a new test:

```csharp
[Fact]
public async Task CreateEmployeeAsync_WithShortName_ThrowsException()
{
    var mockRepository = new Mock<IEmployeeRepository>();
    var service = new EmployeeService(mockRepository.Object);
    var dto = new CreateEmployeeDto { Name = "J", Email = "john@example.com", Salary = 50000, DepartmentId = 1 };

    await Assert.ThrowsAsync<ArgumentException>(() => service.CreateEmployeeAsync(dto));
}
```

The tests pass. You know the new requirement is implemented correctly and the existing requirements still work.

Without tests, you would modify the validation and hope nothing breaks. With tests, you know.

---

## The Cycle Summarized

```text
1. Start with code that is difficult to test.
         |
         v
2. Identify the design problem (DIP violation, SRP violation).
         |
         v
3. Apply SOLID principle (extract interfaces, separate responsibilities).
         |
         v
4. Write unit tests (now possible because of improved design).
         |
         v
5. Refactor with confidence (tests verify behavior is preserved).
         |
         v
6. Add integration tests (verify the components work together).
         |
         v
7. New requirements arrive. Tests catch regressions. Refactoring continues.
         |
         v
   (cycle repeats)
```

The key insight: **good design makes testing easier, tests provide confidence during refactoring, and refactoring improves the design**. These practices are not separate activities. They form a continuous cycle of improvement.

---

## What the Trainee Should Understand

After following this example, you should understand:

- **Good design makes code easier to test.** If you cannot unit test a class, the design is usually the problem, not the test.
- **Tests help detect regressions.** When you change code, tests verify that existing behavior is preserved.
- **Refactoring improves internal structure while preserving behavior.** Tests are the safety net that makes this safe.
- **Mocking is a tool, not a goal.** You mock dependencies to isolate the code under test. You do not mock everything just because you can.
- **SOLID principles guide design decisions.** They are not rules to apply mechanically. They help you recognize design problems and choose better alternatives.

These are not abstract concepts. They are practical practices that you apply every day as a professional developer.
