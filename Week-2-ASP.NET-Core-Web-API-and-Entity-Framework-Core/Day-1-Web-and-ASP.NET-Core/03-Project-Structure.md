# 03 — Project Structure

---

## 🎯 Learning Objectives

By the end of this topic, you will be able to:

- List the files and folders of a .NET 8 Web API project and say what each one is for
- Read a `.csproj` file and explain its properties and package references
- Explain what `launchSettings.json` controls — and what it does not
- Decide which file to edit for code, for settings, and for startup behaviour
- Add folders to a project as it grows, without breaking the build
- Recognise that the structure of a small project is a starting point, not a law

---

## The Problem

You just created a project and see ten files you did not write. Two of them look almost identical. One of them contains a port number. One of them is a folder that keeps coming back after you delete it.

Without a map, you will edit the wrong file and then lose an hour wondering why "nothing changed".

---

## The Project

If you have not created it yet:

```bash
dotnet new webapi -n EmployeeManagement.Api --use-controllers
```

### The structure you get (verified with the .NET 8 template)

```text
EmployeeManagement.Api/
├── Controllers/
│   └── WeatherForecastController.cs      ← sample controller (we replace it)
├── Properties/
│   └── launchSettings.json               ← how `dotnet run` starts the app
├── appsettings.json                      ← configuration for every environment
├── appsettings.Development.json          ← configuration for Development only
├── Program.cs                            ← the entry point (Topic 02)
├── EmployeeManagement.Api.csproj         ← project definition
├── EmployeeManagement.Api.http           ← sample requests for your IDE
├── WeatherForecast.cs                    ← sample model (we replace it)
├── bin/                                  ← appears after the first build (generated)
└── obj/                                  ← appears after the first build (generated)
```

> 💡 **Senior Developer Note:** This is the structure of a **small** project — not a mandatory structure for every ASP.NET Core application. ASP.NET Core does not require a `Controllers/` folder: the folder exists because the template puts controllers there and convention keeps projects readable.

---

## What Each Piece Is Responsible For

| File / folder | Responsibility | Edit it when… |
|---------------|----------------|---------------|
| `Program.cs` | Startup: registration + pipeline | You add a service, middleware, or an endpoint mapping |
| `Controllers/` | HTTP endpoints (Topic 04) | You add or change an endpoint |
| `Models/` *(you create it)* | Data types your API exchanges (Topic 04) | You add or change a type such as `Employee` |
| `Services/` *(you create it)* | Application logic behind the endpoints (Topic 05) | You add or change business behaviour |
| `appsettings.json` | Settings for every environment | The value must exist everywhere |
| `appsettings.Development.json` | Settings only while developing | The value must exist only on your machine |
| `Properties/launchSettings.json` | Port, environment, URL for `dotnet run` | You want another port or environment while developing |
| `.csproj` | Project definition: framework, packages, options | You add a NuGet package or change the target framework |
| `bin/`, `obj/` | Build output | **Never** — they are generated |
| `.http` | Ready-made requests | You want to send a request from your IDE |

---

## The `.csproj` File

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
  </ItemGroup>

</Project>
```

| Piece | Meaning |
|-------|---------|
| `Sdk="Microsoft.NET.Sdk.Web"` | This is a **web** project: it knows ASP.NET Core, not just C# |
| `<TargetFramework>net8.0</TargetFramework>` | Builds against **.NET 8** — the training baseline |
| `<Nullable>enable</Nullable>` | The compiler warns you about possible `null` values |
| `<ImplicitUsings>enable</ImplicitUsings>` | Common namespaces (`System`, `System.Linq`, ASP.NET Core …) are imported automatically — that is why `Program.cs` has no `using` lines |
| `<PackageReference>` | A NuGet package the project depends on (Swagger, here) |

Because this is an SDK-style project, **your `.cs` files are included automatically** — you never list them in the `.csproj`.

> ⚠️ Renaming a file is safe. Deleting `bin/` and `obj/` is safe — the next build recreates them. Editing generated files by hand is not.

---

## `Program.cs`

The entry point you already read in Topic 02: registration first, pipeline second, `app.Run()` last. There is exactly one `Program.cs` per project, and it runs **once per application start**.

---

## Configuration Files

Two files, one rule: **the more specific file wins.**

```text
appsettings.json              ← always loaded
appsettings.Development.json  ← loaded on top, only in Development
```

| You want to change… | Edit |
|----------------------|------|
| A log level for every environment | `appsettings.json` |
| A value that only your machine needs | `appsettings.Development.json` |
| Which environment is active while developing | `Properties/launchSettings.json` (`ASPNETCORE_ENVIRONMENT`) |

---

## `Properties/launchSettings.json`

A trimmed version of the generated file (the real one also contains an `IIS Express` profile; **port numbers are generated per project, so yours will differ**):

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:5189",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7286;http://localhost:5189",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

| Key | Meaning |
|-----|---------|
| `applicationUrl` | The address the app listens on while developing |
| `ASPNETCORE_ENVIRONMENT` | Which environment (and therefore which configuration file) applies |
| `launchUrl` | Which page the IDE opens in the browser |
| `dotnetRunMessages` | Prints `Now listening on: ...` in the console |

> ⚠️ `launchSettings.json` is a **development** file. It configures how `dotnet run` and your IDE start the app on your machine. It is not part of a deployed application — production URLs and environments are configured where the application runs.

---

## `Controllers/`, `Models/`, `Services/`

Only `Controllers/` exists in the template. For today's project we add the other two:

```text
EmployeeManagement.Api/
├── Controllers/
│   └── EmployeesController.cs     ← Topic 04
├── Models/
│   └── Employee.cs                ← Topic 04
├── Services/
│   ├── IEmployeeService.cs        ← Topic 05
│   └── EmployeeService.cs         ← Topic 05
├── Program.cs
├── appsettings.json
└── ...
```

**Folders group code by purpose.** The compiler does not care about them; the *team* does. A new folder needs no configuration — create it, put a `.cs` file inside, and it is compiled with the rest.

---

## `bin/` and `obj/`

| Folder | Content |
|--------|---------|
| `bin/` | The compiled application (`.dll`, `.exe`) |
| `obj/` | Intermediate build files |

Both are **generated by the build**, so you never edit them and you never commit them to version control (Week 1, Day 4). If they ever look wrong, delete them and rebuild.

---

## How the Structure Evolves

A bigger project is the same idea, extended:

```text
EmployeeManagement.Api/
├── Controllers/        ← endpoints
├── Models/             ← domain types
├── Services/           ← logic
├── Dtos/               ← request/response shapes        (later lessons)
├── Middleware/          ← custom pipeline components      (later lessons)
├── appsettings.json
├── appsettings.Production.json      ← appears when environments multiply
├── Program.cs
└── EmployeeManagement.Api.csproj
```

Later lessons will add folders, and a later lesson will discuss structuring larger applications. **Do not pre-build the skeleton of an application you do not have yet.** Add a folder when you have something real to put in it.

> 💡 **Senior Developer Note:** A structure is a communication tool. A developer who joins the project should find the endpoints, the data types, and the logic within seconds — because they live where everyone expects them.

---

## 🧪 Exercise 1 — Match the File to the Job

Write the file/folder you would open for each task:

| # | Task | File / folder |
|---|------|---------------|
| 1 | Change the port used by `dotnet run` | |
| 2 | Add a new endpoint | |
| 3 | Add a NuGet package | |
| 4 | Change a log level for everyone | |
| 5 | Add the `Employee` type | |
| 6 | Register a service | |
| 7 | Find the compiled output | |

<details>
<summary><b>Answers</b></summary>

1. `Properties/launchSettings.json`
2. a file in `Controllers/`
3. `EmployeeManagement.Api.csproj`
4. `appsettings.json`
5. `Models/`
6. `Program.cs`
7. `bin/`

</details>

---

## 🧪 Exercise 2 — Prepare Today's Project

```bash
cd EmployeeManagement.Api
mkdir Models
mkdir Services
```

1. Confirm the two folders appear next to `Controllers/`.
2. Run `dotnet build` — it must still succeed (an empty folder changes nothing).
3. Create `Models/Employee.cs` with the class below (we use it in Topic 04):

```csharp
namespace EmployeeManagement.Api.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}
```

4. Run `dotnet build` again. Expected: **Build succeeded**, 0 errors.

---

## 🧪 Exercise 3 — Debug: "My Setting Has No Effect"

A trainee edits `appsettings.json` and changes the default log level to `Warning`. Nothing changes in the console.

Answer these three questions:

1. Which environment is the app running in while developing?
2. Which configuration file could be overriding the value?
3. What should they check first?

<details>
<summary><b>Answers</b></summary>

1. `Development` — set by `launchSettings.json` (`ASPNETCORE_ENVIRONMENT`) and printed in the console as `Hosting environment: Development`.
2. `appsettings.Development.json` — it is loaded on top of `appsettings.json` and wins.
3. Confirm the effective environment (console line), then look at every file that sets the same key.

</details>

---

## ⚠️ Common Mistakes

| # | Mistake | Result | Fix |
|---|---------|--------|-----|
| 1 | Editing `bin/` or `obj/` | Changes vanish at the next build | Edit source files only |
| 2 | Editing `appsettings.json` when running in Development with an override present | "My setting does nothing" | Check `appsettings.Development.json` |
| 3 | Expecting `launchSettings.json` to affect production | Wrong assumptions about ports and environment | It only affects local `dotnet run` / IDE runs |
| 4 | Listing `.cs` files in the `.csproj` | Not needed in SDK-style projects | Files are included automatically |
| 5 | Deleting a folder and wondering why classes disappeared | The folder *is* the namespace group of those files | Only delete what you intend to remove |
| 6 | Creating every folder from an article "just in case" | Empty architecture noise | Create folders when you have real content |
| 7 | Recreating `WeatherForecast` samples in a new API | Confusing sample code with your own | Replace the sample as soon as your own code arrives (Topic 04) |

---

## 💡 Senior Developer Notes

- Keep a project structure that a new teammate can understand in one glance.
- Configuration belongs in configuration files, not in constants inside classes.
- The `.csproj` is code too: review it in code review — it decides the target framework and the dependencies.
- When something "does not change", first ask: *which file did I edit, and which file is actually loaded?*

---

## 🧠 Knowledge Check

### Question 1

Which file must you edit to add a NuGet package?

**Answer:** The `.csproj` file — usually indirectly, through `dotnet add package`, which edits it for you.

---

### Question 2

True or False: you must list every `.cs` file inside the `.csproj`.

**Answer:** False. SDK-style projects include all source files automatically.

---

### Question 3

You edit `appsettings.json` but only your machine should get the new value. Where does it belong?

**Answer:** `appsettings.Development.json`, the environment-specific file.

---

### Question 4

Which file decides the port your `dotnet run` uses?

**Answer:** `Properties/launchSettings.json`, in the profile's `applicationUrl`.

---

### Question 5

Is the structure shown here mandatory for every ASP.NET Core application?

**Answer:** No. It is a sensible convention for small APIs. ASP.NET Core itself does not require a `Controllers/` folder.

---

## ✅ Check Yourself

- [ ] I can name every important file in the project and its responsibility
- [ ] I can read my `.csproj` and explain the target framework and packages
- [ ] I know the difference between `appsettings.json` and `appsettings.Development.json`
- [ ] I know what `launchSettings.json` does — and that it is development-only
- [ ] I created `Models/` and `Services/` and the build still succeeded
- [ ] I know that project structure evolves and is not a fixed rule

---

## Summary

| Piece | Responsibility |
|-------|----------------|
| `Program.cs` | Startup: register services, build pipeline, run |
| `.csproj` | Framework, options, packages |
| `appsettings.json` | Base configuration |
| `appsettings.Development.json` | Development-only overrides |
| `launchSettings.json` | Local run: URL + environment |
| `Controllers/` | Endpoints |
| `Models/` | Types exchanged by the API |
| `Services/` | Logic behind the endpoints |
| `bin/`, `obj/` | Generated output — never edit |

---

**Next: [04 — Controllers and Routing](04-Controllers-and-Routing.md)**
