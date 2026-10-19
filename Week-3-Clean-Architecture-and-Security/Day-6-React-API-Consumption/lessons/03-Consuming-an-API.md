# Lesson 3 — Consuming an API

**Session block:** 45 minutes

**Goal:** Understand an HTTP GET request and read JSON from JSONPlaceholder with `fetch`.

## 1. Learn the Request Words

- An **API** lets two applications exchange information.
- A **request** asks an API for information.
- A **response** is the API's reply.
- An **endpoint** is the address for a particular API resource.
- **JSON** is a text format that represents data as names and values.
- **HTTP GET** is the basic request used to ask for data.

Our endpoint is:

`https://jsonplaceholder.typicode.com/users`

JSONPlaceholder is a public demo API. It returns sample users and does not require a backend, account, or API key.

## 2. Inspect the Response

Open the endpoint in a browser. Find one user and look for `id`, `name`, `email`, and `address.city`. One user record looks like this:

```json
{
  "id": 1,
  "name": "Leanne Graham",
  "email": "Sincere@april.biz",
  "address": { "city": "Gwenborough" }
}
```

The full response is an array of user records. JSON names such as `name` become JavaScript properties that the app can read.

**Student checkpoint:** Point to a user's name, email, and city in the response.

## 3. Request the Users with `fetch`

`fetch` is built into the browser. It sends the GET request and gives the app a response:

```js
fetch("https://jsonplaceholder.typicode.com/users")
  .then((response) => {
    // fetch can return 404 without treating it as a failed request.
    if (!response.ok) {
      throw new Error("The server could not load users.");
    }

    return response.json();
  })
  .then((data) => console.log(data));
```

`response.ok` is true for a successful HTTP status. `fetch` does not reject automatically for a status such as 404, so the app checks it. `response.json()` reads the response body and turns the JSON text into JavaScript data. The second `then` receives the user array as `data`.

The instructor can type this small request in the browser's developer console. Students can also follow the same `/users` request in **Developer Tools → Network** after the app loads.

**Student checkpoint:** Find the user array in the console or the response panel. JSONPlaceholder normally returns 10 users.

## 4. Give the Data to React

Lesson 2 introduced the `setUsers` state setter. Once the response becomes JavaScript data, the app can save it:

```js
.then((data) => {
  setUsers(data);
});
```

The setter replaces the local sample list with the API list. React then renders cards from the new `users` state.

Do not put `fetch` directly in the component body. React runs that body when it renders, and state changes cause another render. Lesson 4 puts this request in `useEffect` so it runs at the right time.

## Practice Check

1. Open the endpoint and find one user's city.
2. Find the `fetch` line and read the endpoint from left to right.
3. Explain why the response is checked before reading JSON.
4. Explain what `setUsers(data)` changes.

Continue to [Lesson 4 — useEffect and Request States](04-useEffect-and-Request-States.md).
