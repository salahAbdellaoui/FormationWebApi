# 03 — Domain Layer

## What Belongs in the Domain?

The Domain layer contains business concepts and business rules. It is the core of your application.

Think of it this way: if you removed the database, the web API, and the UI, the Domain would still exist. It defines what your application IS, not how it works technically.

The Domain layer has three rules:
- No frameworks (no Entity Framework, no ASP.NET)
- No HTTP concepts (no controllers, no DTOs, no request/response)
- No database concerns (no DbContext, no connection strings, no SQL)

It is pure C# that represents your business.

## Our Domain Entities

The `EmployeeManagement.Domain` project contains two entities: `Employee` and `Department`.

```csharp
namespace EmployeeManagement.Domain.Entities;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
}
```

```csharp
namespace EmployeeManagement.Domain.Entities;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
```

These entities represent business concepts. An Employee has a name, email, salary, and belongs to a Department. A Department has a name and contains Employees.

Notice what is missing: no database attributes, no validation attributes, no framework dependencies. Just the business concept.

## Business Rules vs. Input Validation

These are different concerns, and they live in different layers.

**Input validation** answers: "Is the data format correct?"
- Is the email format valid?
- Is the name not empty?
- Is the salary a number?

Input validation is an API concern. It happens in the presentation layer (controllers) before data reaches the Domain.

**Business rules** answer: "Does this data make business sense?"
- Can salary be negative? No.
- Can an employee exist without a department? Depends on your business.
- Can two employees have the same email? Depends on your business.

Business rules live in the Domain layer.

Example of a domain rule in the Employee entity:

```csharp
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public bool IsValid()
    {
        return Salary > 0 && !string.IsNullOrEmpty(Email);
    }
}
```

The `IsValid()` method enforces business rules: salary must be positive, email must exist. This is Domain logic, not HTTP validation.

## What Does NOT Belong in Domain

| Thing | Why It Does Not Belong |
|-------|------------------------|
| DbContext | Database concern, belongs in Infrastructure |
| Controllers | HTTP concern, belongs in API |
| DTOs | Presentation concern, belongs in Application or API |
| `[Required]`, `[EmailAddress]` | HTTP validation attributes, belong in API |
| `[Table]`, `[Column]` | EF Core attributes, belong in Infrastructure |
| HttpClient | External service concern, belongs in Infrastructure |
| ILogger | Technical concern, can be injected but not stored |
| Configuration classes | Infrastructure concern |

The Domain layer is pure business logic. If you see `using Microsoft.EntityFrameworkCore` or `using Microsoft.AspNetCore.Mvc` in a Domain file, something is wrong.

## Project Setup

The Domain project has zero external dependencies. It is pure C#.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

No Entity Framework. No ASP.NET. No third-party libraries. Just the .NET runtime.

This is intentional. The Domain should be the most stable layer in your application. External dependencies change. Business concepts change less often.

## Key Takeaway

The Domain layer has ZERO dependencies on outer layers. This is non-negotiable.

The Domain does not know about:
- The database (Infrastructure)
- The web API (Presentation)
- The application services (Application)

It only knows about itself. This makes it testable, stable, and portable.

If your Domain project references another project in your solution, you have a problem.

## Common Mistakes

**Mistake 1: Adding Entity Framework to Domain**
Developers add `Microsoft.EntityFrameworkCore` to the Domain project because they want to use data annotations like `[Table]` or `[MaxLength]`. This is wrong. These are database concerns. If you need database-specific configuration, use the Fluent API in the Infrastructure layer.

**Mistake 2: Putting Controllers in Domain**
Controllers handle HTTP requests and responses. They are presentation concerns. The Domain should not know about HTTP verbs, routes, or status codes.

**Mistake 3: Adding DataAnnotations for HTTP Validation**
Attributes like `[Required]` and `[EmailAddress]` are used by ASP.NET for model validation in controllers. They are not business rules. If you need validation in the Domain, write explicit methods like `IsValid()` that express business rules clearly.

**Mistake 4: Referencing DTOs in Domain Entities**
Domain entities should not know about DTOs. DTOs are for transferring data between layers. The Domain works with its own entities. If you find yourself importing a DTO into a Domain entity, the boundary is blurred.

## Knowledge Check

**Question 1:** Can the Domain layer reference the Application layer? Why or why not?

**Question 2:** Where should email format validation happen: in the Domain entity's `IsValid()` method, or in the API controller? Why?

**Question 3:** You need to add a `[MaxLength(100)]` attribute to the Employee.Name property. Which layer should contain this attribute, and why?
