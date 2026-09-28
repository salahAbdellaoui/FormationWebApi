# 01 — Complex Queries

---

## Start With a Business Question

> **The manager wants a report showing each department, the number of employees, the average salary, the highest salary, and the name of the highest-paid employee.**

> 🤔 **Can one simple `SELECT` solve this?**

Part of it can. Let us build it step by step.

### Step 1 — What we can aggregate directly

```sql
SELECT
    d.DepartmentName,
    COUNT(*)        AS EmployeeCount,
    AVG(e.Salary)   AS AverageSalary,
    MAX(e.Salary)   AS HighestSalary
FROM Employees e
INNER JOIN Departments d
    ON e.DepartmentId = d.DepartmentId
GROUP BY d.DepartmentName;
```

### Step 2 — The hard part: the *name* of the highest-paid employee

Aggregation gives us the **value** (3000), not the **name**. For the name we need a second step: find who has that salary **inside that department**.

This is where advanced techniques enter:

| Technique | What it gives you |
|-----------|-------------------|
| Subquery | A query inside another query |
| CTE | A named temporary result you can read like a step |
| `CASE` | A calculated value based on conditions |

> 💡 **Senior Developer Note:** Readability matters in SQL because someone else may maintain your query later.

---

## 🧩 Subqueries

### The problem

> "Find employees whose salary is above the company average."

One `WHERE` clause cannot compare a column with an aggregate of the same column directly. We need the average **as a value first**.

```sql
SELECT
    EmployeeId,
    FullName,
    Salary
FROM Employees
WHERE Salary > (
    SELECT AVG(Salary)
    FROM Employees
);
```

### Reading it step by step

```text
1. Inner query runs first:  SELECT AVG(Salary) FROM Employees
   → produces one value (the company average)

2. Outer query uses that value:
   → employees whose Salary is greater than that value
```

### Try this — predict first

> 🤔 What happens if the inner query returned **more than one value**?

> Answer: an `=` comparison would fail with an error ("subquery returned more than 1 row"). Use `IN` or `EXISTS` when you expect multiple rows.

```sql
-- Employees in departments located in Building A
SELECT FullName
FROM Employees
WHERE DepartmentId IN (
    SELECT DepartmentId
    FROM Departments
    WHERE Location = 'Building A'
);
```

---

## 🧩 Common Table Expressions (CTE)

A **CTE** is a named temporary result defined at the start of a query. Think of it as giving a complex step a **name**.

```sql
WITH DepartmentStatistics AS
(
    SELECT
        DepartmentId,
        COUNT(*)      AS EmployeeCount,
        AVG(Salary)   AS AverageSalary
    FROM Employees
    GROUP BY DepartmentId
)
SELECT *
FROM DepartmentStatistics;
```

### What to understand

- The CTE is **part of the query** — it is not saved in the database
- It is **not** a stored table; run the query again and it is recomputed
- It makes the query read like steps: *"first calculate statistics, then use them"*

```text
WITH <name> AS ( step 1 )
SELECT ... FROM <name>   ← use the result
```

> 🧠 **Question:** Would a CTE make this easier to understand than a nested subquery? Usually yes, when steps grow beyond one level.

### CTE + the manager's report

```sql
WITH DepartmentStatistics AS
(
    SELECT
        d.DepartmentName,
        COUNT(*)      AS EmployeeCount,
        AVG(e.Salary) AS AverageSalary,
        MAX(e.Salary) AS HighestSalary
    FROM Employees e
    INNER JOIN Departments d
        ON e.DepartmentId = d.DepartmentId
    GROUP BY d.DepartmentName
),
HighestPaidPerDepartment AS
(
    SELECT
        d.DepartmentName,
        e.FullName,
        e.Salary,
        ROW_NUMBER() OVER (
            PARTITION BY d.DepartmentId
            ORDER BY e.Salary DESC
        ) AS RowNum
    FROM Employees e
    INNER JOIN Departments d
        ON e.DepartmentId = d.DepartmentId
)
SELECT
    s.DepartmentName,
    s.EmployeeCount,
    s.AverageSalary,
    s.HighestSalary,
    h.FullName AS HighestPaidEmployee
FROM DepartmentStatistics s
INNER JOIN HighestPaidPerDepartment h
    ON h.DepartmentName = s.DepartmentName
   AND h.RowNum = 1;
```

> ⚠️ `ROW_NUMBER()` is a window function — a Day 5 bonus, not a Day 3 requirement. Note how the CTEs keep each step readable.

---

## 🧩 CASE Expressions

`CASE` produces a **calculated value** from conditions — like an `if/else` inside the query.

```sql
SELECT
    FullName,
    Salary,
    CASE
        WHEN Salary >= 5000 THEN 'High'
        WHEN Salary >= 3000 THEN 'Medium'
        ELSE 'Low'
    END AS SalaryLevel
FROM Employees;
```

### CASE in aggregation — counting by condition

```sql
SELECT
    d.DepartmentName,
    SUM(CASE WHEN e.Salary >= 2200 THEN 1 ELSE 0 END) AS HighEarners,
    SUM(CASE WHEN e.Salary <  2200 THEN 1 ELSE 0 END) AS LowEarners
FROM Employees e
INNER JOIN Departments d
    ON e.DepartmentId = d.DepartmentId
GROUP BY d.DepartmentName;
```

> 🤔 **Predict the result:** how many `HighEarners` does HR have? (Sara 2200, Fatima 3000 → both ≥ 2200 → 2.)

---

## 🧪 Exercises 1–3

### Exercise 1 — Subquery

Find all employees whose salary is above the **company average**.

```sql
-- Your query here
```

---

### Exercise 2 — CTE

Generate department statistics: department name, employee count, average salary. Use a CTE.

```sql
-- Your query here
```

---

### Exercise 3 — CASE

Classify employees by salary range:

```text
Salary >= 2000  → 'A'
Salary >= 1700  → 'B'
else            → 'C'
```

Show `FullName`, `Salary`, and the classification.

```sql
-- Your query here
```

---

## 📖 Query Readability

### Hard to maintain

```sql
SELECT d.DepartmentName, COUNT(*) c, AVG(e.Salary) a FROM Employees e INNER JOIN Departments d ON e.DepartmentId = d.DepartmentId WHERE e.Salary > (SELECT AVG(Salary) FROM Employees) GROUP BY d.DepartmentName HAVING COUNT(*) > 1 ORDER BY a DESC;
```

### Readable

```sql
SELECT
    d.DepartmentName,
    COUNT(*)        AS EmployeeCount,
    AVG(e.Salary)   AS AverageSalary
FROM Employees e
INNER JOIN Departments d
    ON e.DepartmentId = d.DepartmentId
WHERE e.Salary > (
    SELECT AVG(Salary)
    FROM Employees
)
GROUP BY d.DepartmentName
HAVING COUNT(*) > 1
ORDER BY AverageSalary DESC;
```

Both return the same result. The second one can be reviewed, debugged, and modified by another developer.

**Habits that keep SQL readable:**

- meaningful aliases (`EmployeeCount`, not `c`)
- one clause per line
- indentation that shows the query's structure
- a blank line between logical steps

---

## 🧪 Complex Query Challenge — HR Report

> **The HR manager wants a report containing:**
>
> - department name
> - employee count
> - average salary
> - highest salary
> - employees earning above their department average

**Design the query yourself first.** Discuss with your neighbour:

1. What are the steps?
2. Which parts need grouping?
3. Which part needs a subquery or CTE?
4. How will you show "employees above the department average" — in the same row, or as separate rows?

> 🔎 Reveal the solution only after trainees have attempted a design.

<details>
<summary>One possible solution</summary>

```sql
WITH DepartmentStats AS
(
    SELECT
        d.DepartmentName,
        COUNT(*)      AS EmployeeCount,
        AVG(e.Salary) AS AverageSalary,
        MAX(e.Salary) AS HighestSalary
    FROM Employees e
    INNER JOIN Departments d
        ON e.DepartmentId = d.DepartmentId
    GROUP BY d.DepartmentName
),
AboveAverage AS
(
    SELECT
        d.DepartmentName,
        e.FullName,
        e.Salary
    FROM Employees e
    INNER JOIN Departments d
        ON e.DepartmentId = d.DepartmentId
    WHERE e.Salary > (
        SELECT AVG(Salary)
        FROM Employees e2
        WHERE e2.DepartmentId = e.DepartmentId
    )
)
SELECT
    s.DepartmentName,
    s.EmployeeCount,
    s.AverageSalary,
    s.HighestSalary,
    a.FullName AS AboveDepartmentAverage
FROM DepartmentStats s
LEFT JOIN AboveAverage a
    ON a.DepartmentName = s.DepartmentName
ORDER BY s.DepartmentName, a.Salary DESC;
```

</details>

---

## 🚫 Common Mistakes (Part 1)

| Mistake | Why it hurts | Better |
|---------|--------------|--------|
| Unreadable one-line queries | Nobody can maintain them | Format, alias, structure |
| Unnecessary nested subqueries | Hard to follow | Use a CTE to name each step |
| Subquery returning many rows with `=` | Runtime error | Use `IN` or `EXISTS` |
| Mixing grouping levels in one query | Confusing or wrong results | One level of aggregation per step |
| Aliasing columns as `a`, `b`, `c` | The report means nothing later | Name columns after their meaning |

---

## ✅ Check Yourself

- [ ] I can read a query with a subquery step by step
- [ ] I know what a CTE is — and what it is **not** (not a stored table)
- [ ] I can write `CASE` expressions and conditional aggregates
- [ ] I completed Exercises 1–3
- [ ] I attempted the HR report challenge before looking at the solution

**Next: [02 — Views and Stored Procedures](02-Views-and-Stored-Procedures.md)**
