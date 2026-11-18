# 03 — Technical Depth: .NET + EF Core
### Grounded in the real `LHDN_STSDS` codebase (branch `eDScompoundCalculation`, HEAD `56d70b61e`)

> This is not a generic .NET tutorial. Every topic below is tied to a file, a line, or a
> measured count **from your own codebase**. You learn the concept, then you fix/refactor the
> real thing in `LHDN_STSDS/`. If you can articulate these nine topics about *this* system,
> you can articulate them about any system in an interview.

---

## 0. How to use this document

- Each topic = **Concept → "What your code does today" (evidence) → Hands-on lab (a real file) → 1 resource**.
- Labs are designed to be done in the 8 h/week budget; each is **≤ 2 h** and produces a PR-able change.
- The counts in this doc were measured on HEAD `56d70b61e`. If you re-measure later, use the commands in §11.
- **Rule:** never claim a perf/architecture win you can't show with a before/after number or a graph. This codebase is old enough that most wins are 10×+, and you need to *prove* them.

---

## 1. Reality map — what the codebase actually is

| Fact | Value | Source |
|---|---|---|
| Target framework | `net8.0` | `LHDN_STSDS/LHDN_STSDS.csproj:3` |
| Nullable context | `annotations` (warnings **off**, ~2,900 CS86xx suppressed) | `csproj:10` |
| C# source files (excl. `bin`/`obj`) | **685** | measured |
| Controller files | **65** | `LHDN_STSDS/Controllers/**` |
| `Program.cs` | **1,303 lines** — god-file: Dapper config, Kestrel limits, ~120 DI regs, HTTP clients, middleware, endpoints | `LHDN_STSDS/Program.cs` |
| `AppDbContext.cs` | **1,751 lines** — one context, hundreds of `DbSet`s, giant `OnModelCreating` | `LHDN_STSDS/Data/AppDbContext.cs` |
| ORM #1 | **EF Core** over Oracle (`Oracle.EntityFrameworkCore 9.23.80`), `UseOracle` | `Program.cs:137` |
| ORM #2 | **Dapper** with a global `TypeMapProvider` for `[Column]` → Oracle `UPPER_SNAKE_CASE` | `Program.cs` |
| EF tooling | `Microsoft.EntityFrameworkCore.Tools 10.0.6` → **version skew vs the 9.x runtime provider** | `csproj` |
| Background jobs | **Hangfire** + dashboard at `/hangfire` | `Program.cs:1007` |
| Realtime | **SignalR** (`NotificationHub` at `/hubs/notification`) | `Program.cs:1262` |
| Multi-schema | Active Oracle schema read from config (`DatabaseSchemas:SchemaName`, default `STSDS_EDS`) in the `AppDbContext` ctor | `AppDbContext.cs:40` |
| Module seams | `LHDN_STSDS/Modules/{BulkProcess, ImportData, BulkUpload, ...}` | `Modules/` |

### Measured pattern frequencies — your starting scorecard

| Pattern | Count | Verdict |
|---|---|---|
| `AsNoTracking` | **863** | ✅ present — but applied ad-hoc, not by policy |
| `Include(` | **1,293** | ⚠️ eager loading everywhere → over-fetch |
| `ThenInclude` | **352** | 🚩 cartesian-explosion risk (no `AsSplitQuery`) |
| `ToListAsync(` | **698** | ✅ |
| `ToList()` | **1,150** | 🚩 mixed — many are sync materialisation of EF queries |
| `SaveChangesAsync` | **594** | ✅ |
| `SaveChanges()` | **37** | ⚠️ sync writes |
| `BeginTransaction` | **34** | ⚠️ manual transactions in controllers |
| `ExecuteSqlInterpolated` | **21** | ✅ safe interpolation |
| `FromSqlRaw` | **9** | ⚠️ audit for concatenation |
| `ExecuteSqlRaw` | **6** | 🚩 injection + parameter-typing risk |
| `.Result` | **21** | 🚩 sync-over-async → thread-pool starvation |
| `.Wait()` | **2** | 🚩 |
| `Task.Run` | **7** | ⚠️ audit — usually wrong in ASP.NET Core |
| `async void` | **0** | ✅ |
| `catch (Exception` | **883** | ⚠️ swallow/`.Message` risk |
| `Console.WriteLine` | **145** | 🚩 should be `ILogger` |
| `new HttpClient(` | **35** | 🚩 socket exhaustion / DNS staleness |

**Reading the scorecard like a lead:** this is a *mature, working, high-throughput* system (594 async saves, 863 no-tracking reads prove the team knows the basics). The gaps are **consistency, policy, and boundaries** — not ignorance. That's exactly the L3→L4 lead transition: you don't "know more EF tricks," you make the good pattern the *default* and the bad one *impossible*.

---

## 2. Async / await — the topic that separates senior from lead

**Concepts**
- Async is about **I/O-bound scalability**, not speed. One thread awaits, the thread returns to the pool.
- `async void` = fire-and-forget with no way to observe failure → process crash. Count: **0**. Good.
- `.Result` / `.Wait()` on an `async` call inside a request = **sync-over-async** → thread-pool starvation under load → cascading timeouts. Count: **21 `.Result`, 2 `.Wait()`**.
- `Task.Run` in ASP.NET Core is (almost) always wrong — you *already* have a thread; you just move work and lose the `HttpContext`. Count: **7** → audit each.
- `ConfigureAwait(false)` in library code matters; in ASP.NET Core app code it's optional (no sync context).
- **CancellationToken** must be threaded request → EF → HTTP. `RequestAborted` is your friend.

### What your code does today
- `Services/AuditLogService.cs:333, :779` — `.ToList()` on queries inside a service (sync materialisation).
- 21 `.Result` sites across services/controllers, 2 `.Wait()`.
- `Program.cs:976` — a custom `app.Use(async (context, next) => …)` middleware (good place to observe cancellation).

### Hands-on lab 2 — kill sync-over-async (≤ 2 h)
1. Run: `findstr /s /n /c:".Result" LHDN_STSDS\*.cs` (or `search_files` with `\.Result(?![a-zA-Z])`).
2. Classify each of the 21 hits: **A) genuine sync-over-async on EF/HTTP** (fix), **B) `Task.Result` on an already-completed task** (leave), **C) property named `Result`** (ignore).
3. Convert category A to `await`. Add `CancellationToken ct` parameters where the method is on a request path; pass `HttpContext.RequestAborted` from the controller.
4. **Deliverable:** a PR that turns N `.Result` into `await` + a note in the PR body: *"N blocking waits removed; under load these starved the thread pool."*

**Resource:** .NET docs — *Async/Await Best Practices* + Stephen Cleary, *Async and Await* (blog series).

---

## 3. DI & lifetimes — the captive-dependency trap

**Concepts**
- `Singleton` lives forever, `Scoped` per request, `Transient` each resolve.
- **Captive dependency** = a `Singleton` holding a `Scoped` (e.g. `AppDbContext`). Symptoms: `ObjectDisposedException`, `A second operation started on this context`, memory growth.
- `Program.cs` holds **~120 registrations** — that's a maintenance and correctness risk.
- **DbContext pooling** (`AddDbContextPool`) reduces allocation; **`IDbContextFactory`** is the correct tool for Hangfire jobs, hosted services, and any code outside a request scope.

### What your code does today
- `Program.cs:137` — `builder.Services.AddDbContext<AppDbContext>(…)`: **scoped, unpooled**.
- `Program.cs` — a mix of `AddScoped` / `AddSingleton` / `AddTransient` across ~120 lines. Any `Singleton` that transitively touches `AppDbContext` is a latent bug.
- **Hangfire** background jobs (`Program.cs:1007`) do **not** have a request scope → they must resolve a fresh context. If they reuse an injected `AppDbContext`, that's a captive/scoped misuse.
- `Services/ApplicationRepositoryServices.cs` injects `AppDbContext` directly (`private readonly AppDbContext _dbContext;`) — fine for scoped, fatal if that repo is ever registered as a singleton.

### Hands-on lab 3 — captive-dependency audit + pool (≤ 2 h)
1. Enable scope validation in Development: `builder.Host.UseDefaultServiceProvider(o => { o.ValidateScopes = true; o.ValidateOnBuild = true; });` — then run the app and **fix every violation it reports**.
2. Grep for `AddSingleton` and follow each type's constructor graph for `AppDbContext` / `HttpContextAccessor`.
3. Switch `AddDbContext` → `AddDbContextPool<AppDbContext>(…, poolSize: 128)` **only after** confirming the context doesn't store per-request state in fields (verify the ctor — it currently reads config, which is safe/constant).
4. **Deliverable:** PR + an ADR: *"0007 — DbContext lifetime & pooling policy."*

**Resource:** Microsoft — *Dependency injection in .NET* (service lifetimes) + *DbContext pooling* docs.

---

## 4. Middleware & the request pipeline — where cross-cutting concerns live

**Concepts**
- Middleware = ordered `Func<RequestDelegate, RequestDelegate>` chain. **Order is semantics.** `UseExceptionHandler` must be first; `UseAuthentication` before `UseAuthorization`; `UseRouting` before `UseEndpoints`.
- Convention vs factory vs minimal middleware; branch with `Map`/`MapWhen`.
- **A custom middleware is the right home** for request correlation IDs, tenant/schema resolution, timing, and security headers.

### What your code does today
- `Middleware/SecurityHeadersMiddleware.cs` + `SecurityHeadersOptions.cs` + `SecurityHeadersExtensions.cs` — a **well-built, options-pattern middleware** and `IApplicationBuilder` extension. This is genuinely good; use it as your *reference implementation* when you explain middleware in interviews.
- `Middleware/Auth/StaticTokenAuthHandler.cs` — a custom `AuthenticationHandler<TOptions>` for static-token auth.
- `Program.cs:920` `app.Use(…)`, `:968` `UseSwagger`, `:976` inline `app.Use(async …)`, `:1007` `UseHangfireDashboard`, `:1262` `MapHub<NotificationHub>`.
- **Risk:** the pipeline order is scattered across ~340 lines (`920–1262`). Hard to reason about; easy to misorder.

### Hands-on lab 4 — request-correlation middleware (≤ 2 h)
1. Read `SecurityHeadersMiddleware.cs` and copy its shape (options class + `InvokeAsync` + extension method).
2. Write `CorrelationIdMiddleware`: read/propagate `X-Correlation-Id`, push it into a `ILogger` scope (`using (_logger.BeginScope(...))`), echo it in the response header, and stash it in `HttpContext.Items` for later use.
3. Register it **immediately after** the exception handler and before routing.
4. **Deliverable:** every log line now carries a correlation id → this is the foundation for the OpenTelemetry work in `04-architecture-system-design.md`.

**Resource:** Microsoft — *ASP.NET Core middleware* (order + custom middleware).

---

## 5. EF Core I — model, mapping & the 1,751-line context

**Concepts**
- `DbContext` = unit of work + identity map + change tracker. A *huge* context (hundreds of sets) slows model building and startup, and makes migrations brittle.
- Mapping essentials: keys, relations, `HasPrecision` for decimals (Oracle `NUMBER`), value converters, `HasDefaultValueSql` for DB-generated values (sequences/`SYSDATE`), owned types, global query filters.
- **Dynamic schema** (reading schema from config in the ctor) interacts with **model caching**: EF caches one model per `DbContext` type per service provider; derived runtime values are fine, but per-tenant *model* differences must use `IModelCacheKeyFactory`.

### What your code does today
- `AppDbContext.cs` — **1,751 lines**, one `OnModelCreating` with hundreds of entity configurations. `DbSet`s for `Form_DS1…DS9`, lookups, stamping models.
- `AppDbContext.cs:538/:543` — `ValueConverter<Guid, string>` / `ValueConverter<Guid?, string>` → **`GUID` stored as `CHAR(36)`** (an Oracle-idiom conversion you must explain).
- `AppDbContext.cs:783` — `.HasDefaultValueSql("STSDS_EDS.FRM_REFUND_SEQ.NEXTVAL")` (Oracle sequence) and `:1612/:1617` `.HasDefaultValueSql("SYSDATE")`.
- `AppDbContext.cs:40` — ctor takes `IConfiguration` and derives the active schema (`STSDS_EDS` default).
- `HasPrecision(…, 2)` calls (e.g. `…VehcPurchase.HasPrecision(13, 2)`) — monetary precision mapping.

### Hands-on lab 5 — split the model without breaking it (≤ 2 h)
1. List every `DbSet` and tag it to a **module** (`Stamping`, `BulkProcess`, `ImportData`, `Audit`, `Lookup`).
2. Extract **one** non-overlapping cluster (e.g. `Audit`) into an `IEntityTypeConfiguration<T>` class set under `Modules/Audit/Persistence/`.
3. Replace the inline `modelBuilder.Entity<AuditX>(…)` blocks with `modelBuilder.ApplyConfigurationsFromAssembly(…)`.
4. Re-run and confirm `dotnet ef migrations add` produces **no diff** (proves you didn't change the schema).
5. **Deliverable:** PR + ADR *"0008 — Configuration-per-entity; context decomposition strategy."* This is the *first physical step* toward a modular monolith.

**Resource:** Microsoft — *EF Core modeling* + `IEntityTypeConfiguration`.

---

## 6. EF Core II — query performance (your biggest, most provable wins)

**Concepts**
- **Read-only queries → `AsNoTracking()`** (no identity map, no snapshot). Or set the default: `UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)` and opt *in* with `AsTracking()` where you write.
- **Projection beats materialisation:** `Select(x => new Dto { … })` fetches only needed columns.
- **Cartesian explosion:** multiple `Include`s produce a JOIN cross-product → duplicated rows. Fix with `AsSplitQuery()`.
- **N+1:** a query inside a `foreach`. Fix with a single `Where(x => ids.Contains(x.Id))`.
- **Pagination:** always `Skip/Take` with a stable `OrderBy`; return `IQueryable` from repos **only** if the caller composes — otherwise materialise at the boundary.

### What your code does today
- `AsNoTracking` **863** ✅, but `Include(` **1,293** and `ThenInclude` **352** with **no** `AsSplitQuery` anywhere → high cartesian risk on the multi-form queries (e.g. `StampingSubmissionController.cs:2365` `.Include(x => x.REAL_PROPERTY_DS9s)`).
- `AuthenticationServices.cs:52`, `StampingService.cs:4584` — `AsNoTracking()` used selectively.
- `ToList()` **1,150** vs `ToListAsync()` **698** → a chunk of reads are still sync.
- `Controllers/StampingController/StampingSubmissionController.cs:190` — `BeginTransactionAsync()` in a controller (query+write logic in the controller).

### Hands-on lab 6 — find and fix one real N+1 / cartesian (≤ 2 h, highest ROI)
1. Turn on SQL logging for a Dev run: in the `UseLoggerFactory` filter (`Program.cs:145`) raise `Microsoft.EntityFrameworkCore.Database.Command` to `Information`.
2. Hit one heavy screen (a Form DS submission list) and **capture the SQL**.
3. Look for (a) the same query repeated N times (N+1) or (b) a JOIN with duplicated entity rows (cartesian).
4. Fix: N+1 → batch with `Contains`; cartesian → `AsSplitQuery()` or a `Select` projection into a DTO.
5. **Deliverable:** PR with the **before/after** `EXPLAIN PLAN` / row counts and a note: *"screen X: 847 queries → 3; 12,400 rows → 220."*

**Resource:** Microsoft — *Efficient querying* + *Single vs split queries*.

---

## 7. EF Core III — writes, transactions & change-tracker hygiene

**Concepts**
- `SaveChangesAsync` groups all pending changes in **one transaction** by default. Explicit transactions are only needed for **multiple** `SaveChanges` calls or raw SQL + EF in one unit.
- **Manual transactions belong in an application service / unit of work — not in a controller.** 34 `BeginTransaction` sites (e.g. `StampingSubmissionController.cs:190`) is a boundary smell.
- **Bulk operations:** `ExecuteUpdateAsync` / `ExecuteDeleteAsync` (EF7+) run a single `UPDATE`/`DELETE` *without* loading entities. Your count here is **0** → likely loading thousands of rows to update a column.
- **Change-tracker hygiene:** long-lived context across a batch accumulates tracked entities → memory + slow `SaveChanges` (it snapshots every tracked entity). Clear between batches.

### What your code does today
- `SaveChangesAsync` **594** vs `SaveChanges()` **37** (37 sync writes on request threads).
- `ExecuteUpdate`/`ExecuteDelete` — **not used** → batch scripts almost certainly load-then-save.
- `StampingService.cs:5884, :5899` — `ExecuteSqlRawAsync` (hand-written SQL for bulk/sequence work).
- `ObjectionAppealController.cs:9064` — `ExecuteSqlInterpolatedAsync` (safe, parameterised).

### Hands-on lab 7 — batch update without loading (≤ 2 h)
1. Find a loop that loads rows then mutates a status field and calls `SaveChanges`.
2. Replace with a single `await _context.Forms.Where(f => ids.Contains(f.Id)).ExecuteUpdateAsync(s => s.SetProperty(f => f.Status, "X"))`.
3. If you can't (Oracle provider/version limits), fall back to `ExecuteSqlInterpolatedAsync` with parameters.
4. **Deliverable:** PR with timings for 5,000 rows (**e.g. 42 s → 0.3 s**) — a number a non-technical stakeholder understands.

**Resource:** Microsoft — *ExecuteUpdate and ExecuteDelete* + *Transactions*.

---

## 8. Raw SQL & the Dapper duality

**Concepts**
- EF `FromSqlRaw`/`ExecuteSqlRaw` take a **format string** → concatenation = SQL injection. Prefer `FromSql`/`ExecuteSqlInterpolated` (parameterised by the interpolated handler).
- **Two ORMs is a legitimate, senior choice** *if* the boundary is explicit: EF for the write/model side, Dapper for hot read paths / set-based reporting. It's an anti-pattern when it's ad-hoc and undocumented.
- Dapper here uses a **global `TypeMapProvider`** mapping `[Column]` → Oracle `UPPER_SNAKE_CASE` (with a cache) — a clean, reusable trick, but a global mutable static that every query depends on.

### What your code does today
- `FromSqlRaw` **9**, `ExecuteSqlRaw` **6**, `ExecuteSqlInterpolated` **21**. Audit the 15 raw ones for string concatenation.
- `OpenApiService.cs:685, :2748` — `FromSqlRaw` (check parameterisation).
- `Program.cs` — Dapper `TypeMapProvider` + custom type handlers, registered once at startup.

### Hands-on lab 8 — injection sweep (≤ 2 h)
1. `search_files` for `FromSqlRaw`, `ExecuteSqlRaw`, `ExecuteSqlInterpolated`.
2. For each: is the SQL string built with `+ $"{x}"` / `string.Format`? If yes → **critical**: convert to `ExecuteSqlInterpolated` or `FromSqlRaw(sql, new OracleParameter(...))`.
3. Write a one-page ADR: *"When to use Dapper vs EF Core"* with the rule: writes/model = EF; set-based reports & tight read loops = Dapper; Dapper queries must live in `Modules/*/Read/`.
4. **Deliverable:** ADR + at least one injection-risk fix, or a written statement that all 15 are parameters-only (with evidence).

**Resource:** Microsoft — *Raw SQL queries* + OWASP *SQL Injection Prevention Cheat Sheet*.

---

## 9. Minimal APIs, gRPC, SignalR & background work

**Concepts**
- **Minimal APIs** (`app.MapGet(...)`) reduce ceremony for small/internal endpoints; MVC stays for Razor + filters + model binding. Choosing is a *lead* decision — document it.
- **gRPC** for internal service-to-service, typed, contract-first (`.proto`), HTTP/2. Use for the microservice split, not for the browser.
- **SignalR** for server→client push (progress, notifications). `IHubContext<T>` to push from services/jobs.
- **Background work:** prefer the built-in `BackgroundService` (`IHostedService`) for in-process; Hangfire for durable, retryable, dashboard-visible jobs — but **always resolve scoped services via `IServiceScopeFactory`**.

### What your code does today
- `Program.cs:1262` `app.MapHub<NotificationHub>("/hubs/notification")`; `Hubs/` present.
- `Modules/ImportData/Services/ImportNotifier.cs` — an import-progress notifier (SignalR bridge) — a real example of pushing to clients from a long-running import.
- `Program.cs:1007` `app.UseHangfireDashboard("/hangfire")` — durable background jobs already in play.

### Hands-on lab 9 — safe DB access from a Hangfire job (≤ 2 h)
1. Find a Hangfire job that touches the database.
2. Ensure it does **not** use an injected `AppDbContext`; instead inject `IServiceScopeFactory` (or `IDbContextFactory<AppDbContext>`) and create a scope per job.
3. Push progress to `NotificationHub` via `IHubContext<NotificationHub>` (pattern already in `ImportNotifier`).
4. **Deliverable:** PR + a short doc *"Background job DB access rules"* — this is the exact thing a lead writes so juniors don't recreate the bug.

**Resource:** Microsoft — *Background tasks with hosted services* + Hangfire docs (scope usage).

---

## 10. Clean / onion / hexagonal — refactoring *this* monolith

**Concepts**
- Layers: **Domain** (entities, invariants) → **Application** (use-cases, interfaces) → **Infrastructure** (EF, HTTP, Oracle) → **Presentation** (controllers/hubs). Dependencies point **inward**.
- The pragmatic middle step for a working monolith is **modular monolith**: strong module boundaries (already hinted by `Modules/`) before physical services.
- Interfaces live in the **inner** layer; EF/Dapper implementations in **outer**.

### What your code does today
- `Modules/` (`BulkProcess`, `ImportData`, `BulkUpload`, …) already groups some functionality — a **partial modular monolith**. But `Program.cs` (DI) and `AppDbContext` (persistence) are still global hubs that every module depends on → the seams leak.
- `Interface/` vs `Services/` split exists but repositories/services are registered globally in `Program.cs`.
- Example of the tangle: `ApplicationRepositoryServices` (repo) + `AuditLogService` (2,534 lines) + `StampingService` (4,500+ lines) all talk to the same giant `AppDbContext`.

### Hands-on lab 10 — define one module boundary (≤ 2 h)
1. Pick the **smallest** module (e.g. `ImportData`).
2. Produce a **module contract**: the public `IX` interfaces other modules may use, and a rule that no other module may reference its `Persistence/` or entities directly.
3. Write an ADR *"0009 — Module boundaries & the ImportData contract"* with a dependency diagram (Mermaid).
4. **Deliverable:** ADR + a `Modules/ImportData/README.md` stating the contract. No code move yet — the *decision* is the deliverable.

**Resource:** *Modular Monoliths* (Kamil Grzybek), and Microsoft's *Architecting modern web apps with ASP.NET Core*.

---

## 11. How to re-measure the codebase (lead habit: make claims evidence-based)

```powershell
# Run from repo root; ignores bin/obj.
$f = Get-ChildItem -Path LHDN_STSDS -Recurse -Include *.cs |
     Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
'{0} files' -f $f.Count
foreach ($p in @('\.Result(?![a-zA-Z])','\.Wait\(\)','async void','Task\.Run',
                'AsNoTracking','SaveChangesAsync','SaveChanges\(\)','FromSqlRaw',
                'ExecuteSqlRaw','ExecuteSqlInterpolated','BeginTransaction',
                'Include\(','ThenInclude','ToList\(\)','ToListAsync\(')) {
  $c = ($f | Select-String -Pattern $p -AllMatches |
        ForEach-Object { $_.Matches.Count } | Measure-Object -Sum).Sum
  '{0,-24} {1}' -f $p, $c
}
```

Paste this into a "before" doc; re-run after 12 weeks; put both in your promo/portfolio evidence.

---

## 12. Anti-pattern catalog (grounded, with your real counts)

| # | Anti-pattern | Evidence here | Fix | Severity |
|---|---|---|---|---|
| 1 | Sync-over-async | `.Result` ×21, `.Wait()` ×2 | `await` + `CancellationToken` | 🔴 |
| 2 | Unpooled, global `DbContext` | `Program.cs:137` | `AddDbContextPool` + scope validation | 🟠 |
| 3 | 1,751-line context / 1,303-line `Program.cs` | both files | `IEntityTypeConfiguration`, module DI extensions | 🟠 |
| 4 | Cartesian multi-`Include` | `Include` ×1,293, `ThenInclude` ×352, 0 `AsSplitQuery` | `AsSplitQuery` / projections | 🟠 |
| 5 | Load-then-save batch | 0 `ExecuteUpdate`/`ExecuteDelete` | `ExecuteUpdateAsync` | 🟠 |
| 6 | Manual tx in controllers | `BeginTransaction` ×34 (e.g. `…SubmissionController.cs:190`) | move to app service / UoW | 🟠 |
| 7 | Raw SQL audit gap | `FromSqlRaw` ×9, `ExecuteSqlRaw` ×6 | parameterise / `ExecuteSqlInterpolated` | 🔴 (if concatenated) |
| 8 | Sync materialisation | `ToList()` ×1,150 | `ToListAsync` + `AsNoTracking` | 🟡 |
| 9 | `catch (Exception)` ×883 | broad catch | catch specific; log with `ILogger`; no swallow | 🟠 |
| 10 | `Console.WriteLine` ×145 | logging | `ILogger<T>` structured logs | 🟡 |
| 11 | `new HttpClient(` ×35 | socket/DNS issues | `IHttpClientFactory` / typed clients | 🟠 |
| 12 | Nullable warnings off | `csproj:10` | enable file-by-file `#nullable enable` on new code | 🟡 |
| 13 | Version skew | EF Tools 10.x vs Oracle provider 9.x | pin/align, add to CI check | 🟡 |

**Lead framing:** you *present* the top 3 (🔴/highest-count 🟠) to the team as a rolling "quality backlog," each with a before/after number — never as a blame list.

---

## 13. Interview drills — modelled on *your* system (with model answers)

**T1. "You have 21 `.Result` calls and 2 `.Wait()` in a high-throughput API. Why is that dangerous, and how do you prioritise the fix?"**
> Sync-over-async blocks a thread-pool thread on an operation that needs *another* thread to complete. Under burst load the pool saturates and the whole API stalls with cascading timeouts. I'd classify all 21 by whether the awaited op is genuinely async I/O on a request path; fix those first, thread `CancellationToken` from `RequestAborted`, and add a Roslyn analyzer/CI grep so `.Result` can't come back.

**T2. "Why `AddDbContextPool` and when is it wrong?"**
> It reuses context instances to cut allocation/GC on hot paths. It's wrong if the context stores per-request state in instance fields, or if you mutate `DbContextOptions` per request. Here the ctor only reads constant config (schema name), so pooling is safe — but I'd confirm with `ValidateScopes`/`ValidateOnBuild` on first.

**T3. "1,293 `Include`s, 352 `ThenInclude`s, no `AsSplitQuery`. What's the risk and the fix?"**
> Multiple collection includes generate one JOIN per include → a cartesian cross-product → duplicated rows over the wire and huge result sets. Fix: `AsSplitQuery()` for read-heavy graphs, or project into a DTO with `Select` to fetch only needed columns; measure rows returned before/after.

**T4. "Hangfire job needs the DB. Why can't you just inject `AppDbContext`?"**
> Background jobs run outside a request scope; injecting a scoped `DbContext` into a long-lived job creates a captive dependency → disposed-context and concurrency errors, and one context across many jobs accumulates change-tracker memory. Correct: inject `IServiceScopeFactory` (or `IDbContextFactory`) and create a scope/context per job execution.

**T5. "We store `Guid` as `CHAR(36)` via a value converter. Trade-offs?"**
> It matches the Oracle schema and keeps queries readable, but stores 36 bytes instead of 16 and loses native GUID indexing — on large tables that hurts index size and range scans. If we control the schema, `RAW(16)` is denser; if not, keep the converter but ensure the column is indexed and queries compare in the same string form.

**T6. "Explain the difference between `SaveChangesAsync` and an explicit transaction in this codebase."**
> `SaveChangesAsync` already wraps all pending changes in one DB transaction — an explicit transaction is only needed when I have *multiple* `SaveChanges` calls or raw SQL plus EF in one atomic unit. The 34 `BeginTransaction` sites in controllers mostly indicate scope/logic-in-the-wrong-layer; I'd move them into application services.

**T7 (leadership). "A junior says EF is slow. How do you respond?"**
> I'd ask for the query and the plan, not the opinion. Then walk them through logging the generated SQL, checking tracking vs no-tracking, N+1, cartesian includes, and projection — turning "EF is slow" into a specific, measurable fix. That teaches the method, not just the answer.

**T8 (leadership). "How do you introduce these refactors without destabilising a regulated system?"**
> Small, reversible PRs behind the existing test surface; one pattern at a time (no-tracking default, then pooling, then split queries); each with a before/after metric; a written ADR per decision; and a CI guard (grep/analyzer) so regressions can't re-enter.

---

## 14. Where this fits the 26-week roadmap

| Week | Focus | This doc's labs |
|---|---|---|
| 1–2 | C#/async fundamentals | Lab 2 (sync-over-async) |
| 3–4 | DI & middleware | Lab 3, Lab 4 |
| 5–7 | **EF Core deep** | Lab 5, Lab 6, Lab 7 |
| 8 | Raw SQL + Dapper + security | Lab 8 |
| 9 | Background jobs + SignalR | Lab 9 |
| 10–11 | Clean/modular architecture | Lab 10 |
| 12 | Consolidation + interview drills | §13 (T1–T8) |

Every lab ends with a **PR + an ADR**, so by week 12 you have a portfolio of *real* changes to a regulated production system — the single most convincing thing in a technical-lead interview.

---

## 15. Resources (one per topic — high signal only)

- **Async:** Stephen Cleary, *Async and Await* — https://blog.stephencleary.com/2012/02/async-and-await.html
- **DI/lifetimes:** Microsoft, *DI in .NET: service lifetimes* — https://learn.microsoft.com/dotnet/core/extensions/dependency-injection
- **Middleware:** Microsoft, *ASP.NET Core middleware* — https://learn.microsoft.com/aspnet/core/fundamentals/middleware/
- **EF modeling:** Microsoft, *Creating and configuring a model* — https://learn.microsoft.com/ef/core/modeling/
- **EF querying perf:** Microsoft, *Efficient querying* — https://learn.microsoft.com/ef/core/performance/efficient-querying
- **EF bulk ops:** Microsoft, *ExecuteUpdate & ExecuteDelete* — https://learn.microsoft.com/ef/core/saving/execute-insert-update-delete
- **EF transactions:** Microsoft, *Transactions* — https://learn.microsoft.com/ef/core/saving/transactions
- **Raw SQL:** Microsoft, *SQL queries* — https://learn.microsoft.com/ef/core/querying/sql-queries
- **Splitting a monolith:** Microsoft, *Architecting modern web apps with ASP.NET Core* — https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/
- **Injection:** OWASP, *SQL Injection Prevention* — https://cheatsheetseries.owasp.org/cheatsheets/SQL_Injection_Prevention_Cheat_Sheet.html

---

### Provenance
Counts and line references measured on `LHDN_STSDS` @ `56d70b61e` (branch `eDScompoundCalculation`).
Re-run §11 to refresh. This doc intentionally cites **your** files so that every answer you give in an interview is one you have actually seen in production code.
