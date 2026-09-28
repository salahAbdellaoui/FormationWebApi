# Day 5 — Advanced SQL

## Professional Training Course (Week 1)

**Duration:** 4 hours
**Level:** Intermediate — Building on Day 3
**Prerequisites:** Day 3 (SQL Server): tables, keys, constraints, relationships, SELECT, WHERE, GROUP BY, HAVING, JOINs
**Database:** Microsoft SQL Server (T-SQL)

---

## 🎯 Day 5 Learning Objectives

By the end of this session, you will be able to:

- Write more complex SQL queries
- Use subqueries appropriately
- Use CTEs for readable queries
- Use `CASE` expressions
- Understand and create Views
- Understand and create Stored Procedures
- Understand transactions
- Use `COMMIT` and `ROLLBACK` safely
- Understand why indexes exist
- Recognize basic index trade-offs
- Understand clustered and nonclustered indexes at a basic level
- Read a basic execution plan
- Investigate slow queries with evidence
- Apply evidence-based optimization
- Build a practical SQL Server database project

---

## Training Schedule

| Part | Topic | Duration |
|------|-------|----------|
| 1 | Complex Queries (subqueries, CTEs, CASE) | ~55 min |
| 2 | Views and Stored Procedures | ~50 min |
| 3 | ☕ Short Break | 10 min |
| 4 | Transactions | ~40 min |
| 5 | Indexes | ~45 min |
| 6 | Query Optimization | ~40 min |
| 7 | Database Project & Knowledge Check | ~40 min |

---

## 🗺️ How Today Connects to Day 3

Day 3 taught the foundation:

```text
Tables → Keys → Constraints → Relationships → Queries → JOINs
```

Day 5 builds on that foundation:

```text
Basic SQL
     ↓
Complex SQL
     ↓
Reusable Queries (Views, Procedures)
     ↓
Transactions
     ↓
Indexes
     ↓
Performance
```

We will **not** repeat basic SQL. If a Day 3 concept appears, we use it as known ground.

---

## 🏢 Main Scenario — Employee Management Database

The same database from Day 3:

```text
Departments
     │
     └── Employees
            │
            ├── Position
            │
            └── EmployeeProjects
                       │
                       └── Projects
```

We use **one database all day** so you can focus on the SQL concept, not on learning a new data model.

### Setup Script

Run this once before the lesson (same schema as Day 3):

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

CREATE TABLE Projects
(
    ProjectId   INT IDENTITY(1,1) PRIMARY KEY,
    ProjectName NVARCHAR(100) NOT NULL
);

CREATE TABLE EmployeeProjects
(
    EmployeeId INT,
    ProjectId  INT,
    PRIMARY KEY (EmployeeId, ProjectId),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId),
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId)
);

INSERT INTO Departments VALUES ('IT', 'Building A');
INSERT INTO Departments VALUES ('HR', 'Building B');
INSERT INTO Departments VALUES ('Finance', 'Building A');

INSERT INTO Positions VALUES ('Developer', 'Mid');
INSERT INTO Positions VALUES ('Manager', 'Senior');
INSERT INTO Positions VALUES ('Analyst', 'Junior');

INSERT INTO Employees (FullName, Email, Salary, DepartmentId, PositionId) VALUES
('Ali',    'ali@company.com',    1800, 1, 1),
('Sara',   'sara@company.com',   2200, 2, 2),
('Omar',   'omar@company.com',   1900, 1, 1),
('Ahmed',  'ahmed@company.com',  2500, 3, 3),
('Fatima', 'fatima@company.com', 3000, 2, 2),
('Khalid', NULL,                 1600, 1, 1),
('Nora',   NULL,                 2100, 3, 3);

INSERT INTO Projects VALUES ('Website');
INSERT INTO Projects VALUES ('Mobile App');
INSERT INTO Projects VALUES ('API');

INSERT INTO EmployeeProjects VALUES (1, 1);
INSERT INTO EmployeeProjects VALUES (1, 2);
INSERT INTO EmployeeProjects VALUES (2, 1);
INSERT INTO EmployeeProjects VALUES (3, 3);
INSERT INTO EmployeeProjects VALUES (4, 2);
INSERT INTO EmployeeProjects VALUES (5, 1);
```

> 💡 **Senior Developer Note:** A database should protect data integrity through constraints, not rely only on application code.

---

## 📚 Lesson Files

| File | Topic |
|------|-------|
| [01 — Complex Queries](01-Complex-Queries.md) | Multiple JOINs, subqueries, CTEs, CASE, readability |
| [02 — Views and Stored Procedures](02-Views-and-Stored-Procedures.md) | Reusable database objects, comparison |
| [03 — Transactions](03-Transactions.md) | COMMIT, ROLLBACK, error handling, ACID basics |
| [04 — Indexes](04-Indexes.md) | Why indexes exist, trade-offs, types, composite |
| [05 — Query Optimization](05-Query-Optimization.md) | Investigation process, execution plans, detective challenge |
| [06 — Database Project](06-Database-Project.md) | Final project, presentation, knowledge check, summary |

Start with File 01.

---

## 🔜 Preview: Week 2 — ASP.NET Core Web API & Entity Framework Core

Next week your SQL meets your C#:

```text
ASP.NET Core Web API
        ↓
Application Logic
        ↓
Entity Framework Core
        ↓
SQL Queries
        ↓
SQL Server
        ↓
Tables + Relationships + Indexes
```

The SQL you learn this week will help you understand what Entity Framework Core does **underneath**. We do not teach EF Core today.

---

**Start with [01 — Complex Queries](01-Complex-Queries.md)**
