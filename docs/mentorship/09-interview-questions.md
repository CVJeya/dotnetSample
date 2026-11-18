# 9. Weekly Interview-Style Questions (with model answers)

Use the drill weekly: answer **out loud, timed (2–3 min)**, then compare. Model answers are the *shape* of a strong answer, not a script. The best answers always add a **trade-off** and a **failure mode**.

Legend: **[T] technical**, **[A] architecture**, **[C] cloud/devops**, **[L] leadership/behavioural**.

---

## Phase 1 — .NET & data (W2–W5)

**W2 — [T] What actually happens when you `await` a method?**
Model: the compiler turns the method into a state machine; `await` on an incomplete task registers a continuation and returns to the caller; the continuation resumes when the task completes (same `SynchronizationContext` unless `ConfigureAwait(false)`). Key points: it's *not* a thread; no thread is blocked; exceptions are captured in the task. Trade-off/ failure: `async void` loses the task and crashes the process; `.Result`/`.Wait()` can deadlock under a sync context.

**W3 — [T] When would you *not* use async?**
Model: CPU-bound work (use parallelism/`Parallel.ForEachAsync`, not `async`), trivial in-memory work where the overhead isn't worth it, or when you're already on a dedicated thread/background service. Failure mode: making everything async "just in case" adds complexity and allocations.

**W3 — [T] How do you propagate cancellation and why does it matter?**
Model: pass `CancellationToken` through every IO call (`[FromServices] CancellationToken`, EF `ToListAsync(ct)`, `HttpClient.SendAsync(ct)`), link tokens for timeouts (`CreateLinkedTokenSource`). Why: releases thread-pool/DB resources when a client disconnects or you time out; prevents wasted work under load.

**W4 — [T] An endpoint is slow. Walk me through your investigation.**
Model: (1) reproduce + measure (App Insights/p95, not averages), (2) break down time — is it app CPU, DB, or a downstream call? (trace spans), (3) capture the SQL + execution plan, (4) fix the top cost (N+1 → projection; missing index; chatty downstream → batch/cache), (5) re-measure and add a regression guard. Trade-off: caching fixes latency but adds invalidation complexity.

**W4 — [T] Explain N+1 and 3 ways to fix it in EF Core.**
Model: N+1 = one query to get N parents + N queries for children. Fixes: `Include`/projection to load in one query, `AsSplitQuery` when a single huge join is worse, batch/loader patterns, and `AsNoTracking` for reads. Failure: `Include`-everything causes cartesian explosion; prefer projections to DTOs.

**W5 — [T] REST vs gRPC — when do you choose which?**
Model: REST for public/browser-friendly, cacheable, human-debuggable APIs; gRPC for internal service-to-service (binary, HTTP/2 streaming, strong contracts, codegen) where performance/contract matter. Trade-off: gRPC is harder to debug in browsers (needs gRPC-Web) and less cache-friendly; versioning discipline differs.

## Phase 2 — Architecture (W6–W9)

**W6 — [A] Modular monolith or microservices for a 5-person team?**
Model: modular monolith with enforced boundaries; microservices add network, data-consistency, and ops cost that small teams can't absorb. Extract a service only on a real driver (independent scale/deploy/data ownership). Trade-off: you must *enforce* module boundaries in a monolith or it rots into a big ball of mud.

**W7 — [A] What is an aggregate and why does it matter?**
Model: a cluster of objects treated as one consistency boundary, with a root that enforces invariants; you load and save it as a unit. Why: it defines the transaction and the scale limit. Failure: making aggregates too large (contention/deadlocks); referencing other aggregates by id, not object, across boundaries.

**W8 — [A] How do you guarantee a message isn't lost when a process crashes after a DB write?**
Model: transactional outbox — write the business change + the outbox row in the same DB transaction; a relay/publisher reads the outbox and publishes, marking sent. Consumer idempotent. Failure modes: at-least-once delivery, duplicates, ordering (per-key only), DLQ for poison messages. "Exactly-once" end-to-end isn't achievable — you get at-least-once + idempotency.

**W8 — [A] Where does CQRS help, and where is it overkill?**
Model: helps when read and write needs diverge (heavy reads, complex invariants, different scaling). Overkill for simple CRUD. Start with CQRS-lite (same DB, separate models/projections) before separate stores.

**W9 — [C] Describe a circuit breaker to a product manager.**
Model: "If a dependency keeps failing, we stop hammering it for a while and fail fast with a fallback, so the whole system stays responsive instead of piling up timeouts; we periodically test the water and restore." Include: retry with backoff+jitter for transient, breaker for persistent, bulkhead to isolate.

**W9 — [A] What are the three pillars of observability and how do they connect?**
Model: logs (what happened), traces (the path of a request across services), metrics (aggregate health). They connect via correlation/trace ids. Use RED for services, USE for resources; alert on symptoms (SLO burn), not causes.

## Phase 3 — Cloud, DevOps, security (W10–W14)

**W10 — [C] Design a multi-tenant SaaS on Azure.**
Model: tenant identity (Entra ID / B2C), tenant context resolved per request; data isolation choice — shared DB with tenant id (cheap, noisy-neighbour) vs schema-per-tenant vs DB/database-silo (isolation, costly); store in SQL/Cosmos with tenant partition; per-tenant config in Key Vault; scale with Container Apps/AKS; observability tagged by tenant; cost + isolation trade-off documented.

**W11 — [C] What happens from `kubectl apply` to a running, receiving pod?**
Model: API server validates + persists to etcd; controller creates a ReplicaSet → Pods; scheduler binds a pod to a node; kubelet pulls the image + starts containers; readiness probe passes → endpoints/Service routes traffic; liveness restarts if unhealthy. Add: resource requests/limits, HPA, rolling update with `maxSurge`/`maxUnavailable`.

**W12 — [C] Map the same app between Azure and AWS.**
Model: App Service/Container Apps ↔ Elastic Beanstalk/ECS; AKS ↔ EKS; Functions ↔ Lambda; Blob ↔ S3; SQL ↔ RDS/Aurora; Cosmos ↔ DynamoDB; Service Bus ↔ SQS/SNS; Key Vault ↔ Secrets Manager; Entra ID ↔ Cognito/IAM; App Insights ↔ CloudWatch/X-Ray. Note differences: IAM model, region/availability zones, pricing granularity, serverless limits.

**W13 — [C] Blue-green vs canary vs rolling — pick one for a payments API.**
Model: canary — route a small % of traffic, watch error rate/latency SLOs, auto-rollback; ideal for risk. Blue-green gives instant rollback but doubles cost and needs slot swap discipline. Rolling is cheapest but mixed versions must be N-1 compatible. Trade-off: canary needs good observability and traffic splitting.

**W14 — [T/C] Secure a public API end to end.**
Model: TLS everywhere; OIDC/JWT with issuer/audience/expiry validation and `kid` rotation; scopes/roles for authZ + per-resource ownership checks (prevent IDOR); rate limiting + WAF; input validation; parameterised SQL; secrets in Key Vault via managed identity; least-privilege pipeline identity (OIDC); SAST/DAST/dependency/container scanning in CI; structured audit logs without PII; SBOM + signed artifacts. Map to OWASP Top 10.

## Phase 4 — Frontend (W15–W17)

**W15 — [T] How would you review a React PR?**
Model: check state ownership (server state in a query lib, not `useState`), hook deps correctness, unnecessary re-renders (`key`, memo only if measured), error/loading boundaries, accessibility, tests for behaviour (RTL), bundle impact, and no secrets in client code. Labels + teaching comments.

**W16 — [T] RxJS vs signals — which for a live dashboard?**
Model: signals for synchronous view state and derived values (simple, zoneless, less boilerplate); RxJS for event streams (debounce, retries, websockets/`switchMap`). Often both. Don't rip working RxJS out; adopt signals for new view state.

**W17 — [A/L] How do you choose a frontend framework for a 10-year product?**
Model: decide for the *team and hiring pipeline*, not trends — team skill, typing/enterprise governance (Angular), flexibility/ecosystem (React), SSR needs (Next), performance/SEO, long-term support. Time-box a spike, write an ADR with consequences, and commit. Trade-off: opinionated frameworks reduce decisions but constrain; flexible ones need your own standards.

## Phase 5 — Modern tech & AI (W18–W20)

**W18 — [T] What problem does .NET Aspire solve?**
Model: local orchestration + service discovery + telemetry + resilient defaults for distributed .NET apps; one `AppHost` composes services and containers and gives a dashboard. When not: single-service apps or teams with mature platform tooling.

**W19 — [A] Blazor vs React SPA — when?**
Model: internal LOB app, .NET-heavy team, fast delivery, real-time → Blazor Server/Auto; public, SEO-heavy, large JS ecosystem, complex client interactivity → React/Next. Trade-off: Blazor Server holds server connections (scale), WASM has download size/startup.

**W20 — [T] Where does RAG fail and how do you make it reliable?**
Model: chunking breaks meaning; retrieval misses (fix: hybrid search + rerank + metadata filters); stale index (reindexing); hallucinated citations (require citations + grounded prompts + refusal); no evals (fix: a 20+ question eval set, faithfulness/relevance, gate merges); PII leakage; token cost. RAG quality is an *engineering* problem, not a model problem.

## Phase 6 — Leadership & behavioural (all weeks)

**L — Tell me about a technical decision you regret.**
Model (STAR): context → the decision + your reasoning at the time → the negative impact you own → what you changed / would decide now. Show you extract principles. Avoid blaming others; avoid a fake "regret".

**L — How do you handle a senior engineer who disagrees publicly with your design?**
Model: separate idea from person; invite the disagreement as data ("what's the failure mode you're worried about?"); find the criterion you differ on; run a time-boxed spike if it's cheap; decide + write the ADR + disagree-and-commit. Never win by authority; never avoid the decision.

**L — How do you estimate and communicate uncertainty?**
Model: reference stories + ranges + confidence; spike unknowns first; give a cone of uncertainty; report RAG status + what changed + what you need. Commitment vs estimate distinction.

**L — How do you give a junior hard feedback about quality?**
Model: SBI, private, specific, timely; focus on the code/behaviour and its impact, not the person; agree on a concrete next action and follow up. Radical candour.

**L — How do you manage technical debt with a product manager who only wants features?**
Model: classify + quantify debt by risk/velocity/cost; make it visible; propose a fixed % per sprint or tie payoff to a specific upcoming goal/incident; let them choose with the trade-off in front of them. Sign flip, not personal flame.

**L — A release is slipping. How do you handle it?**
Model: surface early with options (reduce scope / move date / add people carefully — Brooks' law caveat); recommend one; get the decision from the owner; protect quality and communicate a plan. Never hide it hoping to recover.

**L — How do you know you're a good lead?**
Model: the team's output, growth, and clarity — not your personal LOC. Evidence: people you've grown, decisions you've made that stuck, incidents you prevented, a lower escaped-defect rate, and stakeholders never being surprised.

**L — Describe a time you improved a team process.**
Model (STAR): symptom → measured it → small experiment → result → made it the default. Show you change systems, not just complain.

---

## Full loop simulation (W25)

Run a 3-hour slot in one sitting: 45 min live coding (async/perf/refactor), 60 min system design (design your capstone on a whiteboard from scratch), 45 min behavioural (6 STAR stories), 30 min your questions to them. Record it; watch the playback — the parts you wince at are your next week's study.
