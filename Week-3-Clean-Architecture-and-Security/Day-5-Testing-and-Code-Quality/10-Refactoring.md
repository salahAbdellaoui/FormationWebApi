# 10 — Refactoring

## What Refactoring Means

Refactoring is the process of improving the internal structure of existing code without changing its external behavior. You reorganize code to make it easier to understand, easier to test, and easier to maintain — but the code does the same thing before and after.

Refactoring is not:

- Adding a new feature.
- Fixing a bug.
- Rewriting code from scratch.
- Changing what the code does.

Refactoring is:

- Renaming a variable to make its purpose clear.
- Extracting a method to reduce the length of another method.
- Removing duplicated code by introducing a shared function.
- Replacing a magic number with a named constant.
- Simplifying a complex conditional.
- Moving a method to a more appropriate class.

---

## Why Developers Refactor Existing Code

Code degrades over time. Requirements change. Quick fixes accumulate. Multiple developers add code in different styles. The code that was clean when first written becomes cluttered, duplicated, and difficult to understand.

This degradation is called **technical debt**. Like financial debt, technical debt accumulates interest: the longer you leave it, the more expensive it becomes to change the code.

Refactoring pays down technical debt. It makes the code easier to understand, easier to test, and easier to extend. It reduces the cost of future changes.

---

## The Difference Between Refactoring and Adding New Features

When you add a new feature, you change what the code does. When you refactor, you change how the code is structured without changing what it does.

Mixing the two is dangerous. If you refactor and add features at the same time, you do not know if a test failure is caused by the refactoring or the new feature. You cannot tell if the behavior changed intentionally or accidentally.

Professional developers separate these activities:

1. **Refactor first**: Improve the structure. Run the tests. Verify that all tests pass. The behavior has not changed.
2. **Add features second**: Add the new functionality. Run the tests. Verify that the new tests pass. The behavior has changed as intended.

---

## Why Refactoring Should Preserve Observable Behavior

The entire point of refactoring is to improve the code without introducing bugs. If the behavior changes, you have not refactored — you have modified the code. And if you have not tested the modification, you do not know if the change is correct.

This is why tests are essential for refactoring. Tests verify that the behavior has not changed. If you refactor and a test fails, you know the behavior changed and you can fix it before it reaches production.

Without tests, refactoring is guesswork. You change the code and hope nothing breaks. With tests, refactoring is engineering. You change the code and verify that nothing breaks.

---

## Why Tests Provide Confidence During Refactoring

Consider the `EmployeeService.CreateEmployeeAsync` method. You want to refactor it to extract the validation logic into a separate method. Without tests, you make the change and deploy. If the validation logic has a subtle bug — it allows a negative salary that the original code rejected — you do not know until a user creates an employee with a negative salary.

With tests:

```csharp
[Fact]
public async Task CreateEmployeeAsync_WithNegativeSalary_ThrowsException()
{
    var mockRepository = new Mock<IEmployeeRepository>();
    var service = new EmployeeService(mockRepository.Object);
    var dto = new CreateEmployeeDto { Name = "John", Email = "john@example.com", Salary = -1000, DepartmentId = 1 };
    
    await Assert.ThrowsAsync<ArgumentException>(
        () => service.CreateEmployeeAsync(dto));
}
```

You refactor. You run the tests. The test for negative salary passes. You know the validation logic still works correctly.

Tests are the safety net that makes refactoring safe.

---

## How to Identify Code Smells

Code smells are surface indicators of deeper problems. They do not prove that the code is wrong, but they suggest that something might need improvement.

### Duplicated Code

The same logic appears in two or more places. If you change the logic in one place, you must remember to change it in all other places.

```csharp
// In CreateEmployeeAsync
employee.Email = dto.Email.Trim().ToLower();

// In UpdateEmployeeAsync
employee.Email = dto.Email.Trim().ToLower();
```

### Long Methods

A method with 50+ lines. The reader must hold many details in their head at once.

```csharp
public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
{
    // 50 lines of validation, checking, creating, notifying, logging...
}
```

### Large Classes

A class with 10+ fields or 20+ methods. It has too many responsibilities.

```csharp
public class EmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger _logger;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    // 15 more fields...
}
```

### Excessive Conditional Logic

Long chains of `if-else` or `switch` statements. They are difficult to extend and test.

```csharp
if (employee.Type == EmployeeType.FullTime) { /* ... */ }
else if (employee.Type == EmployeeType.PartTime) { /* ... */ }
else if (employee.Type == EmployeeType.Contractor) { /* ... */ }
else if (employee.Type == EmployeeType.Intern) { /* ... */ }
```

### Tight Coupling

A class creates its own dependencies or depends on concrete classes. It cannot be tested or changed independently.

```csharp
public EmployeeService()
{
    _repository = new EmployeeRepository(new AppDbContext(...));
}
```

### Poor Naming

Method names that do not describe what they do. Variable names that do not describe what they represent.

```csharp
public async Task<EmployeeDto> DoIt(CreateEmployeeDto x) { /* ... */ }
```

### Methods with Too Many Responsibilities

A method that validates, creates, notifies, logs, and exports. It does too many things.

---

## Refactoring Techniques

### Rename Variable or Method

**Before:**

```csharp
public async Task<EmployeeDto> Proc(CreateEmployeeDto x)
{
    var e = new Employee { Name = x.Name, Email = x.Email };
    await _r.Add(e);
    return ToDto(e);
}
```

**After:**

```csharp
public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
{
    var employee = new Employee { Name = dto.Name, Email = dto.Email };
    await _employeeRepository.AddAsync(employee);
    return MapToDto(employee);
}
```

**Why:** The renamed method and variables communicate intent. The reader understands what the code does without reading the implementation.

### Extract Method

**Before:**

```csharp
public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Name))
        throw new ArgumentException("Name is required");
    if (string.IsNullOrWhiteSpace(dto.Email))
        throw new ArgumentException("Email is required");
    if (dto.Salary < 0)
        throw new ArgumentException("Salary must be positive");

    var employee = new Employee { Name = dto.Name, Email = dto.Email, Salary = dto.Salary, DepartmentId = dto.DepartmentId };
    await _repository.AddAsync(employee);
    return MapToDto(employee);
}
```

**After:**

```csharp
public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
{
    ValidateDto(dto);

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

private static void ValidateDto(CreateEmployeeDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Name))
        throw new ArgumentException("Name is required");
    if (string.IsNullOrWhiteSpace(dto.Email))
        throw new ArgumentException("Email is required");
    if (dto.Salary < 0)
        throw new ArgumentException("Salary must be positive");
}
```

**Why:** The main method is shorter and communicates the high-level steps. The validation details are in a separate method that the reader can explore if needed.

### Extract Class

**Before:** `EmployeeService` handles validation, creation, email notification, and logging.

**After:** `EmployeeService` handles creation. `EmployeeValidator` handles validation. `EmployeeCreatedHandler` handles notification and logging.

**Why:** Each class has one responsibility. Changes in one area do not affect unrelated areas. Each class is easier to test.

### Replace Magic Number with a Named Constant

**Before:**

```csharp
if (salary > 50000)
    return salary * 0.15;
```

**After:**

```csharp
const decimal HighSalaryThreshold = 50000;
const decimal HighBonusRate = 0.15m;

if (salary > HighSalaryThreshold)
    return salary * HighBonusRate;
```

**Why:** The named constants communicate intent. If the threshold changes, you update one constant.

### Replace Repeated Logic with a Shared Function

**Before:**

```csharp
// In multiple methods:
employee.Email = dto.Email.Trim().ToLower();
employee.Name = dto.Name.Trim();
```

**After:**

```csharp
private static string NormalizeEmail(string email) => email.Trim().ToLower();
private static string NormalizeName(string name) => name.Trim();

// Usage:
employee.Email = NormalizeEmail(dto.Email);
employee.Name = NormalizeName(dto.Name);
```

**Why:** The normalization rules exist in one place. Changes apply everywhere.

---

## A Progressive Refactoring Example

The following example uses a hypothetical scenario where the `Employee` entity has been extended with `IsActive` and `LastModified` properties. The refactoring techniques apply to any codebase.

Start with a method that has multiple code smells:

```csharp
public async Task<object> Handle(int id, string action)
{
    var e = await _context.Employees.FindAsync(id);
    if (e != null)
    {
        if (action == "activate")
        {
            e.IsActive = true;
            e.LastModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _email.SendAsync(e.Email, "Activated", "Your account is active");
            return new { success = true, message = "Activated" };
        }
        else if (action == "deactivate")
        {
            e.IsActive = false;
            e.LastModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _email.SendAsync(e.Email, "Deactivated", "Your account is inactive");
            return new { success = true, message = "Deactivated" };
        }
    }
    return new { success = false, message = "Not found" };
}
```

Code smells: poor naming, long method, conditional logic, mixed responsibilities, direct DbContext dependency, anonymous return type.

### Step 1: Rename

```csharp
public async Task<ActionResult> ChangeEmployeeStatusAsync(int id, string action)
{
    var employee = await _context.Employees.FindAsync(id);
    if (employee != null)
    {
        if (action == "activate")
        {
            employee.IsActive = true;
            employee.LastModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _emailSender.SendAsync(employee.Email, "Activated", "Your account is active");
            return new OkResult();
        }
        else if (action == "deactivate")
        {
            employee.IsActive = false;
            employee.LastModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _emailSender.SendAsync(employee.Email, "Deactivated", "Your account is inactive");
            return new OkResult();
        }
    }
    return new NotFoundResult();
}
```

### Step 2: Extract Methods

```csharp
public async Task<ActionResult> ChangeEmployeeStatusAsync(int id, string action)
{
    var employee = await _employeeRepository.GetByIdAsync(id);
    if (employee == null) return new NotFoundResult();

    switch (action.ToLower())
    {
        case "activate":
            await ActivateEmployeeAsync(employee);
            break;
        case "deactivate":
            await DeactivateEmployeeAsync(employee);
            break;
        default:
            return new BadRequestResult();
    }

    return new OkResult();
}

private async Task ActivateEmployeeAsync(Employee employee)
{
    employee.IsActive = true;
    employee.LastModified = DateTime.UtcNow;
    await _employeeRepository.UpdateAsync(employee);
    await _emailSender.SendAsync(employee.Email, "Activated", "Your account is active");
}

private async Task DeactivateEmployeeAsync(Employee employee)
{
    employee.IsActive = false;
    employee.LastModified = DateTime.UtcNow;
    await _employeeRepository.UpdateAsync(employee);
    await _emailSender.SendAsync(employee.Email, "Deactivated", "Your account is inactive");
}
```

### Step 3: Remove Duplication

```csharp
private async Task SetEmployeeStatusAsync(Employee employee, bool isActive)
{
    employee.IsActive = isActive;
    employee.LastModified = DateTime.UtcNow;
    await _employeeRepository.UpdateAsync(employee);
    
    var status = isActive ? "Activated" : "Deactivated";
    var message = isActive ? "Your account is active" : "Your account is inactive";
    await _emailSender.SendAsync(employee.Email, status, message);
}

public async Task<ActionResult> ChangeEmployeeStatusAsync(int id, string action)
{
    var employee = await _employeeRepository.GetByIdAsync(id);
    if (employee == null) return new NotFoundResult();

    switch (action.ToLower())
    {
        case "activate":
            await SetEmployeeStatusAsync(employee, true);
            break;
        case "deactivate":
            await SetEmployeeStatusAsync(employee, false);
            break;
        default:
            return new BadRequestResult();
    }

    return new OkResult();
}
```

Each step is small. Each step preserves behavior. Tests verify that the behavior has not changed at each step.

---

## How to Avoid Changing Too Many Things at Once

The golden rule of refactoring: **one change at a time**.

1. Make one small change.
2. Run the tests.
3. If they pass, commit (or proceed to the next change).
4. If they fail, fix the problem before making another change.

Do not rename five methods, extract three new methods, and move two methods to a different class all in one step. If a test fails, you do not know which change caused it.

Small steps with frequent verification are faster than large steps with late verification.

---

## Common Mistakes

**Mistake 1: Refactoring without tests.**

You change the code and hope nothing breaks. This is not refactoring. This is modifying code without verification.

**Mistake 2: Changing behavior during refactoring.**

You "fix" a bug while refactoring. Now you do not know if a test failure is caused by the refactoring or the bug fix. Separate the two activities.

**Mistake 3: Refactoring too much at once.**

You rename, extract, move, and restructure in one big change. If something breaks, you do not know where. Make small, verifiable changes.

**Mistake 4: Refactoring for its own sake.**

Not every piece of code needs refactoring. If the code works, is readable, and is easy to change, leave it alone. Refactor when the code is difficult to understand, difficult to test, or difficult to change.

---

## Connection to Other Topics

Refactoring depends on tests. Tests verify that behavior is preserved during refactoring. Without tests, refactoring is unsafe.

Refactoring applies SOLID principles and Clean Code practices. When you extract a method, you apply SRP. When you remove duplication, you apply Clean Code. When you introduce an interface, you apply DIP.

Refactoring connects to testing, SOLID, and Clean Code in a cycle: tests enable refactoring, refactoring improves the design, and improved design makes testing easier.

The next topic demonstrates this cycle with a single coherent example.
