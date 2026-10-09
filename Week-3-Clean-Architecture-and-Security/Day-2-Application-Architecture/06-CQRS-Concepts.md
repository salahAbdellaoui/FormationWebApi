# 06 — CQRS Concepts

**Duration:** 20 minutes

---

## What Is CQRS?

CQRS (Command Query Responsibility Segregation) separates read operations (queries) from write operations (commands).

- **Command:** An operation that changes state. Create, Update, Delete.
- **Query:** An operation that retrieves data without changing state. Get, Search, List.

The core idea: the methods that write data and the methods that read data have different concerns, different optimization opportunities, and potentially different models.

---

## Our Application Already Does This

Look at `EmployeeService`:

```csharp
// Queries (read)
public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)

// Command (write)
public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
```

We already separate reads from writes. The `Get` methods retrieve data. The `Create` method changes state. We just don't call it CQRS.

This is the simplest form of CQRS: separate methods for separate responsibilities.

---

## The Basic Idea

```
Commands (Write)          Queries (Read)
    |                         |
    v                         v
Change state            Retrieve data
    |                         |
    v                         v
CreateEmployee          GetEmployeeById
UpdateEmployee          GetAllEmployees
DeleteEmployee          SearchEmployees
```

Commands modify the system. Queries observe the system. They're fundamentally different operations, even when they work with the same data.

---

## What CQRS Does NOT Require

Explicitly: CQRS does not mean any of these:

- **Separate databases.** You can use the same database for reads and writes.
- **Event sourcing.** You don't need to store events instead of state.
- **MediatR or message brokers.** You don't need a framework or messaging infrastructure.
- **Microservices.** CQRS works in a monolith.
- **Complex infrastructure.** No special patterns required.

At its simplest, CQRS is just: **use different methods and models for reads and writes.** We already do this. `CreateEmployeeDto` is for writes. `EmployeeDto` is for reads. They're different models.

---

## When Basic CQRS Is Enough

For most applications, separating commands and queries in the service layer is sufficient. You don't need a framework, a message broker, or separate databases.

Basic CQRS:
- Different methods for reads and writes
- Different input/output models
- Same database, same ORM, same service class

This gives you the benefits of CQRS (clear separation, optimized models) without the complexity.

---

## When Advanced CQRS Might Help

Advanced CQRS (separate read models, event sourcing, message queues) becomes useful when:

- **Different performance requirements:** Reads need to be fast and scalable. Writes are slower but less frequent. You optimize them differently.
- **Scaling reads independently:** You need read replicas or a separate read database (e.g., Elasticsearch for search, Redis for caching).
- **Complex queries:** Your reads involve aggregations, denormalized views, or complex joins that benefit from a different data model.
- **Event-driven architecture:** You want to react to domain events, build audit trails, or integrate with other systems.

These are real problems with real solutions. But they're not problems every application has.

---

## Why Not Implement Full CQRS Here?

For this Employee Management application:

- **Simple CRUD operations.** Create, read. No complex queries, no aggregations, no search.
- **No complex queries.** We load employees with their departments. That's it.
- **No scaling requirements.** This is a teaching application, not a high-traffic production system.
- **Adding MediatR would be over-engineering.** Introducing a mediator pattern, command/query handlers, and pipeline behaviors for two methods adds complexity without benefit.

The current service-based approach is appropriate. We have clear separation of concerns, simple operations, and no scaling pressure. Full CQRS would be solving problems we don't have.

---

## The Key Insight

CQRS is a **spectrum**.

At one end: separate methods for reads and writes in a service class. We already do this.

At the other end: separate databases, event sourcing, message queues, read replicas, denormalized views. We don't need this.

**Choose the level that matches your problem.**

If you have simple CRUD, basic CQRS (separate methods) is enough.
If you have complex queries and scaling needs, advanced CQRS might help.

Don't add complexity until the problem demands it. But recognize that you're already doing CQRS when you separate reads from writes.

---

## Common Mistakes

**Mistake 1: Thinking CQRS requires MediatR**
CQRS is a pattern, not a framework. You can implement it with plain methods in a service class. MediatR is one way to organize CQRS, but it's not required.

**Mistake 2: Implementing CQRS for a simple CRUD app**
If your application has 5 endpoints and simple queries, full CQRS with MediatR, command handlers, and query handlers is over-engineering. Use the simplest approach that works.

**Mistake 3: Separating commands and queries but still using the same database connection**
This is fine! CQRS doesn't require separate databases. If someone tells you "you're not doing real CQRS without separate read models," they're gatekeeping. CQRS is about separation of concerns, not infrastructure.

**Mistake 4: Adding complexity without benefit**
Introducing CQRS "because it's best practice" without a concrete problem to solve is a mistake. Every layer of abstraction has a cost. Make sure the benefit exceeds the cost.

---

## Knowledge Check

**Question 1:** What is the difference between a command and a query?

<details>
<summary>Answer</summary>

A command changes state (Create, Update, Delete). A query retrieves data without changing state (Get, Search, List). Commands modify the system. Queries observe it.
</details>

**Question 2:** Does CQRS require separate databases for reads and writes?

<details>
<summary>Answer</summary>

No. CQRS is about separating commands and queries in your code — different methods, different models. You can use the same database for both. Separate databases are one advanced option, not a requirement.
</details>

**Question 3:** Why is full CQRS (with MediatR, command handlers, etc.) not appropriate for this Employee Management application?

<details>
<summary>Answer</summary>

The application has simple CRUD operations, no complex queries, and no scaling requirements. Introducing MediatR and handler classes for two methods adds complexity without benefit. Basic CQRS (separate methods in a service) is sufficient and clearer.
</details>
