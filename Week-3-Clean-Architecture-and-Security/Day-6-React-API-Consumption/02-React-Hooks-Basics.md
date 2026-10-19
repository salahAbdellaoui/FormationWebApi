# 02 — React Hooks Basics

The app uses `useState` to remember data, `useEffect` to load data, and `fetch` to request data from the API.

## `useState`: Remember Values

`useState` keeps a value while the page is open. The app starts with three values:

```jsx
const [users, setUsers] = useState([]);
const [loading, setLoading] = useState(true);
const [error, setError] = useState("");
```

- `users` starts as an empty list. `setUsers` saves the users from the API.
- `loading` starts as `true`, so the page can show `Loading users...`.
- `error` starts as an empty string. The app saves a message here if the request fails.

When a state value changes, React shows the page again with the new value.

## `useEffect`: Load Data When the Page Appears

The app uses `useEffect` to start the request after the page appears:

```jsx
useEffect(() => {
  // Ask the API for users when this component appears.
  fetch("https://jsonplaceholder.typicode.com/users")
    .then((response) => {
      if (!response.ok) {
        throw new Error("The server could not load users.");
      }

      return response.json();
    })
    .then((data) => setUsers(data))
    .catch(() => {
      setError("Unable to load users. Please check your connection and try again.");
    })
    .finally(() => setLoading(false));
}, []);
```

The empty dependency array `[]` tells React that this effect has no changing values to watch. React runs it when the component is added to the page. In development, React Strict Mode may run an effect more than once. That is normal.

## `fetch`: Ask the API for Data

`fetch` sends a web request. Here, it asks JSONPlaceholder for users. The browser uses a GET request by default.

- `response.ok` checks whether the server sent a successful response. `fetch` does not treat an HTTP error, such as 404, as a failed request by itself.
- `response.json()` reads the response as JSON and gives the app JavaScript data.
- `setUsers(data)` saves the returned list, so React can display it.
- `catch` saves an error message if the request fails.
- `finally` runs after success or failure and sets `loading` to `false`.

## Loading and Error Messages

While `loading` is `true`, `App.jsx` displays `Loading users...`. If `error` contains a message, the app displays it as an alert. Otherwise, the app uses `users.map()` to show each user's name, email, and city.

If the list is empty, the app currently shows no user cards. The exercise asks you to add a message for that case.
