# Chapter 8 — The Tech Lead Role & Leading a Team of Three

**Role you are targeting:** Microsoft Fullstack Technical Lead.
**Experience you already have:** 6 years of full-stack web development — 5 of them C#/ASP.NET MVC/Razor/JavaScript/jQuery, the last year Angular + React in production (self-taught, then shipped) — plus delivery ownership for a team of 3.

> **Read `00-START-HERE.md` §0A before this chapter.** That section is the script for how you *introduce* this experience; this chapter is the depth behind the claims you make there.

Chapters 1–7 made you credible as a *senior full-stack engineer*. This chapter and the five that follow (`09`–`13`) are what actually get you hired at **lead** level, because the interview panel changes shape:

| Senior interview | Technical Lead interview |
|---|---|
| "How does `OnPush` work?" | "Two engineers disagree about Angular vs React for the new module. Walk me through the conversation." |
| "What are DI lifetimes?" | "Your team keeps shipping DI bugs across 18 months. Fix it systemically." |
| "How do you do JWT?" | "We must move 40 screens off ASP.NET MVC to the SPA, with three devs, no freeze on delivery. Plan it." |
| Judgement: *can this person build it?* | Judgement: *would I let this person own a roadmap and three careers?* |

> **The lead-level rule that governs every answer in this chapter:**
> A lead is judged on **decisions, leverage and outcomes over time**, not on code volume.
> If your answer contains no trade-off, no named alternative, and no named cost, you are answering as a senior, not a lead.

---

## 1. What actually changes when you become a lead

### 1.1 The value shift (say this in an interview)

An individual contributor (IC) is paid for **their own throughput**. A lead is paid for **the team's throughput plus its predictability**, and for the **quality of the team's decisions** — including decisions made while you are not in the room.

Three concrete consequences:

1. **Your output is now a multiplier, so "I coded it myself" is a weak answer.** A lead who was the top committer for a quarter is a lead who failed to grow the team. Prefer: "I paired with X, they own it now, I reviewed it."
2. **You are now accountable for work you did not produce.** That means your mechanisms (definition of done, review standards, tests, observability) matter more than your skill.
3. **You spend an increasing share of your time on *ambiguity*.** The team's questions have answers; your questions ("is this the right thing to build at all?") often do not.

### 1.2 The player-coach model for a team of 3

With three engineers (four including you) you are not a full-time manager. The honest, saleable model:

| Bucket | Share of week | What it contains |
|---|---|---|
| **Hands-on delivery** | 40–50% | Hard problems, spike work, review depth, architecture-critical code, unblocking |
| **Technical leadership** | 25–30% | Design review, standards, ADRs, debt triage, cross-team design, CI/CD and platform |
| **People & process** | 20–25% | 1:1s, feedback, growth plans, hiring support, retros, planning, status comms |

Ground truth to be able to defend the arithmetic (interviewers do check):
> 1:1s — 3 × 30 min = 1.5 h. Planning + refinement + retro + standups = ~4 h. Reviews = ~4 h. Status/stakeholder = ~2 h. **That's ~11–12 hours of the week gone before you write any code.** So ~25–30% of the week is genuinely non-coding; anything above ~50% hands-on and you are borrowing from leadership time. A 3-person team tolerates a 50/50 player-coach. A 6-person team does not — say so, it shows you understand the model has a boundary.

### 1.3 What "owning" a team of 3 means in practice

Write this list down; it is the job description you can recite:

- **Outcomes** — the roadmap commitments the team makes to the business.
- **Predictability** — whether the team hits them, and early warning when it will not.
- **Technical health** — debt ratio, test signal, build time, dependency currency, incident rate.
- **Individual growth** — each person is measurably better than 6 months ago, and knows their promotion path.
- **Hiring bar** — you protect the bar, even under delivery pressure.
- **Escalation** — you are the stop point. Nothing invisible bubbles past you.

---

## 2. The lead's operating rhythm

### 2.1 The weekly cadence (a concrete, defensible system)

| Rhythm | Length | Lead's job in it |
|---|---|---|
| Team sync (daily) | 15 min | **Not** a status round. Ask "what's blocking?", "is anyone working on the same thing twice?" |
| 1:1s (each person, weekly) | 30 min | Theirs, not yours. Growth, blockers, friction, feedback both ways. Never cancel twice in a row. |
| Refinement | 45–60 min | Force acceptance criteria. Refuse work without them; that is a lead move. |
| Planning | 60 min | Commit to outcomes, not task hours. Name the risk items explicitly. |
| Design review / code walkthrough | 30–45 min | One per significant change. Cheap, high leverage. |
| Retro | 45 min | One **owned** action item per retro, with an owner and a date. Retros without owned actions destroy trust. |
| Stakeholder sync | 30 min | Demonstrate, don't report. Demo beats a status deck. |
| **Your own** "engineering hour" | 60 min | Debt, build times, dependency upgrades, dashboards. This slot is what stops you becoming a pure firefighter. |

### 2.2 The one rule that makes rhythm work

> **Every recurring meeting must have an owner, an artefact, and a decision point.** If it has none of the three, cancel it and defend the cancellation. This is one of the most concrete answers you can give to "how do you manage a team of three without drowning?"

---

## 3. 1:1s — the highest-leverage 30 minutes of your week

### 3.1 Structure (the 6-block template you can cite)

1. **Check-in (2 min)** — genuine, not therapy. Energy, load, anything outside work distorting focus.
2. **Their agenda (10 min)** — they lead. Silence on your side for the first minute; the thing they almost did not raise is usually the real topic.
3. **Feedback both ways (5 min)** — one specific thing you observed, one thing you want them to observe about you.
4. **Growth thread (8 min)** — carry it across weeks. Not every 1:1; a continuous thread with a written target.
5. **Blockers / unblocking (3 min)** — what can *only* you remove? Access, another team, a decision, a stakeholder.
6. **Capture (2 min)** — you write the note, you send a 3-line summary. Continuity is the whole point.

### 3.2 Questions that actually work

**For a junior / mid engineer:**
- "What did you learn this week that you could not have learned last month?"
- "Where did you lose the most time, and was it the code or the context?"
- "Which task would you like to be trusted with next, and what would make you ready?"
- "Is there anything you *pretended* to understand?"

**For a senior engineer (your peer in seniority, different axis):**
- "What part of the system worries you that nobody else is looking at?"
- "What's the most annoying thing in your day that we could delete?"
- "Which decision of mine do you think is wrong?" *(then wait, and thank them properly)*
- "What do you want to be known for in 18 months, and is this project building that?"

**For anyone, at lead level:**
- "On a scale of 1–5, how sustainable is your current workload? What would move it one point?"
- "What would make you leave in the next year — and what would make you stay?"

### 3.3 The failure modes — name them, they are interview gold

- **Status-1:1.** You talk; they report. Cure: their agenda, in writing, 24 h ahead.
- **Ghost 1:1.** Cancelled three weeks running. Cure: treat it as a meeting with your boss — you would not cancel that. Reschedule, never drop.
- **Therapy-1:1.** No action comes out. Cure: always end with one owned action, theirs or yours.
- **Surprise 1:1.** Performance problems first mentioned in a formal review. Cure: no surprise principle — feedback within 48 hours, always.

---

## 4. Feedback that changes behaviour (not just lands)

### 4.1 SBI + intent (the model to quote)

**Situation → Behaviour → Impact**, then ask for **intent** missing context.

> "In yesterday's design review *(S)*, you cut across Priya's proposal before she'd finished the third slide *(B)*. She stopped presenting and we left without the data flow decision *(I)*. What was going on for you? *(intent)*"

Then — and this is the lead move — **separate the intent from the impact**:
> "I don't think you meant to shut her down. But for the next three people in that room, the message was 'don't propose things'. I need you to be the person who makes it safe to propose."

### 4.2 Praise must be as specific as criticism

"Great work" is noise. **"You found the EF Core N+1 in the orders list before a customer did, and you added the logging that would have caught it — that's the standard I want people to copy."** Specific praise tells the team what *good* looks like; vague praise tells them nothing and rewards nothing.

### 4.3 Two feedback artefacts to describe

- **Feedback log / running note** — one line per person per week in your private notes. Six weeks later your review writes itself, and no feedback is a surprise.
- **The "one thing" agreement** — at each cycle you and the engineer agree *one* behaviour change and *one* strength to deploy. More than one and neither happens.

### 4.4 Delivering hard feedback — the 5-step script

1. **Contract:** "I want to give you feedback on something that matters. Can we do 10 minutes now?"
2. **SBI**, no adjectives about the person.
3. **The standard:** what the team needs, not what you prefer. Reference the written expectation (see §10).
4. **Ask, then listen.** Their context may change the story; if it does, say so.
5. **Specific expectation + follow-up date:** "By next Friday I expect the PR split under 400 lines with tests. We'll look at it together Thursday."

### 4.5 The underperformance path (be able to describe this soberly)

Sequence that protects both the person and the team, and is legally/documentarily sound:

1. **Clarify** — is the expectation written, known, and achievable? Most "underperformance" is an unclear-expectation bug, not a person bug. Fix that first, in writing.
2. **Diagnose** — skill gap, clarity gap, motivation gap, or environment gap (on-call burnout, noisy home, tooling friction, a conflict with a peer). Each needs a different remedy.
3. **Coach with a date** — specific behaviour, support you will provide, review date (typically 2–4 weeks).
4. **Formalise** when coaching does not move it — documented plan, HR partner, weekly check-ins, explicit criteria.
5. **Protect the team in parallel** — do not let the team absorb silent rework for months. Say "I'm handling it" and mean it.

Interview line that lands:
> "My first assumption is that I failed to make the expectation concrete. I've had more success fixing unclear expectations than fixing people. But the second assumption after two cycles is that I'm avoiding the conversation — and that's on me, not on them."

---

## 5. Delegation — the skill that decides whether you scale

### 5.1 The delegation ladder (name the rungs)

| Level | You say | Use when |
|---|---|---|
| 1. Do exactly this | "Change the serializer to X, here's the diff shape" | Urgent, low-skill, high-risk |
| 2. Research and report | "Compare RTK Query vs thunks for us, recommend one" | Building judgement |
| 3. Recommend, I decide | "Bring me your recommendation and the trade-off, I'll approve" | Growing a senior |
| 4. Decide, tell me | "You decide; brief me before you merge" | Trusted, reversible |
| 5. Own it | "It's yours. Come to me when you need a decision or a shield" | Growth target |

**The mistake to confess:** most new leads stay on rung 1–2 because it feels safe, then complain they have no time. The fix is deliberate rung-shifting with a safety net.

### 5.2 What to delegate first (a real answer, not a platitude)

- Anything **repeatable** (release checklist, dependency upgrades, onboarding doc).
- Anything **reversible** (refactors inside a bounded module, test coverage, dashboards).
- Anything the person **wants to be known for**.

Keep for yourself: irreversible decisions, cross-team negotiation, personnel matters, and anything where a wrong call is unrecoverable in production (schema migrations with data loss risk, auth changes).

### 5.3 The delegation brief (a 6-line template)

1. **Outcome** — what "done" looks like, in observable terms.
2. **Constraints** — must-do / must-not-do (compat, deadlines, security).
3. **Boundary** — where you need to be consulted *before* action.
4. **Resources** — time, access, who to ask.
5. **Check-in point** — a date, not "let me know".
6. **Why them** — the growth reason. This is free motivation.

### 5.4 "I delegated and it went wrong" — the good answer shape

> "I handed over the Kafka consumer retry work at level 4 without a boundary. Two days later the team had a poison-message loop in staging. My mistake was not the delegation — it was that I hadn't stated the boundary ('anything touching the DLQ topic needs a design review') and I hadn't checked that the failure mode was understood. I now write the boundary into the brief, and for irreversible-ish work I ask them to describe the failure mode back to me before they start. I'd rather lose 20 minutes than a day."

---

## 6. Code review as a leadership instrument

### 6.1 Why it is a *lead* topic

Review is the cheapest place for a lead to shape standards, teach, and enforce consistency without micromanaging. It is also where culture is made: a hostile reviewer destroys more velocity than any technical debt.

### 6.2 The standards you set (concrete, quotable)

| Standard | Value | Why |
|---|---|---|
| PR size | ≤ 400 changed lines, ≤ 1 concern | Review quality collapses past ~400 lines; defect detection drops sharply |
| First response SLA | Within 4 working hours (office hours) | Blocked authors are the single biggest silent waste |
| Approval count | 1 for normal, 2 for auth/data-migration/public-API | Risk-weighted |
| Draft PRs | Required for anything > 1 day | Early steer, no surprise architecture |
| Tests in the PR | Non-negotiable for logic branches | "Later" never comes |
| Self-review | Author comments their own PR first | Halves reviewer cognitive load |

### 6.3 The comment taxonomy — teach your team to label comments

Give reviewers a prefix vocabulary; it ends the "was that a blocker or a thought?" ambiguity:

- `blocking:` must change before merge.
- `suggestion:` better, but author's call.
- `question:` I do not understand; explain in the PR or the code.
- `nit:` trivial, ignore freely.
- `praise:` genuinely good — use deliberately; it is how juniors learn what to repeat.

### 6.4 What a lead reviews *for* (order matters)

1. **Correctness and failure modes** — what happens on null, on retry, on timeout, on concurrent edit?
2. **Boundaries and contracts** — does this leak UI concerns into the API? Does it couple two modules that should not be?
3. **Test signal** — do the tests fail if the behaviour breaks, or are they decoration?
4. **Operability** — logs, metrics, error messages a human can act on, config not hardcoded.
5. **Style** — automated. Never a human argument. Formatters and analyzers exist (`.editorconfig`, `Directory.Build.props`, `dotnet format`, Oxlint, Angular's schematics) so that review time is spent on judgement, not whitespace.

Interview line:
> "Every comment I make in review is either a defect, a design concern, or a teaching moment — style arguments are a process bug, because style belongs in the toolchain."

### 6.5 Reviewing a *senior* vs a *junior*

- **Junior:** review for **learning**, not just correctness. One concept per review, asked as a question: "What happens to this subscription if the component unmounts mid-flight?" You are building their model of the framework.
- **Senior:** review for **intent and alternatives** — "Did we consider doing this in the interceptor instead of three call sites?" Never re-litigate their style; their judgement is the product.

### 6.6 The anti-pattern you must be able to name

**Rubber-stamp review by the lead** ("LGTM" on 900 lines at 6 pm). It is worse than no review: it transfers risk to production while signalling that review is theatre. If you cannot review it, say so and negotiate a split.

---

## 7. Running the ceremonies without wasting the team's life

### 7.1 Daily sync
- 15 minutes, standing, timed.
- Three questions: **blocked?**, **surprised?**, **duplicated?**
- Anything requiring a conversation → park it, name participants, immediately after.
- A lead who lets the standup become a 35-minute design discussion has just cost the team 1.5 hours.

### 7.2 Refinement (the highest-value ceremony for quality)
- Every item leaves with **acceptance criteria** and a **size**. No criteria → no commitment. This single rule prevents most rework.
- Split anything that cannot be demoed. "Demoable" is the best proxy for "understood".
- Include the person who will build it. Estimates given by the lead alone are fiction the lead then has to defend.

### 7.3 Planning
- Commit to **outcomes** and to a **confidence level**, not to task-hours.
- Explicitly list the **top 3 risks** and what would de-risk them early.
- Reserve capacity for **interrupts** — support, incidents, reviews. A 100%-allocated plan is a guaranteed miss. (Typical: 15–20% for a product team.)
- Always schedule the **first slice** of the riskiest item for the first half of the sprint.

### 7.4 Retro
- Format rotation keeps it alive (start/stop/continue; 4Ls: Liked/Learned/Lacked/Longed for; sailboat).
- **One owned action, with owner and date.** Two actions and neither ships.
- Track action completion in the *next* retro. Retros that never close the loop are the fastest way to lose a team's honesty.

### 7.5 Estimations that survive contact with reality

Techniques you should be able to name and compare:

| Technique | Use for | Weakness |
|---|---|---|
| Story points + velocity | Stable teams, forecasting | Gets gamed; meaningless across teams |
| T-shirt sizes | Early triage, epics | Too coarse to forecast |
| **#NoEstimates / slice small** | Mature teams, continuous flow | Needs discipline and small items |
| Three-point (PERT) | High-uncertainty single items | False precision if data is invented |
| Reference-class forecasting | "How long did similar work take last year?" | Requires history |

Lead-level answer:
> "I optimise for *predictability*, not precision. That means small items, a definition of done that includes tests and observability, and a deliberate interrupt budget. If a commitment is going to slip, I want to know on day 3 from the engineer, not on day 9 from the burndown — so I explicitly reward early bad news and I never shoot the messenger in a standup."

---

## 8. Stakeholder communication — the part engineers underestimate

### 8.1 The three registers

| Audience | They want | Format |
|---|---|---|
| Engineers | Precision, trade-offs, constraints | Design doc / ADR / PR |
| Product / delivery | Scope, risk, dates, options | 3 bullets + a date + a decision needed |
| Executives | Outcome, cost, risk, confidence | 1 page: what/why/risk/ask, no jargon |

A lead who sends the same message to all three is either confusing the engineers or boring the executive.

### 8.2 Saying no without being an obstacle

The three-part refusal (use this structure verbatim in an interview):

1. **Acknowledge the goal, not just the request:** "I get why the customer demo matters."
2. **State the constraint in business terms, not engineering terms:** Not "we can't do it" but "shipping all three screens by the 14th means no tests and no telemetry on the payment path — I'm not willing to put the payment path in production unmonitored."
3. **Offer options with a recommendation:** "Option A: two screens on the 14th, the third on the 19th, full quality. Option B: all three on the 14th as a **behind-a-flag** internal demo, not production. I recommend A; B works if the audience is internal."

> "Saying no is a service. Saying no with a trade-off and a recommendation is what makes a lead valuable; saying no without alternatives is just friction."

### 8.3 Bad news protocol (memorise the shape)
**Early + specific + with a plan + with a revised commitment.**
"Inventory sync will miss the 30th. Cause: the supplier API rate-limits at 5 rps and we need 30. We've built a batched pull and it's working in staging but needs two more days of load testing. New date: 2nd. Fallback if the 2nd is unacceptable: ship the manual CSV import we already have and automate in the next sprint."

Never: "It's going to be a bit late, we're looking into it."

### 8.4 Managing up
- Know your sponsor's actual incentive (a launch, a compliance deadline, a cost target), and route your asks through it.
- **Bring decisions, not problems.** "I need you to choose between X and Y, here's my recommendation" is a 2-minute meeting. "We have a problem" is a 40-minute one.
- Escalate early and *flag it as escalation*: "I'm escalating this — not because I've failed, but because it needs someone with more authority than me."

---

## 9. Hiring and interviewing as a lead (even with a team of 3)

### 9.1 Designing a loop for a small team

| Stage | Length | What it must test | Failure if missing |
|---|---|---|---|
| Screen (30 min) | CV deep-dive | Is the CV true? Communication | Wasted panel hours |
| Coding / practical | 60–75 min | Reasoning, not trivia | Hires people who memorised |
| System design | 60 min | Trade-offs, failure modes, operability | Hires people who cannot own a roadmap |
| Craft / collaboration | 45 min | Review behaviour, feedback, conflict | Hires brilliant jerks |
| Manager / values | 30 min | Motivation, growth, honesty | Mismatched expectations |

For a *lead* hire, add a **stakeholder/ambiguity round**: "Here's an underspecified request from a PM. What do you do in the first hour?"

### 9.2 The bar rules
- **Fixed scorecards written before the loop.** Otherwise the loudest interviewer wins.
- **Independent written scores before the debrief.** Otherwise the first opinion anchors everyone.
- **No hire on "we need someone by Monday."** A wrong hire costs 3–6 months of team productivity — you can say that number out loud; it is why leads defend the bar.
- **Evidence in the debrief, not vibes:** "They said X when asked Y" beats "I got a bad feeling".

### 9.3 The one question you should always ask a senior candidate
> "Tell me about the last technical decision you got wrong. What did it cost, and what did you change about how you decide?"

Then listen for: do they name a real cost? Do they name a *thinking* change and not just an apology? A candidate who cannot name a wrong decision either has not made decisions or cannot reflect — both disqualifying at lead level.

---

## 10. Making expectations explicit (the lead's cheapest superpower)

Most team dysfunction is undocumented expectation. Write these once, keep them short, enforce them kindly:

- **Definition of Done** — code reviewed, tests, telemetry, docs updated, feature flag, rollback path known, deployed to staging.
- **Definition of Ready** — acceptance criteria, design noted if cross-cutting, sized, dependencies identified.
- **Working agreement** — core hours, response-time expectations, meeting norms, "no surprise deploys on Friday" (or whatever your team decides), how we disagree.
- **On-call expectations** — what pages you, what waits until morning, escalation path.
- **Branch / release rules** — who can merge, who can deploy, what gates exist.

Interview line:
> "I'd rather spend an hour writing a working agreement than six months refereeing the same argument about PR size."

---

## 11. The three people on your team — lead each differently

Interviewers listen for whether you treat a team as three interchangeable resources or as three people. Have a *named* development thesis for each (use their real names in the room; anonymise in writing).

| Profile | What they need from you | Risk if you get it wrong |
|---|---|---|
| **The strong senior** | Autonomy, hard problems, visibility, a path (staff/architect), to be consulted before decisions | They leave for a lead title you never offered |
| **The solid mid-level** | Clear scope, one stretch area, feedback on judgement (not just code), a specific next-level target | Plateau → quiet disengagement, or over-promotion into failure |
| **The junior** | Structure, pairing, small complete tasks, a safe place to be wrong, fast feedback loops | Shadowing for a year with no ownership |

The lead-level articulation:
> "My job is to know what each of them wants to be in two years, and to make sure the work that lands on them builds that — while still shipping. When those two things conflict, I pay the cost in my own time first, and I say so out loud so they know it's a choice, not neglect."

---

## 12. Conflict — three real scenarios, three scripts

### 12.1 Two engineers, opposite technical opinions (Angular vs React for a new module)
1. **Restate both positions neutrally, in front of both.** Half of all technical conflict is two people solving different problems.
2. **Name the criterion, not the preference:** "What decision would we regret in 18 months?" — usually maintainability, hiring pool, or team cognitive load, almost never framework elegance.
3. **Ask for the strongest version of the other position** ("steelman it"). Often dissolves the dispute.
4. **Timebox a spike** if it is empirically decidable; pick the reversible option if not.
5. **You decide and you own it.** "Both are defensible. We're standardising on one because a 3-person team cannot carry two component models and two mental models. I'm choosing X. If it's wrong, it's my call, and we'll revisit in 6 months."
6. **Then make it safe for the dissenter:** "I want you to be the first to say 'I told you so' with data, not to be right by disengaging."

> The lead failure mode here is *false consensus* — waiting for agreement that never comes while the team stalls. A decision with a documented rationale and a review date beats harmony.

### 12.2 Conflict between a peer lead / another team
- **Go private and low-emotion first.** "I think we're both trying to protect our teams. Let's find the version where we both win."
- **Convert to a written interface:** what you need from them, by when, what happens if not. Ambiguity is what fights are made of.
- **Escalate the *decision*, not the *person*:** "I need a decision on which team owns the customer table, because we're both about to write to it."

### 12.3 Conflict with your manager
- Bring **data and a proposal**, never a complaint: "Here's what I'm seeing, here's what I think it costs us, here's what I'd like to try, here's what I need from you."
- If overruled: **disagree and commit, in public, visibly.** "I recommended B; we're doing A; I'm fully behind A." Then revisit with evidence when there is some.

---

## 13. Time and attention management for a lead

### 13.1 The four-quadrant triage for a lead's inbox
1. **Only I can do this** → do it now, small.
2. **Someone else can, and it grows them** → delegate with a brief (see §5.3).
3. **Nobody needs to do this** → delete and say you deleted it.
4. **It needs a decision, not work** → decide in 60 seconds, write the reason down, move on.

### 13.2 Protecting maker time
- **Two 3-hour deep-work blocks per week**, defended like a customer meeting.
- **Batch the interrupts:** office-hours slot for questions; a channel where engineers can ask without pinging you.
- **Say what you are doing when you go quiet:** "I'm heads-down on the migration script until 3; ping for anything production."

### 13.3 The burnout signals you must be able to name (for yourself and the team)
Hero culture, silent rework, "just this sprint" three sprints running, nobody taking leave, review latency creeping, retro actions never closing, one person always on the critical path. Naming these in an interview signals maturity far more than claiming "I've never had burnout on my team".

---

## 14. Anti-patterns a lead interviewer is actively hunting for

| Anti-pattern | What it sounds like | What to say instead |
|---|---|---|
| Hero lead | "I rewrote it myself over the weekend" | "I paired, then handed it over" |
| Absentee coach | "I let them self-organise" | "Self-organise inside a frame I set" |
| Process maximalist | "We do daily standups, weekly demos, bi-weekly retros…" | "Each ceremony has an artefact and a decision; we cut the rest" |
| Consensus-seeker | "We discuss until everyone agrees" | "I decide with rationale and a review date" |
| Blame narrator | "The team kept making mistakes" | "My definition of done did not include X; I fixed that" |
| Scoreboard manager | "I tracked their commits" | "I tracked outcomes, blockers and growth" |
| Silent escalator | "I reported it and waited" | "I escalated with a recommendation the same day" |

---

## 15. Chapter 8 — questions you must be able to answer out loud

Record yourself; 90 seconds each.

1. What changed in how you measure your own success when you became a lead?
2. Walk me through your week: what is fixed, what is flexible, and how much time do you actually spend coding?
3. How do you run a 1:1 with someone who says "everything's fine" every week?
4. Tell me about a time you gave difficult feedback. What exactly did you say, and what happened next?
5. How do you handle a senior engineer who is technically excellent and toxic in reviews?
6. Describe how you delegate. Give an example where it went wrong.
7. How do you decide what to review yourself versus what to let the team own?
8. Your team of three is asked to deliver 8 weeks of work in 5. What do you do — in the first 24 hours?
9. Two engineers cannot agree on a framework choice. Walk me through it to the decision.
10. How do you handle a stakeholder who keeps adding scope mid-sprint?
11. How do you know whether your team is healthy? Name the indicators you actually look at.
12. What would you do in your first 30 days if you joined us as the lead of this team?
13. What is the hardest management decision you have made, and what did you learn about yourself?
14. How do you make sure a junior engineer grows without giving them work that endangers the release?
15. What do you do when you realise you have become the bottleneck?

> If you can answer all 15 without hesitating, you are already outperforming most senior engineers who interview for lead roles.

**Next:** `09-ARCHITECTURE-DECISIONS.md` — the technical half of technical leadership.
