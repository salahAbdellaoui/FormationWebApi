# Day 1 — Web & ASP.NET Core

## Professional Training Course

**Duration:** 4 hours  
**Level:** Beginner — Professional Foundation  
**Prerequisites:** Basic C# knowledge, familiarity with Visual Studio or VS Code  
**Framework:** .NET 8

---

## 🎯 Day 1 Learning Objectives

By the end of this session, you will understand:

- How the Web works at a fundamental level
- What HTTP is and how requests and responses flow
- REST principles and how to design professional APIs
- ASP.NET Core architecture and project structure
- Controllers, Routing, and how they work together
- Dependency Injection — why it exists and how to use it correctly

---

# Part 1 — Web & HTTP Fundamentals

## 1.1 What Is the Web?


The Web is a system of interconnected documents and resources accessed via the Internet. At its core, it is built on a simple model:

```text
Client requests a resource → Server provides it
```

This seems simple, but there is a lot happening between those two steps. Understanding this process is the foundation of everything you will learn today.

### The Request/Response Flow

When a user opens `https://example.com/api/products`, the following happens:

```text
Client (Browser / Mobile App / Postman)
   ↓
1. DNS Lookup — resolve "example.com" to an IP address
   ↓
2. TCP Connection — establish a connection to the server
   ↓
3. TLS Handshake — secure the connection (HTTPS)
   ↓
4. HTTP Request — send a structured request to the server
   ↓
5. Web Server / ASP.NET Core receives the request
   ↓
6. Application processes the request (business logic, database queries)
   ↓
7. HTTP Response — server sends back a structured response
   ↓
8. Client receives and renders the response
```

You do not need to memorize every networking detail. What you **do** need is this mental model: **the Web is a conversation between a client and a server, and HTTP is the language they speak.**

---

## 1.2 What Is HTTP?

**HTTP** stands for **HyperText Transfer Protocol**. It is the foundation of data communication on the Web.

> 🟦 **Concept:** A **protocol** is simply a set of rules that both sides agree to follow. HTTP defines how clients and servers communicate.

### Why HTTP Exists

Without a standard protocol, every application would invent its own way to exchange data. HTTP provides a **universal standard** so that any client can talk to any server.

### How HTTP Works

Every HTTP exchange consists of two parts:

1. **Request** — sent by the client
2. **Response** — sent by the server

### Anatomy of an HTTP Request

Let us look at what happens when you request a product from an API:

```http
GET /api/products/10 HTTP/1.1
Host: localhost:5000
Accept: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

Breaking this down:

| Part | Value | Meaning |
|------|-------|---------|
| **Method** | `GET` | What action the client wants to perform |
| **Path** | `/api/products/10` | Which resource is being requested |
| **Version** | `HTTP/1.1` | Which version of HTTP is being used |
| **Host Header** | `localhost:5000` | Which server to send the request to |
| **Accept** | `application/json` | What format the client expects in the response |
| **Authorization** | `Bearer <token>` | Authentication credentials |

### Anatomy of an HTTP Response

```http
HTTP/1.1 200 OK
Content-Type: application/json
Content-Length: 64

{
  "id": 10,
  "name": "Laptop",
  "price": 1200,
  "isActive": true
}
```

| Part | Value | Meaning |
|------|-------|---------|
| **Version** | `HTTP/1.1` | HTTP version used |
| **Status Code** | `200` | Result of the request |
| **Reason Phrase** | `OK` | Human-readable status |
| **Content-Type** | `application/json` | Format of the response body |
| **Body** | `{ ... }` | The actual data |

---

## 1.3 URLs Explained

A URL (Uniform Resource Locator) identifies a resource on the Web.

```text
https://example.com:443/api/products?page=1&pageSize=20
│      │            │    │              │
scheme  host       port  path        query string
```

| Component | Example | Purpose |
|-----------|---------|---------|
| **Scheme** | `https` | Protocol to use (http or https) |
| **Host** | `example.com` | Domain name of the server |
| **Port** | `443` | Network port (443 for HTTPS, 80 for HTTP) |
| **Path** | `/api/products` | The specific resource on the server |
| **Query String** | `?page=1&pageSize=20` | Additional parameters for the request |

---

## 1.4 HTTP Methods

HTTP defines several methods (also called verbs). Each method tells the server what the client wants to do with a resource.

| Method | Purpose | Idempotent | Example |
|--------|---------|------------|---------|
| **GET** | Read / Retrieve data | Yes | `GET /api/products` |
| **POST** | Create a new resource | No | `POST /api/products` |
| **PUT** | Replace an entire resource | Yes | `PUT /api/products/10` |
| **PATCH** | Partially update a resource | No* | `PATCH /api/products/10` |
| **DELETE** | Delete a resource | Yes | `DELETE /api/products/10` |

> 🟦 **Concept — Idempotency:** An operation is **idempotent** if calling it multiple times produces the same result as calling it once. GET, PUT, and DELETE are idempotent. POST is not — calling POST twice may create two resources.

### Why This Matters

Consider a product API using a single resource path:

```text
GET    /api/products       → Get all products
GET    /api/products/10    → Get product with ID 10
POST   /api/products       → Create a new product
PUT    /api/products/10    → Replace product 10
PATCH  /api/products/10    → Update parts of product 10
DELETE /api/products/10    → Delete product 10
```

The URL identifies **the resource**. The HTTP method identifies **the action**. This is the foundation of REST.

### PUT vs PATCH — The Key Difference

```text
PUT /api/products/10
{
  "name": "Laptop Pro",
  "description": "Updated description",
  "price": 1500,
  "isActive": true
}
```

With PUT, you send the **entire** resource. Any field not included is typically reset to its default.

```text
PATCH /api/products/10
{
  "price": 1500
}
```

With PATCH, you send **only the fields you want to change**. The rest remains unchanged.

### Why GET Should Never Modify Data

GET requests are idempotent and safe. Browsers may pre-fetch pages, proxies may cache GET responses, and users may bookmark URLs. If a GET request modifies data, you risk unintended side effects from actions you do not control.

> 🟥 **Warning:** Never implement a GET endpoint that deletes, creates, or modifies data. This violates HTTP semantics and creates unpredictable behavior.

---

## 1.5 HTTP Status Codes

Status codes tell the client what happened on the server. They are grouped by their first digit:

| Range | Category | Meaning |
|-------|----------|---------|
| 1xx | Informational | Request received, processing continues |
| 2xx | Success | Request was successfully received and processed |
| 3xx | Redirection | Further action is needed |
| 4xx | Client Error | The request contains bad syntax or cannot be fulfilled |
| 5xx | Server Error | The server failed to fulfill a valid request |

### The Most Important Status Codes

| Code | Name | When to Use | Example |
|------|------|-------------|---------|
| **200** | OK | Request succeeded | GET a product successfully |
| **201** | Created | Resource was created | POST a new product |
| **204** | No Content | Success, but no body to return | DELETE a product |
| **400** | Bad Request | Client sent invalid data | Missing required field |
| **401** | Unauthorized | Client is not authenticated | Missing or invalid token |
| **403** | Forbidden | Client is authenticated but not allowed | User cannot access this resource |
| **404** | Not Found | Resource does not exist | Product with ID 999 not found |
| **409** | Conflict | Request conflicts with current state | Duplicate email registration |
| **500** | Internal Server Error | Something went wrong on the server | Unhandled exception |

### 401 vs 403 — The Critical Distinction

> 🟦 **Concept:** Think of a building with a receptionist at the front door and locked offices inside.

- **401 (Unauthorized)** = "Who are you? Show me your ID." The server does not know who you are.
- **403 (Forbidden)** = "I know who you are, but you cannot enter this office." You are authenticated, but you lack permission.

```text
401 = "I don't know you."
403 = "I know you, but you can't do this."
```

### Common Mistake: Always Returning 200

Some developers return HTTP 200 for every response, even errors:

```json
{
  "status": 200,
  "error": true,
  "message": "Product not found"
}
```

**Why this is a problem:**

- HTTP clients (browsers, mobile apps, load balancers) rely on status codes to make decisions
- Monitoring tools cannot detect errors
- Retries and error handling logic break
- The API misrepresents its actual behavior

> 💡 **Senior Tip:** Always return the correct HTTP status code. Tools like Swagger, Postman, and browser DevTools are designed to work with them.

---

## 1.6 Headers

Headers carry metadata about the request or response. They are key-value pairs.

### Common Request Headers

| Header | Purpose | Example |
|--------|---------|---------|
| `Content-Type` | Format of the request body | `application/json` |
| `Accept` | Desired format of the response | `application/json` |
| `Authorization` | Authentication credentials | `Bearer eyJhbGciOi...` |
| `User-Agent` | Identifies the client | `Mozilla/5.0` |

### Common Response Headers

| Header | Purpose | Example |
|--------|---------|---------|
| `Content-Type` | Format of the response body | `application/json` |
| `Cache-Control` | Caching instructions | `no-cache` |
| `Location` | Redirect URL or created resource | `/api/products/11` |

---


# Part 2 — REST Principles

## 2.1 What Is REST?

**REST** stands for **Representational State Transfer**. It is an **architectural style** — a set of guidelines for designing networked applications.

REST was defined by Roy Fielding in his doctoral dissertation in 2000. It is not a protocol or a standard. It is a set of principles that, when followed, create predictable, scalable, and maintainable web services.

> 🟦 **Concept:** REST is NOT just "URLs + JSON." It is a set of architectural constraints that govern how clients and servers communicate.

### The Six Constraints of REST

| Constraint | Meaning |
|------------|---------|
| **Client-Server** | Client and server are separate; they communicate over a well-defined interface |
| **Stateless** | Each request contains all information the server needs; the server does not remember previous requests |
| **Cacheable** | Responses must indicate whether they can be cached |
| **Uniform Interface** | Resources are identified by URIs; manipulation happens through representations |
| **Layered System** | Intermediaries (load balancers, proxies) can exist between client and server |
| **Code on Demand** (optional) | Server can temporarily extend client functionality |

The most important ones for you to understand today are **statelessness** and the **uniform interface**.

---

## 2.2 Resources and Resource-Oriented URLs

In REST, everything is a **resource**. A resource is any entity that can be named and addressed:

- A product
- A user
- An order
- A list of products

Each resource is identified by a **unique URI** (Uniform Resource Identifier).

### Design Principle: Nouns, Not Verbs

```text
❌ BAD:  GET /getAllProducts
✅ GOOD: GET /api/products

❌ BAD:  POST /createProduct
✅ GOOD: POST /api/products

❌ BAD:  GET /deleteProduct/10
✅ GOOD: DELETE /api/products/10
```

**Why?**

- The HTTP method already communicates the **action** (GET = read, POST = create, DELETE = remove)
- URLs should identify **what** resource you are talking to
- Adding verbs to URLs creates redundancy and confusion

### Consistent URL Structure

```text
GET    /api/products          → Get all products
GET    /api/products/10       → Get product 10
POST   /api/products          → Create a new product
PUT    /api/products/10       → Replace product 10
PATCH  /api/products/10       → Update product 10 partially
DELETE /api/products/10       → Delete product 10
```

The URL identifies the resource. The method identifies the action. This is clean, predictable, and self-documenting.

---

## 2.3 Statelessness

Every HTTP request must contain **all the information** the server needs to process it. The server does not store client state between requests.

```text
❌ BAD (Stateful):
   Client: "Hey, I'm user 5. Remember me."
   Server: "OK, I'll remember you're user 5."
   Client: "Now give me my orders."
   Server: "Sure, you're user 5, here are your orders."

✅ GOOD (Stateless):
   Client: "Here's my token. Give me user 5's orders."
   Server: "Token valid. Here are user 5's orders."
   Client: "Here's my token. Update user 5's email."
   Server: "Token valid. Email updated."
```

**Why statelessness matters:**

- Any server can handle any request (horizontal scaling)
- No session data to synchronize across servers
- Simpler debugging and logging
- Better fault tolerance

---

## 2.4 Representations

When a client requests a resource, the server returns a **representation** of that resource — not the resource itself.

```text
Client: "Give me product 10 as JSON"
Server: { "id": 10, "name": "Laptop", "price": 1200 }

Client: "Give me product 10 as XML"
Server: <product><id>10</id><name>Laptop</name><price>1200</price></product>
```

The same resource can have different representations based on what the client requests via the `Accept` header.

> 💡 **Senior Tip:** In practice, most modern APIs use JSON exclusively. Content negotiation via the Accept header is available but rarely used for public APIs.

---

## 2.5 Bad vs Good REST API Design

### Example: Product Management API

**Bad Design:**

```text
GET    /getAllProducts
GET    /getProductById?id=10
POST   /createNewProduct
POST   /updateProduct
GET    /deleteProduct?id=10
```

Problems:
- Verbs in URLs (HTTP methods already express intent)
- Inconsistent URL patterns
- GET for deletion (dangerous)
- No clear resource model

**Good Design:**

```text
GET    /api/products
GET    /api/products/10
POST   /api/products
PUT    /api/products/10
PATCH  /api/products/10
DELETE /api/products/10
```

Benefits:
- Clean, predictable URL structure
- HTTP methods express the intent
- Easy to understand and document
- Consistent with industry standards

---

## 2.6 Real-World REST Problems

### Problem 1: Using GET for Destructive Operations

```text
GET /api/deleteProduct/10
```

**Why this is bad:**
- GET should be safe (no side effects)
- Browsers may prefetch links, triggering unintended deletions
- Proxies may cache GET requests
- Monitoring tools log GET requests differently

**Solution:** Use `DELETE /api/products/10`

### Problem 2: Always Returning HTTP 200

An API returns `200 OK` for every response, including errors:

```json
// Even for a not-found error:
{
  "status": 200,
  "success": false,
  "message": "Product not found"
}
```

**Consequences:**
- HTTP clients cannot distinguish success from failure
- Monitoring dashboards show 100% success rate
- Error handling logic in clients must parse the body instead of using status codes
- Load balancers and CDNs cannot route errors properly

**Solution:** Return appropriate status codes: 404 for not found, 400 for bad requests, 500 for server errors.

### Problem 3: Exposing Database Details in URLs

```text
❌ BAD:
GET /api/getProductsFromCategoryTable
POST /api/insertIntoProductsTable
```

**Why this is a problem:**
- Tightly couples the API to the database schema
- Database restructuring breaks the API
- Exposes internal implementation details to clients

**Solution:** Use resource-oriented URLs that represent business concepts, not database tables.

### Problem 4: Returning Too Much Data

```text
GET /api/products
```

Returns 10,000 products in a single response. This:
- Consumes excessive bandwidth
- Overwhelms client memory
- Slows down response time

**Solution:** Introduce pagination:

```text
GET /api/products?page=1&pageSize=20
```

```json
{
  "data": [ ... 20 products ... ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 10000,
  "totalPages": 500
}
```

> 💡 **Senior Tip:** Pagination is not optional for production APIs. Always paginate collections.

---

>  *"Can you think of a situation where returning HTTP 200 for everything would actually cause a real problem in production?"*

---

# Part 3 — ASP.NET Core Fundamentals

## 3.1 The .NET Ecosystem

Let us clarify three terms that are often confused:

```text
┌──────────────────────────────────────────────┐
│                    .NET                       │
│  The runtime, libraries, and language (C#)   │
│  Used for: Console apps, desktop, web, etc.  │
├──────────────────────────────────────────────┤
│              ASP.NET Core                    │
│  The web framework built on top of .NET      │
│  Used for: Web apps, APIs, real-time apps    │
├──────────────────────────────────────────────┤
│           ASP.NET Core Web API              │
│  A specific project type for building REST   │
│  APIs using ASP.NET Core                     │
└──────────────────────────────────────────────┘
```

| Technology | Purpose | Example Use |
|------------|---------|-------------|
| **.NET** | General-purpose platform | Console app, library |
| **ASP.NET Core** | Web framework | Web app, API, SignalR |
| **ASP.NET Core Web API** | REST API project template | Product API, User API |

---

## 3.2 Why ASP.NET Core?

| Feature | Benefit |
|---------|---------|
| **Cross-platform** | Runs on Windows, macOS, Linux |
| **High performance** | One of the fastest web frameworks available |
| **Open source** | Community-driven, transparent development |
| **Built-in DI** | Dependency injection is a first-class citizen |
| **Unified** | One framework for web apps, APIs, and real-time |
| **Modern** | Built for cloud and containerized environments |

---

## 3.3 Creating Your First Web API

```bash
dotnet new webapi -n ProductApi
```

This creates a new Web API project. Let us examine the important files:

```text
ProductApi/
├── Controllers/
│   └── WeatherForecastController.cs
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── ProductApi.csproj
```

| File | Purpose |
|------|---------|
| `Program.cs` | Application entry point and configuration |
| `appsettings.json` | Configuration values (connection strings, etc.) |
| `ProductApi.csproj` | Project file (dependencies, target framework) |
| `Controllers/` | API endpoints live here |
| `Properties/launchSettings.json` | Development server settings |

> 💡 **Senior Tip:** You do not need to understand every generated file on Day 1. Focus on `Program.cs`, `Controllers/`, and `appsettings.json`. The rest will become clear with experience.

---

## 3.4 Program.cs — The Heart of the Application

In .NET 8, the minimal hosting model uses a simplified `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
```

### What Happens at Startup

```text
Application Starts
       ↓
1. Create Builder (configuration, services)
       ↓
2. Register Services (AddControllers, etc.)
       ↓
3. Build the Application
       ↓
4. Configure Middleware Pipeline
       ↓
5. Start Listening for Requests
```

Two critical lines:

```csharp
var builder = WebApplication.CreateBuilder(args);  // Build up services
var app = builder.Build();                         // Create the app
```

**`builder`** is used to register services (what the app needs).  
**`app`** is used to configure the pipeline (how requests are processed).

> 🟦 **Concept:** Think of `builder` as the "preparation phase" and `app` as the "execution phase."

---

## 3.5 Middleware

**Middleware** is software that sits between the incoming request and the final response. Each middleware component can:

- Process the request
- Pass it to the next middleware
- Short-circuit the pipeline (not call the next middleware)
- Process the response on the way back

### Analogy: Security Checkpoints

```text
Visitor
  ↓
Security Guard (Authentication) — "Show me your ID"
  ↓
Receptionist (Authorization) — "Are you on the list?"
  ↓
Office (Controller) — "How can I help you?"
  ↓
Receptionist (Response formatting) — "Here's your document"
  ↓
Security Guard (Logging) — "Visitor logged out"
  ↓
Visitor leaves
```

### The Default ASP.NET Core Pipeline

```text
Request
  ↓
UseExceptionHandler (error handling)
  ↓
UseHsts (HTTPS enforcement)
  ↓
UseHttpsRedirection (HTTP → HTTPS)
  ↓
UseStaticFiles (serve CSS, JS, images)
  ↓
UseRouting (match URL to endpoint)
  ↓
UseAuthentication (who are you?)
  ↓
UseAuthorization (are you allowed?)
  ↓
MapControllers (execute the controller)
  ↓
Response
```

### Order Matters

Middleware executes in the order it is registered. If you put `UseAuthorization` before `UseAuthentication`, authorization will fail because the user has not been authenticated yet.

```csharp
// CORRECT order
app.UseAuthentication();  // First: identify the user
app.UseAuthorization();   // Then: check permissions
```

### Custom Middleware Example

A simple request timing middleware:

```csharp
app.Use(async (context, next) =>
{
    var start = DateTime.UtcNow;
    await next();
    var duration = DateTime.UtcNow - start;
    Console.WriteLine($"Request took {duration.TotalMilliseconds}ms");
});
```

> 💡 **Senior Tip:** ASP.NET Core already has built-in request logging middleware. This is just an example to show how middleware works. Always check the framework first.

---

 *"Why do you think the order of middleware matters?"*


 Because each middleware can depend on the work performed by the middleware before it. The order determines what information is available and what actions can be performed at each stage of the request

---

# Part 4 — Project Structure, Controllers & Routing

## 4.1 Project Structure

A typical ASP.NET Core Web API project:

```text
ProductApi/
├── Controllers/
│   ├── ProductsController.cs
│   └── UsersController.cs
├── Models/
│   └── Product.cs
├── Services/
│   └── ProductService.cs
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── ProductApi.csproj
```

| Folder | Purpose |
|--------|---------|
| **Controllers/** | API endpoints that handle HTTP requests |
| **Models/** | Data structures (entities, DTOs) |
| **Services/** | Business logic layer |
| **Properties/** | Development configuration |

> 🟨 **Important:** This is only the **starting structure**. As applications grow, you will evolve toward more organized structures. On Day 1, keep it simple.

### Where Professional Applications Are Heading

As applications grow, the structure may evolve to:

```text
ProductApi/
├── API/              → Controllers, middleware, startup
├── Application/      → Business logic, services, DTOs
├── Domain/           → Entities, value objects, rules
├── Infrastructure/   → Database, external services, email
```

This is called **Clean Architecture** or **Onion Architecture**. You will learn more about this in future sessions. On Day 1, understanding the basic structure is enough.

---

## 4.2 Controllers

### What Is a Controller?

A controller is a class that handles HTTP requests. It receives a request, performs some action, and returns a response.

> 🟦 **Concept:** Think of a controller as a **receptionist** for your API. It receives requests, routes them to the right person, and returns the result.

### Why Controllers Exist

- They provide a clear entry point for HTTP requests
- They separate HTTP concerns (routing, status codes, content negotiation) from business logic
- They make the API structure predictable

### Anatomy of a Controller

```csharp
[ApiController]                              // Enables API-specific behaviors
[Route("api/[controller]")]                  // Base route: "api/products"
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]                                 // GET /api/products
    public IActionResult GetAll()
    {
        var products = _service.GetAll();
        return Ok(products);
    }

    [HttpGet("{id}")]                         // GET /api/products/10
    public IActionResult GetById(int id)
    {
        var product = _service.GetById(id);
        if (product == null)
            return NotFound();
        return Ok(product);
    }

    [HttpPost]                                // POST /api/products
    public IActionResult Create([FromBody] Product product)
    {
        var created = _service.Create(product);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]                         // PUT /api/products/10
    public IActionResult Update(int id, [FromBody] Product product)
    {
        var updated = _service.Update(id, product);
        if (updated == null)
            return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]                      // DELETE /api/products/10
    public IActionResult Delete(int id)
    {
        var deleted = _service.Delete(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
```

### Line-by-Line Explanation

| Line | Purpose |
|------|---------|
| `[ApiController]` | Enables automatic model validation, binding source inference, and problem details |
| `[Route("api/[controller]")]` | Maps requests starting with `api/products` to this controller. `[controller]` is replaced by the class name minus "Controller" |
| `: ControllerBase` | Base class providing helper methods (Ok, NotFound, BadRequest, etc.) without view support |
| `[HttpGet]` | Maps HTTP GET requests to this action method |
| `[HttpGet("{id}")]` | Maps `GET /api/products/{id}` — `{id}` is a route parameter |
| `[HttpPost]` | Maps HTTP POST requests to this action method |
| `[FromBody]` | Binds the request body to the parameter (deserializes JSON) |
| `CreatedAtAction` | Returns 201 with a Location header pointing to the created resource |

---

## 4.3 Routing

Routing determines how HTTP requests are mapped to controller actions.

### Attribute Routing

The most common approach in modern ASP.NET Core:

```csharp
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]                    // → GET /api/products
    public IActionResult GetAll() { ... }

    [HttpGet("{id}")]            // → GET /api/products/10
    public IActionResult GetById(int id) { ... }

    [HttpPost]                   // → POST /api/products
    public IActionResult Create(Product product) { ... }

    [HttpPut("{id}")]            // → PUT /api/products/10
    public IActionResult Update(int id, Product product) { ... }

    [HttpDelete("{id}")]         // → DELETE /api/products/10
    public IActionResult Delete(int id) { ... }
}
```

### Route Parameters vs Query Parameters

**Route parameters** are part of the URL path:

```text
GET /api/products/10
         ↑
     id = 10
```

```csharp
[HttpGet("{id}")]
public IActionResult GetById(int id) { ... }
```

**Query parameters** come after the `?`:

```text
GET /api/products?page=1&pageSize=20
         ↑
     page = 1, pageSize = 20
```

```csharp
[HttpGet]
public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20) { ... }
```

| Type | URL | When to Use |
|------|-----|-------------|
| Route Parameter | `/api/products/10` | Identifying a specific resource |
| Query Parameter | `/api/products?page=1` | Filtering, sorting, pagination |

---

## 4.4 Controller Design — Thin Controllers

> 🟥 **Warning:** Controllers should be **thin**. They should handle HTTP concerns and nothing more.

### Fat Controller (BAD)

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        // ❌ Business logic in controller
        if (id <= 0)
            return BadRequest("Invalid ID");

        // ❌ Database access in controller
        using var connection = new SqlConnection("...");
        var product = connection.Query<Product>(
            "SELECT * FROM Products WHERE Id = @Id", new { Id = id });

        // ❌ Validation logic in controller
        if (product == null)
            return NotFound();

        if (!product.IsActive)
            return BadRequest("Product is inactive");

        // ❌ Mapping logic in controller
        var response = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        };

        return Ok(response);
    }
}
```

**Problems:**
- Difficult to test (cannot test business logic without HTTP)
- Code duplication (other controllers may need the same logic)
- Strong coupling to database
- Hard to maintain as the application grows

### Thin Controller (GOOD)

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }
}
```

The controller's only job is to:
1. Receive the HTTP request
2. Call the appropriate service
3. Return the appropriate HTTP response

All business logic lives in the service layer.

### The Separation of Concerns

```text
BAD:
Controller
 ├── Business Rules
 ├── Database Queries
 ├── Validation
 ├── Calculations
 └── Logging

GOOD:
Controller
    ↓ (HTTP concerns only)
Service / Application
    ↓ (Business logic)
Repository / Data Access
    ↓ (Data access)
Database
```

---

## 4.5 Practical Walkthrough: Building a Products Controller

Let us build a complete controller step by step.

### Step 1: Create the Model

```csharp
namespace ProductApi.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### Step 2: Create the Service Interface

```csharp
namespace ProductApi.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task<Product?> UpdateAsync(int id, Product product);
    Task<bool> DeleteAsync(int id);
}
```

### Step 3: Create the Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using ProductApi.Models;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _service.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _service.GetByIdAsync(id);
        if (product == null)
            return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        var created = await _service.CreateAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
        var updated = await _service.UpdateAsync(id, product);
        if (updated == null)
            return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
```

Notice:
- The controller has **one responsibility**: handle HTTP
- All logic is delegated to `IProductService`
- Status codes are correct (201 for creation, 204 for deletion, 404 for not found)
- The code is **async** for proper I/O handling

---

>**  — *"Look at the controller above. What would happen if we needed to change the database? Would we need to change the controller?"*

---

# Part 5 — Dependency Injection

## 5.1 The Problem

Consider this code:

```csharp
public class ProductsController : ControllerBase
{
    private readonly ProductService _service;

    public ProductsController()
    {
        _service = new ProductService();  // ❌ Creating dependency directly
    }
}
```

**Problems with `new`:**

| Problem | Explanation |
|---------|-------------|
| **Tight coupling** | The controller is permanently tied to `ProductService` |
| **Cannot test** | You cannot replace `ProductService` with a mock in tests |
| **Cannot change** | Switching to a different implementation requires changing the controller |
| **Creates its own dependencies** | `ProductService` may have its own dependencies that need to be configured |

```text
ProductsController
       ↓ (directly creates)
ProductService
       ↓ (directly creates)
DatabaseContext
       ↓ (directly creates)
DatabaseConnection
```

Every class creates its own dependencies. This creates a **chain of hard-coded dependencies** that is impossible to test and difficult to change.

---

## 5.2 The Solution: Dependency Injection

**Dependency Injection (DI)** is a technique where a class receives its dependencies from the outside, rather than creating them itself.

```csharp
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    // ✅ Dependency is "injected" through the constructor
    public ProductsController(IProductService service)
    {
        _service = service;
    }
}
```

### The Difference

```text
BAD:  Controller creates its own dependencies (tight coupling)
      Controller → new ProductService()

GOOD: Controller receives dependencies (loose coupling)
      Controller → IProductService ← ProductService
```

The controller depends on an **abstraction** (`IProductService`), not a **concrete implementation** (`ProductService`). The DI container is responsible for providing the correct implementation.

---

## 5.3 How It Works in ASP.NET Core

ASP.NET Core has a **built-in DI container**. You register services during application startup:

```csharp
// Program.cs

var builder = WebApplication.CreateBuilder(args);

// Register the service with the DI container
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();
```

### What This Registration Means

```csharp
builder.Services.AddScoped<IProductService, ProductService>();
//                         ↑               ↑
//                    Interface      Concrete Implementation
```

When a controller asks for `IProductService`, the DI container provides a `ProductService` instance.

### The Full Flow

```text
1. Controller constructor requests IProductService
         ↓
2. DI container looks up IProductService in its registry
         ↓
3. DI container creates a new ProductService (or reuses existing)
         ↓
4. DI container passes ProductService to the controller
         ↓
5. Controller uses IProductService without knowing the concrete type
```

---

## 5.4 DI Lifetimes

When registering a service, you choose a **lifetime**:

| Lifetime | Method | Instance Behavior | Typical Use |
|----------|--------|-------------------|-------------|
| **Singleton** | `AddSingleton<T>()` | One instance for the entire application | Configuration, shared stateless services |
| **Scoped** | `AddScoped<T>()` | One instance per HTTP request | Business services, DbContext |
| **Transient** | `AddTransient<T>()` | New instance every time it is requested | Lightweight, stateless utilities |

### When to Use Each

```csharp
// Singleton — one instance forever
builder.Services.AddSingleton<IConfiguration>(configuration);

// Scoped — one per request (most common)
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Transient — new instance every time
builder.Services.AddTransient<IEmailService, EmailService>();
```

### Why DbContext Is Scoped

The `DbContext` represents a single database transaction/unit of work. If you registered it as Singleton, all requests would share the same connection and state, leading to data corruption and concurrency issues.

```csharp
// ✅ Correct — one DbContext per request
builder.Services.AddScoped<AppDbContext>();
```

> 💡 **Senior Tip:** When in doubt, use **Scoped**. It is the safest default for most services.

---

## 5.5 DI Registration Examples

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// Register framework services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
```

---

## 5.6 DI Problems and Mistakes

### Mistake 1: Everything as Singleton

```csharp
// ❌ BAD — dangerous
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IDatabaseContext, DatabaseContext>();
```

**Why this is dangerous:**
- Singleton services share state across all requests
- A service that holds request-specific data (like a database context) will have data leaking between requests
- Thread safety issues become common

### Mistake 2: Wrong Lifetime for Request-Specific Data

```csharp
// ❌ BAD — repository holds per-request data
builder.Services.AddSingleton<IOrderRepository, OrderRepository>();
```

If `OrderRepository` stores the current user's orders for a request, making it singleton means all users share the same data.

### Mistake 3: Creating Dependencies with `new` Instead of DI

```csharp
public class OrdersController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly UserService _userService;
    private readonly EmailService _emailService;

    public OrdersController()
    {
        // ❌ Three hard-coded dependencies
        _productService = new ProductService();
        _userService = new UserService();
        _emailService = new EmailService();
    }
}
```

**Why this is a problem:**
- Impossible to unit test (cannot mock dependencies)
- Changing any service requires changing the controller
- Violates the Open/Closed Principle

### The Correct Approach

```csharp
public class OrdersController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    // ✅ Dependencies injected through constructor
    public OrdersController(
        IProductService productService,
        IUserService userService,
        IEmailService emailService)
    {
        _productService = productService;
        _userService = userService;
        _emailService = emailService;
    }
}
```

---

>  *"Why do you think ASP.NET Core uses Dependency Injection as a built-in feature, rather than leaving it to developers to implement?"*

---

# Senior Developer Notes

> 💡 **This section contains practical lessons from real-world experience that are not normally found in beginner tutorials.**

## Lesson 1: Understand Before You Code

> 🧠 **Don't write code before understanding the requirement.**

A developer once spent a week building a notification system. When they finished, they discovered the requirement was for **email notifications only**, not the full real-time notification system they had built. Understanding the problem first saves days of wasted effort.

**Practice:** Before writing any code, write down:
- What is the problem?
- What is the simplest solution?
- What are the constraints?

## Lesson 2: Controllers Are Not Business Logic Containers

> 🧠 **Don't put business logic inside controllers.**

Controllers handle HTTP concerns. Business rules belong in services. When business logic lives in controllers, you cannot reuse it, test it properly, or maintain it as the application grows.

## Lesson 3: Never Blindly Return Database Entities

> 🧠 **Don't return database entities directly from APIs.**

Your database schema will evolve. Your API contract should be stable. Returning entities directly couples your database design to your API consumers.

```csharp
// ❌ BAD — exposes database structure
[HttpGet("{id}")]
public Product GetById(int id)
{
    return _context.Products.Find(id);
}

// ✅ GOOD — returns a controlled representation
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var product = await _service.GetByIdAsync(id);
    return Ok(product);  // Maps to a DTO
}
```

## Lesson 4: Use Correct Status Codes

> 🧠 **Don't use HTTP 200 for every response.**

Status codes are not decoration. They are how HTTP clients, monitoring tools, load balancers, and developers understand what happened. Incorrect status codes hide real problems.

## Lesson 5: Don't Create Abstractions Without a Reason

> 🧠 **Don't create abstractions until you need them.**

An interface with one implementation adds complexity without value. Create abstractions when you have a concrete need: testing, multiple implementations, or decoupling for a specific reason.

```csharp
// ❌ Unnecessary abstraction
public interface ICalculator { int Add(int a, int b); }
public class Calculator : ICalculator { ... }

// ✅ Only abstract when needed
public class TaxCalculator { ... }  // Abstract when you need MockTaxCalculator for tests
```

## Lesson 6: Simple First, Complex Later

> 🧠 **Don't over-engineer small applications.**

A 5-endpoint API does not need Clean Architecture, CQRS, and a domain event system. Start simple. Add complexity when the codebase justifies it. The best architecture is the one you can understand at 3am during an incident.

## Lesson 7: Read Before You Change

> 🧠 **Learn to read existing code before changing it.**

Before modifying a file, understand its context. What does it depend on? What depends on it? What are the side effects of your change? Reading code is a skill — practice it deliberately.

## Lesson 8: Fix Root Causes, Not Symptoms

> 🧠 **Debug the root cause instead of treating symptoms.**

If an API returns null, the symptom is the null. The root cause might be a wrong query, a missing mapping, or a race condition. Fix the cause, not the symptom.

## Lesson 9: Naming Is Quality

> 🧠 **Naming and structure are part of software quality.**

`GetAllActiveUsersForCurrentMonthFilteredByRegion` tells you what the code does. `Process()` does not. Clear naming reduces the need for comments and makes code self-documenting.

---

# Common Beginner Mistakes

## Mistake 1: Confusing Authentication and Authorization

```text
Mistake: Using 401 when the user is authenticated but not allowed
→ Why it happens: Both concepts involve "access"
→ Why it is a problem: 401 means "you're not logged in," 403 means "you don't have permission"
→ Better approach: 401 for unauthenticated, 403 for unauthorized
```

## Mistake 2: Using Incorrect HTTP Methods

```text
Mistake: Using GET to delete a resource
→ Why it happens: Not understanding HTTP method semantics
→ Why it is a problem: GET should be safe; proxies and browsers may call it unexpectedly
→ Better approach: Use DELETE for deletion
```

## Mistake 3: Wrong Status Codes

```text
Mistake: Returning 200 for errors
→ Why it happens: "It still returns data, so 200 is fine"
→ Why it is a problem: Clients and monitoring tools cannot distinguish success from failure
→ Better approach: Use 400, 404, 500 as appropriate
```

## Mistake 4: SQL Queries in Controllers

```text
Mistake: Writing SQL queries directly in controller actions
→ Why it happens: "It's quick and easy"
→ Why it is a problem: Untestable, duplicated, tightly coupled to database
→ Better approach: Use services and repositories
```

## Mistake 5: Creating Dependencies with `new`

```text
Mistake: Using 'new' to create service instances in controllers
→ Why it happens: Familiar OOP pattern
→ Why it is a problem: Impossible to test, tight coupling
→ Better approach: Use Dependency Injection
```

## Mistake 6: Fat Controllers

```text
Mistake: Putting all logic in a single controller
→ Why it happens: "It's just one file, easier to find"
→ Why it is a problem: Unmaintainable, untestable, duplicated
→ Better approach: Thin controllers, services for logic
```

## Mistake 7: Ignoring Async/Await

```text
Mistake: Writing synchronous database calls in a web application
→ Why it happens: "It works, why change it?"
→ Why it is a problem: Threads are blocked, reducing server throughput
→ Better approach: Use async/await for I/O operations
```

## Mistake 8: Hardcoding Configuration

```text
Mistake: Putting connection strings and API keys directly in code
→ Why it happens: "It's faster for now"
→ Why it is a problem: Security risk, requires code changes for environment-specific values
→ Better approach: Use appsettings.json and environment variables
```

## Mistake 9: Unnecessary Abstractions

```text
Mistake: Creating interfaces for every class, even with one implementation
→ Why it happens: "Best practices say to use interfaces"
→ Why it is a problem: Adds complexity without value; defer until needed
→ Better approach: Abstract when you have a reason (testing, multiple implementations)
```

---

# Practical Mini Project: Product Management API

> 🎯 **Objective:** Build a complete Product Management API practicing HTTP, REST, controllers, routing, and dependency injection.

## Requirements

### Entity

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### Endpoints to Implement

| Method | URL | Description | Status Code |
|--------|-----|-------------|-------------|
| GET | `/api/products` | Get all active products | 200 |
| GET | `/api/products/{id}` | Get product by ID | 200 / 404 |
| POST | `/api/products` | Create a new product | 201 |
| PUT | `/api/products/{id}` | Update an existing product | 200 / 404 |
| DELETE | `/api/products/{id}` | Delete a product | 204 / 404 |

### Implementation Steps

**Step 1:** Create the project

```bash
dotnet new webapi -n ProductApi
```

**Step 2:** Create `Models/Product.cs`

```csharp
namespace ProductApi.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}
```

**Step 3:** Create `Services/IProductService.cs`

```csharp
using ProductApi.Models;

namespace ProductApi.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task<Product?> UpdateAsync(int id, Product product);
    Task<bool> DeleteAsync(int id);
}
```

**Step 4:** Create `Services/ProductService.cs`

```csharp
using ProductApi.Models;

namespace ProductApi.Services;

public class ProductService : IProductService
{
    private readonly List<Product> _products = new();
    private int _nextId = 1;

    public Task<IEnumerable<Product>> GetAllAsync()
    {
        var products = _products.Where(p => p.IsActive);
        return Task.FromResult(products.AsEnumerable());
    }

    public Task<Product?> GetByIdAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id && p.IsActive);
        return Task.FromResult(product);
    }

    public Task<Product> CreateAsync(Product product)
    {
        product.Id = _nextId++;
        _products.Add(product);
        return Task.FromResult(product);
    }

    public Task<Product?> UpdateAsync(int id, Product product)
    {
        var existing = _products.FirstOrDefault(p => p.Id == id && p.IsActive);
        if (existing == null) return Task.FromResult<Product?>(null);

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        return Task.FromResult<Product?>(existing);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id && p.IsActive);
        if (product == null) return Task.FromResult(false);

        product.IsActive = false;  // Soft delete
        return Task.FromResult(true);
    }
}
```

> 💡 **Senior Tip:** This service uses an in-memory list. In a real application, you would use a database. The structure remains the same — only the data access changes.

**Step 5:** Create `Controllers/ProductsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using ProductApi.Models;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _service.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _service.GetByIdAsync(id);
        if (product == null)
            return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        var created = await _service.CreateAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
        var updated = await _service.UpdateAsync(id, product);
        if (updated == null)
            return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
```

**Step 6:** Register the service in `Program.cs`

```csharp
using ProductApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Step 7:** Test with `curl` or Postman

```bash
# Get all products
curl https://localhost:5001/api/products

# Create a product
curl -X POST https://localhost:5001/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Laptop","description":"High-performance laptop","price":1200}'

# Get product by ID
curl https://localhost:5001/api/products/1

# Update a product
curl -X PUT https://localhost:5001/api/products/1 \
  -H "Content-Type: application/json" \
  -d '{"name":"Laptop Pro","description":"Updated","price":1500}'

# Delete a product
curl -X DELETE https://localhost:5001/api/products/1
```

---

# Exercises

## Exercise 1: REST Endpoint Design

> 🎯 **Objective:** Design REST endpoints for a library management system.

**Requirements:**
- Manage books (title, author, ISBN, available copies)
- Manage members (name, email, membership date)
- Borrow and return books

**Design the following endpoints:**

| Resource | Method | URL | Purpose |
|----------|--------|-----|---------|
| Books | ? | ? | Get all books |
| Books | ? | ? | Get a specific book |
| Books | ? | ? | Add a new book |
| Members | ? | ? | Register a member |
| Borrowing | ? | ? | Borrow a book |
| Borrowing | ? | ? | Return a book |

### Expected Answer

```text
GET    /api/books              → Get all books
GET    /api/books/{id}         → Get a specific book
POST   /api/books              → Add a new book
POST   /api/members            → Register a member
POST   /api/borrowings         → Borrow a book (creates a borrowing record)
PUT    /api/borrowings/{id}/return → Return a book
```

> 💡 **Senior Tip:** Notice that borrowing and returning are modeled as actions on a borrowing resource, not as actions on the book itself. The book's available copies decrease, but the URL identifies the borrowing, not the book.

---

## Exercise 2: Identify Incorrect HTTP Methods

> 🎯 **Objective:** Find the mistakes in the following API design.

```text
GET    /api/products/delete/10
POST   /api/products/get-all
GET    /api/products/create
DELETE /api/products/10
PUT    /api/products
```

### Expected Answer

| Line | Problem | Fix |
|------|---------|-----|
| `GET /api/products/delete/10` | GET should not delete resources | `DELETE /api/products/10` |
| `POST /api/products/get-all` | GET retrieves data, POST creates | `GET /api/products` |
| `GET /api/products/create` | GET should not create resources | `POST /api/products` |
| `DELETE /api/products/10` | ✅ Correct | — |
| `PUT /api/products` | PUT requires a specific resource | `PUT /api/products/10` |

---

## Exercise 3: HTTP Status Codes

> 🎯 **Objective:** Choose the correct status code for each scenario.

| Scenario | Status Code |
|----------|-------------|
| A user successfully logs in | ? |
| A requested product does not exist | ? |
| A new user account is created | ? |
| A user tries to access a page they don't have permission for | ? |
| The server encounters an unexpected error | ? |
| A user submits a form with missing required fields | ? |
| A product is successfully deleted | ? |
| A user provides an invalid authentication token | ? |

### Expected Answer

| Scenario | Status Code | Name |
|----------|-------------|------|
| A user successfully logs in | 200 | OK |
| A requested product does not exist | 404 | Not Found |
| A new user account is created | 201 | Created |
| A user tries to access a page they don't have permission for | 403 | Forbidden |
| The server encounters an unexpected error | 500 | Internal Server Error |
| A user submits a form with missing required fields | 400 | Bad Request |
| A product is successfully deleted | 204 | No Content |
| A user provides an invalid authentication token | 401 | Unauthorized |

---

## Exercise 4: Controller with Multiple Routes

> 🎯 **Objective:** Create a controller with the following endpoints.

**Design a User controller with:**
- Get all users
- Get a user by ID
- Get a user's orders
- Create a user
- Update a user's profile

### Expected Answer

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]                          // GET /api/users
    public IActionResult GetAll() { ... }

    [HttpGet("{id}")]                  // GET /api/users/10
    public IActionResult GetById(int id) { ... }

    [HttpGet("{id}/orders")]           // GET /api/users/10/orders
    public IActionResult GetUserOrders(int id) { ... }

    [HttpPost]                         // POST /api/users
    public IActionResult Create([FromBody] CreateUserRequest request) { ... }

    [HttpPut("{id}/profile")]          // PUT /api/users/10/profile
    public IActionResult UpdateProfile(int id, [FromBody] UpdateProfileRequest request) { ... }
}
```

---

## Exercise 5: Convert to Dependency Injection

> 🎯 **Objective:** Convert a tightly coupled service into a Dependency Injection design.

**Given this tightly coupled code:**

```csharp
public class OrdersController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly UserService _userService;

    public OrdersController()
    {
        _productService = new ProductService();
        _userService = new UserService();
    }
}
```

**Convert it to use DI.**

### Expected Answer

```csharp
// 1. Create interfaces
public interface IProductService { ... }
public interface IUserService { ... }

// 2. Implement services
public class ProductService : IProductService { ... }
public class UserService : IUserService { ... }

// 3. Register in Program.cs
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();

// 4. Inject through constructor
public class OrdersController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IUserService _userService;

    public OrdersController(IProductService productService, IUserService userService)
    {
        _productService = productService;
        _userService = userService;
    }
}
```

---

# Knowledge Check

Test your understanding of today's material.

### Questions

1. Why should a DELETE endpoint not be implemented using GET?
2. What is the difference between HTTP 401 and HTTP 403?
3. What problem does Dependency Injection solve?
4. Why should controllers remain thin?
5. What is the role of middleware in ASP.NET Core?
6. What is the difference between route parameters and query parameters?
7. What does `builder.Services.AddScoped<IProductService, ProductService>()` mean?
8. Why is HTTP status code 200 for errors a bad practice?
9. What is idempotency, and which HTTP methods are idempotent?
10. What is the difference between PUT and PATCH?

---

### Answers

<details>
<summary>Click to reveal answers</summary>

1. **DELETE vs GET:** GET is idempotent and safe — it should not modify server state. Browsers may prefetch links, proxies may cache requests, and users may bookmark URLs. Implementing DELETE as GET risks unintended data loss.

2. **401 vs 403:** 401 means "you are not authenticated" (the server does not know who you are). 403 means "you are authenticated but not authorized" (the server knows who you are, but you lack permission).

3. **DI solves tight coupling:** Without DI, a class creates its own dependencies with `new`, making it impossible to substitute different implementations for testing or when requirements change. DI allows classes to depend on abstractions.

4. **Thin controllers:** Controllers handle HTTP concerns. Business logic in controllers is untestable, cannot be reused, and makes the application difficult to maintain as it grows.

5. **Middleware role:** Middleware processes requests and responses in a pipeline. Each component can handle, modify, or pass through requests. This enables cross-cutting concerns like authentication, logging, and error handling.

6. **Route vs query parameters:** Route parameters are part of the URL path (`/api/products/{id}`) and identify a specific resource. Query parameters come after `?` (`/api/products?page=1`) and provide additional filtering/sorting criteria.

7. **Scoped registration:** This tells the DI container that whenever `IProductService` is requested within a single HTTP request, provide a new instance of `ProductService`. Each request gets its own instance.

8. **HTTP 200 for errors:** It prevents monitoring tools from detecting failures, makes error handling in clients unreliable, misrepresents API behavior, and breaks standard HTTP tooling.

9. **Idempotency:** An operation is idempotent if calling it multiple times produces the same result as calling it once. GET, PUT, and DELETE are idempotent. POST is not.

10. **PUT vs PATCH:** PUT replaces the entire resource — any field not included is reset. PATCH partially updates the resource — only the specified fields change.

</details>

---

# Day 1 Final Summary

```text
The Web
  ↓
HTTP — the language of the Web
  ↓
REST — an architectural style for APIs
  ↓
ASP.NET Core — the framework that implements it
  ↓
Middleware — processes requests in a pipeline
  ↓
Routing — maps URLs to controllers
  ↓
Controllers — handle HTTP requests
  ↓
Dependency Injection — provides dependencies loosely
  ↓
Services — contain business logic
  ↓
Professional API
```

---

## ✅ What You Should Know After Day 1

- [ ] How HTTP requests and responses work
- [ ] The purpose and meaning of HTTP methods (GET, POST, PUT, PATCH, DELETE)
- [ ] How to use HTTP status codes correctly
- [ ] REST principles and resource-oriented URL design
- [ ] How to create an ASP.NET Core Web API project
- [ ] The role of `Program.cs` and the application startup process
- [ ] How middleware works and why order matters
- [ ] How controllers handle HTTP requests
- [ ] How routing maps URLs to controller actions
- [ ] Why Dependency Injection is essential
- [ ] How to register services in the DI container
- [ ] The difference between Singleton, Scoped, and Transient lifetimes
- [ ] Why controllers should remain thin
- [ ] Common beginner mistakes and how to avoid them

---

# 📅 Preview of Day 2

# Day 2 — Web API Development & Entity Framework Core

In the next session, you will learn:

- **CRUD APIs** — Complete Create, Read, Update, Delete implementations
- **DTOs** — Data Transfer Objects for controlling what your API exposes
- **Model Binding** — How ASP.NET Core maps HTTP request data to C# objects
- **Validation** — Ensuring incoming data meets your requirements using FluentValidation or Data Annotations
- **Entity Framework Core** — The ORM that connects your code to a database
- **DbContext** — Understanding the database session
- **Migrations** — Version controlling your database schema
- **SQL Server Integration** — Connecting to a real database

You will build on the foundation from Day 1 and create a fully functional API backed by a real database.

---

> 🟦 **Remember:** The best architecture is the simplest one that solves the problem. Start simple, add complexity when the codebase justifies it.

---

*End of Day 1 Training Material*
