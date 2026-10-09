# 08 — Guided Implementation

**Duration**: 45 minutes

---

## Prerequisites

- .NET 8 SDK installed (`dotnet --version` returns 8.x)
- SQL Server LocalDB available (installed with Visual Studio or SQL Server Express)
- The `Code/` folder from this lesson (contains the complete solution)

---

## Stage 1 — Create the Solution

Open a terminal in the `Code/` directory. Create the solution and four projects:

```bash
dotnet new sln --name EmployeeManagement
dotnet new classlib -o EmployeeManagement.Domain
dotnet new classlib -o EmployeeManagement.Application
dotnet new classlib -o EmployeeManagement.Infrastructure
dotnet new webapi -o EmployeeManagement.Api
dotnet sln add EmployeeManagement.Domain
dotnet sln add EmployeeManagement.Application
dotnet sln add EmployeeManagement.Infrastructure
dotnet sln add EmployeeManagement.Api
```

This creates:

| Project | Type | Purpose |
|---|---|---|
| EmployeeManagement.Domain | Class library | Business entities |
| EmployeeManagement.Application | Class library | Use cases, interfaces, DTOs |
| EmployeeManagement.Infrastructure | Class library | EF Core, repositories, DI registration |
| EmployeeManagement.Api | Web API | Controllers, Program.cs, composition root |

At this point, all four projects are independent. They have no references to each other. This is expected. The next step adds the references.

---

## Stage 2 — Set Up Project References

Add project references that enforce the Dependency Rule:

```bash
dotnet add EmployeeManagement.Application reference EmployeeManagement.Domain
dotnet add EmployeeManagement.Infrastructure reference EmployeeManagement.Domain
dotnet add EmployeeManagement.Infrastructure reference EmployeeManagement.Application
dotnet add EmployeeManagement.Api reference EmployeeManagement.Domain
dotnet add EmployeeManagement.Api reference EmployeeManagement.Application
dotnet add EmployeeManagement.Api reference EmployeeManagement.Infrastructure
```

Add NuGet packages for EF Core and Swagger:

```bash
dotnet add EmployeeManagement.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.*
dotnet add EmployeeManagement.Api package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.*
dotnet add EmployeeManagement.Api package Swashbuckle.AspNetCore --version 6.6.2
```

Verify the reference graph:

```
EmployeeManagement.Domain          -> (no project references)
EmployeeManagement.Application     -> EmployeeManagement.Domain
EmployeeManagement.Infrastructure  -> EmployeeManagement.Domain, EmployeeManagement.Application
EmployeeManagement.Api             -> EmployeeManagement.Domain, EmployeeManagement.Application, EmployeeManagement.Infrastructure
```

Every reference points inward. Domain has zero references. This matches the Dependency Rule.

---

## Stage 3 — Build the Domain Layer

Create the `Entities` folder in `EmployeeManagement.Domain` and add the two entities.

`Employee.cs`:

```csharp
namespace EmployeeManagement.Domain.Entities;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Salary { get; set; }

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
}
```

`Department.cs`:

```csharp
namespace EmployeeManagement.Domain.Entities;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Employee> Employees { get; set; }
        = new List<Employee>();
}
```

Why these live in Domain:
- They represent business concepts, not database tables.
- No EF Core attributes, no HTTP annotations, no framework dependencies.
- Pure C# classes that exist regardless of how data is stored or presented.

The Domain project file has no external package references. It is pure .NET 8:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

---

## Stage 4 — Build the Application Layer

Create three folders in `EmployeeManagement.Application`: `Interfaces`, `Dtos`, and `Services`.

### Interfaces

`IEmployeeRepository.cs`:

```csharp
using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByIdWithDepartmentAsync(int id);
    Task<IEnumerable<Employee>> GetWithDepartmentAsync();
    Task AddAsync(Employee employee);
}
```

`IEmployeeService.cs`:

```csharp
using EmployeeManagement.Application.Dtos;

namespace EmployeeManagement.Application.Interfaces;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
    Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto);
}
```

The interface-in-inner-layer pattern: Application defines what it needs (`IEmployeeRepository`). Infrastructure fulfills it (`EmployeeRepository`). Application does not know who implements the interface. This is Dependency Inversion.

### DTOs

`CreateEmployeeDto.cs`:

```csharp
namespace EmployeeManagement.Application.Dtos;

public class CreateEmployeeDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public int DepartmentId { get; set; }
}
```

`EmployeeDto.cs`:

```csharp
namespace EmployeeManagement.Application.Dtos;

public class EmployeeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
}
```

DTOs are the boundary of the Application layer. They define the input and output of each use case. Domain entities stay inside. DTOs travel outside.

### Service

`EmployeeService.cs`:

```csharp
using EmployeeManagement.Application.Dtos;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;

    public EmployeeService(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
    {
        var employees = await _repository.GetWithDepartmentAsync();
        return employees.Select(MapToDto);
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _repository.GetByIdWithDepartmentAsync(id);
        return employee == null ? null : MapToDto(employee);
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        var employee = new Employee
        {
            Name = dto.Name,
            Email = dto.Email,
            Salary = dto.Salary,
            DepartmentId = dto.DepartmentId
        };

        await _repository.AddAsync(employee);

        return new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Salary = employee.Salary,
            DepartmentId = employee.DepartmentId
        };
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Salary = employee.Salary,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name
        };
    }
}
```

The service uses `IEmployeeRepository`, not `EmployeeRepository`. It knows nothing about EF Core, SQL Server, or HTTP. It only knows about Domain entities and its own interfaces.

---

## Stage 5 — Build the Infrastructure Layer

Create three items in `EmployeeManagement.Infrastructure`: `Data/AppDbContext.cs`, `Repositories/EmployeeRepository.cs`, and `DependencyInjection.cs`.

### AppDbContext

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

`DbContext` is a technical concern. It belongs in Infrastructure, not Domain or Application.

### EmployeeRepository

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

Infrastructure fulfills Application's contracts. `EmployeeRepository` implements `IEmployeeRepository` (defined in Application). This is where EF Core lives. This is where SQL queries happen.

### DependencyInjection

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

This class is the bridge. It connects Application's interfaces to Infrastructure's implementations. The API calls this method to register all services in the DI container.

---

## Stage 6 — Build the API Layer

The API layer contains the controller, Program.cs, and configuration.

### EmployeesController

```csharp
using EmployeeManagement.Application.Dtos;
using EmployeeManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

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

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);

        if (employee == null)
            return NotFound();

        return Ok(employee);
    }

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

The controller is thin. It receives HTTP requests, calls the Application service, and returns HTTP responses. It does not contain business logic. It does not access the database directly.

### Program.cs

```csharp
using EmployeeManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

Program.cs is the composition root. This is the one place in the entire application where all layers meet. The API references Infrastructure (to call `AddInfrastructure`), which in turn references Application and Domain. Everything gets wired together here.

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EmployeeManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## Stage 7 — Build and Verify

Run the following commands from the `Code/` directory:

```bash
dotnet restore
dotnet build
```

Expected result:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

The code has been verified to compile successfully with zero warnings and zero errors.

To run the application, a SQL Server database is required. The connection string uses LocalDB (`(localdb)\mssqllocaldb`). Before running, you need to create the database using EF Core migrations:

```bash
dotnet tool install --global dotnet-ef
dotnet add EmployeeManagement.Api package Microsoft.EntityFrameworkCore.Design --version 8.0.*
dotnet ef migrations add InitialCreate --project EmployeeManagement.Api --startup-project EmployeeManagement.Api
dotnet ef database update --project EmployeeManagement.Api --startup-project EmployeeManagement.Api
```

Then run:

```bash
dotnet run --project EmployeeManagement.Api
```

---

## Troubleshooting

**Error: "The item forms a circular dependency."**
Cause: A project references another project that references it back. Check the `dotnet add reference` commands. Make sure Domain has no references, Application only references Domain, and Infrastructure references Application and Domain.

**Error: "The type or namespace name 'EmployeeManagement.Application' could not be found."**
Cause: Missing project reference. The file uses a namespace from another project, but the reference was not added. Run the `dotnet add reference` command for the missing project.

**Error: "No service for type 'EmployeeManagement.Application.Interfaces.IEmployeeRepository' has been registered."**
Cause: The DI container does not have a registration for `IEmployeeRepository`. Check that `DependencyInjection.cs` in Infrastructure includes `services.AddScoped<IEmployeeRepository, EmployeeRepository>()` and that Program.cs calls `builder.Services.AddInfrastructure(builder.Configuration)`.

**Error: "A network-related or instance-specific error occurred while establishing a connection to SQL Server."**
Cause: SQL Server LocalDB is not installed or not running. Install SQL Server Express with LocalDB, or change the connection string in `appsettings.json` to point to an available SQL Server instance.

---

## Knowledge Check

1. Why does the controller inject `IEmployeeService` instead of `EmployeeService`? What would break if you changed it?

2. If you remove `builder.Services.AddInfrastructure(builder.Configuration)` from Program.cs, what happens at compile time and at runtime?

3. Which project would need to change if you replaced SQL Server with PostgreSQL? Which projects would stay the same?
