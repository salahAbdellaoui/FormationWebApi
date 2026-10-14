# 02 — Testing Fundamentals

## What a Test Is

A test is a piece of code that verifies a specific behavior of your application. It provides inputs, performs an action, and checks whether the result matches the expected outcome.

A test is not:

- A demonstration that the application works in one specific case.
- A replacement for thinking about edge cases and error conditions.
- Proof that the application is completely bug-free.

A test is:

- A repeatable check that a specific behavior works as expected.
- A safety net that catches regressions when you change code.
- A specification of what the code should do, expressed as executable code.

---

## What Automated Testing Provides

Automated testing provides three things that manual testing does not:

**Speed**. A test suite with hundreds of tests runs in seconds. A human cannot manually execute hundreds of test cases in seconds.

**Consistency**. A test executes the same steps in the same order every time. A human might skip a step, forget an edge case, or misread a result.

**Regression detection**. When you change code, you run the tests. If a test fails, you know the behavior changed. Without tests, you do not know if your change broke something else until a user reports a bug.

Automated testing does not replace manual testing entirely. Exploratory testing, usability testing, and visual inspection still require humans. But for verifying that specific behaviors work correctly, automated testing is faster, more reliable, and more thorough.

---

## The Arrange–Act–Assert Pattern

Every well-structured test follows the Arrange–Act–Assert (AAA) pattern:

**Arrange**: Set up the inputs, dependencies, and initial state.

**Act**: Perform the action being tested.

**Assert**: Verify that the result matches the expected outcome.

Example:

```csharp
[Fact]
public async Task CreateEmployeeAsync_WithValidData_ReturnsEmployeeWithGeneratedId()
{
    // Arrange
    var mockRepository = new Mock<IEmployeeRepository>();
    mockRepository
        .Setup(r => r.AddAsync(It.IsAny<Employee>()))
        .Callback<Employee>(e => e.Id = 1);
    
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
    Assert.Equal(1, result.Id);
    Assert.Equal("John Doe", result.Name);
    Assert.Equal("john@example.com", result.Email);
}
```

The test has three clear phases:

1. **Arrange**: Create a mock repository, configure it to set the employee ID when `AddAsync` is called, create the service with the mock repository, and create the input DTO.

2. **Act**: Call `CreateEmployeeAsync` with the DTO.

3. **Assert**: Verify that the result is not null, has the expected ID, name, and email.

Each phase is separated and clearly labeled. A developer reading the test can understand what it does without reading the implementation.

---

## Test Inputs, Actions, and Expected Outcomes

Every test has three components:

**Inputs**: The data and dependencies the test provides. In the example above, the inputs are the mock repository and the `CreateEmployeeDto`.

**Action**: The method or behavior being tested. In the example, the action is calling `CreateEmployeeAsync`.

**Expected outcome**: What should happen as a result of the action. In the example, the expected outcome is that the result has ID 1, name "John Doe", and email "john@example.com".

A good test makes all three components explicit. The reader should not have to guess what the inputs are, what action is being performed, or what the expected result is.

---

## Positive and Negative Test Cases

A positive test case verifies that the code works correctly with valid inputs. A negative test case verifies that the code handles invalid inputs or error conditions correctly.

Example of a positive test case:

```csharp
[Fact]
public async Task GetEmployeeByIdAsync_WithExistingId_ReturnsEmployee()
{
    // Arrange
    var mockRepository = new Mock<IEmployeeRepository>();
    mockRepository
        .Setup(r => r.GetByIdWithDepartmentAsync(1))
        .ReturnsAsync(new Employee { Id = 1, Name = "John Doe" });
    
    var service = new EmployeeService(mockRepository.Object);

    // Act
    var result = await service.GetEmployeeByIdAsync(1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result.Id);
    Assert.Equal("John Doe", result.Name);
}
```

Example of a negative test case:

```csharp
[Fact]
public async Task GetEmployeeByIdAsync_WithNonExistingId_ReturnsNull()
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

Both tests are important. The positive test verifies that the happy path works. The negative test verifies that the code handles the case where the employee does not exist.

Without the negative test, you might assume the code handles non-existing IDs correctly. But what if the implementation returns a default `Employee` object instead of null? What if it throws an exception? The negative test catches these problems.

---

## Deterministic and Repeatable Tests

A deterministic test produces the same result every time it runs, regardless of when or where it runs. A repeatable test can be executed multiple times with the same result.

Tests should be deterministic and repeatable. They should not depend on:

- The current time or date (unless explicitly provided as input).
- Random numbers (unless the random seed is controlled).
- External services or databases (unless the test is an integration test that explicitly controls the test data).
- The order in which tests run.
- The state left behind by previous tests.

Example of a non-deterministic test:

```csharp
[Fact]
public async Task CreateEmployeeAsync_GeneratesUniqueEmployeeCode()
{
    var mockRepository = new Mock<IEmployeeRepository>();
    mockRepository
        .Setup(r => r.AddAsync(It.IsAny<Employee>()))
        .Callback<Employee>(e => e.Id = 1);

    var service = new EmployeeService(mockRepository.Object);
    var dto = new CreateEmployeeDto { Name = "John", Email = "john@example.com", Salary = 50000, DepartmentId = 1 };

    var result = await service.CreateEmployeeAsync(dto);

    // This assertion depends on a random value — it might pass or fail unpredictably
    Assert.NotEqual("EMP-000", result.Name);
}
```

This test references a generated code that depends on random data. If the generation logic changes, or if the random seed produces a different value, the test fails unpredictably.

A better approach:

```csharp
[Fact]
public async Task CreateEmployeeAsync_ReturnsEmployeeWithCorrectName()
{
    var mockRepository = new Mock<IEmployeeRepository>();
    mockRepository
        .Setup(r => r.AddAsync(It.IsAny<Employee>()))
        .Callback<Employee>(e => e.Id = 1);

    var service = new EmployeeService(mockRepository.Object);
    var dto = new CreateEmployeeDto { Name = "John Doe", Email = "john@example.com", Salary = 50000, DepartmentId = 1 };

    var result = await service.CreateEmployeeAsync(dto);

    // Assert deterministic, observable behavior
    Assert.Equal("John Doe", result.Name);
    Assert.Equal(1, result.Id);
}
```

This test asserts values that are derived directly from the input. The result is predictable regardless of when or where the test runs.

---

## Test Isolation

A unit test should be isolated. It should not depend on other tests, and other tests should not depend on it. Each test should set up its own state and clean up after itself.

Example of a test that violates isolation:

```csharp
private Employee _sharedEmployee;

[Fact]
public async Task Test1()
{
    _sharedEmployee = new Employee { Name = "John" };
    await _repository.AddAsync(_sharedEmployee);
    // ... test logic
}

[Fact]
public async Task Test2()
{
    // Depends on Test1 having created the employee
    var employee = await _repository.GetByIdAsync(1);
    Assert.NotNull(employee);
}
```

If `Test1` does not run, or if it runs after `Test2`, then `Test2` fails. The tests are coupled.

A better approach:

```csharp
[Fact]
public async Task Test2_EmployeeDoesNotExist_ReturnsNull()
{
    // Arrange: Set up the repository with no employees
    var mockRepository = new Mock<IEmployeeRepository>();
    mockRepository
        .Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync((Employee?)null);
    
    var service = new EmployeeService(mockRepository.Object);

    // Act
    var result = await service.GetEmployeeByIdAsync(1);

    // Assert
    Assert.Null(result);
}
```

This test sets up its own state. It does not depend on other tests.

---

## Testing Behavior vs. Testing Implementation Details

A test should verify behavior, not implementation details. Behavior is what the code does from the outside. Implementation details are how the code does it internally.

Example of testing an implementation detail:

```csharp
[Fact]
public async Task CreateEmployeeAsync_CallsRepositoryAddAsync()
{
    var mockRepository = new Mock<IEmployeeRepository>();
    var service = new EmployeeService(mockRepository.Object);
    var dto = new CreateEmployeeDto { Name = "John", Email = "john@example.com", Salary = 50000, DepartmentId = 1 };
    
    await service.CreateEmployeeAsync(dto);
    
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Once);
}
```

This test verifies that the service called `AddAsync` on the repository. But what if the implementation changes to call `SaveAsync` instead? The test fails, even though the behavior (creating an employee) is the same.

A better approach:

```csharp
[Fact]
public async Task CreateEmployeeAsync_WithValidData_ReturnsEmployeeWithGeneratedId()
{
    var mockRepository = new Mock<IEmployeeRepository>();
    mockRepository
        .Setup(r => r.AddAsync(It.IsAny<Employee>()))
        .Callback<Employee>(e => e.Id = 1);
    
    var service = new EmployeeService(mockRepository.Object);
    var dto = new CreateEmployeeDto { Name = "John", Email = "john@example.com", Salary = 50000, DepartmentId = 1 };
    
    var result = await service.CreateEmployeeAsync(dto);
    
    Assert.Equal(1, result.Id);
    Assert.Equal("John", result.Name);
}
```

This test verifies the behavior: the returned employee has the expected ID and name. It does not care how the service achieved that result.

Testing behavior makes tests more resilient to implementation changes. Testing implementation details makes tests brittle and tightly coupled to the code they test.

---

## A Passing Test Does Not Prove the Application Is Defect-Free

A passing test proves that the specific behavior it tests works correctly in the specific case it tests. It does not prove that the application is completely bug-free.

Tests can miss bugs because:

- The test does not cover the edge case where the bug occurs.
- The test asserts the wrong expected outcome.
- The test does not cover the code path that contains the bug.
- The bug is in a component that is not tested.

This is why test coverage (the percentage of code executed by tests) is not a complete measure of test quality. You can have 100% coverage and still miss bugs if the tests do not assert the correct behavior.

Tests are a safety net, not a guarantee. They catch many bugs, but not all bugs. Combine automated testing with code review, manual testing, and good design practices for the best results.

---

## The Difference Between a Useful Test and a Test That Merely Repeats the Implementation

A useful test verifies behavior that could be wrong. A test that merely repeats the implementation verifies behavior that cannot be wrong.

Example of a test that repeats the implementation:

```csharp
[Fact]
public void Employee_Name_ReturnsName()
{
    var employee = new Employee { Name = "John" };
    Assert.Equal("John", employee.Name);
}
```

This test verifies that the `Name` property returns the value that was set. But this behavior is provided by the C# compiler. It cannot be wrong (unless the compiler has a bug). The test adds no value.

A useful test:

```csharp
[Fact]
public async Task GetEmployeeByIdAsync_WithExistingEmployee_MapsDepartmentName()
{
    var mockRepository = new Mock<IEmployeeRepository>();
    mockRepository
        .Setup(r => r.GetByIdWithDepartmentAsync(1))
        .ReturnsAsync(new Employee
        {
            Id = 1, Name = "John", Email = "john@example.com",
            Salary = 50000, DepartmentId = 1,
            Department = new Department { Id = 1, Name = "IT" }
        });

    var service = new EmployeeService(mockRepository.Object);

    var result = await service.GetEmployeeByIdAsync(1);

    // Verifies the mapping logic — if DepartmentName is not copied, this catches it
    Assert.NotNull(result);
    Assert.Equal("IT", result.DepartmentName);
}
```

This test verifies the mapping behavior: the service must copy the department name from the entity's navigation property to the DTO. If a developer changes the mapping and forgets to include the department name, this test fails. The behavior could be wrong. The test has value.

Write tests that verify behavior that could be wrong. Do not write tests that verify trivial behavior provided by the language or framework.

---

## Common Mistakes

**Mistake 1: Testing multiple behaviors in one test.**

```csharp
[Fact]
public async Task EmployeeService_CompleteTest()
{
    // Creates an employee, retrieves it, updates it, deletes it
    // ... 50 lines of test code
}
```

If this test fails, you do not know which behavior is broken. Break it into separate tests, each testing one behavior.

**Mistake 2: Asserting too many things in one test.**

```csharp
[Fact]
public async Task CreateEmployeeAsync_ReturnsCorrectResult()
{
    var result = await service.CreateEmployeeAsync(dto);
    
    Assert.Equal(1, result.Id);
    Assert.Equal("John", result.Name);
    Assert.Equal("john@example.com", result.Email);
    Assert.Equal(50000, result.Salary);
    Assert.Equal(1, result.DepartmentId);
    Assert.Equal("IT", result.DepartmentName);
    // ... 10 more assertions
}
```

If one assertion fails, the test stops and the remaining assertions do not run. You do not know if they would have passed. Focus each test on one or two critical assertions.

**Mistake 3: Ignoring test failures.**

A failing test means the behavior changed. Do not delete the test or change the assertion to make it pass unless you understand why it failed and you intentionally changed the behavior.

---

## Connection to Other Topics

Testing fundamentals apply to all types of tests: unit tests, integration tests, end-to-end tests. The Arrange–Act–Assert pattern, test isolation, deterministic behavior, and testing behavior over implementation details are universal principles.

In the next topic, you will apply these fundamentals to unit testing: testing individual components in isolation. You will learn what constitutes a "unit," how to test business logic without a database, and how to write tests that run quickly and produce repeatable results.
