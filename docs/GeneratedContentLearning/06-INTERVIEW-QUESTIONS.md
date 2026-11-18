# Chapter 6 — Interview Question Bank, Model Answers & Scripts

> **Sitting a timed written assessment first?** That is a different format from this bank — see `14-WORLDBANK-FULLSTACK-ANGULAR-ASSESSMENT.md`. Come back here once you have passed it and the conversation round is scheduled.

> 130+ questions. Model answers are deliberately short — 2–5 sentences. **Read them aloud.** Written fluency does not transfer to speech without rehearsal.
>
> Legend: 🎯 = very likely · 🔥 = the question that decides senior vs mid · 💬 = the exact sentence to say
>
> **This chapter covers the *senior* round.** For the Technical Lead round — behavioural/personnel questions, the architecture case study, the team-of-3 framing and salary negotiation — go to `12-LEAD-INTERVIEW-PLAYBOOK.md`. Technical credibility gets you through Chapters 1–7; the lead round is decided by Chapters 8–13.
>
> **Language-level gate questions are in `05A` and `05B`, and the coding round is in `05C`.** §6 below is the *spoken* form of the framework questions; if the interviewer drops below the framework into the language ("struct vs class", "boxing", "`record` vs `class`", "abstract vs interface", "big-O of that"), those three chapters are the answer bank. §10 of this chapter assumes you can pass them.

---

## 1. The four answer shapes

Use these deliberately — variety stops you sounding scripted.

### Shape A — the trade-off sandwich (use constantly)
**Position → why → counter-position → when I'd flip.**
> "I default to signals over `BehaviorSubject` for component state — they're simpler and integrate with `computed` and zoneless change detection. But if I need time-based operators like `debounceTime`/`switchMap`, RxJS is still the better tool, so I bridge with `toSignal` at the edge."

### Shape B — STAR (behavioural / "tell me about a time")
Situation → Task → Action → Result, in ~90 seconds, ending with a **measurable** result and a lesson.

### Shape C — the honest boundary
> "I haven't used that in production. My mental model is X — is that right? — and I'd verify it before shipping."

Never bluff. Senior interviewers are testing integrity as much as knowledge.

### Shape D — the decision tree (for "how do you choose…")
Enumerate the branches out loud: "It depends on three things: …". Then pick one and justify it.

### The 3 rules
1. **Answer the question in the first sentence**, then contextualise. Don't build up to it.
2. **Never end on the answer** — end on a trade-off, a risk, or a "which is why I'd…".
3. **~60–90 seconds per answer.** Then stop. Let them dig.

---

## 2. JavaScript / TypeScript rapid-fire (you should win these)

| Q | Model answer |
|---|---|
| 🎯 `var` vs `let` vs `const` | `var` is function-scoped and hoisted; `let`/`const` are block-scoped with a temporal dead zone. `const` prevents rebinding, not mutation — `const arr = []` can still be pushed. I use `const` by default, `let` when reassigning, never `var`. |
| 🎯 `==` vs `===` | `==` coerces (`'0' == false` is true); `===` compares type and value. Always `===`. |
| 🎯 Closures | A function keeps a reference to its defining scope. That's how `useState` remembers state across renders, how event handlers capture values, and the cause of the stale-closure bug in `useEffect`. |
| 🎯 `this` | Determined by **call site**, not definition — except arrow functions, which capture the enclosing `this`. That's why arrow functions are mandatory for React class handlers and why RxJS callbacks behave. |
| 🎯 Event loop / microtasks | Call stack → microtasks (promises) drained fully → one macrotask (`setTimeout`) → repeat. That's why `Promise.resolve().then()` runs before `setTimeout(0)`, and why `await` doesn't block the thread. |
| 🎯 Prototypal inheritance | Objects delegate to a prototype chain; `class` is sugar over it. Methods live on `Product.prototype`, not per instance. |
| 🔥 Shallow vs deep copy | Spread/`Object.assign` copy one level — nested objects are shared, which is the source of "I updated my copy and the original changed". `structuredClone()` for a true deep copy. Framework state updates only need shallow copies at the changed level — that's the point of immutable updates. |
| 🎯 `Promise.all` vs `allSettled` vs `race` vs `any` | `all` rejects on the first failure; `allSettled` never rejects and returns per-promise status; `race` settles with the first settled; `any` resolves with the first success. |
| 🔥 Debounce vs throttle | Debounce waits for quiet (search input); throttle allows one call per interval (scroll). In Angular either `debounceTime` or a signal + timer; in React a `useEffect` with a cleanup timer or a `useDebouncedValue` hook. |
| 🎯 `null` vs `undefined` | `undefined` = not assigned; `null` = deliberately empty. Strict TS forces you to handle both. |
| 🔥 `interface` vs `type` | `interface` is open (declaration merging) and extends; `type` supports unions and mapped types. I use `type` for unions/DTO aliases, `interface` for object contracts. |
| 🔥 Structural typing | TS compares **shape**, not names. So an object literal with the right fields satisfies the interface — which is why a typo in a property name is caught at assignment but an excess-property check only fires for literals. |
| 🎯 `unknown` vs `any` | `any` turns checking off; `unknown` forces narrowing first. API payloads should be `unknown` until validated. |

---

## 3. Angular — 40 questions

### Architecture & components

1. 🎯 **Why standalone components?** They remove `NgModule` indirection, declare dependencies per component (better tree-shaking, obvious coupling), and let `loadComponent` lazy-load without a module wrapper. Modules still exist for grouping providers and for legacy code.
2. 🔥 **What does `bootstrapApplication` need the second argument for?** `ApplicationConfig.providers` — the DI composition root, the Angular equivalent of `Program.cs`. Without it you have no router, no `HttpClient`, no interceptors.
3. 🎯 **Difference between `imports` and `declarations`?** `declarations` was module-era (components belong to a module). `imports` on a standalone component lists what its *own template* uses. A missing entry produces "X is not a known element".
4. 🔥 **Component vs directive vs pipe?** A component has a template; a directive changes behaviour of an existing element (attribute/structural); a pipe transforms a value in a template and is **pure by default** (re-runs only when the input reference or primitive changes) — which is why impure pipes on large arrays are a performance trap.
5. 🔥 **`ngOnChanges` vs `ngOnInit` vs `ngAfterViewInit`?** `ngOnChanges` fires per input change (before `ngOnInit`, and only for changed inputs); `ngOnInit` once after the first input binding; `ngAfterViewInit` after the view (and `@ViewChild`) exist — DOM/third-party libraries belong here, not in the constructor.
6. 🔥 **Why is the constructor a bad place to fetch data?** It has no inputs yet and no view; it also runs before DI-heavy setup is complete. And a `subscribe` in a constructor without cleanup is a leak. Use `ngOnInit`, a resolver, or `toSignal` in a field initialiser.

### Signals

7. 🔥 **What problem do signals solve?** They make the dependency graph explicit, so change detection can be **targeted** instead of whole-tree. `computed` also gives you free memoisation of derived state.
8. 🔥 **`computed` vs a getter?** A getter runs on every change-detection pass. `computed` caches and only recomputes when a tracked dependency changes (and tracks dependencies dynamically per branch).
9. 🔥 **When should you *not* use `effect`?** Never for deriving state — use `computed`. `effect` is for syncing with imperative, non-Angular code (localStorage, chart libs, logging). Effects that write state create loops and are hard to test.
10. 🔥 **`signal.set` vs `update`?** `set` replaces; `update(fn)` derives from the previous value and is correct when the new value depends on the old (avoids the stale-read that `set` with a captured value causes).
11. 🎯 **How do you expose a signal from a service safely?** Keep the writable signal private, expose `asReadonly()` — same intent as `private set` in C#.
12. 🔥 **`toSignal` / `toObservable` — why both?** Angular's HTTP and router are Observable-native; components are heading toward signals. `toSignal` adapts at the boundary and handles unsubscription; `toObservable` lets you pipe a signal through RxJS operators (debounce a search term).
13. 🎯 **`input()` vs `@Input()`?** `input()` returns a read-only signal that composes with `computed` and removes `ngOnChanges`; `.required()` turns a missing binding into a clear runtime error. `@Input()` still works — use signals for new code.

### Change detection

14. 🔥 **How does change detection work?** Classically, Zone.js patched async APIs and, after each, Angular re-checked the whole tree (dirty-checking bindings). Signals/zoneless replace that with targeted notification of exactly the views that read the changed value.
15. 🔥 **What does `OnPush` mean and what bugs does it cause?** Only re-check on input reference change, an event from its template, an async pipe emission, or a signal read change. So in-place mutation (`arr.push`) doesn't render — you must update immutably. Same discipline React requires.
16. 🔥 **What is zoneless and why do it?** Drop `zone.js`: smaller bundle, no monkey-patching, only signal-driven renders, better SSR/core-web-vitals. Cost: any non-signal async update needs `markForCheck()`, so it's a migration, not a flag.
17. 🎯 **`ExpressionChangedAfterItHasBeenCheckedError` — cause and fix?** A binding changed after Angular's check pass (usually setting state in `ngAfterViewInit` or reading/writing a parent's value in a child). Fixes: move the work to `ngOnInit`, use `setTimeout`/`queueMicrotask` (last resort), or model it with a signal so the change is registered where it happens.

### DI

18. 🔥 **Explain the DI hierarchy and lifetime mapping.** `providedIn: 'root'` ≈ singleton; route/component `providers` ≈ scoped to that subtree's lifetime; factories ≈ transient. The classic bug: state in a lazy route's provider resets when the route is remounted, or is duplicated across two mount points.
19. 🔥 **`inject()` vs constructor injection?** `inject()` works in field initialisers and in functions (guards, interceptors, factories), avoids constructor noise, and composes with inheritance. Downside: dependencies are less visible at a glance.
20. 🎯 **What is an `InjectionToken`?** A typed, named DI key for config or non-class values — analogous to `IOptions<T>` in ASP.NET Core, and it's overridable in tests.
21. 🎯 **`useClass` vs `useValue` vs `useFactory` vs `useExisting`?** Real implementation / static value / computed instance (possibly depending on other services) / an alias to an already-registered instance.

### RxJS

22. 🔥 **Observable vs Promise:** lazy vs eager, cancellable vs not, multiple values vs one, operators vs `then`. Cancellation is why `switchMap` prevents stale responses.
23. 🔥 **`switchMap` vs `mergeMap` vs `concatMap` vs `exhaustMap`:** cancel-previous (search, route params), concurrent (parallel independent writes), queued (ordered writes), ignore-while-busy (double-click submit).
24. 🔥 **How do you prevent subscription leaks?** Three ways, in order of preference: don't subscribe (`async` pipe or `toSignal`), `takeUntilDestroyed()`, or a `destroy$` Subject + `takeUntil`. Mention `firstValueFrom` when you genuinely need a promise.
25. 🎯 **What is a `BehaviorSubject` and why is it used?** A Subject with a current value; late subscribers get the latest immediately. It was the idiomatic state holder before signals — and it still wins when you need time-based operators.
26. 🎯 **`shareReplay(1)` — what does it solve?** Multiple subscribers triggering multiple HTTP calls. It multicasts and replays the last value to late subscribers. Watch out: it holds the value forever (memory) and needs care with error cases.
27. 🎯 **Hot vs cold Observables.** Cold: each subscription starts a new producer (HTTP). Hot: producer already running, subscribers share it (a `Subject`, a websocket). `share()`/`shareReplay()` make cold hot.
28. 🎯 **How do you handle an HTTP error and still show partial data?** `catchError` returning a fallback Observable (`of([])`) with a side effect on the error state; `retry` for transient failures placed *before* `catchError`.

### Routing

29. 🔥 **Why return a `UrlTree` from a guard instead of `false`?** `false` cancels navigation and leaves the user on the old URL with a dead app; `UrlTree` redirects in one navigation. Cleaner UX, no back-button trap, no redirect flicker.
30. 🔥 **Guard vs interceptor vs resolver — who does what?** Guard decides if a route may activate (navigation/UX). Resolver pre-fetches data so the component renders with it. Interceptor handles transport: headers, retries, global errors. **None of them is a security boundary — the API authorises every request.**
31. 🎯 **`canDeactivate` — real use case?** Prompting on unsaved changes: return `confirm()`/a modal result; the guard blocks navigation until the user decides.
32. 🎯 **How do you pass data between routes?** Path params, query params, route `data`, router state (`navigate(['/x'], { state })`), a resolver, or a shared root service/store. Prefer the store for anything that survives navigation.
33. 🔥 **What's the classic `/orders/1` → `/orders/2` bug?** Angular reuses the component instance, so `ngOnInit` doesn't re-run. Fixes: subscribe to `paramMap` (or `toSignal`), use `withComponentInputBinding()` so the param arrives as a signal input, or force a remount with a `RouteReuseStrategy`/`key`.
34. 🎯 **`pathMatch: 'full'` — why is it needed?** Without it, `path: ''` prefix-matches every URL, so the redirect swallows every route and nothing renders.

### Forms & HTTP

35. 🔥 **Reactive vs template-driven forms?** Reactive keeps the form model in the class: testable, typed (`nonNullable`), supports dynamic controls and cross-field/async validation, and exposes `valueChanges` as a stream. Template-driven is fine for two fields. I default to reactive.
36. 🔥 **How do you validate that password and confirm match?** A validator on the **group** (so it sees both controls), then expose the error on the confirm control for display.
37. 🔥 **How would you implement a 401-refresh-and-retry interceptor?** Intercept, on 401 call `/auth/refresh` **once** — share the in-flight refresh across concurrent 401s (`shareReplay`/a shared `BehaviorSubject`), retry the original request with the new token, and if the refresh fails, clear the token and redirect once. Skip the refresh call itself to avoid a loop.
38. 🎯 **Why must an interceptor clone the request?** Requests are immutable so retries/replays are deterministic; you `req.clone({ setHeaders })`.
39. 🎯 **Where's the API base URL configured and what's the catch?** `environment.apiUrl` — a **build-time** constant, so per-environment URLs mean a build per environment. Better: use a relative `/api` and let nginx/ingress route it, or load runtime config at startup.

### Testing & build

40. 🔥 **What would you test, and how?** Services and pure logic with unit tests; HTTP with `provideHttpClientTesting`; components with `TestBed` but only for meaningful behaviour (renders state, emits output); one E2E smoke path per journey. I avoid snapshot-style tests that break on CSS.

---

## 4. React — 35 questions

1. 🎯 **What is React?** A library that renders UI as a function of state and reconciles the result into the DOM. It is not a framework: routing, data fetching, forms and state management are choices.
2. 🔥 **Why can't hooks be conditional?** React identifies hook state by call **order** — there is no name. A branch that changes the order hands state to the wrong hook. Hence the lint rule, which is a correctness rule, not style.
3. 🔥 **Explain `useEffect` fully.** Runs after paint; deps control when; cleanup runs before the next run and on unmount; omitted deps = every render; StrictMode double-invokes in dev to expose impure effects. It's for synchronising with external systems — not for deriving state and not for fetching that a cache library should own.
4. 🔥 **How do you fix a race condition in data fetching?** `AbortController` in the cleanup (or an `ignore` flag) so a stale response can't win. Then: prefer TanStack Query / RTK Query, which solve it plus caching, dedupe and retries.
5. 🔥 **`useMemo` vs `useCallback` vs `React.memo`.** Memoise a value, a function identity, and a component's render output respectively. All are optimisations; add them after measuring, or let the React Compiler do it.
6. 🔥 **Do custom hooks share state?** No — they share *logic*. Each call gets its own state. Sharing requires Context or a store. This is the most common misconception about hooks.
7. 🎯 **`useState` batching:** updates are batched and async; use the functional form when the next value depends on the previous. Two `setState` calls in one handler produce one render.
8. 🔥 **Why immutable state updates?** React compares by reference (`Object.is`) to decide whether to re-render. Mutating keeps the same reference → no render. Spread/create new objects at the level that changed.
9. 🔥 **What does `key` do?** Identifies an element across renders so React can move rather than recreate DOM. Index keys break on reorder/insert — and changing a key deliberately **remounts** a component (the standard "reset the form when the user changes" trick).
10. 🎯 **Why `StrictMode` double-invokes?** To prove purity and correct cleanup. Production is single. If it breaks your app, your effect is unclean.
11. 🔥 **Context vs Redux?** Context transports a value down the tree (every consumer re-renders on change); Redux is an external store with selectors (only subscribers with changed slices re-render), plus DevTools and middleware. Context is not a state manager.
12. 🔥 **What does `createSlice` give you?** Action creators, action types and a reducer from one object; Immer lets you write `state.x = y` and produces an immutable result.
13. 🔥 **Why does `createAsyncThunk` have three actions?** `pending`/`fulfilled`/`rejected` model loading, success and error explicitly — one `status` enum instead of three booleans that can contradict each other.
14. 🎯 **Redux Toolkit vs TanStack/RTK Query?** The store is for *client* state; a query cache is for *server* state with staleness, dedupe and invalidation. I don't put server data in a hand-rolled slice if I can use a cache.
15. 🔥 **How do you protect routes?** A pathless route rendering `<Outlet/>` when allowed, else `<Navigate to="/login" replace/>`. It's UX only; the API enforces auth.
16. 🎯 **`<Link>` vs `<a href>`?** `href` causes a full document reload: state lost, all JS re-downloaded. `Link` keeps the SPA alive.
17. 🔥 **`BrowserRouter` requires what server config?** A catch-all rewrite to `index.html` for unknown paths, or a hard refresh on `/products` returns 404.
18. 🔥 **How do you code-split?** `React.lazy(() => import(...))` + `<Suspense fallback>` at route level. Same intent as Angular's `loadComponent`.
19. 🔥 **What's new in React 19?** Actions + `useActionState` (form submit state and pending for free), `useOptimistic`, `useFormStatus`, `use()` for promises/context, `ref` as a plain prop (so `forwardRef` is legacy), and the compiler direction (automatic memoisation).
20. 🔥 **`useOptimistic` — when?** When a mutation's latency is user-visible (likes, toggles, posting a comment): render the presumed result immediately and roll back on failure.
21. 🎯 **What can an `ErrorBoundary` not catch?** Errors in event handlers, async callbacks, and itself. Those need try/catch plus a global handler.
22. 🔥 **How do you keep a big list responsive while filtering?** `useDeferredValue`/`startTransition` to keep typing urgent and the filter non-urgent, plus virtualisation if the DOM node count is the problem.
23. 🔥 **How do you debug a slow screen?** React DevTools Profiler to see what committed and why; wide commits = state placed too high; expensive single component = memoise/virtualise; network = add a cache layer. Fix the cause, don't sprinkle `useMemo`.
24. 🎯 **How do you reset a child's state when a prop changes?** Change its `key`.
25. 🎯 **`useRef` vs `useState`?** A ref change doesn't re-render; use it for DOM nodes, timers, previous values, and third-party instances.
26. 🔥 **How do you avoid prop drilling?** Compose (pass elements as children), Context for genuinely ambient data (theme, auth, i18n), or a store for real shared state. Lifting state is for sharing, not tidiness.
27. 🎯 **Controlled vs uncontrolled inputs?** Controlled (value + onChange) is the default and is predictable; uncontrolled + ref/FormData is better for large forms and is what React 19 actions give you. Don't mix.
28. 🎯 **`useId`?** Generates a stable id across server and client — required for `label htmlFor` to match after hydration.
29. 🔥 **CSR vs SSR vs RSC?** Browser-rendered (this repo), server-rendered HTML then hydrated, and server-only components that never ship JS. Chosen by SEO and time-to-content requirements, not fashion.
30. 🎯 **Hydration mismatch — cause?** Server HTML differs from the first client render (usually `Date.now()`, `Math.random()`, `localStorage`, or browser-only branches). Fix by deferring to an effect or `useId`/`useSyncExternalStore`.
31. 🔥 **How do you test React?** Vitest + React Testing Library, asserting by role/label/text — behaviour, not implementation. MSW for the network. Playwright for one journey.
32. 🎯 **Why is Oxlint used here?** It's a Rust-based linter: dramatically faster than ESLint, with a good core rule set. For React-specific hook rules you'd still want `eslint-plugin-react-hooks`.
33. 🎯 **How do you structure a React app?** Feature folders owning slice + API client + components + tests; shared components at the root; never `fetch` inside a component.
34. 🔥 **Where do you store a JWT?** `localStorage` (XSS-readable, no CSRF exposure) vs httpOnly `Secure` cookie (not XSS-readable, needs CSRF defence). Name the trade-off; for high-risk apps prefer cookies + anti-forgery.
35. 🎯 **How would you enforce quality across a React team?** TS strict, lint rules for hooks and a11y, a layer rule (components never call `fetch`), PR templates, and a testing standard that says what *must* be tested.

---

## 5. Framework comparison & front-end architecture (10)

1. 🔥 **Angular or React — which do you prefer and why?** Give the trade-off sandwich: Angular = opinionated, DI, guards/interceptors/forms built in, excellent for large teams with turnover and maps onto .NET; React = small core, choose your ecosystem, smaller learning cliff, larger hiring pool, architecture becomes the senior's job. Then: "I've built the same CRUD in both; the concepts map almost 1:1 — guard ↔ ProtectedRoute, interceptor ↔ fetch wrapper, `providedIn:'root'` ↔ store singleton, `computed` ↔ `useMemo`, `track` ↔ `key`."
2. 🔥 **If you were architecting a new enterprise app tomorrow?** Ask about team size and turnover, existing skills, SEO needs, long-term maintenance, hiring pool, and whether there's a design system. Then decide, and say the decision is reversible only at great cost.
3. 🔥 **How do you manage state in a large app?** The decision tree (§04 §1). Say the anti-pattern out loud: "everything in Redux" and "everything in one giant service".
4. 🔥 **How do you share code between two front ends?** A design-system/component library + generated API clients from OpenAPI + a shared types package. Avoid a shared "utils" dumping ground.
5. 🔥 **How do you handle a framework upgrade?** Read the release notes for breaking changes, upgrade incrementally, run the codemods/`ng update`, keep tests green, and use feature flags for behaviour changes. Example: Angular 17→22 (standalone, control flow, signals, zoneless) — do it module-by-module, not in one heroic PR.
6. 🔥 **How do you keep the front end in sync with the API?** OpenAPI → generated types/client; version the API; contract tests in CI. Manual typing drifts silently.
7. 🎯 **How do you handle i18n?** Angular `i18n`/`@angular/localize` or Transloco; React `react-i18next`. Keys not sentences, locale-aware number/date via `Intl`, RTL support, and never concatenate translated fragments.
8. 🎯 **How do you handle errors in a SPA?** A boundary at route level + a toast channel for recoverable errors + telemetry (`window.onerror`, `unhandledrejection`, framework error handler) enriched with the correlation id from the API.
9. 🔥 **Accessibility — what do you insist on?** Semantic elements, labelled inputs, keyboard reachability, visible focus, `aria-live` for async errors, and axe in CI. "It's cheaper than retrofitting and it's a legal requirement in many markets."
10. 🔥 **Front-end performance — where do you start?** Measure first (Lighthouse/Core Web Vitals: LCP, INP, CLS). Then: bundle size (code-split, drop heavy deps), image strategy, render cost (virtualise, memoise), and network (cache, pagination).

---

## 6. ASP.NET Core / .NET — 40 questions

Full detail in `05-ASPNET-CORE-SENIOR.md`. Answers here are the spoken form.

> **Prerequisite:** these assume the language-level answers in `05A-CSHARP-AND-OOPS-FUNDAMENTALS.md` (value types, boxing, records, NRT, generics, exceptions, concurrency keywords, SOLID, patterns) and the complexity instinct in `05B-DATA-STRUCTURES-AND-ALGORITHMS.md`. If you cannot answer `05A` §11 in 90 seconds each, the framework answers here will sound memorised.

1. 🔥 **Singleton vs Scoped vs Transient — with the bug.** Own process state vs request state vs nothing. The bug: a singleton capturing a scoped `DbContext` → stale tracking, memory growth, concurrency exceptions. .NET's scope validation catches it in Development; Production fails silently.
2. 🔥 **How do you use a `DbContext` in a background service?** Inject `IServiceScopeFactory` and create a scope per unit of work — the worker is a singleton, the context is scoped.
3. 🔥 **Why is `DbContext` not thread-safe, and what breaks?** It tracks entities and one connection; parallel use throws `InvalidOperationException` or corrupts state. One context per parallel branch.
4. 🔥 **`IOptions` vs `IOptionsSnapshot` vs `IOptionsMonitor`.** Singleton/startup, scoped/per-request-with-reload, singleton-with-change-callback. Injecting `Snapshot` into a singleton throws — a captive dependency again.
5. 🔥 **Middleware order — walk it.** Exception handler → HSTS/HTTPS → CORS → static → routing → authN → authZ → rate limiter → endpoints. Order encodes meaning; `UseCors` before auth (preflight), authN before authZ.
6. 🔥 **Middleware vs filter?** Middleware is app-wide and pre-routing; filters are per-action and have MVC context (model state, action arguments). Use middleware for transport concerns, filters for action concerns.
7. 🔥 **What does `builder.Build()` change?** Before it you register services; after it you compose the pipeline. You cannot resolve scoped services while building.
8. 🔥 **`IQueryable` vs `IEnumerable`.** Expression tree translated to SQL vs client-side execution in memory. Mixing them silently pulls a whole table over the wire.
9. 🔥 **N+1 — how do you find it?** Log SQL in development, look for repeated identical queries in a loop; or read the code for lazy-loaded navigation access. Fix with projection or `Include`; `AsSplitQuery()` if multiple collections explode the cartesian product.
10. 🔥 **`AsNoTracking` — when?** Read-only endpoints. It skips change tracking — measurable win on list queries. Not on entities you intend to update.
11. 🔥 **How do you deploy migrations?** Generate idempotent SQL scripts in CI, review them, apply as a pipeline step, and keep them backwards-compatible for one release so rolling deploys are safe. Not `Database.Migrate()` on startup.

> The full SQL/data question bank — indexes, execution plans, parameter sniffing, isolation levels, `rowversion`
> concurrency, deadlocks, EF Core ↔ SQL, least privilege, Azure SQL tiers — is **`05D-SQL-AND-DATA-ACCESS-FOR-DOTNET.md`**.
12. 🔥 **Is `async` faster?** No — it's about scalability. It frees threads during I/O so concurrency doesn't need threads. Never `.Result`/`.Wait()`: thread-pool starvation under load.
13. 🔥 **Why `CancellationToken` everywhere?** When the client disconnects the work should stop; it's wasted DB/HTTP time otherwise.
14. 🔥 **`Task.WhenAll` pitfalls?** First exception only surfaces (others are swallowed unless you inspect the tasks), and a shared `DbContext` is unsafe — one context per task.
15. 🔥 **JWT validation — the five checks + `ClockSkew`.** Issuer, audience, lifetime, signing key, signature. `ClockSkew` defaults to 5 minutes, so set it to `TimeSpan.Zero` if you want expiry to be exact.
16. 🔥 **Symmetric vs asymmetric signing?** HMAC shares a secret with every validator — fine for one API. RSA/ECDSA signs with a private key and validates with a public key — required when many services validate.
17. 🔥 **Access + refresh tokens — why?** Short access token limits the blast radius of theft; refresh tokens allow long sessions with rotation and revocation. Rotate on use and detect reuse.
18. 🔥 **401 vs 403.** Not authenticated vs authenticated but not permitted.
19. 🔥 **Policy vs role auth?** Roles are coarse; policies compose claims/requirements and are the real answer for "can edit own orders" (a resource-based `IAuthorizationHandler`).
20. 🔥 **Why not return EF entities?** Over-posting/mass assignment, circular serialisation, schema leakage, and a contract that changes whenever the table does. DTOs.
21. 🔥 **Error contract?** RFC 7807 `ProblemDetails` for every failure, including validation (`ValidationProblemDetails`) so the SPA can map errors to fields.
22. 🔥 **How do you version an API?** URL segment is pragmatic and cache-friendly; header-based is purer. Decide before clients exist; never break a shipped contract.
23. 🔥 **Pagination?** Server-side `Skip/Take` with a total count envelope, capped `pageSize`; cursor pagination for large/live sets.
24. 🔥 **Idempotency for POST?** An `Idempotency-Key` header whose result is stored and replayed — essential for payments/orders where retries are inevitable.
25. 🔥 **How do you cache?** `IMemoryCache` (per instance) vs Redis (shared) vs HTTP caching/ETag vs output caching. Cache expensive-and-stable with a TTL; invalidation is the hard part, so prefer short TTLs over clever invalidation.
26. 🔥 **What breaks with `IMemoryCache` when you scale to 4 pods?** Each pod has its own copy: inconsistent reads and 4× the cache misses. Move to Redis if correctness depends on the cached value.
27. 🔥 **When messaging instead of HTTP?** When the caller doesn't need the answer to continue, and when the receiver's availability shouldn't affect the caller. Cost: eventual consistency.
28. 🔥 **Transactional outbox?** Write the state change and the event in one DB transaction to an outbox table; a publisher drains it. Prevents "DB committed, message lost".
29. 🔥 **How do you make a consumer idempotent?** Persist processed message ids and skip duplicates; or make the operation naturally idempotent (upsert by natural key).
30. 🔥 **Saga vs 2PC?** 2PC locks and needs distributed transaction support; sagas accept eventual consistency and compensate on failure. I avoid distributed transactions.
31. 🔥 **SignalR at scale?** Add a backplane (Redis) so a broadcast from pod A reaches a client on pod B, or use sticky sessions. `ConnectionId` isn't a user id.
32. 🔥 **RabbitMQ or Kafka?** Work queues and per-message routing vs a replayable log for event streams. Order events that need replay/analytics → Kafka; task distribution → RabbitMQ.
33. 🔥 **gRPC or REST?** gRPC internally (HTTP/2, protobuf, streaming, generated contracts); REST/JSON at the edge for browsers and debuggability.
34. 🔥 **Health checks — liveness vs readiness?** Liveness = "restart me"; readiness = "send me traffic" (DB reachable). Conflating them causes restart loops during a DB blip.
35. 🔥 **How do you debug a production failure with no repro?** Correlation id in every log line and propagated to downstream calls; structured logs searching that id; distributed trace to find the slow hop; metrics for when it started.
36. 🔥 **Graceful shutdown?** Stop accepting new work, drain in-flight requests, close broker/DB connections on `IHostApplicationLifetime`/`SIGTERM` — otherwise you lose in-flight orders on every deploy.
37. 🔥 **Testing strategy?** Unit for domain; `WebApplicationFactory` + Testcontainers for real-SQL integration; contract tests for the API/SPA boundary; a thin E2E layer. Don't mock `DbContext` — it tests the mock.
38. 🔥 **Why not `UseInMemoryDatabase` for tests?** It isn't relational: no constraints, different `Include`/query semantics. Tests pass and production fails.
39. 🔥 **Clean architecture or vertical slices?** Clean gives a dependency rule and a testable domain; slices give cohesion per feature and avoid a god service layer. I start with slices around features and extract a domain layer where the rules are genuinely complex.
40. 🔥 **What's the biggest performance win you've shipped?** Tell a real one: e.g. an EF list endpoint with a projection + `AsNoTracking` + composite index and server-side paging, taking p95 from seconds to ~100 ms. Have the numbers.

---

## 7. System design — three whiteboards to rehearse

### 7.1 "Design an order management system for a food delivery app"

Use `DeliverFoodApp` as your reference implementation — you own it.

**Script (8–10 minutes):**

1. **Clarify** — who are the actors (customer, restaurant, driver, admin)? Read/write ratio? Peak orders/min? Is real-time tracking required? Payment providers? Multi-region?
2. **Core entities** — Customer, Restaurant, MenuItem, Order, OrderItem, Driver, Payment, OrderStatusHistory.
3. **API surface** — `POST /orders`, `GET /orders/{id}`, `GET /orders?status=&page=`, `POST /orders/{id}/cancel` (idempotency key), `GET /restaurants/{id}/menu`.
4. **Data** — SQL Server/Postgres for orders (transactional, relational). Index on `(CustomerId, CreatedAt)`, `(Status, CreatedAt)`. Order state machine: Placed → Accepted → Preparing → Ready → PickedUp → Delivered, with cancellation rules per state. **Status transitions are business rules — enforce server-side and record history.**
5. **Consistency** — payment is a separate service. Don't 2PC: use an outbox + saga. Order placed → reserve payment → if payment fails, compensate with cancellation. Idempotent consumers.
6. **Events** — `OrderPlaced`, `OrderAccepted`, `OrderReady`, `DriverAssigned` published to the broker; notification service consumes; kitchen display consumes.
7. **Real-time** — SignalR hub; clients subscribe to a group per order (`order-{id}`, as in `OrderHub`); Redis backplane for multi-pod; fall back to polling with a cursor if the socket dies.
8. **Front end** — SPA subscribes after load and reconciles with the REST fetch by `updatedAt`/version, so a missed message can't corrupt the view.
9. **Scale & failure** — read replicas for menu browsing; cache menus (short TTL); queue order writes; degrade gracefully (if the notification service is down, orders still work).
10. **Observability** — correlation id from the browser through API → broker → consumer; trace per order; alert on "orders stuck in Prepping > 20 min".

**The line that closes it:** "The hard part isn't the CRUD — it's the state machine across services and the fact that payment, kitchen and driver can each fail independently. So I'd make status transitions the single source of truth, keep every consumer idempotent, and compensate rather than coordinate."

### 7.2 "Users report the app is slow after login. Diagnose."

Demonstrates method, which is what they're testing.

1. **Reproduce & scope** — one user or all? One region? Since which deploy? (Check the deploy timeline first — most incidents are changes.)
2. **Measure** — browser devtools: is it network, JS, or render? Core Web Vitals. Server: p50/p95/p99 of the endpoints hit after login.
3. **Likely suspects, in order** — an N+1 introduced by a new dashboard query; a missing index; a payload that grew (returning entities instead of DTOs); the SPA fetching 6 endpoints serially instead of in parallel or as one aggregate; a large bundle chunk loaded lazily right after login; a cache stampede at shift start.
4. **Confirm with evidence** — a trace showing which hop is slow; SQL logs showing a repeated query; the network waterfall showing serial calls.
5. **Fix & verify** — projection + index + `AsNoTracking`; parallelise or aggregate the calls; preload the next route; then measure again and record the numbers.
6. **Prevent** — a latency SLO on login endpoints, alerting, and a performance budget in CI.

### 7.3 "How would you migrate a 200-page ASP.NET MVC app to a SPA?"

**This is your story.** Answer with the strangler fig.

1. **Don't rewrite.** A big-bang rewrite loses revenue and knowledge. Introduce the SPA alongside.
2. **Seam** — put the SPA at a route (`/app/*`) or a shell around new features; keep MVC pages for the rest; extract shared auth (cookie or token) so both work during transition.
3. **Order of work** — start with the highest-churn, most-interactive screens (jQuery-heavy), not the most complex.
4. **API-first** — each migrated screen needs an API. If the controller currently returns a view model, create a JSON endpoint and DTO; keep the Razor page on the same data until the switch.
5. **State of truth** — the server remains authoritative; the SPA holds a cache, not a copy of the business rules.
6. **Team** — one team owns the SPA shell early (routing, auth, design system, API client) or you'll get 20 inconsistent patterns.
7. **Kill the old** — delete Razor views as soon as the SPA route is live; leaving both is how migrations stall for years.

---

## 8. Live coding — what to expect and how to handle it

| Task | What they're testing | How to pass |
|---|---|---|
| Build a searchable/filterable list from a fetch | State, effects, derived data | Debounce the input, cancel in-flight, derive the filtered list (don't store it), show loading/empty/error states |
| A form with validation and submit | Controlled inputs, error handling | Validate on blur and submit, disable while pending, surface server errors on fields |
| Refactor a 200-line component | Decomposition judgement | Extract the data hook/service, extract presentational children, name the seams before typing |
| "Fix this bug" with a stale-value closure | Effect deps and closure understanding | Narrate the closure over the first render, then fix with a functional update or a ref |
| Explain code you wrote 3 minutes ago | Communication | Comment the intent, then read your own code aloud |

**Rules for live coding:** talk constantly (silence reads as blanking), state your assumptions, get something working end-to-end before refining, and when you finish say what you'd do with more time and what you'd test.

---

## 9. Behavioural — the five STAR stories you must have ready

Prepare **one page each** with real numbers. These cover ~90% of behavioural questions.

### Story 1 — The framework migration (covers: initiative, learning, driving change, ambiguity)
*S:* Our Razor Pages app had jQuery-heavy screens; each new feature made the JS more tangled and bug reports about stale data kept coming.
*T:* I proposed a SPA approach and had to justify it without a mandate.
*A:* I built one feature end-to-end in Angular as a spike, measured the delta (fewer DOM bugs, faster page transitions), documented the trade-offs, and presented a strangler-fig plan: new features in the SPA, old pages untouched.
*R:* N screens migrated over M months, bug rate in those screens dropped, and the pattern became the team default. **Lesson:** the smallest possible proof beats a large proposal.

### Story 2 — The N+1 / performance incident (covers: depth, ownership, production pressure)
*S:* A page's p95 hit several seconds at peak.
*T:* Find it fast without guessing.
*A:* Traced it to a loop over orders touching navigation properties — 1 + N queries; plus the endpoint returned entities instead of DTOs, inflating the payload.
*R:* Projection + `AsNoTracking` + a composite index + paging; p95 to triple-digit ms. Added SQL logging in dev and a latency alert so it can't regress silently. **Lesson:** measure before optimising; the fix was 40 lines.

### Story 3 — The hard bug (covers: debugging method, honesty, cross-team work)
Pick a real one: a duplicate-order bug caused by a retry on a POST. Fix: idempotency key on the client, dedupe on the server. **Lesson:** retries need idempotency; the bug wasn't in either team's code alone.
*(This is the strongest possible story for a senior role because it spans front end, API and data.)*

### Story 4 — The disagreement (covers: influence without authority, conflict)
*S:* A colleague wanted to put all state in Redux; I thought most of it was server data.
*T:* Resolve without it becoming personal.
*A:* Wrote both versions of one screen, compared the code and the re-render behaviour in the profiler, presented the numbers, and proposed the split: Redux for genuine client state, a query cache for server data.
*R:* Adopted; the screen got simpler and the argument became a written guideline. **Lesson:** data ends arguments faster than opinion.

### Story 5 — The mistake (covers: accountability — they always ask)
*S:* I shipped a migration that wasn't backwards-compatible during a rolling deploy; old instances broke for a few minutes.
*T:* Restore service, then prevent recurrence.
*A:* Rolled back, communicated in the incident channel with a timeline, then changed our rule: migrations must be compatible with the previous release for one cycle, reviewed as SQL scripts.
*R:* No recurrence; the rule is now in the team checklist. **Lesson:** deploys are a distributed system; I own the process change, not just the fix.

**Behavioural question → story map:** "tell me about a time you…" learned something fast → 1; improved performance/quality → 2; debugged something hard → 3; disagreed → 4; failed → 5; led without authority → 1 or 4; handled pressure → 2 or 3; influenced architecture → 1 or 4.

---

## 10. Questions to ask them (this is scored — it's not small talk)

Pick three or four, tailored to the interviewer's role:

**For the tech lead / architect**
1. "What does the front-end architecture look like today — is there one blessed pattern, or is it per-feature?"
2. "How do you decide when to upgrade framework majors, and what's blocking the last one?"
3. "Where does the codebase hurt most right now? What would you want a new senior to fix first?"
4. "How do the front-end and API teams agree on contracts — shared OpenAPI, or is it by convention?"
5. "What's your production observability story — can a developer follow a single request through the system?"

**For the manager**
6. "How is 'senior' defined here — is it scope of ownership, technical depth, or mentoring?"
7. "What does the on-call rotation look like, and who fixes production issues?"
8. "What would success look like for this role in the first 90 days?"
9. "How much of this role is front end vs backend vs design/review time?"

**Closing**
10. "Is there anything in my background you'd like me to clarify before we finish?" *(This is the highest-value question in any interview — it gives you a chance to address a doubt you didn't see.)*

---

## 11. Red flags to avoid in the room

| Don't | Why it sinks you |
|---|---|
| Claim more Angular/React years than you have (you: **1 year in production**, per `00` §0A.9) | One follow-up exposes it, and then *everything* you said is suspect |
| "I'd just use Redux for everything" / "signals everywhere" | No boundary judgement = mid-level thinking |
| Criticise the repo's code | Criticise the *decision and its context*, never the author: "I'd migrate this interceptor to the functional style" not "this is wrong" |
| Answer only what was asked, with no trade-off | Reads as memorised |
| Say "we" for everything | They can't tell what *you* did; use "I" for your actions and "we" for context |
| Bluff on a version you've never used | Immediate credibility loss; the honest boundary always scores better |
| No numbers anywhere | A senior should know the p95, the row count, the team size, the timeline |
| Talk about frameworks as identity ("React is better") | Tool-choice ideology signals junior; judgement signals senior |

---

## 30-second version

Answer in the first sentence, then justify, then name a trade-off — 60–90 seconds, then stop. Rehearse the four answer shapes, the decision trees for state management and framework choice, the three whiteboard scripts (order system, slow app, MVC→SPA migration), and the five STAR stories with real numbers. Say the honest boundary instead of bluffing, and always close with the question "is there anything you'd like me to clarify?" — that is where the offer is won.
