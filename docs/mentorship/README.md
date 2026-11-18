# Technical Lead Mentorship Program

A 26-week (~6 month), 8 hrs/week practical program to move from **Senior .NET Full-Stack Developer** to **Technical Lead**.

> **Status:** v1 draft. Built on the assumptions below — tell me what to correct and I'll tune every doc.

## Working assumptions (edit these)

| Field | Assumed value |
|---|---|
| Current level | 4 years, .NET full-stack (ASP.NET Core MVC + Razor), strong C#/SQL |
| Strongest areas | C#, MVC, SQL Server |
| Weakest areas | Cloud, distributed system design, leading people |
| Time available | 8 hrs/week × 26 weeks (≈ 208 hrs) |
| Job context | .NET 8 modular monolith (`LHDN_STSDS/`: Controllers, Modules, Services, Razor) moving toward services |
| Target role | Technical Lead (team ~4–8 devs) in 12–18 months |

## How to use this program

- **Weekly cadence (8 h):** 3 h learning/reading · 3 h hands-on build · 1 h leadership track · 1 h writing (ADR/design doc/**public notes**).
- **Rule of 3:** every topic must end in (1) a runnable artifact, (2) a written decision/doc, (3) something you can explain in 3 minutes to a non-expert.
- **Weekly interview drill:** answer the 2–3 questions in `09-interview-questions.md` **out loud**, on a timer, then compare against the model answer.
- **Definition of done for a topic:** you can (a) draw the architecture, (b) name the trade-offs, (c) name the failure modes, (d) ship a small demo, (e) cite one thing you got wrong and fixed.

## Document map

| File | Purpose |
|---|---|
| `01-gap-analysis.md` | Where you are vs a lead, prioritized gaps |
| `02-roadmap.md` | Week-by-week 26-week plan |
| `03-technical-depth.md` | C#/.NET, frontend, DB, security — concepts + exercise + resource |
| `04-architecture-system-design.md` | Microservices/modular monolith, DDD, CQRS, messaging, resilience, observability |
| `05-cloud-devops.md` | Azure, AWS, CI/CD, containers, IaC, DevSecOps |
| `06-recent-tech.md` | Latest .NET, Aspire, Blazor, RSC/Next.js, signals, AI/RAG, platform eng |
| `07-leadership.md` | Code review, ADRs, estimation, mentoring, stakeholders |
| `08-portfolio-projects.md` | 3 capstone portfolio projects |
| `09-interview-questions.md` | Weekly technical + leadership Q&A with model answers |
| `10-anti-patterns.md` | Common mistakes to avoid at each stage |
| `11-resources.md` | Curated books/courses/docs, minimal and high-signal |
| `12-build-your-own-agent-tool.md` | Build your own ADO-Code-style agent: work item → verified PR (the W20 bonus build) |
| `13-agentic-toolchain-vscode-extension.md` | Toolchain to install — incl. the **AI-agent stack** (model runtimes, frameworks, retrieval, evals, tracing) — + build a **deployable VS Code agent plug-in** (.vsix), step by step |
| `14-microsoft-extensions-ai.md` | **`Microsoft.Extensions.AI` explained** — what it is/isn't, then **10 steps to ship AI *inside* the ASP.NET Core app**: DI + middleware pipeline, chat endpoint, schema-constrained extraction into existing DTOs, embeddings/RAG, tool calling against real services, streaming (SSE vs Hangfire+SignalR), network-free tests, redaction/budget/audit middleware, IIS + Key Vault deployment |
| `15-prompting-an-ai-agent.md` | **The prompts, not the machine.** Four copy-paste prompt files: generate `AGENTS.md` from evidence (verified commands, cited conventions), audit + repair the git repo, build a `REPO-BRIEF.md` comprehension under a token budget, and the always-on answer protocol (classify → plan → verify → cite). Grounded in this repo's real defects: 91 ignored-but-tracked files, 1,187 stale branches, a 716 KB god file, a 1,384-line `Program.cs` |
| `16-any-repo-agent-prompt.md` | **One prompt for *any* repository** — the portable bootstrap: mandatory discovery phase, a 12-field **STACK CARD** with evidence per field, `UNKNOWN` as a legal answer, then `AGENTS.md` + `REPO-BRIEF.md`. Includes the stack adapter table (15 ecosystems), token-budget arithmetic, 10 degenerate-repo cases (monorepo/docs-only/empty/vendored), a 15-row failure table, and a 3-repo × 12-check eval. Read this first on an unfamiliar codebase; doc 15 explains *why* each rule exists |
| `17-free-learning-path-certifications.md` | **How to learn all of this for free, in order.** A 6-tier ladder (LLM literacy → structured output → agentic patterns → evals → ship) with a runnable artefact per tier; a master table of free resources tagged `Free + cert` / `Free, no cert` / `Free tier` / `Paid` and `[verify]`; the honest cost of each certification *plus* the routes that make the exam seat free; a 12-week schedule + a 4-week compressed track; 5 portfolio artefacts with the exact metric that proves each; a 10-row skip list; and an appendix of what could not be verified. Docs 12–16 build it — this teaches it |
| `18-fullstack-dotnet-course-map.md` | **What to pay for, and in what order.** The breadth your job description demands — C#, .NET, patterns/principles, architecture, Azure Functions, Azure, AWS, Key Vault, deployment, React, Angular, SQL Server/PostgreSQL/MySQL/Oracle — mapped to courses. Pluralsight as the one anchor (both clouds + real sandboxes, the reason it wins), Microsoft Learn for Azure authority, Dometrain for .NET judgement; the **Oracle correction** (this repo runs `Oracle.EntityFrameworkCore` — learn it, don't certify it), the React-vs-Angular decision, the 16-week plan with a deliverable per phase, the seven database skills that separate seniors, five portfolio artefacts, cost + cancellation discipline. Doc 17 covers free; this covers paid |

## Progress tracker

Copy this into your notes and tick weekly:

```
[ ] W1  Baseline + env            [ ] W14 Security + DevSecOps
[ ] W2  C#/.NET internals         [ ] W15 Frontend: React
[ ] W3  EF Core performance       [ ] W16 Frontend: Angular
[ ] W4  API styles (Minimal/gRPC) [ ] W17 Frontend: architecture choice
[ ] W5  SignalR + clean arch      [ ] W18 Recent .NET + Aspire
[ ] W6  Modular monolith + DDD    [ ] W19 Blazor + realtime UI
[ ] W7  CQRS + messaging          [ ] W20 AI in dev + RAG
[ ] W8  Resilience + caching      [ ] W21 Capstone kickoff
[ ] W9  Observability             [ ] W22 Capstone build
[ ] W10 Azure core                [ ] W23 Capstone hardening
[ ] W11 Containers + AKS          [ ] W24 Capstone deploy + demo
[ ] W12 AWS core                  [ ] W25 Interview marathon
[ ] W13 IaC + CI/CD + deploy strats[ ] W26 Retro + next 6-month plan
```
