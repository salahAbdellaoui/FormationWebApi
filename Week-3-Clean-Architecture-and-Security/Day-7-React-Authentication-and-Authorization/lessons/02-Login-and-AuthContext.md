# Lesson 2 — Login Form and AuthContext

**Session block:** 00:45–01:30 (45 minutes)

**Goal:** Trace the login form to the real API contract, then share the returned token and login/logout functions through React Context.

## Why Use Context?

The login page receives the user's credentials. Other pages need to know whether a local session exists and need a way to log out. Context shares that state without passing props through every component.

The custom `useAuth` hook gives components one simple place to read the auth state. Later lessons explain the extra UI-only role information it derives from the token.

## 1. Confirm the Real Login Contract

The backend route is `POST /api/auth/login`. Since `apiClient` already uses `/api` as its base URL, React posts to `/auth/login`.

Request body:

```json
{
  "username": "<configured-local-username>",
  "password": "<entered-locally>"
}
```

Success returns `token`, `username`, `email`, and `expiration`. The app needs the token for later requests, so it stores only `data.token`. Invalid credentials return HTTP 401 and the generic message `Invalid username or password.`

## 2. Follow the Login Path

Open `src/pages/Login.jsx`, `src/auth/AuthProvider.jsx`, and `src/api/apiClient.js`.

The Login form controls its username and password fields with React state. `useState` is the React hook that stores a value that can change and makes the component redraw when it changes; `Login.jsx` calls it twice, once for each field. On submit it calls the Context login function. The provider sends the real request:

```jsx
const response = await apiClient.post("/auth/login", {
  username,
  password,
});

const accessToken = response.data?.token;
```

The API base URL changes this path into `https://localhost:7210/api/auth/login`.

After the response contains a token, the provider stores it under the existing key:

```js
sessionStorage.setItem(AUTH_TOKEN_STORAGE_KEY, accessToken);
setToken(accessToken);
```

The key is `employeeManagement.accessToken`. The password stays in form state only and is cleared after successful login. The app never writes the password to storage or logs it.

## 3. See Loading and Error States

`Login.jsx` sets `isLoading` before it awaits `login()` and clears it in `finally`. The button changes to “Signing in…” and is disabled while the request runs.

- HTTP 401 shows the backend's generic credential message.
- A request with no HTTP response shows a connection message.
- Other HTTP failures show a simple general error.

Do not print the Axios error object. Its request configuration can contain the bearer token after login.

## 4. Share the Session

`AuthContext.jsx` creates a context. `AuthProvider.jsx` supplies the current token, `login`, and `logout`. `useAuth.js` reads that context and derives UI state for the current token.

Logout removes `employeeManagement.accessToken` from `sessionStorage` and clears React state. There is no backend logout or token-revocation endpoint. An already issued token can remain valid at the API until it expires.

## Hands-On Task

1. Trace the `onSubmit` handler in `Login.jsx` to `login()` in `AuthProvider.jsx`.
2. Point out the exact request path and the two request fields.
3. If the API and a provisioned local account are available, sign in and confirm the page moves to `/dashboard`.
4. Select **Log out** and inspect the `employeeManagement.accessToken` key in the current tab's Application/Storage panel.

**Expected behavior:** A valid local account receives a token and enters the dashboard. Logout removes the key. With invalid credentials, the form shows a 401 message and does not store a token. If the API is offline, the form shows a connection message instead.

## Common Beginner Mistakes

- **Posting to `/api/auth/login`:** the Axios base URL already includes `/api`.
- **Sending `userName` or `email`:** the verified request fields are `username` and `password`.
- **Saving the whole response or password:** only the token is stored in `sessionStorage`.
- **Assuming a 401 proves the API is offline:** 401 means the API returned an HTTP response; invalid credentials are expected to return it.

## Quick Check

1. Which response field does the provider store?
2. Where does the app keep the token?
3. What does logout remove?

Continue to [Lesson 3 — JWT and Axios Interceptors](03-JWT-and-Axios-Interceptors.md).
