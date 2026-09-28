# 05 — Debugging and Problem Solving

---

## Start Here

> **"The application is broken. What do you do first?"**

Common (bad) answers:

- ❌ "Open the debugger and start clicking."
- ❌ "Change something and see what happens."
- ❌ "Rewrite the method — it must be wrong."

Professional answer:

```text
Problem
  ↓
Reproduce
  ↓
Collect Evidence
  ↓
Form a Hypothesis
  ↓
Test the Hypothesis
  ↓
Find Root Cause
  ↓
Fix
  ↓
Test Again
  ↓
Prevent Regression
```

> 💡 **Senior Developer Note:** Do not fix a bug you cannot reproduce or understand, unless you have enough evidence to justify the change. A fix you cannot explain is a future bug.

---

## 🔎 Debugging Example — Employee Search

**Report from the team:**

> "The employee search endpoint returns no results for some valid names."

### What you are given

**Sample input**

```text
GET /api/employees/search?name=ahmed
```

**Expected result**

```json
[
  { "id": 4, "fullName": "Ahmed Karim", "department": "Finance" }
]
```

**Actual result**

```json
[]
```

But this works:

```text
GET /api/employees/search?name=Ahmed     → returns the employee
```

**The code**

```csharp
public List<Employee> Search(string name)
{
    return db.Employees
        .Where(e => e.FullName.Contains(name))
        .ToList();
}
```

**The database contains**

```text
FullName
--------
Ahmed Karim
Sara Ali
Omar Hassan
```

> 🤔 **What do you investigate first?**
>
> Do not reveal the answer yet. Let the trainees reason for three minutes.

### The evidence so far

| Fact | Observation |
|------|-------------|
| Input | `ahmed` (lowercase) → no results |
| Input | `Ahmed` (uppercase A) → works |
| Data | `Ahmed Karim` stored with uppercase A |
| Code | `Contains(name)` |

**Form a hypothesis:** the comparison is case-sensitive, so `"Ahmed Karim".Contains("ahmed")` is `false`.

**Test it:** try `name=Ahmed Karim` — works. Try `name=KARIM` — fails. Try `name=ahmed` — fails. The hypothesis is confirmed.

**Root cause:** the search does not normalize case.

**Fix (one possible approach):**

```csharp
public List<Employee> Search(string name)
{
    var term = name?.Trim().ToLowerInvariant() ?? string.Empty;

    return db.Employees
        .Where(e => e.FullName.ToLower().Contains(term))
        .ToList();
}
```

> ⚠️ If this data access runs against a **database**, remember what you learned on Day 3: SQL Server's default collation is usually case-*insensitive*, while C# `string.Contains` is case-*sensitive*. The exact behavior depends on how the query is executed (in memory vs translated to SQL). **Verify, do not assume.**

**Test again:** `ahmed`, `Ahmed`, `AHMED` all return the expected result.

**Prevent regression:** add a test with a lowercase search term.

---

## 📋 The Debugging Checklist

Reusable questions — ask them in order:

```text
Can I reproduce it?
When did it start?
Does it happen for everyone?
What changed recently?
What input causes it?
What does the log say?
What does the database contain?
What do I expect?
What actually happens?
```

### Two columns: expect vs actual

| Expect | Actual |
|--------|--------|
| `ahmed` returns 1 employee | returns 0 employees |

Write this down. Most debugging confusion comes from having the expectation *only in your head*.

---

## 🧠 Debugging Mindset Rules

1. **Reproduce first.** If you cannot see it, you cannot fix it.
2. **Change one thing at a time.** If you change five things, you learn nothing.
3. **Read the error message completely.** The first line often lies; the stack trace tells the truth.
4. **Use `git log` and `git diff`.** "What changed recently?" is a Git question.
5. **Shrink the problem.** Find the smallest input that still fails.
6. **Explain it out loud.** Describing the problem to a colleague fixes many bugs.
7. **Write it down.** Evidence beats memory.

> 🧠 **Is this a Git problem or an application problem?** Ask this before you blame the tool. Git rarely breaks your logic — Git records it.

---

## 🧪 Exercise 8 — Debugging Challenge

### Challenge A — The Silent Filter

**Report:**

> "Filtering employees by department returns employees from the wrong department."

**Code:**

```csharp
public List<Employee> GetByDepartment(int departmentId)
{
    return db.Employees
        .Where(e => e.DepartmentId == departmentId || e.IsActive)
        .ToList();
}
```

**Sample data**

| Employee | DepartmentId | IsActive |
|----------|--------------|----------|
| Ali | 1 | true |
| Sara | 2 | true |
| Omar | 1 | false |

**Request:** `GetByDepartment(2)`

> 🤔 **What do you get? What did the caller expect? Which part of the condition causes it?**

Write your answer before reading on.

<details>
<summary>Answer</summary>

You get **Ali, Sara** — every active employee, plus everyone in department 2.

The caller expects only employees where `DepartmentId == 2` (Sara).

The `|| e.IsActive` condition makes the whole filter true for any active employee. It was probably meant to be:

```csharp
.Where(e => e.DepartmentId == departmentId && e.IsActive)
```

**Lesson:** when a filter returns too much, inspect every part of the condition — especially `OR`.

</details>

---

### Challenge B — The Null Department

**Report:**

> "One employee breaks the department report."

**Code:**

```csharp
foreach (var e in db.Employees)
{
    Console.WriteLine($"{e.FullName}: {e.Department.Name.ToUpper()}");
}
```

**Sample data:** employee `Khalid` has `DepartmentId = NULL` (you created this row on Day 3!).

> 🤔 **What happens? What evidence do you have? What is the fix?**

<details>
<summary>Answer</summary>

`e.Department` is `null` → `NullReferenceException` at runtime.

Evidence: the crash happens only when processing Khalid; the stack trace points at the `Department.Name` line.

Fix options: check for null, use a null-safe access, or exclude employees without a department — **depending on what the report should show**.

**Lesson:** the fix depends on the *business intent*, not on making the exception disappear.

</details>

---

## 🎯 Classroom Questions

- 🐛 What evidence do we have?
- 🔎 What would you check first — and why that and not something else?
- 🤔 Did this work yesterday? What changed since then?
- 🧠 Am I fixing the root cause or a symptom?
- ⚠️ What could break if I "fix" this by returning an empty list?

---

## 🚫 Common Mistakes (Part 5)

| Mistake | Why it hurts | Better |
|---------|--------------|--------|
| Debugging without reproducing | You cannot confirm the fix | Reproduce first, always |
| Changing many things at once | You do not know what fixed it | One change at a time |
| Guessing instead of reading evidence | Wasted hours, wrong fixes | Ask the checklist questions |
| Fixing the symptom only | The bug returns next week | Find the root cause |
| Skipping the re-test after the fix | Regressions slip through | Test the original failing case |
| No test added | The same bug comes back | Add one test that fails before the fix |

---

## ✅ Check Yourself

- [ ] I can write the debugging flow from memory
- [ ] I investigated the employee search example with evidence
- [ ] I solved Challenge A and Challenge B
- [ ] I know why "change one thing at a time" matters
- [ ] I can tell a Git problem from an application problem

**Next: [06 — Real-World Development Practices](06-Real-World-Development-Practices.md)**
