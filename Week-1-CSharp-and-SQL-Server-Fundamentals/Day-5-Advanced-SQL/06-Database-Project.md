# 06 — Database Project

---

# 🏗️ Employee Management Database — Production-Ready Database Foundation

**Goal:** build a small but realistic database that combines Day 3 and Day 5 — not isolated exercises, but a system you can explain.

**Team size:** individual or pairs.
**Time:** about 40 minutes, plus presentation.

---

## 📋 Requirements

### Tables and Design

- Departments, Employees, Positions, Projects, EmployeeProjects
- primary keys, foreign keys
- appropriate data types
- constraints
- meaningful names
- one-to-many and many-to-many relationships

> Reuse the setup script from the lesson hub — then extend your design with what you learned.

### Queries — Practical Reports

```text
[ ] Employees by department
[ ] Employees above average salary
[ ] Department statistics (count, average, highest)
[ ] Highest-paid employees
[ ] Project participation (employee + projects)
[ ] Employees without project assignments
[ ] Salary statistics per department
```

### Database Objects

| Object | Minimum requirement |
|--------|---------------------|
| **View** | At least one useful view (e.g. Employee + Department + Position) |
| **Stored procedure** | At least one parameterized procedure (e.g. employees by department) |
| **Transaction** | One realistic multi-step operation with commit/rollback safety |
| **Indexes** | A small number of **justified** indexes |

> ⚠️ For every index, write one sentence: **"This index exists because …"**
> Do not create objects simply to fill a checklist.

### Optimization Task

Choose one report query and investigate it:

1. Examine the query
2. View the execution plan
3. Identify a possible performance issue
4. Propose a change
5. Test the change
6. Explain the reasoning

> 🎯 The objective is **not** an invented performance number. The objective is a scientific investigation.

---

## 🎮 Presentation — "Why did you design it this way?"

Each trainee/team presents:

```text
1. Database design
2. Relationships
3. Important queries
4. View
5. Stored procedure
6. Transaction
7. Index choices
8. Optimization investigation
```

The class asks **"why?"** after each part.

> **Why did you design it this way?** is more important than showing working SQL.

---

## 📝 Classroom Questions for Presentations

- 🔎 Why is this a view and not a procedure?
- ⚠️ What happens if this transaction's second step fails?
- 💭 Why does this index exist? What does it cost?
- 🐢 How did you know this query was slow?
- 📊 What evidence supports your optimization?

---

## 🧠 Knowledge Check (15 Questions)

### Question 1

What is the purpose of a CTE?

- A) To store a permanent table
- B) To give a named temporary result inside a query, improving readability
- C) To create an index
- D) To replace primary keys

**Answer:** B) A named temporary result inside a query — not a permanent object.

---

### Question 2

What is the difference between a View and a Stored Procedure?

- A) They are the same
- B) A view is a reusable query; a stored procedure is an executable operation, usually with parameters
- C) A view runs faster in all cases
- D) A stored procedure can only be used with `SELECT`

**Answer:** B) Reusable query vs executable operation with parameters.

---

### Question 3

Why do we use transactions?

- A) To make every query faster
- B) To make multiple operations behave as one unit — all succeed or all are undone
- C) To create indexes automatically
- D) To avoid writing SQL

**Answer:** B) All or nothing — atomicity.

---

### Question 4

What happens when a transaction is rolled back?

- A) Only the last statement is undone
- B) All changes made inside the transaction are undone
- C) The database is deleted
- D) The transaction is committed anyway

**Answer:** B) All changes inside the transaction are undone.

---

### Question 5

Which command keeps the changes of a transaction?

- A) `ROLLBACK`
- B) `COMMIT`
- C) `THROW`
- D) `DROP`

**Answer:** B) `COMMIT`.

---

### Question 6

Why can indexes improve `SELECT` performance?

- A) Indexes store the whole table twice
- B) They give SQL Server a structure to find rows without reading everything
- C) They remove the need for WHERE clauses
- D) They always work on every column

**Answer:** B) A structure for finding rows faster.

---

### Question 7

What is the cost of an index?

- A) None
- B) Storage, plus extra work on INSERT/UPDATE/DELETE
- C) Only slower `SELECT`
- D) It disables constraints

**Answer:** B) Storage + write maintenance.

---

### Question 8

What is the practical difference between an index seek and a table scan?

- A) They are identical
- B) A seek jumps to matching entries; a scan reads through rows to find them
- C) A scan is always wrong
- D) A seek only works on primary keys

**Answer:** B) Jump to matching entries vs read through rows.

---

### Question 9

Why should we inspect an execution plan?

- A) To make the query shorter
- B) To see how SQL Server executes the query and where the cost is
- C) To create new tables
- D) To replace measurement

**Answer:** B) To see how it runs and where the cost is.

---

### Question 10

Why measure before and after an optimization?

- A) It is a formality
- B) Without measurement you have no evidence that anything improved
- C) To compare different datasets
- D) To avoid reading the plan

**Answer:** B) No measurement, no evidence.

---

### Question 11

A normal view…

- A) Stores a full copy of the data
- B) Stores a query definition and runs the underlying query when used
- C) Is always faster than a JOIN
- D) Replaces a stored procedure

**Answer:** B) A saved query definition, not a data copy.

---

### Question 12

Your transaction's second UPDATE fails. What should happen?

- A) Keep the first update anyway
- B) Roll back so the database stays in its previous state
- C) Run the update again five times
- D) Delete the table

**Answer:** B) Roll back — the business operation did not complete.

---

### Question 13

Which is the best reason to add an index?

- A) Every column in a WHERE clause should have one
- B) A specific, common query needs it, and its costs are acceptable
- C) Indexes are required for all tables
- D) It makes all queries faster

**Answer:** B) Justified by a real query and acceptable cost.

---

### Question 14

What is wrong with this reasoning: *"The query is slow, so I added three indexes and changed the JOIN"*?

- A) Nothing
- B) Multiple changes at once — you cannot tell which change helped, if any
- C) Indexes are forbidden
- D) JOINs should never be changed

**Answer:** B) Change one thing at a time.

---

### Question 15

Where should business logic live — application or database?

- A) Always the database
- B) Always the application
- C) It is a design decision for the team, based on requirements — not a universal rule
- D) Business logic is not allowed anywhere

**Answer:** C) A team design decision based on requirements.

---

## ✅ Final Database Project Checklist

```text
[ ] Relational database design
[ ] Tables
[ ] Primary keys
[ ] Foreign keys
[ ] Constraints
[ ] Relationships
[ ] Complex queries
[ ] Subquery
[ ] CTE
[ ] CASE expression
[ ] View
[ ] Stored Procedure
[ ] Transaction
[ ] Appropriate indexes (each justified)
[ ] Execution plan investigation
[ ] Query optimization investigation
```

> Every object should have a reason.

---

## 🧭 Final Summary

```text
Today I learned:

✓ How to write complex SQL queries
✓ How to use CTEs and subqueries
✓ How to create Views
✓ How to create Stored Procedures
✓ How transactions protect multi-step operations
✓ Why indexes exist
✓ How indexes have costs
✓ How to inspect query performance
✓ How to think about optimization
✓ How to build a practical SQL Server database
```

> **Do not optimize what you have not measured. Understand the problem first.**

---

## 🔜 Next: Week 2 — ASP.NET Core Web API & Entity Framework Core

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

Next week your C# talks to this database. The SQL you learned this week is what helps you understand what Entity Framework Core does **underneath** — every query it generates comes back to the concepts from Days 3 and 5.

**End of Day 5 — end of Week 1.**
