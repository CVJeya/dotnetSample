# 4. Architecture & System Design

The lead skill is **deciding under constraints and being able to defend it.** Every section ends in a decision.

---

## 4.1 Microservices vs modular monolith

**Default recommendation for a team moving off a monolith: modular monolith first.** You already have `LHDN_STSDS/Modules/*` — that's the seam to exploit.

| Criterion | Modular monolith | Microservices |
|---|---|---|
| Team size | Small/medium (< ~15) | Multiple teams owning services |
| Deploy coupling | One pipeline (simpler) | Independent deploys (harder ops) |
| Transactions | Easy (one DB) | Sagas / eventual consistency |
| Failure modes | One blast radius | Partial failure everywhere |
| Debugging | One stack trace | Distributed tracing required |
| Cost | Low | Network + infra + cognitive |

Decision rule: extract a service only when you have a **real driver** — independent scaling, independent deploy cadence, different data ownership, or team autonomy. Otherwise modular monolith with strong module boundaries.

**Enforce boundaries:** one DbContext per module (or schema per module), no cross-module FK, communicate via in-process domain events now / messages later, `InternalsVisibleTo` only for tests.

---

## 4.2 DDD (tactical + strategic)

- **Strategic:** bounded contexts, ubiquitous language, context map, anti-corruption layer between contexts.
- **Tactical:** entities, value objects (`record`), aggregates + aggregate root, invariants, domain events, repositories, domain services, domain vs application vs infrastructure layers.
- **The line leads hold:** business rules live in the **domain**; orchestration in the **application**; IO in **infrastructure**. Controllers do nothing but translate HTTP ↔ application.

```csharp
public sealed record Money(decimal Amount, string Currency)
{
    public static Money operator +(Money a, Money b) =>
        a.Currency == b.Currency ? a with { Amount = a.Amount + b.Amount }
        : throw new DomainException("Currency mismatch");
}

public sealed class Order // aggregate root — the only entry point
{
    private readonly List<OrderLine> _lines = new();
    public OrderId Id { get; }
    public Money Total => _lines.Aggregate(Money.Zero, (s, l) => s + l.Subtotal);
    public void AddLine(ProductId p, int qty, Money price)
    {
        if (qty <= 0) throw new DomainException("Quantity must be positive");
        _lines.Add(new OrderLine(p, qty, price));
    }
}
```

- **Exercise:** model one bounded context in your domain (e.g. claims/orders) with 2 aggregates + value objects + a domain event; write 6 unit tests for invariants with no DB involved.
- **Resource:** "Domain-Driven Design Distilled" (Vernon) — 150 pages, perfect for a lead.

---

## 4.3 CQRS & Event Sourcing

- **CQRS:** separate write model (behaviour, invariants) from read model (denormalised, fast queries). Start with **CQRS-lite** (same DB, different models, read via projections) before separate stores.
- **Event sourcing:** store state as an ordered event log; rebuild aggregates by replay; snapshots for performance; projections to read models. *Only when* you need an audit trail / temporal queries / event-driven integrations. It's a heavy commitment — say so.
- **Exercise:** add a read model for a list endpoint via a projection handler that runs on a domain event; measure query latency vs the write-side model.
- **Resource:** "Implementing Domain-Driven Design" (Vernon) + Marten/Wolverine docs (great .NET event-sourcing examples).

---

## 4.4 Messaging

| Broker | Best for | Watch out |
|---|---|---|
| **Azure Service Bus** | Enterprise messaging, sessions, DLQ, scheduled, transactional | Cost, throughput limits |
| **RabbitMQ** | Flexible routing (exchanges/queues), self-hosted | Clustering/ops, no built-in replay |
| **Kafka** | High-throughput streaming, replayable log, event backbone | Ops complexity, ordering per partition only |
| **SQS/SNS** | AWS simple queues + pub/sub, serverless | No replay (SQS), FIFO limits, visibility timeout |

**Patterns you must own:**
- **Transactional Outbox:** write to DB + outbox in one transaction; a relay publishes; guarantees no lost messages on crash.
- **Idempotent consumer:** dedupe by message id; handlers safe to run twice.
- **Ordering:** per-key ordering (sessions / partition key), don't assume global order.
- **Poison messages:** DLQ + retry with backoff + alerting.
- **Exactly-once is a myth** end-to-end — you get at-least-once + idempotency.

```csharp
// Outbox: same transaction as the business change
await using var tx = await db.Database.BeginTransactionAsync(ct);
order.Confirm();
db.Outbox.Add(OutboxMessage.From(new OrderConfirmed(order.Id))); // same SaveChanges
await db.SaveChangesAsync(ct);
await tx.CommitAsync(ct);
```

- **Exercise:** outbox + idempotent consumer using Service Bus (or RabbitMQ locally via Testcontainers). Kill the consumer mid-processing and prove no data loss and no duplicate side effects.
- **Resource:** "Enterprise Integration Patterns" (Hohpe) + eShopOnContainers .NET reference app.

---

## 4.5 Resilience & scalability

- **Polly (resilience pipelines):** timeout → retry (exp backoff + jitter) → circuit breaker → bulkhead → fallback. Retry only **idempotent** operations; add jitter to avoid thundering herd.
- **Caching:** cache-aside, write-through, read-through; CDN/edge; HTTP caching (ETag, Cache-Control); distributed cache (Redis); `HybridCache` (L1+L2); invalidation is the hard part — define TTL + explicit eviction.
- **Scalability:** stateless services, horizontal scaling, backpressure, queue-based load levelling, database scaling (read replicas, sharding), connection pooling.
- **API gateway:** routing, authN/Z, rate limiting, request/response transformation; don't put business logic in the gateway (keep it "smart endpoints, dumb pipes").
- **Exercise:** wrap a flaky downstream call in a Polly pipeline; write a test that forces open-circuit and asserts fast-fail + fallback.
- **Resource:** Microsoft "Cloud Design Patterns" catalogue (Retry, Circuit Breaker, Bulkhead, Outbox, Saga) — free.

---

## 4.6 Saga

- **Choreography:** each service reacts to events (simple, decoupled, hard to see the whole flow).
- **Orchestration:** a saga orchestrator coordinates steps and **compensating actions** on failure.
- Use when a business transaction spans services; every step needs a compensating action (reserve → release, charge → refund).
- **Exercise:** implement a 3-step saga (Reserve stock → Take payment → Create shipment) with compensation when payment fails; assert compensating events fire.
- **Resource:** Microsoft "Saga distributed transactions" pattern doc.

---

## 4.7 Observability

Three pillars + the glue:
- **Logs:** structured (`ILogger` + scopes), correlation/trace IDs, log levels, no PII.
- **Traces:** OpenTelemetry (ASP.NET Core, HttpClient, EF Core auto-instrumented), W3C `traceparent` propagation across HTTP + messaging.
- **Metrics:** RED (Rate, Errors, Duration) for services, USE (Utilisation, Saturation, Errors) for resources; counters/histograms via OTel.
- **Backends:** App Insights / Azure Monitor, CloudWatch + X-Ray, Grafana + Tempo/Loki/Prometheus, Jaeger.
- **Alerting:** SLOs + error budget; alert on symptoms (latency/error rate), not causes; page on burn rate.

```csharp
builder.Services.AddOpenTelemetry()
  .ConfigureResource(r => r.AddService("orders-api"))
  .WithTracing(t => t.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation()
      .AddEntityFrameworkCoreInstrumentation().AddOtlpExporter())
  .WithMetrics(m => m.AddAspNetCoreInstrumentation().AddRuntimeInstrumentation().AddOtlpExporter());
```

- **Exercise:** instrument one service end-to-end so a single request shows up as one trace across HTTP → EF Core → message publish; build a dashboard with p95 latency + error rate + an alert.
- **Resource:** OpenTelemetry .NET docs + "Observability Engineering" (Majors et al.).

---

## Design-doc / ADR template (use every week)

```markdown
# ADR-000X: <decision>
Status: Proposed | Accepted | Superseded by ADR-00YY
Date: YYYY-MM-DD
## Context   — forces at play, constraints, what we know
## Options   — 2–4 real options, with trade-offs
## Decision  — the one we chose, in one sentence
## Consequences — good, bad, and the failure modes we accept
## Revisit if — the signal that would change this decision
```

A decision without a "Revisit if" is a guess. Leads write down **consequences** (especially the bad ones) — that's what separates a lead from a senior.
