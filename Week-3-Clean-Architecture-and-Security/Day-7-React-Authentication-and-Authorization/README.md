# Day 7 — React Authentication and Authorization

Build and inspect a beginner React app that signs in to the existing EmployeeManagement API. Use the lessons in order as a four-hour, instructor-led code-along. The checked-in app is the final reference; students type each small step before comparing their work with it.

## Learning Objectives

- Explain authentication and authorization in simple terms.
- Configure an Axios client with an environment-based API URL.
- Submit a login form and share session state with React Context and `useAuth`.
- Explain bearer tokens, `sessionStorage`, and Axios interceptor behavior.
- Protect React routes and show role-specific UI for `Admin` and `Employee`.
- Explain why the backend must validate every token and enforce every permission.

## Prerequisites

- Node.js 20 or newer and npm. React Router 7 requires Node.js 20 or newer.
- A code editor and a modern browser.
- Basic JavaScript, JSX, React components, and HTML forms. If you are new to React, start with the Day 6 lessons in [Day-6-React-API-Consumption](../Day-6-React-API-Consumption/README.md) before this day.
- The existing EmployeeManagement backend, .NET 8 SDK, and SQL Server (LocalDB, Express, or Developer Edition) for live login.
- The `dotnet-ef` tool to apply backend migrations.

## Start the React App

Run these commands from this folder. Create `.env.local` from the tracked example first.

PowerShell:

```powershell
Copy-Item .env.example .env.local
```

macOS, Linux, or Git Bash:

```bash
cp .env.example .env.local
```

Install dependencies and start Vite:

```bash
npm install
npm run dev
```

Open `http://localhost:3000`. Vite is configured to use port `3000` because the current backend CORS list allows this origin. Keep the terminal open and stop Vite with `Ctrl+C`.

Create a production bundle with:

```bash
npm run build
```

## API URL and CORS

`.env.example` sets the local API base URL:

```text
VITE_API_BASE_URL=https://localhost:7210/api
```

`src/api/apiClient.js` creates one Axios instance with `axios.create()` and reads this value from Vite. Vite variables are included in browser code; put only public configuration such as the API address there. Never put a password, token, signing key, or other secret in `.env.local`.

The backend HTTPS launch profile listens at `https://localhost:7210`; its API base path is `/api`. Its current CORS configuration allows `http://localhost:3000`. If either port or origin changes, update the matching local configuration. The browser must trust the ASP.NET Core development certificate. If needed, run `dotnet dev-certs https --trust` in a terminal.

## Start the Existing ASP.NET Core API

The API needs a working SQL Server connection and its migrations before it can authenticate users. The existing backend README has full database setup details: [EmployeeManagement README](../../EmployeeManagement/README.md).

Configure `JwtSettings:SigningKey` in User Secrets before starting the API or running EF commands that start its host. Use the private placeholder command in [Configure Local Credentials Safely](#configure-local-credentials-safely) and replace it locally.

From the repository root, apply migrations:

```bash
dotnet ef database update --project EmployeeManagement/src/EmployeeManagement.Infrastructure --startup-project EmployeeManagement/src/EmployeeManagement.API
```

Start the API with its HTTPS development profile:

```bash
dotnet run --project EmployeeManagement/src/EmployeeManagement.API --launch-profile https
```

In Development, Swagger is available at `https://localhost:7210/swagger`. Confirm the HTTPS certificate is trusted and the API's `CorsSettings:AllowedOrigins` includes `http://localhost:3000` before testing from React.

### Configure Local Credentials Safely

No universal development usernames or passwords are committed. Use credentials supplied by the instructor, or configure local-only credentials through .NET User Secrets. Do not put these values in React files, `.env.local`, screenshots, or source control.

Set a private JWT signing key of at least 32 characters:

```bash
dotnet user-secrets set "JwtSettings:SigningKey" "<replace-with-a-private-value-of-32-or-more-characters>" --project EmployeeManagement/src/EmployeeManagement.API
```

The Development seeder reads these optional account settings. Replace each placeholder locally; do not commit the resulting values:

```bash
dotnet user-secrets set "DevTestAccounts:Employee:Username" "<local-employee-username>" --project EmployeeManagement/src/EmployeeManagement.API
dotnet user-secrets set "DevTestAccounts:Employee:Password" "<local-employee-password>" --project EmployeeManagement/src/EmployeeManagement.API
dotnet user-secrets set "DevTestAccounts:Admin:Username" "<local-admin-username>" --project EmployeeManagement/src/EmployeeManagement.API
dotnet user-secrets set "DevTestAccounts:Admin:Password" "<local-admin-password>" --project EmployeeManagement/src/EmployeeManagement.API
```

The seeder runs only in Development and creates an account only when its username and password are configured. The Identity password policy requires at least eight characters, an uppercase letter, a lowercase letter, and a digit. Alternatively, `POST /api/auth/register` creates an `Employee` account; public registration cannot assign `Admin`. Admin accounts must come from a controlled local provisioning process.

## Verified API Contract

The login form sends this request to `POST /api/auth/login`:

```json
{
  "username": "<configured-local-username>",
  "password": "<entered-locally>"
}
```

Successful login returns these fields:

```json
{
  "token": "<issued-token>",
  "username": "<account-name>",
  "email": "<account-email>",
  "expiration": "<UTC-date-time>"
}
```

Invalid credentials return HTTP `401` with the generic message `Invalid username or password.` The React app stores only the token in `sessionStorage`; it does not store the password.

## Authentication and Authorization Flow

1. `Login.jsx` submits credentials through `useAuth`.
2. `AuthProvider.jsx` calls the verified login endpoint and stores the returned token under `employeeManagement.accessToken` in `sessionStorage`.
3. `apiClient.js` attaches the token as `Authorization: Bearer <token>` to requests.
4. A login `401` remains a form error. A protected-request `401` clears local auth state. A `403` is passed to the caller without logging the user out.
5. `ProtectedRoute.jsx` redirects unauthenticated users to `/login`; login returns them to a safe internal path. `/admin` uses the `Admin` claim for a client-side route decision.
6. `useAuth.js` reads the backend's lowercase `role` claim for UI display and route experience. The backend remains the authority.

| Route | Client behavior |
|---|---|
| `/` | Redirects to `/login` or `/dashboard` based on local auth state. |
| `/login` | Shows the login form; an existing local session redirects to `/dashboard`. |
| `/dashboard` | Requires local auth state; displays role values for training. |
| `/admin` | Requires the `Admin` role for the client route only. |
| `/forbidden` | Shows an access-denied page. |
| Unknown path | Redirects through `/` to a sensible route. |

The backend returns one `ClaimTypes.Role` claim per role. In the JWT payload, the wire name is lowercase `role`; JSON represents one role as a string and multiple roles as an array. The exact role values are `Admin` and `Employee`. All Employee and Department API actions (`/api/employee` and `/api/department`) require `Admin`.

## Lesson Sequence and Four-Hour Schedule

The teaching plan totals exactly 240 minutes. Follow the code-along checkpoints; do not paste the full reference app at the start.

| Time | Topic | Lesson |
|---|---|---|
| 00:00–00:45 | Authentication concepts and Axios setup | [Lesson 1](lessons/01-Authentication-and-Axios.md) |
| 00:45–01:30 | Login form and AuthContext | [Lesson 2](lessons/02-Login-and-AuthContext.md) |
| 01:30–02:15 | JWT and Axios interceptors | [Lesson 3](lessons/03-JWT-and-Axios-Interceptors.md) |
| 02:15–03:00 | Protected routes | [Lesson 4](lessons/04-Protected-Routes.md) |
| 03:00–03:45 | Role-based authorization and error handling | [Lesson 5](lessons/05-Role-Based-Authorization.md) |
| 03:45–04:00 | Review and testing | [Instructor plan](instructor/Four-Hour-Teaching-Plan.md) |
| **Total** | | **240 minutes** |

- [Four-Hour Teaching Plan](instructor/Four-Hour-Teaching-Plan.md)
- [Student Exercise](exercises/Exercise.md)
- [Review Questions](exercises/Review-Questions.md)
- [Instructor Exercise Hints](instructor/Exercise-Hints.md)
- [Instructor Review Answers](instructor/Review-Answers.md)

## Test the App

- `npm run build` checks that Vite can compile the app. It does not test API connectivity or authentication.
- An anonymous `GET /api/employee` should return `401`; an authenticated `Employee` should receive `403`; an `Admin` may receive `200` when the backend and database are working.
- The React Admin page does not call an Admin-only API and does not claim that an API request succeeded.
- Use Swagger or the browser Network panel for live API checks only when the API, database, certificate, CORS, and local credentials are ready. Never save or share a bearer token.

## Security Notes and Known Limitations

- `sessionStorage` is readable by JavaScript in the page. An XSS bug could expose the token; session storage is not fully secure.
- Logout clears the browser's token. The backend has no token-revocation endpoint, so an already-issued stateless JWT remains usable until it expires.
- The login response has no `/me` endpoint and no role field. The app decodes only the `role` and `exp` payload values for display and route experience. It does not verify the token signature.
- Client-side role checks can be manipulated or become stale. The backend independently validates the JWT signature, issuer, audience, and expiration, and enforces Admin authorization on every protected endpoint.
- There is no secure-cookie login or refresh-token endpoint in the existing backend.
- Live authentication has **not** been verified. The API at `https://localhost:7210` was unavailable during documentation verification. The local certificate's trust state and valid configured account credentials are environment-dependent.
- No frontend automated test suite is configured. Do not report a successful browser or backend authentication test unless one was actually run.
