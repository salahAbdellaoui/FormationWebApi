# 05 — Infrastructure Layer

## What Is the Infrastructure Layer?

The Infrastructure Layer implements technical details. This is where EF Core, SQL Server, file system access, email sending, logging, and external API calls live.

Everything in this layer implements interfaces defined by the Application Layer. The Infrastructure Layer contains no business rules. It contains no domain logic. It only contains code that talks to the outside world.

The Infrastructure Layer sits on the outside of the Clean Architecture onion. It references inner layers (Domain and Application), but inner layers never reference it.

## AppDbContext

The `AppDbContext` class lives in the Infrastructure Layer, not in Domain or Application. It references Domain entities because Infrastructure is allowed to reference inner layers.

```csharp
using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
}
```

`DbContext` is a technical concern. It knows about SQL Server, connection strings, and entity tracking. These concerns do not belong in Domain or Application. By placing `AppDbContext` here, we keep EF Core isolated from the business logic.

## The Repository Implementation

The `EmployeeRepository` class implements `IEmployeeRepository`, which was defined in the Application Layer. The interface lives in the inner layer; the implementation lives in the outer layer. This is the Dependency Inversion Principle in action.

```csharp
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _context.Employees.AsNoTracking().ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees.FindAsync(id);
    }

    public async Task<Employee?> GetByIdWithDepartmentAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Employee>> GetWithDepartmentAsync()
    {
        return await _context.Employees
            .Include(e => e.Department)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
    }
}
```

Key points about this implementation:

- The repository depends on `IEmployeeRepository` from Application, not the other way around.
- `AsNoTracking()` is used on read queries for better performance.
- `Include()` is used to eager-load related entities when needed.
- Each method does exactly one thing: fetch data or persist data.

## Dependency Injection Registration

The `DependencyInjection` class exposes an extension method that registers all infrastructure services. The API calls this method in `Program.cs`.

```csharp
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Application.Services;
using EmployeeManagement.Infrastructure.Data;
using EmployeeManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeService, EmployeeService>();

        return services;
    }
}
```

This is where concrete implementations are wired to interfaces. The `AddDbContext` call configures EF Core to use SQL Server with a connection string from configuration. The `AddScoped` calls register the repository and service so that they are injected wherever their interfaces are used.

## Project References

The Infrastructure project references both Domain and Application. It also pulls in the EF Core SQL Server NuGet package.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\EmployeeManagement.Domain\EmployeeManagement.Domain.csproj" />
    <ProjectReference Include="..\EmployeeManagement.Application\EmployeeManagement.Application.csproj" />
  </ItemGroup>

</Project>
```

The project references show the dependency direction clearly: Infrastructure depends on Domain and Application. Neither Domain nor Application depends on Infrastructure.

## Key Insight

The Infrastructure Layer depends ON the Application Layer. The Application Layer does NOT depend on the Infrastructure Layer. The dependency direction is always inward.

At compile time, Application has no idea that Infrastructure exists. Application only knows about interfaces it defines.

At runtime, dependency injection wires the concrete Infrastructure implementations to the Application interfaces. This is how the layers stay decoupled while still working together.

This means you can swap out Infrastructure implementations without changing any code in Domain or Application. You could replace SQL Server with PostgreSQL, or replace file-based storage with cloud storage, and the business rules stay exactly the same.

## Common Mistakes

**Mistake 1: Putting business rules in the repository.**
Repositories should only fetch and persist data. If your repository contains logic like "calculate overtime pay" or "check if employee is eligible for promotion", that logic belongs in a Domain entity or an Application service.

**Mistake 2: Making Domain or Application depend on Infrastructure.**
If you add a `using EmployeeManagement.Infrastructure;` statement inside a Domain or Application file, the dependency direction is broken. Inner layers must never know about outer layers.

**Mistake 3: Putting DbContext in the Application Layer.**
`DbContext` is a technical detail. Placing it in Application would force Application to reference EF Core, which makes Application depend on a framework. Application should only define interfaces and let Infrastructure provide the implementations.

**Mistake 4: Exposing entities directly from the API without DTOs.**
The repository returns Domain entities. The service layer maps them to DTOs. The controller returns DTOs. Skipping this mapping leaks domain structure to the outside world and makes your API fragile to internal changes.

## Knowledge Check

1. Why does `AppDbContext` live in Infrastructure instead of Application?

2. The `IEmployeeRepository` interface is defined in Application, but the `EmployeeRepository` class is in Infrastructure. Which design principle does this follow, and why does it matter?

3. At what point does the application connect the Infrastructure implementations to the Application interfaces: compile time or runtime? Explain how.
