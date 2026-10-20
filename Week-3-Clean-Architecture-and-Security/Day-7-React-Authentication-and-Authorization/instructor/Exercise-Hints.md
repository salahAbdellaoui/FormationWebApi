# Instructor-Only — Exercise Hints

Use these prompts after students have tried the task. They are hints, not replacement code.

1. The username and password inputs use HTML `required`. Ask students to find the `onSubmit` handler in `Login.jsx`.
2. The loading state is set before `await login(...)` and reset in `finally`. Login 401 uses the backend message; no HTTP response means the browser did not receive an API response.
3. `AuthProvider` posts `{ username, password }`, stores `response.data.token`, and exposes `login` and `logout` through `AuthContext`. `useAuth` derives local auth status and roles.
4. `apiClient.js` reads the existing key for each request. Ask students to identify the header name only; never ask them to paste a token into a document or chat.
5. `ProtectedRoute` saves the current location in route state. `Login.jsx` accepts only an internal return path and falls back to `/dashboard`.
6. `tokenClaims.js` reads lowercase `role`. A single role is a string; multiple values can be an array. The Admin route uses the exact case-sensitive value `Admin`.
7. Backend source marks Employee and Department controllers with `[Authorize(Roles = "Admin")]`. If the API is available, source-defined behavior is 401 without an accepted token and 403 for an authenticated wrong role. Record observed results; do not assume a live 200.
8. `logout()` removes `employeeManagement.accessToken`. The API has no token-revocation endpoint, so clearing the browser does not invalidate a copied stateless JWT.

If the API is offline, students can still inspect source and exercise anonymous client-route redirects. Mark server response checks as **not run**, not failed or passed.
