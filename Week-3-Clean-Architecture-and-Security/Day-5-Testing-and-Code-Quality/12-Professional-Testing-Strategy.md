# 12 — Professional Testing Strategy

## How a Professional Team Decides What to Test

You cannot test everything. A professional team does not try to write tests for every method, every property, and every edge case. Instead, the team focuses on what matters: the behaviors that, if broken, would cause real problems.

The testing strategy answers these questions:

- What should we test?
- How thoroughly should we test it?
- When should tests run?
- How do we maintain the tests over time?

---

## What to Test

### Business-Critical Logic

Test the logic that defines how the application works. If this logic is wrong, the application produces wrong results.

Examples from EmployeeManagement:

- Creating an employee: the name is required, the email must be valid and unique, the salary must be positive.
- Salary calculation: bonuses, deductions, tax calculations.
- Employee status changes: activating, deactivating, transferring between departments.
- Permission checks: only admins can delete employees, only managers can approve leave requests.

These are the behaviors that matter most. If they are wrong, users lose trust in the application.

### High-Risk Operations

Test operations where mistakes have serious consequences.

Examples:

- Deleting an employee: the wrong employee should not be deleted.
- Modifying salary: the salary should not be changed to an incorrect value.
- Authentication: unauthorized users should not access protected endpoints.
- Data import: importing from CSV should not corrupt existing data.

### Failure Scenarios

Test what happens when things go wrong.

Examples:

- Database connection fails: the application should return a meaningful error, not crash.
- Duplicate email: the application should reject the request with a clear message.
- Invalid input: the application should validate and reject, not save corrupted data.
- Expired token: the API should return 401 Unauthorized.
- Insufficient permissions: the API should return 403 Forbidden.

### API Contracts

Test that the API endpoints behave as documented.

Examples:

- POST `/api/employees` with valid data returns 201 Created with the employee in the response body.
- GET `/api/employees/{id}` with a non-existing ID returns 404 Not Found.
- GET `/api/employees` without authentication returns 401 Unauthorized.
- The response body is valid JSON with the expected fields.

### Database-Specific Behavior

Test behavior that depends on the database engine.

Examples:

- Unique constraints: duplicate emails should be rejected by the database.
- Foreign key constraints: an employee cannot be assigned to a non-existent department.
- Transaction behavior: if creating an employee fails, no partial data should be saved.

These require integration tests with a real database (or a test database that enforces constraints).

### Regression Tests for Fixed Bugs

When a bug is found and fixed, write a test that reproduces the bug. The test should fail before the fix and pass after. This prevents the bug from reappearing.

Example: A bug causes employees with the same email to be created when two requests arrive simultaneously. Fix the bug and add a test that verifies only one employee is created.

---

## Appropriate Unit and Integration Test Coverage

### Unit Tests Should Cover

- All business rules and validation logic.
- All service methods that make decisions.
- Data transformation and mapping logic.
- Edge cases and boundary conditions.
- Error handling and exception paths.

### Integration Tests Should Cover

- All API endpoints with valid and invalid inputs.
- Authentication and authorization flows.
- Middleware interactions (CORS, exception handling, logging).
- Database queries and constraints.
- Dependency injection configuration.

### What Not to Test

- Framework behavior (ASP.NET Core routing, EF Core query translation).
- Language features (C# property getters and setters).
- Trivial code (DTOs, simple mapping methods).
- Third-party libraries (System.Text.Json, FluentValidation).

---

## Test Reliability and Maintainability

### Test Reliability

A reliable test produces the same result every time it runs. Unreliable tests (flaky tests) destroy trust in the test suite.

Causes of flaky tests:

- **Time dependency**: Tests that use `DateTime.Now` might fail at midnight.
- **Random data**: Tests that generate random data might produce different results.
- **Shared state**: Tests that share a database might interfere with each other.
- **Network dependency**: Tests that call external services might fail if the service is down.
- **Order dependency**: Tests that depend on other tests running first.

Solutions:

- Use fixed time or inject a clock abstraction.
- Use deterministic test data.
- Isolate test data (fresh database per test or transaction rollback).
- Mock external services for unit tests.
- Make each test independent.

### Test Maintainability

Tests are code. They need to be maintained. Poorly written tests become expensive to update when the production code changes.

Principles for maintainable tests:

- **Test behavior, not implementation**. Assert the output, not the internal method calls.
- **Keep tests simple**. A test should be easy to read and understand.
- **Avoid duplication in tests**. Use helper methods or test data builders for common setup.
- **Name tests clearly**. The test name should describe what is being tested and what the expected result is.
- **Do not over-specify**. Assert only what matters for the behavior being tested.

---

## What Code Coverage Can and Cannot Tell Us

Code coverage measures the percentage of code that is executed by tests. It is a useful metric, but it has limitations.

### What Coverage Tells Us

- Which code is not tested at all. If a method has 0% coverage, it is definitely not tested.
- Areas of the codebase that might be risky. Low coverage in business-critical code is a warning sign.

### What Coverage Cannot Tell Us

- Whether the tests assert the correct behavior. A test that executes a method but asserts the wrong result still contributes to coverage.
- Whether edge cases are tested. A method might be covered by one test that only tests the happy path.
- Whether the tests are reliable. Flaky tests contribute to coverage but do not provide confidence.
- Whether the code is correct. 100% coverage does not mean 0% bugs.

### Professional Guidance

- Use coverage as a guide, not a goal. Aim for high coverage in business-critical code. Do not chase 100% overall coverage.
- Focus on meaningful tests. A test suite with 70% coverage and meaningful assertions is better than a test suite with 100% coverage and trivial assertions.
- Review uncovered code. If critical business logic has low coverage, add tests. If trivial code has low coverage, it might not need tests.

---

## When Tests Should Run Locally and in CI/CD

### Locally

Developers should run tests:

- **Before every commit**: Run the full test suite. If any test fails, fix it before committing.
- **During development**: Run relevant tests after each change. Do not wait until the end to discover that an early change broke something.
- **Before pushing**: Run the full test suite again to catch anything missed.

### In CI/CD

The CI/CD pipeline should run tests:

- **On every pull request**: The PR should not be merged if tests fail.
- **On every merge to the main branch**: Verify that the merged code passes all tests.
- **Before deployment**: Run the full test suite before deploying to production.

The CI/CD pipeline should also:

- Run tests in parallel to reduce execution time.
- Generate code coverage reports.
- Archive test results for debugging.
- Fail the build if tests fail.

### Test Speed

For the feedback loop to work, tests must run quickly:

- **Unit tests**: Seconds. Developers run these frequently.
- **Integration tests**: Minutes. Run these on every commit and PR.
- **End-to-end tests**: 10+ minutes. Run these on every PR and before deployment.

If the full test suite takes too long, split it into fast and slow suites. Run the fast suite locally and on every commit. Run the slow suite on every PR.

---

## A Realistic Testing Strategy for EmployeeManagement

Based on the application structure:

### Unit Tests (EmployeeManagement.Application.Tests)

- Test `EmployeeService` methods: creation, retrieval, mapping.
- Test validation logic: required fields, email format, salary range.
- Test error handling: duplicate email, non-existing department.
- Use mocks for `IEmployeeRepository`.

### Integration Tests (EmployeeManagement.Api.Tests)

- Test API endpoints: GET, POST, PUT, DELETE.
- Test authentication: valid token, expired token, missing token.
- Test authorization: admin access, user access, denied access.
- Test error responses: 400 Bad Request, 401 Unauthorized, 404 Not Found, 500 Internal Server Error.
- Use `WebApplicationFactory` with a test database.

### Infrastructure Tests (EmployeeManagement.Infrastructure.Tests)

- Test repository methods: add, retrieve, update, delete.
- Test database constraints: unique email, foreign key department.
- Use a real SQL Server test database (LocalDB or Docker).

### Domain Tests (EmployeeManagement.Domain.Tests)

- Test domain entity behavior: if you add business rules to `Employee` or `Department`.
- Currently, the domain entities are simple data containers. Tests would be minimal.

---

## Common Mistakes

**Mistake 1: Testing everything.**

Writing tests for every property and every method wastes time. Focus on behavior that could be wrong.

**Mistake 2: Ignoring test failures.**

A failing test is a signal. Do not delete the test or change the assertion to make it pass unless you understand why it failed and you intentionally changed the behavior.

**Mistake 3: Writing tests after the fact.**

If you write tests months after the code, you might test what the code does instead of what it should do. Write tests as close to the code as possible.

**Mistake 4: Treating coverage as a goal.**

100% coverage with trivial assertions is worse than 70% coverage with meaningful assertions. Focus on what the tests verify, not how many lines they execute.

**Mistake 5: Not running tests in CI/CD.**

Tests that only run on the developer's machine are not reliable. Developers might forget to run them. CI/CD ensures tests run on every change.

---

## Connection to Other Topics

The testing strategy connects all the topics in this module:

- **Testing fundamentals** (Topic 02): The strategy is based on the principles of automated testing.
- **Unit testing** (Topic 03): Unit tests cover business logic.
- **Mocking** (Topic 05): Mocking enables unit testing by replacing dependencies.
- **Integration testing** (Topic 06): Integration tests cover the API and database.
- **SOLID principles** (Topic 08): Good design makes testing possible and effective.
- **Clean Code** (Topic 09): Clean code is easier to test.
- **Refactoring** (Topic 10): Tests provide confidence during refactoring.

The strategy is not a fixed plan. It is a set of guidelines that you adapt to your application. As the application grows, the strategy evolves. New risks emerge. New tests are added. Old tests are maintained or removed.

The goal is not perfect testing. The goal is reliable software that continues to work correctly as it evolves.

---

## Final Thought

Testing, SOLID, Clean Code, and refactoring are not separate practices. They are parts of a single approach to building software that works reliably over time.

Good design makes code testable. Tests catch bugs and prevent regressions. Clean Code makes code understandable. Refactoring improves the design. And the cycle continues.

You do not need to master all of these practices at once. Start with the basics: write unit tests for your service methods, apply meaningful names, extract methods that are too long, and run your tests before every commit. As you gain experience, you will recognize design problems earlier, apply SOLID principles more naturally, and refactor with more confidence.

The EmployeeManagement application you built in Days 1–2 is your starting point. It has a solid Clean Architecture structure. It uses Dependency Inversion. It separates concerns. These design choices make it testable. Now you have the knowledge to write the tests, improve the code, and keep it maintainable as requirements change.

That is the goal of this module. Not perfection. Continuous improvement.
