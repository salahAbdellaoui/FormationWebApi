# Instructor Guide — Capstone Exercise

> **This document is for instructors only.** Do not share with trainees. It contains evaluation criteria, expected answers, and progressive hints.

---

## Weighted Evaluation Rubric

Total weight: **100%**. Score each criterion on the three-level scale below, multiply by the weight, and sum.

| # | Criterion | Weight |
|---|---|---|
| 1 | Architecture and SOLID | 12% |
| 2 | CRUD Functionality | 13% |
| 3 | Authentication and Authorization | 13% |
| 4 | Validation and Error Handling | 10% |
| 5 | Testing | 25% |
| 6 | Logging, Configuration, and CORS | 8% |
| 7 | Code Quality and Refactoring | 10% |
| 8 | Design Reasoning | 9% |
| | **Total** | **100%** |

---

### Criterion 1 — Architecture and SOLID (12%)

| Level | Description |
|---|---|
| **Poor (0–4%)** | Project references violate the dependency rule (e.g., Domain references Infrastructure). Service depends on concrete classes. Layers are missing or collapsed. |
| **Acceptable (5–8%)** | Four projects with correct references. Dependencies flow inward. `EmployeeService` depends on `IEmployeeRepository` interface. Minor issues like an unnecessary interface or a leaked EF Core type in a DTO. |
| **Strong (9–12%)** | Clean dependency flow. SOLID principles applied thoughtfully. Written analysis identifies real trade-offs, not just restates definitions. No layer violations. Interfaces justified by testability or flexibility. |

**What to look for:**
- `dotnet build` succeeds, confirming references compile.
- Check each `.csproj`: Domain has no project references, Application references only Domain, Infrastructure references Application and Domain, API references all three.
- `EmployeeService` constructor should accept interfaces defined in Application, not concrete Infrastructure types.

---

### Criterion 2 — CRUD Functionality (13%)

| Level | Description |
|---|---|
| **Poor (0–4%)** | Missing endpoints (Update or Delete not implemented). Wrong HTTP methods or status codes. Controllers contain business logic. |
| **Acceptable (5–8%)** | All five endpoints work: GET all, GET by id, POST, PUT, DELETE. Correct status codes (200, 201, 204, 404). Controller delegates to service. Service throws exceptions for invalid states. |
| **Strong (9–13%)** | Clean endpoint contracts. DTOs contain only what the client needs. `DepartmentName` included in Create response via re-query. Async throughout. Consistent error handling across all endpoints. |

**What to look for:**
- Test each endpoint manually with curl, Swagger, or Postman.
- Verify PUT returns 200 with updated data and 404 for missing IDs.
- Verify DELETE returns 204 (no body) and 404 for missing IDs.
- Verify POST returns 201 with `Location` header pointing to the new resource.

---

### Criterion 3 — Authentication and Authorization (13%)

| Level | Description |
|---|---|
| **Poor (0–4%)** | No authentication configured. Endpoints accessible without tokens. Signing key in `appsettings.json`. |
| **Acceptable (5–8%)** | JWT validation works. Unauthenticated requests return 401. HR and Admin roles enforced. Delete restricted to Admin. Signing key in User Secrets or environment variable. |
| **Strong (9–13%)** | Authorization policies used instead of repeated role strings. Correct 401 vs 403 distinction. Token contains appropriate claims. Login endpoint validates credentials properly. |

**What to look for:**
- Send a request without `Authorization` header → expect 401.
- Send a request with a valid HR token to DELETE → expect 403.
- Send a request with a valid Admin token to DELETE → expect 204.
- Check that the JWT signing key is not in any committed file (`git log -p` or search for the key value).
- Verify `CanManageEmployees` and `AdminOnly` policies are registered in `Program.cs`.

---

### Criterion 4 — Validation and Error Handling (10%)

| Level | Description |
|---|---|
| **Poor (0–3%)** | No validation. Exceptions leak stack traces or database errors to the client. |
| **Acceptable (4–6%)** | FluentValidation validators for Create and Update DTOs. Global exception handler returns Problem Details. No internal details in error responses. |
| **Strong (7–10%)** | All validation rules enforced (Name length, Email format, Salary positive, DepartmentId > 0). Multiple validation errors returned in one response. Exception handler logs full details server-side while returning safe client response. |

**What to look for:**
- POST with empty Name → expect 400 with "Name is required" error.
- POST with Salary = -100 → expect 400 with salary validation error.
- POST with invalid Email format → expect 400.
- Trigger a server error (e.g., invalid DepartmentId that passes validation but fails at DB) → expect 500 with Problem Details body, no stack trace.

---

### Criterion 5 — Testing (25%)

This is the heaviest-weighted criterion because Day 5 focuses on testing.

| Level | Description |
|---|---|
| **Poor (0–8%)** | No test projects, or tests do not compile, or tests fail. Unit tests depend on a database. |
| **Acceptable (9–16%)** | Unit test project and integration test project exist. Unit tests for `EmployeeService` use mocking. Integration tests use `WebApplicationFactory`. All tests pass. Minimum required tests present. |
| **Strong (17–25%)** | Tests go beyond the minimum. Edge cases covered (duplicate email, concurrent updates, empty result sets). Test names clearly describe the scenario. AAA pattern followed consistently. Integration tests verify auth behavior (401 without token, 403 with wrong role). |

**What to look for:**
- Run `dotnet test` in the solution. All tests must pass.
- Check unit test project references: should reference Application project and a mocking library (Moq or NSubstitute). Should NOT reference Infrastructure.
- Check integration test project references: should reference API project and `Microsoft.AspNetCore.Mvc.Testing`.
- Verify unit tests mock `IEmployeeRepository`, not `AppDbContext`.
- Verify integration tests test HTTP behavior (status codes, response body shape), not just "the method returns something."
- Look for AAA structure in each test.

---

### Criterion 6 — Logging, Configuration, and CORS (8%)

| Level | Description |
|---|---|
| **Poor (0–2%)** | No logging. No CORS. Secrets in committed files. |
| **Acceptable (3–5%)** | `ILogger<T>` injected and used with structured logging. CORS policy configured for a specific origin. JWT key in User Secrets. |
| **Strong (6–8%)** | Correct log levels (Information for success, Warning for not-found, Error for exceptions). Named placeholders used (not string interpolation). Middleware pipeline in correct order. |

**What to look for:**
- Grep for string interpolation in log calls: `_logger.LogInformation($"...")` is wrong. `_logger.LogInformation("...{Name}", name)` is correct.
- Check middleware order in `Program.cs`: Exception handler → CORS → HTTPS → Auth → AuthZ → MapControllers.
- Check `appsettings.json` does not contain the JWT signing key.
- Verify CORS policy allows specific origin, not `*`.

---

### Criterion 7 — Code Quality and Refactoring (10%)

| Level | Description |
|---|---|
| **Poor (0–3%)** | Long methods, code duplication, poor names, magic strings. No evidence of refactoring. |
| **Acceptable (4–6%)** | Clean names. Methods under 20 lines. No obvious duplication. Build passes with zero warnings. |
| **Strong (7–10%)** | Evidence of deliberate refactoring (refactoring log). Guard clauses instead of nested ifs. Common logic extracted. Tests used as safety net during refactoring. |

**What to look for:**
- Check the refactoring log (Phase 8 deliverable). Does it describe specific smells and fixes?
- Run `dotnet build` — should have zero warnings.
- Scan for long methods, deep nesting, and copy-paste between Create and Update logic.

---

### Criterion 8 — Design Reasoning (9%)

| Level | Description |
|---|---|
| **Poor (0–3%)** | No written analysis, or answers are superficial restatements of the questions. |
| **Acceptable (4–6%)** | Phase 1 answers are thoughtful. Phase 4 SOLID analysis identifies real examples. At least one trade-off discussed. |
| **Strong (7–9%)** | Answers show genuine understanding of why decisions were made. Trade-offs discuss alternatives that were considered and rejected. Review questions answered with specific examples from the codebase. |

**What to look for:**
- Does the Phase 1 document answer all seven questions?
- Does the Phase 4 analysis name specific classes and methods, not just abstract principles?
- Is there at least one trade-off where the trainee chose simplicity over strict principle adherence, with a clear justification?

---

## Progressive Hints

Use these when trainees are stuck. Reveal one hint at a time.

### Phase 2 — Repository Extension

1. **Hint 1:** Think about what operations are missing from `IEmployeeRepository`. You need to update and delete. What method signatures would those need?
2. **Hint 2:** For update, consider two approaches: fetch the entity, modify its properties, then save. Or attach a modified entity and save. Which approach works better with EF Core's change tracking?
3. **Hint 3:** The `DepartmentExistsAsync` method should use `_context.Departments.AnyAsync(d => d.Id == departmentId)`.

### Phase 3 — API Design

1. **Hint 1:** Follow the existing pattern. Look at how `Create` is implemented across controller → service → repository. Mirror that structure for Update and Delete.
2. **Hint 2:** The Update endpoint needs the ID in the route and the updated data in the body. The route should be `PUT /api/employees/{id}`.
3. **Hint 3:** For the re-query after Create, use `GetByIdWithDepartmentAsync` after `SaveChangesAsync` to get the employee with the Department navigation property loaded.

### Phase 5 — Authentication

1. **Hint 1:** Review Topic 02 (JWT Authentication) for the token generation code. You need `JwtSecurityTokenHandler`, `SecurityTokenDescriptor`, and the signing key from configuration.
2. **Hint 2:** The login endpoint should accept `{ "username": "...", "password": "..." }`, check against the in-memory store, and return `{ "token": "..." }`.
3. **Hint 3:** Register policies in `AddAuthorization`: `options.AddPolicy("CanManageEmployees", policy => policy.RequireRole("Admin", "HR"))`.

### Phase 7 — Testing

1. **Hint 1:** The unit test project needs a reference to `EmployeeManagement.Application` and a mocking library. Create it with `dotnet new xunit`.
2. **Hint 2:** To mock `IEmployeeRepository`, create a mock object, set up `GetByIdWithDepartmentAsync` to return a test employee, then create `EmployeeService` with that mock.
3. **Hint 3:** For integration tests, `WebApplicationFactory<Program>` needs `Program` to be accessible. Add `public partial class Program { }` at the bottom of `Program.cs`.

---

## Common Mistakes

| Mistake | Why It Happens | How to Guide |
|---|---|---|
| Putting validation in the controller | Familiar from earlier exercises | "If a background job creates an employee, does the controller validation still run?" |
| Returning `BadRequest` from the service | Confusing HTTP concerns with business logic | "The service should not know about HTTP status codes. What should it do instead?" |
| Committing the JWT signing key | Convenience | "Search your repo for the key value. If `git grep` finds it, it is committed." |
| Unit tests that use a real database | Not understanding test isolation | "What happens when the database is empty? When it has stale data? Can you run tests in parallel?" |
| String interpolation in log calls | Habit | "Can a log aggregation tool search for EmployeeId=42 in your log message? Not with interpolation." |
| Missing `AsNoTracking` on read queries | Forgotten optimization | "Does the DbContext need to track an entity you are only reading and converting to a DTO?" |
| Putting `SaveChangesAsync` in the repository | Day 1 pattern | "If the service needs to call two repository methods in one transaction, where should Save go?" |
| Creating unnecessary interfaces | Over-applying DIP | "Does this interface have more than one implementation, or could it? If not, is the abstraction justified by testability?" |

---

## Expected Timeline

| Phase | Estimated Time | Cumulative |
|---|---|---|
| Phase 1 — Analyze | 20–30 min | 30 min |
| Phase 2 — Data Access | 25–35 min | ~1 hr |
| Phase 3 — API Design | 35–45 min | ~1:40 |
| Phase 4 — SOLID Analysis | 20–25 min | ~2:05 |
| Phase 5 — Auth | 30–40 min | ~2:40 |
| Phase 6 — Validation/Logging/CORS | 30–40 min | ~3:15 |
| Phase 7 — Testing | 40–50 min | ~4:05 |
| Phase 8 — Refactoring | 15–20 min | ~4:25 |

Trainees who finish in 3 hours are working efficiently. Those who take 4+ hours are either being thorough or are stuck. Check in at the 2-hour mark (after Phase 4).

---

## Grading Sheet

Use this table to record scores. Multiply the level percentage by the criterion weight.

| # | Criterion | Weight | Level % | Score |
|---|---|---|---|---|
| 1 | Architecture and SOLID | 12% | | |
| 2 | CRUD Functionality | 13% | | |
| 3 | Authentication and Authorization | 13% | | |
| 4 | Validation and Error Handling | 10% | | |
| 5 | Testing | 25% | | |
| 6 | Logging, Configuration, and CORS | 8% | | |
| 7 | Code Quality and Refactoring | 10% | | |
| 8 | Design Reasoning | 9% | | |
| | **Total** | **100%** | | **/100** |

**Level % guidance:**
- Poor: 0–33% of the criterion's weight
- Acceptable: 34–66% of the criterion's weight
- Strong: 67–100% of the criterion's weight

**Example:** If a trainee scores "Acceptable" on Testing (25% weight), their score for that criterion is 25% × 50% = 12.5%.

---

## Pass/Fail Thresholds

| Result | Total Score |
|---|---|
| **Pass with distinction** | 85–100% |
| **Pass** | 60–84% |
| **Resubmit required** | 40–59% |
| **Fail** | Below 40% |

A trainee must score at least "Acceptable" in Testing (criterion 5) to pass, regardless of total score. Testing is the core focus of Day 5.
