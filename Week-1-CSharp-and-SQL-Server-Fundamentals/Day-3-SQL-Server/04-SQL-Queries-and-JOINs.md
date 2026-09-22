# 04 — SQL Queries and JOINs

---

## SELECT — Asking Questions

We have data. Now we need to ask questions.

```sql
-- Get everything
SELECT * FROM Employees;

-- Get specific columns
SELECT FullName, Salary FROM Employees;
```

> 💡 **Senior Developer Note:** Avoid `SELECT *` in applications. Select only the columns you need. It is clearer and more efficient.

---

## WHERE — Filtering

> "Find employees with salary greater than 2000."

```sql
SELECT FullName, Salary
FROM Employees
WHERE Salary > 2000;
```

### Operators

| Operator | Meaning |
|----------|---------|
| `=` | Equal to |
| `<>` or `!=` | Not equal to |
| `>` | Greater than |
| `<` | Less than |
| `>=` | Greater than or equal |
| `<=` | Less than or equal |

### Combining Conditions

```sql
-- Employees in IT with salary above 1800
SELECT FullName, Salary
FROM Employees
WHERE DepartmentId = 1 AND Salary > 1800;

-- Employees in IT or HR
SELECT FullName, DepartmentId
FROM Employees
WHERE DepartmentId = 1 OR DepartmentId = 2;
```

### IN — Multiple Values

```sql
SELECT FullName
FROM Employees
WHERE DepartmentId IN (1, 2);
```

Same as writing `DepartmentId = 1 OR DepartmentId = 2`.

### BETWEEN — Range

```sql
SELECT FullName, Salary
FROM Employees
WHERE Salary BETWEEN 1800 AND 2500;
```

### LIKE — Pattern Matching

```sql
-- Names starting with 'A'
SELECT FullName FROM Employees
WHERE FullName LIKE 'A%';

-- Names containing 'a'
SELECT FullName FROM Employees
WHERE FullName LIKE '%a%';
```

| Pattern | Meaning |
|---------|---------|
| `%` | Any sequence of characters |
| `_` | Any single character |

### IS NULL — Checking for NULL

```sql
SELECT FullName
FROM Employees
WHERE Email IS NULL;
```

---

## ORDER BY — Sorting

```sql
-- Salary ascending (lowest first)
SELECT FullName, Salary
FROM Employees
ORDER BY Salary ASC;

-- Salary descending (highest first)
SELECT FullName, Salary
FROM Employees
ORDER BY Salary DESC;

-- Sort by department, then by salary
SELECT FullName, DepartmentId, Salary
FROM Employees
ORDER BY DepartmentId ASC, Salary DESC;
```

---

## TOP — Limiting Results

> "Who are the three highest-paid employees?"

```sql
SELECT TOP 3 FullName, Salary
FROM Employees
ORDER BY Salary DESC;
```

---

## Aggregate Functions

> "How many employees do we have? What is the average salary?"

```sql
SELECT COUNT(*) AS TotalEmployees FROM Employees;

SELECT AVG(Salary) AS AverageSalary FROM Employees;

SELECT MAX(Salary) AS HighestSalary FROM Employees;

SELECT MIN(Salary) AS LowestSalary FROM Employees;

SELECT SUM(Salary) AS TotalSalaryCost FROM Employees;
```

| Function | What It Returns |
|----------|----------------|
| `COUNT(*)` | Number of rows |
| `AVG(column)` | Average value |
| `MAX(column)` | Highest value |
| `MIN(column)` | Lowest value |
| `SUM(column)` | Total sum |

---

## GROUP BY — Grouping Rows

> "How many employees are in each department?"

```sql
SELECT DepartmentId, COUNT(*) AS EmployeeCount
FROM Employees
GROUP BY DepartmentId;
```

**Result:**

```text
DepartmentId | EmployeeCount
1            | 3
2            | 2
3            | 2
```

### Multiple Columns

> "How many employees are in each department, grouped by position?"

```sql
SELECT DepartmentId, PositionId, COUNT(*) AS Count
FROM Employees
GROUP BY DepartmentId, PositionId;
```

---

## HAVING — Filtering Groups

> "Show only departments with more than 2 employees."

```sql
SELECT DepartmentId, COUNT(*) AS EmployeeCount
FROM Employees
GROUP BY DepartmentId
HAVING COUNT(*) > 2;
```

### WHERE vs HAVING

| | `WHERE` | `HAVING` |
|---|---------|----------|
| Filters | Individual rows | Groups |
| Used with | Any query | `GROUP BY` |
| Timing | Before grouping | After grouping |

```sql
-- WHERE: filter rows first, then group
SELECT DepartmentId, COUNT(*)
FROM Employees
WHERE Salary > 1800
GROUP BY DepartmentId;

-- HAVING: group first, then filter groups
SELECT DepartmentId, COUNT(*)
FROM Employees
GROUP BY DepartmentId
HAVING COUNT(*) > 2;
```

---

## 🤔 Think

> What is the difference between `WHERE Salary > 2000` and `HAVING AVG(Salary) > 2000`?

**Answer:** `WHERE` filters individual rows. `HAVING` filters groups after aggregation. You cannot use `WHERE` with aggregate functions like `AVG()`.

---

## JOINs — Connecting Tables

We have Employees and Departments. The employee table only has `DepartmentId`.

> "How can we display the department name?"

This is what JOINs are for.

---

## INNER JOIN

Returns only rows that have a match in both tables.

```sql
SELECT
    e.FullName,
    d.DepartmentName
FROM Employees e
INNER JOIN Departments d
    ON e.DepartmentId = d.DepartmentId;
```

**Result:**

```text
FullName | DepartmentName
Ali      | IT
Sara     | HR
Omar     | IT
Ahmed    | Finance
Fatima   | HR
Khalid   | IT
Nora     | Finance
```

Every employee has a matching department, so all rows appear.

---

## LEFT JOIN

Returns all rows from the left table, and matching rows from the right table.

```sql
SELECT
    e.FullName,
    d.DepartmentName
FROM Employees e
LEFT JOIN Departments d
    ON e.DepartmentId = d.DepartmentId;
```

If an employee has no matching department, the department name will be NULL.

> Use `LEFT JOIN` when you want **all** records from the first table, even if they have no match.

---

## The Difference

```text
INNER JOIN: Only matching rows from both tables
LEFT JOIN:  All rows from left table + matching from right
```

> 🔮 **SQL Detective:** If you use `INNER JOIN` and some employees disappear from the results, what does that mean?

**Answer:** Those employees have a `DepartmentId` that does not exist in the `Departments` table (or their `DepartmentId` is NULL).

---

## Multiple JOINs

> "Show employee name, department name, and position title."

```sql
SELECT
    e.FullName,
    d.DepartmentName,
    p.Title AS PositionTitle
FROM Employees e
INNER JOIN Departments d
    ON e.DepartmentId = d.DepartmentId
INNER JOIN Positions p
    ON e.PositionId = p.PositionId;
```

**Result:**

```text
FullName | DepartmentName | PositionTitle
Ali      | IT             | Developer
Sara     | HR             | Manager
Omar     | IT             | Developer
Ahmed    | Finance        | Analyst
Fatima   | HR             | Manager
Khalid   | IT             | Developer
Nora     | Finance        | Analyst
```

---

## Many-to-Many JOIN

> "Show every employee and the projects they work on."

```sql
SELECT
    e.FullName,
    p.ProjectName
FROM Employees e
INNER JOIN EmployeeProjects ep
    ON e.EmployeeId = ep.EmployeeId
INNER JOIN Projects p
    ON ep.ProjectId = p.ProjectId;
```

**Result:**

```text
FullName | ProjectName
Ali      | Website
Ali      | Mobile App
Sara     | Website
Omar     | API
Ahmed    | Mobile App
Fatima   | Website
```

---

## JOIN Types Summary

| JOIN Type | Returns |
|-----------|---------|
| `INNER JOIN` | Only matching rows from both tables |
| `LEFT JOIN` | All rows from left table + matching from right |
| `RIGHT JOIN` | All rows from right table + matching from left |
| `FULL JOIN` | All rows from both tables |

> 💡 **Senior Developer Note:** Most real-world queries use `INNER JOIN` and `LEFT JOIN`. Know these two well before worrying about the others.

---

## SQL Thinking Process

Before writing a query, think:

```text
What information do I need?
        ↓
Which table contains it?
        ↓
Do I need another table?
        ↓
How are the tables related?
        ↓
Which JOIN do I need?
        ↓
Do I need filtering?
        ↓
Do I need grouping?
        ↓
Do I need sorting?
```

---

## 🧪 Practice Queries

Try to write these yourself before looking at the answer.

### Q1: Find all employees with salary above 2000

```sql
-- Your query
```

### Q2: Show employee names and their department names

```sql
-- Your query
```

### Q3: Count employees per department

```sql
-- Your query
```

### Q4: Find the department with the most employees

```sql
-- Your query
```

---

## Summary

| Concept | SQL |
|---------|-----|
| Select columns | `SELECT col1, col2 FROM table` |
| Filter rows | `WHERE condition` |
| Sort | `ORDER BY column ASC/DESC` |
| Limit | `TOP n` |
| Count | `COUNT(*)` |
| Average | `AVG(column)` |
| Group | `GROUP BY column` |
| Filter groups | `HAVING condition` |
| Connect tables | `INNER JOIN ... ON condition` |
| All from left | `LEFT JOIN ... ON condition` |

> 💡 **Senior Developer Note:** Think about what the question is asking before writing SQL. The question tells you which tables, which columns, and which joins you need.

---

**Next: [05 — Practical Exercises](05-Practical-Exercises.md)**
