# C# Masterclass: From Fundamentals to Production

**A Comprehensive Developer Guide**

*Written by a Senior Software Engineer & Lead Technical Instructor with 10+ years of industry and teaching experience.*

---

## Table of Contents

1. [Introduction to C#](#1-introduction-to-c)
2. [Variables & Data Types](#2-variables--data-types)
3. [Operators](#3-operators)
4. [Conditions](#4-conditions)
5. [Loops](#5-loops)
6. [Methods](#6-methods)
7. [Arrays](#7-arrays)
8. [Collections](#8-collections)
9. [Generics](#9-generics)
10. [Nullable Types](#10-nullable-types)
11. [Exception Handling](#11-exception-handling)
12. [Object-Oriented Programming](#12-object-oriented-programming)
13. [Advanced C#](#13-advanced-c)
14. [Capstone Project: Employee Management System](#14-capstone-project-employee-management-system)

---

## 1. Introduction to C\#

### What is C#?

C# (pronounced "C-sharp") is a **modern, type-safe, object-oriented programming language** developed by Microsoft, led by Anders Hejlsberg. It runs on the **.NET runtime (CLR)**, which compiles code to **Intermediate Language (IL)** and then to native machine code via the **Just-In-Time (JIT) compiler**.

### Why C#?

| Feature | Benefit |
|---------|---------|
| **Type Safety** | Catches errors at compile time, not runtime |
| **Managed Memory** | Garbage collector handles memory allocation/deallocation |
| **Rich Ecosystem** | NuGet packages, Visual Studio, strong community |
| **Cross-Platform** | .NET 6+ runs on Windows, macOS, Linux |
| **Performance** | Competitive with C++ in modern workloads |
| **Enterprise Standard** | Backbone of banking, healthcare, government software |

### The .NET Compilation Pipeline

```
Source Code (.cs)
      │
      ▼
  C# Compiler (Roslyn)
      │
      ▼
Intermediate Language (IL)
      │
      ▼
CLR + JIT Compiler
      │
      ▼
Native Machine Code
```

> **Pro-Tip**: Understanding the compilation pipeline matters. When you see "Type A cannot be converted to Type B," that is the compiler protecting you. When you see a `NullReferenceException`, the JIT let your code run and it hit a runtime problem. Different problems, different tools.

### Your First C# Program

```csharp
// Every C# application starts with a Main method
// In modern C# (top-level statements), the file IS the entry point

Console.WriteLine("Hello, World!");
```

**What happens behind the scenes:**

```csharp
// The compiler generates something equivalent to:
using System;

namespace MyApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
```

### Project Structure

```
MyProject/
├── MyProject.csproj      ← Project file (dependencies, settings)
├── Program.cs            ← Entry point
├── Models/               ← Data classes
├── Services/             ← Business logic
├── Utils/                ← Helper methods
└── bin/                  ← Compiled output
```

### Common Pitfalls

1. **Forgetting to install .NET SDK**: Always run `dotnet --version` first.
2. **Case sensitivity**: C# is case-sensitive. `MyVar` and `myVar` are different.
3. **Missing semicolons**: Every statement must end with `;`.
4. **Confusing `=` (assignment) with `==` (comparison)**: Use `=` to assign, `==` to compare.

---

## 2. Variables & Data Types

### What is a Variable?

A variable is a **named storage location** in memory that holds a value. Think of it as a labeled box where you can put data.

### Declaration Syntax

```csharp
// Explicit type
int age = 28;

// var keyword (compiler infers type)
var name = "Alice"; // Compiler knows this is a string

// Explicit type with initializer
string email = "alice@example.com";

// Multiple declarations
int x = 1, y = 2, z = 3;
```

### Primitive Data Types

| Type | Size | Range | Default | Use Case |
|------|------|-------|---------|----------|
| `int` | 4 bytes | ±2.1 billion | 0 | Most whole numbers |
| `long` | 8 bytes | ±9.2 quintillion | 0L | Large numbers |
| `short` | 2 bytes | ±32,767 | 0 | Small integers |
| `byte` | 1 byte | 0 to 255 | 0 | Raw data, binary |
| `float` | 4 bytes | ±3.4 × 10^38 | 0f | Scientific calculations |
| `double` | 8 bytes | ±1.7 × 10^308 | 0d | Default decimal type |
| `decimal` | 16 bytes | ±7.9 × 10^28 | 0m | Financial calculations |
| `bool` | 1 bit | true/false | false | Logical values |
| `char` | 2 bytes | Unicode character | '\0' | Single characters |
| `string` | Reference | Variable length | null | Text |

### The `var` Keyword

```csharp
// var lets the compiler infer the type
var number = 42;        // int
var price = 19.99;      // double
var isActive = true;    // bool
var message = "Hello";  // string

// Use var when the type is obvious from context
var users = new List<string>();    // Type is clear
var result = CalculateTotal();     // Type is clear from method name

// Use explicit type when it adds clarity
int retryCount = 3;
string connectionString = "...";

// DON'T do this — type is unclear
var x = GetData();  // What type is GetData() returning?
```

> **Pro-Tip**: `var` is not "dynamic." It is still strongly typed—the compiler determines the type at compile time. Use it when the type is obvious; use explicit types when clarity matters.

### Type Casting

```csharp
// Implicit casting (safe, no data loss)
int myInt = 9;
double myDouble = myInt;    // int → double (widening)

// Explicit casting (potential data loss)
double myDouble2 = 9.78;
int myInt2 = (int)myDouble2; // double → int (narrowing, truncates to 9)

// Convert class (handles type conversions)
string numberStr = "42";
int parsed = int.Parse(numberStr);           // Throws on invalid input
int safe = Convert.ToInt32(numberStr);       // Returns 0 on null
bool success = int.TryParse(numberStr, out int result); // Safe, no exception
```

### Constants and Read-Only

```csharp
// const — compile-time constant, must be assigned at declaration
const double PI = 3.14159265359;
const string AppName = "MyApp";

// readonly — runtime constant, can be assigned in constructor
readonly int _maxRetries;
public MyClass(int maxRetries)
{
    _maxRetries = maxRetries; // Allowed in constructor
}

// _maxRetries = 5; // NOT allowed elsewhere
```

### String Interpolation

```csharp
string name = "Alice";
int age = 28;

// String interpolation (preferred)
string greeting = $"Hello, {name}. You are {age} years old.";

// Verbatim string (preserves newlines)
string path = @"C:\Users\Alice\Documents";

// Raw string literal (C# 11)
string json = """
    {
        "name": "Alice",
        "age": 28
    }
    """;
```

### Common Pitfalls

1. **Integer overflow**: `int.MaxValue + 1` wraps to `int.MinValue`. Use `checked` to catch this:
   ```csharp
   checked { int result = int.MaxValue + 1; } // Throws OverflowException
   ```
2. **Floating-point precision**: `0.1 + 0.2 != 0.3` due to binary representation. Use `decimal` for money.
3. **String immutability**: `string` is immutable. Concatenating in loops creates garbage. Use `StringBuilder`.
4. **Default values**: `bool` defaults to `false`, numeric types to `0`, reference types to `null`.

---

## 3. Operators

### Arithmetic Operators

```csharp
int a = 10, b = 3;

Console.WriteLine(a + b);   // 13  (addition)
Console.WriteLine(a - b);   // 7   (subtraction)
Console.WriteLine(a * b);   // 30  (multiplication)
Console.WriteLine(a / b);   // 3   (integer division, truncates)
Console.WriteLine(a % b);   // 1   (modulo/remainder)
```

### Comparison Operators

```csharp
int x = 5, y = 10;

Console.WriteLine(x == y);  // false (equal)
Console.WriteLine(x != y);  // true  (not equal)
Console.WriteLine(x > y);   // false (greater than)
Console.WriteLine(x < y);   // true  (less than)
Console.WriteLine(x >= 5);  // true  (greater or equal)
Console.WriteLine(x <= 3);  // false (less or equal)
```

### Logical Operators

```csharp
bool a = true, b = false;

Console.WriteLine(a && b);  // false (AND)
Console.WriteLine(a || b);  // true  (OR)
Console.WriteLine(!a);      // false (NOT)

// Short-circuit evaluation: if the first operand determines the result,
// the second is NOT evaluated.
if (a && ExpensiveFunction())  // ExpensiveFunction() never called if a is false
{
    // ...
}
```

### Assignment Operators

```csharp
int x = 10;

x += 5;   // x = x + 5  → 15
x -= 3;   // x = x - 3  → 12
x *= 2;   // x = x * 2  → 24
x /= 4;   // x = x / 4  → 6
x %= 4;   // x = x % 4  → 2
```

### Null-Conditional and Null-Coalescing Operators

```csharp
string? name = GetName();

// Null-conditional (?.): returns null if left side is null
int? length = name?.Length;  // null if name is null, otherwise name.Length

// Null-coalescing (??): provides default if left side is null
string displayName = name ?? "Unknown";

// Null-coalescing assignment (??=): assigns only if null
name ??= "Default";
```

### Ternary Operator

```csharp
int age = 20;
string status = age >= 18 ? "Adult" : "Minor";

// Nested ternary (avoid for readability)
string category = age < 13 ? "Child" : age < 18 ? "Teen" : "Adult";
```

### Type Checking Operators

```csharp
object obj = "Hello";

// is — type check
if (obj is string s)
{
    Console.WriteLine($"String value: {s}");
}

// as — safe cast (returns null if cast fails)
string? str = obj as string;  // "Hello"
string? num = 42 as string;   // null (no exception)

// typeof — gets the System.Type
Type type = typeof(string);
```

> **Pro-Tip**: Always prefer `is` pattern matching over casting. It is safer and more readable.

### Operator Precedence (Simplified)

| Priority | Operators |
|----------|-----------|
| Highest | `()` `.` `?.` `->` |
| | `!` `~` `++` `--` `typeof` `new` |
| | `*` `/` `%` |
| | `+` `-` |
| | `<` `>` `<=` `>=` `is` `as` |
| | `==` `!=` |
| | `&&` `\|\|` |
| | `??` `??=` |
| Lowest | `=` `+=` `-=` `*=` `/=` |

> **Pro-Tip**: When in doubt, use parentheses. They cost nothing and prevent bugs.

---

## 4. Conditions

### if / else if / else

```csharp
int temperature = 35;

if (temperature > 30)
{
    Console.WriteLine("It's hot outside.");
}
else if (temperature > 20)
{
    Console.WriteLine("It's pleasant.");
}
else if (temperature > 10)
{
    Console.WriteLine("It's cool.");
}
else
{
    Console.WriteLine("It's cold.");
}
```

### Pattern Matching with `if`

```csharp
object obj = 42;

// Type pattern
if (obj is int number)
{
    Console.WriteLine($"Integer: {number}");
}

// Constant pattern
if (obj is 42)
{
    Console.WriteLine("The answer!");
}

// Relational pattern (C# 9+)
if (obj is int n and > 0 and < 100)
{
    Console.WriteLine($"Positive two-digit number: {n}");
}
```

### switch Statement

```csharp
string day = "Monday";

switch (day)
{
    case "Monday":
    case "Tuesday":
    case "Wednesday":
    case "Thursday":
    case "Friday":
        Console.WriteLine("Weekday");
        break;
    case "Saturday":
    case "Sunday":
        Console.WriteLine("Weekend");
        break;
    default:
        Console.WriteLine("Invalid day");
        break;
}
```

### switch Expression (C# 8+)

```csharp
string day = "Monday";

string dayType = day switch
{
    "Monday" or "Tuesday" or "Wednesday" or "Thursday" or "Friday" => "Weekday",
    "Saturday" or "Sunday" => "Weekend",
    _ => "Invalid"  // _ is the discard/default pattern
};

// Pattern matching in switch expression
object value = 42;
string description = value switch
{
    int n when n > 0 => "Positive integer",
    int n when n < 0 => "Negative integer",
    0 => "Zero",
    string s => $"String: {s}",
    null => "Null value",
    _ => "Unknown type"
};
```

### Guard Clauses (Early Returns)

Instead of deeply nested `if` blocks, use guard clauses:

```csharp
// BAD — deep nesting
public void ProcessOrder(Order order)
{
    if (order != null)
    {
        if (order.Items.Count > 0)
        {
            if (order.Total > 0)
            {
                // Process order...
            }
        }
    }
}

// GOOD — guard clauses (flat, readable)
public void ProcessOrder(Order? order)
{
    if (order is null)
        return;

    if (order.Items.Count == 0)
        return;

    if (order.Total <= 0)
        return;

    // Process order...
}
```

### Common Pitfalls

1. **Fall-through in switch**: C# does NOT allow fall-through between cases. You must use `break`, `return`, `goto case`, or throw.
2. **String comparison**: `switch` on strings is case-sensitive. Use `StringComparison.OrdinalIgnoreCase` with `if/else` if needed.
3. **Missing default**: Always include a `default` case to handle unexpected values.

---

## 5. Loops

### for Loop

Use when you **know the number of iterations** in advance.

```csharp
for (int i = 0; i < 10; i++)
{
    Console.WriteLine(i);
}

// Counting backwards
for (int i = 10; i >= 0; i--)
{
    Console.WriteLine(i);
}
```

### while Loop

Use when you **don't know how many iterations** you need—loop until a condition is false.

```csharp
int count = 0;
while (count < 5)
{
    Console.WriteLine(count);
    count++;
}
```

### do-while Loop

Executes **at least once**, then repeats while the condition is true.

```csharp
int input;
do
{
    Console.Write("Enter a positive number: ");
} while (!int.TryParse(Console.ReadLine(), out input) || input <= 0);

Console.WriteLine($"You entered: {input}");
```

### foreach Loop

Use for **iterating over collections** (arrays, lists, dictionaries).

```csharp
string[] fruits = { "Apple", "Banana", "Cherry" };

foreach (string fruit in fruits)
{
    Console.WriteLine(fruit);
}

// With index (using LINQ)
foreach (var (fruit, index) in fruits.Select((f, i) => (f, i)))
{
    Console.WriteLine($"{index}: {fruit}");
}
```

### break and continue

```csharp
// break — exit the loop entirely
for (int i = 0; i < 100; i++)
{
    if (i == 5) break;    // Stops at 4
    Console.WriteLine(i);
}

// continue — skip to next iteration
for (int i = 0; i < 10; i++)
{
    if (i % 2 == 0) continue;  // Skips even numbers
    Console.WriteLine(i);       // Prints: 1, 3, 5, 7, 9
}
```

### Nested Loops

```csharp
// Multiplication table
for (int i = 1; i <= 10; i++)
{
    for (int j = 1; j <= 10; j++)
    {
        Console.Write($"{i * j,4}");
    }
    Console.WriteLine();
}
```

### Loop Performance Tips

| Scenario | Recommendation |
|----------|---------------|
| Iterating arrays | `for` is fastest (no enumerator overhead) |
| Iterating collections | `foreach` is cleanest and often optimized |
| Searching | Use LINQ `.Find()`, `.Any()`, `.First()` |
| Nested loops with large data | Consider algorithmic optimization (hash maps) |

### Common Pitfalls

1. **Off-by-one errors**: `for (int i = 0; i <= array.Length; i++)` throws `IndexOutOfRangeException`. Use `<` not `<=`.
2. **Infinite loops**: Always ensure the loop variable changes toward the exit condition.
3. **Modifying collection during foreach**: This throws `InvalidOperationException`. Use `for` or create a copy.
4. **Using `foreach` on large arrays**: `for` is marginally faster due to no enumerator allocation. Micro-optimization—don't overthink it.

---

## 6. Methods

### Method Structure

```csharp
// [access] [modifier] returnType MethodName(parameters)
// {
//     // body
// }

public static int Add(int a, int b)
{
    return a + b;
}
```

### Parameter Types

```csharp
// Value parameter (default) — receives a copy
void Increment(int number)
{
    number++;  // Original unchanged
}

// ref parameter — passes by reference
void Increment(ref int number)
{
    number++;  // Original changes
}

// out parameter — must be assigned inside the method
void Divide(int a, int b, out int quotient, out int remainder)
{
    quotient = a / b;
    remainder = a % b;
}

// in parameter — read-only reference (no copying for large structs)
void PrintReadOnly(in LargeStruct data)
{
    Console.WriteLine(data);  // Cannot modify data
}

// Named arguments (can be passed in any order)
Print(name: "Alice", age: 28);

// Default parameters
void Greet(string name, string greeting = "Hello")
{
    Console.WriteLine($"{greeting}, {name}!");
}
```

### Expression-Bodied Members

```csharp
// Single-line methods
int Square(int x) => x * x;

// Properties (expression-bodied)
public string FullName => $"{FirstName} {LastName}";

// Constructors
public Person(string name) => Name = name;
```

### Overloading

```csharp
// Same method name, different parameter signatures
int Add(int a, int b) => a + b;
double Add(double a, double b) => a + b;
string Add(string a, string b) => a + b;

// Return type is NOT part of the overload signature
// int Add(int a, int b) and double Add(int a, int b) → COMPILE ERROR
```

### Local Functions

```csharp
int Factorial(int n)
{
    return CalculateFactorial(n);

    int CalculateFactorial(int number)
    {
        if (number <= 1) return 1;
        return number * CalculateFactorial(number - 1);
    }
}
```

### Common Pitfalls

1. **Passing reference types by ref**: Usually unnecessary. Classes are already reference types. Use `ref` when you want to reassign the reference itself.
2. **out parameters vs return values**: Prefer return values. Use `out` only when you need to return multiple values (or use a tuple/record).
3. **Over-millisizing methods**: If a method exceeds 30 lines, consider splitting it. Aim for methods that do **one thing**.

---

## 7. Arrays

### Array Declaration

```csharp
// Array initialization
int[] numbers = new int[5];              // [0, 0, 0, 0, 0]
int[] values = { 1, 2, 3, 4, 5 };        // Implicit size
int[] sized = new int[] { 10, 20, 30 };  // Explicit

// Multi-dimensional arrays
int[,] matrix = new int[3, 3]
{
    { 1, 2, 3 },
    { 4, 5, 6 },
    { 7, 8, 9 }
};

// Jagged arrays (array of arrays)
int[][] jagged = new int[3][];
jagged[0] = new int[] { 1, 2 };
jagged[1] = new int[] { 3, 4, 5 };
jagged[2] = new int[] { 6 };
```

### Array Properties and Methods

```csharp
int[] numbers = { 5, 3, 8, 1, 9, 2 };

// Properties
int length = numbers.Length;         // 6
int rank = numbers.Rank;            // 1 (number of dimensions)

// Static methods
Array.Sort(numbers);                // [1, 2, 3, 5, 8, 9]
Array.Reverse(numbers);             // [9, 8, 5, 3, 2, 1]
int index = Array.IndexOf(numbers, 5);  // Find index of value
int found = Array.Find(numbers, x => x > 4);  // First match
bool exists = Array.Exists(numbers, x => x == 9);  // true

// Instance methods
int[] copy = new int[6];
numbers.CopyTo(copy, 0);           // Copy to another array
int[] sliced = numbers[1..4];       // Range operator: [8, 5, 3]
```

### Common Array Operations

```csharp
// Finding elements
int[] data = { 10, 20, 30, 40, 50 };

int first = data[0];           // 10
int last = data[^1];           // 50 (index from end)
int range = data[1..4];        // {20, 30, 40}

// Filtering (convert to list first)
int[] filtered = data.Where(x => x > 25).ToArray();  // {30, 40, 50}

// Transform
int[] doubled = data.Select(x => x * 2).ToArray();   // {20, 40, 60, 80, 100}

// Check if array contains value
bool has30 = data.Contains(30);  // true
```

### When to Use Arrays vs Lists

| Use Array When | Use List When |
|----------------|---------------|
| Fixed, known size | Size changes dynamically |
| Performance-critical code | Need Add/Remove/Insert |
| Multi-dimensional data | Single-dimensional is fine |
| Interop with unmanaged code | General-purpose collection |

### Common Pitfalls

1. **Fixed size**: Arrays cannot grow. Use `List<T>` for dynamic collections.
2. **Reference type arrays**: `string[] names = new string[5]` creates 5 `null` entries, not empty strings.
3. **Cloning**: `array1 = array2` copies the **reference**, not the values. Use `(int[])array2.Clone()` for a copy, or `Array.Copy()`.

---

## 8. Collections

### List\<T\>

`List<T>` is a **dynamic array**—resizable, fast indexed access, slower insert/remove in the middle.

```csharp
// Initialization
List<string> names = new List<string>();
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };

// Adding
names.Add("Alice");
names.Add("Bob");
names.AddRange(new[] { "Charlie", "Diana" });
names.Insert(1, "Zara");  // Insert at index 1

// Removing
names.Remove("Bob");
names.RemoveAt(0);         // Remove first element
names.RemoveAll(n => n.StartsWith("C"));  // Remove by condition

// Querying
string first = names[0];
int count = names.Count;
bool hasAlice = names.Contains("Alice");
int index = names.IndexOf("Charlie");

// Sorting
names.Sort();
names.Reverse();

// Filtering and Transforming
List<int> evens = numbers.Where(n => n % 2 == 0).ToList();
List<string> upper = names.Select(n => n.ToUpper()).ToList();

// Iterating
foreach (string name in names)
{
    Console.WriteLine(name);
}

// Capacity optimization
names.TrimExcess();  // Reduces internal array to actual size
```

### Dictionary\<TKey, TValue\>

`Dictionary<TKey, TValue>` stores **key-value pairs**. O(1) lookup by key.

```csharp
// Initialization
Dictionary<string, int> ages = new Dictionary<string, int>();
Dictionary<string, int> scores = new Dictionary<string, int>
{
    ["Alice"] = 95,
    ["Bob"] = 87,
    ["Charlie"] = 92
};

// Adding
ages["Alice"] = 28;
ages.Add("Bob", 32);  // Throws if key already exists

// Accessing
int aliceAge = ages["Alice"];               // Throws if key not found
ages.TryGetValue("Bob", out int bobAge);     // Safe access, returns bool
int unknown = ages.GetValueOrDefault("Unknown", -1);  // Default if missing

// Checking
bool exists = ages.ContainsKey("Alice");
bool hasValue = ages.ContainsValue(28);

// Removing
ages.Remove("Bob");
ages.Remove("Nonexistent", out _);  // Safe remove, no exception

// Iterating
foreach (KeyValuePair<string, int> entry in ages)
{
    Console.WriteLine($"{entry.Key}: {entry.Value}");
}

// Iterating with deconstruction
foreach (var (name, age) in ages)
{
    Console.WriteLine($"{name}: {age}");
}

// LINQ on dictionaries
var filtered = ages.Where(kv => kv.Value > 25)
                   .ToDictionary(kv => kv.Key, kv => kv.Value);
```

### Other Useful Collections

```csharp
// HashSet<T> — unique elements, O(1) lookup
HashSet<string> uniqueNames = new HashSet<string> { "Alice", "Bob" };
uniqueNames.Add("Alice");  // No effect, already exists

// Queue<T> — FIFO (First In, First Out)
Queue<string> queue = new Queue<string>();
queue.Enqueue("First");
string next = queue.Dequeue();  // "First"

// Stack<T> — LIFO (Last In, First Out)
Stack<string> stack = new Stack<string>();
stack.Push("First");
string top = stack.Pop();  // "First"

// LinkedList<T> — O(1) insert/remove at both ends
LinkedList<string> linked = new LinkedList<string>();
linked.AddFirst("Alice");
linked.AddLast("Bob");
```

### Common Pitfalls

1. **Key not found in Dictionary**: Use `TryGetValue` instead of `[]` indexer to avoid `KeyNotFoundException`.
2. **Modifying collection during iteration**: Use `for` loop with index, or create a copy.
3. **Dictionary key equality**: Keys must implement `GetHashCode` and `Equals` correctly. Custom types as keys need proper implementations.
4. **List capacity**: `List<T>` doubles its internal array when full. If you know the size, initialize with capacity: `new List<int>(1000)`.

---

## 9. Generics

### What Are Generics?

Generics let you write **type-independent code**. You define a placeholder for the type, and the caller specifies it at usage.

```csharp
// Without generics — duplicate code
void PrintInt(int value) => Console.WriteLine(value);
void PrintString(string value) => Console.WriteLine(value);
void PrintDouble(double value) => Console.WriteLine(value);

// With generics — one method for all types
void Print<T>(T value) => Console.WriteLine(value);

Print(42);        // T is int
Print("hello");   // T is string
Print(3.14);      // T is double
```

### Generic Classes

```csharp
// A generic repository pattern
public class Repository<T> where T : class
{
    private readonly List<T> _items = new();

    public void Add(T item) => _items.Add(item);
    public void Remove(T item) => _items.Remove(item);
    public T? GetById(int index) => _items.ElementAtOrDefault(index);
    public IReadOnlyList<T> GetAll() => _items.AsReadOnly();
}

// Usage
var intRepo = new Repository<int>();     // Compile error: int is not a class
var userRepo = new Repository<User>();    // Works, T is User
var orderRepo = new Repository<Order>();  // Works, T is Order
```

### Generic Methods

```csharp
// Generic method with multiple type parameters
TResult Convert<TInput, TResult>(TInput input, Func<TInput, TResult> converter)
{
    return converter(input);
}

// Usage
string result = Convert(42, x => x.ToString());
int length = Convert("hello", s => s.Length);
```

### Generic Constraints

```csharp
// where T : class           — T must be a reference type
// where T : struct         — T must be a value type
// where T : class?         — T can be reference type or null
// where T : struct?        — T can be nullable value type
// where T : notnull        — T must be non-nullable
// where T : new()          — T must have parameterless constructor
// where T : BaseClass      — T must inherit from BaseClass
// where T : IInterface     — T must implement IInterface
// where T : U              — T must derive from U (another type parameter)

public class Service<T> where T : class, IComparable<T>, new()
{
    // T is a reference type
    // T implements IComparable<T>
    // T has a parameterless constructor
}
```

### Generic Type Variance

```csharp
// Covariance (out): produces values (IEnumerable<T>)
IEnumerable<string> strings = new List<string> { "a", "b" };
IEnumerable<object> objects = strings;  // OK, string is subtype of object

// Contravariance (in): consumes values (Action<T>)
Action<object> printObject = obj => Console.WriteLine(obj);
Action<string> printString = printObject;  // OK
```

### Common Pitfalls

1. **Performance of value types**: `List<int>` is faster than `ArrayList` because it avoids boxing/unboxing.
2. **Runtime type checks**: You cannot do `T x = default; if (x is int)` because T is not known at compile time. Use `typeof(T)` or constraints.
3. **Default(T)**: For reference types, `default` is `null`. For value types, it is the zero-initialized value.

---

## 10. Nullable Types

### Why Nullable Types?

In C#, value types (`int`, `bool`, `double`) **cannot be null** by default. But sometimes you need to represent "no value"—a missing database field, an uncomputed result, an optional parameter.

```csharp
// Non-nullable (default)
int number = 42;

// Nullable (can be null)
int? nullableNumber = null;
double? price = null;
bool? isActive = null;

// All are valid
nullableNumber = 42;
nullableNumber = null;
```

### Nullable Operators

```csharp
int? a = null;
int? b = 5;

// Null-conditional (?.)
int? length = a?.Length;  // null if a is null

// Null-coalescing (??)
int valueA = a ?? 0;      // 0 if a is null

// Null-coalescing assignment (??=)
a ??= 10;  // Assigns 10 only if a is null

// Null-conditional with method call
string? name = GetName();
int? nameLength = name?.Trim()?.Length;  // Safe chaining
```

### Nullable Value Types vs Reference Types

```csharp
// With nullable reference types enabled (C# 8+):
string nonNull = "Hello";   // Compiler warns if assigned null
string? canBeNull = null;   // Explicitly nullable

// Nullable value types
int? x = null;   // Nullable<int> struct
double? y = null; // Nullable<double> struct

// Getting the value
int xValue = x.Value;        // Throws if null
int xValue2 = x.GetValueOrDefault();  // Returns default(int) = 0 if null
int xValue3 = x ?? -1;       // Returns -1 if null
```

### Checking Nullable Types

```csharp
int? number = GetNumber();

// Method 1: HasValue property
if (number.HasValue)
{
    Console.WriteLine(number.Value);
}

// Method 2: Null check
if (number != null)
{
    Console.WriteLine(number.Value);
}

// Method 3: Pattern matching
if (number is int n)
{
    Console.WriteLine(n);
}

// Method 4: Null-coalescing
Console.WriteLine(number ?? "No value");
```

### Nullable in Generics

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }         // Reference type, nullable
    public string? ErrorMessage { get; set; }

    public ApiResponse(T data)
    {
        Success = true;
        Data = data;
    }

    public ApiResponse(string error)
    {
        Success = false;
        ErrorMessage = error;
    }
}
```

### Common Pitfalls

1. **Forgetting null checks**: Enable nullable reference types in your project (`<Nullable>enable</Nullable>` in `.csproj`) and let the compiler warn you.
2. **Null-forgiving operator (!)**: `name!.Length` suppresses the warning but throws at runtime if null. Use sparingly.
3. **Nullable value types**: `int?` is actually `Nullable<int>`—a struct. It has overhead compared to plain `int`.

---

## 11. Exception Handling

### What Are Exceptions?

Exceptions are **runtime errors** that disrupt normal program flow. Proper handling prevents crashes and provides meaningful feedback.

```csharp
try
{
    int[] numbers = { 1, 2, 3 };
    Console.WriteLine(numbers[10]);  // Throws IndexOutOfRangeException
}
catch (IndexOutOfRangeException ex)
{
    Console.WriteLine($"Array error: {ex.Message}");
}
```

### Exception Hierarchy

```
System.Exception
├── System.ApplicationException
├── System.SystemException
│   ├── System.ArgumentException
│   │   ├── System.ArgumentNullException
│   │   └── System.ArgumentOutOfRangeException
│   ├── System.InvalidOperationException
│   ├── System.NotSupportedException
│   ├── System.NullReferenceException
│   ├── System.IndexOutOfRangeException
│   ├── System.StackOverflowException
│   └── System.OutOfMemoryException
├── System.IOException
│   ├── System.FileNotFoundException
│   └── System.DirectoryNotFoundException
├── System.Net.WebException
└── System.Data.DataException
    └── System.Data.SqlClient.SqlException
```

### try-catch-finally

```csharp
StreamReader? reader = null;

try
{
    reader = new StreamReader("config.txt");
    string content = reader.ReadToEnd();
    Console.WriteLine(content);
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.FileName}");
}
catch (IOException ex)
{
    Console.WriteLine($"IO error: {ex.Message}");
}
catch (Exception ex)
{
    // Catch-all — always put this last
    Console.WriteLine($"Unexpected error: {ex.Message}");
    throw; // Re-throw to preserve stack trace
}
finally
{
    // ALWAYS executes, whether exception occurs or not
    reader?.Dispose();
    Console.WriteLine("Cleanup complete.");
}
```

### Custom Exceptions

```csharp
// Custom exception with additional context
public class InsufficientFundsException : Exception
{
    public decimal Balance { get; }
    public decimal AttemptedAmount { get; }

    public InsufficientFundsException(decimal balance, decimal amount)
        : base($"Insufficient funds. Balance: {balance:C}, Attempted: {amount:C}")
    {
        Balance = balance;
        AttemptedAmount = amount;
    }
}

// Usage
public class BankAccount
{
    public decimal Balance { get; private set; }

    public void Withdraw(decimal amount)
    {
        if (amount > Balance)
            throw new InsufficientFundsException(Balance, amount);

        Balance -= amount;
    }
}
```

### Exception Filtering

```csharp
try
{
    ProcessData();
}
catch (IOException ex) when (ex.Message.Contains("timeout"))
{
    // Only catches IOException if the message contains "timeout"
    Console.WriteLine("Network timeout — retrying...");
}
catch (IOException ex)
{
    Console.WriteLine($"IO error: {ex.Message}");
}
```

### Best Practices

| Do | Don't |
|----|-------|
| Catch specific exceptions | Use bare `catch (Exception)` without logging |
| Use `finally` for cleanup | Use exceptions for flow control |
| Log exceptions with context | Swallow exceptions silently |
| Create custom exceptions when needed | Catch and re-throw without adding info |
| Use exception filters (`when`) | Nest try-catch blocks deeply |

### Common Pitfalls

1. **Swallowing exceptions**: `catch { }` hides bugs. At minimum, log the exception.
2. **Throwing `ex` instead of `throw`**: `throw ex` resets the stack trace. Use `throw` to preserve it.
3. **Using exceptions for control flow**: Exceptions are expensive. Don't use them for expected conditions (use `TryParse`, null checks, etc.).
4. **Not disposing resources**: Always use `using` statements for `IDisposable` objects.

```csharp
// The using statement automatically calls Dispose()
using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    // Use connection...
}  // Dispose called here automatically

// C# 8+ using declaration
using var connection = new SqlConnection(connectionString);
connection.Open();
// Dispose called at end of scope
```

---

## 12. Object-Oriented Programming

### Classes and Objects

A **class** is a blueprint. An **object** is an instance of that blueprint.

```csharp
// Class definition
public class Person
{
    // Properties (auto-implemented)
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; private set; }  // Read-only outside

    // Constructor
    public Person(string name, int age, string email)
    {
        Name = name;
        Age = age;
        Email = email;
    }

    // Constructor overload
    public Person(string name) : this(name, 0, "unknown@example.com") { }

    // Method
    public string GetSummary()
    {
        return $"{Name} ({Age}) — {Email}";
    }

    // Override ToString
    public override string ToString()
    {
        return GetSummary();
    }
}

// Creating objects
Person alice = new Person("Alice", 28, "alice@example.com");
Person bob = new Person("Bob");

Console.WriteLine(alice.GetSummary());
Console.WriteLine(bob);  // Calls ToString()
```

### Encapsulation

Encapsulation is the practice of **bundling data and methods** that operate on that data within a class, and **restricting direct access** to some components.

```csharp
public class BankAccount
{
    // Private fields — hidden from outside
    private decimal _balance;
    private readonly List<decimal> _transactions = new();

    // Public properties — controlled access
    public decimal Balance => _balance;
    public string AccountNumber { get; }
    public IReadOnlyList<decimal> Transactions => _transactions.AsReadOnly();

    // Constructor
    public BankAccount(string accountNumber, decimal initialBalance = 0)
    {
        AccountNumber = accountNumber ?? throw new ArgumentNullException(nameof(accountNumber));
        _balance = initialBalance;
    }

    // Public methods — controlled operations
    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive.", nameof(amount));

        _balance += amount;
        _transactions.Add(amount);
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive.", nameof(amount));

        if (amount > _balance)
            throw new InvalidOperationException("Insufficient funds.");

        _balance -= amount;
        _transactions.Add(-amount);
    }
}
```

**Why this matters**: External code cannot modify `_balance` directly. All changes go through `Deposit` and `Withdraw`, which enforce business rules.

### Inheritance

Inheritance lets a class **derive from another class**, inheriting its properties and methods.

```csharp
// Base class
public class Animal
{
    public string Name { get; set; }
    public int Age { get; set; }

    public Animal(string name, int age)
    {
        Name = name;
        Age = age;
    }

    public virtual string Speak()
    {
        return $"{Name} makes a sound.";
    }
}

// Derived class
public class Dog : Animal
{
    public string Breed { get; set; }

    public Dog(string name, int age, string breed) : base(name, age)
    {
        Breed = breed;
    }

    // Override base method
    public override string Speak()
    {
        return $"{Name} barks!";
    }
}

// Another derived class
public class Cat : Animal
{
    public bool IsIndoor { get; set; }

    public Cat(string name, int age, bool isIndoor) : base(name, age)
    {
        IsIndoor = isIndoor;
    }

    public override string Speak()
    {
        return $"{Name} meows!";
    }
}
```

### Polymorphism

Polymorphism means **many forms**—the same interface, different implementations.

```csharp
// Base class reference can hold derived class objects
Animal myDog = new Dog("Rex", 5, "German Shepherd");
Animal myCat = new Cat("Whiskers", 3, true);

// Same method call, different behavior
Console.WriteLine(myDog.Speak());  // "Rex barks!"
Console.WriteLine(myCat.Speak());  // "Whiskers meows!"

// Polymorphic collection
List<Animal> animals = new List<Animal>
{
    new Dog("Buddy", 4, "Golden Retriever"),
    new Cat("Luna", 2, false),
    new Dog("Max", 7, "Beagle")
};

foreach (Animal animal in animals)
{
    Console.WriteLine(animal.Speak());  // Each animal speaks differently
}
```

### Abstract Classes

Abstract classes **cannot be instantiated**—they serve as base classes for other classes to inherit from.

```csharp
public abstract class Shape
{
    public string Color { get; set; }

    // Abstract method — MUST be implemented by derived classes
    public abstract double CalculateArea();

    // Virtual method — CAN be overridden
    public virtual string Describe()
    {
        return $"A {Color} shape with area {CalculateArea():F2}";
    }
}

public class Circle : Shape
{
    public double Radius { get; set; }

    public override double CalculateArea()
    {
        return Math.PI * Radius * Radius;
    }
}

public class Rectangle : Shape
{
    public double Width { get; set; }
    public double Height { get; set; }

    public override double CalculateArea()
    {
        return Width * Height;
    }
}
```

### Interfaces

An interface is a **contract**—it defines what a class must do, not how.

```csharp
// Interface definition
public interface IRepository<T> where T : class
{
    T? GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
}

// Interface with default implementation (C# 8+)
public interface ILoggable
{
    string Log() => ToString();
}

// A class can implement multiple interfaces
public class UserRepository : IRepository<User>, ILoggable
{
    private readonly List<User> _users = new();

    public User? GetById(int id) => _users.FirstOrDefault(u => u.Id == id);
    public IEnumerable<User> GetAll() => _users.AsReadOnly();
    public void Add(User entity) => _users.Add(entity);
    public void Update(User entity)
    {
        var existing = GetById(entity.Id);
        if (existing != null)
        {
            existing.Name = entity.Name;
            existing.Email = entity.Email;
        }
    }
    public void Delete(int id) => _users.RemoveAll(u => u.Id == id);

    public string Log() => $"UserRepository with {_users.Count} users";
}

// Interface as parameter type
public void ProcessRepository(IRepository<User> repo)
{
    var users = repo.GetAll();
    // ...
}
```

### Composition

Composition is **building complex objects by combining simpler ones**. Prefer composition over inheritance.

```csharp
// Components
public class Engine
{
    public int Horsepower { get; set; }
    public string FuelType { get; set; }

    public Engine(int horsepower, string fuelType)
    {
        Horsepower = horsepower;
        FuelType = fuelType;
    }
}

public class GPS
{
    public string GetCurrentLocation() => "40.7128° N, 74.0060° W";
}

// Composite class
public class Car
{
    public string Make { get; set; }
    public string Model { get; set; }
    public Engine Engine { get; }       // Composition
    public GPS Navigation { get; }     // Composition

    public Car(string make, string model, Engine engine, GPS gps)
    {
        Make = make;
        Model = model;
        Engine = engine;
        Navigation = gps;
    }

    public string GetInfo()
    {
        return $"{Make} {Model} — {Engine.Horsepower}HP {Engine.FuelType} | Location: {Navigation.GetCurrentLocation()}";
    }
}
```

### Access Modifiers

| Modifier | Class | Assembly | Derived Class | World |
|----------|-------|----------|---------------|-------|
| `public` | Yes | Yes | Yes | Yes |
| `private` | Yes | No | No | No |
| `protected` | Yes | No | Yes | No |
| `internal` | Yes | Yes | No | No |
| `protected internal` | Yes | Yes | Yes | No |
| `private protected` | Yes | No | Yes (same assembly) | No |

```csharp
public class MyClass
{
    public int PublicField = 1;           // Accessible everywhere
    private int _privateField = 2;        // Accessible only in this class
    protected int _protectedField = 3;    // Accessible in derived classes
    internal int _internalField = 4;      // Accessible in same assembly
    protected internal int _bothField = 5; // Either protected OR internal
    private protected int _bothField2 = 6; // protected AND internal (both conditions)
}
```

### Common OOP Pitfalls

1. **God classes**: A class that does everything. Split into focused, single-responsibility classes.
2. **Deep inheritance chains**: More than 2-3 levels of inheritance is a design smell. Use composition.
3. **Favoring composition over inheritance**: Inheritance creates tight coupling. Composition gives flexibility.
4. **Not using interfaces**: Interfaces enable testability, dependency injection, and loose coupling.

---

## 13. Advanced C\#

### LINQ (Language Integrated Query)

LINQ lets you query collections with a **declarative syntax**.

```csharp
List<int> numbers = new List<int> { 5, 3, 8, 1, 9, 2, 7, 4, 6 };

// Method syntax (preferred)
var evenNumbers = numbers.Where(n => n % 2 == 0).ToList();          // {8, 2, 4, 6}
var sorted = numbers.OrderBy(n => n).ToList();                       // {1, 2, 3, 4, 5, 6, 7, 8, 9}
var sum = numbers.Sum();                                             // 45
var average = numbers.Average();                                     // 5.0
var first = numbers.First(n => n > 5);                               // 8
var count = numbers.Count(n => n > 4);                               // 5
var hasAny = numbers.Any(n => n == 9);                               // true
var allPositive = numbers.All(n => n > 0);                           // true
var grouped = numbers.GroupBy(n => n % 2 == 0 ? "Even" : "Odd");    // Group by even/odd

// Query syntax (SQL-like)
var query = from n in numbers
            where n > 3
            orderby n descending
            select n;

// Chaining
var result = numbers
    .Where(n => n > 2)
    .OrderBy(n => n)
    .Select(n => new { Value = n, Square = n * n })
    .ToList();

// Complex LINQ with objects
List<Employee> employees = GetEmployees();

var result = employees
    .Where(e => e.Department == "Engineering" && e.Salary > 80000)
    .OrderByDescending(e => e.Salary)
    .Select(e => new { e.Name, e.Salary, e.Department })
    .Take(10)
    .ToList();
```

### Lambda Expressions

Lambdas are **anonymous functions**—concise syntax for creating delegates or expression trees.

```csharp
// Lambda syntax
Func<int, int> square = x => x * x;
Func<int, int, int> add = (a, b) => a + b;
Action<string> print = message => Console.WriteLine(message);

// Multi-line lambda
Func<int, int, string> describe = (a, b) =>
{
    int sum = a + b;
    return $"{a} + {b} = {sum}";
};

// Lambda in LINQ
var results = numbers.Where(n => n > 5).Select(n => n * 2);

// Lambda in sorting
var sorted = employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName);
```

### Delegates

Delegates are **type-safe function pointers**—they let you pass methods as arguments.

```csharp
// Delegate declaration
public delegate int MathOperation(int a, int b);

// Using delegates
public static int Add(int a, int b) => a + b;
public static int Multiply(int a, int b) => a * b;

MathOperation operation = Add;
Console.WriteLine(operation(3, 4));  // 7

operation = Multiply;
Console.WriteLine(operation(3, 4));  // 12

// Built-in delegates
Func<int, int, int> func = (a, b) => a + b;   // Returns a value
Action<string> action = msg => Console.WriteLine(msg);  // No return value
Predicate<int> isPositive = n => n > 0;        // Returns bool

// Delegate as method parameter
public static int ApplyOperation(int a, int b, MathOperation operation)
{
    return operation(a, b);
}

int result = ApplyOperation(5, 3, Add);       // 8
result = ApplyOperation(5, 3, Multiply);      // 15
```

### async/await and Task

Asynchronous programming lets you **perform non-blocking operations**—the UI stays responsive, and threads aren't wasted waiting.

```csharp
// async method returns Task or Task<T>
public async Task<string> FetchDataAsync(string url)
{
    using HttpClient client = new();
    // await suspends this method, returns control to the caller
    // When the HTTP call completes, execution resumes here
    string data = await client.GetStringAsync(url);
    return data;
}

// Calling async methods
public async Task ProcessDataAsync()
{
    try
    {
        string data = await FetchDataAsync("https://api.example.com/data");
        Console.WriteLine($"Received {data.Length} characters");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Request failed: {ex.Message}");
    }
}

// Running multiple async operations in parallel
public async Task LoadDashboardAsync()
{
    var userTask = GetUserAsync(1);
    var ordersTask = GetOrdersAsync(1);
    var notificationsTask = GetNotificationsAsync(1);

    // All three run concurrently
    await Task.WhenAll(userTask, ordersTask, notificationsTask);

    User user = await userTask;
    List<Order> orders = await ordersTask;
    List<Notification> notifications = await notificationsTask;
}

// async/await with Task.Run (CPU-bound work)
public async Task<int> ComputeExpensiveValueAsync()
{
    return await Task.Run(() =>
    {
        // CPU-intensive work on thread pool
        int result = 0;
        for (int i = 0; i < 1000000; i++)
        {
            result += i;
        }
        return result;
    });
}
```

**Rules of async/await:**
1. Method must be marked `async`
2. Use `await` to asynchronously wait for `Task` or `Task<T>`
3. Method name conventionally ends with `Async`
4. Don't use `.Result` or `.Wait()`—it causes deadlocks
5. `async void` is only for event handlers—avoid it everywhere else

### Records (C# 9+)

Records are **immutable data types** with built-in equality, `ToString`, and destructuring.

```csharp
// Record declaration
public record Person(string Name, int Age);

// Usage
var alice = new Person("Alice", 28);
var bob = new Person("Alice", 28);

Console.WriteLine(alice);              // Person { Name = Alice, Age = 28 }
Console.WriteLine(alice == bob);       // True (value equality)
Console.WriteLine(alice.GetHashCode()); // Same hash as bob

// Record with mutable properties
public record PersonWithMutable(string Name, int Age)
{
    public string Email { get; set; } = string.Empty;
}

// Record with custom logic
public record Temperature(double Celsius)
{
    public double Fahrenheit => Celsius * 9 / 5 + 32;
    public double Kelvin => Celsius + 273.15;
}

// Inheritance
public record Employee(string Name, int Age, string Department) : Person(Name, Age);
```

### Pattern Matching (C# 9+)

```csharp
// Property patterns
public static string ClassifyAnimal(Animal animal) => animal switch
{
    { Name: "Rex" } => "That is Rex",
    { Age: > 5 } => "Old animal",
    { Age: < 1 } => "Baby animal",
    _ => "Unknown animal"
};

// Relational patterns
public static string GetTaxBracket(decimal income) => income switch
{
    < 0 => "Invalid",
    < 10000 => "Low",
    < 50000 => "Medium",
    < 100000 => "High",
    _ => "Very High"
};

// Logical patterns
public static string Classify(int value) => value switch
{
    > 0 and < 100 => "Small positive",
    >= 100 and <= 1000 => "Medium",
    > 1000 or < 0 => "Extreme",
    _ => "Zero"
};

// Type patterns with when
public static string ProcessObject(object obj) => obj switch
{
    int n when n > 0 => $"Positive integer: {n}",
    int n => $"Non-positive integer: {n}",
    string s when s.Length > 10 => $"Long string: {s}",
    string s => $"Short string: {s}",
    null => "Null",
    _ => $"Unknown type: {obj.GetType()}"
};
```

### Extension Methods

Extension methods let you **add methods to existing types** without modifying them.

```csharp
// Extension method definition
public static class StringExtensions
{
    // 'this' keyword marks the extended type
    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length <= maxLength ? value : value[..maxLength] + "...";
    }

    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return System.Globalization.CultureInfo.CurrentCulture
            .TextInfo.ToTitleCase(value.ToLower());
    }
}

// Usage (called as if they were native methods)
string name = "hello world";
Console.WriteLine(name.IsNullOrEmpty());        // False
Console.WriteLine(name.Truncate(5));            // "hello..."
Console.WriteLine(name.ToTitleCase());          // "Hello World"

// Extension method on any collection
public static class CollectionExtensions
{
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (T item in source)
        {
            action(item);
        }
    }

    public static T? RandomElement<T>(this IEnumerable<T> source)
    {
        var list = source as IList<T> ?? source.ToList();
        return list.Count == 0 ? default : list[new Random().Next(list.Count)];
    }
}

// Usage
numbers.ForEach(n => Console.WriteLine(n));
string randomFruit = fruits.RandomElement();
```

### Common Advanced C# Pitfalls

1. **async void**: Only for event handlers. Use `async Task` everywhere else.
2. **Blocking async code**: Never call `.Result` or `.Wait()` on a Task. Always use `await`.
3. **LINQ deferred execution**: `Where`, `Select`, etc. are lazy. Call `.ToList()` to force evaluation.
4. **Overusing extension methods**: Don't extend core types with confusing methods. Keep them intuitive.

---

## 14. Capstone Project: Employee Management System

A complete, production-quality console application demonstrating all concepts from this masterclass.

### Project Structure

```
EmployeeManagement/
├── Models/
│   ├── Employee.cs
│   ├── Department.cs
│   └── Enums.cs
├── Interfaces/
│   ├── IRepository.cs
│   └── INotificationService.cs
├── Services/
│   ├── EmployeeService.cs
│   ├── DepartmentService.cs
│   └── ConsoleNotificationService.cs
├── Extensions/
│   └── CollectionExtensions.cs
├── Exceptions/
│   └── EmployeeNotFoundException.cs
└── Program.cs
```

### Models/Enums.cs

```csharp
namespace EmployeeManagement.Models;

public enum EmployeeRole
{
    Junior,
    MidLevel,
    Senior,
    Lead,
    Manager,
    Director
}

public enum EmployeeStatus
{
    Active,
    OnLeave,
    Terminated
}
```

### Models/Employee.cs

```csharp
namespace EmployeeManagement.Models;

public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public EmployeeRole Role { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public int DepartmentId { get; set; }
    public DateTime HireDate { get; set; } = DateTime.UtcNow;

    public string FullName => $"{FirstName} {LastName}";

    public override string ToString()
    {
        return $"[{Id}] {FullName} | {Role} | {Status} | {Salary:C}";
    }
}
```

### Models/Department.cs

```csharp
namespace EmployeeManagement.Models;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"[{Id}] {Name}";
    }
}
```

### Interfaces/IRepository.cs

```csharp
namespace EmployeeManagement.Interfaces;

/// <summary>
/// Generic repository contract for CRUD operations.
/// Demonstrates Generics, Interfaces, and encapsulation.
/// </summary>
public interface IRepository<T> where T : class
{
    T? GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
    int Count { get; }
}
```

### Interfaces/INotificationService.cs

```csharp
namespace EmployeeManagement.Interfaces;

/// <summary>
/// Abstraction for sending notifications.
/// Demonstrates Dependency Inversion and Polymorphism.
/// </summary>
public interface INotificationService
{
    Task SendAsync(string recipient, string subject, string message);
}
```

### Exceptions/EmployeeNotFoundException.cs

```csharp
namespace EmployeeManagement.Exceptions;

/// <summary>
/// Custom exception for when an employee is not found.
/// Demonstrates Custom Exceptions and exception handling patterns.
/// </summary>
public class EmployeeNotFoundException : Exception
{
    public int EmployeeId { get; }

    public EmployeeNotFoundException(int employeeId)
        : base($"Employee with ID {employeeId} was not found.")
    {
        EmployeeId = employeeId;
    }

    public EmployeeNotFoundException(int employeeId, Exception innerException)
        : base($"Employee with ID {employeeId} was not found.", innerException)
    {
        EmployeeId = employeeId;
    }
}
```

### Extensions/CollectionExtensions.cs

```csharp
namespace EmployeeManagement.Extensions;

/// <summary>
/// Extension methods for collections.
/// Demonstrates Extension Methods and Generics.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Executes an action on each element in the collection.
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (T item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// Returns a random element from the collection.
    /// </summary>
    public static T? RandomElement<T>(this IReadOnlyList<T> source)
    {
        if (source is null || source.Count == 0)
            return default;

        return source[Random.Shared.Next(source.Count)];
    }

    /// <summary>
    /// Partitions the collection into pages.
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Paginate<T>(this IEnumerable<T> source, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .Select((item, index) => new { item, index })
            .GroupBy(x => x.index / pageSize)
            .Select(group => group.Select(x => x.item));
    }
}
```

### Services/EmployeeService.cs

```csharp
using EmployeeManagement.Exceptions;
using EmployeeManagement.Interfaces;
using EmployeeManagement.Models;

namespace EmployeeManagement.Services;

/// <summary>
/// Handles employee business logic.
/// Demonstrates Encapsulation, Exception Handling, LINQ, and async patterns.
/// </summary>
public class EmployeeService
{
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<Department> _departmentRepository;
    private readonly INotificationService _notificationService;

    // Constructor injection — demonstrates Dependency Inversion
    public EmployeeService(
        IRepository<Employee> employeeRepository,
        IRepository<Department> departmentRepository,
        INotificationService notificationService)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    /// <summary>
    /// Adds a new employee with validation.
    /// </summary>
    public async Task<Employee> AddEmployeeAsync(Employee employee)
    {
        // Validation
        ValidateEmployee(employee);

        // Check for duplicate email
        var existing = _employeeRepository.GetAll()
            .FirstOrDefault(e => e.Email.Equals(employee.Email, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
            throw new InvalidOperationException($"An employee with email '{employee.Email}' already exists.");

        // Assign ID
        employee.Id = _employeeRepository.Count > 0
            ? _employeeRepository.GetAll().Max(e => e.Id) + 1
            : 1;

        _employeeRepository.Add(employee);

        // Send welcome notification
        await _notificationService.SendAsync(
            employee.Email,
            "Welcome to the Company!",
            $"Hello {employee.FirstName}, welcome aboard!");

        return employee;
    }

    /// <summary>
    /// Updates an existing employee.
    /// </summary>
    public async Task<Employee> UpdateEmployeeAsync(int id, Employee updates)
    {
        var employee = _employeeRepository.GetById(id)
            ?? throw new EmployeeNotFoundException(id);

        // Validate department exists
        if (_departmentRepository.GetById(updates.DepartmentId) is null)
            throw new InvalidOperationException($"Department with ID {updates.DepartmentId} does not exist.");

        // Update fields
        employee.FirstName = updates.FirstName;
        employee.LastName = updates.LastName;
        employee.Email = updates.Email;
        employee.Salary = updates.Salary;
        employee.Role = updates.Role;
        employee.DepartmentId = updates.DepartmentId;

        _employeeRepository.Update(employee);

        // Notify manager if role changed
        if (updates.Role == EmployeeRole.Manager || updates.Role == EmployeeRole.Director)
        {
            await _notificationService.SendAsync(
                employee.Email,
                "Role Updated",
                $"Congratulations! You have been promoted to {updates.Role}.");
        }

        return employee;
    }

    /// <summary>
    /// Terminates an employee (soft delete).
    /// </summary>
    public async Task TerminateEmployeeAsync(int id)
    {
        var employee = _employeeRepository.GetById(id)
            ?? throw new EmployeeNotFoundException(id);

        employee.Status = EmployeeStatus.Terminated;
        _employeeRepository.Update(employee);

        await _notificationService.SendAsync(
            employee.Email,
            "Employment Status Update",
            "Your employment status has been updated. Please contact HR for details.");
    }

    /// <summary>
    /// Gets employee by ID with null-safe navigation.
    /// </summary>
    public Employee? GetEmployee(int id)
    {
        return _employeeRepository.GetById(id);
    }

    /// <summary>
    /// Gets all active employees.
    /// Demonstrates LINQ filtering and projection.
    /// </summary>
    public IEnumerable<Employee> GetActiveEmployees()
    {
        return _employeeRepository.GetAll()
            .Where(e => e.Status == EmployeeStatus.Active)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName);
    }

    /// <summary>
    /// Searches employees by name or email.
    /// Demonstrates LINQ with complex predicates.
    /// </summary>
    public IEnumerable<Employee> SearchEmployees(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<Employee>();

        return _employeeRepository.GetAll()
            .Where(e =>
                e.FirstName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.LastName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.Email.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.LastName);
    }

    /// <summary>
    /// Gets employees by department.
    /// Demonstrates LINQ GroupBy and joins.
    /// </summary>
    public IEnumerable<Employee> GetEmployeesByDepartment(int departmentId)
    {
        return _employeeRepository.GetAll()
            .Where(e => e.DepartmentId == departmentId)
            .OrderBy(e => e.Role)
            .ThenBy(e => e.LastName);
    }

    /// <summary>
    /// Gets department statistics.
    /// Demonstrates LINQ aggregation methods.
    /// </summary>
    public object GetDepartmentStats(int departmentId)
    {
        var employees = _employeeRepository.GetAll()
            .Where(e => e.DepartmentId == departmentId && e.Status == EmployeeStatus.Active)
            .ToList();

        var department = _departmentRepository.GetById(departmentId);

        return new
        {
            Department = department?.Name ?? "Unknown",
            TotalEmployees = employees.Count,
            AverageSalary = employees.Any() ? employees.Average(e => e.Salary) : 0,
            TotalSalary = employees.Sum(e => e.Salary),
            RoleDistribution = employees
                .GroupBy(e => e.Role)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
        };
    }

    /// <summary>
    /// Validates employee data.
    /// Demonstrates Guard Clauses and validation patterns.
    /// </summary>
    private static void ValidateEmployee(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        if (string.IsNullOrWhiteSpace(employee.FirstName))
            throw new ArgumentException("First name is required.", nameof(employee));

        if (string.IsNullOrWhiteSpace(employee.LastName))
            throw new ArgumentException("Last name is required.", nameof(employee));

        if (string.IsNullOrWhiteSpace(employee.Email) || !employee.Email.Contains('@'))
            throw new ArgumentException("A valid email is required.", nameof(employee));

        if (employee.Salary < 0)
            throw new ArgumentException("Salary cannot be negative.", nameof(employee));
    }
}
```

### Services/DepartmentService.cs

```csharp
using EmployeeManagement.Interfaces;
using EmployeeManagement.Models;

namespace EmployeeManagement.Services;

/// <summary>
/// Handles department business logic.
/// Demonstrates CRUD operations with repository pattern.
/// </summary>
public class DepartmentService
{
    private readonly IRepository<Department> _repository;

    public DepartmentService(IRepository<Department> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Department AddDepartment(string name, string description = "")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Department name is required.", nameof(name));

        var department = new Department
        {
            Id = _repository.Count > 0
                ? _repository.GetAll().Max(d => d.Id) + 1
                : 1,
            Name = name,
            Description = description
        };

        _repository.Add(department);
        return department;
    }

    public Department? GetDepartment(int id) => _repository.GetById(id);

    public IEnumerable<Department> GetAllDepartments() => _repository.GetAll();
}
```

### Services/ConsoleNotificationService.cs

```csharp
using EmployeeManagement.Interfaces;

namespace EmployeeManagement.Services;

/// <summary>
/// Console-based notification service.
/// Demonstrates Interface implementation and Polymorphism.
/// </summary>
public class ConsoleNotificationService : INotificationService
{
    public Task SendAsync(string recipient, string subject, string message)
    {
        // Simulate async I/O (e.g., sending an email or SMS)
        return Task.Run(() =>
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  [NOTIFICATION] To: {recipient}");
            Console.WriteLine($"  Subject: {subject}");
            Console.WriteLine($"  Message: {message}");
            Console.WriteLine();
            Console.ResetColor();
        });
    }
}
```

### Program.cs

```csharp
using EmployeeManagement.Extensions;
using EmployeeManagement.Models;
using EmployeeManagement.Services;

namespace EmployeeManagement;

/// <summary>
/// Main application entry point.
/// Demonstrates all C# concepts in a real-world console application.
/// </summary>
public class Program
{
    // In-memory repositories (replace with database in production)
    private static readonly List<Employee> _employees = new();
    private static readonly List<Department> _departments = new();

    // Services
    private static EmployeeService _employeeService = null!;
    private static DepartmentService _departmentService = null!;

    public static async Task Main(string[] args)
    {
        // Initialize services with dependency injection
        var employeeRepo = new InMemoryEmployeeRepository();
        var departmentRepo = new InMemoryDepartmentRepository();
        var notificationService = new ConsoleNotificationService();

        _employeeService = new EmployeeService(employeeRepo, departmentRepo, notificationService);
        _departmentService = new DepartmentService(departmentRepo);

        // Seed data
        SeedData();

        // Main application loop
        await RunApplicationAsync();
    }

    private static async Task RunApplicationAsync()
    {
        bool running = true;

        while (running)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║   Employee Management System v1.0   ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nPlease select an option:");
            Console.WriteLine("1. List All Employees");
            Console.WriteLine("2. Add New Employee");
            Console.WriteLine("3. Update Employee");
            Console.WriteLine("4. Terminate Employee");
            Console.WriteLine("5. Search Employees");
            Console.WriteLine("6. View Department Statistics");
            Console.WriteLine("7. List Departments");
            Console.WriteLine("0. Exit");

            Console.Write("\nYour choice: ");
            string? choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await ListEmployeesAsync();
                        break;
                    case "2":
                        await AddEmployeeAsync();
                        break;
                    case "3":
                        await UpdateEmployeeAsync();
                        break;
                    case "4":
                        await TerminateEmployeeAsync();
                        break;
                    case "5":
                        SearchEmployees();
                        break;
                    case "6":
                        ViewDepartmentStats();
                        break;
                    case "7":
                        ListDepartments();
                        break;
                    case "0":
                        running = false;
                        Console.WriteLine("Goodbye!");
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError: {ex.Message}");
                Console.ResetColor();
            }

            if (running)
            {
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }
    }

    private static async Task ListEmployeesAsync()
    {
        Console.WriteLine("\n--- Active Employees ---\n");

        var employees = _employeeService.GetActiveEmployees().ToList();

        if (employees.Count == 0)
        {
            Console.WriteLine("No active employees found.");
            return;
        }

        // Using our extension method
        employees.ForEach(e => Console.WriteLine($"  {e}"));

        Console.WriteLine($"\nTotal: {employees.Count} employee(s)");
    }

    private static async Task AddEmployeeAsync()
    {
        Console.WriteLine("\n--- Add New Employee ---\n");

        Console.Write("First Name: ");
        string firstName = Console.ReadLine() ?? string.Empty;

        Console.Write("Last Name: ");
        string lastName = Console.ReadLine() ?? string.Empty;

        Console.Write("Email: ");
        string email = Console.ReadLine() ?? string.Empty;

        Console.Write("Salary: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal salary))
        {
            Console.WriteLine("Invalid salary. Using 0.");
            salary = 0;
        }

        Console.Write("Role (Junior/MidLevel/Senior/Lead/Manager/Director): ");
        if (!Enum.TryParse<EmployeeRole>(Console.ReadLine(), true, out EmployeeRole role))
        {
            Console.WriteLine("Invalid role. Using Junior.");
            role = EmployeeRole.Junior;
        }

        Console.Write("Department ID: ");
        if (!int.TryParse(Console.ReadLine(), out int departmentId))
        {
            Console.WriteLine("Invalid department ID. Using 1.");
            departmentId = 1;
        }

        var employee = new Employee
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Salary = salary,
            Role = role,
            DepartmentId = departmentId
        };

        var result = await _employeeService.AddEmployeeAsync(employee);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nEmployee added successfully: {result}");
        Console.ResetColor();
    }

    private static async Task UpdateEmployeeAsync()
    {
        Console.Write("\nEnter Employee ID to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID.");
            return;
        }

        var existing = _employeeService.GetEmployee(id);
        if (existing is null)
        {
            Console.WriteLine("Employee not found.");
            return;
        }

        Console.WriteLine($"\nCurrent: {existing}");
        Console.WriteLine("Enter new values (leave blank to keep current):\n");

        Console.Write($"First Name [{existing.FirstName}]: ");
        string firstName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(firstName)) firstName = existing.FirstName;

        Console.Write($"Last Name [{existing.LastName}]: ");
        string lastName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(lastName)) lastName = existing.LastName;

        Console.Write($"Email [{existing.Email}]: ");
        string email = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(email)) email = existing.Email;

        Console.Write($"Salary [{existing.Salary}]: ");
        string salaryInput = Console.ReadLine();
        decimal salary = string.IsNullOrWhiteSpace(salaryInput)
            ? existing.Salary
            : decimal.Parse(salaryInput);

        Console.Write($"Role [{existing.Role}]: ");
        string roleInput = Console.ReadLine();
        EmployeeRole role = string.IsNullOrWhiteSpace(roleInput)
            ? existing.Role
            : Enum.Parse<EmployeeRole>(roleInput, true);

        var updates = new Employee
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Salary = salary,
            Role = role,
            DepartmentId = existing.DepartmentId
        };

        var result = await _employeeService.UpdateEmployeeAsync(id, updates);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nEmployee updated: {result}");
        Console.ResetColor();
    }

    private static async Task TerminateEmployeeAsync()
    {
        Console.Write("\nEnter Employee ID to terminate: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID.");
            return;
        }

        var employee = _employeeService.GetEmployee(id);
        if (employee is null)
        {
            Console.WriteLine("Employee not found.");
            return;
        }

        Console.WriteLine($"\nEmployee: {employee}");
        Console.Write("Are you sure you want to terminate? (y/n): ");

        if (Console.ReadLine()?.ToLower() == "y")
        {
            await _employeeService.TerminateEmployeeAsync(id);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Employee terminated successfully.");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine("Termination cancelled.");
        }
    }

    private static void SearchEmployees()
    {
        Console.Write("\nEnter search query: ");
        string? query = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(query))
        {
            Console.WriteLine("Please enter a search term.");
            return;
        }

        var results = _employeeService.SearchEmployees(query).ToList();

        Console.WriteLine($"\n--- Search Results ({results.Count} found) ---\n");

        if (results.Count == 0)
        {
            Console.WriteLine("No employees match your search.");
            return;
        }

        results.ForEach(e => Console.WriteLine($"  {e}"));
    }

    private static void ViewDepartmentStats()
    {
        Console.Write("\nEnter Department ID: ");
        if (!int.TryParse(Console.ReadLine(), out int departmentId))
        {
            Console.WriteLine("Invalid department ID.");
            return;
        }

        var stats = _employeeService.GetDepartmentStats(departmentId);

        Console.WriteLine($"\n--- Department Statistics ---\n");
        Console.WriteLine(stats);
    }

    private static void ListDepartments()
    {
        Console.WriteLine("\n--- Departments ---\n");

        var departments = _departmentService.GetAllDepartments().ToList();

        if (departments.Count == 0)
        {
            Console.WriteLine("No departments found.");
            return;
        }

        departments.ForEach(d => Console.WriteLine($"  {d}"));
    }

    private static void SeedData()
    {
        // Seed departments
        _departmentService.AddDepartment("Engineering", "Software development and infrastructure");
        _departmentService.AddDepartment("Human Resources", "People management and recruitment");
        _departmentService.AddDepartment("Marketing", "Brand and customer acquisition");
        _departmentService.AddDepartment("Finance", "Financial planning and accounting");

        // Seed employees
        var employees = new[]
        {
            new Employee { FirstName = "Alice", LastName = "Johnson", Email = "alice@company.com", Salary = 95000, Role = EmployeeRole.Senior, DepartmentId = 1 },
            new Employee { FirstName = "Bob", LastName = "Smith", Email = "bob@company.com", Salary = 75000, Role = EmployeeRole.MidLevel, DepartmentId = 1 },
            new Employee { FirstName = "Charlie", LastName = "Brown", Email = "charlie@company.com", Salary = 110000, Role = EmployeeRole.Lead, DepartmentId = 1 },
            new Employee { FirstName = "Diana", LastName = "Lee", Email = "diana@company.com", Salary = 85000, Role = EmployeeRole.Manager, DepartmentId = 2 },
            new Employee { FirstName = "Eve", LastName = "Wilson", Email = "eve@company.com", Salary = 65000, Role = EmployeeRole.Junior, DepartmentId = 3 },
            new Employee { FirstName = "Frank", LastName = "Garcia", Email = "frank@company.com", Salary = 120000, Role = EmployeeRole.Director, DepartmentId = 4 }
        };

        foreach (var emp in employees)
        {
            _employeeService.AddEmployeeAsync(emp).Wait();
        }
    }
}

/// <summary>
/// In-memory implementation of IRepository for Employee.
/// Demonstrates Interface implementation.
/// </summary>
internal class InMemoryEmployeeRepository : Interfaces.IRepository<Employee>
{
    private readonly List<Employee> _employees = new();

    public int Count => _employees.Count;

    public Employee? GetById(int id) => _employees.FirstOrDefault(e => e.Id == id);
    public IEnumerable<Employee> GetAll() => _employees.AsReadOnly();
    public void Add(Employee entity) => _employees.Add(entity);
    public void Update(Employee entity)
    {
        var existing = GetById(entity.Id);
        if (existing is not null)
        {
            var index = _employees.IndexOf(existing);
            _employees[index] = entity;
        }
    }
    public void Delete(int id) => _employees.RemoveAll(e => e.Id == id);
}

/// <summary>
/// In-memory implementation of IRepository for Department.
/// </summary>
internal class InMemoryDepartmentRepository : Interfaces.IRepository<Department>
{
    private readonly List<Department> _departments = new();

    public int Count => _departments.Count;

    public Department? GetById(int id) => _departments.FirstOrDefault(d => d.Id == id);
    public IEnumerable<Department> GetAll() => _departments.AsReadOnly();
    public void Add(Department entity) => _departments.Add(entity);
    public void Update(Department entity)
    {
        var existing = GetById(entity.Id);
        if (existing is not null)
        {
            var index = _departments.IndexOf(existing);
            _departments[index] = entity;
        }
    }
    public void Delete(int id) => _departments.RemoveAll(d => d.Id == id);
}
```

### Concepts Demonstrated in the Capstone

| Concept | Where It Appears |
|---------|-----------------|
| **Classes & Objects** | Employee, Department, EmployeeService |
| **Encapsulation** | Private fields, public methods with validation |
| **Inheritance** | Department inherits from object, records |
| **Polymorphism** | Interface implementations, method overriding |
| **Abstraction** | IRepository, INotificationService interfaces |
| **Interfaces** | IRepository, INotificationService |
| **Composition** | EmployeeService composes repositories + notification service |
| **Access Modifiers** | public, private, internal across all files |
| **Generics** | IRepository\<T\> |
| **LINQ** | Where, Select, OrderBy, GroupBy, Sum, Average, Any, First |
| **Lambda Expressions** | All LINQ queries |
| **async/await** | AddEmployeeAsync, UpdateEmployeeAsync, TerminateEmployeeAsync |
| **Exception Handling** | try-catch, custom exceptions, guard clauses |
| **Nullable Types** | Nullable references, null-conditional operators |
| **Extension Methods** | ForEach, RandomElement, Paginate |
| **Pattern Matching** | switch expressions, null checks |

---

## Summary: C# Concepts at a Glance

| Concept | Core Idea | Senior Tip |
|---------|-----------|------------|
| Variables & Types | Store data with type safety | Use `var` when type is obvious, explicit when clarity matters |
| Operators | Manipulate values | Always use parentheses over memorized precedence |
| Conditions | Control flow | Use guard clauses, pattern matching, switch expressions |
| Loops | Repeat operations | `for` for performance, `foreach` for readability |
| Methods | Reusable logic blocks | Single responsibility, max 20 lines |
| Arrays | Fixed-size collections | Use List for dynamic sizes |
| Collections | Dynamic data structures | Dictionary for fast lookup, List for ordered data |
| Generics | Type-independent code | Eliminates boxing, enforces type safety |
| Nullable Types | Represent missing values | Enable nullable reference types in every project |
| Exception Handling | Graceful error management | Catch specific, log always, use `using` for disposal |
| OOP | Structured, maintainable code | Favor composition, use interfaces everywhere |
| LINQ | Declarative data queries | Chain operations, prefer method syntax |
| async/await | Non-blocking operations | Never block with `.Result`, use `Task.WhenAll` for parallelism |

---

*This masterclass covers the complete C# developer journey from fundamentals to production-quality code. Practice each section, build projects, and revisit these patterns regularly. Mastery comes from repetition and real-world application.*

---

**Last Updated**: September 2026

**Version**: 1.0
