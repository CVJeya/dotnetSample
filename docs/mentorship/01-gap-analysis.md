# 1. Gap Analysis

Mapping assumed level → Technical Lead. Rating scale: **1 = heard of it, 3 = productive with help, 5 = can lead & teach it**.

## Engineer levels used here

| Level | Scope | Signature behaviour |
|---|---|---|
| L2 Junior | Task | Needs well-specified work, asks "how" |
| L3 Mid | Feature | Owns a feature end-to-end, asks "why" |
| L4 Senior | System/Area | Owns a service, designs within constraints, unblocks others, optimises for team |
| **L5 Tech Lead** | Team + Architecture | Owns *decisions and direction*, multiplies the team, manages ambiguity & trade-offs, accountable for delivery + quality |

You are assumed **solid L4 on code/SQL**, **L2–L3 on cloud & distributed design**, **L1–L2 on formal leadership**. The program closes the right-hand gaps without rebuilding what you already know.

## Skill gap table

| Area | You (now) | Lead target | Gap | Priority |
|---|---|---|---|---|
| C# / .NET correctness & idioms | 4 | 5 | Low | Medium |
| Async/await correctness at scale | 3 | 5 | Med | **High** |
| EF Core performance & data access | 3 | 5 | Med | **High** |
| API design (REST/Minimal/gRPC) | 4 | 5 | Low | Medium |
| Distributed architecture (DDD, CQRS, messaging) | 2 | 4 | **High** | **Critical** |
| Resilience & scalability patterns | 2 | 4 | High | **Critical** |
| Observability (OTel, tracing, metrics) | 1 | 4 | **High** | **Critical** |
| Cloud (Azure deep / AWS breadth) | 2 | 4 | High | **Critical** |
| Containers / Kubernetes | 2 | 4 | High | **High** |
| CI/CD + IaC + deploy strategies | 3 | 5 | Med | **High** |
| Security (OAuth2/OIDC, OWASP, secrets) | 2 | 4 | High | **High** |
| React / Angular depth | 3 | 4 | Med | Medium |
| Modern .NET / Aspire / Blazor | 2 | 4 | Med | Medium |
| AI-assisted development & RAG | 1 | 3 | Med | Medium |
| Code review (as a teacher) | 3 | 5 | Med | **High** |
| Technical decision-making / ADRs | 2 | 5 | **High** | **Critical** |
| Estimation & sprint planning | 2 | 4 | High | **High** |
| Mentoring & feedback | 2 | 5 | High | **High** |
| Stakeholder comms & design docs | 2 | 5 | High | **High** |

## The three real gaps (name them honestly)

1. **Thinking in failure modes, not in happy paths.** Senior = "I made it work." Lead = "here is how it breaks, and what we do then." Most of the architecture/cloud content below is really *failure-mode training*.
2. **Deciding under uncertainty and writing it down.** Leads are judged on decisions, not keystrokes. Hence the ADR/design-doc habit from week 1.
3. **Multiplying others.** Your output stops being code and becomes *the team's output*. Practice by reviewing code weekly and mentoring deliberately, not by accident.

## 90-day quick wins (do these regardless of the roadmap)

- Ship one ADR per week for real decisions in your current monolith (e.g. "Why modular monolith before microservices").
- Add structured logging + a correlation ID + one health check to the module you own.
- Cut one slow SQL query with a plan + index; document before/after ms.
- Introduce a `Directory.Build.props` with nullable + analyzers + warnings-as-errors on a new project.
- Review 2 PRs/week with a written rubric; note one teaching moment each time.

## How you'll know it worked

- You can whiteboard any of your services and answer "what happens when X dies?" for every dependency.
- You've shipped a containerised app through a pipeline to a cloud environment, with secrets in a vault.
- You've written 10+ ADRs someone else has read and agreed/disagreed with.
- A junior on your team says a specific thing you taught them changed how they work.
