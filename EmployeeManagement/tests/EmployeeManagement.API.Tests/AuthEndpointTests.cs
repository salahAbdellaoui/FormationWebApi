using System.Net;
using System.Net.Http.Json;
using EmployeeManagement.Application.DTOs;

namespace EmployeeManagement.API.Tests;

/// <summary>
/// Integration tests for the authentication endpoints (POST /api/auth/register and /api/auth/login).
/// These endpoints are anonymous and do not require a JWT token.
/// </summary>
public class AuthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuthEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // POST /api/auth/register with valid data should return 200 OK with a JWT token.
    [Fact]
    public async Task Register_WithValidData_ReturnsOk_WithToken()
    {
        var dto = new RegisterRequestDto
        {
            Username = "testuser_valid",
            Email = "valid@test.com",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.Equal("testuser_valid", result.Username);
    }

    // POST /api/auth/register with duplicate username should return 409 Conflict.
    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsConflict()
    {
        var dto = new RegisterRequestDto
        {
            Username = "duplicate_user",
            Email = "dup1@test.com",
            Password = "Password123!"
        };

        // Register first time.
        await _client.PostAsJsonAsync("/api/auth/register", dto);

        // Register second time with same username.
        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // POST /api/auth/register with empty username should return 400 Bad Request.
    [Fact]
    public async Task Register_WithInvalidData_ReturnsBadRequest()
    {
        var dto = new RegisterRequestDto
        {
            Username = "",
            Email = "invalid@test.com",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // POST /api/auth/login with valid credentials should return 200 OK with a JWT token.
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk_WithToken()
    {
        var registerDto = new RegisterRequestDto
        {
            Username = "loginuser_valid",
            Email = "login@test.com",
            Password = "Password123!"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new LoginRequestDto
        {
            Username = "loginuser_valid",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    // POST /api/auth/login with wrong password should return 401 Unauthorized.
    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var registerDto = new RegisterRequestDto
        {
            Username = "loginuser_invalid",
            Email = "loginfail@test.com",
            Password = "Password123!"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new LoginRequestDto
        {
            Username = "loginuser_invalid",
            Password = "WrongPassword!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // POST /api/auth/login with missing fields should return 400 Bad Request.
    [Fact]
    public async Task Login_WithMissingFields_ReturnsBadRequest()
    {
        var dto = new LoginRequestDto
        {
            Username = "",
            Password = ""
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
