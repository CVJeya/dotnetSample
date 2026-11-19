# User Profile Page Design

## Intent

Let a signed-in user view their own account details on a separate page in both the React and Angular clients. The username in each authenticated navbar opens this page. The shared .NET API supplies the current account's data.

## Goals

- Provide the same read-only profile experience in React and Angular.
- Display Name, Email, Account ID, and Member since with equal emphasis, following layout B from the brainstorming mockup.
- Link the signed-in username in each navbar to the profile page.
- Resolve the profile from the authenticated identity, not from a user ID supplied by the browser.
- Show loading and request errors clearly.

## Non-goals

- Editing profile information, changing passwords, or managing account settings.
- Changing registration, login, logout, or the existing user-management pages.
- Adding profile fields, avatar uploads, or database schema changes.
- Restricting or otherwise changing the existing user-management API.

## Constraints

- Both frontends use the existing `AngularDotnetDemo.Api` backend and its JWT authentication.
- The JWT contains a `ClaimTypes.NameIdentifier` claim for the user's database ID.
- The existing protected `GET /api/users/{id}` route returns profile fields but accepts a client-supplied ID. The profile page must not use that route to identify the current account.
- The user entity contains `Id`, `Name`, `Email`, `PasswordHash`, and `CreatedAt`. Only the first four, excluding the password hash, are suitable for the profile response.
- Keep each frontend aligned with its existing routing, authentication, and styling patterns.

## Approaches

1. **Use the browser's saved auth user.** This avoids a new request but the saved data can be stale and does not include `CreatedAt`.
2. **Call `GET /api/users/{id}` with the ID saved in the browser.** This reuses the existing response, but depends on a client-supplied identity and the API currently permits any authenticated user to request any ID.
3. **Add `GET /api/users/me` and resolve the account from the validated JWT.** This adds a small API endpoint, gives both clients fresh data, and ties the response to the authenticated user without trusting a URL or request-body ID.

**Selected:** Approach 3. It best meets the requirement that a user see their own account details in both clients.

## Design

### API

Add an authenticated `GET /api/users/me` endpoint. It reads the account ID from the validated JWT's `ClaimTypes.NameIdentifier` claim, loads that account, and returns a safe profile object containing `id`, `name`, `email`, and `createdAt`. It must not return `PasswordHash`. The endpoint accepts no account ID from the client. A missing account should produce a not-found response; malformed or missing identity handling can follow the existing API conventions.

No database migration is needed. The existing user list, user-by-ID, create, update, and delete behavior remains unchanged.

### React client

- Add a `/profile` route wrapped in `ProtectedRoute`.
- Change the authenticated navbar username into a link to `/profile`; keep logout behavior separate and unchanged.
- Add a profile page that requests `/api/users/me` with the existing authorization header helper.
- Render the four fields as equally weighted account details, with a clear page title and loading/error states.

### Angular client

- Add a `/profile` route protected by the existing `authGuard`.
- Change the authenticated navbar username into a link to `/profile`; keep logout behavior separate and unchanged.
- Add an auth-aware service method for `GET /api/users/me` and a profile page that uses it.
- Render the same four fields and loading/error states as the React client.

### User experience

The page is titled **My Profile** and is read-only. The username is clickable only while authenticated. Visiting `/profile` while signed out follows each client's existing protected-route behavior. Format the account creation date using the frontend's existing date-display conventions.

## Calls left to the builder

- Follow current file organization and style conventions in each client.
- Choose whether to share a profile model/type or define one locally in each frontend, as appropriate to the existing code.
- Use the smallest focused tests covering endpoint identity scoping and both clients' profile navigation/data states.
