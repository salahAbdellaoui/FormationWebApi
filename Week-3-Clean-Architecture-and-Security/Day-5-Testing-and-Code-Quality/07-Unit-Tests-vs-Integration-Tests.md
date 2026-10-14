# 07 — Unit Tests vs. Integration Tests

## Comparison Table

| Aspect | Unit Tests | Integration Tests |
|---|---|---|
| **Main purpose** | Verify individual components work correctly in isolation | Verify multiple components work together correctly |
| **Scope** | Single method, class, or small group of methods | Endpoints, middleware, database, external services |
| **Dependencies** | Replaced with mocks or stubs | Real or partially real (test database, test server) |
| **Execution speed** | Milliseconds per test | Hundreds of milliseconds to seconds per test |
| **Isolation** | Fully isolated from other components | Shared state (database, test server, test data) |
| **Typical failures detected** | Logic errors, incorrect calculations, wrong data mapping | Configuration errors, routing errors, serialization errors, middleware issues, database constraint violations |
| **Maintenance cost** | Low — tests are simple and focused | Moderate — tests require setup, teardown, and data management |
| **Appropriate use cases** | Business logic, data transformation, validation rules, orchestration | API endpoints, authentication, authorization, database queries, middleware interactions |

---

## These Approaches Complement Each Other

Unit tests and integration tests are not alternatives. They are complementary.

**Unit tests catch logic errors quickly.** A developer writes a new business rule. Unit tests verify the rule works for all cases — valid input, invalid input, edge cases. The tests run in milliseconds. The developer gets immediate feedback.

**Integration tests catch wiring errors.** The developer deploys the new business rule. The API returns a 500 error because the new service is not registered in the DI container. The unit test did not catch this because it constructs the service manually. The integration test catches it because it uses the real DI container.

**Unit tests are fast and numerous.** You can have hundreds of unit tests that run in seconds. They cover many scenarios and edge cases.

**Integration tests are slower and fewer.** You might have dozens of integration tests that take minutes to run. They cover the most important scenarios but not every edge case.

Together, they provide a comprehensive safety net. Unit tests catch bugs in the logic. Integration tests catch bugs in the connections.

---

## The Test Pyramid

The test pyramid is a model that suggests how many tests of each type you should write:

```text
        /\
       /  \       End-to-end tests (fewest)
      / E2E\      Slow, expensive, comprehensive
     /------\
    /        \
   / Integra- \   Integration tests (moderate)
  /  tion      \  Moderate speed, verify component interactions
 /--------------\
/                \
/   Unit tests    \  Unit tests (most)
/   Fast, cheap,   \ Fast, isolated, verify individual behaviors
/------------------\
```

**Unit tests form the base.** They are the most numerous because they are the fastest and cheapest to write and run. They cover individual behaviors and edge cases.

**Integration tests form the middle.** They are fewer because they are slower and more expensive. They cover the most important interactions between components.

**End-to-end tests form the top.** They are the fewest because they are the slowest and most expensive. They simulate a real user interacting with the complete application.

The test pyramid is a useful design model, not an inflexible law. The appropriate balance depends on the application. An API with complex business logic might have many unit tests and fewer integration tests. An API with simple logic but complex middleware and authentication might have fewer unit tests and more integration tests.

---

## What to Test at Each Level

### Unit Tests Should Cover

- **Business rules**: Salary must be positive. Email must be unique. An employee cannot be assigned to a non-existent department.
- **Data transformation**: Mapping entities to DTOs, formatting dates, calculating derived values.
- **Validation logic**: Input validation rules, boundary conditions, format checks.
- **Orchestration**: Service methods that coordinate multiple steps, handle errors, and make decisions.
- **Edge cases**: Empty collections, null values, maximum values, concurrent operations (where testable in isolation).

### Integration Tests Should Cover

- **API contracts**: The endpoint URLs, HTTP methods, request/response formats, status codes.
- **Dependency injection**: Services, repositories, and other dependencies are registered and resolved correctly.
- **Middleware pipeline**: Authentication, authorization, CORS, exception handling execute in the correct order.
- **Database interactions**: Queries return correct data, constraints are enforced, transactions work correctly.
- **Serialization**: Request bodies are deserialized correctly, response bodies are serialized correctly.
- **Security**: Unauthorized requests are rejected, tokens are validated, roles and policies are enforced.

### What Not to Test

- **Framework behavior**: Do not test that ASP.NET Core routes requests correctly. The framework has its own tests.
- **Language features**: Do not test that C# properties get and set values. The compiler handles this.
- **Trivial code**: Do not test that a DTO property returns the value that was set.
- **Third-party libraries**: Do not test that `System.Text.Json` serializes objects correctly. Trust the library.

---

## A Practical Example from EmployeeManagement

### Unit Test

```csharp
[Fact]
public async Task GetEmployeeByIdAsync_WithExistingEmployee_ReturnsDtoWithDepartmentName()
{
    // Arrange
    var employee = new Employee
    {
        Id = 1, Name = "John", Email = "john@example.com",
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
    Assert.Equal("John", result.Name);
    Assert.Equal("IT", result.DepartmentName);
}
```

This unit test verifies that the service correctly maps an employee entity to a DTO, including the department name. It runs in milliseconds and does not require a database.

### Integration Test

```csharp
[Fact]
public async Task GetById_WithExistingEmployee_ReturnsOkWithJsonBody()
{
    // Arrange (test data is seeded in the test database)
    
    // Act
    var response = await _client.GetAsync("/api/employees/1");

    // Assert
    response.EnsureSuccessStatusCode();
    
    var employee = await response.Content.ReadFromJsonAsync<EmployeeDto>();
    Assert.NotNull(employee);
    Assert.Equal(1, employee.Id);
    Assert.Equal("John Doe", employee.Name);
    Assert.Equal("application/json; charset=utf-8",
        response.Content.Headers.ContentType?.ToString());
}
```

This integration test verifies that the complete request flow works: routing matches `/api/employees/1` to the `GetById` action, the DI container provides the service, the service queries the database, the result is serialized to JSON, and the response has the correct content type and status code.

The unit test catches mapping errors. The integration test catches routing errors, serialization errors, and DI configuration errors. They test different things. Both are necessary.

---

## When to Choose One Over the Other

**Choose a unit test when:**

- You want to verify a specific business rule or calculation.
- You want fast feedback during development.
- The logic can be tested in isolation.
- You want to test many edge cases quickly.

**Choose an integration test when:**

- You want to verify that an API endpoint works correctly.
- You want to verify that the DI container is configured correctly.
- You want to verify that the middleware pipeline works correctly.
- You want to verify that database queries return correct results.
- You want to verify that authentication and authorization work correctly.

**Choose both when:**

- The logic is complex and the connections are complex.
- You want comprehensive coverage of both behavior and wiring.

---

## Common Mistakes

**Mistake 1: Writing only unit tests.**

Unit tests verify individual components. They do not verify that the components work together. Without integration tests, you might deploy code where every component passes its unit tests but the API returns 500 errors because of a DI configuration issue.

**Mistake 2: Writing only integration tests.**

Integration tests are slow. If you try to cover every business rule with integration tests, your test suite takes minutes to run. Developers stop running tests. Bugs slip through.

**Mistake 3: Treating the test pyramid as a rigid rule.**

The pyramid is a guideline, not a law. Some applications need more integration tests. Some need more unit tests. The right balance depends on the application's complexity, risk areas, and team preferences.

**Mistake 4: Counting tests instead of evaluating their value.**

Having 1,000 tests is not better than having 100 tests if the 100 tests cover the critical behaviors. Focus on what the tests verify, not how many there are.

---

## Connection to Other Topics

The decision of what to test at each level connects to the professional testing strategy (Topic 12). You need to decide which business rules require unit tests, which endpoints require integration tests, and how to balance coverage with maintainability.

The relationship between unit tests and integration tests also connects to Dependency Inversion (Topic 08, section 8.5). Clean Architecture makes unit testing easier because dependencies are replaced with interfaces. Integration tests verify that the architecture is wired correctly.

In Topic 11, you will see how unit tests and integration tests work together in a single coherent example.
