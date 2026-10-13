# 12 — Putting Everything Together

**Duration**: 30 minutes

---

## The Complete Picture

Over the previous eleven topics, you learned each piece in isolation. Authentication. Authorization. Validation. Exception handling. Logging. Configuration. CORS. Security practices. Each solves a specific problem.

But a real API does not run these concerns separately. They work together, in sequence, on every request. Understanding how they fit into a single request flow is the final step.

This topic walks through one complete request — from the moment it arrives at the server to the moment the response leaves — and shows where each concern belongs, why it is in that order, and how the layers interact.

---

## The Scenario

An authenticated HR user named Alice sends a POST request to create a new employee:

```http
POST https://api.example.com/api/employees
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "name": "Bob Smith",
  "email": "bob.smith@example.com",
  "salary": 65000,
  "departmentId": 2
}
```

Alice has a valid JWT token. Her token contains claims: user ID, username, and the role "HR". She is making a cross-origin request from a frontend application at `https://app.example.com`.

Follow this request through the entire pipeline.

---

## Step 1: HTTPS and the Network Layer

Before ASP.NET Core processes the request, the transport layer handles it.

The request arrives over HTTPS. TLS encryption protects the request body, the JWT token, and all headers from eavesdropping on the network. Without HTTPS, the JWT token would be visible in plain text to anyone on the network path.

`app.UseHttpsRedirection()` in `Program.cs` ensures that any HTTP request is redirected to HTTPS. In production, `app.UseHsts()` tells browsers to always use HTTPS for this domain.

**Where this is configured:** `Program.cs` middleware pipeline.

**What could go wrong:** If HTTPS is not enforced, tokens and data travel in plain text. An attacker on the same network can capture them.

---

## Step 2: Exception Handling Middleware

The first significant middleware in the pipeline is the exception handler.

```csharp
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.ContentType = "application/problem+json";
        // ... produce safe Problem Details response
    });
});
```

This middleware wraps everything that comes after it. If any downstream middleware or controller action throws an unhandled exception, this handler catches it and produces a safe error response.

At this point, the exception handler does nothing — the request has not failed yet. It is simply in position, ready to catch anything that goes wrong.

**Why it is first:** Exception handling must wrap the entire pipeline. If it were placed after authentication, an exception in the authentication middleware would not be caught.

**What this connects to:** Topic 06 (Global Exception Handling).

---

## Step 3: CORS Middleware

Next, the CORS middleware runs.

```csharp
app.UseCors("AllowFrontend");
```

The request comes from `https://app.example.com`, which is a different origin than `https://api.example.com`. The browser has already sent a preflight `OPTIONS` request, which was handled by this same middleware. Now, on the actual `POST` request, the CORS middleware checks the `Origin` header against the allowed origins in the policy.

The request's origin (`https://app.example.com`) matches one of the configured origins. The CORS middleware adds the `Access-Control-Allow-Origin` header to the response later.

If the origin were not allowed, the CORS middleware would reject the request. The browser would then block the response from reaching the JavaScript code. But the request would still reach the server — CORS is a browser restriction, not a server-side security mechanism.

**Why it is before authentication:** CORS must handle preflight `OPTIONS` requests, which are anonymous. If authentication ran first, preflight requests would fail with `401 Unauthorized` before CORS headers were added.

**What this connects to:** Topic 10 (CORS).

---

## Step 4: Authentication Middleware

```csharp
app.UseAuthentication();
```

The authentication middleware reads the `Authorization: Bearer <token>` header and extracts the JWT.

It performs four validations:

1. **Signature validation** — recalculates the HMAC-SHA256 signature using the server's secret key and compares it to the token's signature. They match. The token has not been tampered with.

2. **Issuer validation** — checks that the `iss` claim matches the expected issuer (`EmployeeManagementApi`). It does.

3. **Audience validation** — checks that the `aud` claim matches the expected audience (`EmployeeManagementClient`). It does.

4. **Lifetime validation** — checks that the current time is before the `exp` (expiration) claim. The token was issued 20 minutes ago and expires in 60 minutes. It is still valid.

All four validations pass. The middleware extracts the claims from the token payload and builds a `ClaimsPrincipal` object:

```text
ClaimsPrincipal
├── ClaimTypes.NameIdentifier = "7"
├── ClaimTypes.Name = "alice"
├── ClaimTypes.Role = "HR"
├── JwtRegisteredClaimNames.Jti = "abc123..."
└── (other standard claims: iat, exp, iss, aud)
```

This `ClaimsPrincipal` is attached to the `HttpContext.User` property. From this point forward, every middleware and controller action can access the authenticated user's identity through `User`.

If any validation had failed — expired token, wrong signature, unknown issuer — the middleware would not create a `ClaimsPrincipal`. The request would proceed without an authenticated user. The authorization middleware would then return `401 Unauthorized`.

**What this connects to:** Topic 02 (JWT Authentication).

---

## Step 5: Authorization Middleware

```csharp
app.UseAuthorization();
```

The authorization middleware checks whether the authenticated user satisfies the requirements for the target endpoint.

The `EmployeesController` has `[Authorize]` at the controller level, so the request must be authenticated. It is — the `ClaimsPrincipal` exists.

The `Create` action has `[Authorize(Policy = "CanManageEmployees")]`. The middleware looks up the `CanManageEmployees` policy, which was registered in `Program.cs`:

```csharp
options.AddPolicy("CanManageEmployees", policy =>
    policy.RequireRole("Admin", "HR"));
```

The policy requires the user to have the `Admin` role OR the `HR` role. Alice's token contains `ClaimTypes.Role = "HR"`. The requirement is satisfied.

Authorization passes. The request proceeds to the controller.

If Alice had the role `Employee` instead of `HR`, the authorization check would fail. The middleware would return `403 Forbidden` without calling the controller action.

**What this connects to:** Topic 04 (Authorization Policies), Topic 03 (Roles and Claims).

---

## Step 6: Routing and Model Binding

ASP.NET Core's routing middleware matches the URL `/api/employees` and HTTP method `POST` to the `Create` action on `EmployeesController`.

The model binding layer reads the `Content-Type: application/json` header, reads the request body, and deserializes the JSON into a `CreateEmployeeDto` object:

```csharp
public class CreateEmployeeDto
{
    public string Name { get; set; } = "Bob Smith";
    public string Email { get; set; } = "bob.smith@example.com";
    public decimal Salary { get; set; } = 65000;
    public int DepartmentId { get; set; } = 2;
}
```

The DTO is passed as the argument to the `Create` action.

---

## Step 7: FluentValidation

Before the controller action body executes, the FluentValidation middleware runs.

It finds the `CreateEmployeeDtoValidator` class (registered via `AddFluentValidation` in `Program.cs`) and executes all rules against the DTO:

```csharp
RuleFor(x => x.Name)
    .NotEmpty()          → "Bob Smith" is not empty → PASS
    .MaximumLength(100)  → 9 characters ≤ 100 → PASS

RuleFor(x => x.Email)
    .NotEmpty()          → "bob.smith@example.com" is not empty → PASS
    .EmailAddress()      → valid email format → PASS
    .MaximumLength(200)  → 20 characters ≤ 200 → PASS

RuleFor(x => x.Salary)
    .GreaterThan(0)      → 65000 > 0 → PASS
    .LessThanOrEqualTo(1000000) → 65000 ≤ 1,000,000 → PASS

RuleFor(x => x.DepartmentId)
    .GreaterThan(0)      → 2 > 0 → PASS
```

All rules pass. The validator returns a valid result. The controller action executes.

If any rule had failed — for example, if `Name` were empty — the FluentValidation middleware would short-circuit the pipeline. The controller action would not execute. The response would be `400 Bad Request` with Problem Details containing the validation errors.

**What this connects to:** Topic 07 (FluentValidation).

---

## Step 8: Controller Action

The controller action now executes. Authentication passed, authorization passed, validation passed.

```csharp
[Authorize(Policy = "CanManageEmployees")]
[HttpPost]
public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
{
    _logger.LogInformation("Creating employee with name {EmployeeName}", dto.Name);

    var created = await _employeeService.CreateEmployeeAsync(dto);

    _logger.LogInformation("Employee created with ID {EmployeeId}", created.Id);

    return CreatedAtAction(
        actionName: nameof(GetById),
        routeValues: new { id = created.Id },
        value: created);
}
```

The controller does three things:

1. **Logs** the operation start (structured logging with the employee name as a property).
2. **Delegates** to the application service to perform the business logic.
3. **Logs** the operation result (the new employee's ID).

The controller does not access the database directly. It does not validate business rules. It handles HTTP concerns (reading the DTO, returning the appropriate response) and delegates everything else.

**What this connects to:** Topic 08 (Logging), Day 1 (Clean Architecture — API layer responsibility).

---

## Step 9: Application Service (Use Case)

The `EmployeeService.CreateEmployeeAsync` method runs. This is in the Application layer.

The service:

1. **Checks business rules** — for example, whether an employee with the same email already exists. This is a domain invariant, not an input format check. If the email is taken, the service returns a result indicating a conflict. The controller would return `409 Conflict`.

2. **Maps the DTO to a domain entity** — creates an `Employee` object with the provided values.

3. **Persists through the repository** — calls `_employeeRepository.AddAsync(employee)` and then `_unitOfWork.SaveChangesAsync(cancellationToken)`.

4. **Maps the entity back to a DTO** — returns an `EmployeeDto` to the controller.

If the database is unavailable, `SaveChangesAsync` throws an exception. The exception propagates up through the service, through the controller, and is caught by the global exception handler (Step 2). The handler logs the full exception and returns a safe `500 Internal Server Error` response.

**What this connects to:** Day 2 (Services and Use Cases, Repository Pattern, Unit of Work).

---

## Step 10: Infrastructure and Database Access

The repository uses EF Core to execute the database operation.

```csharp
public async Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken)
{
    await _context.Employees.AddAsync(employee, cancellationToken);
    await _context.SaveChangesAsync(cancellationToken);
    return employee;
}
```

EF Core translates this into a parameterized SQL `INSERT` statement. Parameterized queries prevent SQL injection — user input is never concatenated into the SQL string.

The database executes the query and returns the new employee's ID (generated by the database as an identity column).

**What this connects to:** Day 1 (Infrastructure layer, Repository Pattern), Topic 05 (SQL injection prevention).

---

## Step 11: Response Generation

The response flows back through the pipeline in reverse order:

1. The application service returns an `EmployeeDto` to the controller.
2. The controller returns `CreatedAtAction(...)`, which produces a `201 Created` response with a `Location` header pointing to the new resource.
3. The response passes through the authorization middleware (no changes needed).
4. The response passes through the authentication middleware (no changes needed).
5. The CORS middleware adds `Access-Control-Allow-Origin: https://app.example.com` to the response headers.
6. The response passes through the exception handler (no exception occurred, so no changes).
7. HTTPS encrypts the response for transport.

The final response:

```http
HTTP/1.1 201 Created
Content-Type: application/json
Location: https://api.example.com/api/employees/42
Access-Control-Allow-Origin: https://app.example.com

{
  "id": 42,
  "name": "Bob Smith",
  "email": "bob.smith@example.com",
  "salary": 65000,
  "departmentId": 2
}
```

---

## The Middleware Pipeline in Order

Here is the complete middleware pipeline as configured in `Program.cs`:

```csharp
var app = builder.Build();

// 1. Exception handler — wraps everything
app.UseExceptionHandler(...);

// 2. CORS — before authentication (handles preflight)
app.UseCors("AllowFrontend");

// 3. HTTPS enforcement
app.UseHttpsRedirection();

// 4. Authentication — establishes identity
app.UseAuthentication();

// 5. Authorization — checks permissions
app.UseAuthorization();

// 6. Routing — maps URL to controller action
app.MapControllers();

app.Run();
```

Each middleware has a specific responsibility and a specific position. The order is not arbitrary:

- **Exception handler first** — catches failures from all downstream middleware.
- **CORS before authentication** — preflight requests are anonymous and must not be rejected by authentication.
- **Authentication before authorization** — authorization needs the identity that authentication establishes.
- **Authorization before routing** — the controller action should not execute if the user is not authorized.

---

## What Happens When Things Go Wrong

The flow above is the happy path. Let us examine what happens when something fails.

### Scenario A: Expired Token

1. Exception handler — passes through.
2. CORS — passes through.
3. Authentication — token validation fails (expired). No `ClaimsPrincipal` is created.
4. The request reaches the `[Authorize]` attribute. No authenticated user. Returns `401 Unauthorized`.

The controller action never runs. No logs about employee creation. No database access.

### Scenario B: Valid Token, Insufficient Permissions

Alice has the role `Employee` instead of `HR`.

1. Exception handler — passes through.
2. CORS — passes through.
3. Authentication — token is valid. `ClaimsPrincipal` is created with role `Employee`.
4. Authorization — the `CanManageEmployees` policy requires `Admin` or `HR`. Alice has neither. Returns `403 Forbidden`.

The controller action never runs.

### Scenario C: Valid Token, Authorized, Invalid Input

The request body contains `"name": ""` and `"salary": -1000`.

1. Exception handler — passes through.
2. CORS — passes through.
3. Authentication — passes.
4. Authorization — passes.
5. FluentValidation — rules fail for `Name` (empty) and `Salary` (not > 0). Returns `400 Bad Request` with Problem Details.

The controller action never runs. The service never runs. The database is never touched.

### Scenario D: Everything Passes, But Database Is Down

1. Exception handler — passes through.
2. CORS — passes through.
3. Authentication — passes.
4. Authorization — passes.
5. FluentValidation — passes.
6. Controller — logs, delegates to service.
7. Service — calls repository.
8. Repository — `SaveChangesAsync` throws `SqlException` (cannot connect to database).
9. The exception propagates up through the service and controller.
10. Exception handler catches it. Logs the full exception with stack trace. Returns `500 Internal Server Error` with a safe Problem Details response.

The client sees:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An unexpected error occurred",
  "status": 500,
  "traceId": "00-abc123def456-789xyz-00"
}
```

The developer uses the `traceId` to find the full exception in the logs.

---

## The Complete Program.cs

Here is the full `Program.cs` showing all the middleware and service registrations from the previous topics, in the correct order:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EmployeeManagement.Api.Settings;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Infrastructure;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration and Secrets (Topic 09) ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

var settings = jwtSettings.Get<JwtSettings>()!;

// Validate configuration at startup
if (string.IsNullOrEmpty(settings.SecretKey) || settings.SecretKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT SecretKey must be configured and at least 32 characters.");
}

// --- Authentication (Topic 02) ---
var key = Encoding.UTF8.GetBytes(settings.SecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = settings.Issuer,
        ValidAudience = settings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// --- Authorization (Topic 04) ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("CanManageEmployees", policy =>
        policy.RequireRole("Admin", "HR"));
});

// --- CORS (Topic 10) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://app.example.com")
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Content-Type", "Authorization")
              .AllowCredentials();
    });
});

// --- Infrastructure (Day 1) ---
builder.Services.AddInfrastructure(builder.Configuration);

// --- FluentValidation (Topic 07) ---
builder.Services.AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<Program>();
        fv.AutomaticValidationEnabled = true;
    });

// --- Problem Details (Topic 06) ---
builder.Services.AddProblemDetails();

// --- Service registrations ---
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Middleware pipeline (order matters) ---

// 1. Exception handling — wraps everything
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.ContentType = "application/problem+json";
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        if (app.Environment.IsDevelopment())
        {
            var feature = context.Features.Get<IExceptionHandlerFeature>();
            if (feature != null)
            {
                problemDetails.Detail = feature.Error.Message;
                problemDetails.Instance = feature.Error.StackTrace;
            }
        }

        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

// 2. CORS — before authentication
app.UseCors("AllowFrontend");

// 3. HTTPS enforcement
app.UseHttpsRedirection();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// 4. Authentication — establishes identity
app.UseAuthentication();

// 5. Authorization — checks permissions
app.UseAuthorization();

// 6. Routing
app.MapControllers();

app.Run();
```

---

## Architecture Recap

Each concern belongs in a specific layer:

| Concern | Layer | Where |
|---|---|---|
| HTTPS enforcement | API | `Program.cs` middleware |
| Exception handling | API | `Program.cs` middleware |
| CORS | API | `Program.cs` middleware + service registration |
| Authentication | API | `Program.cs` middleware + JWT settings |
| Authorization | API | `Program.cs` middleware + policies |
| Input validation | API | FluentValidation validators in `Validators/` |
| Logging | Cross-cutting | `ILogger<T>` injected where needed |
| Configuration | API | `appsettings.json` + User Secrets + env vars |
| Token generation | Infrastructure | `TokenService` implementing `ITokenService` |
| Business rules | Application | `EmployeeService` |
| Data access | Infrastructure | `EmployeeRepository` with EF Core |
| Domain entities | Domain | `Employee`, `Department` |

The API layer handles all HTTP and framework concerns. The Application layer handles business logic. The Infrastructure layer handles external systems (database, token generation). The Domain layer defines business concepts.

---

## Key Takeaways

1. **Middleware order matters.** Each middleware depends on the ones before it. Exception handler first, then CORS, then authentication, then authorization, then routing.

2. **Each concern has a position.** Authentication must run before authorization. Validation must run before the controller action. Exception handling must wrap everything.

3. **The happy path is rare.** The real test of a pipeline is how it handles failures: expired tokens, insufficient permissions, invalid input, database errors. Each failure should be caught at the right layer and produce the correct response.

4. **The layers protect each other.** Validation rejects bad input before it reaches business logic. Authentication rejects anonymous requests before they reach authorization. Authorization rejects unauthorized requests before they reach the controller. Exception handling catches anything that slips through.

5. **Security is not one thing.** It is the combination of HTTPS, authentication, authorization, validation, safe error handling, secure configuration, CORS, logging, and dependency management. Removing any one layer weakens the others.

---

## What Comes Next

You now understand the complete security and professional practices pipeline. The EmployeeManagement API has:

- JWT authentication with proper token validation
- Role-based and policy-based authorization
- Input validation with FluentValidation
- Global exception handling with safe error responses
- Structured logging with correlation IDs
- Secure configuration management
- CORS for cross-origin browser requests
- A defense-in-depth security posture

The next step is practice. Apply these concepts to the EmployeeManagement solution. Add authentication to the existing controllers. Create authorization policies. Write validators. Configure exception handling. Set up logging. The patterns are in these topics. The codebase is your laboratory.
