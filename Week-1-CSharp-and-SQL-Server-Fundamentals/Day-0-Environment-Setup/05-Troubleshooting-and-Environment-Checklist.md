# 05 — Troubleshooting and Environment Checklist

---

## The Troubleshooting Mindset

> **When something fails: read first, act second.**

```text
Read the first meaningful error
        ↓
Identify which tool reported it (SDK? IDE? compiler?)
        ↓
Form a hypothesis
        ↓
Change ONE thing
        ↓
Try again
```

> 💡 **Senior Developer Note:** Do not blindly reinstall everything when a build fails. First identify the actual error.

---

## Problem 1 — `'dotnet' is not recognized`

**Windows:**

```text
'dotnet' is not recognized as an internal or external command
```

**macOS / Linux:**

```text
command not found: dotnet
```

### Possible causes (more than one can be true)

- the .NET **SDK** is not installed
- the SDK was installed **after** the terminal was opened (PATH not refreshed)
- the terminal/PATH environment has an issue
- only the Runtime was installed, not the SDK

### What to check

1. Did you install the **SDK** (not only the Runtime)?
2. Did you **close and reopen** the terminal after installing?
3. Does a *newly opened* terminal behave differently from your current one?

> ⚠️ Do not claim one cause is always responsible. Check them in order.

---

## Problem 2 — Wrong or Missing SDK

**Symptoms:** a project refuses to build; the error mentions a target framework the SDK cannot find.

### Inspect your machine

```bash
dotnet --list-sdks
dotnet --info
```

Compare what is installed with what the project needs in `.csproj`:

```xml
<TargetFramework>net8.0</TargetFramework>
```

| Finding | Action |
|---------|--------|
| No 8.x SDK listed | Install the .NET 8 SDK |
| SDK present, but terminal was open before install | Restart the terminal |
| Several SDKs listed | Normal — but know which one builds your project |

---

## Problem 3 — The Project Fails to Build

Teach yourself this order:

1. **Read the first meaningful error** — later errors are often consequences.
2. **Check the target framework** in `.csproj` against `dotnet --list-sdks`.
3. **Check missing packages** — was `dotnet restore` run? Is a package reference broken?
4. **Check project configuration** — does the `.csproj` XML still look valid?
5. **Do not randomly change many things.** One change → rebuild → read the result.

> 🤔 **Ask:** is this an SDK problem, a project problem, or a code problem? The error message usually tells you.

---

## Problem 4 — VS Code Has No .NET Tooling

**Symptoms:** no completion, no debugging option, no project view.

### Possible causes

- the required C# extension/tooling is **not installed**
- the extension is installed but **not enabled/loaded**
- the **wrong folder** was opened (folder without a `.csproj`)
- the **SDK is not installed correctly**, so tooling has nothing to work with

### Quick checks

```text
[ ] Extensions view: is the C# tooling installed and enabled?
[ ] Explorer: can you see the .csproj in the opened folder?
[ ] Integrated terminal: does dotnet --version work?
```

---

## Problem 5 — Visual Studio Does Not Show the Expected Project or Template

**Possible causes**

- the required **workload/components** were not selected during installation
- you are opening a folder instead of the project/solution
- the template list differs because of installed components

### What to do

1. Reopen the Visual Studio **Installer** and review which workloads are installed.
2. Confirm that a workload providing **.NET / ASP.NET Core** development is present.
3. Restart Visual Studio after changing components.

> ⚠️ Installer options differ between Visual Studio versions. Review the options on **your** machine rather than expecting a specific screenshot.

---

## 🕵️ Environment Detective Challenge

You are the developer responsible for fixing a colleague's machine.

**Report:**

```text
1. `dotnet` command not found
2. Wrong SDK installed (project needs .NET 8)
3. VS Code has no .NET tooling
4. Project targets a framework that is not installed
```

> **"Where do you start?"**

### Your task

For **each** symptom, write:

1. Which tool reports the problem? (terminal / SDK / VS Code / build)
2. What is your first check?
3. What is the likely fix?

Discuss in pairs, then compare with the class.

**Hint:** the four symptoms are connected. Fixing one may remove another.

---

## 🎯 Classroom Activities

### Activity 2 — Predict the Result

Before running, write what you think each command does. Then run and compare.

```bash
dotnet --version
dotnet --list-sdks
dotnet build
dotnet run
```

### Activity 3 — Fix the Machine

Use the detective challenge above. Role-play: one trainee plays the "broken machine" (answers only with what the screen shows), another plays the "support developer" (asks questions and decides the fix).

### Activity 4 — Visual Studio or VS Code?

For each situation, choose a tool and justify it:

1. Beginner wants all .NET tooling visible in one window
2. Quick terminal-driven check inside a larger folder
3. Complex debugging session with breakpoints and variable watches
4. Small edit while a long `dotnet build` runs elsewhere

> No single correct answer — a **justified** answer is the goal.

---

## 🧠 Knowledge Check

### Question 1

What is the difference between the .NET SDK and the .NET Runtime?

- A) They are the same
- B) The SDK creates and builds applications; the Runtime runs them
- C) The Runtime creates projects; the SDK only runs them
- D) The SDK is only for websites

**Answer:** B) The SDK creates and builds applications; the Runtime runs them.

---

### Question 2

Which command shows the installed SDKs?

- A) `dotnet --run`
- B) `dotnet --list-sdks`
- C) `dotnet build`
- D) `dotnet new`

**Answer:** B) `dotnet --list-sdks`.

---

### Question 3

What is the purpose of a `.csproj` file?

- A) It stores the compiled program
- B) It describes the project to the SDK (framework, output type, references)
- C) It is the same as `Program.cs`
- D) It stores user passwords

**Answer:** B) It describes the project to the SDK.

---

### Question 4

What is `Program.cs`?

- A) The build output folder
- B) The application entry point — where the program starts
- C) A package manager file
- D) The Git configuration

**Answer:** B) The application entry point.

---

### Question 5

Why do we have `bin` and `obj`?

- A) They contain source code
- B) They contain generated build output and intermediate files
- C) They contain configuration secrets
- D) They are required in Git

**Answer:** B) Generated build output and intermediate files.

---

### Question 6

Why should secrets not be committed?

- A) They make the build slower
- B) Everyone with access to the repository can see them
- C) Git cannot store text files
- D) Secrets belong in `bin/`

**Answer:** B) Everyone with access to the repository can see them.

---

### Question 7

What is the role of `appsettings.json`?

- A) It stores compiled code
- B) It stores application configuration settings
- C) It stores the C# compiler
- D) It replaces the SDK

**Answer:** B) It stores application configuration settings.

---

### Question 8

What is the difference between Visual Studio and VS Code?

- A) Only one of them can run .NET
- B) Visual Studio is a full IDE; VS Code is a lightweight editor with extensions
- C) VS Code includes the .NET SDK
- D) They cannot open the same project

**Answer:** B) Visual Studio is a full IDE; VS Code is a lightweight editor with extensions.

---

### Question 9

What should you check first when `dotnet` is not recognized?

- A) Reinstall Windows
- B) Whether the SDK is installed and the terminal was restarted after installation
- C) Delete the project
- D) Change the `.csproj`

**Answer:** B) Whether the SDK is installed and the terminal was restarted.

---

### Question 10

What does `dotnet new console` do?

- A) Runs an existing program
- B) Creates a new console project from a template
- C) Installs the SDK
- D) Publishes an application

**Answer:** B) Creates a new console project from a template.

---

### Question 11

A project builds, but the running program shows old behavior. What is most likely?

- A) The SDK is broken
- B) You are running an older build output instead of rebuilding
- C) VS Code is required
- D) The Runtime must be reinstalled

**Answer:** B) You are running an older build output instead of rebuilding — run `dotnet build` or `dotnet run` again after changes.

---

### Question 12

Which statement about `launchSettings.json` is correct?

- A) It configures Production
- B) It is a local development launch profile for running from the IDE
- C) It stores secrets safely
- D) It replaces `appsettings.json`

**Answer:** B) It is a local development launch profile — not a production configuration mechanism.

---

### Question 13

You added files and folders by mistake to Git (`bin/`, `obj/`). Adding them to `.gitignore` now will…

- A) delete them from Git history automatically
- B) only affect files Git has not tracked yet — already tracked files stay tracked
- C) delete them from your disk
- D) do nothing at all

**Answer:** B) `.gitignore` only affects files Git has not tracked yet.

---

### Question 14

Which command opens the current folder in VS Code?

- A) `dotnet open .`
- B) `code .`
- C) `dotnet run .`
- D) `code dotnet`

**Answer:** B) `code .`.

---

### Question 15

Your build reports an error. What is the best first step?

- A) Reinstall the SDK
- B) Change five settings at once
- C) Read the first meaningful error message
- D) Delete `Program.cs`

**Answer:** C) Read the first meaningful error message.

---

## ✅ Environment Readiness Checklist

Every trainee must satisfy this list before Day 1:

```text
[ ] .NET 8 SDK installed
[ ] dotnet command works
[ ] dotnet --info works
[ ] Visual Studio installed
[ ] Required .NET development tooling available
[ ] VS Code installed
[ ] Required .NET tooling/extensions available
[ ] Terminal works
[ ] Can create a .NET project
[ ] Can build a .NET project
[ ] Can run a .NET project
[ ] Understands .csproj
[ ] Understands Program.cs
[ ] Understands basic configuration
[ ] Can open the same project in Visual Studio
[ ] Can open the same project in VS Code
[ ] Can identify common setup problems
```

---

## 🎮 Final Practical Challenge — "Prepare Your Development Machine"

Work independently. Complete all ten steps:

```text
 1. Verify .NET SDK           → dotnet --version / --list-sdks
 2. Open Visual Studio
 3. Open VS Code
 4. Create a .NET project     → dotnet new console (or VS template)
 5. Build it                   → dotnet build
 6. Run it                     → dotnet run
 7. Inspect the project        → open .csproj, Program.cs, bin/, obj/
 8. Create a basic Web API     → dotnet new webapi
 9. Run the API                → dotnet run
10. Verify that it starts      → read the terminal output
```

Then the instructor asks each trainee:

> **"What is the role of each tool?"**

Expected idea: the SDK creates/builds/runs; Visual Studio and VS Code are different ways to work with the same project; configuration files describe the project's settings.

> **"If the project does not build, where would you start investigating?"**

Expected idea: read the first error, check the SDK (`dotnet --list-sdks`), check the target framework in `.csproj`, check restore/packages — one change at a time.

---

## 🔗 Next: Day 1 — C# Fundamentals

```text
Today (Day 0)
Development Environment
       ↓
.NET SDK
       ↓
Visual Studio / VS Code
       ↓
Project → Run

Tomorrow (Day 1)
       ↓
C#
       ↓
Variables → Types → Methods → OOP
```

Tomorrow you stop preparing the environment and start writing **real C# code**.

---

> **A prepared machine is the first professional habit. Understand your tools before you depend on them.**

**End of Day 0.**
