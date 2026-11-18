# Chapter 1 — Angular 18+ Architecture & Fundamentals

> **Sitting a timed assessment?** `14-WORLDBANK-FULLSTACK-ANGULAR-ASSESSMENT.md` §7 asks this material as 30 exam questions with answers, plus a 30-question timed mock in §14.

> Every section references a real file in `frontend/`. Open them side by side.

---

## 1. The mental model shift (read this twice)

**Razor + jQuery:** the server renders HTML, you find elements and mutate them.

```js
// jQuery — imperative: you tell the DOM exactly what to do
$('#product-count').text(products.length);
$('#product-list').append('<li>' + p.name + '</li>');
```

Problems you have already felt: string concatenation in HTML, XSS via `.html()`, state living in two places (JS variables *and* the DOM), and "why is this stale?" bugs.

**Angular/React:** state is the only truth; the framework derives the DOM.

```ts
// Angular — declarative: you describe what the view looks like for a given state
products = signal<Product[]>([]);
```
```html
<p>{{ products().length }} products</p>
@for (p of products(); track p.id) { <li>{{ p.name }}</li> }
```

You never write "update the DOM". You write "set the state", and the framework diffs and patches. **A senior interviewer's favourite question is exactly this difference.** Memorise a crisp version:

> "In jQuery I owned DOM mutation and therefore every staleness bug. In Angular/React I own state transitions; the framework owns DOM mutation. That removes a whole bug class — fan-out updates, stale reads — and it's why I can unit-test the logic without touching the DOM."

---

## 2. How this Angular app boots

### `frontend/src/main.ts` (whole app, 7 lines)

```ts
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
```

Three things a senior candidate must be able to explain here:

| Element | What it means | The older equivalent (say this to show range) |
|---|---|---|
| `bootstrapApplication(App, appConfig)` | **Standalone bootstrap.** The root component *is* the application. | `platformBrowserDynamic().bootstrapModule(AppModule)` |
| No `AppModule` | Angular 15+ made `NgModule` optional; from v17+ standalone is the default and from v19+ `standalone` flag itself is implicit | `@NgModule({ declarations: [...], imports: [...], bootstrap: [...] })` |
| `.catch(...)` | Bootstrap is async — DI graph resolution and lazy chunk loading can fail | — |

**Interview line:** "Angular has been standalone-first since v17; `NgModule` still exists for compatibility and for grouping providers, but new code shouldn't need it. Removing modules is why imports are now per-component and lazy loading is reified via `loadComponent`."

### `frontend/src/app/app.config.ts` — the composition root

```ts
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
  ],
};
```

This is the Angular equivalent of `Program.cs` in your .NET world — **the composition root**. Draw the analogy explicitly in interviews; it's a powerful credibility move:

| `app.config.ts` | `Program.cs` |
|---|---|
| `providers: [...]` | `builder.Services.Add...()` |
| `provideHttpClient()` | `AddHttpClient()` |
| `provideRouter(routes)` | `UseRouting()` + endpoint map |
| `HTTP_INTERCEPTORS` multi provider | Middleware in the pipeline |
| `provideBrowserGlobalErrorListeners()` | `UseExceptionHandler()` / global filter |

### `providers` — and what `provide*` functions really are

`provideHttpClient()` returns a **provider array**, not a magic keyword. That's the modern "standalone provider" convention: features ship a function so you don't need `HttpClientModule`.

```ts
// legacy style — do NOT write this today
imports: [HttpClientModule]

// modern style
providers: [provideHttpClient()]
```

**Likely senior follow-up:** *"Difference between `provideHttpClient()` and `provideHttpClient(withFetch())`?"*
Answer: the default uses `XMLHttpRequest`; `withFetch()` switches to the native `fetch` API, which enables streaming (`reportProgress` events with `fetch` require it), better interop with service workers, and is required for SSR hydration performance. Add feature functions:
- `withInterceptors([fn])` — functional interceptors (preferred now)
- `withFetch()` — fetch backend
- `withRequestsMadeViaParent()` — for nested injectors/micro-frontends

### ⚠️ Your repo uses the **legacy interceptor style** — know why

`withInterceptorsFromDi()` + `{ provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }` is how you register a **class-based** interceptor. Modern Angular prefers a **functional** one:

```ts
// modern, v15+ preferred
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthService).token;
  return token
    ? next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }))
    : next(req);
};

// registered as:
provideHttpClient(withInterceptors([authInterceptor]))
```

**Say this in the interview:** "The repo I've been working in still uses the class-based DI interceptor with `withInterceptorsFromDi` — that's the pre-v15 pattern. I'd migrate to `HttpInterceptorFn` because it's a plain function: easier to unit test, no `multi: true` bookkeeping, and it composes with tree-shaking."

That single sentence demonstrates you know *current* Angular and can recognise legacy code. Extremely high value.

---

## 3. Components — the unit of everything

### Anatomy of a modern component (`frontend/src/app/components/product-list/product-list.ts`)

```ts
import { Component, OnInit, inject } from '@angular/core';
import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product';

@Component({
  selector: 'app-product-list',
  imports: [/* standalone: import only what THIS template needs */],
  templateUrl: './product-list.html',
  styleUrl: './product-list.css',
})
export class ProductList implements OnInit {
  private readonly productService = inject(ProductService);
  products: Product[] = [];
  loading = false;
  error = '';
  ...
}
```

Points to raise:

| Topic | Senior-level statement |
|---|---|
| `standalone: true` is default | No `declarations` array anywhere. A component declares its own dependencies in `imports`. Consequence: **good tree-shaking and obvious dependency graphs.** |
| `imports: [...]` | Only for components/directives/pipes **used in that template**. Forgetting one is the #1 beginner error ("`routerLink` is not a known property") |
| `inject()` over constructor | Works in field initialisers, in functions (guards/interceptors), and in inheritance. Constructor injection still works but `inject()` is the idiomatic v17+ style |
| File naming | v20+ CLI generates `product-list.ts` / `.html` / `.css` (no `.component` suffix). Both work — this repo uses the new convention |
| `implements OnInit` | Optional but good: compile-time typo protection on lifecycle names |

### Lifecycle hooks — and what is *not* in the list

`ngOnChanges` → `ngOnInit` → `ngDoCheck` → `ngAfterContentInit` → `ngAfterContentChecked` → `ngAfterViewInit` → `ngAfterViewChecked` → `ngOnDestroy`.

**The senior trap:** *"Which lifecycle hook should you use for data loading?"*
Weak answer: "`ngOnInit`". Strong answer:

> "`ngOnInit` for a component that receives its params as inputs, but if data depends on a route parameter, I prefer a **resolver** so the component receives already-resolved data — no empty-state flash — or I bind the route param to an input with `withComponentInputBinding()` and react with `effect`/`computed` on a signal. `ngOnInit` also doesn't re-run when only the param changes, which is the classic `/orders/1` → `/orders/2` same-component bug."

That last sentence — the classic bug — is a real-world scar. It's exactly the kind of thing that marks you as having shipped code.

Also state: **`ngOnChanges` only fires for `@Input()` bound values, not for signal inputs** (`input()`), where you use `effect()` or `computed()` instead.

---

## 4. Data binding — the four forms (memorise the table)

| Form | Syntax | Direction | Example in repo |
|---|---|---|---|
| Interpolation | `{{ value }}` | component → view | `{{ product.name }}` |
| Property binding | `[prop]="expr"` | component → view | `[disabled]="saving"` |
| Event binding | `(event)="handler($event)"` | view → component | `(click)="save()"` |
| Two-way | `[(ngModel)]="prop"` | both | template-driven forms only |

Plus attribute vs property: `[attr.aria-label]` sets an HTML attribute, `[class.active]` toggles a class, `[style.width.px]` sets a style. And the *banana-in-a-box* naming: `[(x)]` is literally `[x]` + `(xChange)`.

**Angular 18+ gotcha:** `[(ngModel)]` requires importing `FormsModule` into the component's `imports`. Under `NgModule` it was global. This is the second-most common beginner error.

---

## 5. Control flow blocks (Angular 17+ / default in 18+)

`*ngIf` / `*ngFor` / `*ngSwitch` are legacy. The block syntax is built into the template compiler — **no import needed**, and measurably faster (no wrapping comment nodes, better hydration, `track` required).

```html
@if (loading) {
  <p>Loading…</p>
} @else if (error) {
  <p class="error">{{ error }}</p>
} @else {
  @for (p of products(); track p.id) {
    <app-product-card [product]="p" />
  } @empty {
    <p>No products yet.</p>
  }
}

@switch (role) {
  @case ('admin') { <app-admin-panel /> }
  @default      { <app-readonly /> }
}
```

| Legacy | Modern | Notes |
|---|---|---|
| `*ngIf="x"` | `@if (x) { }` | `@else if`, `@else` are real keywords |
| `*ngFor="let p of ps"` | `@for (p of ps; track p.id)` | **`track` is mandatory** — replaces `trackBy` |
| `*ngSwitch` | `@switch` | |
| `ng-template #tpl` + `*ngTemplateOutlet` | `@if/@for` + `<ng-template>` still for advanced cases | |
| `let-x="y"` microsyntax | `@let x = expr;` (v18.1+) | Local template variable derived from a signal — avoids duplicating expressions |

### Why `track` matters — the performance answer

`track` tells Angular how to identify a row across renders. Without a stable key, inserting at the top of a 500-row list re-creates every DOM node. With `track p.id`, Angular **moves** existing nodes and only creates the new one.

**Interview quality line:** "`trackBy` did the same job in `*ngFor`, but it was optional so people forgot it. In the block syntax it's required by the compiler — Angular used a breaking change to force the performance-correct default. That's a pattern I like: make the fast path the only path."

---

## 6. Signals — the biggest change since Angular 2

Signals are how Angular 18+ does reactivity. **If you learn one thing from this chapter, learn this.**

### The problem they solve

With plain fields, Angular doesn't know what changed, so it re-checks the whole component tree (that's change detection). With signals, Angular knows **exactly** which views depend on which values.

```ts
import { signal, computed, effect } from '@angular/core';

export class CartComponent {
  // writable signal — source of truth
  readonly items = signal<CartItem[]>([]);

  // derived — lazy, cached, recomputes only when items changes
  readonly count = computed(() => this.items().length);
  readonly total = computed(() =>
    this.items().reduce((s, i) => s + i.price * i.qty, 0));

  // side effect — runs once per change, after render
  constructor() {
    effect(() => console.log('cart now', this.count()));
  }

  add(p: Product) {
    this.items.update(list => [...list, { ...p, qty: 1 }]); // immutable update
  }
}
```

### The three primitives

| Primitive | Role | React analogue |
|---|---|---|
| `signal(v)` | writable state | `useState` |
| `computed(fn)` | derived, memoised | `useMemo` |
| `effect(fn)` | side effect on change | `useEffect` (⚠️ different timing/rules) |

Read/write API: `s()` to read, `s.set(v)`, `s.update(fn)`. `set` for replacement, `update` for deriving from the previous value.

### Why `computed` is not just a getter

A getter re-executes on every change-detection cycle. A `computed` caches its value and only recomputes when a **dependency signal** changes — and it tracks dependencies **dynamically** (an `if` branch that doesn't read `b` won't register `b`).

### `effect()` — the guarded tool

Senior position: **`effect` is for synchronising with non-Angular systems** (localStorage, a chart library, analytics). It is *not* the right way to derive state.

> "I avoid `effect` for state derivation — a `computed` is declarative and testable, an `effect` writes somewhere else and can produce loops. I'll use `effect` to sync a signal into `localStorage`, or to call a charting library's imperative API after the signal changes."

Angular's own docs and team members say exactly this. Saying it signals you read beyond tutorials.

### Signals in a service — the real pattern

```ts
@Injectable({ providedIn: 'root' })
export class CartStore {
  private readonly _items = signal<CartItem[]>([]);
  readonly items = this._items.asReadonly();     // expose read-only
  readonly total = computed(() => …);
}
```

`asReadonly()` prevents consumers from calling `.set()` — encapsulation in the same spirit as a `private set` in C#. **Make that comparison out loud** — it lands well with .NET interviewers.

### Interop with RxJS (both directions, both matter)

```ts
// Observable → Signal
readonly products = toSignal(this.productService.getAll(), { initialValue: [] });

// Signal → Observable
readonly query$ = toObservable(this.query).pipe(debounceTime(300), switchMap(q => this.api.search(q)));
```

This is the bridge pattern for real projects: HTTP is Observable-based, components want signals. `toSignal` must be called in an **injection context** (constructor/field initialiser).

### Signal inputs / outputs / queries (v17.2+)

```ts
export class ProductCard {
  readonly product = input.required<Product>();          // replaces @Input() + required
  readonly currency = input('EUR');                      // with default
  readonly selected = output<Product>();                 // replaces @Output() + EventEmitter

  // view query, replaces @ViewChild
  private readonly chart = viewChild.required<ElementRef>('chart');
}
```

Do NOT say "signals replace `@Input`" — `@Input` still works. Say: *"signal inputs are the new default because they compose with `computed` — `<ProductCard [product]="p"/>` where `p` is a signal now updates reactively without `ngOnChanges`."*

**Out loud answer to "input() vs @Input()":**
> "Both are inputs. `@Input()` gives you a mutable field and `ngOnChanges` calls; `input()` gives you a read-only signal that participates in the reactive graph, so I can build a `computed` off it. `input.required()` replaces the old `@Input() x!: T` with a runtime error if the parent forgets the binding. I'd use signal inputs for anything new."

---

## 7. Dependency Injection — depth for a senior

### The hierarchy

Angular DI is **hierarchical**, resolved up the injector tree:

1. **`providedIn: 'root'`** → one instance for the whole app (like a .NET **Singleton**).
2. **Provided in a route's `providers`** → instance scoped to that route subtree (like **Scoped**).
3. **Provided in a component's `providers`** → new instance per component instance (like **Transient per-instance**, but retained for the component's lifetime).

**This table is gold in a .NET interview.** Angular lifetimes map onto .NET lifetimes almost exactly:

| Angular | .NET | Lifetime |
|---|---|---|
| `providedIn: 'root'` | `AddSingleton<T>()` | app lifetime |
| route/component `providers` | `AddScoped<T>()` (per-request-ish) | subtree/component lifetime |
| `useFactory` (no cache) | `AddTransient<T>()` | new each injection |

**A real perf/state trap to mention:** putting a state service in a **lazy-loaded route's** `providers` means two route visits give two instances, and state silently resets. `providedIn: 'root'` avoids it but grows memory. Know both sides.

### `inject()` — why it exists

```ts
export const authGuard = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  ...
};
```

`inject()` works in: field initialisers, constructors, **functions executed in an injection context** (guards, interceptors, factories), and `runInInjectionContext`. It does *not* work in a random `setTimeout` callback.

Why it beats constructor injection in practice: no constructor-parameter noise, works with inheritance/mixins, and it's the only option in functional guards/interceptors. **Downside to name:** it hides dependencies (harder to see what a class needs at a glance). A senior names the downside.

### `InjectionToken` — the Angular way to inject config

```ts
export const API_URL = new InjectionToken<string>('API_URL');

providers: [{ provide: API_URL, useValue: environment.apiUrl }];
// consume
private readonly apiUrl = inject(API_URL);
```

Compare to `IOptions<T>` in ASP.NET Core — same idea: inject a typed, named value instead of a stringly-typed config lookup. This repo instead imports `environment.apiUrl` directly, which is fine for a single app but doesn't allow per-test override and bakes the value at build time. Say that trade-off.

### Provider kinds

`useClass` (default), `useValue` (static), `useFactory` (call a function — can depend on other services via `deps` or `inject`), `useExisting` (alias an existing instance). Testing lives off `useValue`/`useFactory` + `provide: X, useValue: mock`.

---

## 8. Change detection, `OnPush` and zoneless (v18 → v20+)

This is a favourite senior question because it separates people who read release notes from people who read the runtime.

### The old world — Zone.js

Angular patched every async browser API (`setTimeout`, `addEventListener`, XHR, promises). After *any* of them the whole component tree was re-checked ("dirty checking"). It worked everywhere and scaled badly.

### `ChangeDetectionStrategy.OnPush`

```ts
@Component({ ..., changeDetection: ChangeDetectionStrategy.OnPush })
```

OnPush means: only re-check this component if (a) an **input reference** changed, (b) an **event** originated from its template, (c) an observable bound with `async` emitted, or (d) a **signal** it reads changed.

**The #1 OnPush bug you must be able to describe:** mutating an array/object in place (`this.items.push(x)`) does not change the reference, so an OnPush component never re-renders. Fix: immutable update (`[...items, x]`) — which is exactly what `signal.update` encourages, and exactly what React requires too. **This is a great cross-framework point:** *both* frameworks want immutable updates; Angular's signal API nudges you into it.

### Zoneless (the present and future)

Angular 18 shipped `provideExperimentalZonelessChangeDetection()`; it stabilised (`provideZonelessChangeDetection()`) in v20/21 as the default for new apps, dropping `zone.js` entirely. Change detection then runs on **signal notifications** and explicit `markForCheck()`/`ChangeDetectorRef` calls.

Why it matters: no 100 KB-ish zone.js patch cost, no "everything re-checks after every async call", better interop with non-Angular libs, better SSR/core-web-vitals.

**Speakable answer:**
> "Angular is moving from implicit zone-triggered dirty checking to explicit signal-driven notifications. `OnPush` was the manual version of that. Zoneless makes it the default, which is why signals are described as a foundation rather than a nice-to-have — and it's why the codebase I worked in moved `providedIn` services to signal stores rather than `BehaviorSubject`."

Note the interoperability rule: with zoneless, **anything not signal-based** (a `setTimeout` handler mutating a field) won't trigger a render unless you `markForCheck()`. That's the migration pain and a great "what would you watch out for" answer.

---

## 9. TypeScript you actually need

You know C#; TypeScript is easy for you *except* the structural typing and the `undefined` discipline.

| TS concept | C# analogue / note |
|---|---|
| `interface Product { id: number }` | Like a C# interface but **structural** — shape matters, not declared name |
| `interface` vs `type` | `interface` merges & extends; `type` allows unions/mapped types. Use `type` for unions |
| `string \| null` (union) | No direct C# analogue — like `Nullable<T>` but for any shape |
| optional `x?: string` | means `string \| undefined` |
| `readonly` | like C# `readonly` field / `init` property |
| `Partial<T>`, `Pick<T,K>`, `Omit<T,K>`, `Required<T>` | utility types; `Partial` ≈ "all properties optional", great for DTO/patch payloads |
| Generics | same as C# |
| `unknown` vs `any` | `any` disables checking (avoid); `unknown` forces a narrowing check first |
| `strict: true` | **the single most valuable tsconfig flag** — forces null handling. This is what makes TS feel like C# |
| type guards | `function isProduct(x: unknown): x is Product` — like a C# `is` pattern with custom logic |
| decorators | `@Component`, `@Injectable` — TS "experimental decorators", analogous to C# attributes but executed at build time |

### The demo talking point: 5 years of C# and now TS

> "TypeScript felt like coming home from JavaScript — structural typing instead of nominal, but generics, unions and null-safety give me the confidence I had in C#. The main habit I had to build is that `undefined` is everywhere in the DOM/HTTP world, so `strictNullChecks` is non-negotiable, and API responses get validated at the boundary rather than trusted."

This repo's models (`frontend/src/app/models/product.ts`) are plain interfaces — worth noting that interfaces are **erased at runtime**, so a cast `as Product[]` on an HTTP response is a *lie* unless you validate. Seniors mention runtime validation (Zod/Valibot, or a mapping layer).

---

## 10. Build & tooling (not glamorous, always asked)

`frontend/package.json` scripts typically: `start` (`ng serve`), `build` (`ng build`), `test` (Vitest), `lint`.

- **`@angular/build:application`** in `angular.json` — the modern esbuild/Vite builder replacing webpack (`browser` builder). Benefits: much faster cold build, better tree-shaking, native ESM.
- **`@angular/build:unit-test` + Vitest** — replaces Karma+Jasmine. Karma was browser-only and slow; Vitest runs in Node with jsdom, parallel, with a Jest-compatible API.
- **Budgets** — `maximumWarning`/`maximumError` on bundle size. Interview point: *"budgets failed our CI when a developer imported all of lodash — that guardrail is worth more than a code review comment."*
- **`environment.ts` is build-time, not runtime.** The value is inlined into the bundle. So "change the API URL per environment" needs a build per environment, `fileReplacements` in `angular.json`, or runtime config loaded via `APP_INITIALIZER`/a `/config.json` fetch. **This is a genuinely senior question** ("how do you deploy one artifact to three environments?") and the repo's `apiUrl: '/api'` + nginx proxy is the simple answer: keep the URL relative and let infra route it. In Docker, nginx proxies `/api` to the backend, so one artifact works everywhere. Say exactly that.
- **`provideBrowserGlobalErrorListeners()`** — funnels uncaught errors to `ErrorHandler`. In production you'd feed that to a monitoring sink (Sentry/App Insights).

---

## 11. Testing — the modern setup

Vitest + Angular's `TestBed`-free style is the direction: test **signals and services as plain classes**.

```ts
// service with inject() is easy to test if you avoid constructor side effects
describe('CartStore', () => {
  it('computes total', () => {
    const store = new CartStore();
    store.add({ id: 1, name: 'X', price: 10 } as Product);
    expect(store.total()).toBe(10);
  });
});
```

For components with `HttpClient`, use `provideHttpClientTesting()` + `HttpTestingController`:

```ts
TestBed.configureTestingModule({
  providers: [provideHttpClient(), provideHttpClientTesting()],
});
const httpMock = TestBed.inject(HttpTestingController);
const svc = TestBed.inject(ProductService);
svc.getAll().subscribe(r => expect(r.length).toBe(2));
httpMock.expectOne('/api/products').flush([{ id: 1 }, { id: 2 }] as Product[]);
```

**Senior framing on the testing pyramid for front ends:**
> "Components get few tests — they're mostly framework wiring. Services and pure functions get most of them. End-to-end gets a smoke path per critical journey. The trap is snapshot/`TestBed` tests that break on every CSS tweak and get deleted two sprints later."

---

## 12. Chapter 1 — questions you must be able to answer

1. Explain the difference between Angular and jQuery in one sentence. *(§1)*
2. What does `bootstrapApplication` do and what replaced `AppModule`? *(§2)*
3. What are `app.config.ts` providers analogous to in ASP.NET Core? *(§2)*
4. Difference between `provideHttpClient()` and `provideHttpClient(withFetch())`? *(§2)*
5. Class interceptor vs functional interceptor — and why prefer the latter? *(§2)*
6. `providedIn: 'root'` vs component `providers` — state the lifetime difference and the bug each causes. *(§7)*
7. `input()` vs `@Input()`, `output()` vs `@Output()`. *(§6)*
8. Why is `track` mandatory in `@for`? *(§5)*
9. What is a `computed` and how does it differ from a getter? *(§6)*
10. When should you NOT use `effect()`? *(§6)*
11. What does `OnPush` change, and what bug appears if you mutate an array in place? *(§8)*
12. What is zoneless change detection and why does Angular want it? *(§8)*
13. Which lifecycle hook for route-param-dependent data, and why is `ngOnInit` risky there? *(§3)*
14. How do you make one build artifact deployable to multiple environments? *(§10)*

---

## 30-second version

Angular apps bootstrap a **standalone root component** with a **provider list** (the composition root, like `Program.cs`). Components are classes with templates; they hold **signals** as state, derive with **computed**, and use `@if`/`@for` blocks with mandatory `track`. DI is hierarchical: `root` ≈ singleton, route/component providers ≈ scoped. **Signals + `OnPush`/zoneless** replaced zone.js dirty-checking, and `effect` is only for syncing with non-Angular code. Interceptors and guards are now **plain functions** using `inject()`.
