# 04 — Project Structure and Configuration

---

## 🛑 Stop and Inspect

You created projects. Now **look at what you actually made**.

After `dotnet new console -n TrainingDemo`:

```text
TrainingDemo/
├── TrainingDemo.csproj
├── Program.cs
└── obj/
```

Depending on the project type and SDK version, other files and folders may appear (for example `bin/`, or extra configuration files in a Web API project).

> ⚠️ There is no single structure that every .NET project always has. Inspect *your* project instead of memorizing an image.

---

## 📄 Program.cs — Where the Application Starts

`Program.cs` is the **entry point**: the place where your application begins running.

Modern .NET console projects use **top-level statements** — you can write code from the first line, without the older ceremony of classes and `Main` methods.

A very small example:

```csharp
Console.WriteLine("Hello from TrainingDemo");
Console.WriteLine("Environment is ready");
```

That is enough for today.

> ⚠️ Day 1 teaches C# fundamentals — variables, types, methods, classes. Do **not** try to learn C# in this session; only understand that `Program.cs` is where the program starts.

**Try this:** change the text, save, run `dotnet run` again. What changed and why?

---

## 📄 The .csproj File — The Project's Identity

The `.csproj` file is an XML file that describes the project to the SDK: what it is and what it needs.

A small example:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

### The important parts

| Element | What it means |
|---------|---------------|
| `Sdk="Microsoft.NET.Sdk"` | Which SDK builds this project |
| `<OutputType>Exe</OutputType>` | This project produces a runnable program |
| `<TargetFramework>net8.0</TargetFramework>` | Built for .NET 8 |
| `<ImplicitUsings>enable</ImplicitUsings>` | Some common namespaces are available without typing them |
| `<Nullable>enable</Nullable>` | The compiler helps you catch null-reference problems |

For a web project you would also see a **web** SDK reference — check your own `TrainingApi.csproj` and compare.

> 💡 **Senior Developer Note:** The `.csproj` is part of the project — it is not "IDE settings". If it changes, the build changes. Read it before you edit it.

> 🤔 **Question:** If `<TargetFramework>` says `net8.0` but your machine only has a different SDK installed, what might happen? (The build may fail — one reason to run `dotnet --list-sdks` when troubleshooting.)

---

## 📦 NuGet Packages — Where Extra Tools Come From

> **NuGet is the package ecosystem used by .NET projects.**

Instead of writing everything yourself, you add a package that provides a feature.

```bash
dotnet add package <PackageName>
```

> ⚠️ `<PackageName>` is a placeholder. In real work you add the specific package your task needs — today you do **not** need any extra package. Advanced package management is not part of this session.

What matters now:

- adding a package **changes your project file** (a reference appears in the `.csproj`)
- the packages are downloaded during **restore**
- you can see package references by opening the `.csproj`

---

## ⚙️ Configuration Basics

Applications need settings. The pattern:

```text
Application
     ↓
Configuration
     ↓
Settings
```

In ASP.NET Core projects you will commonly see:

```text
appsettings.json
appsettings.Development.json
```

Their general purpose: store **settings** for the application in a readable place.

A simple example (always inside a fenced block, so Markdown does not break):

```json
{
  "ApplicationName": "Training API",
  "Environment": "Development"
}
```

> ⚠️ Advanced configuration providers (environment variables, key vaults, options pattern) are **not** taught today. You will meet them in the main training.

---

## 🔐 Configuration vs Secrets

> **Configuration values and secrets are not the same thing.**

| Configuration | Secret |
|---------------|--------|
| Application name, feature switches, UI text | Passwords, API keys, connection-string credentials |
| Safe to commit | **Never** commit |

**Do NOT** put real passwords, API keys, or connection-string credentials in training files.

Sensitive values belong **outside** the repository — for example:

- environment variables on your machine
- a local secrets store (the .NET user-secrets approach)
- your platform's secret management

This is enough for today. This session is not a security course — but the rule is absolute:

> ⚠️ **No real secrets in source control. Ever.**

---

## 🌍 Environments

Applications often run in different places:

```text
Development   → your machine, while you build
Testing       → a shared test area
Production    → the real system users depend on
```

Why does this matter? The **same application** may need different settings in each environment:

- Development: talk to a local database
- Production: talk to the real database, real logging, real limits

Conceptually, configuration can change per environment — you already saw the file name `appsettings.Development.json` ("Development" is the environment part).

> ⚠️ Keep this conceptual. Deployment and environment configuration are advanced topics — not today.

---

## 📄 launchSettings.json — Handle With Care

In ASP.NET Core development projects you may find a `launchSettings.json` file. It can contain **local development launch profiles** — for example which URL the app uses when you press Run inside the IDE.

> ⚠️ **`launchSettings.json` is a local development convenience, not a production configuration mechanism.** Settings there apply to your local run from the IDE — do not assume they reach Production.

---

## 📁 bin and obj — Generated Files

```text
bin/   → build output (the compiled program)
obj/   → intermediate build files
```

Both are **generated** by the SDK. You do not edit them by hand.

- They are recreated by `dotnet build`.
- They should normally **not** be committed to Git.
- Deleting them is usually safe (the next build recreates them) — but understand *why* before deleting anything.

> ❓ **Activity:** run `dotnet build`, then look inside `bin/`. Where is the compiled program?

---

## 🧹 .gitignore — Keep Generated Files Out of Git

A `.gitignore` file tells Git which files **not to track**.

A .NET project typically ignores generated/local files. Example:

```gitignore
bin/
obj/
.vs/
*.user
```

| Entry | Meaning |
|-------|---------|
| `bin/`, `obj/` | build output, regenerated on every build |
| `.vs/` | Visual Studio local settings |
| `*.user` | per-developer settings |

> ⚠️ This example is a starting point, not a complete guarantee for every project. Adjust `.gitignore` to what **your** project actually generates.

### The critical rule

> **`.gitignore` does not remove files that are already tracked.**

It only affects files Git has never tracked. A file already committed stays in history until it is explicitly removed from tracking — and history is a separate problem.

---

## 🧪 Exercise 6 — Project Inspection

Open your **Web API** project and answer:

```text
[ ] Where is the project file?           (.csproj)
[ ] Where is the application entry point? (Program.cs)
[ ] Where are build outputs?             (bin/)
[ ] Where are intermediate files?        (obj/)
[ ] Where is configuration?              (appsettings.json)
[ ] Where would packages be referenced?  (inside the .csproj)
```

For each answer: **open the file/folder and look inside**. Do not answer from memory.

---

## 🧠 Activity 1 — What Is This?

Show these to a partner and explain each one in one sentence:

```text
.csproj
Program.cs
appsettings.json
bin/
obj/
```

---

## 🚫 Common Mistakes (Part 4)

| Mistake | Why it hurts | Better |
|---------|--------------|--------|
| Editing `bin/` or `obj/` by hand | Changes are wiped on the next build | Never edit generated files |
| Deleting `bin`/`obj` without knowing why | Confusion when something "disappears" | They are generated — build recreates them |
| Committing `bin/` and `obj/` | Repository bloat, constant conflicts | `.gitignore` them from the start |
| Secrets in `appsettings.json` | Credentials leak with the repository | Keep secrets out of the repository |
| Treating `launchSettings.json` as production config | Wrong assumptions about real deployments | It is a local development convenience |
| Changing many configuration values blindly | You no longer know which change did what | Change one value, run, observe |
| Confusing project folder and solution folder | Commands run in the wrong directory | Check where the `.csproj` lives |

---

## ✅ Check Yourself

- [ ] I opened `.csproj` and can explain its five key elements
- [ ] I know where `Program.cs` starts the application
- [ ] I can explain the difference between configuration and secrets
- [ ] I know what `bin/` and `obj/` are for
- [ ] I can explain what `.gitignore` does — and what it does **not** do
- [ ] I completed the project inspection exercise

**Next: [05 — Troubleshooting and Checklist](05-Troubleshooting-and-Environment-Checklist.md)**
