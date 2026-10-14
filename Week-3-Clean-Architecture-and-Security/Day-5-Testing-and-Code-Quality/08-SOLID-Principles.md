# 08 — SOLID Principles

## Why SOLID Principles Matter

SOLID is an acronym for five design principles that guide you toward code that is easier to understand, test, and maintain. They are not rules to apply mechanically. They are guidelines that help you recognize design problems and choose better alternatives.

Each principle addresses a specific problem:

| Principle | Problem It Solves |
|---|---|
| SRP — Single Responsibility | Classes that do too many things become impossible to change safely |
| OCP — Open/Closed | Code that requires modification for every change becomes fragile |
| LSP — Liskov Substitution | Subtypes that break caller expectations cause unexpected failures |
| ISP — Interface Segregation | Large interfaces force implementations to depend on methods they do not need |
| DIP — Dependency Inversion | High-level logic coupled to low-level details becomes impossible to test or change |

These principles are connected. Code that follows SRP is easier to make follow OCP. Code that follows DIP is easier to test. Code that follows all five is easier to maintain.

In this topic, each principle is explained with: the problem, a problematic example, an improved example, why the improvement is better, and when applying the principle too far becomes counterproductive.

---

## 8.1 Single Responsibility Principle (SRP)

### The Principle

A class should have one, and only one, reason to change.

"Responsibility" means a reason to change. If a class handles both business logic and email notifications, it has two reasons to change: a business rule change and a notification change. These are independent concerns that should not be in the same class.

### The Problem

Imagine extending the `EmployeeService` to handle multiple concerns:

```csharp
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly SmtpClient _smtpClient;
    private readonly ILogger _logger;
    private readonly IExcelExporter _excelExporter;

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required");
        if (dto.Salary < 0)
            throw new ArgumentException("Salary must be positive");

        // Check duplicate email
        var existing = await _repository.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new InvalidOperationException("Email already exists");

        // Create employee
        var employee = new Employee
        {
            Name = dto.Name,
            Email = dto.Email,
            Salary = dto.Salary,
            DepartmentId = dto.DepartmentId
        };
        await _repository.AddAsync(employee);

        // Send notification email
        await _smtpClient.SendMailAsync(
            "hr@company.com",
            dto.Email,
            "Welcome",
            $"Welcome {dto.Name}!");

        // Log the action
        _logger.LogInformation("Employee {Name} created with ID {Id}",
            dto.Name, employee.Id);

        // Export to Excel if requested
        if (dto.ExportToExcel)
        {
            var file = _excelExporter.Export(employee);
            await File.WriteAllBytesAsync($"employee_{employee.Id}.xlsx", file);
        }

        return MapToDto(employee);
    }
}
```

This class has four responsibilities:

1. Creating employees (business logic).
2. Sending email notifications (infrastructure concern).
3. Logging (cross-cutting concern).
4. Exporting to Excel (presentation concern).

Each responsibility is a reason to change. If the email format changes, you modify this class. If the Excel export changes, you modify this class. If the business rule changes, you modify this class. Changes in one area risk breaking other areas.

### The Improved Version

Separate each responsibility into its own class:

```csharp
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IEmployeeValidator _validator;

    public EmployeeService(
        IEmployeeRepository repository,
        IEmployeeValidator validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        _validator.Validate(dto);

        var employee = new Employee
        {
            Name = dto.Name,
            Email = dto.Email,
            Salary = dto.Salary,
            DepartmentId = dto.DepartmentId
        };

        await _repository.AddAsync(employee);

        return MapToDto(employee);
    }

    private static EmployeeDto MapToDto(Employee employee) { /* ... */ }
}
```

The other responsibilities move to their own classes:

```csharp
public class EmployeeCreatedEventHandler : INotificationHandler<EmployeeCreatedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmployeeCreatedEventHandler> _logger;

    public async Task Handle(EmployeeCreatedEvent notification, CancellationToken ct)
    {
        await _emailSender.SendAsync(
            notification.Email, "Welcome", $"Welcome {notification.Name}!");
        
        _logger.LogInformation("Welcome email sent to {Email}", notification.Email);
    }
}
```

Now each class has one reason to change. The service changes when business rules change. The email handler changes when notification logic changes. The validator changes when validation rules change.

### Why This Is Better

- Each class is small and focused. You can understand it quickly.
- Changes in one area do not affect unrelated areas.
- Each class is easier to test. Testing the service does not require an SMTP server.
- Responsibilities can be reused independently. The email sender can be used for other events.

### When Applying SRP Too Far Becomes Counterproductive

Splitting a class into ten tiny classes, each with one method, creates unnecessary indirection. If the `MapToDto` method is only called from one place and is five lines long, extracting it to a separate class adds complexity without value.

SRP is about separating things that change for different reasons. If two responsibilities always change together, they can stay in the same class. Use your judgment.

---

## 8.2 Open/Closed Principle (OCP)

### The Principle

Software entities should be open for extension but closed for modification.

You should be able to add new behavior without changing existing code. When a new requirement arrives, you add new code. You do not modify code that already works.

### The Problem

The following example imagines that the `Employee` entity has been extended with a `Type` property and an `EmployeeType` enum. This is a hypothetical extension to demonstrate the principle.

Consider a notification service that sends different messages based on employee type:

```csharp
public class NotificationService
{
    public void SendWelcomeMessage(Employee employee)
    {
        if (employee.Type == EmployeeType.FullTime)
        {
            SendFullTimeWelcome(employee);
        }
        else if (employee.Type == EmployeeType.PartTime)
        {
            SendPartTimeWelcome(employee);
        }
        else if (employee.Type == EmployeeType.Contractor)
        {
            SendContractorWelcome(employee);
        }
        // Every time a new employee type is added, this method must change
    }
}
```

Every time a new employee type is added, this method must be modified. The method grows with each new type. Each modification risks breaking existing behavior. This violates OCP.

### The Improved Version

Use interfaces and polymorphism:

```csharp
public interface IWelcomeMessageStrategy
{
    EmployeeType Type { get; }
    Task SendWelcomeAsync(Employee employee);
}

public class FullTimeWelcomeStrategy : IWelcomeMessageStrategy
{
    public EmployeeType Type => EmployeeType.FullTime;
    public Task SendWelcomeAsync(Employee employee) { /* ... */ }
}

public class PartTimeWelcomeStrategy : IWelcomeMessageStrategy
{
    public EmployeeType Type => EmployeeType.PartTime;
    public Task SendWelcomeAsync(Employee employee) { /* ... */ }
}

public class NotificationService
{
    private readonly IEnumerable<IWelcomeMessageStrategy> _strategies;

    public NotificationService(IEnumerable<IWelcomeMessageStrategy> strategies)
    {
        _strategies = strategies;
    }

    public async Task SendWelcomeMessage(Employee employee)
    {
        var strategy = _strategies.FirstOrDefault(s => s.Type == employee.Type);
        if (strategy != null)
            await strategy.SendWelcomeAsync(employee);
    }
}
```

When a new employee type is added, you create a new strategy class. You register it in the DI container. You do not modify `NotificationService`. The service is closed for modification but open for extension.

### Why This Is Better

- Adding a new employee type does not risk breaking existing types.
- Each strategy is small and focused.
- Strategies can be tested independently.
- The `NotificationService` does not grow with each new type.

### When Applying OCP Too Far Becomes Counterproductive

Not every conditional statement violates OCP. If you have two or three cases that are unlikely to change, a simple `if-else` is cleaner than an interface, three classes, and DI registration.

OCP is most valuable when you know new variations will be added. If the employee types are fixed and will never change, the original `if-else` is simpler. Do not predict the future. Apply OCP where change is likely.

---

## 8.3 Liskov Substitution Principle (LSP)

### The Principle

Subtypes must be substitutable for their base types without altering the correctness of the program.

If code uses a base class or interface, it should work correctly with any implementation. The implementation should not surprise the caller with unexpected exceptions, stronger preconditions, or different behavior.

### The Problem

Consider a repository interface and two implementations:

```csharp
public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id);
    Task<IEnumerable<Employee>> GetAllAsync();
}

public class SqlEmployeeRepository : IEmployeeRepository
{
    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees.FindAsync(id);
        // Returns null if not found
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _context.Employees.ToListAsync();
    }
}

public class CachedEmployeeRepository : IEmployeeRepository
{
    public async Task<Employee?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be positive");
        // The SQL repository does not throw for id <= 0 — it just returns null
        
        var cached = _cache.Get<Employee>($"employee_{id}");
        return cached;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        throw new NotSupportedException("Cache does not support GetAll");
        // The SQL repository supports GetAll — this breaks substitution
    }
}
```

The `CachedEmployeeRepository` violates LSP in two ways:

1. `GetByIdAsync` throws an exception for `id <= 0`, but the `SqlEmployeeRepository` returns null. A caller that handles null but not the exception breaks.
2. `GetAllAsync` throws `NotSupportedException`. A caller that calls `GetAllAsync` on the interface expects it to work. It does not with this implementation.

### The Improved Version

Either make the implementations consistent or split the interface:

**Option 1: Make the cache consistent.**

```csharp
public class CachedEmployeeRepository : IEmployeeRepository
{
    public async Task<Employee?> GetByIdAsync(int id)
    {
        // Return null for invalid IDs, matching the SQL repository behavior
        if (id <= 0) return null;
        
        var cached = _cache.Get<Employee>($"employee_{id}");
        return cached;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        // Return empty collection instead of throwing
        return await _cache.GetAllEmployeesAsync() ?? Enumerable.Empty<Employee>();
    }
}
```

**Option 2: Split the interface if the cache genuinely cannot support all operations.**

```csharp
public interface IEmployeeReadOnlyRepository
{
    Task<Employee?> GetByIdAsync(int id);
}

public interface IEmployeeFullRepository : IEmployeeReadOnlyRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
    Task AddAsync(Employee employee);
}
```

The cache implements `IEmployeeReadOnlyRepository`. The SQL repository implements `IEmployeeFullRepository`. Callers that need `GetAll` depend on the full interface. Callers that only need `GetById` depend on the read-only interface.

### Why This Is Better

- Callers can trust that any implementation of the interface behaves consistently.
- No unexpected exceptions.
- Code that uses the interface works correctly with any implementation.

### When Applying LSP Too Far Becomes Counterproductive

Some implementations legitimately cannot support all operations. In that case, split the interface (as in Option 2). Do not force an implementation to provide a meaningless result just to satisfy the interface.

LSP is about honoring contracts. If the contract is wrong (the interface promises too much), fix the contract. Do not force implementations to pretend they can do something they cannot.

---

## 8.4 Interface Segregation Principle (ISP)

### The Principle

Clients should not be forced to depend on methods they do not use.

Large interfaces with many methods force implementations to provide methods they do not need. This creates coupling and makes the interface difficult to change.

### The Problem

Consider a large repository interface:

```csharp
public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByIdWithDepartmentAsync(int id);
    Task<IEnumerable<Employee>> GetWithDepartmentAsync();
    Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<Employee>> GetBySalaryRangeAsync(decimal min, decimal max);
    Task AddAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> EmailExistsAsync(string email);
    Task<int> CountAsync();
    Task ImportFromCsvAsync(Stream file);
    Task ExportToCsvAsync();
}
```

A service that only reads employees by ID must depend on an interface that includes `ImportFromCsvAsync`, `ExportToCsvAsync`, `DeleteAsync`, and other methods it does not use. If the CSV export logic changes, the service must be recompiled even though it does not use CSV export.

A mock that replaces this interface for testing must configure or ignore all 14 methods, even though the test only uses one.

### The Improved Version

Split the interface by purpose:

```csharp
public interface IEmployeeReadRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByIdWithDepartmentAsync(int id);
    Task<IEnumerable<Employee>> GetWithDepartmentAsync();
}

public interface IEmployeeWriteRepository
{
    Task AddAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(int id);
}

public interface IEmployeeQueryRepository
{
    Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<Employee>> GetBySalaryRangeAsync(decimal min, decimal max);
    Task<bool> ExistsAsync(int id);
    Task<bool> EmailExistsAsync(string email);
}
```

A service that only reads employees depends on `IEmployeeReadRepository`. A service that creates employees depends on `IEmployeeWriteRepository`. Each interface is small and focused.

### Why This Is Better

- Services depend only on the methods they use.
- Changes to CSV export do not affect services that do not use CSV.
- Mocks in tests only configure the methods being used.
- Interfaces are easier to understand because they are small.

### When Applying ISP Too Far Becomes Counterproductive

Do not split an interface until there is a real reason. If all services use all methods, splitting the interface creates unnecessary complexity.

The current `IEmployeeRepository` in the EmployeeManagement application has five methods. This is manageable. If it grows to 15 methods and only some services use some methods, then splitting is appropriate.

ISP is about reducing coupling. If there is no coupling problem, there is no need to split.

---

## 8.5 Dependency Inversion Principle (DIP)

### The Principle

High-level modules should not depend on low-level modules. Both should depend on abstractions.

Abstractions should not depend on details. Details should depend on abstractions.

"High-level" means business logic. "Low-level" means technical details like databases, file systems, and external services. The business logic should not know which database is used. The database implementation should conform to the business logic's expectations.

### Dependency Inversion vs. Dependency Injection

These are related but different concepts:

- **Dependency Injection** is a technique: passing dependencies to a class through its constructor rather than creating them inside the class.
- **Dependency Inversion** is a principle: high-level logic should depend on abstractions (interfaces), not on concrete implementations.

Dependency Injection is how you achieve Dependency Inversion in practice. DI is the mechanism. DIP is the design guideline.

### The Problem

```csharp
public class EmployeeService
{
    private readonly EmployeeRepository _repository;
    // Depends on a concrete class

    public EmployeeService()
    {
        _repository = new EmployeeRepository(
            new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer("Server=...;Database=...;")
                    .Options));
        // Creates its own dependencies, coupled to SQL Server
    }
}
```

This service:

- Depends on a concrete class (`EmployeeRepository`), not an interface.
- Creates its own database connection.
- Is coupled to SQL Server.
- Cannot be tested without a database.
- Cannot use a different database without modification.

### The Improved Version

```csharp
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    // Depends on an abstraction

    public EmployeeService(IEmployeeRepository repository)
    {
        _repository = repository;
    }
}
```

This is the actual structure of `EmployeeService` in the EmployeeManagement application. It depends on `IEmployeeRepository`, an interface defined in the Application layer. The `EmployeeRepository` in the Infrastructure layer implements the interface. The DI container wires them together.

The business logic (`EmployeeService`) does not know which database is used. The database implementation (`EmployeeRepository`) conforms to the interface defined by the business logic.

### Why This Is Better

- The service can be tested with a mock repository.
- The database can be changed without modifying the service.
- The service is decoupled from infrastructure details.
- This is the foundation of Clean Architecture: dependencies point inward toward the business logic.

### How DIP Supports Unit Testing and Clean Architecture

Without DIP, you cannot unit test the service because it creates its own database connection. With DIP, you replace the database with a mock.

Without DIP, the Application layer depends on the Infrastructure layer. With DIP, the Application layer defines interfaces and the Infrastructure layer implements them. The dependency direction is inverted: Infrastructure depends on Application, not the other way around.

This is the architecture you built in Days 1–2. DIP is not a new concept. It is the principle that explains why the architecture works.

### When Applying DIP Too Far Becomes Counterproductive

Not every class needs an interface. If a class has one implementation and is unlikely to change, an interface adds indirection without value.

Example: The `EmployeeMapper` class (if you extract it from `EmployeeService`) has one implementation. Creating an `IEmployeeMapper` interface is unnecessary unless you want to replace the mapper in tests or support multiple mapping strategies.

DIP is about depending on abstractions where it matters: at the boundaries between layers, at the boundaries between business logic and infrastructure, and anywhere you need to replace an implementation for testing or flexibility.

---

## How the SOLID Principles Work Together

These principles are not independent. They support each other:

- **SRP** separates responsibilities into focused classes.
- **OCP** allows you to add new behavior without modifying existing classes.
- **LSP** ensures that implementations honor their contracts.
- **ISP** keeps interfaces small and purpose-specific.
- **DIP** ensures that high-level logic depends on abstractions, not concrete implementations.

Code that follows all five principles is:

- Easy to understand (SRP).
- Easy to extend (OCP).
- Safe to substitute (LSP).
- Easy to implement and test (ISP).
- Easy to test and change (DIP).

In the next two topics, you will learn how Clean Code practices and refactoring techniques help you move existing code toward these principles.
