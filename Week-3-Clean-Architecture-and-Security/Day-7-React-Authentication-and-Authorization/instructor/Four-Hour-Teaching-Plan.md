# Day 7 — Four-Hour Teaching Plan

- **Week:** Week 3 — Clean Architecture and Security
- **Duration:** 4 hours (240 minutes)
- **Level:** Beginner
- **Format:** Instructor-led explanation, live coding, guided practice, and checkpoints

## Purpose

Students trace and build the React authentication flow in small steps. They use the existing EmployeeManagement API contract and learn that browser route checks improve user experience but never replace backend authorization.

The checked-in `src/` folder is the final reference app. Ask students to follow the lessons in order, type each focused example in a working copy, and compare at each checkpoint. Do not paste a whole file at the start.

## Learning Objectives

By the end of the session, students can:

1. Explain authentication and authorization.
2. Configure Axios with `VITE_API_BASE_URL`.
3. Submit `{ username, password }` to the real login endpoint.
4. Explain how Context, `useAuth`, and `sessionStorage` share local auth state.
5. Explain bearer headers and the interceptor behavior for 401 and 403.
6. Trace protected routes and a safe return to the requested path.
7. Describe the real `role` claim and the `Admin` and `Employee` roles.
8. Explain why the backend must verify the token and enforce roles for every API request.

## Instructor Preparation

Before class:

1. From the Day 7 folder, run `npm install`, `npm run build`, and `npm audit`.
2. Check `.env.example`; create `.env.local` locally without adding secrets.
3. If demonstrating live login, start SQL Server, apply EmployeeManagement migrations, configure its JWT signing key and test accounts with User Secrets, and start the API using the HTTPS profile.
4. Trust the local HTTPS certificate and confirm `CorsSettings:AllowedOrigins` includes `http://localhost:3000`.
5. Confirm which local test accounts are configured. No universal usernames or passwords are committed.
6. Open the editor, browser, Vite terminal, and—only if available—Swagger/Network panels.
7. Do not display, log, or capture credentials or bearer tokens.

## Exact Schedule

| Time | Duration | Topic |
|---|---:|---|
| 00:00–00:45 | 45 min | Authentication concepts and Axios setup |
| 00:45–01:30 | 45 min | Login form and AuthContext |
| 01:30–02:15 | 45 min | JWT and Axios interceptors |
| 02:15–03:00 | 45 min | Protected routes |
| 03:00–03:45 | 45 min | Role-based authorization and error handling |
| 03:45–04:00 | 15 min | Review and testing |
| **Total** | **240 min** | |

## Demo Plan: Source/Build Checks vs Live API Tests

### Can be checked without the backend

- Read the login DTO, response DTO, JWT source, role attributes, route config, and interceptor code.
- Start Vite and confirm the login page renders at `http://localhost:3000`.
- Run `npm run build` and `npm audit`.
- Visit `/dashboard` without a local session and inspect the redirect to `/login`.
- Inspect code that returns `/forbidden` for a missing Admin role.

These checks do not prove that a login, token, or API permission works at runtime.

### Requires a running backend

Live login and status-code demonstrations require SQL Server and migrations, a signing key, configured local credentials, a trusted HTTPS development certificate, and matching CORS settings. The API must be reachable at `https://localhost:7210/api`.

When those prerequisites are met, test:

- Invalid login returns 401 and stays a form error.
- Anonymous access to `GET /api/employee` returns 401.
- A valid Employee token to that endpoint returns 403.
- A valid Admin token may receive 200 when the database and endpoint succeed.
- The Admin React page itself makes no API call and does not claim API success.

At the time this plan was written, `https://localhost:7210` was not listening. Live authentication has not been verified in this environment. Mark any unavailable live check **not run**.

---

## Block 1 — Authentication Concepts and Axios Setup

**Time:** 00:00–00:45

**Learning objective:** Distinguish authentication from authorization and identify where Axios gets the API URL.

### Instructor explanation and demonstration

- Define authentication as checking who signed in; define authorization as checking what that user may access.
- Start Vite at port 3000. Trace `index.html` → `main.jsx` → `App.jsx`.
- Show `.env.example` and `src/api/apiClient.js`. Explain that the base URL ends in `/api`.
- Demonstrate the small GET check in [Lesson 1](../lessons/01-Authentication-and-Axios.md). It calls the verified, Admin-protected `/api/employee` route and prints only the status.
- Explain the expected results: no token gives 401; an Employee token gives 403; no HTTP response means a network, certificate, CORS, or server problem.

### Live coding and student practice

- Students copy `.env.example` to `.env.local`, install packages, and start Vite.
- Students identify `axios.create()` and read `VITE_API_BASE_URL`.
- Students run the temporary GET in the browser console, first signed out. They do not log token contents.

### Checkpoint and expected result

The login screen appears at port 3000. If the backend is available, the signed-out GET shows HTTP 401. If not, students record “No HTTP response” and continue with source inspection.

**Common mistake:** Adding `/api` twice to the path.

**Question:** Why can an HTTP 401 still prove that the request reached the API?

---

## Block 2 — Login Form and AuthContext

**Time:** 00:45–01:30

**Learning objective:** Trace the verified credential request and explain how Context shares the token and logout function.

### Instructor explanation and demonstration

- Read `LoginRequestDto`, `AuthController.Login`, and `AuthResponseDto` from the backend.
- Show the exact request: `{ username, password }` to `POST /api/auth/login`.
- Show the success fields: `token`, `username`, `email`, and `expiration`. Invalid credentials return 401 with a generic message.
- Trace `Login.jsx` → `useAuth()` → `AuthProvider.login()` → `apiClient.post()`.
- Explain `AuthContext`, `AuthProvider`, and the custom hook. The app stores only the token under `employeeManagement.accessToken`.
- Show loading, error, and logout behavior. Logout clears this browser tab; it does not revoke a JWT at the server.

### Live coding and student practice

- Students type the small login request from [Lesson 2](../lessons/02-Login-and-AuthContext.md).
- Students connect the controlled username/password fields to `handleSubmit`.
- If a configured account and backend are available, students sign in and log out. Otherwise, they trace success and 401 branches from source.

### Checkpoint and expected result

The request field names match the DTO. A successful response stores the token only; invalid credentials show the generic message; the loading state ends in both cases.

**Common mistake:** Storing the password or assuming the API returns roles outside the token.

**Question:** Which value does the app keep after the API response, and where?

---

## Block 3 — JWT and Axios Interceptors

**Time:** 01:30–02:15

**Learning objective:** Describe the bearer header and distinguish login 401, protected 401, and 403.

### Instructor explanation and demonstration

- Explain JWT header, payload, and signature. Decoding the payload does not verify its signature.
- Explain the `Authorization: Bearer <token>` header.
- Trace the request interceptor in `src/api/apiClient.js`; it reads the one session key for each request.
- Trace the response interceptor. Login POST 401 is excluded; another 401 calls the handler registered by `AuthProvider`; 403 and other errors pass through.
- Explain that interceptors do not navigate or retry. React state changes and route guards handle the UI.
- Explain `sessionStorage` XSS risk. Never log passwords, tokens, or raw request config.

### Live coding and student practice

- Students write the request header snippet from [Lesson 3](../lessons/03-JWT-and-Axios-Interceptors.md).
- Students trace `registerAuthExpiryHandler(logout)` and describe what happens after a protected-request 401.
- If the API is available, students inspect only the header name in Network tools; they do not capture the token.

### Checkpoint and expected result

Students can state: login 401 is a form error; protected 401 clears local session state; 403 does not log out; other errors remain available to the caller.

**Common mistake:** Treating every non-200 result as the same or adding an automatic retry.

**Question:** Why should an authenticated user's 403 not automatically clear the token?

---

## Block 4 — Protected Routes

**Time:** 02:15–03:00

**Learning objective:** Follow a React Router redirect and return a signed-in user to a safe internal path.

### Instructor explanation and demonstration

- Show `BrowserRouter` wrapping the auth provider and app in `main.jsx`.
- Trace `/`, `/login`, `/dashboard`, `/forbidden`, and the wildcard route in `App.jsx`.
- Show `ProtectedRoute`: anonymous users go to login with the current location in route state.
- Trace `getSafeReturnPath` in `Login.jsx`. It accepts an internal path and rejects external-looking paths.
- State the boundary: these routes use local auth state for the user interface. They do not validate the JWT or secure API data.
- Session initialization is a synchronous `sessionStorage` read, so this app has no asynchronous auth-loading step.

### Live coding and student practice

- Students write the protected `/dashboard` route from [Lesson 4](../lessons/04-Protected-Routes.md).
- Students open `/dashboard` while signed out and observe the redirect.
- With live login available, students sign in and observe the return to the intended path; otherwise, they trace the route state and navigation code.

### Checkpoint and expected result

Signed-out `/dashboard` resolves to `/login`. Authenticated `/login` resolves to `/dashboard`. An unknown path goes to `/` and then to login or dashboard without a loop.

**Common mistake:** Sending untrusted route state to an external URL or treating a React guard as backend security.

**Question:** Where is the original route saved before login?

---

## Block 5 — Role-Based Authorization and Error Handling

**Time:** 03:00–03:45

**Learning objective:** Read the actual role claim format, show role-aware UI, and distinguish client presentation from backend authorization.

### Instructor explanation and demonstration

- Read `JwtTokenService`: it adds one `ClaimTypes.Role` claim per role. The JWT wire name is lowercase `role`; one role is a string and multiple values may be an array.
- Confirm exact values from `RoleInitializer`: `Admin` and `Employee`.
- Read `Program.cs`: signature, issuer, audience, and lifetime are validated; `role` maps to `ClaimTypes.Role`.
- Read the class attributes on `EmployeeController` and `DepartmentController`: all actions require `Admin`.
- Show `tokenClaims.js`, `useAuth`, Dashboard role display, and `/admin`'s `requiredRoles={["Admin"]}`.
- Read the security note on `AdminPage`. The page makes no Admin API request.
- Compare source-defined 401/403 responses. Do not claim live API access without observing it.

### Live coding and student practice

- Students type the short role-normalization example from [Lesson 5](../lessons/05-Role-Based-Authorization.md).
- Students verify that `/admin` requires exact `Admin` casing and routes other local roles to `/forbidden`.
- With configured local accounts, students compare Employee and Admin route experiences. With backend access, they use Swagger to compare API statuses without saving tokens.

### Checkpoint and expected result

Dashboard displays readable unexpired role values. An Employee is sent to Forbidden for `/admin`; an Admin sees the Admin page. Students can explain that these are UI outcomes and the API still checks roles.

**Common mistake:** Treating decoded `role` as proof of permission or expecting public registration to create an Admin.

**Question:** Which source line makes the backend enforce Admin on Department requests?

---

## Block 6 — Review and Testing

**Time:** 03:45–04:00

**Learning objective:** Separate source/build verification from runtime API evidence.

### Instructor explanation and demonstration

- Recap request → login response → token storage → bearer header → route display.
- Run `npm run build`; explain that compilation does not test login or authorization.
- Run `npm audit` if time permits.
- Use the review questions. Compare answers with [Review Answers](Review-Answers.md) after students respond.
- If all backend prerequisites are ready, run the live matrix. If not, mark those checks “not run.”

### Student practice

- Students answer review questions independently for five minutes.
- In pairs, students explain one 401 path, one 403 path, and one client-versus-server security boundary.

### Checkpoint and expected result

Students can describe the correct request fields, token key, role values, route behavior, and why API authorization is still required. No runtime claim is made unless the request was actually observed.

**Common mistake:** Calling a successful build proof of successful backend authentication.

**Question:** What evidence would prove an Employee received 403 from the live API?
