# Lesson 4 — useEffect and Request States

**Session block:** 45 minutes

**Goal:** Start the API request after rendering and show what is happening to the user.

## Why Use `useEffect`?

React runs a component function when it needs to draw the page. If `fetch` is written directly in that function, it can start again every time the component renders. Updating state causes another render, so the app could keep starting requests.

`useEffect` runs code after React renders the component. It is the right place for this beginner API request.

## 1. Track the Request State

At the top of `src/App.jsx`, import both hooks:

```jsx
import { useEffect, useState } from "react";
```

Inside `App`, add state for the users and the two request messages:

```jsx
const [users, setUsers] = useState([]);
const [loading, setLoading] = useState(true);
const [error, setError] = useState("");
```

The user list starts empty. Loading starts as `true` because the request has not finished. Error starts as an empty string because there is no error yet.

## 2. Start the Request in the Effect

Use this request inside `useEffect`:

```jsx
useEffect(() => {
  // Request users after this component appears on the page.
  fetch("https://jsonplaceholder.typicode.com/users")
    .then((response) => {
      if (!response.ok) {
        throw new Error("The server could not load users.");
      }

      return response.json();
    })
    .then((data) => {
      setUsers(data);
    })
    .catch(() => {
      setError("Unable to load users. Please check your connection and try again.");
    })
    .finally(() => {
      setLoading(false);
    });
}, []);
```

The empty dependency array `[]` means there are no changing values for React to watch. React runs the effect when this component appears. The project uses `<StrictMode>` in development, so React may run the effect more than once while checking for problems. This is normal.

Follow the request in order:

1. `fetch` asks for the users.
2. `response.ok` checks the HTTP result. A bad status throws an error because `fetch` does not do this by itself.
3. `response.json()` reads the JSON response as JavaScript data.
4. `setUsers(data)` saves that list in state.
5. `catch()` saves a friendly error message if the request fails.
6. `finally()` runs after success or failure and stops the loading message.

## 3. Display Loading, Error, and Success

Before the normal user-list return in `App`, add these checks:

```jsx
if (loading) {
  return (
    <main>
      <h1>Users</h1>
      <p aria-live="polite">Loading users...</p>
    </main>
  );
}

if (error) {
  return (
    <main>
      <h1>Users</h1>
      <p role="alert">{error}</p>
    </main>
  );
}
```

React checks loading first. While the request is running, the user sees `Loading users...`. When the request finishes, `finally()` sets loading to false. If there is an error, the next check displays it. Otherwise, the app displays the users.

## Student Checkpoints

1. Reload the page. **Expected:** loading appears briefly, then user cards appear.
2. Change the endpoint temporarily to `https://jsonplaceholder.typicode.com/no-such-route`. **Expected:** the error message appears after the 404 response. Restore the `/users` endpoint afterward.
3. Find `[]` at the end of the effect. **Expected:** explain that it means the effect watches no changing values and starts when the component appears.

## Remember

- `catch()` handles a rejected request or the error thrown after a bad HTTP status.
- `finally()` ends loading whether the request succeeds or fails.
- React Strict Mode can make an effect run more than once in development.
- Do not add request cancellation or more hooks in this beginner lesson.

Continue to [Lesson 5 — Guided Practical Workshop](05-Guided-Practical-Workshop.md).
