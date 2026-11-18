# Chapter 12 — The Technical Lead Interview Playbook

**What this chapter is:** the loop, the actual questions, and model answers written for **your** specific position — 5 years .NET, current-ish Angular/React depth, and real experience leading a team of three.

Chapter 6 gave you senior-level question drilling. This chapter is the **lead-level** round: it is a different exam, scored against a different rubric.

---

## 1. Know the rubric before you walk in

Panels score leads on five axes. Everything you say should make one of these easy to tick:

| Axis | What they're actually testing | Evidence that scores | Evidence that doesn't |
|---|---|---|---|
| **Technical depth** | Can you still be the technical authority? | A concrete trade-off with a cost you accepted | Buzzword lists; "we used microservices" |
| **Judgement** | Do you choose well under uncertainty? | Rejected alternatives + a reversal trigger | "Best practice" |
| **Leverage** | Do you multiply the team? | Mechanisms you installed; people you grew | "I did it myself" |
| **Ownership** | Do you carry outcomes, including bad ones? | A failure you owned and fixed systemically | Blaming others or the org |
| **Communication** | Can you speak to engineers, PMs and execs? | Adapted register; short answers | 10-minute monologues |

**The scoring reality:** a lead interview is not a knowledge test with a leadership section. It is a **judgement test** with a knowledge section. A candidate who has perfect Angular answers and vague leadership answers fails. A candidate with solid technical answers and sharp, specific leadership stories passes.

---

## 2. The loop you should expect (and prepare for each)

| Stage | Typical length | What it is really testing | Preparation |
|---|---|---|---|
| **Recruiter / screen** | 30 min | Level fit, salary range, motivation | Your 90-second story + salary ask |
| **Hiring manager** | 45–60 min | Judgement, ownership, team leadership | §5 stories + §9 questions |
| **Technical deep-dive** | 60–90 min | Are you still credible hands-on? | Chapters 1–5 |
| **System design / case study** | 60–90 min | Architecture + trade-offs + delivery plan | §7 |
| **Practical / coding** | 60–75 min | Reasoning, not trivia | Chapter 7 syntax drills |
| **Craft / collaboration / values** | 45–60 min | Conflict, feedback, culture | §5 stories |
| **Skip-level / director** | 30 min | "Would I want this person near my roadmap?" | §6 framing, §10 questions |

**Lead-specific stages to expect that seniors don't get:**
- **"How would you improve our engineering organisation?"** — answer with a 90-day plan (Chapter 13).
- **"You disagree with the VP's decision. What do you do?"** — §5.7.
- **"Which of our services would you rewrite?"** — never "rewrite" (Chapter 9 §4).

---

## 3. Your 90-second opening story (script it, rehearse until boring)

Every loop starts with "walk me through your background". This is the single highest-leverage 90 seconds in the day, because it frames everything afterwards.

### 3.1 The structure (4 beats)
1. **Identity + span** — who you are, how long, what stack.
2. **The arc** — the *progression* that makes lead plausible.
3. **The lead evidence** — team size, what you owned, one quantified outcome.
4. **The why-here** — why this role, this company, now.

### 3.2 The script (adapt to your real numbers)

> **This section is a shorter companion to `00-START-HERE.md` §0A**, which holds the full introduction method (three-beat structure, 20-second and 3-minute variants, word swaps, and the follow-up map). Use §0A as the canonical script; use this as the lead-loop tightening of it.

> "I'm a full-stack .NET developer — six years building and running ASP.NET applications, five of them in MVC, Razor, jQuery and JavaScript, and the last year doing that work with Angular and React against ASP.NET Core APIs.
> For the last year I've also **owned delivery for a team of three** engineers — I still write code most of the week, but I also do the task breakdown, the technical design calls, the code-review gate and the release decisions. Concretely: I **[pick the two that are true — introduced ADRs for irreversible decisions / standardised the front-end state strategy across our Angular and React apps / added a CI gate with integration tests]**.
> Before that I spent about three years at a product company building features and carrying production support, so I've been on the receiving end of a defect — that's where my instinct to reproduce before theorising comes from.
> What I want next is a fuller technical-lead scope: formal accountability for a team, more ownership of the architectural roadmap, and a Microsoft/ASP.NET/Azure stack — which is where my depth is. That's why this role."

> **Metric honesty:** the original version of this script contained bracketed metrics (change-failure rate, deploy frequency). **Delete every bracket you cannot evidence.** A fabricated change-failure rate is the single claim an interviewer can dismantle in two questions — "how was it measured, over what window, by which tool?" — and losing that argument poisons the rest of the loop. Keep only numbers you could show on a dashboard.

### 3.3 The three things this script does
- **Establishes the lead claim early** so the rest is scored at lead level.
- **Puts a number on leadership** (rare; immediately differentiating).
- **Tells them what you want**, which lets the interviewer sell to you rather than probe defensively.

---

## 4. How to talk about a team of three without inflating it

The trap: "I led a team of 3" can read as *"I was the senior dev on a small team"*. The fix is **specificity of ownership**, not bigger numbers.

| Weak phrasing | Strong phrasing |
|---|---|
| "I managed three developers." | "I own delivery and technical direction for three engineers — one senior, one mid, one junior — plus the architecture and the pipeline." |
| "I was the team lead." | "I run their 1:1s, their growth plans and our code standards; I'm the escalation point for anything that can't be resolved in the team." |
| "We delivered a big project." | "We moved **N** screens off Razor onto an Angular SPA in **X** weeks with no delivery freeze and no customer-visible incident." |
| "I helped with hiring." | "I built our interview loop and I have veto on the technical bar." |
| "I did code reviews." | "I set the review standard — PR size, SLA, comment taxonomy — and our review latency went from **2 days to 4 hours**." |

**If asked "isn't three people quite small?"** — do not be defensive. Reframe on scope, not headcount:
> "It's small in headcount and wide in scope: I own the architecture, the delivery process, the pipeline, the cloud setup and the people. What it hasn't given me is the challenge of scale — multiple teams, cross-team dependencies, managing managers — and that's exactly the part of this role I want. I'd rather be honest about that than pretend three people is the same as fifteen."

That answer converts your gap into a stated motivation. Interviewers score honesty about scope higher than inflated claims they'll catch anyway.

---

## 5. The behavioural bank — model answers

**How to use this section:** each answer is a *skeleton with your variables in bold*. Do not memorise verbatim; memorise the **shape** and the **punchline sentence**. Record yourself; 90–120 seconds each, never more.

Answer shape for every one of these:
**Context (1 line) → The tension (1 line) → What I did (3–4 lines, specific) → Result (1 line, quantified) → What I changed about myself (1 line).**

That last line is what marks you as a lead rather than a doer.

---

### 5.1 "Tell me about a time you had to deliver bad news."

> "We were three weeks from a go-live and I could see the data-migration was going to overrun — our supplier API rate-limited at 5 requests a second and we needed roughly six times that. **I told the programme lead the same day I knew**, with three things: the cause, the two options, and my recommendation. Option A was a batched nightly pull that got us to 90% of the data in time and the rest by day three, behind a flag. Option B was to delay two weeks. **I recommended A and we shipped.** The thing I took away: I'd spent four days trying to solve it quietly before telling anyone, and those four days were the most expensive part of the whole episode. Now I have a rule — I escalate the moment I know, not the moment I've failed."

**Why this scores:** bad news early, options not problems, a self-critique, and a *rule* extracted.

---

### 5.2 "Tell me about a time you disagreed with your manager or a senior stakeholder."

> "My product owner wanted three new screens shipped before a customer demo, and I thought shipping them without automated tests on the payment path was not a risk I was willing to own. I didn't say no — **I said 'here's what that costs'**. I laid out: all three, on time, means no tests and no telemetry on payments; I'm not willing to put the payment path into production unmonitored. Then I gave two alternatives: ship two fully and the third the following week, or ship all three behind a flag for the demo only, not production. **I recommended the first.** He chose the flag option, because the demo was the actual goal — which I'd have known if I'd asked what he needed rather than arguing about dates. That's the lesson: I was arguing about the plan when the disagreement was about the objective."

**Why this scores:** refusal with alternatives, willingness to be overruled and commit, and a genuine insight (ask about the objective).

---

### 5.3 "Tell me about a technical decision you got wrong."

> "I pushed us to adopt [**Kafka / a second front-end framework / microservices split**] for a project where three engineers had to own it. My reasoning was sound on paper — **[it gave us independent scaling / the developer experience the team wanted]** — but I underweighted operational cost. **[Six weeks in we had a poison-message loop in staging, one engineer was the only one who could debug the broker, and the team's velocity dropped.]** I reversed it. Cost was **[~5 weeks of engineering time]**. The change I made to how I decide: I now always score a decision on **operational burden and team skill**, not only technical merit, and for anything irreversible I write an ADR with the negative consequences spelled out first — if I can't write three costs I'd accept, I'm rationalising, not deciding."

**Why this scores:** a real cost, a decision-*thinking* change, and a mechanism.
*(Pick a real reversal you can defend in follow-up detail — they will ask "what exactly did the poison loop do?" and you must be able to answer.)*

---

### 5.4 "Tell me about a time you turned around an underperformer."

> "One of my engineers was missing commitments — not catastrophically, but consistently, and the team had started quietly building around him. My first step was **not** a performance conversation: I checked whether the expectations were actually written down, and they weren't. So I made them concrete — what 'done' means for a task, what finishing without review rework looks like — and we agreed one behaviour to work on. Two weeks later nothing had changed, **and I recognised that the delay was mine, not his**: I'd been avoiding the direct conversation because I didn't want to hurt the relationship. When I did have it, using situation-behaviour-impact and a specific expectation with a date, it turned out he was drowning on a task he was afraid to ask for help on. We split the work, added a daily 10-minute check-in, and he delivered his next two items without rework. The lesson I'd carry into your team: **my first assumption should be an unclear-expectation bug, but my second should be that I'm avoiding the conversation — and the second one is on me.**"

**Why this scores:** diagnosis before judgement, owns the delay, uses a named model (SBI), produces a concrete change and a measurable outcome.

---

### 5.5 "How do you handle a brilliant engineer who is toxic in reviews?"

Hand this as reasoning, not a story — it's usually a scenario question:

> "Separate the behaviour from the person and separate the impact from the intent. In a 1:1 I'd use situation-behaviour-impact and name the effect on the team specifically: 'When you wrote "this is fundamentally wrong" on a junior's PR, he stopped raising designs for three weeks.' Then I'd separate the standard from the style: I want the technical rigour, I will not accept the delivery. Second, I'd make the standard explicit for everyone — a comment taxonomy with `blocking:` / `suggestion:` / `nit:` — so it isn't my personal preference against theirs. Third, I'd give him a role that uses the strength: review standards owner, or pairing. If the behaviour continued, I'd be explicit about consequences, because **the cost of keeping a brilliant person who makes three others smaller is almost always higher than the cost of losing them** — and I'd rather say that out loud than pretend it isn't a decision."

**Why this scores:** a principle with a stated cost, a mechanism (taxonomy), and a willingness to act.

---

### 5.6 "Tell me about the hardest prioritisation call you've made."

> "We had a security finding — a dependency with a known RCE in a transitive package — landing in the same sprint as a committed customer feature. I ranked it: **a known exploit is not a backlog item, it's a date on the calendar.** I took one engineer off the feature for a day to upgrade and validate. The feature slipped by two days; I told the stakeholder the same day with the reason in one line — 'we had a known-vulnerable dependency and we patched it; the feature is two days later.' Nobody argued, because I gave them the reason and the new date instead of a vague delay. What I'd flag as a lead: I now keep a **fixed risk reserve** of about 15% of sprint capacity, because 'unplanned' always happens and pretending it doesn't just moves the cost onto someone's Friday night."

**Why this scores:** a decision rule, communication of the cost, and a structural fix.

---

### 5.7 "You strongly disagree with the VP's technical direction. What do you do?"

> "First, I make sure I actually understand it — half the time an executive decision that seems technically wrong is solving a business constraint I wasn't in the room for. So I go and ask, without an argument to win. Second, if I still disagree, I put the disagreement in writing, briefly: the risk, the cost, and the alternative, with a recommendation — because a written objection is useful and a verbal one evaporates. Third, **I state clearly that I understand it's their call**, and if they decide against me I disagree and commit, publicly and genuinely: my team hears me say 'this is the direction, I'm fully behind it', because a lead who lets the team see them sulking has just licensed the whole team to disengage. Fourth, I set a review point — 'let's look at the first two data points in six weeks' — so the objection stays a hypothesis rather than a grievance."

**Why this scores:** the disagree-and-commit discipline plus a review point. Lead panels love this answer because it's the exact scenario they face with their own skip-level.

---

### 5.8 "How do you onboard someone? / How do you make a new joiner productive fast?"

> "My first-week plan is: a runnable environment on day one — and I mean *runnable*, which means somebody has already validated the README, because a broken onboarding doc is the first impression of our engineering standards. Then one small, real, deployed change in the first two days, ideally a bug fix that touches the whole path: front end, API, and a test. That teaches more than a week of reading. Then a named buddy for two weeks, all 1:1s with me daily for the first week and weekly after, and a written 30/60/90 with named outcomes. The measure I care about isn't comfort, it's **the day they ship something that a customer sees** — for a mid-level engineer that should be inside a week."

---

### 5.9 "How do you keep technical debt from killing you?"

> "I classify it before I negotiate it — deliberate vs reckless, and bit rot vs architectural — because 'the code is bad' gets no budget and 'two of three engineers can't touch the React app, so every change queues behind one person with a 22% change-failure rate' gets budget. Then I ask for a small, repeating slice: 20% of each sprint, visible on the board, rather than a stop-the-world refactor that never gets approved. And I defend it with a trend, so the conversation is about whether the trend improved, not about whether my opinion is right."

---

### 5.10 "Tell me about a production incident you were responsible for."

If you have one, tell it straight. If your incidents were small, that's fine — say so and go deep on the *response*, because that's what's being tested.

> "We shipped a change to the product search endpoint that added a filter in [**LINQ, which translated to a query that did a table scan because of a nullable column / a client-side evaluation**]. It didn't show in staging because staging had 2,000 products and production had 400,000 — so p95 went from 180 ms to 9 seconds and effectively took the catalogue down for about [**25 minutes**].
> **First thing I did was roll back**, not diagnose — the deploy correlated, so I didn't need to know the cause to restore service. Then comms: I nominated one person to update the business and gave them the next-update time. Root cause took about 20 minutes after rollback. Then a blameless postmortem with four actions: an index on the filtered column, a staging data volume that is within an order of magnitude of production, a p95 latency alert on that endpoint, and **a step in the PR template: 'does this query change require an index?'** The lesson I keep: our testing was fine and our *detection* was the failure — we found out from a customer. Detection is cheaper to fix than prevention, and teams systematically under-invest in it."

**Why this scores:** rollback-first, comms discipline, blameless postmortem, owned detection gap with a mechanism.

---

### 5.11 "How do you handle a team member who wants a promotion they're not ready for?"

> "I take it seriously and make it concrete — 'not yet' without a path is how you lose good people. I'd ask what they want the new level *for*, because sometimes the answer is money and the answer is a raise conversation, not a title change. Then I'd write the gap in observable behaviours: 'at senior, you'd be expected to take an ambiguous problem and break it down without me, and to be the person others come to for review on X — here are three pieces of work over the next quarter that would demonstrate both.' Then a review date. And I'd be honest: if I can't find the work that would prove it, that's a problem in the team's opportunities, not just in the person, and I'd say so."

---

### 5.12 "Tell me about a time you influenced a decision you had no authority over."

> "Another team owned the customer table and were about to add a second writer to it — their service needed to update a status field. I had no authority over them, so I didn't argue ownership, I showed the cost: 'if we both write this table, neither of us can deploy a schema change without the other, and we've each just halved our release flexibility.' Then I offered a cheap way out: they publish a `CustomerStatusChanged` event, we consume it, we keep one writer. It took one meeting, and I think it worked because I brought a mechanism rather than a boundary dispute."

---

## 6. How to answer "why are you leaving / why us / why lead?"

- **Why lead:** "I found I was more useful making three people effective than being the fastest individual — and I want to still be technically credible while doing it. I don't want to stop coding."
- **Why us:** anchor on something specific from the JD or a stage you've had: "you're consolidating two front ends onto one framework — I've done that decision and the migration planning, and I know where it goes wrong."
- **Why leaving:** never criticise. "I've taken the team as far as the current scope allows — I want bigger surface area and more of the architectural roadmap."

**Never say:** "I want less coding." Lead roles are player-coach; saying you want out of code reads as "I want out of accountability".

---

## 7. The system-design / case-study round at lead level

The difference from senior: you are also scored on **delivery, cost, team and risk**.

### 7.1 The 8-step structure (use it every time, out loud)

1. **Clarify the goal and the user** (2 min). "Who's the user, what do they do today, what hurts?" Do not skip this even if you're tempted.
2. **Scope ruthlessly** (2 min). "I'll assume X and Y; I'll exclude Z. Is that right?" Interviewers give you credit for shaping scope.
3. **Functional requirements, numbered** (2 min) — 5–8, drawn from the user's job.
4. **Non-functional requirements with numbers** (2 min). Availability target, p95 latency, data volume, consistency requirement, security/compliance. *This is where senior candidates fail and leads score.*
5. **The boring baseline** (3 min). "The simplest thing that works: one API, one SQL DB, one SPA, one cache if needed." Say its cost. Always show you can choose boring.
6. **Draw and walk the main flow** (10 min). Component boundaries, data storage, sync vs async, failure modes at each hop.
7. **Then scale / harden only where the numbers demand it** (5 min). Justify each addition against a requirement you stated in step 4.
8. **Delivery plan** (5 min): slices, what you'd build first (highest risk, thinnest vertical slice), how it deploys, how you'd know it works, and what you'd deliberately not build yet.

### 7.2 The four phrases that earn the "lead" tick

1. *"Before I design, what's the actual constraint — is this latency, cost, or a compliance deadline?"*
2. *"The simplest version is X. The cost of X is Y. I'd only add the complexity when we see Z."*
3. *"What breaks first under load is the database, so my first move is indexes and caching, not a redesign."*
4. *"Given three engineers, I'd slice it this way, and I'd want the riskiest assumption proven in week two, not week twelve."*

### 7.3 The three failure modes to avoid
- **Jumping to technology** in the first 60 seconds.
- **Only happy-path design** — no failure modes, no retries, no idempotency, no data-loss story.
- **No team or delivery angle** — you designed a system, not a plan.

### 7.4 The case study specific to this repo (prepare this one)
"**We're moving a Razor + jQuery ASP.NET MVC app to an Angular SPA, with one API, three engineers, no delivery freeze. Plan it.**"

Your answer, in order: inventory and rank screens by value÷risk → fix the auth seam first (shared OIDC) → publish stable API contracts + generate the client → build the shell and router, then move screens one at a time behind a route flag with 30-second rollback → delete the legacy route as part of each slice's DoD → track route count, legacy lines deleted, and seam incidents → expect velocity dip in weeks 3–6 and communicate the curve, not an excuse. (Full detail in Chapter 9 §4.)

---

## 8. Salary, level and negotiation (lead-level specifics)

1. **Never name a number first** if you can avoid it: "I'd rather understand the band for the level than anchor early — what range is approved for this role?"
2. **If pushed:** give the *band* and *the reason*, not a single figure: "For a technical lead role with this scope — team ownership plus architectural accountability — I'm targeting [X to Y], and I'm flexible for the right overall package."
3. **Anchor on level, not cash.** Level determines band, autonomy and the next two years. "Before the number — is this scoped as lead of one team or with a manager-of-managers path? That materially changes the band."
4. **Negotiate on the whole package:** base, bonus target, sign-on (easiest for them to move), pension, holiday, remote policy, learning budget, and **scope** (headcount, ownership).
5. **Get the lead scope in writing** — "you'll own architecture and delivery for the team" belongs in the offer letter, because it's what you're optimising for.
6. **The one line to remember:** *"I'm excited about the role; I just want to make sure the level matches the scope we discussed, because 'lead' means very different things in different organisations."*
7. **Be honest about competing processes** if true; never fabricate.

---

## 9. The questions you should ask (they score this too)

Lead-level questions are scored for **calibration and seriousness**. Ask 4–6, spread across:

**About the team**
- "How many engineers, and what's the seniority mix? Who's the strongest person on the team, and what will I learn from them?"
- "Has anyone been promoted on this team in the last 18 months? What did it take?"
- "What's the team's biggest delivery pain right now?"

**About the technical reality**
- "What's the thing in your architecture that you'd most like to change but haven't been able to?"
- "What does your CI/CD pipeline take, end to end, and what's the change-failure rate?"
- "How many front-end frameworks are in production today?" *(a wonderfully diagnostic question for a full-stack lead role)*

**About the leadership context**
- "What does success look like for this role at 6 and 12 months?"
- "Who will I work most closely with, and how does that person define good engineering?"
- "What's the decision-making model — who decides, who's consulted?"
- "Why is this role open? Was there a predecessor?"

**About reality and risk**
- "What's the on-call arrangement?"
- "How much unplanned work does the team absorb in a sprint?"
- "What's the budget situation for cloud and tooling?"

**And one that separates you from most candidates:**
> "What would make you not hire me — is there a gap you're worried about?" *(ask it late, in the hiring-manager round, and answer honestly; it gives you a chance to address the objection instead of losing to it in the debrief.)*

---

## 10. Red flags to watch for (they're interviewing you too)

| Red flag | Diagnostic question |
|---|---|
| "We need someone who can hit the ground running" + no docs/tests | "How long did the last person take to ship independently?" |
| No CI/CD, deploys from laptops | "What's your deploy process today, step by step?" |
| "Lead" with no authority (no review rights, no growth input) | "Who owns code quality and who signs off technical decisions?" |
| Permanent hero culture | "Who got paged most last quarter, and why?" |
| Two frameworks, no owner | "Who owns the front-end standard?" |
| High turnover | "How long is average tenure on the team?" |
| Perpetual roadmap pressure | "What's unplanned work as a % of sprint capacity?" |

---

## 11. The 15 things that most often sink lead candidates

1. Saying "I" for everything → use "we", and name who did what.
2. Talking about coding volume as evidence of leadership.
3. No numbers anywhere.
4. Claiming 5 years of Angular/React when it's 2 → get caught, lose the room.
5. Treating "microservices" as a maturity signal.
6. No rejected alternative in any decision.
7. All leadership stories are successes.
8. Criticising a previous employer or colleague.
9. Vague answers about a team of three ("we just collaborated").
10. Using "process" as a solution with no artefact or metric.
11. Not knowing who decides what in their own team.
12. Designing a system in the case study but never mentioning delivery.
13. Answering the salary question with a single number too early.
14. Asking no questions at the end.
15. Not being able to name the last technical decision they got wrong.

---

## 12. Chapter 12 — final rehearsal checklist

Run this a week before, then the day before:

- [ ] 90-second story recorded and word-perfect.
- [ ] The 12 behavioural stories written out in bullets (not scripts) with numbers filled in.
- [ ] One-page cheat sheet of your own numbers: team size, sprint length, deploys/week, change-failure rate, PR latency, incident MTTR, migration duration, headcount growth.
- [ ] The repo-based demo rehearsed: open `frontend/`, `ReactDotNetDemo/`, `backend/`, `DeliverFoodApp/`, `azure-pipelines.yml` and explain each in 60 seconds.
- [ ] The 11 pipeline findings from Chapter 10 §4.2 memorised — you can deliver those as a critique of a real pipeline.
- [ ] The strangler migration plan whiteboarded twice from memory.
- [ ] 6 questions to ask chosen, mapped to your interviewers.
- [ ] Salary band + the "level, not cash" framing ready.
- [ ] One honest gap identified and a scripted answer for it.

**Next:** `13-FIRST-90-DAYS.md` — your plan for the job, and the move that closes the interview.
