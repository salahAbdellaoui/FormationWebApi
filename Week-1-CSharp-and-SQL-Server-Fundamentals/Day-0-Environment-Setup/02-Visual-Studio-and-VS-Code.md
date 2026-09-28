# 02 — Visual Studio and VS Code

---

## The Big Idea First

You will use two tools in this course:

- **Visual Studio** — a full IDE (Integrated Development Environment)
- **Visual Studio Code (VS Code)** — a lightweight editor

> 🧠 **Important mental model:** a project is **not** "a Visual Studio project" or "a VS Code project". It is a **.NET project**. The tools are different ways of working with the same project.

---

## Part 1 — Visual Studio

**Visual Studio** is Microsoft's full IDE for .NET development. It brings many tools into one window:

| Capability | What it gives you |
|------------|-------------------|
| Project tooling | Create, open, and organize projects and solutions |
| Editor | Code writing with completion and refactorings |
| Solution Explorer | Navigate the project files |
| Debugger | Breakpoints, stepping, variable inspection |
| Build | Compile the project with one click or key |
| NuGet | Browse and manage packages |
| Integrated tools | Many .NET-specific tools built in |

### Installation (conceptual)

1. Download the Visual Studio **Installer** from the official Microsoft Visual Studio site.
2. In the installer, choose the workload for **.NET / ASP.NET Core web development**.
   > ⚠️ Workload names and layouts can change between Visual Studio versions. Read the installer options carefully; the exact text you see may differ.
   > If your screen does not match a description, choose the workload that mentions **.NET** and **ASP.NET / web** development.
3. Install, then restart Visual Studio when prompted.

> 💡 **Senior Developer Note:** A project file is part of the project. Understand what it contains — the IDE is only an editor for those files.

### After installation — know these areas

```text
Visual Studio
├── Start window        → open or create a project
├── Menu bar            → File, Build, Debug, ...
├── Solution Explorer   → see the project files
├── Editor              → write code
├── Output window       → see build messages
└── Debug tools         → breakpoints, variables, call stack
```

---

## Part 2 — Visual Studio Code

**VS Code** is a lightweight, fast editor from Microsoft. It becomes a .NET tool through **extensions** and the **integrated terminal**.

What developers use it for:

- editing code with completion
- running commands in the integrated terminal
- navigating project files
- debugging (with the appropriate tooling installed)
- working comfortably in terminal-driven workflows

### Extensions for this course

Search the VS Code Extensions view for:

```text
C#                 → core C# language support
C# Dev Kit         → project and solution tooling for .NET
```

> ⚠️ Extensions, names, and capabilities change over time. Install what the Extensions view shows as the current Microsoft C# tooling, and confirm your instructor's recommendation for this course.

---

## 🆚 Visual Studio vs VS Code

| Visual Studio | VS Code |
|---------------|---------|
| Full IDE | Lightweight editor |
| Integrated project tooling | Extension-based tooling |
| Rich debugging experience | Debugging through extensions/tooling |
| Many .NET tools integrated | Flexible and lightweight |
| Strong enterprise .NET workflow | Excellent for terminal-driven workflows |

> **Both can be used effectively for .NET development. The workflow is different.**

Neither tool is universally better. In this course you will use **both** so you can choose based on the task and your team.

---

## 🧪 Exercise 2 — Visual Studio Setup

1. Open Visual Studio.
2. Sign in only if your training setup requires it (not needed for the basic workflow).
3. Create a temporary project using any C# template to check that the tooling works — or simply open an existing training folder if one is provided.
4. Find the **Solution Explorer**. Where are the project files shown?
5. Find the **Output** window. What does it show after a build?
6. Close the project.

**Predict first:** where do you think the compiled program appears after a build? (Answer in File 04 — on `bin/`.)

---

## 🧪 Exercise 3 — VS Code Setup

1. Open VS Code.
2. Open the training workspace/folder (**File → Open Folder**).
3. Open the integrated terminal (**Terminal → New Terminal**).
4. Run:

```bash
dotnet --version
```

5. Confirm the terminal recognizes .NET.
6. Open the project folder in the Explorer view. Which files can you see?
7. Check the Extensions view: are the C# tooling extensions installed?

> 🤔 **Question:** VS Code printed the .NET version. Did VS Code provide .NET, or did the terminal use something else? (The terminal used the **SDK** installed on your machine. VS Code only gave you a terminal window.)

---

## 🧠 Activity — Visual Studio or VS Code?

For each task, which tool would you choose? Defend your answer.

1. You are new to .NET and want every tool visible in one window.
2. You want to quickly check a file while a terminal command is running.
3. You will debug a complex web application with breakpoints.
4. You are working on a small script inside a larger non-.NET project.

> There is no single correct tool — only a justified choice.

---

## 🚫 Common Mistakes (Part 2)

| Mistake | Why it hurts | Better |
|---------|--------------|--------|
| Opening the wrong folder in VS Code | Files and tooling "disappear" | Open the folder that contains the `.csproj` |
| Confusing project folder and solution folder | Commands run in the wrong place | Check where the project files actually live |
| Installing VS Code but no C# tooling | No completion, no debugging | Install the C# extensions |
| Reinstalling everything when tooling fails | Wasted time; problem may be the folder or the SDK | Check the actual error first (File 05) |
| Believing one tool is "the professional one" | Wrong tool for the task | Choose by task, not by fashion |

> 💡 **Senior Developer Note:** Do not blindly reinstall everything when something fails. First identify the actual error.

---

## ✅ Check Yourself

- [ ] I can explain the difference between Visual Studio and VS Code
- [ ] I opened Visual Studio and found Solution Explorer and Output
- [ ] I opened VS Code, opened the terminal, and ran `dotnet --version`
- [ ] I know that a .NET project belongs to neither tool specifically

**Next: [03 — Create and Run Projects](03-Create-and-Run-Projects.md)**
