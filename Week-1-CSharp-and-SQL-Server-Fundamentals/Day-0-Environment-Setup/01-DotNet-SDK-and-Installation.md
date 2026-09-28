# 01 — .NET SDK and Installation

---

## Part 1 — What Is .NET?

> **.NET is a platform from Microsoft for building applications.**

With .NET you can build:

- console applications (text-based programs)
- web APIs and websites
- desktop applications
- and more

### What is C#?

> **C# is the programming language. .NET is the platform that runs it.**

```text
C#   = the language you write in        (the words)
.NET = the platform that runs your code  (the engine)
```

You write C# code → .NET builds it → .NET runs it.

---

## Part 2 — SDK vs Runtime vs Project vs Application

These four words confuse every beginner. Separate them now:

| Component | Main purpose | Who needs it? |
|-----------|--------------|---------------|
| **.NET SDK** | Create, build, run and develop applications | Developers |
| **.NET Runtime** | Run applications that target it | Machines that only *run* apps |
| **.NET Project** | Your source code + configuration | Developers |
| **.NET Application** | The built program that actually runs | End users |

### Simple analogy

```text
SDK      = a workshop with tools (saw, drill, measuring tape)
Runtime  = electricity — enough to USE what was built
Project  = the blueprint on the table
Application = the finished shelf someone can use
```

> 🤔 **Why do developers normally need the SDK?**
>
> Because you must **create and build** the project, not only run it. The SDK also contains the `dotnet` command you will use constantly.

**Rule of thumb:**

- You are **developing** → install the **SDK**
- A machine only **runs** your finished app → the **Runtime** can be enough

---

## Part 3 — Which .NET Version Are We Using?

This training program targets:

> **.NET 8**

All examples in this course use .NET 8. If your machine has a different SDK, your commands may behave differently — verify first, do not assume.

---

## Part 4 — Verify Before You Install

Open a terminal:

- **Windows:** search for "Terminal" or "PowerShell" or "Command Prompt"
- **macOS / Linux:** open the Terminal application

Then run:

```bash
dotnet --version
```

**If the version of .NET 8 appears** — the SDK is installed and working.

**If you see an error** such as `'dotnet' is not recognized` or `command not found` — the SDK is missing or not on your PATH (covered in the troubleshooting file).

### The three useful commands

```bash
dotnet --version      # which SDK does this terminal use right now?
dotnet --list-sdks    # which SDKs are installed?
dotnet --info         # full environment report
```

| Command | Use it when |
|---------|-------------|
| `dotnet --version` | Quick check: "is dotnet working?" |
| `dotnet --list-sdks` | You need to see *all* installed SDKs |
| `dotnet --info` | Something is wrong and you need details |

> ⚠️ Example output below is **only an example**. Your machine will show your own installed versions.

```text
Example output of dotnet --list-sdks

8.0.xxx [C:\Program Files\dotnet\sdk]
```

---

## Part 5 — Installing the .NET 8 SDK

If `dotnet --version` failed, install the SDK:

1. Go to the official Microsoft .NET download page (search for "Microsoft .NET download" — do not trust random mirror sites).
2. Download the **.NET 8 SDK** (not only the Runtime).
3. Run the installer and follow the screens.
   > ⚠️ Installer screens change over time. The exact buttons you see may differ from any screenshot — read the labels, not a picture.
4. **Close and reopen your terminal.** An already-open terminal usually does not see the new installation.
5. Verify again:

```bash
dotnet --version
dotnet --list-sdks
```

### Success looks like

- `dotnet --version` prints a version starting with `8.` (assuming .NET 8 is installed)
- `dotnet --list-sdks` lists at least one SDK
- No "command not found" error

> ⚠️ Do not invent or expect a specific patch number. Any valid 8.x SDK version is fine for this course.

---

## 🧪 Exercise 1 — Installation Verification

Every trainee runs all three commands and fills the checklist:

```bash
dotnet --version
dotnet --list-sdks
dotnet --info
```

```text
[ ] .NET SDK installed
[ ] .NET 8 SDK available
[ ] dotnet command works
[ ] Terminal recognizes dotnet
```

> 🤔 **Predict first:** before you run `dotnet --info`, what kind of information do you think it will show? Run it and compare your answer.

---

## 🧠 Activity 1 — Predict the Result

Discuss in pairs — what does each command do?

```bash
dotnet --version
dotnet --list-sdks
dotnet --list-runtimes
dotnet --info
```

Then run them. Did your prediction match?

---

## 🚫 Common Mistakes (Part 1)

| Mistake | Why it hurts | Better |
|---------|--------------|--------|
| Installing only the Runtime | You cannot create or build projects | Install the **SDK** |
| Forgetting to restart the terminal | `dotnet` still not found after a successful install | Close and reopen the terminal |
| Assuming the SDK is installed | Waste hours debugging later | Always verify with `dotnet --version` |
| Installing several SDKs without understanding why | Unclear which version builds your project | Check `dotnet --list-sdks` and know why each one exists |
| Downloading from unofficial sites | Broken or unsafe installers | Use the official Microsoft .NET download page |

> 💡 **Senior Developer Note:** Learn to read the terminal. It often tells you exactly what is wrong — the first line of an error is usually the real problem.

---

## ✅ Check Yourself

- [ ] I can explain .NET, C#, SDK, and Runtime in my own words
- [ ] I know why developers need the SDK and not only the Runtime
- [ ] I ran the three verification commands
- [ ] My checklist is complete

**Next: [02 — Visual Studio and VS Code](02-Visual-Studio-and-VS-Code.md)**
