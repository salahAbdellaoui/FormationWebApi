# 03 — Step-by-Step Practice

Follow these steps to run the existing React app and trace the API data to the page.

## Run the App

1. Open a terminal at the repository root.
2. Move into the Day 6 app folder:

   ```bash
   cd Week-3-Clean-Architecture-and-Security/Day-6-React-API-Consumption
   ```

3. Install the app's packages:

   ```bash
   npm install
   ```

4. Start the development server:

   ```bash
   npm run dev
   ```

5. Open the local address shown in the terminal. Keep the terminal open while you use the app.

## Follow the Request

Open `src/App.jsx` and follow these steps:

1. The app starts with an empty `users` list and `loading` set to `true`.
2. `useEffect` starts `fetch` for `https://jsonplaceholder.typicode.com/users`.
3. The API sends back a response containing sample users in JSON.
4. The app checks `response.ok`, then reads the JSON with `response.json()`.
5. `setUsers(data)` saves the returned list in React state.
6. `users.map()` makes a card for each user. Each card shows the user's name, email, and city.
7. `finally` sets `loading` to `false`, whether the request worked or failed.

To watch the request in your browser, open Developer Tools, select **Network**, and reload the page. Find the request whose address ends with `/users`.

## Check Your Result

- [ ] The page first shows `Loading users...`.
- [ ] The page displays user cards after the request succeeds.
- [ ] Each card shows a name, email, and city.
- [ ] An error message appears if the request fails.

Stop the development server with `Ctrl + C` in the terminal.
