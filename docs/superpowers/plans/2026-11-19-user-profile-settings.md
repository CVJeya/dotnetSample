# User Profile Page Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a read-only “My Profile” page to both clients, opened by clicking the signed-in username and showing that account's details.

**Architecture:** Add an authenticated `GET /api/users/me` endpoint that looks up the user from the JWT's `ClaimTypes.NameIdentifier` claim and returns only safe profile fields. Both clients add a protected `/profile` route, link the navbar username to it, and display the endpoint response using their existing UI patterns.

**Tech Stack:** ASP.NET Core minimal APIs, Entity Framework Core, JWT bearer authentication, React, Redux Toolkit, React Router, Angular, RxJS, Vitest.

**Spec:** `docs/superpowers/specs/2026-11-19-user-profile-settings-design.md`

## Global Constraints

- The signed-in username in each client links to `/profile`.
- The profile page is read-only and shows Name, Email, Account ID, and Member since.
- `GET /api/users/me` resolves the user from the validated JWT identity, not from an ID supplied by the browser.
- The endpoint returns `id`, `name`, `email`, and `createdAt`; it never returns `PasswordHash`.
- No database migration or profile-editing behavior is added.
- Existing user-management behavior remains unchanged.

## Review Focus

- Anonymous or invalid-token request to `/api/users/me` must return 401; verify both in the API HTTP smoke test.
- Alice and Bob tokens must each return only the account identified by that token; verify with both login responses and `/me` requests.
- A valid token whose account row is gone must return 404; verify with a disposable development account.
- Profile API failures must render a visible error instead of leaving a blank page; cover in the Angular component test and React browser smoke test.
- Direct navigation to `/profile` while signed out must follow the existing redirect to `/login`; verify in both clients.

---

### Task 1: Add the current-user API endpoint

**Files:**
- Modify: `backend/AngularDotnetDemo.Api/Program.cs`
- Modify: `backend/AngularDotnetDemo.Api/AngularDotnetDemo.Api.http`

**Interfaces:**
- Produces: `GET /api/users/me`, requiring the existing JWT authorization. A successful response has `id: number`, `name: string`, `email: string`, and `createdAt: string` in JSON camel case.
- Error responses: 401 when the authenticated principal has no valid user ID claim; 404 when that ID has no user row.

- [ ] **Step 1: Add API smoke requests**

Add requests to `AngularDotnetDemo.Api.http` for:
- Logging in as Alice and Bob using the existing seeded demo credentials, then calling `/api/users/me` with each bearer token.
- Calling `/api/users/me` without a bearer token.
- Calling `/api/users/me` with an invalid bearer token.
- Calling `/api/users/me` with a token for a disposable account after deleting only that account in the local development database.

Copy each token from its login response into the corresponding bearer header before running the profile request. Expected assertions: Alice and Bob each receive their own `id`, `name`, `email`, and `createdAt`; the JSON has no `passwordHash`; anonymous and invalid-token requests return 401; the deleted account returns 404.

- [ ] **Step 2: Run the smoke requests before implementing the endpoint**

Run the API with the local development database and required `DATABASE_URL` and `Jwt__Key` configuration, then run the new requests.

Expected: the successful `/me` requests do not yet return 200 because the endpoint is not implemented.

- [ ] **Step 3: Implement `GET /api/users/me` in `Program.cs`**

Map the endpoint in the existing protected `/api/users` group. Parse the current user's ID from `ClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)`, return 401 if it is missing or invalid, and query with `AsNoTracking()` while projecting only `Id`, `Name`, `Email`, and `CreatedAt`. Return 404 when no row matches.

- [ ] **Step 4: Run API smoke requests and build**

Run: `dotnet build backend\AngularDotnetDemo.Api\AngularDotnetDemo.Api.csproj`

Expected: build succeeds and the HTTP assertions from Step 1 pass. Existing `/api/users/{id}` behavior is unchanged.

### Task 2: Add the React profile experience

**Files:**
- Modify: `ReactDotNetDemo/src/features/users/usersAPI.js`
- Modify: `ReactDotNetDemo/src/features/users/usersSlice.js`
- Create: `ReactDotNetDemo/src/features/users/Profile.jsx`
- Modify: `ReactDotNetDemo/src/App.jsx`
- Modify: `ReactDotNetDemo/src/components/Navbar.jsx`

**Interfaces:**
- Consumes: `GET /api/users/me` from Task 1.
- Produces: `usersAPI.getCurrentUser(): Promise<{ id: number, name: string, email: string, createdAt: string }>`, `fetchCurrentUser`, `selectCurrentUser`, `selectCurrentUserStatus`, and `selectCurrentUserError`.
- Profile request state is separate from the existing selected-user detail state.

- [ ] **Step 1: Add `usersAPI.getCurrentUser()`**

Request `${BASE_URL}/me` using `getAuthHeaders()` and throw an error when the response is not successful, following the existing `usersAPI` conventions.

- [ ] **Step 2: Add profile Redux state and thunk**

Add `currentUser`, `currentUserStatus`, and `currentUserError` to the users slice. Implement `fetchCurrentUser` with pending, fulfilled, and rejected handling, plus the three selectors listed above.

- [ ] **Step 3: Create `Profile.jsx`**

Load `fetchCurrentUser` on mount. Render a loading state while pending, a visible error on failure, and a read-only “My Profile” view with the four equally weighted fields on success. Reuse the existing detail-card and detail-row styles; do not show edit or delete actions.

- [ ] **Step 4: Add the protected route and navbar link**

Add `/profile` under `ProtectedRoute` in `App.jsx`. Render the authenticated username in `Navbar.jsx` as a React Router link to `/profile`; leave logout behavior unchanged.

- [ ] **Step 5: Verify the React client**

Run from `ReactDotNetDemo`:

```powershell
npm run lint
npm run build
```

Expected: both commands pass. With the API running, manually verify that clicking the signed-in username opens `/profile`, the four fields match `/api/users/me`, a request failure is visible, and signed-out direct navigation redirects to `/login`.

The React project has no test runner or component-test dependencies. Do not add a test stack for this feature; use the browser smoke checks above together with the existing lint and build commands.

### Task 3: Add the Angular profile experience

**Files:**
- Modify: `frontend/src/app/services/user.service.ts`
- Create: `frontend/src/app/components/profile/profile.ts`
- Create: `frontend/src/app/components/profile/profile.html`
- Create: `frontend/src/app/components/profile/profile.spec.ts`
- Modify: `frontend/src/app/app.routes.ts`
- Modify: `frontend/src/app/app.html`

**Interfaces:**
- Consumes: `GET /api/users/me` from Task 1 and the existing `User` model.
- Produces: `UserService.getCurrentUser(): Observable<User>` and a standalone `ProfileComponent`.

- [ ] **Step 1: Add the profile component tests**

Use the existing Angular/Vitest setup and HTTP testing utilities. Add tests that:
- `requestsCurrentUserProfile`: assert the component requests `GET ${environment.apiUrl}/users/me`; flush `{ id: 1042, name: "Avery Morgan", email: "avery@example.com", createdAt: "2026-11-18T00:00:00Z" }`; assert the page displays the name, email, account ID, and member-since date.
- `showsLoadingWhileProfileRequestIsPending`: hold the HTTP request open and assert the loading indicator is visible before flushing it.
- `showsErrorWhenProfileRequestFails`: flush an HTTP 500 response and assert a visible profile error.

- [ ] **Step 2: Run the focused component test and verify it fails**

Run from `frontend`:

```powershell
npm test -- --watch=false --include src\app\components\profile\profile.spec.ts
```

Expected: the test fails because the profile component and service method do not yet exist.

- [ ] **Step 3: Add `UserService.getCurrentUser()`**

Return `this.http.get<User>(`${this.apiUrl}/me`)`; the existing auth interceptor adds the bearer token.

- [ ] **Step 4: Implement the standalone profile component and template**

Create `ProfileComponent` with loading, user, and error signals. Fetch through `UserService.getCurrentUser()` on initialization. Render the same four read-only fields and use the existing global `detail-card`, `detail-row`, `detail-label`, `detail-value`, `loading`, and `error` styles.

- [ ] **Step 5: Add the guarded route and clickable username**

Add a lazy-loaded `/profile` route protected by `authGuard`. Change the authenticated username in `app.html` from a span to a RouterLink for `/profile`; keep logout behavior unchanged.

- [ ] **Step 6: Run Angular tests and build**

Run from `frontend`:

```powershell
npm test -- --watch=false --include src\app\components\profile\profile.spec.ts
npm run build
```

Expected: focused profile tests pass and the production build succeeds. With the API running, verify clicking the username opens `/profile` and signed-out direct navigation redirects to `/login`.
