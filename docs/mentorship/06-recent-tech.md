# 6. Recent Technologies

Keep this section **survey-deep, hands-on-shallow**: leads need to know what exists, what it's for, and when to adopt — not to master every shiny thing.

---

## 6.1 Latest .NET

- **Language:** primary constructors, collection expressions, `required` members, pattern matching improvements, `params` collections, inline arrays, `ref readonly`. Enable **nullable** everywhere + **warnings-as-errors**.
- **Runtime/ASP.NET:** rate limiting middleware (`AddRateLimiter`), output caching (`AddOutputCache`), `HybridCache`, metrics + OTel built-in, NativeAOT for CLI/edge, `TimeProvider` (testable time), `IAsyncEnumerable` streaming.
- **Practical lead stance:** adopt runtime-level features that remove third-party deps and reduce ops (output caching, rate limiting, HybridCache) — they cut cost and code.

```csharp
builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter("per-user", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 60;
        opt.QueueLimit = 0;
    });
});
app.UseRateLimiter();
```

- **Exercise:** replace a hand-rolled cache + throttle with `HybridCache` + rate limiter; delete the old code; measure the reduction.
- **Resource:** "What's new in .NET" + "What's new in ASP.NET Core" (Microsoft, per release).

## 6.2 .NET Aspire

- **What:** opinionated cloud-ready stack for local orchestration, service discovery, health checks, telemetry, and a dashboard for distributed apps. `AppHost` composes services + containers (Redis, Postgres, Service Bus emulator).
- **Why a lead cares:** it kills the "how do I run this 5-service app on my laptop?" problem and standardises telemetry. It's the fastest path from monolith to distributed dev experience.
- **When not:** tiny single-service apps (overhead), or if your team already has strong docker-compose/platform tooling.
- **Exercise:** add an `AppHost` to compose your week-8 service + a message broker + Redis, wire service discovery, and open the Aspire dashboard to see traces.
- **Resource:** learn.microsoft.com/dotnet/aspire + the `dotnet/aspire-samples` repo.

## 6.3 Blazor

- **Hosting models:** Blazor Server (SignalR, low latency, server resources), WebAssembly (client-side, download size, offline), **Blazor United / Auto** (.NET 8+ hybrid — SSR then interactivity), hybrids (MAUI).
- **Pick:** internal line-of-business app on a .NET team → Blazor often fastest. Public, SEO-heavy, huge JS ecosystem → React/Next. Interactive real-time dashboards → Blazor Server + SignalR or React + SignalR.
- **Exercise:** build one Blazor (Auto) page consuming your API, with streaming rendering, and add live updates via SignalR.
- **Resource:** Blazor docs "Hosting models" + .NET 8 Blazor "Auto" guide.

## 6.4 React Server Components / Next.js

- **RSC:** components render on the server, ship zero JS, stream HTML; client components opt in with `"use client"`. Next.js App Router = RSC + streaming + server actions.
- **Trade-off:** better perf/SEO/data-fetching, but a mental-model shift and tight vendor coupling; hosting must support Node/edge.
- **Exercise:** rebuild your W15 React list page as a Next.js App Router page with a server component fetching data + one client component for interactivity; compare bundle size.
- **Resource:** react.dev "Server Components" + Next.js "App Router" docs.

## 6.5 Angular signals

- **What:** fine-grained reactivity (`signal`, `computed`, `effect`, `linkedSignal`), enabling zoneless change detection and simpler state than RxJS for view state. RxJS still shines for async streams/events.
- **Lead stance:** new apps → signals-first for state, RxJS for event streams; don't rip RxJS out of a working app.
- **Exercise:** convert a component's state to signals and enable `ChangeDetectionStrategy.OnPush` (or zoneless); measure change-detection cycles.
- **Resource:** angular.dev "Signals" guide.

## 6.6 AI in development

- **Copilot / LLM in workflow:** use for boilerplate, tests scaffolding, code explanation, refactoring suggestions; **review every line**; never paste secrets/PII. Treat it as a fast junior.
- **LLM APIs / Azure OpenAI:** chat + embeddings + function/tool calling; streaming; token cost; rate limits; prompt templates; guardrails.
- **RAG basics:** ingest → **chunk** (semantic, overlap) → **embed** → store in a vector DB (Azure AI Search / pgvector / Cosmos) → **retrieve** top-k → ground the prompt → answer with citations. **Evals** (faithfulness, relevance) and **hybrid search + rerank** are where quality comes from.
- **Where RAG fails:** chunking splits meaning; retrieval misses; stale index; hallucinated citations; no eval = no idea if it improved; PII leakage; cost blowups.
- **Exercise:** a RAG mini-app over your own project docs using Azure OpenAI + AI Search (or pgvector), returning answers **with citations**; add a 20-question eval set and record the score before/after adding reranking.
- **Resource:** Microsoft "AI for .NET developers" + "Azure AI Search RAG" learn path.

## 6.7 Platform engineering / serverless / WASM / edge

- **Platform engineering:** internal developer platform (IDP) — golden paths, self-service templates, `dotnet new` templates + Backstage/Port; goal = reduce cognitive load, not add a gate.
- **Serverless:** Functions/Lambda for event-driven, spiky, glue work; watch cold starts, state, and vendor lock-in.
- **WASM:** Blazor WASM in-browser; WASI for edge/plugins; still maturing for server workloads.
- **Edge:** CDN compute (Cloudflare Workers/Azure Front Door Rules/AWS CloudFront Functions) for latency-sensitive, small logic; keep state out of the edge.
- **Exercise:** write a short "adopt / trial / assess / hold" tech-radar entry for each of these four for *your* team, with reasoning.

---

## How a lead adopts new tech (the honest framework)

1. **Driver first** — what pain does this remove? (If you can't name it, don't adopt.)
2. **Reversibility** — how expensive to back out? Prefer reversible bets.
3. **Bus factor** — can ≥2 people own it?
4. **Ops cost** — who runs it at 2am?
5. **Time-box a spike** — 1 week, throwaway, written results, then decide.
6. **Write the ADR** — including what you're *not* doing.
