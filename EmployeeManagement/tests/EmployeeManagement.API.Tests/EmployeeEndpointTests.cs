using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Infrastructure.Persistence;
using EmployeeManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagement.API.Tests;

/// <summary>
/// Integration tests for the Employee endpoints (GET/POST /api/employee).
/// These endpoints require Admin role authorization.
/// </summary>
public class EmployeeEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public EmployeeEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // GET /api/employee without a JWT token should return 401 Unauthorized.
    [Fact]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/employee");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // GET /api/employee with a non-Admin (Employee role) token should return 403 Forbidden.
    [Fact]
    public async Task GetAll_WithEmployeeRoleToken_ReturnsForbidden()
    {
        var token = await RegisterAndGetTokenAsync("emp_role_user", "emprole@test.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/employee");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    // GET /api/employee/{id} with non-existent ID should return 404 Not Found (admin token).
    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/employee/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    // GET /api/employee/{id} with valid ID should return 200 OK (admin token).
    // Seeds a department first via the DbContext, then creates an employee via the API.
    [Fact]
    public async Task GetById_WithValidId_ReturnsOk()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Seed a department via DbContext.
        var departmentId = await SeedDepartmentAsync("Test Dept GetById");

        var createDto = new CreateEmployeeDto
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@test.com",
            HireDate = DateTime.UtcNow.AddYears(-1),
            Salary = 50000,
            DepartmentId = departmentId
        };

        var createResponse = await _client.PostAsJsonAsync("/api/employee", createDto);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeResponseDto>();
        Assert.NotNull(created);

        var response = await _client.GetAsync($"/api/employee/{created.Id}");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<EmployeeResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("Doe", result.LastName);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    // POST /api/employee with invalid model (empty required fields) should return 400 Bad Request.
    [Fact]
    public async Task Create_WithInvalidModel_ReturnsBadRequest()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var dto = new CreateEmployeeDto
        {
            FirstName = "",
            LastName = "",
            Email = "",
            HireDate = default,
            Salary = -1,
            DepartmentId = 0
        };

        var response = await _client.PostAsJsonAsync("/api/employee", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    // POST /api/employee with valid data should return 201 Created (admin token).
    // Seeds a department via DbContext first.
    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var departmentId = await SeedDepartmentAsync("Test Dept Create");

        var dto = new CreateEmployeeDto
        {
            FirstName = "John",
            LastName = "Smith",
            Email = "john.smith@test.com",
            HireDate = DateTime.UtcNow.AddMonths(-6),
            Salary = 75000,
            DepartmentId = departmentId
        };

        var response = await _client.PostAsJsonAsync("/api/employee", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<EmployeeResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Smith", result.LastName);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    // Helper: register a user and return the JWT token.
    private async Task<string> RegisterAndGetTokenAsync(string username, string email)
    {
        var registerDto = new RegisterRequestDto
        {
            Username = username,
            Email = email,
            Password = "Password123!"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new LoginRequestDto
        {
            Username = username,
            Password = "Password123!"
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();
        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        return authResult!.Token;
    }

    // Helper: create an Admin user via UserManager and return a JWT token via login.
    private async Task<string> GetAdminTokenAsync()
    {
        const string adminUsername = "admin_test_user";
        const string adminEmail = "admin_test@test.com";
        const string adminPassword = "AdminPass123!";

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        // Ensure roles exist.
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole<int>("Admin"));
        if (!await roleManager.RoleExistsAsync("Employee"))
            await roleManager.CreateAsync(new IdentityRole<int>("Employee"));

        // Create admin user if not exists.
        var existing = await userManager.FindByNameAsync(adminUsername);
        if (existing is null)
        {
            var user = new ApplicationUser
            {
                UserName = adminUsername,
                Email = adminEmail
            };
            var result = await userManager.CreateAsync(user, adminPassword);
            Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(user, "Employee");
            await userManager.AddToRoleAsync(user, "Admin");
        }

        // Login to get token.
        var loginDto = new LoginRequestDto
        {
            Username = adminUsername,
            Password = adminPassword
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();
        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        return authResult!.Token;
    }

    // Helper: seed a department directly into the DbContext and return its ID.
    private async Task<int> SeedDepartmentAsync(string name)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var department = new EmployeeManagement.Domain.Entities.Department(name, null);
        db.Departments.Add(department);
        await db.SaveChangesAsync();

        return department.Id;
    }
}
