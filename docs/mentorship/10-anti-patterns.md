# 10. Common Mistakes & Anti-Patterns (by stage)

Use this as a weekly self-check. Add your own as you catch yourself.

---

## Stage 1 — .NET / code

- **Async overuse / misuse:** `async void`, `.Result`/`.Wait()` (deadlocks), fire-and-forget tasks not tracked, `Task.Run` on the server "to be async", no `CancellationToken`.
- **Async/await ≠ threads:** expect parallelism → the real bug is usually in `await foreach` / `Parallel.ForEachAsync` usage.
- **EF Core:** lazy loading in services (N+1), `Include`-everything, `ToListAsync` before filtering, `Count() > 0` instead of `Any()`, long-lived `DbContext`, client-side evaluation, catching `DbUpdateConcurrencyException` never.
- **DI:** service locator, singleton holding a scoped dependency (captive dependency), `BuildServiceProvider()` at startup, `IOptions` captured in a singleton.
- **Correctness:** swallowing exceptions, `catch (Exception) { }`, mutation of shared state without locks, `DateTime.Now` instead of injected `TimeProvider`, culture/tick dependence in money/date logic.
- **Over-engineering a senior's tell:** abstraction for a single implementation, generic repositories wrapping EF, mappers for the sake of mappers.

## Stage 2 — Architecture

- **Distributed monolith:** services that must deploy together / share a database — worst of both worlds.
- **Premature microservices:** splitting before you have drivers; the team can't operate what they built.
- **Chatty synchronous chains:** deep service-to-service call chains that multiply latency and failure probability.
- **Shared database as integration:** coupling teams through tables; use APIs/events with explicit contracts.
- **Anemic domain + fat services:** business rules scattered in application services and controllers.
- **Cargo-cult CQRS/event sourcing:** adding them "because Netflix" without the read/write divergence or audit need.
- **Ignoring the outbox:** publishing inside the DB transaction with no outbox → lost messages on crash.
- **Exactly-once fantasy:** assuming a broker gives it; build idempotency instead.
- **No failure design:** retries without jitter/timeout, no circuit breaker, no DLQ, no idempotency → cascade failures and thundering herds.
- **Observability afterthought:** no trace propagation across boundaries; debugging distributed issues becomes archaeology.
- **God gateway:** business logic in API Management → a new single point of failure and a hidden monolith.

## Stage 3 — Cloud / DevOps / security

- **Secrets in git / appsettings:** including "just for now"; use a vault + managed identity + `gitleaks`.
- **Snowflake infra:** resources clicked in the portal; make it IaC or it isn't reproducible.
- **Rebuild per environment:** violates "build once, deploy many"; config belongs to the environment.
- **No rollback plan / untested rollback:** if you've never rolled back, you can't.
- **Breaking DB migrations:** renaming/dropping columns while old code runs → expand/contract instead.
- **K8s without probes/limits:** the pod gets traffic before it's ready or eats the node.
- **`latest` image tags + no scan:** non-reproducible and vulnerable.
- **God IAM role / long-lived cloud keys in CI:** use OIDC federation + least privilege.
- **Ignoring cost:** no budget alerts, no tags, no right-sizing → surprise bill.
- **Alert on causes, not symptoms:** CPU-alerts everywhere → alert fatigue; alert on SLO burn.
- **Blocking security as "phase 2":** scanning/secrets/auth are cheapest at the start, not the end.

## Stage 4 — Frontend

- **Premature `useMemo`/`useCallback`:** optimisation without measurement; hides bad structure.
- **Server state in client state:** hand-rolling fetch/cache instead of a query library → duplicate logic, stale data.
- **Wrong `key`:** index as key → subtle state bugs on reorder.
- **RxJS subscription leaks:** missing `takeUntilDestroyed`/`async` pipe; nested subscribes instead of operators.
- **Framework by fashion:** choosing React/Angular for trends, not team fit; no ADR.
- **No error/loading/empty states:** only the happy path exists.
- **Client-side secrets:** putting API keys in frontend code (always public).
- **No tests / testing implementation details:** RTL tests that break on refactor and prove nothing.

## Stage 5 — Modern tech & AI

- **Shiny-object adoption:** new tech with no driver → maintenance burden, no bus factor.
- **Aspire/compose as prod:** these are *dev* orchestration; deployment is separate.
- **Blazor Server at internet scale without planning:** connection limits and stateful scale.
- **RAG as magic:** no evals, no reranking, no citations → confident wrong answers.
- **PII into LLM prompts / logs:** leakage + risk; redact and govern.
- **Copilot-generated code unreviewed:** treat it as a fast junior; you own every line.
- **Cost blindness on LLMs:** no token budgets, no caching, no rate limits → runaway spend.

## Stage 6 — Leadership (the big ones)

- **The hero engineer:** doing everything yourself instead of multiplying the team — the #1 transition failure.
- **Avoiding decisions:** "let's discuss more" forever → the team stalls; decide with a revisit condition.
- **Authority over reasoning:** "because I'm the lead" destroys trust and growth.
- **Feedback avoidance (ruinous empathy):** letting quality slide to avoid discomfort; everyone loses.
- **No writing:** decisions in chat/heads only → repeated debates, lost context, single point of knowledge.
- **Assuming status is known:** stakeholders surprised by slips; over-communicate outcomes and risks.
- **Velocity as a productivity metric:** rewarding story points → estimation inflation and gaming.
- **Debt denial:** never paying down debt; every sprint is features until velocity collapses.
- **Meetings instead of unblocking:** most of your value is removing blockers and clarifying, not attending.
- **Confusing senior IC with lead:** still measuring yourself by personal output; switch to team output.

---

## The three questions to ask before any big change

1. What **driver** (pain) forces this? Can I name it without buzzwords?
2. How **hard is it to reverse**? If reversible, move fast; if not, be deliberate + write it down.
3. What's the **failure mode** I'm accepting, and who will notice it first?

If you can't answer all three, you're not ready to lead the change — yet.
