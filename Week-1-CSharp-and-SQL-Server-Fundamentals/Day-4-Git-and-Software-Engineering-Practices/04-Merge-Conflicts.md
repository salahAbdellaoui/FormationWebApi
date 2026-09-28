# 04 — Merge Conflicts

---

## First: Create the Situation

Do not start with the conflict markers. Start with the story.

**File:** `EmployeeService.cs`

| Developer | What they did |
|-----------|---------------|
| Developer A | Changed the **salary validation** rules on lines 40–45 |
| Developer B | Changed the **salary calculation** on the same lines 40–45 |

Both worked on their own branch. Both pushed. Both opened PRs.

Now Developer A merges `main` — and Git reports a **conflict**.

```text
Auto-merging EmployeeService.cs
CONFLICT (content): Merge conflict in EmployeeService.cs
Automatic merge failed; fix conflicts and then commit the result.
```

> 🤔 **Why did this happen?**
>
> Git can merge changes in *different* parts of a file automatically. When both branches changed the **same lines**, Git does not know which change you want — so it stops and asks you.

---

## 🔎 What the Conflict Looks Like

Open `EmployeeService.cs` in your editor:

```text
<<<<<<< HEAD
        if (salary < 0)
            throw new ArgumentException("Salary cannot be negative.");
        if (salary > 100000)
            throw new ArgumentException("Salary exceeds company limit.");
=======
        var bonus = salary * 0.1m;
        var total = salary + bonus;
        return Math.Round(total, 2);
>>>>>>> feature/employee-search
```

### What each marker means

| Marker | Meaning |
|--------|---------|
| `<<<<<<< HEAD` | Start of **your** changes — the branch you are on right now |
| `=======` | Separator between the two versions |
| `>>>>>>> feature/employee-search` | End of the **incoming** changes — the branch being merged in |

Everything between `<<<<<<<` and `=======` is **ours** (current branch).
Everything between `=======` and `>>>>>>>` is **theirs** (incoming branch).

> ⚠️ These markers are plain text. They are not code. The file will not compile until you remove them.

---

## 🛠️ The Conflict Resolution Workflow

```text
1. Do not panic
2. Read the conflict
3. Understand both changes
4. Decide the intended final code
5. Remove the conflict markers
6. Save the file
7. Test the result
8. Stage the resolved file
9. Complete the merge
```

In commands:

```bash
git status                          # see which files conflict
# open the file, resolve it in your editor
git diff                            # check your resolution
git add EmployeeService.cs          # stage the RESOLVED file
git commit                          # complete the merge
# or, if the platform started the merge for you, follow its prompt
```

> ⚠️ `git merge --abort` returns to the state before the merge. Use it when you want to start over safely — not as a way to avoid solving the problem.

### The most important rule

> **Resolving a conflict is not simply choosing "ours" or "theirs."**
>
> You must understand the business and technical intent of *both* changes, then write the final code that is correct.

---

## ❓ Classroom Questions

- 🔎 What would you check before resolving this conflict?
- ⚠️ What could happen if you blindly choose "ours"?
- 🧠 Is this a Git problem or an application problem? (Git *found* the problem. Understanding the code *solves* it.)
- 💭 Which of the two changes should survive in the example above?

---

## 🧪 Exercise 7 — Controlled Merge Conflict

You will create a real conflict on purpose. Work in pairs.

### Setup

```bash
git switch main
git switch -c conflict-exercise
```

Create a file `SalaryRules.md`:

```text
# Salary Rules

Minimum salary: 1000
Maximum salary: 5000
```

Commit it:

```bash
git add SalaryRules.md
git commit -m "Add salary rules document"
```

### Developer A (you)

```bash
git switch -c feature/salary-validation
```

Change the file to:

```text
# Salary Rules

Minimum salary: 1500
Maximum salary: 5000
```

```bash
git add SalaryRules.md
git commit -m "Raise minimum salary to 1500"
git switch main
git merge feature/salary-validation
```

### Developer B (your partner, or you simulating the second developer)

```bash
git switch main
git switch -c feature/salary-calculation
```

Change the file to:

```text
# Salary Rules

Minimum salary: 1000
Maximum salary: 8000
```

```bash
git add SalaryRules.md
git commit -m "Raise maximum salary to 8000"
git switch main
git merge feature/salary-calculation
```

### Now the conflict

Go back to Developer A's branch and merge:

```bash
git switch feature/salary-validation
git merge main
```

Git reports a conflict on the **minimum salary** line (both developers changed it).

> 🤔 **Which change should survive?**
>
> There is no Git answer. There is a *business* answer:
>
> - Did the company really raise the minimum to 1500? Then keep 1500.
> - Did the team agree the minimum stays 1000? Then keep 1000.
> - Did *both* decisions happen? Then the correct file may be:

```text
# Salary Rules

Minimum salary: 1500
Maximum salary: 8000
```

**That is the real skill:** Git shows you the disagreement. *You* decide the correct outcome using your understanding of the requirements.

### Finish

```bash
# after fixing the file
git add SalaryRules.md
git commit -m "Resolve salary rules conflict: keep new minimum and new maximum"
git log --oneline
```

---

## 🚫 Common Mistakes (Part 4)

| Mistake | Why it hurts | Better |
|---------|--------------|--------|
| Panicking at the word "conflict" | People click random buttons | Conflicts are normal — follow the workflow |
| Blindly choosing "ours" or "theirs" | One developer's work silently disappears | Understand both changes first |
| Leaving conflict markers in the file | Build fails in a confusing way | Search the file for `<<<<<<<` before committing |
| Resolving without testing | The file compiles but behaves wrongly | Run the tests after every resolution |
| Forgetting to `git add` the resolved file | The merge never completes | Stage the file, then commit |
| Avoiding conflicts for weeks | The longer you wait, the bigger the conflict | Pull and merge often |

---

## 💡 Senior Developer Notes

> Conflicts are cheaper when branches are **short-lived**. A branch that lives one day conflicts less than a branch that lives two weeks.

> When resolving a conflict, understand the **intent behind both changes** — not just the text.

> If you do not understand why both changes exist, **ask**. A two-minute conversation beats a wrong merge.

---

## ✅ Check Yourself

- [ ] I can explain the three conflict markers
- [ ] I know the 9-step resolution workflow
- [ ] I completed the controlled conflict exercise
- [ ] I can explain why "ours" is not always correct
- [ ] I know that resolving ends with `git add` + commit

**Next: [05 — Debugging and Problem Solving](05-Debugging-and-Problem-Solving.md)**
