# Chapter 7 — Cheatsheets

> Print these. Read the left/right columns until the mapping is automatic. **The mapping is the whole trick** — if you can translate a concept from Angular to React (or from C# to TS) in your head, you look like someone who learned principles rather than a single API.
>
> **Scope:** syntax and concept recall — the *senior* round. For the Technical Lead round (decisions, delivery, people, Azure) use `08`–`13`; `13-FIRST-90-DAYS.md` holds a one-page scorecard that tells you which of those to study next.

---

## 1. Side-by-side: the same feature in both frameworks

### 1.1 Local state + list rendering

**Angular**
```ts
@Component({
  selector: 'app-product-list',
  imports: [],
  templateUrl: './product-list.html',
})
export class ProductList {
  private readonly svc = inject(ProductService);
  readonly products = signal<Product[]>([]);

  constructor() {
    this.svc.getAll().subscribe(p => this.products.set(p));
  }
}
```
```html
@for (p of products(); track p.id) {
  <li>{{ p.name }} — {{ p.price | currency }}</li>
} @empty {
  <li>No products</li>
}
```

**React**
```jsx
function ProductList() {
  const [products, setProducts] = useState([]);
  useEffect(() => {
    let ignore = false;
    productsAPI.getAll().then(p => { if (!ignore) setProducts(p); });
    return () => { ignore = true; };
  }, []);

  if (!products.length) return <li>No products</li>;
  return products.map(p => <li key={p.id}>{p.name} — {formatCurrency(p.price)}</li>);
}
```

| Concept | Angular | React |
|---|---|---|
| Local state | `signal(initial)` | `useState(initial)` |
| Read | `s()` | `s` |
| Write | `s.set(v)` / `s.update(fn)` | `setS(v)` / `setS(f => …)` |
| Derived | `computed(() => …)` | `useMemo(() => …, [deps])` or plain expression |
| Side effect | `effect(() => …)` | `useEffect(() => …, [deps])` |
| List | `@for (x of xs; track x.id)` | `xs.map(x => <li key={x.id}>)` |
| Conditional | `@if (c) { } @else { }` | `{c ? <A/> : <B/>}` |
| Empty state | `@empty { }` | `xs.length === 0 && <Empty/>` |

### 1.2 Parent → child data flow

**Angular**
```ts
@Component({ selector: 'app-card', template: `<h3>{{ title() }}</h3><button (click)="pick.emit(id())">Pick</button>` })
export class CardComponent {
  readonly title = input.required<string>();
  readonly id = input.required<number>();
  readonly pick = output<number>();
}
```
```html
<app-card [title]="p.name" [id]="p.id" (pick)="select($event)" />
```

**React**
```jsx
function Card({ title, id, onPick }) {
  return <><h3>{title}</h3><button onClick={() => onPick(id)}>Pick</button></>;
}
// usage
<Card title={p.name} id={p.id} onPick={select} />
```

| Concept | Angular | React |
|---|---|---|
| Input | `input()` / `@Input()` | prop |
| Required input | `input.required()` | TS: `{ title: string }` |
| Output | `output()` / `@Output() EventEmitter` | callback prop |
| Two-way | `[(value)]` = `[value]` + `(valueChange)` | `value` + `onChange` |
| Children | `<ng-content/>` | `{children}` prop |
| Slot named | `select="header"` | a named prop (`header={<X/>}`) |

### 1.3 Routing + guard + lazy load

**Angular**
```ts
export const routes: Routes = [
  { path: 'products', canActivate: [authGuard],
    loadComponent: () => import('./product-list/product-list').then(m => m.ProductList) },
  { path: '**', redirectTo: 'products' },
];
export const authGuard = () => {
  const auth = inject(AuthService), r = inject(Router);
  return auth.isLoggedIn ? true : r.createUrlTree(['/login']);
};
```

**React**
```jsx
const ProductList = lazy(() => import('./ProductList'));

function ProtectedRoute() {
  const { isLoggedIn } = useAuth();
  return isLoggedIn ? <Outlet /> : <Navigate to="/login" replace />;
}

<Routes>
  <Route element={<ProtectedRoute />}>
    <Route path="/products" element={<ProductList />} />
  </Route>
</Routes>
```

| Concept | Angular | React Router 7 |
|---|---|---|
| Route table | `routes` array | `<Routes><Route>` |
| Lazy | `loadComponent` | `lazy()` + `<Suspense>` |
| Guard | `canActivate: [fn]` | pathless `<Route element={<Guard/>}>` |
| Nested outlet | `<router-outlet/>` | `<Outlet/>` |
| Redirect | `redirectTo` | `<Navigate replace>` |
| Navigate prog. | `router.navigate(['/x'])` | `navigate('/x')` |
| Read params | `input()` via `withComponentInputBinding()` / `paramMap` | `useParams()` |
| Query | `queryParamMap` | `useSearchParams()` |
| Link | `[routerLink]` | `<Link to>` |
| 404 | `path: '**'` | `path="*"` |

### 1.4 HTTP + auth token

**Angular**
```ts
// functional interceptor (modern)
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');
  return token ? next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })) : next(req);
};
// register
provideHttpClient(withInterceptors([authInterceptor]))
// call
getAll(): Observable<Product[]> { return this.http.get<Product[]>(`${this.apiUrl}/products`); }
```

**React**
```js
// api client module — the "interceptor"
async function request(path, options = {}) {
  const token = localStorage.getItem('token');
  const res = await fetch(`/api${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  });
  if (!res.ok) throw new Error((await res.json().catch(() => ({}))).detail ?? res.statusText);
  return res.status === 204 ? null : res.json();
}
export const productsAPI = { getAll: () => request('/products') };
```

| Concept | Angular | React |
|---|---|---|
| HTTP | `HttpClient` (Observable) | `fetch` (Promise) — or axios |
| Interceptor | `HttpInterceptorFn` | wrapper function in the API client |
| Cancel | unsubscribe / `takeUntilDestroyed` | `AbortController` |
| Retry | `retry({count:2})` | loop / library |
| Global error | interceptor `catchError` | try/catch + error boundary |

---

## 2. The concept-translation dictionary (memorise)

| Need | Angular | React | C# / .NET |
|---|---|---|---|
| Container | Component | Component (function) | Class |
| Template | HTML + Angular syntax | JSX | Razor `.cshtml` |
| Local state | `signal` | `useState` | field |
| Derived state | `computed` | `useMemo` / inline | expression-bodied property |
| Side effect | `effect` | `useEffect` | event handler / hosted service |
| Cross-cutting state | `providedIn:'root'` service w/ signals | Context / Redux store | Singleton service |
| DI | `inject(Token)` | Context / import | `IServiceProvider` |
| Middleware | HTTP interceptor | API client wrapper | ASP.NET middleware / `DelegatingHandler` |
| Route protection | `canActivate` | `<ProtectedRoute>` | `[Authorize]` |
| Pre-fetch | resolver | loader / suspending hook | action filter |
| List identity | `track` | `key` | — |
| Lazy module | `loadComponent` / `loadChildren` | `lazy()` | separate assembly |
| Build-time config | `environment.ts` | `import.meta.env.VITE_*` | `appsettings.{Env}.json` |
| Validation | `Validators` | library or custom | Data annotations / FluentValidation |
| Global error | `provideBrowserGlobalErrorListeners` | ErrorBoundary + `window.onerror` | `UseExceptionHandler` |
| Testing | Vitest + TestBed | Vitest + RTL | xUnit + WebApplicationFactory |

**The three questions this table answers in an interview:** "how does Angular do X?", "how does React do X?", "how would you do X in .NET?" — you can now answer all three in one breath, which is close to unique among candidates.

---

## 3. Angular syntax card

```ts
// ---- imports you'll actually type
import { Component, signal, computed, effect, inject, input, output,
         viewChild, OnInit, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { Router, RouterLink, ActivatedRoute, Routes } from '@angular/router';
import { HttpClient, HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { toSignal, toObservable, takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { map, switchMap, filter, debounceTime, distinctUntilChanged,
         catchError, tap, retry, shareReplay, of } from 'rxjs';

// ---- component skeleton
@Component({
  selector: 'app-x',
  imports: [RouterLink, ReactiveFormsModule],
  templateUrl: './x.html',
  styleUrl: './x.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class XComponent implements OnInit {
  private readonly http = inject(HttpClient);
  readonly items = signal<Item[]>([]);
  readonly total = computed(() => this.items().length);
  readonly search = signal('');
  readonly filtered = computed(() =>
    this.items().filter(i => i.name.toLowerCase().includes(this.search().toLowerCase())));

  ngOnInit() {
    this.http.get<Item[]>('/api/items')
      .pipe(takeUntilDestroyed())          // needs an injection context (field/ctor) — use DestroyRef in ngOnInit
      .subscribe(items => this.items.set(items));
  }
}

// ---- template syntax
// {{ expr }}   [prop]="expr"   (event)="fn($event)"   [(ngModel)]="prop"
// [class.active]="isActive"   [style.width.px]="w"    [attr.aria-label]="label"
// @if (c) {} @else if {} @else {}   @for (x of xs; track x.id) {} @empty {}
// @switch (v) { @case (1) {} @default {} }
// @let total = price * qty;
// {{ value | currency:'EUR' }}  {{ d | date:'short' }}  {{ o | json }}  {{ s | async }}

// ---- rxjs recipes
search$ = toObservable(this.search).pipe(debounceTime(300), distinctUntilChanged(),
  switchMap(q => this.http.get<Item[]>(`/api/items?q=${q}`)));

items$ = this.http.get<Item[]>('/api/items').pipe(shareReplay(1),
  catchError(() => of([])));

// ---- guard
export const authGuard: CanActivateFn = () =>
  inject(AuthService).isLoggedIn ? true : inject(Router).createUrlTree(['/login']);
```

**Gotchas:** `takeUntilDestroyed()` must run in an injection context — in `ngOnInit` pass `inject(DestroyRef)` as the argument. `@for` without `track` won't compile. `[(ngModel)]` needs `FormsModule` in `imports`. An OnPush component won't render on in-place mutation.

---

## 4. React syntax card

```jsx
// ---- imports
import { useState, useEffect, useMemo, useCallback, useRef, useContext,
         useReducer, useTransition, useDeferredValue, useId,
         useActionState, useOptimistic, use, lazy, Suspense, StrictMode } from 'react';
import { createSlice, createAsyncThunk, configureStore } from '@reduxjs/toolkit';
import { useDispatch, useSelector } from 'react-redux';
import { Routes, Route, Link, Navigate, Outlet, useParams,
         useSearchParams, useNavigate } from 'react-router-dom';

// ---- component skeleton
function ItemList({ initial = [] }) {
  const [items, setItems] = useState(initial);          // lazy init: useState(() => compute())
  const [query, setQuery] = useState('');
  const deferred = useDeferredValue(query);
  const filtered = useMemo(
    () => items.filter(i => i.name.toLowerCase().includes(deferred.toLowerCase())),
    [items, deferred]);

  useEffect(() => {
    const controller = new AbortController();
    (async () => {
      try { setItems(await api.getAll(controller.signal)); }
      catch (e) { if (e.name !== 'AbortError') console.error(e); }
    })();
    return () => controller.abort();
  }, []);                                                // deps: [] once, [a,b] on change, omitted = every render

  return (
    <>
      <input value={query} onChange={e => setQuery(e.target.value)} aria-label="Search" />
      {filtered.length === 0
        ? <p>No items</p>
        : <ul>{filtered.map(i => <li key={i.id}>{i.name}</li>)}</ul>}
    </>
  );
}

// ---- React 19 action form
const [state, formAction, isPending] = useActionState(loginAction, {});
<form action={formAction}>
  <input name="email" /><input name="password" type="password" />
  <button disabled={isPending}>Sign in</button>
  {state?.error && <p role="alert">{state.error}</p>}
</form>

// ---- RTK slice
export const fetchItems = createAsyncThunk('items/fetch', async (_, { rejectWithValue }) => {
  try { return await api.getAll(); } catch (e) { return rejectWithValue(e.message); }
});
const slice = createSlice({
  name: 'items',
  initialState: { items: [], status: 'idle', error: null },
  reducers: { add: (s, a) => { s.items.push(a.payload); } },   // Immer
  extraReducers: b => b
    .addCase(fetchItems.pending,   s => { s.status = 'loading'; })
    .addCase(fetchItems.fulfilled, (s, a) => { s.status = 'succeeded'; s.items = a.payload; })
    .addCase(fetchItems.rejected,  (s, a) => { s.status = 'failed'; s.error = a.payload; }),
});
export const { add } = slice.actions;
```

**Gotchas:** hooks only at the top level. No `async` directly in `useEffect`. `key` must be stable (never the index). `&&` with `0` renders `0` — use a ternary. Mutating state skips the render.

---

## 5. .NET / C# cheatsheet

> This is the **compressed** version. The full answer bank is `05A-CSHARP-AND-OOPS-FUNDAMENTALS.md` (language + OOP + patterns), `05B-DATA-STRUCTURES-AND-ALGORITHMS.md` (collections, Big-O, algorithm triggers, SQL) and `05C-CODING-QUESTION-BANK.md` (the coding round). Use this section for the 10-minute pre-interview refresh; use those for rehearsal.

```csharp
// ---- Collections: pick by access pattern
// List<T>        O(1) index, O(n) Contains        default ordered collection
// HashSet<T>     O(1) Contains                  membership / de-dupe
// Dictionary     O(1) lookup                    key -> value
// SortedDict     O(log n) everything             ordered + mutable
// Queue/Stack    O(1)                           BFS / DFS, undo, retry queue
// PriorityQueue  O(log n) enqueue/dequeue       top-K, scheduling, Dijkstra
// Channel<T>     async producer/consumer          modern in-process queue

// ---- LINQ: the two boundaries that decide performance
// deferred execution  -> nothing runs until enumerated; enumerate twice = run twice
// IQueryable vs IEnumerable -> SQL vs memory; ToList()/AsEnumerable() is the boundary
await db.Orders.AsNoTracking().Where(...).Select(o => new Dto(...)).ToListAsync(ct);

// ---- Money & time
decimal for money (never double); MidpointRounding.AsEven; round once, store it
DateTimeOffset for instants; TimeProvider for testability

// ---- DI lifetimes
builder.Services.AddSingleton<ICache, MemoryCacheService>();   // one for app lifetime
builder.Services.AddScoped<IOrderService, OrderService>();     // one per request
builder.Services.AddTransient<IEmailSender, SmtpSender>();     // new each resolve
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(cs));  // = Scoped

// ---- scope inside a singleton / hosted service
public class Worker(IServiceScopeFactory factory) : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken ct) {
        using var scope = factory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // ...
    }
}

// ---- options
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddOptions<JwtOptions>().BindConfiguration("Jwt").ValidateDataAnnotations().ValidateOnStart();

// ---- pipeline order
app.UseExceptionHandler();
app.UseHsts(); app.UseHttpsRedirection();
app.UseCors("spa");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health/ready");

// ---- EF Core query that scales
var page = await db.Orders
    .AsNoTracking()
    .Where(o => o.Status == OrderStatus.Pending && o.CustomerId == customerId)
    .OrderByDescending(o => o.CreatedAt)
    .Skip((pageNo - 1) * size).Take(size)
    .Select(o => new OrderSummaryDto { Id = o.Id, Total = o.Total, Items = o.Items.Count })
    .ToListAsync(ct);

// ---- bulk write without materialisation (EF 7+)
await db.Orders.Where(o => o.Status == Old).ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, New), ct);

// ---- transaction
await using var tx = await db.Database.BeginTransactionAsync(ct);
await db.SaveChangesAsync(ct);
await tx.CommitAsync(ct);

// ---- JWT validation
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true, ValidIssuer = cfg["Jwt:Issuer"],
        ValidateAudience = true, ValidAudience = cfg["Jwt:Audience"],
        ValidateLifetime = true, ClockSkew = TimeSpan.Zero,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!)),
    });

// ---- ProblemDetails
builder.Services.AddProblemDetails();
app.UseExceptionHandler();   // emits RFC 7807

// ---- resilience for outgoing HTTP
builder.Services.AddHttpClient<IPaymentClient, PaymentClient>()
    .AddStandardResilienceHandler();   // retry + timeout + circuit breaker
```

**Rules to recite:** `async` is scalability not speed · `CancellationToken` on every action · never `.Result`/`.Wait()` · `IQueryable` composes to SQL · `AsNoTracking` for reads · project to DTOs, never return entities · migrations as reviewed SQL scripts, not at startup · singletons own process state only.

---

## 6. HTTP status codes & error contract

| Code | Use |
|---|---|
| 200 | OK with body |
| 201 | Created + `Location` header |
| 204 | Success, no body (DELETE/PUT) |
| 400 | Malformed request / validation (`ValidationProblemDetails`) |
| 401 | Missing/invalid credentials |
| 403 | Authenticated, not allowed |
| 404 | Not found (also used to hide existence) |
| 409 | Conflict (concurrency, duplicate) |
| 422 | Semantically invalid |
| 429 | Rate limited (+ `Retry-After`) |
| 500 | Unhandled server error |
| 503 | Dependency unavailable (readiness failing) |

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-8f2c…-01",
  "errors": { "Email": ["Email is already registered"] }
}
```

`traceId` is the correlation hook — the SPA surfaces it, support quotes it, you find the log line instantly.

---

## 7. The 60-second project walkthrough (rehearse until automatic)

> "This repo has three applications that deliberately solve the same problem three ways.
>
> The **Angular 22** app in `frontend/` is standalone-bootstrapped — `bootstrapApplication` with an `ApplicationConfig` that plays the role of `Program.cs`. Routes are lazy via `loadComponent`, protected by a functional guard that returns a `UrlTree`, and the JWT is attached by an HTTP interceptor. Services are `providedIn: 'root'` singletons using `inject()`, and I've been moving component state to signals so change detection is targeted rather than zone-driven.
>
> The **React 19** app in `ReactDotNetDemo/` builds the same CRUD with hooks, Redux Toolkit slices with `createAsyncThunk`, and React Router 7 — the guard is a pathless route rendering `<Outlet/>` or `<Navigate>`.
>
> The **.NET** side has a compact demo API using JWT bearer auth and controllers, and a larger food-delivery system — `DeliverFoodApp` — with EF Core, MassTransit over RabbitMQ, Kafka, SignalR hubs for live order updates, gRPC, health checks and a separate consumer worker.
>
> Having built both front ends is why I can talk about the trade-offs rather than pick a favourite: Angular gives a large team one blessed path and maps naturally onto .NET's DI and middleware; React gives a small core and hands architecture decisions to the senior."

**That is 60 seconds and it proves breadth, depth, and ownership at once. Record yourself until it's fluent.**

---

## 8. Pre-interview checklist (the night before)

- [ ] `npm install && npm start` works in `frontend/`
- [ ] `npm install && npm run dev` works in `ReactDotNetDemo/`
- [ ] `dotnet run` works in `backend/AngularDotnetDemo.Api/`
- [ ] I can open `app.config.ts`, `app.routes.ts`, `auth.guard.ts`, `auth.interceptor.ts`, `productsSlice.js`, `store.js`, `Program.cs` and explain each in 30 seconds
- [ ] The 60-second walkthrough (§7) is fluent out loud
- [ ] The introduction (`00` §0A) is recorded at 90 seconds, 20 seconds and 3 minutes, word-perfect
- [ ] The five STAR stories each have a number in them
- [ ] I have four questions to ask them
- [ ] I know my answer to "how many years of Angular/React?" — **"six years of front-end work; the last year in Angular and React, in production, self-taught"** (`00` §0A.9 — never round a year up to two)
- [ ] I have the honest-boundary sentence ready for anything I don't know
- [ ] I slept. Tiredness looks like incompetence in interviews.

---

## 30-second version

The cheatsheets exist so that a concept question never finds you hunting for syntax: state → `signal`/`useState`, effect → `effect`/`useEffect`, cross-cutting → interceptor/API-client-wrapper/middleware, protection → `canActivate`/`ProtectedRoute`/`[Authorize]`, identity → `track`/`key`. Say the mapping out loud, because the mapping is the senior signal — it proves you learned the principle, not the API.
