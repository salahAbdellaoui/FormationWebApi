# 01 — What Is an API?

An API is a way for two applications to communicate. One application asks for information, and another application sends a reply.

## Request and Response

- A **request** is a message that asks an API to do something or return information.
- A **response** is the API's reply. It can contain the requested information or an error.

In this lesson, the React app asks for a list of users. The API sends the list back.

## What Is JSON?

JSON is a text format used to send data between applications. It stores information as names and values. A user might look like this:

```json
{
  "id": 1,
  "name": "Leanne Graham",
  "email": "Sincere@april.biz"
}
```

The response contains a list of these user records. React turns the response into JavaScript data and displays each user.

## JSONPlaceholder

JSONPlaceholder is a public demo API. It returns sample data for practice, so this lesson does not need a real database or account.

The endpoint used by the app is:

`https://jsonplaceholder.typicode.com/users`

An **endpoint** is a specific address where an API provides information. Open this address in a browser to see the sample users.
