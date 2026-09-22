# 05 — Practical Exercises

---

## Employee Data Setup

Use this data for all exercises:

```csharp
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

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

## Exercise 1 — Collections

Create a `List<Employee>` and:

1. Add 3 more employees
2. Remove an employee by name
3. Loop through the list and display all employees

```csharp
// Your code here
```

---

## Exercise 2 — Generics

Create a generic `Repository<T>` class:

```csharp
public class Repository<T>
{
    private List<T> _items = new List<T>();

    public void Add(T item) { /* your code */ }
    public List<T> GetAll() { /* your code */ }
    public T? GetById(int index) { /* your code */ }
}
```

Test it with `Employee`.

```csharp
// Your code here
```

---

## Exercise 3 — Lambda

Using the employee list, write lambdas to:

1. Find all employees from the IT department
2. Find employees with salary above 5500
3. Get names of all inactive employees

```csharp
// Your code here
```

---

## Exercise 4 — LINQ Queries

Write LINQ queries for:

1. Active employees sorted by salary (highest first)
2. The top 3 highest-paid employees
3. Names of employees whose name starts with "A"
4. First employee from the Finance department

```csharp
// Your code here
```

---

## Exercise 5 — Aggregation

Calculate:

1. Total number of employees
2. Number of active employees
3. Average salary across all employees
4. Highest salary in the IT department
5. Number of employees per department

```csharp
// Your code here
```

---

## Exercise 6 — async / await

Create an async method that simulates loading employees:

```csharp
public async Task<List<Employee>> LoadEmployeesAsync()
{
    // Simulate a 2-second database call
    await Task.Delay(2000);

    // Return the employee list
    return employees;
}
```

Then call it and display the result.

```csharp
// Your code here
```

---

## 🏆 Final Challenge — Employee Query System

Create a small console application that demonstrates everything from today.

### Requirements

1. Create an `Employee` class with: Id, Name, Salary, Department, IsActive
2. Create a list of at least 8 employees
3. Create a `Repository<T>` generic class
4. Add all employees to the repository
5. Write methods that use LINQ to:
   - Get active employees
   - Get employees by department
   - Get the highest-paid employee
   - Get the average salary
   - Get employee count per department
6. Create an async method that loads employees (simulate delay)
7. Display results to the console

### Starter Code

```csharp
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

// 1. Create Repository<T>

// 2. Create EmployeeService with LINQ methods

// 3. Create async loading method

// 4. Test everything
```

### Expected Output

```text
=== Active Employees ===
Ali - IT - 5000
Sara - HR - 7000
Ahmed - Finance - 6000
Fatima - HR - 8000
Khalid - IT - 4500

=== IT Employees ===
Ali - 5000
Omar - 3000
Khalid - 4500

=== Highest Paid Employee ===
Fatima - 8000

=== Average Salary ===
5571.43

=== Employees Per Department ===
IT: 3
HR: 2
Finance: 2

=== Async Loading ===
Loading employees...
Loaded 10 employees in 2.01 seconds
```

---

## 🧠 Knowledge Check

### Question 1

Which collection should you use when you need to find items by a unique key?

- A) `List<T>`
- B) `Dictionary<K,V>`
- C) `Array`
- D) `HashSet<T>`

**Answer:** B) `Dictionary<K,V>` — provides fast lookup by key.

---

### Question 2

What does `FirstOrDefault()` return when no items match?

- A) Throws an exception
- B) Returns the first item
- C) Returns null (for reference types) or default value
- D) Returns an empty list

**Answer:** C) Returns null or default value.

---

### Question 3

What is the output?

```csharp
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
var result = numbers.Where(n => n > 3);
Console.WriteLine(result.Count());
```

**Answer:** `2` — numbers 4 and 5 match the condition.

---

### Question 4

What does `await` do?

- A) Makes code run faster
- B) Blocks the thread until the task completes
- C) Pauses the method without blocking the thread
- D) Creates a new thread

**Answer:** C) Pauses the method without blocking the thread.

---

### Question 5

Which is correct?

```csharp
// A
var result = GetEmployeesAsync().Result;

// B
var result = await GetEmployeesAsync();
```

**Answer:** B) Using `.Result` can cause deadlocks. Always use `await`.

---

### Question 6

What is deferred execution in LINQ?

- A) LINQ queries execute when defined
- B) LINQ queries execute when iterated
- C) LINQ queries never execute
- D) LINQ queries execute only with `.ToList()`

**Answer:** B) Queries execute when you iterate over them.

---

### Question 7

What does this return?

```csharp
employees.Any(e => e.Salary > 10000)
```

**Answer:** `false` — no employee has salary above 10000.

---

### Question 8

When should you use `.ToList()`?

- A) Always, for every LINQ query
- B) When you need to iterate multiple times or cache the result
- C) Only when the compiler requires it
- D) Never

**Answer:** B) When you need to iterate multiple times or cache the result.

---

### Question 9

What is the difference between `Where` and `Select`?

- A) `Where` transforms items, `Select` filters them
- B) `Where` filters items, `Select` transforms them
- C) They do the same thing
- D) `Where` sorts items, `Select` groups them

**Answer:** B) `Where` filters, `Select` transforms.

---

### Question 10

Why is `async` needed on a method that uses `await`?

- A) It is optional
- B) The compiler requires it to know the method is asynchronous
- C) It makes the method run on a new thread
- D) It prevents errors

**Answer:** B) The compiler requires it.

---

### Question 11

What is the output?

```csharp
var query = employees.Where(e => e.IsActive);
employees.RemoveAt(0);
Console.WriteLine(query.Count());
```

**Answer:** Depends on timing. Because of deferred execution, the query re-evaluates when `.Count()` is called, so the removed employee will not be included.

---

### Question 12

When should you NOT use async/await?

- A) When calling a database
- B) When making an HTTP request
- C) When doing a simple calculation with no I/O
- D) When reading a file

**Answer:** C) Simple calculations do not benefit from async.

---

## 📊 Common Mistakes Summary

| Mistake | Why It's Wrong | Fix |
|---------|---------------|-----|
| Choosing wrong collection | Inefficient operations | Match collection to your needs |
| Unreadable LINQ chains | Hard to maintain | Break into steps |
| Forgetting deferred execution | Unexpected behavior | Use `.ToList()` when needed |
| Using `.Result` or `.Wait()` | Can cause deadlocks | Use `await` |
| Making everything async | Unnecessary complexity | Only async for I/O operations |
| Not using `await` | Lost results | Always await async calls |
| Overusing `Dictionary` | Unnecessary complexity | Use `List<T>` unless key lookup needed |

---

## 💡 Senior Developer Takeaways

1. **Choose the simplest collection** that works. `List<T>` covers most cases.
2. **Use generics** to make code reusable and type-safe.
3. **Keep lambdas short.** If it exceeds one line, use a method.
4. **Write readable LINQ.** If a query is hard to read, break it into steps.
5. **Use async/await for I/O operations.** Database calls, HTTP requests, file operations.
6. **Never use `.Result` or `.Wait()`.** Always `await`.
7. **Use `.ToList()` intentionally**, not by default.

---

**Return to [Day 2 Overview](Day-2-Advanced-CSharp.md)**
