# 2. Six-Month Roadmap (26 weeks × 8 hrs)

Structure of each week: **3 h learn · 3 h build · 1 h leadership · 1 h write/interview drill.**
Leadership is a **parallel weekly track** the whole way; the "Leadership" column shows the focus of that week's hour. Later weeks shift weight into the capstone.

## Phase overview

| Phase | Weeks | Theme | Outcome |
|---|---|---|---|
| 0 | 1 | Baseline & environment | Everything runs locally, tracking set up |
| 1 | 2–5 | .NET & data depth | Fast, correct services; performance instincts |
| 2 | 6–9 | Architecture & system design | Can design + defend a distributed system |
| 3 | 10–14 | Cloud, DevOps, security | Ships to cloud via pipeline with IaC |
| 4 | 15–17 | Frontend depth | Chooses & leads frontend architecture |
| 5 | 18–20 | Recent tech & AI | Modern toolchain fluency |
| 6 | 21–26 | Capstone + lead mode | Portfolio project + interview-ready |

## Week-by-week

| Wk | Technical focus | Build artifact | Leadership hour | Interview drill |
|---|---|---|---|---|
| 1 | Baseline: .NET 8/9 SDK, Docker Desktop, Azure free tier, `dotnet-tools.json` (already in repo) | Repo skeleton, `Directory.Build.props`, CI that just builds | Read `10-anti-patterns.md`; pick 3 habits to kill | Why be a lead? STAR story bank started |
| 2 | C# internals: value/reference, spans, records, pattern matching, nullable, GC basics | Console "benchmark playground" (BenchmarkDotNet: allocations of common ops) | How to give a technical opinion without being the loudest voice | "Explain async/await under the hood" |
| 3 | Async/await at scale: `ConfigureAwait`, `Task.WhenAll`, `CancellationToken`, `IAsyncEnumerable`, no `async void`, deadlocks | Parallel HTTP client that respects cancellation + timeouts + retries | Review an old PR of yours critically | "When is async the wrong choice?" |
| 4 | EF Core performance: N+1, projections, `AsNoTracking`, compiled queries, split queries, keyset pagination, raw SQL | Fix a real slow query in your monolith; measure before/after | Writing a review comment that teaches, not criticises | "Debug a slow endpoint top-down" |
| 5 | API styles: Minimal APIs, controllers, gRPC, versioning, OpenAPI, validation, ProblemDetails | Small service exposing REST + gRPC for the same use case | ADR #1: "Minimal APIs vs Controllers for new endpoints" | "REST vs gRPC — when which?" |
| 6 | Clean/onion/hexagonal; ports & adapters; DI lifetimes & pitfalls; configuration/options pattern | Refactor one module to ports & adapters with a real test | ADR #2: modular monolith vs microservices | "Where do you put business rules?" |
| 7 | DDD tactical: entities, value objects, aggregates, domain events; bounded contexts; modular monolith seams | Model a bounded context (Orders / Claims) with aggregates + domain events | Facilitation: run a 30-min design session on your module | "What's an aggregate and why does it matter?" |
| 8 | Messaging: outbox, idempotency, Azure Service Bus / RabbitMQ / Kafka / SQS-SNS; CQRS read models | Outbox + consumer with idempotent handler (Service Bus or RabbitMQ) | Estimation: Gantt/pert vs story points; do one estimate | "How do you guarantee exactly-once?" |
| 9 | Resilience & observability: Polly (retry, circuit breaker, timeout, bulkhead), caching strategies, OpenTelemetry | Add Polly + OTel traces/metrics to the week-8 service; dashboard | Communicating risk/slippage to a stakeholder | "Explain a circuit breaker to a PM" |
| 10 | Azure core: App Service, Functions, Container Apps, Key Vault, Storage, Entra ID, App Insights | Deploy one service to Azure with managed identity + Key Vault | Delegation: hand off a task with clear acceptance criteria | "Design a multi-tenant SaaS on Azure" |
| 11 | Containers & Kubernetes: Dockerfiles, multi-stage, AKS/Container Apps, probes, HPA, config/secrets | Containerise the service; deploy to AKS or Container Apps | Running a blameless post-incident review | "What happens on `kubectl apply` → running pod?" |
| 12 | AWS breadth: IAM, VPC basics, EC2/ECS/EKS, Lambda, S3, RDS, DynamoDB, SQS/SNS, CloudWatch | Deploy the *same* service to AWS (ECS Fargate or Lambda) — compare | Stakeholder comms: explaining cloud cost/benefit | "Azure vs AWS mapping" |
| 13 | IaC & pipelines: Terraform/Bicep, Azure DevOps Pipelines, GitHub Actions, envs, approvals | Bicep/Terraform for infra + CI/CD pipeline with environments | Decision-making: RACI and who decides what | "Blue-green vs canary — pick one" |
| 14 | Security: OAuth2/OIDC, JWT validation, Entra ID/Cognito, OWASP Top 10, secrets, SAST/DAST, container scanning, SBOM | Add OIDC auth + secrets + dependency/container scanning to the pipeline | Giving difficult feedback (role-play) | "How do you secure a public API end to end?" |
| 15 | React depth: hooks rules, state management (server vs client state), Suspense, performance, testing (RTL, Playwright) | React front end for your service: auth + list/detail + tests | Mentoring: pairing session with a junior | "How would you review a React PR?" |
| 16 | Angular depth: RxJS operators, signals, standalone vs NgModule, change detection, NgRx | Angular equivalent of the same screen + tests | Tech-debt: build a visible debt backlog & prioritise | "RxJS vs signals — pick one for a dashboard" |
| 17 | Frontend architecture: when React vs Angular, BFF pattern, contract-first, design systems | ADR: frontend framework choice with team-fit criteria | Stakeholder pitch: 10-min roadmap presentation | "How do you choose a frontend framework at scale?" |
| 18 | Modern .NET: latest language features, Minimal APIs growth, .NET Aspire orchestration, rate limiting, output caching | Wire your services into an Aspire AppHost with service discovery | Writing a one-page design doc for a non-engineer | "What problem does Aspire solve?" |
| 19 | Blazor + realtime UX; WebAssembly/edge basics; serverless patterns | Blazor Server/WASM page + SignalR live updates | Presenting a technical topic to non-technical leaders | "Blazor vs SPA — when?" |
| 20 | AI in dev: Copilot workflows, LLM APIs, Azure OpenAI, RAG basics, evals, prompt safety | RAG mini-app over your own docs; add an eval step | Ethical/practical AI adoption for a team | "Where does RAG fail?" |
| 21 | Capstone kickoff: architecture, ADRs, backlog, estimation | Capstone repo, ADRs, CI that builds | Sprint planning facilitation; estimate the capstone | "Estimate a 6-week feature" |
| 22 | Capstone build sprint 1 | Service 1 + 2 running locally via Aspire | Code-review rubric applied to 4 PRs | "Design review — defend your architecture" |
| 23 | Capstone build + hardening | AuthN/Z, resilience, caching, tests | Mentoring loop + feedback | "How do you handle a disagreement in design review?" |
| 24 | Capstone deploy + observability | Full pipeline to cloud, dashboards, alerts, load test | Executive demo (5 slides) | "Present your architecture in 5 minutes" |
| 25 | Interview marathon + polish | Portfolio READMEs, diagrams, demo video | Mock interview with a peer/manager | Full loop: technical + system design + behavioural |
| 26 | Retrospective + next 6 months | Written retro + 6-month plan | Career conversation with your manager | Re-test weakest answers |

## Weekly rhythm (fixed time blocks)

| Slot | Day | Content |
|---|---|---|
| A (3 h) | Mon/Tue | Learn: docs/course + notes |
| B (3 h) | Wed/Thu | Build: the week's artifact, committed |
| C (1 h) | Fri | Leadership track + ADR/design doc |
| D (1 h) | Sat | Interview drill out loud + retro (10 min) |

## Rules that make it stick

- **Ship weekly.** A committed artifact beats a finished course. If you can't finish, ship the smaller version.
- **Public by default.** Redact secret details and write about what you built (blog/internal wiki). Writing forces clarity.
- **One topic, one artifact.** Never learn two hard things at the same time in the same week.
- **Reuse your real job.** Prefer applying the week to the actual monolith over toy apps — real constraints teach more.
