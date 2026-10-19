# Student Exercise — Improve the User List

Work in `src/App.jsx` and `src/index.css`. Keep the request, loading state, and error state working.

Start from the Lesson 4 checkpoint, before the workshop adds the count and empty-list message. If you use the finished app, temporarily remove those two parts first so you can add them yourself.

## Tasks

1. Remove the city from each user card. Keep the user's name and email.
2. Display the total number of users above the cards.
3. Display a friendly message when no users are available.
4. Customize the card styling. Change at least two styles, such as the border, spacing, background, or heading color.
5. Test the successful request and an error response.

## Hints

- The number of users is available as `users.length`.
- Check whether `users.length === 0` before displaying the cards.
- Keep `users.map()` for the cards when users are available.
- To test an empty response, temporarily change `setUsers(data)` to `setUsers([])`. Restore the original line after checking the message.
- To test an HTTP error, temporarily change the endpoint to `https://jsonplaceholder.typicode.com/no-such-route`. Restore the `/users` endpoint after checking the error message.

## Acceptance Criteria

- Cards show a user's name and email, with no city.
- The page displays the current total user count.
- An empty list displays a friendly message instead of an empty area.
- Your card styles are visibly different from the starter style.
- The real endpoint still loads users after you restore it.
- A failed request displays the app's error message.

## Checklist

- [ ] I removed the city from the card.
- [ ] I added the total user count.
- [ ] I handled an empty user list.
- [ ] I customized at least two card styles.
- [ ] I tested both success and failure.
- [ ] I restored the working `/users` endpoint.
