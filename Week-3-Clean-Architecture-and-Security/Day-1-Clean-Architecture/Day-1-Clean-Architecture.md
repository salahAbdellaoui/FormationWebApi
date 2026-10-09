# Day 1 — Clean Architecture

**Week 3 — Clean Architecture and Security**

**Duration**: 3 hours

**Level**: Intermediate (builds on Week 2)

**Prerequisites**: Week 2 (ASP.NET Core Web API, EF Core)

---

## Learning Objectives

By the end of this session, you will be able to:

- Explain why a flat controller-based API becomes unmaintainable as the application grows
- Describe the four layers of Clean Architecture: Domain, Application, Infrastructure, and API
- Apply the Dependency Rule: source code dependencies point inward toward Domain
- Distinguish between compile-time project references (inward) and runtime request flow (outward to inward)
- Define interfaces in the inner layer and implement them in the outer layer (Dependency Inversion)
- Build a 4-project .NET 8 solution following Clean Architecture principles
- Use Dependency Injection as the bridge between compile-time separation and runtime wiring
- Identify common mistakes: EF Core in Domain, business rules in controllers, circular references
- Trace a request through all four layers from HTTP to database and back
- Decide when Clean Architecture is appropriate and when a simpler structure suffices

---

## Training Schedule

| Section | Duration |
|---|---|
| Architecture principles | 25 min |
| Separation of Concerns | 20 min |
| Understanding the four layers | 35 min |
| Dependency direction and project references | 25 min |
| Guided implementation | 45 min |
| Exercises and review | 30 min |
| **Total** | **180 min** |

---

## Topics

| # | Topic | File |
|---|---|---|
| 01 | Architecture Principles | [01-Architecture-Principles.md](01-Architecture-Principles.md) |
| 02 | Separation of Concerns | [02-Separation-of-Concerns.md](02-Separation-of-Concerns.md) |
| 03 | Domain Layer | [03-Domain-Layer.md](03-Domain-Layer.md) |
| 04 | Application Layer | [04-Application-Layer.md](04-Application-Layer.md) |
| 05 | Infrastructure Layer | [05-Infrastructure-Layer.md](05-Infrastructure-Layer.md) |
| 06 | API Layer | [06-API-Layer.md](06-API-Layer.md) |
| 07 | Dependency Rule and Project References | [07-Dependency-Rule-and-Project-References.md](07-Dependency-Rule-and-Project-References.md) |
| 08 | Guided Implementation | [08-Guided-Implementation.md](08-Guided-Implementation.md) |

---

## Practical Implementation

This lesson builds a complete Employee Management API using Clean Architecture. The solution contains four projects:

| Project | Layer | Responsibility |
|---|---|---|
| EmployeeManagement.Domain | Innermost | Business entities (Employee, Department) |
| EmployeeManagement.Application | Middle | Use cases, interfaces, DTOs |
| EmployeeManagement.Infrastructure | Outer | EF Core, repositories, DI registration |
| EmployeeManagement.Api | Outermost | Controllers, Program.cs, HTTP handling |

**Expected result**: a working API with 4 projects following Clean Architecture principles. Each project has a single responsibility. Dependencies point inward. The solution builds with zero warnings and zero errors.

The complete source code is in the `Code/` folder.

---

## The Big Picture

This lesson connects what you built in Week 2 to a more structured architecture:

```text
Week 2 Day 1-3: Single project, controller does everything
                    |
                    v
Week 2 Day 4-5:   Queryable, validated API (still single project)
                    |
                    v
Week 3 Day 1:     Clean Architecture (separated concerns)
                    |
                    v
Week 3 Day 2:     Security (builds on Clean Architecture)
```

In Week 2, you built a single-project API where the controller handled HTTP, business logic, and database access. That worked, but it created problems: untestable business rules, tight coupling to EF Core, and a controller that grew without limit.

Today, you split those concerns into four projects with strict dependency rules. The controller handles HTTP only. The service handles business logic. The repository handles persistence. The domain defines the business concepts.

Tomorrow, you add authentication and authorization on top of this architecture. Clean Architecture makes that easier because security concerns go in their own layer without touching business logic.

---

## Final Checklist

By the end of this session, you should be able to:

- [ ] Draw the four-layer Clean Architecture diagram from memory
- [ ] Explain why Domain has zero project references
- [ ] Explain why Application defines `IEmployeeRepository` but Infrastructure implements it
- [ ] Explain the difference between compile-time references and runtime flow
- [ ] Describe what the composition root (Program.cs) does and why it is the only place where all layers meet
- [ ] Build the EmployeeManagement solution from scratch using the guided implementation
- [ ] Identify a violation of the Dependency Rule in a codebase
- [ ] Explain why swapping SQL Server for PostgreSQL only requires changes in Infrastructure

---

## Next: Day 2

Day 2 covers Security. You will add authentication (JWT tokens) and authorization (role-based access) to the Clean Architecture solution you built today. The layered structure you build today makes security concerns easy to add without modifying business logic.
