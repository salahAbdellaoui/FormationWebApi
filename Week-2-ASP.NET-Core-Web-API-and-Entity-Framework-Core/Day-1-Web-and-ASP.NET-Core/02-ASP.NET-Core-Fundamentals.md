# 02 — ASP.NET Core Fundamentals

---

## 🎯 Learning Objectives

By the end of this topic, you will be able to:

- Explain the difference between .NET, ASP.NET Core, and ASP.NET Core Web API
- Create and run a .NET 8 Web API project from the command line
- Read `Program.cs` and say what each block does
- Describe what happens to a request from `Kestrel` to the controller and back
- Explain what middleware is and why the order of the pipeline matters
- Say where services, configuration, and logging live in the application

---

## The Problem

Topic 01 gave us a contract:

```text
GET /api/employees   →   200 + JSON
```

But a contract needs somebody to **answer**. Who receives the request, decides which code runs, and writes the response?

That somebody is **ASP.NET Core** — a C# framework for building web applications and HTTP APIs.

---

## The Three Names (Do Not Mix Them)

| Name | What it is | Example from Week 1 |
|------|------------|---------------------|
| **.NET** | The platform that compiles and runs C# code | You wrote `class Employee` and ran it with `dotnet run` |
| **ASP.NET Core** | The web framework built *on* .NET (HTTP, routing, dependency injection, configuration …) | Nothing yet — this is new today |
| **ASP.NET Core Web API** | The ASP.NET Core project type for building **HTTP APIs that return data** (JSON), not HTML pages | The project we create below |

```text
.NET  (platform: runs C#)
  └── ASP.NET Core  (web: understands HTTP)
        └── Web API  (project type: returns JSON to clients)
```

> 💡 **Senior Developer Note:** ASP.NET Core is cross-platform, open source, and runs on the web server **Kestrel**, which is included in the framework. You do not install a separate web server to develop.

---

## Create and Run the Project

### Step 1 — Create

```bash
dotnet new webapi -n EmployeeManagement.Api --use-controllers
```

| Part | Meaning |
|------|---------|
| `dotnet new webapi` | The Web API project template |
| `-n EmployeeManagement.Api` | The project (and folder) name |
| `--use-controllers` | Build the API with **controllers** — the style we learn in Topic 04 |

> ⚠️ On .NET 8 the `webapi` template builds a **minimal API** sample by default. Without `--use-controllers` the project contains no `Controllers/` folder at all — only a `Program.cs` with a sample `app.MapGet(...)`. Today's whole lesson is about controllers, so always pass `--use-controllers`.

### Step 2 — Run

```bash
cd EmployeeManagement.Api
dotnet run
```

Verified console output (the exact port depends on **your** project):

```text
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5189
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\...\EmployeeManagement.Api
```

| Line | Meaning |
|------|---------|
| `Now listening on:` | The address your clients must call |
| `Application started` | Startup finished; the pipeline is ready for requests |
| `Hosting environment: Development` | Which configuration and error behaviour apply (see below) |

Stop the app with `Ctrl + C`.

> 💡 **Senior Developer Note:** `dotnet run` reads `Properties/launchSettings.json` to choose the address and environment. Use `dotnet run --launch-profile https` to pick another profile, or `--no-launch-profile` to ignore the file.

---

## Application Startup — `Program.cs`

Modern .NET uses **top-level statements**: `Program.cs` *is* the entry point — no `Main` method, no `Startup` class. The file runs **once**, from top to bottom, when the application starts.

The generated file (from the .NET 8 `--use-controllers` template), with the Swagger comments removed for readability:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

### Two halves, two jobs

```text
┌───────────────────────────────────────────────────────┐
│  PART 1 — BEFORE the app runs   (builder)             │
│                                                       │
│  builder.Services  → register what the app can create │
│                      controllers, services, config    │
└───────────────────────────────────────────────────────┘
                         ↓ builder.Build()
┌───────────────────────────────────────────────────────┐
│  PART 2 — the REQUEST PIPELINE   (app)                │
│                                                       │
│  app.Use...    → middleware (what runs around requests)│
│  app.Map...    → endpoints (which code answers)        │
└───────────────────────────────────────────────────────┘
                         ↓ app.Run()
                   listen for requests
```

| Line | What it does |
|------|--------------|
| `WebApplication.CreateBuilder(args)` | Creates the builder: loads configuration, prepares the service container |
| `builder.Services.AddControllers()` | Registers MVC-style controllers (the feature `[ApiController]` needs) |
| `AddEndpointsApiExplorer()` / `AddSwaggerGen()` | Support for the Swagger test page the template ships |
| `builder.Build()` | Turns the builder into the running application (`app`) |
| `if (app.Environment.IsDevelopment())` | Development-only middleware |
| `app.UseSwagger()` / `UseSwaggerUI()` | Serves the Swagger page so you can click requests while learning |
| `app.UseHttpsRedirection()` | Sends HTTP clients to the HTTPS address when an HTTPS port is known |
| `app.UseAuthorization()` | Authorization middleware — we do not use it today (authentication and authorization come in a later lesson) |
| `app.MapControllers()` | **Publishes every controller route** — without this line, no controller exists for clients |
| `app.Run()` | Starts the server and blocks until you stop it |

> ⚠️ Registration (`builder.Services`) must happen **before** `builder.Build()`. The container is created at `Build()`; anything registered afterwards is not available.

---

## The Request Pipeline

**Middleware** is a small component that sits in the pipeline and can:

1. look at the request,
2. change it or answer it and **stop** the pipeline,
3. otherwise pass it to the next component,
4. and on the way back, look at or change the response.

```text
                 REQUEST (way in)
   Client ─────────────────────────────────────►
             │
             ▼
      [ ROUTING ]               ASP.NET Core matches URL + method → endpoint
             │                    no match? → 404, your code never runs
             ▼
      [ Swagger UI ]             only in Development
             │
             ▼
      [ HTTPS redirection ]
             │
             ▼
      [ Authorization ]          present in the template; unused today
             │
             ▼
      [ CONTROLLER ACTION ]      the matched endpoint executes — your C# code
             │
             ▼
   ◄─────────────────────────────────────────────  RESPONSE (way out)
                 the response travels back out
```

Three rules:

- **Order matters.** The components run in the order they were added. A component added later runs later.
- **Routing matches first.** Routing is placed before the components you add (verified: a middleware asked ASP.NET Core for the selected endpoint and it was already chosen). If nothing matches the URL and method, your controller never runs — the request ends with `404` and an empty body.
- **Routing middleware is not yours to write.** `app.MapControllers()` publishes your controller routes; ASP.NET Core executes the matched action *after* the middleware in front of it.

> 💡 **Senior Developer Note:** Learn the pipeline before you add complexity. Half of "the API does not work" bugs are solved by asking: *did the request even reach my code?*

---

## Where Do Registered Things Live?

Everything the application can create for you is registered in one place:

```csharp
builder.Services.AddControllers();          // framework feature
builder.Services.AddScoped<IEmployeeService, EmployeeService>();   // your service (Topic 05)
```

`builder.Services` is the **service registry**. ASP.NET Core reads it when a controller is created and hands it the objects it asked for. This mechanism is **Dependency Injection** — Topic 05 devotes a full session to it. Here, you only need to know *where* it happens: always in Part 1 of `Program.cs`.

Why it is separated from the pipeline: registration happens **once at startup**, while the pipeline runs **for every request**.

---

## Configuration

Settings must not live inside your C# code. ASP.NET Core loads them from files:

| File | Loaded when | Purpose |
|------|-------------|---------|
| `appsettings.json` | Always | Default settings for every environment |
| `appsettings.Development.json` | Only when the environment is `Development` | Values that override the defaults while you develop |

Both files exist in the generated project. The generated `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

How the layers work:

```text
appsettings.json               ← base values
        +
appsettings.Development.json   ← overrides, only in Development
        +
environment variables          ← e.g. ASPNETCORE_ENVIRONMENT=Development
        =
      effective configuration
```

- Values are read **by key**, for example `Logging:LogLevel:Default`.
- The environment comes from `Properties/launchSettings.json` (`ASPNETCORE_ENVIRONMENT`), which is why the console printed `Hosting environment: Development`.
- Later in the course, a database connection string will be added here. **Never hard-code secrets in code.**

> 💡 **Senior Developer Note:** A value that differs between your machine, the test server, and production belongs in configuration — not in an `if` statement in your service.

---

## Logging

Logging is how the application tells you what it is doing while it runs.

The generated project already configures it (see the `Logging` section above):

| Setting | Meaning |
|---------|---------|
| `Default: Information` | Show informational messages and above |
| `Microsoft.AspNetCore: Warning` | Silence framework chatter below `Warning` |

Your code receives a logger through **constructor injection** (Topic 05 explains how):

```csharp
private readonly ILogger<EmployeesController> _logger;

public EmployeesController(ILogger<EmployeesController> logger)
{
    _logger = logger;
}

_logger.LogInformation("Getting all employees");
```

Logs appear in the console where the application runs. Logging has levels — `Information`, `Warning`, `Error` and others — and you will use them more in later lessons. Today: **know that it exists, know that it is configured in `appsettings.json`, and know that the logger is injected like any other service.**

---

## Development vs Production

The environment changes behaviour:

| Behaviour | Development | Production |
|-----------|-------------|------------|
| Environment name | `Development` | `Production` (default when not set) |
| Configuration | `appsettings.json` + `appsettings.Development.json` | `appsettings.json` only |
| Swagger page | Available (when the template middleware is enabled) | Not enabled by the template |
| Errors | Detailed error page with the exception | No internal details revealed to the client |

Verified example: when a service is not registered, the **Development** environment answered `500` with an error page containing:

```text
Unable to resolve service for type '...IEmployeeService' while attempting to
activate '...EmployeesController'.
```

That page is a development tool. You will not see those internals in Production — which is exactly why you read logs instead of guessing.

---

## 🧪 Exercise 1 — Read `Program.cs`

Open the `Program.cs` of your project and answer without running anything:

1. Which lines belong to **Part 1** (registration, before the app runs)?
2. Which lines belong to **Part 2** (the pipeline)?
3. Which single line publishes your controller routes?
4. If you comment out `builder.Services.AddControllers()` and run the app, what do you expect to happen — a `404` for every request, or a failure while starting? Try it.

<details>
<summary><b>Answers</b></summary>

1. Everything from `var builder = WebApplication.CreateBuilder(args);` to `var app = builder.Build();` — the `builder.Services...` calls.
2. Everything after `builder.Build()` until `app.Run()`.
3. `app.MapControllers();`
4. It fails **while starting**: `app.MapControllers()` cannot publish routes when the controller services are missing, so the application stops with an error instead of answering requests.

</details>

---

## 🧪 Exercise 2 — Run and Observe

Hands-on, five minutes:

1. Create a project: `dotnet new webapi -n Exercise02 --use-controllers`
2. Run it: `dotnet run`
3. Copy the `Now listening on:` address.
4. Call the sample endpoint from a browser or `curl` (replace the port with **your** `Now listening on:` address):

```bash
curl -i http://localhost:5189/weatherforecast
```

(`-i` prints the status line and headers — everything Topic 01 taught.)

5. Write down: status code, `Content-Type`, and whether the body is JSON.
6. Call a wrong path, for example `http://localhost:5189/doesnotexist`, and compare: status code and body.

**Expect:** `200` with a JSON array for the sample endpoint, and `404` with an empty body for the wrong path.

---

## 🧪 Exercise 3 — Trace the Request

Fill in the blanks from memory:

```text
Client sends GET /api/employees
        ↓
[1] ____________ (the web server) receives the bytes
        ↓
[2] middleware runs in ____________ they were added
        ↓
[3] ____________ matches the URL + method to an endpoint
        ↓
[4] the controller action runs, using services from ____________
        ↓
[5] the response (status + headers + JSON) travels back to the ____________
```

<details>
<summary><b>Answers</b></summary>

1. Kestrel
2. the order in which
3. Routing
4. the service registry (`builder.Services`, via Dependency Injection)
5. client

</details>

---

## ⚠️ Common Mistakes

| # | Mistake | What really happens | Fix |
|---|---------|---------------------|-----|
| 1 | Editing the pipeline in the wrong place | Registration after `Build()` is ignored | Register in `builder.Services`, before `builder.Build()` |
| 2 | Expecting routes to exist without mapping them | Every request answers `404`, the app still runs | Keep `app.MapControllers()` |
| 3 | Forgetting that configuration is layered | "My value in `appsettings.json` is ignored" | The environment-specific file (or an environment variable) is overriding it |
| 4 | Putting secrets in code | They leak into source control | Configuration files / environment variables |
| 5 | Reading production errors like a developer | Production hides exception details | Check the logs; reproduce in Development |
| 6 | Confusing *startup* errors with *request* errors | Wrong debugging direction | Startup error = registration/pipeline problem; request error = routing/action/DI problem |

> 💡 **Senior Developer Note:** If the application does not even start, no amount of route debugging will help. Startup failures almost always come from `Program.cs`.

---

## 💡 Senior Developer Notes

- Keep `Program.cs` readable: registration first, pipeline second, `Run()` last.
- Understand the request pipeline **before** adding middleware you found on the internet.
- Prefer dependency injection over constructing objects by hand inside controllers — Topic 05 explains why.
- Configuration and logs are part of the framework, not extra features: use them from day one.
- Do not add architectural layers before the fundamentals are solid. A small, correct API teaches you more than a large, confusing one.

---

## 🧠 Knowledge Check

### Question 1

Which is which: .NET, ASP.NET Core, ASP.NET Core Web API?

**Answer:** .NET runs C#. ASP.NET Core is the web framework on top of .NET. Web API is the project type for HTTP APIs that return data (JSON).

---

### Question 2

Which line publishes every controller route?

**Answer:** `app.MapControllers();`

---

### Question 3

True or False: you can call `builder.Services.AddScoped(...)` after `builder.Build()`.

**Answer:** False. The container is created by `Build()`; registration must happen before it.

---

### Question 4

What is middleware?

**Answer:** A component in the pipeline that can inspect a request, change it, answer it and stop the pipeline, or pass it to the next component. The response travels back through the same components.

---

### Question 5

Where do `appsettings.json` values live in the code?

**Answer:** They are not "in" the code — they are loaded into the configuration system at startup and read by key (for example `Logging:LogLevel:Default`).

---

### Question 6

Your app starts, but every request returns `404` with an empty body. Is this a startup problem or a request problem?

**Answer:** A request problem — the app runs, but no endpoint matched. In Topic 04 you will check route mapping (`app.MapControllers()`, the route template, the URL, the method).

---

## ✅ Check Yourself

- [ ] I can explain .NET vs ASP.NET Core vs Web API in one sentence
- [ ] I created and ran a Web API project with `--use-controllers`
- [ ] I can read `Program.cs` and separate registration from pipeline
- [ ] I know what middleware is and why order matters
- [ ] I know where configuration and logging are configured
- [ ] I traced a request from the client to the controller and back

---

## Summary

| Concept | What to remember |
|---------|------------------|
| .NET | Platform that runs C# |
| ASP.NET Core | The web framework built on .NET |
| Web API | Project type that answers HTTP with JSON |
| `Program.cs` | Runs once at startup: register → build pipeline → run |
| `builder.Services` | The service registry (registration zone) |
| `builder.Build()` | Creates the application |
| Middleware | Components the request passes through, in order |
| Routing | Matches URL + method to an endpoint |
| `app.MapControllers()` | Publishes controller routes |
| Configuration | Layered files: base + environment overrides |
| Logging | Configured in `appsettings.json`, injected with `ILogger<T>` |

---

**Next: [03 — Project Structure](03-Project-Structure.md)**

