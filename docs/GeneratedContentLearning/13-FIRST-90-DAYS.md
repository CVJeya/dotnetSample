# Chapter 13 — Your First 90 Days (and how to use it to win the interview)

**Why a first-90-days plan wins a lead interview:** it is the only artefact that proves you understand the job is *entering a system you don't control*. A panel can't verify any claim about your past, but they can watch you reason about *their* future — and almost no candidate brings a plan.

Bring it to the hiring-manager round. Do not read it out. Use the *structure* in answers, and produce the one-pager if they ask "how would you approach the first few months?"

---

## 1. How to use this chapter in the interview

Three uses, in order of value:

1. **As the answer to "what would you do in your first 90 days?"** — the most common lead question and the easiest to win.
2. **As the answer to "you've found X problems — how would you fix this organisation?"** — case-study round.
3. **As a one-page handout** if you're at final stage and they invite follow-up. (Rare, but it converts well when appropriate; never send it unsolicited before the technical rounds.)

**The rule:** two-thirds of the plan is **listening and observing**, not changing. A candidate who arrives with a re-architecture plan in week one has told you they will not read the system before changing it. Interviewers are specifically listening for whether you know this.

---

## 2. The plan in one page

> **Days 0–30 — See the system.**
> Ship one small change end to end. Run every 1:1. Read the pipeline, the ADRs (or write them for what I find), and the last three postmortems. Deploy something on day 10 so my understanding is real, not narrated.

> **Days 31–60 — Establish the frame.**
> Write down what I found: the system map, the top five technical risks, the delivery baseline (lead time, change-failure rate, PR latency). Agree one improvement with the team and one with my manager. Start the recurring 1:1s and the review standard. Install the smallest viable gate: tests + a green-pipeline requirement.

> **Days 61–90 — Deliver one visible outcome.**
> One measurable improvement shipped — usually the test gate, the framework consolidation decision (ADR), or the pipeline fix. Publish the roadmap for the next two quarters. Review my own first 90 days with the team, openly.

**The line that closes it:** *"My first 90 days are mostly diagnosis, one small win that builds trust, and a written plan for what comes next. I don't reorganise anything I haven't operated."*

---

## 3. Days 0–30 — See the system (detail)

### 3.1 The learning agenda (what you're actually trying to learn)

| Domain | Questions I need answered | How |
|---|---|---|
| **Business** | What does the product do, who pays, what's the growth constraint, what's the most expensive failure? | Talk to the PM, read the last two quarters of roadmap, look at support tickets |
| **People** | Who knows what? Who's disengaged? Who's the informal leader? Who's overloaded? | 1:1s with everyone, in the first two weeks |
| **Technical** | What's the deploy path? Where's the complexity? What's fragile? Where's the data? | Read the pipeline, run it, read the code, deploy |
| **Delivery** | How do we plan, how often do we ship, what slips and why? | Attend all ceremonies; read the last three sprints |
| **Operational** | What pages? What broke recently? What's the recovery story? | Read postmortems, join an on-call shift as shadow |
| **Relationships** | Who depends on us, who do we depend on, who's frustrated? | One coffee with each adjacent team lead |

### 3.2 The 1:1 template for day 1–10 (every person on the team, 30 min)

Ask these five, in this order, and write everything down:
1. "What are you working on, and what's in your way?"
2. "What's the most annoying thing about working here that nobody fixes?"
3. "What would you want a new lead to *not* change?"
4. "What should I know that's not written down anywhere?"
5. "What do you want to be doing in 18 months?"

Question 3 is the most important and the least asked. It's where you learn what's load-bearing that you'd otherwise break.

### 3.3 Ship something in week 2

Do a small, real change through the full path: branch → PR → review → pipeline → deployed. This single act:
- validates the onboarding doc (and reveals if it's broken),
- teaches you the actual (not documented) process,
- shows the team you're not a manager who stopped coding,
- gives you credibility to criticise the pipeline afterwards — because you've run it.

**Say this in the interview:** *"In week two I'd want to have shipped one small change end to end, because you cannot critique a delivery process you haven't personally used."*

### 3.4 What you do NOT do in the first 30 days
- Reorganise, rename, or re-architect anything.
- Kill a ceremony because you think it's wasteful (understand why it exists first — someone added it for a reason, usually a specific bad memory).
- Make a framework or vendor decision.
- Criticise the previous lead (you don't know their constraints, and it makes you look unsafe to confide in).
- Promise dates.
- Hire.

---

## 4. Days 31–60 — Establish the frame

### 4.1 The five artefacts you produce

1. **A one-page system map** (C4 container level, Mermaid, in the repo) — so everyone has the same picture, including the new joiner.
2. **A risk register** — top 5 technical risks, each with likelihood, impact, and the cheapest next check. Not a 40-item audit.
3. **A delivery baseline** — lead time, deploy frequency, change-failure rate, PR review latency, flaky test count. Just measure; no targets yet. You cannot claim improvement without a baseline, and you cannot set credibly ambitious targets either.
4. **A written working agreement + DoD** — co-created with the team, in the repo, versioned. Not imposed.
5. **An ADR or two** recording the decisions that already exist implicitly, so future-you has a trail.

### 4.2 The two commitments

- **One improvement with the team** — usually the test gate or PR review standards, because both are cheap and both are felt immediately.
- **One agreement with your manager** — "here's what I think matters most, here's what I'm going to do about it, here's how you'll know it worked." This is how you convert your own priorities into *their* priorities, which is the whole game of leading upward.

### 4.3 The conversation that defines your relationship with your manager

Ask in the first two weeks, explicitly:
- "What would make you say, in six months, that hiring me was a good decision?"
- "What's the thing you personally lose sleep about on this team?"
- "How do you want to be told about problems — immediately, or with options ready?"
- "What's my decision space? Where do I not need to check in?"

Leads who never ask the fourth question either escalate everything or surprise their boss. Both end badly.

---

## 5. Days 61–90 — One visible outcome, then the plan

### 5.1 The outcome should be a *mechanism*, not a heroic effort

Good first outcomes (all cheap, all permanent):
- A green-pipeline merge gate with integration tests on the two riskiest endpoints.
- PR review SLA + standards, with measured latency improvement.
- The framework-consolidation ADR, decided and communicated.
- A pipeline fix: test stage, immutable tags, no push on PRs (Chapter 10 §4.2 gives you eleven findings ready to go).
- A 5-alert baseline with runbooks.

Bad first outcome: "I rewrote the authentication layer." High risk, low trust, undefined scope, and it makes you the bottleneck.

### 5.2 Then publish the quarter's plan
Four things, on one page:
1. **The outcome** you're committing to (business-shaped).
2. **The top three risks** and what de-risks each, early.
3. **What you are deliberately not doing** — the most senior line on the page.
4. **How you'll know** — the metric and the date you'll review it.

### 5.3 The 90-day review with the team
Sit down with the team and ask: *"What did I get wrong in my first three months, and what should I do differently?"* Then act on one answer visibly. This is the highest-trust action available to a new lead, and it costs 45 minutes.

---

## 6. Applying the plan to *this* repo (your concrete answer)

If the interviewer says "this is roughly our stack" — and you have the repo in front of you — here is the first 90 days, specific:

| Week | Action |
|---|---|
| 1 | Read `frontend/`, `ReactDotNetDemo/`, `backend/`, `DeliverFoodApp/`; draw the container map; run `docker-compose up`; read `azure-pipelines.yml` |
| 2 | Ship one small change through PR → pipeline; measure the pipeline end to end; write down every friction point |
| 3 | 1:1s with all three engineers; read the last three incidents and any ADRs (probably zero — that's the first gap) |
| 4 | Baseline: build duration, PR latency, change-failure rate, flaky tests, warnings count, dependency vulnerabilities |
| 5 | Top-five risk register → present to manager with a recommendation, not a list |
| 6 | Add a test project + 3 integration tests on the riskiest endpoints (Testcontainers); make the pipeline gate on it |
| 7 | ADR: consolidate on one front-end framework; communicate the decision and the migration shape |
| 8 | Pipeline: test stage, immutable SHA tags, no PR pushes, caching, smoke test post-deploy |
| 9 | Migration slice 1 planned: auth seam via shared OIDC, contract-first client, route flag |
| 10 | Ship the first migrated slice behind a flag; publish the 2-quarter roadmap; 90-day retrospective with the team |

**Note what this does in the room:** it proves you can read a real codebase, find the genuine gaps (no tests, no ADRs, PRs pushing images, two front-end frameworks), and sequence the fixes by risk rather than by preference. That is the whole lead exam in one table.

---

## 7. The Lead Readiness Scorecard

Self-assess honestly. Score each 1–5 (**1** = can't discuss, **3** = can discuss and defensibly answer, **5** = have done it and can show evidence/results). Date it, re-score in four weeks.

### Technical credibility (weight: essential)
| # | Item | Ch | Score /5 |
|---|---|---|---|
| 1 | Angular standalone + signals + DI lifetimes, explained with trade-offs | 1 | |
| 2 | RxJS flattening operators, guards, functional interceptors | 2 | |
| 3 | React hooks, reconciliation/`key`, StrictMode, React 19 Actions/`useActionState`/`useOptimistic` | 3 | |
| 4 | RTK vs TanStack Query, Router 7 guarding, FE module boundaries | 4 | |
| 5 | ASP.NET Core DI, EF Core (N+1, transactions, concurrency), async correctness, JWT/OIDC | 5 | |
| 6 | Distributed systems: outbox, idempotency, saga, retries, SignalR | 5 | |

### Architecture & delivery (weight: differentiating)
| # | Item | Ch | Score /5 |
|---|---|---|---|
| 7 | Write an ADR with rejected alternatives and accepted costs | 9 | |
| 8 | Choose monolith vs modular monolith vs services with an argument | 9 | |
| 9 | Plan and defend a strangler-fig migration (auth seam first) | 9 | |
| 10 | Govern API contracts, versioning, error model, generated clients | 9 | |
| 11 | Critique a real CI/CD pipeline and sequence the fixes by risk | 10 | |
| 12 | State a testing strategy you'd actually enforce, and what you'd skip | 10 | |
| 13 | Run an incident and a blameless postmortem with owned actions | 10 | |
| 14 | Use SLOs/error budgets and DORA metrics in a business conversation | 10 | |

### Platform (weight: expected for "Microsoft")
| # | Item | Ch | Score /5 |
|---|---|---|---|
| 15 | Choose hosting (App Service / Container Apps / AKS) with a defended reason | 11 | |
| 16 | Explain managed identity → Key Vault → no secrets anywhere | 11 | |
| 17 | Entra ID / OIDC flow end to end, and why the SPA has no secret | 11 | |
| 18 | Service Bus vs Event Hubs vs Event Grid, chosen with a reason | 11 | |
| 19 | Correlate a request across SPA → API → queue → consumer | 11 | |
| 20 | Name the top three levers on an Azure bill | 11 | |

### Leadership (weight: the actual gate)
| # | Item | Ch | Score /5 |
|---|---|---|---|
| 21 | Run a 1:1 that isn't a status report | 8 | |
| 22 | Deliver hard feedback with SBI and a follow-up date | 8 | |
| 23 | Delegate with a brief and a stated boundary | 8 | |
| 24 | Set code review standards (size, SLA, taxonomy) and enforce them | 8 | |
| 25 | Turn around an underperformer without losing the person or the team | 8 | |
| 26 | Decide between two engineers' opposing positions and own it | 8 | |
| 27 | Say no to a stakeholder with options and a recommendation | 8 | |
| 28 | Explain why a team of three is real leadership scope without inflating it | 12 | |
| 29 | Tell a failure story with a thinking-change, not an apology | 12 | |
| 30 | Bring a 90-day plan to the hiring-manager round | 13 | |

### Scoring guidance
- **Under 90 total** → don't book lead interviews yet; work the chapters in the order of your lowest scores.
- **90–110** → interview-ready for senior; lead rounds will be marginal.
- **111–130** → lead-ready. Expect to pass technical rounds and be judged on stories.
- **131+** → make sure you're not over-preparing and under-sleeping. Rehearse out loud instead of reading more.

**The honest bias to watch:** most .NET candidates score 4–5 on 1–6 and 2–3 on 21–30. The gate is not technical. Fix the right column.

---

## 8. The four-week rehearsal schedule (do this, don't just read)

| Week | Focus | Daily 45 min | Output |
|---|---|---|---|
| 1 | **Stories** | One behavioural story out loud, recorded, then re-recorded | 12 stories in bullet form, with numbers |
| 2 | **Architecture** | Whiteboard one design per day from memory (100 s each) | The repo mapping + the migration plan, twice from memory |
| 3 | **Platform + delivery** | Critique the pipeline; explain identity chain; 10 Azure questions/day | 11 pipeline findings and the hosting answer, fluent |
| 4 | **Full loop simulation** | One 60-min mock: 90-second story, 4 random behavioural, 1 design, 3 salary questions | A written list of every stumble, then re-drill only those |

### If the loop includes a coding round (add this column)

Most Microsoft fullstack lead loops include at least one **live coding or code-review round**, and it is the one candidates prep least. Run this in parallel — 45 min/day, all four weeks:

| Week | Coding focus | Source | Self-grade |
|---|---|---|---|
| 1 | Warm-ups P1–P11, narrated out loud | `05C` §1–§2 | `05C` §11 rubric |
| 2 | P12–P25 + the 10-minute algorithm-trigger drill | `05C` §3–§6, `05B` §4 | 6 yeses in `05C` §11 |
| 3 | **P26–P36 (LINQ/EF, async, SQL) — your differential. Aim for flawless.** | `05C` §7–§9 | Would you approve it in a PR? |
| 4 | Two timed 45-min mocks on unseen problems, on camera | any §2–§8 pair | Re-record failures the next day |

**Also do once, before week 2:** answer all 25 questions in `05A` §11 out loud. It takes 40 minutes and it closes the language gate — which is the cheapest credibility purchase available to you, because the answers come from 6 years of real .NET, not from memorisation.

**Re-recording the same problem beats doing a new one.** You are training narration and structure, not solutions.

**Two rules:** (1) **out loud, recorded** — reading silently builds recognition, not recall, and interviews are recall; (2) **one story, one number** — if a story has no number, it's an opinion about yourself.

---

## 9. The final consolidation — what to hold in your head on the day

If you remember nothing else from these thirteen chapters, remember these nine sentences:

1. **"Five years of .NET, two years of SPA on top of it"** — never inflate the front end.
2. **"I own delivery and technical direction for three engineers"** — scope, not headcount.
3. **Every answer ends on a trade-off** — what you gave up and why you accepted the cost.
4. **Every decision has a rejected alternative and a reversal trigger.**
5. **Quality is enforced by mechanisms, not intentions** — build gates, arch tests, DoD, ADRs.
6. **Deploy ≠ release** — flags decouple them, and that's what makes small batches possible.
7. **Boundaries are real only if a test fails when you cross them.**
8. **Detection is cheaper than prevention and always under-invested** — the honest incident story.
9. **My first 90 days are diagnosis, one mechanism, and a written plan.**

Good luck. You already own the harder half — the depth. These chapters exist to make your vocabulary match it.

---

*End of the Learning set: `00`–`13`. Start at `00-START-HERE.md` for the reading order and the study plan.*
