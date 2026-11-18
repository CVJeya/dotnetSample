# 8. Portfolio Projects (Capstones)

Three projects that progressively combine every skill. **Project 3 is your flagship**; Projects 1–2 are warm-ups you can finish in the earlier phases. All three should live in public-ready repos with a README, an architecture diagram, ADRs, and a 3-minute demo video.

Every project must demonstrate the same non-negotiables:

- [ ] Architecture diagram (C4/context) + at least 5 ADRs
- [ ] AuthN/Z (OIDC), secrets in a vault, no secrets in git
- [ ] Observability: structured logs + traces + metrics + one alert
- [ ] CI/CD: build once → test → scan → deploy per environment with approvals
- [ ] IaC for all infra; reproducible from scratch
- [ ] Automated tests: unit + integration (Testcontainers) + one E2E
- [ ] Resilience: retry/circuit breaker, health checks, graceful degradation
- [ ] Cost estimate vs actual; load test with numbers

---

## Project 1 — "Modular Monolith → Services" E-commerce (Azure)

**Goal:** prove the migration path and the seams, not the shopping features.

- **Domain (minimal):** Catalog, Orders, Payments, Notifications.
- **Stage A:** modular monolith (.NET, one deploy) with strict module boundaries (schema-per-module, in-process domain events).
- **Stage B:** extract **Payments** and **Notifications** into services using the **outbox** pattern; keep Catalog + Orders in the monolith. Justify each extraction with an ADR (driver = independent scaling / different release cadence).
- **Azure stack:** Container Apps (or AKS), Service Bus, SQL + Cosmos (or Postgres), Key Vault, App Insights, Entra ID.
- **Demonstrate:** a payment flow as a **saga** with compensation; idempotent consumers; end-to-end trace across the boundary.
- **Deliverable:** ADR "Why we did *not* split Catalog" + before/after latency & deploy-independence metrics.
- **Skills:** modular monolith, DDD seams, outbox, saga, Service Bus, OTel, Azure, CI/CD.

## Project 2 — Multi-tenant SaaS Claims/Field-Service (AWS)

**Goal:** multi-tenancy, IAM discipline, and serverless breadth on the second cloud.

- **Domain:** tenants create claims; technicians upload photos; status notifications to customers.
- **Architecture:** API (ECS Fargate or Lambda + API Gateway), SQS for async work, S3 for photos, DynamoDB (single-table, tenant-partitioned) or Aurora Serverless, Cognito for tenant identity, EventBridge for events.
- **Multi-tenancy:** tenant id in every request context; row/partition isolation strategy written up + the trade-off (shared vs silo). Row-level security or partition-key scoping.
- **Security focus:** IAM least-privilege roles per service (not one god role), no long-lived keys (OIDC federation for CI), S3 pre-signed URLs, PII handling.
- **Pipeline:** GitHub Actions → build/test → Trivy + CodeQL + gitleaks → deploy to ECS/Lambda via Blue-Green or canary (CodeDeploy) with auto-rollback on alarm.
- **Demonstrate:** a load test that trips an alarm and rolls back automatically; tenant isolation tests.
- **Skills:** AWS, IAM, DynamoDB, SQS/SNS, serverless, canary, DevSecOps, multi-tenancy.

## Project 3 (Flagship) — "Policy Copilot": Realtime + AI Decision Platform

**Goal:** combine distributed design, realtime UX, AI, modern .NET, and full leadership artifacts.

- **Domain (pick one you know):** policy/claims documents with a Q&A assistant and an operations dashboard. Reuse your LHDN_STSDS domain if helpful.
- **Architecture (distributed, .NET Aspire-orchestrated):**
  - **Ingestion service:** upload → queue → chunk + embed → vector store (Azure AI Search or pgvector) + blob/S3.
  - **Query/RAG service:** hybrid search + rerank → LLM answer **with citations**; 20-question eval harness in CI.
  - **Operations API:** CQRS-lite (writes on SQL, read models for dashboards), SignalR push of live status/metrics.
  - **Notification service:** Service Bus/SNS consumer → email/webhook, idempotent + DLQ + alerting.
  - **Gateway:** rate limiting + auth (Entra ID/Cognito).
- **Frontend:** Next.js (RSC) dashboard **or** Angular signals dashboard — justify the choice in an ADR; consume the realtime SignalR stream; auth + role-based views.
- **Cross-cutting:** Aspire AppHost for local dev, OTel everywhere, HybridCache, Polly pipelines, health checks, blue-green or canary deploy, full IaC (Terraform or Bicep) + one-click environment.
- **AI quality:** eval dashboard (faithfulness/relevance/citation accuracy) that gates merges — this is what separates a toy from a product.
- **Leadership artifacts:** complete ADR set (≥8), a 1-page design doc for a non-technical leader, a risk register, a delivery plan with estimates vs actuals, and a blameless post-incident write-up of a failure you *deliberately* injected (chaos test).
- **Deliverable:** live demo (or a video), architecture diagrams, cost report, load-test numbers (p50/p95/p99), and a written retro.

---

## Skills-to-project matrix

| Skill | P1 | P2 | P3 |
|---|---|---|---|
| Modular monolith / DDD seams | ● | | ● |
| Microservices / saga / outbox | ● | | ● |
| Azure | ● | | ● |
| AWS | | ● | |
| Containers / K8s | ● | ● | ● |
| CI/CD + blue-green/canary | ● | ● | ● |
| IaC (Terraform/Bicep) | ● | ● | ● |
| Observability (OTel) | ● | ● | ● |
| Security / IAM / OIDC | ● | ● | ● |
| Frontend (React/Next/Angular) | | | ● |
| Realtime (SignalR) | | ● | ● |
| AI / RAG / evals | | | ● |
| Leadership artifacts (ADRs, docs, postmortem) | ● | ● | ● |

## How to present these in an interview

1. **Problem & constraints** (2 sentences) → 2. **Architecture** (diagram) → 3. **The 2–3 hardest decisions** and their trade-offs → 4. **What broke and how you fixed it** → 5. **Numbers** (latency, cost, test coverage) → 6. **What you'd change next.**
   Rehearse this to 5 minutes; the "hardest decisions" and "what broke" parts are what get you hired as a lead.
