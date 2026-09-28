# Day 0 — .NET Environment Setup

## Preparatory Session (before Day 1)

**Duration:** ~3 hours
**Level:** Beginner — no prior .NET experience required
**Target framework:** .NET 8
**Tools:** .NET SDK, Visual Studio, VS Code

---

## ⚠️ What Is This Day?

This is **not** Day 1. This is a preparation session.

Its purpose: every trainee can **independently** prepare a development machine, create a project, run it, inspect it, and fix common setup problems — **before** the real training starts.

```text
Understand → Install → Verify → Create → Run → Inspect
     → Configure → Troubleshoot → Practice
```

---

## 🎯 Learning Objectives

By the end of this session, you will be able to:

- Explain the difference between the .NET SDK and the .NET Runtime
- Verify a .NET installation with CLI commands
- Install and use the .NET 8 SDK
- Work with Visual Studio
- Work with VS Code
- Use the .NET CLI
- Create a basic .NET project
- Build and run a project
- Understand the basic project structure
- Understand the `.csproj` file
- Understand `Program.cs`
- Understand basic configuration files
- Understand basic environment concepts (Development vs Production)
- Identify common environment problems
- Open and work with the same project in Visual Studio **and** VS Code

---

## 🗺️ The Big Picture

Before installing anything, see how the pieces fit together:

```text
Developer
   │
   ├── Visual Studio
   │
   ├── VS Code
   │
   └── Terminal
          │
          ▼
      .NET SDK
          │
          ▼
      .NET Project
          │
          ▼
      Application
```

| Component | In simple words |
|-----------|-----------------|
| **Developer** | You |
| **Visual Studio / VS Code / Terminal** | The places where you write and run code |
| **.NET SDK** | The tools that create, build, and run .NET projects |
| **.NET Project** | Your source code + its configuration |
| **Application** | The running program |

> 💡 **Senior Developer Note:** The IDE is a tool. The .NET SDK is what enables the .NET development workflow. If the environment is not understood, debugging application code becomes much harder.

---

## 📅 Session Schedule

| Part | Topic | Duration |
|------|-------|----------|
| 1 | .NET SDK and Installation | ~35 min |
| 2 | Visual Studio and VS Code | ~35 min |
| 3 | ☕ Short Break | 10 min |
| 4 | Create and Run Projects | ~45 min |
| 5 | Project Structure and Configuration | ~40 min |
| 6 | Troubleshooting, Challenge & Checklist | ~35 min |

---

## 📚 Lesson Files

| File | Topic |
|------|-------|
| [01 — .NET SDK and Installation](01-DotNet-SDK-and-Installation.md) | What .NET is, SDK vs Runtime, .NET 8, verify, install |
| [02 — Visual Studio and VS Code](02-Visual-Studio-and-VS-Code.md) | Two tools, one project, extensions, exercises |
| [03 — Create and Run Projects](03-Create-and-Run-Projects.md) | CLI, Visual Studio, build/run, debugging basics, first Web API |
| [04 — Project Structure and Configuration](04-Project-Structure-and-Configuration.md) | `.csproj`, `Program.cs`, config, NuGet, `bin`/`obj`, `.gitignore` |
| [05 — Troubleshooting and Checklist](05-Troubleshooting-and-Environment-Checklist.md) | Common problems, detective challenge, knowledge check, readiness list |

Start with File 01.

---

## 🔗 Connection to Day 1

```text
Today (Day 0)
Development Environment
       ↓
.NET SDK
       ↓
Visual Studio / VS Code
       ↓
Project → Run

Tomorrow (Day 1 — C# Fundamentals)
       ↓
C# → Variables → Types → Methods → OOP
```

Tomorrow you stop preparing the environment and start writing real C# code.

---

**Start with [01 — .NET SDK and Installation](01-DotNet-SDK-and-Installation.md)**
