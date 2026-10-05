# 04 — Relationships

**Duration:** ~30 min

---

## What You'll Learn

By the end of this topic, you will:

- Understand a one-to-many relationship
- Define Primary Key and Foreign Key
- Use navigation properties
- See how EF Core manages relationships

---

## The Relationship

```text
Department  1  --------  *  Employee
```

One Department has many Employees. Each Employee belongs to one Department.

```text
+------------------+          +---------------------+
|   Department     |          |     Employee        |
+------------------+          +---------------------+
| Id        (PK)   |          | Id           (PK)   |
| Name             |          | Name                |
+------------------+          | Email               |
        ^                     | Salary              |
        |                     | DepartmentId (FK)---+--- points to
        |                     +---------------------+    Department.Id
        |                            |
        +----------------------------+
         one department
         has many employees
```

---

## How It Works in Code

### Department Entity (the "one" side)

```csharp
public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Employee> Employees { get; set; }
        = new List<Employee>();
}
```

| Property | Role |
|----------|------|
| `Id` | Primary Key |
| `Name` | Data |
| `Employees` | **Collection navigation property** — access all employees in this department |

The `= new List<Employee>()` initializes the collection so it is never null.

### Employee Entity (the "many" side)

```csharp
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

| Property | Role |
|----------|------|
| `Id` | Primary Key |
| `DepartmentId` | **Foreign Key** — stores the Department's Id |
| `Department` | **Reference navigation property** — access the parent department |

---

## The Three Parts of a Relationship

Every EF Core relationship has three pieces:

```text
1. Primary Key       →  Department.Id
2. Foreign Key       →  Employee.DepartmentId
3. Navigation        →  Employee.Department  and  Department.Employees
```

| Part | Where | Purpose |
|------|-------|---------|
| Primary Key | `Department.Id` | Uniquely identifies a department |
| Foreign Key | `Employee.DepartmentId` | Links an employee to a department |
| Navigation (reference) | `Employee.Department` | Access the parent object |
| Navigation (collection) | `Department.Employees` | Access all child objects |

---

## How EF Core Discovers the Relationship

EF Core uses conventions:

1. It sees `DepartmentId` on `Employee`
2. It sees `Department` navigation property on `Employee`
3. It sees `Employees` collection navigation on `Department`
4. It matches `DepartmentId` to `Department.Id` by naming convention

No Fluent API configuration needed. Conventions handle everything for this simple relationship.

---

## Visual Summary

```text
Departments Table          Employees Table
+----+----------+         +----+-------+-------+--------+--------------+
| Id | Name     |         | Id | Name  | Email  | Salary | DepartmentId |
+----+----------+         +----+-------+-------+--------+--------------+
| 1  | IT       |  <---+  | 1  | Alice | a@x.c  | 5000   | 1            |
| 2  | HR       |      |  | 2  | Bob   | b@x.c  | 4000   | 1            |
+----+----------+      |  | 3  | Carol | c@x.c  | 4500   | 2            |
                       |  +----+-------+-------+--------+--------------+
                       |
                       |  FK: Employees.DepartmentId → Departments.Id
                       |
                       +-- One department can appear many times in DepartmentId
```

---

## Navigating the Relationship

Once data is loaded, you can navigate in both directions:

```csharp
// From employee to department
var employee = await context.Employees
    .Include(e => e.Department)
    .FirstAsync(e => e.Id == 1);

string deptName = employee.Department.Name;   // "IT"

// From department to employees
var department = await context.Departments
    .Include(d => d.Employees)
    .FirstAsync(d => d.Id == 1);

int count = department.Employees.Count;       // 2
```

The `Include()` method tells EF Core to load the related data. This is covered in detail in Topic 05.

---

## Common Mistakes

**Mistake 1: Forgetting the foreign key property.**

Having only the navigation property (`Department`) without the FK (`DepartmentId`) works, but makes it harder to set relationships and query by FK.

**Mistake 2: Setting the navigation instead of the FK.**

When creating an employee, set `DepartmentId`, not the `Department` object:

```csharp
// Correct
var employee = new Employee
{
    Name = "Alice",
    Email = "alice@company.com",
    Salary = 5000,
    DepartmentId = 1     // set the FK
};

// Wrong (unless Department is already tracked by EF Core)
var employee = new Employee
{
    Name = "Alice",
    Department = new Department { Id = 1 }   // confusing, avoid this
};
```

**Mistake 3: Not initializing the collection.**

Without `= new List<Employee>()`, the `Employees` collection is null until EF Core loads it. Initializing it avoids null reference exceptions.

---

## Knowledge Check

1. What is a one-to-many relationship?
2. What is the Primary Key of `Department`?
3. What is the Foreign Key on `Employee`?
4. What is the difference between a reference navigation and a collection navigation?
5. How does EF Core discover the relationship without explicit configuration?

---

## Next

The database has tables with a relationship. Next, we write LINQ queries to read data through EF Core.
