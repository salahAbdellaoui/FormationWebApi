# Day 2 — Application Architecture

**Week 3 — Clean Architecture and Security**

**Duration**: 3 hours (180 minutes)

**Level**: Intermediate (builds on Day 1)

**Prerequisites**: Day 1 (Clean Architecture four-project solution)

---

## What You Will Learn

By the end of this session, you will be able to:

- Explain the Dependency Inversion Principle and distinguish it from Dependency Injection
- Justify why the Application layer defines interfaces that Infrastructure implements
- Describe the Repository Pattern and articulate its benefits and costs
- Apply the Unit of Work pattern to coordinate multiple repository operations
- Design service classes that encapsulate use cases and orchestrate business logic
- Create DTOs that decouple the API contract from domain entities
- Evaluate when to use manual mapping versus a mapping library
- Describe the CQRS pattern and identify the problems it solves
- Make deliberate architectural decisions based on trade-offs rather than convention

---

## Training Schedule

| Section | Duration |
|---|---|
| Dependency Inversion | 25 min |
| Repository Pattern | 30 min |
| Unit of Work | 25 min |
| Services and Use Cases | 30 min |
| DTOs and Mapping | 25 min |
| CQRS Concepts | 20 min |
| Integrated exercise and review | 25 min |
| **Total** | **180 min** |

---

## Topics

| # | Topic | File |
|---|---|---|
| 01 | Dependency Inversion | [01-Dependency-Inversion.md](01-Dependency-Inversion.md) |
| 02 | Repository Pattern | [02-Repository-Pattern.md](02-Repository-Pattern.md) |
| 03 | Unit of Work | [03-Unit-of-Work.md](03-Unit-of-Work.md) |
| 04 | Services and Use Cases | [04-Services-and-Use-Cases.md](04-Services-and-Use-Cases.md) |
| 05 | DTOs and Mapping | [05-DTOs-and-Mapping.md](05-DTOs-and-Mapping.md) |
| 06 | CQRS Concepts | [06-CQRS-Concepts.md](06-CQRS-Concepts.md) |
| 07 | Guided Practice | [07-Guided-Practice.md](07-Guided-Practice.md) |

---

## The Big Picture

Day 1 built the structure. Day 2 explains why each piece exists.

```text
Day 1: Built the four-layer structure
         |
         v
Day 2: Understand WHY each pattern exists and its trade-offs
         |
         v
Day 3: Security (authentication, authorization)
```

On Day 1, you created four projects: Domain, Application, Infrastructure, and API. You defined interfaces in Application and implemented them in Infrastructure. You wired everything together with Dependency Injection in the composition root.

That gave you a working solution. But working is not the same as understood. Today, you examine every pattern you used and ask: why this? What problem does it solve? What does it cost? When should you skip it?

The goal is not to add more patterns. The goal is to understand the ones you already have.

---

## Practical Outcome

This lesson uses the code you built on Day 1. You will not add new projects, new libraries, or new patterns to the solution. Instead, you will dissect what is already there.

Every pattern has a cost. Every abstraction adds indirection. Today's lesson teaches you to recognize when a pattern earns its keep and when it creates overhead without value.

By the end of this session, you should be able to look at any architectural pattern and answer three questions:

1. What problem does it solve?
2. What does it cost?
3. When is the cost worth paying?

---

## Completion Checklist

By the end of this session, you should be able to:

- [ ] Explain DIP without confusing it with DI, and give a concrete example from the codebase
- [ ] Describe what the Repository Pattern provides and when `DbContext` alone is sufficient
- [ ] Implement a Unit of Work that coordinates `SaveChangesAsync` across multiple repositories
- [ ] Justify why `EmployeeService` depends on `IEmployeeRepository` instead of `EmployeeRepository`
- [ ] Explain why DTOs exist and what breaks if you return entities directly from the API
- [ ] Describe the CQRS pattern and identify a scenario where it adds value
- [ ] Audit a codebase for unnecessary indirection and unnecessary directness
- [ ] Make a deliberate, defensible decision about which patterns to keep or remove

---

## Next: Day 3

Day 3 adds security to the Clean Architecture solution. You will implement JWT-based authentication, role-based authorization, and policy-based access control. The layered structure you understand deeply after today makes security concerns easy to add without modifying business logic or breaking the Dependency Rule.
