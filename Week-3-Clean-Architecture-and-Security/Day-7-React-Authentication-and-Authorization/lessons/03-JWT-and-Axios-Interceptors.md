# Lesson 3 — JWT and Axios Interceptors

**Session block:** 01:30–02:15 (45 minutes)

**Goal:** Explain the bearer-token request flow and show how the existing Axios interceptors distinguish login 401s, protected 401s, and 403 responses.

## 1. What a JWT Does

After a successful login, the API returns a signed JSON Web Token (JWT). React keeps the token for the current browser tab and sends it to the API in this header:

```text
Authorization: Bearer <token>
```

A JWT has three dot-separated parts: a header, a payload, and a signature. The browser can decode the payload, but it cannot verify the signature. A user can edit browser data or send a changed token. Only the backend can validate the signature, issuer, audience, and lifetime.

The backend's token service adds one `ClaimTypes.Role` claim for each user role. The JWT wire name is lowercase `role`. One role is represented as a string; multiple role claims can be represented as an array. Values are exactly `Admin` and `Employee`.

## 2. Store the Token Only for This Tab

The shared key is defined in `src/auth/authStorage.js`:

```js
export const AUTH_TOKEN_STORAGE_KEY = "employeeManagement.accessToken";
```

`AuthProvider.jsx` stores the token in `sessionStorage`, not the password. Session storage is readable by JavaScript on the page. An XSS vulnerability could expose it, so `sessionStorage` is not fully secure.

## 3. Let the Request Interceptor Add the Header

Open `src/api/apiClient.js`. One module-level interceptor reads the shared key for each request:

```js
apiClient.interceptors.request.use((config) => {
  const token = sessionStorage.getItem(AUTH_TOKEN_STORAGE_KEY);

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});
```

This avoids adding the same header code to each API call. The interceptor never logs the token. It is installed once when the Axios module loads, not every time React renders.

## 4. Distinguish 401 from 403

- **401 Unauthorized:** The API did not accept the token. A protected-request 401 invokes the handler registered by `AuthProvider`; that handler clears the local token and Context state. The React routes then show login.
- **Login 401:** The response interceptor recognizes `POST /api/auth/login` and does not clear state. `Login.jsx` receives the original error and shows the generic credential message.
- **403 Forbidden:** The server accepted the user's identity but denied permission. The interceptor does not log out; it passes the original error to the caller.
- **Other failures:** The original Axios error is passed through so the caller can inspect its HTTP status. The UI should show a safe message rather than raw server details.

The response interceptor's important check is:

```js
if (error.response?.status === 401 && !isLoginRequest(error.config)) {
  authExpiryHandler?.();
}

return Promise.reject(error);
```

`isLoginRequest` prevents an invalid-password response from being treated as an expired protected session. Returning the original error lets `Login.jsx` show the API's generic message.

The response interceptor has no retry loop and does not navigate. React state controls the page. `ProtectedRoute` owns redirects.

## Hands-On Task

1. Read `isLoginRequest` and identify how it recognizes the login POST.
2. Trace `registerAuthExpiryHandler(logout)` in `AuthProvider.jsx`.
3. If the backend is available, submit invalid login details and observe the form's 401 message. Do not copy or log the password.
4. Inspect the Network panel for the `Authorization` header on a protected request. Do not copy the token into notes or screenshots.

**Expected behavior:** Login 401 remains a form error. A protected-request 401 clears the browser session. A 403 leaves the session intact. The API must be available to observe these HTTP responses; the build alone does not test them.

## Common Beginner Mistakes

- **Decoding a JWT and trusting its roles:** decoded claims are unverified browser data. The backend makes the final decision.
- **Logging `error` or request config:** it can contain a bearer token. Show only a safe message and status.
- **Logging out on 403:** a 403 is not a failed login. Keep the user signed in.
- **Redirecting inside Axios:** this couples HTTP code to browser navigation and can create loops. Interceptors update auth state; routes handle navigation.
- **Adding interceptors in a component:** rerenders could register duplicates. The current client registers one pair at module scope.

## Quick Check

1. Which header carries the token?
2. Why does a login 401 bypass global session clearing?
3. Does a 403 clear the local session?

Continue to [Lesson 4 — Protected Routes](04-Protected-Routes.md).
