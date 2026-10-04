# 01 — HTTP and REST Principles

---

## 🎯 Learning Objectives

By the end of this topic, you will be able to:

- Describe the client / server relationship
- Read an HTTP request and an HTTP response line by line
- Use GET, POST, PUT, PATCH, and DELETE for the right purpose
- Read status codes and choose the correct one for an API
- Explain what REST means in practice
- Design resource-oriented URLs instead of action-oriented URLs
- Design the endpoints of the Employee Management API before writing any code

---

## The Problem

A mobile app must show the list of employees. The data is on a server in a data center.

```text
Phone (no data)                         Server (has the data)
"Give me the employees"   ←————————→            ?
```

Two different machines, two different programs, two different memories. They need a **shared language**.

That language is **HTTP**. The style we use to design URLs and methods on top of HTTP is called **REST**.

---

## Client and Server

| Role | What it does | Example |
|------|--------------|---------|
| **Client** | Sends a request, waits for an answer | Browser, mobile app, another backend, Postman, `curl` |
| **Server** | Listens, processes, returns a response | Your ASP.NET Core Web API |

```text
Client                         Server
  │                              │
  │──── HTTP Request ───────────▶│
  │                              │  route → controller → service
  │◀── HTTP Response ────────────│
  │                              │
```

The same program can be **both**: your API calls another company's API as a client.

> 💡 **Senior Developer Note:** When you debug, always ask: *which side am I on?* The server produces the `404`, but it is usually the client that asked for the wrong URL.

---

## What Is HTTP?

**HTTP (HyperText Transfer Protocol)** is the request/response protocol of the web.

Rules that matter to an API developer:

- A client sends a **request**; the server returns a **response**. Exactly one response per request.
- Each request carries a **method** (what to do) and a **URL** (which resource).
- Messages are **text** — you can read them with `curl`, Postman, or browser DevTools.
- HTTP is **stateless**: the server does not remember your previous request. Every request must carry everything needed to understand it. Cookies and sessions are built *on top* of HTTP for applications that need memory.
- Default ports: `80` for HTTP, `443` for HTTPS.

---

## Anatomy of an HTTP Request

A real request to our future API:

```http
GET /api/employees HTTP/1.1
Host: localhost:5189
Accept: application/json
```

| Line | Meaning |
|------|---------|
| `GET` | The **method** — what the client wants to do |
| `/api/employees` | The **path** — which resource it wants |
| `HTTP/1.1` | The protocol version |
| `Host` | Which server (host + port) |
| `Accept` | Which format the client can read back |

Now with a body (a client sending a new employee):

```http
POST /api/employees HTTP/1.1
Host: localhost:5189
Content-Type: application/json

{
  "name": "Karim Haddad",
  "email": "karim@company.com",
  "department": "HR"
}
```

> ⚠️ The port `5189` comes from **my** generated project. Yours will be different. Always read it from the console line `Now listening on: ...` or from `Properties/launchSettings.json`.

---

## Anatomy of an HTTP Response

This is the **actual response** our API returns when it works (verified output):

```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Server: Kestrel

[
  { "id": 1, "name": "Amira Benali", "email": "amira@company.com", "department": "Engineering" },
  { "id": 2, "name": "Karim Haddad",  "email": "karim@company.com",  "department": "HR" }
]
```

| Part | Meaning |
|------|---------|
| `HTTP/1.1 200 OK` | **Status line** — protocol + status code + reason phrase |
| `Content-Type` | Format of the response body (here: JSON) |
| `Server: Kestrel` | The web server built into ASP.NET Core |
| The JSON | The **response body** — the actual data |

Every response has three parts: **status line, headers, body**.

---

## The URL

```text
https://localhost:7156/api/employees/2?pretty=true
```

| Part | Value in the example | Purpose |
|------|----------------------|---------|
| Scheme | `https` | Which protocol (HTTP or HTTPS) |
| Host | `localhost` | Which machine |
| Port | `7156` | Which door on that machine |
| Path | `/api/employees/2` | Which resource |
| Query | `pretty=true` | Extra options (filtering, paging …) |

Today we only design **paths**. Query strings return later with filtering and pagination — not a Day 1 topic.

---

## HTTP Methods

The URL says **which resource**. The method says **what to do with it**.

| Method | Meaning | Idempotent* | Employee API example |
|--------|---------|-------------|----------------------|
| `GET` | Read a resource (never changes it) | Yes | `GET /api/employees` |
| `POST` | Create a new resource | No | `POST /api/employees` |
| `PUT` | Replace a resource completely | Yes | `PUT /api/employees/2` |
| `PATCH` | Update part of a resource | Not necessarily | `PATCH /api/employees/2` |
| `DELETE` | Remove a resource | Yes | `DELETE /api/employees/2` |

\* **Idempotent** = doing it twice has the same effect as doing it once. Calling `DELETE /api/employees/2` twice still leaves the employee deleted.

**Today we build only the two `GET` endpoints.** `POST`, `PUT`, `PATCH`, and `DELETE` arrive on Day 2 — that is the CRUD lesson.

> ⚠️ When our API is running with only `GET` actions (Topic 04), calling `POST /api/employees` returns **405 Method Not Allowed**: the path exists, but no action accepts `POST`. That is not a bug — it is the API telling the truth.

---

## Headers

Headers are **metadata**: information *about* the message, not the message itself.

| Direction | Header | Meaning |
|-----------|--------|---------|
| Request | `Accept: application/json` | "Send me JSON" |
| Request | `Content-Type: application/json` | "The body I am sending is JSON" |
| Response | `Content-Type: application/json; charset=utf-8` | "This body is JSON, UTF-8" |
| Response | `Allow: GET` | Sent with a `405`: this path only accepts GET |
| Response | `Location: /api/employees/4` | Sent with a `201`: the new resource is over there |

`Allow` is a verified example. When we call `POST` on a path that only serves `GET`, the response is:

```http
HTTP/1.1 405 Method Not Allowed
Allow: GET
```

---

## Status Codes

The status code is the **one-line verdict** of the server. Clients — and developers — make decisions from it.

### 2xx — It worked

| Code | Name | When |
|------|------|------|
| `200` | OK | Request succeeded, body attached |
| `201` | Created | A new resource was created (Day 2) |
| `204` | No Content | Succeeded, nothing to return (for example after a delete) |

### 3xx — Go somewhere else

| Code | Name | When |
|------|------|------|
| `307` | Temporary Redirect | The template's HTTPS redirection answers with it |

### 4xx — You (the client) made a mistake

| Code | Name | When |
|------|------|------|
| `400` | Bad Request | Malformed request, missing body, failed validation |
| `401` | Unauthorized | Not authenticated (later lessons) |
| `403` | Forbidden | Authenticated, but not allowed (later lessons) |
| `404` | Not Found | The resource does not exist — or no route matched |
| `405` | Method Not Allowed | The path exists, but not for this HTTP method |
| `409` | Conflict | The request conflicts with the current state |

### 5xx — We (the server) have a problem

| Code | Name | When |
|------|------|------|
| `500` | Internal Server Error | An unhandled exception in your code |
| `503` | Service Unavailable | The service is temporarily down or overloaded |

### Two different 404s (verified)

This distinction saves hours of debugging:

```text
404 + empty body        → no route matched at all. The path does not exist.
404 + JSON body         → the route matched, your action ran, and it
                          answered NotFound(). The path exists;
                          the specific resource does not.
```

In our project:

- `GET /api/nope` → `404`, **empty body** (no endpoint for that path)
- `GET /api/employees/999` → `404` with a JSON body describing the error (the action ran and returned `NotFound()`)

> 💡 **Senior Developer Note:** `404` with an empty body means *your code was never reached*. Always check the URL first, then the code.

---

## Request Body and JSON

**JSON** is the standard representation format for web APIs. Our `Employee` looks like this on the wire:

```json
{
  "id": 2,
  "name": "Karim Haddad",
  "email": "karim@company.com",
  "department": "HR"
}
```

Verified behaviour of our .NET 8 API: C# property names are written in **camelCase** in the JSON (`Name` → `name`), and the response `Content-Type` is `application/json`.

A **representation** is one representation of a resource at one moment. The same employee could be XML, CSV, or JSON — REST does not care. The `Accept` header says which one the client can handle.

---

## What Is REST?

**REST (Representational State Transfer)** is a set of practical guidelines for designing web APIs. It is guidance, not a law book — but following it gives you an API that other developers can understand without reading your code first.

| Principle | What it means in practice |
|-----------|---------------------------|
| **Resources** | Everything is a *thing*: employees, departments, positions |
| **Resource-oriented URLs** | The URL names the thing, not the action: `/api/employees` |
| **HTTP methods carry the action** | Reading, creating, replacing, deleting are the methods |
| **Stateless** | Each request is complete by itself; the server keeps no memory of earlier requests |
| **Representations** | The resource is sent as JSON (or another format) |
| **Meaningful status codes** | The verdict is in the status code, not hidden in a `200` with `{"error": "..."}` |

### Resource URLs vs action URLs

| Action-oriented (avoid) | Resource-oriented (prefer) | Why |
|-------------------------|----------------------------|-----|
| `GET /api/getEmployees` | `GET /api/employees` | The method already says "get" — repeating it in the URL is noise |
| `GET /api/employee/2` | `GET /api/employees/2` | A collection holds many → plural |
| `POST /api/addEmployee` | `POST /api/employees` | Creating into the collection |
| `GET /api/deleteEmployee?id=2` | `DELETE /api/employees/2` | Deleting is a method, not a URL word |
| `GET /api/employees/getById/2` | `GET /api/employees/2` | The last segment *is* the identifier |

The rule is simple:

```text
URL    →  WHAT the resource is
METHOD →  WHAT to do with it
```

If your URL contains a verb (`get`, `add`, `remove`, `update`), the design is probably wrong.

> 💡 **Senior Developer Note:** REST is not a certification. If a design decision breaks a REST guideline but makes the API much simpler for your clients, that can be a good trade-off. The goal is an API that is **predictable**, not an API that is theoretically pure.

---

## Practical Scenario — Design Before You Code

Before opening the IDE, decide the contract of the **Employee Management API**.

### Endpoints we build today

| Method | URL | Purpose | Success | Not found |
|--------|-----|---------|---------|-----------|
| `GET` | `/api/employees` | List all employees | `200` + JSON array | — |
| `GET` | `/api/employees/{id}` | One employee | `200` + JSON object | `404` |

### Endpoints designed now, built on Day 2

| Method | URL | Purpose |
|--------|-----|---------|
| `POST` | `/api/employees` | Create an employee |
| `PUT` | `/api/employees/{id}` | Replace an employee |
| `PATCH` | `/api/employees/{id}` | Update parts of an employee |
| `DELETE` | `/api/employees/{id}` | Remove an employee |

### Related resources (same rules, later)

```text
/api/departments
/api/departments/{id}
/api/positions
/api/positions/{id}
```

Notice: **nouns, plural, lowercase, no verbs.** The HTTP method supplies the verb.

---

## 🧪 Exercise 1 — Method and Status Card

For each situation, write (a) the HTTP method, (b) the URL, (c) the expected status code.

1. The mobile app loads the employee list.
2. The mobile app opens employee `7`.
3. The app asks for employee `999`, which does not exist.
4. The app calls `POST /api/employees` on today's API, where only `GET` actions exist.
5. A developer calls `/api/employeez`.

<details>
<summary><b>Answers</b></summary>

1. `GET /api/employees` → `200`
2. `GET /api/employees/7` → `200`
3. `GET /api/employees/999` → `404`
4. `POST /api/employees` → `405` (with `Allow: GET`)
5. `GET /api/employeez` → `404` with an **empty body** (no such route)

</details>

---

## 🧪 Exercise 2 — Fix the URLs

Rewrite each action-oriented URL as a resource-oriented one.

| # | Action-oriented URL | Your resource-oriented URL |
|---|---------------------|----------------------------|
| 1 | `GET /api/getEmployees` | |
| 2 | `GET /api/findEmployee/5` | |
| 3 | `POST /api/createDepartment` | |
| 4 | `GET /api/departments/getAll` | |
| 5 | `DELETE /api/employees/remove/5` | |

<details>
<summary><b>Answers</b></summary>

1. `GET /api/employees`
2. `GET /api/employees/5`
3. `POST /api/departments`
4. `GET /api/departments`
5. `DELETE /api/employees/5`

</details>

---

## 🧪 Exercise 3 — Read the Response

Three responses were captured from a real .NET 8 API. For each one, say what happened and who is wrong — client or server.

**Response A**

```http
HTTP/1.1 405 Method Not Allowed
Allow: GET
```

**Response B**

```http
HTTP/1.1 404 Not Found
Content-Length: 0
```

**Response C**

```http
HTTP/1.1 400 Bad Request
Content-Type: application/json; charset=utf-8

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "traceId": "00-c90dfb691b3f12e3199937610ae0e88c-150876f195e2dca0-00"
}
```

*(the `traceId` value is different for every request — it identifies one request in the server logs)*

<details>
<summary><b>Answers</b></summary>

- **A:** The path exists but was called with the wrong method (here `POST` or `PUT` instead of `GET`). The **client** must change the method — `Allow` tells it which one is accepted.
- **B:** No route matched this path at all — your controller was never reached. The **client** sent a wrong URL, *or* the server forgot to map the route (you will meet this bug in Topic 04).
- **C:** The request itself was malformed (for example a missing or broken JSON body). The **client** must fix the request.

</details>

---

## ⚠️ Common Mistakes

| # | Mistake | Why it hurts | Better |
|---|---------|--------------|--------|
| 1 | Putting verbs in URLs (`/api/getEmployees`) | Clients must guess; the design fights the protocol | URL = noun, method = verb |
| 2 | Returning `200 OK` for everything | Clients cannot distinguish success, missing, and broken | Use `404`, `400`, `500` honestly |
| 3 | Returning `404` when the route simply does not exist *in your code* | You hide your own routing bug behind a "not found" | Fix the route; a real 404 means the resource is missing |
| 4 | Using `GET` to change data | Proxies and browsers may repeat the request | Only `POST`/`PUT`/`PATCH`/`DELETE` change data |
| 5 | Confusing `404` with `405` | Wasted hours debugging the wrong layer | `404` = path unknown, `405` = path known, method rejected |
| 6 | Designing URLs in the middle of coding | Route changes break clients once the API is live | Write the endpoint table first (as we just did) |

> 💡 **Senior Developer Note:** A route is part of an **API contract**. Once mobile apps and frontends ship with your URLs, changing them breaks real users. Design them on paper, agree on them, then implement.

---

## 🧠 Knowledge Check

### Question 1

A client sends `GET /api/employees/3`. Which part tells the server *which* resource, and which part tells it *what to do*?

**Answer:** The path `/api/employees/3` identifies the resource. The method `GET` says "read it".

---

### Question 2

True or False: the server remembers what you asked for in your previous request.

**Answer:** False. HTTP is stateless — each request stands alone.

---

### Question 3

Your colleague calls `PATCH /api/employees/2` on today's API and gets `405`. Is the URL wrong?

**Answer:** No. The URL is right — today's API only implements `GET` actions, so the method is rejected with `405`. The `Allow` header shows which methods the path accepts.

---

### Question 4

Which status code should an API return when the request body is missing for a creation call?

**Answer:** `400 Bad Request`.

---

### Question 5

Why is `GET /api/getEmployees` worse than `GET /api/employees`?

**Answer:** The verb belongs to the HTTP method, not the URL. Action words in URLs lead to inconsistent designs (`/api/addEmployee`, `/api/removeEmployee`, …) and make the API harder to predict.

---

## ✅ Check Yourself

- [ ] I can explain the request / response cycle of HTTP
- [ ] I can read an HTTP request and an HTTP response line by line
- [ ] I know the five HTTP methods and when to use each one
- [ ] I can tell a `404` from a `405` — and explain the two kinds of `404`
- [ ] I can rewrite an action URL as a resource URL
- [ ] I designed the endpoints of the Employee API before coding them

---

## Summary

| Concept | What to remember |
|---------|------------------|
| Client / Server | Client asks, server answers |
| HTTP | One request → one response, text-based, stateless |
| Request | Method + URL + headers + optional body |
| Response | Status line + headers + body |
| Methods | `GET` read, `POST` create, `PUT` replace, `PATCH` update, `DELETE` remove |
| Status codes | `2xx` worked, `4xx` client's fault, `5xx` server's fault |
| `404` vs `405` | Path unknown vs method rejected |
| REST | Resources + resource URLs + methods + statelessness + JSON |
| Rule of thumb | URL = noun, HTTP method = verb |

---

**Next: [02 — ASP.NET Core Fundamentals](02-ASP.NET-Core-Fundamentals.md)**
