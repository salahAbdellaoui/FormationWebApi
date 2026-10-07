# Day 4-5 Exercises — Advanced Web API

These exercises continue the completed Day 3 `EmployeeManagement.Api`. Make each change in the existing project; do not create a second API.

## Exercise 1 — Filtering

Add `departmentId` and `minSalary` to the employee listing query.

Verify:

```http
GET /api/employees?departmentId=2
GET /api/employees?minSalary=3000
GET /api/employees?departmentId=2&minSalary=3000
```

**Common mistake:** executing the query before applying both filters.

## Exercise 2 — Search

Add a `search` query parameter that searches employee `Name` or `Email`:

```http
GET /api/employees?search=john
```

Trim the value and ignore an empty search. Do not introduce a search engine or full-text-search feature.

## Exercise 3 — Sorting

Support only the explicit fields `name`, `email`, and `salary`:

```http
GET /api/employees?sortBy=name
GET /api/employees?sortBy=salary&sortOrder=desc
```

Use a switch or equivalent explicit mapping. Do not let a client choose arbitrary property names.

## Exercise 4 — Pagination

Add `pageNumber` and `pageSize`, validate them, and return:

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3
}
```

Verify:

```http
GET /api/employees?pageNumber=2&pageSize=10
```

## Exercise 5 — Validation

Add data annotations to the create/update DTOs and query model:

- name is required;
- email is required and must be an email address;
- salary must be in a sensible range;
- page number must be greater than zero; and
- page size must be between 1 and 100.

Verify that invalid requests return `400` without entering the action.

## Exercise 6 — Error handling

Verify that:

- a missing employee returns `404`;
- invalid input returns `400`; and
- an unexpected exception is handled centrally as Problem Details without a stack trace in the response.

Use the existing application pipeline. Do not add a throw-only production endpoint.

## Exercise 7 — Final API challenge

Combine all Day 4 features in the order:

```text
Filter → Search → Sort → Count → Skip/Take → Project → Execute
```

Implement and test:

```http
GET /api/employees?search=ali&departmentId=2&minSalary=3000&sortBy=salary&sortOrder=desc&pageNumber=1&pageSize=10
```

Before considering the challenge complete, run `dotnet restore` and `dotnet build`, then test valid and invalid requests through the existing Swagger UI.
