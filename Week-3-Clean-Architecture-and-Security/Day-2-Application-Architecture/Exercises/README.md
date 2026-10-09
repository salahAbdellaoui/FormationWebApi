# Day 2 — Exercises

**Duration:** 25 minutes (can extend for deeper learning)

These exercises build on Day 1's Clean Architecture implementation. You will analyze dependency flow, evaluate design patterns, refactor code, and recognize architectural patterns in existing code.

---

## Exercise 1 — Trace the Dependency Flow

**Goal:** Understand how dependencies flow through the layers.

**Starting Point:** The existing Clean Architecture implementation with:
- `IEmployeeRepository` interface
- `EmployeeRepository` implementation
- `IEmployeeService` interface
- `EmployeeService` implementation
- `EmployeesController`

**Task:** Given a Create Employee request, draw a diagram showing:
- Which interface is used
- Which class implements it
- Where the interface is defined
- Where the implementation is defined
- How Dependency Injection connects them

**Expected Behavior:** A correct dependency diagram showing the Dependency Inversion Principle in action. The diagram should show:
- Controller depends on `IEmployeeService` (not concrete `EmployeeService`)
- Service depends on `IEmployeeRepository` (not concrete `EmployeeRepository`)
- Interfaces defined in Application layer
- Implementations defined in Infrastructure layer
- DI container wires everything together in Program.cs

**Hints:**
- Start from the Controller and work backwards to the database
- Remember: dependencies point inward toward the domain
- Check Program.cs to see how services are registered

**Verification:** Draw the dependency diagram on paper or using a tool like draw.io. Verify that:
- All arrows point from outer layers to inner layers
- No layer references a concrete class from another layer
- The DI registration in Program.cs matches your diagram

**Common Mistake:** Drawing arrows from implementations to interfaces instead of from consumers to interfaces. Remember: the dependency direction is determined by who needs whom, not by who implements whom.

**Review Question:** If you removed the `IEmployeeRepository` interface, what would break?

---

## Exercise 2 — Analyze the Repository Trade-Off

**Goal:** Understand when the Repository Pattern adds value.

**Starting Point:** The current `EmployeeRepository` has methods like:

```csharp
public async Task<IEnumerable<Employee>> GetAllAsync()
{
    return await _context.Employees
        .AsNoTracking()
        .ToListAsync();
}
```

**Task:** A developer says: "This is just wrapping DbSet. We don't need the repository." 

Do you agree? Write a 3-paragraph argument for or against keeping the repository.

**Expected Behavior:** A reasoned argument that considers:
- **Testing benefits:** Can mock `IEmployeeRepository` in unit tests for `EmployeeService`
- **Boundary benefits:** Application layer doesn't know about EF Core, can swap data sources
- **Cost:** Extra files, indirection, learning curve for new developers
- **When the abstraction pays off:** Complex queries, multiple data sources, testability requirements
- **When it's overhead:** Simple CRUD apps, small teams, tight deadlines

**Hints:**
- Think about what happens when you need to write unit tests for `EmployeeService`
- Consider: what if you need to switch from SQL Server to a NoSQL database?
- Remember: abstractions have costs. When do the benefits outweigh the costs?

**Verification:** Your argument should have exactly 3 paragraphs. Each paragraph should address a different aspect (testing, boundaries, costs). You should have a clear position (for or against).

**Common Mistake:** Taking an extreme position without acknowledging trade-offs. Good architecture is about balance, not dogma.

**Review Question:** If you removed the repository and injected `DbContext` directly into `EmployeeService`, what would change?

---

## Exercise 3 — Move SaveChangesAsync to the Service

**Goal:** Understand the Unit of Work trade-off.

**Starting Point:** Currently, `EmployeeRepository.AddAsync` calls `SaveChangesAsync`:

```csharp
public async Task<Employee> AddAsync(Employee employee)
{
    _context.Employees.Add(employee);
    await _context.SaveChangesAsync();
    return employee;
}
```

**Task:** Move the `SaveChangesAsync` call to `EmployeeService.CreateEmployeeAsync` instead.

**Steps:**
1. Change `EmployeeRepository.AddAsync` to just track the entity:
```csharp
public async Task AddAsync(Employee employee)
{
    _context.Employees.Add(employee);
}
```

2. Add a `SaveChangesAsync` method to `IEmployeeRepository`:
```csharp
Task SaveChangesAsync();
```

3. Implement it in `EmployeeRepository`:
```csharp
public async Task SaveChangesAsync()
{
    await _context.SaveChangesAsync();
}
```

4. Call it from `EmployeeService.CreateEmployeeAsync`:
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

    await _repository.AddAsync(employee);
    await _repository.SaveChangesAsync();

    return new EmployeeDto
    {
        Id = employee.Id,
        Name = employee.Name,
        Email = employee.Email,
        Salary = employee.Salary,
        DepartmentId = employee.DepartmentId
    };
}
```

5. Build and verify

**Expected Behavior:** The service now controls when to save. The repository just tracks changes. This gives the service more control over the unit of work.

**Hints:**
- The repository method should not be async if it doesn't await anything
- Remember to update the interface, not just the implementation
- Check that the code compiles after each change

**Verification:** Run the application and create a new employee. Verify that:
- The employee is saved to the database
- The response returns the correct employee data
- No compilation errors

**Common Mistake:** Forgetting to add `SaveChangesAsync` to the interface and just calling `_context.SaveChangesAsync` directly in the service. This would require injecting `DbContext` into the service, breaking the abstraction.

**Review Question:** What are the pros and cons of saving in the service vs. saving in the repository?

---

## Exercise 4 — Fix the DepartmentName Issue

**Goal:** Understand DTO mapping trade-offs.

**Starting Point:** The Create endpoint returns an `EmployeeDto` with `DepartmentName = null`:

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
```

**Task:** Fix this by re-querying the employee with the Department after insert.

**Steps:**
1. After `_repository.AddAsync(employee)` and `_repository.SaveChangesAsync()`, call:
```csharp
var employeeWithDepartment = await _repository.GetByIdWithDepartmentAsync(employee.Id);
```

2. Check if the result is null (defensive coding):
```csharp
if (employeeWithDepartment == null)
{
    throw new Exception("Employee not found after creation");
}
```

3. Use the result to build the DTO with `DepartmentName` populated:
```csharp
return new EmployeeDto
{
    Id = employeeWithDepartment.Id,
    Name = employeeWithDepartment.Name,
    Email = employeeWithDepartment.Email,
    Salary = employeeWithDepartment.Salary,
    DepartmentId = employeeWithDepartment.DepartmentId,
    DepartmentName = employeeWithDepartment.Department?.Name
};
```

4. Build and test with curl or Swagger

**Expected Behavior:** Create now returns the full employee with `DepartmentName` populated correctly.

**Hints:**
- You need to call `SaveChangesAsync` first (from Exercise 3) before re-querying
- Use `GetByIdWithDepartmentAsync` which includes the Department navigation property
- Check if `Department` is null before accessing `Name` (use null-conditional operator `?.`)

**Verification:** Create a new employee with a valid `DepartmentId` and verify the response includes the correct `DepartmentName`.

**Common Mistake:** Not checking if the re-query returns null. While it shouldn't happen (the employee was just inserted), defensive coding matters in production systems.

**Review Question:** What is the performance cost of re-querying? When would you accept the null vs. re-query?

---

## Exercise 5 — Identify CQRS in the Code

**Goal:** Recognize that CQRS is already present at a basic level.

**Starting Point:** The existing `IEmployeeService` interface:

```csharp
public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
    Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto);
}
```

**Task:** List all methods in `IEmployeeService`. Categorize each as a Command (changes state) or Query (retrieves data). Explain why the separation exists.

**Expected Behavior:**

| Method | Type | Description |
|--------|------|-------------|
| `GetAllEmployeesAsync()` | Query | Retrieves data, doesn't change state |
| `GetEmployeeByIdAsync(int id)` | Query | Retrieves data, doesn't change state |
| `CreateEmployeeAsync(CreateEmployeeDto createDto)` | Command | Creates new employee, changes state |

**Explanation:** The separation exists because:
- Commands mutate state and should have side effects
- Queries read data and should not have side effects
- This separation makes the code easier to understand and test
- It enables optimizations like caching queries separately from commands
- It's the foundation for more advanced patterns like Event Sourcing

**Hints:**
- Ask: "Does this method change the database?" If yes, it's a Command. If no, it's a Query.
- CQRS = Command Query Responsibility Segregation
- At this basic level, the separation is implicit. In advanced CQRS, you'd have separate interfaces or even separate models for reads and writes.

**Verification:** Your categorization should match the table above. Your explanation should mention state mutation.

**Common Mistake:** Confusing Commands with Queries. Remember: if it writes to the database, it's a Command. If it only reads, it's a Query.

**Review Question:** If you added an `UpdateEmployee` method, would it be a command or query? What about a `SearchEmployees` method?

---

## Summary

These exercises reinforced:
- Dependency flow in Clean Architecture
- Trade-offs in the Repository Pattern
- Unit of Work pattern and where to save changes
- DTO mapping and re-querying strategies
- CQRS at a basic level

You now have a deeper understanding of architectural patterns and when to apply them.
