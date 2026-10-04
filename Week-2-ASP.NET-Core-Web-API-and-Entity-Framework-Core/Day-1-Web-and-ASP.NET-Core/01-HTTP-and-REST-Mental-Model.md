# 01 — HTTP and REST Mental Model

---

## 🎯 What You Will Learn (20 min)

In 20 minutes you will know enough HTTP and REST to understand every API request your ASP.NET Core application receives.

By the end you can explain:

- What an HTTP request carries
- What an HTTP response returns
- Why `/api/employees` is better than `/api/getEmployees`

---

## The Two Sides of a Web API

```text
Client  ────────── HTTP Request ─────────▶  Server
Client  ◀────────── HTTP Response ─────────  Server
```

That is the whole conversation. The client asks, the server answers.

---

## Anatomy of a Request

```http
GET /api/employees HTTP/1.1
Host: localhost:5000
Accept: application/json
```

| Part | Job |
|------|-----|
| **Method** (`GET`) | What the client wants to do |
| **URL** (`/api/employees`) | Which resource it wants |
| **Headers** (`Accept: ...`) | Extra metadata (format, auth, caching hints) |
| **Body** | Optional payload (used with POST, PUT, PATCH) |

> 💡 **Senior Developer Note:** A `GET` request should have **no body**. The URL + method already tell the server everything it needs to read a resource.

---

## Anatomy of a Response

```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

[
  { "id": 1, "name": "Amira", "department": "Engineering" }
]
```

| Part | Job |
|------|-----|
| **Status code** (`200`) | The verdict: success, error, or redirect |
| **Headers** (`Content-Type`) | Metadata about the response |
| **Body** | The actual payload (the resource, or an error description) |

### Status Codes You Will Use on Day 1

| Code | Meaning | When |
|------|---------|------|
| `200` | OK | A `GET` or `PUT` succeeded |
| `201` | Created | A resource was created (Day 2) |
| `204` | No Content | The operation succeeded, nothing to return (Day 2) |
| `400` | Bad Request | The request is malformed or invalid |
| `404` | Not Found | The resource does not exist |
| `500` | Internal Server Error | Your code threw an exception |

That is enough for today. The rest comes in Day 2.

---

## REST — A Naming Convention for URLs

REST is not a technology. It is a convention that says:

> **The URL names the resource. The HTTP method says what to do with it.**

### Example: Employees

| Operation | Method | URL | Why |
|-----------|--------|-----|-----|
| List all employees | `GET` | `/api/employees` | The URL names the collection |
| Get employee 5 | `GET` | `/api/employees/5` | The last segment identifies the item |
| *(Day 2)* Create | `POST` | `/api/employees` | The body carries the new data |
| *(Day 2)* Delete 5 | `DELETE` | `/api/employees/5` | The method says "delete" |

### Why not `/api/getEmployees`?

Because the URL already says *what* you want, and the HTTP method says *how* to act. Repeating the verb in the URL is noise:

```text
/api/getEmployees   ← noise: "get" is already in the method
/api/employees      ← clean: the method says GET, the URL says employees
```

---

## 🧪 Think

1. You want to read employee number 10. What request do you send?
2. Your colleague sends `POST /api/employees/10`. What would that probably mean?
3. You get back `404`. What does that actually mean?

<details>
<summary><b>Answers</b></summary>

1. `GET /api/employees/10`
2. `POST` to a specific id is unusual. Normally `POST /api/employees` creates a *new* resource. This might be a bug or an unusual API design.
3. The resource was not found at that URL. Either it does not exist, or the URL is wrong.

</details>

---

## 📌 One-Liner to Remember

```text
Method + URL  →  Server decides which code runs  →  Status + Body
```

Hold onto that. Every topic today builds this one line.

---

## ✅ Check Yourself

- [ ] I can name the four parts of an HTTP request
- [ ] I can name the three parts of an HTTP response
- [ ] I know the status codes 200, 400, 404, 500
- [ ] I can explain why `/api/employees` is better than `/api/getEmployees`

---

**Next: [02 — ASP.NET Core Fundamentals](02-ASP.NET-Core-Fundamentals.md)**
