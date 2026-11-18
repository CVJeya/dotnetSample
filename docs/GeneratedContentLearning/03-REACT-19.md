# Chapter 3 — React 19 Fundamentals & Hooks

> Files: `ReactDotNetDemo/src/main.jsx`, `App.jsx`, `components/*`, `features/*`
> Stack: React 19.2 · Vite 8 · react-router-dom 7 · Redux Toolkit 2 · Oxlint

---

## 1. React's model — the smallest possible surface

React has **one idea**: `UI = f(state)`. You write functions that return markup; React calls them again when state changes and reconciles the difference into the DOM.

| | Angular | React |
|---|---|---|
| Unit | Class + template (2 files) | **Function returning JSX** (1 file) |
| State | `signal()` object you read with `()` | `useState` — a value + setter |
| Rendering | Framework-driven, dirty-checking/signals | You call `setState` → React re-runs the component function |
| Template syntax | Real HTML + `{{ }}`, `[ ]`, `( )` | **JSX** — HTML-looking expressions in JS |
| DI | Built-in hierarchical injector | None — **Context** + module imports |
| Style | CSS scoped per component | CSS modules / plain CSS / inline |
| Change strategy | `OnPush` / zoneless | **Immutable state + memoisation** |

**The single most important difference to articulate:** Angular re-renders *components* that depend on changed signals. React re-renders **the component that owns the changed state and everything below it**, then reconciles. Everything about React performance flows from that — which is why immutability and memoisation matter more in React than in Angular.

### What is JSX, really?

JSX is not HTML and not a template language — it is **syntax sugar for `createElement` calls** (the classic runtime) or `jsx()` (the automatic runtime, which React 17+ uses and which is why you don't import React everywhere).

```jsx
const el = <h1 className="t">Hello {name}</h1>;

// compiles to roughly:
const el = jsx('h1', { className: 't', children: ['Hello ', name] });
```

Consequences you must know cold:

| JSX rule | Why |
|---|---|
| `className` not `class` | `class` is a reserved word in JS |
| `htmlFor` not `for` | same |
| Must return **one** root (or a fragment `<>…</>`) | A function returns one value |
| Attributes are camelCase (`onClick`, `tabIndex`) | They're object properties |
| `{expr}` inserts a value; **strings are escaped** | XSS-safe by default |
| `style` takes an object: `style={{ marginTop: 8 }}` | Not a string |
| `{cond && <X/>}` renders `cond` if `cond` is `0` | **Classic bug**: use `cond ? <X/> : null` when cond can be `0` |
| `{arr.map(x => <li key={x.id}/>)}` | Lists need a stable `key` |

`key` is the analogue of Angular's `track`. **Use a stable id, never the array index** — index keys break reordering, focus, and component state (the classic "typing in a list item moves text to the wrong row" bug).

---

## 2. Bootstrap (`ReactDotNetDemo/src/main.jsx`)

```jsx
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { Provider } from 'react-redux';
import { store } from './app/store';
import App from './App';
import './index.css';

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <Provider store={store}>
      <App />
    </Provider>
  </StrictMode>
);
```

| Piece | Meaning |
|---|---|
| `createRoot(...)` | React 18+ concurrent root. Replaces `ReactDOM.render` (legacy). |
| `.render(<App/>)` | Mounts. |
| `<StrictMode>` | **Dev-only** double-invocation of components/effects to surface impure code. This is the #1 "why did my API call fire twice?" question. |
| `<Provider store>` | Makes the Redux store available via context. |
| No `index.js` routing at root | Router lives inside `App.jsx` (`<BrowserRouter>` there or in `App`). |

**The StrictMode answer (memorise):**
> "In development `StrictMode` renders components twice and mounts/unmounts effects twice, on purpose — to prove your component is pure and your effect cleans up properly. In production it runs once. It's not a bug; if it breaks my app, my code was relying on an impure render or a leaky effect, so I fix the code, not the mode."

An interviewer *will* ask this. Answering it correctly and confidently signals real React experience.

---

## 3. Hooks — the whole mental model

### The rules (non-negotiable, they are runtime rules)

1. **Only call hooks at the top level** — never inside an `if`, loop, or nested function.
2. **Only in React function components or custom hooks.**

Why: React tracks state by **call order**, not by name. There is no name. A conditional hook shifts the order and React hands the wrong state back. This is the single most illuminating fact about hooks — and a favourite interview question. Strong follow-up line:

> "That's also why the `eslint-plugin-react-hooks` rule exists — it's not style, it's correctness. And it's why `useState` returns a pair rather than `this.state` — there is no `this`; the closure and the call order *are* the identity."

### `useState`

```jsx
const [count, setCount] = useState(0);
const [form, setForm] = useState({ name: '', email: '' });
```

Traps:
- **State updates are asynchronous and batched.** Reading `count` right after `setCount(count + 1)` gives the old value.
- Use the **functional updater** when the next value depends on the previous: `setCount(c => c + 1)`, and for objects `setForm(f => ({ ...f, name: 'x' }))` (spread — never mutate).
- **`useState` with an object/array: mutate = no re-render** if the reference is unchanged (React bails out when `Object.is` sees the same value).
- **Lazy initialiser:** `useState(() => expensiveParse())` runs only on mount, not every render. (Passing `expensiveParse()` would run it every render.)
- `setX` with a **new** value always re-renders even if deep-equal — so keep a single state object small, or memoise derived data.

### `useEffect` — the one everybody gets wrong

```jsx
useEffect(() => {
  const controller = new AbortController();
  productsAPI.getAll({ signal: controller.signal })
    .then(setProducts)
    .catch(err => { if (err.name !== 'AbortError') setError(err.message); });

  return () => controller.abort();     // cleanup
}, []);                                // deps
```

Facts to state precisely:

| Aspect | Truth |
|---|---|
| When it runs | **After** paint (does not block render). `useLayoutEffect` runs before paint. |
| Dependency array | `[]` = once on mount; `[a, b]` = when `a` or `b` change by `Object.is`; **omitted** = after every render |
| Cleanup | Runs before the next effect run **and** on unmount |
| StrictMode | Dev double-invoke means mount → cleanup → mount. Your effect must be idempotent and its cleanup correct |
| Never | Put an `async` function directly in the callback — return a cleanup or nothing. Wrap: `useEffect(() => { (async () => {...})(); }, [])` |
| Stale closures | The effect closes over the render's variables. Missing deps = stale reads. This is the mechanism behind most "why is my value always the initial one" bugs |

**Race condition pattern (the senior answer):** fetch inside `useEffect` + `AbortController` (or an `ignore` flag) so a slow earlier request can't overwrite a newer one. Then say: *"In practice I'd reach for a data library — TanStack Query or RTK Query — because caching, retries, dedupe, and cancel-on-unmount are solved problems and hand-rolled `useEffect` fetching is where I've seen the most bugs."*

**"What would you NOT use `useEffect` for?"** — Same trap as Angular's `effect`:
> "For deriving state. If `total` can be computed from `items`, it should be `const total = items.reduce(...)` in the render body, not state synchronised by an effect. Effects that set state from props are almost always a sign you want derived data or a remount (`key`)."

### `useRef`

```jsx
const inputRef = useRef(null);       // DOM access
const timerRef = useRef(null);       // mutable box, no re-render
```

Two purposes: (1) reaching a real DOM node, (2) holding a mutable value across renders **without causing a re-render**. Compare with Angular's `viewChild()` for (1). Ref mutation is the escape hatch from React's model — use it for timers, previous-value tracking, third-party chart instances.

### `useContext` — React's "DI"

```jsx
const ThemeContext = createContext('light');

// provider (App.jsx)
<ThemeContext.Provider value={theme}>...</ThemeContext.Provider>

// consumer anywhere below
const theme = useContext(ThemeContext);
```

The Angular comparison: **Context is React's hierarchical injection**. But there is a crucial difference: **every consumer re-renders when the context value changes**, so providers are typically split by concern (theme context separate from auth context) to limit fan-out. "Context is not a state manager" is a good line — it's a transport; the state still lives somewhere.

### `useMemo` / `useCallback`

```jsx
const sorted = useMemo(() => [...products].sort(byName), [products]);
const onSelect = useCallback((id) => setSelected(id), []);
```

| Hook | Caches | Angular analogue (roughly) |
|---|---|---|
| `useMemo` | a **value** | `computed` |
| `useCallback` | a **function identity** | none |
| `memo(Component)` | the **render output** | `OnPush` |

**The senior honesty answer:**
> "`useMemo`/`useCallback` are optimisations, not semantics — with React 19 and the compiler direction they're increasingly unnecessary. Adding them everywhere costs readability and allocates more than it saves. I add them when a profile shows a real problem: an expensive computation on every render, a dependency in another effect's array, or a memoised child whose props change identity each render. `useCallback` exists mainly so `React.memo` children don't re-render."

React 19's **compiler** (opt-in via `babel-plugin-react-compiler` / the React Compiler) auto-memoises — that's the trajectory and a great "what's new" talking point.

### Custom hooks — the reuse primitive

```jsx
function useFetch(url) {
  const [state, setState] = useState({ loading: true, data: null, error: null });
  useEffect(() => {
    let ignore = false;
    fetch(url).then(r => r.json())
      .then(d => !ignore && setState({ loading: false, data: d, error: null }));
    return () => { ignore = true; };
  }, [url]);
  return state;
}
```

Custom hooks are how React does what Angular does with services — **but they share logic, not state**. Two components calling `useFetch` get two independent states. Say that explicitly: it's the most common misconception. Shared state must live in a Context, a store, or a third-party cache.

---

## 4. React 19 — what actually changed (and what to say)

React 19 is the version in this repo (`19.2.8`). Know these five things well and you can answer any "what's new" question.

### 4.1 Actions & `useActionState`

The pendulum swung toward forms. Instead of hand-rolling `loading`/`error` around a submit handler:

```jsx
import { useActionState } from 'react';

async function loginAction(prevState, formData) {
  const res = await fetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      email: formData.get('email'),
      password: formData.get('password'),
    }),
  });
  if (!res.ok) return { error: 'Invalid credentials' };
  return { ok: true };
}

function Login() {
  const [state, formAction, isPending] = useActionState(loginAction, {});
  return (
    <form action={formAction}>
      <input name="email" /><input name="password" type="password" />
      {state.error && <p role="alert">{state.error}</p>}
      <button disabled={isPending}>{isPending ? 'Signing in…' : 'Sign in'}</button>
    </form>
  );
}
```

Key points: the action receives `FormData`, `isPending` comes free, and the form works **before hydration** when used with React Server Components. It also handles the double-click problem for you.

### 4.2 `useOptimistic` and `useFormStatus`

`useOptimistic` renders the presumed result immediately and rolls back if the action fails — the "like button" pattern.

```jsx
const [optimisticLikes, addLike] = useOptimistic(likes, (cur, delta) => cur + delta);
```

`useFormStatus` (from `react-dom`) lets a **child** of a `<form>` read `pending` without prop-drilling — the classic "SubmitButton" component.

### 4.3 `use()` — read a promise or a context

```jsx
const products = use(productsPromise);   // suspends until resolved
```

`use()` can be called conditionally (unlike other hooks) and it integrates with `<Suspense>` and `ErrorBoundary`. It's the bridge to Server Components.

### 4.4 `ref` as a prop — `forwardRef` is obsolete

```jsx
// React 18 and earlier
const Input = forwardRef((props, ref) => <input ref={ref} {...props} />);

// React 19
function Input({ ref, ...props }) { return <input ref={ref} {...props} />; }
```

React 19 lets function components take `ref` as a normal prop; `forwardRef` is deprecated (still works). **Also:** refs can now have a cleanup function returned from the callback ref, which replaces the old "save the node in a variable" trick.

### 4.5 Context as provider, and `useContext` shorthand

In React 19 you can render `<ThemeContext value={x}>` instead of `<ThemeContext.Provider value={x}>`, and `use(ThemeContext)` works.

### 4.6 Server Components / SSR — the strategic view

| Term | Meaning |
|---|---|
| **CSR** (this repo) | Browser downloads JS, React renders. Simple, but slow first paint and a big bundle |
| **SSR** | Server renders HTML per request, client hydrates. Fast paint, still ships JS |
| **RSC** (React Server Components) | Components that run **only on the server**, never shipped as JS. Data access at the source, zero bundle cost |
| **Hydration** | Client attaches to server HTML. Mismatches cause warnings and re-render |
| **Streaming** | `renderToPipeableStream` sends HTML in chunks with `<Suspense>` boundaries |

**The honest framing (this repo is a CSR Vite SPA — do not pretend otherwise):**
> "This demo is client-rendered with Vite. I understand the RSC/Next direction — server components for data-heavy static parts, client components for interactivity — but I'd choose it based on SEO and time-to-first-content requirements, not fashion. For an internal authenticated app behind a login, CSR is often the simpler and better call."

That is a genuinely senior judgement, and it's safe because it doesn't claim experience you don't have.

---

## 5. Rendering, reconciliation, and keys

**Reconciliation:** React keeps two trees (previous and next). It compares element **type** and **key** at each position. Same type → update props in place. Different type → unmount the subtree and rebuild it.

Why it matters:
- Changing `<div>` to `<section>` destroys all children (including their state). That's why a component that "loses its input focus/state" is usually being replaced rather than updated.
- A `key` change **forces a remount** — the deliberate reset mechanism: `<UserForm key={userId} />` resets the form when you switch users. This is a genuinely useful pattern to mention.
- List reconciliation with stable keys → DOM nodes are moved, not recreated; uncontrolled inputs and local state survive reordering.

**Batching:** React 18+ batches all state updates, including inside promises/timeouts (React 17 batched only in event handlers). So two `setState` calls in one handler = one render. Add "automatic batching" to your vocabulary.

**Concurrent features:** `useTransition` / `startTransition` mark non-urgent updates so typing stays responsive; `<Suspense>` shows fallbacks. These are the tools for the answer "how would you keep a heavy list responsive while filtering?" → `useDeferredValue(query)` / `startTransition`.

---

## 6. Error handling and boundaries

`ErrorBoundary` is **still class-only** (no hook equivalent for catching render errors) — one of the few places you must write a class component:

```jsx
class ErrorBoundary extends React.Component {
  state = { error: null };
  static getDerivedStateFromError(error) { return { error }; }
  componentDidCatch(error, info) { logToTelemetry(error, info); }
  render() { return this.state.error ? <p>Something broke.</p> : this.props.children; }
}
```

What it does **not** catch: errors in event handlers, async code, or the boundary itself — those need try/catch and `window.onerror`/`unhandledrejection`. Knowing the limitation is the senior part. Angular's equivalent is `provideBrowserGlobalErrorListeners()` + `ErrorHandler`.

---

## 7. React 19 performance — the honest checklist

In order of what actually helps:

1. **Don't over-render.** Keep state as local as possible; don't lift it "just in case" (lifting is needed for sharing, not for tidiness).
2. **Stable keys.** Never index.
3. **Virtualise long lists** (TanStack Virtual / react-window). Rendering 10,000 rows is the problem, not reconciliation.
4. **Split context** so a theme change doesn't re-render the auth tree.
5. **Code-split routes** with `React.lazy(() => import('./ProductList'))` + `<Suspense>`.
6. **`memo`/`useMemo`/`useCallback`** — only after measuring. React DevTools Profiler's "why did this render" is the tool to name.
7. **Cache server data** (TanStack Query / RTK Query) instead of refetching in effects.
8. **Bundle budget** — check what's large (`rollup-plugin-visualizer`), prefer named imports, avoid moment.js-class libraries.

**Interview answer to "how do you debug a slow React screen?"** Walk the method, not the tools:
> "First I reproduce with React DevTools Profiler and look at what committed and why. If the commit is wide, it's a state-placement problem — too much state at a high node. If it's one component rendering expensively, it's computation — memoise or virtualise. If it's network, it's a caching problem. I fix the cause the profiler shows rather than sprinkling `useMemo`."

---

## 8. Chapter 3 — questions you must be able to answer

1. What is `UI = f(state)` and how does it change how you write code vs jQuery? *(§1)*
2. Why does JSX need `className` and `key`? What does `key` actually do? *(§1, §5)*
3. Why can't hooks be conditional? What mechanism breaks? *(§3)*
4. `useEffect` dependency array: `[]`, `[a]`, and omitted — behaviour of each, and the cleanup timing. *(§3)*
5. Why does my `useEffect` fire twice in development? *(§2)*
6. How do you fix a race condition in an effect that fetches? *(§3)*
7. What should never be done with `useEffect`? *(§3)*
8. `useMemo` vs `useCallback` vs `React.memo` — what each caches. *(§3)*
9. Do custom hooks share state between components? *(§3)*
10. `useRef` vs `useState` — when does a change *not* re-render? *(§3)*
11. What's new in React 19 that you'd actually use? *(§4)*
12. `forwardRef` — what replaced it and why? *(§4)*
13. CSR vs SSR vs RSC — when would you choose each? *(§4.6)*
14. What does an ErrorBoundary NOT catch? *(§6)*
15. How do you reset a component's internal state when a prop changes? *(§5 — `key`)*
16. How do you keep a search input responsive while filtering 20k rows? *(§5 — `useDeferredValue`/`startTransition`)*

---

## 30-second version

React is `UI = f(state)`: a component is a function returning JSX, and `setState` re-runs it. Hooks are identified by **call order**, so they can't be conditional. `useState` needs immutable updates; `useEffect` is for synchronising with the outside world — never for derived state — and its cleanup plus `AbortController` are how you avoid leaks and races. `key` drives reconciliation, and changing a `key` deliberately remounts. React 19 adds Actions/`useActionState`, `useOptimistic`, `use()`, and `ref`-as-prop (killing `forwardRef`). Performance is a **state-placement** problem before it's a memoisation problem.
