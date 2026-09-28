# 03 — Create and Run Projects

---

## First: What Actually Happens When We Create a .NET Project?

Creating a project does **not** write your whole application. It creates a **folder with a small set of files**:

```text
A project = source code + configuration file (.csproj)
```

The SDK then uses those files to build and run the program.

```text
dotnet new   →  creates the files
dotnet build →  compiles the code
dotnet run   →  starts the program
```

> ⚠️ Do not assume you already know the terminal. Type the commands yourself, one at a time, and read what appears.

---

## Part 1 — Create a Console Project (CLI)

Open a terminal, go to the folder where you keep training work, then:

```bash
dotnet new console -n TrainingDemo
```

| Part | Meaning |
|------|---------|
| `dotnet new` | create something from a template |
| `console` | the template: a console application |
| `-n TrainingDemo` | the name of the new project |

Then enter the folder:

```bash
cd TrainingDemo
```

Then run it:

```bash
dotnet run
```

### What to expect

- The first run may take a short time (the SDK downloads/uses what it needs).
- The program runs and prints output. The default console template prints a short greeting — your exact text depends on the template version.

> 🤔 **Predict first:** before typing `dotnet run`, what do you think the program will print? Compare with the real result.

---

## Part 2 — Create a Project With Visual Studio

The same result, through the IDE:

1. Open Visual Studio → **Create a new project**.
2. Choose a C# **Console Application** template.
3. Select the target framework: **.NET 8** (if several appear, pick .NET 8).
4. Set the project name (for example `TrainingDemo`) and the location.
5. Create the project.
6. Run it with the run button (or the Debug menu).

> ⚠️ Visual Studio's screens change between versions. Understand the **steps** — name, template, framework, location — not a fixed picture of buttons.

**Verify the result:** does your new project folder contain a `.csproj` file and a `Program.cs`?

---

## Part 3 — Create a Project With VS Code

VS Code does **not** replace the .NET SDK. The SDK performs the creation, build, and run work. VS Code gives you an editor and a terminal around it.

```bash
dotnet new console -n TrainingDemo
cd TrainingDemo
code .
```

| Command | Meaning |
|---------|---------|
| `code .` | open the **current folder** in VS Code |

Then, inside VS Code:

- open the integrated terminal
- run `dotnet run`

> 🧠 **Key concept:** Visual Studio, VS Code, and the plain terminal all work with the **same project**. Nothing about the project is owned by one editor.

---

## Part 4 — Build, Run, Restore

Three commands, three jobs:

```text
Project
   ↓
Restore dependencies
   ↓
Build
   ↓
Run
```

| Command | What it does |
|---------|--------------|
| `dotnet restore` | downloads/prepares what the project depends on |
| `dotnet build` | compiles the code and reports errors |
| `dotnet run` | runs the application (and can perform needed steps automatically depending on the project's state) |

### 🧪 Exercise 4 — Build Pipeline

Run these one at a time in your project folder:

```bash
dotnet restore
dotnet build
dotnet run
```

**Answer after running:**

1. What did `restore` print? Did it download anything?
2. What did `build` print? Where did it say the output went?
3. What did `run` print? Was it different from `build`?

> The objective is **understanding**, not memorizing commands.

### Debug vs Release (basic level)

Builds have configurations. The two you will meet first:

| Configuration | Purpose |
|---------------|---------|
| **Debug** | Default while developing: easier to debug, checks enabled, usually slower |
| **Release** | Used for publishing/deploying: optimized, without debug extras |

You can choose the configuration in Visual Studio's toolbar, or on the command line:

```bash
dotnet build -c Release
dotnet build -c Debug
```

> 💡 **Senior Developer Note:** During the course, stay in **Debug** while developing. You will learn when Release matters later — do not spend time on it today.

---

## Part 5 — Debugging Basics (Introduction Only)

There is a difference between **running** and **debugging**:

| | Run | Debug |
|---|-----|-------|
| What happens | The program executes from start to finish | The program **pauses** where you place a breakpoint |
| You can see | Final output | Variable values, the current line, the path of execution |

Core ideas:

- **Breakpoint** — a marker that pauses the program on a specific line
- **Stepping** — move one line at a time (into or over method calls)
- **Variables** — inspect current values while paused
- **Call stack** — which method calls led to the current line

How to start:

- **Visual Studio:** click in the margin to set a breakpoint, then start with **Start Debugging** (commonly F5).
- **VS Code:** set a breakpoint in the editor gutter, then use **Run and Debug** with the C# tooling.

> ⚠️ This is only an introduction so the environment feels familiar. **Day 4** teaches debugging as a real professional problem-solving skill.

---

## Part 6 — Create a Minimal ASP.NET Core Web API

Your main training program is a **.NET 8 Web API**, so create one now.

In a terminal (outside your console project folder):

```bash
dotnet new webapi -n TrainingApi
cd TrainingApi
dotnet run
```

| Part | Meaning |
|------|---------|
| `dotnet new webapi` | the standard .NET template for a web API |
| `-n TrainingApi` | project name |

> ⚠️ Template names, options, and generated files can change between SDK versions. If `dotnet new webapi` reports an error, list the available templates with `dotnet new list` and use the web API template shown there.

### What the generated project gives you

```text
TrainingApi/
├── TrainingApi.csproj
├── Program.cs            ← application startup
├── appsettings.json      ← configuration
└── ...
```

Depending on the template and SDK version, the generated project may also include Swagger/OpenAPI tooling so you can explore the API in the browser. **Verify on your own machine** — check what `Program.cs` and the launch settings actually contain instead of assuming.

### 🧪 Exercise 5 — First Web API

1. Create the Web API project with the CLI.
2. Open the **same folder** in Visual Studio.
3. Open the **same folder** in VS Code.
4. Build it (`dotnet build` or the IDE build).
5. Run it (`dotnet run` or the IDE run).
6. Verify it starts — read the terminal output for the local address it listens on.
7. If your project includes a Swagger/OpenAPI page, open it in the browser.
8. Stop the application (usually `Ctrl+C` in the terminal).
9. Run it again from the terminal.

> 🎯 **The objective is confidence with the environment**, not understanding web APIs. Controllers, DTOs, and databases come in the main training days.

---

## 🧠 Activity — Same Project, Two Tools

```text
Visual Studio          OR          VS Code + .NET CLI
        \                          /
         \                        /
          ▼                      ▼
         One .NET project
```

Answer:

1. Did the project files change when you opened it with the other tool?
2. Which tool created the `.csproj`? (Neither — `dotnet new` did.)
3. Which tool compiled the code? (The SDK, whether invoked by an IDE button or the terminal.)

---

## 🚫 Common Mistakes (Part 3)

| Mistake | Why it hurts | Better |
|---------|--------------|--------|
| Running `dotnet` in the wrong folder | "No project found" errors | `cd` into the folder that contains the `.csproj` |
| Expecting VS Code to "contain" .NET | Confusion when commands fail outside it | The SDK does the work; any terminal can call it |
| Editing generated files without understanding them | Breaks the build in confusing ways | Know what each generated file is for (File 04) |
| Ignoring build errors and running anyway | Old output runs instead of your new code | Read the **first** error from `dotnet build` |
| Changing many things while the build fails | You cannot tell what fixed it | Change one thing, rebuild, check |

---

## ✅ Check Yourself

- [ ] I created a console project with `dotnet new console`
- [ ] I created a project with Visual Studio
- [ ] I opened a project in VS Code with `code .`
- [ ] I ran `restore`, `build`, and `run` and can explain each
- [ ] I can explain Debug vs Release in one sentence
- [ ] I created a Web API project and saw it start
- [ ] I can explain why VS Code does not replace the SDK

**Next: [04 — Project Structure and Configuration](04-Project-Structure-and-Configuration.md)**
