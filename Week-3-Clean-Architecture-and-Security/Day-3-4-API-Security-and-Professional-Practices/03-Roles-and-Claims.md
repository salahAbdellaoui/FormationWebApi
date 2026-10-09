# 03 — Roles and Claims

**Duration**: 30 minutes

---

## What Are Claims?

A **claim** is a statement about the user. It is a key-value pair that describes who the user is, what they can access, or any other piece of identity information.

Examples of claims:

- "The user's name is Alice"
- "The user's email is alice@example.com"
- "The user's role is Admin"
- "The user belongs to the HR department"

In JWT-based authentication, claims are stored inside the **JWT payload**. When a request arrives with a valid token, the ASP.NET Core authentication middleware reads the JWT, extracts the claims, and builds a .NET identity object from them.

A claim by itself is just data. It is not a permission. It is not a role. It is a statement that some trusted party (the identity provider) has asserted about the user.

### Where Claims Live in a JWT

A JWT has three parts: header, payload, and signature. The payload contains the claims:

```json
{
  "sub": "12345",
  "name": "Alice",
  "email": "alice@example.com",
  "role": "Admin",
  "department": "HR",
  "exp": 1700000000
}
```

Each of these key-value pairs (`name`, `email`, `role`, `department`) is a claim. The `sub` (subject) claim identifies the user. The `exp` (expiration) claim tells the system when the token expires.

---

## What Are Roles?

A **role** is a specific type of claim used for authorization. It represents a group, a permission level, or a category that the user belongs to.

Common roles in the Employee Management system:

- **Admin** — full system access
- **HR** — can manage employee records
- **Employee** — can view their own data

Roles answer the question: "What group does this user belong to, and what can that group do?"

Roles are still claims under the hood. When the JWT contains `"role": "Admin"`, that is a claim with the key `role` and the value `Admin`. ASP.NET Core recognizes certain claims as roles and uses them for authorization checks.

---

## Claims vs Roles

| Concept | Purpose | Example |
|---|---|---|
| Claim | Any statement about the user | "User's name is Alice", "User's email is alice@example.com" |
| Role | A claim used for authorization | "User is an Admin", "User is in HR" |

Every role is a claim, but not every claim is a role. The user's email is a claim. The user's role as "Admin" is also a claim, but we call it a role because we use it specifically to make authorization decisions.

Think of it this way:

- Claims describe **who the user is** (name, email, department, date of birth).
- Roles describe **what the user can do** (at a coarse-grained level).

---

## The ClaimsPrincipal

After ASP.NET Core authenticates a request, it creates a `ClaimsPrincipal` object. This object contains all the claims extracted from the JWT.

In a controller, you access the `ClaimsPrincipal` through the `User` property (which is shorthand for `HttpContext.User`):

```csharp
// 'User' is a ClaimsPrincipal
var principal = User;
```

The `ClaimsPrincipal` has one or more `ClaimsIdentity` objects. Each identity contains a collection of `Claim` objects. In typical JWT authentication, there is one identity with all the claims from the token.

Key properties and methods:

| Member | Description |
|---|---|
| `User.Identity` | The primary `ClaimsIdentity` |
| `User.Identity.IsAuthenticated` | `true` if the user is authenticated |
| `User.FindFirst(type)` | Returns the first claim of the given type |
| `User.FindAll(type)` | Returns all claims of the given type |
| `User.IsInRole(role)` | Checks if the user has the specified role |

---

## Using Roles in Authorization

ASP.NET Core provides the `[Authorize]` attribute with a `Roles` parameter. This is the simplest way to restrict access based on roles.

### Require a Single Role

```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("{id:int}")]
public async Task<IActionResult> Delete(int id)
{
    await _employeeService.DeleteEmployeeAsync(id);
    return NoContent();
}
```

Only users with the `Admin` role can call this endpoint. Everyone else gets a `403 Forbidden` response.

### Require One of Several Roles

```csharp
[Authorize(Roles = "Admin,HR")]
[HttpPost]
public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
{
    var created = await _employeeService.CreateEmployeeAsync(dto);
    return CreatedAtAction(
        actionName: nameof(GetById),
        routeValues: new { id = created.Id },
        value: created);
}
```

Users with the `Admin` role OR the `HR` role can create employees. The comma means OR, not AND.

### Require Authentication Only

```csharp
[Authorize]
[HttpGet]
public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
{
    var employees = await _employeeService.GetAllEmployeesAsync();
    return Ok(employees);
}
```

No specific role required. Any authenticated user can access this endpoint. The `[Authorize]` attribute without `Roles` just checks that the user has a valid token.

---

## Accessing Claims in Controllers

You can read individual claims from the `User` object. This is useful when you need specific user information inside an action.

```csharp
[Authorize]
[HttpGet("me")]
public IActionResult GetCurrentUser()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var username = User.FindFirst(ClaimTypes.Name)?.Value;
    var email = User.FindFirst(ClaimTypes.Email)?.Value;
    var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);

    return Ok(new
    {
        UserId = userId,
        Username = username,
        Email = email,
        Roles = roles
    });
}
```

Key points:

- `ClaimTypes.NameIdentifier` is the standard claim type for the user's unique ID.
- `ClaimTypes.Name` is the standard claim type for the username.
- `ClaimTypes.Email` is the standard claim type for the email address.
- `ClaimTypes.Role` is the standard claim type for roles. A user can have multiple roles, so use `FindAll` instead of `FindFirst`.
- `FindFirst` returns `null` if the claim does not exist. Use `?.Value` to avoid a `NullReferenceException`.

These claim type constants come from `System.Security.Claims.ClaimTypes`.

---

## Role-Based Authorization Scenarios

Here is how roles map to operations in the Employee Management system:

### Admin

- Delete employees
- View all employee data, including salaries
- Manage system-wide settings
- Create, update, and delete any record

```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("{id:int}")]
public async Task<IActionResult> Delete(int id)
{
    await _employeeService.DeleteEmployeeAsync(id);
    return NoContent();
}
```

### HR

- Create and update employee records
- View salaries and sensitive data
- Cannot delete employees

```csharp
[Authorize(Roles = "Admin,HR")]
[HttpPut("{id:int}")]
public async Task<ActionResult<EmployeeDto>> Update(int id, [FromBody] CreateEmployeeDto dto)
{
    var updated = await _employeeService.UpdateEmployeeAsync(id, dto);
    return Ok(updated);
}
```

### Employee

- View their own data only
- Cannot access other employees' records
- Cannot view salary details of others
- Cannot create, update, or delete records

An employee-level endpoint might look like this:

```csharp
[Authorize]
[HttpGet("me")]
public async Task<ActionResult<EmployeeDto>> GetMyProfile()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int parsedUserId))
    {
        return BadRequest("Invalid user identifier");
    }
    
    var employee = await _employeeService.GetEmployeeByIdAsync(parsedUserId);
    return Ok(employee);
}
```

---

## Why Roles Don't Mean Unrestricted Access

Having the `Admin` role does not mean a user can do anything without restriction. Authorization should still be granular and intentional.

Consider these scenarios:

- An Admin might be able to delete employees but should not be able to bypass input validation.
- An Admin might manage employees in their department but not in other departments.
- An Admin might be able to view salaries but not modify them.

Roles are a coarse-grained mechanism. They answer "is this user in this group?" For finer-grained control, you need **policies** (covered in the next topic) or resource-based authorization.

Never assume that `[Authorize(Roles = "Admin")]` is enough protection for every operation. Ask: "Even if the user is an Admin, should they be allowed to do THIS specific thing?"

---

## Where Roles Come From

In JWT authentication, roles are added as claims when the token is generated. The flow works like this:

1. The user logs in with credentials (username and password).
2. The authentication service validates the credentials.
3. The authentication service looks up the user's roles from the database.
4. The authentication service creates a JWT and includes the roles as claims.
5. The client sends the JWT with each request.
6. ASP.NET Core extracts the roles from the JWT and makes them available via `User.IsInRole()` and `[Authorize(Roles = ...)]`.

The identity provider (or authentication service) decides what roles a user has. Your API trusts the JWT because it is signed. If the JWT says the user is an Admin, your API treats the user as an Admin.

This means the security of your role system depends entirely on:

- The security of the authentication service that generates the tokens.
- The security of the signing key that protects the tokens from tampering.
- The accuracy of the role data in your database.

If an attacker can modify the JWT payload, they can add any role they want. This is why JWTs are signed and why the signing key must be kept secret.

---

## Common Mistakes

**Mistake 1: Trusting client-supplied roles.**
Never accept a role from the request body or query string. A malicious client could send `"role": "Admin"` in the JSON body and your code might use it. Always read roles from the authenticated `ClaimsPrincipal` (`User`), never from user input.

**Mistake 2: Using roles for everything.**
Roles are coarse-grained. If you need to check "can this user edit this specific employee?", a role check alone is not enough. You need policies or resource-based authorization. Do not try to create a role for every possible permission. That leads to role explosion (dozens of roles that are hard to manage).

**Mistake 3: Not validating role membership.**
Just because a user claims to have a role does not mean they should. The role must come from a trusted source (the authentication service, backed by a database). Never let users assign themselves roles. Role assignment should be an administrative action.

**Mistake 4: Hardcoding role strings everywhere.**
Writing `Roles = "Admin"` in dozens of controllers creates maintenance problems. If the role name changes, you have to find every occurrence. Use constants or policies (covered in the next topic) to centralize role references.
