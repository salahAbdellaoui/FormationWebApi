# 02 — JWT Authentication

**Duration**: Implementation focus

---

## What Is JWT?

JWT stands for **JSON Web Token** (RFC 7519). It is a compact, URL-safe token that represents a set of claims between two parties. The server issues it, the client presents it, and the server validates it — without needing to look up the token in a database.

A JWT is a single string composed of three parts separated by dots:

```text
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

Each part is Base64Url-encoded. The three parts are:

1. **Header** — algorithm and token type
2. **Payload** — claims (data about the user)
3. **Signature** — proves the token has not been tampered with

---

## JWT Structure

```mermaid
graph LR
    A[Header] -->|Base64Url| B[eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJd]
    C[Payload] -->|Base64Url| D[eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ]
    E[Signature] -->|HMACSHA256| F[SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c]
    B --> G[JWT Token]
    D --> G
    F --> G
```

### Header

```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

Declares the signing algorithm (HS256 = HMAC-SHA256) and the token type. This part tells the server how to verify the signature.

### Payload

```json
{
  "sub": "1234567890",
  "name": "John Doe",
  "iat": 1516239022
}
```

Contains **claims** — statements about the user. Common claims:

| Claim | Meaning |
|---|---|
| `sub` | Subject — the user's identifier |
| `name` | User's display name |
| `iat` | Issued at — when the token was created |
| `exp` | Expiration — when the token becomes invalid |
| `iss` | Issuer — who created the token |
| `aud` | Audience — who the token is intended for |
| `role` | User's role (custom claim) |

### Signature

```text
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  secretKey
)
```

The signature is computed from the header and payload using the secret key. If anyone modifies the header or payload after signing, the signature becomes invalid. The server recalculates the signature and compares it to the one in the token.

---

## How JWT Authentication Works

The complete flow from login to protected API access:

```mermaid
sequenceDiagram
    participant Client
    participant Server

    Client->>Server: POST /api/auth/login (username, password)
    Server->>Server: Validate credentials against database
    Server->>Server: Generate JWT (claims: userId, roles, expiry)
    Server->>Server: Sign JWT with secret key
    Server-->>Client: { "token": "eyJhbG..." }

    Note over Client: Store token in memory

    Client->>Server: GET /api/employees<br>Authorization: Bearer eyJhbG...
    Server->>Server: Validate signature (recalculate with secret key)
    Server->>Server: Validate expiry (exp claim)
    Server->>Server: Validate issuer (iss claim)
    Server->>Server: Validate audience (aud claim)
    Server->>Server: Extract claims → ClaimsPrincipal
    Server->>Server: Check [Authorize] requirements
    Server-->>Client: 200 OK (employee data)
```

Step by step:

1. **Client sends credentials** — POST to `/api/auth/login` with username and password
2. **Server validates credentials** — checks against the user database (or identity provider)
3. **Server generates JWT** — creates a token with claims: user ID, username, roles, issue time, expiry time
4. **Server signs JWT** — uses the secret key to compute the signature
5. **Server returns JWT** — sends the token back in the response body
6. **Client stores JWT** — keeps it in memory, localStorage, or a secure cookie
7. **Client sends JWT in requests** — adds `Authorization: Bearer <token>` header to every request
8. **Server validates the token** — checks signature, expiry, issuer, audience
9. **Server extracts claims** — builds a `ClaimsPrincipal` from the token's claims
10. **Controller accesses the user** — uses `User.Identity` and `User.FindFirst()` to read claims

The server does not store the token. It does not need a database lookup for each request. The signature proves authenticity. This is what makes JWT **stateless** — the server does not maintain session state.

---

## Why Signed, Not Encrypted

This is the most common misunderstanding about JWT.

The payload is **Base64Url-encoded**, not encrypted. Anyone who intercepts the token can decode it and read every claim:

```text
eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ
```

Decoded:

```json
{
  "sub": "1234567890",
  "name": "John Doe",
  "iat": 1516239022
}
```

No secret required. No special tools. Base64 is an encoding, not encryption. You can decode it in a browser console: `atob("eyJzdWIiOiIxMjM0NTY3ODkwIn0")`.

### What the signature provides

The signature guarantees **integrity** — the token has not been modified since it was signed. If an attacker changes the `role` claim from `"Employee"` to `"Admin"`, the signature becomes invalid and the server rejects the token.

### What the signature does NOT provide

The signature does not provide **confidentiality**. It does not hide the content. Anyone with the token can read it.

### The rule

**Never put sensitive data in a JWT payload.** No passwords, no credit card numbers, no social security numbers. Put user ID, username, roles, and expiry — information that helps the server identify and authorize the user, but that does not cause harm if exposed.

If you need confidentiality, use JWE (JSON Web Encryption). But for most API authentication scenarios, JWS (JSON Web Signature) with non-sensitive claims is sufficient. HTTPS provides transport-layer encryption anyway.

---

## Implementation

### Step 1: Install the package

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

This package provides the JWT Bearer authentication handler for ASP.NET Core. It is the standard package for JWT authentication and is maintained by Microsoft.

For the target framework `net8.0`, use version `8.0.*` to match your other packages.

### Step 2: Add JWT settings to appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=EmployeeManagement;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "EmployeeManagementApi",
    "Audience": "EmployeeManagementClient",
    "ExpiryInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Security warning**: This secret key is in `appsettings.json` for teaching purposes only. In development, move it to **User Secrets**. In production, use **Azure Key Vault**, **AWS Secrets Manager**, or **environment variables**. Never commit real secrets to source control.

The secret key must be at least 32 characters for HS256 (HMAC-SHA256). Longer is better. Use a random string — not a word or phrase.

### Step 3: Create a JwtSettings class

```csharp
namespace EmployeeManagement.Api.Settings;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryInMinutes { get; set; }
}
```

This class maps to the `JwtSettings` section in `appsettings.json`. Using a strongly-typed settings class avoids string-based configuration access scattered throughout the code.

### Step 4: Register authentication in Program.cs

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EmployeeManagement.Api.Settings;

var builder = WebApplication.CreateBuilder(args);

// Bind JWT settings from configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

// Configure JWT authentication
var settings = jwtSettings.Get<JwtSettings>()!;
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

// Add authorization services
builder.Services.AddAuthorization();

// ... rest of service registration (controllers, DbContext, services, etc.)

var app = builder.Build();

// Middleware order matters
app.UseAuthentication();
app.UseAuthorization();

// ... rest of middleware pipeline
```

Key points:

- `DefaultAuthenticateScheme` and `DefaultChallengeScheme` tell ASP.NET Core to use JWT Bearer by default for all authentication checks
- `ValidateIssuer` — ensures the token was issued by *this* server, not a different one
- `ValidateAudience` — ensures the token was intended for *this* API, not a different service
- `ValidateLifetime` — ensures the token has not expired
- `ValidateIssuerSigningKey` — ensures the token was signed with *this* server's secret key
- All four validations should be `true` in production. Never disable them.

### Step 5: Create a token generation service

Define the interface in the Application layer:

```csharp
namespace EmployeeManagement.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(string userId, string username, IEnumerable<string> roles);
}
```

Implement in the Infrastructure layer (or Api layer for teaching):

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using EmployeeManagement.Api.Settings;
using EmployeeManagement.Application.Interfaces;

namespace EmployeeManagement.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(string userId, string username, IEnumerable<string> roles)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        }
        .Concat(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

What each claim does:

| Claim | Purpose |
|---|---|
| `ClaimTypes.NameIdentifier` | Unique user ID — used to identify the user in the system |
| `ClaimTypes.Name` | Username — used for display and logging |
| `JwtRegisteredClaimNames.Jti` | Unique token ID — helps with token revocation scenarios |
| `ClaimTypes.Role` | User's role — used for authorization checks |

Register the service in the DI container:

```csharp
builder.Services.AddScoped<ITokenService, TokenService>();
```

### Step 6: Create a login endpoint

```csharp
using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Application.Interfaces;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public AuthController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // WARNING: Teaching code. Not production-ready.
        // A real application validates credentials against a user database
        // with hashed passwords (ASP.NET Identity, custom hashing, etc.)
        if (request.Username == "admin" && request.Password == "password")
        {
            var token = _tokenService.GenerateToken(
                userId: "1",
                username: "admin",
                roles: new[] { "Admin" }
            );
            return Ok(new { Token = token });
        }

        if (request.Username == "employee" && request.Password == "password")
        {
            var token = _tokenService.GenerateToken(
                userId: "2",
                username: "employee",
                roles: new[] { "Employee" }
            );
            return Ok(new { Token = token });
        }

        return Unauthorized();
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

The hardcoded credentials are intentional — this is teaching code. In production, you would:

- Store users in a database (ASP.NET Identity or custom user table)
- Hash passwords with a strong algorithm (bcrypt, PBKDF2, Argon2)
- Never compare plain-text passwords
- Use constant-time comparison to prevent timing attacks

### Step 7: Protect endpoints

Add `[Authorize]` to the `EmployeesController`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        return Ok(employee);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
    {
        var employee = await _employeeService.CreateEmployeeAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }
}
```

What changed:

- `[Authorize]` at the controller level requires authentication for all endpoints
- `[Authorize(Roles = "Admin")]` on `Create` requires the Admin role in addition to authentication
- `GetAll` and `GetById` require authentication but any authenticated role can access them

---

## Testing the Flow

### Step 1: Login to get a token

```bash
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "password"}'
```

Response:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImp0aSI6IjNkZjI4YjQxLTVlNGItNGU5Zi1hMGM2LTM1ZjY4MjAxNzI5MiIsImV4cCI6MTY5Njg1NjQwMCwiaXNzIjoiRW1wbG95ZWVNYW5hZ2VtZW50QXBpIiwiYXVkIjoiRW1wbG95ZWVNYW5hZ2VtZW50Q2xpZW50In0.signature_here"
}
```

### Step 2: Use the token in a request

```bash
curl https://localhost:7001/api/employees \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

Response: `200 OK` with employee data.

### Step 3: Request without a token

```bash
curl https://localhost:7001/api/employees
```

Response: `401 Unauthorized`.

### Step 4: Request with an Employee token on an Admin-only endpoint

```bash
curl -X POST https://localhost:7001/api/employees \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <employee_token>" \
  -d '{"name": "New Employee", "email": "new@example.com", "salary": 50000, "departmentId": 1}'
```

Response: `403 Forbidden`.

### Using Postman

1. Send POST to `/api/auth/login` with username/password in the body
2. Copy the token from the response
3. Go to the Authorization tab on the next request
4. Select "Bearer Token" as the type
5. Paste the token
6. Send the request

---

## Token Validation

When a request arrives with a Bearer token, the JWT Bearer middleware performs four checks:

### 1. Signature validation

```text
Received token: header.payload.signature
Recalculate:    HMACSHA256(header + "." + payload, secretKey)
Compare:        calculated signature == received signature?
```

If the signatures do not match, the token was tampered with. Reject with `401`.

### 2. Issuer validation

```text
Token "iss" claim: "EmployeeManagementApi"
Expected issuer:   "EmployeeManagementApi"
Match?
```

If the token was issued by a different service, reject it. This prevents tokens from one environment or service from being used in another.

### 3. Audience validation

```text
Token "aud" claim: "EmployeeManagementClient"
Expected audience:  "EmployeeManagementClient"
Match?
```

If the token was not intended for this API, reject it. This matters when multiple APIs share the same identity provider.

### 4. Lifetime validation

```text
Token "exp" claim: 1696856400 (Unix timestamp)
Current time:       1696860000
Expired?
```

If the token has expired, reject with `401`. The client must obtain a new token.

All four validations are enabled by default when you set `Validate*` to `true` in `TokenValidationParameters`. Never disable them in production.

---

## Access Tokens vs Refresh Tokens

### Access token

- Short-lived: minutes to hours (60 minutes is typical)
- Sent with every API request in the `Authorization` header
- Contains claims about the user (ID, roles, expiry)
- Stateless: the server does not store it

### Refresh token

- Long-lived: days to weeks
- Used only to request a new access token when the current one expires
- Stored server-side (database) — can be revoked
- Opaque: not a JWT, just a random string

### The flow with refresh tokens

```mermaid
sequenceDiagram
    participant Client
    participant Server

    Client->>Server: POST /api/auth/login
    Server-->>Client: { accessToken, refreshToken }

    Client->>Server: GET /api/employees<br>Authorization: Bearer <accessToken>
    Server-->>Client: 200 OK

    Note over Client: Access token expires

    Client->>Server: POST /api/auth/refresh<br>{ refreshToken }
    Server->>Server: Validate refresh token, check revocation
    Server-->>Client: { newAccessToken, newRefreshToken }

    Client->>Server: GET /api/employees<br>Authorization: Bearer <newAccessToken>
    Server-->>Client: 200 OK
```

### For this teaching application

We use **access tokens only**. The 60-minute expiry is short enough that users simply log in again when the token expires. Refresh tokens add complexity: token storage, rotation, revocation lists, and concurrent request handling. These are production concerns that belong in a later module.

---

## Security Considerations

### Never log tokens

Tokens contain user identity information. Logging them creates a security liability. If logs are compromised, attacker has valid credentials.

```csharp
// BAD
_logger.LogInformation("User logged in, token: {Token}", token);

// GOOD
_logger.LogInformation("User {Username} logged in successfully", username);
```

### Use HTTPS in production

JWT tokens travel in the `Authorization` header. Without HTTPS, the token is transmitted in plain text. An attacker on the network can intercept it and impersonate the user.

In development, HTTPS is optional (localhost is trusted). In production, HTTPS is mandatory. Enforce it with:

```csharp
app.UseHttpsRedirection();
```

### Store tokens securely on the client

- **In-memory**: safest for SPAs — token is lost on page refresh, but immune to XSS
- **httpOnly cookie**: protects against XSS, but requires CSRF protection
- **localStorage**: vulnerable to XSS — if an attacker can run JavaScript on your page, they can read the token

### Short expiry times

60 minutes is a reasonable default. For high-security applications, 5-15 minutes is better. The shorter the expiry, the smaller the window for a stolen token to be useful.

### Strong signing keys

The signing key is the foundation of JWT security. If an attacker obtains it, they can forge any token. Requirements:

- At least 32 characters (256 bits) for HS256
- Randomly generated (not a word or phrase)
- Stored securely (User Secrets, Key Vault, environment variables)
- Rotated periodically

### Validate everything

Every validation parameter exists for a reason:

| Parameter | If disabled |
|---|---|
| `ValidateIssuer` | Tokens from other services are accepted |
| `ValidateAudience` | Tokens intended for other APIs are accepted |
| `ValidateLifetime` | Expired tokens are accepted forever |
| `ValidateIssuerSigningKey` | Tokens signed with any key are accepted |

Disabling any of these creates a security hole.

---

## Common Mistakes

### 1. Hardcoding the secret key in source code

```csharp
// BAD - committed to git, visible to everyone
var key = Encoding.UTF8.GetBytes("my-secret-key-123");
```

Use User Secrets in development (`dotnet user-secrets set "JwtSettings:SecretKey" "..."`) and Key Vault or environment variables in production. The secret must never appear in source control.

### 2. Disabling validation for convenience

```csharp
// BAD - accepts any token, including expired ones from other services
ValidateIssuer = false,
ValidateAudience = false,
ValidateLifetime = false,
```

This turns JWT authentication into "trust whatever string the client sends." Always validate all four parameters.

### 3. Putting sensitive data in the payload

```json
{
  "userId": "1",
  "name": "John",
  "ssn": "123-45-6789",
  "creditCard": "4111-1111-1111-1111"
}
```

The payload is Base64-encoded, not encrypted. Anyone can read it. Never put passwords, SSNs, credit card numbers, or other sensitive data in a JWT payload.

### 4. Not handling expired tokens gracefully

When a token expires, the API returns `401`. The client should catch this and prompt the user to log in again (or use a refresh token). If the client ignores the `401` and keeps retrying, the user gets stuck in an error loop.

### 5. Using symmetric keys without understanding the trade-off

HS256 uses a shared secret — the same key signs and verifies tokens. This works for a single server. If you have multiple servers (load balancer, microservices), every server needs the same key. An alternative is RS256 (asymmetric): the signing server has a private key, and verifying servers have a public key. RS256 is better for distributed systems but adds key management complexity.
