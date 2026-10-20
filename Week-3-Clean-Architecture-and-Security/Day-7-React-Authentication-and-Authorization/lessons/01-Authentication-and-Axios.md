# Lesson 1 — Authentication Concepts and Axios Setup

**Session block:** 00:00–00:45 (45 minutes)

**Goal:** Explain who authentication and authorization are for, configure the shared Axios client, and make one small request to a verified endpoint.

## What You Will Learn

- Authentication asks, “Who signed in?”
- Authorization asks, “What may this signed-in user do?”
- Axios sends HTTP requests from React to the API.
- An Axios instance keeps the API base URL in one place.
- An HTTP status such as `401` is still a response from the API.

## What Is React?

React is a JavaScript library for building user interfaces. A React app is a tree of small pieces called components; each component is a JavaScript function that returns JSX, an HTML-like description of what should appear on screen. React re-runs a component when its data changes and updates the page to match. Day 6 of this course introduced React, JSX, `useState`, and fetching an API from a React page; if you have never used React, read or re-read [Day 6 — React API Consumption](../../Day-6-React-API-Consumption/lessons/01-React-Foundation.md) before this lesson. Day 7 adds one new idea on top: sharing sign-in state across the whole app.

## 1. Start the React App

From this folder, copy `.env.example` to `.env.local`, then run:

```bash
npm install
npm run dev
```

Open `http://localhost:3000`. The current app shows the login page. It does not send a request until a student submits the form or runs the short request check below.

## 2. Separate Authentication from Authorization

The API accepts a username and password at login. If they are correct, it returns a JWT. That answers the authentication question. Later, the API checks the token's role before it allows an Employee or Department request. That answers the authorization question.

The browser may hide a page to make the app easier to use. The backend must still check the token and role. A page hidden by React is not protected data.

## 3. Find the Axios Configuration

Open these files:

| File | Purpose |
|---|---|
| `vite.config.js` | Runs the Vite development server on port `3000`. |
| `.env.example` | Shows the local API base URL. |
| `src/api/apiClient.js` | Creates the shared Axios instance. |
| `src/main.jsx` | Starts React and wraps the app in its providers. |
| `src/App.jsx` | Defines the React routes. |

The current client is short:

```js
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
});
```

The local base URL is `https://localhost:7210/api`. Code can then request `/employee`; Axios combines that path with the base URL. Do not add `/api` to the request path, or the URL will contain it twice.

The browser origin is `http://localhost:3000`. The API's current CORS configuration allows this origin. The HTTPS development certificate must also be trusted before the browser can read an HTTPS response.

## 4. Send a First Axios GET

The backend already has `GET /api/employee`. It requires the `Admin` role, so it is useful for seeing a real protected response. Sign out first so the request has no token.

In the browser console on the Vite page, run:

```js
const { apiClient } = await import("/src/api/apiClient.js");

try {
  const response = await apiClient.get("/employee");
  console.log(`HTTP ${response.status}`);
} catch (error) {
  const status = error.response?.status ?? "No HTTP response";
  console.log(`Result: ${status}`);
}
```

If the API is running and CORS and HTTPS are configured, an anonymous request should show `HTTP 401`. If the API is unavailable, it shows `No HTTP response`. The code logs only a status, never a token or credential.

## Hands-On Task

1. Confirm `.env.local` contains the API URL from `.env.example`.
2. Start Vite and find the page at port `3000`.
3. Read `apiClient.js` and point to `axios.create()` and its `baseURL`.
4. Run the GET check above when the backend is ready.

**Expected behavior:** The login page appears. The optional GET returns `401` when the API is reachable and no token is present. A network, certificate, CORS, or server problem produces no HTTP response.

## Common Beginner Mistakes

- **Adding `/api` twice:** `baseURL` already ends in `/api`; request `/employee`.
- **Changing the Vite port:** port `3000` matches the current API CORS origin.
- **Treating 401 as a network failure:** 401 is an HTTP response. A missing response points to a connection, certificate, or CORS problem.
- **Putting credentials in `.env.local`:** Vite variables are browser-visible. Keep usernames and passwords out of frontend configuration.

## Quick Check

1. What does authentication answer?
2. Why does the first GET return 401 without a token?
3. Where does Axios get its API base URL?

Continue to [Lesson 2 — Login and AuthContext](02-Login-and-AuthContext.md).
