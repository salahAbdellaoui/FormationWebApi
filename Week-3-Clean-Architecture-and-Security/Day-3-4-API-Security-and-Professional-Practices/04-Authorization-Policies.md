# 04 — Authorization Policies

**Duration**: 30 minutes

---

## What Are Authorization Policies?

An authorization **policy** is a named, reusable set of authorization rules. Instead of scattering `[Authorize(Roles = "Admin,HR")]` across your controllers, you define a policy once and apply it by name.

```csharp
// Without policies — role names repeated everywhere
[Authorize(Roles = "Admin,HR")]
[HttpPost]
public async Task<ActionResult<EmployeeDto>> Create(...) { }

[Authorize(Roles = "Admin,HR")]
[HttpPut("{id:int}")]
public async Task<IActionResult> Update(...) { }

// With policies — define once, use by name
[Authorize(Policy = "CanManageEmployees")]
[HttpPost]
public async Task<ActionResult<EmployeeDto>> Create(...) { }

[Authorize(Policy = "CanManageEmployees")]
[HttpPut("{id:int}")]
public async Task<IActionResult> Update(...) { }
```

A policy wraps one or more **requirements**. The most common requirement is a role check, but policies can also check claims, run custom logic, or combine multiple conditions.

---

## Why Use Policies?

### Reusability

Define the rule once. Apply it to any number of controllers and actions. No copy-pasting role lists.

### Maintainability

Change the rule in one place. If "managing employees" later requires a new role or an additional claim, you update the policy definition. Every endpoint using that policy picks up the change automatically.

### Readability

`[Authorize(Policy = "CanManageEmployees")]` tells you exactly what the endpoint requires. `[Authorize(Roles = "Admin,HR")]` tells you which roles are allowed, but not why. Policies express intent, not implementation.

### Flexibility

Policies can combine multiple requirements:

- Role checks ("user must be Admin or HR")
- Claim checks ("user must have an email claim")
- Claim value checks ("user's department claim must be HR")
- Custom logic ("user must have been active in the last 30 days")

---

## Policy Registration

Policies are registered in `Program.cs` inside the `AddAuthorization` call:

```csharp
builder.Services.AddAuthorization(options =>
{
    // Role-based policy
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // Multiple roles (user must have at least one)
    options.AddPolicy("CanManageEmployees", policy =>
        policy.RequireRole("Admin", "HR"));

    // Claim-based policy (user must have the claim, any value)
    options.AddPolicy("MustHaveEmail", policy =>
        policy.RequireClaim(ClaimTypes.Email));

    // Claim with specific value
    options.AddPolicy("HRDepartment", policy =>
        policy.RequireClaim("Department", "HR"));

    // Combining requirements (ALL must be satisfied)
    options.AddPolicy("HRCanManageEmployees", policy =>
    {
        policy.RequireRole("Admin", "HR");
        policy.RequireClaim("Department", "HR");
    });
});
```

Key points:

- `RequireRole("Admin", "HR")` means the user must have the Admin role OR the HR role.
- `RequireClaim(ClaimTypes.Email)` means the user must have an email claim, regardless of its value.
- `RequireClaim("Department", "HR")` means the user must have a `Department` claim with the value `HR`.
- When you add multiple requirements to a policy, ALL of them must be satisfied. They are combined with AND logic.

---

## Applying Policies

Once a policy is registered, apply it to controllers or actions with the `Policy` parameter:

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    // Anyone authenticated can list employees
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        return Ok(employees);
    }

    // Only Admin or HR can create employees
    [Authorize(Policy = "CanManageEmployees")]
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
    {
        var created = await _employeeService.CreateEmployeeAsync(dto);
        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = created.Id },
            value: created);
    }

    // Only Admin can delete employees
    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _employeeService.DeleteEmployeeAsync(id);
        return NoContent();
    }
}
```

If the user does not satisfy the policy requirements, ASP.NET Core returns a `403 Forbidden` response automatically.

---

## Policy vs Role Authorization

| Approach | Example | When to Use |
|---|---|---|
| Role-based | `[Authorize(Roles = "Admin")]` | Simple role checks, few roles, quick prototyping |
| Policy-based | `[Authorize(Policy = "CanDelete")]` | Complex rules, reusable logic, better maintainability, production code |

Role-based authorization is fine for simple cases. But once you need more than a single role check, policies become the better choice.

Consider what happens when business rules change:

- **With roles:** "Now Managers can also create employees." You search the codebase for every `[Authorize(Roles = "Admin,HR")]` and add `Manager`.
- **With policies:** You change one line in `Program.cs`: `policy.RequireRole("Admin", "HR", "Manager")`. Every endpoint using the policy is updated.

---

## Custom Policy Requirements

For complex authorization logic, you can create custom requirements. A custom requirement is a class that implements `IAuthorizationRequirement` and a handler that implements `AuthorizationHandler<T>`.

Example scenario: "A user can only edit employees in their own department."

This cannot be expressed with a simple role or claim check. It requires custom logic that compares the user's department claim against the employee's department. You would:

1. Create a class `DepartmentMatchRequirement` that implements `IAuthorizationRequirement`.
2. Create a handler `DepartmentMatchHandler` that extends `AuthorizationHandler<DepartmentMatchRequirement>`.
3. The handler reads the user's department claim and compares it to a resource (the employee being edited).
4. Register the requirement in a policy and apply the policy to the controller action.

This is an advanced topic. The key takeaway is that policies can go far beyond simple role checks. They can express any authorization logic your application needs.

---

## Resource-Based Authorization as a Concept

Sometimes authorization depends not just on the user, but on the specific resource being accessed. This is called **resource-based authorization**.

Examples:

- "Alice can edit Employee #5 because she is in the same department."
- "Bob cannot delete Employee #10 because he did not create it."
- "Carol can view Employee #3's salary because she is that employee's manager."

In resource-based authorization, the authorization check receives both the user (the `ClaimsPrincipal`) and the resource (the specific entity being accessed). The handler can then make decisions based on the relationship between the user and the resource.

This is different from role-based or policy-based authorization, which only look at the user. Resource-based authorization answers: "Can this user access THIS specific thing?"

ASP.NET Core supports resource-based authorization through `IAuthorizationService` and custom authorization handlers. The controller injects `IAuthorizationService` and calls `AuthorizeAsync` with the user, the resource, and the policy name.

For the Employee Management training app, we do not implement resource-based authorization. But you should know it exists for scenarios where per-entity access control is needed.

---

## Policy vs Input Validation

These are different concerns that are sometimes confused.

| Concern | Question | Example |
|---|---|---|
| Authorization (Policy) | Can this user perform this action? | "Is this user allowed to delete employees?" |
| Validation | Is this input valid? | "Is the email format correct? Is the salary positive?" |

Authorization runs before the action executes. It checks the user's identity and permissions. If authorization fails, the action never runs.

Validation runs inside the action (or via model validation attributes). It checks whether the input data meets business rules. If validation fails, the action returns an error response (typically `400 Bad Request`).

An endpoint needs both:

```csharp
[Authorize(Policy = "CanManageEmployees")]  // Can this user create employees?
[HttpPost]
public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
// Model validation checks: Is Name non-empty? Is Email valid? Is Salary > 0?
{
    var created = await _employeeService.CreateEmployeeAsync(dto);
    return CreatedAtAction(
        actionName: nameof(GetById),
        routeValues: new { id = created.Id },
        value: created);
}
```

The policy says "only Admin or HR can create employees." Validation says "the name must not be empty, the email must be valid, and the salary must be positive." Both checks are necessary. Neither replaces the other.

---

## Common Mistakes

**Mistake 1: Duplicating role checks instead of using policies.**
Writing `[Authorize(Roles = "Admin,HR")]` in ten different places means ten places to update when the rule changes. Define a policy once and reference it by name. This is the entire point of policies.

**Mistake 2: Confusing policies with validation.**
A policy checks "can this user do this?" Validation checks "is this input correct?" If you try to validate input inside a policy handler, you are mixing concerns. Policies deal with identity and permissions. Validation deals with data quality.

**Mistake 3: Not registering policies before using them.**
If you apply `[Authorize(Policy = "CanManageEmployees")]` but never register the `CanManageEmployees` policy in `Program.cs`, every request to that endpoint will be rejected with a `403 Forbidden`. The policy name in the attribute must match a registered policy exactly.

**Mistake 4: Putting all requirements in one policy.**
Creating a single "SuperAdmin" policy that checks five different claims and roles is hard to understand and maintain. Break complex authorization into multiple focused policies. A policy should have a clear, descriptive name that explains what it authorizes.
