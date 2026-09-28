# 04 — Indexes

---

## The Problem

> 🤔 **What happens when a table contains millions of rows and SQL Server must find one employee?**

Without help, SQL Server may have to **look through a large amount of data** to answer the question. Reading a lot of data costs time and resources.

**What do professional teams do?** They give SQL Server a way to find rows **without reading everything**.

---

## 📚 The Analogy

> **An index is like the index of a book. You find the topic without reading every page.**

```text
Book without index:   read page by page until you find the topic
Book with index:      look up the topic → go straight to the page
```

> ⚠️ This is an **analogy**, not a literal description of SQL Server's internal storage. It explains the *idea*: a helper structure that speeds up **finding** rows.

---

## 🔎 Creating an Index

```sql
CREATE INDEX IX_Employees_DepartmentId
ON Employees(DepartmentId);
```

| Part | Meaning |
|------|---------|
| `IX_Employees_DepartmentId` | Index name (a team convention: `IX_` + table + column) |
| `Employees` | The table |
| `DepartmentId` | The indexed column |

### Why might this help?

```sql
SELECT EmployeeId, FullName
FROM Employees
WHERE DepartmentId = 3;
```

With a useful index on `DepartmentId`, SQL Server can **look up** department 3 instead of checking every employee row.

> ⚠️ An index **may** help this query **depending on the data and workload**. An index does not automatically make every query faster — the query optimizer decides whether an index is useful.

---

## ⚖️ Index Trade-offs (Very Important)

```text
More useful indexes
        ↓
Faster access for some queries
        +
Additional storage
        +
Additional work when data changes
```

| Area | Effect of adding an index |
|------|---------------------------|
| `SELECT` (some queries) | May become faster |
| `INSERT` | SQL Server must also maintain the index |
| `UPDATE` on indexed columns | Index maintenance cost |
| `DELETE` | Index maintenance cost |
| Storage | Extra space on disk/in memory |
| Maintenance | More objects to review over time |

> 💡 **Senior Developer Note:** An index is a **design decision**, not a decoration. Ask: *which query does it serve, and what does it cost?*

---

## 🧠 Clustered vs Nonclustered (Basic Level)

| Type | Simple explanation |
|------|--------------------|
| **Clustered index** | Defines the **physical order** of the data rows in the table. There can be **one** per table. |
| **Nonclustered index** | A **separate structure** with the indexed columns plus a way to find the matching rows. A table can have **several**. |

Important detail:

> In SQL Server, if a table has a **primary key** and no clustered index exists yet, the primary key typically becomes the clustered index by default. This is a default behavior, not a universal design law — teams can choose differently.

```text
Clustered index   = how the data itself is ordered
Nonclustered index= a lookup structure pointing to the data
```

We do not go deeper into storage internals today — the goal is to choose indexes with understanding, not to become a DBA.

---

## 🧩 Composite Indexes

An index on **more than one column**:

```sql
CREATE INDEX IX_Employees_Department_Salary
ON Employees(DepartmentId, Salary);
```

### Column order matters

The index is organized by `DepartmentId` **first**, then by `Salary` inside each department.

```text
(DepartmentId, Salary)   → helps queries that filter by DepartmentId
                            (and optionally Salary)

(Salary, DepartmentId)   → organized primarily by Salary
```

> ⚠️ Whether a specific query **uses** a composite index depends on how the query is written, the data, and the optimizer's decisions. **Test with an execution plan** (File 05) — do not assume.

---

## 🧪 Exercise 7 — Choose an Index

Given this query:

```sql
SELECT
    EmployeeId,
    FullName
FROM Employees
WHERE DepartmentId = 3;
```

1. Which column would you consider indexing?
2. Write the `CREATE INDEX` statement.
3. Explain **why** this index exists in one sentence.

```sql
-- Your index here
```

### The follow-up discussion

> **Would you create an index for every column used in a `WHERE` clause?**

Discuss in pairs. Some starting points:

- the same table is written to as well as read
- each index costs storage and maintenance
- the best choice depends on **which queries run often** and **how the data changes**
- small tables may not need extra indexes at all

There is no single correct answer without knowing the workload — and saying *"it depends"* **with reasons** is a professional answer.

---

## 📝 Classroom Questions

- 📚 **Would an index help this query?** (Which query? How selective is the column? What does the plan say?)
- 💭 **What is the cost of adding another index?** (Storage + maintenance on writes.)
- ⚠️ **What could happen if we index everything?** (Slow writes, wasted space, confusing maintenance.)
- 🔎 **How would you prove an index helped?** (Execution plan + measured comparison — File 05.)

---

## 🚫 Common Mistakes (Part 4)

| Mistake | Why it hurts | Better |
|---------|--------------|--------|
| Creating indexes everywhere | Slower writes, wasted space | One index per justified need |
| Assuming an index is always beneficial | Some indexes are never used | Check if the plan actually uses it |
| Ignoring maintenance costs | Slow inserts/updates over time | Review write-heavy tables carefully |
| Duplicate/overlapping indexes | Extra cost, no extra benefit | Check existing indexes first |
| Indexing tiny tables "for speed" | No measurable benefit | Measure first |
| Never reviewing indexes | Old decisions outlive old requirements | Review indexes like any design decision |

---

## ✅ Check Yourself

- [ ] I can explain the book-index analogy — and its limits
- [ ] I can create a single-column and a composite index
- [ ] I know the costs of an index
- [ ] I can explain clustered vs nonclustered at a basic level
- [ ] I know column order matters in a composite index
- [ ] I completed Exercise 7 with a written justification

**Next: [05 — Query Optimization](05-Query-Optimization.md)**
