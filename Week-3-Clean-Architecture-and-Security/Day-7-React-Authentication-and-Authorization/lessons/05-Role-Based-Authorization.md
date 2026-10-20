# Lesson 5 — Role-Based Authorization and Error Handling

**Session block:** 03:00–03:45 (45 minutes)

**Goal:** Show the actual backend roles in the UI, protect the Admin route for user experience, and explain the backend's 401/403 decisions.

## 1. Use the Backend's Real Role Claim

`JwtTokenService` adds one `ClaimTypes.Role` claim for every role on the user. The API maps the lowercase JWT wire claim `role` back to `ClaimTypes.Role`. A JWT with one role normally has a string value; multiple role claims are serialized as multiple values, represented as an array in the JSON payload.

The exact values are case-sensitive:

- `Admin`
- `Employee`

The login JSON response does not include roles, and the backend has no `/me` endpoint. `src/auth/tokenClaims.js` reads only `role` and `exp` from the JWT payload. It supports a string or an array and returns no roles for missing, malformed, or expired-looking data.

This code only reads the payload. It does not verify the signature. An attacker can change it, so the returned roles are suitable only for display and route experience.

The helper normalizes the backend's actual string-or-array format:

```js
const roleClaim = payload.role;
const roles =
  typeof roleClaim === "string"
    ? [roleClaim]
    : Array.isArray(roleClaim)
      ? roleClaim.filter((role) => typeof role === "string" && role.length > 0)
      : [];
```

It also checks the numeric `exp` claim for local display. A future-looking expiration does not prove that the signature is valid.

## 2. Require a Role in `ProtectedRoute`

Routes without `requiredRoles` still need only a local session. A route can pass one or more accepted roles:

```jsx
<ProtectedRoute requiredRoles={["Admin"]}>
  <AdminPage />
</ProtectedRoute>
```

`ProtectedRoute` first sends unauthenticated users to `/login`. It then checks whether any required role is in `useAuth().roles`. If not, it redirects to `/forbidden`. It does not change the backend's permissions.

## 3. Explain the API Boundary

The backend applies `[Authorize(Roles = "Admin")]` at the controller level to both `EmployeeController` and `DepartmentController`. Therefore all actions under these routes require Admin:

- `/api/employee`
- `/api/department`

With no accepted token, the API returns `401 Unauthorized`. With a valid Employee token, it returns `403 Forbidden`. With an Admin token, the API may proceed to the action. The React `AdminPage` does not call either endpoint, so it does not claim that an Admin API call succeeded.

## 4. Show Roles on the Dashboard

`Dashboard.jsx` displays the roles returned by `useAuth`. This is a training display, not an API permission check. `AdminPage.jsx` states the security boundary directly: browser-decoded claims may be changed or become stale; the backend must validate the JWT and enforce the role on every protected request.

## Hands-On Task

1. Read `readTokenUiClaims` in `src/auth/tokenClaims.js` and identify how it handles a string and an array.
2. Find the exact `Admin` string in the `/admin` route in `src/App.jsx`.
3. With a valid Employee account, visit `/admin`; expect the client route to send you to `/forbidden`.
4. With a valid Admin account, visit `/admin`; expect the Admin page and its security note.
5. If the API is available, use its Swagger UI to verify anonymous `401` and Employee `403` for an existing protected endpoint. Do not copy a token into logs, screenshots, or shared notes.

**Expected behavior:** The Dashboard lists available token roles. An Employee sees Forbidden at `/admin`; an Admin sees the Admin page. These browser results do not prove backend access. Verify backend status codes separately only when the API, database, CORS, certificate, and local accounts are ready.

## Common Beginner Mistakes

- **Writing `admin` instead of `Admin`:** role names and casing come from the backend source.
- **Expecting the login response to contain a role:** it contains token, username, email, and expiration; the app reads role data from the token payload for UI only.
- **Treating a hidden button or route as security:** a caller can bypass React. The API controller still checks Admin.
- **Treating 401 and 403 as the same:** 401 means the API did not accept authentication; 403 means the authenticated user lacks permission.
- **Claiming the Admin page proves API access:** it makes no Admin-only API call.

## Quick Check

1. What is the exact Admin role value?
2. Does the client verify the token's signature?
3. Which existing API controllers enforce Admin?

Finish with the [student exercise](../exercises/Exercise.md) and [review questions](../exercises/Review-Questions.md).
