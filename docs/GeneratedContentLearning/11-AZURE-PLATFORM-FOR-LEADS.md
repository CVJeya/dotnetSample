# Chapter 11 — Azure Platform Knowledge for a Microsoft Fullstack Technical Lead

**Why this chapter exists:** the title says *Microsoft*. Panels at Microsoft-aligned companies assume you can answer **"where does this run, how is it secured, how is it paid for?"** without a specialist. An ASP.NET Core developer normally learns this late; as a lead you cannot defer it.

This chapter is deliberately **decision-oriented, not documentation-oriented**. You will not be asked to recite App Service SKU names. You *will* be asked "why Container Apps and not AKS?" and "how do secrets get into the app?"

**Everything is grounded in this repo:** four Dockerfiles (`backend/AngularDotnetDemo.Api/Dockerfile`, `frontend/Dockerfile`, `ReactDotNetDemo/Dockerfile`, `DeliverFoodApp/Dockerfile.api`, `DeliverFoodApp/Dockerfile.consumers`), `docker-compose.yml`, `azure-pipelines.yml`, and an ASP.NET Core `net10.0` API.

---

## 1. The hosting decision — the question you will actually be asked

> "You have this repo. Where does it run in Azure, and why?"

### 1.1 The matrix

| Option | Best for | You get | You pay for | You must own |
|---|---|---|---|---|
| **App Service** | Monolith API, fastest path, team without k8s skills | Managed runtime, slots, easy scaling, built-in auth | Plan instance-hours | Little |
| **Container Apps** | 2–5 services, containers, scale-to-zero, jobs | Ingress, revisions, traffic splitting, KEDA autoscale, Dapr optional | vCPU/s + requests | Image + ingress config |
| **AKS** | Many teams, custom operators, fine-grained networking | Total control | Cluster + your time, seriously | **Everything**: ingress, certs, RBAC, upgrades |
| **Container Instances** | One-off jobs, quick demos | A container, no orchestration | Per second | Nothing |
| **Azure Functions** | Event handlers, timers, glue | Serverless trigger/binding model | Executions | Cold start, local dev parity |
| **Static Web Apps** | SPA + optional API | CDN, auth, PR previews, free TLS | Bandwidth | Almost nothing |

### 1.2 The answer for *this* repo (memorise the shape — it's a perfect demo)

> "Today it's four containers plus a compose file, so my first move is **Azure Container Apps**, not AKS. Reasons: it takes the same Dockerfiles with no change; it gives me per-service revisions, so a bad deploy is a revision rollback rather than a rebuild; it scales to zero for the React app that gets little traffic; and it doesn't hand me a Kubernetes cluster to babysit with three engineers.
> Concretely: one Container App environment; `AngularDotnetDemo.Api` as a Container App behind external ingress; `FoodDelivery.Consumers` as an internal-only app (no ingress); the Angular SPA as a Container App serving nginx **or** — better — as a **Static Web App** because the SPA is static files and doesn't need a container at all; Azure SQL for data; Service Bus or Event Hubs for the messaging that `FoodDelivery.Consumers` currently takes from Kafka. Managed identity everywhere, secrets in Key Vault, all provisioned in Bicep.
> I'd only move to AKS if we grew to several teams needing independent cluster-level control, or needed specific operators/service mesh — and I'd say that out loud as a reversal trigger."

**Why that answer wins:** it names a default, gives three concrete reasons, changes one thing deliberately (SPA → Static Web App), and states the trigger for the more complex option.

### 1.3 The anti-answer
"We'd put it on Kubernetes for scalability." — Immediately probed: *who does the cluster upgrades, ingress, cert-manager, HPA tuning, and on-call for the control plane with three engineers?* If you cannot answer that staffing question, you have chosen a technology you cannot operate, which is a lead-level failure.

---

## 2. Identity and secrets — the highest-signal topic in a Microsoft interview

This is where senior .NET candidates are most often exposed. Three rules:

1. **No secret ever in `appsettings.json`, an environment variable in a pipeline, or a repo.**
2. **Prefer no secret at all** — managed identity to Key Vault means there is nothing to rotate.
3. **Every app has its own identity with least privilege.**

### 2.1 The pattern (say it in this order)

```
Container App / App Service
  └─ system-assigned managed identity
       └─ role: "Key Vault Secrets User" on the vault (not Contributor)
            └─ reads secrets at startup, or via Key Vault reference in config
Azure SQL
  └─ Entra ID admin + contained DB user for the app's identity  → no SQL password
Storage / Service Bus
  └─ "Data Sender"/"Data Receiver" role assignments, no connection strings with keys
```

```csharp
// Program.cs — the modern .NET version: no secrets anywhere.
builder.Services.AddAzureClients(cfg =>
{
    var vaultUri = new Uri(builder.Configuration["KeyVault:Uri"]!);
    cfg.AddSecretClient(vaultUri);           // DefaultAzureCredential under the hood
});
// Locally it uses your `az login`; deployed it uses the managed identity. Same code.
```

"The phrase I want on the record: **connection strings with embedded keys are a secret-management failure, not a convenience.** `DefaultAzureCredential` gives me the same code path on a laptop and in production, so there is no 'works locally, fails in Azure' identity class of bug."

### 2.2 Key Vault vs App Configuration — know the split
| | Key Vault | App Configuration |
|---|---|---|
| Secrets, keys, certs | ✅ | ❌ |
| Feature flags | ❌ | ✅ |
| Non-secret settings, per environment | Possible but wrong | ✅ |
| Change without redeploy | Yes (with cache) | Yes (with refresh) |

Cost of getting this wrong: teams put 60 non-secret settings in Key Vault, hit the transaction bill, and cannot tell which of them are actually sensitive.

### 2.3 Authentication in the app (Entra ID end to end)

```
Browser  →  Entra ID (OIDC, PKCE)  →  id_token + access_token (aud = API)
Browser  →  API: Authorization: Bearer <access_token>
API      →  validates issuer, audience, signature, lifetime, scopes/roles
```

- **Two app registrations** — the SPA (public client, PKCE, no secret) and the API (exposes scopes/`app roles`).
- The SPA never holds a client secret. A secret in a browser is a public secret.
- **Authorisation lives in the API**, in policies — never in the client.
```csharp
builder.Services.AddAuthorizationBuilder()
       .AddPolicy("CanManageProducts", p => p.RequireRole("Admin", "CatalogEditor"));

[Authorize(Policy = "CanManageProducts")]
```
- **Access tokens are short-lived (≈1 h) and are not for authorisation decisions cached forever.**
- Say the mistake out loud, because you own it in this repo: *"our Angular interceptor does 401 → refresh → retry, which is right, but the refresh token handling and the queue-while-refreshing behaviour are the parts that break in production — two simultaneous 401s must not trigger two refreshes."*

This repo's `backend` already issues JWTs itself (BCrypt + `JwtBearer`); the lead's framing is:
> "Self-issued JWT is fine for a demo, but the moment we have SSO, multiple apps, MFA or conditional access, we move to Entra ID as the issuer. I'd plan that as an ADR rather than a rewrite: the API switches issuer/audience validation; the SPA switches to MSAL; our own user table keeps only application-specific data."

---

## 3. Messaging on Azure — pick the right one, on the record

This repo uses Kafka (via MassTransit) in `DeliverFoodApp`. On Azure the choice must be deliberate:

| Service | Model | Best for | Watch out |
|---|---|---|---|
| **Azure Service Bus** | Queues + topics/subscriptions, sessions, DLQ, scheduled | Business commands/events, ordering with sessions, transactions | Throughput ceiling vs Kafka; per-message pricing |
| **Event Hubs** | Partitioned log, Kafka-compatible endpoint | High-throughput streaming, telemetry, event sourcing | No per-message DLQ semantics; you own the consumer offsets |
| **Event Grid** | Reactive, push, ~24 h retry | Resource/domain event fan-out, near-real-time reactions | Not a durable work queue |
| **Storage Queues** | Simple, cheap | Huge volume of simple work items | 64 KB messages, at-least-once only |

**The lead answer:**
> "MassTransit over Kafka maps cleanly onto **Event Hubs' Kafka endpoint** if we must stay Kafka-compatible, or onto **Service Bus** if we want the managed DLQ, scheduled messages and topic subscriptions — which for a delivery system with per-order state transitions I'd actually prefer. The decisive question isn't throughput, it's **who owns the dead-letter and retry semantics**: Service Bus gives me that as a platform feature; with Kafka I write it. With three engineers, I take the platform feature."

### 3.1 The two messaging guarantees you must explain correctly
- **At-least-once is the default.** Therefore **every consumer is idempotent**: dedupe on `MessageId` or a business key, in the same transaction as the effect.
- **Ordering is per-partition/per-session only.** Never assume global order; model state transitions with a version/sequence check.

### 3.2 Outbox — the answer to "how do you avoid dual writes?"
```
BEGIN TRAN
  INSERT order
  INSERT outbox_message
COMMIT
-- background relay: read outbox → publish → mark sent
```
"There is no way to atomically write to SQL and publish to a broker. The outbox is not an architecture pattern for fun — it is the only correct way to do both without losing events or double-publishing."

---

## 4. Observability on Azure — three products, one job

| Need | Product | The lead's rule |
|---|---|---|
| Logs + metrics + traces for apps | **Application Insights** (workspace-based) | One per environment, not one per app |
| Central log analytics, queries, alerts | **Log Analytics workspace** | Control **retention** — this is usually the biggest surprise line on the bill |
| Dashboards / on-call | Azure Monitor, Workbooks, Action Groups | 5 alerts max to start; each must have a runbook |

### 4.1 The .NET wiring you should be able to write from memory
```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddEntityFrameworkCoreInstrumentation()
                       .AddAzureMonitorTraceExporter())
    .WithMetrics(m => m.AddAspNetCoreInstrumentation()
                       .AddAzureMonitorMetricExporter());
```
"Correlation is the whole point: one `traceId` from the Angular HTTP interceptor → the API middleware → the EF query → the Service Bus message → the consumer. Without that, an incident is archaeology."

### 4.2 The four things to measure, always
1. **Rate / errors / duration** per endpoint (RED).
2. **Dependency latency** — SQL and HTTP out (this is where the real p95 lives).
3. **Queue depth and DLQ count** — a growing DLQ is a silent outage of a workflow nobody is watching.
4. **Business metric** — orders/minute. The one alert infrastructure can't give you.

### 4.3 Cost trap to name
"Log Analytics ingestion is the classic Azure cost surprise. I set retention explicitly (30/90 days hot, then archive), sample verbose logs, and **never** log request bodies or PII. Observability that costs 40% of the platform bill gets switched off by finance, so it has to be engineered to be affordable."

---

## 5. Data on Azure — the choice, and the honest default

| Option | Use when | Don't use when |
|---|---|---|
| **Azure SQL** | Relational data, transactions, EF Core, team knows SQL | Massive multi-region write scale |
| **Cosmos DB** | Global distribution, unknown/huge scale, document shape | Your data is relational and you like JOINs — you'll pay for it in RU/s and in refactors |
| **PostgreSQL Flexible** | Cost-sensitive, OSS preference, EF Core fully supported | You need SQL Server-specific features |
| **Blob Storage** | Files, images, exports | You need queries |

**The lead line:** *"Cosmos is not 'SQL but faster'. If the domain is relational — orders, products, users with foreign keys — then modelling it in Cosmos means denormalising what the business thinks of as joins, and that cost lands on the team forever. I default to Azure SQL and only move for a stated scale or geo requirement."*

Also always name: **zone-redundant / geo-redundant backup, PITR window, and a restore you have actually performed.** "We have backups" without a tested restore is a belief.

---

## 6. The repo mapped to Azure (a concrete, screen-shareable answer)

| Repo artefact | Azure target | Note |
|---|---|---|
| `backend/AngularDotnetDemo.Api/Dockerfile` | Container App (external ingress) | Or App Service container; Container Apps for revision rollback |
| `frontend/Dockerfile` (Angular 22, nginx) | **Static Web App** (preferred) or Container App | Static files don't need a container; Static Web Apps gives CDN + PR previews free |
| `ReactDotNetDemo/Dockerfile` (Vite build) | Static Web App | Same reasoning; deployment slot per PR environment |
| `DeliverFoodApp/Dockerfile.api` | Container App, internal ingress | Only reachable from the consumers + gateway |
| `DeliverFoodApp/Dockerfile.consumers` | Container App, **no ingress** | Scale on queue depth via KEDA, not CPU |
| `docker-compose.yml` | **Dev only** | Compose is a laptop convenience; it is not a production topology |
| SQL Server in compose | **Azure SQL** | Managed identity auth, not `sa` |
| Kafka in compose | Service Bus or Event Hubs | See §3 |
| `azure-pipelines.yml` | Same file, extended | Add test stage, immutable tags, environments, approval |
| `appsettings.json` secrets | **Key Vault + managed identity** | Nothing sensitive in the image |
| JWT signing key | Key Vault / Entra ID | Rotation plan required |

**The line to deliver:** *"`docker-compose.yml` is a development tool. The production topology is a set of managed services with identities — and the gap between the two is exactly where production incidents come from."*

---

## 7. Networking and edge — the minimum a lead must not fumble

| Need | Service | One-line answer |
|---|---|---|
| Global HTTP entry, WAF, caching | **Azure Front Door** | "TLS termination, WAF, and a CDN in front of everything" |
| API gateway, keys, quotas, versioning | **API Management** | "When external consumers or per-client quota matter; otherwise it's cost without benefit" |
| Private access to SQL/storage | **Private endpoint + VNet integration** | "App talks to SQL over the private network; SQL has public access disabled" |
| Egress control | NAT gateway / firewall | "Predictable outbound IPs for partner allow-lists" |
| Service-to-service only | Internal ingress + Container Apps environment | "Consumers have no public endpoint at all" |

Interview-ready position: *"For a product with an internal SPA and one API, API Management is usually premature. Front Door plus the API's own auth and rate limiting gets you 90% of the value at 10% of the cost. I'd add APIM when there is a partner or a paid-tier consumer."*

---

## 8. Infrastructure as Code and environments

- **Bicep** (Azure-native, no state file) or **Terraform** (multi-cloud, mature). Either is fine; **choosing neither is the sin**.
- IaC lives in the same repo, deployed **by the pipeline** with a service connection using workload identity federation (no stored credentials).
- **Parameterise per environment**, never per-environment copies of the template.
- **Tagging standard** from day one: `env`, `owner`, `cost-centre`, `system`, `data-classification`. Retrofitting tags is miserable and finance will ask.
- **Resource group per environment** minimum. Subscription separation for prod if compliance requires it.
- **No manual portal changes** — if it must be changed now, change it in code the same day, or it will be lost at the next deploy.

Lead framing: *"If I can't rebuild the environment from the repo after a bad day, the environment is the source of truth and the repo is documentation. That's backwards, and it's an incident waiting for a date."*

---

## 9. Cost — the part of the job nobody trains engineers for

A lead is expected to hold a cost conversation. Five levers, in the order they usually pay:

1. **Right-size** — the App Service plan / Container App CPU is usually 2–4× what the load needs. Measure before you buy bigger.
2. **Retention and ingestion** — Log Analytics is often the top line for small systems. Sample, tier, expire.
3. **Environment sprawl** — dev/staging switched off out of hours; PR environments auto-deleted. Often 30% of the bill.
4. **Reserved capacity / savings plans** — 1–3 year commitment on the stable baseline, pay-as-you-go for the rest. Typically 30–60% off.
5. **Egress and cross-region traffic** — chatting services across regions quietly bills.

**The one-sentence version:** *"Every architectural choice has a monthly price, and part of my job is to know what it is — I keep the platform bill on the same dashboard as the error rate, because a design decision that triples the run cost is a design decision someone should have to defend."*

---

## 10. Governance for a small team (don't over-do it)

| Concern | Right-sized answer for a team of 3 |
|---|---|
| Access | Entra ID groups per environment; no shared accounts; break-glass documented |
| Deploy rights | Pipeline service connection; humans have Reader in prod by default |
| Change control | PR + green pipeline + approval on prod environment. Not a CAB meeting |
| Secrets | Key Vault + managed identity; rotation owner named per secret |
| Data classification | 3 levels (public / internal / personal); dictates logging and retention |
| Compliance evidence | Pipeline run history + ADRs + postmortems are the evidence trail. Keep them |

---

## 11. Chapter 11 — questions you must be able to answer out loud

1. Where would you host this application in Azure, and why not AKS?
2. How do secrets get into your application in production? Walk me through the identity chain.
3. Difference between Key Vault and App Configuration — and what goes where?
4. Entra ID: how does a SPA authenticate a user and call your API? What's in each token?
5. Why must the SPA not have a client secret?
6. Service Bus vs Event Hubs vs Event Grid — pick one for order events and defend it.
7. How do you guarantee a message isn't lost when the DB write succeeds but the publish fails?
8. Your consumer receives the same message twice. What must be true?
9. How do you correlate a failing user request across SPA → API → queue → consumer?
10. Cosmos DB or Azure SQL for an orders system? Why?
11. How do you keep the staging environment identical to production?
12. What would you change in this repo's `.yml` and `docker-compose.yml` before going to production?
13. How do you control Azure cost on a system like this? Name the top three levers.
14. A container works locally via compose but fails in Container Apps. What are the first four things you check? *(config/`ASPNETCORE_URLS`, identity/Key Vault access, ingress port, missing dependency service names — compose's DNS names don't exist in Azure)*
15. How do you roll back a bad deploy in Container Apps vs App Service vs AKS?

**Next:** `12-LEAD-INTERVIEW-PLAYBOOK.md` — the loop, the behavioural bank, and the negotiation.
