# 03 — LINQ

---

## The Problem

You have 1,000 employees. You need to:

1. Find all active employees from the IT department
2. Sort them by salary (highest first)
3. Get only their names and salaries

**Without LINQ**, you would write:

```csharp
List<Employee> result = new List<Employee>();

foreach (var emp in employees)
{
    if (emp.IsActive && emp.Department == "IT")
    {
        result.Add(emp);
    }
}

result.Sort((a, b) => b.Salary.CompareTo(a.Salary));

foreach (var emp in result)
{
    Console.WriteLine($"{emp.Name} - {emp.Salary}");
}
```

This works. But it is long, repetitive, and hard to read.

> LINQ makes this cleaner.

---

## What Is LINQ?

**LINQ** (Language Integrated Query) lets you query data using a clean, readable syntax directly in C#.

The same example with LINQ:

```csharp
var result = employees
    .Where(e => e.IsActive && e.Department == "IT")
    .OrderByDescending(e => e.Salary)
    .Select(e => new { e.Name, e.Salary });

foreach (var emp in result)
{
    Console.WriteLine($"{emp.Name} - {emp.Salary}");
}
```

Same result. Much cleaner.

---

## The Employee List

We will use this list throughout the lesson:

```csharp
List<Employee> employees = new List<Employee>
{
    new Employee { Id = 1, Name = "Ali",     Salary = 5000,  Department = "IT",       IsActive = true },
    new Employee { Id = 2, Name = "Sara",    Salary = 7000,  Department = "HR",       IsActive = true },
    new Employee { Id = 3, Name = "Omar",    Salary = 3000,  Department = "IT",       IsActive = false },
    new Employee { Id = 4, Name = "Ahmed",   Salary = 6000,  Department = "Finance",  IsActive = true },
    new Employee { Id = 5, Name = "Fatima",  Salary = 8000,  Department = "HR",       IsActive = true },
    new Employee { Id = 6, Name = "Khalid",  Salary = 4500,  Department = "IT",       IsActive = true },
    new Employee { Id = 7, Name = "Nora",    Salary = 5500,  Department = "Finance",  IsActive = false }
};
```

---

## Filtering — Where

Find items that match a condition:

```csharp
var activeEmployees = employees.Where(e => e.IsActive);
var itEmployees = employees.Where(e => e.Department == "IT");
var highSalary = employees.Where(e => e.Salary > 5000);
```

You can combine conditions:

```csharp
var activeIT = employees.Where(e => e.IsActive && e.Department == "IT");
```

---

## Projection — Select

Transform each item into something else:

```csharp
// Get only names
var names = employees.Select(e => e.Name);

// Get name and salary
var summaries = employees.Select(e => new { e.Name, e.Salary });

// Transform to a different shape
var display = employees.Select(e => $"{e.Name} earns {e.Salary}");
```

---

## Sorting — OrderBy / OrderByDescending

```csharp
var bySalary = employees.OrderBy(e => e.Salary);           // Low to high
var bySalaryDesc = employees.OrderByDescending(e => e.Salary);  // High to low
var byName = employees.OrderBy(e => e.Name);               // Alphabetical
```

Multiple levels:

```csharp
var sorted = employees
    .OrderBy(e => e.Department)
    .ThenByDescending(e => e.Salary);
```

---

## Finding One Item — FirstOrDefault / SingleOrDefault

```csharp
// First item that matches, or default (null) if none found
var first = employees.FirstOrDefault(e => e.Department == "IT");

// First item that matches, or throws if none found
var firstExact = employees.First(e => e.Department == "IT");

// The ONLY item that matches, or null
var single = employees.SingleOrDefault(e => e.Id == 1);
```

> ⚠️ What is the difference between `FirstOrDefault` and `SingleOrDefault`?

| Method | Returns | Throws When |
|--------|---------|-------------|
| `FirstOrDefault` | First match or default | N/A |
| `SingleOrDefault` | Single match or default | More than one match exists |

Use `SingleOrDefault` when you expect **exactly one** result.

---

## Checking — Any / All

```csharp
// Are there any active employees?
bool hasActive = employees.Any(e => e.IsActive);  // true

// Are all employees active?
bool allActive = employees.All(e => e.IsActive);  // false

// Are there any IT employees with salary > 5000?
bool hasHighIT = employees.Any(e => e.Department == "IT" && e.Salary > 5000);
```

---

## Counting and Aggregation

```csharp
int total = employees.Count();
int activeCount = employees.Count(e => e.IsActive);
int itCount = employees.Count(e => e.Department == "IT");

decimal totalSalary = employees.Sum(e => e.Salary);
decimal avgSalary = employees.Average(e => e.Salary);
decimal maxSalary = employees.Max(e => e.Salary);
decimal minSalary = employees.Min(e => e.Salary);
```

---

## Grouping — GroupBy

Group employees by department:

```csharp
var grouped = employees.GroupBy(e => e.Department);

foreach (var group in grouped)
{
    Console.WriteLine($"{group.Key}: {group.Count()} employees");

    foreach (var emp in group)
    {
        Console.WriteLine($"  {emp.Name}");
    }
}
```

**Output:**

```text
IT: 3 employees
  Ali
  Omar
  Khalid
HR: 2 employees
  Sara
  Fatima
Finance: 2 employees
  Ahmed
  Nora
```

---

## Chaining Operations

LINQ operations can be chained together:

```csharp
var result = employees
    .Where(e => e.IsActive)                    // Filter
    .OrderByDescending(e => e.Salary)          // Sort
    .ThenBy(e => e.Name)                       // Then sort by name
    .Select(e => new { e.Name, e.Salary })     // Project
    .Take(3);                                  // Take first 3
```

Read it like a sentence: "Filter active employees, sort by salary descending, then by name, select name and salary, take the first 3."

---

## ⚠️ Deferred Execution

This is important to understand.

```csharp
// This does NOT execute immediately
var query = employees.Where(e => e.IsActive);

// This executes when you loop through it
foreach (var emp in query)
{
    Console.WriteLine(emp.Name);
}
```

The query is **defined** when you write it. It **executes** when you iterate over it.

### When does it execute?

```csharp
// Deferred — executes when you loop
var query = employees.Where(e => e.IsActive);

// Immediate — executes right away
var list = employees.Where(e => e.IsActive).ToList();
var count = employees.Count(e => e.IsActive);
var first = employees.FirstOrDefault(e => e.IsActive);
```

### Why does this matter?

```csharp
// This re-queries the source every time you loop
var query = employees.Where(e => e.IsActive);
// Loop 1: filters all employees
// Loop 2: filters all employees again

// This stores the result once
var list = employees.Where(e => e.IsActive).ToList();
// Loop 1: uses cached list
// Loop 2: uses cached list
```

> 💡 **Senior Developer Note:** Use `.ToList()` when you need to iterate multiple times or when the source might change. Do not use it "just in case" — it creates unnecessary memory allocation.

---

## 🤔 Think

> What will this print?

```csharp
var query = employees.Where(e => e.Salary > 5000);
employees.Add(new Employee { Name = "New", Salary = 9000, IsActive = true });

Console.WriteLine(query.Count());
```

**Answer:** It depends. If `employees` is a `List<Employee>`, the query executes at `.Count()`, so it will include the newly added employee. Deferred execution means the query runs fresh each time.

---

## LINQ Methods Quick Reference

| Method | What It Does | Returns |
|--------|-------------|---------|
| `Where` | Filter items | `IEnumerable<T>` |
| `Select` | Transform items | `IEnumerable<T>` |
| `OrderBy` | Sort ascending | `IOrderedEnumerable<T>` |
| `OrderByDescending` | Sort descending | `IOrderedEnumerable<T>` |
| `FirstOrDefault` | First match or default | `T` or `null` |
| `SingleOrDefault` | Single match or default | `T` or `null` |
| `Any` | Does any item match? | `bool` |
| `All` | Do all items match? | `bool` |
| `Count` | How many items? | `int` |
| `Sum` | Total of values | `decimal` |
| `Average` | Average of values | `decimal` |
| `Min` | Smallest value | `decimal` |
| `Max` | Largest value | `decimal` |
| `GroupBy` | Group by key | `IEnumerable<IGrouping>` |
| `Take` | Take N items | `IEnumerable<T>` |
| `ToList` | Convert to list | `List<T>` |

---

## 🧪 Exercise 4 — LINQ

Using the employee list, write LINQ queries to:

1. Find all employees with salary above 5000
2. Get the names of active employees from IT
3. Sort employees by salary (highest first) and take the top 3
4. Check if any employee from HR has salary above 6000

```csharp
// Your code here
```

---

## 🧪 Exercise 5 — Aggregation

Calculate from the employee list:

1. Total number of employees
2. Number of active employees
3. Average salary
4. Highest salary
5. Number of employees per department

```csharp
// Your code here
```

---

## Summary

| Concept | Syntax | Example |
|---------|--------|---------|
| Filter | `.Where(...)` | `.Where(e => e.IsActive)` |
| Transform | `.Select(...)` | `.Select(e => e.Name)` |
| Sort | `.OrderBy(...)` | `.OrderBy(e => e.Salary)` |
| Find one | `.FirstOrDefault(...)` | `.FirstOrDefault(e => e.Id == 1)` |
| Check | `.Any(...)` / `.All(...)` | `.Any(e => e.Salary > 5000)` |
| Count | `.Count(...)` | `.Count(e => e.IsActive)` |
| Group | `.GroupBy(...)` | `.GroupBy(e => e.Department)` |
| Take | `.Take(n)` | `.Take(3)` |

> 💡 **Senior Developer Note:** LINQ is one of C#'s most powerful features. Keep your queries readable. If a query becomes too complex, break it into smaller steps.

---

**Next: [04 — async / await](04-Async-Await.md)**
