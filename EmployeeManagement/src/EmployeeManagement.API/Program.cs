// ============================================================================
// Program.cs — Application entry point and middleware pipeline configuration
// ============================================================================
//
// This file is the composition root of the ASP.NET Core application.
// It is responsible for two things:
//
//   1. Building the service container (dependency injection registrations).
//   2. Configuring the HTTP request pipeline (middleware order).
//
// In a Clean Architecture application, this file is the only place in the
// API layer where Infrastructure implementations are wired to Application
// interfaces. The API project knows about both Application (for interfaces
// and DTOs) and Infrastructure (for concrete implementations), but the
// Application project itself never references Infrastructure directly.
//
// Authentication architecture:
//   - ASP.NET Core Identity manages user accounts and password hashing.
//   - JWT Bearer tokens authenticate API requests.
//   - Role-based authorization protects endpoints.
//   - The signing key is read from an environment variable (never committed).
// ============================================================================

using System.Text;
using EmployeeManagement.Application;
using EmployeeManagement.Infrastructure;
using EmployeeManagement.Infrastructure.Identity;
using EmployeeManagement.Infrastructure.Persistence;
using EmployeeManagement.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

// WebApplication.CreateBuilder sets up the default configuration sources
// (appsettings.json, environment variables, user secrets) and creates
// a WebApplicationBuilder with sensible defaults for a web API.
var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Service registration — this is where we tell ASP.NET Core which services
// are available for dependency injection throughout the application.
// ---------------------------------------------------------------------------

// Bind JwtSettings from configuration for use in JWT validation setup.
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);

// Read the signing key from configuration (environment variable or User Secrets).
// This key is the same one used by JwtTokenService to sign tokens.
var signingKey = builder.Configuration["JwtSettings:SigningKey"]
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_JWT_SIGNING_KEY")
    ?? string.Empty;

// Register ApplicationDbContext with the DI container.
// The Identity tables (AspNetUsers, AspNetRoles, etc.) are stored in the
// same database alongside Employees and Departments.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Infrastructure-layer services: repositories, Identity, and auth.
// This single call registers ApplicationDbContext (already done above, but
// Identity also needs it), UserManager, RoleManager, repositories, and
// authentication services.
builder.Services.AddInfrastructure();

// Register Application-layer services (EmployeeService, DepartmentService).
builder.Services.AddApplication();

// ---------------------------------------------------------------------------
// JWT Bearer authentication configuration
// ---------------------------------------------------------------------------
// This tells ASP.NET Core how to validate incoming JWT tokens.
// When a request arrives with "Authorization: Bearer <token>", the
// authentication middleware validates the token using these settings.
builder.Services.AddAuthentication(options =>
{
    // The default authentication scheme is "Bearer" (JWT Bearer tokens).
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // The token validation parameters define what the server checks when
    // it receives a JWT. All four validations must pass for the token
    // to be considered valid.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validate the cryptographic signature. This proves the token was
        // issued by our server and has not been tampered with.
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(signingKey)),

        // Validate the issuer. The token's "iss" claim must match our
        // configured issuer. This prevents tokens from other services
        // from being accepted.
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,

        // Validate the audience. The token's "aud" claim must match our
        // configured audience. This ensures the token was intended for
        // this API.
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,

        // Validate the expiration. Expired tokens are rejected.
        // The server clock must be reasonably accurate for this to work.
        ValidateLifetime = true,

        // Map the "role" claim from the JWT to ClaimTypes.Role.
        // Without this, [Authorize(Roles = "Admin")] would not work because
        // the JWT uses "role" (lowercase) while ASP.NET Core expects
        // "http://schemas.microsoft.com/ws/2008/06/identity/claims/role".
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };
});

// ---------------------------------------------------------------------------
// Authorization policies
// ---------------------------------------------------------------------------
// Named policies make authorization rules explicit and reusable.
// Instead of repeating role names across controllers, define them once here.
builder.Services.AddAuthorization(options =>
{
    // AdminOnly policy: requires the "Admin" role.
    // Applied to all Employee and Department CRUD endpoints.
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // AnyAuthenticated policy: requires any authenticated user (any role).
    // Available for future use when some endpoints should be accessible
    // to all authenticated users, not just Admins.
    options.AddPolicy("AnyAuthenticated", policy =>
        policy.RequireAuthenticatedUser());
});

// ---------------------------------------------------------------------------
// CORS configuration
// ---------------------------------------------------------------------------
// CORS (Cross-Origin Resource Sharing) controls which browser origins can
// call this API. Without CORS, a browser would block JavaScript from a
// frontend at http://localhost:3000 from reading API responses.
//
// The allowed origins are read from appsettings.json's "CorsSettings"
// section. In production, this should list only the actual frontend URL(s).
// Using "*" (allow all) with credentials is insecure and not permitted.
var allowedOrigins = builder.Configuration
    .GetSection("CorsSettings:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // WithOrigins restricts access to specific, trusted origins.
        // Each origin must include the scheme (http/https) and port.
        policy.WithOrigins(allowedOrigins)
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .WithHeaders("Authorization", "Content-Type")
            .AllowCredentials();
    });
});

// AddControllers registers the MVC services needed to discover and invoke
// controller actions, perform model binding, content negotiation, and
// JSON serialization.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger to include JWT Bearer authentication support.
// This adds an "Authorize" button in Swagger UI where users can paste
// their JWT token for testing protected endpoints.
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token. Example: eyJhbGciOiJIUzI1NiIs..."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ---------------------------------------------------------------------------
// Middleware pipeline — the order of these calls matters.
// ---------------------------------------------------------------------------

var app = builder.Build();

// 1. Exception handling — placed first so it wraps all subsequent middleware.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. Swagger — available in Development for interactive testing.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. HTTPS redirection — ensures all traffic is encrypted.
app.UseHttpsRedirection();

// 4. CORS — must run before authentication so that preflight OPTIONS
// requests (which are anonymous) are handled correctly. If CORS ran
// after authentication, preflight requests would fail with 401 because
// they do not carry a JWT token.
app.UseCors("AllowFrontend");

// 5. Authentication — reads and validates JWT tokens from the Authorization header.
// Must come BEFORE authorization so that the user identity is available.
app.UseAuthentication();

// 6. Authorization — checks whether the authenticated user has permission
// to access the requested endpoint based on [Authorize] attributes and policies.
app.UseAuthorization();

// 7. Map controllers — discovers and maps all [ApiController] endpoints.
app.MapControllers();

// ---------------------------------------------------------------------------
// Role initialization — ensure required roles exist before serving requests.
// ---------------------------------------------------------------------------
// This runs once at startup. It creates the "Admin" and "Employee" roles
// if they do not exist, and optionally creates a bootstrap admin user if
// the ADMIN_USERNAME, ADMIN_EMAIL, and ADMIN_PASSWORD environment variables
// are all set.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    await RoleInitializer.InitializeAsync(roleManager, userManager, logger);

    // Development-only test account seeding.
    // Creates the admin and user1 accounts when their credentials are
    // configured via User Secrets or environment variables. This runs ONLY
    // in the Development environment and does not affect production.
    if (app.Environment.IsDevelopment())
    {
        var config = services.GetRequiredService<IConfiguration>();
        await DevelopmentSeeder.SeedAsync(userManager, config, logger);
    }
}

// Start listening for HTTP requests.
app.Run();

// ---------------------------------------------------------------------------
// Make the implicit Program class visible to the integration test project.
// ---------------------------------------------------------------------------
public partial class Program { }
