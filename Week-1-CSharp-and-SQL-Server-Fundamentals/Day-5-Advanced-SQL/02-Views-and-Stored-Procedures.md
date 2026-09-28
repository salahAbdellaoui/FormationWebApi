# 02 — Views and Stored Procedures

---

## The Problem First

> **We keep writing the same complex query for an employee report. Can we save the query as a reusable database object?**

The answer has two directions:

- a **View** — a saved query you can select from like a table
- a **Stored Procedure** — a saved operation you execute

They solve related but different problems.

---

## 📐 Part 1 — Views

### What is a view?

> **A view is a database object that represents a query.**

When you query a view, SQL Server runs the underlying query and gives you the result.

> ⚠️ A normal view does **not** store a separate copy of the data. It is a saved query definition (unless a special type of index is later added to it — an advanced topic we do not cover today).

### Creating a view

```sql
CREATE VIEW vw_EmployeeDepartment
AS
SELECT
    e.EmployeeId,
    e.FullName,
    e.Salary,
    d.DepartmentName
FROM Employees e
INNER JOIN Departments d
    ON e.DepartmentId = d.DepartmentId;
```

### Using a view — like a table

```sql
SELECT *
FROM vw_EmployeeDepartment;
```

```sql
SELECT DepartmentName, AVG(Salary) AS AverageSalary
FROM vw_EmployeeDepartment
GROUP BY DepartmentName;
```

> ⚠️ In SSMS, `CREATE VIEW` must be the **only statement in its batch**. If you get a strange syntax error near `CREATE`, put `GO` before and after the statement.

### Why views are useful

| Benefit | Meaning |
|---------|---------|
| Readability | A report becomes `FROM vw_...` instead of 30 lines of joins |
| Reuse | The same join logic written once |
| Consistency | Everyone reads "employee + department" the same way |
| Simpler access | You can expose only the columns a report needs |

### Views — common mistakes

| Mistake | Why it hurts |
|---------|--------------|
| Creating a view for everything | Objects multiply; nobody knows which to use |
| Hiding very complicated logic inside views | Debugging becomes detective work |
| Assuming views improve performance by themselves | A normal view runs the query each time |
| Using `SELECT *` in long-lived views | The underlying table changes can break reports later |
| Changing base tables without checking dependencies | Views may break or return wrong columns |

> 🤔 **Question:** Should this logic be a view or a stored procedure? Ask: *is it a reusable **read**, or an **operation** with parameters and steps?*

---

## ⚙️ Part 2 — Stored Procedures

### The problem

> **The application repeatedly needs to execute a specific database operation. How can we package the database logic?**

> **A stored procedure is a saved, executable database operation**, usually with parameters.

### Creating one

```sql
CREATE PROCEDURE GetEmployeesByDepartment
    @DepartmentId INT
AS
BEGIN
    SELECT
        EmployeeId,
        FullName,
        Salary
    FROM Employees
    WHERE DepartmentId = @DepartmentId;
END;
```

### Executing it

```sql
EXEC GetEmployeesByDepartment
    @DepartmentId = 2;
```

> ⚠️ Like views, `CREATE PROCEDURE` must be the first statement in its batch — use `GO` around it if your tool requires it.

### Parameters — the key advantage

```text
View:              you filter outside the object:   SELECT ... FROM vw_X WHERE ...
Stored procedure:  you pass values into it:         EXEC GetEmployeesByDepartment @DepartmentId = 2
```

A procedure can also contain steps:

```text
multiple statements
conditional logic (IF)
variables
other operations
```

Keep procedures simple in this course. Advanced procedural logic inside the database is a design decision with real trade-offs.

---

## ⚖️ Views vs Stored Procedures

| Feature | View | Stored Procedure |
| ---------------------------- | --------------------------- | --------------------------- |
| Main purpose | Reusable query | Reusable database operation |
| Parameters | Limited / context-dependent | Yes |
| Used with `SELECT` | Yes | Executed with `EXEC` |
| Can contain procedural logic | Limited | Yes |
| Typical use | Reusable read model | Parameterized database operation |

> **The correct choice depends on the application's design and requirements.** Neither is "the professional choice" in every situation.

### A practical rule of thumb

```text
Reusable read, used like a table          → View
Operation with parameters and steps       → Stored Procedure
Business logic that belongs to the app    → Application code (later weeks)
```

> 💡 **Senior Developer Note:** Do not put all business logic inside database objects. The more logic hides in the database, the harder the system is to version, test, and change.

---

## 🧪 Exercises 4–5

### Exercise 4 — View

Create a reusable report view: **Employee + Department + Position**.

Show: employee name, department name, position title, salary.

```sql
-- Your view here
```

Then query it:

```sql
-- Your query here
```

---

### Exercise 5 — Stored Procedure

Create a parameterized procedure that returns employees for one department, **ordered by salary descending**.

```sql
-- Your procedure here
```

Run it for Department 1 and Department 3. Predict the results first.

---

### Bonus thinking question

> 🔎 You built the view in Exercise 4. Someone now asks for the same data **for one department only**. Do you create a new view? (Usually no — query the existing view with a `WHERE` clause.)

---

## 🚫 Common Mistakes (Part 2)

| Mistake | Why it hurts | Better |
|---------|--------------|--------|
| Views for everything | Object clutter | Views for genuinely reused queries |
| Assuming views are faster | Normal views are saved queries, not caches | Measure if performance matters (File 05) |
| Procedures without a clear reason | Extra objects with no benefit | Ask: what does packaging this buy us? |
| Too much logic in DB objects | Hard to version and test with the app | Keep logic where the team agrees it belongs |
| Forgetting `GO` around `CREATE VIEW/PROC` | Confusing syntax errors | One statement per batch |
| Changing tables without checking dependent views | Reports silently break or fail | Know your dependencies |

---

## ✅ Check Yourself

- [ ] I can create and query a view
- [ ] I know a normal view does not store a data copy
- [ ] I can create and execute a parameterized stored procedure
- [ ] I can compare views and procedures with a real example
- [ ] I completed Exercises 4–5

**Next: [03 — Transactions](03-Transactions.md)**
