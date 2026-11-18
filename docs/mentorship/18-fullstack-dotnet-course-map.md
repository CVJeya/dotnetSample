# 18 — The Full-Stack .NET Course Map

**C#, .NET, design patterns & principles, architecture, Azure Functions, Azure, AWS, Key Vault, cloud deployment, React, Angular, and four databases — mapped to courses, with an honest 16-week plan.**

> Companion to doc 17 (free learning path). Doc 17 answers *"what can I learn for free?"*. This doc answers a harder question: *"I want to be hireable/promotable as a senior .NET developer with cloud and full-stack range — what do I actually buy, what do I ignore, and in what order?"* It is opinionated on purpose. Where a claim is a judgement rather than a measurement it says so; where a price or a module list is involved it carries **[verify]**.

---

## §0 How to use this document

### §0.1 Who this is written for

| If you are… | Read |
|---|---|
| A mid-level .NET developer aiming at senior | §2 → §3 → §8 (the plan). Skip the rest on first read. |
| Evaluating *which* subscription to buy this month | §3, §4, §6, §11 |
| Being told to "learn Azure and AWS" and unsure what that means in practice | §7 (certifications), §8 weeks 10–12 |
| Strong in .NET, weak in frontend | §5.2 then §8 weeks 13–14 |
| The person in your team who gets the Oracle ticket | §9.4 — and read the correction there carefully |
| Preparing for a senior interview next month | §8 "what to skip", §13 |

### §0.2 Honesty box — read this before spending money

Three uncomfortable truths drive every recommendation below:

1. **No single course covers your list.** The list spans at least six disciplines that each have their own 30-hour learning path, two competing cloud vendors, and a database ecosystem where one member is a commercial product. Anyone selling you "the complete full-stack .NET masterclass" in 60 hours is selling a *catalogue of highlights* — you will recognise the vocabulary of fourteen technologies and be able to use none of them under pressure.
2. **Breadth is not the senior signal.** Senior interviews test depth in three or four areas plus judgement about the rest. Your list is a **job-description defensiveness list**, not a competence target. §10 turns it into five artefacts you can show.
3. **Courses expire faster than your attention.** Specific APIs (Azure Functions isolated worker, EF Core providers, React data-fetching idioms, Angular standalone components) have all changed materially in the last two years. A 2021 course is not "80% useful" — it teaches patterns you must then unlearn. This is why §3 grounds everything in your *own* repo's versions and §6 gives you named sources that track the current surface.

### §0.3 Acceptance criteria for this document

- [ ] I can name the **one** purchase I am making and the **one** I am deferring, and say why.
- [ ] I can state which three technologies from my list are **not** in my current codebase, and therefore need a built-and-deployed artefact to count.
- [ ] I have a week-by-week plan with a **deliverable** per phase, not a viewing list.
- [ ] I know which certifications I am targeting and which I am deliberately skipping.
- [ ] I know the total spend and the cancellation date.

---

## §1 The requirement, restated as a table

Your request, decomposed. "Coverage" is what a course *can* give you; "retention" is what you keep without a project.

| # | Skill | Depth a senior needs | A course can give coverage? | Retained without a project? |
|---|---|---|---|---|
| 1 | C# language | Deep: generics, variance, `async`, spans, records, pattern matching | Yes | Partly |
| 2 | .NET platform | Deep: DI lifetimes & scopes, configuration, hosting, GC basics | Yes | Partly |
| 3 | Design patterns & principles | Deep: SOLID *applied*, GoF patterns as vocabulary, not goals | Yes | **No** — needs a codebase to refactor |
| 4 | .NET architecture | Deep: Clean/hexagonal, DDD tactical patterns, modular monolith vs microservices, trade-off reasoning | Yes (but courses overstate microservices) | **No** — needs a real bounded context |
| 5 | Azure Functions | Working: triggers, bindings, isolated worker, Durable Functions | Yes | Partly |
| 6 | Azure | Working-wide: App Service, Container Apps, Storage, Service Bus, Entra ID, monitoring, cost | Yes | Partly |
| 7 | AWS | Working-wide: IAM, Lambda, S3, RDS, SQS, CloudWatch | Yes | Partly |
| 8 | Key Vault / secrets / identity | Working: managed identity + `DefaultAzureCredential`, no secrets in config | Yes | **No** — needs a deployed app to prove it |
| 9 | Cloud deployment | Deep: CI/CD gates, slots, rollback, infrastructure as code, observability | Partly | **No** — needs a broken deploy you fixed |
| 10 | React | Working-deep: state, data fetching, testing, auth | Yes | **No** |
| 11 | Angular | Working-deep: same list, different idioms | Yes | **No** |
| 12 | SQL Server | Deep: indexes, execution plans, isolation levels, EF Core translation | Yes | Partly |
| 13 | PostgreSQL | Working: as a contrast dialect + its ecosystem | Yes | Partly |
| 14 | MySQL | Working: enough to be productive | Yes | Partly |
| 15 | Oracle | Working: enough to survive an enterprise ticket | **Poorly** (see §9.4) | **No** |

The five rows marked **No** are where every course promise quietly fails, and where §10 puts the artefacts.

---

## §2 Why no single course exists — the structural reasons

Worth understanding, because it stops you from hunting for a mythical product.

**2.1 Vendors are identity-bound.** Microsoft's own learning estate (Learn, Pluralsight's Microsoft paths, Microsoft Press) is deep on Azure and thin-to-absent on AWS — Microsoft is not going to teach you Lambda. AWS's estate is the mirror image. A "both clouds" course is invariably shallow on both because the instructor cannot have production scars in both.

**2.2 Frontend moves faster than course runways.** A course records in ~3 months, publishes after review, and is stale in 18. React's recommended data-fetching story and Angular's standalone/signals migration both invalidated earlier teaching. Prefer sources that publish continuously (§6.3) for frontend.

**2.3 Database vendors differ in licensing, not just SQL.** SQL Server, PostgreSQL and MySQL can be taught in one course because their *skills transfer*: set-based thinking, indexes, plans. Oracle transfers at the SQL level but diverges hard at tooling, optimizer behaviour, data types, and enterprise plumbing (DB links, RAC, tablespaces) — and Oracle's own licensing makes a sandbox course awkward to produce.

**2.4 Architecture cannot be taught in a vacuum.** Every good architecture course needs a business domain with real constraints. That is why the best ones look like case studies, and why you must *apply* one to your own codebase (§8 week 4–6).

**2.5 Your list is really three careers.** "Senior .NET developer" + "cloud engineer" + "full-stack developer" + "DBA-ish". Seniors are T-shaped: one deep pillar, wide arms. Decide your pillar now — for this repo and this mentorship it is **ASP.NET Core + enterprise integration + Oracle-backed .NET**, with cloud breadth as arms.

---

## §3 The anchor: one subscription that spans the map

**Recommendation: Pluralsight**, as the closest single purchase to "all of the above with hands-on labs".

### §3.1 Coverage matrix

| Your requirement | Pluralsight | Notes |
|---|---|---|
| C# | ✅ Paths, current-ish (C# 12/13 features covered on release cadence) | Skip the beginner path if you already ship C# |
| .NET / ASP.NET Core | ✅ | Prefer the *internals* and *performance* courses over "build a CRUD app" |
| Design patterns & principles | ✅ Strong | Look for applied refactoring courses, not GoF catalogue tours |
| .NET architecture | ✅ Strong (Clean Architecture, DDD, modular monolith, microservices) | Courses overstate microservices — see §5.4 |
| Azure Functions | ✅ Includes isolated worker model | Verify the course covers **isolated** (in-process is deprecated) |
| Azure | ✅ Wide | Pair with Microsoft Learn for anything that must actually deploy |
| **AWS** | ✅ Wide | The reason Pluralsight wins the "one purchase" case |
| Key Vault / managed identity | ✅ Inside security + Azure developer paths | Usually under-taught; Microsoft Learn is better (§4.1) |
| Cloud deployment / CI-CD | ✅ | Generic; your GitLab Windows-runner + IIS reality is not covered by anyone (§3.2) |
| React | ✅ Current | Deep enough to start, not enough alone |
| Angular | ✅ Current | Same |
| SQL Server / PostgreSQL / MySQL | ✅ | Good fundamentals, light on production tuning |
| Oracle | ❌ **Absent** | See §9.4 |
| **Hands-on cloud without a credit card** | ✅ **Cloud Sandboxes** | The decisive feature: real Azure/AWS consoles, time-boxed, no billing risk |

### §3.2 Where it is weak, stated plainly

- **It is broad by design, so it is shallow per-topic.** The architecture path will not make you an architect; it makes you able to *follow* an architecture conversation and pick the next book.
- **Labs are sandboxed, not production-shaped.** No course will teach you the thing you actually need: deploying to an **IIS app pool on a Windows runner via MSDeploy** with slots and a rollback plan — which is exactly what `.gitlab-ci.yml` in this repo does. That skill comes from doing it (§8 week 10) and from doc `CICD-DEVOPS.md` §7.4/§8.1 in the architecture set.
- **Oracle is a hole.** Assume zero coverage.
- **Nothing here teaches you to read a 715 KB service file.** See §5.3.

### §3.3 Cost and the cancellation rule

- ~USD 29–39/month depending on plan and region, with a free trial that is genuinely enough to audit a path. **[verify current price and trial length before subscribing]**
- **Rule: subscribe for a defined window (2–3 months max), then cancel.** The failure mode of subscription learning is paying for 14 months and finishing two courses. Set a calendar reminder on day 1.
- Check whether your employer has a licence — several enterprises do, and it is free to ask.

---

## §4 The two supplements (both cheaper than they look)

### §4.1 Microsoft Learn — free, and *authoritative* for the cloud half

Not a course, a documentation estate with learning paths and free sandboxes. Its value over any paid course is **precision**: when Pluralsight says "use managed identity", Learn gives you the exact `az` command, the exact RBAC role name, the exact Bicep snippet that works **this month**.

Use it for: Azure Functions isolated worker, Key Vault + `DefaultAzureCredential`, App Service slots, Entra ID app registrations, and **AZ-204 exam prep** (the exam objectives page is the best free syllabus for "Azure for .NET developers" that exists).

Non-obvious prerequisite on your machine: the keyless `DefaultAzureCredential` path needs the **Azure CLI (`az`)** for local dev sign-in — and `az` is **not installed** here (verified). Every "just use managed identity locally" tutorial fails silently until you install it. This is recorded as a real gap in doc 13 §1.6 and §5.

### §4.2 Dometrain — the .NET *judgement* supplement [verify current catalogue and price]

Nick Chapsas's platform. Why it earns a slot next to Pluralsight: Pluralsight teaches patterns **generically**; Dometrain teaches the things a senior reviewer at a .NET shop actually marks you on——

- DI lifetimes and the captive-dependency bug class,
- `async`/`await` correctness (`.Result`, `Task.Run` misuse, `ConfigureAwait`, `ValueTask`),
- EF Core: tracking vs `AsNoTracking`, N+1, compiled queries, translation failures,
- minimal APIs vs controllers: when each is right,
- performance: allocations, `Span<T>`, pooling — with benchmarks rather than assertions.

That gap — between "knows SOLID" and "won't ship the N+1" — is precisely the senior threshold. ~EUR 30/month **[verify]**.

**If you buy exactly one and care about .NET depth over cloud breadth: buy Dometrain and accept the AWS/React gaps.**
**If you buy exactly one and need the whole map: buy Pluralsight and accept the depth gap.**

---

## §5 Three genuine gaps — and what to do instead of a course

### §5.1 Oracle — the correction this document owes you

My earlier verbal advice was "don't study Oracle speculatively". **That advice was wrong for your situation**, and the code proves it. Verified from this repository:

| Fact | Evidence |
|---|---|
| The production ORM is **Oracle**, not SQL Server | `LHDN_STSDS/LHDN_STSDS.csproj` → `Oracle.EntityFrameworkCore 9.23.80` |
| The main `DbContext` is registered against Oracle | `Program.cs` → `options.UseOracle(...)`, connection string key `OracleDb` |
| DbContext class | `LHDN_STSDS/Data/AppDbContext.cs:26` → `public class AppDbContext : DbContext` |
| A second context for STAMPS reached via a **database link**, not a second connection | comment at the `AddDbContext` calls: "registered for STAMPS" over `EDS_LINK` |
| **Dapper** is also in the dependency set | `LHDN_STSDS.csproj` (raw-SQL path alongside EF) |

So Oracle is not a speculative skill here — it is the daily database. Revised guidance:

- **Learn Oracle as a first-class dialect** if you intend to stay in this codebase or this sector (Malaysian e-invoicing / LHDN integrations are Oracle-heavy by procurement habit). Pluralsight will not teach it; use **Oracle's own free documentation and Oracle University's free-tier material [verify]**, plus your own database.
- **The trap to learn first** is not `SELECT` — it is EF Core *translation* against Oracle: identifier casing and quoting (`"TABLE"."COLUMN"`), `NUMBER`/`DATE` type mapping, sequences vs identity, `VARCHAR2` byte-vs-char semantics, and why a LINQ query that works on SQL Server throws or silently slow-paths on Oracle. That is a two-day investigation on a real schema and worth more than any course.
- **Version skew to be aware of (real, in this repo):** `Oracle.EntityFrameworkCore 9.23.80` at runtime while `Microsoft.EntityFrameworkCore.Tools` is **10.0.6** — design-time tooling a major version ahead of the runtime provider. Migrations can then generate SQL the runtime provider rejects. If you hit "migration scaffolding oddity", check this pair before blaming your model. [Inspected in `LHDN_STSDS.csproj`; confirm intent with the team — it may be deliberate.]

### §5.2 React vs Angular — pick one, read the other

Learning both at survey depth is the most common senior-candidate mistake: two shallow halves instead of one whole. Decide by target, not taste.

| | React | Angular |
|---|---|---|
| Job overlap with .NET backends | Broad, especially product companies | Strong in enterprise/regulated shops — your sector |
| What "knowing it" means | Components, hooks rules, state placement, server-state library, testing, auth flows | Components, RxJS, DI, routing, forms, signals migration, testing |
| Failure mode to avoid | "I did the tutorial and built a to-do app" | Same, plus clinging to NgModule-era patterns |
| Minimum credible artefact | A React app calling **your** .NET API with token auth, deployed | Same, in Angular |

**Recommendation for this mentorship: React** — it maximises transferable job overlap, and the React skills map more cleanly onto the Razor/JS you already touch. **Your repo does not use either** (verified): 281 `.cshtml` views, and `package.json` has exactly **one** devDependency, `prettier`. So this is genuinely new ground and must be proved with an artefact (§10), not a certificate.

### §5.3 Your own repo says what "architecture" means here — and it is not Clean Architecture

Verified reality of `LHDN_STSDS`:

| Dimension | Actual | Implication for your learning |
|---|---|---|
| Web stack | ASP.NET Core MVC + Razor views (**281** `.cshtml`), plus Blazor Server registered | Framework for your "architecture" practice is MVC, not minimal-API-first |
| Controllers | **57** | Boundary design matters more than pattern names |
| Projects | **1** csproj (`net8.0`), 24 package references | You are *inside* a modular monolith — the realistic target (§5.4) |
| Tests | **Zero test projects** | The first test project you add is itself a portfolio artefact |
| Size | 754 `.cs` files, 11.3 MB; `AmendmentServices.cs` is **715,917 B** (≈179k tokens) | "Clean architecture" courses assume 20-file samples. Your skill is *containment* |
| Background work | Hangfire 1.8.22 (+ `Hangfire.InMemory` 1.0.0), SignalR `BulkProcessingHub` | Distributed-work patterns are already in play |
| Deploy | `.gitlab-ci.yml` build+deploy on a **Windows runner** → `bin\Release\net8.0\publish` → IIS `C:\inetpub\eds-api`, AppPool `TestAppPool` | Your "cloud deployment" skill is **MSDeploy + slots + rollback**, and later containers |
| Secrets today | `appsettings.json` holds a live API key for the AI service (see doc 14 §2.2) | Key Vault is not an academic topic here — it is a fix you should ship |
| Not present | React, Angular, Azure Functions, Key Vault, containers | Those four are your *sandbox* skills until deployed |

**Practical use of the god files:** take `AmendmentServices.cs` (715 KB) and practise the only technique that scales — extract one cohesive responsibility behind an interface, add a test around it, keep the diff reviewable. That single exercise teaches more architecture than a 40-hour DDD course, and it is exactly what doc 15 §6 (the L0–L4 comprehension ladder + outline-then-range) exists to support.

### §5.4 Treat microservices courses as *one* option, not the goal

Almost every architecture path treats microservices as the destination. For a single-team, Oracle-backed, LHDN-integrated monolith, the honest senior position is: **modular monolith with enforced boundaries first**; extract a service only when a specific force demands it (independent scaling, separate release cadence, isolation of a failing subsystem). Doc `MICROSERVICES-MIGRATION.md` §11 ("when to stop") already encodes this. Say that sentence in an interview and you sound experienced; say "microservices" and you sound like you finished a course.

---

## §6 Alternatives to the anchor, ranked

If Pluralsight's price or its shallowness-per-topic bothers you, these are the real substitutes. Judgements, not measurements.

| Option | Best for | Strengths | Weaknesses | Cost |
|---|---|---|---|---|
| **Pluralsight** | Whole map + sandboxes | Breadth, both clouds, all three DBs, React+Angular, hands-on sandbox | Shallow per topic, no Oracle | ~$29–39/mo **[verify]** |
| **Dometrain** | .NET depth | Modern, opinionated, senior-level judgement, benchmark-driven | Narrow cloud/frontend coverage | ~EUR 30/mo **[verify]** |
| **Udemy (cherry-pick)** | One specific hole, cheaply | Targeted courses (e.g. a strong Angular course, a Functions course) | Quality varies wildly; stale copies persist; no labs | $10–20 on sale **[verify]** |
| **Frontend Masters** | React/Angular depth | Genuinely deep JS/frontend instruction | No .NET, no cloud | ~$39/mo **[verify]** |
| **A Cloud Guru / Pluralsight Cloud** | Cert-focused cloud | Labs + exam alignment | Weaker .NET depth | ~$35–49/mo **[verify]** |
| **Whizlabs / Tutorials Dojo** | Exam practice exams | Best-in-class practice tests | Not a learning path | $10–20 per exam bank **[verify]** |
| **Microsoft Learn** | Azure authority | Free, current, exact | Not linear; assumes self-direction | Free |
| **Books** | Depth that doesn't expire | See list below | Slow, no labs | Varies |
| **YouTube (Chapsas, Milan Jovanović, Nick's .NET/architecture series)** | Current idioms & architecture reasoning | Free, current, opinionated | Not structured; no labs | Free |

**Books worth the time (architecture & principles do not go stale like APIs):**

- *Dependency Injection Principles, Practices, and Patterns* — Seemann & van Deursen (the DI book that makes lifetimes stick)
- *Patterns of Enterprise Application Architecture* — Fowler (vocabulary you will reuse for 20 years)
- *Domain-Driven Design* — Evans; and *Implementing DDD* — Vernon (tactical patterns with code)
- *Clean Architecture* — Martin (principles, not the folder diagram — the folder diagram is cargo cult)
- *Designing Data-Intensive Applications* — Kleppmann (the book that separates seniors on data reasoning)
- *ASP.NET Core in Action* — Lock (the framework, accurately)

---

## §7 Certifications — the shortest honest ladder

### §7.1 Target: AZ-204 (Azure Developer Associate)

The highest-value certification **for a .NET developer specifically** — it is literally "develop Azure compute, storage, security and monitoring solutions with .NET". Its objectives list doubles as your Azure syllabus. Cost ~$165 **[verify regional pricing]**; Microsoft Learn prep is free; Whizlabs/TD practice tests are worth the $15.

### §7.2 Second: AWS Certified Developer – Associate (DVA-C02)

Only after AZ-204, and only if AWS appears in job descriptions you actually want. ~$150 **[verify]**. Learning two clouds with one certification is the efficient order: the second cloud is 60% "the same idea, different nouns".

### §7.3 Skip: Oracle certifications

Oracle certs are expensive, tied to release versions, and rarely filtered-for in .NET hiring. **Learn Oracle (see §5.1); do not certify it.** Same for "certified scrum master"-style credentials unless a specific role demands them.

### §7.4 What certifications actually do

| They do | They do not |
|---|---|
| Clear HR filters | Prove you can design anything |
| Force coverage of topics you'd skip (monitoring, cost, security) | Substitute for a deployed artefact |
| Give you a deadline | Survive an interview question like "why did you choose this over that?" |

**Positioning rule:** one cloud cert + three deployed artefacts beats four certs and no deployments. Verified-by-deployment is what §10 is for.

---

## §8 The 16-week plan (~10 h/week)

Built around **one anchor + one depth supplement**, and front-loaded so the artefacts exist early enough to be improved.

| Weeks | Focus | Deliverable (the thing that proves it) |
|---|---|---|
| 1–3 | **C# depth**: `async` correctness, generics/variance, spans, records, pattern matching. Supplement: DI lifetimes & scopes. | A refactor of one repo service: remove every `.Result`/`.Wait()`, fix a lifetime bug, add tests. Measured before/after. |
| 4–6 | **Design patterns + principles applied**: SOLID as *review criteria*, GoF as vocabulary, refactoring discipline. | Second pass on `AmendmentServices.cs`: extract one cohesive responsibility behind an interface; keep the diff under 400 lines; test it. |
| 7–9 | **ASP.NET Core internals + EF Core performance** (Oracle dialect — §5.1): middleware order, filters, model binding, tracking vs `AsNoTracking`, N+1, plans. | A measured query improvement on a real table: before/after timings + the execution plan + why. |
| 10–11 | **Azure**: App Service / Functions (isolated), Key Vault + managed identity, slots, CI/CD, monitoring, cost guardrails. | Deploy a small app with **zero secrets in config** (`DefaultAzureCredential`), two slots, and a working rollback. Install `az` first. |
| 12 | **AWS**: IAM, Lambda, S3, RDS, SQS, CloudWatch. | The same app's equivalent deployed on AWS; write the one-page "what differs and why". |
| 13–14 | **React** (chosen in §5.2): state, server-state, testing, auth. | A React app against **your** .NET API with token auth, deployed to a public URL. |
| 15–16 | **Databases**: SQL Server tuning (indexes, plans, isolation), PostgreSQL as a contrast, MySQL basics; Oracle translation traps. | Migrations + indexing exercise + one query optimised in three dialects, with a table of differences. |
| Ongoing | **AZ-204**, then optionally DVA-C02. | Exam pass; use the objectives page as the syllabus. |
| Continuous | **Interview reps** (§13). | Two rehearsed stories per week, written down. |
| Optional | **Fundamentals in C** (§8.3) — deliberately off the critical path | Four containers built by hand, ported to C#, benchmarked against the BCL, plus the "why the BCL wins" write-up. |
| Optional | **Prompting AI to build and fix a web app** (§8.4) — off the critical path | One real repo bug fixed through the agent loop, failing test first and diff reviewed, plus a 20-task eval log with success rate and failure classes. |

### §8.1 What to skip, deliberately

- **Beginner C# / "build your first web app"** — you ship ASP.NET Core for a living.
- **Angular** (for now) — §5.2. Read it later; do not split focus in weeks 13–14.
- **Microservices deep-dives** — §5.4. Learn the *forces*, not the topology.
- **Oracle certification** — §7.3.
- **Any course whose sample is a to-do list.** You learn nothing transferable from a domain with no constraints.
- **MongoDB/NoSQL** — not on your list, not used here; resist the detour.

### §8.2 If you only have 4 h/week

Compress to: weeks 1–3 C#/DI → weeks 4–6 architecture on the real file → weeks 7–9 EF Core/Oracle → weeks 10–12 Azure (Key Vault + deploy). **Drop AWS and frontend entirely** for now. Two deployed artefacts and a real refactor beat six half-touched topics.

### §8.3 Optional fundamentals track — data structures in C (~24 h, off the critical path)

An honest framing first: **nothing in this track is required for a senior .NET interview, and no job description on your list asks for C.** It is here because it closes a specific weakness that courses do not fix — you use `Dictionary<K,V>`, `List<T>` and `SortedSet<T>` correctly without being able to derive their costs from first principles.

What C makes visible that .NET hides:

| Hidden in .NET | Visible in C | Why a senior interview cares |
|---|---|---|
| Capacity doubling inside `List<T>` | the `realloc` you call yourself, and the copy you pay for | explains amortised O(1) `Add`, and why pre-sizing matters |
| Chaining inside `Dictionary<K,V>` | open addressing vs chaining, load factor, tombstones | lets you answer "why is this lookup slow?" instead of guessing |
| B-tree indexes inside the database | a tree you wrote, node by node | explains why DB indexes are B-trees, not binary trees (§9 item 1) |
| GC pauses and object headers | explicit `malloc`/`free`, cache-line layout | explains why `Span<T>` and structs exist at all |

**Do it only if** you cannot already answer, from memory: what a red-black insert rotation does, and why a hash table degrades from O(1) to O(n). If you can answer both, skip this section — it is depth, never a prerequisite.

**Books — one anchor, one hands-on, one prerequisite**

| Role | Title | Why this one | Caveat |
|---|---|---|---|
| Anchor | **Weiss — *Data Structures and Algorithm Analysis in C*, 2nd ed.** | treats *analysis* as the subject and structures as the application; the amortised-analysis and hashing chapters transfer directly | textbook-dense; C89 code throughout |
| Reading | **Sedgewick — *Algorithms in C, Parts 1–5*, 3rd ed.** | cleanest C in the genre; Part 5 is the best graph-algorithms treatment in C | element types hidden behind `Item`/`eq`/`less` macros |
| Hands-on | **Loudon — *Mastering Algorithms with C*** | library-quality implementations with headers, error handling and Makefiles — you type these, you do not read them | proves "works", not "bounded"; pair it with Weiss |
| Prerequisite | **K.N. King — *C Programming: A Modern Approach*, 2nd ed.** | best teaching book for modern C; the memory model done properly | skip if your C is current |
| Free | CS50 weeks 0–5 · mycodeschool · Exercism C track | CS50 is taught *in C*; Exercism is the only free option with reviewed code | none of it substitutes for typing the containers yourself |

Skip **CLRS** as a learning book (reference only) and **Knuth, TAOCP Vol. 1** entirely — MIX/MMIX assembly, and not a tutorial.

**The .NET half — books and free sources**

The C books above explain *why* a hash table degrades. This table explains what the BCL already decided for you and what that decision costs. It is the other half of the same idea, and the material the port step below actually consumes.

| Role | Source | Why this one | Caveat |
|---|---|---|---|
| Practical | **Jamro — *C# Data Structures and Algorithms*, 2nd ed.** (Packt) | the current hands-on one: every structure in modern C# with complexity notes; cheapest route to the C# side of the port exercise | shows *how*, rarely *why this and not that*; thin on analysis |
| Rigorous | **Drozdek — *Data Structures and Algorithms in C#*, 3rd ed.** (Cengage) | formal complexity, AVL/red-black, sorting proofs — a real CS textbook using C# syntax | dated C# style and dry; expect to read four chapters, not fourteen |
| Gentle on-ramp | **McMillan — *Data Structures and Algorithms Using C#*** (Cambridge) | hand-holding through building each structure yourself | C# 2.0 era — pre-generics-heavy, no `Span<T>`; skim it, do not buy it new |
| **The actual senior book** | **Kokosa et al. — *Pro .NET Memory Management*, 2nd ed.** (Apress) | GC generations, object headers, allocation cost, cache behaviour — the real cost model sitting behind every choice above | not a data-structures book; you need the basics first |
| Measurement | **Watson — *Writing High-Performance .NET Code*, 2nd ed.** | what is actually fast versus what merely looks fast | complements the two above; replaces neither |
| Free · primary | **`dotnet/runtime` source — `List.cs`, `Dictionary.cs`, `PriorityQueue.cs`** | the authoritative implementation with the trade-offs stated in the comments; `Dictionary`'s single `Entry[]` plus `int[] buckets` design beats every book's explanation of it | you must know what you are looking for |
| Free · measured | **Stephen Toub — "Performance Improvements in .NET 8 / 9 / 10"** | long-form, benchmarked, and it explains *why* the BCL changed rather than only what changed | annual posts; read the collections sections, not all 200 pages |
| Free · reference | **Microsoft Learn — "Collections and Data Structures"**, plus each type's *Remarks* | states thread-safety, complexity and pitfalls explicitly | reference, not pedagogy |
| Free · practice | **BenchmarkDotNet** (with Akinshin, *Pro .NET Benchmarking*) | turns "I think this is faster" into a number — the instrument for the port step | the methodology book is optional |

Buy one, not five: **Jamro** for currency, **Drozdek** for depth, **Kokosa** if you are already past both (you likely are). Editions and .NET-version coverage move — **[verify]** that the edition you are buying covers `Span<T>`, `FrozenDictionary` and `PriorityQueue` before paying.

Then learn these, not linked lists — the facts actually tested in senior interviews and reviews:

- `List<T>`: capacity 0 → 4 → **doubling**, hence amortised O(1) `Add`, and why `new List<T>(n)` matters when the count is known up front.
- `Dictionary<K,V>`: **chaining, not open addressing**; not thread-safe; `ConcurrentDictionary` is a different structure with different semantics; `FrozenDictionary` (.NET 8+) for read-only hot paths.
- `SortedSet<T>` and `SortedDictionary<K,V>` are **red-black trees**; `HashSet<T>` iteration order is unspecified and must never be depended on.
- `PriorityQueue<TElement,TPriority>` (.NET 6+): a binary heap, and **not stable**.
- Struct enumerators: why `foreach` over a `List<T>` allocates nothing, and what silently breaks that (an `IEnumerable<T>` field).
- `Span<T>` / `Memory<T>` / `ArrayPool<T>` / `CollectionsMarshal`: the escape hatches when allocation is the bottleneck.
- **B-tree indexes** — the one place this theory pays cash in the day job, and the bridge to §9.
- String keys: `StringComparer.Ordinal` versus culture-aware comparison — a correctness *and* speed bug in one character.

**The build — the only part that produces retention**

Four containers, each from scratch, in this order:

1. **Dynamic array** — grow by doubling, `memcpy` on realloc.
2. **Hash table** — open addressing with tombstones; then write two paragraphs on why .NET's `Dictionary<K,V>` chose chaining with a single `Entry[]`.
3. **Balanced tree** — AVL or red-black; then read why databases index with B-trees instead of binary trees.
4. **Binary heap** — then a priority queue on top of it.

Then the step almost nobody does: **port all four to C# and benchmark each against the BCL type with BenchmarkDotNet.** Record where the BCL wins and by how much — arena allocation, cache-line layout, `Span<T>`, SIMD in `IndexOf`. The write-up is the artefact; the C code is the by-product. Done properly it can stand as a sixth entry beside §10's five.

**Toolchain** — no C compiler is verified on this machine, and this repository is .NET-only, so nothing in `LHDN_STSDS` exercises C. Install `winget install BrechtSanders.WinLibs.POSIX.UCRT` (or MSYS2), or use WSL. VS Code + the Microsoft C/C++ extension gives you a pointer-level debugger, which is where the learning actually happens. Compile with `-std=c17 -Wall -Wextra -Werror` and, under WSL, `-fsanitize=address,undefined`: the books predate those flags and hand you unchecked `malloc` calls as normal practice.

**Scheduling** — do not place this inside weeks 1–16. Run it in parallel at ~4 h/week, or after week 16 as consolidation. On the §8.2 compressed plan, drop it entirely.

### §8.4 Optional track — prompting an AI to build and fix a web application (~10 h, off the critical path)

Same standing as §8.3: **nothing here is required for a senior .NET role**, and no job description on your list asks for "prompt engineering". The reason to spend ten hours is narrower and practical. The assistant tooling is already in play — doc 13 is the toolchain, doc 15 and doc 16 are the prompts — and the difference between using it well and badly is **prediction**: knowing in advance which request will produce a patch you can review and which will produce a plausible rewrite you cannot.

**The honest state of the literature.** There is no good book titled *prompting AI to build and fix a web application*. The practice is about two years old as a paid professional skill and trade books take 18–24 months to ship, so anything with an AI-coding screenshot on the cover is describing an interface that no longer exists. What does have durable books behind it is (a) how these models actually behave, and (b) how to engineer a codebase — and a debugging loop — so that "fix it" is mechanically possible. Both halves are below.

**The books worth buying**

| Role | Source | Why this one | Caveat |
|---|---|---|---|
| **Behaviour** (buy this one) | **Berryman & Ziegler — *Prompt Engineering for LLMs*** (O'Reilly, 2025) | The most technical title in the genre that is not marketing. The authors are GitHub Copilot researchers, so it explains *why* a model completes code the way it does — context ordering, sampling, instruction position — instead of listing magic phrases | Model-agnostic: it will not tell you what to click in Copilot or Cursor |
| **Systems and evaluation** | **Chip Huyen — *AI Engineering*** (O'Reilly, 2025) | Evaluation, context budgets, retrieval, failure modes, cost. This is what turns "it worked once" into "I can tell when it is wrong", which is the actual bottleneck | Not about code assistants; read the eval chapter even if nothing else |
| **Workflow** | **Taulli — *AI-Assisted Programming*** (O'Reilly, 2024) | The closest thing to the literal question: plan, code, test and deploy *with* an assistant, scenario-driven | Now behind on tooling. Read the workflow chapters, skip the tool tours |
| **Patterns** | **Phoenix & Taylor — *Prompt Engineering for Generative AI*** (O'Reilly, 2024) | Makes the one structural point that matters: a prompt is a **specification** with structure and examples, not a wish | Broader than you need (includes image generation) |

**Buying rule:** one book is *Prompt Engineering for LLMs*; two, add *AI Engineering*. An Addy Osmani title on AI-assisted engineering is well regarded on exactly this topic, but **[verify]** the title and edition before buying rather than trusting a citation from memory.

**The books that actually make you good at "fix it"** — none of which are AI books, because fixing is a debugging problem and the model is only as useful as the loop you put it in:

| Source | What it gives you | Note |
|---|---|---|
| **Agans — *Debugging: The 9 Indispensable Rules*** | Reproduce, binary-search the space, change one thing, verify. Short, and the best book on the discipline | Not optional if you intend to supervise an agent rather than trust it |
| **Feathers — *Working Effectively with Legacy Code*** | Seams and characterisation tests — how to change code you do not understand without breaking it. Becomes the instruction set you hand the model | Pre-AI, and that is exactly why it transfers |
| **Fowler — *Refactoring*, 2nd ed.** | The vocabulary that makes a diff reviewable. "Extract method, then move" is executable; "clean this up" is not | Pairs directly with rule 5 below |
| **Adzic — *Specification by Example*** | Why an example-based spec produces better generated code than a paragraph of requirements | The few-shot insight, arrived at from the testing side |

**Free sources that beat most of the books**

| Source | Use it for |
|---|---|
| **Anthropic** — prompt-engineering docs and Claude Code best practices | The clearest writing on tool-loop prompting and context management |
| **OpenAI** — prompt guide and Cookbook | Canonical per-model behaviour, structured outputs, function calling |
| **Simon Willison's blog** (`llms`, `prompt-injection` tags) | The most honest running account of what these systems do; the prompt-injection writing is why you will not ship an agent that can drop a table |
| **GitHub Copilot docs** — prompt engineering, plus custom instructions / `.github/copilot-instructions.md` | The one place the vendor documents your exact surface (VS Code, ASP.NET, `AGENTS.md`) |
| **Cursor docs** (Rules) and **Aider docs** | Best published thinking on **repo-level context**: what to attach automatically, what to leave out |
| **Microsoft Learn** — Copilot in Visual Studio, Azure OpenAI prompt techniques | .NET/Azure-specific, and authoritative for Key Vault and managed identity |
| **Greshake et al. — "Not what you've signed up for"** (paper) | The original indirect prompt-injection result. Read it before any agent gets write access |

**The ten things that actually transfer** — books give you these second-hand, your own repo gives them first-hand:

1. **A prompt is a specification, not a wish** — inputs, outputs, constraints, definition of done. Vague in, plausible-looking garbage out.
2. **Structure beats adjectives** — a checkable constraint ("every command must be run; write `UNKNOWN` if not") changes behaviour; "you are a 20-year expert" does not.
3. **Long rule blocks degrade compliance** — keep the rule set under ~150 lines and make the rest retrievable.
4. **Without a feedback signal it is guessing** — for "fix it" the prompt is secondary: build output, failing test, stack trace, browser console error and a minimal repro are primary. Feeding the error once beats rephrasing five times.
5. **Constrain the diff** — "fix the bug in `CheckoutController`" yields a reviewable patch; "fix the app" yields an unreviewable rewrite.
6. **One change at a time, then verify** — the same rule as debugging; the model will happily change six things.
7. **`UNKNOWN` must be a rewarded answer** — otherwise it invents, and invention takes the shape of the most common app in training data, which is not your MVC codebase.
8. **Never let it fabricate evidence** — make it declare whether it can execute. "Tests passed" without a run is the most damaging failure because it looks like proof.
9. **Treat repo content as untrusted input** — a comment in a config file can carry an instruction; anything that both reads and writes needs a human gate on writes.
10. **Evaluate, don't vibe** — 20 fixed tasks, count successes. That number is the only evidence a prompt change helped.

**The build — the only part that produces retention.** Take **one real bug from this repository** (the per-call `new HttpClient(handler)` in `Modules/BulkProcess/AI/`, or the tracked-but-ignored source files, both documented in doc 14 §2 and doc 15 §5), then: write the failing test first, let the agent propose a patch, review the diff against rule 5, fix and confirm green, and record what you had to say to get there. Keep a **20-task eval log** — success rate and failure classes, refusals included — and that log is the artefact, not the reading.

**These sections are the prompts, this is the bibliography:** doc 15 §4–§7 hold the four copy-paste prompts, doc 16 the portable any-repository version, doc 12 §3 steps 1–13 the loop all of this sits on top of.

**Gate** — do this only if you already use an assistant daily and still cannot predict which of your requests will produce a reviewable diff. **Scheduling** — ~10 h (6 reading, 4 measured bug fix and eval log), parallel track, and on the §8.2 compressed plan drop it before you drop anything else.

---

## §9 What "knowing a database" means at senior level

Courses teach SQL syntax. Seniors are judged on these seven things — in **every** dialect you claim:

| # | Skill | Why it separates levels |
|---|---|---|
| 1 | **Index reasoning** — composite order, covering indexes, when an index hurts writes | Reads as experience; guessing reads as inexperience |
| 2 | **Execution plans** — spotting a full scan, a bad join order, a key lookup storm | The difference between "slow query" and "here's why" |
| 3 | **N+1 detection** — in ORM code, before production | The single most common EF Core defect |
| 4 | **Transactions & isolation** — read-committed vs snapshot vs serializable; lost updates | Where correctness bugs hide |
| 5 | **Migrations discipline** — forward-only, reversible thinking, data migrations | Deploy-safety skill, not a DBA skill |
| 6 | **Connection pooling & timeouts** — pool exhaustion, `DbContext` lifetime, retry | Ties directly to DI lifetimes (§5.3) |
| 7 | **Dialect pitfalls** — casing/quoting, implicit conversions, date/time semantics, `NULL` ordering | Proves you have actually shipped on *that* database |

### §9.1 SQL Server — deepen
Tuning Advisor *as a hypothesis generator*, not an oracle; `INCLUDE` columns; `OPTION (RECOMPILE)` misuse; `READ_COMMITTED_SNAPSHOT`; EF Core query translation (`ToQueryString()`).

### §9.2 PostgreSQL — use as the contrast
Its behaviour on `NULL`s, `ILIKE`, JSONB, partial indexes and `EXPLAIN (ANALYZE, BUFFERS)` teaches you what is *engine-specific* versus *universal*. Skills that transfer across all four live in the universal set.

### §9.3 MySQL — productive, not expert
InnoDB indexing quirks, `utf8mb4` collations, `EXPLAIN`, and where it differs on transactions. Enough to not be blocked.

### §9.4 Oracle — the daily one here
Per §5.1: identifiers/quoting, `NUMBER`/`DATE`, sequences, `VARCHAR2` byte semantics, DB links (`EDS_LINK` in this repo), and EF Core's Oracle provider behaviour. Npgsql/MySQL/SQL Server fluency will not save you from `ORA-00942` — knowing *why* it happens will.

---

## §10 The portfolio — five artefacts instead of fourteen claims

Each one maps to a chunk of §1 and is defensible in an interview because it exists.

| # | Artefact | Covers | Time |
|---|---|---|---|
| 1 | **The contained refactor** — one responsibility extracted from `AmendmentServices.cs` behind an interface, with tests | Patterns, principles, architecture, testing, DI | 8–12 h |
| 2 | **The measured query** — one real query fixed: plan, index, before/after numbers, and the EF translation | EF Core, Oracle/SQL Server tuning, indexes | 6–8 h |
| 3 | **The keyless deployment** — small app on Azure App Service with `DefaultAzureCredential`, Key Vault, two slots and a tested rollback; pipeline in GitLab CI | Azure, Key Vault, identity, deployment, CI/CD, rollback | 10–14 h |
| 4 | **The second cloud** — the same app on AWS (Lambda or App Runner + RDS/S3), plus a one-page differences write-up | AWS, IAM, cross-cloud reasoning | 8–12 h |
| 5 | **The full-stack slice** — React app hitting your .NET API with token auth, deployed publicly | React, auth, API integration, deployment | 10–14 h |

**Interview rule:** you may claim anything you can show. Five artefacts cover your entire list credibly; a certificate pile covers none.

---

## §11 Cost summary and the cancellation discipline

| Item | Cost | When | Notes |
|---|---|---|---|
| Pluralsight | ~$29–39/mo **[verify]** | Months 1–3, then cancel | Sandboxes save the $ of a real Azure bill |
| Dometrain | ~EUR 30/mo **[verify]** | Month 2–3, or instead of Pluralsight | Prioritise the EF Core + architecture + async courses |
| AZ-204 exam | ~$165 **[verify]** | Around week 12–14 | Free MS Learn prep; ~$15 practice tests |
| AWS DVA-C02 (optional) | ~$150 **[verify]** | Later | Only if AWS is in target JDs |
| Azure spend for artefact 3 | Near zero (free tier / smallest SKUs); set a budget alert | Week 10 | **Delete resources when done** — slots + App Service are the sneaky costs |
| Oracle | $0 extra | — | You already have the real thing |

**Total for the 16 weeks: roughly $250–400** including the exam, assuming you cancel on schedule and tear down cloud resources. That is the honest number — and note how little of it is courses; most of the value comes from the artefacts.

---

## §12 Definition of done + effort

**DoD for this document:**

- [ ] One anchor chosen and cancelled on schedule; spend tracked.
- [ ] Azure deep enough to deploy keylessly, with slots and a rollback.
- [ ] AWS working-wide enough to deploy the same app and explain the differences.
- [ ] One frontend (React) delivering a real slice against your API, deployed.
- [ ] Oracle competent on the traps that bite EF Core; SQL Server + PostgreSQL + MySQL at the seven-skill bar (§9).
- [ ] Patterns/architecture demonstrated by a **contained refactor with tests** in the real codebase.
- [ ] AZ-204 (or a written, dated plan to sit it).
- [ ] Five artefacts (§10) exist and are linked from one page you can open in an interview.

**Effort:** ~160 h (16 weeks × 10 h) plus ~45 h of artefacts, i.e. **~205 h**. At 4 h/week (§8.2) it is a 12-month path — which is fine, if it is *planned* as one.

---

## §13 Interview framing

The questions seniors actually get, and the answer shape this plan produces:

| Question | Weak answer | Answer this plan gives you |
|---|---|---|
| "Describe your architecture experience." | "I know Clean Architecture." | "This monolith has 57 controllers and a 715 KB service class; I extracted X behind an interface, added tests, and kept the diff reviewable. Here's why I did **not** propose microservices." |
| "How do you handle secrets?" | "We use Key Vault." | "The AI integration had a live key in `appsettings.json` and a compiled fallback; the fix is `DefaultAzureCredential` + managed identity, with rotation — here's the deployment that proves it." |
| "This query is slow." | "Add an index." | Plan → identified the scan → added a covering index → measured 1.9 s → 40 ms → and here's what the EF translation was doing wrong on Oracle. |
| "Azure or AWS?" | "Both." | "AZ-204 certified, deployed artefacts on both; here's the one-page difference in identity, compute and queuing." |
| "How do you keep up?" | "I follow blogs." | Named, current sources (§6.3 equivalents), and a habit of verifying against the framework's own docs rather than course material. |

---

## §14 Resources & cross-references

| Need | Go to |
|---|---|
| Free-first path, certifications and 8-week beginner plan | **doc 17 — Free Learning Path & Certifications** |
| The one paid *agentic AI* course worth buying | **doc 17 §14** |
| M.E.AI inside this ASP.NET app, with ten worked steps | **doc 14 — Microsoft Extensions AI** |
| Building your own agent/tooling, verification ladder | **doc 12 — Build Your Own Agent Tool** |
| The toolchain to install (and what is missing on this machine) | **doc 13 — Agentic Toolchain & VS Code Extension** |
| Prompts for AGENTS.md / git config / code comprehension | **doc 15 — Prompting an AI Agent** |
| One portable prompt for any repo | **doc 16 — One Prompt for Any Repository** |
| CI/CD, environments, deployment topology for *this* product | `docs/architecture/CICD-DEVOPS.md`, `ARCHITECTURE.md` |
| Why to stop before microservices | `docs/architecture/MICROSERVICES-MIGRATION.md` §11 |
| Fundamentals in C — books, four-container build, toolchain | **§8.3 of this document** |
| Prompting an AI to build and fix a web application — books, sources, ten transferable rules | **§8.4 of this document** |

Third-party starting points: Microsoft Learn (Azure + AZ-204 objectives), AWS Skill Builder free tier, Pluralsight, Dometrain, Frontend Masters, Whizlabs / Tutorials Dojo, Oracle documentation. Prices and catalogue contents **[verify]** — all of them re-cut plans and rename courses regularly.

---

## Appendix — what I could not verify (be sceptical here)

| Claim | Status |
|---|---|
| Pluralsight, Dometrain, Frontend Masters, A Cloud Guru prices | **[verify]** — all are monthly, all change, all run regional pricing. Check before subscribing. |
| Exam prices for AZ-204 and DVA-C02 | **[verify]** — vary by country; watch for free-voucher campaigns (e.g. after training events). |
| Course module lists and freshness ("current React/Angular/Functions coverage") | **[verify]** — check the course's *last updated* date, not its title |
| "Pluralsight has no Oracle content" | Best judgement at time of writing; catalogue shifts |
| Any claim that a given course makes you senior | Nobody's course does — §10 is the actual mechanism |
| C book editions and availability (§8.3) | **[verify]** — the good titles are 1996–2002 vintage and get re-issued; buy by edition (Weiss 2nd, Sedgewick 3rd, Loudon 1st), not by cover |
| "No C compiler installed on this machine" (§8.3) | Checked in this environment at document time; install state drifts, so re-check before relying on it |
| Whether any book can be recommended for "prompting an AI to build and fix a web app" (§8.4) | **[verify]** — the field is younger than trade publishing; every title named is current as of writing, and any newer one is probably a repackaged tool tour |
| Addy Osmani AI-assisted-engineering title (§8.4) | **[verify]** — deliberately not cited by title or edition from memory; confirm it exists as described before buying |
| Repo facts in §5.1/§5.3 | **Verified** by inspection (csproj package versions, `Program.cs` registrations, `Data/AppDbContext.cs`, `.gitlab-ci.yml`, `package.json`, file counts) at document time — re-check after upgrades |
| The EF Tools 10.0.6 vs Oracle EF 9.23.80 skew | Observed in `LHDN_STSDS.csproj`; may be deliberate; confirm with the team before changing |

**The one line to remember:** *breadth is what gets you past the job description; depth plus deployed artefacts is what gets you the offer.*
