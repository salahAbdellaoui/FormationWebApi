import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/useAuth.js";

function Dashboard() {
  const { roles, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate("/login", { replace: true });
  }

  return (
    <main className="lesson-card">
      <p className="eyebrow">Authenticated area</p>
      <h1>Dashboard</h1>
      <p>
        This page uses local sign-in state for navigation. The API still
        validates the token when protected data is requested.
      </p>
      <h2>Roles from this token</h2>
      {roles.length > 0 ? (
        <ul>
          {roles.map((role) => (
            <li key={role}>{role}</li>
          ))}
        </ul>
      ) : (
        <p>No readable, unexpired role claims are available.</p>
      )}
      <p>These roles are for display only and do not prove API permission.</p>
      <button type="button" onClick={handleLogout}>
        Log out
      </button>
    </main>
  );
}

export default Dashboard;
