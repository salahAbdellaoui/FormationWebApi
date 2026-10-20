# Student Exercise — Authentication and Route Access

Complete these tasks in order. Use the current `src/` application as your reference. Keep the API contracts and routes unchanged. Do not edit the backend or add credentials to source files.

## Tasks

1. **Check login validation.** Submit the form with one or both fields empty. Observe the browser's required-field validation. Submit invalid credentials when the API is running.
2. **Check loading and errors.** Confirm the button is disabled while a request is pending. Record the difference between an API `401` message and a connection failure.
3. **Trace Context state.** Find `login`, `logout`, `token`, and `isAuthenticated`. Confirm that the token is stored under `employeeManagement.accessToken` and the password is not stored.
4. **Check the bearer request.** With a valid local session, repeat the temporary `apiClient.get("/employee")` check from Lesson 1. Use the browser Network panel to confirm that it carries an `Authorization: Bearer` header. Dashboard and AdminPage do not call a protected data endpoint. Do not copy the token into notes or screenshots.
5. **Check protected navigation.** Sign out, open `/dashboard`, and confirm you reach `/login`. If login succeeds, confirm the app returns to the requested path. Then visit `/login` while signed in.
6. **Check the role UI.** Sign in with an `Employee` account and visit `/admin`; expect `/forbidden`. Sign in with an `Admin` account and visit `/admin`; expect the Admin page.
7. **Compare API statuses.** When backend access is available, request `GET /api/employee` without a token, with an Employee account, and with an Admin account. Record the actual status each time. The Admin page itself does not send this request.
8. **Check logout.** Log out, inspect Application/Storage for the current tab, and try `/dashboard` and `/admin` again.

## Expected Outcomes

- Empty form fields are rejected before submission.
- Invalid credentials show the API's generic 401 message; an unavailable API shows a connection error.
- A successful login stores the token only. Logout removes it and resets React auth state.
- A signed-out user is sent to login. A non-Admin local role is sent to Forbidden for `/admin`.
- The browser's role display is not evidence that the API would authorize the user.
- Backend status checks run only when the API, SQL Server, HTTPS certificate, CORS, and local accounts are configured.

## Acceptance Checklist

- [ ] Empty username or password does not submit.
- [ ] Loading and login error states are clear.
- [ ] No password is stored or logged.
- [ ] The token is stored under the existing session key and removed on logout.
- [ ] Anonymous `/dashboard` access returns to login.
- [ ] `/admin` checks the exact `Admin` role for UI routing.
- [ ] A 403 does not become an automatic logout.
- [ ] I can explain why backend authorization remains necessary.

Instructor hints are in [Exercise Hints](../instructor/Exercise-Hints.md). Try the exercise first.
