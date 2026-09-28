# Day 4 — Git & Software Engineering Practices

## Professional Training Course (Week 1)

**Duration:** 4 hours
**Level:** Beginner — Building on Days 1-3
**Prerequisites:** Day 1 (C#), Day 2 (Advanced C#), Day 3 (SQL Server)
**Tools:** Git, GitHub or GitLab (or any Git server)

---

## 🎯 Day 4 Learning Objectives

By the end of this session, you will be able to:

- Understand the purpose of Git and why teams need it
- Understand the Git working model (working directory → staging → repository → remote)
- Use the basic Git workflow: status, add, commit, log, diff, push, pull, fetch
- Create and use branches
- Write meaningful commits
- Understand merging and what a merge commit is
- Understand the purpose of Pull Requests
- Participate in a code review
- Understand and resolve merge conflicts
- Approach debugging systematically
- Communicate technical problems clearly
- Follow professional development practices
- Avoid committing secrets
- Work as part of a development team

---

## Training Schedule

| Part | Topic | Duration |
|------|-------|----------|
| 1 | Git Workflow & Commits | ~55 min |
| 2 | Branching and Merging | ~50 min |
| 3 | ☕ Short Break | 10 min |
| 4 | Pull Requests & Code Review | ~50 min |
| 5 | Merge Conflicts | ~35 min |
| 6 | Debugging & Problem Solving | ~40 min |
| 7 | Real-World Practices & Team Simulation | ~40 min |

---

## 🗺️ How Today Connects

Days 1 to 3 taught you how to **write code and store data**:

```text
Day 1: C# Fundamentals        → write code
Day 2: Advanced C#            → write better code
Day 3: SQL Server             → store and query data
Day 4: Git + team practices   → work with other developers safely
```

Knowing how to write code is only one part of being a professional developer.

A developer also needs to know how to:

- collaborate with other developers
- track changes over time
- review code before it reaches production
- solve conflicts between changes
- debug problems in a systematic way
- communicate clearly
- work safely inside a team

That is today's topic.

---

## 🧑‍🤝‍🧑 Meet the Team

Throughout the lesson you will follow a small team working on an **Employee Management API**:

```text
Developer A   →  you
Developer B   →  your colleague
Developer C   →  another colleague
Team Lead     →  assigns tasks
Reviewer      →  reviews Pull Requests
```

Their project:

```text
Employee Management API

Features:
- Employees
- Departments
- Positions
- Authentication
```

---

## 📖 Our Story Today

The whole lesson follows one realistic workflow:

```text
Task Assigned → Create Branch → Write Code → Test → Commit → Push
      → Pull Request → Code Review → Changes Requested → Fix
      → Merge → Another Developer Pulls Changes
```

Then a problem appears:

```text
Two Developers Changed the Same Code
             ↓
        Merge Conflict
             ↓
       Investigate
             ↓
     Resolve Conflict
             ↓
           Test
             ↓
          Merge
```

---

## 📚 Lesson Files

| File | Topic |
|------|-------|
| [01 — Git Workflow](01-Git-Workflow.md) | Why Git exists, mental model, daily commands, commits |
| [02 — Branching and Merging](02-Branching-and-Merging.md) | Branches, naming, merge, merge vs rebase |
| [03 — Pull Requests and Code Review](03-Pull-Requests-and-Code-Review.md) | PR lifecycle, review mindset, role-play |
| [04 — Merge Conflicts](04-Merge-Conflicts.md) | Conflict markers, resolution workflow, exercise |
| [05 — Debugging and Problem Solving](05-Debugging-and-Problem-Solving.md) | Debugging method, checklist, challenges |
| [06 — Real-World Practices](06-Real-World-Development-Practices.md) | Tickets, Definition of Done, secrets, team simulation, knowledge check |

Start with File 01.

---

## 🔜 Preview: Day 5 — Advanced SQL

Tomorrow you go deeper into SQL Server:

- complex queries
- views
- stored procedures
- transactions
- indexes
- query optimization
- database project

Today's work connects to it: your team will branch, review, and merge the code that talks to the database you designed on Day 3.

---

**Start with [01 — Git Workflow](01-Git-Workflow.md)**
