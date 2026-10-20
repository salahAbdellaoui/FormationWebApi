import { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/useAuth.js";

function getSafeReturnPath(from) {
  const pathname = from?.pathname;

  if (
    typeof pathname !== "string" ||
    !pathname.startsWith("/") ||
    pathname.startsWith("//") ||
    pathname.includes("\\")
  ) {
    return "/dashboard";
  }

  const search = typeof from.search === "string" && from.search.startsWith("?")
    ? from.search
    : "";
  const hash = typeof from.hash === "string" && from.hash.startsWith("#")
    ? from.hash
    : "";

  return `${pathname}${search}${hash}`;
}

function Login() {
  const { login } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  async function handleSubmit(event) {
    event.preventDefault();
    setErrorMessage("");
    setIsLoading(true);

    try {
      await login(username, password);
      setPassword("");
      navigate(getSafeReturnPath(location.state?.from), { replace: true });
    } catch (error) {
      if (error.response?.status === 401) {
        setErrorMessage(
          error.response.data?.message || "Invalid username or password.",
        );
      } else if (!error.response) {
        setErrorMessage(
          "Could not reach the API. Check that the backend is running and try again.",
        );
      } else {
        setErrorMessage("Login failed. Please try again.");
      }
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <main className="lesson-card">
      <p className="eyebrow">Week 3 · Day 7</p>
      <h1>React Authentication and Authorization</h1>
      <form onSubmit={handleSubmit}>
        <p>
          <label htmlFor="username">Username</label>
          <br />
          <input
            autoComplete="username"
            disabled={isLoading}
            id="username"
            onChange={(event) => setUsername(event.target.value)}
            required
            type="text"
            value={username}
          />
        </p>
        <p>
          <label htmlFor="password">Password</label>
          <br />
          <input
            autoComplete="current-password"
            disabled={isLoading}
            id="password"
            onChange={(event) => setPassword(event.target.value)}
            required
            type="password"
            value={password}
          />
        </p>
        <button disabled={isLoading} type="submit">
          {isLoading ? "Signing in…" : "Sign in"}
        </button>
      </form>
      {isLoading && <p aria-live="polite">Signing in…</p>}
      {errorMessage && <p role="alert">{errorMessage}</p>}
    </main>
  );
}

export default Login;
