# 02 — Lambda Expressions

---

## The Problem

You have a list of employees. You need to find only the active ones.

**Without lambdas**, you write a separate method:

```csharp
static bool IsActive(Employee emp)
{
    return emp.IsActive;
}

// Then pass it somewhere
var activeEmployees = employees.Where(IsActive);
```

This works, but you need a full method for a simple one-line check.

> Can we write the condition more concisely?

---

## What Is a Lambda?

A lambda is a **short, anonymous function**. It lets you write logic inline without creating a named method.

```csharp
// Traditional method
static bool IsActive(Employee emp)
{
    return emp.IsActive;
}

// Lambda — same thing, shorter
emp => emp.IsActive
```

Both do the same job. The lambda is shorter.

---

## Lambda Syntax

```text
(input) => expression
```

| Part | Meaning |
|------|---------|
| `input` | The parameter(s) |
| `=>` | "goes to" (reads as "arrow") |
| `expression` | What the lambda returns |

### Examples

```csharp
// One parameter
emp => emp.Name

// One parameter with condition
emp => emp.Salary > 5000

// Multiple parameters
(x, y) => x + y

// No parameters
() => Console.WriteLine("Hello")

// Multiple statements (use braces)
emp =>
{
    Console.WriteLine(emp.Name);
    Console.WriteLine(emp.Salary);
}
```

---

## Lambda with Collections

Lambdas are most useful when passed to collection methods:

```csharp
List<Employee> employees = new List<Employee>
{
    new Employee { Name = "Ali", Salary = 5000, IsActive = true },
    new Employee { Name = "Sara", Salary = 7000, IsActive = true },
    new Employee { Name = "Omar", Salary = 3000, IsActive = false }
};

// Find active employees
var active = employees.Where(emp => emp.IsActive);

// Find high earners
var highSalary = employees.Where(emp => emp.Salary > 4000);

// Get just the names
var names = employees.Select(emp => emp.Name);
```

---

## The Evolution

```text
Step 1: Separate method
        ↓
Step 2: Lambda inline
        ↓
Step 3: LINQ (next topic)
```

Lambdas are the bridge between old-style code and modern LINQ queries.

---

## 🤔 Think

> What is the difference between `emp => emp.Name` and `emp => emp.Salary > 5000`?

**Answer:** The first returns a value (the name). The second returns a boolean (true/false). This is important because methods like `Where()` need a boolean condition.

---

## Lambda As a Variable

You can store a lambda in a variable:

```csharp
Func<Employee, bool> isHighSalary = emp => emp.Salary > 5000;

bool result = isHighSalary(new Employee { Salary = 7000 });  // true
```

- `Func<T, TResult>` — a delegate that takes input and returns a result
- `Action<T>` — a delegate that takes input but returns nothing

You do not need to memorize these. Just know they exist.

---

## Lambda with Methods

Lambdas can call methods:

```csharp
employees.Where(emp => emp.Name.StartsWith("A"));
employees.Select(emp => emp.Name.ToUpper());
employees.Where(emp => emp.Department.Name.Equals("IT"));
```

The lambda can use any valid C# expression.

---

## 🧪 Exercise 3 — Lambda

Given this list:

```csharp
List<Employee> employees = new List<Employee>
{
    new Employee { Name = "Ali", Salary = 5000, Department = "IT" },
    new Employee { Name = "Sara", Salary = 7000, Department = "HR" },
    new Employee { Name = "Omar", Salary = 3000, Department = "IT" },
    new Employee { Name = "Ahmed", Salary = 6000, Department = "Finance" }
};
```

Write lambdas to:

1. Find employees from the IT department
2. Find employees with salary above 5000
3. Get the names of all employees

```csharp
// Your code here
```

---

## Summary

| Syntax | Meaning |
|--------|---------|
| `x => x.Name` | Return the Name property |
| `x => x.Salary > 5000` | Return true if salary > 5000 |
| `(x, y) => x + y` | Two parameters, return sum |
| `() => Console.WriteLine("Hi")` | No parameters |

> 💡 **Senior Developer Note:** Keep lambdas short and readable. If a lambda becomes longer than one line, consider using a regular method instead.

---

**Next: [03 — LINQ](03-LINQ.md)**
