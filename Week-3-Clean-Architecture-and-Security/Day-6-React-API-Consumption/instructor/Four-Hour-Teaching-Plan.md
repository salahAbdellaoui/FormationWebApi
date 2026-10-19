# Day 6 — Four-Hour Teaching Plan

- **Week:** Week 3 — Clean Architecture and Security
- **Duration:** 4 hours (240 minutes)
- **Level:** Beginner
- **Format:** Instructor-led explanation, live demonstration, and student code-along

---

## Purpose

Students build a small React page that requests sample users from JSONPlaceholder. They begin with a title and local sample data, then add state, a request, and clear messages for each request result.

This is four hours of teaching and practice, not four hours of reading. Students type code, run the app, inspect the request, make a planned error, and explain what they built.

## Learning Objectives

By the end of the session, students will be able to:

1. Describe how `index.html`, `main.jsx`, and `App.jsx` work together.
2. Write basic JSX that displays a title and user cards.
3. Store a list and request status with `useState`.
4. Render an array with `map()` and give each card a stable `key`.
5. Explain an API request, response, HTTP GET, and JSON.
6. Use `fetch`, check `response.ok`, and read data with `response.json()`.
7. Use `useEffect` with `[]` to start the request when the page appears.
8. Display loading, error, empty-list, and success states.

## Prerequisites

- Node.js and npm installed. `node --version` and `npm --version` return version numbers.
- Basic HTML, CSS, JavaScript arrays, and React function-component syntax.
- An internet connection for the JSONPlaceholder request.
- No backend, API key, or database is needed.

## Instructor Preparation

Before class:

1. Open the Day 6 folder and run `npm install` if dependencies are not installed.
2. Confirm `npm run dev` starts the app.
3. Open `https://jsonplaceholder.typicode.com/users` in a browser.
4. Open `src/App.jsx`, `src/main.jsx`, and `src/index.css` in the editor.
5. Ask students to keep the browser, terminal, and editor visible.
6. Teach from the linked lessons in order. Students type each checkpoint; do not paste the final app at the start.

## Exact Session Schedule

| Time | Duration | Topic |
|---|---:|---|
| 00:00–00:45 | 45 min | React project foundation and rendering |
| 00:45–01:30 | 45 min | `useState`, arrays, and rendering lists |
| 01:30–02:15 | 45 min | HTTP basics and consuming a public API with `fetch` |
| 02:15–03:00 | 45 min | `useEffect`, loading, and error handling |
| 03:00–03:45 | 45 min | Guided practical workshop and student exercises |
| 03:45–04:00 | 15 min | Review, knowledge checks, and recap |
| **Total** | **240 min** | |

---

## Block 1 — React Project Foundation and Rendering

**Time:** 00:00–00:45

**Lesson:** [01 — React Foundation](../lessons/01-React-Foundation.md)

| Time | Instructor explains and demonstrates | Students do and checkpoint |
|---|---|---|
| 00:00–00:05 | State the goal: build a user page in small steps. Ask what students expect a browser app to do. | Open editor and terminal. **Checkpoint:** students can point to the Day 6 folder. |
| 00:05–00:12 | Run `npm run dev` and open the local URL. Explain that Vite serves the existing React app. | Start the server and open the page. **Checkpoint:** the current Users page appears. |
| 00:12–00:22 | Show `package.json`, `index.html`, `src/main.jsx`, `src/App.jsx`, and `src/index.css`. Trace the `root` element to `main.jsx`, then to `<App />`. | Follow the file path in the editor. **Checkpoint:** students find `App.jsx` and `main.jsx`. |
| 00:22–00:32 | In `App.jsx`, demonstrate a component that returns `<main><h1>Users</h1></main>`. Explain the one root `<main>` and the HTML-like JSX. | **Students write:** the title component, save, and confirm the browser updates. **Checkpoint:** the page shows `Users`. |
| 00:32–00:42 | Add a small local `users` array and render cards with `map()`. Say that the list is temporary local data; Lesson 2 explains `map()` and `key`. | **Students write:** two sample user records and one card per record. **Checkpoint:** two cards show names and emails. |
| 00:42–00:45 | Ask students to trace which file mounts `App`. Review the result before moving on. | **Checkpoint:** students name `main.jsx` as the entry point and `App.jsx` as the page component. |

**Ask:** “Where does the browser find the element React fills?”

**Expected answer:** The `<div id="root">` in `index.html`.

**Expected result:** A Vite page with a Users heading and two cards built from local sample data.

---

## Block 2 — `useState`, Arrays, and Rendering Lists

**Time:** 00:45–01:30

**Lesson:** [02 — useState and Rendering Lists](../lessons/02-useState-and-Rendering.md)

| Time | Instructor explains and demonstrates | Students do and checkpoint |
|---|---|---|
| 00:45–00:52 | Explain state as data a component remembers while it is on the page. Contrast it with a fixed label in JSX. | Identify the `users` array and the displayed cards. **Checkpoint:** students can point to the data and its output. |
| 00:52–01:02 | Demonstrate `const [users, setUsers] = useState(sampleUsers)`. Explain the current value, setter, and initial value. | **Students write:** move the sample list into state. **Checkpoint:** the same cards still appear. |
| 01:02–01:15 | Demonstrate `users.map(...)`. Explain that each record creates one card and `key={user.id}` gives React a stable identity for that card. | Add a third sample user with a unique `id`. **Checkpoint:** three cards appear without a missing-key warning. |
| 01:15–01:25 | Explain that local data is written in the file, while API data arrives from another application. Show `setUsers(data)` as the way to replace the list later. | **Students write:** change a sample email and add a record. **Checkpoint:** the changed values appear after save. |
| 01:25–01:30 | Ask what the setter does and why a key is used. | **Checkpoint:** students explain that the setter updates state and each list item needs a stable key. |

**Ask:** “What should the app use as the key if every user has an `id`?”

**Expected answer:** `user.id`, because it identifies that user and stays the same when the list changes.

**Expected result:** A state-backed local array renders one card for each sample user.

---

## Block 3 — HTTP Basics and `fetch`

**Time:** 01:30–02:15

**Lesson:** [03 — Consuming an API](../lessons/03-Consuming-an-API.md)

| Time | Instructor explains and demonstrates | Students do and checkpoint |
|---|---|---|
| 01:30–01:38 | Define API, request, response, endpoint, and JSON using the user-list example. Explain GET as asking to read data. | Say which part is the request and which part is the response. **Checkpoint:** students can identify both. |
| 01:38–01:46 | Open `https://jsonplaceholder.typicode.com/users`. Show that the response is a JSON array of sample user objects. | Open the same endpoint. **Checkpoint:** students find `name`, `email`, and `address.city` in a record. |
| 01:46–01:56 | Demonstrate a small `fetch` request in the browser console. Explain that GET is the default request type. | **Students type:** the short request from Lesson 3 and inspect the returned array. **Checkpoint:** the console shows user records. |
| 01:56–02:08 | Add `response.ok`, `response.json()`, and then `setUsers(data)` to the explanation. Explain why HTTP errors need an explicit status check. | Trace each line aloud. **Checkpoint:** students explain why the JSON must be read before the app uses its users. |
| 02:08–02:15 | Open Developer Tools → Network, reload, and select the `/users` request. | **Students verify:** a GET request appears and its response contains users. |

**Ask:** “Does `fetch` reject automatically when a server responds with 404?”

**Expected answer:** No. Check `response.ok` and handle a non-success response.

**Expected result:** Students can inspect the endpoint, explain its JSON response, and follow a GET request in the Network panel.

---

## Block 4 — `useEffect`, Loading, and Error Handling

**Time:** 02:15–03:00

**Lesson:** [04 — useEffect and Request States](../lessons/04-useEffect-and-Request-States.md)

| Time | Instructor explains and demonstrates | Students do and checkpoint |
|---|---|---|
| 02:15–02:22 | Explain that a component renders again when state changes. Show why putting `fetch` directly in the component body can start another request every render. | Point to the component body and the effect block. **Checkpoint:** students state where the request belongs. |
| 02:22–02:31 | Introduce `useEffect` as code that runs after rendering. Add the effect around the request. | **Students write:** the effect wrapper and move their request into it. **Checkpoint:** the `/users` request appears after page load; the next section explains why development may show it twice. |
| 02:31–02:38 | Explain the empty dependency array `[]`. Show `<StrictMode>` in `main.jsx` and explain why development may show a repeated request. | **Checkpoint:** students say `[]` means no changing values are watched and understand the development repeat. |
| 02:38–02:51 | Add `loading`, `error`, `catch()`, and `finally()`. Demonstrate the `response.ok` check. | **Students write:** request states and handlers. **Checkpoint:** successful data replaces loading; a failed request shows the error. |
| 02:51–02:58 | Demonstrate the early loading and error returns. Explain the order: loading first, error next, users after success. | **Students write:** the two status messages. **Checkpoint:** the success case still displays cards. |
| 02:58–03:00 | Ask students to explain why `finally()` runs. | **Checkpoint:** students answer that it ends loading after success or failure. |

**Ask:** “Why does the app check `response.ok` before calling `response.json()`?”

**Expected answer:** `fetch` can return a 404 response without rejecting, so the app must turn a bad HTTP status into an error.

**Expected result:** The app loads users after the component appears and shows a loading or error message when needed.

---

## Block 5 — Guided Workshop and Student Exercises

**Time:** 03:00–03:45

**Lesson:** [05 — Guided Practical Workshop](../lessons/05-Guided-Practical-Workshop.md)

**Exercise:** [Improve the User List](../exercises/Exercise.md)

| Time | Instructor explains and demonstrates | Students do and checkpoint |
|---|---|---|
| 03:00–03:05 | State the final acceptance criteria. Ask students to keep the request working while adding the view states. | Read the checklist. **Checkpoint:** students can name the success, loading, error, and empty states. |
| 03:05–03:15 | Walk through the request and three state values in the workshop. | **Students write:** the user, loading, and error state plus the effect. **Checkpoint:** the Network panel shows `/users`. |
| 03:15–03:23 | Demonstrate loading and error branches. | **Students write:** the status messages. **Checkpoint:** loading ends and errors are visible. |
| 03:23–03:32 | Add user count, empty message, and cards. Explain that the count comes from `users.length`. | **Students write:** the count and conditional list. **Checkpoint:** the page shows 10 users, a count of 10, and each user's city. |
| 03:32–03:38 | Demonstrate the simple CSS. Avoid styling detours. | **Students write:** or adjust the card and count styles. **Checkpoint:** text is readable and cards are distinct. |
| 03:38–03:43 | Demonstrate temporary empty and error tests, then restore the working endpoint and `setUsers(data)`. | **Students test:** empty state and failed HTTP request. **Checkpoint:** both messages appear and the success case works again. |
| 03:43–03:45 | Ask students to compare their page to the checklist. | **Checkpoint:** app meets the final acceptance criteria. |

**Expected result:** A styled page shows the user count and user cards. Loading, error, and empty states have been checked.

---

## Block 6 — Review and Knowledge Checks

**Time:** 03:45–04:00

**Material:** [Review Questions](../exercises/Review-Questions.md)

| Time | Instructor explains and demonstrates | Students do and checkpoint |
|---|---|---|
| 03:45–03:50 | Recap the path: request → response → JSON → state → cards. | Students trace one user's name from the API response to the screen. |
| 03:50–03:56 | Ask six short questions from the review sheet. Use the instructor answer key after students respond. | Students answer individually, then compare with a partner. |
| 03:56–04:00 | Review the completion checklist and ask each student for one thing they can explain. | **Checkpoint:** each student can describe the request and one request state. |

**Expected result:** Students can explain the basic request flow and identify the purpose of each state.

---

## Common Beginner Mistakes

| Mistake | Why it happens | Instructor explanation |
|---|---|---|
| Editing `main.jsx` when changing the page | The entry point and page component look related. | `main.jsx` starts React and renders `<App />`; page content belongs in `App.jsx`. |
| Writing `fetch()` in the component body | The body looks like a natural place to put page work. | React runs the body again after state changes. Use `useEffect` for this request. |
| Calling `users.push(...)` | It looks like an ordinary JavaScript array update. | React needs the setter to know the state changed. Use `setUsers(...)`. |
| Leaving out `key` in `map()` | The cards look correct without it. | React needs a stable identifier to match list items between renders. Use `user.id`. |
| Assuming 404 enters `catch()` | Many expect every HTTP error to reject. | `fetch` rejects for network errors, not ordinary HTTP status codes. Check `response.ok`. |
| Forgetting `response.json()` | The response object is mistaken for the user list. | The response must be read as JSON before it becomes the array used by the app. |
| Leaving loading set to `true` | The success path works but the status state is forgotten. | `finally()` runs after success or failure and sets loading to false. |
| Seeing two requests in development | Strict Mode is unfamiliar. | `main.jsx` uses Strict Mode; effects may run again in development. This helps expose unsafe effects. |
| Showing a blank page for an empty array | `map()` returns no cards. | Add an explicit `users.length === 0` message. |

## Final Knowledge Check

Ask students to answer without looking at the code:

1. What does the app request, and from which endpoint?
2. Why does the app call `response.json()`?
3. Why does it check `response.ok`?
4. What does `useState` remember in this app?
5. Why is `fetch` inside `useEffect`?
6. What does the empty dependency array mean here?
7. What does `finally()` do?
8. Why does each card use `key={user.id}`?

Use the [review questions and answer key](../exercises/Review-Questions.md) after students have answered.

## Instructor Completion Checklist

- [ ] Students started the existing Vite app.
- [ ] Students edited `App.jsx` and rendered local sample cards.
- [ ] Students stored and rendered an array with `useState` and `map()`.
- [ ] Students identified a stable `key`.
- [ ] Students inspected the JSONPlaceholder endpoint and its response.
- [ ] Students explained `response.ok` and `response.json()`.
- [ ] Students placed the request in `useEffect` with `[]`.
- [ ] Students observed loading, success, error, and empty states.
- [ ] Students completed a small independent change in the exercise.
- [ ] Students can describe the request-to-screen flow in simple words.
