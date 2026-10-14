# 03 — Unit Testing

## What a Unit Test Is

A unit test verifies that a single piece of behavior works correctly in isolation. The "unit" is typically a single method, a class, or a small group of closely related methods.

A unit test:

- Runs quickly (milliseconds).
- Produces the same result every time.
- Does not depend on a database, network, file system, or other external systems.
- Does not depend on other tests.
- Tests one behavior at a time.

A unit test is not:

- A test that verifies multiple components working together (that is an integration test).
- A test that requires a database or external service (that is an integration test).
- A test that takes seconds to run (unit tests should run in milliseconds).

---

## What Constitutes a Unit in the EmployeeManagement Application

In the EmployeeManagement application, useful units to test include:

- **`EmployeeService` methods**: `CreateEmployeeAsync`, `GetEmployeeByIdAsync`, `GetAllEmployeesAsync`. These methods contain business logic that maps DTOs to entities, orchestrates repository calls, and makes decisions.

- **Domain entity behavior**: If you add validation or business rules to the `Employee` or `Department` entities, those rules are testable in isolation.

- **DTO validation**: If you add FluentValidation validators (as described in Day 3–4), each validator can be tested independently.

- **Mapping logic**: The `MapToDto` method in `EmployeeService` converts entities to DTOs. This logic is testable.

What is not a useful unit for unit testing:

- The `EmployeeRepository` (it depends on EF Core and a database).
- The `EmployeesController` (it depends on ASP.NET Core routing and model binding — test it with integration tests instead).
- The `AppDbContext` (it depends on a database connection).

The boundary is clear: test logic that makes decisions. Test infrastructure with integration tests.

---

## Why Unit Tests Should Run Quickly and Produce Repeatable Results

Unit tests run frequently. A developer runs them before every commit. A CI/CD pipeline runs them on every push. If unit tests take minutes to run, developers skip them or run them less often.

Unit tests must be repeatable. A test that passes today but fails tomorrow due to some external factor (time, database state, random numbers) is worse than no test at all. It creates noise. Developers start ignoring test failures. Real bugs hide among false positives.

Fast, repeatable tests build trust. When a unit test passes, the developer trusts that the behavior works. When it fails, the developer trusts that something is actually wrong.

---

## Which Logic Is Suitable for Unit Testing

Unit testing is most valuable for logic that makes decisions:

- **Business rules**: "An employee salary must be positive." "An employee cannot be assigned to a non-existent department." "An email must be unique."
- **Data transformation**: Mapping entities to DTOs, calculating derived values, formatting output.
- **Validation**: Checking input before it reaches the database.
- **Orchestration**: Coordinating multiple steps, handling errors, deciding which repository methods to call.

Logic that is not suitable for unit testing:

- **Database queries**: These depend on the database engine, connection, and data. Test them with integration tests.
- **HTTP serialization**: This depends on ASP.NET Core's serialization pipeline. Test it with integration tests.
- **Authentication and authorization**: These depend on middleware and security tokens. Test them with integration tests.

---

## Testing EmployeeService: A Practical Example

The `EmployeeService` in the EmployeeManagement application is the primary candidate for unit testing. It contains business logic and depends on `IEmployeeRepository`, which can be replaced with a test double.

Here is the current implementation of `EmployeeService`:

```csharp
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;

    public EmployeeService(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
    {
        var employees = await _repository.GetWithDepartmentAsync();
        return employees.Select(MapToDto);
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _repository.GetByIdWithDepartmentAsync(id);
        return employee == null ? null : MapToDto(employee);
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        var employee = new Employee
        {
            Name = dto.Name,
            Email = dto.Email,
            Salary = dto.Salary,
            DepartmentId = dto.DepartmentId
        };

        await _repository.AddAsync(employee);

        return new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Salary = employee.Salary,
            DepartmentId = employee.DepartmentId
        };
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Salary = employee.Salary,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name
        };
    }
}
```

### Testing GetAllEmployeesAsync

This method retrieves employees with their departments and maps them to DTOs. To test it, you need to provide a controlled set of employees from the repository and verify the mapping is correct.

```csharp
[Fact]
public async Task GetAllEmployeesAsync_ReturnsMappedEmployeesWithDepartmentNames()
{
    // Arrange
    var employees = new List<Employee>
    {
        new Employee
        {
            Id = 1, Name = "John Doe", Email = "john@example.com",
            Salary = 50000, DepartmentId = 1,
            Department = new Department { Id = 1, Name = "IT" }
        },
        new Employee
        {
            Id = 2, Name = "Jane Smith", Email = "jane@example.com",
            Salary = 60000, DepartmentId = 2,
            Department = new Department { Id = 2, Name = "HR" }
        }
    };

    var mockRepository = new Mock<IEmployeeRepository>();
    mockRepository
        .Setup(r => r.GetWithDepartmentAsync())
        .ReturnsAsync(employees);

    var service = new EmployeeService(mockRepository.Object);

    // Act
    var result = await service.GetAllEmployeesAsync();
    var resultList = result.ToList();

    // Assert
    Assert.Equal(2, resultList.Count);
    Assert.Equal("John Doe", resultList[0].Name);
    Assert.Equal("IT", resultList[0].DepartmentName);
    Assert.Equal("Jane Smith", resultList[1].Name);
    Assert.Equal("HR", resultList[1].DepartmentName);
}
```

This test:

1. **Arranges** a controlled set of employees with known department names.
2. **Acts** by calling `GetAllEmployeesAsync`.
3. **Asserts** that the DTOs have the correct names and department names.

The test does not require a database. It runs in milliseconds. It produces the same result every time.

### Testing GetEmployeeByIdAsync

This method has two paths: the employee exists, or the employee does not exist. You should test both.

```csharp
[Fact]
public async Task GetEmployeeByIdAsync_WithExistingEmployee_ReturnsMappedDto()
{
    // Arrange
    var employee = new Employee
    {
        Id = 1, Name = "John Doe", Email = "john@example.com",
        Salary = 50000, DepartmentId = 1,
        Department = new Department { Id = 1, Name = "IT" }
    };

    var mockRepository = new Mock<IEmployeeRepository>();
    mockRepository
        .Setup(r => r.GetByIdWithDepartmentAsync(1))
        .ReturnsAsync(employee);

    var service = new EmployeeService(mockRepository.Object);

    // Act
    var result = await service.GetEmployeeByIdAsync(1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result.Id);
    Assert.Equal("John Doe", result.Name);
    Assert.Equal("IT", result.DepartmentName);
}

[Fact]
public async Task GetEmployeeByIdAsync_WithNonExistingEmployee_ReturnsNull()
{
    // Arrange
    var mockRepository = new Mock<IEmployeeRepository>();
    mockRepository
        .Setup(r => r.GetByIdWithDepartmentAsync(999))
        .ReturnsAsync((Employee?)null);

    var service = new EmployeeService(mockRepository.Object);

    // Act
    var result = await service.GetEmployeeByIdAsync(999);

    // Assert
    Assert.Null(result);
}
```

The first test verifies the happy path. The second test verifies the null-return path. Together, they cover both branches of the `employee == null ? null : MapToDto(employee)` expression.

### Testing CreateEmployeeAsync

This method creates a new employee from a DTO, saves it via the repository, and returns a new DTO with the generated ID.

```csharp
[Fact]
public async Task CreateEmployeeAsync_WithValidData_ReturnsEmployeeWithGeneratedId()
{
    // Arrange
    var mockRepository = new Mock<IEmployeeRepository>();
    mockRepository
        .Setup(r => r.AddAsync(It.IsAny<Employee>()))
        .Callback<Employee>(e => e.Id = 42);

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
}
```

The `Callback` simulates the database assigning an ID. This is important because the current implementation returns a DTO that includes the `employee.Id`, which is set by the database after `SaveChangesAsync`. The test verifies that the returned DTO contains the correct ID.

---

## Testing the Mapping Logic

The `MapToDto` method is a private static method. You cannot call it directly in a test. However, you can test its behavior indirectly through the public methods that use it.

For example, the test for `GetEmployeeByIdAsync` already verifies that the DTO contains the correct values from the entity. If the mapping is wrong (e.g., the name is not copied), the test fails.

If the mapping logic becomes complex enough to warrant dedicated tests, you can extract it to a separate class:

```csharp
public static class EmployeeMapper
{
    public static EmployeeDto ToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Salary = employee.Salary,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name
        };
    }
}
```

Then test it directly:

```csharp
[Fact]
public void ToDto_WithEmployeeHavingDepartment_MapsAllFields()
{
    var employee = new Employee
    {
        Id = 1, Name = "John", Email = "john@example.com",
        Salary = 50000, DepartmentId = 1,
        Department = new Department { Id = 1, Name = "IT" }
    };

    var result = EmployeeMapper.ToDto(employee);

    Assert.Equal(1, result.Id);
    Assert.Equal("John", result.Name);
    Assert.Equal("IT", result.DepartmentName);
}
```

This is a teaching example. The current `EmployeeService` uses a private method for mapping. If you want to test the mapping directly, extracting it to a separate class is a refactoring you can make. The tests verify that the refactoring preserves behavior.

---

## How to Name Tests Clearly

A test name should describe the behavior being tested, the conditions, and the expected result. The common pattern is:

`MethodName_Scenario_ExpectedResult`

Examples:

- `GetEmployeeByIdAsync_WithExistingEmployee_ReturnsMappedDto`
- `GetEmployeeByIdAsync_WithNonExistingEmployee_ReturnsNull`
- `CreateEmployeeAsync_WithValidData_ReturnsEmployeeWithGeneratedId`
- `GetAllEmployeesAsync_WithEmptyDatabase_ReturnsEmptyList`

A good test name answers three questions:

1. **What is being tested?** The method name.
2. **Under what conditions?** The scenario.
3. **What should happen?** The expected result.

Avoid vague names like `Test1`, `TestCreate`, or `ShouldWork`. These names do not tell the reader what the test verifies.

---

## How to Avoid Excessive Setup and Brittle Assertions

**Excessive setup** occurs when a test requires many dependencies, many lines of arrangement, or complex data construction. This usually indicates that the class being tested has too many dependencies or responsibilities.

Solution: Reduce the dependencies of the class. If `EmployeeService` required five repositories and three external services, the test setup would be enormous. The solution is not to write a test helper that constructs all five repositories. The solution is to question whether the service should have five repositories.

**Brittle assertions** occur when a test asserts too many details, especially implementation details. If you assert every property of a returned object, any change to the object breaks the test, even if the behavior being tested has not changed.

Solution: Assert only the properties that are relevant to the behavior being tested. If you are testing that the service creates an employee and returns the correct ID, assert the ID. You do not need to assert every property.

---

## Connection to Other Topics

Unit testing depends on the Clean Architecture and Dependency Inversion you learned in Days 1–2. The `EmployeeService` depends on `IEmployeeRepository` (an interface), not on `EmployeeRepository` (a concrete class). This means you can replace the repository with a test double. If the service depended on a concrete repository, you could not test it without a database.

Unit testing also depends on mocking, which is the subject of the next topic. You will learn how to create test doubles, when to use them, and when real objects are better.
