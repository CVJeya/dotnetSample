# Chapter 5 — ASP.NET Core for a Senior Role

> Files: `backend/AngularDotnetDemo.Api/Program.cs` (.NET 10), `DeliverFoodApp/src/FoodDelivery.Api/**` (.NET 8), `docker-compose.yml`, `azure-pipelines.yml`
>
> **Your goal in this chapter is not to learn ASP.NET Core — it's to learn how to *talk about* it like a senior.** Every section has a "say it like this" box.

---

## 1. Hosting model & the request pipeline (12)

### `backend/AngularDotnetDemo.Api/Program.cs` in outline

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddPolicy("dev", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters { /* ... */ });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<AppData>();     // in-memory demo store

var app = builder.Build();

app.UseCors("dev");
app.UseAuthentication();      // WHO are you
app.UseAuthorization();       // are you ALLOWED
app.MapControllers();
app.Run();
```

### The three things an interviewer listens for

1. **`Builder` phase vs `Pipeline` phase.** Everything before `builder.Build()` is *configuration* (register services, options, auth); everything after is *behaviour* (middleware, endpoints). **You cannot resolve scoped services at builder time.** Mixing them up is the classic junior bug.
2. **Middleware order is semantic, not cosmetic.** `UseAuthentication()` must precede `UseAuthorization()`; both must precede `MapControllers()`. `UseCors` must come before auth for preflight to work; exception handling must be first to catch everything.
3. **There is no `Startup` class by default** (since .NET 6). Top-level statements + minimal hosting. `Startup.ConfigureServices` → `builder.Services`, `Startup.Configure` → the middleware block.

### The correct pipeline order (memorise this list)

```
ExceptionHandler / DeveloperExceptionPage
HSTS / HttpsRedirection
StaticFiles   (before routing if you want short-circuit)
Routing
CORS
Authentication
Authorization
RateLimiter
Endpoints (MapControllers / MapHub / MapGrpcService / MapHealthChecks)
```

### Say it like this

> "The pipeline is a chain of `RequestDelegate`s — essentially an ordered list of middleware where each can short-circuit or pass `next()`. It's the same mental model as OWIN/IHttpModule from my MVC days, minus the config ceremony. Order encodes meaning: exception handling wraps everything, CORS has to be before authentication or preflight fails, and authentication must precede authorization because authorization reads `HttpContext.User`."

### Minimal APIs vs controllers

| | Minimal API | Controllers |
|---|---|---|
| Best for | Small services, few endpoints, high throughput (fewer allocations) | Rich APIs: filters, model binding, conventions, validation attributes, API versioning |
| This repo | `DeliverFoodApp` mixes both (controllers + `MapGet` for health/gRPC) | `AngularDotnetDemo.Api` uses controllers |
| Trade-off to name | "Minimal APIs are leaner and test-friendly but you re-implement cross-cutting concerns by hand; controllers give you filters and conventions out of the box." | |

---

## 2. Dependency Injection lifetimes (13) — the #1 senior .NET interview area

| Lifetime | Instance | When |
|---|---|---|
| **Singleton** | One for app lifetime | Stateless services, caches, `HttpClientFactory`-produced clients, options |
| **Scoped** | One per HTTP request (per scope) | `DbContext`, repositories, `IHttpContextAccessor` consumers, per-request caches |
| **Transient** | New every resolution | Lightweight stateless helpers; **never** anything you inject into a singleton |

### The three bugs that follow from getting it wrong

1. **Captive dependency** — a singleton injecting a scoped `DbContext`. The context lives forever → stale tracked entities, unbounded memory, and concurrency exceptions. .NET's scope validation catches it in Development; in Production it silently misbehaves. **This is the question. Learn it cold.**
2. **State in a singleton** — a singleton holding per-user state leaks data between users. Test for it explicitly.
3. **Transient in a singleton** — a transient injected once into a singleton becomes a de-facto singleton (the capture is permanent). Same trap, subtler.

### The `DbContext` scoping rule

`AddDbContext<T>()` registers **Scoped** by default. That's correct for web requests. Problems arrive via:
- **Background services** (`IHostedService`) — they're **singletons**. You must create a scope per unit of work:
  ```csharp
  using var scope = _scopeFactory.CreateScope();
  var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  ```
  `FoodDelivery.Consumers` is exactly this shape: a hosted worker consuming messages in its own scope. Mention it.
- **Parallel work with `Task.WhenAll`** — a single `DbContext` is **not thread-safe**. One context per parallel branch.
- **Blazor Server** — scoped = circuit lifetime, which is much longer than a request. Use `IDbContextFactory<T>` there. Knowing this is a genuine senior signal.

`AddDbContextPool` pools context instances to reduce allocation — but you lose per-context state, so only for simple, stateless usage.

### Also know

- **`IServiceProvider` vs `IServiceScopeFactory`**: in a singleton, inject the *factory* and create scopes; never resolve scoped services from the root provider.
- **Keyed services** (.NET 8): `AddKeyedScoped<IPayment, StripePayment>("stripe")` + `[FromKeyedServices("stripe")]` — that's the modern replacement for the "inject `IEnumerable<IPayment>` and pick one" pattern.
- **`IOptions` vs `IOptionsSnapshot` vs `IOptionsMonitor`**:

| Interface | Lifetime | Reloads on config change | Use in |
|---|---|---|---|
| `IOptions<T>` | Singleton | No | Startup-time values |
| `IOptionsSnapshot<T>` | **Scoped** | Yes, per request | Values that may change per request |
| `IOptionsMonitor<T>` | Singleton | Yes, with `OnChange` callback | Background services, live settings |

The classic gotcha: injecting `IOptionsSnapshot` into a singleton throws (it's scoped — a captive dependency again). Everything ties back to §2.

### Say it like this

> "I think of DI lifetimes as ownership of state. Singleton owns process state; scoped owns request state; transient should own nothing. Almost every DI bug I've debugged has been a lifetime mismatch: a `DbContext` captured by a singleton, or request data kept in a field of a singleton service. In production I'd keep `ValidateScopes` on outside of Development too, and code-review every singleton's constructor looking for scoped dependencies."

---

## 3. Configuration, options and secrets (13)

Precedence: `appsettings.json` → `appsettings.{Environment}.json` → user secrets (dev) → **environment variables** → command line. Last wins.

- **Never** commit secrets. `dotnet user-secrets` locally; Azure Key Vault / a managed identity in Azure; env vars in Docker (see `docker-compose.yml`).
- `Environment` comes from `ASPNETCORE_ENVIRONMENT`; `IsDevelopment()` enables the developer exception page and Swagger.
- The **options pattern** binds a POCO to a section (`builder.Services.Configure<JwtOptions>(cfg.GetSection("Jwt"))`) so configuration isn't stringly-typed deep inside services. This is the exact analogy for Angular's `InjectionToken` — say it.
- **Validate config at startup** (`ValidateOnStart` + data annotations) so a missing connection string fails the deploy, not the first request. That's a real senior habit: fail fast at boot.

---

## 4. Middleware vs filters vs interceptors — the cross-cutting map

| Mechanism | Scope | Can short-circuit | Typical use |
|---|---|---|---|
| **Middleware** | Whole app, before routing | Yes | Exception handling, auth, request logging, CORS, correlation IDs |
| **Endpoint filter** (minimal APIs) | One endpoint/group | Yes | Validation, tenant resolution |
| **Action filter** | MVC action | Yes | Model enrichment, audit |
| **`IAsyncActionFilter` / resource filter** | Around action execution | Yes | Caching, transactions |
| **`DelegatingHandler`** | Outgoing `HttpClient` | Yes | Retry, auth headers for downstream calls |
| **Angular interceptor** | Front-end HTTP | Yes | Token attach, error toast |

**This table is a gift in a full-stack interview** — you can say: "the same conceptual layer exists on both sides: middleware in the ASP.NET pipeline, a `DelegatingHandler` for my server-to-server calls, and an Angular interceptor in the browser. All three are 'wrap the request, add cross-cutting behaviour, delegate or short-circuit'."

---

## 5. EF Core at senior depth (15)

### `DeliverFoodApp/src/FoodDelivery.Api/Data/AppDbContext.cs`

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Order>().Property(o => o.Total).HasPrecision(18, 2);
        b.Entity<Order>().HasMany(o => o.Items).WithOne().OnDelete(DeleteBehavior.Cascade);
        b.Entity<MenuItem>().HasIndex(m => m.Name);
    }
}
```

### The five questions you will be asked

**1. `IQueryable` vs `IEnumerable` — the single most important EF distinction.**

`IQueryable` composes an expression tree that is translated to SQL; `IEnumerable` forces client evaluation. So:

```csharp
// GOOD: WHERE + TOP in SQL
var page = await db.Orders.AsNoTracking()
    .Where(o => o.Status == OrderStatus.Pending)
    .OrderByDescending(o => o.CreatedAt)
    .Skip((page - 1) * size).Take(size)
    .Select(o => new OrderSummaryDto(o.Id, o.Total, o.CreatedAt))
    .ToListAsync(ct);

// BAD: full table into memory, then filtered
var all = await db.Orders.ToListAsync();
var pending = all.Where(o => o.Status == OrderStatus.Pending);
```

**Say:** "Projecting with `Select` is both a performance and a shape decision — I fetch only the columns the UI needs, and I avoid leaking navigation properties into JSON (which is how you get cyclic serialisation and N+1)."

**2. N+1 queries.** Lazy loading is **off by default**. If you turn it on, a loop over 100 orders that touches `o.Items` issues 101 queries. Fixes: `Include`/`ThenInclude`, or (better) project with `Select` — and watch for **cartesian explosion** when `Include`-ing multiple collections (use `AsSplitQuery()`).

**3. Tracking vs `AsNoTracking`.** Tracked entities cost memory and change-detection time. **Read-only queries should be `AsNoTracking()`** — often a 2–3× throughput improvement on list endpoints. But `Update` needs tracking, so don't blanket-apply it in repositories that also write.

**4. Migrations.** `dotnet ef migrations add X` → script reviewed in PR → `dotnet ef database update` **or** a generated idempotent SQL script applied by the deployment pipeline. **Senior opinion:** "I don't run `database update` from app startup in production — concurrent instances race, and a bad migration takes down the app with no rollback story. I generate idempotent SQL scripts in CI, review them, apply them as a pipeline step, and keep them backwards-compatible for one release so a rolling deploy never sees a mismatched schema."

**5. Transactions & concurrency.**

```csharp
await using var tx = await db.Database.BeginTransactionAsync(ct);
try { /* ... */ await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); }
catch { await tx.RollbackAsync(ct); throw; }
```

`SaveChanges` is already atomic for a single call. You need an explicit transaction for **multiple** `SaveChanges` or raw SQL. Note: `SaveChanges` is *not* a distributed transaction — you cannot include an HTTP call or a message publish (see §9 outbox pattern).

**Optimistic concurrency:** a `RowVersion`/`xmin` token → `DbUpdateConcurrencyException` when two users edit the same row. Show you know to resolve it (server-wins, client-wins, or merge) rather than just throwing 500.

### The EF Core performance checklist (recite this)

> The SQL-side depth behind this checklist — plans, statistics, parameter sniffing, isolation levels, migration
> process, security — is **`05D-SQL-AND-DATA-ACCESS-FOR-DOTNET.md`** (98 Q&A).

1. Project with `Select` to a DTO.
2. `AsNoTracking()` for reads.
3. Page on the server (`Skip`/`Take`), never `ToList()` then slice.
4. Avoid N+1 — prefer projection over `Include`; `AsSplitQuery()` for multiple collections.
5. Index the columns in `WHERE`/`ORDER BY`/`JOIN`; check the execution plan.
6. Use `ExecuteUpdateAsync`/`ExecuteDeleteAsync` for bulk operations (EF 7+) — no entity materialisation at all.
7. `AddDbContextPool` for high-throughput read services.
8. Cache genuinely static data in memory/distributed cache instead of re-querying.
9. Compile hot queries (`EF.CompileAsyncQuery`) for very high QPS.
10. Log SQL in dev (`EnableSensitiveDataLogging` only in dev — it leaks PII) and watch `Microsoft.EntityFrameworkCore.Database.Command` in production for slow queries.

---

## 6. `async`/`await` — how you demonstrate 5 years, not 1

You have the advantage here; use it.

| Rule | Why |
|---|---|
| `async` all the way down | `.Result`/`.Wait()` block a thread pool thread → thread starvation under load, and can deadlock with a synchronisation context |
| `CancellationToken` on every action | When the browser disconnects, the work should stop. Pass it into EF (`ToListAsync(ct)`) and `HttpClient` |
| `ConfigureAwait(false)` in libraries | Not needed in ASP.NET Core (no sync context), **is** needed in shared libraries |
| Never `async void` (except event handlers) | Exceptions become unobservable crashes |
| `Task.WhenAll` for parallelism | But **one `DbContext` per branch** |
| Don't parallelise DB calls on one connection | The connection is not thread-safe; it serialises anyway |

**Say it like this:**
> "Async in ASP.NET Core is about **scalability, not speed**. It frees the thread while I/O is in flight, so 1,000 concurrent slow queries don't need 1,000 threads. It doesn't make any single request faster. And I treat `CancellationToken` as part of the contract — a request that keeps querying after the client left is wasted database time."

That distinction — *scalability, not speed* — is the line that marks you as senior.

---

## 7. Authentication & authorization (14)

`backend/AngularDotnetDemo.Api/Program.cs` uses **JWT bearer**:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true,
            ValidateLifetime = true, ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
```

### What makes a "senior" answer

- **Know all five validations** and that `ValidateLifetime = true` is what makes expiry real. `ClockSkew` defaults to **5 minutes** — so an "expired" token still works for 5 more minutes unless you set `ClockSkew = TimeSpan.Zero`. Mentioning that is a *free credibility point*.
- **Signing**: HMAC (`SymmetricSecurityKey`) means every validating service shares the secret — fine for one API, bad at scale. Asymmetric (RSA/ECDSA) lets services validate with the public key only. Say you'd choose asymmetric for multi-service.
- **Where to store**: `Authorization: Bearer` header. Cookies vs header is the CSRF/XSS trade-off (`04` §7).
- **Lifetimes**: short-lived access token (5–15 min) + refresh token (rotate on use, revoke on logout). Refresh rotation with reuse detection is the industry pattern; naming it shows depth.
- **Authorisation**: `[Authorize]`, `[Authorize(Roles="Admin")]`, and **policy-based** auth (`RequireClaim`, `RequireAssertion`, custom `IAuthorizationHandler`) for real rules like "can edit own orders". Resource-based handlers (`IAuthorizationService.AuthorizeAsync(user, order, "OwnOrder")`) are the senior-level answer for row-level rules.
- **`401` vs `403`**: 401 = not authenticated / bad credentials; 403 = authenticated but not allowed. Getting this right is a small but revealing detail.
- **Never** build your own crypto, never store passwords with SHA-256 — ASP.NET Core `Identity`'s `PasswordHasher` uses PBKDF2 with per-password salt and iteration count. Say it.

**Say it like this:**
> "In the demo API the token is issued by a `/auth/login` endpoint, signed HMAC-SHA256, validated by the JwtBearer handler with issuer, audience and lifetime checks, and the browser attaches it via an Angular interceptor. For a real system I'd move to short-lived access tokens with rotating refresh tokens, asymmetric signing so services validate with a public key, and policy-plus-resource handlers so authorisation rules live in one place instead of being sprinkled as attributes."

---

## 8. API design that senior interviewers respect (13)

| Topic | Position |
|---|---|
| **REST vs RPC** | Resources + verbs for CRUD; a command endpoint (`POST /orders/{id}/cancel`) is legitimate. Don't be dogmatic. |
| **Status codes** | 200/201(+`Location`)/204, 400 (validation), 401/403, 404, 409 (conflict), 422 (semantic), 429 (rate limit), 500. |
| **Errors** | **RFC 7807 ProblemDetails** — one shape everywhere. `builder.Services.AddProblemDetails()`. |
| **Validation** | Data annotations / FluentValidation, returning `ValidationProblemDetails` the SPA can map to form fields. |
| **Versioning** | URL segment (`/v1/`) is the pragmatic default; header-based for strict REST. Decide *before* you have clients. |
| **Pagination** | `?page=&pageSize=` + envelope `{ items, totalCount }`, or cursor pagination for large/live data. Cap `pageSize` server-side — an unbounded `pageSize` is a DoS vector. |
| **Idempotency** | `PUT` is idempotent; `POST` is not. For payments/orders, accept an `Idempotency-Key` header and store the result. **This is a senior-level answer.** |
| **DTOs vs entities** | Never return EF entities: over-posting mass assignment, circular references, schema leakage. `backend`'s demo returns entities directly — **call that out as a smell you'd fix** and explain why. Honest self-critique reads as seniority. |
| **API docs** | OpenAPI/Swagger in non-prod; generate the client types from it so the SPA can't drift. |
| **CORS** | Locked to known origins in prod, `AllowCredentials` only when required and never with `AllowAnyOrigin`. |
| **Rate limiting** | .NET 7+ `AddRateLimiter` — fixed window / sliding / token bucket / concurrency. |
| **Health checks** | `AddHealthChecks().AddDbContextCheck<T>()` + `MapHealthChecks("/health")`; separate liveness from readiness for k8s. `DeliverFoodApp` maps health checks — reference it. |

**Say it like this:**
> "My rule is that the API contract is a product: stable error shape, versioned from day one, paginated by default, DTO-projected so the database schema can change without breaking the client, and documented with OpenAPI so the Angular service types are generated rather than hand-maintained. Almost every front-end/back-end argument I've seen came from an unstated contract."

---

## 9. Distributed systems — the senior differentiator (16)

`DeliverFoodApp` is your evidence here: `Program.cs` wires EF Core + **MassTransit/RabbitMQ** + **Kafka** + **SignalR** + health checks; `FoodDelivery.Shared/Events/DomainEvents.cs` holds integration events; `FoodDelivery.Consumers/Program.cs` is a worker service.

### 9.1 Why messaging instead of direct HTTP calls

| Synchronous HTTP | Asynchronous messaging |
|---|---|
| Caller waits; failure propagates | Decoupled; caller doesn't care |
| Retry needs care; cascading failures | Broker holds the message, retries policy |
| Latency adds up (chained calls) | Fire-and-forget, seconds are fine |
| Simple to reason about | Eventual consistency, ordering, duplicates |

**Say it like this:**
> "I use messaging when the caller doesn't need an answer to continue. Placing an order should not fail because the notification service is down. The trade-off is eventual consistency and duplicates, so I design consumers to be idempotent and I make state transitions explicit."

### 9.2 The patterns you must be able to name

| Pattern | Purpose |
|---|---|
| **Transactional outbox** | Save the order and the "OrderPlaced" event **in one DB transaction** into an outbox table; a publisher drains it. Kills the dual-write problem (DB committed, message lost). |
| **Idempotent consumer** | Store processed message ids; skip duplicates. At-least-once delivery is the default guarantee — you get **exactly-once only via idempotency**. |
| **Saga / process manager** | A multi-step business transaction across services with compensating actions (order → payment → stock; if stock fails, refund). Choreography (events) vs orchestration (a saga state machine) — name both and the trade-off (coupling vs visibility). |
| **Dead-letter queue** | Poison messages go where a human can inspect them instead of blocking the queue forever. |
| **Retry with backoff + circuit breaker** | `.NET 8` resilient `HttpClient` via `Microsoft.Extensions.Http.Resilience` / Polly: retry, timeout, circuit breaker, bulkhead. |
| **Saga vs 2PC** | Two-phase commit needs distributed transactions and locks; sagas trade atomicity for availability. Say the sentence: "I avoid distributed transactions; I accept eventual consistency and compensate." |

### 9.3 MassTransit specifics (as used here)

- `bus.Publish(new OrderPlaced(...))` = pub/sub (many subscribers). `bus.Send` = point-to-point.
- Consumers implement `IConsumer<TEvent>`; configure retry and error queues (`ConfigureEndpoints` + `UseMessageRetry`).
- Messages should be **contracts**, versioned, and never contain EF entities — `DomainEvents.cs` in the shared project is the right shape.
- Adding a message to a queue needs a migration story: additive changes only, tolerate missing fields.

### 9.4 Caching (asked in every senior interview)

| Layer | Tool | Note |
|---|---|---|
| In-process | `IMemoryCache` | Fast, **per instance** — inconsistent across pods |
| Distributed | Redis (`IDistributedCache`) | Shared, needs serialisation + TTL discipline |
| HTTP | `ResponseCache` / `ETag` / `Cache-Control` | Lets the browser/CDN do the work |
| Output | `AddOutputCache` (.NET 7+) | Cache the whole response |

Rules: cache the **expensive and stable**, always with TTL; use cache-aside, not write-through, unless you can afford it; **invalidations are the hard part** — prefer short TTL over clever invalidation. Never cache per-user data in a shared key (that's a data leak, not a bug).

---

## 10. Real-time & RPC — `OrderHub` and gRPC (17)

### SignalR (`DeliverFoodApp/src/FoodDelivery.Api/Hubs/OrderHub.cs`)

```csharp
public class OrderHub : Hub
{
    public async Task SubscribeToOrder(int orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
        _logger.LogInformation("Connection {ConnectionId} subscribed to order {OrderId}",
            Context.ConnectionId, orderId);
    }

    public async Task UnsubscribeFromOrder(int orderId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
}
```

Talking points:
- SignalR picks the best transport: **WebSockets**, falling back to Server-Sent Events, then long-polling.
- **Groups** = server-side subscription lists; broadcast with `Clients.Group("order-5").SendAsync(...)`.
- **Scale-out** requires a backplane (Redis) so a message published on pod A reaches a client connected to pod B. Naming the backplane is a senior signal.
- It's **stateful per connection** — sticky sessions or a backplane, and the front end must handle reconnect.
- `ConnectionId` ≠ user id. Authenticated hubs give you `Context.User`. Map user → connections if you need to push to a user.
- Front-end side: `@microsoft/signalr` client, `withAutomaticReconnect()`, and treat the connection as an Observable/store so the UI can show "live" vs "reconnecting".

### gRPC

- HTTP/2, binary protobuf, strongly generated contracts, streaming (client/server/bidirectional).
- `.proto` in a shared project → server `MapGrpcService<T>()`, client typed stub.
- Great for internal service-to-service (low latency, small payloads). Poor for browsers without gRPC-Web and a proxy.
- **Answer the "REST or gRPC?" question:** "gRPC internally between services for performance and contract strength; REST/JSON at the edge for browser friendliness and debuggability. And the choice is about the consumer, not about which is 'better'."

### Kafka vs RabbitMQ (both appear in this repo)

| | RabbitMQ | Kafka |
|---|---|---|
| Model | Message broker (smart broker, dumb consumer) | Distributed log (dumb broker, smart consumer) |
| Consumption | Message deleted after ack | Retained; consumers track offsets |
| Replay | No | **Yes** — rewind and reprocess |
| Ordering | Per-queue | Per-partition |
| Best for | Task/work queues, per-message routing | Event streaming, event sourcing, analytics, replay |

---

## 11. Testing (senior level)

| Type | Tool | What it proves |
|---|---|---|
| Unit | xUnit/NUnit + FluentAssertions | Pure logic, validators, domain rules |
| Integration | **`WebApplicationFactory`** + `Testcontainers` (real SQL server in Docker) | Endpoint → DB → JSON, including auth and filters |
| Contract | Pact / OpenAPI diff | The API and the SPA still agree |
| E2E | Playwright | The journey works in a browser |

**The opinionated answer:**
> "I test the domain with unit tests, the API surface with `WebApplicationFactory` against a real database in a container — because an in-memory provider hides real SQL behaviour — and I keep a thin layer of browser E2E for critical journeys. What I refuse to do is mock `DbContext`: it tests the mock, not the query."

`.UseInMemoryDatabase()` trap: it doesn't enforce constraints, ignores relational semantics, and `Include`s behave differently. Mentioning why you *avoid* it is a senior signal.

---

## 12. Observability & production readiness (18)

- **Structured logging** with `ILogger` + message templates (`_logger.LogInformation("Order {OrderId} placed", id)`) — never string concatenation, because the fields must be queryable. Serilog → Seq/App Insights.
- **Correlation ID**: accept or generate `X-Correlation-Id` in middleware, push into the log scope, propagate to downstream calls and back to the client. This is how you debug "the user says it failed" without guessing. **Great story to tell: it's a real scar, not a theory.**
- **Metrics** (`System.Diagnostics.Metrics` / OpenTelemetry) — request rate, error rate, p95 latency, DB time. "The four golden signals."
- **Tracing** — OpenTelemetry spans across HTTP → EF → message broker, so one trace shows where the 3 seconds went.
- **Health checks** — `/health/live` (process up) vs `/health/ready` (DB reachable) — readiness gates traffic, liveness restarts the pod.
- **Graceful shutdown** — `IHostApplicationLifetime`; stop accepting new work, drain in-flight, close the broker connection. Kubernetes sends SIGTERM then SIGKILL; ignoring shutdown is how you lose in-flight orders.

### Docker / CI (`docker-compose.yml`, `azure-pipelines.yml`)

Be able to narrate the deployment honestly:

> "Multi-stage Dockerfiles: build with the SDK image, publish, then copy only the artefacts into the ASP.NET runtime image — smaller and no SDK in production. `docker-compose` for local parity (API + DB + RabbitMQ). CI builds, runs unit and integration tests, produces the image once, and the same image is promoted through environments with config injected as environment variables — never rebuilt per environment. That's the 'build once, deploy many' rule."

Front-end in the same story: the Angular app is built to static files, served by nginx, and nginx proxies `/api` to the API container — which is why `environment.apiUrl = '/api'` works and why CORS isn't needed in that topology. **Saying this makes you look like the person who owns the whole system, which is exactly what a senior role asks for.**

---

## 13. Architecture vocabulary you should be fluent in

| Term | One-line definition | When to invoke it |
|---|---|---|
| **Clean / Onion** | Dependencies point inward: Domain ← Application ← Infrastructure; the web layer is a detail | "Where does business logic live?" |
| **Vertical slice** | Organise by feature (Orders/GetOrderById: request, handler, response) instead of by layer | "How do you stop the service layer becoming a 4,000-line god class?" |
| **CQRS** | Separate read models from write models | "Reads and writes scale differently" |
| **MediatR pipeline** | Cross-cutting behaviour (validation, logging, transactions) as decorators around handlers | "How do you avoid repeating validation in every handler?" |
| **Repository + Unit of Work** | Abstract persistence | ⚠️ Have an opinion: "EF Core's `DbContext` *is* a Unit of Work and `DbSet` is a repository. Adding another generic repository usually loses `IQueryable` composition and hides EF's real capabilities. I'd use it for testability boundaries, not ceremony." |
| **Anti-corruption layer** | Map external contracts to your domain | "Integrating a third-party API" |
| **Strangler fig** | Migrate a monolith incrementally behind a facade | "How would you move MVC views to a SPA?" — **your story** |
| **Idempotency / at-least-once** | Duplicates happen; make handling safe | Messaging, payments, webhooks |
| **12-factor config** | Config in the environment, not the code | Deployment questions |

---

## 14. Chapter 5 — questions you must be able to answer

1. Difference between the builder phase and the pipeline phase; what can't you do before `Build()`? *(§1)*
2. Middleware ordering: why must `UseAuthentication` precede `UseAuthorization`, and `UseCors` come early? *(§1)*
3. Singleton vs Scoped vs Transient — one concrete example of each, and the bug that follows from a mismatch. *(§2)*
4. Why is a captive `DbContext` in a singleton bad, and how does .NET warn you? *(§2)*
5. How do you safely use a `DbContext` inside an `IHostedService` or a parallel `Task.WhenAll`? *(§2)*
6. `IOptions` vs `IOptionsSnapshot` vs `IOptionsMonitor`. *(§2)*
7. `IQueryable` vs `IEnumerable` — what happens to the SQL? *(§5)*
8. What is an N+1 problem and how do you detect and fix it? *(§5)*
9. `AsNoTracking` — when and when not? *(§5)*
10. How do you apply EF Core migrations in production, and why not on startup? *(§5)*
11. Is `async` faster? Explain what it actually buys. *(§6)*
12. Why `CancellationToken` on every action? *(§6)*
13. Walk the JWT validation parameters; what does `ClockSkew` do? *(§7)*
14. 401 vs 403. *(§7)*
15. Why never return EF entities from an action? *(§8)*
16. Design the error contract for an API. *(§8)*
17. When do you publish a message instead of making an HTTP call? *(§9.1)*
18. What is the transactional outbox and what problem does it solve? *(§9.2)*
19. How do you make a consumer idempotent? *(§9.2)*
20. `IMemoryCache` vs `IDistributedCache` — what breaks in a multi-pod deployment? *(§9.4)*
21. SignalR: groups, backplane, and what breaks when you scale to 2 pods. *(§10)*
22. RabbitMQ vs Kafka — pick one for order events and defend it. *(§10)*
23. How do you debug a production issue with no reproduction steps? *(§12 — correlation IDs)*
24. Liveness vs readiness. *(§12)*
25. What's the difference between Clean Architecture and vertical slices, and which do you prefer? *(§13)*

---

## 30-second version

`Program.cs` is two phases: register services (choosing lifetimes that own the right amount of state), then compose an ordered middleware pipeline whose order *is* the security model. EF Core is `IQueryable` composition — project, `AsNoTracking`, paginate, index, and never lazy-load by accident. `async` is a scalability tool, not a speed one, and `CancellationToken` is part of the contract. The API contract is a product: ProblemDetails, DTOs, versioning, pagination, idempotency. Beyond one process, correctness comes from idempotent consumers, the outbox pattern, sagas with compensation, and cache/backplane discipline — and you debug all of it with correlation IDs and traces, not guesswork.
