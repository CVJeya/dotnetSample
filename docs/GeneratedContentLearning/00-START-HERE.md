# Microsoft Fullstack Technical Lead — Interview Training
### Angular 22 · React 19 · ASP.NET Core / .NET · Azure · Team Leadership

**Built for you:** **6 years** of full-stack web development — ASP.NET MVC / Razor Pages / C# / SQL / JavaScript / jQuery — with the **last 12 months in Angular and React (self-taught, then taken into production)** and **delivery ownership for a three-person team**, now targeting a **Microsoft Fullstack Technical Lead** role where .NET, Angular (18+) and React (19) are required and **leadership is scored as heavily as code**.

> **Start with §0A.** It is the script for the first 90 seconds of every interview, built from your real timeline. Everything else in this set is support material for the claims you make there.

**Grounded in:** this repository. Every concept is explained through file paths you can open, run, and point at on a screen-share. You are not reciting tutorials — you are narrating a system you own. That is the single biggest advantage you have over other candidates.

---

## 0. Read this first — the two-part positioning

### Part 1: the front-end claim (corrected to your real numbers — keep it this way)

Your timeline gives you **two** front-end numbers, and you should always quote them together because the pair is stronger than either alone:

> **"Six years of front-end web work — five of them plain JavaScript, jQuery and CSS against server-rendered Razor views, and the last year in Angular and React against ASP.NET Core APIs."**

That is the honest, defensible claim: **6 years of front-end, 1 year of framework-based front-end.** It is also a *stronger* sentence than "1 year of Angular", because it is true and because it shows the transition you actually made.

**The rule: never say "2 years of Angular/React".** Your professional framework work is 12 months. The moment you say "2 years" you invite *"which version in year one, what was the migration, what broke?"* and you will have to retreat — and after a retreat, every other number you gave becomes suspect too. One year, described with depth and evidence, is credible and unbreakable.

What *is* legitimately expandable (use it once, precisely):

> "I've been building in Angular and React for about 18 months if you count the self-study period before it went into production — the last 12 months of that have been shipping."

Only say that if the self-study genuinely predates the production work by ~6 months. Never round a year up to two.

Honest pivots you should have ready:

| They probe | You say |
|---|---|
| "You've used Angular since when?" | "Professionally, the last year — I adopted it on my own initiative when I saw the front end moving that way, and then we took it into production. Before that I was doing Razor and jQuery, so I came at it with six years of DOM, events and async behind me. I know standalone components, signals, `inject()`, OnPush and zoneless change detection, and I can tell you what the pre-standalone `NgModule`/constructor-DI code looked like because I read and migrated that style." |
| "Have you led front-end architecture?" | "I've owned the front-end architecture for my team — routing, state strategy, interceptors, module boundaries — but I've been the beneficiary of a mid-level front-end ecosystem, not a world expert on Angular internals. My depth is back end plus integration." |
| "Why should we trust a back-end dev on our React codebase?" | "Because most React problems in a full-stack team are integration problems: state ownership, contracts drifting from the API, error handling, auth refresh, and caching. That's exactly my depth — and I bring the API and data model with it." |

### Part 2: the lead claim (this is the new part, and it is what the title is about)

> **"I own delivery and technical direction for a team of three engineers — one senior, one mid, one junior. I still write code about half my week; the rest is architecture decisions, delivery process, review standards, cloud setup and the growth of the three people."**

Three rules that govern every lead answer:

1. **Scope, not headcount.** Never defend "3 people" — describe *what you own*. `12-LEAD-INTERVIEW-PLAYBOOK.md` §4 gives the exact phrasing.
2. **Every answer ends on a trade-off**, with a rejected alternative and a cost you accepted. A lead without trade-offs is a senior with a title.
3. **Mechanisms, not intentions.** "We care about quality" scores zero; "warnings-as-errors, architecture tests, and a green-pipeline merge gate" scores full marks.

### The gap you must close to be credible at lead level
Most .NET candidates for this role are strong on Chapters 1–5 and weak on Chapters 8–13. **The technical chapters are not the gate — they're the entry ticket.** The gate is:

- Can you make and defend an architecture decision, with the cost stated? (Ch 9)
- Can you make delivery predictable and safe with mechanisms? (Ch 10)
- Can you hold an Azure conversation — hosting, identity, secrets, cost? (Ch 11)
- Would this person be safe owning three people's careers? (Ch 8, 12, 13)

---

## 0A. Your introduction — the six-year story, scripted

This is the highest-leverage 90 seconds of the entire loop. Every interviewer's first real question is "tell me about your background", and whatever you say in that answer **sets the frame** for the next 45 minutes: what they probe, what level they assume, and whether they spend the rest of the interview testing your depth or testing your honesty.

Most candidates with your shape get this wrong in the same way: they recite their CV chronologically ("I started in 2019 at X, then I moved to Y, then Z"), which sounds like a witness statement and quietly labels them as a task-taker. **A lead candidate leads with scope, then uses the timeline as evidence for it.**

---

### 0A.1 The structure — three beats, not a timeline

| Beat | Length | What it does |
|---|---|---|
| **1. Headline** | ~15 s | Who you are, total span, **what you own today**. Sets your level before any detail can drag it down. |
| **2. Arc** | ~55 s | The three eras as an **argument**, each contributing one lead competency — not a diary. |
| **3. Why here** | ~20 s | Why *this* role, why now, and what you want to be accountable for. Turns a history lesson into a motivation. |

Three rules for the whole answer:

1. **Say "I own", "I decided", "I led" — not "I was assigned", "I worked on", "I helped with".** This is the single biggest perception lever in the intro.
2. **Claim six years once, then never repeat it.** Repeating "six years" sounds defensive; stating it once and moving to evidence sounds settled.
3. **End on the role, not on your last job.** The last sentence should be about *their* problem, not your history.

---

### 0A.2 How to read your own timeline (the mapping that makes you a lead)

Your CV reads as three jobs. Re-read as three competencies, it reads as a lead's formation. This table is the argument — memorise the *right-hand column*, not the dates.

| Era | Duration | Say this | What it buys you at lead level |
|---|---|---|---|
| **Now — [XYZ company]**, module delivery with a 3-person team, Angular + React | **~1 yr** | "I own delivery of [module] end-to-end for a three-person team, on ASP.NET Core with Angular and React front ends." | **Delivery ownership** + current-stack credibility + learning velocity. This is the era that makes "lead" plausible, so it must be the first thing they hear. |
| **Before — product-based company**, new features + technical support | **~3 yrs** | "I built new features for a live product **and** carried production support — I was the person customers and support staff escalated to." | **Customer-facing production ownership.** Triage under SLA, reproducing from a vague report, root-cause discipline, the judgement to refuse a risky hotfix. This is the rarest sentence in a developer interview — most candidates have never spoken to a customer. It maps directly onto incident command, SLIs/SLOs, prioritisation and "would I put this in front of a customer" (Ch 10 §5–§7). |
| **Before that — software development**, ASP.NET / C# / MVC / Razor / JS / CSS / jQuery | **~2 yrs** | "I learned the craft on classic ASP.NET MVC and Razor — C#, SQL, jQuery, JavaScript — which is also why server-rendered HTML is native to me and not a foreign concept." | **Craft foundations** and a real advantage over SPA-only candidates: you understand the request/response model, forms, postbacks and progressive enhancement from the inside. |

**The point to make out loud (once, in these words):** *"So the shape of it is two years learning to build, three years building a live product and supporting it, and the last year owning delivery and moving the front end onto Angular and React."* That single sentence is your whole career as a progression, and it takes eight seconds.

---

### 0A.3 Word swaps that change the level you are perceived at

Your own phrasing is currently under-claiming. These are not lies — they are accurate descriptions of the same facts at the correct altitude. Run your CV and your speech through this table before the first call.

| What you say now | What a lead says | Why |
|---|---|---|
| "completed the assigned modules" | "owned delivery of [module] end-to-end — design, estimate, review, release, and the conversation when it slipped" | *Assigned* and *completed* are task-taker words. *Owned* and *end-to-end* are the same work described at the level you're being hired for. |
| "I worked with a team of 3 members" | "I led delivery for a three-person team" *(only if the §0A.5 tests pass)* | Membership vs accountability. Pick the one that is true. |
| "technical support" | "production support rotation — customer-facing triage, root cause, and fixes shipped under SLA" | *Support* sounds like a help desk. The reality is production engineering, and it is a lead asset. |
| "learnt by self interest" | "adopted Angular and React on my own initiative, and then took them into production" | Self-directed learning is **initiative** — the exact trait a lead is scored on. Never apologise for how you learned; state it as a decision you made. |
| "5 years of experience in .NET" | "Six years building web applications; five of those in JavaScript and jQuery, the last year full-stack with Angular and React" | Precise numbers survive probing. Vague numbers invite a test you don't need. |
| "I was responsible for" | "I own" / "I decided" / "I led" | *Responsible for* is passive and unfalsifiable. Active verbs are checkable — which is why they carry weight. |
| "I helped with the architecture" | "I made the architecture call on X, with these trade-offs" | Either you decided or you didn't. If you didn't, say "I contributed the API contract" — narrower but real. |
| "I'm familiar with Angular" | "I ship in Angular" or don't claim it | *Familiar* is read as "read a tutorial". Never downgrade a skill you actually used. |
| "I'm still learning React" | "I've shipped React in production; the things I'm deepening now are [X]" | Growth mindset framing without the confession the interviewer didn't ask for. |
| "junior developer" | "developer" (and let the dates carry the level) | You are not junior. Don't hand them the label. |

---

### 0A.4 Script 1 — the 90-second default

Rehearse this until it is *boring*. Fill the brackets with your real specifics — a real module name and a real domain beat any general claim.

> "I'm a full-stack .NET developer — six years building web applications, and for the last year I've owned delivery of [the orders / billing / reporting module] for a three-person team at [X]: ASP.NET Core and SQL on the back end, with Angular and React on the front end.
>
> Before that I spent about three years at a product-based company, where I built new features for [product] and also carried production support — I was the person who got the escalation when a customer's data looked wrong. That taught me what a defect actually costs, and it changed how I debug: I start from reproduction and blast radius now, not from the stack trace. It also made me careful about hotfixes — I've said no to a Friday-evening deploy more than once, and I've been right.
>
> I started out two years earlier on classic ASP.NET MVC and Razor Pages — C#, jQuery, JavaScript, CSS, SQL. That's why server-rendered HTML is native to me, and why moving to Angular and React wasn't a leap: I already had six years of DOM, events and async behind me.
>
> On the front end — nobody assigned Angular or React to me. I picked them up on my own time because I could see where the front end was going, and then we took both into production. The project I'd show you has an Angular 22 app and a React 19 app against the same style of .NET API, so I can talk about that choice as an engineering decision rather than a preference.
>
> The reason I'm interested in this role is that it's the job I've been doing for a year with less authority: I'm already doing the design discussions, the task breakdown, the code review and the release calls for three engineers. I'd like to do it deliberately, and be accountable for it."

**Why each beat is where it is:** "six years" up front sets the level; "owned delivery for three" claims lead before any detail can undercut it; the support era is deliberately second because it contains your most differentiated sentence; Angular/React comes fourth because it's the requirement, not your identity; the last line reframes a job change as a scoping-up.

---

### 0A.5 The three tests — is "led" defensible?

Before you use the word *led* about the team of three, run this honestly. A lead-interviewer will ask "what did *you* decide?" within 30 seconds, and the answer has to be concrete.

| Test | Ask yourself | If yes | If no |
|---|---|---|---|
| **Work allocation** | Did you break down the module and decide who did what? | "I did the task breakdown and allocated it across three people." | "I owned my modules and coordinated with the team." |
| **Technical direction** | Did you choose the approach — data model, API shape, state strategy, library? | "I made the design calls for [X]." | "I proposed the approach and the senior signed it off." |
| **Quality gate** | Did you review other people's code and hold a standard? | "I was the reviewing gate on our PRs." | "We cross-reviewed." |
| **Representation** | Did you speak for the team — to the client, PM, or another team? | "I ran the status and scope conversations." | Don't claim it. |
| **Growth / performance** | Did you help someone get better, or shape their work? | "I onboarded [name] and set their ramp-up plan." | "I mentored informally on reviews." |

**Scoring:** 3+ yes → use "led delivery for a three-person team". 1–2 yes → say **"senior developer on a three-person team, owning module delivery and the technical design for my areas"** and let Chapters 8–13 carry the leadership argument. Zero of these claimed while still saying "led" is the one error that ends a lead loop — `12-LEAD-INTERVIEW-PLAYBOOK.md` §4 has the exact phrasing for the honest middle position.

---

### 0A.6 Script 2 — the 20-second version (recruiter screen, "briefly")

> "Six years full-stack .NET — ASP.NET Core, C#, SQL, and six years of front-end work from Razor and jQuery through to Angular and React, which I've been shipping for the last year. For the last year I've also owned module delivery for a three-person team. I'm looking for a technical lead role where I'm accountable for the whole stack and for the team, not just my tickets."

Use it when someone says "just briefly" **and** resist the urge to expand — the 20-second version ending cleanly reads as confidence. Expire it, then stop talking.

---

### 0A.7 Script 3 — the 3-minute version (panel: "walk me through your CV")

Same arc, one concrete depth-story hook per era. Hook, not story — you're offering them the thread they should pull.

- **Earliest (2 yrs).** "Classic ASP.NET MVC and Razor — server-rendered views, jQuery, postbacks. The thing I took from it is that I actually understand what the browser is doing: request/response, forms, validation on both sides. *If you want, I can talk about why I think that makes me better at SPA state ownership.*"
- **Product company (3 yrs).** "Features plus support. The formative part was support — I learned to reproduce before I theorise. *Happy to go into an incident where we found a race condition that only appeared under load.*"
- **Last year.** "Delivery ownership and the front-end move: ASP.NET Core API, Angular 22 and React 19 front ends, message-driven processing, Docker, Azure Pipelines. *I can walk you through the two-front-end architecture decision and what I'd consolidate if it were mine.*"

Finish with the same "why here" line as the 90-second version. Do not let it drift past three minutes — a candidate who can compress is demonstrating the same judgement you're claiming.

---

### 0A.8 The follow-up map — where intros actually die

Every claim in your intro invites a specific follow-up. These are the ones you will get, in roughly this order.

| You said | They will ask | Where your answer is |
|---|---|---|
| "owned delivery for a three-person team" | "What did *you* decide, and what did you delegate?" | §0A.5 + `08` §3–§5 (delegation ladder, review standards) |
| "Angular and React" | "Which do you prefer, and why?" | `09` §6.3 — answer as a *decision with criteria*, never a preference |
| "one year, self-taught" | "How did you actually learn it?" | Have the concrete answer: the repo you built, the docs path, what you got wrong first (e.g. reaching for jQuery-style DOM code, subscribing in the template, missing `track`) |
| "production support" | "What was the worst incident?" | `12` §5 story 1 + `10` §6 (incident command) |
| "new features for a product" | "What's the hardest feature you designed?" | `09` §3 (ADR with rejected alternative) |
| "the last year" | "Why are you moving after a year?" | One sentence, no criticism of the employer: "I've grown into delivery ownership; the next step needs to be a formal lead role with a team I'm accountable for — that role doesn't exist here right now." |
| "six years" | "You've been a developer six years — why aren't you a lead already?" | "I have been doing it for a year without the title; that's precisely why I'm looking — I want the accountability and the scope to be explicit." |
| "front end in JS/jQuery then Angular/React" | "So are you a front-end or back-end developer?" | "Full stack with a back-end centre of gravity — I bring the API, data model and delivery with me. That's usually what a full-stack *lead* is for." |

---

### 0A.9 When they push on the year of Angular/React

It will come, probably as a friendly probe rather than a challenge. **Do not apologise, and do not inflate.** Three moves:

1. **Right-size it first — take the sting out yourself.** "To be straight with you: a year of professional Angular and React, not five. What I'd claim is that I've shipped both in production, I understand the models, and I can lead and review front-end work — I'm not going to out-code a front-end specialist in their own framework."
2. **Then reframe to what the role actually needs.** "For a technical lead, the front-end requirement is usually *"can this person make and enforce a front-end decision, keep two frameworks from fighting, and stop the API contract drifting?"* That's the part I'm strong at — and I've got both frameworks side by side in one system, which most single-framework developers don't."
3. **Then produce evidence, don't assert.** "In my current project there's an Angular 22 app and a React 19 app against the same API. I keep the auth refresh, error handling and state ownership consistent across both, and in a code review I'm looking for subscription leaks, unstable `key`s and prop drilling — the same defects dressed differently."

That answer scores *higher* than an inflated claim, because it demonstrates calibration — the quality that most distinguishes a lead from a senior. Interviewers are trained to discount someone who must be caught.

---

### 0A.10 What not to say in the first two minutes

| Don't | Why | Say instead |
|---|---|---|
| "I only have one year of Angular." | *Only* does the interviewer's work for them. | State the year as fact, then move to what it lets you do. |
| "I'm a .NET person, not really front end." | You just disqualified yourself from half the role. | "Full stack, back-end centre of gravity." |
| "I taught myself, so..." (trailing off) | Reads as an apology for initiative. | "I adopted it on my own initiative and took it into production." |
| "I completed the assigned modules." | Task-taker language, and *assigned* is the word they'll remember. | "I owned delivery of [module] end-to-end." |
| "I've been doing Angular for 2 years." | One follow-up collapses it, and then nothing else you said is trusted. | "A year in production, plus self-study before that." |
| A chronological CV recital. | Sounds like a witness; hides your scope. | Headline → arc → why here (§0A.1). |
| Any criticism of your current employer or team. | Reads as a risk you'll do it about them. | Motive for the *next* step, never a complaint about the last. |
| "I'm looking for a new challenge." | Empty. Every candidate says it. | "I want explicit ownership of a team and the technical direction." |

---

### 0A.11 The 40-minute rehearsal protocol

1. **Write it once, in full, in your own words** — not mine. Placeholders filled with real names. (~10 min)
2. **Record take 1 without reading.** It will be 2½ minutes. That's normal. (~5 min)
3. **Cut 40%.** Delete every sentence that isn't a claim you can defend. Target 90 seconds. (~10 min)
4. **Record takes 2–4 until it's word-perfect and bored-sounding.** Bored is the goal: fluency reads as seniority, enthusiasm reads as nerves. (~10 min)
5. **Rehearse the interruption.** Play the recording and stop yourself at a random point with "hold on — what did you decide there?" Answer it cold. Interviewers interrupt at 40 seconds, not at the end. (~5 min)

Also record the **20-second** and the **3-minute** versions. Recruiters and panels want different lengths, and candidates who can compress on request are silently marked as good communicators.

---

### 0A.12 The one-liner (email, LinkedIn, and the JD-matching line)

> **Full-stack .NET technical lead — 6 years ASP.NET Core/C#/SQL, 6 years front-end from Razor and jQuery to Angular and React, and a year of ownership for a three-person team.**

This is also the sentence to adapt when a recruiter asks "how do you match the JD?". Map **their** requirement to **your** evidence, in their order — never your order:

| JD line | Your line |
|---|---|
| ".NET / C# / ASP.NET Core" | Six years; current work is ASP.NET Core APIs, EF Core, SQL, message-driven processing (this repo) |
| "Angular 18+" | A year in production; current app is Angular 22 — standalone components, signals, `@if`/`@for`, OnPush |
| "React" | A year in production; current app is React 19 — hooks, RTK, Router 7, Actions |
| "SQL / data" | Six years; indexing and execution plans, EF Core translation, concurrency and transactions (Ch 5 + **`05D`**) |
| "Technical leadership" | Delivery ownership for three engineers: design calls, task breakdown, review gate, release decisions |
| "Azure / CI-CD" | Docker, Azure Pipelines, Container Apps, managed identity, Key Vault (Ch 11) |
| "Stakeholder / customer" | Three years of customer-facing production support — escalations, root cause, and scope conversations |

Fill that table in for the real JD before every interview, and say the top three rows out loud in the order the JD lists them. It takes five minutes and it is the difference between "strong candidate" and "exactly who we described".

---

### 0A.13 Wire it into the rest of the set

- Full lead-loop scripts, rubric and 12 behavioural stories: `12-LEAD-INTERVIEW-PLAYBOOK.md` §3–§5.
- The technical self-introduction when someone says "pick a project": `07-CHEATSHEETS.md` §7 (60-second walkthrough).
- Leadership depth behind every claim above: `08-LEAD-ROLE-AND-TEAM.md`.
- The scorecard to check whether you can survive the follow-ups: `13-FIRST-90-DAYS.md` §7.

Read §0A once, record the 90-second version tonight, then work backwards from the follow-up map (§0A.8) into the chapters it points at.

---

## 1. What you already have (and should not re-learn)

| Asset | Market value |
|---|---|
| ASP.NET MVC / Razor Pages / jQuery | You understand server-rendered HTML, postbacks, and progressive enhancement — the thing SPA devs often can't do |
| JavaScript + jQuery | DOM, events, async, XHR — the foundation under React and Angular |
| ASP.NET Core / MVC pipeline | Middleware, DI, filters, model binding, EF Core — you are *ahead* of most front-end candidates |
| SQL / Entity Framework | A genuine senior differentiator; SPA-only candidates are usually weak here |
| Deployment / Docker / pipelines | Operational maturity; lead interviews probe incident handling and CI/CD |
| **Leading a team of 3** | You have the hardest thing to fake. Most "senior" candidates have never run a 1:1 |

**The four genuinely new mental models (Chapters 1–4 fix these):**
1. **Declarative vs imperative rendering** — "find this element and change it" → "here is state, here is what the screen must look like".
2. **Client-side state ownership** — the server used to own the truth; now the client does, and staleness is your problem.
3. **The build toolchain** — Node, npm, esbuild/Vite, TS compilation, cache-busting, build-time env vars.
4. **Async everywhere** — Observables/RxJS, cancellation, race conditions, cleanup on unmount/destroy.

---

## 2. The chapter set (19 files, four tracks)

### Track A — Foundation: technical depth (Chapters 1–7)

| File | Size | What it gives you |
|---|---|---|
| `01-ANGULAR-18-PLUS.md` | 26 KB | Standalone bootstrap, DI lifetimes, **signals**, `@if`/`@for`+`track`, OnPush/zoneless, TypeScript |
| `02-ANGULAR-ASYNC-ROUTING-HTTP-FORMS.md` | 17 KB | RxJS flattening operators, guards returning `UrlTree`, interceptors + 401 refresh, forms |
| `03-REACT-19.md` | 20 KB | Hooks in depth, `key`/reconciliation, StrictMode, **React 19 Actions / `useActionState` / `useOptimistic` / `use()`** |
| `04-REACT-STATE-ROUTING-AND-FRONTEND-ARCH.md` | 18 KB | RTK slices/thunks, Router 7 guarding, **Angular-vs-React matrix**, testing, FE security |
| `05-ASPNET-CORE-SENIOR.md` | 32 KB | DI lifetimes + captive dependencies, EF Core, async correctness, JWT, API design, outbox/saga/SignalR/Kafka, observability |
| `05A-CSHARP-AND-OOPS-FUNDAMENTALS.md` | 75 KB | **The gate questions** — value/ref semantics, boxing, strings, records, NRT, generics, `Span`, closures, exceptions, concurrency keywords; OOP pillars → SOLID with honest caveats; 14 design patterns with repo hooks; **the four modelling decisions (money, time, identity, status)**; 64 Q&A + 50 one-liners + self-test |
| `05B-DATA-STRUCTURES-AND-ALGORITHMS.md` | 33 KB | Big-O without the maths, the **.NET collection decision table**, the 8 complexity questions about real code (quadratic spotting, N+1, deep paging), **algorithm-pattern trigger table**, LRU/heap/sliding-window worked designs, **SQL & index questions**, and the front-end data-structure parallels |
| `05C-CODING-QUESTION-BANK.md` | 49 KB | **The live-coding round** — the 8-step protocol and 4-dimension rubric, **36 problems with model C# solutions** (warm-ups → arrays → hashing → two pointers → sorting → stack/heap → **LINQ/EF** → **async/concurrency** → **SQL**), the 10 things interviewers flag, a 45-minute round plan, and a 4-week drill schedule with a self-grading rubric |
| `05D-SQL-AND-DATA-ACCESS-FOR-DOTNET.md` | 57 KB | **The SQL round, in full** — 98 Q&A: fundamentals (joins, NULL traps, window functions, keyset paging) → **indexing & execution plans** (reading a plan, SARGability, parameter sniffing, statistics, plan-regression guards) → **transactions, isolation, deadlocks, `rowversion` concurrency** → schema & data modelling (money, time, status, soft delete, multi-tenancy, zero-downtime migrations) → **EF Core ↔ SQL** (what SQL it generates, N+1, `Include` explosion, bulk ops, retries, migration process) → the 8-step troubleshooting playbook → security (injection, least privilege, RLS/masking/encryption) → scaling, Azure SQL tiers, RPO/RTO → a **6-minute whiteboard script** and a 25-question self-test |
| `06-INTERVIEW-QUESTIONS.md` | 44 KB | **130+ senior questions with model answers**, design scripts, 5 STAR stories, questions to ask |
| `07-CHEATSHEETS.md` | 19 KB | Side-by-side Angular/React/.NET syntax, concept dictionary, 60-second project walkthrough |

### Track B — Lead: the part that decides the outcome (Chapters 8–13)

| File | Size | What it gives you |
|---|---|---|
| `08-LEAD-ROLE-AND-TEAM.md` | 31 KB | The IC→lead shift, player-coach time budget, **1:1 framework**, SBI feedback, underperformance path, delegation ladder, code-review standards + comment taxonomy, ceremonies, estimation, stakeholder comms, saying no, hiring, **managing three different people** |
| `09-ARCHITECTURE-DECISIONS.md` | 30 KB | **ADR template with rejected alternatives**, C4, quality attributes with numbers, monolith vs modular-monolith vs services, **enforcing boundaries with arch tests**, the **strangler-fig migration plan**, front-end governance (incl. the Angular-vs-React decision), API versioning, data ownership, caching tiers |
| `10-DELIVERY-AND-ENGINEERING-EXCELLENCE.md` | 24 KB | **Standards as code**, trunk-based + flags, **an 11-point critique of this repo's real `azure-pipelines.yml`**, testing strategy + test-quality metrics, DoD, **DORA**, incident command, postmortems, SLI/SLO/error budget, on-call for a team of 3, security gates |
| `11-AZURE-PLATFORM-FOR-LEADS.md` | 19 KB | **Where this repo actually runs** (App Service vs Container Apps vs AKS), managed identity → Key Vault, **Entra ID/OIDC end to end**, Service Bus vs Event Hubs vs Event Grid, outbox, observability, Cosmos vs Azure SQL, IaC, **cost levers** |
| `12-LEAD-INTERVIEW-PLAYBOOK.md` | 28 KB | The lead loop + rubric, **90-second opening script**, how to talk about a team of 3, **12 behavioural stories with model answers**, the case-study round structure, salary/level negotiation, questions to ask, red flags, **the 15 things that sink lead candidates** |
| `13-FIRST-90-DAYS.md` | 15 KB | **A real 30/60/90 plan** (plus the repo-specific week-by-week version), the **Lead Readiness Scorecard** (30 items), a 4-week rehearsal schedule, and the nine sentences to hold in your head on the day |

---

### Track C — Assessment simulation

| File | Size | What it gives you |
|---|---|---|
| `14-WORLDBANK-FULLSTACK-ANGULAR-ASSESSMENT.md` | 104 KB | **The 60-minute screening assessment** — format blueprint and minute-by-minute plan, then **158 numbered questions with answers** (C# 24 · ASP.NET Core 20 · EF Core/SQL 18 · **Angular 30** · TS/JS 15 · RxJS 10 · predict-the-output 15 · coding tasks 10 · troubleshooting 11 · values 5), the **20 facts to have at instant recall**, a **30-question timed mock with answer key and scoring bands**, the **48-hour and 90-minute plans**, and the **10 ways people fail a timed paper** |

---

## 3. Repository map — what to open when studying each chapter

### 3.1 `frontend/` — Angular 22.0.7

| File | What it teaches | Chapter |
|---|---|---|
| `frontend/package.json` | Angular 22, TS 6.0, **Vitest** (not Karma), `@angular/build` (esbuild/Vite), Node ≥22 | 1 |
| `frontend/angular.json` | `@angular/build:application` + `@angular/build:unit-test` builders, budgets, prod config | 1 |
| `frontend/src/main.ts` | `bootstrapApplication(App, appConfig)` — no `AppModule` | 1 |
| `frontend/src/app/app.config.ts` | `ApplicationConfig` providers, `provideRouter`, `provideHttpClient`, `provideBrowserGlobalErrorListeners`, and the **legacy** `HTTP_INTERCEPTORS` + `withInterceptorsFromDi()` (your best self-critique) | 1, 2 |
| `frontend/src/app/app.routes.ts` | Lazy `loadComponent`, `canActivate: [authGuard]`, `redirectTo` + `pathMatch`, `:id` params | 2 |
| `frontend/src/app/guards/auth.guard.ts` | Functional guard, `inject()`, returns `UrlTree` instead of `false` | 2 |
| `frontend/src/app/interceptors/auth.interceptor.ts` | Class-based `HttpInterceptor` — compare with the functional `HttpInterceptorFn` you *would* write | 2, 9 |
| `frontend/src/app/services/auth.service.ts` | `providedIn: 'root'` singleton ↔ `AddSingleton`, `inject()`, `tap()` side effect, token in `localStorage` (and why that's an XSS trade-off) | 1, 2 |
| `frontend/src/app/services/product.service.ts` | Typed `HttpClient` CRUD returning `Observable<T>` | 2 |
| `frontend/src/app/components/**` | Standalone components, template + styles, list rendering, reactive login form | 1, 2 |
| `frontend/src/environments/environment.ts` | Build-time config (`apiUrl: '/api'`) — and why env vars are baked at build, not runtime | 1, 10 |

### 3.2 `ReactDotNetDemo/` — React 19.2.8 + Vite 8 + RTK 2

| File | What it teaches | Chapter |
|---|---|---|
| `ReactDotNetDemo/package.json` | React 19.2, Vite 8, **Oxlint**, react-router-dom 7, RTK 2 / react-redux 9 | 3 |
| `ReactDotNetDemo/src/main.jsx` | `createRoot`, `StrictMode` (double-invoked effects!), `<Provider>` | 3 |
| `ReactDotNetDemo/src/App.jsx` | `<Routes>`/`<Route>` v7 declarative routing with a layout route | 4 |
| `ReactDotNetDemo/src/app/store.js` | `configureStore` + reducer map | 4 |
| `ReactDotNetDemo/src/features/auth/authSlice.js` | `createSlice`, `createAsyncThunk`, `extraReducers` pending/fulfilled/rejected, `localStorage` hydration | 3, 4 |
| `ReactDotNetDemo/src/features/products/productsSlice.js` | Same pattern for products — compare directly with the Angular service | 4 |
| `ReactDotNetDemo/src/components/ProtectedRoute.jsx` | Route guarding the React way (`<Navigate>` / `Outlet`) ↔ Angular `canActivate` | 4 |
| `ReactDotNetDemo/src/components/ProductList.jsx` | `useEffect` + `useSelector` + `useDispatch` — the classic effect/deps trap | 3 |
| `ReactDotNetDemo/src/components/ProductForm.jsx` | Controlled form, validation, navigation after save | 3 |
| `ReactDotNetDemo/src/features/users/UserList.jsx` | Feature-folder structure (the boundary discipline of Ch 9 §5.3) | 4, 9 |

### 3.3 `backend/AngularDotnetDemo.Api/` — ASP.NET Core (net10.0)

| File | What it teaches | Chapter |
|---|---|---|
| `backend/AngularDotnetDemo.Api/Program.cs` | Top-level statements, DI registration, CORS, JWT bearer, BCrypt, OpenAPI, endpoint mapping | 5, 9 |
| `Models/User.cs`, `Models/Product.cs` | POCO entities returned directly — and why that's a code smell at scale | 5, 9 |
| `backend/.../Dockerfile` | Multi-stage build → the container that Ch 11 deploys | 10, 11 |

### 3.4 `DeliverFoodApp/` — the lead showcase: message-driven services

| Area | What it demonstrates | Chapter |
|---|---|---|
| `src/FoodDelivery.Api/Program.cs` | EF Core + MassTransit + Kafka + SignalR + health checks + gRPC in one host — "how many sidecars is too many?" | 5, 9 |
| `src/FoodDelivery.Api/Controllers/OrderController.cs` | Controller → service → EF, async all the way, DTO projection | 5 |
| `src/FoodDelivery.Api/Hubs/OrderHub.cs` | SignalR hub, groups, push instead of polling | 5, 11 |
| `src/FoodDelivery.Consumers/**` | The async side: idempotency, retries, DLQ, poison messages | 5, 9, 11 |
| `src/FoodDelivery.Shared/**` | Shared contracts — and the boundary discipline that stops it becoming a god-project | 9 |
| `Dockerfile.api`, `Dockerfile.consumers` | Two deployables → two Container Apps in Ch 11 | 10, 11 |

### 3.5 Delivery and platform files (your Ch 10/11 lab)

| File | What it teaches |
|---|---|
| `azure-pipelines.yml` | Three stages, three Docker builds, ACR push, `BuildId`+`latest` tags, stub deploy — **11 documented findings** in Ch 10 §4.2 |
| `docker-compose.yml` | Local topology, `app-network`, frontend on `8082` — and why compose is not a production design |
| `DOCKER_AZURE_DEPLOYMENT_GUIDE.md`, `DEPLOYMENT_SUMMARY.md` | The Azure story to rehearse in Ch 11 |
| `DEBUGGING_GUIDE.md` | Incident-response posture: how to answer "production is broken, go" |
| `Diagram.md` | Turn it into the C4 container map for your Ch 9 answer |
| *(absent)* test projects | **The gap to name out loud** — Ch 10 §6.3 turns it into a strength |

---

## 4. The study plan

### 4.0 If you have a 1-hour screening assessment first

Chapters 1–13 prepare you for a *conversation*. A proctored 60-minute paper is a different sport: recall under time pressure, no follow-up questions, no chance to explain.

1. **`14` §14 mock, 30 min, timed, no notes.** Score against §14.2. This tells you where the marks are actually going.
2. **`14` §3 (the 20 facts)** — recite until instant. This alone is worth several points.
3. **`14` §2 (game plan)** — write the minute budget on paper next to you before you start. Triage → easy pass → coding → flagged pass → no blanks.
4. **Weakest section only.** If it's Angular: `14` §7 then `01` §signals/DI/lifecycle. If SQL: `14` §6 then `05D` §11. Don't re-read what the mock says you already know.
5. **`14` §16** the day before — environment check, second browser, hotspot.

**Assessment day rule:** answer every question, 60-second cap per MCQ, working code before elegant code, and never spend your freshest minutes on C#.

### 4.1 If the interview is 8 weeks away (full lead track)

| Week | Focus | Chapters | Deliverable at end of week |
|---|---|---|---|
| 1 | Angular depth | 1, 2, 7 | Record yourself explaining the Angular app end to end in 5 min |
| 2 | React depth + FE architecture | 3, 4 | Same for the React app; write the Angular-vs-React comparison from memory |
| 3 | ASP.NET Core + distributed | 5 | Whiteboard the `DeliverFoodApp` flow: API → Kafka → consumer → SignalR |
| 3b | **Language gate + coding drill + data round** *(runs in parallel, 45 min/day)* | 5A, 5B, 5C, **5D** | Answer 20 of the 25 self-test questions in 5A without notes; run 3 recorded coding drills from 5C §12; recite the 12 rapid-fire SQL drills in **5D §11** and rehearse the **5D §12** whiteboard script twice |
| 4 | Architecture ownership | 9 | Write 2 real ADRs for this repo (framework consolidation; SPA hosting) |
| 5 | Delivery + Azure | 10, 11 | Deliver the 11 pipeline findings and the hosting answer, fluent |
| 6 | Leadership | 8 | Script your 1:1 template; write 6 behavioural stories |
| 7 | Interview simulation | 6, 12 | Full mock loop; 90-second story word-perfect; salary answers ready |
| 8 | Consolidation | 13 | 90-day plan memorised; scorecard above 115; re-drill only stumbles |

### 4.2 If the interview is 3 weeks away (triage)

1. **Week 1:** `07-CHEATSHEETS.md` + `06-INTERVIEW-QUESTIONS.md` — get to "can discuss" on every technical topic. Do not chase mastery. **Then `05A` §10 + §11 and `05B` §2 + §4 only** — the rapid-fire tables and the trigger drills cover the whole fundamentals gate in about 90 minutes.
2. **Week 2:** `08` (leadership) + `12` (stories + scripts). This is where lead interviews are won or lost.
3. **Week 3:** `13` (90-day plan) + `10` §4.2 (pipeline critique) + `11` (hosting + identity answer), then mock loops.

### 4.3 If you have one evening (emergency)

0. If the loop includes a **coding round**, read `05C` §0 and §10 first — 25 min. That protocol alone is worth a full scoring point, and §10 tells you exactly what to avoid.
1. `12-LEAD-INTERVIEW-PLAYBOOK.md` §3 (90-second story) and §5 (stories) — 90 min.
2. `13-FIRST-90-DAYS.md` §2 (the one-page plan) — 20 min.
3. `10` §4.2 (11 pipeline findings) — 20 min.
4. `07` §7 (60-second project walkthrough) — 20 min.
5. **`05D` §11 (12 rapid-fire SQL drills) + §12 (the whiteboard script)** — 25 min. SQL is asked in four of the rounds, and it's where your six years are visible.
6. Sleep. Recall beats cramming.

### 4.4 The daily loop (45 minutes, every day)
- **15 min** — one chapter section, out loud, standing.
- **20 min** — answer 3 questions from that chapter's list, **recorded**, then listen back.
- **10 min** — write the one line you'd keep if you could only keep one.

**The one rule that matters:** reading builds recognition; interviews test **recall**. Everything out loud, everything recorded.

---

## 5. The habits that make you sound like a lead

1. **Trade-off sandwich** — answer, then "the cost of that is…", then "if my assumption is wrong, the alternative is…".
2. **Rejected alternative** — never present one option. Three, with one recommended.
3. **Reversal trigger** — "we'd revisit if [traffic passes X / the team grows past Y / the bill exceeds Z]".
4. **Name the mechanism** — not "we value quality" but "warnings-as-errors, arch tests, green-pipeline merge gate".
5. **Numbers** — leads get remembered for numbers. Deploys per week, change-failure rate, PR latency, MTTR, migration weeks, headcount growth.
6. **"We" for delivery, "I" for decisions and failures.**
7. **The self-critique** — one per interview, always a real one from this repo (the class-based interceptor, the missing tests, the two frameworks).
8. **Two-second pause before answering leadership questions.** It reads as thought, not delay — and it stops you giving a 4-minute answer.

---

## 6. The 30-second version

> "Six years building web applications on .NET — MVC, Razor, jQuery and JavaScript, then ASP.NET Core and EF. The last year I've owned delivery for a three-person team, and I moved our front end onto Angular and React — self-taught, then shipped, with both frameworks live in the same system right now. I want a technical lead scope where I'm accountable for the team and the technical direction, not just my tickets. In this repo you can see both sides of me: two front ends and a .NET API, a message-driven delivery system, and a pipeline I can tell you eleven things I'd change about, in priority order."

For the 20-second and 3-minute variants of this, and the method behind all of them, see **§0A** above — that section supersedes any older script in this set.

---

**Start here tonight (90 minutes):**
1. **§0A of this document** — write and record the 90-second introduction. Nothing else matters until this is fluent.
2. `12-LEAD-INTERVIEW-PLAYBOOK.md` §3 — the lead-loop tightening of the same story.
3. `13-FIRST-90-DAYS.md` §2 — the one-page plan.
4. `08-LEAD-ROLE-AND-TEAM.md` §1.2 — memorise the player-coach time budget, so the first leadership question is a win rather than a wobble.

Then work `13-FIRST-90-DAYS.md` §7's scorecard. Whatever scores lowest is your next chapter.
