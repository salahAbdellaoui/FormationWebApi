# 07 — FluentValidation

**Duration**: 35 minutes

---

## What Is FluentValidation?

FluentValidation is a popular .NET library for building strongly-typed validation rules using a fluent interface. Instead of decorating your DTOs with attributes, you create separate validator classes that define what makes the input valid or invalid.

It is not the only way to validate input in ASP.NET Core. Data Annotations (the `[Required]`, `[MaxLength]`, `[EmailAddress]` attributes) also work. But FluentValidation offers a different set of trade-offs that make it a strong choice for anything beyond trivial validation.

---

## Why Use FluentValidation?

### Separation of Concerns

Data Annotations embed validation rules directly in the DTO class. The DTO becomes responsible for two things: defining the shape of the data and defining what makes that data valid. FluentValidation keeps these separate. The DTO stays a simple data carrier. The validator stays a dedicated rules engine.

### Testable

Validators are plain classes. You can instantiate one, feed it a DTO, and assert on the result. No HTTP pipeline, no controller, no mocking framework needed. Testing Data Annotations requires instantiating a `ValidationContext` or running through the model binding pipeline.

### Reusable

The same validator works in the API layer, in a background job, in a message handler, or in a console application. The validation rules live in one place and travel with the DTO wherever it goes.

### Readable

The fluent syntax reads like a specification:

```csharp
RuleFor(x => x.Email)
    .NotEmpty().WithMessage("Email is required")
    .EmailAddress().WithMessage("A valid email is required");
```

Compare this to Data Annotations:

```csharp
[Required(ErrorMessage = "Email is required")]
[EmailAddress(ErrorMessage = "A valid email is required")]
public string Email { get; set; } = string.Empty;
```

Both are readable, but FluentValidation scales better. Complex rules (conditional validation, cross-field validation, async validation) are natural in FluentValidation and awkward or impossible with attributes.

### Powerful

FluentValidation supports:

- Conditional validation (`When`, `Unless`)
- Cross-field validation (compare two properties)
- Collection validation (validate each item in a list)
- Custom validators (encapsulate reusable rules)
- Async validation (check database uniqueness)
- Rule sets (different rules for different scenarios)

---

## Validation vs Domain Invariants

Before writing validators, understand the distinction between input validation and domain invariants. They are different concerns and belong in different places.

### Input Validation

Input validation asks: "Is the format and structure of this data correct?"

- Is the name non-empty and no longer than 100 characters?
- Is the email in a valid format?
- Is the salary a positive number?
- Is the department ID greater than zero?

These are questions about the shape of the data. They can be answered without any knowledge of the business domain. FluentValidation handles these.

### Domain Invariants

Domain invariants ask: "Does this data make business sense?"

- Does this department exist in the database?
- Is this employee eligible for this salary range based on their role?
- Can this employee be transferred to this department?
- Has this employee already been terminated?

These are questions about business rules. They require knowledge of the domain and often require a database lookup. Domain invariants belong in the Domain or Application layer, not in a validator class.

### Where Each Lives

```text
API Layer
├── FluentValidation (input validation)
│   "Is the email format correct?"
│   "Is the name within the length limit?"
│
Application / Domain Layer
└── Domain invariants (business rules)
    "Does this department exist?"
    "Is this salary within the allowed range for this role?"
```

Do not duplicate. If FluentValidation checks that `DepartmentId > 0`, the domain layer should not repeat that check. If the domain layer checks that the department exists, FluentValidation should not try to duplicate that database lookup. Each layer validates what it knows.

---

## Installation

Install the core FluentValidation package and the ASP.NET Core integration package:

```bash
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
```

For ASP.NET Core integration with automatic model validation:

```bash
dotnet add package FluentValidation.AspNetCore
```

Add the packages to the API project:

```bash
dotnet add EmployeeManagement.Api/EmployeeManagement.Api.csproj package FluentValidation.AspNetCore
```

---

## Defining Validators

Create a validator for `CreateEmployeeDto`. The validator lives in the API project (validation is an API-layer concern).

Create `Validators/CreateEmployeeDtoValidator.cs`:

```csharp
using FluentValidation;
using EmployeeManagement.Application.Dtos;

namespace EmployeeManagement.Api.Validators;

public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email is required")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters");

        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than 0")
            .LessThanOrEqualTo(1000000).WithMessage("Salary must not exceed 1,000,000");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("A valid department is required");
    }
}
```

### Understanding the Rules

| Property | Rule | Meaning |
|---|---|---|
| `Name` | `NotEmpty()` | Cannot be null, empty, or whitespace |
| `Name` | `MaximumLength(100)` | Must be 100 characters or fewer |
| `Email` | `NotEmpty()` | Cannot be null, empty, or whitespace |
| `Email` | `EmailAddress()` | Must be a valid email format |
| `Email` | `MaximumLength(200)` | Must be 200 characters or fewer |
| `Salary` | `GreaterThan(0)` | Must be a positive number |
| `Salary` | `LessThanOrEqualTo(1000000)` | Must not exceed one million |
| `DepartmentId` | `GreaterThan(0)` | Must be a positive integer |

Each rule chains off `RuleFor(x => x.Property)`. Multiple rules can chain on the same property. `.WithMessage()` overrides the default error message for each rule.

---

## Registering Validators

In `Program.cs`, configure FluentValidation to discover validators automatically:

```csharp
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<Program>();
        fv.AutomaticValidationEnabled = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
```

`RegisterValidatorsFromAssemblyContaining<Program>()` scans the API assembly for any class that inherits from `AbstractValidator<T>` and registers it in the DI container. You do not need to register each validator manually.

`AutomaticValidationEnabled = true` tells ASP.NET Core to run the validator automatically before the controller action executes. If validation fails, the action never runs.

---

## How It Works

The validation flow integrates into the ASP.NET Core pipeline:

```text
    Client sends POST /api/employees
    Body: { "name": "", "email": "not-an-email", "salary": -500, "departmentId": 0 }
              |
              v
    [1] ASP.NET Core model binding creates CreateEmployeeDto from the JSON body
              |
              v
    [2] FluentValidation finds CreateEmployeeDtoValidator
              |
              v
    [3] Validator runs all rules against the DTO
        - Name is empty → FAIL
        - Email is not valid → FAIL
        - Salary is not > 0 → FAIL
        - DepartmentId is not > 0 → FAIL
              |
              v
    [4] Validation failed → ASP.NET Core short-circuits the pipeline
        Controller action does NOT execute
              |
              v
    [5] Response: 400 Bad Request with validation errors
```

Key point: the controller action never executes when validation fails. The service layer never runs. The database is never touched. Invalid data is rejected at the door.

---

## Validation Response Format

When validation fails, ASP.NET Core returns a 400 Bad Request with a Problem Details response that includes an `errors` object:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-abc123def456-789xyz-00",
  "errors": {
    "Name": [
      "Name is required"
    ],
    "Email": [
      "A valid email is required"
    ],
    "Salary": [
      "Salary must be greater than 0"
    ],
    "DepartmentId": [
      "A valid department is required"
    ]
  }
}
```

Each key in `errors` is a property name. Each value is an array of error messages (an array because multiple rules can fail for the same property).

This format is standardized. Clients can parse it programmatically. UI code can display the errors next to the corresponding form fields.

---

## Async Validators

Some validation rules require a database lookup. The most common example: checking if an email address is already in use.

```csharp
using FluentValidation;
using EmployeeManagement.Application.Dtos;
using EmployeeManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Validators;

public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
{
    private readonly AppDbContext _context;

    public CreateEmployeeDtoValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email is required")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters")
            .MustAsync(BeUniqueEmail).WithMessage("Email already exists");

        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than 0")
            .LessThanOrEqualTo(1000000).WithMessage("Salary must not exceed 1,000,000");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("A valid department is required");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return !await _context.Employees
            .AnyAsync(e => e.Email == email, cancellationToken);
    }
}
```

> **Important**: Async validators that access the database introduce a dependency on Infrastructure from the API layer. This violates Clean Architecture. For the training application, this is acceptable as a demonstration. In a production system, move the uniqueness check to the Application layer (as part of the service or a dedicated specification) and keep the API validator focused on format and structure.

> **Note on automatic validation**: `FluentValidation.AspNetCore` does not execute async validators during automatic model validation. If you need async validation, you must invoke the validator manually in the controller or use a filter. For this training, synchronous rules are sufficient.

---

## Testing Validators

One of FluentValidation's strengths is testability. Validators are plain classes. You test them without any HTTP infrastructure.

```csharp
using FluentValidation.TestHelper;
using EmployeeManagement.Application.Dtos;
using EmployeeManagement.Api.Validators;

namespace EmployeeManagement.Api.Tests.Validators;

public class CreateEmployeeDtoValidatorTests
{
    private readonly CreateEmployeeDtoValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var dto = new CreateEmployeeDto
        {
            Name = "",
            Email = "test@example.com",
            Salary = 50000,
            DepartmentId = 1
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_100_Characters()
    {
        var dto = new CreateEmployeeDto
        {
            Name = new string('A', 101),
            Email = "test@example.com",
            Salary = 50000,
            DepartmentId = 1
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 100 characters");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var dto = new CreateEmployeeDto
        {
            Name = "Alice",
            Email = "not-an-email",
            Salary = 50000,
            DepartmentId = 1
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Salary_Is_Zero_Or_Negative()
    {
        var dto = new CreateEmployeeDto
        {
            Name = "Alice",
            Email = "test@example.com",
            Salary = 0,
            DepartmentId = 1
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Salary);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_All_Fields_Are_Valid()
    {
        var dto = new CreateEmployeeDto
        {
            Name = "Alice",
            Email = "alice@example.com",
            Salary = 50000,
            DepartmentId = 1
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
```

`TestValidate` is a FluentValidation extension method that runs the validator and returns a result object with assertion methods. `ShouldHaveValidationErrorFor` checks that a specific property has at least one error. `ShouldNotHaveAnyValidationErrors` checks that the entire DTO is valid.

These tests run in milliseconds. No database, no HTTP server, no middleware. They give you confidence that your validation rules work correctly before any request reaches the controller.

---

## Validation in the Architecture

Understanding where validation belongs prevents duplication and confusion.

```text
┌─────────────────────────────────────────────────┐
│ API Layer                                        │
│                                                  │
│  FluentValidation                                │
│  - Name length, email format, salary range       │
│  - "Is the input structurally correct?"          │
│                                                  │
├─────────────────────────────────────────────────┤
│ Application Layer                                │
│                                                  │
│  Service / Use Case                              │
│  - Business rule checks (with repository calls)  │
│  - "Does this data satisfy business rules?"      │
│                                                  │
├─────────────────────────────────────────────────┤
│ Domain Layer                                     │
│                                                  │
│  Entity / Value Object                           │
│  - Invariants enforced in constructors           │
│  - "Can this entity exist in this state?"        │
│                                                  │
└─────────────────────────────────────────────────┘
```

### Rules of Thumb

1. **Format and structure** → FluentValidation (API layer)
2. **Business rules that need external data** → Application service
3. **Invariants that define entity integrity** → Domain entity

### Avoid Duplication

If FluentValidation checks `Salary > 0`, the domain entity should not repeat that check in its constructor. If the domain entity checks `Name` is not empty, FluentValidation does not need to repeat it (though it does not hurt to have both for early rejection).

Duplication is acceptable when:

- The API layer rejects early (saves a service call)
- The domain layer enforces invariants (protects against callers that skip validation)

Duplication is harmful when:

- The same complex rule exists in two places and they drift apart
- Different layers return different error messages for the same violation

---

## Common Mistakes

**Mistake 1: Duplicating validation across every layer.**
The same `NotEmpty` check appears in the validator, in the service, and in the entity constructor. Three places to maintain. If one changes and the others do not, you get inconsistent behavior. Each layer should validate what it owns: the validator checks format, the service checks business rules, the entity checks invariants.

**Mistake 2: Using async validators with automatic validation.**
`FluentValidation.AspNetCore` runs synchronous validation during model binding. Async validators (like database uniqueness checks) will not execute automatically. If you need async validation, invoke the validator manually in the controller action using `await validator.ValidateAsync(dto)` and check `result.IsValid` before proceeding.

**Mistake 3: Validating domain entities instead of DTOs.**
The validator should validate the incoming DTO, not the domain entity. The DTO represents external input. The entity represents internal state. They have different shapes and different rules. If you validate the entity, you are checking internal invariants at the API boundary, which is the wrong place and the wrong time.

**Mistake 4: Not testing validators.**
Validators contain logic. Logic has bugs. Without tests, you will not discover that `MaximumLength(100)` was accidentally written as `MinimumLength(100)` until a client reports it. Validator tests are trivial to write and run in milliseconds. Write them.

**Mistake 5: Putting business rules in the validator.**
FluentValidation is for input validation: format, length, range, pattern. It is not for business rules like "an employee cannot earn more than their manager" or "a terminated employee cannot receive a salary increase." Those rules belong in the Application or Domain layer where they have access to the data they need.

---

## Knowledge Check

**Question 1**: The current `CreateEmployeeDto` has no validation. A client sends `{ "name": "", "email": "xyz", "salary": -1000, "departmentId": 0 }`. Without FluentValidation, what happens? What happens after you add the validator?

**Question 2**: Why does the validator live in the API project instead of the Application or Domain project?

**Question 3**: You need to validate that an employee's email is unique before creating them. Should this check live in the FluentValidation validator or in the Application service? Explain your reasoning.

<details>
<summary>Answers</summary>

**Answer 1**: Without validation, the DTO reaches the controller, the controller calls the service, and the service attempts to create an entity with an empty name, an invalid email, a negative salary, and a department ID of 0. Depending on database constraints, this might throw a database exception (500 Internal Server Error), create invalid data, or fail silently. With the validator, the request is rejected at the pipeline level with a 400 Bad Request. The controller action never executes. The service never runs. The database is never touched. The client gets clear, field-level error messages.

**Answer 2**: The validator validates HTTP input. It is an API-layer concern. It depends on ASP.NET Core's model binding pipeline (through `FluentValidation.AspNetCore`). The Application and Domain layers should not depend on ASP.NET Core. If you need the same validation in a non-API context (a background job, a console app), you would use the Application layer for business rules and keep the FluentValidation validator in the API project.

**Answer 3**: The uniqueness check requires a database lookup. This is a business rule, not an input format check. It belongs in the Application service. The service can query the database through the repository and throw a domain exception or return a result indicating the email is taken. The API layer can then return a 409 Conflict. If you put this in FluentValidation, the validator needs access to the database, which couples the API layer to Infrastructure and violates Clean Architecture. For this training app, it is acceptable as a demonstration, but in production, the check belongs in the service.

</details>
