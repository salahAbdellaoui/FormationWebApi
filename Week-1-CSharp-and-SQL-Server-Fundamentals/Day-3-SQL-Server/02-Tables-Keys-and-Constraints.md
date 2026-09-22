# 02 — Tables, Keys and Constraints

---

## Creating a Table

```sql
CREATE TABLE Employees
(
    EmployeeId   INT PRIMARY KEY,
    FullName     NVARCHAR(100) NOT NULL,
    Salary       DECIMAL(10,2),
    DepartmentId INT,
    HireDate     DATE
);
```

This creates a table called `Employees` with 5 columns.

---

## Primary Key

A **primary key** uniquely identifies each row in a table.

```sql
EmployeeId INT PRIMARY KEY
```

Rules:

- Must be unique (no two rows can have the same value)
- Cannot be NULL
- Each table should have exactly one primary key

### Identity Columns

For auto-incrementing IDs:

```sql
EmployeeId INT IDENTITY(1,1) PRIMARY KEY
```

- `IDENTITY(1,1)` — starts at 1, increases by 1 for each new row
- SQL Server generates the value automatically

```sql
INSERT INTO Employees (FullName, Salary, DepartmentId)
VALUES ('Ali', 1800, 2);
-- EmployeeId is generated automatically
```

---

## Foreign Key

A **foreign key** connects one table to another.

```sql
DepartmentId INT
FOREIGN KEY REFERENCES Departments(DepartmentId)
```

This ensures:

- You cannot add an employee with a non-existent department
- You cannot delete a department that has employees (unless you configure cascade)

```sql
CREATE TABLE Employees
(
    EmployeeId   INT IDENTITY(1,1) PRIMARY KEY,
    FullName     NVARCHAR(100) NOT NULL,
    Salary       DECIMAL(10,2),
    DepartmentId INT,
    PositionId   INT,
    HireDate     DATE DEFAULT GETDATE(),
    FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId),
    FOREIGN KEY (PositionId) REFERENCES Positions(PositionId)
);
```

---

## Constraints

| Constraint | What It Does | Example |
|------------|-------------|---------|
| `PRIMARY KEY` | Uniquely identifies each row | `EmployeeId INT PRIMARY KEY` |
| `FOREIGN KEY` | Connects to another table | `FOREIGN KEY REFERENCES Departments(DepartmentId)` |
| `NOT NULL` | Requires a value | `FullName NVARCHAR(100) NOT NULL` |
| `UNIQUE` | No duplicate values | `Email NVARCHAR(100) UNIQUE` |
| `CHECK` | Restricts allowed values | `Salary DECIMAL(10,2) CHECK (Salary > 0)` |
| `DEFAULT` | Provides a default value | `HireDate DATE DEFAULT GETDATE()` |

---

## Creating the Full Database

```sql
CREATE TABLE Departments
(
    DepartmentId   INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL UNIQUE,
    Location       NVARCHAR(100)
);

CREATE TABLE Positions
(
    PositionId   INT IDENTITY(1,1) PRIMARY KEY,
    Title        NVARCHAR(100) NOT NULL,
    Level        NVARCHAR(50)
);

CREATE TABLE Employees
(
    EmployeeId   INT IDENTITY(1,1) PRIMARY KEY,
    FullName     NVARCHAR(100) NOT NULL,
    Email        NVARCHAR(100) UNIQUE,
    Salary       DECIMAL(10,2) CHECK (Salary > 0),
    DepartmentId INT,
    PositionId   INT,
    HireDate     DATE DEFAULT GETDATE(),
    FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId),
    FOREIGN KEY (PositionId) REFERENCES Positions(PositionId)
);
```

---

## NULL

> `NULL` does not mean zero.
> `NULL` does not mean an empty string.
> `NULL` means the value is **unknown or not provided**.

```sql
-- This employee has no email
INSERT INTO Employees (FullName, Salary, DepartmentId)
VALUES ('Nora', 3000, 1);
```

To check for NULL, use `IS NULL` — not `=`:

```sql
-- WRONG
SELECT * FROM Employees WHERE Email = NULL;

-- CORRECT
SELECT * FROM Employees WHERE Email IS NULL;
```

> ⚠️ This is one of the most common SQL mistakes. `NULL = NULL` does not return `true`. Always use `IS NULL` or `IS NOT NULL`.

---

## Inserting Data

```sql
INSERT INTO Departments (DepartmentName, Location)
VALUES ('IT', 'Building A');

INSERT INTO Departments (DepartmentName, Location)
VALUES ('HR', 'Building B');

INSERT INTO Positions (Title, Level)
VALUES ('Developer', 'Mid');

INSERT INTO Positions (Title, Level)
VALUES ('Manager', 'Senior');

INSERT INTO Employees (FullName, Email, Salary, DepartmentId, PositionId)
VALUES ('Ali', 'ali@company.com', 1800, 1, 1);

INSERT INTO Employees (FullName, Email, Salary, DepartmentId, PositionId)
VALUES ('Sara', 'sara@company.com', 2200, 2, 2);
```

---

## Updating Data

```sql
UPDATE Employees
SET Salary = 2000
WHERE FullName = 'Ali';
```

> ⚠️ Always use a `WHERE` clause when updating. Without it, **all rows** will be updated.

---

## Deleting Data

```sql
DELETE FROM Employees
WHERE EmployeeId = 3;
```

> ⚠️ Always use a `WHERE` clause when deleting. Without it, **all rows** will be deleted.

---

## 🤔 Think

> What happens if you try to insert an employee with `DepartmentId = 99`, but department 99 does not exist?

**Answer:** SQL Server will throw an error because of the foreign key constraint. The database protects data integrity.

---

## 🧪 Mini Exercise

Write the SQL to:

1. Create a `Departments` table with: DepartmentId (PK, identity), DepartmentName (not null, unique)
2. Create an `Employees` table with: EmployeeId (PK, identity), FullName (not null), Salary (must be > 0), DepartmentId (FK)

```sql
-- Your SQL here
```

---

## Summary

| Concept | Key Point |
|---------|-----------|
| Primary Key | Uniquely identifies each row |
| Foreign Key | Connects tables, enforces relationships |
| NOT NULL | Value is required |
| UNIQUE | No duplicates allowed |
| CHECK | Restricts allowed values |
| DEFAULT | Provides automatic default |
| NULL | Value is unknown, not zero |

> 💡 **Senior Developer Note:** Constraints are not optional. They protect your data. Always define them. application code can have bugs — constraints catch problems at the database level.

---

**Next: [03 — Relationships](03-Relationships.md)**
