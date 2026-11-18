# 7. Leadership Skills

The promotion to lead is won here, not in `Program.cs`. Practise weekly; a lead's product is the **team's output and clarity**, not personal throughput.

---

## 7.1 Code review (as a teacher, not a gate)

- **Goal:** improve the code *and* the author. Label comments:
  - `[blocking]` must change (bug, security, correctness)
  - `[suggestion]` would be better, author's call
  - `[nit]` style/preference — don't block on it
  - `[question]` genuine "help me understand"
  - `[praise]` call out good work explicitly
- Review the **smallest thing**: scope, correctness, tests, security, readability, observability. Don't rewrite in comments — if it's big, pair.
- **Rule:** review PRs within one business day; small PRs (< 400 lines) get better reviews — say so and split.
- **Exercise:** adopt the labels for 4 weeks; track how many `[blocking]`s you file and how many bugs you catch pre-merge.

```text
[blocking] This query runs inside the loop → N+1. Use a projection + single query.
[suggestion] Consider a value object for Money; it'd remove the currency checks in 3 places.
[praise] Nice use of a domain event here — that keeps the module boundary clean.
```

## 7.2 Technical decision-making & ADRs

- Decisions live in **writing** (see `04-architecture` ADR template). Two-way-door vs one-way-door decisions: move fast on reversible ones, be deliberate on irreversible ones.
- **Deciding is a service to the team.** "We don't know yet" is fine — pair it with "here's the spike and the date."
- Disagree-and-commit: voice disagreement, then support the decision. Model this publicly.
- **Exercise:** write 1 ADR/week for 26 weeks. Offer 3 of them for review by a peer/architect.

## 7.3 Estimation & sprint planning

- **Estimate in ranges + confidence**, not false precision. Story points measure effort/uncertainty for *this* team, not time.
- Use **reference stories** ("this is a 3 like the login ticket") instead of debates.
- **Split** work: vertical slices, spike the unknown first, "walking skeleton" for new tech.
- Track **cycle time** and **escaped defects**, not velocity-as-productivity.
- Give a **cone of uncertainty**: "2–3 weeks, +1 week if the vendor API is undocumented."
- **Exercise:** estimate next sprint with the team; 2 weeks later, write down the delta and *why* (the why is the lesson).

## 7.4 Technical debt management

- **Classify:** deliberate/reckless, prudent/reckless (Fowler's quadrant). Name it so it's a choice, not an accident.
- Keep a **visible debt backlog** with impact (risk/velocity/cost) + rough effort. Fund it as a **percentage** of each sprint (e.g. 10–20%), not "when we have time."
- Pay debt when it *blocks* a goal or *causes incidents* — otherwise it's a preference.
- **Exercise:** build the debt backlog for a module you own (10 items, each with impact + effort), and negotiate 15% of a sprint to burn the top 2.

## 7.5 Mentoring juniors

- **Model:** I do (demo) → We do (pair) → You do (they drive, you observe) → Review (feedback).
- Ask questions before giving answers ("what have you tried?", "what does the error actually say?").
- Tailor to the person: some want deep context, some want the smallest next step.
- Give a growth plan with **one** focus area at a time. Celebrate the improvement, not the person's identity.
- **Exercise:** run a weekly 30-min 1:1 with a junior; each time, they leave with one concrete next action and one thing they learned.

## 7.6 Feedback & conflict

- **SBI model:** Situation → Behaviour → Impact. Specific, timely, private for criticism, public for praise.
- **Radical candour** (care personally + challenge directly); avoid ruinous empathy (soft feedback that hides the truth).
- Handle **conflict = disagreement without personal attack**: separate the idea from the person, restate their position to their satisfaction, seek the *criterion* (cost? safety? time?) you disagree about.
- **Exercise:** role-play one hard conversation (a peer who keeps shipping untested code). Write the SBI script first, then deliver it.

## 7.7 Stakeholder communication & design docs

- **Audience-first:** execs want outcome, risk, cost, timeline; engineers want trade-offs and details. Same decision, two documents.
- **Design doc 1-pager:** problem → options → recommendation → risks → ask. Lead with the recommendation.
- **Status:** RAG (Red/Amber/Green) + "what changed since last time" + "what I need from you" — never just "in progress."
- **Bad news early, with options** ("we can ship reduced scope X by date D, or full scope by D+2 weeks — I recommend X. Your call.")
- **Exercise:** present your capstone architecture to a non-technical friend/PM in 5 minutes; if they can't restate it, simplify.

---

## The lead operating system (weekly habits)

| Habit | Frequency | Why |
|---|---|---|
| 1:1s with each teammate | Weekly | Trust, growth, early risk signals |
| PR review with labels | Daily/2–3 | Quality + teaching |
| ADR / design doc | Weekly | Decisions in writing |
| Risk + status update | Weekly | Stakeholders never surprised |
| Debt triage | Per sprint | Prevent slow rot |
| Retro + one process tweak | Per sprint | Continuous improvement |
| "What did I unblock?" journal | Weekly | Measure lead output, not code |
