# 3. Technical Depth

Format per topic: **Concepts → Hands-on → Resource.** Keep artifacts small and *measurable*.

---

## 3.1 C# / .NET

### Async/await (the #1 interview + production trap)
- Concepts: state machine, `SynchronizationContext`, `Task` vs `ValueTask`, `ConfigureAwait(false)`, `Task.WhenAll` vs sequential, `CancellationToken` propagation, `IAsyncEnumerable`, no `async void`, no `.Result`/`.Wait()`, avoid `Task.Run` on the server, thread-pool starvation.
- **Exercise:** an HTTP client that fetches 3 endpoints with `Task.WhenAll`, enforces a per-request timeout via linked `CancellationTokenSource`, and degrades gracefully. Prove with logs that it fails fast and doesn't leak.
- **Resource:** [Async in C# — Stephen Cleary / Microsoft "Async guidance"](https://learn.microsoft.com/dotnet/standard/asynchronous-programming-patterns/).

### DI & middleware
- Concepts: lifetimes (transient/scoped/singleton) and *captive dependency* bugs; the container being a service locator anti-pattern; `IOptions<T>` vs `IOptionsSnapshot` vs `IOptionsMonitor`; ordering of middleware; `UseExceptionHandler`, `UseAuthentication`/`Authorization` order; endpoint filters.
- **Exercise:** write a custom middleware that adds a correlation ID, times the request, and logs a structured line. Then deliberately add a bug (singleton → scoped) and show how `ValidateScopes` catches it in Development.
- **Resource:** Microsoft docs "Dependency injection in ASP.NET Core".

```csharp
// Correct: options validated at startup, scoped for per-request
builder.Services.AddOptions<DownstreamOptions>()
    .Bind(builder.Configuration.GetSection("Downstream"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### C# modern features worth leading with
- records / `required` members / init-only; pattern matching (`switch` expressions); `IAsyncEnumerable`; primary constructors; collection expressions; `FrozenDictionary`/`FrozenSet`; spans + `Memory<T>`; source generators; `System.Threading.Channels`.
- **Exercise:** mixin a `record` command/query layer for a small app, using `record` + `required` + pattern matching; measure allocation with `dotnet-counters`.
- **Resource:** "C# language reference — What's new" (Microsoft, per version).

### Testing that a lead expects
- Concepts: test pyramid, unit vs integration vs contract; `WebApplicationFactory`; Testcontainers for real DB/broker; deterministic clocks (`TimeProvider`); FluentAssertions/xUnit; approval tests.
- **Exercise:** one integration test using `WebApplicationFactory` + Testcontainers SQL Server that proves an endpoint's happy path *and* its 400/409 paths.
- **Resource:** Testcontainers for .NET docs.

---

## 3.2 EF Core performance

- Concepts: N+1, `Include` vs projection (`Select`), `AsNoTracking`, split vs single query, compiled queries, `ExecuteUpdate`/`ExecuteDelete`, keyset (seek) pagination, `DbContext` lifetime, connection pooling/`MARS`, execution plan first (SQL Server), avoiding client-side evaluation, indexing strategy, `EF.CompileQuery`.
- **Exercise:** take a slow endpoint in `LHDN_STSDS`, capture the generated SQL + plan, apply projection + `AsNoTracking` + a covering index, and record before/after (ms + logical reads) in an ADR.
- **Resource:** Microsoft "EF Core performance" docs + Brent Ozar "How to think like the SQL Server engine" (free).

```csharp
// Projection beats Include when you only need a shape
var page = await db.Orders
    .AsNoTracking()
    .Where(o => o.CustomerId == id && o.Id > afterId) // keyset paging
    .OrderBy(o => o.Id)
    .Take(50)
    .Select(o => new OrderListItem(o.Id, o.Number, o.Total, o.Status))
    .ToListAsync(ct);
```

- Anti-pattern alerts: lazy loading in services, `Include`-everything, `ToList()` before filtering, `Count()` for existence (use `Any()`), long-running "god" DbContext, migrations in app startup for prod.

---

## 3.3 Databases (SQL + NoSQL)

- **SQL Server tuning:** index types (clustered/nonclustered, covering, filtered, columnstore), SARGability, parameter sniffing, statistics, isolation levels + locking (`READ COMMITTED SNAPSHOT`), deadlocks, `MERGE` pitfalls, tempdb, partitioning basics.
- **Transactions:** ACID vs BASE, optimistic concurrency (`rowversion`) vs pessimistic, unit of work, distributed transactions vs saga, exactly-once at app level via idempotency keys.
- **NoSQL:** Cosmos DB (partition key design is *the* skill, RU/s, consistency levels), DynamoDB (single-table design, GSIs, hot partitions), Redis (cache-aside, TTL jitter, cache stampede, distributed locks via RedLock caveats, eviction policies).
- **Exercise:** implement cache-aside with Redis for a hot read endpoint + stampede protection (`SemaphoreSlim` per key or `HybridCache`); show hit-rate metrics. Then model a Cosmos container with a chosen partition key and justify it.
- **Resource:** "Designing Data-Intensive Applications" (Kleppmann) — chapters 2–3, 5–7, 9.

---

## 3.4 Security

- **OAuth2 / OIDC:** flows (auth code + PKCE for SPA/mobile; client credentials for services), scopes vs roles, access vs refresh tokens, token lifetimes, audience/issuer validation, `IClaimsTransformation`.
- **JWT:** signature (RS256 vs HS256), `kid` rotation, clock skew, don't put secrets in claims, validate `aud`/`iss`/`exp`/`nbf`, never trust client-supplied `alg`.
- **Entra ID / Azure AD:** app registrations, managed identity (no secrets in code), app roles, groups → claims, conditional access. AWS equivalent: Cognito + IAM roles/STS.
- **OWASP Top 10 (2021) applied:** A01 broken access control (IDOR — check every resource ownership), A02 crypto failures, A03 injection (parameterised SQL always; EF helps but raw SQL bites), A04 insecure design (rate-limit, anti-automation), A05 misconfig, A06 vulnerable components (dependency scanning), A07 auth failures (MFA, lockout), A08 integrity (SBOM, signed artifacts), A09 logging failures, A10 SSRF (validate/allow-list outbound URLs).
- **Secrets management:** Key Vault / Secrets Manager + managed identity; never appsettings.json; rotate; scan git history (`gitleaks`).
- **Exercise:** add OIDC JWT bearer auth to one endpoint, enforce a scope, add rate limiting, and wire dependency + container scanning into CI. Test with a token that has the wrong audience to prove rejection.
- **Resource:** OWASP Top 10 + "OAuth 2 in Action" (Richer & Sanso).

---

## 3.5 Frontend

### React
- Hooks rules & deps; `useState`/`useReducer`/`useMemo`/`useCallback` only when measured; **server state vs client state** (TanStack Query / RTK Query); context pitfalls (re-render storms); Suspense + Error Boundaries; `key` correctness; memoisation; forms (react-hook-form + zod); testing with React Testing Library + Playwright; Vite build; bundle analysis.
- **Exercise:** an authenticated list/detail screen with TanStack Query, optimistic update, error boundary, and 5 RTL tests. Measure re-renders with React DevTools Profiler before/after a `useMemo`.
- **Resource:** react.dev (new docs, "Learn" + "Escape hatches").

### Angular
- RxJS: cold vs hot, `switchMap`/`mergeMap`/`concatMap`/`exhaustMap`, `shareReplay`, `takeUntilDestroyed`, unsubscription; **signals** (`signal`, `computed`, `effect`, `linkedSignal`); standalone components vs NgModules; change detection (`OnPush`, zoneless); DI; routing guards/resolvers; NgRx vs signal-based state.
- **Exercise:** a dashboard with a search box using `debounceTime + switchMap` **and** a signal-based alternative; compare code size and reasoning. Use `ChangeDetectionStrategy.OnPush`.
- **Resource:** angular.dev (new docs) + "RxJS in Action".

### When to choose which
| Factor | React | Angular |
|---|---|---|
| Team skill / hiring | Larger pool, JS-first | Strong typing, opinionated, enterprise-friendly |
| Structure | You assemble (routing/state/forms) | Batteries included |
| Long-lived enterprise app | Fine with discipline | Often lower governance cost |
| Interactivity/composition | Excellent | Excellent (signals modernised it) |
Lead's rule: **choose for the team and the hiring pipeline, not for the trend**; document as an ADR (W17).

---

## Cross-cutting: "explain it in 3 minutes"

For every topic above, write 5 bullets: what it is · why it exists · the trade-off · the failure mode · when *not* to use it. That's your interview answer and your teaching material.
