# Day 6 — React API Consumption

Build a small React app that requests users from JSONPlaceholder. This is a teacher-led, four-hour code-along for beginners.

**Duration:** 4 hours (240 minutes)

**Level:** Beginner

**Tools:** Node.js, npm, a code editor, and a web browser

## Learning Goals

By the end of the session, students will be able to:

- Explain how a browser app requests data from an API.
- Use `useState` to store a list and request status.
- Use `useEffect` to start a request when a component appears.
- Use `fetch` to read JSON and check for request errors.
- Display users, loading and error messages, an empty-list message, and a user count.

## Prerequisites

- Node.js and npm are installed. `node --version` and `npm --version` show version numbers.
- Students know basic HTML, CSS, JavaScript arrays, and React components.
- An internet connection is available for JSONPlaceholder.

## Start the App

From the repository root, run:

```bash
cd Week-3-Clean-Architecture-and-Security/Day-6-React-API-Consumption
npm install
npm run dev
```

Open the local address shown in the terminal. Keep the server running during the session. Stop it with `Ctrl + C`.

## Four-Hour Session

The instructor-led schedule totals exactly 240 minutes. Read the [Four-Hour Teaching Plan](instructor/Four-Hour-Teaching-Plan.md) for timings, instructor prompts, student checkpoints, and common mistakes.

| Time | Topic | Material |
|---|---|---|
| 00:00–00:45 | React project foundation and rendering | [Lesson 1 — React Foundation](lessons/01-React-Foundation.md) |
| 00:45–01:30 | `useState`, arrays, and rendering lists | [Lesson 2 — useState and Rendering](lessons/02-useState-and-Rendering.md) |
| 01:30–02:15 | HTTP basics and consuming an API with `fetch` | [Lesson 3 — Consuming an API](lessons/03-Consuming-an-API.md) |
| 02:15–03:00 | `useEffect`, loading, and error handling | [Lesson 4 — useEffect and Request States](lessons/04-useEffect-and-Request-States.md) |
| 03:00–03:45 | Guided workshop and student exercises | [Lesson 5 — Guided Practical Workshop](lessons/05-Guided-Practical-Workshop.md) |
| 03:45–04:00 | Review and knowledge checks | [Review Questions](exercises/Review-Questions.md) |

## Lesson Sequence

1. [React Foundation](lessons/01-React-Foundation.md) — run Vite, trace rendering, and build cards with local data.
2. [useState and Rendering Lists](lessons/02-useState-and-Rendering.md) — store an array and render it with `map()`.
3. [Consuming an API](lessons/03-Consuming-an-API.md) — understand HTTP GET, JSON, and `fetch`.
4. [useEffect and Request States](lessons/04-useEffect-and-Request-States.md) — load users and show request states.
5. [Guided Practical Workshop](lessons/05-Guided-Practical-Workshop.md) — combine the steps into the finished app.

## Student Practice

- [User List Exercise](exercises/Exercise.md)
- [Review Questions and Instructor Answer Key](exercises/Review-Questions.md)

## Finished App

The app requests `https://jsonplaceholder.typicode.com/users`. It displays each user's name, email, and city, with loading, error, and empty-list messages. It also displays the number of users. React Strict Mode may start the request effect more than once during development; this is normal.

To create a production build, run this from the Day 6 folder:

```bash
npm run build
```

The numbered files at the folder root are short links to the expanded lessons. The full teaching sequence lives in `lessons/`.
