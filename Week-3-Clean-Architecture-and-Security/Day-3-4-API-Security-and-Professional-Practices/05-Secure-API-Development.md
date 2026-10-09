# 05 — Secure API Development

**Duration**: 35 minutes

---

## Protecting Existing Endpoints

The Employee Management API currently exposes endpoints without any authentication or authorization. Before adding JWT authentication, we need to decide which endpoints require authentication and which are public.

This decision follows a simple principle: if the endpoint reads or writes sensitive data, it requires authentication. The only public endpoints should be those that do not expose any protected data.

---

## Choosing Which Endpoints Require Authentication

| Endpoint | Authentication Required? | Reason |
|---|---|---|
| `POST /api/auth/login` | No | Public endpoint to obtain a token |
| `GET /api/employees` | Yes | Returns sensitive employee data |
| `GET /api/employees/{id}` | Yes | Returns sensitive employee data |
| `POST /api/employees` | Yes | Creates a new record (write operation) |
| `PUT /api/employees/{id}` | Yes | Modifies an existing record (write operation) |
| `DELETE /api/employees/{id}` | Yes | Removes a record (destructive operation) |

The login endpoint is the only public endpoint. It must be public because the user needs it to get a token in the first place. Everything else requires authentication.

---

## Applying Authorization at the Right Level

There are two approaches to applying the `[Authorize]` attribute.

### Approach 1: Apply to Individual Actions

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        return Ok(employees);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
            return NotFound();
        return Ok(employee);
    }

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
}
```

Use this approach when different actions have different authorization requirements.

### Approach 2: Apply to the Entire Controller

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        return Ok(employees);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
            return NotFound();
        return Ok(employee);
    }

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
}
```

When `[Authorize]` is on the controller class, every action requires authentication. Individual actions can then add stricter policies (like `CanManageEmployees`). If a specific action should be public, use `[AllowAnonymous]`:

```csharp
[AllowAnonymous]
[HttpGet("public-count")]
public IActionResult GetEmployeeCount()
{
    // Anyone can access this
    return Ok(42);
}
```

### When to Use Each Approach

- **Controller-level `[Authorize]`** when most or all actions require authentication. This is the safer default — you will not forget to protect an endpoint.
- **Action-level `[Authorize]`** when only some actions require authentication, or when different actions need different policies.

For the Employee Management API, controller-level `[Authorize]` is the better choice. Every employee endpoint deals with sensitive data.

---

## Protecting Sensitive Write Operations

Write operations (POST, PUT, DELETE) should always require authentication and appropriate authorization. This is not optional.

- **POST** creates data. Without authorization, anyone can insert records.
- **PUT** modifies data. Without authorization, anyone can change records.
- **DELETE** removes data. Without authorization, anyone can destroy records.

Always apply at minimum `[Authorize]` to write operations. Apply role-based or policy-based authorization when only certain users should be able to perform the operation:

```csharp
[Authorize(Policy = "AdminOnly")]
[HttpDelete("{id:int}")]
public async Task<IActionResult> Delete(int id)
{
    await _employeeService.DeleteEmployeeAsync(id);
    return NoContent();
}
```

Deleting an employee is a destructive, high-impact operation. Only Admins should be able to do it. This is enforced on the server, not the client.

---

## Server-Side Verification

Authorization must be enforced on the server. The frontend might hide a "Delete" button from non-admin users. That is good UX. That is not security.

A malicious user can:

- Open the browser dev tools and un-hide the button.
- Send a DELETE request directly with curl or Postman.
- Modify the frontend JavaScript to bypass UI restrictions.

If the server does not check authorization, the delete will succeed regardless of what the frontend does.

The frontend controls what the user sees. The server controls what the user can do. Never rely on the frontend for security.

---

## Handling 401 and 403

ASP.NET Core handles authentication and failure responses automatically when the middleware is configured correctly.

| Scenario | HTTP Status | Meaning |
|---|---|---|
| No token provided | `401 Unauthorized` | The request is anonymous. Authentication is required. |
| Invalid or expired token | `401 Unauthorized` | The token cannot be validated. The user must re-authenticate. |
| Valid token, insufficient permissions | `403 Forbidden` | The user is authenticated but does not satisfy the authorization policy. |
| Resource not found | `404 Not Found` | The user is authorized, but the requested resource does not exist. |

The middleware pipeline handles this:

```csharp
var app = builder.Build();

app.UseAuthentication();   // Reads and validates the token
app.UseAuthorization();    // Checks authorization policies
app.MapControllers();      // Routes to controller actions
```

The order matters. `UseAuthentication` must come before `UseAuthorization`. If the token is not validated first, authorization has nothing to check.

---

## Avoiding Insecure Direct Object Access (IDOR)

Consider this scenario: Alice is authenticated and can view her own employee record at `/api/employees/5`. Can she also access `/api/employees/6`? Can she access every employee in the system?

If the answer is yes, the API has an **Insecure Direct Object Reference (IDOR)** vulnerability. The user is authenticated (they have a valid token), but they can access resources they should not have access to.

IDOR happens when:

- The API checks "is the user logged in?" but not "is the user allowed to access THIS specific resource?"
- The URL contains a resource identifier (an ID) that the user can guess or enumerate.

For this training app, we do not implement per-user data isolation. All authenticated users can view all employees. But in a real application, you would need to:

1. Check the user's identity against the requested resource.
2. Return `403 Forbidden` if the user does not own or have access to that resource.
3. Use resource-based authorization or query filters to limit data access.

Example of what a safe endpoint would look like:

```csharp
[Authorize]
[HttpGet("{id:int}")]
public async Task<ActionResult<EmployeeDto>> GetById(int id)
{
    var employee = await _employeeService.GetEmployeeByIdAsync(id);
    if (employee == null)
        return NotFound();

    // Check if the current user is allowed to view this employee
    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!CanUserViewEmployee(currentUserId, employee))
        return Forbid();

    return Ok(employee);
}
```

---

## Not Trusting Client-Supplied Identity

Never accept the user's identity from the request body or query string. Always use the authenticated principal.

### Wrong

```csharp
[HttpPost("transfer")]
public IActionResult TransferOwnership([FromBody] TransferRequest request)
{
    // request.UserId comes from the client — DO NOT trust this
    var user = await _userService.GetByIdAsync(request.UserId);
    // ...
}
```

A malicious client can send any `UserId` in the request body. The server has no way to verify that the client is actually that user.

### Correct

```csharp
[Authorize]
[HttpPost("transfer")]
public IActionResult TransferOwnership()
{
    // User identity comes from the validated JWT — trust this
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var user = await _userService.GetByIdAsync(userId);
    // ...
}
```

The `User` property comes from the JWT, which is signed and validated by the server. The client cannot change the claims inside the JWT without invalidating the signature.

This rule applies to roles too. Never accept a role from the request body. Always read roles from `User.FindAll(ClaimTypes.Role)`.

---

## Protecting Credentials and Tokens

### Never Log Tokens

JWT tokens grant access to the system. If a token appears in a log file, anyone with access to that log can impersonate the user.

```csharp
// WRONG — never do this
_logger.LogInformation("User token: {Token}", token);

// WRONG — even in "debug" contexts
_logger.LogDebug("Auth header: {Header}", request.Headers["Authorization"]);
```

If you must log request details for debugging, strip the Authorization header first.

### Never Log Passwords

Same principle. Passwords in logs are exposed credentials.

```csharp
// WRONG
_logger.LogInformation("Login attempt: user={User}, password={Password}", user, password);
```

Log the username if necessary. Never log the password.

### Use HTTPS in Production

JWT tokens travel in the HTTP Authorization header. Without HTTPS, the token is sent in plaintext. Anyone on the network can intercept it.

In development, HTTP is acceptable. In production, HTTPS is mandatory. ASP.NET Core enforces this with `app.UseHttpsRedirection()`.

### Store Tokens Securely on the Client

Tokens should not be stored in localStorage (vulnerable to XSS attacks). HttpOnly cookies are the safer option for web applications. For mobile apps, use secure storage APIs (Keychain on iOS, Keystore on Android).

---

## Avoiding Sensitive Information in Responses

API responses should not include sensitive data that the client does not need.

```csharp
// WRONG — returns internal data
public class EmployeeResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PasswordHash { get; set; }     // Never return this
    public string InternalNotes { get; set; }     // Not for the client
    public string SocialSecurityNumber { get; set; } // Highly sensitive
}
```

Use DTOs to control what the API returns. The DTO should contain only the data the client needs:

```csharp
// Correct — returns only what the client needs
public class EmployeeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public decimal Salary { get; set; }
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
}
```

This is one of the reasons DTOs exist. They act as a boundary between your internal data model and the external API contract.

---

## Authentication vs Authorization vs Validation vs Errors

These are four distinct concerns. Each has a different purpose, runs at a different time, and returns a different response.

| Concern | Question | Example | HTTP Status on Failure |
|---|---|---|---|
| Authentication | Who are you? | Login with username and password | `401 Unauthorized` |
| Authorization | What can you do? | Can this user delete employees? | `403 Forbidden` |
| Validation | Is the input valid? | Is the email format correct? Is the salary positive? | `400 Bad Request` |
| Server Error | Did something go wrong? | Database connection failed, null reference | `500 Internal Server Error` |

The order of execution is always:

1. **Authentication** — validate the token, identify the user.
2. **Authorization** — check if the user has permission for this action.
3. **Validation** — check if the input data meets business rules.
4. **Execution** — perform the operation. If something fails, return a server error.

If authentication fails, the request never reaches authorization. If authorization fails, the request never reaches validation. Each gate must pass before the next one is checked.

---

## Common Mistakes

**Mistake 1: Accepting userId from the client.**
A request body contains `"userId": 5`. The server uses this ID to identify the current user. A malicious client sends `"userId": 1` and acts as a different user. Always use `User.FindFirst(ClaimTypes.NameIdentifier)` to get the authenticated user's ID.

**Mistake 2: Not authorizing write operations.**
GET endpoints have `[Authorize]` but POST, PUT, and DELETE do not. This means anyone can create, modify, or delete data without authentication. Every endpoint that changes data must require authorization.

**Mistake 3: Logging tokens or passwords.**
Tokens and passwords end up in log files. Log files are read by developers, sysadmins, monitoring tools, and potentially attackers. Sensitive data in logs is exposed data.

**Mistake 4: Exposing internal errors to the client.**
Returning the full exception message in the API response leaks implementation details: database schema, file paths, stack traces, library versions. In production, return a generic error message and log the full exception server-side.

**Mistake 5: Applying authentication inconsistently.**
Some endpoints are protected, others are not, and the pattern is not clear. This leads to accidental exposure. Use controller-level `[Authorize]` as the default and opt out with `[AllowAnonymous]` only where explicitly needed.
