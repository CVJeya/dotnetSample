# Technical Lead Mentorship Program — `docs/mentorship/`

Mentorship program for a .NET full-stack developer on the `LHDN_STSDS` system
(ASP.NET Core MVC, `net8.0`, Oracle + EF Core + Dapper, monolith → modular/services),
targeting the **technical lead** role. Budget: **8 h/week × 26 weeks**.

## Documents

| # | File | Status | What's in it |
|---|---|---|---|
| 03 | [`03-technical-depth.md`](./03-technical-depth.md) | ✅ **written** | .NET + EF Core deep-dive **grounded in the real `LHDN_STSDS` codebase** — 9 topics, 10 hands-on labs against real files, measured anti-pattern catalog, 8 interview drills with model answers |
| — | 01 gap analysis | ⏳ to generate | L2–L5 level model, skill-gap table, 90-day quick wins |
| — | 02 roadmap | ⏳ to generate | 26-week week-by-week plan |
| — | 04 architecture & system design | ⏳ to generate | modular monolith vs microservices, DDD, CQRS/ES, messaging, resilience, observability |
| — | 05 cloud & DevOps | ⏳ to generate | Azure deep / AWS breadth, containers, K8s, CI/CD, IaC, DevSecOps |
| — | 06 recent tech | ⏳ to generate | .NET Aspire, Blazor, RSC/Next, signals, AI/RAG, platform engineering |
| — | 07 leadership | ⏳ to generate | code review, ADRs, estimation, mentoring, stakeholder comms |
| — | 08 portfolio projects | ⏳ to generate | 3 capstone projects + skills matrix |
| — | 09 interview questions | ⏳ to generate | weekly technical + leadership bank (drills for the .NET/EF stream already in `03`) |
| — | 10 anti-patterns | ⏳ to generate | mistakes by stage (the .NET/EF subset is in `03` §12) |
| — | 11 resources | ⏳ to generate | one primary resource per topic (the .NET/EF subset is in `03` §15) |

> **Note:** the .NET + EF Core section was regenerated against the actual codebase and is the
> first document produced. The remaining files are pending; the roadmap (§14 of doc 03) already
> maps where the .NET/EF labs land in the 26 weeks, so doc `02` will slot in around them.

## How to work a lab

1. Read the topic in `03-technical-depth.md`.
2. Do the lab against the **real file** named in the topic (all labs are ≤ 2 h).
3. Every lab produces **a PR + an ADR** (architecture decision record).
4. Re-run the codebase measurement script (`03` §11) and keep before/after numbers.

By week 12 you have a portfolio of real, measured improvements to a regulated production system.

## Provenance

`03-technical-depth.md` was generated against `LHDN_STSDS` @ `56d70b61e`
(branch `eDScompoundCalculation`). Re-measure with §11 as the code changes.
