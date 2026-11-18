# Chapter 4 — React State, Routing, Architecture & the Angular-vs-React Decision

> Files: `ReactDotNetDemo/src/app/store.js`, `app/hooks.js`, `features/*/*Slice.js`, `features/*/*API.js`, `App.jsx`, `components/ProtectedRoute.jsx`

---

## 1. State management — the decision tree

This is the question that most reliably separates mid from senior, in **both** frameworks. Learn a decision tree and recite it.

```
Is the state used by exactly one component?
├─ YES → local state (useState / signal in the component)
└─ NO, used by a few nearby components
   ├─ Parent/child → lift to nearest common parent, pass props (props drilling)
   └─ Deep tree, same feature → Context (React) / shared service with signals (Angular)
      NO, used across features, or the server is the source of truth
      ├─ Client state to be cached/invalidated → RTK Query / TanStack Query
      └─ Genuine app state (auth, cart, wizard) → Redux Toolkit / signal store
```

**The trap:** "we put everything in Redux". Say this instead:
> "Redux is for state that is shared across features and needs time-travel/debuggability or middleware. Server data doesn't belong in a hand-rolled store — it belongs in a cache with staleness semantics (RTK Query / TanStack Query). And a modal's open flag belongs in the component that renders it. Over-centralling state is a common failure mode I've had to unwind."

That answer is senior because it's about *boundaries*, not APIs.

---

## 2. Redux Toolkit in this repo

### 2.1 The store (`src/app/store.js`)

```js
import { configureStore } from '@reduxjs/toolkit';
import usersReducer from '../features/users/usersSlice';
import authReducer from '../features/auth/authSlice';
import productsReducer from '../features/products/productsSlice';

export const store = configureStore({
  reducer: { users: usersReducer, auth: authReducer, products: productsReducer },
});
```

`configureStore` gives you, for free: `redux-thunk` middleware, immutability + serialisability checks in dev, Redux DevTools wiring. Compare with the pain of legacy `createStore` + manual `applyMiddleware` + `combineReducers`.

State shape follows the reducer map: `state.auth`, `state.products`, `state.users`.

### 2.2 The typed hooks (`src/app/hooks.js`)

```js
import { useDispatch, useSelector } from 'react-redux';

export const useAppDispatch = () => useDispatch();
export const useAppSelector = useSelector;
```

Purpose: one place to add types (`.withTypes<RootState, AppDispatch>()` in TS) so components don't repeat generics and so `RootState` stays a single source of truth. Even in JS, the wrapper is where you'd add logging or a selector lint rule.

### 2.3 A slice (`src/features/products/productsSlice.js`)

```js
import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { productsAPI } from './productsAPI';

export const fetchProducts = createAsyncThunk('products/fetchAll', async (_, { rejectWithValue }) => {
  try {
    return await productsAPI.getAll();
  } catch (err) {
    return rejectWithValue(err.message);
  }
});

const slice = createSlice({
  name: 'products',
  initialState: { items: [], status: 'idle', error: null },
  reducers: {
    productAdded(state, action) { state.items.push(action.payload); },   // Immer lets you "mutate"
    productRemoved(state, action) {
      state.items = state.items.filter(p => p.id !== action.payload);
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchProducts.pending,   (s) => { s.status = 'loading'; s.error = null; })
      .addCase(fetchProducts.fulfilled, (s, a) => { s.status = 'succeeded'; s.items = a.payload; })
      .addCase(fetchProducts.rejected,  (s, a) => { s.status = 'failed'; s.error = a.payload ?? a.error.message; });
  },
});

export const { productAdded, productRemoved } = slice.actions;
export default slice.reducer;
```

### The six things to explain about this file

| Concept | What to say |
|---|---|
| `createSlice` | Generates action creators + action types + a reducer from one object. Replaces three files of boilerplate. |
| **Immer** | `state.items.push(...)` looks like mutation but Immer records the draft and produces a new immutable state. **Never** mutate state outside a slice. |
| `createAsyncThunk` | Three lifecycle actions (`pending`/`fulfilled`/`rejected`) so loading and error states aren't hand-written booleans. |
| `rejectWithValue` | Controls what lands in `action.payload` on failure (otherwise you get a serialised error). Makes error UI predictable. |
| `status: 'idle' \| 'loading' \| 'succeeded' \| 'failed'` | A single enum beats `isLoading` + `hasError` + `isDone`, which can contradict each other. **Senior point:** impossible states should be unrepresentable. |
| Selectors | Components read `useAppSelector(s => s.products.items)`; deriving filtered lists in a selector (or `createSelector`) keeps components dumb and memoised. |

### 2.4 The API layer (`features/products/productsAPI.js`)

Keep `fetch` out of components and out of the slice body. One thin module: build URL, attach token, parse JSON, throw on non-2xx.

**Why it matters:** if it's in the component you can't test it, and if it's in the slice you can't reuse it. This three-layer split — **component → slice/thunk → API module** — is the same layering you already do on the backend (controller → service → repository). Say the analogy:

> "It's the same layering I use server-side: controller talks to a service, service talks to a repository. Here: component talks to a thunk, thunk talks to an API client. The rule is the same — the layer that decides *what* never talks HTTP, and the layer that speaks HTTP never decides *why*."

That framing turns your .NET experience into a front-end strength. Use it repeatedly.

### 2.5 Thunk vs RTK Query

| | `createAsyncThunk` (this repo) | RTK Query |
|---|---|---|
| Caching | Manual | **Automatic**, keyed by endpoint + args |
| Dedupe/refetch | Manual | Built-in (`refetchOnFocus`, `pollingInterval`) |
| Loading state | You write `pending/fulfilled/rejected` | `isLoading`, `isFetching` per hook |
| Invalidations | Manual | `providesTags` / `invalidatesTags` |
| When to choose | Complex workflows, non-HTTP side effects, or learning the primitives | **Everything CRUD** — this is the modern default |

**Say:** "This demo uses `createAsyncThunk` because it shows the mechanics, but in a new project I'd reach for RTK Query — or TanStack Query if I weren't already on Redux — because cache invalidation and request dedupe are the bugs I don't want to re-implement."

---

## 3. Routing (react-router-dom 7, `App.jsx`)

```jsx
<BrowserRouter>
  <Routes>
    <Route path="/" element={<Layout />}>
      <Route index element={<Navigate to="/products" replace />} />
      <Route path="login" element={<Login />} />
      <Route path="register" element={<Register />} />
      <Route element={<ProtectedRoute />}>
        <Route path="products" element={<ProductList />} />
        <Route path="products/new" element={<ProductForm />} />
        <Route path="users/:id" element={<UserDetail />} />
      </Route>
      <Route path="*" element={<NotFound />} />
    </Route>
  </Routes>
</BrowserRouter>
```

- **Layout route** (`<Route element={<Layout/>}>` + `<Outlet/>` inside `Layout`) — the analogue of Angular's parent route with `<router-outlet>`. Navbar renders once; children swap.
- **Pathless guard route** — `<Route element={<ProtectedRoute/>}>` wraps children. `ProtectedRoute` renders `<Outlet/>` if allowed, else `<Navigate to="/login" replace />`. This is the React equivalent of `canActivate`.
- `Navigate ... replace` prevents a back-button loop (the same reasoning as returning `UrlTree` in Angular).
- `useParams()`, `useSearchParams()`, `useNavigate()`, `useLocation()` are the hooks.
- **`<Link to>` not `<a href>`** — an `<a>` triggers a full document reload and throws away all state. This is the same category of mistake as unbinding your jQuery handlers on a partial page update.

**`BrowserRouter` vs `HashRouter`:** BrowserRouter needs the server to serve `index.html` for unknown paths (a rewrite rule — `try_files $uri /index.html` in nginx, or `MapFallbackToFile("index.html")` in ASP.NET Core). HashRouter works without server config but has ugly `#/` URLs. Interview answer: "I'd configure the fallback — it's one line in nginx — because shareable clean URLs matter."

**Lazy routes:**

```jsx
const ProductList = lazy(() => import('./pages/ProductList'));
<Suspense fallback={<Spinner />}><Routes>…</Routes></Suspense>
```

The direct analogue of `loadComponent`. Same chunking story, different API.

---

## 4. Frontend architecture that a senior should propose

### Feature-folder structure (what this repo does)

```
src/
  app/            store.js, hooks.js                ← cross-cutting
  components/     Layout, Navbar, Login, …          ← shared/generic
  features/
    auth/         authSlice.js, Login.jsx
    products/     productsSlice.js, productsAPI.js, ProductList.jsx, ProductForm.jsx
    users/        usersSlice.js, usersAPI.js, UserList.jsx, UserForm.jsx, UserDetail.jsx
```

Rule: **a feature folder owns its slice, its API client, its components, and its tests.** Cross-feature imports go through `components/` or a shared `lib/`. This makes deletion safe (delete a folder) and code-review scoping obvious. The Angular equivalent is a lazy-loaded route folder per feature.

### Standards you'd set as the senior in the room

| Concern | Recommendation |
|---|---|
| Structure | Feature folders, not type folders (`/components`, `/services` at root scale badly) |
| API access | One client module per feature; never `fetch` in a component |
| Error surfacing | Central error boundary + a toast channel; never `alert()` |
| Env config | Build-time `import.meta.env.VITE_*` (Vite only exposes `VITE_`-prefixed vars) and a relative `/api` base URL so one artifact works in every environment |
| Accessibility | Semantic HTML first; `label` bound to `input`; `aria-live` for async errors; keyboard path for every action |
| Testing | Vitest/RTL for behaviour, MSW for network, Playwright for one smoke journey |
| Lint | Oxlint this repo (fast, Rust); ESLint + `react-hooks` plugin if you need the full rule set |
| Type safety | TS + `strict`; validate API payloads at the boundary |

### Vite specifics worth knowing

`VITE_` prefix is mandatory for client exposure — **anything else stays server-side**. That's a security default. And `import.meta.env` is statically replaced at build time, so "runtime config" needs a `config.json` fetch or a container-generated env file. Same problem as Angular's `environment.ts`, and the same answer: keep URLs relative, let the edge route them.

---

## 5. Angular vs React — the comparison table to memorise

Interviewers love this because it reveals whether you chose a framework or defaulted to one.

| Dimension | Angular | React |
|---|---|---|
| Nature | **Batteries-included framework** | **Library** + ecosystem choices |
| Language | TypeScript first-class, required | JSX (TS optional but strongly advised) |
| Templates | HTML files with Angular syntax | JSX inside the component |
| State | Signals (+ RxJS) | Hooks (`useState`) |
| Async | RxJS Observables, rich operators | Promises + `async/await`, hooks |
| DI | Built-in hierarchical injector | None (Context + imports) |
| Change detection | Zone/signals; `OnPush` manual | Whole subtree re-render + memoisation |
| Data binding | Two-way (`[(ngModel)]`) or reactive forms | One-way; forms via libraries |
| Forms | Built-in, two systems | Unopinionated (react-hook-form etc.) |
| Routing | Built-in `@angular/router` | `react-router` (v7 here) |
| State mgmt | Services + signals, or NgRx | Redux Toolkit / TanStack Query / Zustand |
| Learning curve | Steep, then very consistent | Shallow, then many decisions |
| Structure | Opinionated — every Angular app looks alike | Team-defined; needs discipline |
| Testing | Jasmine/Karma → **Vitest** now | Jest/Vitest + React Testing Library |
| Bundle | Framework baseline is larger | Smaller baseline, more per-decision variance |
| Best fit | Large teams, long-lived enterprise, .NET shops | Product teams, fast iteration, broad hiring pool |

### The "why did you build both?" answer (rehearse this)

> "They solve the same problem with opposite philosophies. Angular gives you one blessed way to do routing, DI, HTTP, and forms — which is why it's excellent for a large team that turns over, and it maps naturally onto a .NET shop: services with lifetimes, DI, interceptors that look like middleware, decorators that look like attributes. React is a rendering library plus a set of choices — you pick the router, the store, the form library — which means a smaller core, faster onboarding for one strong developer, and a larger hiring pool, but the architecture becomes *your* job as the senior.
>
> In this repo I built the same product CRUD both ways so I could compare them concretely: Angular's `ProductService` with `Observable<Product[]>` and a guard-returning-`UrlTree`, versus React's `productsSlice` + `createAsyncThunk` + a `ProtectedRoute` wrapper. The surprising thing is how much the *concepts* map — guards ↔ `ProtectedRoute`, interceptors ↔ fetch wrapper middleware, `providedIn:'root'` ↔ store singleton, `computed` ↔ `useMemo`, `track` ↔ `key`."

That last paragraph is worth more than any amount of API trivia. It's the whole point of owning this repo.

---

## 6. Front-end testing — what a senior actually proposes

| Layer | What to test | Tool | Proportion |
|---|---|---|---|
| Pure functions / reducers / validators | Logic | Vitest | most |
| Services / hooks / slices | State transitions + async flows | Vitest + MSW | many |
| Components | **Behaviour via roles/labels**, not internals | React Testing Library / Angular TestBed | some |
| E2E | One happy path per critical journey | Playwright | few |

**The RTL philosophy (say this):** "Test the way the user interacts — get by role, label, or text — not by CSS class or component internals. If a refactor breaks my test but not the app, the test was wrong."

**What to never do:** snapshot everything (they get rubber-stamped), assert on implementation details (`state.isLoading === true` instead of "a spinner is visible"), or mock so heavily the test only proves the mocks work.

---

## 7. Accessibility & security — the front-end senior differentiators

### Accessibility minimums (mention these unprompted and you'll stand out)

- Real elements: `<button>` for actions, `<a>` for navigation. A `div` with `onClick` is invisible to keyboard and screen-reader users.
- Every input has an associated `<label htmlFor>`, or `aria-label` — placeholders are not labels.
- Errors announced: `aria-live="polite"` region or `role="alert"`.
- Focus management on route change and on modal open/close (focus trap + return focus).
- Colour contrast ≥ 4.5:1; never communicate state with colour alone.
- Never `aria-hidden` something focusable; never `tabIndex` > 0.

### Front-end security (your backend background is an unfair advantage here)

| Risk | Reality |
|---|---|
| **XSS** | Frameworks escape by default. The danger is `dangerouslySetInnerHTML` / `bypassSecurityTrustHtml` / `innerHTML`. **Say:** "The only XSS in a modern SPA is the escape hatch you deliberately opened, usually for a rich-text field — and there I'd sanitise on the server too." |
| **Token storage** | `localStorage` is readable by any script on the origin → one XSS = full account takeover. httpOnly `Secure` `SameSite=Strict` cookies are not XSS-readable but need CSRF defence. **Name the trade-off, don't just pick.** |
| **CSRF** | Not an issue for `Authorization`-header tokens; mandatory for cookie auth (`SameSite` + anti-forgery token). |
| **Authorisation** | Front-end guards are UX. **Every** endpoint re-authorises server-side. |
| **Secrets** | Anything in the bundle is public: `VITE_*`/`environment.ts` must contain no secrets. |
| **Dependencies** | `npm audit` in CI; supply-chain risk is real (the whole reason lockfiles exist). |

**The answer that ends this topic in an interview:**
> "I treat the browser as fully untrusted. The SPA is a UX layer; the API enforces every rule and re-validates every input. That's identical to how I think about Razor Pages — the difference is only where the HTML is produced."

---

## 8. Chapter 4 — questions you must be able to answer

1. Walk the state-management decision tree. *(§1)*
2. What does `createSlice` generate, and what does Immer let you do? *(§2)*
3. Why `createAsyncThunk`'s three states instead of one `isLoading`? *(§2)*
4. Where should `fetch` live, and why not in the component or slice? *(§2.4)*
5. RTK Query vs `createAsyncThunk` — what does the cache give you? *(§2.5)*
6. Layout route vs guard route in React Router. *(§3)*
7. `<Link>` vs `<a href>` — what breaks with `href`? *(§3)*
8. What server config does `BrowserRouter` require? *(§3)*
9. Which problems does Angular solve that React makes you decide? *(§5)*
10. Which problems does React solve that Angular makes harder? *(§5)*
11. What's your front-end testing pyramid, and what do you refuse to test? *(§6)*
12. Where do you store a JWT and what's the trade-off? *(§7)*
13. Is a route guard a security control? Why not? *(§7)*
14. How do you enforce code structure on a React team? *(§4)*

---

## 30-second version

State stays as local as possible; shared client state goes to Redux Toolkit (slice + thunk, Immer for immutable updates, a `status` enum so impossible states can't exist); server data belongs in a cache (RTK Query), not a hand-rolled store. Routing is a layout route plus a **pathless guard route** rendering `<Outlet/>` or `<Navigate>`, with `BrowserRouter` requiring a server-side `index.html` fallback. Angular is the opinionated framework that mirrors .NET's DI/middleware/style; React is a renderer plus decisions you own. And regardless of framework, the browser is untrusted — the API enforces every rule.
