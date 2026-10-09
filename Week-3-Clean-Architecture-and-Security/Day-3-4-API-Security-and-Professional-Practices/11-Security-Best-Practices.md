# 11 — Security Best Practices

**Duration**: 30 minutes

---

## Introduction

This topic consolidates the security practices covered throughout this module and adds additional considerations. Authentication, authorization, error handling, configuration, and CORS each address a specific threat. Together, they form a layered defense.

No single middleware, attribute, or setting makes an API secure. Security is the result of multiple layers working together. If one layer fails, others should still protect the system.

---

## HTTPS

Always use HTTPS in production. HTTPS encrypts data in transit, preventing eavesdropping and man-in-the-middle attacks. Without HTTPS, JWT tokens travel in plain text. Passwords travel in plain text. Employee data travels in plain text.

```csharp
app.UseHttpsRedirection();
```

This middleware redirects HTTP requests to HTTPS. In production, you should also configure HSTS (HTTP Strict Transport Security) to tell browsers to always use HTTPS:

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
```

During development, HTTPS is optional. In production, it is mandatory.

---

## Strong Token Validation

When configuring JWT authentication, use strong validation settings. The defaults in ASP.NET Core are a good starting point, but verify they are all enabled:

```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = jwtSettings.Issuer,
    ValidAudience = jwtSettings.Audience,
    IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
    ClockSkew = TimeSpan.FromMinutes(1)
};
```

Every validation flag matters:

- **ValidateIssuer** — ensures the token was issued by your API, not by an attacker's server
- **ValidateAudience** — ensures the token was intended for your API
- **ValidateLifetime** — ensures the token has not expired
- **ValidateIssuerSigningKey** — ensures the token was signed with your key, not forged
- **ClockSkew** — a small tolerance for clock differences between servers (default is 5 minutes; reduce if possible)

Use signing keys that are at least 32 characters long for HS256. A short key is vulnerable to brute-force attacks.

---

## Secret Management

Secrets include JWT signing keys, database connection strings with passwords, third-party API keys, and encryption keys.

**Development:**

- Use .NET User Secrets
- Secrets stored locally, outside the project directory
- Never committed to source control

**Production:**

- Use Azure Key Vault, AWS Secrets Manager, HashiCorp Vault, or environment variables
- Secrets injected at runtime by the deployment pipeline
- Rotated periodically without code changes

**Rules:**

- Never commit secrets to source control (not in code, not in config files, not in comments)
- Never hardcode secrets
- Never use the same secrets across environments
- Rotate secrets on a schedule and immediately if compromised

---

## Least Privilege

Grant only the permissions each user or role needs. Avoid making everyone an Admin.

- **Employees** can view their own data
- **Managers** can view and edit their team's data
- **HR** can manage all employee records
- **Admins** can manage users and system settings

Use fine-grained authorization policies rather than broad role assignments. A policy named `CanViewSalaries` is more specific and more maintainable than a blanket `Admin` role on every endpoint.

```csharp
[Authorize(Policy = "CanManageEmployees")]
[HttpDelete("{id:int}")]
public async Task<IActionResult> Delete(int id) { }
```

---

## Endpoint Authorization

Protect all sensitive endpoints. If an endpoint should not be accessible to anonymous users, add the `[Authorize]` attribute.

```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    // All endpoints require authentication

    [Authorize(Policy = "CanManageEmployees")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) { }
}
```

Consider applying `[Authorize]` at the controller level and using `[AllowAnonymous]` only for specific endpoints that should be public (like login).

Unprotected endpoints are the most common authorization vulnerability. Review every controller and every action. Ask: "Should this really be public?"

---

## Input Validation

Validate all input before processing it. Use FluentValidation or data annotations. Reject invalid input early with clear error messages.

```csharp
public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(DateTime.Today);
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0);
    }
}
```

Input validation prevents:

- Invalid data entering the database
- SQL injection (when combined with parameterized queries via EF Core)
- Buffer overflows and other injection attacks
- Business logic errors from malformed data

---

## Safe Exception Responses

Never expose stack traces, SQL errors, or internal implementation details in production responses. These details help attackers understand your system and find vulnerabilities.

Use Problem Details (RFC 7807) with safe messages:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(options =>
    {
        options.AllowStatusCode404Response = true;
    });
}
else
{
    app.UseExceptionHandler("/error");
}
```

In development: detailed errors help you debug.
In production: generic error messages protect your internals.

```json
// Development — detailed
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "The FirstName field is required.",
  "traceId": "0HN5MQP3V4GQJ:00000001"
}

// Production — safe
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Bad Request",
  "status": 400,
  "detail": "Validation failed. Check your input and try again."
}
```

---

## Sensitive Data Handling

### Never Log Sensitive Data

```csharp
// BAD — logging secrets
_logger.LogInformation($"User logged in with token: {token}");
_logger.LogInformation($"User password: {password}");

// GOOD — log the event, not the secret
_logger.LogInformation("User {UserId} logged in successfully", userId);
```

### Never Return Sensitive Data in API Responses

```csharp
// BAD — returning password hash
return new { user.Email, user.PasswordHash };

// GOOD — return only what the client needs
return new { user.Email, user.FullName };
```

### Additional Guidelines

- Encrypt sensitive data at rest if required by compliance (GDPR, HIPAA, etc.)
- Use HTTPS to protect data in transit
- Exclude sensitive properties from serialization with `[JsonIgnore]`
- Be careful with navigation properties — EF Core can eagerly load related data you did not intend to expose

---

## Logging Hygiene

Logging is essential for debugging and monitoring, but poorly managed logs become a security liability.

### Do

- Log important events: authentication successes and failures, authorization failures, data modifications
- Use structured logging with message templates
- Use appropriate log levels (`Debug` in development, `Information` or higher in production)
- Protect log files with appropriate file system permissions
- Include correlation IDs to trace requests across services

```csharp
_logger.LogInformation("Employee {EmployeeId} updated by {UserId}", employeeId, userId);
```

### Do Not

- Log passwords, tokens, secrets, or connection strings
- Log full request/response bodies in production (they may contain sensitive data)
- Log at `Debug` or `Trace` level in production (performance and storage impact)
- Store logs indefinitely without a retention policy

---

## Secure Configuration

- Use strongly typed options classes (see Topic 09)
- Validate required settings at startup
- Use environment-specific configuration files and environment variables
- Never hardcode secrets or connection strings
- Keep `appsettings.json` free of sensitive data

A well-configured application fails fast when something is wrong and cannot be tricked into running with missing or invalid settings.

---

## CORS Configuration

- Use explicit origins in production (never `AllowAnyOrigin`)
- Do not combine `AllowAnyOrigin` with `AllowCredentials`
- Place CORS middleware **before** authentication and authorization
- Remember: CORS is a browser restriction, not a security layer

CORS protects browser users from cross-origin attacks. It is not a substitute for authentication and authorization. See Topic 10 for detailed guidance.

---

## Security Headers

Security headers are HTTP response headers that instruct browsers to apply additional security measures. They are not implemented in this course, but you should be aware of them:

- **X-Content-Type-Options: nosniff** — prevents browsers from MIME-type sniffing, reducing drive-by download attacks
- **X-Frame-Options: DENY** — prevents clickjacking by disallowing the page from being embedded in iframes
- **Content-Security-Policy** — controls which resources (scripts, styles, images) the browser is allowed to load
- **Strict-Transport-Security** — tells browsers to always use HTTPS for this domain (HSTS)
- **X-XSS-Protection** — enables browser-level XSS filtering (legacy, but still useful for older browsers)

These headers can be added with middleware or via the `NWebsec` NuGet package. For APIs consumed only by non-browser clients, they are less relevant, but they matter when your API serves data rendered in a browser.

---

## Dependency Maintenance

Your application depends on NuGet packages. Those packages may contain security vulnerabilities. Keeping dependencies up to date is a security practice, not just good housekeeping.

### Check for Vulnerabilities

```bash
dotnet list package --vulnerable
```

This command reports packages with known vulnerabilities. Review the output and update affected packages promptly.

### General Practices

- Update NuGet packages regularly, especially after security advisories
- Monitor the [GitHub Advisory Database](https://github.com/advisories) or use a tool like Dependabot
- Test after updating — a newer version may introduce breaking changes
- Remove unused packages — fewer dependencies mean fewer potential vulnerabilities

---

## Production vs Development

Use different settings for different environments:

| Setting | Development | Production |
|---------|-------------|------------|
| Errors | Detailed (Developer Exception Page) | Safe (generic messages) |
| CORS | Permissive (AllowAnyOrigin or localhost) | Restrictive (explicit origins) |
| Logging | Debug / Information | Warning / Error |
| HTTPS | Optional | Required |
| Secrets | User Secrets | Key Vault / Environment Variables |
| Database | LocalDB | Production SQL Server |
| HSTS | Disabled | Enabled |

Never deploy development settings to production. Never use production secrets on developer machines.

---

## Avoiding Insecure Defaults

### Do Not Use Default Passwords

If your application creates default users, ensure they are forced to change the password on first login. Better: do not create default users at all. Use a registration flow.

### Do Not Use AllowAnyOrigin in Production

Every website on the internet becomes an allowed origin. Combine with credentials and you have handed access to everyone.

### Do Not Disable Token Validation

Setting `ValidateLifetime = false` means expired tokens work forever. Setting `ValidateIssuerSigningKey = false` means anyone can forge tokens. All validation flags should be `true`.

### Do Not Expose Internal Errors

Stack traces reveal your code structure, library versions, and file paths. SQL error messages reveal your schema. All of this helps attackers.

### Do Not Trust Client Input

Every value from a request is potentially malicious. Validate everything. Use parameterized queries (EF Core does this by default). Sanitize output if rendering in HTML.

---

## Security Is Layered

No single measure secures an API. Security is the result of multiple layers:

1. **HTTPS** — encrypts data in transit
2. **Authentication** — verifies identity
3. **Authorization** — enforces permissions
4. **Input validation** — rejects malicious input
5. **Safe error handling** — hides internal details
6. **Secure configuration** — protects secrets
7. **CORS** — restricts browser-based access
8. **Logging and monitoring** — detects and records suspicious activity
9. **Dependency maintenance** — closes known vulnerabilities
10. **Security headers** — adds browser-level protections

If one layer fails, others should still protect the system. An attacker who bypasses CORS still faces authentication. An attacker who obtains a token still faces authorization. An attacker who finds an injection point is blocked by input validation.

This is called **defense in depth**.

---

## Final Checklist

Before deploying to production, verify:

- [ ] HTTPS is enforced (`UseHttpsRedirection`, `UseHsts`)
- [ ] JWT signing key is at least 32 characters and stored securely
- [ ] Token validation is fully enabled (issuer, audience, lifetime, signing key)
- [ ] Secrets are not in source control (check git history)
- [ ] User Secrets are used for development; Key Vault or environment variables for production
- [ ] CORS allows only specific production origins
- [ ] CORS middleware is placed before authentication middleware
- [ ] All sensitive endpoints have `[Authorize]`
- [ ] Authorization policies follow least privilege
- [ ] Input validation is applied to all commands and DTOs
- [ ] Exception handling returns safe messages in production
- [ ] Sensitive data is excluded from logs
- [ ] Sensitive data is excluded from API responses
- [ ] Configuration is validated at startup
- [ ] Log level is set appropriately for production
- [ ] NuGet packages are up to date and free of known vulnerabilities
- [ ] Default users and passwords have been removed
- [ ] Database connection strings use integrated security or secure credentials
- [ ] `appsettings.Production.json` does not contain secrets
- [ ] `.gitignore` covers `appsettings.*.json` files with sensitive data

---

## Common Mistakes

### Mistake 1: Treating CORS as Security

CORS stops browsers from making cross-origin requests. It does not stop `curl`, Python scripts, or any non-browser client. Authentication and authorization are your actual security layers.

### Mistake 2: Committing Secrets to Source Control

Once a secret is in git, it is there forever — even if you delete it in a later commit. Git history retains it. Use User Secrets or environment variables.

### Mistake 3: Disabling Token Validation for Testing

Leaving `ValidateLifetime = false` in production means expired tokens never expire. This is a convenience during testing but a critical vulnerability in production. Use environment-specific configuration to keep validation enabled.

### Mistake 4: Returning Detailed Errors in Production

Stack traces, SQL errors, and exception messages reveal your system's internals. Use Problem Details with safe, generic messages in production.

### Mistake 5: Using AllowAnyOrigin with Credentials

This combination is rejected by ASP.NET Core for good reason. It would allow any website to make authenticated requests to your API. Always specify explicit origins when credentials are involved.

### Mistake 6: Placing CORS Middleware After Authentication

Preflight requests are anonymous. If authentication middleware runs first, preflight requests fail with `401 Unauthorized`. The browser then reports a CORS error. Place `UseCors` before `UseAuthentication`.

---

## Conclusion

Security is not a feature you add. It is a practice you follow throughout development and beyond. The configurations, middleware, and patterns covered in this module — authentication, authorization, error handling, configuration management, CORS — are the foundation. But security does not stop here.

Stay informed about new vulnerabilities. Monitor your dependencies. Review your authorization policies as business requirements change. Test your security measures regularly. Respond to incidents promptly.

An API is secure when multiple layers defend it, when secrets are protected, when input is validated, when errors are handled safely, and when the team remains vigilant. The work is never truly finished.
