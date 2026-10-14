# 09 — Clean Code

## Why Clean Code Matters

Code is read far more often than it is written. A developer might write a method once but read it ten times — their own code and other developers' code — while debugging, extending, or reviewing it.

Clean code is code that is easy to read, easy to understand, and easy to change. It does not mean clever code or minimal code. It means code that communicates its intent clearly to the next developer who reads it. That developer might be you in six months, and you will not remember every detail.

This topic covers practical Clean Code practices. Each practice is shown as a before-and-after example with an explanation of why the improvement matters.

---

## Meaningful Naming

### The Problem

```csharp
public async Task<EmployeeDto> DoIt(CreateEmployeeDto x)
{
    var t = new Employee
    {
        Name = x.Name,
        Email = x.Email,
        Salary = x.Salary,
        DepartmentId = x.DepartmentId
    };
    await _repo.Add(t);
    return Map(t);
}
```

What does `DoIt` do? What is `x`? What is `t`? What is `_repo`? The reader must study the implementation to understand the intent.

### The Improvement

```csharp
public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
{
    var employee = new Employee
    {
        Name = dto.Name,
        Email = dto.Email,
        Salary = dto.Salary,
        DepartmentId = dto.DepartmentId
    };

    await _employeeRepository.AddAsync(employee);

    return MapToDto(employee);
}
```

The method name describes the action. The parameter name describes the type. The variable names describe what they represent. The reader understands the intent without reading the implementation.

### Guidelines

- Name methods after what they do: `CreateEmployeeAsync`, `CalculateTotalSalary`, `ValidateEmail`.
- Name variables after what they represent: `employee`, `departmentName`, `totalSalary`.
- Name booleans as questions: `isActive`, `hasPermission`, `emailExists`.
- Avoid abbreviations unless they are universally understood: `dto`, `id`, `url` are fine. `emp`, `dept`, `mgr` are not.
- Avoid generic names: `data`, `info`, `temp`, `result`, `handle`. These names do not communicate intent.

---

## Small, Focused Methods

### The Problem

The following example shows a hypothetical extended service that handles validation, duplicate checking, department lookup, and email notification. It demonstrates what happens when a method accumulates too many responsibilities.

```csharp
public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
{
    // Validate
    if (string.IsNullOrWhiteSpace(dto.Name))
        throw new ArgumentException("Name is required");
    if (string.IsNullOrWhiteSpace(dto.Email))
        throw new ArgumentException("Email is required");
    if (!dto.Email.Contains("@"))
        throw new ArgumentException("Email must be valid");
    if (dto.Salary < 0)
        throw new ArgumentException("Salary must be positive");

    // Check duplicate
    var existing = await _repository.GetByEmailAsync(dto.Email);
    if (existing != null)
        throw new InvalidOperationException("Email already exists");

    // Check department exists
    var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId);
    if (department == null)
        throw new InvalidOperationException("Department not found");

    // Create employee
    var employee = new Employee
    {
        Name = dto.Name,
        Email = dto.Email,
        Salary = dto.Salary,
        DepartmentId = dto.DepartmentId
    };
    await _repository.AddAsync(employee);

    // Send notification
    await _emailSender.SendAsync(
        dto.Email, "Welcome", $"Welcome {dto.Name}!");

    // Log
    _logger.LogInformation("Employee {Name} created", dto.Name);

    return MapToDto(employee);
}
```

This method has four responsibilities: validation, duplicate checking, employee creation, and notification. A reader must hold all four in their head at once.

### The Improvement

```csharp
public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
{
    ValidateCreateDto(dto);
    await EnsureEmailIsUniqueAsync(dto.Email);
    await EnsureDepartmentExistsAsync(dto.DepartmentId);

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

private static void ValidateCreateDto(CreateEmployeeDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Name))
        throw new ArgumentException("Name is required");
    if (string.IsNullOrWhiteSpace(dto.Email))
        throw new ArgumentException("Email is required");
    if (!dto.Email.Contains("@"))
        throw new ArgumentException("Email must be valid");
    if (dto.Salary < 0)
        throw new ArgumentException("Salary must be positive");
}

private async Task EnsureEmailIsUniqueAsync(string email)
{
    var existing = await _repository.GetByEmailAsync(email);
    if (existing != null)
        throw new InvalidOperationException("Email already exists");
}

private async Task EnsureDepartmentExistsAsync(int departmentId)
{
    var department = await _departmentRepository.GetByIdAsync(departmentId);
    if (department == null)
        throw new InvalidOperationException("Department not found");
}
```

The main method now reads like a summary: validate, check email, check department, create, return. The details are in smaller methods that the reader can explore if needed.

Note: Sending email and logging are cross-cutting concerns that should be handled by separate handlers (see SRP in Topic 08), not by the service itself.

---

## Guard Clauses and Reducing Nesting

### The Problem

```csharp
public async Task<EmployeeDto?> ProcessAsync(int id)
{
    var employee = await _repository.GetByIdAsync(id);
    if (employee != null)
    {
        if (employee.Salary > 0)
        {
            if (employee.Department != null)
            {
                return MapToDto(employee);
            }
        }
    }
    return null;
}
```

Three levels of nesting. The reader must track three conditions to understand when the method returns a value.

### The Improvement

```csharp
public async Task<EmployeeDto?> ProcessAsync(int id)
{
    var employee = await _repository.GetByIdAsync(id);

    if (employee == null) return null;
    if (employee.Salary <= 0) return null;
    if (employee.Department == null) return null;

    return MapToDto(employee);
}
```

Guard clauses check the failure conditions first and exit early. The main logic is at the top level, not nested three levels deep. The reader understands the happy path immediately.

---

## Avoiding Magic Numbers and Unexplained Constants

### The Problem

```csharp
public decimal CalculateBonus(decimal salary)
{
    if (salary > 50000)
        return salary * 0.15;
    else if (salary > 30000)
        return salary * 0.10;
    else
        return salary * 0.05;
}
```

What do `50000`, `30000`, `0.15`, `0.10`, and `0.05` mean? Where do they come from? Can they change?

### The Improvement

```csharp
private const decimal HighSalaryThreshold = 50000;
private const decimal MediumSalaryThreshold = 30000;
private const decimal HighBonusRate = 0.15m;
private const decimal MediumBonusRate = 0.10m;
private const decimal LowBonusRate = 0.05m;

public decimal CalculateBonus(decimal salary)
{
    if (salary > HighSalaryThreshold)
        return salary * HighBonusRate;
    if (salary > MediumSalaryThreshold)
        return salary * MediumBonusRate;
    return salary * LowBonusRate;
}
```

Named constants communicate intent. A reader understands that `50000` is the high salary threshold and `0.15` is the high bonus rate. If the threshold changes, you update one constant instead of searching for every occurrence of `50000`.

---

## Avoiding Duplicated Logic

### The Problem

```csharp
public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
{
    var employee = new Employee
    {
        Name = dto.Name.Trim(),
        Email = dto.Email.Trim().ToLower(),
        Salary = Math.Round(dto.Salary, 2),
        DepartmentId = dto.DepartmentId
    };
    await _repository.AddAsync(employee);
    return MapToDto(employee);
}

public async Task<EmployeeDto> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto)
{
    var employee = await _repository.GetByIdAsync(id);
    if (employee == null) throw new KeyNotFoundException();
    
    employee.Name = dto.Name.Trim();
    employee.Email = dto.Email.Trim().ToLower();
    employee.Salary = Math.Round(dto.Salary, 2);
    employee.DepartmentId = dto.DepartmentId;
    
    await _repository.UpdateAsync(employee);
    return MapToDto(employee);
}
```

The normalization logic (`Trim()`, `ToLower()`, `Math.Round()`) is duplicated. If the normalization rules change, you must update both methods.

### The Improvement

```csharp
public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
{
    var employee = CreateEmployeeFromDto(dto);
    await _repository.AddAsync(employee);
    return MapToDto(employee);
}

public async Task<EmployeeDto> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto)
{
    var employee = await _repository.GetByIdAsync(id)
        ?? throw new KeyNotFoundException();
    
    ApplyDtoToEmployee(dto, employee);
    await _repository.UpdateAsync(employee);
    return MapToDto(employee);
}

private static Employee CreateEmployeeFromDto(CreateEmployeeDto dto)
{
    return new Employee
    {
        Name = NormalizeName(dto.Name),
        Email = NormalizeEmail(dto.Email),
        Salary = NormalizeSalary(dto.Salary),
        DepartmentId = dto.DepartmentId
    };
}

private static void ApplyDtoToEmployee(UpdateEmployeeDto dto, Employee employee)
{
    employee.Name = NormalizeName(dto.Name);
    employee.Email = NormalizeEmail(dto.Email);
    employee.Salary = NormalizeSalary(dto.Salary);
    employee.DepartmentId = dto.DepartmentId;
}

private static string NormalizeName(string name) => name.Trim();
private static string NormalizeEmail(string email) => email.Trim().ToLower();
private static decimal NormalizeSalary(decimal salary) => Math.Round(salary, 2);
```

The normalization rules exist in one place. If the email normalization changes (e.g., to preserve case), you update one method.

---

## Comments That Explain Intent

### The Problem: Comments That Restate the Code

```csharp
// Check if the employee is null
if (employee == null)
    return null;

// Set the name
employee.Name = dto.Name;

// Return the DTO
return MapToDto(employee);
```

These comments add noise. They repeat what the code already says.

### The Improvement: Comments That Explain Why

```csharp
// Salary is rounded to 2 decimal places to match the accounting system's precision
employee.Salary = Math.Round(dto.Salary, 2);

// Employees with zero salary are kept in the system for audit purposes
// but are excluded from active employee lists
if (employee.Salary <= 0) return null;

// Email is normalized to lowercase to ensure uniqueness checks are case-insensitive
employee.Email = dto.Email.Trim().ToLower();
```

These comments explain why the code does something that might not be obvious from the code itself.

---

## Handling Errors Explicitly

### The Problem

```csharp
public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
{
    try
    {
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
    catch
    {
        return null;
    }
}
```

This catches all exceptions and returns null. The caller does not know if the employee was created or if an error occurred. Validation errors, database errors, and network errors are all treated the same way.

### The Improvement

```csharp
public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Name))
        throw new ArgumentException("Name is required", nameof(dto));
    
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
```

Let exceptions propagate. The global exception handler (from Day 3–4) converts them to appropriate HTTP responses. The caller receives a meaningful error message.

---

## Consistent Formatting and Coding Conventions

Establish and follow conventions:

- **Async methods** end with `Async`: `GetAllEmployeesAsync`, `CreateEmployeeAsync`.
- **Private fields** use `_camelCase`: `_repository`, `_employeeService`.
- **Interface names** start with `I`: `IEmployeeRepository`, `IEmployeeService`.
- **DTOs** end with `Dto`: `EmployeeDto`, `CreateEmployeeDto`.
- **Braces**: Use Allman style (opening brace on its own line) or K&R style (opening brace on same line). Pick one and use it consistently.
- **Namespaces**: Use file-scoped namespaces (`namespace X;`) to reduce nesting.

Consistency reduces the cognitive load of reading code. When every file follows the same conventions, the reader focuses on what the code does, not how it is formatted.

---

## Using DTOs and Clear Interfaces

The EmployeeManagement application already uses DTOs correctly. The `CreateEmployeeDto` defines the input contract. The `EmployeeDto` defines the output contract. The `IEmployeeService` interface defines what the service does without revealing how.

This separation allows you to change the internal implementation without affecting the API contract. If you add a field to `EmployeeDto`, you do not change the `Employee` entity. If you change the service implementation, you do not change the controller.

---

## Avoiding Misleading Names and Hidden Side Effects

### The Problem

```csharp
public int Process(Employee employee)
{
    employee.Email = employee.Email.Trim().ToLower();
    // The method name "Process" does not suggest that it modifies the input
    
    if (employee.Salary > 100000)
        _auditLog.Log("High salary", employee.Id);
    // The method has a hidden side effect: logging
    
    return employee.DepartmentId;
    // The return value is not obvious from the method name
}
```

### The Improvement

```csharp
public int GetDepartmentIdAndNormalizeEmail(Employee employee)
{
    employee.Email = employee.Email.Trim().ToLower();
    _auditLog.Log("Employee accessed", employee.Id);
    return employee.DepartmentId;
}
```

Or better, separate the side effect:

```csharp
public int GetDepartmentId(Employee employee) => employee.DepartmentId;

public void NormalizeEmailAndRecordAccess(Employee employee)
{
    employee.Email = employee.Email.Trim().ToLower();
    _auditLog.Log("Employee accessed", employee.Id);
}
```

A method should do what its name says and nothing more. If a method modifies its input, the name should indicate that. If a method has side effects, they should be explicit.

---

## Connection to Other Topics

Clean Code practices support testing. Code with clear names is easier to test because the test names describe what is being tested. Code with small methods is easier to test because each method has fewer behaviors to verify. Code without duplication is easier to test because you test the logic once.

Clean Code practices support SOLID principles. Small, focused methods are easier to keep Single Responsibility. Guard clauses reduce nesting, which makes code easier to extend (OCP). Clear naming helps ensure that implementations honor their contracts (LSP).

Clean Code practices support refactoring. When code is clear and well-named, it is easier to recognize where improvements are needed and to verify that refactoring preserves behavior.

The next topic covers refactoring: the process of improving existing code while preserving its behavior.
