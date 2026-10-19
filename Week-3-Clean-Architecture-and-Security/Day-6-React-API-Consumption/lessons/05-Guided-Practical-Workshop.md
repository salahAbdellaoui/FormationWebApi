# Lesson 5 — Guided Practical Workshop

**Session block:** 45 minutes

**Goal:** Build the final user page in small steps and check every request state.

Keep the Vite server running while you code. Work in `src/App.jsx` and `src/index.css`. Type one step at a time; do not paste the whole finished app at the start.

## Step 1 — Create the Page and State

**Task:** Prepare the page and three values it needs.

**File:** `src/App.jsx`

```jsx
import { useEffect, useState } from "react";

function App() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  return (
    <main>
      <h1>Users</h1>
    </main>
  );
}

export default App;
```

**Explanation:** `users` will hold the array from the API. Loading starts as `true`. Error starts as an empty string.

**Checkpoint:** Save the file. The page still displays the Users heading and the terminal has no syntax error.

## Step 2 — Request and Save Users

**Task:** Add the request inside the component, before its `return`.

**File:** `src/App.jsx`

```jsx
useEffect(() => {
  // Load users after this component appears on the page.
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

**Explanation:** The effect starts the GET request after rendering. The status check catches HTTP errors, JSON parsing gives the app an array, and the setters save the result or error. `finally` stops loading after either result.

**Checkpoint:** Open Developer Tools → Network and reload. Find the `/users` GET request and its JSON response.

## Step 3 — Show Loading and Errors

**Task:** Add these checks after the effect and before the normal list return.

**File:** `src/App.jsx`

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

**Explanation:** The loading check appears first. After loading ends, the error check shows a message if the request failed. When neither applies, React reaches the user list.

**Checkpoint:** Reload. Loading disappears and the app reaches the normal page. The error will be checked in Step 6.

## Step 4 — Count Users and Render Cards

**Task:** Use this as the normal page return after both request-state checks.

**File:** `src/App.jsx`

```jsx
return (
  <main>
    <h1>Users</h1>
    <p className="user-count">Total users: {users.length}</p>

    {users.length === 0 ? (
      <p className="empty-message" role="status">
        No users are available right now.
      </p>
    ) : (
      users.map((user) => (
        <article key={user.id} className="user-card">
          <h2>{user.name}</h2>
          <p>Email: {user.email}</p>
          <p>City: {user.address.city}</p>
        </article>
      ))
    )}
  </main>
);
```

**Explanation:** `users.length` is the current count. The condition shows a friendly message for an empty array. Otherwise, `map()` makes one card for each user. `user.id` is the stable key.

**Checkpoint:** The page shows a total of 10 users and 10 cards. Each card has a name, email, and city.

## Step 5 — Make the Page Easy to Read

**Task:** Keep the existing body, main, heading, and `.user-card` styles in `src/index.css`. Add these styles for the count and empty message.

**File:** `src/index.css`

```css
.user-count {
  color: #444;
  font-weight: bold;
  text-align: center;
}

.empty-message {
  padding: 24px;
  background: #fff;
  border: 1px solid #ddd;
  border-radius: 8px;
  text-align: center;
}
```

**Explanation:** The user count is easy to find. The empty message uses the same simple card style as the users.

**Checkpoint:** Cards are separate from the background, and the count is readable above them.

## Step 6 — Verify Success, Empty, and Error Results

**Task:** Test each result. Restore the real endpoint and setter after temporary tests.

**Files:** `src/App.jsx` and the browser

1. **Success:** Keep the real `/users` endpoint. **Expected:** 10 cards, a total of 10, and a city on each card.
2. **Empty list:** Temporarily replace `setUsers(data)` with this line:

   ```jsx
   setUsers([]);
   ```

   **Expected:** The count is 0 and the friendly empty-list message appears. Restore `setUsers(data)`.

3. **Error:** Temporarily change the endpoint to:

   ```js
   "https://jsonplaceholder.typicode.com/no-such-route"
   ```

   **Expected:** The error message appears after the failed HTTP response. Restore the `/users` endpoint and reload.

**Explanation:** JSONPlaceholder returns a fixed sample list, so the temporary setter checks the empty state. The bad endpoint checks the `response.ok` path.

**Checkpoint:** After both temporary tests, the original endpoint and `setUsers(data)` are restored, and the successful user list works again.

## Finished App Checklist

- [ ] The app requests users from JSONPlaceholder.
- [ ] It displays name, email, and city for each user.
- [ ] It displays loading while the request runs.
- [ ] It displays an error if the request fails.
- [ ] It displays the user count.
- [ ] It displays a friendly message for an empty list.
- [ ] The page uses simple readable styling.

Continue with the [Student Exercise](../exercises/Exercise.md), then use the [Review Questions](../exercises/Review-Questions.md).
