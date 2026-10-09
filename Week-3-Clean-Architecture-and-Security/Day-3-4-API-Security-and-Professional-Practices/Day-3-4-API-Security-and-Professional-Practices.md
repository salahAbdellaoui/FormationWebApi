# Days 3-4 — API Security and Professional Practices

**Week 3 — Clean Architecture and Security**

**Duration**: Self-paced (comprehensive module)

**Level**: Intermediate (builds on Days 1-2)

**Prerequisites**: Days 1-2 (Clean Architecture, Application Architecture)

---

## Purpose

This combined module covers two essential areas that transform a working API into a production-ready one: **security** and **professional practices**.

Days 1 and 2 gave you a well-structured application with Clean Architecture, clear layer separation, and solid design patterns. But the API you built has no authentication, no authorization, no input validation, and no error handling strategy. Anyone can access any endpoint. Invalid data can reach the database. Exceptions crash the application or leak internal details.

This module fixes that. You will learn how to secure an API, handle errors gracefully, validate input, log events, manage configuration safely, and apply professional practices that separate production code from tutorial code.

---

## Learning Objectives

By the end of this module, you will be able to:

1. Distinguish between authentication (who are you?) and authorization (what can you do?)
2. Implement JWT-based authentication in an ASP.NET Core Web API
3. Apply role-based authorization to protect API endpoints
4. Create policy-based authorization for complex access rules
5. Configure and use CORS for cross-origin requests
6. Build a global exception handling strategy that protects sensitive information
7. Apply input validation using Data Annotations and FluentValidation
8. Implement structured logging with Serilog or built-in logging
9. Manage configuration and secrets securely (User Secrets, Key Vault)
10. Apply security headers and HTTPS enforcement
11. Understand rate limiting, CORS, and defense-in-depth principles
12. Recognize common security anti-patterns and avoid them

---

## Prerequisites

- **Day 1 completed**: You built a four-layer Clean Architecture solution (Domain, Application, Infrastructure, API)
- **Day 2 completed**: You understand Dependency Inversion, Repository Pattern, Unit of Work, Services, DTOs, and CQRS concepts
- **Week 1-2 completed**: You can build a basic ASP.NET Core Web API with EF Core

---

## Learning Sequence

```text
Day 1: Clean Architecture (four-layer structure)
         |
         v
Day 2: Application Architecture (patterns and trade-offs)
         |
         v
Days 3-4: Security & Professional Practices
         |
         v
Future: Advanced topics, real-world deployment
```

Each topic builds on the previous one. Authentication must exist before authorization. Validation must exist before you can trust input. Logging must exist before you can diagnose failures. The order is deliberate.

---

## Topics

| # | Topic | Focus |
|---|---|---|
| 01 | [Authentication vs Authorization](01-Authentication-vs-Authorization.md) | Core concepts, 401 vs 403 |
| 02 | [JWT Authentication](02-JWT-Authentication.md) | Token structure, implementation, validation |
| 03 | [Roles and Claims](03-Roles-and-Claims.md) | Roles, claims, endpoint protection |
| 04 | [Authorization Policies](04-Authorization-Policies.md) | Custom policies, requirements, handlers |
| 05 | [Secure API Development](05-Secure-API-Development.md) | CORS, HTTPS, security headers |
| 06 | [Global Exception Handling](06-Global-Exception-Handling.md) | Middleware, safe error responses |
| 07 | [FluentValidation](07-FluentValidation.md) | Input validation, validator classes |
| 08 | [Logging](08-Logging.md) | Structured logging, log levels, correlation |
| 09 | [Configuration and Secrets](09-Configuration-and-Secrets.md) | appsettings, User Secrets, Key Vault |
| 10 | [CORS](10-CORS.md) | Cross-origin requests, preflight, policies |
| 11 | [Security Best Practices](11-Security-Best-Practices.md) | Comprehensive security checklist |

---

## Part A — Authentication and Authorization

Topics 01-05 cover the security fundamentals. You start with the conceptual difference between authentication and authorization (Topic 01), then implement JWT tokens (Topic 02), add role-based access control (Topic 03), create custom policies for complex rules (Topic 04), and configure CORS and security headers (Topic 05).

By the end of Part A, your API requires a valid identity before any protected endpoint executes, and unauthorized requests receive the correct HTTP status code.

---

## Part B — Professional API Practices

Topics 06-11 cover the practices that separate production-ready code from tutorial code. You build a global exception handler that never leaks stack traces (Topic 06), validate input before it reaches business logic (Topic 07), implement structured logging for diagnostics (Topic 08), manage secrets without committing them to source control (Topic 09), apply secure API design patterns (Topic 10), and understand common attack vectors (Topic 11).

By the end of Part B, your API handles errors gracefully, rejects invalid input, logs meaningful diagnostics, protects secrets, and resists common attacks.

---

## Expected Outcome

After completing this module, you will understand:

- **Why** authentication and authorization are separate concerns that must both be enforced server-side
- **How** JWT tokens work: structure, signing, validation, expiry, and refresh
- **What** role-based and policy-based authorization provide and when to use each
- **How** to build a defense-in-depth strategy: validation, authentication, authorization, error handling, logging, and secure configuration working together
- **Why** security is not a feature you add once, but a property you maintain continuously

You will not build a perfectly secure API. Perfect security does not exist. You will build a **significantly more secure** API than where you started, and you will understand the principles that let you keep improving it.

---

## Final Review Checklist

After completing all 11 topics, you should be able to:

- [ ] Explain the difference between authentication and authorization with concrete examples
- [ ] Describe the structure of a JWT token and why the signature matters
- [ ] Implement JWT authentication in Program.cs with proper validation parameters
- [ ] Protect endpoints with `[Authorize]` and specify required roles or policies
- [ ] Write a custom authorization policy with a requirement and handler
- [ ] Configure CORS to allow specific origins without using `AllowAny`
- [ ] Build a global exception handler that returns safe error responses
- [ ] Validate input using both Data Annotations and FluentValidation
- [ ] Implement structured logging with appropriate log levels
- [ ] Manage secrets using User Secrets (dev) or Key Vault (production) — never in source control

---

## Important Notes

1. **This is learning content, not exercises.** This module contains explanations, code examples, and diagrams. You read it, understand it, and apply it to the codebase. There are no separate exercise files in this phase.

2. **Security is layered.** No single mechanism protects your API. Authentication, authorization, validation, error handling, logging, and configuration all work together. Removing one layer weakens the others.

3. **Teaching code vs production code.** Some examples in this module are simplified for clarity. Security warnings are included where the teaching code differs from production requirements. Always read the warnings.

4. **The codebase is your laboratory.** Apply what you learn to the EmployeeManagement solution. Read the existing code, understand what is missing, then add security features using the patterns in these topics.

5. **Ask "what could go wrong?"** Every topic includes a "Common Mistakes" section. Read it. Most security incidents come from known mistakes, not novel attacks.
