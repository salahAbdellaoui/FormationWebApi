# Day 3 — Entity Framework Core

## Professional Training Course

**Duration:** 4 hours  
**Level:** Beginner-Intermediate — Building on Day 2  
**Prerequisites:** Day 1 (ASP.NET Core), Day 2 (Advanced C# — LINQ, async/await, generics)  
**Framework:** .NET 8 / EF Core 8

---

## 🎯 Day 3 Learning Objectives

By the end of this session, you will understand:

- What Entity Framework Core is and why ORMs exist
- How EF Core communicates with SQL Server
- DbContext, DbSet, and entity design
- Code First approach and database migrations
- Relationships and how to configure them
- LINQ queries with EF Core — from C# to SQL
- Tracking vs No-Tracking
- Loading related data — Include, projection
- Common EF Core performance pitfalls
- How EF Core fits into a real ASP.NET Core application

---

## Training Schedule

| Part | Topic | Duration |
|------|-------|----------|
| 1 | Understanding ORM & EF Core | ~35 min |
| 2 | DbContext, DbSet & Entities | ~45 min |
| 3 | Code First & Migrations | ~40 min |
| 4 | Relationships & Configuration | ~55 min |
| 5 | LINQ with EF Core | ~45 min |
| 6 | Tracking, Projection & Performance | ~35 min |
| 7 | Practical Exercise & Review | ~25 min |

---

# Part 1 — Understanding ORM & EF Core

## 1.1 The Problem

> **Instructor Note:** Ask participants — *"Imagine you have an ASP.NET Core Web API for managing employees. You need to insert, update, delete, search, and retrieve employees from SQL Server. How would you do that?"*

Let us think about what we need:

- Insert new employees
- Update existing employees
- Delete employees
- Search employees by name
- Retrieve all employees
- Retrieve employees by department

The traditional approach looks like this:

```text
C# Application
      ↓
SQL Connection
      ↓
SQL Command
      ↓
SQL Query (string)
      ↓
SQL Server
      ↓
Rows (DataReader)
      ↓
Manual mapping to C# objects
```

Here is what the code would look like without any ORM:

```csharp
using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

var command = new SqlCommand(
    "SELECT Id, Name, Email, Salary, DepartmentId FROM Employees WHERE IsActive = 1",
    connection);

using var reader = await command.ExecuteReaderAsync();

var employees = new List<Employee>();
while (await reader.ReadAsync())
{
    employees.Add(new Employee
    {
        Id = reader.GetInt32(0),
        Name = reader.GetString(1),
        Email = reader.GetString(2),
        Salary = reader.GetDecimal(3),
        DepartmentId = reader.GetInt32(4)
    });
}
```

This works. But look at the problems:

- **SQL strings everywhere** — scattered throughout your code, impossible to refactor safely
- **Manual mapping** — you write the mapping from rows to objects by hand, for every query
- **Repetitive code** — every query follows the same pattern: connection, command, reader, map
- **Difficult maintenance** — changing a column name means hunting through SQL strings
- **SQL mistakes are invisible** — typos in SQL strings are not caught at compile time
- **Impedance mismatch** — objects have relationships; tables have foreign keys; you bridge them manually

---

## 1.2 What Is an ORM?

An **ORM** (Object-Relational Mapping) is a technique that maps C# objects to database tables and rows.

```text
C# Entity          ↔        Database Table
C# Property        ↔        Table Column
C# Relationship    ↔        Foreign Key
C# Query (LINQ)    ↔        SQL Query
```

> 🟦 **Concept:** The ORM sits between your C# code and the database. You work with objects and LINQ; the ORM translates that into SQL.

---

## 1.3 Entity Framework Core

**Entity Framework Core** (EF Core) is Microsoft's ORM for .NET. It is the modern, lightweight, cross-platform version of Entity Framework.

```text
Application Code (C# / LINQ)
        ↓
    EF Core
        ↓
  Database Provider (SQL Server, PostgreSQL, SQLite...)
        ↓
    Database
```

### What EF Core Does

- Maps C# classes to database tables
- Translates LINQ queries into SQL
- Tracks changes to objects (inserts, updates, deletes)
- Manages relationships between entities
- Handles database migrations (schema changes)
- Manages connections and transactions

### What EF Core Does NOT Do

- **Does not replace SQL knowledge** — it generates SQL, and you need to understand that SQL
- **Does not make all queries fast** — a bad LINQ query produces a bad SQL query
- **Does not remove the database from the architecture** — the database is still part of your application's performance

> 💡 **Senior Developer Lesson:** "EF Core is a tool. SQL is still the language of the database. Using EF Core without understanding SQL is like driving a car without knowing how to brake."

---

## 1.4 How EF Core Works — Conceptually

When you write:

```csharp
var employees = await context.Employees
    .Where(e => e.IsActive)
    .ToListAsync();
```

EF Core does the following:

```text
LINQ Query (C#)
      ↓
IQueryable<Employee>
      ↓
EF Core Expression Tree Analysis
      ↓
SQL Generation
      ↓
SQL sent to SQL Server
      ↓
Results come back as rows
      ↓
EF Core maps rows to Employee objects
      ↓
Returns List<Employee>
```

The generated SQL is conceptually similar to:

```sql
SELECT [e].[Id], [e].[Name], [e].[Email], [e].[Salary], [e].[DepartmentId]
FROM [Employees] AS [e]
WHERE [e].[IsActive] = 1
```

> ⚠️ **Important:** The actual generated SQL depends on the query, the provider, and the EF Core version. Do not assume the SQL is exactly this — use `ToQueryString()` to inspect it.

---

## 1.5 ORM Trade-Offs

### Advantages

| Advantage | Explanation |
|-----------|-------------|
| Productivity | Write less boilerplate code |
| Strongly typed | LINQ queries are checked at compile time |
| Object mapping | Automatic mapping from rows to objects |
| Change tracking | EF Core knows what changed and generates the right SQL |
| Relationships | Navigations, includes, joins are built in |
| Migrations | Schema changes are versioned and repeatable |
| LINQ integration | Use the same query language you already know |
| .NET ecosystem | First-class support from Microsoft |

### Disadvantages / Risks

| Risk | Explanation |
|------|-------------|
| Hidden SQL | Developers may not understand what queries run |
| Poor LINQ | Bad LINQ generates expensive SQL |
| N+1 queries | Loop-based loading creates excessive database round-trips |
| Over-fetching | Loading entire entities when only a few fields are needed |
| Over-tracking | Tracking entities you never intend to update |
| Lazy loading traps | Navigation properties can trigger hidden queries |

> 💡 **Senior Developer Lesson:** "Using EF Core does not mean you can stop understanding SQL. It means you write less SQL — but the SQL still runs."

---

## 1.6 Instructor Interaction

> 🧠 **Think About It:** "If EF Core generates SQL from LINQ, what do you think happens when a developer writes a complex LINQ query with multiple joins and subqueries?"

> **Expected answer:** EF Core attempts to translate the entire LINQ expression into a single SQL query. If the expression is too complex or uses unsupported patterns, EF Core may throw an exception or pull data into memory and filter there — which is dangerous for performance.

---

# Part 2 — DbContext, DbSet & Entities

## 2.1 What Is DbContext?

> **Instructor Note:** Ask participants — *"What do you think DbContext represents? Is it a database connection?"*

**DbContext** is the primary interface between your C# application and the database. It is:

- Your **session** with the database
- The **unit of work** that tracks changes
- The **query gateway** that translates LINQ to SQL
- The **configuration holder** for your model

Think of DbContext as a **conversation with the database**. You tell it what you want; it translates, executes, and tracks.

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
}
```

Let us break this down:

| Line | Purpose |
|------|---------|
| `: DbContext` | Inheritance — ApplicationDbContext IS a DbContext |
| `DbContextOptions<...>` | Configuration (connection string, provider, etc.) |
| `: base(options)` | Pass configuration to the base DbContext |
| `DbSet<Employee>` | Represents the Employees table |
| `=> Set<Employee>()` | Short syntax for the backing field |

> 🧠 **Think About It:** *"Is DbSet a database table?"*
>
> **Answer:** Not exactly. DbSet is an **entry point for queries** against a table. It represents a logical collection of entities that maps to a table. The actual table is created by migrations/schema generation.

---

## 2.2 Entity Design

Entities are C# classes that represent database tables.

### Employee Entity

```csharp
public class Employee
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public decimal Salary { get; set; }

    public bool IsActive { get; set; } = true;

    public int DepartmentId { get; set; }

    public Department? Department { get; set; }
}
```

| Property | Purpose |
|----------|---------|
| `Id` | Primary key — uniquely identifies each row |
| `Name` | Employee name |
| `Email` | Employee email (should be unique in real systems) |
| `Salary` | Employee salary |
| `IsActive` | Soft-delete flag |
| `DepartmentId` | Foreign key — links to Department |
| `Department` | Navigation property — the related Department object |

### Department Entity

```csharp
public class Department
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Employee> Employees { get; set; }
        = new List<Employee>();
}
```

| Property | Purpose |
|----------|---------|
| `Id` | Primary key |
| `Name` | Department name |
| `Employees` | Collection navigation — all employees in this department |

---

## 2.3 Key Concepts

### Primary Key

Every entity needs a primary key. EF Core convention: a property named `Id` or `<EntityName>Id` is automatically the primary key.

```csharp
public int Id { get; set; }  // ← EF Core recognizes this as the PK
```

### Foreign Key

A foreign key links one entity to another. In `Employee`, `DepartmentId` is the foreign key that references `Department.Id`.

### Navigation Property

A navigation property lets you traverse relationships in C#:

```csharp
// From Employee to Department
var departmentName = employee.Department?.Name;

// From Department to Employees
var employeeCount = department.Employees.Count;
```

### Collection Navigation vs Reference Navigation

| Type | Example | Meaning |
|------|---------|---------|
| **Reference navigation** | `Employee.Department` | One related entity |
| **Collection navigation** | `Department.Employees` | Many related entities |

### Why Initialize Collections

```csharp
public ICollection<Employee> Employees { get; set; }
    = new List<Employee>();  // ← prevents null reference exceptions
```

Without initialization, accessing `department.Employees` could throw a `NullReferenceException`.

---

## 2.4 Dependency Injection — Registering DbContext

In ASP.NET Core, you register DbContext in the DI container:

```csharp
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
```

| Part | Purpose |
|------|---------|
| `AddDbContext<>()` | Registers DbContext with DI |
| `UseSqlServer` | Tells EF Core to use SQL Server |
| `GetConnectionString` | Reads from appsettings.json |

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\mssqllocaldb;Database=EmployeeManagementDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

> ⚠️ **Important:** Never commit real connection strings with credentials to source control. Use user secrets or environment variables in production.

---

## 2.5 What Happens Internally

When a controller requests `ApplicationDbContext`:

```text
HTTP Request arrives at Controller
      ↓
Controller has constructor dependency on ApplicationDbContext
      ↓
DI container creates a new DbContext instance
      ↓
DbContext opens a database connection (when needed)
      ↓
LINQ queries are translated to SQL
      ↓
SQL is executed against SQL Server
      ↓
Results are materialized as C# objects
      ↓
DbContext tracks changes to those objects
      ↓
When SaveChanges is called, tracked changes are written to the database
      ↓
DbContext is disposed (connection closed)
```

---

## 2.6 Common Mistakes

### Treating DbContext as a Singleton

```csharp
// ❌ WRONG — DbContext is not thread-safe
services.AddSingleton<ApplicationDbContext>();

// ✅ CORRECT — Scoped (one instance per request)
services.AddDbContext<ApplicationDbContext>(...);
```

DbContext is designed to be **short-lived**. One instance per HTTP request. Never share it across threads.

### Not Disposing DbContext

In ASP.NET Core, DI handles disposal automatically. But in console apps or background services, use `using`:

```csharp
using var context = new ApplicationDbContext(options);
// context is disposed at the end of this block
```


---

# Part 3 — Code First & Migrations

## 3.1 What Is Code First?

> **Instructor Note:** Ask participants — *"Do you create the database first and then write C# classes? Or do you write C# classes first and then create the database?"*

With **Code First**, you define your entities in C# and let EF Core create the database schema from those classes.

```text
C# Entity Classes
        ↓
EF Core Model (how EF interprets your classes)
        ↓
Migration (a description of schema changes)
        ↓
SQL (what to execute against the database)
        ↓
SQL Server (database is created/updated)
```

> 🟦 **Concept:** Code First means the C# code is the source of truth for the database schema. The database follows the code, not the other way around.

### Code First vs Database First

| Approach | Source of Truth | When to Use |
|----------|----------------|-------------|
| **Code First** | C# classes | New projects, greenfield development |
| **Database First** | Existing database | Working with legacy databases |

For new projects, **Code First** is the standard approach in modern .NET development.

---

## 3.2 Creating Your First Migration

### Step 1: Ensure Entities and DbContext Are Ready

You need your `ApplicationDbContext` and entity classes.

### Step 2: Create the Migration

```bash
dotnet ef migrations add InitialCreate
```

What happens:

1. EF Core analyzes your DbContext and entities
2. EF Core generates a migration class with `Up()` and `Down()` methods
3. The `Up()` method contains code to create tables, columns, and constraints
4. A snapshot file is created to track the current model state

### Step 3: Apply the Migration

```bash
dotnet ef database update
```

What happens:

1. EF Core connects to the database
2. EF Core checks the `__EFMigrationsHistory` table
3. EF Core executes the SQL from the migration
4. EF Core records the migration in the history table

---

## 3.3 Modifying the Schema

### Adding a New Property

```csharp
public class Employee
{
    // ... existing properties

    public string? PhoneNumber { get; set; }  // ← new property
}
```

### Creating a New Migration

```bash
dotnet ef migrations add AddPhoneNumberToEmployee
```

### Applying the Change

```bash
dotnet ef database update
```

The migration adds the `PhoneNumber` column to the `Employees` table.

---

## 3.4 Migration Best Practices

> 💡 **Senior Developer Lesson:** "Migrations are not disposable files. They are the version history of your database schema."

### Always Review Generated Migrations

Open the migration file and read the `Up()` method. Understand what SQL will execute.

### Commit Migrations to Source Control

Migrations are part of your codebase. They should be reviewed in pull requests just like any other code.

### Do Not Delete Migrations Carefully

Deleting migrations can leave your database in an inconsistent state. If you need to combine migrations, use `dotnet ef migrations remove` to remove the last one, then recreate.

### Development vs Production

| Environment | Approach |
|-------------|----------|
| **Development** | `dotnet ef database update` is fine |
| **Production** | Generate SQL scripts or use a deployment tool |

```bash
# Generate a SQL script for production
dotnet ef migrations script -o migration.sql
```

> 🟥 **Warning:** Never run `dotnet ef database update` directly against production without understanding the exact changes being applied.

---

## 3.5 Common Migration Mistakes

| Mistake | Consequence |
|---------|-------------|
| Forgetting to create a migration | Schema changes not applied |
| Wrong connection string | Updates the wrong database |
| Changing entities without a migration | Database and code are out of sync |
| Not reviewing generated SQL | Destructive changes applied blindly |
| Treating migrations as disposable | Lost schema history |

---

## 3.6 Instructor Interaction

> 🧠 **Think About It:** "You added a new property to Employee but forgot to create a migration. What happens when you run the application?"
>
> **Answer:** In a normal application, nothing happens immediately — the property exists in C# but not in the database. When you try to query or save using that property, you get a runtime error. This is why migrations matter.


---

# Part 4 — Relationships & Configuration

## 4.1 Relationship Types

### One-to-One

One entity relates to exactly one other entity.

```text
Employee ─── EmployeeProfile
```

Example: An employee has one profile; a profile belongs to one employee.

### One-to-Many

One entity relates to many of another entity.

```text
Department
    │
    ├── Employee
    ├── Employee
    └── Employee
```

Example: A department has many employees; each employee belongs to one department.

### Many-to-Many

Many entities relate to many other entities.

```text
Employee ←──→ Project
```

Example: An employee can work on many projects; a project can have many employees.

---

## 4.2 One-to-Many — Detailed

This is the most common relationship. Let us implement it.

### Entity Configuration

```csharp
public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Collection navigation — "a department has many employees"
    public ICollection<Employee> Employees { get; set; }
        = new List<Employee>();
}

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Salary { get; set; }

    // Foreign key — links to Department
    public int DepartmentId { get; set; }

    // Reference navigation — "an employee belongs to one department"
    public Department? Department { get; set; }
}
```

### How SQL Server Represents This

```sql
CREATE TABLE Departments (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL
);

CREATE TABLE Employees (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL,
    Salary DECIMAL(18,2) NOT NULL,
    DepartmentId INT NOT NULL,
    FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)
);
```

---

## 4.3 Many-to-Many — Detailed

### Using a Join Entity (Explicit)

```csharp
public class EmployeeProject
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public DateTime AssignedDate { get; set; }
}

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<EmployeeProject> EmployeeProjects { get; set; }
        = new List<EmployeeProject>();
}
```

The join entity `EmployeeProject` allows additional data (like `AssignedDate`) on the relationship.

### Using Skip Navigation (Simpler)

If you do not need extra data on the relationship:

```csharp
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Project> Projects { get; set; }
        = new List<Project>();
}

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Employee> Employees { get; set; }
        = new List<Employee>();
}
```

EF Core automatically creates a join table (`EmployeeProject`) for you.

---

## 4.4 Relationship Exercise

> 🧠 **Think About It:** Give trainees this scenario:
>
> "A company has departments. Each department has many employees. An employee can participate in multiple projects. A project can contain multiple employees."

**Ask trainees to identify:**

1. What are the entities?
2. What are the primary keys?
3. What are the foreign keys?
4. What are the relationships?

**Expected answers:**

| Entity | Primary Key | Foreign Keys |
|--------|-------------|--------------|
| Department | Id | — |
| Employee | Id | DepartmentId |
| Project | Id | — |

| Relationship | Type |
|-------------|------|
| Department → Employee | One-to-Many |
| Employee ↔ Project | Many-to-Many |

---

## 4.5 Fluent API

The Fluent API is used inside `OnModelCreating` to configure entity relationships and constraints.

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

### Breaking This Down

| Line | Meaning |
|------|---------|
| `modelBuilder.Entity<Employee>()` | Configure the Employee entity |
| `.HasOne(e => e.Department)` | Employee has one Department |
| `.WithMany(d => d.Employees)` | Department has many Employees |
| `.HasForeignKey(e => e.DepartmentId)` | The foreign key is DepartmentId |
| `.OnDelete(DeleteBehavior.Restrict)` | Prevent deleting a department with employees |

### Other Fluent API Configurations

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Table name
    modelBuilder.Entity<Employee>()
        .ToTable("tbl_Employees");

    // Column name
    modelBuilder.Entity<Employee>()
        .Property(e => e.Name)
        .HasColumnName("EmployeeName");

    // Column type
    modelBuilder.Entity<Employee>()
        .Property(e => e.Salary)
        .HasColumnType("decimal(18,2)");

    // Required / Max length
    modelBuilder.Entity<Employee>()
        .Property(e => e.Email)
        .IsRequired()
        .HasMaxLength(200);

    // Unique index
    modelBuilder.Entity<Employee>()
        .HasIndex(e => e.Email)
        .IsUnique();

    // Default value
    modelBuilder.Entity<Employee>()
        .Property(e => e.IsActive)
        .HasDefaultValue(true);
}
```

---

## 4.6 Data Annotations

An alternative to Fluent API — attributes directly on entity properties:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("tbl_Employees")]
public class Employee
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column(TypeName = "nvarchar(200)")]
    public string Email { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Salary { get; set; }

    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(Department))]
    public int DepartmentId { get; set; }

    public Department? Department { get; set; }
}
```

---

## 4.7 Data Annotations vs Fluent API

| Feature | Data Annotations | Fluent API |
|---------|-----------------|------------|
| Syntax | Attributes on properties | Code in `OnModelCreating` |
| Simplicity | Simple, close to entity | More verbose |
| Power | Limited | Full control |
| Centralization | Spread across entities | Centralized in one place |
| Complex constraints | Limited | Full support |
| Naming conventions | Good for basic rules | Better for advanced mappings |

> 💡 **Senior Developer Lesson:** "In most projects, developers use Fluent API for relationships and complex configurations, and Data Annotations for simple constraints like `[MaxLength]` and `[Required]`. Both are valid — pick what fits your project's conventions."

---

## 4.8 Instructor Interaction

> 🧠 **Think About It:** "Why would you use `OnDelete(DeleteBehavior.Restrict)` instead of `OnDelete(DeleteBehavior.Cascade)`?"
>
> **Answer:** Cascade delete automatically deletes related records when you delete a parent. This can be dangerous — deleting a department cascades to all its employees. Restrict prevents deletion if related records exist, forcing you to handle the relationship explicitly.


---

# Part 5 — LINQ with EF Core

This is one of the most important sections of the entire course.

## 5.1 The Problem

> **Instructor Note:** Ask participants — *"Give me all active employees with salary greater than 3000."*

How would you write this in SQL?

```sql
SELECT * FROM Employees WHERE IsActive = 1 AND Salary > 3000;
```

How would you write this in C# with EF Core?

```csharp
var employees = await context.Employees
    .Where(e => e.IsActive && e.Salary > 3000)
    .ToListAsync();
```

Same result. Different syntax. EF Core translates your LINQ into SQL.

---

## 5.2 The LINQ Pipeline

```text
LINQ (C#)
   ↓
IQueryable<Employee>
   ↓
EF Core Expression Tree Analysis
   ↓
SQL Generation
   ↓
SQL sent to SQL Server
   ↓
Rows returned
   ↓
EF Core materializes into Employee objects
   ↓
Returns List<Employee>
```

> 🟦 **Concept:** LINQ with EF Core is not LINQ-to-Objects. It is **LINQ-to-Entities** — your query is translated to SQL and executed in the database, not in memory.

---

## 5.3 When Does the Query Execute?

This is critical to understand.

### Building a Query (No Execution)

```csharp
IQueryable<Employee> query = context.Employees
    .Where(e => e.IsActive);
```

**Nothing happens here.** No database call. This is a query **description**, not a query **execution**.

### Executing a Query

```csharp
List<Employee> employees = await query.ToListAsync();
```

**NOW the query executes.** The SQL is sent to the database.

### Execution Methods

| Method | When It Executes | What It Returns |
|--------|------------------|-----------------|
| `ToListAsync()` | Immediately | `List<T>` |
| `ToArrayAsync()` | Immediately | `T[]` |
| `FirstAsync()` | Immediately | First matching element |
| `FirstOrDefaultAsync()` | Immediately | First matching element or default |
| `SingleAsync()` | Immediately | Exactly one matching element |
| `SingleOrDefaultAsync()` | Immediately | Zero or one matching element |
| `AnyAsync()` | Immediately | `bool` |
| `CountAsync()` | Immediately | `int` |
| `ToQueryString()` | Does not execute | SQL string (for inspection) |

> 🧠 **Think About It:** "When will this query actually execute?"
>
> ```csharp
> var query = context.Employees.Where(e => e.IsActive);
> var count = await query.CountAsync();
> ```
>
> **Answer:** The query executes when `CountAsync()` is called, not when the variable is assigned.

---

## 5.4 First vs Single

### FirstOrDefaultAsync

```csharp
var employee = await context.Employees
    .FirstOrDefaultAsync(e => e.Id == id);
```

**Use when:** "Give me the first matching record if one exists."

- Returns null if no match
- Returns the first match if multiple exist
- Does NOT throw if multiple matches exist

### SingleOrDefaultAsync

```csharp
var employee = await context.Employees
    .SingleOrDefaultAsync(e => e.Email == email);
```

**Use when:** "There should be zero or one matching record."

- Returns null if no match
- Returns the single match
- **Throws `InvalidOperationException`** if more than one match exists

### When to Use Which

| Scenario | Method | Why |
|----------|--------|-----|
| Find by ID (primary key) | `FirstOrDefaultAsync` | ID is unique; there will be at most one |
| Find by Email (unique constraint) | `SingleOrDefaultAsync` | Enforces that Email is unique |
| Get first result of a query | `FirstOrDefaultAsync` | You want the first result regardless |
| Enforce single result | `SingleOrDefaultAsync` | You expect exactly one result |

> 🟥 **Warning:** `SingleOrDefaultAsync` throws if multiple records match. If Email is not actually unique in the database, this will throw at runtime.

---

## 5.5 Filtering

### Basic Filtering

```csharp
var activeEmployees = await context.Employees
    .Where(e => e.IsActive)
    .ToListAsync();

var highEarners = await context.Employees
    .Where(e => e.Salary > 5000)
    .ToListAsync();

var departmentEmployees = await context.Employees
    .Where(e => e.DepartmentId == 3)
    .ToListAsync();
```

### Combining Conditions

```csharp
var results = await context.Employees
    .Where(e => e.IsActive && e.Salary > 3000 && e.DepartmentId == 1)
    .ToListAsync();
```

### String Contains (LIKE query)

```csharp
var searchResults = await context.Employees
    .Where(e => e.Name.Contains("Ali"))
    .ToListAsync();
```

Generated SQL (conceptually):

```sql
SELECT ... FROM Employees
WHERE Name LIKE '%Ali%'
```

---

## 5.6 Sorting

```csharp
// Ascending
var sorted = await context.Employees
    .OrderBy(e => e.Name)
    .ToListAsync();

// Descending
var sortedDesc = await context.Employees
    .OrderByDescending(e => e.Salary)
    .ToListAsync();

// Multiple levels
var sorted = await context.Employees
    .OrderBy(e => e.DepartmentId)
    .ThenByDescending(e => e.Salary)
    .ToListAsync();
```

---

## 5.7 Projection

This is a critical production concept.

### The Problem with Loading Everything

```csharp
var employees = await context.Employees.ToListAsync();
```

This loads **every column** of **every employee** into memory. But what if the API only needs `Id`, `Name`, and `Department Name`?

### The Solution: Projection

```csharp
var employees = await context.Employees
    .Select(e => new EmployeeListItemDto
    {
        Id = e.Id,
        Name = e.Name,
        Department = e.Department!.Name
    })
    .ToListAsync();
```

Generated SQL (conceptually):

```sql
SELECT e.Id, e.Name, d.Name AS Department
FROM Employees e
INNER JOIN Departments d ON e.DepartmentId = d.Id
```

### Why Projection Matters

| Aspect | Without Projection | With Projection |
|--------|-------------------|-----------------|
| Data transferred | All columns | Only needed columns |
| Memory usage | Higher | Lower |
| Network usage | Higher | Lower |
| Query performance | Slower | Faster |
| API response size | Larger | Smaller |

> 💡 **Senior Developer Lesson:** "Don't retrieve the entire entity when the API only needs three fields. Projection is one of the simplest and most effective performance improvements."

---

## 5.8 Include — Loading Related Data

### The Problem

```csharp
var employees = await context.Employees.ToListAsync();

foreach (var employee in employees)
{
    // ❌ N+1 problem — each access triggers a new query
    Console.WriteLine(employee.Department?.Name);
}
```

### The Solution: Include

```csharp
var employees = await context.Employees
    .Include(e => e.Department)
    .ToListAsync();
```

This generates a single query with a JOIN:

```sql
SELECT e.*, d.*
FROM Employees e
INNER JOIN Departments d ON e.DepartmentId = d.Id
```

### When to Use Include

- You need the related data for display
- You need to navigate relationships immediately

### When Include Can Be Dangerous

- Loading too many related collections
- Including unrelated data
- Creating very complex queries with multiple joins

> 🟥 **Warning:** Do not add Include to every navigation "just in case." Each Include adds complexity to the query.

---

## 5.9 Instructor Interaction

> 🧠 **Think About It:** "Would you use `Include` or `Select` here?"
>
> ```csharp
> // Option A
> var employees = await context.Employees
>     .Include(e => e.Department)
>     .ToListAsync();
>
> // Option B
> var employees = await context.Employees
>     .Select(e => new { e.Id, e.Name, DepartmentName = e.Department!.Name })
>     .ToListAsync();
> ```
>
> **Answer:** It depends. If you need the full Employee entity for further processing, use `Include`. If you only need those three fields for an API response, use `Select` (projection). Projection is almost always more efficient for read-only scenarios.


---

# Part 6 — Tracking, Projection & Performance

## 6.1 What Is Tracking?

When EF Core retrieves an entity, it **tracks** it. This means EF Core remembers:

- The entity was retrieved
- What its original values are
- What properties have changed

When you call `SaveChangesAsync()`, EF Core compares the current values with the original values and generates the appropriate `UPDATE` statement.

### Example: Tracking in Action

```csharp
// EF Core tracks this employee
var employee = await context.Employees
    .FirstOrDefaultAsync(e => e.Id == 1);

// Modify the tracked entity
employee.Salary = 6000;

// EF Core detects the change and generates UPDATE
await context.SaveChangesAsync();
```

Generated SQL:

```sql
UPDATE Employees SET Salary = 6000 WHERE Id = 1
```

---

## 6.2 When Tracking Is Useful

- You retrieve an entity and plan to **update** it
- You need **change detection**
- You are working within a **unit of work** pattern

## 6.3 When Tracking Is Unnecessary

- **Read-only queries** — you just need to display data
- **API responses** — you return data to the client, you do not update it
- **Reports** — you read data but never write it back

Tracking adds overhead. Every tracked entity is stored in the DbContext's change tracker. For large result sets, this wastes memory.

---

## 6.4 AsNoTracking()

```csharp
var employees = await context.Employees
    .AsNoTracking()
    .ToListAsync();
```

EF Core retrieves the data but does **not** track it. The entities are "disconnected" — changes to them will not be detected.

### When to Use AsNoTracking

```text
Read-only query
      ↓
AsNoTracking()
      ↓
Less memory usage
      ↓
Faster query execution
```

**Typical scenario:** An API endpoint that returns data but does not update it.

### When NOT to Use AsNoTracking

```csharp
// ❌ WRONG — AsNoTracking but then trying to update
var employee = await context.Employees
    .AsNoTracking()
    .FirstOrDefaultAsync(e => e.Id == 1);

employee.Salary = 6000;
await context.SaveChangesAsync();  // ← Nothing happens! EF Core does not track this entity.
```

If you need to update, either:
1. Remove `AsNoTracking()`, or
2. Attach the entity explicitly and mark it as modified

> 💡 **Senior Developer Lesson:** "Do not blindly add `AsNoTracking()` everywhere. Use it for read-only queries. For update operations, you need tracking or an explicit update strategy."

---

## 6.5 The N+1 Query Problem

This is one of the most common performance problems with EF Core.

### What Is N+1?

```text
1 query to get all employees
   +
N queries to get each employee's department
   =
N+1 queries total
```

### Problematic Code

```csharp
var employees = await context.Employees.ToListAsync();  // 1 query

foreach (var employee in employees)
{
    // Each access triggers a NEW database query!
    Console.WriteLine(employee.Department?.Name);  // N queries
}
```

### How Many Queries?

If you have 100 employees, this executes **101 queries** against the database.

```text
Application
   ↓
Query Employees (1)
   ↓
Loop through 100 employees
   ├── Query Department for Employee 1
   ├── Query Department for Employee 2
   ├── Query Department for Employee 3
   ├── ...
   └── Query Department for Employee 100
```

### How to Detect N+1

- Your API is slow with small data but fine with test data
- You see repeated similar queries in logs
- You enable EF Core logging and see unexpected query counts

### Solutions

**Solution 1: Use Include**

```csharp
var employees = await context.Employees
    .Include(e => e.Department)
    .ToListAsync();
```

**Solution 2: Use Projection**

```csharp
var employees = await context.Employees
    .Select(e => new
    {
        e.Id,
        e.Name,
        DepartmentName = e.Department!.Name
    })
    .ToListAsync();
```

> 💡 **Senior Developer Lesson:** "The code can look clean and still generate terrible database traffic. Always think about what queries your code generates."

---

## 6.6 Performance Principles

### 1. Do Not Load Everything

```csharp
// ❌ BAD — loads everything
var employees = await context.Employees.ToListAsync();
var activeEmployees = employees.Where(e => e.IsActive).ToList();

// ✅ GOOD — filter in the database
var activeEmployees = await context.Employees
    .Where(e => e.IsActive)
    .ToListAsync();
```

### 2. Filter in the Database

```text
Database does the filtering
        ↓
Only required rows travel over the network
        ↓
Application processes less data
```

### 3. Project Only What You Need

```csharp
// ❌ BAD — loads all columns
var employees = await context.Employees.ToListAsync();

// ✅ GOOD — loads only needed fields
var employees = await context.Employees
    .Select(e => new EmployeeDto
    {
        Id = e.Id,
        Name = e.Name
    })
    .ToListAsync();
```

### 4. Avoid Unnecessary ToListAsync()

```csharp
// ❌ BAD — materializes too early
var employees = await context.Employees.ToListAsync();
var result = employees.Where(e => e.Salary > 5000).ToList();

// ✅ GOOD — let the database do the filtering
var result = await context.Employees
    .Where(e => e.Salary > 5000)
    .ToListAsync();
```

### 5. Understand Indexes

If you frequently query by a column, that column should be indexed:

```csharp
modelBuilder.Entity<Employee>()
    .HasIndex(e => e.Email)
    .IsUnique();

modelBuilder.Entity<Employee>()
    .HasIndex(e => e.DepartmentId);
```

Without an index, every `WHERE DepartmentId = 5` query scans the entire table.

---

## 6.7 Inspect Generated SQL

During development, you can inspect the SQL EF Core generates:

```csharp
var query = context.Employees
    .Where(e => e.IsActive)
    .Select(e => new { e.Id, e.Name });

var sql = query.ToQueryString();
Console.WriteLine(sql);
```

This prints the actual SQL. Use this during development to:

- Verify your query does what you expect
- Check for unnecessary joins or subqueries
- Debug performance issues

> ⚠️ **Important:** `ToQueryString()` is for development and debugging. Do not use it as a replacement for proper SQL monitoring in production.

---

## 6.8 EF Core Does NOT Replace SQL Knowledge

> 💡 **Senior Developer Lesson:** "If you use EF Core without understanding SQL, eventually EF Core will surprise you."

You should understand:

- **Generated SQL** — what SQL your LINQ produces
- **Indexes** — why certain queries are slow
- **Joins** — how Include and navigation properties work at the SQL level
- **Execution plans** — how SQL Server processes your queries
- **Data volume** — a query that works with 100 rows may fail with 1,000,000 rows

An ORM abstracts SQL; it does not eliminate SQL.

---

## 6.9 EF Core + ASP.NET Core Architecture

```text
HTTP Request
     ↓
Controller
     ↓
Application / Service Layer
     ↓
EF Core (DbContext)
     ↓
SQL Server
```

In a Clean Architecture application, the database logic is not in the Controller. The Controller calls a Service, and the Service uses the DbContext.

> "Today we are learning the database/data-access foundation. Tomorrow we will learn how to organize this code properly using Clean Architecture."

---

# Part 7 — Practical Exercise & Review

## Employee Management API --- Database Layer


### Entities

```csharp
public class Department
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Employee> Employees { get; set; }
        = new List<Employee>();
}
```

```csharp
public class Employee
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public decimal Salary { get; set; }

    public bool IsActive { get; set; } = true;

    public int DepartmentId { get; set; }

    public Department? Department { get; set; }
}
```

### DbContext

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId);

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();
    }
}
```

### Steps

1. Create the entities
2. Create `ApplicationDbContext`
3. Configure SQL Server in `Program.cs`
4. Create initial migration
5. Apply the migration
6. Add departments
7. Add employees
8. Query active employees
9. Query employees by department
10. Sort employees by salary
11. Retrieve one employee by ID
12. Retrieve an employee with department information
13. Project data into a DTO
14. Use `AsNoTracking()` for a read-only query
15. Demonstrate a potential N+1 problem
16. Fix the N+1 problem
17. Inspect generated SQL


---

## Practical Exercise 1 — Basic Queries (Beginner)

### Problem

Write a method that returns all active employees sorted by name.

### Expected Behavior

```csharp
var employees = await GetActiveEmployeesAsync(context);
// Returns: List of active employees, sorted by Name ascending
```

### Starting Code

```csharp
public static async Task<List<Employee>> GetActiveEmployeesAsync(
    ApplicationDbContext context)
{
    // Write your query here
    throw new NotImplementedException();
}
```

### Hints

- Use `Where()` to filter
- Use `OrderBy()` to sort
- Use `ToListAsync()` to execute

### Instructor Solution

```csharp
public static async Task<List<Employee>> GetActiveEmployeesAsync(
    ApplicationDbContext context)
{
    return await context.Employees
        .Where(e => e.IsActive)
        .OrderBy(e => e.Name)
        .ToListAsync();
}
```

### Explanation

- `Where(e => e.IsActive)` filters to only active employees at the database level
- `OrderBy(e => e.Name)` sorts by name
- `ToListAsync()` executes the query and returns results

### Common Mistakes

- Filtering in memory: `.ToListAsync()` first, then `.Where()` loads ALL employees
- Forgetting `OrderBy()` results come back in an unpredictable order

> ًں’، **Senior Developer Note:** "Always filter in the database. Only retrieve the data you actually need."


---

## Practical Exercise 2 — Filtering and Projection (Intermediate)

### Problem

Write a method that returns employee names and their department names for employees earning more than a given salary.

### Expected Behavior

```csharp
var results = await GetHighEarnersAsync(context, 5000);
// Returns: List of { Name, DepartmentName } for employees with Salary > 5000
```

### Starting Code

```csharp
public record HighEarnerDto(string EmployeeName, string DepartmentName);

public static async Task<List<HighEarnerDto>> GetHighEarnersAsync(
    ApplicationDbContext context, decimal minSalary)
{
    // Write your query here
    throw new NotImplementedException();
}
```

### Hints

- Use `Where()` to filter by salary
- Use `Select()` to project into a DTO
- Join with Department through the navigation property

### Instructor Solution

```csharp
public record HighEarnerDto(string EmployeeName, string DepartmentName);

public static async Task<List<HighEarnerDto>> GetHighEarnersAsync(
    ApplicationDbContext context, decimal minSalary)
{
    return await context.Employees
        .Where(e => e.Salary > minSalary)
        .Select(e => new HighEarnerDto(
            e.Name,
            e.Department!.Name))
        .ToListAsync();
}
```

### Explanation

- The query runs entirely in the database
- Only the needed columns are transferred
- The JOIN with Departments is handled by EF Core automatically

### Common Mistakes

- Loading the full entity and then projecting in memory
- Forgetting to handle the nullable Department navigation

> ًں’، **Senior Developer Note:** "Projection is one of the simplest and most effective performance improvements. Use it whenever you only need a subset of fields."


---

## Practical Exercise 3 — Include and N+1 (Intermediate)

### Problem

The following code has an N+1 problem. Find it and fix it.

```csharp
public static async Task DemonstrateNPlus1(ApplicationDbContext context)
{
    var employees = await context.Employees.ToListAsync();

    foreach (var employee in employees)
    {
        Console.WriteLine($"{employee.Name} - {employee.Department?.Name}");
    }
}
```

### Instructor Solution

```csharp
public static async Task FixNPlus1(ApplicationDbContext context)
{
    var employees = await context.Employees
        .Include(e => e.Department)
        .ToListAsync();

    foreach (var employee in employees)
    {
        Console.WriteLine($"{employee.Name} - {employee.Department?.Name}");
    }
}
```

### Alternative: Projection

```csharp
public static async Task FixNPlus1WithProjection(ApplicationDbContext context)
{
    var employees = await context.Employees
        .Select(e => new
        {
            e.Name,
            DepartmentName = e.Department!.Name
        })
        .ToListAsync();

    foreach (var employee in employees)
    {
        Console.WriteLine($"{employee.Name} - {employee.DepartmentName}");
    }
}
```

### Explanation

- `Include()` loads Department in a single query with a JOIN
- Projection also solves N+1 by selecting only what you need in a single query

---

## Practical Exercise 4 — Tracking and Updates (Intermediate)

### Problem

Write a method that updates an employee's salary. Explain whether tracking is needed.

### Instructor Solution

```csharp
public static async Task UpdateSalaryAsync(
    ApplicationDbContext context, int employeeId, decimal newSalary)
{
    // Tracking is needed here - we retrieve and update
    var employee = await context.Employees
        .FirstOrDefaultAsync(e => e.Id == employeeId);

    if (employee is null)
        throw new InvalidOperationException("Employee not found");

    employee.Salary = newSalary;
    await context.SaveChangesAsync();
}
```

### When Tracking Is NOT Needed

```csharp
// Direct update without tracking - use ExecuteUpdate
await context.Employees
    .Where(e => e.Id == employeeId)
    .ExecuteUpdateAsync(s => s.SetProperty(e => e.Salary, newSalary));
```

> ًں’، **Senior Developer Note:** "For simple updates where you do not need the full entity, `ExecuteUpdateAsync` is more efficient. It generates a single UPDATE statement without loading the entity first."


---

## Practical Exercise 5 — Generated SQL Inspection (Advanced)

### Problem

Write code that inspects the SQL generated by the following LINQ query:

```csharp
var query = context.Employees
    .Where(e => e.IsActive)
    .Include(e => e.Department)
    .OrderByDescending(e => e.Salary)
    .Select(e => new
    {
        e.Name,
        e.Salary,
        DepartmentName = e.Department!.Name
    });
```

### Instructor Solution

```csharp
var sql = query.ToQueryString();
Console.WriteLine(sql);
```

### Expected SQL (approximately)

```sql
SELECT [e].[Name], [e].[Salary], [d].[Name] AS [DepartmentName]
FROM [Employees] AS [e]
INNER JOIN [Departments] AS [d] ON [e].[DepartmentId] = [d].[Id]
WHERE [e].[IsActive] = 1
ORDER BY [e].[Salary] DESC
```

### Explanation

- EF Core generates an INNER JOIN because of the navigation property
- The WHERE clause filters active employees
- The ORDER BY sorts by salary descending
- Only the selected columns are returned

> ًں’، **Senior Developer Lesson:** "Always inspect the generated SQL during development. It helps you understand what your LINQ is asking the database to do."


---

# Debugging Challenge

## Find the Problems

The following code contains several realistic mistakes. Find all of them.

```csharp
public class EmployeeService
{
    private readonly ApplicationDbContext _context;

    public EmployeeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Employee>> GetEmployees(int departmentId)
    {
        // Problem 1
        var allEmployees = await _context.Employees.ToListAsync();

        var filtered = allEmployees.Where(e => e.DepartmentId == departmentId).ToList();

        return filtered;
    }

    public async Task<Employee?> GetEmployeeByEmail(string email)
    {
        // Problem 2
        var employees = await _context.Employees.ToListAsync();

        return employees.FirstOrDefault(e => e.Email == email);
    }

    public async Task<List<EmployeeDto>> GetEmployeeDtos()
    {
        // Problem 3
        var employees = await _context.Employees.ToListAsync();

        return employees.Select(e => new EmployeeDto
        {
            Id = e.Id,
            Name = e.Name,
            DepartmentName = e.Department?.Name ?? "Unknown"
        }).ToList();
    }

    public async Task PrintEmployeesWithDepartments()
    {
        // Problem 4
        var employees = await _context.Employees.ToListAsync();

        foreach (var emp in employees)
        {
            // Problem 5
            Console.WriteLine($"{emp.Name} works in {emp.Department?.Name}");
        }
    }

    public async Task<Employee?> FindEmployee(int id)
    {
        // Problem 6
        return await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}
```

### What Is Wrong?

**Problem 1:** Loading ALL employees into memory, then filtering in C#. Should filter in the database with `.Where()`.

**Problem 2:** Loading ALL employees, then searching in memory. Should use `.FirstOrDefaultAsync()` directly.

**Problem 3:** Loading ALL columns, then projecting only 3 fields. Should use `.Select()` for projection.

**Problem 4 & 5:** Loading employees without Include, then accessing Department in a loop. This is an N+1 problem.

**Problem 6:** Using `AsNoTracking()` on a method named `FindEmployee`. If the caller intends to update the returned entity, tracking is needed. If it is read-only, `AsNoTracking()` is correct — but the method name should indicate the intent.

### Corrected Code

```csharp
public class EmployeeService
{
    private readonly ApplicationDbContext _context;

    public EmployeeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Employee>> GetEmployees(int departmentId)
    {
        return await _context.Employees
            .Where(e => e.DepartmentId == departmentId)
            .ToListAsync();
    }

    public async Task<Employee?> GetEmployeeByEmail(string email)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.Email == email);
    }

    public async Task<List<EmployeeDto>> GetEmployeeDtos()
    {
        return await _context.Employees
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                Name = e.Name,
                DepartmentName = e.Department!.Name
            })
            .ToListAsync();
    }

    public async Task PrintEmployeesWithDepartments()
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .ToListAsync();

        foreach (var emp in employees)
        {
            Console.WriteLine($"{emp.Name} works in {emp.Department?.Name}");
        }
    }

    public async Task<Employee?> FindEmployeeReadOnly(int id)
    {
        return await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}
```


---

# Think Like a Senior Developer

## Scenario 1: Slow API

> "There are 500,000 employees and the API takes 8 seconds to respond."

**What would you investigate first?**

### Senior Developer Reasoning

1. **Check the generated SQL.** Is the query loading all 500,000 rows?
2. **Check for missing WHERE clauses.** Are we filtering in the database or in memory?
3. **Check for missing indexes.** Is the WHERE column indexed?
4. **Check for unnecessary Includes.** Are we loading related data we do not need?
5. **Check for N+1 problems.** Are we making excessive database calls?

Most likely cause: loading all 500,000 rows into memory, then filtering in C#.

---

## Scenario 2: Over-Fetching

> "The API only needs Id, Name, and Department Name, but the developer loads the complete Employee entity with all columns."

**Is this a problem?**

### Senior Developer Reasoning

Yes. It wastes:

- **Database resources** — reading columns that are not needed
- **Network bandwidth** — transferring unnecessary data
- **Memory** — storing unnecessary properties
- **API response size** — if serialized, the response is larger than needed

The fix is projection with `Select()`.

---

## Scenario 3: Over-Using Include

> "A developer uses Include for every navigation property on every query."

**Is more data always better?**

### Senior Developer Reasoning

No. Each Include:

- Adds a JOIN to the query
- Increases query complexity
- Loads data that may not be needed
- Slows down the query

Only Include what you actually use. If you only need the department name, use Select with projection instead.

---

## Scenario 4: Data Volume Changes Everything

> "The query works perfectly with 100 records but becomes extremely slow with 2 million records."

### Senior Developer Reasoning

Data volume changes the engineering problem.

- With 100 records, almost any approach works
- With 2 million records, every inefficiency is amplified
- Missing indexes become critical
- Loading unnecessary data becomes expensive
- N+1 queries become catastrophic

> ًں’، **Senior Developer Lesson:** "Always test with realistic data volumes. What works in development may fail in production."


---

# Common Beginner Mistakes

## 1. Treating DbContext as a Permanent Connection

**What developers do:**
```csharp
// Singleton — shared across all requests, threads
services.AddSingleton<ApplicationDbContext>();
```

**Why it is a problem:** DbContext is not thread-safe. Sharing it causes data corruption and runtime exceptions.

**Better approach:**
```csharp
services.AddDbContext<ApplicationDbContext>(...); // Scoped — one per request
```

> ًں’، **Senior Developer Lesson:** "DbContext is short-lived. One instance per request. Always."

---

## 2. Loading All Data

**What developers do:**
```csharp
var employees = await context.Employees.ToListAsync();
var active = employees.Where(e => e.IsActive).ToList();
```

**Why it is a problem:** Loads every row from the database into memory.

**Better approach:**
```csharp
var active = await context.Employees
    .Where(e => e.IsActive)
    .ToListAsync();
```

---

## 3. Filtering in Memory

**What developers do:**
```csharp
var allData = await context.Employees.ToListAsync();
var result = allData.Where(e => e.Salary > 5000).OrderBy(e => e.Name).ToList();
```

**Why it is a problem:** The database is much better at filtering and sorting than your application.

**Better approach:**
```csharp
var result = await context.Employees
    .Where(e => e.Salary > 5000)
    .OrderBy(e => e.Name)
    .ToListAsync();
```

---

## 4. Calling ToListAsync Too Early

**What developers do:**
```csharp
var employees = await context.Employees.ToListAsync();
var count = employees.Count;
```

**Why it is a problem:** Materializes the entire result set when you only needed a count.

**Better approach:**
```csharp
var count = await context.Employees.CountAsync();
```

---

## 5. Overusing Include

**What developers do:**
```csharp
var employees = await context.Employees
    .Include(e => e.Department)
    .Include(e => e.Projects)
    .Include(e => e.PerformanceReviews)
    .ToListAsync();
```

**Why it is a problem:** Loads related data you may never use. Each Include adds a JOIN.

**Better approach:** Only Include what you actually need. Use Select for projection when possible.

---

## 6. N+1 Queries in Loops

**What developers do:**
```csharp
var employees = await context.Employees.ToListAsync();
foreach (var emp in employees)
{
    var dept = await context.Departments.FindAsync(emp.DepartmentId);
}
```

**Why it is a problem:** One query for employees + N queries for departments = N+1 total queries.

**Better approach:**
```csharp
var employees = await context.Employees
    .Include(e => e.Department)
    .ToListAsync();
```

---

## 7. Using SingleOrDefault When Uniqueness Is Not Guaranteed

**What developers do:**
```csharp
var emp = await context.Employees
    .SingleOrDefaultAsync(e => e.DepartmentId == departmentId);
```

**Why it is a problem:** Multiple employees can belong to the same department. This throws if more than one exists.

**Better approach:**
```csharp
var emp = await context.Employees
    .FirstOrDefaultAsync(e => e.DepartmentId == departmentId);
```

---

## 8. Not Understanding Tracking

**What developers do:**
```csharp
var emp = await context.Employees
    .AsNoTracking()
    .FirstOrDefaultAsync(e => e.Id == id);

emp.Salary = 7000;
await context.SaveChangesAsync(); // Does nothing!
```

**Why it is a problem:** AsNoTracking means EF Core does not track the entity. Changes are not detected.

**Better approach:** Remove AsNoTracking if you plan to update, or use ExecuteUpdateAsync.

---

## 9. Assuming LINQ Always Means In-Memory

**What developers do:**
```csharp
var list = await context.Employees.ToListAsync();
var expensive = list.Where(e => SomeComplexMethod(e)).ToList();
```

**Why it is a problem:** If SomeComplexMethod cannot be translated to SQL, EF Core may load everything into memory first.

**Better approach:** Understand what LINQ operations can be translated to SQL and which cannot.

---

## 10. Making Database Queries Inside Loops

**What developers do:**
```csharp
foreach (var id in employeeIds)
{
    var emp = await context.Employees.FindAsync(id);
    ProcessEmployee(emp);
}
```

**Why it is a problem:** Each FindAsync is a separate database round-trip.

**Better approach:**
```csharp
var employees = await context.Employees
    .Where(e => employeeIds.Contains(e.Id))
    .ToListAsync();

foreach (var emp in employees)
{
    ProcessEmployee(emp);
}
```


---

# Senior Developer Notes

Throughout this session, keep these principles in mind:

> "EF Core is a tool. SQL is still the language of the database."

> "A clean LINQ query can still produce an expensive SQL query."

> "Don't optimize based on fear. Optimize based on evidence."

> "Always understand where your query executes — database or memory?"

> "Retrieve only the data you actually need."

> "The database is part of your application's performance."

> "An ORM abstracts SQL; it does not eliminate SQL."

> "Code First means the C# code is the source of truth."

> "Migrations are the version history of your database schema."

> "Tracking is useful for updates. Skip it for read-only queries."

> "Projection is the simplest performance win."

> "N+1 queries are the silent killer of API performance."

---

# Comparison Tables

## EF Core Concepts

| Concept | Meaning | Example |
|---------|---------|---------|
| Entity | C# representation of data | `Employee` |
| DbSet | Entry point for entity queries | `context.Employees` |
| DbContext | Database session / unit of work | `ApplicationDbContext` |
| Migration | Schema change description | `InitialCreate` |
| Navigation Property | Related entity reference | `Employee.Department` |
| Foreign Key | Links entities | `Employee.DepartmentId` |
| Primary Key | Unique identifier | `Employee.Id` |
| Projection | Selecting specific fields | `.Select(e => new { e.Name })` |
| Include | Loading related data | `.Include(e => e.Department)` |

## Tracking vs NoTracking

| Feature | Tracking | NoTracking |
|---------|----------|------------|
| Change tracking | Yes | No |
| Read-only scenarios | Sometimes unnecessary | Excellent |
| Update scenarios | Useful | Requires care |
| Overhead | Higher | Lower |
| Memory usage | Higher (change tracker stores originals) | Lower |
| Use case | When you plan to update | When you only read |

## Execution Methods

| Method | Returns | Throws If | Use When |
|--------|---------|-----------|----------|
| `FirstOrDefaultAsync` | First match or null | Never (returns null) | You want one result |
| `SingleOrDefaultAsync` | Single match or null | More than one match | Uniqueness is expected |
| `FirstAsync` | First match | No match found | You expect at least one |
| `SingleAsync` | Single match | Zero or more than one | Exactly one must exist |
| `AnyAsync` | bool | Never | Check existence |
| `CountAsync` | int | Never | Count records |
| `ToListAsync` | List | Never | Get all results |

## Relationship Types

| Relationship | SQL Representation | Example |
|-------------|-------------------|---------|
| One-to-One | FK + Unique constraint | Employee - EmployeeProfile |
| One-to-Many | FK on many side | Department - Employees |
| Many-to-Many | Join table | Employee - Project |

## Configuration Approaches

| Approach | Best For | Limitations |
|----------|----------|-------------|
| Data Annotations | Simple constraints (`[Required]`, `[MaxLength]`) | Limited power |
| Fluent API | Relationships, indexes, complex config | More verbose |
| Conventions | Default mappings | Limited to naming conventions |


---

# Mermaid Diagrams

## 1. EF Core Architecture

```text
Application (Controller / Service)
        |
        v
    DbContext
        |
        v
    EF Core Engine
        |
        v
  Database Provider (SQL Server)
        |
        v
    SQL Server
```

## 2. Code First Flow

```text
C# Entity Classes
        |
        v
  EF Core Model
        |
        v
    Migration
        |
        v
  SQL Schema Changes
        |
        v
    SQL Server
```

## 3. LINQ Query Execution

```text
LINQ (C#)
   |
   v
IQueryable<Employee>
   |
   v
EF Core Expression Tree
   |
   v
SQL Generation
   |
   v
SQL Server
   |
   v
Rows
   |
   v
C# Objects
```

## 4. N+1 Problem

```text
Application
   |
   v
Query Employees (1 query)
   |
   v
Loop through N employees
   |
   +---> Query Department for Employee 1
   +---> Query Department for Employee 2
   +---> Query Department for Employee 3
   +---> ...
   +---> Query Department for Employee N
```

Total: 1 + N queries

## 5. Include Solution

```text
Application
   |
   v
Query Employees with Include(Department)
   |
   v
Single SQL with JOIN
   |
   v
Results with Department loaded
```

Total: 1 query

## 6. DbContext Lifecycle

```text
Request arrives
   |
   v
DI creates DbContext (Scoped)
   |
   v
LINQ queries translated to SQL
   |
   v
Results materialized as objects
   |
   v
Changes tracked automatically
   |
   v
SaveChangesAsync writes to DB
   |
   v
Request ends
   |
   v
DbContext disposed
```


---

# Knowledge Check

## Questions

### Question 1 — Multiple Choice

When does a LINQ-to-EF Core query normally execute?

A. When you write the LINQ expression  
B. When you call an execution method like `ToListAsync()`  
C. When the DbContext is created  
D. When the request begins  

---

### Question 2 — True/False

Calling `ToListAsync()` before `Where()` means the filtering happens in the database.

---

### Question 3 — Code Analysis

What is wrong with this code?

```csharp
var employees = await context.Employees.ToListAsync();
var result = employees.Where(e => e.Salary > 5000).ToList();
```

---

### Question 4 — Multiple Choice

When is `AsNoTracking()` useful?

A. When you plan to update the entity  
B. When the query is read-only  
C. When you need change tracking  
D. When you are using Include  

---

### Question 5 — Multiple Choice

What problem does `Include()` solve?

A. N+1 query problem  
B. Missing indexes  
C. Slow network  
D. Memory leaks  

---

### Question 6 — True/False

`SingleOrDefaultAsync()` throws an exception if more than one record matches.

---

### Question 7 — Code Analysis

How many database queries does this code execute?

```csharp
var employees = await context.Employees.ToListAsync();
foreach (var emp in employees)
{
    Console.WriteLine(emp.Department?.Name);
}
```

---

### Question 8 — Multiple Choice

Why is projection often preferable to loading a complete entity?

A. Projection is easier to write  
B. Projection transfers only needed data  
C. Projection uses less CPU  
D. Projection avoids SQL  

---

### Question 9 — True/False

EF Core eliminates the need to understand SQL.

---

### Question 10 — Multiple Choice

What is the default lifetime of DbContext in ASP.NET Core?

A. Singleton  
B. Transient  
C. Scoped  
D. Static  

---

### Question 11 — Code Analysis

What is the issue?

```csharp
var emp = await context.Employees
    .AsNoTracking()
    .FirstOrDefaultAsync(e => e.Id == id);

emp.Salary = 7000;
await context.SaveChangesAsync();
```

---

### Question 12 — Multiple Choice

Which generates more efficient SQL for a read-only API response?

A. `Include()` + `ToListAsync()`  
B. `Select()` + `ToListAsync()`  
C. Both are equal  
D. Neither works  

---

### Question 13 — What Would You Choose?

You need to display employee name and department name. Which approach do you choose?

A. Load full Employee entity with Include  
B. Project with Select into a DTO  

---

### Question 14 — SQL/LINQ Reasoning

What SQL does this LINQ generate?

```csharp
await context.Employees
    .Where(e => e.IsActive)
    .OrderBy(e => e.Name)
    .ToListAsync();
```

---

### Question 15 — Performance Scenario

Your API returns 10,000 employees. It takes 12 seconds. What do you check first?

---

## Answers

### Answer 1

**B.** The query executes when you call an execution method like `ToListAsync()`. Building a LINQ expression does not hit the database.

### Answer 2

**False.** Calling `ToListAsync()` first materializes ALL records into memory. The subsequent `Where()` filters in memory, not in the database.

### Answer 3

Two problems:
1. Loads ALL employees into memory before filtering
2. Filtering happens in C#, not in the database

Better: `await context.Employees.Where(e => e.Salary > 5000).ToListAsync()`

### Answer 4

**B.** When the query is read-only. AsNoTracking skips change tracking, reducing overhead.

### Answer 5

**A.** Include solves the N+1 query problem by loading related data in a single query with a JOIN.

### Answer 6

**True.** SingleOrDefaultAsync throws `InvalidOperationException` if more than one record matches.

### Answer 7

**1 + N queries.** 1 query for employees, then N queries for each employee's department. This is the N+1 problem.

### Answer 8

**B.** Projection transfers only needed data — fewer columns, less memory, smaller response.

### Answer 9

**False.** EF Core generates SQL. You need to understand that SQL to optimize queries and debug issues.

### Answer 10

**C.** Scoped — one instance per HTTP request. DbContext is not thread-safe.

### Answer 11

AsNoTracking means EF Core does not track the entity. The update to `Salary` is not detected. `SaveChangesAsync()` does nothing.

### Answer 12

**B.** `Select()` with projection is more efficient — it only transfers the columns the API needs.

### Answer 13

**B.** Projection with Select. It transfers only the two needed fields, not the entire entity.

### Answer 14

```sql
SELECT [e].[Id], [e].[Name], [e].[Email], [e].[Salary], [e].[DepartmentId]
FROM [Employees] AS [e]
WHERE [e].[IsActive] = 1
ORDER BY [e].[Name]
```

### Answer 15

1. Check if all 10,000 rows are being loaded (no WHERE clause)
2. Check for missing indexes on filtered columns
3. Check for unnecessary Includes
4. Check for N+1 queries in loops
5. Check if projection is used


---

# Final Practical Challenge

## Build a Production-Style Employee Query API

### Requirements

Build a console application that demonstrates all concepts learned today.

**Entities:**
- Employee (Id, Name, Email, Salary, IsActive, DepartmentId, Department)
- Department (Id, Name, Employees)

**Requirements:**
1. SQL Server with EF Core
2. At least 3 departments with 10+ employees each
3. Filter active employees
4. Sort by salary descending
5. Project into DTOs (not full entities)
6. Use Include for one query, Select for another
7. Use AsNoTracking for a read-only query
8. Demonstrate N+1 problem and fix it
9. Inspect generated SQL
10. Async queries with CancellationToken

### Starting Point

```csharp
using Microsoft.EntityFrameworkCore;

// Entity classes
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public bool IsActive { get; set; } = true;
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
}

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Employee> Employees { get; set; }
        = new List<Employee>();
}

// DbContext
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();
    }
}

// DTO
public record EmployeeDto(int Id, string Name, string DepartmentName, decimal Salary);
```

### Your Task

Implement the following methods:

```csharp
public class EmployeeQueryService
{
    private readonly ApplicationDbContext _context;

    public EmployeeQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. Get all active employees sorted by name
    public async Task<List<Employee>> GetActiveEmployeesAsync(
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    // 2. Get high earners as DTOs (projection)
    public async Task<List<EmployeeDto>> GetHighEarnersAsync(
        decimal minSalary,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    // 3. Get employee with department (Include)
    public async Task<Employee?> GetEmployeeWithDepartmentAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    // 4. Search employees by name (LIKE query)
    public async Task<List<Employee>> SearchByNameAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    // 5. Get employee count per department
    public async Task<List<object>> GetEmployeeCountPerDepartmentAsync(
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
```

### Hints

1. Use `.Where(e => e.IsActive).OrderBy(e => e.Name)`
2. Use `.Where(e => e.Salary > minSalary).Select(e => new EmployeeDto(...))`
3. Use `.Include(e => e.Department).FirstOrDefaultAsync(e => e.Id == id)`
4. Use `.Where(e => e.Name.Contains(searchTerm))`
5. Use `.GroupBy(e => e.DepartmentId).Select(g => new { ... })`

### How to Run

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Seed data, run queries, etc.
```

---

> "Today we are learning the database/data-access foundation. Tomorrow we will learn how to organize this code properly using Clean Architecture."


---

# Final Summary

## What We Covered Today

```text
ORM (why it exists)
  |
  v
EF Core (Microsoft's ORM)
  |
  v
DbContext (database session)
  |
  v
Entities / DbSet (C# classes map to tables)
  |
  v
Code First (C# is the source of truth)
  |
  v
Migrations (schema versioning)
  |
  v
Relationships (One-to-One, One-to-Many, Many-to-Many)
  |
  v
Fluent API / Data Annotations (configuration)
  |
  v
LINQ with EF Core (querying)
  |
  v
Query Execution (when does the SQL run?)
  |
  v
Tracking / NoTracking (change detection)
  |
  v
Projection (select only what you need)
  |
  v
Include (loading related data)
  |
  v
Performance (filter in DB, project, avoid N+1)
```

## What You Should Be Able To Do Now

- [ ] Explain what EF Core is and why ORMs exist
- [ ] Create a DbContext with DbSet properties
- [ ] Configure SQL Server in an ASP.NET Core application
- [ ] Create and apply database migrations
- [ ] Model One-to-One, One-to-Many, and Many-to-Many relationships
- [ ] Configure relationships with Fluent API and Data Annotations
- [ ] Write LINQ queries that translate to SQL
- [ ] Understand when a query executes (execution methods)
- [ ] Use projection with Select to retrieve only needed data
- [ ] Use Include to load related data and avoid N+1
- [ ] Understand the difference between Tracking and AsNoTracking
- [ ] Identify and fix N+1 query problems
- [ ] Inspect generated SQL with ToQueryString()
- [ ] Think about query performance in production

---

# Preview of Day 4

## Tomorrow — Advanced Web API

Now that you understand how to work with the database using EF Core, we will learn how to build a complete, production-style Web API.

**Topics:**
- Filtering and searching with query parameters
- Sorting with dynamic order-by
- Pagination for large datasets
- Input validation with Data Annotations and FluentValidation
- Error handling with middleware and exception handlers
- Proper HTTP status codes
- API versioning preview
- Building a complete Employee Management API

> "Today you learned how to talk to the database. Tomorrow you will learn how to expose that data through a professional API."

---

*End of Day 3 — Entity Framework Core*

