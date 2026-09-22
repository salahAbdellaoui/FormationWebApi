# 01 — Database Design and Relational Concepts

---

## Why Do We Need a Database?

Yesterday you stored data in C# collections:

```csharp
List<Employee> employees = new List<Employee>();
```

This works while the program is running. But what happens when the program stops?

> The data is gone.

A **database** stores data permanently. It survives program restarts, server reboots, and power failures.

---

## What Is a Database?

A **database** is an organized collection of data.

A **DBMS** (Database Management System) is the software that manages the database.

**SQL Server** is Microsoft's DBMS. It stores data in tables and lets you query it using SQL.

```text
Your Application
       ↓
   SQL (language)
       ↓
   SQL Server (DBMS)
       ↓
   Database (tables with data)
```

---

## What Is a Relational Database?

A **relational database** stores data in **tables** that can be related to each other.

Think of a spreadsheet:

```text
Employees
------------------------------------------------
| EmployeeId | FullName  | Salary | DepartmentId |
------------------------------------------------
| 1          | Ali       | 1800   | 2            |
| 2          | Sara      | 2200   | 1            |
| 3          | Omar      | 1900   | 2            |
------------------------------------------------
```

Each table has:

- **Rows** — one row = one record (one employee)
- **Columns** — one column = one piece of information (name, salary, etc.)

---

## Design Thinking

Before writing any SQL, think about the data.

### The Business Requirement

> "A company has departments. Each employee belongs to one department. Employees have positions and salaries."

### Step 1 — Identify Entities

What things do we need to track?

- Employees
- Departments
- Positions

### Step 2 — Identify Attributes

What information does each entity need?

- **Employee:** name, salary, hire date
- **Department:** name, location
- **Position:** title, level

### Step 3 — Identify Relationships

How are entities connected?

- An employee **belongs to** a department
- An employee **has** a position
- A department **contains** many employees

### Step 4 — Design Tables

```text
Departments          Positions           Employees
-----------          ---------           --------
DepartmentId         PositionId          EmployeeId
DepartmentName       Title               FullName
Location             Level               Salary
                                         DepartmentId
                                         PositionId
```

> 💡 **Senior Developer Note:** Always design your data model before writing queries. A good design makes every query easier. A bad design makes every query painful.

---

## 🤔 Think

> Should we store the department name inside every employee row?

For example:

```text
EmployeeId | FullName | DepartmentName
1          | Ali      | IT
2          | Sara     | HR
3          | Omar     | IT
```

**Problem:** If the IT department changes its name, we must update every employee row. And if we forget one, the data becomes inconsistent.

> This is why we use separate tables and connect them with keys.

---

## Key Concepts

| Concept | What It Is |
|---------|-----------|
| **Table** | A collection of related data in rows and columns |
| **Row** | One record in a table |
| **Column** | One field/attribute in a table |
| **Primary Key** | A column that uniquely identifies each row |
| **Foreign Key** | A column that connects to another table's primary key |
| **SQL** | The language used to query and modify data |

---

## SQL Server Data Types (Common)

| Data Type | Use For | Example |
|-----------|---------|---------|
| `INT` | Whole numbers | 42 |
| `BIGINT` | Large whole numbers | 9999999 |
| `DECIMAL(10,2)` | Money / precise numbers | 1800.50 |
| `NVARCHAR(100)` | Text (variable length) | 'Ali' |
| `VARCHAR(100)` | Text (ASCII only) | 'Ali' |
| `BIT` | True/false | 1 or 0 |
| `DATE` | Date only | '2024-01-15' |
| `DATETIME` | Date and time | '2024-01-15 10:30:00' |

> Use `NVARCHAR` for text that might contain special characters or non-English text.

---

## Summary

| Concept | Simple Meaning |
|---------|---------------|
| Database | Organized permanent storage |
| Table | A spreadsheet-like structure |
| Row | One record |
| Column | One field |
| SQL Server | Microsoft's database management system |
| SQL | The language to talk to the database |

> 💡 **Senior Developer Note:** Good database design is 80% of the work. If the design is right, queries are simple. If the design is wrong, queries become complicated hacks.

---

**Next: [02 — Tables, Keys and Constraints](02-Tables-Keys-and-Constraints.md)**
