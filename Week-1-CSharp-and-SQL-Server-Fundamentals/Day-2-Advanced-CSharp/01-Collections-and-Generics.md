# 01 — Collections and Generics

---

## The Problem

Imagine you need to store 500 employee names.

Yesterday you would write:

```csharp
string employee1 = "Ali";
string employee2 = "Sara";
string employee3 = "Omar";
// ... 497 more variables
```

> This is impossible. We need a better way to store groups of data.

---

## What Is a Collection?

A collection is a **container that holds multiple values** of the same type.

Instead of creating 500 variables, you create one collection:

```csharp
List<string> employees = new List<string>
{
    "Ali",
    "Sara",
    "Omar"
};
```

One variable. Many values.

---

## The Main Collections

### Array — Fixed Size

```csharp
string[] names = { "Ali", "Sara", "Omar" };
```

- Fixed size — cannot add or remove items after creation
- Fast access by index
- Use when you know the exact number of items

```csharp
Console.WriteLine(names[0]);  // Ali
// names[3] = "Ahmed";       // Error — only 3 elements
```

### List — Dynamic Size

```csharp
List<string> names = new List<string> { "Ali", "Sara" };
names.Add("Omar");        // Now has 3 items
names.Remove("Sara");     // Now has 2 items
```

- Grows and shrinks as needed
- Most commonly used collection
- Fast access by index
- Use when the number of items changes

### Dictionary — Key-Value Pairs

```csharp
Dictionary<int, string> employees = new Dictionary<int, string>
{
    { 1, "Ali" },
    { 2, "Sara" },
    { 3, "Omar" }
};

Console.WriteLine(employees[2]);  // Sara
```

- Each item has a **key** and a **value**
- Fast lookup by key
- Use when you need to find items by a unique identifier

### HashSet — Unique Items

```csharp
HashSet<string> departments = new HashSet<string>
{
    "IT",
    "HR",
    "IT"      // Duplicate — ignored
};

Console.WriteLine(departments.Count);  // 2, not 3
```

- No duplicates allowed
- Fast check: "does this item exist?"
- Use when you need unique values

---

## When to Choose Which?

| Collection | Size | Duplicates | Lookup By | Use When |
|------------|------|------------|-----------|----------|
| `Array` | Fixed | Yes | Index | Size never changes |
| `List<T>` | Dynamic | Yes | Index | Items are added/removed |
| `Dictionary<K,V>` | Dynamic | Keys: No | Key | Need fast lookup by ID |
| `HashSet<T>` | Dynamic | No | Value | Need unique items only |

> 💡 **Senior Developer Note:** Start with `List<T>` unless you have a specific reason to choose something else. It covers most everyday situations.

---

## 🤔 Think

> You need to store 100 products. Each product has an ID and a name. You need to find a product by its ID quickly. Which collection would you choose?

**Answer:** `Dictionary<int, string>` — because you need fast lookup by ID.

---

## The Problem with Non-Generic Collections

Before generics, C# had collections that could store **any type**:

```csharp
// Old approach (avoid this)
ArrayList list = new ArrayList();
list.Add("Ali");
list.Add(42);        // No compile error — but dangerous!
```

The problem: the list does not know what type it contains. When you get an item back, you must convert it manually:

```csharp
string name = (string)list[0];   // Works
int number = (int)list[1];       // Works
string oops = (string)list[1];   // Runtime error!
```

> This is unsafe and hard to maintain.

---

## What Are Generics?

Generics let you create a collection (or method) that **works with a specific type**, while keeping the code reusable.

```csharp
List<string> names = new List<string>();     // Only strings
List<int> numbers = new List<int>();         // Only integers
List<Employee> employees = new List<Employee>(); // Only Employee objects
```

Now the compiler protects you:

```csharp
List<string> names = new List<string>();
names.Add("Ali");
names.Add(42);       // Compile error! Only strings allowed.
```

> Generics give us **type safety** and **reusable code** at the same time.

---

## Generic Methods

You can write methods that work with any type:

```csharp
static void PrintItem<T>(T item)
{
    Console.WriteLine(item);
}
```

**Usage:**

```csharp
PrintItem("Hello");   // T is string
PrintItem(42);        // T is int
PrintItem(3.14);      // T is double
```

The compiler figures out the type automatically.

---

## Generic Classes

You can create classes that work with any type:

```csharp
public class Repository<T>
{
    private List<T> _items = new List<T>();

    public void Add(T item)
    {
        _items.Add(item);
    }

    public T? GetById(int index)
    {
        if (index >= 0 && index < _items.Count)
            return _items[index];
        return default;
    }
}
```

**Usage:**

```csharp
var employeeRepo = new Repository<Employee>();
employeeRepo.Add(new Employee { Name = "Ali" });

var productRepo = new Repository<Product>();
productRepo.Add(new Product { Name = "Laptop" });
```

One class. Reusable for any type.

---

## 🧪 Exercise 1 — Collections

Create a `List<Employee>` with 5 employees. Loop through the list and display each employee's name.

```csharp
// Your code here
```

---

## 🧪 Exercise 2 — Generics

Create a generic `Repository<T>` class with:

- `Add(T item)` method
- `GetAll()` method that returns the list

Test it with `Employee` and `Department`.

```csharp
// Your code here
```

---

## Summary

| Concept | What It Does |
|---------|-------------|
| `Array` | Fixed-size collection |
| `List<T>` | Dynamic-size collection |
| `Dictionary<K,V>` | Key-value lookup |
| `HashSet<T>` | Unique items only |
| Generics `<T>` | Type-safe reusable code |

> 💡 **Senior Developer Note:** Choose the simplest collection that works. Most of the time, `List<T>` is enough. Do not reach for `Dictionary` or `HashSet` unless you have a clear reason.

---

**Next: [02 — Lambda Expressions](02-Lambda-Expressions.md)**
