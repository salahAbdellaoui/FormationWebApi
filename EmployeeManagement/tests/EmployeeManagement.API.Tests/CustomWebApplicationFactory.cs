using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EmployeeManagement.Infrastructure.Persistence;

namespace EmployeeManagement.API.Tests;

/// <summary>
/// Custom WebApplicationFactory that replaces SQL Server with SQLite in-memory
/// and provides test JWT configuration for integration tests.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Keep the SQLite connection alive for the lifetime of the factory.
    // SQLite in-memory databases exist only while the connection is open.
    private SqliteConnection? _connection;

    // Test signing key — must be at least 32 characters for HMAC-SHA256.
    private const string TestSigningKey = "TestSigningKeyThatIsAtLeast32CharactersLong!";

    public CustomWebApplicationFactory()
    {
        // Set env var so Program.cs top-level code picks it up at startup.
        Environment.SetEnvironmentVariable("ASPNETCORE_JWT_SIGNING_KEY", TestSigningKey);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Open a persistent in-memory SQLite connection.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override CORS with empty origins to avoid production URL issues in tests.
            // JwtSettings (Issuer, Audience, ExpiryInMinutes) intentionally NOT overridden:
            // both Program.cs (validation) and JwtTokenService (generation) read from the
            // same IConfiguration, so appsettings.json values stay consistent.
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:AllowedOrigins:0"] = "",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the production DbContext registration.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Register ApplicationDbContext with SQLite in-memory.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Ensure the database schema is created.
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Dispose();
            _connection = null;
        }
    }
}
