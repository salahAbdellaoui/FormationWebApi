# 05 — Practical Exercises

---

## Sample Database

Use this database setup for all exercises:

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

-- Insert data
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

---

## Challenge 1 — Find the Employees

> Find all employees whose salary is greater than 2000.

```sql
-- Your query here
```

---

## Challenge 2 — Who Works Where?

> Display each employee's name and their department name.

```sql
-- Your query here
```

---

## Challenge 3 — Salary Detective

> Find the three highest-paid employees. Show their name, salary, and department.

```sql
-- Your query here
```

---

## Challenge 4 — Department Competition

> Find the department with the largest number of employees.

```sql
-- Your query here
```

---

## Challenge 5 — Missing Department

> Find employees who do not have a matching department (use LEFT JOIN).

```sql
-- Your query here
```

---

## Challenge 6 — Salary Statistics

> Calculate the average salary for each department. Show only departments where the average salary is above 2000.

```sql
-- Your query here
```

---

## Challenge 7 — Project Team

> Display every employee and the projects they are assigned to. Include employees who have no projects.

```sql
-- Your query here
```

---

## 🕵️ SQL Detective Challenge

The manager asked for a query to find departments with more than 2 employees. Here is the query:

```sql
SELECT DepartmentId, COUNT(*) AS EmployeeCount
FROM Employees
WHERE COUNT(*) > 2
GROUP BY DepartmentId;
```

> **What is wrong? How would you fix it?**

**Answer:** The `WHERE` clause cannot be used with aggregate functions. It should use `HAVING`:

```sql
SELECT DepartmentId, COUNT(*) AS EmployeeCount
FROM Employees
GROUP BY DepartmentId
HAVING COUNT(*) > 2;
```

---

## 🏆 Mini Project — Employee Management Database

Build this database step by step.

### Step 1 — Create Tables

Create the following tables:

- `Departments` — DepartmentId (PK), DepartmentName, Location
- `Positions` — PositionId (PK), Title, Level
- `Employees` — EmployeeId (PK), FullName, Salary, DepartmentId (FK), PositionId (FK), HireDate
- `Projects` — ProjectId (PK), ProjectName
- `EmployeeProjects` — EmployeeId + ProjectId (composite PK, FKs)

### Step 2 — Insert Data

Insert at least:

- 3 departments
- 3 positions
- 8 employees (distributed across departments)
- 3 projects
- At least 6 employee-project assignments

### Step 3 — Write Queries

Write SQL queries for:

1. List all employees with their department and position names
2. Count employees per department
3. Find the highest-paid employee in each department
4. List all projects with the number of employees assigned
5. Find employees who are not assigned to any project
6. Calculate the total salary cost per department
7. Find departments where the total salary cost exceeds 5000

### Step 4 — Bonus

Write a query that shows:

```text
Employee Name | Department | Position | Salary | Projects
Ali           | IT         | Developer | 1800  | Website, Mobile App
Sara          | HR         | Manager   | 2200  | Website
...
```

---

## 🧠 Knowledge Check

### Question 1

What is the purpose of a primary key?

- A) To sort data
- B) To uniquely identify each row
- C) To connect tables
- D) To filter data

**Answer:** B) To uniquely identify each row.

---

### Question 2

What does `NULL` mean?

- A) Zero
- B) Empty string
- C) The value is unknown or not provided
- D) The value is false

**Answer:** C) The value is unknown or not provided.

---

### Question 3

Which of these is correct for checking NULL?

- A) `WHERE Email = NULL`
- B) `WHERE Email IS NULL`
- C) `WHERE Email == NULL`
- D) `WHERE Email NULL`

**Answer:** B) `WHERE Email IS NULL`

---

### Question 4

What is the difference between `INNER JOIN` and `LEFT JOIN`?

- A) They are the same
- B) `INNER JOIN` returns all rows from both tables
- C) `LEFT JOIN` returns all rows from the left table plus matching from the right
- D) `INNER JOIN` returns more rows

**Answer:** C) `LEFT JOIN` returns all rows from the left table plus matching from the right.

---

### Question 5

What happens if you use `WHERE COUNT(*) > 2`?

- A) It works correctly
- B) SQL Server throws an error — you cannot use `WHERE` with aggregates
- C) It returns all rows
- D) It returns no rows

**Answer:** B) SQL Server throws an error. Use `HAVING` instead.

---

### Question 6

Which table holds the foreign key in a one-to-many relationship between Departments and Employees?

- A) Departments
- B) Employees
- C) Either table
- D) A separate junction table

**Answer:** B) Employees — the "many" side holds the foreign key.

---

### Question 7

What does this query return?

```sql
SELECT TOP 1 FullName
FROM Employees
ORDER BY Salary DESC;
```

- A) The employee with the lowest salary
- B) The employee with the highest salary
- C) All employees
- D) The first employee alphabetically

**Answer:** B) The employee with the highest salary.

---

### Question 8

What is the difference between `WHERE` and `HAVING`?

- A) They are the same
- B) `WHERE` filters rows before grouping, `HAVING` filters groups after grouping
- C) `WHERE` is used with JOINs, `HAVING` is used with DELETE
- D) `HAVING` is always faster

**Answer:** B) `WHERE` filters rows before grouping, `HAVING` filters groups after grouping.

---

### Question 9

Why do we need a junction table for many-to-many relationships?

- A) To store more data
- B) Because SQL Server cannot directly connect two tables in a many-to-many relationship
- C) To make queries faster
- D) We do not need it

**Answer:** B) SQL Server cannot directly connect two tables in a many-to-many relationship.

---

### Question 10

What does `SELECT *` do?

- A) Selects only the primary key
- B) Selects all columns from the table
- C) Selects the first row
- D) Counts all rows

**Answer:** B) Selects all columns from the table.

---

### Question 11

Which constraint ensures that no two employees can have the same email?

- A) `NOT NULL`
- B) `PRIMARY KEY`
- C) `UNIQUE`
- D) `CHECK`

**Answer:** C) `UNIQUE`

---

### Question 12

What is the output?

```sql
SELECT DepartmentId, AVG(Salary) AS AvgSalary
FROM Employees
GROUP BY DepartmentId
HAVING AVG(Salary) > 2000;
```

- A) All departments with their average salary
- B) Only departments where the average salary is above 2000
- C) The department with the highest average salary
- D) All employees earning above 2000

**Answer:** B) Only departments where the average salary is above 2000.

---

## 💡 Senior Developer Takeaways

1. **Design before coding.** Think about entities, attributes, and relationships first.
2. **Use constraints.** Primary keys, foreign keys, NOT NULL, UNIQUE, CHECK — they protect your data.
3. **Understand NULL.** It is not zero. It is not empty. It is unknown.
4. **Choose the right JOIN.** `INNER JOIN` for matching rows. `LEFT JOIN` when you need all rows from the left table.
5. **Think before writing SQL.** What information do you need? Which tables? How are they related?
6. **Avoid `SELECT *`** in applications. Select only what you need.
7. **Use `GROUP BY` with `HAVING`** — not `WHERE` — to filter aggregated results.

---

**Return to [Day 3 Overview](Day-3-SQL-Server.md)**
