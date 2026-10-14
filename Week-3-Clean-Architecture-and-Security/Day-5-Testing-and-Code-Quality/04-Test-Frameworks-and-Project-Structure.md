# 04 — Test Frameworks and Project Structure

## Choosing a Test Framework for .NET 8

The .NET ecosystem provides three main test frameworks:

| Framework | Strengths | When to Choose |
|---|---|---|
| **xUnit** | Modern, extensible, community standard for new .NET projects | Default choice for new .NET 8 projects |
| **NUnit** | Mature, flexible, familiar to Java developers (JUnit) | Existing NUnit codebase or team preference |
| **MSTest** | Microsoft-backed, integrates with Visual Studio | Strong Visual Studio integration needed |

For this training, we use **xUnit**. It is the most widely used framework in the modern .NET community, the default template in `dotnet new`, and the framework used in most open-source .NET projects. The concepts apply to all three frameworks, but the syntax differs slightly.

---

## The Role of Each Component

### The Test Project

A test project is a separate .NET project that contains test classes and methods. It references the production project it tests. Production projects never reference the test project.

The test project has:

- Its own `.csproj` file with `IsPackable=false` and test framework packages.
- References to the production projects whose code it tests.
- Test classes with test methods marked with `[Fact]` or `[Theory]` attributes.

### The Test Framework

The test framework provides:

- **Attributes** that mark methods as tests: `[Fact]` for a single test case, `[Theory]` for parameterized tests.
- **Assert class** with methods that verify expected outcomes: `Assert.Equal`, `Assert.NotNull`, `Assert.Throws`, etc.
- **Test lifecycle management**: setup and teardown methods if needed.

### Test Assertions

Assertions are the verification step in Arrange–Act–Assert. Common xUnit assertions:

```csharp
Assert.Equal(expected, actual);        // Values are equal
Assert.NotEqual(notExpected, actual);  // Values are not equal
Assert.Null(object);                   // Object is null
Assert.NotNull(object);                // Object is not null
Assert.True(condition);                // Condition is true
Assert.False(condition);               // Condition is false
Assert.Empty(collection);              // Collection is empty
Assert.Contains(item, collection);     // Collection contains item
Assert.Throws<ExceptionType>(() => action); // Action throws expected exception
```

### Test Runners and Test Discovery

Test runners find and execute tests. The `dotnet test` command:

1. Builds the solution.
2. Discovers test projects (projects that reference a test framework).
3. Finds test methods (methods with `[Fact]` or `[Theory]`).
4. Executes each test method.
5. Reports results: passed, failed, skipped.

### Test Naming and Organization

Organize test classes to mirror the production code structure:

```text
EmployeeManagement.Application.Tests/
    Services/
        EmployeeServiceTests.cs
    Dtos/
        CreateEmployeeDtoValidatorTests.cs

EmployeeManagement.Domain.Tests/
    Entities/
        EmployeeTests.cs
```

Each test class tests one production class. The test class name follows the pattern `{ClassName}Tests`.

---

## Proposed Test Project Structure for EmployeeManagement

The EmployeeManagement solution currently has four projects:

```text
EmployeeManagement.sln
├── EmployeeManagement.Domain/
├── EmployeeManagement.Application/
├── EmployeeManagement.Infrastructure/
└── EmployeeManagement.Api/
```

A professional test structure adds test projects that mirror the production projects:

```text
EmployeeManagement.sln
├── EmployeeManagement.Domain/
├── EmployeeManagement.Application/
├── EmployeeManagement.Infrastructure/
├── EmployeeManagement.Api/
├── EmployeeManagement.Domain.Tests/
├── EmployeeManagement.Application.Tests/
├── EmployeeManagement.Infrastructure.Tests/
└── EmployeeManagement.Api.Tests/
```

| Test Project | Tests | Dependencies |
|---|---|---|
| Domain.Tests | Domain entities, business rules | Domain only |
| Application.Tests | Services, DTOs, validators, mappers | Application, Domain |
| Infrastructure.Tests | Repositories with a real test database | Infrastructure, Application, Domain |
| Api.Tests | Controllers, middleware, endpoints with `WebApplicationFactory` | Api, Infrastructure, Application, Domain |

Each test project references only the production project it tests and any projects needed to construct test inputs.

---

## Project References

The reference direction is critical:

```text
Test projects → Production projects  (allowed)
Production projects → Test projects  (forbidden)
```

If a production project references a test project, the production code depends on test code. This creates a circular dependency and violates the principle that production code should not know about tests.

Example of correct references:

```xml
<!-- EmployeeManagement.Application.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="Moq" Version="4.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\EmployeeManagement.Application\EmployeeManagement.Application.csproj" />
  </ItemGroup>
</Project>
```

This test project references `EmployeeManagement.Application`. It does not reference `EmployeeManagement.Infrastructure` unless it needs to test infrastructure-related behavior.

---

## Required Packages for .NET 8

For a unit test project targeting .NET 8 with xUnit:

```bash
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Moq
```

| Package | Purpose |
|---|---|
| `Microsoft.NET.Test.Sdk` | Test platform that enables `dotnet test` |
| `xunit` | Test framework with `[Fact]`, `[Theory]`, and `Assert` |
| `xunit.runner.visualstudio` | Test adapter that discovers and runs xUnit tests |
| `Moq` | Mocking library for creating test doubles from interfaces |

For integration tests with `WebApplicationFactory`:

```bash
dotnet add package Microsoft.AspNetCore.Mvc.Testing
```

This package provides `WebApplicationFactory<TEntryPoint>`, which creates an in-memory test server that hosts your API for integration testing.

---

## Creating a Test Project (Proposed)

The following commands create the test project structure. This is documentation only — do not execute these commands during this lesson. They are shown so you understand the structure.

```bash
# From the solution directory
dotnet new xunit -n EmployeeManagement.Application.Tests
dotnet new xunit -n EmployeeManagement.Domain.Tests
dotnet sln add EmployeeManagement.Application.Tests
dotnet sln add EmployeeManagement.Domain.Tests

# Add project references
dotnet add EmployeeManagement.Application.Tests reference EmployeeManagement.Application
dotnet add EmployeeManagement.Application.Tests reference EmployeeManagement.Domain

# Add mocking library
dotnet add EmployeeManagement.Application.Tests package Moq
```

The `dotnet new xunit` command creates a test project with xUnit pre-configured. It includes `Microsoft.NET.Test.Sdk`, `xunit`, and `xunit.runner.visualstudio` automatically.

---

## The dotnet test Command

Running tests:

```bash
# Run all tests in the solution
dotnet test

# Run tests in a specific project
dotnet test EmployeeManagement.Application.Tests

# Run tests with verbose output
dotnet test --verbosity normal

# Run tests with a filter
dotnet test --filter "FullyQualifiedName~EmployeeService"

# Run tests and generate a code coverage report
dotnet test --collect:"XPlat Code Coverage"
```

The output shows:

```text
Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5
```

Each test method appears in the output. Failed tests show the assertion message and stack trace.

---

## Example: A Complete Test Class

Here is a complete test class for the `EmployeeService`:

```csharp
using EmployeeManagement.Application.Dtos;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Application.Services;
using EmployeeManagement.Domain.Entities;
using Moq;
using Xunit;

namespace EmployeeManagement.Application.Tests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _mockRepository;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _mockRepository = new Mock<IEmployeeRepository>();
        _service = new EmployeeService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllEmployeesAsync_ReturnsMappedEmployees()
    {
        // Arrange
        var employees = new List<Employee>
        {
            new Employee
            {
                Id = 1, Name = "John", Email = "john@example.com",
                Salary = 50000, DepartmentId = 1,
                Department = new Department { Id = 1, Name = "IT" }
            }
        };

        _mockRepository
            .Setup(r => r.GetWithDepartmentAsync())
            .ReturnsAsync(employees);

        // Act
        var result = await _service.GetAllEmployeesAsync();
        var resultList = result.ToList();

        // Assert
        Assert.Single(resultList);
        Assert.Equal("John", resultList[0].Name);
        Assert.Equal("IT", resultList[0].DepartmentName);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_WithExistingEmployee_ReturnsDto()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 1, Name = "John", Email = "john@example.com",
            Salary = 50000, DepartmentId = 1,
            Department = new Department { Id = 1, Name = "IT" }
        };

        _mockRepository
            .Setup(r => r.GetByIdWithDepartmentAsync(1))
            .ReturnsAsync(employee);

        // Act
        var result = await _service.GetEmployeeByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.Name);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_WithNonExistingEmployee_ReturnsNull()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdWithDepartmentAsync(999))
            .ReturnsAsync((Employee?)null);

        // Act
        var result = await _service.GetEmployeeByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithValidData_ReturnsEmployeeWithId()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Employee>()))
            .Callback<Employee>(e => e.Id = 42);

        var dto = new CreateEmployeeDto
        {
            Name = "John Doe",
            Email = "john@example.com",
            Salary = 50000,
            DepartmentId = 1
        };

        // Act
        var result = await _service.CreateEmployeeAsync(dto);

        // Assert
        Assert.Equal(42, result.Id);
        Assert.Equal("John Doe", result.Name);
    }
}
```

This test class:

- Uses the constructor to set up shared dependencies (the mock repository and the service).
- Each test method sets up its own specific mock behavior.
- Each test follows Arrange–Act–Assert.
- Test names describe the method, scenario, and expected result.
- Assertions verify observable behavior, not implementation details.

---

## Common Mistakes

**Mistake 1: Sharing mutable state between tests through the constructor.**

The constructor in the example creates a new mock and a new service for each test. xUnit creates a new instance of the test class for each test method, so each test gets fresh dependencies. This is correct.

But if you store mutable state in a field and modify it across tests, tests become order-dependent:

```csharp
private int _counter = 0; // Shared across tests in the same instance

[Fact]
public void Test1()
{
    _counter++; // Modifies shared state
    Assert.Equal(1, _counter);
}

[Fact]
public void Test2()
{
    _counter++; // Depends on Test1 not running first
    Assert.Equal(1, _counter); // Fails if Test1 ran first
}
```

Solution: Do not share mutable state between tests. Each test should be independent.

**Mistake 2: Forgetting `IsPackable=false` in the test project.**

Without this property, the test project can be packaged as a NuGet package. This is rarely intended and causes confusion.

**Mistake 3: Not adding the test project to the solution.**

After creating a test project, add it to the solution with `dotnet sln add`. Otherwise, `dotnet test` at the solution level does not discover it.

---

## Connection to Other Topics

The test project structure follows Clean Architecture principles. Each test project mirrors a production project. Tests for the Application layer do not depend on Infrastructure, and tests for the Domain do not depend on Application or Infrastructure.

Mocking (the next topic) is what makes the unit tests in `EmployeeServiceTests` possible. Without mocking, you would need a real database to test the service. With mocking, you replace the repository with a controlled test double.
