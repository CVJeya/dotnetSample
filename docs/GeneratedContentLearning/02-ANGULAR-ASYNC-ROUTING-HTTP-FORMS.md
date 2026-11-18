# Chapter 2 — Angular Async (RxJS), Routing, HTTP & Forms

> **Sitting a timed assessment?** `14-WORLDBANK-FULLSTACK-ANGULAR-ASSESSMENT.md` §8 (TS/JS) and §9 (RxJS, 10 Q&A) are the exam-speed versions of this chapter.

> Files: `frontend/src/app/services/*`, `guards/auth.guard.ts`, `interceptors/auth.interceptor.ts`, `app.routes.ts`, `components/login/login.ts`

---

## 1. RxJS without fear

You know `Task<T>` and `async/await`. RxJS is the hard part of Angular for every .NET developer. Here is the bridge.

| Concept | .NET / JS analogue | Key difference |
|---|---|---|
| `Promise<T>` | `Task<T>` | Single value, eager, not cancellable |
| `Observable<T>` | `IObservable<T>` (Rx.NET) | **Lazy**: nothing happens until `subscribe()` |
| `Subject<T>` | `IObservable` + `IObserver` | You push values into it |
| `BehaviorSubject<T>` | `IObservable<T>` + current value | Late subscribers get the latest value |
| Operator | LINQ method (`Select`, `Where`, `Debounce`) | `map`, `filter`, `debounceTime` |
| `switchMap` | like `SelectMany` + cancel previous | **Cancels** the prior inner observable |
| `mergeMap` | like `SelectMany` (no cancel) | runs all concurrently |
| `concatMap` | like `SelectMany` ordered | queues |
| `exhaustMap` | ignores new while busy | ideal for login/double-submit |

**The one-sentence bridge you should rehearse:**
> "An Observable is a lazy, cancellable stream — it's `IObservable<T>` from Rx.NET, which is why operators feel like LINQ. The critical difference from a `Task` is that unsubscription cancels the in-flight work, which is how Angular avoids race conditions and memory leaks."

### The four flattening operators — the #1 RxJS interview question

Scenario: a search box. User types `a`, `ab`, `abc`.

- `switchMap` — cancels `a` and `ab`, only `abc` result is used. **Correct for search & route params.**
- `mergeMap` — all three requests fire; results may arrive out of order → stale UI. Use when order doesn't matter and you want parallelism.
- `concatMap` — requests queue in order; slow but ordered. Correct for sequential writes (form submits that must not interleave).
- `exhaustMap` — while one request is in flight, new emissions are **dropped**. Correct for "click Save twice" protection.

**The senior answer includes *why*:** "`switchMap` is the safe default for reads because the only valid response is the newest one. `mergeMap` on a route-param stream is a classic bug: you navigate quickly and the older response overwrites the newer one."

### Marble-ish mental model of your repo's usage

`auth.service.ts` uses `tap`:

```ts
login(creds: LoginRequest): Observable<AuthResult> {
  return this.http.post<AuthResult>(`${this.apiUrl}/auth/login`, creds).pipe(
    tap(res => {
      localStorage.setItem('token', res.token);   // side effect, keeps the stream value unchanged
      this._currentUser.set(res.user);
    })
  );
}
```

`tap` = "peek without changing the value". Interview point: *"Side effects belong in `tap`; transformations in `map`. Mixing them makes streams untestable."*

### Memory leaks and the modern answer

The old way:

```ts
private destroy$ = new Subject<void>();
ngOnInit() { this.http.get(...).pipe(takeUntil(this.destroy$)).subscribe(...); }
ngOnDestroy() { this.destroy$.next(); this.destroy$.complete(); }
```

The current way (v16+):

```ts
private readonly http = inject(HttpClient);
data$ = this.http.get<Product[]>('/api/products').pipe(takeUntilDestroyed());
```

`takeUntilDestroyed()` (from `@angular/core/rxjs-interop`) auto-unsubscribes on destroy. Even better: **`toSignal()` handles it for you** and gives you a signal.

> Best answer: "The best subscription is no subscription — `toSignal()` in the field initialiser, or the `async` pipe, means Angular owns the lifecycle. If I must subscribe imperatively, `takeUntilDestroyed()`."

### `async` pipe vs manual subscribe

```html
@if (products$ | async; as products) {
  @for (p of products; track p.id) { ... }
}
```

The `async` pipe subscribes, unwraps, and unsubscribes automatically, and marks OnPush components for check. Manual `subscribe()` in the component means you own cancellation and you must assign into a property (which under OnPush may not trigger a render).

---

## 2. Routing in depth (`frontend/src/app/app.routes.ts`)

```ts
export const routes: Routes = [
  { path: '', redirectTo: '/products', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./components/login/login').then(m => m.LoginComponent),
  },
  {
    path: 'products',
    canActivate: [authGuard],
    loadComponent: () => import('./components/product-list/product-list').then(m => m.ProductList),
  },
  { path: '**', redirectTo: '/products' },
];
```

### `loadComponent` — lazy loading, standalone style

`loadComponent` returns a dynamic `import()`, so the bundler emits a **separate chunk** fetched on first navigation. Before standalone, lazy loading required a lazy `NgModule` (`loadChildren`). `loadChildren` still exists for grouping a feature's routes.

Talking points:
- Lazy loading improves **initial** bundle size and time-to-interactive; it does not reduce total bytes.
- Each chunk is a network round-trip on first visit → preload the likely-next route with `PreloadAllModules` or a custom strategy.
- Under `@angular/build` (esbuild) the chunking is automatic from the dynamic import.

### Route configuration features worth naming

```ts
provideRouter(
  routes,
  withComponentInputBinding(),   // :id becomes an input() on the component
  withInMemoryScrolling({ scrollPositionRestoration: 'enabled' }),
  withPreloading(PreloadAllModules),
  withViewTransitions(),         // native View Transitions API
)
```

`withComponentInputBinding()` is the one to mention: it removes the `ActivatedRoute.snapshot.paramMap.get('id')` boilerplate and makes the component a pure input consumer, which is far easier to test.

### Path matching, params, query params

| Piece | Meaning |
|---|---|
| `path: ''` + `pathMatch: 'full'` | exact empty path. Without `full`, `''` prefix-matches everything and the redirect swallows all routes — **a classic bug** |
| `:id` | required param |
| `path: '**'` | wildcard; must be **last** |
| `data: { role: 'admin' }` | static metadata available to guards/resolvers |
| `resolve: { product: productResolver }` | pre-fetch before activation |
| `children` | nested routes rendered into parent's `<router-outlet>` |
| `canActivateChild`, `canDeactivate`, `canLoad` | auth, unsaved-changes prompt, pre-lazy-load gate |

### Guards — functional (`guards/auth.guard.ts`)

```ts
export const authGuard = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.isLoggedIn ? true : router.createUrlTree(['/login']);
};
```

Two important details:

1. **Returning a `UrlTree` is better than returning `false`.** `false` blocks navigation and leaves the user staring at a dead screen; a `UrlTree` (or `router.parseUrl('/login')`) **redirects** in a single navigation, with no flicker and no double history entry. This is exactly the kind of detail that separates someone who has debugged a redirect loop from someone who hasn't.
2. **Guards can return `Observable<boolean|UrlTree>` or `Promise<...>`** — Angular awaits them. So an auth guard can validate the token with the server: `return this.auth.verify().pipe(map(ok => ok ? true : this.router.createUrlTree(['/login'])))`.

**Guard vs interceptor split — say it plainly:** "Guards decide *whether a route can activate* (UI navigation). Interceptors decide *whether a request carries credentials and how failures are handled* (transport). Server-side authorisation is still mandatory — a guard is a UX feature, not a security boundary." That last clause is a senior-only sentence. Never let an interviewer think you believe a front-end guard secures an API.

### Resolvers — and when they hurt

```ts
export const productResolver: ResolveFn<Product> = (route) =>
  inject(ProductService).getById(Number(route.paramMap.get('id')));
```

Pros: no empty-state flash, component gets data synchronously, failure can be handled by aborting navigation.
Cons: navigation waits (`await`), so a slow API freezes the transition.

**Balanced senior answer:** "Resolvers for data the page cannot render without — a detail page title/id. Not for optional or slow data — there you want the shell immediately and a skeleton for the section. Otherwise the whole app feels like it's hanging on one query."

---

## 3. HttpClient (`services/product.service.ts`)

```ts
@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.apiUrl}/products`);
  }
  getById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/products/${id}`);
  }
  create(p: Omit<Product, 'id' | 'createdAt'>): Observable<Product> {
    return this.http.post<Product>(`${this.apiUrl}/products`, p);
  }
}
```

### Facts to have ready

- `HttpClient` methods are **cold** Observables: one HTTP call per subscription. Two `subscribe()`s = two requests. (`shareReplay(1)` de-duplicates.)
- Typed `get<Product[]>()` is a **compile-time cast only**. Validate at the boundary if the API isn't yours.
- **`HttpParams`** for query strings, **`HttpHeaders`** for headers, `observe: 'response'` when you need status/headers.
- **Errors:** `HttpErrorResponse` carries `status`, `error`, `message`. A thrown error in an Observable terminates that stream — retry operators must be placed before the failure point.
- **`catchError`** substitutes a fallback; **`retry({count:2, delay:1000})`** for transient failures; **`timeout(5000)`** to bound latency. Combine: `retry` then `catchError` (retry inner, catch outer).
- **Cancellation:** unsubscribing aborts the XHR. `switchMap` therefore cancels the previous request.
- **`withCredentials`** needed for cookie-based auth across origins; your JWT-in-header approach doesn't need it.

### Interceptors (`interceptors/auth.interceptor.ts`)

What interceptors are for: cross-cutting transport concerns — auth headers, correlation IDs, global error toasts, retry/refresh, loading spinners, request caching.

```ts
// repo's class-based style
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = localStorage.getItem('token');
    const cloned = token
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;
    return next.handle(cloned);
  }
}
```

Key concept: **requests are immutable** — you must `clone()`. Explain why: immutability lets Angular replay/retry requests deterministically.

**The 401-refresh question (asked constantly).** Model answer:
> "On a 401 I don't want to log the user out immediately — the access token may just have expired. I'd have a refresh flow: intercept the 401, call `/auth/refresh` once, and retry the original request with the new token. The traps are (1) not refreshing the refresh call itself — infinite loop; (2) concurrent 401s firing N refreshes — so I'd share the refresh as a single in-flight Observable via `shareReplay` or a `BehaviorSubject` of the refresh result; (3) a failed refresh must clear the token and redirect to login exactly once."

That answer, delivered calmly, is a *five-year* answer. It's also true of real apps, so learn it as a pattern, not a script.

**Interceptor ordering:** interceptors run as a chain — request in registration order, response in reverse. `HTTP_INTERCEPTORS` multi-provider order = array order. One non-obvious pitfall: an interceptor provided in a **lazy-loaded route's** injector only applies to requests made from that subtree.

---

## 4. Forms — the comparison interviewers love

Angular has **two** form systems; React effectively has none built in (you use a library). This asymmetry is a great talking point.

### Template-driven (`FormsModule` + `ngModel`)

```html
<form #f="ngForm" (ngSubmit)="save(f)">
  <input name="name" [(ngModel)]="model.name" required minlength="3" #nameRef="ngModel" />
  @if (nameRef.invalid && nameRef.touched) { <p>Name is required</p> }
</form>
```

Fast to write, model is a plain object, validation is in the template. Weaknesses: async validators are awkward, logic isn't unit-testable, and it relies on two-way binding which hides data flow.

### Reactive forms (`ReactiveFormsModule`) — the senior default

```ts
form = this.fb.nonNullable.group({
  email: ['', [Validators.required, Validators.email]],
  password: ['', [Validators.required, Validators.minLength(8)]],
});

submit() {
  if (this.form.invalid) { this.form.markAllAsTouched(); return; }
  this.auth.login(this.form.getRawValue()).subscribe({ ... });
}
```

Why prefer it:
- The form model is a **TypeScript object you can build and test without the DOM**.
- Sync + async validators, cross-field validators, dynamic controls (`fb.array`), and value/status changes as **Observables** (`form.valueChanges.pipe(debounceTime(300), distinctUntilChanged())`).
- `nonNullable.group` (v14+) fixes the historic annoyance that `reset()` wrote `null` into typed fields.
- You can add a control later without touching markup.

**Trade-off sentence:** "Reactive forms cost a bit more boilerplate for a two-field search box, but the moment you need conditional fields, cross-field validation, or a test, the reactive model pays for itself. I default to reactive and use template-driven only for tiny isolated widgets."

### Validation patterns worth naming

- Cross-field: password/confirm is a validator on the **group**, not the field:
  `const match = (g: AbstractControl) => g.get('a')!.value === g.get('b')!.value ? null : { mismatch: true };`
- Async validator (check email availability) returns `Observable<ValidationErrors | null>`; pair with `updateOn: 'blur'` or `debounceTime` so you don't hammer the API on every keystroke.
- `markAsTouched()` before showing errors — otherwise every form screams red on first paint.
- **Custom error component** beats repeating `@if (ctrl.errors?.['required'])` in 40 templates.
- Never trust client validation — the server re-validates (and this repo's API returns 400 with `ModelState` errors; map them back onto controls with `setErrors`).

---

## 5. Angular + ASP.NET Core integration realities

The most valuable thing you can bring to this interview is that you have *both halves*.

| Front-end concern | Backend reality | What you say |
|---|---|---|
| CORS | `builder.Services.AddCors(...)` + `app.UseCors()` | "In dev I proxy `/api` via the Angular dev-server proxy so there's no CORS at all; in Docker nginx proxies `/api` to the API — one origin, so CORS stays an edge concern only." |
| Auth | JWT bearer in `Program.cs`, `[Authorize]` on controllers | "The token is issued once, stored in `localStorage`, attached by an interceptor, validated by `JwtBearerDefaults`. `localStorage` is XSS-readable, so for higher-risk apps I'd raise httpOnly cookies + CSRF tokens as the alternative and name the trade-off." |
| Validation | `ModelState` / `ValidationProblemDetails` (RFC 7807) | "I map 400 problem-details back onto form controls instead of showing a generic toast." |
| Errors | `ProblemDetails` standard shape | "A stable error contract is what makes the front end's error handling simple." |
| Pagination | `?page=1&pageSize=20` + total count | "Server-driven paging with a `totalCount` in the response envelope; client-side paging dies at 5,000 rows." |
| Timezones | UTC in DB, ISO-8601 over the wire | "The API never sends local times; the client formats with `Intl.DateTimeFormat`." |

---

## 6. Chapter 2 — questions you must be able to answer

1. Observable vs Promise — three differences that matter in practice. *(§1)*
2. `switchMap` vs `mergeMap` vs `concatMap` vs `exhaustMap` — one scenario each. *(§1)*
3. How do you prevent memory leaks from subscriptions? Name all three ways. *(§1)*
4. Why is `async` pipe preferred over `subscribe()` in a component? *(§1)*
5. `loadComponent` vs `loadChildren`. *(§2)*
6. Why return a `UrlTree` from a guard instead of `false`? *(§2)*
7. Guard vs resolver vs interceptor — what belongs to each? *(§2, §3)*
8. Where is authorisation *actually* enforced, and why? *(§2)*
9. Why must an interceptor `clone()` the request? *(§3)*
10. Design a 401-refresh-with-queue interceptor; name the three traps. *(§3)*
11. `shareReplay(1)` — what problem does it solve? *(§3)*
12. Reactive vs template-driven forms — when is each correct? *(§4)*
13. How do you validate that two fields match? *(§4)*
14. How does the front end handle a 400 with field-level validation errors? *(§5)*

---

## 30-second version

RxJS streams are lazy, cancellable, LINQ-like; `switchMap` for reads and route params, `mergeMap`/`concatMap`/`exhaustMap` for writes and double-submit; use `toSignal`/`async`/`takeUntilDestroyed` so Angular owns the subscription. Routes are lazy via `loadComponent`, guarded by **functional guards that return a `UrlTree`**, with `withComponentInputBinding()` replacing `paramMap` plumbing. HTTP cross-cutting concerns live in interceptors (immutable requests, `clone()`), where the 401-refresh queue is the standard hard case. Forms default to reactive, and none of it replaces server-side validation.
