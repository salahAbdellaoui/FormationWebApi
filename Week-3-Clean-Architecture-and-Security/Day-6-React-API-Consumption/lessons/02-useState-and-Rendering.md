# Lesson 2 — useState and Rendering Lists

**Session block:** 45 minutes

**Goal:** Store a list in component state and render it as user cards.

## What Is State?

State is a value a component remembers while it is on the page. When state changes, React renders the page again with the new value.

In Lesson 1, `sampleUsers` is a fixed JavaScript array. We will let React keep the current list by putting it in state.

## 1. Add `useState`

At the top of `src/App.jsx`, import `useState`:

```jsx
import { useState } from "react";
```

Keep the `sampleUsers` array from Lesson 1. Inside `App`, replace uses of `sampleUsers` with the `users` state:

```jsx
function App() {
  const [users, setUsers] = useState(sampleUsers);

  return (
    <main>
      <h1>Users</h1>

      {users.map((user) => (
        <article key={user.id} className="user-card">
          <h2>{user.name}</h2>
          <p>Email: {user.email}</p>
          <p>City: {user.address.city}</p>
        </article>
      ))}
    </main>
  );
}
```

`useState(sampleUsers)` gives the state its first value. It returns two things:

- `users` is the list the component reads and displays.
- `setUsers` is the function used to replace that list.

Do not change the list with `users.push(...)`. Use `setUsers(...)` when the list changes so React knows to update the page.

## 2. Render the Array

`users.map((user) => ...)` goes through the list and returns one `<article>` for each user. The JSX inside the parentheses is the card. `{user.name}` reads a value from the current record.

Each card uses `key={user.id}`. A key is a stable label that helps React keep track of each list item. The user id is a good key because it belongs to one user and does not change when the list order changes.

## 3. Local Data and API Data

At this point, the first state value comes from `sampleUsers` in the file. That is **local data**. Later, JSONPlaceholder will send data over the network. That is **API data**.

The setter will save the API list like this:

```jsx
// Replace the current list with the users received from the API.
setUsers(data);
```

The next lesson shows where `data` comes from. Do not call the setter directly in the component body; that would update state while React is rendering.

## Code-Along Checkpoints

1. Change one sample user's email. **Checkpoint:** only that card shows the new email.
2. Add a third sample record with a new `id`. **Checkpoint:** three cards appear and the browser console has no missing-key warning.
3. Point to `users`, `setUsers`, and `sampleUsers`. **Checkpoint:** explain current list, update function, and starting value.

## Quick Check

- What happens when `users` changes through `setUsers`? React renders the page with the new list.
- Why use `user.id` as a key? It is a stable value that identifies the user.
- Where do the sample users come from right now? They are written locally in `App.jsx`.

Continue to [Lesson 3 — Consuming an API](03-Consuming-an-API.md).
