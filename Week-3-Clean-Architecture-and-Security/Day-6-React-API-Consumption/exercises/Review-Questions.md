# Day 6 — Review Questions

Answer these questions without looking at the code first. The instructor answer key is hidden in the section at the end.

## Student Questions

1. What is an API request? What is an API response?
2. What is JSON, and why does this app receive users as JSON?
3. What values does `useState` store in the finished app?
4. What does `setUsers(data)` do?
5. What does `fetch("https://jsonplaceholder.typicode.com/users")` request?
6. Why does the app check `response.ok`?
7. What does `response.json()` do?
8. What do the loading and error states tell the user?
9. What does `users.map()` do, and why does each card need `key={user.id}`?
10. In this app, what does the empty dependency array `[]` mean?

## Quick Self-Check

- [ ] I can describe the request and response.
- [ ] I can explain how the API data becomes user cards.
- [ ] I can explain when the app displays loading and error messages.
- [ ] I can explain why the app uses `useEffect` and `[]`.

<details>
<summary>Instructor Answer Key — open after the student activity</summary>

1. A request asks the API for information. A response is the API's reply.
2. JSON is a text format for data. The API sends user records in JSON so the browser can read them.
3. `users` stores the user list, `loading` stores whether the request is still running, and `error` stores an error message.
4. It replaces the current user-list state with the users returned by the API. React then renders the new list.
5. It sends a GET request for the sample users at the JSONPlaceholder `/users` endpoint.
6. `fetch` does not reject automatically for HTTP errors such as 404. `response.ok` tells the app whether the HTTP response succeeded.
7. It reads the response body as JSON and returns JavaScript data the app can use.
8. Loading tells the user the request is in progress. Error tells the user the request did not succeed.
9. `map()` makes a rendered card for each user. `key={user.id}` gives each card a stable identity in the list.
10. The effect has no changing values to watch, so it runs when the component appears. React Strict Mode may run it more than once in development.

</details>
