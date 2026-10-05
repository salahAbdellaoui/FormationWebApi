# 01 — DbContext and DbSet

**Duration:** ~30 min

---

## What You'll Learn

By the end of this topic, you will:

- Understand what Entity Framework Core is
- Understand what `DbContext` represents
- Understand what `DbSet<T>` represents
- Configure a SQL Server connection
- Register `DbContext` with Dependency Injection

---

## What Is Entity Framework Core?

Entity Framework Core (EF Core) is an **Object-Relational Mapper (ORM)**.

It lets your C# code talk to a database without writing raw SQL:

```text
Application (C#)
    |
    v
DbContext
    |
    v
EF Core
    |
    v
SQL Server
```

Instead of writing:

```sql
SELECT * FROM Employees WHERE Id = 1
```

You write:

```csharp
var employee = await context.Employees.FindAsync(1);
```

EF Core translates the C# code into SQL.

---

## What Is DbContext?

`DbContext` is the **bridge** between your application and the database.

It does three things:

1. **Holds `DbSet<T>` properties** — one for each table
2. **Tracks changes** — knows what you added, modified, or deleted
3. **Saves changes** — sends everything to the database in one operation

Think of `DbContext` as a session. You open it, do work, save, and close.

```text
DbContext
    |
    +-- DbSet<Employee>     (maps to the Employees table)
    +-- DbSet<Department>   (maps to the Departments table)
```

---

## What Is DbSet<T>?

`DbSet<T>` represents a **collection of entities** in the database.

Each `DbSet<T>` corresponds to one table:

| DbSet | Table |
|-------|-------|
| `DbSet<Employee>` | Employees |
| `DbSet<Department>` | Departments |

You use `DbSet<T>` to:

- Query data (`ToListAsync`)
- Add data (`AddAsync`)
- Update data (`Update`)
- Delete data (`Remove`)

---

## Step 1: Install EF Core Packages

Open a terminal in your project folder and run:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.*
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.*
```

**What these packages do:**

| Package | Purpose |
|---------|---------|
| `Microsoft.EntityFrameworkCore.SqlServer` | Lets EF Core talk to SQL Server |
| `Microsoft.EntityFrameworkCore.Tools` | Provides CLI commands for migrations |

The `8.0.*` version matches .NET 8. NuGet will resolve the latest patch.

---

## Step 2: Add the SQL Server Connection String

Open `appsettings.json` and add a connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EmployeeManagement;Trusted_Connection=True;TrustServerCertificate=True;"
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

**What does this mean?**

| Part | Meaning |
|------|---------|
| `Server=localhost` | SQL Server runs on this machine |
| `Database=EmployeeManagement` | Database name (EF Core will create it) |
| `Trusted_Connection=True` | Use Windows authentication |
| `TrustServerCertificate=True` | Accept the default SSL certificate |

> **Note:** Adjust the connection string if your SQL Server uses a named instance (e.g., `Server=localhost\SQLEXPRESS`) or SQL authentication.

---

## Step 3: Create AppDbContext

Create a `Data/` folder and add `AppDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
}
```

**What does each part do?**

| Part | Purpose |
|------|---------|
| `: DbContext` | Inherits EF Core's base context |
| `DbContextOptions<AppDbContext>` | Receives configuration (connection string, provider) |
| `DbSet<Employee>` | Represents the Employees table |
| `DbSet<Department>` | Represents the Departments table |
| `Set<T>()` | Tells EF Core to create a set for entity `T` |

---

## Step 4: Register DbContext in Program.cs

Open `Program.cs` and add the EF Core registration before `var app = builder.Build()`:

```csharp
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

**What does `AddDbContext` do?**

1. Registers `AppDbContext` with the DI container
2. Configures the SQL Server provider
3. Passes the connection string from `appsettings.json`
4. Creates a new `AppDbContext` per HTTP request (scoped lifetime)

---

## Why Scoped Lifetime?

ASP.NET Core creates one `AppDbContext` per request:

```text
Request 1 → new AppDbContext → ... → disposed
Request 2 → new AppDbContext → ... → disposed
```

This is safe. Each request gets its own context. No shared state between requests.

---

## Common Mistakes

**Mistake 1: Forgetting to install the SQL Server provider package.**

Without `Microsoft.EntityFrameworkCore.SqlServer`, the `UseSqlServer()` method does not exist.

**Mistake 2: Wrong connection string format.**

If SQL Server uses a named instance, the connection string must include it:
```text
Server=localhost\SQLEXPRESS;Database=EmployeeManagement;...
```

**Mistake 3: Not calling `AddDbContext`.**

Without DI registration, the controller cannot receive `AppDbContext` through its constructor.

---

## Knowledge Check

1. What is the role of `DbContext`?
2. What does `DbSet<Employee>` represent?
3. Why do we register `DbContext` with `AddDbContext` instead of `AddSingleton`?
4. Where does the connection string come from?

---

## Next

We have a `DbContext` and a connection string. Next, we create the entities that will become database tables using the Code First approach.
