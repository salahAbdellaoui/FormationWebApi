# 04 — Application Layer

## What Is the Application Layer?

The Application layer coordinates use cases. It defines what the application can do, without knowing how it is implemented.

Think of it as the orchestrator. The Application layer says: "I need to create an employee." It does not say: "I need to insert a row into the Employees table using Entity Framework."

The Application layer:
- Depends on the Domain layer (it uses Domain entities)
- Does NOT depend on Infrastructure (it does not know about databases)
- Does NOT depend on the API (it does not know about HTTP)

The Infrastructure layer implements what the Application layer needs. This is called Dependency Inversion.

## Components of the Application Layer

The Application layer has three main parts:

**Interfaces** define what the Application needs from the outside world. Example: `IEmployeeRepository` defines how to get and save employees, but does not say how.

**DTOs** (Data Transfer Objects) define the input and output of use cases. Example: `CreateEmployeeDto` is what the API sends to create an employee. `EmployeeDto` is what the Application returns.

**Services** implement the use cases. Example: `EmployeeService` coordinates the Domain entities and the repository interfaces to accomplish a task.

## The Repository Interface

The repository interface is defined in the Application layer:

```csharp
using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByIdWithDepartmentAsync(int id);
    Task<IEnumerable<Employee>> GetWithDepartmentAsync();
    Task AddAsync(Employee employee);
}
```

This is a key insight: the interface is defined in the Application layer, but it is implemented in the Infrastructure layer.

The Application layer says: "I need a way to get employees and save employees." The Infrastructure layer says: "I will use Entity Framework to do that."

This is Dependency Inversion. The inner layer (Application) defines the contract. The outer layer (Infrastructure) fulfills it. The Application does not depend on Entity Framework. Entity Framework depends on the Application's interface.

## The Application Service

The service implements the use case:

```csharp
using EmployeeManagement.Application.Dtos;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Services;

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

What does this service do?

1. Receives a DTO (`CreateEmployeeDto`) from the API
2. Creates a Domain entity (`Employee`) from the DTO
3. Calls the repository interface to save the entity
4. Returns a DTO (`EmployeeDto`) to the API

What does this service NOT do?

- It does not know about Entity Framework
- It does not know about SQL Server
- It does not know about HTTP requests or responses
- It does not know about JSON serialization

It only knows about Domain entities and repository interfaces. This makes it testable and independent of infrastructure concerns.

## Application vs. Domain

The boundary between Application and Domain is important.

**Application layer orchestrates.** It says: "Create an employee, save it, and return the result." It coordinates the flow of data and operations.

**Domain layer owns business concepts.** It says: "Here is what an Employee is. Here are its properties. Here are its rules."

Example:
- Application: "I need to create an employee with this name, email, salary, and department."
- Domain: "An Employee has a name, email, salary, and department. The salary must be positive."

The Application layer uses Domain entities, but it does not contain business logic. If you find business rules in the Application layer (like "if salary is less than 30000, reject it"), move that logic to the Domain layer.

## Why No MediatR or CQRS Here?

MediatR and CQRS are patterns for larger applications. They help manage complexity when you have many use cases, complex workflows, or need strict separation between commands and queries.

For this lesson and this application size, simple service classes are appropriate. The `EmployeeService` is easy to understand, easy to test, and does the job.

Adding MediatR here would be over-engineering. You would add complexity without benefit. If the application grows to 50+ use cases, or if you need advanced features like automatic retry, caching, or complex workflows, then consider MediatR.

Start simple. Add complexity only when you need it.

## Common Mistakes

**Mistake 1: Putting Entity Framework Queries in the Service**
Developers write `_context.Employees.Include(e => e.Department).ToListAsync()` inside the Application service. This is wrong. The Application layer should not know about Entity Framework. Use the repository interface instead. The repository implementation in Infrastructure will contain the EF Core queries.

**Mistake 2: Putting HTTP Concerns in the Service**
Developers add `HttpContext`, `ModelState`, or HTTP status codes to the Application service. This is wrong. The Application layer should not know about HTTP. It works with DTOs and Domain entities. HTTP concerns belong in the API layer.

**Mistake 3: Creating Interfaces for Everything**
Developers create `IEmployeeService` and `IDepartmentService` and inject them everywhere. This adds complexity without benefit. If you only have one implementation of a service, you do not need an interface. Create interfaces when you need to swap implementations (like for testing) or when multiple implementations exist.

**Mistake 4: Returning Domain Entities from the Service**
Developers return `Employee` (Domain entity) directly from the service to the API. This is wrong. Domain entities should not leak to the API layer. Use DTOs to transfer data. DTOs protect the Domain from external changes and allow you to shape the data for the client.

## Knowledge Check

**Question 1:** The `IEmployeeRepository` interface is defined in the Application layer but implemented in the Infrastructure layer. Why is this important? What principle does this follow?

**Question 2:** You need to add logging to the `CreateEmployeeAsync` method. Should you add `ILogger` to the Application service? Why or why not?

**Question 3:** The `CreateEmployeeAsync` method receives a `CreateEmployeeDto` and creates an `Employee` entity. Why not just pass the `Employee` entity directly from the API to the service?
