# Day 7 — Review Questions

Answer without opening the answer key first.

1. What is the difference between authentication and authorization?
2. Which endpoint does the login form call, and which two fields does it send?
3. Which fields does the backend return after a successful login?
4. What status and generic message does the backend return for invalid login credentials?
5. Which key stores the token, and why does the app not store the password?
6. What header does the Axios request interceptor add?
7. How does the app treat a login `401`, a protected-request `401`, and a `403` differently?
8. What is the JWT role claim's wire name and format? What are the exact role values?
9. Why can the client display a role but not use that role as security enforcement?
10. What happens when a signed-out user opens `/dashboard`, and what does the backend require for `/api/employee`?

## Self-Check

- [ ] I can trace login from the form to `AuthProvider` and back to React state.
- [ ] I can explain why `sessionStorage` has XSS risk.
- [ ] I can explain why 401 and 403 are different.
- [ ] I can explain which component protects `/admin` and why the API still checks Admin.

The instructor answer key is separate: [Review Answers](../instructor/Review-Answers.md).
