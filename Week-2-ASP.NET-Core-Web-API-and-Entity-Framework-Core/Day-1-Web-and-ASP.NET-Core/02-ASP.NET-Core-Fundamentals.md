# 02 — ASP.NET Core Fundamentals

---

## 🎯 What You Will Learn (25 min)

By the end you can explain:

- What .NET, ASP.NET Core, and ASP.NET Core Web API each are
- What happens when you run `dotnet run`
- How a request enters the application and reaches your code

---

## The Three Names (Do Not Mix Them)

| Name | What it is |
|------|------------|
| **.NET** | The platform that compiles and runs C# |
| **ASP.NET Core** | The web framework built on .NET |
| **ASP.NET Core Web API** | The project type for APIs that return JSON |

```text
.NET  (runs C# code)
  └── ASP.NET Core  (understands HTTP)
        └── Web API  (returns JSON, not HTML)
```

> 💡 **Senior Developer Note:** If someone asks *"is this .NET or ASP.NET?"* the answer is both. .NET is the runtime; ASP.NET Core is the web stack running on top of it.

---

## Application Startup — `Program.cs`

A .NET 8 Web API has exactly one entry point: `Program.cs`. Here is the generated version:

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
┌─────────────────────────────────────────────┐
│ PART 1 — before the app runs                │
│                                             │
│ builder.Services.AddXxx(...)  register      │
│ builder.Build()               build it      │
└─────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────┐
│ PART 2 — the request pipeline               │
│                                             │
│ app.UseXxx(...)  middleware (around each    │
│                  request)                   │
│ app.MapXxx(...)  endpoints (who answers)    │
└─────────────────────────────────────────────┘
                     ↓
               app.Run()  listen
```

| Line | Job |
|------|-----|
| `WebApplication.CreateBuilder(args)` | Create the builder, load configuration |
| `builder.Services.AddControllers()` | Register controllers (the feature `[ApiController]` needs) |
| `AddEndpointsApiExplorer` / `AddSwaggerGen` | Swagger documentation support |
| `builder.Build()` | Turn the builder into the running app |
| `app.UseSwagger()` | Swagger page (Development only) |
| `app.UseHttpsRedirection()` | Send HTTP clients to HTTPS |
| `app.UseAuthorization()` | Authorization (not used today — Day 2+) |
| `app.MapControllers()` | **Publish every controller route** — without this, nothing answers |
| `app.Run()` | Start listening for requests |

> ⚠️ The registration zone is `builder.Services`, **before** `builder.Build()`. The pipeline zone is everything after.

---

## How a Request Enters the Application

```text
Client sends request
         │
         ▼
      Kestrel (the web server)
         │
         ▼
      Pipeline of middleware
         │
         ▼
      Routing matches URL + method to an endpoint
         │
         ▼
      Controller action runs
         │
         ▼
      Response travels back out
```

**Three rules:**

1. **Order matters.** Middleware runs in the order you add it.
2. **Routing matches first.** If no route matches, your code never runs → `404`.
3. **Routing is automatic.** You only add `app.MapControllers()` — ASP.NET Core handles the rest.

---

## Running It

```bash
dotnet run
```

The console prints:

```text
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5189
info: Microsoft.Hosting.Lifetime[0]
      Application started.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
```

The address (`localhost:5189` here) comes from `Properties/launchSettings.json`. Yours will differ — **read it from the console**.

---

## 🧪 Think

1. You comment out `app.MapControllers()`. What happens when you call an endpoint?
2. You add `app.UseXxx()` before `app.MapControllers()`. Does it run for every request?

<details>
<summary><b>Answers</b></summary>

1. Every request returns `404` with an empty body. The routes exist in your code but were never published.
2. Yes — middleware runs for every request that reaches it, before the endpoint.

</details>

---

## ✅ Check Yourself

- [ ] I can name the three names (.NET / ASP.NET Core / Web API)
- [ ] I can explain what `Program.cs` does in one sentence
- [ ] I know the difference between `builder.Services` and `app.Use...`
- [ ] I know that `app.MapControllers()` publishes controller routes
- [ ] I ran `dotnet run` and read the listening address

---

**Next: [03 — Project Structure](03-Project-Structure.md)**
