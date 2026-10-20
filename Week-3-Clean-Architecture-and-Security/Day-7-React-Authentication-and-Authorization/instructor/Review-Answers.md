# Instructor Answer Key — Day 7 Review

1. **Authentication** verifies who signed in. **Authorization** decides what that user may access.
2. `POST /api/auth/login` with `{ username, password }`.
3. `{ token, username, email, expiration }`.
4. HTTP `401 Unauthorized` with `Invalid username or password.`
5. `employeeManagement.accessToken` in `sessionStorage`. Passwords are sensitive and are needed only for the login request; the app never persists them.
6. `Authorization: Bearer <token>` when a token is present.
7. Login 401 stays a form error. A protected-request 401 clears local auth state. A 403 passes through without logging the user out.
8. Lowercase `role`. The backend adds one role claim per role; the serialized payload can hold a string for one role or an array for several. Values are `Admin` and `Employee`.
9. Browser claims are unverified and can be altered or stale. They guide the UI only. The backend validates the token signature, issuer, audience, lifetime, and required role.
10. `/dashboard` redirects a signed-out user to `/login`. `/api/employee` requires a valid token with the `Admin` role; an authenticated Employee receives 403.

**Testing note:** These answers describe the verified source contract. They do not claim that live API behavior was tested in this environment.
