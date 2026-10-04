# 05 — Swagger / OpenAPI

**Duration:** ~25 min

---

## 🎯 What You'll Learn

By the end of this topic, you'll be able to:

- Explain what OpenAPI and Swagger are
- Access Swagger UI in your API
- Test API endpoints using Swagger UI
- Understand how ASP.NET Core generates API documentation

---

## 📖 What Are OpenAPI and Swagger?

**OpenAPI** is a specification for describing REST APIs. It's a standard format (JSON/YAML) that describes:
- Available endpoints
- HTTP methods (GET, POST, PUT, DELETE)
- Request/response schemas
- Parameters (route, query, body)

**Swagger** is a set of tools that implement OpenAPI:
- **Swagger UI** — Interactive web interface to explore and test the API
- **Swashbuckle** — .NET library that generates OpenAPI docs from your code

**Relationship:**
```text
Your Code
    ↓
Swashbuckle (generates OpenAPI spec)
    ↓
OpenAPI Specification (JSON/YAML)
    ↓
Swagger UI (interactive documentation)
```

---

## 🔧 How It's Already Configured

The .NET 8 Web API template includes Swagger out of the box. Check `Program.cs`:

```csharp
builder.Services.AddSwaggerGen();  // Generates OpenAPI spec

// ...

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();       // Serves the OpenAPI JSON
    app.UseSwaggerUI();     // Serves the interactive UI
}
```

**What this means:**
- In Development, Swagger is available
- In Production, it's disabled (for security)

---

## 🌐 Accessing Swagger UI

**Start the API:**

```bash
dotnet run
```

**Open in browser:**

```text
http://localhost:5000/swagger
```

or

```text
http://localhost:5000/swagger/index.html
```

You'll see the Swagger UI with all your endpoints grouped by controller.

---

## 🧪 Testing Endpoints with Swagger UI

### Example 1: GET /api/employees

1. Click on **GET /api/employees** to expand it
2. Click **Try it out**
3. Click **Execute**
4. You'll see:
   - **Curl** command (you can copy it)
   - **Request URL**
   - **Server response** (status code, body, headers)

### Example 2: POST /api/employees

1. Click on **POST /api/employees**
2. Click **Try it out**
3. Edit the request body:

```json
{
  "name": "Omar",
  "department": "Finance"
}
```

4. Click **Execute**
5. You'll see:
   - Status: `201 Created`
   - Response body: the created employee
   - Location header: URL to the new resource

### Example 3: PUT /api/employees/{id}

1. Click on **PUT /api/employees/{id}**
2. Click **Try it out**
3. Set `id` to `4` (or whatever ID exists)
4. Edit the request body:

```json
{
  "name": "Omar Ben Ali",
  "department": "Marketing"
}
```

5. Click **Execute**
6. You'll see: `204 No Content`

### Example 4: DELETE /api/employees/{id}

1. Click on **DELETE /api/employees/{id}**
2. Click **Try it out**
3. Set `id` to `4`
4. Click **Execute**
5. You'll see: `204 No Content`

---

## 🔍 Exploring the OpenAPI Spec

Swagger UI is built from the OpenAPI JSON spec. Access it at:

```text
http://localhost:5000/swagger/v1/swagger.json
```

This JSON file describes your entire API. It includes:

```json
{
  "openapi": "3.0.1",
  "info": {
    "title": "EmployeeManagement.Api",
    "version": "1.0"
  },
  "paths": {
    "/api/employees": {
      "get": { ... },
      "post": { ... }
    },
    "/api/employees/{id}": {
      "get": { ... },
      "put": { ... },
      "delete": { ... }
    }
  },
  "components": {
    "schemas": {
      "Employee": { ... },
      "CreateEmployeeDto": { ... }
    }
  }
}
```

**What's in the spec:**
- **paths** — All endpoints and their operations
- **components/schemas** — Data models (DTOs)
- **parameters** — Route and query parameters
- **requestBody** — Expected request bodies
- **responses** — Possible responses and their schemas

---

## 🧪 Practice Exercise

**Task:** Use Swagger UI to test all CRUD operations.

**Steps:**
1. Start the API: `dotnet run`
2. Open Swagger UI: `http://localhost:5000/swagger`
3. Perform the following operations in order:
   - **GET** `/api/employees` — list all employees
   - **POST** `/api/employees` — create a new employee (e.g., "Omar", "Finance")
   - **GET** `/api/employees/{id}` — get the new employee by ID
   - **PUT** `/api/employees/{id}` — update the employee (change name/department)
   - **GET** `/api/employees/{id}` — verify the update
   - **DELETE** `/api/employees/{id}` — delete the employee
   - **GET** `/api/employees` — verify deletion

4. For each operation, note:
   - Status code
   - Response body
   - Any headers (e.g., Location for POST)

---

## 💡 Enhancing Swagger Documentation

You can add XML comments to make Swagger more descriptive:

### Enable XML Comments

In `.csproj`:

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

In `Program.cs`:

```csharp
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
```

### Add Comments to Code

```csharp
/// <summary>
/// Creates a new employee.
/// </summary>
/// <param name="dto">The employee data.</param>
/// <returns>The created employee.</returns>
/// <response code="201">Returns the created employee</response>
/// <response code="400">If the input is invalid</response>
[HttpPost]
[ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public IActionResult Create([FromBody] CreateEmployeeDto dto)
{
    // ...
}
```

Now Swagger UI shows your descriptions and response codes.

---

## 💡 Key Takeaways

**Swagger UI is your best friend for:**
- Testing endpoints without curl
- Exploring the API contract
- Debugging request/response shapes
- Sharing the API with other developers

**OpenAPI spec is useful for:**
- Generating client code (SDKs)
- Mock servers
- API validation
- Documentation

**Best practices:**
- Test with Swagger UI during development
- Keep the spec accurate (it's generated from your code)
- Add XML comments for better documentation
- Disable Swagger in Production (it's already disabled by default)

---

## 🔜 Next

We've tested our API with Swagger. Next, we'll review API design best practices to make our API clean and consistent.
