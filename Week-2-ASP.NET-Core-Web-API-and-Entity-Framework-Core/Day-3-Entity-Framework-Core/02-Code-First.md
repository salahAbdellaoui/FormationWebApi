# 02 — Code First

**Duration:** ~30 min

---

## What You'll Learn

By the end of this topic, you will:

- Understand the Code First approach
- Create entity classes for Department and Employee
- Understand EF Core conventions (Primary Key, Foreign Key)
- See how C# classes become database tables

---

## What Is Code First?

Code First means you write C# classes, and EF Core creates the database from them.

```text
C# Entity Classes
    |
    v
EF Core reads the classes
    |
    v
EF Core builds a model
    |
    v
Database tables are created
```

The alternative is Database First (create tables first, generate classes from them). We use Code First in this training.

---

## Why Code First?

- Your C# code is the single source of truth
- Schema changes follow code changes
- Migrations track every change automatically
- No manual SQL to maintain

---

## Step 1: Create the Department Entity

The Day 1 and Day 2 `Employee` model has `Department` as a plain string. We now make `Department` a separate entity with its own table.

Create `Models/Department.cs`:

```csharp
namespace EmployeeManagement.Api.Models;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

**EF Core conventions applied:**

| Convention | How it applies |
|------------|---------------|
| Property named `Id` becomes the Primary Key | `Id` is the PK |
| `string` maps to `nvarchar` in SQL Server | `Name` becomes `nvarchar(max)` |

---

## Step 2: Update the Employee Entity

Replace the existing `Models/Employee.cs` with:

```csharp
namespace EmployeeManagement.Api.Models;

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

**What changed?**

| Change | Reason |
|--------|--------|
| Added `Email` | New field |
| Added `Salary` | New field (decimal for money) |
| Removed `Department` (string) | Department is now a separate entity |
| Added `DepartmentId` | Foreign Key — links Employee to Department |
| Added `Department` (navigation) | Navigation property — accesses the related Department |

---

## Step 3: Understand the Conventions

EF Core uses **conventions** to figure out the database structure from your classes.

### Primary Key Convention

A property named `Id` or `<ClassName>Id` becomes the Primary Key.

```csharp
public class Department
{
    public int Id { get; set; }   // PK — convention
}
```

### Foreign Key Convention

A property named `<NavigationPropertyName>Id` becomes a Foreign Key.

```csharp
public class Employee
{
    public int DepartmentId { get; set; }        // FK — convention
    public Department Department { get; set; }    // navigation property
}
```

EF Core sees `DepartmentId` and knows it points to `Department.Id`.

### Navigation Property Convention

A property whose type is another entity becomes a navigation property.

```csharp
public Department Department { get; set; }   // navigation to Department
```

The `= null!;` tells the compiler "this will be set by EF Core, not null in practice."

---

## Step 4: See the Full Picture

After these two classes, EF Core understands:

```text
Department table:
    Id          INT         PRIMARY KEY (identity)
    Name        NVARCHAR

Employee table:
    Id          INT         PRIMARY KEY (identity)
    Name        NVARCHAR
    Email       NVARCHAR
    Salary      DECIMAL
    DepartmentId INT        FOREIGN KEY → Department(Id)
```

No SQL was written. EF Core inferred everything from conventions.

---

## What About the Old Employee Model?

The Day 2 `Employee` had:

```csharp
public int Id { get; set; }
public string Name { get; set; } = string.Empty;
public string Department { get; set; } = string.Empty;
```

The Day 3 `Employee` replaces this. The `Department` string property becomes:

- A `Department` entity with its own table
- A `DepartmentId` foreign key on `Employee`
- A `Department` navigation property on `Employee`

This is a schema change. The old DTOs also need updating to match.

---

## Step 5: Update the DTOs

Update `Dtos/EmployeeDto.cs`:

```csharp
namespace EmployeeManagement.Api.Dtos;

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

Update `Dtos/CreateEmployeeDto.cs`:

```csharp
namespace EmployeeManagement.Api.Dtos;

public class CreateEmployeeDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public int DepartmentId { get; set; }
}
```

Update `Dtos/UpdateEmployeeDto.cs`:

```csharp
namespace EmployeeManagement.Api.Dtos;

public class UpdateEmployeeDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public int DepartmentId { get; set; }
}
```

**What changed?**

| Before | After |
|--------|-------|
| `Department` (string) | `DepartmentId` (int FK) |
| No email | `Email` field |
| No salary | `Salary` field |
| | `DepartmentName` included in response DTO only |

---

## Common Mistakes

**Mistake 1: Missing navigation property.**

Having `DepartmentId` without the `Department` navigation property works, but makes it harder to access related data. Always include the navigation property.

**Mistake 2: Forgetting `= null!;` on the navigation property.**

Without it, the compiler warns about a possible null reference. The `null!` is a hint to the compiler — EF Core sets this value when loading data.

**Mistake 3: Using `string` for money.**

Always use `decimal` for monetary values. `double` and `float` lose precision.

---

## Knowledge Check

1. What is Code First?
2. Which property convention makes `Id` the Primary Key?
3. How does EF Core know that `DepartmentId` is a Foreign Key?
4. What is a navigation property?
5. Why use `decimal` for `Salary`?

---

## Next

We have entity classes. Next, we create a migration to tell EF Core to generate the database tables.
