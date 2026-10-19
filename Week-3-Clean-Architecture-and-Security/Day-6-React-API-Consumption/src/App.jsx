import { useEffect, useState } from "react";

function App() {
  // Store the users returned by the API so React can display them.
  const [users, setUsers] = useState([]);
  // Start in the loading state while the API request is running.
  const [loading, setLoading] = useState(true);
  // Keep an error message to show if the request fails.
  const [error, setError] = useState("");

  useEffect(() => {
    // Load users after the component appears on the page.
    fetch("https://jsonplaceholder.typicode.com/users")
      .then((response) => {
        // fetch does not treat HTTP errors, like 404, as request failures.
        if (!response.ok) {
          throw new Error("The server could not load users.");
        }

        // Convert the response body from JSON into JavaScript data.
        return response.json();
      })
      .then((data) => {
        // Save the users in state so React can display them.
        setUsers(data);
      })
      .catch(() => {
        // Show a friendly message for network or server errors.
        setError("Unable to load users. Please check your connection and try again.");
      })
      .finally(() => {
        // Hide the loading message after the request finishes.
        setLoading(false);
      });
  }, []);

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
}

export default App;
