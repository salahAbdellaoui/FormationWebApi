# 01 — Architecture Principles

**Duration**: 25 minutes

---

## Why Architecture Matters

Software lives longer than anyone expects. The code you write today will be read, modified, and extended by people who were not in the room when the decisions were made. Architecture is not about making code look clever. It is about making code **changeable** without breaking things that were not supposed to break.

Good architecture keeps the cost of change low. When a business rule changes, you should edit one file, not five. When you swap a database, the rest of the app should not notice. When you write a test, you should not need a running server.

Architecture also communicates intent. A well-structured project tells a new developer where to look, where to add code, and where not to touch. A flat project with no boundaries forces every developer to read everything before changing anything.

Finally, architecture enables parallel work. When concerns are separated, two developers can work on different layers without stepping on each other's code.

---

## What Happens Without Architecture

Recall the `EmployeesController` from Week 2. It receives the HTTP request, maps the DTO to an entity, saves to the database, maps the entity back to a DTO, and returns the response. All in one method.

```csharp
[HttpPost]
public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
{
    var employee = new Employee
    {
        Name = dto.Name,
        Email = dto.Email,
        Salary = dto.Salary,
        DepartmentId = dto.DepartmentId
    };

    _context.Employees.Add(employee);
    await _context.SaveChangesAsync();

    var response = new EmployeeDto
    {
        Id = employee.Id,
        Name = employee.Name,
        Email = employee.Email,
        Salary = employee.Salary,
        DepartmentId = employee.DepartmentId
    };

    return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
}
```

This works. For now. But read on.

---

## Problems with This Approach

1. **Cannot test without a database.** The controller creates a real `Employee` and calls `SaveChangesAsync` on a real `AppDbContext`. To unit test "create employee," you need a running database or a complex in-memory setup. Business logic is locked behind infrastructure.

2. **Business rules live inside the controller.** If tomorrow you need to validate that salary must be positive, or that email must be unique, that logic goes into the controller action. The controller now handles HTTP concerns and business concerns at the same time.

3. **Cannot swap the persistence layer.** The controller depends directly on `AppDbContext` (an EF Core class). If you want to replace EF Core with Dapper, a microservice call, or a file-based store, you must rewrite the controller.

4. **Mapping is duplicated.** Every action that creates, reads, or updates an employee repeats the same entity-to-DTO mapping code. Change a property name and you hunt through the entire controller.

5. **The controller grows without limit.** Every new feature adds code to the same file. After a few weeks, the controller is 500+ lines, every developer is afraid to touch it, and merge conflicts are constant.

---

## What Good Architecture Gives You

| Dimension | Without Architecture | With Clean Architecture |
|---|---|---|
| **Testability** | Need a running database to test anything | Unit test business rules with plain objects |
| **Changing database** | Rewrite controller actions | Swap the infrastructure project; nothing else changes |
| **Changing business rules** | Edit controller methods scattered across actions | Edit one service class or domain entity |
| **Adding a new use case** | Add more code to the controller | Add a new service method; controller stays thin |
| **Understanding the code** | Must read the entire controller to find logic | Each layer has one job; navigate by concern |
| **Parallel development** | Everyone edits the same files | Frontend, business logic, and persistence can evolve independently |

---

## Clean Architecture Is Not Over-Engineering

There is a real trade-off. More projects means more files, more project references, and more initial setup. For some situations, a single project is the right answer.

**A single project is fine when:**
- The app is a small CRUD tool with 3-5 endpoints and no complex business logic.
- It is a prototype or proof of concept that may be thrown away.
- There is one developer and no plan to scale the team.
- The domain is trivial: no rules beyond "save to database."

**Clean Architecture earns its keep when:**
- The app has real business rules (validation, calculations, state transitions).
- The team is growing and needs clear boundaries to avoid conflicts.
- You need to test business logic without infrastructure.
- The persistence mechanism might change, or you need to support multiple data sources.
- The domain is complex enough that mixing HTTP and business logic causes real bugs.

The goal is not maximum number of projects. The goal is **appropriate boundaries** that reduce the cost of change for your specific application. Start with the boundaries that solve your current pain, not the boundaries that a diagram on the internet says you should have.

---

## Senior Developer Note

> **Every architectural decision is a trade-off.** Clean Architecture adds indirection. More layers mean more files to navigate for simple operations. The question is not "is this the most elegant design?" but "does this structure reduce the cost of the changes I expect to make?"
>
> A small app with one project and clear internal separation is better than a 4-project solution where every change requires editing 8 files. Architecture should match the problem, not the other way around.
>
> The best developers know when to add a layer and when to leave it out. That judgment comes from experience, not from rules.

---

## Knowledge Check

1. Name two concrete problems that occur when a controller directly uses `AppDbContext` to handle business logic.

2. A team has a single-project API with 3 endpoints and no business rules beyond "save to database." A new developer suggests splitting it into 4 projects. What would you advise, and why?

3. What does "the cost of change" mean in the context of software architecture? Give one example where good architecture lowers that cost.
