# World Bank — .NET Fullstack Angular Assessment
### One-hour written/online technical assessment · expected questions, model answers, and a minute-by-minute plan

**Who this is for:** you — 6 years full-stack .NET (MVC/Razor/C#/SQL/jQuery), last 12 months Angular + React in production, delivery ownership for a team of 3, sitting a **1-hour** screening assessment for a **Microsoft Fullstack (.NET + Angular) Technical Lead** role.

**How to read it:** §0–§3 first (30 min, tonight). Then §14's 30-question mock under exam conditions. Then use the numbered banks (§4–§12) as revision, not as reading.

---

## §0. What a one-hour assessment actually is

### 0.1 The honest disclaimer — read this before you memorise anything

**Nobody outside World Bank's talent acquisition team knows the actual paper.** What follows is a reconstruction from three things you *can* reason about:

1. **The format constraints.** A 60-minute proctored slot is a *screen*, not an interview. It must be auto-gradable or gradable in under 10 minutes, so it is built from MCQs, "predict the output", short SQL/TS/C# snippets, and at most one or two small coding tasks. There is no room for essay answers or system design.
2. **The published stack.** The role is .NET + Angular for an international organisation on Azure. That narrows the syllabus hard: ASP.NET Core, EF Core, C#, REST, SQL Server, Angular (components/routing/DI/RxJS/services), TypeScript, plus accessibility and secure-coding awareness.
3. **How vendor assessments are assembled.** Most large employers run an off-the-shelf platform (SHL/AMCAT, TestDome, Codility, HackerRank, iMocha, Mettl) and either pick a standard "Full Stack .NET + Angular" item bank or hand-pick 5–10 items per section. Those banks are remarkably similar in shape, and that similarity is what this document exploits.

**So: treat this as the highest-probability question set, not a leaked paper.** The value is that ~80% of what appears will be a variant of something below, and every answer here is one you can defend out loud.

### 0.2 The four blueprints — and what each means for your pacing

| Blueprint | Shape | Typical duration | Pass bar | Your risk |
|---|---|---|---|---|
| **A. Pure MCQ screen** (most common) | 40–50 questions, 4 options each, no negative marking | 45–60 min | 65–75% | Easy questions you *over-think*; time bleed on 2 hard ones |
| **B. MCQ + 2 coding tasks** | 20–25 MCQ (25 min) + 2 tasks (35 min) | 60 min | Partial credit per test case | Running out of time on task 2 — start it before perfecting task 1 |
| **C. MCQ + SQL + snippet repair** | 25 MCQ + 1 SQL query + 1 "fix this method" | 60 min | Weighted by section | SQL written for SQL Server syntax, not Postgres/MySQL |
| **D. Sectioned with per-section timers** | Sections lock when their timer expires (C# 12 min, API 12 min, Angular 18 min, SQL 10 min, JS/TS 8 min) | 60 min | Each section must pass | **A disaster section cannot be compensated.** Never let Angular — your weakest — be the one you enter cold |

**Assume D or B.** Both punish the same mistake: spending your freshest 25 minutes on C#, where you are already strong, and hitting Angular tired.

### 0.3 What the paper will almost certainly *not* contain

Don't prepare these, and don't burn time wishing they were there:

- System design / whiteboard architecture (that's the later interview round)
- Long-form behavioural answers (they come as 1–3 "what would you do" MCQs at most)
- Deep Azure resource configuration (maybe 2–3 identity/security items, no portal screenshots)
- Angular version archaeology ("what changed in v16") — versions matter only where behaviour differs
- Anything requiring you to remember an exact API signature you'd never type from memory

### 0.4 Scoring reality

- **You are scored against a bar, not against other candidates** — but the bar is usually "senior", so 60% is not a pass.
- **Most platforms report per-topic scores.** A strong total with a weak Angular section still fails a "must have Angular" role. Fix your weakest section, not your best.
- **Partial credit is common on coding items** (one point per hidden test case). A brute-force solution that passes 6/8 beats an elegant one that throws on 8/8.
- **Negative marking is rare on this kind of screen.** When unsure, answer.

---

## §1. What World Bank specifically screens for

Framed as inference from the organisation's context, not inside knowledge. Every point below translates into question shapes you should expect.

| Their reality | Question shapes it produces | Your prep |
|---|---|---|
| **International organisation, public scrutiny** — code is audited, data is sensitive (country/economic data, procurement, HR) | Secure coding MCQs: SQL injection, XSS, CSRF, secrets in source, least privilege, audit trails, PII logging | `05D` §9, `05` §security, this doc Q81–Q88 |
| **Accessibility is contractual, not optional** (WCAG 2.1 AA is standard for UN-system web properties) | "Which attribute makes this control announce correctly?", semantic HTML vs `div` + click, focus management, `alt` text, colour-only signalling, keyboard traps, ARIA roles | §7 Q84–Q88 — small, cheap, high-probability |
| **Multi-language users** (English/French/Spanish/Arabic across offices) | Angular i18n, `$localize`, formatting dates/numbers with locale, RTL, why not to hard-code strings | §7 Q89, `01` §i18n |
| **Azure + Entra ID (formerly Azure AD) shop** | OAuth2/OIDC flows, JWT structure, access token vs ID token, scopes vs roles, `Authorization: Bearer`, token storage in SPA | §5 Q36–Q40, `11` §Entra |
| **Enterprise Angular + .NET, not startup chaos** | Strict typing, `strict` TS, DI lifetimes, `OnPush`, change-detection discipline, environment config, versioning, error handling consistency | §7, §8 |
| **Long-lived systems, many contractors** | "Why is this code hard to maintain?" refactoring MCQs, DI over `new`, interfaces at boundaries, N+1 and performance | §6, §9 |
| **Angular is their front-end standard** | The largest block of questions. Assume **18–30% of the paper is Angular** | §7 — do this section properly |
| **Procurement/compliance-heavy delivery** | A couple of items on audit logging, soft delete/retention, data residency, least-privilege DB accounts | `05D` §G |

**One more, unsaid but real:** for an international organisation you will also get a values/integrity questionnaire (or 3–5 MCQs). Answer consistently and honestly on conflict-of-interest, fraud reporting, and treating people fairly. There is no technique; there is only not being flippant. §13.

---

## §2. The 60-minute game plan

### 2.1 Minute budget

Print this. It is the single highest-value page in the document.

| Window | Action | Rule |
|---|---|---|
| **0:00–0:03** | **Triage.** Scroll every question without answering. Count them. Note the coding tasks and their point values. Note any section timers. | *Never* start at Q1 blind. 3 minutes of map-reading saves 10 minutes of panic |
| **0:03–0:30** | **First pass, easy 70%.** Answer everything you know in under 60 seconds. Flag (don't solve) the rest. | **60-second cap per MCQ.** At the cap, guess-and-flag, move on |
| **0:30–0:45** | **Coding / SQL tasks.** Highest point density. Start with the one you're most certain of. | Write the working brute force *first*, run it, then improve. Never submit an unrun solution |
| **0:45–0:54** | **Second pass on flagged items.** Now use domain reasoning per type (§2.3). | Only 90 seconds each. Still stuck → your gut answer stands |
| **0:54–0:58** | **Integrity sweep.** No blanks. No accidental multi-select. Re-read any "which is **NOT**" you flagged. | One blank is a guaranteed zero; a guess is 25% |
| **0:58–1:00** | Submit. | Do not start a new line of thought at 0:57 |

### 2.2 Nine rules that decide this paper

1. **Read the stem's last line first.** "Which is **NOT**", "Choose **two**", "Select the **best**" change the answer completely, and they're always at the end.
2. **Answer every question.** Blanks cost more than wrong guesses.
3. **Never debug in your head.** If a "predict the output" item takes more than 90 seconds, you don't know it — pick your best reading and move.
4. **For MCQs: eliminate two, then choose between the survivors on principle, not memory.** Principles: *immutability over mutation, async over blocking, interface over concrete, parameterise over concatenate, dispose over leak, configuration over hard-code, let the DB filter, measure before optimising.*
5. **"All of the above" / "Both A and B"** — on enterprise-vendor banks these are correct more often than chance when the distractors are individually true.
6. **When two options are near-identical but one is stricter/safer** (e.g. `AsNoTracking()` vs tracking, `DATETIMEOFFSET` vs `DATETIME`, `Guid` vs `int` for a public id) — for a bank/institutional employer, pick the safer one and be ready to justify it.
7. **Code tasks: read the constraints before the story.** Input size tells you the intended complexity. `n ≤ 10⁵` means "no nested loops".
8. **Name things in code tasks.** `result`, `temp`, `x` read as careless even when the logic is right; human reviewers give benefit of the doubt to readable code.
9. **Assume SQL Server syntax** unless the stem says otherwise. `TOP`, `ISNULL`, `GETUTCDATE()`, `[brackets]`, `OFFSET ... FETCH`. If it's platform-locked (Codility), Postgres syntax may be expected — check the sample item.

### 2.3 Fallback logic per question type, when you don't know

| Type | Fallback move |
|---|---|
| "What does this C# code output?" | Track **type** (value/reference), **copy vs reference**, **deferred vs executed** (LINQ), **ordering** (async continuations). Those four cause 80% of these items |
| "Which Angular lifecycle…" | `ngOnInit` = once after inputs; `ngOnChanges` = whenever a bound input changes (before `ngOnInit`); `ngAfterViewInit` = after child views exist; `ngOnDestroy` = cleanup. If the question mentions removing a subscription/timer → `ngOnDestroy` |
| "Which RxJS operator…" | Is the question about *order*, *latest*, *all in parallel*, or *ignoring while busy*? → `concatMap`, `switchMap`, `forkJoin`/`mergeMap`, `exhaustMap` |
| "Which index…" | Filter/join columns, most selective first, include the selected columns → covering index |
| "Why is this slow?" | N+1, missing index, no pagination, loading whole entity then projecting, sync-over-async, change detection storm |
| "Which is most secure?" | Parameterise, least privilege, hash+salt (not encrypt) for passwords, never log secrets, HTTPS, validate server-side |
| "Which is best practice?" | The one that's boring, explicit, and testable |

---

## §3. The 20 facts to have at instant recall

If you memorise nothing else, memorise these. They are the answers to the questions that appear in *every* .NET + Angular bank.

| # | Fact | Where it's asked |
|---|---|---|
| 1 | A **clustered index is the table** (one per table); non-clustered is a separate structure with a pointer/key lookup | §6 |
| 2 | `AsNoTracking()` for reads — no change-tracker overhead, no identity resolution | §6 |
| 3 | **N+1**: one query for the parent + one per child. Fix with `Include`/projection, not loops | §6, §9 |
| 4 | `IEnumerable` = in-memory, deferred, re-executes on each enumeration. `IQueryable` = translated to SQL, accumulates expression until materialised | §4, §6 |
| 5 | `async void` only for event handlers. `Task` everywhere else | §4 |
| 6 | `.Result`/`.Wait()` on an async call in a sync context → **deadlock**; in ASP.NET Core it's thread-pool starvation + sync-over-async | §4 |
| 7 | DI: `AddSingleton` one for app lifetime, `AddScoped` one per request, `AddTransient` per resolution. **Never inject a scoped service into a singleton** (captive dependency) | §5 |
| 8 | Middleware order matters: exception handler → HTTPS/HSTS → static files → routing → CORS → authentication → **authorization** → endpoints | §5 |
| 9 | JWT = header.payload.signature; **signed, not encrypted** — never put secrets or PII in the payload | §5 |
| 10 | Angular DI: `providedIn: 'root'` = tree-shakable singleton for the whole app; component-level `providers` = a fresh instance per component instance | §7 |
| 11 | `OnPush` + **signals** (or immutable inputs + `async` pipe) = check only when something signals a change | §7 |
| 12 | Angular `HttpClient` Observables are **cold** — one request per `subscribe`; they don't auto-unsubscribe from a component → leak | §7, §10 |
| 13 | `switchMap` cancels the previous inner observable; `mergeMap` doesn't; `concatMap` queues; `exhaustMap` ignores new while busy. **Search/typeahead → `switchMap`** | §10 |
| 14 | `forkJoin` = "all complete, then one array" (like `Promise.all`); **completes without emitting if the source is empty** | §10 |
| 15 | `Subject` is hot and has no current value; `BehaviorSubject` has an initial value and `getValue()`; `ReplaySubject` replays n | §10 |
| 16 | `@for` requires `track`; `@if`/`@for`/`@switch` are built-in control flow since v17 — no `NgIf`/`NgFor` import needed | §7 |
| 17 | A functional `CanActivateFn` returns `true | false | UrlTree` (a `UrlTree` = redirect) | §7 |
| 18 | JavaScript: `var` is function-scoped and hoisted (`undefined`); `let`/`const` are block-scoped with a temporal dead zone | §8 |
| 19 | Event loop order: **synchronous → microtasks (`Promise.then`, `queueMicrotask`) → macrotasks (`setTimeout`)** | §8 |
| 20 | SQL evaluation order: `FROM → JOIN → WHERE → GROUP BY → HAVING → SELECT → ORDER BY → TOP/OFFSET` — which is *why* a `SELECT` alias can't be used in `WHERE` but can in `ORDER BY` | §6 |

---

## §4. Part A — C# and .NET (Q1–Q24)

*Your strongest ground. Be fast here: 20–25 seconds per item, save your time for Angular.*

---

**Q1.** What is the output?
```csharp
int a = 10;
object o = a;        // boxing
a = 20;
Console.WriteLine(o);
```
**A:** `10`. Boxing copies the value into a new heap object; later changes to `a` are invisible to `o`. *Unboxing back into a new variable gives 10 too.*

---

**Q2.** Which statement about `struct` is correct?
a) Structs are heap-allocated · b) Structs support inheritance from classes · c) Structs are value types copied on assignment · d) A struct can be `null` without `Nullable<T>`

**A:** **c**. A struct assignment copies all fields. Structs can implement interfaces but not inherit from a class (all structs derive from `System.ValueType`); a plain struct is never null (only `T?`).

---

**Q3.** What does this print?
```csharp
string s1 = "hello";
string s2 = "hel" + "lo";
Console.WriteLine(ReferenceEquals(s1, s2));
```
**A:** `True` on a normal build. Both are compile-time string literals; the compiler folds `"hel" + "lo"` into the interned literal `"hello"`, so both reference the same interned instance. *If one part were a runtime variable, the result would be `False` — and that's the follow-up they'd ask.*

---

**Q4.** `record` vs `class` — the one difference that matters in an interview?
**A:** Records give **value-based equality** (and `with`-expressions, and a compiler-generated `ToString`), classes give reference equality. Also `record struct` gives value semantics with value-type allocation. Everything else (inheritance, interfaces, members) is the same.

---

**Q5.** What is the output?
```csharp
var list = new List<int> { 1, 2, 3 };
IEnumerable<int> q = list.Where(x => x > 1);
list.Add(4);
Console.WriteLine(q.Count());
```
**A:** `3` — that is, `2, 3, 4`. LINQ over `IEnumerable` is **deferred**; the query re-evaluates when enumerated, so elements added after the query was built are included. This is the canonical "deferred execution" item.

---

**Q6.** Why is this a bug?
```csharp
foreach (var o in orders)
    foreach (var i in o.Items)
        total += i.Price * i.Quantity;   // orders is IQueryable<Order>
```
**A:** **N+1 queries.** Lazy loading or a per-parent query means 1 query for `orders` + 1 per order for `Items`. Fix: `Include(o => o.Items)` or project to a DTO and aggregate in SQL (`Sum`), or `AsSplitQuery()` when the include set is wide.

---

**Q7.** `First()` vs `FirstOrDefault()` vs `Single()` vs `SingleOrDefault()`?
**A:**
- `First` — first match; **throws** `InvalidOperationException` if none
- `FirstOrDefault` — first match or `default` (`null` for reference types)
- `Single` — exactly one; **throws if zero *or* more than one**
- `SingleOrDefault` — zero → default; **more than one still throws**

Use `SingleOrDefault` when the data model guarantees uniqueness — it turns a silent data bug into an exception.

---

**Q8.** `if (orders.Count() > 0)` — what's wrong?
**A:** On `IQueryable` this issues `SELECT COUNT(*)` (scans; expensive on large tables). Use `orders.Any()`, which translates to `EXISTS` and short-circuits at the first row. On `ICollection`/`List`, `Any()` is also cheaper than `Count()` for enumerables that don't implement `ICollection`.

---

**Q9.** What's the difference between `const` and `static readonly`?
**A:** `const` is a compile-time literal **baked into every consuming assembly** — change it and consumers keep the old value until *they* recompile (the classic versioning trap). `static readonly` is resolved at runtime, so it updates. Use `const` for genuinely eternal values (`DaysInWeek`), `static readonly` for versioned/config-ish values.

---

**Q10.** Predict the output.
```csharp
var tasks = new List<Task>();
for (int i = 0; i < 3; i++)
    tasks.Add(Task.Run(() => Console.WriteLine(i)));
await Task.WhenAll(tasks);
```
**A:** Three lines, **not necessarily `0,1,2`** — commonly `3,3,3`. `for` uses one captured variable; all closures see the final value. `foreach` (since C# 5) captures a fresh variable per iteration, so `foreach` would print the intended values. Fix: `int copy = i;`.

---

**Q11.** Difference between `Task.WhenAll` and `Task.WaitAll`?
**A:** `WhenAll` is async, returns a `Task`, doesn't block a thread, and surfaces **all** exceptions in `.Exception` (aggregate; the first is rethrown on `await`). `WaitAll` blocks the calling thread synchronously and throws `AggregateException` immediately. Use `WhenAll`.

---

**Q12.** Why is `async void` dangerous?
**A:** You can't `await` it, so you can't observe completion; exceptions escape to the synchronization context and typically **crash the process** (or vanish). It's acceptable only for event handlers. Elsewhere: return `Task`.

---

**Q13.** `decimal` vs `double` for money?
**A:** `double` is binary floating point — `0.1 + 0.2 != 0.3` — so it silently loses cents under accumulation. `decimal` is base-10 with 28–29 significant digits and exact decimal representation. Use `decimal` for money, prices, rates; `double` for physical/scientific measurements where speed and range matter. Then `decimal(18,2)` in SQL Server to match.

---

**Q14.** `string` vs `StringBuilder`?
**A:** `string` is immutable — every concatenation allocates a new string, so a loop of *n* concatenations is O(n²) in copying. `StringBuilder` mutates an internal buffer, amortised O(n). Rule: use concatenation/interpolation for a handful of parts (the compiler folds literals and uses efficient helpers), `StringBuilder` in loops.

---

**Q15.** What is the difference between `throw;` and `throw ex;`?
**A:** `throw;` re-throws preserving the **original stack trace**; `throw ex;` resets it to the current line, destroying the evidence. Also: exception filters (`catch (Exception ex) when (…)`) let you log-and-rethrow without a catch block, keeping the trace intact.

---

**Q16.** Why is `using (var conn = new SqlConnection(...))` preferable to `conn.Close()`, and when do you *not* need it?
**A:** `using` guarantees `Dispose()` on both success and exception paths (compiles to `try/finally`), which returns the connection to the pool. In a method that returns a `Task` and awaits, prefer `await using` for `IAsyncDisposable`. With `DbContext`, `use`/`dispose` is usually handled by DI — and in ASP.NET Core, `using` a *pooled* service manually can break pooling.

---

**Q17.** What does `lock (this)` do wrong?
**A:** It locks a publicly reachable object, so external code can deadlock you by locking the same instance. `lock (typeof(MyClass))` is worse — it's process-wide and visible to other libraries. Lock on a **private, dedicated `readonly object`**, or use `Interlocked`/`SemaphoreSlim`/`System.Threading.Lock` (C# 13).

---

**Q18.** `IReadOnlyList<T>` vs `IEnumerable<T>` vs `List<T>` as a return type — which and why?
**A:** `List<T>` exposes mutation and pins callers to the implementation. `IEnumerable<T>` is minimal but says nothing about count or indexed access, and it's lazily evaluated (deferred query outliving its `DbContext` = the notorious "DataReader already open" error). **`IReadOnlyList<T>`** is the usual best public return type: stable, countable, indexable, non-mutable through the interface — and materialise with `ToList()` *before* returning from the repository.

---

**Q19.** What does `volatile` guarantee — and what does it *not*?
**A:** It prevents reordering/caching of a field read/write at the instruction level for that field. It does **not** make `count++` atomic. Atomicity needs `Interlocked.Increment(ref count)`; compound invariants need `lock`.

---

**Q20.** Is `int` `Nullable<int>` in terms of equality with `null`? What does this print?
```csharp
int? x = null;
Console.WriteLine(x == null);        // A
Console.WriteLine(x.HasValue);       // B
```
**A:** `True`, `False`. `int?` is `Nullable<int>` — a struct with `HasValue`/`Value`. `x == null` compiles to a lifted comparison (no exception). `(int)x` where `x` is null throws `InvalidOperationException`; `x ?? 0` is the safe read.

---

**Q21.** `==` vs `Equals()` vs `ReferenceEquals()`?
**A:** `==` is operator overload (for `string`, it's value comparison; for most reference types, reference comparison). `Equals()` is virtual — value semantics like `record`/`string` override it. `ReferenceEquals` is always reference identity. For strings, therefore, `s1 == s2` is *not* the same operator as in Java.

---

**Q22.** Write a snippet: return the top 3 departments by total salary, only departments with more than 10 people, as a DTO.
```csharp
var report = await db.Employees
    .GroupBy(e => e.Department.Name)
    .Select(g => new DepartmentPayrollDto(
        g.Key,
        g.Count(),
        g.Sum(e => e.Salary)))
    .Where(d => d.HeadCount > 10)
    .OrderByDescending(d => d.TotalSalary)
    .Take(3)
    .ToListAsync();
```
**A:** (Model answer.) Note the two things an examiner looks for: **projection to a DTO inside the query** (only the needed columns cross the wire) and **`Where` after `Select` on the projected shape** — which is fine here and translates; if you filtered on an entity column pre-projection that would be more index-friendly. Mention `AsNoTracking()` for a read-only report.

---

**Q23.** What's the difference between `IEnumerable<T>` and `IAsyncEnumerable<T>`?
**A:** `IAsyncEnumerable` streams items asynchronously (`await foreach`), so a query of 2M rows can be processed with constant memory instead of buffering; it also avoids blocking a thread per item. Use it for streaming exports and in minimal APIs that `return Results.Stream`/write incrementally.

---

**Q24.** `sealed` — name two reasons to use it.
**A:** (1) It documents design intent and prevents a subclass breaking your invariants. (2) The JIT can devirtualise calls, and it's required for some pattern-matching/`record`-style designs. Do **not** seal things you expect to mock — Moq/NSubstitute need virtual members or an interface.

---

## §5. Part B — ASP.NET Core (Q25–Q44)

---

**Q25.** What is the correct order of these middleware calls?
```csharp
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("spa");
app.UseRouting();
app.MapControllers();
```
**A:**
```
UseExceptionHandler → UseHsts → UseHttpsRedirection → UseStaticFiles
→ UseRouting → UseCors → UseAuthentication → UseAuthorization
→ UseEndpoints / MapControllers
```
Key rules to say out loud: **`UseRouting` before CORS/auth** (routing decides which endpoint, CORS must run before auth so preflight isn't rejected), **authentication before authorization**, **CORS before authentication**, and the exception handler **first** so it wraps everything.

---

**Q26.** Why does a `401` come back with no `WWW-Authenticate` / why does CORS fail *only* on errors?
**A:** Because a middleware further down short-circuited before CORS headers were added (`UseCors` was registered after `UseAuthentication`/authorization or after an error-handling middleware that produced a response), or the exception-handler middleware returned a response outside the CORS pipeline. Ordering, almost always.

---

**Q27.** Explain the three DI lifetimes and the **captive dependency** bug.
**A:** `Singleton` — one instance for the app. `Scoped` — one per HTTP request (in ASP.NET Core; per scope elsewhere). `Transient` — new instance each resolution. **Captive dependency:** a singleton (or any longer-lived service) holding a scoped service — the scoped instance then lives for the app's lifetime, sharing and leaking state (`DbContext` shared across requests → concurrency exceptions, stale tracking, "A second operation started on this context"). In dev, `ValidateScopes`/`ValidateOnBuild` throws at startup; in production it fails silently under load.

---

**Q28.** Which of these belong in a `DbContext` lifetime, and why?
**A:** `Scoped` — one per request. It's not thread-safe, it's a unit of work, and it accumulates change-tracker state. For read-heavy paths, `AddDbContextPool`, or better, `IDbContextFactory<T>` for short-lived contexts in Blazor/background work.

---

**Q29.** How does `[ApiController]` change behaviour?
**A:** It enables: attribute routing requirement, **automatic 400** with `ValidationProblemDetails` on model-state failure (so you don't write `if (!ModelState.IsValid)`), binding source inference (`[FromBody]` for complex types), and `ProblemDetails`-shaped error responses. Then add `builder.Services.AddProblemDetails()` for consistent error contracts.

---

**Q30.** How do you return different status codes from an action without exceptions?
**A:** `ActionResult<T>` (`NotFound()`, `BadRequest(dto)`, `CreatedAtAction(nameof(Get), new { id }, dto)`, `NoContent()`, `Ok(dto)`) or, in minimal APIs, `Results<Ok<T>, NotFound>`. In both cases declare `[ProducesResponseType]` so Swagger documents the contract.

---

**Q31.** `[Authorize(Roles = "Admin")]` vs a policy — which is preferred and why?
**A:** Policies. `Roles` hard-codes role names into attributes (stringly-typed, brittle, no test seam); a policy (`AddAuthorization(o => o.AddPolicy("CanApprove", p => p.RequireClaim("perm", "orders.approve")))`) expresses a **requirement** and can be unit-tested. Claims-based + policy is the standard answer for an enterprise role.

---

**Q32.** Where do you check "is this user allowed to edit *this* order"?
**A:** Not in the controller with an inline `if`. Options: (1) `IAuthorizationService.AuthorizeAsync(User, order, "OrderOwner")` with a resource-based handler, (2) a MediatR pipeline behaviour, (3) EF Core **query filter** so the row can't even be read (`Where(o => o.TenantId == tenantId)`). Mention the modern `required`/`IAuthorizationRequirement` handler. This is also the IDOR/BOLA answer for the security section.

---

**Q33.** How do you validate a JWT in ASP.NET Core, and what does each parameter do?
```csharp
.AddJwtBearer(o => {
    o.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true, ValidIssuer = cfg.Issuer,
        ValidateAudience = true, ValidAudience = cfg.Audience,
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30),
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
```
**A:** Validates who issued it, who it's for, that it hasn't expired (with a small clock-skew tolerance — the default 5 min is too generous for revocation-sensitive apps), and that the signature matches *your* key. The classic exam trap: leaving `ValidateIssuer`/`ValidateAudience` false "because it worked" — that accepts a token minted for a different app by the same IdP.

---

**Q34.** Where should a SPA store its access token?
**A:** Preference order: **(1) don't** — use a BFF pattern with `HttpOnly; Secure; SameSite` cookies, so JavaScript never sees a token. **(2)** In-memory in a service (lost on refresh, immune to XSS exfiltration). **(3)** `sessionStorage` — better than `localStorage` (shorter window, per-tab), still XSS-readable. **Never** `localStorage` for long-lived tokens with sensitive scopes. `localStorage` cannot be read by a different origin, so it's not CSRF-exposed — but XSS defeats it, which is why the answer is "BFF or memory".

---

**Q35.** Access token vs ID token vs refresh token?
**A:** **Access token** — presented to the *API*, authorises; opaque to the SPA, short-lived (5–60 min), audience = the API. **ID token** — proof of authentication *for the client*, a JWT with user claims, audience = the client app; never send it to the API. **Refresh token** — long-lived credential used to get new access tokens; belongs to the client, must be rotated and revocable, ideally never in the browser.

---

**Q36.** Why does `AddHttpClient` exist instead of `new HttpClient()`?
**A:** `new HttpClient()` per call exhausts sockets (TIME_WAIT) and ignores DNS changes if you keep it forever; a single static instance ignores DNS changes. `IHttpClientFactory` pools the **message handler** and rotates it on a configured lifetime, giving connection reuse *and* DNS refresh, plus Polly resilience policies, per-client base addresses and headers, and typed clients for testability.

---

**Q37.** How do you run work after the response is returned (e.g. send a welcome email)?
**A:** Not with fire-and-forget `Task.Run` in the request (the scope is disposed when the request ends — that's where `Cannot access a disposed context` comes from). Options: a **queue** (`IHostedService`/`BackgroundService` + `Channel<T>`, or Azure Service Bus) with a `BackgroundService` consumer that creates its own scope via `IServiceScopeFactory`; or `IHostedService`'s `BackgroundService`; or a durable outbox table polled by a worker. The senior answer mentions at-least-once delivery and idempotency.

---

**Q38.** `IOptions<T>` vs `IOptionsSnapshot<T>` vs `IOptionsMonitor<T>`?
**A:** `IOptions` — singleton, evaluated once, no reload. `IOptionsSnapshot` — scoped, recomputed per request (sees reloaded config within a request). `IOptionsMonitor` — singleton with `OnChange` callback, always current, safe for singletons. Rule: singleton service → `Monitor`; per-request → `Snapshot`; static-at-startup → `Options`.

---

**Q39.** How do you avoid returning a 500 for expected validation failures?
**A:** Validate in a pipeline behaviour or the action, return `ValidationProblemDetails` (400) or a typed `Result<T>` from a handler, and add an exception-handling middleware mapping known domain exceptions (`NotFoundException` → 404, `ConflictException` → 409, `DomainValidationException` → 422) so business outcomes never surface as 500s — which also keeps alerting meaningful.

---

**Q40.** What's in a `ProblemDetails` response and why standardise on it?
**A:** `type, title, status, detail, instance` (+ `extensions` such as `traceId`, `errors`). It's RFC 9457, so any client or gateway can parse it, and it lets you add a correlation id consistently (`Activity.Current?.Id`). A consistent error envelope is one of the fastest "this codebase is grown-up" signals.

---

**Q41.** How do you version an API?
**A:** `Asp.Versioning.Mvc` (`AddApiVersioning` + `[ApiVersion("2.0")]`) with URL-segment or header/query versioning; URL segment is the most cache- and gateway-friendly for public APIs. Say: **additive changes are safe, breaking changes need a new version, and old versions need a deprecation policy (headers + sunset date).**

---

**Q42.** Where does rate limiting go, and how?
**A:** In-process: `builder.Services.AddRateLimiter(...)` + `app.UseRateLimiter()` with fixed-window/sliding-window/token-bucket/concurrency policies; partition by API key or user. At the edge: Azure API Management or Front Door. A lead's answer names both layers and notes that in-process counters are per-instance, so a multi-instance deployment needs a distributed store or an edge gateway.

---

**Q43.** Minimal APIs or controllers for a new service?
**A:** Minimal APIs for small, focused services (less ceremony, faster startup, better AOT story, easy `TypedResults`); controllers for large teams where filters, conventions, `[ApiController]` model binding, and familiar structure reduce friction. The point isn't the winner — it's that you **state the criteria**: team size, surface area, middleware/filters needed, AOT/trimming requirements, and consistency with the rest of the estate.

---

**Q44.** How do you make an ASP.NET Core app testable?
**A:** `WebApplicationFactory<Program>` + `HttpClient` for integration tests against a real pipeline with the DB swapped for a container/InMemory; unit tests for pure logic and validators; **never** mock `DbContext` (use a real or SQLite/Testcontainers DB) — mocking LINQ providers produces tests that assert nothing about SQL. Mention `public partial class Program {}` as the hook.

---

## §6. Part C — EF Core and SQL Server (Q45–Q62)

---

**Q45.** What does EF Core send for this, and how do you *see* it?
```csharp
var list = await db.Products.Where(p => p.Price > 100).ToListAsync();
```
**A:** `SELECT [p].[Id], [p].[Name], [p].[Price], ... FROM [Products] AS [p] WHERE [p].[Price] > 100`. Print it with `.ToQueryString()` before materialising, and read the SQL Server **actual execution plan** for index behaviour. Naming `ToQueryString()` unprompted is a strong senior signal.

---

**Q46.** `AsNoTracking()` — when, and the one caveat?
**A:** For read-only queries: skips identity resolution and snapshotting, typically 20–50% faster and less memory, and mandatory for large lists. Caveat: no identity resolution means the same row in two includes becomes two objects, and you cannot `Update` the result expecting tracking. Also `AsNoTrackingWithIdentityResolution()` when the graph must be consistent.

---

**Q47.** Cartesian explosion — what is it and the two fixes?
**A:** `Include(o => o.Items).Include(o => o.Payments).Include(o => o.Notes)` produces a JOIN of all three collections → rows = product of cardinalities (e.g. 10 × 5 × 4 = 200 rows per order, and EF de-duplicates in memory). Fixes: **`AsSplitQuery()`** (one query per collection, consistent under a `RepeatableRead`/snapshot if you need atomicity) or **project to a DTO** so you only fetch what you need.

---

**Q48.** How do you detect N+1 in a real application?
**A:** SQL logging or OpenTelemetry EF instrumentation counting commands per request; a middleware that counts `DiagnosticListener` `CommandExecuted` events; or a dev-only `DbCommandInterceptor` that warns above a threshold. Then fix by projecting/including, not by adding caching.

---

**Q49.** `ExecuteUpdate`/`ExecuteDelete` — what do they do that `SaveChanges` can't?
**A:** They run a **single `UPDATE`/`DELETE` statement** server-side with no entities materialised and no change tracker — so updating 100k rows goes from "load, mutate, save in batches" to one statement. Caveats: they bypass the change tracker (so already-tracked entities are stale — re-query or `ChangeTracker.Clear()`), bypass EF interceptors/`SaveChanges`-level validation and soft-delete query filters, and don't cascade in the same way.

---

**Q50.** How do you handle optimistic concurrency in EF Core, end to end?
**A:**
```csharp
public byte[] RowVersion { get; set; } = default!;        // [Timestamp] on the entity
modelBuilder.Entity<Order>().Property(o => o.RowVersion).IsRowVersion();

try { await db.SaveChangesAsync(); }
catch (DbUpdateConcurrencyException ex) {
    // 409 Conflict: someone changed it after you read it
    var entry = ex.Entries.Single();
    var dbValues = await entry.GetDatabaseValuesAsync();
    if (dbValues is null) return Conflict("Deleted by another user");
    // either return 409 with the current values, or reapply user's changes onto dbValues
}
```
**A:** In SQL Server the token is `rowversion`; EF adds `WHERE RowVersion = @original` and throws if 0 rows are affected. Map that to **409**, not 500, and give the client the current state so the UI can show "this record changed, here's the latest".

---

**Q51.** Difference between `SaveChanges()` and `SaveChangesAsync()`? And is `SaveChanges` transactional?
**A:** `SaveChanges` blocks a thread; `SaveChangesAsync` frees it while SQL Server works — use async end to end. A single `SaveChanges` **is** wrapped in a transaction by EF (all statements commit or roll back together). Multiple `SaveChanges` calls need an explicit `IDbContextTransaction` or an execution strategy (`CreateExecutionStrategy().ExecuteAsync(...)`) to stay atomic and retryable.

---

**Q52.** `Database.EnsureCreated()` vs `Database.Migrate()` — which in production?
**A:** `Migrate()` (applies versioned migrations, is idempotent, is what CI/CD should run as a **deploy step**, not at app startup). `EnsureCreated()` creates the schema without migrations and is only for tests/prototypes — mixing the two breaks migrations permanently. Best practice: generate an **idempotent SQL script** (`dotnet ef migrations script --idempotent`) in the pipeline so the DB user needs DDL rights only during deploy, not at runtime.

---

**Q53.** Why is `DateTime.UtcNow` in an entity a code-review comment?
**A:** It's untestable (can't fix time in a test) and it bakes a framework dependency into the domain. Inject `TimeProvider` (or `ISystemClock`) and use `timeProvider.GetUtcNow()`. Pair with `DateTimeOffset` for anything crossing time zones and `datetime2`/`datetimeoffset` in SQL.

---

**Q54.** Given a `Products` table of 5M rows and this query, what index and why?
```sql
SELECT Id, Name, Price FROM Products
WHERE CategoryId = @id AND IsActive = 1
ORDER BY Name
OFFSET 40 ROWS FETCH NEXT 20 ROWS ONLY;
```
**A:**
```sql
CREATE INDEX IX_Products_Category_Active_Name
ON Products (CategoryId, IsActive, Name)
INCLUDE (Price);
```
Reasoning to say out loud: equality columns first (`CategoryId`), then the range/sort column (`Name`) so the sort is satisfied by the index; `Price` and `Id` in `INCLUDE` make it **covering** so no key lookup. `OFFSET 40` is still fine; at `OFFSET 400000` you'd switch to **keyset pagination** (`WHERE Name > @lastName`).

---

**Q55.** Clustered vs non-clustered vs covering — explain in one breath.
**A:** A **clustered** index *is* the table — the leaf level holds the actual rows, physically ordered; only one per table. A **non-clustered** index is a sorted structure of `(key, clustered-key/locator)`; a lookup to fetch other columns is a "key lookup". A **covering** index contains every column the query needs (`INCLUDE`), so the lookup disappears.

---

**Q56.** What's wrong here?
```sql
SELECT * FROM Orders WHERE OrderDate > '2024-01-01' AND Status = @s;
```
**A:** Nothing fatal, but: `SELECT *` prevents covering indexes and drags bloated columns; the literal relies on implicit conversion (culture/format risk — use `'20240101'`/ISO 8601 or `@date`); and `Status` as a string is a design smell. Also mention SARGability: keeping the column bare so the index can seek.

---

**Q57.** Which query is faster and why?
```sql
-- A
SELECT c.Name
FROM Customers c
WHERE c.Id NOT IN (SELECT o.CustomerId FROM Orders o);

-- B
SELECT c.Name
FROM Customers c
WHERE NOT EXISTS (SELECT 1 FROM Orders o WHERE o.CustomerId = c.Id);
```
**A:** **B**. `NOT IN` with a `NULL` in the subquery returns **zero rows** (any comparison to NULL yields UNKNOWN), and it can't short-circuit. `NOT EXISTS` is NULL-safe, short-circuits on the first match, and optimises to an anti-semi-join. Mention `LEFT JOIN … WHERE o.Id IS NULL` as an equivalent third form.

---

**Q58.** `WHERE` vs `HAVING` — and why can't an alias from `SELECT` be used in `WHERE`?
**A:** `WHERE` filters rows *before* aggregation; `HAVING` filters groups *after*. Aliases exist only after `SELECT`, and the logical evaluation order is `FROM → WHERE → GROUP BY → HAVING → SELECT → ORDER BY`, so an alias is invisible to `WHERE`/`HAVING` (SQL Server) but visible to `ORDER BY`.

---

**Q59.** Write: for each department, the highest-paid employee, one row per department.
```sql
WITH Ranked AS (
  SELECT e.*, d.Name AS DeptName,
         ROW_NUMBER() OVER (PARTITION BY e.DepartmentId
                            ORDER BY e.Salary DESC, e.Id) AS rn
  FROM Employees e
  JOIN Departments d ON d.Id = e.DepartmentId
)
SELECT DeptName, e.FirstName, e.Salary
FROM Ranked e
WHERE rn = 1;
```
**A:** (Model answer.) Points to state: `ROW_NUMBER` breaks ties deterministically via the second sort key; `RANK()` would return multiple rows for ties; older interviewers may accept `OUTER APPLY (SELECT TOP 1 …)` — mention you know both.

---

**Q60.** What is a deadlock, who wins, and how do you reduce them?
**A:** Two sessions each hold a lock the other needs; the engine's deadlock monitor kills one as the **victim** (returning error 1205). Reduce by: accessing tables **in a consistent order**, keeping transactions short, avoiding user interaction inside a transaction, matching indexes so scans don't escalate, using `READ COMMITTED SNAPSHOT` where appropriate, and **retrying on 1205** at the application layer (the victim is a retry, not a bug).

---

**Q61.** `NOLOCK` — should you?
**A:** I wouldn't, except for an approximate dashboard where dirty/missing rows are acceptable. It takes no shared locks (so it can read uncommitted data, miss rows, or read a row twice) and does **not** prevent deadlocks. The correct fix for blocking reads is **RCSI** (`ALTER DATABASE … SET READ_COMMITTED_SNAPSHOT ON`) — row-versioning gives readers a consistent snapshot without blocking writers.

---

**Q62.** How does `READ COMMITTED` differ from `READ COMMITTED SNAPSHOT`, and from `SERIALIZABLE`?
**A:** `READ COMMITTED` takes short shared locks per statement — readers can block writers. `RCSI` reads from a version store, so **readers never block writers** and each statement sees a consistent snapshot. `SNAPSHOT` isolation extends that transaction-wide. `SERIALIZABLE` takes range locks to prevent phantoms — correct but the slowest and most deadlock-prone; use it narrowly (e.g. "next sequence number") and mention `sp_getapplock` as an alternative for application-level critical sections.

---

## §7. Part D — Angular (Q63–Q92)

*Expect this to be the biggest block — 18–30% of the paper. If you only have two hours left before the assessment, spend them here and on §10.*

---

**Q63.** Put the lifecycle hooks in order for a component with a bound input.
**A:** `constructor` → `ngOnChanges` (once before `ngOnInit`, then on every input change) → `ngOnInit` → `ngDoCheck` → `ngAfterContentInit` → `ngAfterContentChecked` → `ngAfterViewInit` → `ngAfterViewChecked` → `ngOnDestroy`. The exam favourites: **`ngOnChanges` fires *before* `ngOnInit`** and again on every change; **`ngOnInit` runs once**; **`ngAfterViewInit` is where `@ViewChild` is guaranteed**.

---

**Q64.** Which hook do you use to unsubscribe, and what's the modern alternative?
**A:** `ngOnDestroy` — with `takeUntilDestroyed()` (from `@angular/core/rxjs-interop`) injected in the constructor/field initialiser, or the `async` pipe which unsubscribes for you. `DestroyRef.onDestroy()` is the DI-friendly equivalent when you're not in an injection context.

---

**Q65.** What's wrong with this component?
```ts
export class OrdersComponent implements OnInit {
  orders: Order[] = [];
  constructor(private http: HttpClient) {}
  ngOnInit() {
    this.http.get<Order[]>('/api/orders').subscribe(r => this.orders = r);
  }
}
```
**A:** The subscription is never unsubscribed → it survives component destruction and can write to a dead component (leak; and with `OnPush` you'd also need `markForCheck`). Also: no error handling, no typing of the error path, and the component owns HTTP directly (hard to test/mock). Fix: `this.http.get<Order[]>('/api/orders').pipe(takeUntilDestroyed(this.destroyRef)).subscribe(...)`, or expose an Observable and use `async`, or (idiomatic modern) `toSignal(...)`.

---

**Q66.** Explain Angular's default change detection and why `OnPush` matters.
**A:** Default (`ChangeDetectionStrategy.Default`) means **every** component in the tree is checked on every event/microtask/XHR/`setTimeout` — because Zone.js patches those APIs and triggers `ApplicationRef.tick()`. With `OnPush`, a component is checked only when: an input **reference** changes, an event handler inside it fires, an `async`-pipe/observable it uses emits, a signal it reads changes, or you call `markForCheck()`. Result: far fewer checks in large lists and dashboards.

---

**Q67.** `detectChanges()` vs `markForCheck()`?
**A:** `detectChanges()` checks *this* component and its children **immediately and synchronously**. `markForCheck()` marks the component and its ancestors dirty so they're checked in the *next* normal CD cycle. Calling `detectChanges()` from a setTimeout inside `OnPush` is the classic "I made it work by brute force" answer — say so, and say why `markForCheck`/signals is correct.

---

**Q68.** What are signals and what problem do they solve?
**A:** `signal(initial)` creates a reactive value; reading it inside a template/`computed`/`effect` registers a dependency; `.set()`/`.update()` notifies dependents. `computed()` is lazy and **memoised** — recomputes only when its sources change, so it replaces a getter that recalculates on every CD cycle. `effect()` is for **side effects** (logging, localStorage, syncing to a non-Angular lib) and runs asynchronously during change detection — never derive state in it. The benefit: fine-grained reactivity, no Zone.js needed, and derived state that can't go stale (unlike a field you forgot to update).

---

**Q69.** `signal` vs `BehaviorSubject` vs `Observable` — when do you pick each?
**A:** **Signal** — synchronous state owned by a component/service (current user, filters, cart). **`BehaviorSubject`/`Observable`** — streams: events over time, debounced input, websockets, HTTP with operators. Signals don't replace RxJS for async composition; `toSignal()`/`toObservable()` bridge the two. Legacy `EventEmitter`-based "simple state service" is the anti-pattern signals remove.

---

**Q70.** Explain `@if` / `@for` vs `*ngIf` / `*ngFor`.
**A:** Since v17 the built-in control flow is compiler-level, not directives: no imports needed (unless in an older project), better type narrowing (`@if (user; as u)`), and `@for` **requires a `track`** expression for identity/diffing (`track item.id`), with `@empty` for the empty case and `$index/$first/$last/$even/$odd` in scope. `@defer` (v17+) lazily loads a block's dependencies — a good answer to "how would you improve first-load performance?".

---

**Q71.** Why does `@for` *require* `track` — what does it actually do?
**A:** It provides the identity used to reconcile DOM nodes between renders. Without it Angular would have to recreate/move nodes (or, in older `*ngFor`, re-render on new object identities), losing focus, input state, and animation. `track item.id` keeps DOM stable when the array is replaced with new object instances — the single most common list-performance fix.

---

**Q72.** Standalone components — what changed and why does it matter?
**A:** A component declares its own imports (`imports: [CommonModule, RouterLink]`) instead of belonging to an `NgModule`. Benefits: no `NgModule` boilerplate, lazily loadable per-component (`loadComponent`), explicit dependency graph, better tree-shaking. In v19+ **standalone is the default** (`standalone: true` is no longer needed), and `bootstrapApplication(App, appConfig)` replaces `platformBrowserDynamic().bootstrapModule(AppModule)`.

---

**Q73.** How does Angular DI resolve a service, and what's the difference between `providedIn: 'root'` and a component `providers` entry?
**A:** The injector hierarchy is component-tree based (plus a root/module injector and a platform injector); resolution walks **up** from the requesting component. `providedIn: 'root'` registers in the root injector → one **tree-shakable singleton** for the app. A component's `providers: [MyService]` creates a **new instance per component instance**, scoped to its subtree — the classic mistake is doing this with a "shared state" service and wondering why the state resets.

---

**Q74.** `useClass` vs `useValue` vs `useFactory` vs `useExisting`?
**A:** `useClass` — substitute implementation (mocking/base-vs-concrete). `useValue` — a literal/config object. `useFactory` — computed at runtime, can depend on other services (`deps: [...]`). `useExisting` — alias to an already-registered token (so both tokens share one instance, unlike `useClass` which creates two).

---

**Q75.** What is an `InjectionToken` for?
**A:** A non-class dependency — a config object, an environment flag, a callback. Needed because interfaces are erased at runtime so they can't be injection tokens. `new InjectionToken<AppConfig>('app.config')`, provided with `useFactory`/`useValue`, injected with `inject(APP_CONFIG)`. `providedIn: 'root'` + `factory` gives a tree-shakable default (`APP_CONFIG`/`API_BASE_URL` pattern).

---

**Q76.** Write a functional route guard that redirects to login, preserving the attempted URL.
```ts
export const authGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.isAuthenticated()
    ? true
    : router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};
```
**A:** (Model answer.) Two things examiners look for: **`inject()` inside the function** (guards run in an injection context) and **returning a `UrlTree`** rather than navigating imperatively (declarative, testable, no double navigation). Also `CanMatchFn` for role-based feature gating — it runs *before* the lazy chunk downloads, which `canActivate` can't prevent.

---

**Q77.** `canActivate` vs `canMatch` vs `canDeactivate` vs `resolve`?
**A:** `canActivate` — may this route activate? (chunk may already be downloaded). `canMatch` — may this *route definition* be matched at all (best for role-based feature separation; the bundle is never fetched). `canDeactivate` — may we leave (unsaved-changes dialog). `resolve` — prefetch data before activation so the component renders with data and no spinner state.

---

**Q78.** How do you handle a route parameter change on the same component?
**A:** `ActivatedRoute.paramMap` is an **Observable** and emits on `/orders/1 → /orders/2` without recreating the component; the `.snapshot` does not update. So either subscribe to `paramMap` (with `switchMap` into the data call) or use `withComponentInputBinding()` so the router pushes params straight into a signal input — the modern answer.

---

**Q79.** `Router.navigate(['/x'])` vs `navigateByUrl('/x')`?
**A:** `navigate` takes a **commands array** and interprets relative to the current route (respects `relativeTo`); `navigateByUrl` takes an absolute URL **string** and is resolved against the app root. Use `navigate` with `relativeTo: this.route` for child routes; `navigateByUrl` for whole-URL redirects.

---

**Q80.** How do you lazy-load a feature route and preload it?
```ts
export const routes: Routes = [
  { path: 'orders', canMatch: [roleGuard],
    loadChildren: () => import('./orders/orders.routes').then(m => m.ORDERS_ROUTES) },
  { path: 'login', loadComponent: () => import('./login/login.component').then(m => m.LoginComponent) },
];
// main.ts
provideRouter(routes, withPreloading(PreloadAllModules), withComponentInputBinding())
```
**A:** `loadChildren` for a route set, `loadComponent` for a single component; `PreloadAllModules` fetches lazily after first paint (best of both: fast first load, instant later navigation). `@defer` covers the template-level case.

---

**Q81.** `Promise` vs `Observable` in Angular's HttpClient?
**A:** An `HttpClient` Observable is **cold and unicast**: nothing happens until `subscribe`, and **each subscription fires a new request**; it can emit multiple values, be cancelled (`unsubscribe` aborts the XHR), and composes with operators (`retry`, `switchMap`, `debounceTime`). A `Promise` is eager, single-value, non-cancellable (barring `AbortController`). That's why `subscribe` twice = two HTTP calls — a favourite "why is this called twice?" exam item.

---

**Q82.** What does an HTTP interceptor do, and how do you register one today?
```ts
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthService).token();
  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
// app.config.ts
provideHttpClient(withInterceptors([authInterceptor]), withFetch())
```
**A:** Interceptors wrap every request/response: add headers, base URL, retry, error normalisation, loading spinner, correlation id. **`HttpRequest` is immutable** — you must `clone()`. Order: request interceptors run in registration order, responses in reverse. Class-based `HTTP_INTERCEPTORS` + `withInterceptorsFromDi()` is the pre-v15 style — true of many real codebases (including this repo's `frontend/`), so if the stem shows that, note it as legacy with a one-line justification for migrating.

---

**Q83.** How do you handle a 401 and refresh the token without infinite loops?
**A:** In an interceptor with `catchError`: on `status === 401` and *not* already retried and *not* the refresh endpoint itself → call the refresh once (guarded by a shared `BehaviorSubject`/`shareReplay` in-flight promise so concurrent 401s trigger **one** refresh), update the token, then `next(req.clone(...))`; otherwise propagate the error and trigger logout. The keywords the examiner wants: **single-flight refresh**, **`HttpContext` retry flag**, **no retry for `/auth/refresh`**.

---

**Q84.** (Accessibility — very likely for World Bank.) What's wrong with this?
```html
<div class="btn" (click)="save()">Save</div>
<input placeholder="Email">
```
**A:** A `div` isn't focusable or activatable by keyboard and has no role — use `<button type="button">`, which gives focus, Enter/Space, and screen-reader semantics for free. The `input` has no `<label for>` (a placeholder is not a label: it disappears on typing and is poorly announced) — add `<label for="email">Email</label><input id="email" type="email" autocomplete="email">`. Mention: no colour-only status, visible focus ring, `aria-live="polite"` for async status, and that Angular CDK provides `cdkTrapFocus` for modals and `LiveAnnouncer`.

---

**Q85.** How does Angular protect against XSS?
**A:** Interpolation `{{ }}` and property bindings are escaped/treated as text; `[innerHTML]` is **sanitised** by `DomSanitizer` (strips `<script>`, event handlers). Danger: `bypassSecurityTrustHtml/Url/Script` opts out — never with user data. Best practice is to avoid `innerHTML` entirely and bind data to the template. Angular does **not** sanitise what the server returns in JSON — bad data in a styled component is still a trust boundary issue.

---

**Q86.** CSRF: does Angular handle it?
**A:** With cookies, yes: `HttpClientXsrfModule`/`withXsrfConfiguration` reads the `XSRF-TOKEN` cookie and sends `X-XSRF-TOKEN` on mutating requests, and ASP.NET Core's antiforgery validates it. If auth is a `Bearer` header from memory (not a cookie), CSRF is structurally impossible — the trade-off is then XSS protection instead. Say both, because it shows you understand *why*.

---

**Q87.** Reactive vs template-driven forms — which and when?
**A:** **Reactive** — model defined in the class (`FormBuilder`), synchronous, typed (`FormControl<string>`), immutable value snapshots, trivial to unit-test, supports dynamic fields, `updateOn: 'blur'|'submit'`. **Template-driven** — `ngModel` in the template, async, NgModule-based, fine for a 2-field search box. Default answer for an enterprise form: **reactive, typed, with `NonNullableFormBuilder`.**

---

**Q88.** Typed forms: why is `new FormControl('')` non-nullable in v14+ and what's the practical consequence?
**A:** `FormControl<string>` — `.value` is `string`, so `reset()` without a value leaves the control in an invalid state unless the type includes `null` (`FormControl<string | null>`). Use `{ nonNullable: true }` / `fb.nonNullable.group({...})` for non-nullable controls so `reset()` returns the initial value instead of null, and you avoid `value!` assertions.

---

**Q89.** (i18n — likely.) How do you internationalise an Angular app?
**A:** Built-in: mark strings with `i18n` attributes or `$localize` tags, run `ng extract-i18n` to produce `messages.xlf`, get translations, then **build one bundle per locale** (`--localize`, `i18n` config in `angular.json`) served by path or host — with `LOCALE_ID` and `registerLocaleData` for pipes, and ICU expressions for plurals/select. Note the trade-off: build-time i18n means a separate build per language (fine for 4–6 locales, World Bank scale) vs runtime libraries (ngx-translate/Transloco) which allow switching without a reload but ship the merge logic. Also: `dir="rtl"` + logical CSS properties for Arabic.

---

**Q90.** How do you configure the API base URL for different environments without rebuilding?
**A:** File replacement (`environment.ts` → `environment.prod.ts`) is the Angular default but bakes the URL into the bundle — bad for container promotion ("build once, deploy anywhere"). Better: fetch a runtime `assets/config.json` with `provideAppInitializer()` (or an injection token resolved by a factory) before bootstrap, so the same image runs in every environment. This exact answer is a *lead* differentiator — mention build-once-promote.

---

**Q91.** How do you unit-test an Angular component that calls HTTP?
**A:** `TestBed` with `provideHttpClientTesting()`, then `HttpTestingController.expectOne('/api/orders').flush(mockData)`, `fixture.detectChanges()`, assert on the DOM, `httpMock.verify()` to catch unexpected requests. For anything with `OnPush`, call `fixture.detectChanges()` after flushing. Don't mock `HttpClient` with spies — it hides URL/method mistakes.

---

**Q92.** A list of 5,000 rows re-renders slowly on filter input. Name four fixes in order of value.
**A:** (1) **`@for` with `track item.id`** and `OnPush` (or signals) — stop re-rendering everything. (2) **Virtual scrolling** (`cdk-virtual-scroll-viewport`) or server-side paging — don't put 5,000 DOM nodes in the page. (3) **`debounceTime(300)` + `distinctUntilChanged`** on the filter input, with the heavy filtering moved to the server or a `computed`. (4) `NgZone.runOutsideAngular` for high-frequency events (scroll, drag), plus `@defer` for below-the-fold panels. Mention measuring first (Angular DevTools profiler) — "I'd profile before optimising" is the senior framing.

---

## §8. Part E — TypeScript and JavaScript (Q93–Q107)

---

**Q93.** `var` vs `let` vs `const`?
**A:** `var` — function-scoped, hoisted and initialised to `undefined`, so usable before declaration; leaks out of `for`/`if` blocks. `let`/`const` — block-scoped with a **temporal dead zone** (accessing before declaration throws `ReferenceError`). `const` prevents reassignment of the binding, **not** mutation of the object (`const a = []; a.push(1)` is legal).

---

**Q94.** Predict the output.
```js
for (var i = 0; i < 3; i++) setTimeout(() => console.log(i));
for (let j = 0; j < 3; j++) setTimeout(() => console.log(j));
```
**A:** `3 3 3` then `0 1 2`. `var` shares one binding across iterations; `let` creates a fresh binding per iteration. The classic fix for `var` was an IIFE; today `let`.

---

**Q95.** Predict the output order.
```js
console.log('1');
setTimeout(() => console.log('2'));
Promise.resolve().then(() => console.log('3'));
queueMicrotask(() => console.log('4'));
console.log('5');
```
**A:** `1, 5, 3, 4, 2`. Synchronous first, then the **microtask** queue (promise callbacks, `queueMicrotask`) in FIFO order, then **macrotasks** (`setTimeout`). This is the highest-frequency JS output question in existence.

---

**Q96.** `==` vs `===` — and name the worst case of `==`.
**A:** `==` coerces types (`'0' == 0` → true, `'' == 0` → true, `null == undefined` → true, `[] == false` → true). `===` compares without coercion. Worst case: `'0' == false` is true and `'0' == 0` is true while `false == 0` is true — a coercion triangle that has produced real bugs. Always `===`; the only conventional exception is `x == null` to test null-or-undefined in one go.

---

**Q97.** Predict the output.
```js
console.log(typeof null);
console.log(null instanceof Object);
console.log([] + []);
console.log([] + {});
console.log(typeof NaN);
console.log(0.1 + 0.2 === 0.3);
```
**A:** `'object'` (a historical bug kept for compatibility); `false`; `''` (both arrays stringify to `''`); `'[object Object]'`; `'number'` (`NaN` is a number, and `NaN !== NaN`); `false` (`0.30000000000000004`).

---

**Q98.** What does `this` refer to in each?
```js
const o = {
  v: 1,
  a() { return this.v; },
  b: () => this.v,
};
```
**A:** `o.a()` → `1` (method call, `this` = `o`). `o.b()` → `undefined` (arrow functions capture `this` lexically, i.e. from module/global scope, **ignoring the call site**). This is why passing a method as a callback loses `this` — fix with `bind`, an arrow wrapper, or a class field arrow.

---

**Q99.** `interface` vs `type` in TypeScript?
**A:** Both describe object shapes and are mostly interchangeable. Differences: `interface` supports **declaration merging** (libraries augment it) and is slightly better for error messages; `type` handles unions, intersections, tuples, mapped/conditional types, and primitives (`type Id = string | number`). Project convention: `interface` for public object contracts, `type` for unions/utility compositions.

---

**Q100.** `any` vs `unknown` vs `never`?
**A:** `any` disables type-checking (contagious, defeats the point of TS). `unknown` accepts anything but requires narrowing before use — the correct type for parsed JSON / `catch` variables. `never` — a value that can't exist; used for exhaustive `switch` checks (`const _x: never = value`) and for functions that always throw.

---

**Q101.** What is `strict` mode made of, and why does it matter for an enterprise codebase?
**A:** `strictNullChecks` (null/undefined must be handled), `noImplicitAny`, `strictFunctionTypes`, `strictPropertyInitialization`, `strictBindCallApply`, `useUnknownInCatchVariables`, plus `strictTemplates` for Angular's template checker. It converts a whole class of runtime `Cannot read property of undefined` errors into compile errors — the single cheapest quality gate to switch on for legacy code, done incrementally by module.

---

**Q102.** `Partial<T>`, `Pick<T, K>`, `Omit<T, K>`, `Record<K, V>`, `Readonly<T>` — give a realistic use for each.
**A:** `Partial<T>` — PATCH/DTO update payloads. `Pick` — list/summary DTOs (`Pick<Order,'id'|'status'>`). `Omit` — create-DTOs (`Omit<Order,'id'|'createdAt'>`) — this mirrors C#'s `record` with a narrower constructor. `Record` — lookup maps (`Record<OrderStatus, string>` forces exhaustiveness in a label map). `Readonly<T>` — config/frozen constants, catches accidental mutation.

---

**Q103.** What does `satisfies` do, and why not just `: Type`?
**A:** `const cfg = {...} satisfies Config` type-checks the literal **without widening** it — so `cfg.retries` keeps its literal type (`3`, not `number`) and you keep autocomplete + narrowing, while still getting validation. With `: Config` the literal is widened and excess-property/`as const` behaviours differ.

---

**Q104.** `enum` vs `const enum` vs a union of string literals?
**A:** `enum` emits runtime code (an object), so it exists in the bundle and can be reverse-mapped; numeric enums allow any number, which lets invalid values through. `const enum` is inlined at compile time — but breaks under `isolatedModules`/modern bundlers. **A union of string literals** (`type Status = 'Pending' | 'Paid'`) plus a `Record` for labels is the front-end standard: zero runtime cost, exhaustive `switch` checking, and it maps naturally to a C# `enum` serialised as a string.

---

**Q105.** What's the difference between `?.` and `??` — and the trap with `||`?
**A:** `a?.b` short-circuits to `undefined` if `a` is null/undefined. `a ?? b` returns `b` only for `null`/`undefined`. The trap: `count || 10` gives `10` when `count === 0`, silently overriding a legitimate zero — use `??`.

---

**Q106.** Predict the output.
```js
console.log([10, 9, 1].sort());
console.log([1, 2, 3].map(parseInt));
```
**A:** `[1, 10, 9]` — default `sort` compares as **strings** (lexicographic) and mutates in place; fix with `sort((a, b) => a - b)`. `[1, NaN, NaN]` — `map` passes `(value, index)`, so `parseInt('2', 1)` → `NaN` (radix 1 invalid). Both are in every JS bank.

---

**Q107.** How do you deep-clone an object, and what does `structuredClone` improve?
**A:** `JSON.parse(JSON.stringify(x))` loses `Date` (becomes a string), `undefined`, `Map`/`Set`, and throws on cycles. `structuredClone(x)` handles Dates, Maps, Sets, ArrayBuffers, and cycles natively (but not functions or class prototypes). For Angular state, prefer **immutable updates** (`{...state, items: [...state.items, item]}`) over clones — that's what makes `OnPush`/signal equality checks cheap.

---

## §9. Part F — RxJS and async patterns (Q108–Q117)

---

**Q108.** `switchMap` vs `mergeMap` vs `concatMap` vs `exhaustMap` — one line each plus a use case.
**A:**
- `switchMap` — **cancels** the previous inner Observable on each new value. Typeahead/search, route-param-driven fetches.
- `mergeMap` — runs all inner Observables **concurrently**, results interleave (order lost). Parallel independent writes.
- `concatMap` — **queues**, one at a time, order preserved. Ordered POSTs / preserving sequence.
- `exhaustMap` — **ignores** new values while one is in flight. Login/checkout double-click protection.

If a stem says "the user types fast and responses arrive out of order" → `switchMap` **plus** a server-side guard, because cancellation doesn't stop an in-flight response that already left.

---

**Q109.** `forkJoin` vs `combineLatest` vs `zip` vs `merge`?
**A:** `forkJoin` — wait for **all to complete**, emit one array (like `Promise.all`; **emits nothing and completes if any source is empty**). `combineLatest` — emits on every change once all have emitted at least once; use for form filters. `zip` — pairs by index; rarely what you want for HTTP. `merge` — a flat stream of all values, no combination.

---

**Q110.** `Subject` vs `BehaviorSubject` vs `ReplaySubject` vs `AsyncSubject`?
**A:** `Subject` — hot, no initial value, subscribers only get future values. `BehaviorSubject` — requires an initial value, gives the current value on subscribe, `getValue()` (the state-service workhorse). `ReplaySubject(n)` — replays the last *n* (or all with `bufferSize`) to new subscribers. `AsyncSubject` — emits only the last value on completion (rare). All must be `complete()`d/`unsubscribe`d or they leak.

---

**Q111.** What does `shareReplay({ bufferSize: 1, refCount: true })` do and why does `refCount` matter?
**A:** It multicasts one subscription to many subscribers and replays the last value to late subscribers — the standard way to cache an HTTP result across components. With `refCount: true` it tears down the source when the last subscriber leaves (preventing a permanent "zombie" subscription); without it, the source stays subscribed for the app's lifetime. Also note `shareReplay(1)` alone can leak in older versions — say `refCount: true` explicitly.

---

**Q112.** What happens in this code, and how do you fix it?
```ts
search = new FormControl('');
ngOnInit() {
  this.search.valueChanges.subscribe(v => this.http.get('/api/x?q=' + v).subscribe(r => this.r = r));
}
```
**A:** Every keystroke fires a request, in parallel, unordered, never unsubscribed — a race (last response to arrive wins, not the last typed), a leak, and a `switchMap` anti-pattern (nested subscribe). Fix:
```ts
this.search.valueChanges.pipe(
  debounceTime(300),
  distinctUntilChanged(),
  switchMap(q => this.http.get<Result[]>('/api/x', { params: { q: q ?? '' } })),
  takeUntilDestroyed(this.destroyRef),
).subscribe(r => this.r = r);
```
Bonus points: `catchError` **inside** the `switchMap` (so a failed search doesn't kill the stream forever — the classic bug).

---

**Q113.** `catchError` → return what? `throwError` vs `of(null)`?
**A:** `catchError` must return an Observable: `of(fallback)` to recover (the stream continues), `throwError(() => err)` to rethrow (note the factory function — required since v7, a common exam trap), or `EMPTY` to swallow and complete. The design point: a component-level `catchError` should restore the stream; a global error handler should log and rethrow.

---

**Q114.** `retry({ count: 2, delay: 500 })` — what's the risk?
**A:** Blind retries duplicate **non-idempotent** requests (a POST that partially succeeded now creates two orders) and can amplify an outage (retry storms). Only retry idempotent GETs, ideally with exponential backoff + jitter, and never on 4xx. In .NET, pair with `EnableRetryOnFailure` carefully — retrying a non-idempotent command is the same bug on the server.

---

**Q115.** How do you turn an Observable into a signal, and back?
**A:** `toSignal(obs$, { initialValue: [] })` (needs an initial value or it's `undefined` until first emit — in templates `@if` handles it) and `toObservable(sig)`. Both come from `@angular/core/rxjs-interop` and must be created in an injection context. `toSignal` auto-unsubscribes with the component's `DestroyRef`.

---

**Q116.** `async` pipe — why is it better than subscribing, and one caveat?
**A:** It subscribes automatically, unsubscribes on destroy, works with `OnPush`, and handles the loading/`null` state in the template (`| async as vm`), so no manual teardown. Caveat: if the Observable is **cold** (a bare `http.get`), every re-render/`async` usage can re-trigger a request — wrap with `shareReplay(1)` or assign it once in a field. Multiple `| async` on the same cold observable = multiple requests (the classic "called twice" bug).

---

**Q117.** Front end needs three independent API calls for a dashboard. Show the pattern with per-section error handling.
```ts
const vm$ = combineLatest({
  orders:    this.api.orders().pipe(catchError(() => of(null))),
  invoices:  this.api.invoices().pipe(catchError(() => of(null))),
  shipments: this.api.shipments().pipe(catchError(() => of(null))),
}).pipe(shareReplay({ bufferSize: 1, refCount: true }));
```
**A:** (Model answer.) `combineLatest` with an **object** (not array) keeps the template readable; per-source `catchError` means one failure degrades one panel instead of blanking the dashboard; `shareReplay` stops four `| async` usages from firing twelve requests. Contrast with `forkJoin`, which fails the whole dashboard if any call fails.

---

## §10. Part G — "Predict the output / spot the bug" drills (Q118–Q132)

*These are the cheapest marks on the paper and the easiest to lose. Say the rule, not the instinct.*

---

**Q118.**
```csharp
var arr = new[] { 1, 2, 3 };
var q = arr.Where(x => x > 1);
arr[0] = 10;
Console.WriteLine(string.Join(",", q));
```
**A:** `10,2,3`. Arrays are reference types (the query sees the mutation) **and** LINQ is deferred (it runs at enumeration, not at `.Where()`).

---

**Q119.**
```csharp
struct Point { public int X; public Point(int x) => X = x; }
var a = new Point(1);
var b = a;
b.X = 2;
Console.WriteLine(a.X);
```
**A:** `1`. Struct assignment is a field-by-field copy.

---

**Q120.**
```csharp
var x = "abc";
var y = new string(new[] { 'a', 'b', 'c' });
Console.WriteLine(x == y);
Console.WriteLine((object)x == y);
```
**A:** `True` then `False`. `string` overloads `==` for value comparison; casting to `object` uses reference equality, and the runtime-constructed string is a different instance.

---

**Q121.**
```csharp
public record Money(decimal Amount, string Currency);
var a = new Money(10, "USD");
var b = new Money(10, "USD");
Console.WriteLine(a == b);
Console.WriteLine(ReferenceEquals(a, b));
```
**A:** `True` then `False`. Records synthesise value equality (`Equals`/`==`/`GetHashCode`) but remain reference types.

---

**Q122.**
```csharp
var list = new List<int> { 1, 2, 3, 4 };
var evens = list.Where(i => i % 2 == 0);
list.Add(6);
Console.WriteLine(evens.Count() + " " + evens.First());
```
**A:** `3 2`. Deferred execution again — but note the follow-up: `evens.First()` re-enumerates the whole query, and calling `Count()` then `First()` executes the predicate **twice**. Fix: `.ToList()` once, or ask for what you need in a single pass.

---

**Q123.**
```csharp
async Task<int> Get() { await Task.Delay(5); return 1; }
Console.WriteLine(Get().Result + Get().Result);
```
**A:** Prints `2`. Two sequential, **blocking** waits. What's *wrong*: sync-over-async — in a UI/synchronisation context this deadlocks; in ASP.NET Core it starves the thread pool under load. Correct: `await Get() + await Get()` or `var t = Task.WhenAll(Get(), Get())`.

---

**Q124.**
```js
const adders = [];
for (var i = 0; i < 3; i++) adders.push(() => i);
console.log(adders.map(f => f()).join(','));
```
**A:** `3,3,3`. One shared `var` binding; `let` would give `0,1,2`.

---

**Q125.**
```js
const a = Array(2).fill([]);
a[0].push(1);
console.log(JSON.stringify(a));
```
**A:** `[[1],[1]]`. `fill` copies the **same array reference** twice. Use `Array.from({length:2}, () => [])`.

---

**Q126.**
```ts
const state = { items: [1, 2], total: 3 };
state.items.push(3);
console.log(state.items.length);
```
then
```ts
interface S { readonly items: readonly number[] }
const s: S = { items: [1, 2] };
s.items.push(3);   // ?
```
**A:** First prints `3` (const doesn't deep-freeze). Second is a **compile error** — `readonly number[]` has no `push`. And the Angular follow-up: with `OnPush`, mutating an `@Input()` array **will not re-render**, because CD compares references — this is the #1 "why doesn't my list update?" bug.

---

**Q127.**
```ts
@Component({ selector: 'app-l', template: `@for (i of items; track i.id) { <p>{{ i.name }}</p> }` })
export class LComponent { }
```
**A:** Two problems: `items` is not declared (compile error under `strictTemplates`), and if you remove `track i.id` the **compiler rejects the `@for`** — `track` is mandatory. That's the exam's usual point.

---

**Q128.**
```ts
this.form.setValue({ name: 'x' });
console.log('changed');
// elsewhere
this.form.valueChanges.subscribe(v => console.log('valueChanges', v));
```
**A:** `valueChanges` **does** emit for programmatic `setValue`/`patchValue` — not just user typing. That's why "my save button thinks the form is dirty" bugs exist. Suppress with `{ emitEvent: false }`, and use `markAsPristine()` when you genuinely want to reset dirtiness.

---

**Q129.**
```sql
CREATE TABLE T (Id int IDENTITY PRIMARY KEY, V int NULL);
INSERT INTO T (V) VALUES (1), (NULL), (3);

SELECT COUNT(*), COUNT(V), SUM(V), AVG(V) FROM T;
```
**A:** `3, 2, 4, 2`. `COUNT(*)` counts rows; `COUNT(V)` ignores NULLs; `SUM`/`AVG` ignore NULLs — so `AVG` is `4/2 = 2`, **not** `4/3`. That asymmetry is the question.

---

**Q130.**
```sql
SELECT Id FROM Orders WHERE Status NOT IN (SELECT Status FROM ExcludedStatuses);
-- ExcludedStatuses contains one NULL row; Orders has 5 rows
```
**A:** **Zero rows.** `x NOT IN (… NULL …)` is never TRUE (it's UNKNOWN for every row). Use `NOT EXISTS`, or `NOT IN (SELECT Status FROM … WHERE Status IS NOT NULL)`.

---

**Q131.**
```csharp
var all = await db.Orders.AsEnumerable().Where(o => o.CustomerId == id).ToListAsync();
```
**A:** Compile error on `ToListAsync` (it needs `IQueryable`) — and the deeper bug: `AsEnumerable()` switches to client evaluation, so **every order in the table is fetched** and filtered in memory. Correct: `await db.Orders.Where(...).ToListAsync()`. Same trap with `AsAsyncEnumerable` misuse.

---

**Q132.**
```csharp
var tasks = new[] { Task.FromException(new InvalidOperationException("a")),
                    Task.FromException(new ArgumentException("b")) };
try { await Task.WhenAll(tasks); }
catch (Exception ex) { Console.WriteLine(ex.GetType().Name + " " + ex.Message); }
```
**A:** `InvalidOperationException a`. `await WhenAll` rethrows **only the first** exception; the rest live in `whenAllTask.Exception.InnerExceptions`. To log all of them you inspect the task, not the caught exception — a genuine production-logging bug.

---

## §11. Part H — Coding tasks you may be asked to write (Q133–Q142)

*If the paper has hands-on items, it will have one or two of these shapes: a small algorithm in C#, a LINQ/EF query, a SQL query, and an Angular component/service. Write the working version first, run it, then improve.*

---

**Q133. (C#, very likely) Reverse the words in a sentence, collapsing extra spaces.**
```csharp
public static string ReverseWords(string s)
{
    if (string.IsNullOrWhiteSpace(s)) return string.Empty;
    var words = s.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    Array.Reverse(words);
    return string.Join(' ', words);
}
```
**A:** O(n) time, O(n) space. Note the options flags — `RemoveEmptyEntries` is what handles multiple spaces, and saying *"`Split` without it leaves empty entries and you get double spaces"* shows you've written this for real. In-place variant (reverse the whole string, then each word) only if the interviewer asks for O(1) extra space.

---

**Q134. (C#) Find the first duplicate in an array; return -1 if none.**
```csharp
public static int FirstDuplicate(int[] nums)
{
    var seen = new HashSet<int>();
    foreach (var n in nums)
        if (!seen.Add(n)) return n;   // Add returns false on duplicate
    return -1;
}
```
**A:** O(n) time, O(n) space. The elegant bit: `HashSet.Add` returns `false` if already present, so you avoid a `Contains` + `Add` double lookup. Mention the trade-off: sorting first gives O(1) space at O(n log n) time.

---

**Q135. (C#) Given `List<Order>`, return total revenue per customer, top 3.**
```csharp
var top = orders
    .GroupBy(o => new { o.CustomerId, o.CustomerName })
    .Select(g => new
    {
        g.Key.CustomerId,
        g.Key.CustomerName,
        Revenue = g.Sum(o => o.Lines.Sum(l => l.UnitPrice * l.Quantity)),
        OrderCount = g.Count()
    })
    .OrderByDescending(x => x.Revenue)
    .Take(3)
    .ToList();
```
**A:** Group on an anonymous type so both id and name travel together (a common miss). If the source is `IQueryable`, add `AsNoTracking()` upstream and keep the projection to avoid materialising entities.

---

**Q136. (C#) Implement a bounded in-memory cache with expiry in ~20 lines.**
```csharp
public sealed class TtlCache<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, (TValue Value, DateTimeOffset Expires)> _map = new();
    private readonly TimeSpan _ttl;
    public TtlCache(TimeSpan ttl) => _ttl = ttl;

    public bool TryGet(TKey key, out TValue value)
    {
        if (_map.TryGetValue(key, out var e) && e.Expires > DateTimeOffset.UtcNow)
        { value = e.Value; return true; }
        _map.TryRemove(key, out _);
        value = default!; return false;
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
    {
        if (TryGet(key, out var v)) return v;
        v = factory(key);
        _map[key] = (v, DateTimeOffset.UtcNow.Add(_ttl));
        return v;
    }
}
```
**A:** Points: `ConcurrentDictionary` for lock-free reads; expiry checked on read (no timer thread needed — say the trade-off: expired entries linger until read, so memory is bounded by distinct keys, not time); and for a multi-instance deployment this becomes `IMemoryCache` locally **plus** Redis if the data must be consistent. Also note the real answer for .NET: use `IMemoryCache` with `AbsoluteExpirationRelativeToNow` — writing your own is an exercise, not a design.

---

**Q137. (SQL, very likely) Second-highest salary — handle ties and the empty case.**
```sql
-- Correct with ties returning one value
SELECT MAX(Salary) AS SecondHighest
FROM Employees
WHERE Salary < (SELECT MAX(Salary) FROM Employees);

-- Rank-aware, returns NULL when it does not exist
SELECT DISTINCT Salary
FROM (SELECT Salary, DENSE_RANK() OVER (ORDER BY Salary DESC) AS rnk FROM Employees) t
WHERE rnk = 2;
```
**A:** `MAX(...) WHERE < MAX` is the interview classic and returns `NULL` if there's no second value (which is the desired "no second highest"). `DENSE_RANK` handles the "list of high earners" variant. Say explicitly: `TOP 1 … ORDER BY Salary DESC OFFSET 1` fails when the highest salary appears twice (it returns the highest again) — that's the trap the question is testing.

---

**Q138. (SQL) Find customers with no orders in the last 90 days, including customers who never ordered.**
```sql
SELECT c.Id, c.Name
FROM Customers c
WHERE NOT EXISTS (
    SELECT 1 FROM Orders o
    WHERE o.CustomerId = c.Id
      AND o.OrderDate >= DATEADD(day, -90, SYSUTCDATETIME())
);
```
**A:** `NOT EXISTS` handles both "never ordered" and "not recently" and is NULL-safe (unlike `NOT IN`). Alternative: `LEFT JOIN … WHERE o.Id IS NULL` — but that only works for "never ordered"; for "not in the last 90 days" the predicate must live inside the subquery, not in the outer `WHERE` (putting it outside silently converts the anti-join into an inner join). Saying that out loud is the differentiator.

---

**Q139. (Angular) A search component: debounce, cancel, clear on empty.**
```ts
@Component({
  selector: 'app-search',
  standalone: true,
  imports: [ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <input [formControl]="query" placeholder="Search orders" aria-label="Search orders">
    @if (loading()) { <p role="status">Loading…</p> }
    @if (results(); as rows) {
      @for (r of rows; track r.id) { <p>{{ r.reference }} — {{ r.status }}</p> }
    } @empty { @if (!loading()) { <p>No orders found.</p> } }
  `,
})
export class SearchComponent {
  private readonly api = inject(OrderService);
  private readonly destroyRef = inject(DestroyRef);
  readonly query = new FormControl('', { nonNullable: true });
  readonly results = signal<OrderSummary[]>([]);
  readonly loading = signal(false);

  constructor() {
    this.query.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      tap(() => this.loading.set(true)),
      switchMap(q => this.api.search(q).pipe(
        catchError(() => of([])),
        finalize(() => this.loading.set(false)),
      )),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe(rows => this.results.set(rows));
  }
}
```
**A:** Everything the examiner is looking for: reactive typed non-nullable control, `debounceTime` + `distinctUntilChanged`, **`switchMap`** (cancels stale requests), **`catchError` inside the switch** (one failed search must not kill the stream), `takeUntilDestroyed`, signals + `OnPush`, `track`, `@empty`, `role="status"` for the screen reader, `aria-label` on the input. If asked for the "no component state" version: `toSignal` the same pipeline as a field initialiser.

---

**Q140. (Angular) A custom validator: at least one uppercase and one digit, minimum 8 characters.**
```ts
export const strongPassword: ValidatorFn = (c: AbstractControl<string>): ValidationErrors | null => {
  const v = c.value ?? '';
  const errors: ValidationErrors = {};
  if (v.length < 8) errors['tooShort'] = true;
  if (!/[A-Z]/.test(v)) errors['noUppercase'] = true;
  if (!/\d/.test(v)) errors['noDigit'] = true;
  return Object.keys(errors).length ? errors : null;
};
```
**A:** Return **`null` when valid** (the single most common mistake is returning `false`)/**an object keyed by failure when invalid**, so the template can show several messages at once. Mention: a validator must be pure and synchronous; async uniqueness checks need `AsyncValidatorFn` + `updateOn: 'blur'`.

---

**Q141. (C#) This service call is slow in production. Name the causes and rewrite it.**
```csharp
var orders = await db.Orders.Where(o => o.CustomerId == id).ToListAsync();
return orders.Select(o => new OrderDto(o.Id, o.Reference, o.Lines.Count)).ToList();
```
**A:** If `Lines` is lazy-loaded: **1 + N queries**. If not loaded: `o.Lines.Count` is 0 or throws. Also `ToList` then `Select` materialises full entities you discard. Rewrite:
```csharp
return await db.Orders
    .AsNoTracking()
    .Where(o => o.CustomerId == id)
    .Select(o => new OrderDto(o.Id, o.Reference, o.Lines.Count))
    .ToListAsync();
```
**A:** One SQL statement, three columns, no change tracking. Verify with `ToQueryString()`.

---

**Q142. (TS/JS) Implement `debounce` once, from scratch.**
```ts
function debounce<T extends (...a: any[]) => void>(fn: T, ms = 300) {
  let t: ReturnType<typeof setTimeout> | undefined;
  return (...args: Parameters<T>) => {
    if (t) clearTimeout(t);
    t = setTimeout(() => fn(...args), ms);
  };
}
```
**A:** The marks are for **clearing the previous timer**, preserving `args`, and typing it with `Parameters`/`ReturnType` rather than `any[]`. Follow-up they almost always ask: *"now add leading-edge or a `cancel()`"* — have an answer ready.

---

## §12. Part I — Scenario and troubleshooting questions (Q143–Q153)

*Short-answer items. Use a structure: **symptom → likely causes ranked → how I'd confirm → fix → prevention.** Five sentences max.*

---

**Q143. "`GET /api/orders` takes 8 seconds in production but 200 ms locally. Where do you look?"**
**A:** First, is it data volume or plan? Check the actual execution plan and top queries by total CPU on the server (not locally). Ranked causes: missing/stale statistics and a bad plan; N+1 or an unbounded `Include`; `ToList` before filtering; deep `OFFSET`; lazy loading; no pagination; sync-over-async thread starvation; contention/locking. Confirm with `ToQueryString()`, the plan, and EF command logging per request. Fix the biggest one, re-measure, then add a regression guard (a load test or a query-count assertion).

---

**Q144. "Users get logged out randomly."**
**A:** Ranked: access-token lifetime shorter than the session and no refresh flow; clock skew between issuers/instances (`ClockSkew`); the refresh endpoint itself returning 401 (causing a logout loop); two tabs racing on a rotating refresh token (one invalidates the other); token in memory lost on refresh/deploy, with no silent-renew; audience/issuer mismatch after a config change. Confirm from the `/auth` logs and the interceptor's decision path. Fix: single-flight refresh, `HttpOnly` cookie or BFF, longer refresh lifetime with rotation + reuse detection.

---

**Q145. "The same order is created twice."**
**A:** Missing **idempotency**: double-click, client retry, or a queue redelivery. Fix: a client-supplied `Idempotency-Key` persisted with a unique constraint and returning the original response for repeats; disable the button on submit *and* treat that as cosmetic only; make consumers idempotent by natural key; at-least-once delivery must be assumed. Prevention: a unique index on the business key so the DB — not the code — is the final arbiter.

---

**Q146. "A date shows as one day earlier for users in a different time zone."**
**A:** A `DateTime` was serialised without an offset (or parsed as local on the client). Fix: store `datetimeoffset`/UTC `datetime2`, transmit ISO 8601 with `Z`, and format **in the client's locale** (`DatePipe`/`Intl`) rather than on the server. Prevention: no `DateTime.Now`, no `toLocaleString` on the server, and a lint rule or test asserting round-trip.

---

**Q147. "Intermittent `A second operation started on this context before a previous operation completed`."**
**A:** A single `DbContext` is being used concurrently — typically because it leaked into a singleton, into a `Task.WhenAll`, or into a parallel loop. `DbContext` is not thread-safe and is scoped by design. Fix: one context per unit of work; `IDbContextFactory<T>` for parallel/background work; never `Task.WhenAll` over the same context; find the real cause (usually a captured context in a background task) rather than adding a lock.

---

**Q148. "The UI still shows old data after a successful save."**
**A:** Three usual causes: the component mutates the array in place so `OnPush` never re-renders (fix: replace the reference / use a signal); the read query is cached (`shareReplay` with no invalidation) or an HTTP GET is served from the browser cache (add `Cache-Control: no-store` on user data, or revalidate); or the write goes to a different database/node than the read. Confirm by checking the network tab before changing code — the exam wants "verify whether the response was even requested".

---

**Q149. "Your team's CI passes but production breaks on deploy."**
**A:** Classic environment drift: config differences, a migration not applied (or applied out of order), secrets/permissions only in the dev environment, or an untested combination (different Angular build config, CDN cache). Fix structurally: build once and promote the same artefact, versioned migrations run as a deploy step, configuration validated at startup (fail fast), a smoke test after deploy, and a rollback path that doesn't require a rebuild.

---

**Q150. "An accessibility audit fails on your forms."**
**A:** The usual five: inputs without programmatic labels; validation errors not linked (`aria-describedby`) and not announced (`aria-live`/`role="alert"`); non-semantic clickable `div`s; focus not moved on route change or dialog open (`cdkTrapFocus`, return focus on close); insufficient contrast / colour-only error signalling. Fix by adding an axe scan (or pa11y) to CI so it can't regress — "the audit is a test, not an event".

---

**Q151. "A package upgrade breaks the build two days before release."**
**A:** Don't force it. Pin back to the last green version, ship, then upgrade on its own branch with the changelog read. Root cause is the process: no lockfile discipline, no scheduled upgrade cadence, no automated regression suite to prove the upgrade is safe. Prevention: `npm ci` + committed `package-lock.json`, Dependabot/Renovate PRs weekly (small, reviewable), and a smoke test.

---

**Q152. "How would you decide between fixing a flaky test and deleting it?"**
**A:** Delete-or-fix is a false binary — the real question is what guarantee that test provides. Non-determinism almost always comes from time, ordering, network, or shared state; each has a known fix (`TimeProvider`, deterministic data, `WebApplicationFactory`/`HttpTestingController`, isolation). If the behaviour it protects is genuinely covered elsewhere, removing it is correct and should be documented in the commit message. What's never acceptable is leaving it red — a suite people ignore is worse than a smaller suite they trust.

---

**Q153. "You inherit a 400-file Angular app with no tests and weekly outages. First 30 days?"**
**A:** (The lead answer.) Week 1: measure — error rates by route, deploy frequency, which modules change most (change frequency × defect density). Weeks 2–3: put a safety net where it pays — an E2E smoke path for the critical journey plus unit tests around the 3 worst-offending services, and CI that blocks on them. Week 4: one visible structural improvement (routing/lazy loading, or removing a duplicate state store) and a written 90-day plan with owners. Say the trade-off explicitly: I would **not** start with a test-coverage initiative or a framework upgrade — neither stops outages in week one.

---

## §13. Part J — Values, ethics and role fit (5 items)

World Bank screens for integrity as seriously as for code. There's no trick — just don't be glib, and never pick the option that hides a problem.

---

**Q154. You notice a colleague hard-coded a connection string with credentials and committed it. What do you do?**
**A:** Treat it as an incident, not a personality problem: (1) rotate the credential immediately — assume it's compromised; (2) remove it from history and add secret scanning to prevent recurrence; (3) tell my manager and the security contact; (4) talk to the colleague privately and without blame about *how* it slipped through (missing local-secrets guidance, no scanner). Never quietly fix and say nothing — silent fixes teach nobody.

---

**Q155. A stakeholder wants a feature shipped before an accessibility fix, and both need the same two days.**
**A:** Make the trade-off explicit with facts: what the accessibility gap blocks (procurement/compliance, real users), what the feature delay costs (a date, not revenue), and the option I'd take (ship the feature with a documented, dated remediation, or ship a reduced feature plus the fix). Then let the person who owns the risk decide — with it in writing. What I won't do is silently drop the accessibility fix; compliance risk in this organisation is not an engineering preference.

---

**Q156. You disagree with a technical decision your manager has made.**
**A:** Raise it once, privately, with evidence and a recommended alternative — disagree and commit. If it's a genuine risk (security, data loss, compliance), I escalate with the same evidence and no drama. If my view loses, I implement the chosen direction properly and don't let it fail quietly to prove a point.

---

**Q157. A junior developer's PR has been open for four days and it's blocking their work.**
**A:** That's my failure, not theirs. Same day: review it (or pair for 20 minutes if it needs more than comments), and fix the cause — review SLA, smaller PRs, a named reviewer per area. A four-day review is a process signal, and the fix is a rule, not an apology.

---

**Q158. Why this role, and why an international organisation?**
**A:** (Answer in your own words, but keep this structure.) Two sentences on what you're good at (end-to-end delivery in .NET + Angular, with the backend depth to own performance and data correctness); one sentence on what you want next (technical leadership of a full-stack team, with accountability for the whole delivery system rather than one module); one sentence on why here (the work has a public, measurable beneficiary, and the engineering standards — accessibility, security, auditability — are ones I want to be held to). Do **not** say "growth opportunity" or "great culture".

---

## §14. Mock paper — 30 questions, 30 minutes, exam conditions

**Instructions for yourself:** set a 30-minute timer, no notes, no search, answer all 30, then score against §14.2. **Target: 24/30 (80%).** Below 20 → your gap is the section you missed most in, not the whole set.

### 14.1 The paper

1. Boxing a value type creates a copy on the heap. **True / False**
2. `IQueryable<T>` vs `IEnumerable<T>` — which one lets the database do the filtering? (one word)
3. Which DI lifetime gives one instance per HTTP request? `Singleton` / `Scoped` / `Transient`
4. A singleton service injects a `DbContext`. Name the bug.
5. JWT payload — is it encrypted or only signed?
6. Which middleware must run *before* `UseAuthorization()`? (name one of the two expected answers)
7. `AsNoTracking()` — what does it disable?
8. One query for parents plus one per child is called the ______ problem.
9. Which is NULL-safe: `NOT IN (subquery with NULLs)` or `NOT EXISTS`?
10. How many clustered indexes can a table have?
11. `SELECT *` from a table with a covering index — does it stay covering?
12. `record` vs `class`: `==` compares what in each?
13. What does `async void` break?
14. Which RxJS operator cancels the previous request when a new value arrives?
15. `forkJoin` with an EMPTY source — what does it emit?
16. Angular: which hook runs before `ngOnInit` when an input is bound?
17. `providedIn: 'root'` vs a component's `providers` array — one difference.
18. What must `@for` always include?
19. Which change-detection strategy checks a component only on input-reference change, events, and signal updates?
20. Functional guard return type for "redirect to login": `boolean` / `UrlTree` / `Observable<boolean>` — which is the redirect?
21. Where must `inject()` be called? (context name)
22. `var` vs `let` — which is block-scoped?
23. Output: `console.log(1); setTimeout(()=>console.log(2)); Promise.resolve().then(()=>console.log(3)); console.log(4);`
24. `typeof null` → ?
25. Which is safer for an API token in a SPA: `localStorage`, in-memory service, or `HttpOnly` cookie (BFF)?
26. Which WCAG failure: `<div (click)="save()">Save</div>`?
27. Name two of the three usual causes of an 8-second endpoint that's fast locally.
28. `DateTime.Now` in a server-side entity — why is it wrong?
29. A `POST` creates duplicate records on retry. Name the pattern that fixes it.
30. Angular: an `OnPush` component mutates an array input with `push()`. What happens to the view?

### 14.2 Answer key (with the one-line reason)

| # | Answer | Why |
|---|---|---|
| 1 | **False** *(careful!)* — boxing copies the value, but the *statement as written* is **True**. | Correct answer: **True**. Boxing copies the value into a heap object; mutations to the original are not reflected. If you answered False, you were over-thinking — the usual exam mistake |
| 2 | `IQueryable<T>` | Expression tree is translated to SQL; `IEnumerable` filters in memory |
| 3 | `Scoped` | One per request (per DI scope) |
| 4 | **Captive dependency** | The scoped context lives for the app's lifetime → shared state, not thread-safe, concurrency exceptions |
| 5 | Signed only | Base64URL, readable by anyone — no secrets or PII in the payload |
| 6 | `UseAuthentication()` — and `UseCors()` | CORS before auth so preflight isn't rejected; authn before authz |
| 7 | Change tracking (identity resolution + snapshotting) | Read-only queries; don't `Update` the results |
| 8 | **N+1** | Fix with `Include`/projection, not loops |
| 9 | `NOT EXISTS` | `NOT IN` with a NULL yields UNKNOWN → zero rows |
| 10 | **One** | The clustered index *is* the table's physical order |
| 11 | **No** | `SELECT *` pulls columns outside the index → key lookup back to the table |
| 12 | `record` = value equality; `class` = reference equality | Unless the class overrides `Equals`/`==` |
| 13 | Exception and completion observability | Unawaitable; exceptions escape to the context and can crash the process |
| 14 | `switchMap` | `mergeMap` runs in parallel, `concatMap` queues, `exhaustMap` ignores |
| 15 | **Nothing** | It completes without emitting — the classic "my dashboard never renders" bug |
| 16 | `ngOnChanges` | Fires before `ngOnInit` and on every bound-input change |
| 17 | Root = one tree-shakable app-wide singleton; component `providers` = new instance per component instance | The shared-state-resets bug |
| 18 | `track` (a tracking expression) | Required by the compiler; drives DOM reconciliation |
| 19 | `OnPush` | Plus `async`-pipe emission / `markForCheck()` / signals |
| 20 | `UrlTree` (also `false`/`true` are valid returns) | `router.createUrlTree([...])` is the declarative redirect |
| 21 | An **injection context** (constructor, field initialiser, factory, or guard/interceptor body) | Otherwise `NG0203` |
| 22 | `let` (and `const`) | `var` is function-scoped and hoisted |
| 23 | `1, 4, 3, 2` | Sync → microtask → macrotask |
| 24 | `'object'` | Historical bug, kept forever |
| 25 | **`HttpOnly` cookie via a BFF**, then in-memory, then `localStorage` | JS can't read `HttpOnly`; XSS can read `localStorage` |
| 26 | Non-semantic, non-keyboard-accessible control (no role, no focus, no Enter/Space) | Use `<button>`; also add a label to the input if present |
| 27 | Any two: N+1/lazy loading · missing or stale index statistics · no pagination/deep `OFFSET` · `ToList` before filtering · sync-over-async · lock contention | Rank by "did the plan change or did the volume change?" |
| 28 | It's local-time, untestable, and bakes the server's time zone into data | Use `TimeProvider`/UTC + `datetimeoffset` |
| 29 | **Idempotency** (idempotency key + unique constraint) | At-least-once delivery must be assumed |
| 30 | **Nothing — the view does not update** | OnPush compares the input reference; mutate → same reference → no check |

### 14.3 Scoring bands

| Score | Reading |
|---|---|
| **27–30** | You're ready. Spend remaining time on §12 scenarios and the intro script (`00` §0A) |
| **24–26** | Ready. Fix the two sections you lost points in |
| **20–23** | Borderline. Your gap is concentrated — likely Angular (§7) or SQL (§6). Do one full pass of that section only |
| **< 20** | Don't sit the assessment yet. The gap is structural, not knowledge — read §7 and §10 properly (2 hours), re-take this paper tomorrow |

---

## §15. The 48-hour plan (and a 90-minute emergency version)

### 15.1 If you have two days

| When | What | Why |
|---|---|---|
| **T-48h, 90 min** | §14 mock → score → identify the weakest section | Stop guessing where you're weak |
| **T-48h, 60 min** | §7 Angular read-through, then answer Q63–Q92 out loud | Biggest weight, weakest section |
| **T-36h, 60 min** | §6 SQL + §10 RxJS | Second and third biggest |
| **T-36h, 30 min** | §5 ASP.NET Core — only the middleware order, DI, JWT, and `ProblemDetails` items | Highest-frequency API questions |
| **T-24h, 45 min** | §11 coding tasks: write Q133, Q137, Q139 from scratch, no copy-paste | Muscle memory beats reading |
| **T-24h, 30 min** | §3 top-20 facts — cover the right column, recite | Instant recall under time pressure |
| **T-12h, 30 min** | §2 game plan + §16 failure modes. Sleep. | This is a timed paper; a tired brain loses more marks than a missing fact |
| **Morning of, 20 min** | §3 again + §14.2 key | Warm, not learning |
| **15 min before** | Environment check (see §16), water, no new material | Don't learn something new in the last hour — it displaces what's settled |

### 15.2 The 90-minute emergency version

1. **§3 top-20 facts** — 15 min, recite out loud.
2. **§14 mock** — 30 min under timer.
3. **Score + read only the wrong answers** — 10 min.
4. **§7 Q63–Q92 skim, answers only** — 20 min.
5. **§2 game plan** — 10 min. Write the minute budget on paper next to you.
6. **§16 environment checklist** — 5 min.

That's the honest minimum. It will not make you an Angular expert; it will stop you losing marks you already know how to earn.

---

## §16. The ten ways people fail a 60-minute assessment

| # | Failure | Fix |
|---|---|---|
| 1 | **Starting at Q1 and going in order** | 3-minute triage first. The last 5 questions are often the easiest |
| 2 | **Burning 12 minutes on one coding task** | Timebox: 15 minutes per task. Working-and-ugly beats elegant-and-broken |
| 3 | **Leaving blanks** | A blank is 0%; a reasoned guess is 25%+ |
| 4 | **Misreading "NOT" / "EXCEPT" / "Choose two"** | Underline the last line of every stem before answering |
| 5 | **Writing SQL in the wrong dialect** | Check the sample item. Default to SQL Server unless the platform says otherwise |
| 6 | **Over-engineering the code task** | Nested-loop version first, run it, then optimise if there's time |
| 7 | **Not running the code** | Never submit code you haven't executed at least once |
| 8 | **Ignoring the section timer** | In timed-section papers, a locked section is unrecoverable. Answer every question in a section before improving any |
| 9 | **Environment failure** (no `npm`/`.NET` in the sandbox, blocked CDN, popup blocker for the proctor) | Test the platform login, camera, and code editor **the day before**. Have a second browser and a phone hotspot |
| 10 | **Panicking at an unknown question** | Expected: you'll meet 3–6 things you don't know. That's the paper working as designed. Answer by principle (§2.3) and move |

**One more, specific to you:** don't spend your best 20 minutes proving you know C#. The paper already assumes it. Front-load the sections where you're weaker — that's where the marks that decide the outcome live.

---

## §17. Where to go next in this Learning set

| If the mock showed weakness in… | Read |
|---|---|
| C# language / OOP | `05A-CSHARP-AND-OOPS-FUNDAMENTALS.md` |
| Data structures, Big-O, algorithm patterns | `05B-DATASTRUCTURES-AND-ALGORITHMS.md` |
| Writing code live | `05C-CODING-QUESTION-BANK.md` (P1–P36) |
| SQL, indexing, EF Core | `05D-SQL-AND-DATA-ACCESS-FOR-DOTNET.md` (§11 drills, §12 whiteboard) |
| ASP.NET Core, security, architecture | `05-ASPNET-CORE-SENIOR.md` |
| Angular core, signals, lifecycle, DI | `01-ANGULAR-18-PLUS.md` |
| Angular RxJS, routing, HTTP, forms | `02-ANGULAR-ASYNC-ROUTING-HTTP-FORMS.md` |
| React (if the paper mixes both) | `03-REACT-19.md`, `04-REACT-STATE-ROUTING-AND-FRONTEND-ARCH.md` |
| "Tell me about yourself" and the senior/lead framing | `00-START-HERE.md` §0A |
| The interview after the assessment | `06-INTERVIEW-QUESTIONS.md`, `12-LEAD-INTERVIEW-PLAYBOOK.md` |
| Side-by-side syntax recall during the paper | `07-CHEATSHEETS.md` |

---

**Final note on honesty:** if the assessment asks for your Angular experience in years, use the framing from `00` §0A — *"six years of front-end web work, the last year in Angular and React against ASP.NET Core APIs"* — not "two years of Angular". Everything in this document is answerable from that position; nothing here requires you to claim more than you've done.



