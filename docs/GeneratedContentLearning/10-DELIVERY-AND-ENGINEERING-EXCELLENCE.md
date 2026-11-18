# Chapter 10 — Delivery & Engineering Excellence

**Why this chapter exists:** a Technical Lead is judged on whether the team ships **predictably and safely** — not on whether it writes clever code. Interview panels ask this as *"how do you make sure quality doesn't slip when deadlines hit?"* and they accept only answers backed by **mechanisms**, not intentions.

Everything here is grounded in this repo's real delivery setup — `azure-pipelines.yml`, `docker-compose.yml`, four `Dockerfile`s — including an honest critique of it that you can use as a whiteboard exercise in the interview.

---

## 1. The lead's delivery philosophy in four lines

1. **Small batches** — smaller PRs, smaller releases, smaller migrations. Almost every delivery pathology is a batch-size pathology.
2. **Fast feedback** — the pipeline's job is to tell you you're wrong in minutes, not at release.
3. **Reversibility** — every change must be deployable *and* un-deployable independently of everything else.
4. **Make it visible** — a metric on a wall beats a status meeting.

> Interview line: *"Delivery problems are usually batch-size problems wearing a disguise. When a release is scary, the fix is almost never more testing at the end — it's smaller releases, earlier."*

---

## 2. Standards as code (how you enforce quality without being a reviewer bottleneck)

The leverage insight: **anything a human has to remember, a tool should enforce.** A lead who argues style in review is a lead doing the compiler's job.

### 2.1 The enforcement ladder (weakest → strongest)

| Level | Mechanism | Sticks? |
|---|---|---|
| Wiki page | "Please follow our conventions" | No |
| Review comment | "We generally use `sealed` here" | Sometimes, and it costs a human every time |
| **`.editorconfig` + `Directory.Build.props` + analyzers** | IDE squiggle, build warning | Yes |
| **Warnings as errors in CI** | Build fails | Yes, universal |
| **Architecture tests** | Build fails on boundary violation | Yes, structural |
| **Generated contracts** | Build fails on drift | Yes, and free |

### 2.2 Concrete artefacts to describe (this reads as "has actually done this")

```xml
<!-- Directory.Build.props — one file, applies to every project in the repo -->
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
    <GenerateDocumentationFile>false</GenerateDocumentationFile>
  </PropertyGroup>
</Project>
```

```ini
# .editorconfig — style decided once, not per review
[*.{cs}]
indent_style = space
dotnet_style_qualification_for_field = false:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
dotnet_diagnostic.CA1062.severity = error   # validate arguments
dotnet_diagnostic.CA2007.severity = error   # ConfigureAwait in libraries
```

```csharp
// Architecture tests — the boundary is real only if a test fails.
[Fact] public void Domain_must_not_depend_on_EF() =>
    Types.InAssembly(typeof(Order).Assembly)
         .ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")
         .GetResult().IsSuccessful.Should().BeTrue();
```

Front end equivalents: `oxlint` (already configured in `ReactDotNetDemo/`), Angular's `ng lint`/schematics, Prettier, `axe` for a11y, and a `tsconfig` with `strict: true`.

**The line that lands:** *"Every rule that lives in a document is a rule I'll have to argue about again. Every rule that lives in the build is a rule nobody argues about."*

---

## 3. Branching and integration strategy

### 3.1 The honest comparison

| Model | Integration pain | Release flexibility | Good for |
|---|---|---|---|
| GitFlow (`develop` + `release/*` + `hotfix/*`) | High, merge hell, long-lived drift | Formally rigid | Regulated release calendars, desktop software |
| **Trunk-based + short-lived branches + feature flags** | Low | High | **Web services, teams ≤ 10 — this is what you recommend** |
| GitHub-flow (main + PR branches) | Low | Medium | Small teams, simple products |
| Release branches kept alive long-term | Grows unbounded | Medium | Only when you must support old versions |

### 3.2 What this repo does, and the lead's critique of it

`azure-pipelines.yml` triggers on **`main`, `master`, `develop`** — three long-lived branches. That is GitFlow residue on a web product:

> "Three long-lived branches on a SaaS-style product is three truths. `develop` drifts from `main`, the merge back is a multi-day event, and 'works on develop' becomes a phrase. I'd move to trunk-based: everything merges to `main` behind a flag, deploy on every merge, and the only branches are the ones that live under two days. That changes the hardest metric — lead time — from days to hours."

Lead-level detail they'll probe for: **how do you release unfinished work?**
> "Feature flags. Short-lived branches plus flags mean the code is in `main` continuously, but the behaviour is off. The flag is created with an **owner and an expiry date** in the ticket, and expired flags are a review item in the retro. Flags without expiry become the new technical debt."

### 3.3 Branch protection rules to state plainly
- No direct pushes to `main`; PR required.
- **Pipeline must be green** before merge (all stages, including tests).
- 1 approval (2 for auth, migrations, public contracts).
- Branch must be up to date / behind-base blocked.
- **Squash merge** so `main` history is one meaningful commit per change, revertable in one click.

---

## 4. The pipeline: design it properly (and critique the real one)

### 4.1 What the current `azure-pipelines.yml` does

```
Build stage
  ├─ checkout (fetchDepth: 1)
  ├─ docker login to ACR (conditional on a service connection)
  ├─ build & push dotnet-backend   (backend/AngularDotnetDemo.Api/Dockerfile)  → tags: $(Build.BuildId), latest
  ├─ build & push angular-frontend (frontend/Dockerfile)                       → tags: $(Build.BuildId), latest
  └─ build & push react-frontend   (ReactDotNetDemo/Dockerfile)                → tags: $(Build.BuildId), latest
Deploy stage (not on PRs, environment: production)
  └─ echo deployment instructions (ACI / App Service / AKS) — a stub
```

### 4.2 The critique — eleven real findings (this is your whiteboard gold)

Present these as *"here's what I'd change and why"*, highest risk first:

| # | Finding | Risk | Fix |
|---|---|---|---|
| 1 | **No test stage.** No `dotnet test`, no `npm test`, no lint | You can merge untested code and it deploys | Add a `Test` stage before `Build`; gate on it |
| 2 | **PRs push images.** `pr:` triggers the workflow and `Build` has no PR condition, so a PR overwrites `:latest` in ACR | A branch can deploy to production by accident | Only publish on `main`; PRs build without `--push` |
| 3 | **Mutable `latest` tag** | Cannot tell what's running; rollback is guesswork | Immutable tags: git SHA + semver; `latest` is a pointer only, never deployed from |
| 4 | **Deploy is a stub** — no actual deployment | No release capability at all | Real deploy task per environment (see §5) |
| 5 | **No EF Core migration step** | Schema drift between deploy and DB | Migration job/bundle runs before the app starts, with a lock |
| 6 | **No security scanning** | Vulnerable base images/deps ship silently | `dotnet list package --vulnerable`, `npm audit --audit-level=high`, Trivy/Defender image scan |
| 7 | **No caching** — no NuGet cache, no Docker layer cache, no npm cache | 10–15 min builds, slow feedback | Cache `~/.nuget/packages`, use BuildKit `--cache-from`, cache `node_modules` by lockfile hash |
| 8 | **One job, three sequential images** | One failure wastes all; no parallelism | Three jobs / matrix — failures are isolated and total time drops |
| 9 | **No smoke test after deploy** | "Deployed" ≠ "working" | Post-deploy: hit `/health` and one critical authenticated endpoint; fail the release |
| 10 | **No version stamping in the app** | Cannot correlate an incident to a build | `InformationalVersion` from git SHA, surfaced on `/health` and in the UI footer |
| 11 | **No build once, promote many** | Each environment can be built differently | Build the image once, tag by SHA, promote the *same digest* dev → staging → prod |

> Say this sentence in the interview: **"A pipeline's real output is not an artefact — it's confidence per unit time. If it takes 20 minutes to learn you broke a test, the team will start batching changes, and every problem downstream follows from that."**

### 4.3 The pipeline you'd build (draw this)

```
PR validation  (~4 min)          Main build (~8 min)             Release
─────────────                    ──────────────                  ───────
 restore (cached)                 same, plus:                     deploy digest
 build                            push images (sha + semver)      → migrate
 lint / analyzers                 scan (deps + image)             → smoke test
 unit tests                       publish OpenAPI + client diff   → flag on 5%
 contract diff                    (fail on drift)                 → watch 30 min
 arch tests                                                       → flag 100%
 build docker (no push)
 FE unit tests (vitest)
                                    ▼
                            Staging (auto)  →  Prod (approval gate)
```

Key design rules to name:
- **Fail fast, cheapest first.** Lint/analyzers before tests before Docker builds. Never make the team wait 8 minutes for a nullability warning.
- **Same image all the way up.** Build once, promote by digest, promote by environment variable.
- **Approval gates on production**, not on a separate "release" meeting.
- **Environment-specific config from the environment** (App Configuration / Key Vault), never baked into the image.
- **The pipeline is the only deployer.** No `az`/`kubectl` from a laptop against production — that is an unreviewable, unaudited change.

### 4.4 Environments — how many, and why three is right
- **Dev** — integration, synthetic data, may break. Auto-deploy on every merge.
- **Staging / pre-prod** — production-shaped, anonymised or synthetic data, real auth, auto-deploy on merge + smoke tests. This is where you prove migrations and load.
- **Production** — approval gate, real traffic, monitored, progressive rollout (flags / slots / canary).
- **Ephemeral / preview** per PR — optional but high leverage for the front end; cheap with container apps.

Never more than four. Every extra environment is a maintenance and configuration-drift tax.

---

## 5. Release strategy: how change reaches users safely

| Technique | What it buys | Cost | Use when |
|---|---|---|---|
| **Blue-green** | Instant rollback | Double infra briefly | Stateless services |
| **Canary** | Limits blast radius to 5% of traffic | Needs good metrics + traffic split | Risky changes, high traffic |
| **Deployment slots** (App Service) | Warm swap, instant revert | Platform-specific | App Service workloads |
| **Feature flags** | Decouples deploy from release | Flag debt | **Default for behaviour change** |
| **Dark launch** | Real load, no user impact | Instrumentation | Perf-sensitive paths |
| **Rolling** | No downtime, no double infra | Slower rollback | Container/orchestrated |

**The distinction a lead must make crisply:**
> **Deploy ≠ release.** Deploy is a technical event (code is in production). Release is a business event (users can see it). Decoupling them with flags is what lets you deploy fifty times a day without a change-management meeting.

**Rollback rules to state:**
- Every deploy has a **tested** rollback path — tested, not assumed. A rollback you have never executed is a wish.
- Migrations are **forward-only**; rollback is a forward fix. Which is why you use expand→contract (Chapter 9 §7.4).
- **A rollback must never require a code change.** If it does, you have a hotfix culture, not a release process.

---

## 6. Testing strategy — the lead's version (a policy, not a pyramid chant)

Everyone recites the pyramid. Lead-level answers add **what you enforce, what you're willing to skip, and how you measure whether the tests are worth their cost**.

### 6.1 The shape you'd enforce

```
        ▲  ~5%   E2E (Playwright): 6–10 critical user journeys only
       ███       Integration: API + real DB (Testcontainers) — the highest-value layer for .NET
     █████       Component/FE: React Testing Library / Angular TestBed on behaviour
   ███████       Unit: pure logic, domain rules, mapping, validators
```

Two rules that separate you from a mid-level candidate:

1. **Integration tests catch what unit tests only pretend to.** "For an EF Core + ASP.NET Core app, the highest-value test is a real HTTP call against a real database in a container — it catches DI misconfiguration, serialisation, filters and SQL translation, all of which mocks hide."
2. **E2E is expensive and flaky; use it only for revenue/critical journeys.** "Twenty E2E tests will make the pipeline slow and untrusted. Six will protect the things you actually cannot break."

### 6.2 The metrics that matter (not coverage %)

| Metric | Target | Why it beats coverage |
|---|---|---|
| **Mutation score** on domain logic | > 60% | Measures whether tests *assert*, not whether lines ran |
| Coverage on **changed** lines | > 80% | Diff coverage is actionable; repo coverage is vanity |
| Pipeline test duration | < 5 min unit, < 15 min total | Feedback speed drives branch size |
| **Flake rate** | < 1%, quarantined within a day | A flaky test destroys trust in the whole suite |
| Escaped-defect rate | Trending down | The only metric the business cares about |

> "I don't have a coverage target. I have a **test-quality** target. A file with 100% coverage and no assertions is worse than no test, because it lies."

### 6.3 Testing policy gaps — use this repo as the example
There are **no test projects** in the solution today. That is *the* first thing a lead fixes, and saying so out loud is a strength, not an admission:

> "If I joined and found this repo, my week one actions are: add a test project for the API, three integration tests over the highest-risk endpoints using Testcontainers, one Playwright journey for login → core action, and a PR gate that requires them. Not a testing programme — four tests and a gate. The gate is what changes behaviour; the tests just make the gate meaningful."

---

## 7. Definition of Done (the cheapest quality mechanism you have)

Write it once, refine in review when it's missing, and never let "we'll do it in the next sprint" apply to it:

- [ ] Acceptance criteria met and demoed
- [ ] Unit/integration tests for logic branches; they fail when behaviour breaks
- [ ] No new warnings (build is warnings-as-errors)
- [ ] Telemetry: structured log or metric for the new path
- [ ] Config externalised (no secrets, no hardcoded URLs)
- [ ] Error handling: one user-visible path, one actionable log
- [ ] Contract updated (OpenAPI regenerated, client regenerated)
- [ ] Migration included if schema changed, with expand/contract noted
- [ ] Behind a flag; flag has owner and expiry
- [ ] Rollback path known and stated in the PR
- [ ] Docs/runbook updated if behaviour for operators changed
- [ ] Deployed to staging and smoke-tested

> "Our DoD exists because I got tired of the same three review comments. It's twelve checkboxes and it removed about a third of our review round-trips."

---

## 8. DORA metrics — how a lead talks to management about engineering

| Metric | What it measures | Elite-ish target | Cheap signal in a small team |
|---|---|---|---|
| **Lead time for change** | commit → production | < 1 day | Pipeline timestamps |
| **Deployment frequency** | how often you release | daily+ | Pipeline runs on main |
| **Change failure rate** | % deploys causing degradation | < 5% | Incident ↔ deploy correlation |
| **Time to restore** | MTTR | < 1 hour | Incident timeline |

Plus one the DORA set omits and leads should add: **review latency** (PR open → first review). It is the biggest controllable contributor to lead time in a small team, and it is *entirely* a leadership variable.

**Why this wins in an interview:**
> "DORA metrics are the only way I've found to make an engineering-quality argument to a non-technical stakeholder without whining. 'Our change failure rate went from 22% to 6% after we added the test gate' is a business sentence. 'We need to write more tests' is not."

And be honest about the trap: **do not use DORA metrics to compare individuals.** Velocity and deploy counts gamed per-person destroy the culture in a quarter.

---

## 9. Incident management — the capability that earns trust fast

### 9.1 The severity model (state it plainly)
| Sev | Definition | Response | Comms |
|---|---|---|---|
| **Sev1** | All users down / data loss / security breach | All hands, incident commander, page immediately | Exec update every 30 min |
| **Sev2** | Major feature degraded / significant subset affected | On-call + owner, 1 h fix target | Hourly |
| **Sev3** | Minor degradation, workaround exists | Next business day | Ticket |
| **Sev4** | Cosmetic / single user | Backlog | None |

### 9.2 The lead's role during an incident (three jobs, in order)
1. **Stabilise, don't diagnose.** Roll back first if a recent deploy correlates. Debugging in production while users are broken is a choice to extend the outage.
2. **Run comms, or appoint someone to.** You are the least useful debugger in the room if you are the one who can talk to stakeholders. Announce status, next update time, and keep it.
3. **Protect the responders.** Kill the "who broke it" conversation inside the incident. Blameless is not softness — it is the only way you keep getting honest information at the moment you need it.

### 9.3 The postmortem template (blameless) — cite it
```
Title / date / severity / duration / customer impact (quantified)
Timeline (UTC, from first signal to all-clear)
Detection — how did we find out? (alert? customer? luck?)
Root cause — the technical story, no names
Contributing factors — the systemic ones (3–5)
What went well
Action items — owner, date, type (prevent / detect / mitigate / document)
```
**The rule:** every action item has an **owner and a date**, and postmortem actions are tracked on the same board as feature work — otherwise postmortems become theatre. Target: 3–5 actions, not 20.

### 9.4 Detection before reaction
| Layer | Tool | What it catches |
|---|---|---|
| Health | `/health/live`, `/health/ready` | Container is alive vs able to serve |
| Metrics | App Insights / Prometheus | Latency, error rate, saturation |
| Logs | Structured + correlation ID | The detail, joinable across services |
| Traces | Distributed tracing | Where the 4 s went |
| Synthetic | Ping key journeys every minute | "Is it working for a stranger?" |
| Business | Orders/minute, signups/hour | The failure the infrastructure can't see |

> "The most under-invested alert in every team I've joined is a **business-metric** alert. Infrastructure green + zero orders is a Sev1, and no CPU dashboard will tell you."

---

## 10. SLI / SLO / error budget — the modern reliability vocabulary

- **SLI** — the measurement: "proportion of `/api/orders` requests returning 2xx in < 500 ms".
- **SLO** — the target: "99.5% over 28 days".
- **Error budget** — 0.5% of requests = the amount of unreliability you're *allowed*.

**Why a lead cares:** the error budget turns reliability from an argument into arithmetic.
> "If we've spent 80% of the budget in two weeks, we stop adding risk and fix reliability — and that conversation is data, not opinion. If we're at 5%, I can defend shipping faster. It also stops the opposite failure: chasing 100% reliability on a feature nobody uses."

For a small product, start with **two SLOs** (availability and latency of the main journey) and one **business** SLI (orders/minute). Three numbers, on one dashboard, is a realistic lead's scope.

---

## 11. On-call and operational ownership

For a team of three, an honest design:
- **Weekly rotation**, primary + secondary, never one person indefinitely.
- **Page only for user-visible problems with a clear action.** Everything else is a ticket. Alert fatigue is the fastest way to make on-call meaningless.
- **Runbook per alert**, 5 lines minimum: what it means, first check, common cause, escalation.
- **Compensation and recovery time** — if the team paid for a night, the lead gives the time back. Say this; it is a retention signal interviewers remember.
- **Every page produces an action.** A page that repeats weekly without an action is a known-broken alert and a morale leak.
- **The lead takes the worst rotation** — the first rotations personally, so they feel the pain they're designing for.

---

## 12. Security in the delivery pipeline (the lead's minimum bar)

| Control | Where it lives | Cheap implementation here |
|---|---|---|
| No secrets in code | Repo + review | Key Vault + managed identity; `.gitignore` + secret scanning |
| Dependency scanning | Pipeline | `dotnet list package --vulnerable`, `npm audit` |
| Image scanning | Pipeline | Trivy/Defender on the four Dockerfiles |
| AuthN/AuthZ tested | Tests | Integration test that an unauthenticated call is 401, and a wrong-role call is 403 |
| Least privilege | Cloud config | Managed identity per app; no shared admin connection strings |
| Audit trail | Platform | Every deploy via pipeline; no manual `az` against prod |
| OWASP ASVS basics | Design review | Authz on every endpoint, input validation, output encoding, no secrets in logs |
| Data protection | Code + config | PII not logged; classification on new fields; retention policy |

**The lead sentence:** *"Security is a delivery-pipeline property, not a review step. If a check isn't automated, it happens on the day someone remembers."*

---

## 13. The engineering-health dashboard (what you'd actually put on a screen)

Four quadrants, all cheap to produce, all causally linked to problems:

1. **Flow** — lead time, deploy frequency, PR review latency, WIP.
2. **Quality** — change failure rate, escaped defects, flaky test count, warnings count, dependency vulnerabilities.
3. **Reliability** — availability vs SLO, error budget burn, p95 latency, top 3 alerts by volume.
4. **Team** — sprint commitment vs completion, unplanned work %, on-call pages per week, open P1s.

One rule: **no individual attribution.** The moment a dashboard becomes a performance scoreboard, people optimise the number and you lose the signal.

---

## 14. Chapter 10 — questions you must be able to answer out loud

1. How do you keep quality from slipping when the deadline moves? Give mechanisms, not intentions.
2. Walk me through your ideal CI/CD pipeline for a .NET + Angular product.
3. What would you change in *this* pipeline, and in what order? *(use §4.2 — it is real and verifiable)*
4. Trunk-based or GitFlow? Defend it. How do you release unfinished features?
5. How do you do code review in a team of three without becoming a bottleneck?
6. What's your testing strategy, and what won't you test?
7. What does your Definition of Done contain, and what did it remove?
8. How do you release safely? What's your rollback story?
9. How do you handle a Sev1? What are you personally doing in the first 15 minutes?
10. How do you run a blameless postmortem, and how do you make the actions actually happen?
11. How do you explain engineering quality to a non-technical stakeholder?
12. What metrics do you track, and what would you deliberately *not* track?
13. How would you set up on-call for a team of three without burning them out?
14. A deploy went out and errors tripled. Walk me through the next hour.
15. What's the first thing you'd add to this repo's delivery setup, and why that first?

**Next:** `11-AZURE-PLATFORM-FOR-LEADS.md` — the Microsoft-specific platform knowledge a lead is expected to hold.
