# Lesson 1 — React Foundation

**Session block:** 45 minutes

**Goal:** Start the existing Vite app and build a page with local sample users.

## What You Will Learn

- Find the important files in the React project.
- Start the Vite development server.
- Explain how `main.jsx` renders `App.jsx`.
- Write simple JSX for a title and user cards.

## 1. Start the App

Open a terminal at the repository root and run:

```bash
cd Week-3-Clean-Architecture-and-Security/Day-6-React-API-Consumption
npm install
npm run dev
```

Open the local URL printed by Vite. Keep the terminal open. Stop the server with `Ctrl + C` when the session ends.

**Checkpoint:** The existing Users page appears in the browser.

## 2. Find the Main Project Files

| File | Purpose |
|---|---|
| `index.html` | Provides the page and the `<div id="root">` React uses. |
| `src/main.jsx` | Starts React and tells it to render `<App />`. |
| `src/App.jsx` | Contains the page the students will build. |
| `src/index.css` | Styles the page and user cards. |
| `package.json` | Lists the commands and packages used by the app. |
| `vite.config.js` | Enables React support in Vite. |

In `src/main.jsx`, find this part:

```jsx
createRoot(document.getElementById("root")).render(
  <StrictMode>
    <App />
  </StrictMode>
);
```

`createRoot` connects React to the `root` element in `index.html`. `<App />` is the page. `StrictMode` helps show problems while developing; it does not add a visible part to the page.

## 3. Build the Page Title

Open `src/App.jsx`. Temporarily replace its contents with this small component:

```jsx
function App() {
  return (
    <main>
      <h1>Users</h1>
    </main>
  );
}

export default App;
```

`function App` creates a React component. The component returns JSX. JSX looks like HTML, but it is written inside JavaScript. `<main>` is the page content and `<h1>` is its heading. `export default App` lets `main.jsx` import the component.

**Students type:** Save the file and look at the browser.

**Checkpoint:** The browser shows only the Users heading. If it does not, check the terminal for a spelling or bracket error.

## 4. Render Local Sample Users

Add a small array above the component, then render one card for every user:

```jsx
const sampleUsers = [
  {
    id: 1,
    name: "Leanne Graham",
    email: "Sincere@april.biz",
    address: { city: "Gwenborough" },
  },
  {
    id: 2,
    name: "Ervin Howell",
    email: "Shanna@melissa.tv",
    address: { city: "Wisokyburgh" },
  },
];

function App() {
  return (
    <main>
      <h1>Users</h1>

      {sampleUsers.map((user) => (
        <article key={user.id} className="user-card">
          <h2>{user.name}</h2>
          <p>Email: {user.email}</p>
          <p>City: {user.address.city}</p>
        </article>
      ))}
    </main>
  );
}

export default App;
```

`sampleUsers` is JavaScript data stored in this file. The curly braces put JavaScript values inside JSX. `map()` makes a card for each record. Keep `key={user.id}` in place; Lesson 2 explains why React needs it.

**Students type:** Add the sample array and card markup. Do not add the API request yet.

**Checkpoint:** Two cards appear. Each card shows a name, email, and city. These users are local sample data; later lessons replace them with API data.

## Quick Check

1. Which file starts React? **Answer:** `src/main.jsx`.
2. Which file contains this page? **Answer:** `src/App.jsx`.
3. Where does the browser show the component? **Answer:** Inside the root element from `index.html`.

Continue to [Lesson 2 — useState and Rendering Lists](02-useState-and-Rendering.md).
