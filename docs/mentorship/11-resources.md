# 11. Curated Resources

High-signal only. One primary per topic; don't collect — finish. Prefer official docs (they're now excellent for .NET/Angular/React) plus one book per discipline.

## .NET / C#
- **Microsoft Learn:** ASP.NET Core, EF Core, C# language reference (primary).
- **"Async guidance"** — David Fowler (one page that prevents 80% of async bugs).
- **"Dependency Injection" / "Configuration"** — Microsoft docs.
- **Book:** *C# in Depth* (Skeet) — or *Pro .NET Memory Management* (Kokosa) for perf depth.
- **Book:** *Dependency Injection Principles, Practices, and Patterns* (Seemann & van Deursen).
- **Blogs:** Stephen Cleary (async), Andrew Lock, Steve Smith (Ardalis), Nick Chapsas (video, .NET perf).

## Testing
- **Microsoft docs:** integration tests with `WebApplicationFactory`.
- **Testcontainers for .NET** docs.
- **xUnit** + **FluentAssertions**; **Playwright for .NET** or **Playwright** for E2E.
- **Book:** *Unit Testing Principles, Practices, and Patterns* (Khorikov).

## Databases
- **"How to Think Like the SQL Server Engine"** — Brent Ozar (free) + **Use The Index, Luke!** (Markus Winand).
- **Book:** *Designing Data-Intensive Applications* (Kleppmann) — read ch. 2–3, 5–7, 9.
- **Cosmos DB / DynamoDB:** official "partition key design" / "single-table design" guidance (Alex DeBrie's writing for Dynamo).

## Architecture & System Design
- **Book:** *Domain-Driven Design Distilled* (Vernon) → then *Implementing DDD* (Vernon).
- **Book:** *Software Architecture: The Hard Parts* (Ford, Richards) — trade-off focused, perfect for a lead.
- **Book:** *Building Evolutionary Architectures* (Ford et al.); *Fundamentals of Software Architecture* (Richards & Ford).
- **Microsoft:** Cloud Design Patterns catalogue (Retry, Circuit Breaker, Outbox, Saga, etc.) — free.
- **Reference app:** **eShopOnContainers / eShop** (.NET) and **.NET Aspire samples**.
- **Book:** *Enterprise Integration Patterns* (Hohpe) — as a reference.

## Resilience / Observability
- **Polly** docs + Microsoft "resilience" guidance.
- **OpenTelemetry .NET** docs.
- **Book:** *Observability Engineering* (Majors, Fong-Jones, Miranda).
- **Google SRE Book** (free online) — SLOs, error budgets, alerting.

## Cloud & DevOps
- **Microsoft Learn** AZ-204 / AZ-305 learning paths; **AWS Skill Builder** (Solutions Architect Associate).
- **Terraform** docs + **Bicep** docs.
- **Azure DevOps Pipelines** + **GitHub Actions** docs; "Deployment patterns" (Microsoft).
- **Book:** *The Phoenix Project* (why flow/DevOps) → *The DevOps Handbook* (how) → *Accelerate* (data/evidence).
- **Container security:** Trivy, CodeQL, OWASP ZAP docs.

## Security
- **OWASP Top 10** (2021) + **OWASP ASVS**.
- **Book:** *OAuth 2 in Action* (Richer & Sanso).
- **Microsoft:** Entra ID / MSAL docs; **AWS:** IAM best practices.
- **Tool:** `gitleaks` (secrets), **Snyk/Dependabot** (deps).

## Frontend
- **react.dev** (new docs) + **TanStack Query** docs + **Testing Library**.
- **angular.dev** (new docs, signals/RxJS guides).
- **Book:** *RxJS in Action*; *Fullstack React* (community).
- **Next.js** "App Router" docs (RSC).
- **Perf:** Chrome DevTools + Web Vitals.

## AI / LLM / RAG
- **Microsoft:** "AI for .NET developers", Azure OpenAI, **Azure AI Search RAG** learn path.
- **Concept:** "Building LLM apps" / prompt engineering guide; **RAG evaluation** (faithfulness/relevance).
- **Tooling:** Semantic Kernel / Microsoft.Extensions.AI (unified .NET LLM abstraction).
- **Use it in the product (.NET):** → [`14-microsoft-extensions-ai.md`](14-microsoft-extensions-ai.md) — `Microsoft.Extensions.AI` explained, then a 10-step ASP.NET Core walkthrough: DI + middleware pipeline, a chat endpoint, schema-constrained extraction into your own DTOs, embeddings/RAG, tool calling, streaming (SSE vs Hangfire + SignalR), fake-client tests, redaction/budget/audit middleware, IIS + Key Vault deploy.
- **Make the agent behave:** → [`15-prompting-an-ai-agent.md`](15-prompting-an-ai-agent.md) — the four prompts that drive docs 12–14: generate `AGENTS.md` from evidence rather than filenames, audit the git repo (`git ls-files -i -c --exclude-standard` must print nothing), build a `REPO-BRIEF.md` within a hard token budget (outline-then-range on 700 KB god files), and the always-on answer contract (classify → plan → `path:line` or `[unverified]` → verify). Ships the assembled system prompt, a 15-row failure-mode table, and a 20-task eval set.
- **One prompt for *any* repository:** [`16-any-repo-agent-prompt.md`](16-any-repo-agent-prompt.md) — discovery-first bootstrapping (STACK CARD, `UNKNOWN` allowed), stack adapter table for 15 ecosystems, token-budget arithmetic, monorepo/docs-only/empty-repo handling, and a 3-repo eval with 12 pass checks. Start here when the repo is not the one doc 15 was verified against.
- **Build one yourself:** → [`12-build-your-own-agent-tool.md`](12-build-your-own-agent-tool.md) — step-by-step: how to build a tool like the agent you are using now (ADO access → repo tools → agent loop → worktree/PR → verification).
- **Learn it free, in order:** [`17-free-learning-path-certifications.md`](17-free-learning-path-certifications.md) — the AI *learning* track that pairs with every doc above: a 6-tier ladder ending in a runnable artefact, which free courses actually issue certificates (and which are better without one), honest certification costs plus the free-voucher routes, a 12-week schedule, 5 portfolio artefacts with metrics, and the 10 traps that waste 100 hours.
- **Ship it as a plug-in:** → [`13-agentic-toolchain-vscode-extension.md`](13-agentic-toolchain-vscode-extension.md) — Part A: the toolchain to install (VS Code, Node, `vsce`, esbuild, test-cli, in-tenant model access); Part B: step-by-step build of a packaged, versioned, deployable `.vsix` VS Code extension.
- **AI-agent stack to install:** → [`13-agentic-toolchain-vscode-extension.md`](13-agentic-toolchain-vscode-extension.md) §1.6–1.8 — model runtimes (Azure OpenAI in-tenant, Ollama, Foundry Local), orchestration (Semantic Kernel / `Microsoft.Extensions.AI`, LangGraph, Pydantic AI), retrieval (pgvector, `sqlite-vec`, Azure AI Search), evals (promptfoo, Ragas, DeepEval), tracing (Langfuse, Phoenix, OTel GenAI), plus the .NET/TS/Python track table and a `ai-stack-check.ps1` verifier.
- **Microsoft.Extensions.AI, end to end:** → [`14-microsoft-extensions-ai.md`](14-microsoft-extensions-ai.md) — the abstraction layer, the middleware pipeline (PII redaction → tool loop → token budget), structured output via JSON schema, and the honest verdict on which *existing* AI integration in this repo to leave alone.

## Courses — what to actually buy

- **Full-stack .NET breadth (paid):** → [`18-fullstack-dotnet-course-map.md`](18-fullstack-dotnet-course-map.md) — no single course covers C# + patterns + architecture + Azure + AWS + Key Vault + React + Angular + four databases, so this maps the *purchases*: **Pluralsight** as the one anchor (both clouds, real sandboxes, React/Angular, three DBs), **Microsoft Learn** for Azure authority and AZ-204, **Dometrain** for the .NET judgement a senior reviewer marks you on. Includes the Oracle correction (§5.1 — this repo's EF provider *is* Oracle), the React-vs-Angular decision, a 16-week deliverable-per-phase plan, the seven skills that actually define "knows a database", five portfolio artefacts, total cost, and the cancellation rule.
- **Free-first equivalent:** → [`17-free-learning-path-certifications.md`](17-free-learning-path-certifications.md). Read 17 first; buy from 18 only where 17's free options fail you.

## Leadership & career
- **Book:** *The Manager's Path* (Fournier) — even if you stay IC/tech-lead.
- **Book:** *An Elegant Puzzle* (Larson) — engineering org design & systems thinking.
- **Book:** *Radical Candor* (Scott) — feedback. *Crucial Conversations* — conflict.
- **Book:** *Accelerate* (Forsgren et al.) — measure what matters.
- **ADRs:** Michael Nygard's original ADR post + adr.github.io templates.
- **Estimation:** *Software Estimation: Demystifying the Black Art* (McConnell).
- **Writing:** internal wiki + (optionally) a blog; Amazon-style "narrative" one-pagers.

## Newsletters / staying current
- **.NET Blog**, **InfoQ**, **ByteByteGo** (system design), **React/Angular official blogs**.
- Deliberately limit input: 2 newsletters, 1 podcast, 30 min/week — depth beats breadth.

---

## How to consume this without drowning

- **One primary resource per week**, tied to the roadmap week. If a topic has a book and docs, use docs to *do* and the book to *understand*.
- **Active over passive:** build the artifact; skim videos at 1.5×; write your own summary (the Feynman test).
- **Re-read your own ADRs and notes** at month-end — that's your curriculum, personalised.
