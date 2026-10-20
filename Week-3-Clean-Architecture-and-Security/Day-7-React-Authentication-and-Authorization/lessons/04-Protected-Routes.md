# Lesson 4 — Protected Routes

**Session block:** 02:15–03:00 (45 minutes)

**Goal:** Follow the React Router flow for public, authenticated, and unknown paths, including a safe return to the requested path after login.

## 1. Put the Router Around the App

In `src/main.jsx`, `BrowserRouter` wraps `AuthProvider` and `App`. This gives route components access to the browser URL and to auth state.

`src/App.jsx` defines these routes:

| Path | Behavior |
|---|---|
| `/` | Sends a local session to `/dashboard`; otherwise sends the user to `/login`. |
| `/login` | Shows Login unless a local session exists, then redirects to `/dashboard`. |
| `/dashboard` | Uses `ProtectedRoute`; no role is required. |
| `/admin` | Uses `ProtectedRoute` and requires `Admin` (Lesson 5). |
| `/forbidden` | Shows an access-denied message. |
| Unknown path | Redirects to `/`, which chooses login or dashboard. |

## 2. Protect the Dashboard

`src/auth/ProtectedRoute.jsx` reads `isAuthenticated` from `useAuth`. If there is no usable local session, it redirects to login and saves the current location:

```jsx
<Navigate to="/login" replace state={{ from: location }} />
```

After login, `Login.jsx` reads `location.state?.from` and calls `getSafeReturnPath`. It accepts an internal path and rejects values such as `//outside.example` or paths containing a backslash. If there is no safe requested path, it goes to `/dashboard`.

## 3. Follow One Route

1. Open `http://localhost:3000/dashboard` while signed out.
2. `ProtectedRoute` redirects to `/login` and saves `/dashboard` in route state.
3. On successful login, `Login.jsx` navigates to the saved safe path.
4. If a protected API request later returns 401, the Axios callback clears Context state. The route guard redirects to login again.

The current session initialization reads `sessionStorage` synchronously, so it does not need an asynchronous loading screen.

## Hands-On Task

1. With no token in the current tab, open `/dashboard` directly.
2. Confirm the URL moves to `/login`.
3. If the backend and a valid local account are available, sign in and confirm the app returns to `/dashboard`.
4. While signed in, open `/login`; confirm it redirects to `/dashboard`.
5. Try an unknown URL and confirm it returns through `/` to a sensible route.

**Expected behavior:** Anonymous users see Login. Authenticated users do not stay on Login. A safe internal path is restored after login. The redirect flow does not loop. Live login requires a reachable API and valid account; source inspection can verify the redirect logic when it is not available.

## Security Boundary

`ProtectedRoute` controls what React displays. It does not prove that the token is valid and does not protect API data. The backend must validate every bearer token and authorize each protected endpoint.

## Common Beginner Mistakes

- **Putting `BrowserRouter` inside `App`:** `App` uses route hooks and components, so the router must wrap it in `main.jsx`.
- **Redirecting Login to itself:** `/login` sends an authenticated local session to `/dashboard`; `/dashboard` sends signed-out state to `/login`.
- **Trusting route state as a URL:** return only safe internal paths. Do not redirect to a user-supplied external address.
- **Calling the route guard backend security:** a user can edit React state or call an API directly. The API still needs its own authorization.

## Quick Check

1. Where is the original location saved?
2. Which component sends an anonymous user to `/login`?
3. Does the route guard validate the JWT signature?

Continue to [Lesson 5 — Role-Based Authorization](05-Role-Based-Authorization.md).
