# Chapter 5D — SQL & Data Access for .NET (Q&A)

> **Sitting a timed assessment?** `14-WORLDBANK-FULLSTACK-ANGULAR-ASSESSMENT.md` §6 condenses this into 18 exam questions (including the two SQL coding tasks), and §10 has SQL predict-the-output traps.

> **Why this chapter exists separately.** SQL is the one topic in a *Microsoft Fullstack Technical Lead* loop where six
> years of real .NET delivery gives you an advantage no bootcamp candidate can fake — and it is the topic most
> candidates prepare last. It is asked in four different places: the C#/fundamentals screen (*"what's a clustered
> index?"*), the coding round (*"write me this query"*), the EF Core deep-dive (*"what SQL does this generate?"*), and the
> architecture round (*"this endpoint is slow at 2M rows — go"*).
>
> `05B` §7 is the one-page instinct table, and `05C` §9 has three SQL coding problems. **This chapter is the full
> question-and-answer bank** — 98 numbered questions across eight sections, plus the whiteboard script, the
> rapid-fire drills and a scored self-test.
>
> Assume **SQL Server / Azure SQL** unless a question says otherwise (that is what a Microsoft role asks). Notes on
> PostgreSQL differences appear where they matter.

---

## 0. How the SQL round is actually scored

There are three tiers of SQL question, and candidates usually prepare only tier 1.

| Tier | What it sounds like | What they're scoring | How many in a lead loop |
|---|---|---|---|
| **1 — Vocabulary** | "Clustered vs non-clustered?" "What's a CTE?" | Do you have the words at all | 3–5 (warm-up) |
| **2 — Diagnosis** | "This query went from 200 ms to 8 s after a release. What do you check?" | Structured debugging, not memorised facts | 3–5 (the real scoring) |
| **3 — Judgement** | "Two services need `Orders`. One writes, one reports. Design it." "EF Core or Dapper here, and why?" | Ownership, trade-offs, cost | 2–3 (lead differentiator) |

**The trap:** tier-1 answers are binary — right or wrong, no partial credit. Tier-2 and tier-3 answers are graded on
structure, and a candidate who says *"I don't know the exact DMV name, but here's how I'd find it"* scores **higher**
than one who recites a fact and stops. You have six years of tier-2 and tier-3 experience. Answer in tier 2 and 3
whenever you're allowed to.

### The two sentences that set your frame

> **"I've spent six years on SQL Server with EF Core on top — I know what the ORM generates and I know when to drop
> below it."**

That single line does more work than any individual answer, because it tells the interviewer where to aim. Say it in
the intro (see `00-START-HERE.md` §0A) and again at the start of the data round.

### What sinks candidates in this round

| Behaviour | Why it's fatal | Do instead |
|---|---|---|
| Answers "index" for every slow query | Signals you've never read a plan | "First I'd capture the actual plan and check the reads-to-rows ratio." |
| Can't say what SQL EF Core generated | Proves you treat the ORM as a black box | "I'd run `ToQueryString()` — or turn on `LogTo` in dev." |
| Adds an index for every problem | Indexes cost writes; a lead must know that | "An index here fixes the read and adds ~X to every insert. Worth it only if the read path is hot." |
| Says "just add `WITH (NOLOCK)`" | Dirty reads + wrong totals — a career-limiting answer | "That's the thing I'd explicitly *not* do — here's why, and here's what I'd do." |
| Has no story about a data incident | Tier-2 questions have no answer without one | Prepare one (see §7.8). |

---

## 1. The answer template — use this for every SQL question

Four beats, 20–40 seconds for tier 1, 2–4 minutes for tier 2–3.

1. **Name it mechanically, not conceptually.** Not "an index makes it faster" — "a nonclustered index is a separate B-tree
   whose leaf holds the key plus a pointer to the row, so a lookup that only needs the key touches ~3 pages instead of
   scanning 200,000."
2. **Say where it bites in *our* code.** "...which is why our `Orders` list query, which filters `Status` and sorts
   `CreatedAt`, does a scan today — there's no index on that pair."
3. **State the cost.** "...adding it costs one more B-tree per insert and slows the write path; if `Orders` has a hot
   insert rate that's a real trade."
4. **Land on the risk or the tripwire.** "...so I'd add it, watch write latency, and add a regression test that asserts
   the plan seeks rather than scans."

Beats 1 and 3 are what separate senior from mid. **Beat 3 alone will not be said by any other candidate in the room.**

### If you genuinely don't know

> "I'd have to check the exact DMV, but the approach is: pull the top queries by total CPU from Query Store, grab the
> actual plan for that one, and look for the key lookup and the row-count estimate mismatch. In my experience that
> finds it in ten minutes; I'd rather do that than guess an index."

Never invent a name. Never stop at "I don't know".

---

## 2. The schema this chapter uses

Referencing one consistent schema is how you sound like you own a system. This is the shape of the app in this repo —
`Product` and `User` are literally `backend/AngularDotnetDemo.Api/Models/`, and the order tables come from the
`DeliverFoodApp` demo (`Order`, `OrderItem`, `MenuItem`, `Inventory`) — simplified and documented here so every query
in the chapter is reproducible.

```sql
-- Assumed schema, used by every query in this chapter.
Users     (Id INT PK, Username NVARCHAR(100) NOT NULL UNIQUE,
           PasswordHash NVARCHAR(200) NOT NULL, CreatedAt DATETIME2 NOT NULL)
Products  (Id INT PK, Name NVARCHAR(200) NOT NULL, Description NVARCHAR(1000),
           Price DECIMAL(18,2) NOT NULL, CreatedBy NVARCHAR(100), CreatedAt DATETIME2 NOT NULL)
Orders    (Id INT PK, UserId INT NOT NULL FK→Users, Status NVARCHAR(30) NOT NULL,
           TotalAmount DECIMAL(18,2) NOT NULL, CreatedAt DATETIME2 NOT NULL)
OrderItems(Id INT PK, OrderId INT NOT NULL FK→Orders, ProductId INT NOT NULL FK→Products,
           Quantity INT NOT NULL, UnitPrice DECIMAL(18,2) NOT NULL)
```

**Three things I want you to say out loud about this schema** (they're the honest self-critique that buys credibility,
and they cross-link `05A` §"four modelling decisions"):

| Weakness in the real repo | Why it matters | What you'd change |
|---|---|---|
| `Status` is a free-text `NVARCHAR(30)` | Typos create new statuses; no compile-time safety; `'Completed '` is a different value | Enum-backed `int`/lookup table + app-owned state machine, `CHECK` constraint as a safety net |
| `CreatedAt DATETIME2` (no offset) | Ambiguous across regions; `DATETIME2` has no timezone | `DATETIMEOFFSET` + `SYSUTCDATETIME()` at the edge, convert in the UI |
| `TotalAmount` is stored and settable | Can drift from `SUM(OrderItems)` | Recompute from items in the domain, store as a materialised value, or add a `CHECK`/trigger guard |

> **Say:** "The schema's the part I'd change if I inherited it — `Status` as a string means the database can't enforce
> a state machine, and `DATETIME2` without an offset means 'when was this ordered' is ambiguous across regions.
> Those are one-hour fixes now and a data migration in eighteen months."

---

## 3. Section A — Fundamentals (Q1–Q18)

Warm-ups. Answer fast, correctly, then stop. Do **not** volunteer depth here — save it for tier 2.

### Q1. Clustered vs non-clustered index?

**Clustered:** the table *is* the index. Leaf level = the actual data rows, stored in key order. **One per table.** The
primary key by default — and that means a bad PK choice is a storage decision, not just an identity decision.

**Non-clustered:** a separate B-tree. Leaf level = index key + row locator (the clustered key, or a RID for a heap).
So every non-clustered index **contains the clustered key** — which is why a wide clustered key (a 4-part composite, or
a 16-byte `UNIQUEIDENTIFIER`) bloats every index on the table.

> **Say:** "One clustered index per table and it *is* the table, so I choose it for the access pattern — usually an
> ever-increasing key so inserts append to the end of the B-tree. Everything else is non-clustered, and each one pays
> for its reads with slower writes."

**Follow-up they will ask:** *why not cluster on `ProductName`?* Because random insert points fragment the B-tree and
every row update that changes the name moves the row. Range scans on name are better served by a non-clustered index.

### Q2. What's a covering index, and why do you care?

An index that contains every column the query references, so the engine never touches the base table. You add
non-key columns with `INCLUDE`:

```sql
CREATE NONCLUSTERED INDEX IX_Orders_Status_CreatedAt
    ON dbo.Orders (Status, CreatedAt DESC)
    INCLUDE (UserId, TotalAmount);
```

`Status` and `CreatedAt` are key columns (filter and sort order matter); `UserId` and `TotalAmount` are payload — they
only need to be *present*, not ordered. This turns a seek + 5,000 key lookups into a single seek.

**Cost:** wider leaf pages = more I/O per scan and more write amplification per insert. Covering indexes are how
tables end up with 14 indexes and 40% write overhead.

> **Say:** "A covering index is the single highest-leverage fix for a hot read path, and the honest cost is that it
> widens the B-tree, so I only do it for a query I know is in the top-5 by total CPU."

### Q3. `WHERE` vs `HAVING`?

`WHERE` filters rows **before** grouping; `HAVING` filters groups **after**. Put the predicate in `WHERE` whenever it
doesn't reference an aggregate — it reduces the rows that reach the aggregation, so it's both faster and clearer.

```sql
SELECT o.UserId, SUM(o.TotalAmount) AS Revenue
FROM   dbo.Orders o
WHERE  o.Status = 'Completed'              -- before grouping: fewer rows to aggregate
  AND  o.CreatedAt >= DATEADD(DAY, -30, SYSUTCDATETIME())
GROUP BY o.UserId
HAVING SUM(o.TotalAmount) > 10000;         -- after grouping: filters on the aggregate
```

### Q4. `COUNT(*)` vs `COUNT(column)` vs `COUNT(1)`?

`COUNT(*)` and `COUNT(1)` count rows. `COUNT(col)` counts **non-NULL** values of `col`. That difference is a real bug
generator: `COUNT(ManagerId)` silently under-reports headcount because some rows have no manager.

`COUNT(*)` is also the only one the optimiser can satisfy from the narrowest index; `COUNT(1)` is identical work, so
prefer `*` for readability.

### Q5. `INNER` vs `LEFT` vs `CROSS` join — and the `LEFT` join trap?

- **INNER** — only matching pairs.
- **LEFT** — every left row, NULLs where no match.
- **CROSS** — cartesian product; an accidental `CROSS` (a missing join predicate) is how a report turns 50,000 rows into
  2.5 billion.

**The trap:** putting a *right-table* predicate in `WHERE` silently converts a `LEFT` join into an `INNER`:

```sql
-- WRONG: the WHERE makes it an inner join — products with no orders vanish entirely
SELECT p.Id, COUNT(o.Id) AS OrderCount
FROM   dbo.Products p
LEFT   JOIN dbo.Orders o ON o.Id = p.Id
WHERE  o.Status = 'Completed'      -- NULL rows fail this predicate
GROUP BY p.Id;

-- RIGHT: predicate belongs in the JOIN for the "keep the unmatched rows" intent
SELECT p.Id, COUNT(o.Id) AS OrderCount
FROM   dbo.Products p
LEFT   JOIN dbo.Orders o ON o.Id = p.Id AND o.Status = 'Completed'
GROUP BY p.Id;
```

**Tier-2 habit:** on any `LEFT JOIN` answer, add *"I check `IS NULL` filters first, because that's where the semantics
flip."* It's a 5-second sentence that sounds like experience because it is.

### Q6. `EXISTS` vs `IN` vs `JOIN` for the "has any related row" question?

They're semantically different and the optimiser often produces the same plan, but the **NULL behaviour** differs:

| Construct | Returns | NULL danger |
|---|---|---|
| `EXISTS (SELECT 1 ...)` | Boolean; stops at first match | None. Safest. |
| `IN (SELECT col ...)` | True/false/unknown | `NOT IN` with a NULL in the subquery returns **no rows at all** |
| `JOIN` | Multiplies rows when the relation is one-to-many | Needs `DISTINCT` to answer an existence question — usually a smell |

```sql
-- "products that have never been ordered" — safe
SELECT p.Id, p.Name
FROM   dbo.Products p
WHERE  NOT EXISTS (SELECT 1 FROM dbo.OrderItems oi WHERE oi.ProductId = p.Id);
```

> **Say:** "For existence I write `EXISTS` — it expresses the intent, it short-circuits, and it can't be broken by a
> NULL. `NOT IN` against a nullable subquery is the query that returns zero rows and nobody believes the bug report."

**Follow-up:** *is `EXISTS` always faster?* No — with older SQL Server versions and simple equality `NOT IN` can be
faster because the optimiser recognises it as an anti-semi-join with a different shape. Correctness first, then measure.

### Q7. `UNION` vs `UNION ALL`?

`UNION` de-duplicates → requires a sort or hash, which costs memory and time and can spill to tempdb. `UNION ALL`
concatenates. **Use `UNION ALL` unless you need de-duplication**, and if you find yourself needing `UNION` a lot, ask
whether the query is really two queries.

Also worth naming: `UNION` de-duplicates *all* columns including ones you didn't think were discriminating (a
`CreatedAt` timestamp makes every row unique, so `UNION` silently stops removing anything).

### Q8. What's a CTE, and when is it *not* a performance improvement?

A `WITH cte AS (...)` is a named subquery that makes a query readable. **It is not automatically materialised** — in SQL
Server (2019+) a regular CTE is inlined, so referencing it three times can execute it three times.

```sql
-- Readable, but the CTE is inlined: OrderTotals may be computed once or thrice depending on the plan
WITH OrderTotals AS (
    SELECT OrderId, SUM(Quantity * UnitPrice) AS Total, COUNT(*) AS LineCount
    FROM   dbo.OrderItems
    GROUP BY OrderId
)
SELECT u.Username, COUNT(*) AS Orders, SUM(t.Total) AS Revenue
FROM   dbo.Orders o
JOIN   OrderTotals t ON t.OrderId = o.Id
JOIN   dbo.Users   u ON u.Id = o.UserId
WHERE  o.Status = 'Completed'
GROUP BY u.Username;
```

**When it isn't:** when you need the intermediate result twice and it's expensive — then materialise into a `#temp`
table (with statistics, indexable, reusable) rather than relying on the CTE. And recursive CTEs (`WITH x AS (... UNION ALL
... )`) *are* materialised, which is why an accidental infinite recursion hits the `MAXRECURSION` limit.

**They may ask:** *CTE vs temp table vs table variable?* → §A Q16.

### Q9. Window functions — why do leads care?

Because they replace three patterns that are otherwise written badly: paging, ranking-with-ties, and running totals.
And because `ROW_NUMBER()` is how you de-duplicate a table.

```sql
-- Latest order per user (the classic "top-N per group")
WITH Ranked AS (
    SELECT o.*, ROW_NUMBER() OVER (PARTITION BY o.UserId ORDER BY o.CreatedAt DESC) AS rn
    FROM   dbo.Orders o
)
SELECT * FROM Ranked WHERE rn = 1;
```

| Function | Ties | Use |
|---|---|---|
| `ROW_NUMBER()` | Arbitrary but deterministic | Paging, de-duplication, "the newest one" |
| `RANK()` | Ties share a number, then **skip** (1,1,3) | Competition-style top-N (include everyone tied at 3rd) |
| `DENSE_RANK()` | Ties share, no skip (1,1,2) | Leaderboards, percentile buckets |
| `SUM() OVER (ORDER BY ...)` | — | Running totals without a self-join |
| `LAG()` / `LEAD()` | — | Day-over-day deltas without a self-join |

> **Say:** "Window functions let me answer 'per group' questions in one pass instead of a correlated subquery per row —
> which is the most common accidental O(n²) I see in review."

**Tier-2 follow-up:** *why does `ROW_NUMBER()` need a `PARTITION BY` here?* Without it you rank the whole table and get
one row back — a bug that looks like "the endpoint returns 1 order".

### Q10. How do you paginate a large table?

`OFFSET/FETCH` is correct but **costs O(offset)** — the engine still walks past the skipped rows. Page 500 of a
500,000-row list is slow no matter how good your index is.

```sql
-- OK for shallow pages; degrades linearly
SELECT Id, Name, Price
FROM   dbo.Products
ORDER  BY Id
OFFSET 100000 ROWS FETCH NEXT 25 ROWS ONLY;

-- Keyset / seek pagination: constant cost, the answer for deep paging and infinite scroll
SELECT TOP (25) Id, Name, Price
FROM   dbo.Products
WHERE  Id > @lastSeenId          -- or (CreatedAt, Id) for a non-unique sort
ORDER  BY Id;
```

**The trade-off you must state:** keyset paging can't jump to "page 500" and needs a stable, unique, indexed sort key
(composite `(CreatedAt, Id)` to break ties). That's why admin UIs with page numbers keep `OFFSET` and public feeds use
keyset.

`sys.dm_exec_query_stats`-based homework: if a query has high `logical_reads` but low `row_count`, deep `OFFSET` is a
prime suspect.

### Q11. `GROUP BY` with conditional aggregation — how do you pivot without `PIVOT`?

```sql
SELECT o.UserId,
       SUM(CASE WHEN o.Status = 'Completed' THEN o.TotalAmount ELSE 0 END) AS CompletedRevenue,
       SUM(CASE WHEN o.Status = 'Cancelled' THEN o.TotalAmount ELSE 0 END) AS CancelledRevenue,
       COUNT(CASE WHEN o.Status = 'Completed' THEN 1 END)                      AS CompletedCount
FROM   dbo.Orders o
GROUP BY o.UserId;
```

Note `COUNT(CASE ... THEN 1 END)` — omit the `ELSE 0` so non-matching rows yield NULL and aren't counted. Adding
`ELSE 0` makes the count equal the total row count, which is a genuinely common bug.

### Q12. How do you concatenate rows into one string?

```sql
SELECT o.Id,
       STRING_AGG(p.Name, ', ') WITHIN GROUP (ORDER BY p.Name) AS Items
FROM   dbo.Orders o
JOIN   dbo.OrderItems oi ON oi.OrderId = o.Id
JOIN   dbo.Products   p  ON p.Id = oi.ProductId
GROUP BY o.Id;
```

`STRING_AGG` (2017+) replaced the `FOR XML PATH` hack. **Watch the 8000-byte truncation** on `VARCHAR` — cast to
`NVARCHAR(MAX)` if the output can be long, or you get a silent cut.

**Lead note:** this is the query behind every "export shows only the first few items" support ticket.

### Q13. `ISNULL` vs `COALESCE` — is there a difference that matters?

Yes, two:

```sql
SELECT ISNULL(col, 'n/a')     -- returns the datatype of the FIRST argument
SELECT COALESCE(col, 'n/a')   -- returns the datatype of the HIGHEST precedence argument
```

1. **Truncation:** `ISNULL(CAST('long text' AS VARCHAR(5)), 'default')` returns `VARCHAR(5)`. If the default is longer
   than the column, it's silently cut. `COALESCE` sizes correctly.
2. **`COALESCE` is ANSI and translates to a `CASE` expression** — which means it can be non-SARGable in a predicate:
   `WHERE COALESCE(Status,'x') = 'Completed'` won't seek.
3. `ISNULL` takes exactly 2 arguments; `COALESCE` takes N.

> **Say:** "For a predicate, neither belongs — I fix the data instead of coercing it, because any function on a column
> costs me the index seek. Inside a `SELECT` list, `ISNULL` for a single fallback, `COALESCE` for the ANSI-comparable
> form."

### Q14. What does `NULL` mean, and what's `NULL = NULL`?

`NULL` is *unknown*, not empty and not zero. Therefore:

- `NULL = NULL` → **unknown** (not true), so `WHERE col = NULL` returns nothing. Use `IS NULL`.
- Any arithmetic with `NULL` → `NULL` (`TotalAmount + NULL = NULL`) — this is how a nullable join column turns a whole
  revenue report into NULLs.
- `COUNT(col)` skips them; `SUM(col)` skips them (so an empty group sums to NULL, not 0 — use `ISNULL(SUM(x),0)`).
- `NOT IN (… NULL …)` → returns no rows (§A Q6).
- Aggregates: `AVG` skips NULLs, so `AVG` over a column with blanks is the average of *known* values — often not what
  the business thinks it asked for. That's a reporting bug, not a SQL bug.
- `@@ROWCOUNT` after `SET NOCOUNT ON` inside a proc — separate trap, see §E.

**The modelling opinion to add:** *"I prefer `NOT NULL` with a sentinel only where the sentinel is genuinely meaningful;
a nullable column with a default of `''` or `0` means nobody can tell 'unknown' from 'zero' later."*

### Q15. `DISTINCT` vs `GROUP BY`?

For pure de-duplication they produce the same plan. `GROUP BY` is the right tool when you aggregate; `DISTINCT` when
you don't. But **a `DISTINCT` in a query is usually a bug symptom** — it means a join multiplied your rows, and the
cheap fix (decorating the symptom) hides a fan-out that breaks `SUM` at scale.

> **Say:** "When I see `DISTINCT` in a PR on a reporting query, I ask what duplicated the rows. If it was a one-to-many
> join, the totals are already wrong — `DISTINCT` de-duplicates the rows but doesn't repair the sum."

### Q16. CTE vs temp table vs table variable?

| | CTE | `#temp` | `@tablevar` |
|---|---|---|---|
| Scope | Single statement | Session/batch, real table in tempdb | Batch, like a variable |
| Statistics | Uses base-table stats | **Yes** — real stats | **No** (pre-2019 estimate = 1 row; SQL 2019+ has table variable deferred compilation) |
| Reused in query | Re-executed / inlined | Yes, cheaply | Yes |
| Indexes | No | Yes | Only PK/UNIQUE |
| Good for | Readability, recursion | Big intermediate results, multi-step procs | Small row counts (< ~100) |

**The one-line rule:** *"under a few hundred rows, table variable; anything bigger, `#temp` for the statistics; and a
CTE only when it makes the query readable and is referenced once."*

That last clause is the answer that shows you've been burned by an inlined CTE being executed three times.

### Q17. `MERGE` — should I use it for upserts?

Be careful, and saying so is a strong lead answer.

`MERGE` reads well but has a documented history of bugs and race conditions (the `MERGE` + `HOLDLOCK` requirement;
concurrent inserts can still throw duplicate-key violations because `MERGE` takes the snapshot before locking). It also
concurrency-violates in ways that are hard to test.

**What I actually recommend:**

```sql
-- Portable, predictable upsert pattern with explicit locking
BEGIN TRAN;

UPDATE dbo.Inventory WITH (UPDLOCK, SERIALIZABLE)
SET    Quantity = Quantity + @delta
WHERE  ProductId = @productId;

IF @@ROWCOUNT = 0
    INSERT INTO dbo.Inventory (ProductId, Quantity) VALUES (@productId, @delta);

COMMIT;
```

Or, better, express the intent in one index-friendly statement when the semantics allow:

```sql
MERGE dbo.Inventory AS tgt
USING (SELECT @productId AS ProductId) AS src ON tgt.ProductId = src.ProductId
WHEN MATCHED THEN UPDATE SET Quantity = tgt.Quantity + @delta
WHEN NOT MATCHED THEN INSERT (ProductId, Quantity) VALUES (@productId, @delta);
-- ...but HOLDLOCK and a unique index are mandatory, or you get duplicates under concurrency
```

> **Say:** "I'd rather use an `UPDATE`-then-`INSERT` inside a transaction with `UPDLOCK, SERIALIZABLE`, or better, a
> unique constraint plus a retry — `MERGE` has a history of bugs and the race is subtle enough that I don't want it in
> a payment path."

### Q18. Stored procedure vs ad-hoc SQL vs an ORM — how do you choose as a lead?

| | Strength | Cost |
|---|---|---|
| **EF Core / ORM** | Type safety, migrations, testability, refactor safety | Plan you didn't write; N+1; harder to tune deep queries |
| **Dapper / raw parameterised SQL** | Exact SQL, exact plan, best for hot read paths & reports | Manual mapping, no migration story, string-heavy |
| **Stored procedure** | Encapsulated, permissionable (`GRANT EXECUTE`), one plan shape, DBAs can tune without a deploy | Logic outside the repo, versioned separately, harder to unit test, easy to hide business rules where no reviewer sees them |

**The judgement, stated as a rule:**

> "Default to EF Core because refactor safety and migrations matter more than the last 10% of a query. Drop to
> parameterised SQL for the handful of hot paths and reports where I've measured a plan problem. Use stored procedures
> when the *permissions* boundary demands it (a batch job that must not see PII) rather than because they're faster —
> they're not automatically."

That last clause is the differentiator; "stored procedures are faster" is a myth that a good interviewer will puncture.

---

## 4. Section B — Indexing & execution plans (Q19–Q32)

This is where the round is won. Tier-2 questions live here.

### Q19. Walk me through how you'd read an execution plan.

Have a fixed order and say it out loud — the order *is* the answer:

1. **Read right-to-left** — the rightmost operator is the leaf (data access). That's where the cost usually is.
2. **Find the thickest arrows** — arrow thickness = rows. The biggest row flow is the biggest problem, not the
   highest-percentage operator.
3. **Compare estimated vs actual rows** — hover the operator. A 10× or 100× difference means stale statistics, a bad
   cardinality estimate, or parameter sniffing. **This is the number-one root cause** of "it was fast yesterday".
4. **Look for `Key Lookup` / `RID Lookup`** — a seek plus thousands of lookups is worse than a scan.
5. **Look for `Sort`, `Hash Match` with a yellow bang, `Spill to tempdb`** — memory grants underneath. Yellow warnings
   on the operator = operator-level warnings.
6. **Look for scans on a big table with a selective predicate** — missing/non-SARGable predicate.
7. **Check the predicate placement** — is the filter pushed down (`Seek Predicates`) or applied above (`Predicate`)?
   A predicate in the wrong place means the index isn't being used to filter.
8. **Only then** think about indexes — and go back and re-measure after.

> **Say:** "I read the plan right-to-left, then look for the fattest arrow, then compare estimated against actual rows
> on that operator. Nine times out of ten the answer is a cardinality estimate that's off by an order of magnitude, not
> a missing index."

**Bonus point:** *"I use the actual plan, not the estimated one, but I capture it with `SET STATISTICS IO, TIME ON` so I
have logical reads as a number I can compare before and after."* Logical reads is the metric to quote — it's stable,
while elapsed time varies with cache state.

### Q20. `Seek` vs `Scan` — when is a scan the *right* plan?

A scan is correct when you're reading a large fraction of the table (typically > ~10–20% for a narrow query, lower for a
wide one) or when the index isn't selective enough. The optimiser's choice is cost-based, and it's often right.

> "A scan isn't a bug. A scan on 50 rows is fine and a scan on 50 million rows with a filter returning 3 rows is a bug.
> So I check rows-read against rows-returned — that ratio is the actual diagnosis."

**Say the ratio:** *"10,000 pages read to return 5 rows is a missing or unusable index. 10,000 pages read to return
9,000 rows is a scan doing its job."*

### Q21. Why did my query stop using the index? (SARGability)

The predicate must be **S**earch-**ARG**ument-**able** — the optimiser needs the indexed column bare on one side of a
comparison.

```sql
-- NON-SARGable: function on the column kills the seek
WHERE YEAR(o.CreatedAt) = 2024
WHERE CAST(o.CreatedAt AS DATE) = '2024-06-01'
WHERE o.Status + '' = 'Completed'            -- any expression/concat on the column
WHERE o.Name LIKE '%widget%'                 -- leading wildcard: no seek possible
WHERE o.TotalAmount * 1.18 > 100             -- arithmetic on the column

-- SARGable: range the column, keep it bare
WHERE o.CreatedAt >= '2024-01-01' AND o.CreatedAt < '2025-01-01'
WHERE o.CreatedAt >= '2024-06-01' AND o.CreatedAt < '2024-06-02'
WHERE o.Name LIKE 'widget%'                  -- trailing wildcard can seek
WHERE o.TotalAmount > 100 / 1.18             -- move the maths to the constant
```

**The one to memorise for the EF Core round:** date filtering done in C# rather than as a range predicate.

```csharp
// BAD: EF Core will translate the property access fine, but Year() may not translate → client eval or a scan
var orders = await db.Orders.Where(o => o.CreatedAt.Year == 2024).ToListAsync();

// GOOD: half-open range, always SARGable, always seeks
var from = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
var to   = from.AddYears(1);
var orders = await db.Orders
    .Where(o => o.CreatedAt >= from && o.CreatedAt < to)
    .ToListAsync();
```

> **Say:** "Half-open ranges — `>= from AND < to` — because they're SARGable, they're correct for `DATETIME2` (no
> `.999` rounding trap), and they survive both `DATE` and `DATETIME2` column types."

### Q22. What's the left-prefix rule for composite indexes?

A composite index `(A, B, C)` can be seeked by predicates on `A`, `(A, B)`, or `(A, B, C)` — but **not** on `B` alone.
The B-tree is sorted by `A` first, so `B` values aren't contiguous.

Consequences:

- Put the **equality** column first, then the **range/sort** column. `(Status, CreatedAt)` serves `WHERE Status = @s
  ORDER BY CreatedAt` perfectly. `(CreatedAt, Status)` serves it badly.
- Add `(A, B, C)` and `(A)` separately and `(A)` is redundant — the wider index covers it. Duplicate indexes are pure
  write cost (§B Q26).
- A skipped middle column (`A, C` without `B`) still seeks on `A` but only filters on `C` — seek predicate plus
  residual predicate. Not useless, just not what you wanted.

### Q23. Selectivity and statistics — what do you actually check?

**Selectivity** = how many rows a predicate returns relative to the table. A column with 3 distinct values is a poor
first key; a column with 500,000 distinct values is excellent.

The optimiser decides using **statistics** — a histogram of value distribution plus a density vector. It goes stale as
data changes (by default auto-update kicks in at ~20% change + 500 rows on a large table, so a table that doubles in a
day can be badly estimated).

```sql
-- How stale are the stats, and when did they last update?
SELECT s.name AS Statistic, sp.last_updated, sp.rows, sp.rows_sampled, sp.modification_counter
FROM   sys.stats AS s
CROSS APPLY sys.dm_db_stats_properties(s.object_id, s.stats_id) AS sp
WHERE  s.object_id = OBJECT_ID('dbo.Orders');

UPDATE STATISTICS dbo.Orders;   -- or WITH FULLSCAN for a skewed table
```

**Diagnostic habit:** when estimated vs actual rows differ hugely and the plan is wrong, update statistics *before*
adding an index. I have seen a 40-second query drop to 300 ms from `UPDATE STATISTICS` alone.

> **Say:** "The estimand is statistics, not indexes. If the estimate is 1 row and the actual is 400,000, the optimiser
> is building a nested-loop plan for one row — the fix is statistics or a plan hint, not another index."

### Q24. What is parameter sniffing, and how do you fix it?

The plan is cached on first execution, compiled for *that* parameter value. If the first caller passes a rare value
(selective → seeks + nested loops) and a later caller passes a common value (returns 30% of the table → needs a scan and
a hash join), the cached plan is catastrophic for the second.

Symptoms: *"it's fast in SSMS and slow from the app"* — because SSMS compiles with literals.

Fixes, in the order I'd try them:

| Fix | When | Cost |
|---|---|---|
| `OPTION (RECOMPILE)` on that statement | Statement runs a few times a minute; per-call compile is affordable | Compile CPU per execution |
| `OPTION (OPTIMIZE FOR (@p = UNKNOWN))` | The distribution is fairly uniform | May be mediocre for both tails |
| Split the proc into branches on the parameter shape | Very different predicates (e.g. `@status IS NULL`) | Duplicated SQL, needs a reviewer |
| Local variable copy (`DECLARE @local = @p`) | Quick containment | Same as `OPTIMIZE FOR UNKNOWN` — engine can't sniff a variable |
| Fix the data / add the right index | The estimate is wrong because the index is missing | Real fix, sometimes bigger change |

Then close the loop: *"and I'd add a regression check — the query with both parameter shapes as an integration test
asserting logical reads stay under a budget."*

**Bonus, if they're on SQL Server 2022+ / Azure SQL:** mention **Intelligent Query Processing** (adaptive joins,
memory grant feedback, parameter sensitivity plan optimisation) — it may already handle this, which changes the
recommendation.

### Q25. What's implicit conversion and how does it break an index?

If the types don't match, SQL Server converts — and it converts **the column** (not the parameter) when the column has
lower datatype precedence. A conversion on the column = no seek, plus a warning in the plan
(`CONVERT_IMPLICIT`) and an `CardinalityEstimate` the optimiser gets wrong.

The classic: `VARCHAR` column, `NVARCHAR` parameter.

```csharp
// Column is VARCHAR(50); parameter arrives as NVARCHAR → implicit convert of COLUMN → scan
cmd.Parameters.Add("@code", SqlDbType.NVarChar, 50).Value = code;
// Fix: send it as ANSI when the column is ANSI
cmd.Parameters.Add("@code", SqlDbType.VarChar, 50).Value = code;
```

In **EF Core this mostly doesn't bite** because SQL Server maps `string` → `nvarchar` everywhere and columns are
`nvarchar`. It bites in three places: (a) Dapper/raw SQL against a legacy `varchar` schema, (b) Dapper passing a `string`
where the parameter type isn't declared, (c) mixed `Date`/`DateTime2`/`DateTimeOffset` columns.

> **Say:** "Implicit conversion is the sneakiest one — the query has an index, the plan says index scan, and the answer
> is one `CONVERT_IMPLICIT` warning on the seek predicate. I check the plan's warnings for exactly that."

### Q26. Too many indexes — what's the cost?

Every index is maintained on every `INSERT`, `UPDATE` (if the column is touched) and `DELETE`. Symptoms of index bloat:
slow inserts/bulk loads, `PAGEIOLATCH` waits, a big `UPDATE` that doubles in duration, and a table with more index
pages than data pages.

Find them:

```sql
-- Unused (no seeks/scans/lookups since the last restart) and duplicate indexes
SELECT OBJECT_NAME(i.object_id) AS TableName, i.name, i.type_desc
FROM   sys.indexes i
LEFT   JOIN sys.dm_db_index_usage_stats s
       ON s.object_id = i.object_id AND s.index_id = i.index_id AND s.database_id = DB_ID()
WHERE  OBJECTPROPERTY(i.object_id,'IsUserTable') = 1
  AND  i.type_desc = 'NONCLUSTERED'
  AND  ISNULL(s.user_seeks,0) + ISNULL(s.user_scans,0) + ISNULL(s.user_lookups,0) = 0
ORDER  BY TableName;
```

**Caveat to say out loud:** *"that DMV resets on restart, so I'd only act on indexes with zero usage after a full
month that includes a month-end batch. Dropping an index that fires once a quarter is how you cause a month-end
incident."* That caveat is the lead answer; quoting the DMV alone is not.

### Q27. What's the `Missing Index` DMV and why don't you just apply it?

`sys.dm_db_missing_index_details` suggests indexes based on a single query's needs. Applying them blindly is a classic
junior move:

- The suggestions are **not costed** — the DMV doesn't know about your write volume.
- Suggestions are **per-query**, so 20 queries produce 20 overlapping suggestions, and applying all of them creates
  near-duplicate indexes.
- The `avg_total_user_cost * avg_user_impact` ranking helps prioritise, but it's relative.

> **Say:** "I look at missing-index DMVs as hints about *which queries* are hurting, not as DDL. I merge the overlapping
> ones into a single composite, verify with the actual plan, and measure the write-side cost before I keep it."

### Q28. Fragmentation — when do you rebuild and when do you ignore it?

Page splits cause logical fragmentation. But **`avg_fragmentation_in_percent` alone is a bad trigger**, and the
10%/30% rebuild/reorganize rule of thumb is a heuristic from an era of spinning disks.

Principled version:

1. Check `page_count` first — under ~1,000 pages, fragmentation doesn't matter; the whole thing is in memory.
2. Check `avg_page_space_used_in_percent` — if pages are full, rebuilds split them again.
3. Use `REORGANIZE` for 10–30% on large indexes (online), `REBUILD` above (online if Enterprise/Azure, `ONLINE = ON`).
4. Fix the **cause** — a random clustered key (`NEWID()`, non-sequential `UNIQUEIDENTIFIER`) splits a page on almost
   every insert. Changing to `NEWSEQUENTIALID()` or a sequential id removes the maintenance need.

**Azure SQL note:** nothing to maintain manually — Azure SQL does automatic tuning/index maintenance for you, so
mentioning `AUTO_UPDATE_STATISTICS` and Intelligent Query Processing is the correct answer for a cloud-first role.

### Q29. How do you enforce a query plan doesn't regress during a deploy?

This is a **tier-3** question and most candidates have nothing. Answer:

1. **Query Store** (ON by default on Azure SQL; `ALTER DATABASE … SET QUERY_STORE = ON` on-prem). It keeps plan history
   per query — so after a deploy you can see "this query got a new plan and regressed 40×", and force the good plan
   back (`sp_query_store_force_plan`).
2. **Perf smoke test in the pipeline:** a small harness that runs the top-10 queries against a seeded database and
   asserts logical reads under a threshold. Not a benchmark — a tripwire.
3. **Review the generated SQL** for any query touched by an EF Core change (`ToQueryString()` in a test) so an
   accidental N+1 or a `NOT IN` is caught in code review, not production.

> **Say:** "Query Store gives me plan regression detection and a one-command rollback of a bad plan; the pipeline
> gives me the guardrail. Neither replaces reading the plan."

### Q30. Sorts, memory grants and spills — what do you do about them?

A `SORT` or `HASH MATCH` needs a memory grant estimated at compile time. Underestimate → **spill to tempdb** → the query
gets 10–100× slower and tempdb becomes the bottleneck for everyone.

Fixes: add an index that provides the order (avoids the sort entirely — best), fix the estimate (statistics), or
consider `MIN_GRANT_PERCENT` hints (rarely). The 2019+ **memory grant feedback** feature can self-correct over
executions.

**Say:** *"A sort on 3 million rows is a design signal — the index should deliver the order I'm asking for. Fixing the
index usually deletes the sort and the memory grant problem with it."*

### Q31. What's the difference between logical reads, physical reads and elapsed time?

- **Logical reads** — pages touched from cache. Deterministic, cache-independent → the number to compare before/after.
- **Physical reads** — pages fetched from disk into cache. Noisy, depends on cache state.
- **Elapsed time** — includes network, client, contention, and your machine's mood.

> "I tune against logical reads because it's the only number that doesn't move when someone else refreshes the page."

### Q32. When would you *denormalise* for read performance, and what do you give up?

Legitimate cases: a dashboard that aggregates months of rows per request (pre-aggregate into a summary table refreshed
on a schedule or by the outbox); a list endpoint that needs a joined column it would otherwise N+1; a search page needing
denormalised text.

What you give up: single source of truth — so you need an invalidation strategy, and invalidation is where these break.

**Say:** *"I'd rather materialise a read model than denormalise the write model. A read model can be rebuilt from
events; a denormalised table can't be un-wronged."* (cross-ref `05` §outbox, `09` ADR format).

---

## 5. Section C — Transactions, concurrency & locking (Q33–Q44)

The section where a .NET candidate with production experience visibly outranks a tutorial candidate.

### Q33. The isolation levels — name them with the anomaly each one stops.

| Level | Stops | Still allows | Locks / mechanism |
|---|---|---|---|
| `READ UNCOMMITTED` | — | Dirty reads, non-repeatable, phantoms | No shared locks |
| `READ COMMITTED` (**default**) | Dirty reads | Non-repeatable reads, phantoms | Shared locks released immediately |
| `REPEATABLE READ` | + non-repeatable reads | Phantoms | Shared locks held to end of txn |
| `SERIALIZABLE` | + phantoms | Nothing (correct, but deadlock-prone) | Range locks |
| `SNAPSHOT` | + non-repeatable + phantoms (via row versioning) | Update conflicts (error 3960) | Version store in tempdb, readers don't block |

**The four anomalies, in one line each** — say them fast, in one breath:

- **Dirty read** — you read uncommitted data that later rolls back.
- **Non-repeatable read** — you read the same row twice and it changed.
- **Phantom** — you run the same *predicate* twice and new rows appear.
- **Lost update** — two writers read-modify-write; the first write vanishes (this is the one that loses money).

### Q34. What's RCSI and why does every modern app enable it?

`READ_COMMITTED_SNAPSHOT ON` makes the default `READ COMMITTED` level use row versioning: readers see the last committed
version instead of waiting for writers' locks. **Readers stop blocking writers and writers stop blocking readers.**

```sql
ALTER DATABASE YourDb SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;
```

Cost: the version store in tempdb grows with your update rate, and long-running transactions pin versions (tempdb
bloat). And it changes semantics — a report inside a transaction no longer sees blocking, which can hide a design
problem you previously noticed because it was slow.

> **Say:** "RCSI is the first thing I check in a legacy app with mysterious blocking. It usually removes 80% of the
> `LCK_M_S` waits, at the cost of tempdb version-store pressure — which is worth it, but it isn't free."

**They may ask:** *SNAPSHOT vs RCSI?* SNAPSHOT gives statement-*and transaction*-level consistency (the whole txn sees
one point in time) and requires optimistic conflict handling; RCSI gives statement-level consistency only. SNAPSHOT costs
more version store.

### Q35. Optimistic vs pessimistic concurrency — how do you implement each in EF Core?

**Pessimistic** = lock the row for the duration (`SELECT … WITH (UPDLOCK)`). Safe but scales badly and risks deadlocks.

**Optimistic** = don't lock; detect the conflict at write time. This is the default for web apps, and EF Core supports it
properly:

```csharp
public class Order
{
    public int Id { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();   // SQL Server rowversion
}

// DbContext
modelBuilder.Entity<Order>().Property(o => o.RowVersion).IsRowVersion();

// service
try
{
    order.Status = "Shipped";
    await db.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    // Someone else changed or deleted the row between my read and my write.
    // Do NOT silently retry a state transition — return 409 and let the caller decide.
    throw new ConflictException("This order was modified by someone else. Reload and retry.");
}
```

Mapping to HTTP: **409 Conflict** with the current state, not 500. And the controller-side detail that earns points:
*"I return the fresh entity so the client can show the difference — `UseOptimisticConcurrency` without a friendly
message just produces a user who clicks 'save' again and loses the other person's change."*

**Alternative without a `rowversion` column:** a concurrency token on the fields that matter
(`.HasChangeToken(...)`) — cheaper, but only detects changes to those columns.

### Q36. What causes a deadlock, and what do you actually do?

Two sessions each hold a lock the other needs, and the engine picks a victim (error **1205**).

The real fixes, in order of preference:

1. **Access objects in a consistent order** in every transaction (e.g. always `Orders` then `OrderItems`). This removes
   the circular wait, which is the actual cause.
2. **Shorten the transaction** — no HTTP calls, no `.Result`, no email sending inside a transaction. Long transactions
   hold locks longer, widening the window.
3. **Reduce lock footprint** — a covering index means the update touches fewer rows/pages.
4. **Retry with backoff** on 1205 — necessary for the tail, but not a fix. EF Core's `EnableRetryOnFailure` handles
   transient errors, and you should know it *does* include 1205 in the retryable set.
5. **Use RCSI** so readers don't take shared locks (removes the reader-vs-writer class of deadlock entirely).

**Diagnosis on Azure SQL:** `system_health` extended events session contains `xml_deadlock_report` graphs. In the portal
there's a deadlock graph view. Say *"I pull the deadlock graph from `system_health` and look at the two statements and
the lock types — the graph names the resource and the order."*

> **Say:** "A deadlock is a design problem in the access order, not a random event. If the same two statements deadlock
> twice, I fix the order; retries only cover the ones I can't eliminate."

### Q37. `NOLOCK` — what's the honest answer?

`WITH (NOLOCK)` = `READ UNCOMMITTED`: no shared locks, so no blocking — and also:

- **Dirty reads** of uncommitted rows (a report may include an order that then rolls back).
- **Missing rows** and **duplicate rows** — because pages move during a scan. This is the pathological one: `SUM()` can
  double-count or drop rows, and it's not reproducible.
- Broken `FK`-looking rows: a row read before its parent is committed.

> **Say:** "I wouldn't add `NOLOCK`. It trades correctness for latency, and the failure mode — a report that
> double-counts revenue — is invisible and unreproducible. If I need non-blocking reads I want RCSI or a snapshot
> isolation level, which are *consistent* rather than merely unlocked."

That's a strong, senior, defensible answer and it's the exact thing a junior would get wrong.

### Q38. How long is a transaction allowed to be, and what goes wrong if it's long?

Rules I'd enforce in review:

- **No I/O inside a transaction** — no HTTP, no message publish, no file write. (A message publish inside a DB txn is the
  two-phase-commit trap → use the **outbox** pattern, see `05` §outbox.)
- **No user think-time** — never hold a transaction across a UI round trip.
- **Batch long updates** — `UPDATE … WHERE Id IN (…)` in chunks of 5–10k rather than one 2-hour statement (also keeps
  the log from growing unboundedly and avoids blocking escalation).

Costs of a long transaction: lock footprint and blocking chains, log growth, version-store growth under RCSI, and
tempdb spills. And a rollback of a 2-hour transaction takes as long as the transaction did.

### Q39. What's lock escalation?

SQL Server starts with row/page locks and, at roughly **5,000 locks** on one statement, escalates to a table lock. So a
bulk `UPDATE` of 200,000 rows blocks every reader on the table for its duration — including readers touching *different*
rows.

Mitigations: batch it, add a covering index (fewer locks needed), or on very hot tables use `LOCK_ESCALATION = DISABLE`
(but then you pay in memory for lock structures — usually the wrong trade for OLTP).

### Q40. How do you implement an application-level lock (e.g. "only one worker processes the batch")?

```sql
DECLARE @rc INT;
EXEC @rc = sp_getapplock @Resource = 'nightly-batch', @LockMode = 'Exclusive',
                         @LockOwner = 'Session', @LockTimeout = 0;
IF @rc < 0 RETURN;   -- someone else owns it; exit quietly

-- ... do the work ...

EXEC sp_releaseapplock @Resource = 'nightly-batch', @LockOwner = 'Session';
```

**But the better answer for a lead:** *"I'd rather use a distributed lease — a row with `LockedUntil` claimed with an
atomic `UPDATE … WHERE LockedUntil < SYSUTCDATETIME()`, or Azure Blob lease, or Service Bus with a competing consumer.
`sp_getapplock` is fine within one database, but it doesn't survive a move to multiple databases or a different
engine."* Naming the migration risk is a lead signal.

### Q41. Distributed transactions — `TransactionScope` or something else?

`TransactionScope` with MSDTC works across two SQL Server instances in the same network, and it's the wrong answer for
anything cloud-native (MSDTC is not supported in most PaaS offerings; Azure SQL doesn't support distributed
transactions over MSDTC).

What I do instead, in order of preference:

1. **Keep it in one database** — design boundaries so a transaction is single-DB.
2. **Outbox pattern** — write the domain change and the message in one local transaction; a relay publishes
   at-least-once. Consumers must be idempotent.
3. **Saga with compensation** — a sequence of local transactions with compensating actions for failure (see `05`
   §saga, and `DeliverFoodApp` for the messaging shape).

> **Say:** "Cross-service consistency isn't a transaction problem, it's a workflow problem. I'd use the outbox for
> 'eventually published' and a saga when there's a real multi-step business process with a compensation."

### Q42. What is idempotency and why does a lead insist on it?

Because at-least-once delivery is the only realistic guarantee (retries, outbox relay, service bus redelivery, network
timeouts where you don't know if the write landed). So:

- **Payments/orders:** a client-supplied idempotency key stored in a unique index; duplicate key → return the original
  result rather than creating a second order.
- **Consumers:** record processed message ids, or make the operation naturally idempotent (`SET Status = 'Shipped'
  WHERE Id = @id` is idempotent; `SET Quantity = Quantity - 1` is not).
- **`EnableRetryOnFailure`** on EF Core retries a `SaveChangesAsync` — if the write actually succeeded and the error was
  on the response path, you can double-insert. **This is the trap**: retries are only safe with a unique constraint or an
  idempotency key. Very few candidates know this; it's worth a lot.

### Q43. What's the difference between blocking and deadlocking, and how do you find a blocking chain?

- **Blocking** — normal, expected waiting (one session waits for another to commit). A problem only when the wait is
  long.
- **Deadlock** — a cycle; the engine kills one.

Find the chain:

```sql
SELECT r.session_id, r.blocking_session_id, r.wait_type, r.wait_time, r.status,
       t.text AS sql_text
FROM   sys.dm_exec_requests r
CROSS  APPLY sys.dm_exec_sql_text(r.sql_handle) t
WHERE  r.blocking_session_id <> 0
ORDER  BY r.wait_time DESC;
```

Then walk to the head of the chain (`blocking_session_id = 0`) — that's the session to look at, and it's often an idle
open transaction from a leaked connection in app code.

> **Say:** "The head of the blocking chain is the culprit, and the top of the chain is usually an app bug — a
> `DbContext` held open across an HTTP call, or a forgotten `TransactionScope`."

### Q44. `SaveChanges` in EF Core — is it a transaction, and how do I group several?

Yes — by default each `SaveChanges` is wrapped in a single transaction covering all its statements. To group several:

```csharp
await using var tx = await db.Database.BeginTransactionAsync();
try
{
    db.Orders.Add(order);
    await db.SaveChangesAsync();
    db.OrderItems.AddRange(order.Items);
    await db.SaveChangesAsync();
    await tx.CommitAsync();
}
catch
{
    await tx.RollbackAsync();
    throw;
}
```

Three details that separate a good answer:

1. **With a retrying execution strategy** you cannot begin a transaction directly — the strategy has to replay the whole
   block:
   ```csharp
   var strategy = db.Database.CreateExecutionStrategy();
   await strategy.ExecuteAsync(async () =>
   {
       await using var tx = await db.Database.BeginTransactionAsync();
       /* ...work... */
       await tx.CommitAsync();
   });
   ```
2. Prefer **one `SaveChanges`** (it's already one transaction) over manually grouping two.
3. `SaveChanges` is **not atomic with your message publish** → outbox (§C Q41).

---

## 6. Section D — Schema design & data modelling (Q45–Q54)

Lead-flavoured: these questions have no single right answer, so the *reasoning* is the score.

### Q45. Normalisation — how deep do you go?

- **1NF** — atomic columns, no repeating groups (no `Item1, Item2, Item3` columns; that becomes `OrderItems`).
- **2NF** — no partial dependency on part of a composite key.
- **3NF** — no transitive dependency (`Order` shouldn't store `CustomerCity` when it has `CustomerId`).

**My practical rule:** OLTP schemas to 3NF, then a *deliberate, documented* set of denormalisations for read paths. The
word "documented" is the answer — an undocumented denormalisation is a future data-integrity incident.

**When I deliberately break it:** analytics/read models (pre-aggregated), a column that's expensive to join on every one
of 40 list queries, and a snapshot value that must not change retroactively (`OrderItems.UnitPrice` — must NOT follow the
current `Products.Price`; that's why the price is copied at order time). That last example is my favourite: it shows the
difference between normalisation for integrity and snapshotting for *business* correctness.

### Q46. Natural vs surrogate key — and what do you cluster on?

| | Natural (`Username`, `Code`) | Surrogate (`Id`) |
|---|---|---|
| Meaning | Self-describing in URLs/logs | Meaningless; needs a join to interpret |
| Stability | Changes (people change emails) | Never changes |
| Size | Wide → bloats every non-clustered index | Narrow |
| Uniqueness enforcement | Needs a unique constraint anyway | Identity |

**My default:** surrogate key as `PK` + clustered index, natural key as a `UNIQUE` constraint (which you need regardless).
Then: `int` identity up to ~2B rows, `bigint` beyond, and `UNIQUEIDENTIFIER` **only** when the id must be generated in
the app/offline (and then `NEWSEQUENTIALID()` or `Guid.CreateVersion7()`-style sequential GUIDs, or cluster on a
sequential column and keep the GUID as a *non-clustered* unique key).

**Why it matters:** a random GUID clustered key inserts at a random page → page splits → fragmentation → 90% more I/O on
a big table. That's a real thing I've had to fix.

**Bonus (Microsoft-flavoured, worth mentioning):** "int leaks cardinality and enables enumeration — for public-facing
ids I'd keep an `int` PK internally and expose a non-enumerable id (GUIDv7 or a hash) externally."

### Q47. How do you store money?

`decimal(18,2)` — never `float`/`double` (binary floating point can't represent `0.1`, so `SUM` of money drifts).
Also: pick a scale and hold it (2 for most currencies, but the schema should be explicit — `decimal(19,4)` if you compute
unit prices with tax proration), and store the currency code alongside if multi-currency.

`float` for money is an automatic red flag in review, along with "we round at display time".

**In C#:** `decimal` maps to `decimal(18,2)` by default in EF Core; **be explicit** in the model
(`HasPrecision(18,2)`), because the default changes your rounding behaviour and a silent `decimal(18,2)` truncation on a
4-decimal unit price is a real bug.

### Q48. `DATETIME` vs `DATETIME2` vs `DATETIMEOFFSET` — and how does .NET map them?

| SQL type | Range/precision | .NET | Use |
|---|---|---|---|
| `DATETIME` | 1753→9999, 3.33 ms, **no** semantics | `DateTime` | Legacy only |
| `DATETIME2` | 0001→9999, 100 ns | `DateTime` / `DateTime?` | Default for UTC timestamps |
| `DATETIMEOFFSET` | + offset | `DateTimeOffset` | Anything user-facing across regions, or contractual |
| `DATE` / `TIME` | date only / time only | `DateOnly` / `TimeOnly` (EF Core 6+) | Birthdays, business dates |

**My rule:** store UTC in `DATETIME2` (or `DATETIMEOFFSET` if the offset itself is meaningful) and convert at the edge.
Never store local time. Add `SYSUTCDATETIME()` as a default rather than letting the app decide.

The `.999` trap worth naming: `DATETIME` rounds to `.000/.003/.007` so `WHERE CreatedAt = '2024-06-01 23:59:59.999'`
can miss a row — another reason for half-open ranges (§B Q21).

**In the app:** inject `TimeProvider` (`05A` modelling decisions) so tests can control "now" instead of
`DateTime.UtcNow` — and *do not* call `DateTime.UtcNow` inside a LINQ query translated to SQL; it becomes a parameter
that breaks the plan cache reuse, or worse, a client evaluation.

### Q49. Status as a string, an enum, or a lookup table?

| Option | Pro | Con |
|---|---|---|
| `NVARCHAR(30)` | Readable in SSMS, flexible | Typos become new states; no validation; **my repo's current state** |
| `INT` + C# enum | Compact, fast, compile-time safety in app | Unreadable in raw queries; adding a value needs a deploy (fine) |
| Lookup table + FK | Referential integrity, descriptions, sort order, no deploy to add a state *description* | Extra join |

**My pick:** `INT` with a C# enum + a `CHECK` constraint listing valid values, plus a state-machine validator in the
domain. Lookup table when the business needs to configure the list (and then you need caching). And explicitly: *"a string
status means the database can't enforce the state machine, so an invalid transition is a data corruption bug that only
shows up in a report three months later."*

**If they ask "isn't a CHECK constraint a maintenance burden?"** — yes, adding a value requires a migration; that's the
point: it's a deliberate speed bump on a business rule change.

### Q50. Soft delete or hard delete?

**Soft delete** (`IsDeleted`, `DeletedAt`) keeps referential history and makes "undelete" possible — and then every
query must filter it, which means one forgotten `WHERE IsDeleted = 0` is a data leak. In EF Core, do it with a **global
query filter** so it's impossible to forget, and use `IgnoreQueryFilters()` deliberately:

```csharp
modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
// and make the column indexed, since every query filters on it
```

**Costs to name:** table growth, indexes get bigger, unique constraints get harder (`UNIQUE(Email)` must become
`UNIQUE(Email, IsDeleted)` or a filtered unique index — a genuinely subtle problem), and GDPR right-to-erasure means you
*do* have to hard-delete personal data eventually.

**My default:** soft delete for entities with relationships and audit requirements; hard delete + audit log for
high-volume, low-value rows (logs, sessions, drafts). Say both — that's the answer.

### Q51. How do you do an audit trail?

Three options, in increasing order of cost:

1. **Audit columns** (`CreatedAt`, `CreatedBy`, `ModifiedAt`, `ModifiedBy`) via EF Core `SaveChanges` override — cheap,
   answers "who last touched it", not "what changed".
2. **Shadow audit table** — in the `SaveChanges` override, capture the `ChangeTracker` entries (old/new values) into an
   `AuditLog` table. Full change history in app code.
3. **Temporal tables** (system-versioned) — SQL Server maintains history automatically with `ValidFrom`/`ValidTo`; you
   query `… FOR SYSTEM_TIME AS OF '2024-01-01'`. Excellent for "what did this look like", but the history table grows
   forever and `ALTER TABLE` on a system-versioned table has constraints.

> **Say (the lead bit):** "I'd pick per data class. Order status history is *business* data and deserves a real
> `OrderStatusHistory` table with the reason, not a generic audit table — the audit table isn't queryable in a way
> support can use. Generic CDC is for forensics; business events deserve domain tables."

### Q52. How do you handle multi-tenancy in the schema?

| Model | Isolation | Cost | When |
|---|---|---|---|
| **Shared schema + `TenantId`** | Weakest — one bad query leaks | Cheapest, easiest to operate and report on | SaaS, hundreds of small tenants |
| **Schema per tenant** | Medium | Migration per schema (EF Core migrations don't do this natively — real cost) | Tens of tenants, moderate isolation needs |
| **Database per tenant** | Strongest | Most expensive; connection routing, scale limits | Enterprise tenants, contractual isolation |

If shared-schema, say the enforcement mechanism — that's what they're listening for: a **global query filter on
`TenantId`** plus **row-level security** in the database as defence in depth, plus `TenantId` as the *first* column of
every composite index.

> **Say:** "Shared schema with `TenantId` everywhere, enforced by a global query filter *and* RLS, with `TenantId`
> leading every index. The query filter is the developer experience; RLS is the guarantee for anything that bypasses the
> app."

### Q53. How do you version/deploy a schema change with zero downtime?

The **expand → migrate → contract** pattern. This is a tier-3 question — have the three phases ready:

| Phase | Change | Why it's safe |
|---|---|---|
| **Expand** | Add a nullable `StatusId` column (default backfilled in batches) | Old code ignores it; new code handles both |
| **Migrate** | Dual-write / backfill in batches; verify | Neither old nor new pods break during rollout |
| **Contract** | Drop the old column in a *later* release once nothing reads it | You never roll back a deploy that dropped data |

Concretely, the deployment sequence: apply the migration as a **separate pipeline step before** the app rolls (never
`Database.Migrate()` at startup in a multi-instance app — two pods race the migration), then rolling deploy, then the
contract migration one release later.

Renaming a column is the case people get wrong: it's never rename, it's add-new → dual-write → backfill → switch reads →
drop old. Say that.

### Q54. What's an index strategy for this repo's `Orders` table?

This is a concrete exercise — answer it as if you own it:

| Query | Index | Reasoning |
|---|---|---|
| `WHERE Status = @s ORDER BY CreatedAt DESC` (list page) | `(Status, CreatedAt DESC) INCLUDE (UserId, TotalAmount)` | Equality then range; covering for the list payload |
| `WHERE UserId = @u ORDER BY CreatedAt DESC` ("my orders") | `(UserId, CreatedAt DESC)` | Different leading column — can't reuse the above (left-prefix rule) |
| `JOIN OrderItems ON OrderId` | `(OrderId)` on `OrderItems` (usually covered by the FK index) | FK columns must be indexed or every parent delete scans children |
| `WHERE CreatedAt >= @from` (reporting) | Included in the first index if `Status` is always supplied; otherwise `(CreatedAt)` | Reporting scans are usually too wide to justify a second index on OLTP — offload to a replica (§H) |
| `WHERE TotalAmount > @x` (ad-hoc) | **No index** | Low selectivity, rare — a scan is fine |

> **Say:** "Three indexes on `Orders`, no more. Every one is justified by a specific query I can name, and I'd revisit
> them in a month with the usage DMV — the ones with zero seeks go."

---

## 7. Section E — EF Core ↔ SQL (Q55–Q72)

The section that proves you're not an ORM black-box user. Expect at least three of these in any senior .NET loop.

### Q55. What SQL does this EF Core query generate, and how do I find out?

```csharp
var products = await db.Products
    .Where(p => p.Price > 100)
    .OrderBy(p => p.Name)
    .Take(20)
    .ToListAsync();
```

Before *ever* answering, say the mechanism: **`ToQueryString()`**.

```csharp
Console.WriteLine(db.Products.Where(p => p.Price > 100).OrderBy(p => p.Name).Take(20).ToQueryString());
```

```sql
SELECT TOP(@__p_0) [p].[Id], [p].[Name], [p].[Description], [p].[Price], [p].[CreatedBy], [p].[CreatedAt]
FROM [Products] AS [p]
WHERE [p].[Price] > 100.0
ORDER BY [p].[Name]
```

**Then** narrate what matters about that SQL, because that's the actual answer:

- `TOP(@p)` — the take is parameterised; the plan is reusable.
- The `WHERE` is SARGable *if* there's an index on `Price`.
- The `ORDER BY Name` needs an index on `Name` to avoid a sort, which for 20 rows is fine and for 20,000 rows is not.
- **It selects every column** — so it will never be covered by an index on `(Price, Name)`. If the endpoint only needs
  `Id` and `Name`, project it (§E Q58).

> **Say:** "In development I turn query logging on with `LogTo(Console.WriteLine, LogLevel.Information)` and
> `EnableSensitiveDataLogging()`; on a per-query basis I call `ToQueryString()`. I never guess the SQL."

**Trap:** `ToQueryString()` is not available on some compositions and doesn't include the client-side parts — so it
tells you the *server* query, which is exactly what you want for a plan problem.

### Q56. What's N+1 and how do you find it in EF Core?

One query for the parents, then one per parent for the children. Symptom: an endpoint whose SQL count scales with the
page size (20 orders → 21 queries).

Cause in EF Core: iterating a navigation property that wasn't eager-loaded, with **lazy loading** enabled (or a
serialisation walk over entities).

```csharp
// N+1: 1 query for orders, then one per order for Items
var orders = await db.Orders.Take(50).ToListAsync();
foreach (var o in orders) Console.WriteLine(o.Items.Count);

// Fixed: one query, JOIN
var orders = await db.Orders.Include(o => o.Items).Take(50).ToListAsync();

// Better for a read path: project — one query, only the columns needed, no tracking
var rows = await db.Orders.Take(50)
    .Select(o => new OrderSummaryDto(o.Id, o.Status, o.Items.Count))
    .ToListAsync();
```

**How I detect it:** count `DbCommand` executions per request in tests (an interceptor counting commands and asserting
`< 5` for the list endpoint is the cleanest guard), or read the log in dev. On a deployed system, the symptom is query
volume — thousands of near-identical statements — which is obvious in Query Store.

> **Say:** "The fix isn't always `Include` — `Include` on a list endpoint with ten children per row gets you a cartesian
> explosion and a 30,000-row transfer. For read paths I project; `Include` is for when I actually need the entity graph
> for a write."

### Q57. `Include` + collection = cartesian explosion. What do you do?

`Orders.Include(Items).Include(Payments)` produces `orders × items × payments` rows — 50 orders with 10 items and 3
payments = 1,500 rows with duplicated order data. For deep graphs this is catastrophic.

Two fixes:

```csharp
// 1. Split queries: one query per collection. Extra round trip, no multiplication.
var orders = await db.Orders
    .Include(o => o.Items)
    .Include(o => o.Payments)
    .AsSplitQuery()
    .ToListAsync();

// 2. Projection (preferred on read paths): no tracking, no multiplication
var orders = await db.Orders.Select(o => new {
    o.Id, o.Status,
    Items    = o.Items.Select(i => new { i.ProductId, i.Quantity }),
    Payments = o.Payments.Select(p => new { p.Amount })
}).ToListAsync();
```

**Trade-offs to state:** split queries are **not** transactionally consistent by default across round trips (wrap in a
transaction if it matters), and `AsSplitQuery` can be slower for small graphs because of the extra round trips.

### Q58. What's the single biggest EF Core performance win, and why?

**Projection to a DTO.** Not `AsNoTracking` (second), not indexes.

Why: (1) fewer columns over the wire, (2) no change-tracker entries (a 5,000-entity result means 5,000 tracked objects
and a `DetectChanges` pass), (3) the narrow result *can* be served by a covering index, (4) no accidental lazy-loading
walk during serialisation.

```csharp
// 5,000 entities tracked, all columns, then mapped in memory
var products = await db.Products.Where(...).ToListAsync();
return products.Select(p => new ProductDto(p.Id, p.Name, p.Price));

// One query, four columns, no tracking, no post-processing
return await db.Products.AsNoTracking()
    .Where(...)
    .Select(p => new ProductDto(p.Id, p.Name, p.Price))
    .ToListAsync();
```

Second biggest: **`AsNoTracking()`** for anything read-only. But name the caveat — with `AsNoTracking` you lose identity
resolution, so the same row appearing twice in a result set becomes two objects (use
`AsNoTrackingWithIdentityResolution()` when the graph shape matters).

### Q59. When does EF Core evaluate on the client, and why is it dangerous?

EF Core 3.0+ **throws** `InvalidOperationException` for a predicate that can't be translated (it no longer silently
pulls the table into memory). Client evaluation is permitted only in the **final projection**.

Why it matters: `Where(o => SomeHelper(o.Status))` can't translate — before v3 it fetched the whole table and filtered in
memory (works on 100 rows, disintegrates on 10 million).

What you do: rewrite the predicate so it translates, or evaluate in SQL with `EF.Functions`:

```csharp
.Where(o => EF.Functions.Like(o.Status, "Comp%"))
.Where(o => EF.Functions.DateDiffDay(o.CreatedAt, DateTime.UtcNow) <= 30)  // works, but not SARGable — prefer ranges
```

`EF.Functions.DateDiffDay` is a good example of "translates but still bad for the index" — a function on the column.

### Q60. `FromSqlRaw` vs `FromSqlInterpolated` — and where's the injection risk?

```csharp
// DANGEROUS: concatenation → injectable
var sql = "SELECT * FROM Products WHERE Name = '" + name + "'";
db.Products.FromSqlRaw(sql);

// SAFE: interpolated form sends parameters, not text
db.Products.FromSqlInterpolated($"SELECT * FROM Products WHERE Name = {name}");

// ALSO SAFE: raw SQL with explicit parameters (EF Core 3+ overload)
db.Products.FromSqlRaw("SELECT * FROM Products WHERE Name = {0}", name);
```

The rule: **`FromSqlRaw` with `+` is a bug in review**. `FromSqlInterpolated` (or `FromSql`, which took over the name in
EF Core 8) is safe because the interpolated string becomes parameters.

Two more traps worth naming:

- `FromSql` results must be composable-friendly — you can't `Include` over arbitrary SQL, and `.Where()` over a raw
  query is appended, which can surprise you.
- **Dynamic SQL fragments** (sort column, table name) can't be parameterised at all → allow-list (§G Q82).

### Q61. How do you run a bulk update or delete without loading entities?

EF Core 7+:

```csharp
// One statement, no entities loaded, no change tracker, no concurrency tokens
await db.Orders
    .Where(o => o.Status == "Pending" && o.CreatedAt < cutoff)
    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, "Expired"));

await db.Products.Where(p => p.IsDeleted).ExecuteDeleteAsync();
```

**The caveats a lead must state** (this is what separates the answer from a doc quote):

1. It **bypasses the change tracker** — already-tracked entities in the same context are now stale.
2. It **does not fire** concurrency tokens, shadow-property updates you have in `SaveChanges`, or EF Core interceptors
   that rely on `SaveChanges`.
3. It **does not cascade** in EF's sense — database-level `ON DELETE CASCADE` still applies (which may delete more than
   you expect).
4. It's **not transactional with a following `SaveChanges`** unless you wrap both.

So: use it for maintenance/backfill/batch, not for business transitions that carry domain events.

### Q62. `Context` lifetime and connection pooling — how does it actually work?

- `AddDbContext` registers the context as **Scoped** → one per HTTP request. The context is **not thread-safe** —
  `Task.WhenAll` over the same context throws or corrupts state.
- The context opens a connection lazily and returns it to the **pool** on dispose — it does **not** close the physical
  connection. Pool default max is **100**; the "connection pool exhausted" exception means 100 concurrent leases and
  someone is holding one too long.
- `AddDbContextPool` reuses context *instances* to avoid allocation churn on very high-throughput APIs. Sounds free, is
  not: your context must not carry per-request state in fields, because it's recycled.
- Background jobs / Blazor: don't inject a scoped context into a singleton — use `IDbContextFactory<T>` and create a
  context per unit of work.

> **Say:** "A leaked connection is almost never a not-disposed `DbContext` — it's a long-running reader or an await
> inside a transaction. I'd look at what's holding the lease, not at the pool size; raising `Max Pool Size` converts one
> visible failure into a database-wide thread-pool starvation."

### Q63. `EnableRetryOnFailure` — what does it retry, and what does it not protect you from?

```csharp
options.UseSqlServer(cs, sql => sql.EnableRetryOnFailure(
    maxRetryCount: 3,
    maxRetryDelay: TimeSpan.FromSeconds(10),
    errorNumbersToAdd: null));   // default list includes 1205 (deadlock)
```

It retries **transient** SQL errors: connection failures, deadlocks (1205), Azure SQL throttling (40613/40501),
"database is busy". It retries **the whole `SaveChangesAsync`**.

What it does **not** do:

- It doesn't make non-idempotent writes safe — if the write committed but the response was lost, you may double-write.
  Guard with a unique constraint or idempotency key (§C Q42).
- It doesn't cover user-initiated transactions unless you wrap them in
  `db.Database.CreateExecutionStrategy().ExecuteAsync(...)` (§C Q44).
- It doesn't cover timeouts caused by a bad plan — retrying a 30-second query five times is five 30-second failures.

### Q64. How do you see and correlate the SQL EF Core issues from a specific request?

```csharp
// 1) Tag the query — appears as a comment in the SQL and in Query Store / profiler
await db.Orders.TagWith("OrdersListEndpoint").Where(...).ToListAsync();

// 2) Global interception + logging (dev)
options.AddInterceptors(new CommandLoggingInterceptor(logger));

// 3) Correlation: push the trace id into the query text
public class TraceTagInterceptor(ITraceContext trace) : DbCommandInterceptor
{
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData,
        InterceptionResult<DbDataReader> result, CancellationToken ct = default)
    {
        command.CommandText = $"/* trace={trace.TraceId} */ {command.CommandText}";
        return base.ReaderExecutingAsync(command, eventData, result, ct);
    }
}
```

> **Say:** "`TagWith` is the cheapest observability investment on the data layer — when a P1 fires I can find the exact
> query in Query Store by tag instead of guessing from the SQL text. The APM trace id in the same comment closes the loop
> between the slow request and the slow query."

That's a `05` observability + `10` incident-response answer wearing a SQL hat. Very strong.

### Q65. EF Core or Dapper — how do you decide *per query*?

| Use | Tool |
|---|---|
| CRUD, domain writes, anything needing change tracking + concurrency + migrations | EF Core |
| Reports/aggregates over many tables, window functions, CTEs | Dapper / raw SQL |
| Hot read path where you've measured the plan and need control | Dapper / raw SQL + covering index |
| Bulk insert of 500k rows | `SqlBulkCopy` (or a table type + `MERGE`), not EF |
| Simple, non-trivial joins that EF translates badly | `FromSql` + a projection |

**The lead framing:** *"I default to EF Core and treat raw SQL as an exception with a reason. Both in the same project is
normal; two different data-access idioms with no stated boundary is not — the boundary is 'reads we've measured' versus
'the domain model'."*

**Don't** say "Dapper is faster". Say "Dapper gives me the exact SQL and no tracking overhead; whether that's faster is
something I measure."

### Q66. How do you ship a schema change to production with EF Core?

The process I'd enforce (cross-ref §D Q53 and `10`):

| Step | Command / action |
|---|---|
| Generate | `dotnet ef migrations add AddOrderStatusIndex` (review the generated `Up`/`Down` — never ship unreviewed) |
| Validate | Run against a restored copy of production-shaped data, not an empty DB |
| Script | `dotnet ef migrations script --idempotent -o migrate.sql` (or `dotnet ef migrations bundle`) |
| Apply | A **pipeline step**, using a privileged DB identity, *before* the app deploy; log the applied migration list |
| Verify | Post-deploy smoke query; a health check that asserts the schema version |
| Contract | Drop the now-unused column in a **later** release |

**Three rules I'd say out loud:**

1. **Never `Database.Migrate()` on startup in a multi-instance deployment.** Two pods start, both try, one wins and one
   throws (or worse, partially applies). The repo's `DeliverFoodApp` does exactly this — flagging it as something you'd
   change is a free credibility point.
2. **The migration identity ≠ the app identity.** The app runs as `db_datareader/db_datawriter`; DDL uses a separate
   login used only by the pipeline. Least privilege (§G Q83).
3. **`--idempotent`** so a partially-failed run can be re-run safely.

### Q67. What about `EnsureCreated()`?

Dev/demo only. `EnsureCreated` creates the schema from the model **without** a migrations history table, so you can never
add migrations afterwards without dropping the database, and it can't apply incremental changes. If I see it outside
`if (env.IsDevelopment())` it's a finding.

### Q68. How do you test data access?

Three layers, and being specific is the point:

1. **Unit/domain tests** — no database; validation and state-machine logic behind an interface in memory.
2. **Integration tests against a real database** — this is the one juniors skip and leads insist on. Use
   **Testcontainers** (Real SQL Server / mssql server container) or SQL Server LocalDB/Container. `UseInMemoryDatabase`
   is *not* a substitute: no constraints, no SQL translation, different semantics — it will pass a query that throws in
   production.
3. **Plan/read tests** — a seeded DB + assert logical reads or command count for the top queries (§B Q29, §E Q56).

**Say:** *"`UseInMemoryDatabase` gives false confidence — it can't fail a NOT NULL, a unique constraint, or a
translation error. For anything touching SQL I want a real engine in the test."*

### Q69. How do you make EF Core queries fast *at the extremes* — 10k inserts, 1M row export?

| Scenario | Approach |
|---|---|
| 10k inserts | `AddRange` + one `SaveChanges` (EF batches into round trips), `AutoDetectChangesEnabled = false` for bulk, or `SqlBulkCopy` for 100k+ |
| 100k+ inserts | `SqlBulkCopy` (or `ExecuteUpdate`-style staging table + `MERGE`) — EF's per-entity tracking is the bottleneck |
| 1M row export | Stream with `AsAsyncEnumerable()` + `AsNoTracking`, never `ToList()` |
| 500k row update | `ExecuteUpdateAsync` in batches, or staging table + set-based `UPDATE … FROM` |

```csharp
await foreach (var p in db.Products.AsNoTracking().AsAsyncEnumerable())
    await writer.WriteLineAsync(JsonSerializer.Serialize(p));
```

**Name the memory risk:** `ToList()` on a million rows is an `OutOfMemoryException` waiting for a bigger VM, and it's a
classic "it works in test, dies in production" bug.

### Q70. What do compiled queries buy you?

```csharp
private static readonly Func<AppDbContext, decimal, IAsyncEnumerable<Product>> Expensive =
    EF.CompileAsyncQuery((AppDbContext db, decimal minPrice) =>
        db.Products.Where(p => p.Price > minPrice));
```

It skips the LINQ expression → SQL translation (the *query compilation*), not the SQL Server plan. Gains matter only in
tight loops — microseconds per call. So: *"worth it on a genuinely hot path, and I'd only reach for it after measuring;
otherwise it's premature ceremony."*

### Q71. How do you handle soft-deleted rows, tenant filters and audit columns in one place?

Global query filters + a `SaveChanges` override. This is the "can you own a data layer" question:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    foreach (var entry in ChangeTracker.Entries<IAuditable>())
    {
        if (entry.State == EntityState.Added)
            entry.Entity.CreatedAt = _clock.GetUtcNow();
        if (entry.State == EntityState.Modified)
            entry.Entity.ModifiedAt = _clock.GetUtcNow();
    }
    return await base.SaveChangesAsync(ct);
}
```

Caveats: global filters apply to *every* query including `FromSql` composition, they don't apply to
`IgnoreQueryFilters()` (obviously), and **they don't apply to `ExecuteUpdate`/`ExecuteDelete`** — an important asymmetry
worth naming.

### Q72. What did you learn from a hard data problem?

Have this ready as a 90-second story (§8 Q80 is the incident template). A real one beats a hypothetical, and the strongest
shape is: *symptom → what the plan/DMVs said → the fix → the guardrail you added so it can't recur → what it cost.* End on
the guardrail; that's the lead signal.

---

## 8. Section F — The troubleshooting playbook (Q73–Q80)

Expect at least one "this got slow, go" scenario. **The playbook is the answer**, and it must come out in order.

### The eight steps (memorise the order)

1. **Quantify** — which endpoint, p50 or p95, since when, how many users. ("It's slow" is not a bug report.)
2. **Reproduce or measure** — get a real number on a production-shaped dataset. Dev datasets don't reproduce plans.
3. **Locate** — application or database? Check the APM span durations: if SQL time is 4 s of a 4.2 s request, stop
   looking at code.
4. **Capture the actual plan** for that query with its real parameters, plus `SET STATISTICS IO, TIME ON`.
5. **Diagnose** — fattest arrow, estimated-vs-actual rows, key lookups, scans with a selective predicate, spills,
   `CONVERT_IMPLICIT`, trivially-non-SARGable predicate.
6. **Fix the smallest thing** — index, statistics, predicate rewrite, projection, pagination. One change at a time, so you
   know what worked.
7. **Verify with numbers** — logical reads before/after, p95 before/after. Not "feels faster".
8. **Guard it** — Query Store validation, a plan-regression test, or a perf smoke test in the pipeline + an alert on the
   metric (§10 alerting). Without step 8 it comes back.

> **Say:** "The reason I'm strict about the order is that steps 1–3 prevent the most common mistake, which is optimising
> a query that isn't the problem. I've spent an afternoon tuning a scan that turned out to be 40 ms inside a 6-second
> request — the time was DNS in the connection string."

### Q73. "The list endpoint is slow." First five questions?

1. **p50 or p95, and is it slow for everyone or one tenant?** (One tenant = data volume/distribution, not a missing
   index.)
2. **What changed?** Deploy, data growth, an index dropped, a new client hitting a different sort order.
3. **Is the time in SQL or in the app?** APM span durations answer this in 30 seconds.
4. **How many queries per request?** N+1 is the most common application-level cause (§E Q56).
5. **What's the page size and sort?** Deep `OFFSET` plus a non-indexed sort is the most common SQL-level cause.

### Q74. How do you find the query that's actually hurting, database-wide?

```sql
-- Top consumers by total CPU — "total" is what matters for load, "average" misleads
SELECT TOP 20
       qs.execution_count,
       qs.total_worker_time / 1000 AS TotalCpuMs,
       qs.total_worker_time / NULLIF(qs.execution_count,0) / 1000 AS AvgCpuMs,
       qs.total_logical_reads / NULLIF(qs.execution_count,0) AS AvgLogicalReads,
       SUBSTRING(st.text, qs.statement_start_offset/2 + 1, 200) AS StatementText
FROM   sys.dm_exec_query_stats qs
CROSS  APPLY sys.dm_exec_sql_text(qs.sql_handle) st
ORDER  BY qs.total_worker_time DESC;
```

**The framing that earns points:** *"A query that runs 500,000 times at 5 ms each is a bigger problem than one that runs
five times at 2 s — but the slow one gets all the attention. I rank by total CPU and total logical reads, not by the
worst offender someone complained about."*

On Azure SQL: **Query Store** (`sys.query_store_runtime_stats`) or Query Performance Insight in the portal; both keep
history across restarts, which the DMVs don't.

### Q75. The query is fast in SSMS and slow from the app. Why?

The classic five, in order of likelihood:

1. **Parameter sniffing** (§B Q24) — SSMS compiled with literals.
2. **Implicit conversion** (§B Q25) — different parameter types from `SqlParameter`/Dapper.
3. **`ARITHABORT`/SET options mismatch** between SSMS and the client (plan cache key includes SET options — different
   cached plan).
4. **Different connection options** — `Application Name`, `MultipleActiveResultSets`, transaction state.
5. **The app is the slow part** — tracking, serialisation, or the 20 extra queries around it.

> **Say:** "Fast in SSMS and slow in the app is a signal, not a mystery — it means the plan is being compiled
> differently or the plan is right and the measurement is wrong. Either way I get the plan from the app's own session."

### Q76. How do you diagnose a sudden spike in CPU on the database?

1. **Is it us?** Top queries by CPU (§F Q74) — a plan change, a new report, a missing index.
2. **Is it volume?** Check `batch requests/sec` — a retry storm or a runaway client loop looks exactly like a slow
   database.
3. **Is it waits?** `sys.dm_os_wait_stats` — `CXPACKET/CXCONSUMER` (parallelism, often mis-set MAXDOP/cost threshold),
   `PAGEIOLATCH` (I/O), `LCK_M_*` (blocking), `RESOURCE_SEMAPHORE` (memory grants), `THREADPOOL` (starvation →
   usually too many connections).
4. **Is it the server?** DTU/vCore consumption vs tier limit on Azure SQL — throttling looks like slowness with
   `40613`/`40501` errors.

**Lead add-on:** *"and I'd check whether the fix is a bigger tier. On Azure SQL the cheapest fix is sometimes one tier
up rather than a week of tuning — that's a cost decision, not a heroics decision."*

### Q77. What's the difference between plan cache bloat and plan cache miss?

- **Bloat** — thousands of one-off ad-hoc statements each with a plan, filling the cache and evicting useful plans.
  Symptom: high `SQL Plans` memory, low reuse. Fix: **`optimize for ad hoc workloads`** (server setting — caches a stub
  first), plus parameterising queries.
- **Miss** — plans not reused at all: unparameterised literals, different SET options, `OPTION (RECOMPILE)`,
  `WITH RECOMPILE`, or forced recreation. Symptom: high compiles/sec.

Both are found in `sys.dm_exec_cached_plans` + `sys.dm_exec_query_stats`, and both are worth one sentence each in an
interview.

### Q78. How do you handle a timeout — what did you change?

The wrong answer is "increased the timeout". The right answer is a sequence:

1. **Find the query** that timed out and get its plan (a timeout is a symptom; the DB usually has the evidence).
2. **Decide if the query should take that long at all** — if it's a report over 12 months of orders, the fix is
   architecture (offload to a replica / pre-aggregate), not tuning.
3. **Then** tune (index, predicate, projection, pagination).
4. **Then** set a *deliberate* command timeout per query class: 5 s for user-facing reads, 120 s for batch. A global
   30 s default that's never revisited is a smell.
5. **Add the guardrail** — an alert on slow-query count, not on timeouts (timeouts are the late signal).

**Say:** *"Increasing a timeout is how you convert a fast failure into a slow failure and fill the connection pool while
you do it."*

### Q79. Read the plan: what would you fix here?

```sql
SELECT o.Id, o.TotalAmount, u.Username
FROM   dbo.Orders o
JOIN   dbo.Users u ON u.Id = o.UserId
WHERE  o.Status = 'Completed' AND YEAR(o.CreatedAt) = 2024
ORDER BY o.CreatedAt DESC;
```

Answer in the eight-step shape, compressed:

- **Two scans** (Orders, Users) and a `Sort` with a memory grant.
- `YEAR(o.CreatedAt)` is **non-SARGable** → the predicate is applied after the scan → the whole table is read to return
  one year of rows.
- **Fix 1:** rewrite the predicate as a half-open range → seek.
- **Fix 2:** `(Status, CreatedAt DESC) INCLUDE (UserId, TotalAmount)` → seek, ordered, covering; the sort disappears.
- **Fix 3:** project only the needed columns (already narrow here).
- **Verify:** logical reads and the plan shape (no `Sort`, no scan).
- **Cost:** one more index maintained per insert/update on `Orders`.

That's a complete tier-2 answer in under two minutes, and it uses every beat from §1.

### Q80. Tell me about a production data incident you owned.

Have a STAR with these specific beats — the *diagnosis* is the story, not the fix:

- **Symptom with numbers:** "checkout p95 went 400 ms → 12 s, 8% of attempts timing out, started 14:20."
- **Triage order:** dashboard → APM says 11.6 s in SQL → the exact query → its plan → **plan changed that morning**.
- **Root cause, named precisely:** statistics went stale after a nightly backfill crossed a row-count threshold, so the
  estimate for the join dropped to 1 row and the optimiser chose nested loops over 400k rows.
- **Mitigation vs fix:** immediate = `UPDATE STATISTICS` (+ Query Store plan force); permanent = scheduled stats
  maintenance, index on the join column, and an alert on p95 checkout.
- **What you'd do differently:** "I'd have caught it in the pipeline — the plan regression would have shown in a Query
  Store review if we'd looked at it after the backfill."

The "what I'd do differently" beat is what makes it a lead story (cross-ref `12` §behavioural stories).

---

## 9. Section G — Security (Q81–Q86)

Short section, high signal. A lead is expected to have opinions here.

### Q81. SQL injection — how does it actually happen in a .NET codebase?

Three ways, and naming all three is the answer:

1. **String concatenation** into `SqlCommand.CommandText` or `FromSqlRaw`.
   ```csharp
   // Injectable
   cmd.CommandText = "SELECT * FROM Users WHERE Username = '" + username + "'";
   // Parameterised
   cmd.CommandText = "SELECT * FROM Users WHERE Username = @u";
   cmd.Parameters.Add("@u", SqlDbType.NVarChar, 100).Value = username;
   ```
2. **Dynamic identifiers** — column/table names and `ORDER BY` can't be parameterised:
   ```csharp
   // Injectable: sort column from the query string
   var sql = $"SELECT * FROM Products ORDER BY {sortColumn}";
   // Fix: allow-list
   var order = sortColumn switch {
       "name"  => "Name",
       "price" => "Price",
       _       => "Id"
   };
   ```
3. **Dynamic SQL inside stored procedures** (`EXEC(@sql)` with concatenation) — the app is safe and the proc isn't.
   Fix: `sp_executesql` with parameters.

**Then the leadership part:** *"I don't rely on review to catch this — I'd wire a lint/SAST rule (banned-API analyzer
on `FromSqlRaw`, `SqlCommand` with `+`) and an integration test that posts `' OR 1=1--` to every string filter."*

### Q82. Parameterisation — is EF Core automatically safe?

Mostly yes, because LINQ parameters become `SqlParameter`s. The gaps:

- `FromSqlRaw` with concatenation or `string.Format`.
- Dynamic `ORDER BY`/`SELECT`/table names (never parameterisable — allow-list).
- `ExecuteSqlRaw` with interpolation.
- `Contains` over a large `List<int>` — EF Core parameterises (or, in EF Core 8+, uses `OPENJSON` for primitive
  collections), so it's safe; but hand-built `IN ('a','b',...)` strings are not.

> **Say:** "EF Core is safe by default and unsafe the moment someone writes SQL by hand. So my rule is: raw SQL goes
> through `FromSqlInterpolated`/`FromSql` with parameters, identifiers come from an allow-list, and anything else is a
> review blocker."

### Q83. Least privilege — what identity does the app use?

Three identities, and being able to name them is the answer:

| Identity | Used by | Rights |
|---|---|---|
| **App identity** | The API at runtime | `db_datareader`, `db_datawriter`, `EXECUTE` on granted procs. **Not** `db_owner`, no DDL |
| **Migration identity** | The deployment pipeline | `db_ddladmin` + `db_datawriter` on the target DB |
| **Human/DBA** | Break-glass | Just-in-time, Entra ID, audited |

And on Azure: **managed identity** instead of a SQL password —
`Authentication=Active Directory Managed Identity` in the connection string, with the identity granted in the database
(`CREATE USER [app-mi] FROM EXTERNAL PROVIDER`). That removes the credential from config entirely, which means it can't
leak into a repo or a log.

**Say the risk:** *"`db_owner` for the app is the most common audit finding I see — an injection or an SSRF then becomes
'create a login, grant sysadmin', and the blast radius is the whole server. Least privilege is the control that limits
the damage after the other controls fail."*

### Q84. Row-level security, masking, encryption — what's the difference and when?

| Control | Protects against | Cost |
|---|---|---|
| **Row-level security (RLS)** | A user seeing *other tenants'/regions' rows* | Every query gets a predicate function; needs `SESSION_CONTEXT` set per connection; can hurt plans |
| **Dynamic data masking** | Casual viewing of PII in an admin tool | Masking only — no cryptographic protection; a user with `UNMASK` or clever predicates can still infer |
| **Always Encrypted** | DBAs, backups, stolen MDF | Client-side encryption; can't do range queries/`LIKE` on encrypted columns; key management |
| **TDE** | Stolen disk/backup | Transparent, negligible app change, does **not** protect against a DB connection |

**The judgement:** pick by threat. TDE is table stakes (and on by default in Azure SQL). RLS for tenant isolation as
defence-in-depth behind the app filter. Always Encrypted only for the specific columns where a DBA must not see the value
(card numbers, health data) — because it breaks your indexes and range queries.

### Q85. How do you handle PII retention and deletion requests?

- **Classify** columns (PII: name, email, phone, address; sensitive: card, health).
- **Pseudonymise** where the business doesn't need the raw value (hash for lookups instead of storing an email you never
  read).
- **Retention job** — an `ExecuteDelete` on a schedule, or archive-to-cold then delete.
- **Right to erasure** — soft delete is *not* erasure. You need a real hard-delete path that also covers backups (a
  documented retention window is the honest answer for backups).
- **Audit** who read sensitive data (a `SensitiveDataAccess` log), not just who wrote it.

That's a five-part answer most candidates can't produce; it's a good place to spend 90 seconds.

### Q86. Connection strings and secrets — where do they live?

Not in `appsettings.json` committed to the repo. Order of preference:

1. **Managed identity / Entra ID auth** — no secret at all (best).
2. **Key Vault + `DefaultAzureCredential`** (or App Configuration with Key Vault references).
3. **Environment variable injected by the platform** (Container Apps secret / App Service setting / pipeline variable
   group).
4. Never: a literal in `appsettings.json`, a connection string in a Dockerfile, or in `azure-pipelines.yml`.

**Also:** a connection string must not appear in logs — EF Core's `EnableSensitiveDataLogging` redacts by default, but
your own exception logging may not. Check that your exception handler doesn't serialise the `Options` object.

---

## 10. Section H — Scaling, HA/DR and lead-level data decisions (Q87–Q98)

Tier-3 questions. These are won by naming the trade-off and the trigger, not the technology.

### Q87. The database is the bottleneck. What's your escalation ladder?

In order, and say that it's in order because each step is cheaper than the next:

1. **Fix the query** — index, predicate, projection, N+1, pagination (§B, §E, §F). Free, and usually enough.
2. **Cache** — output/memory cache for read-heavy data with an explicit invalidation story (§H Q90).
3. **Read replica / readable secondary** — offload reports and exports.
4. **Scale up** — more vCores/RAM. Costs money, buys time, no code change.
5. **Partition / archive** — move old data out of the hot table; queries touch less data.
6. **CQRS + separate read store** — a materialised read model for query-heavy paths.
7. **Shard / split by domain** — the last resort, because it moves complexity into the application permanently.

> **Say:** "Most teams jump to step 4 or 7 because it's the most exciting. The last three database incidents I've dealt
> with were all solvable at step 1 — one index, one N+1, one missing projection."

### Q88. When is a read replica the right answer, and what's the catch?

**Right when:** reporting/BI, exports, search, or any workload that scans a lot and can tolerate being seconds behind.

**The catches, all three:**

- **Replication lag** — a "read your own write" screen will show stale data. Fix: route reads to the primary for N
  seconds after a write (session stickiness), or read the write's result from the response, not a follow-up query.
- **Cost and shared load** — the replica still costs money, and on Azure SQL it's only available on certain tiers
  (Business Critical / Hyperscale readable secondaries).
- **Failover semantics** — a failover group gives you a readable secondary in another region, but it's not a free
  horizontal scale-out; writes still go to one primary.

### Q89. Horizontal vs vertical scaling for the database — what do you tell the business?

| | Vertical (bigger) | Horizontal (shard/split) |
|---|---|---|
| Code change | None | Permanent complexity: routing, distributed queries, cross-shard transactions |
| Ceiling | Real (hardware/tier limit) | Effectively unbounded |
| Failure modes | None new | Cross-shard consistency, rebalancing, hotspots |
| Time to relief | Hours | Months |
| Cost | Linear | Linear + engineering |

**The line to use:** *"Vertical scaling is a decision you can make on a Friday. Horizontal scaling is an architecture
programme. I'd exhaust vertical and partitioning first, and I'd only shard with a measured ceiling and a defined shard
key."*

### Q90. Caching — where, and how do you keep it correct?

| Layer | Tool | Invalidation |
|---|---|---|
| Per-request / per-URL | `IMemoryCache` / output cache | TTL seconds, small |
| Cross-instance | Redis (`IDistributedCache`) | Explicit eviction on write + short TTL |
| Read model | Materialised table / search index | Rebuilt from events or on a schedule |
| Response (HTTP) | `Cache-Control`/ETag | Client + CDN |

**The lead content is the invalidation table, not the cache.** Three patterns:

- **TTL-only** — simplest, bounded staleness, acceptable for reference data (product categories).
- **Write-through evict** — on write, delete the key (delete, don't set — a concurrent read can write a stale value back
  with a "set"). Say *"I delete rather than set, and accept a brief miss."*
- **Versioned keys** — include a version/timestamp in the key so a redeploy invalidates everything implicitly.

**And the honest risk:** *"caching is a correctness decision with a performance benefit, not the other way round. The
stale-data bug is always more expensive than the load was."*

### Q91. Partitioning — when does it help?

**It helps:** with sliding-window retention (`DELETE` becomes "switch out a partition" — instant, no log growth),
and with very large tables where the planner can eliminate partitions on an equality predicate.

**It doesn't help:** by itself making queries faster. A query without a partition-key predicate touches every partition. It
also requires the partition key in the clustered index and a partition function/scheme, which is real operational
overhead, and it's an Enterprise/`Azure SQL` tier feature in some configurations.

> **Say:** "Partitioning is a manageability feature — retention and maintenance windows — before it's a performance
> feature. If I need it for speed, the query's probably touching more data than it should."

### Q92. Columnstore — when?

For analytics/aggregation over large volumes: a clustered columnstore gives 5–10× compression and batch-mode
aggregation. Use it for a reporting table, a materialised view, or the archive table.

Not for OLTP: single-row updates on a columnstore are slow (deltastore fragmentation), and you can't have both a
clustered rowstore and a clustered columnstore on the same table.

### Q93. Azure SQL tier — DTU vs vCore, and how do you choose?

| | DTU | vCore |
|---|---|---|
| Unit | Blended CPU/IO/log | CPU cores + memory + IOPS, provisioned separately |
| Portability | Not portable off Azure | Maps to on-prem hardware expectations |
| Fit | Small, simple workloads | Everything you'd fine-tune; Azure Hybrid Benefit applies |
| Tiers | Basic/Standard/Premium | General Purpose / Business Critical / Hyperscale |

What I'd say in the room: *"vCore, because I can reason about cores and memory and I get hybrid benefit; DTU is fine for
a small internal app but it's an opaque blend."* Plus the two things that actually bite:

- **Serverless** is cheaper for intermittent workloads (auto-pause), with **cold-start latency** after a pause — wrong for
  a user-facing API.
- **Business Critical** gives local SSD and a free readable secondary; **Hyperscale** gives fast restore and huge
  databases with a separate compute/storage scale.

### Q94. Backup, RPO, RTO — what are yours and how do you decide?

- **RPO** (max acceptable data loss) determines backup frequency and replication: point-in-time restore gives you
  ~5–10 min granularity; geo-replication gives near-zero RPO but with async lag.
- **RTO** (max acceptable downtime) determines failover strategy: auto-failover group (minutes) vs restore (hours for a
  large DB).

**Say the ownership line:** *"RPO and RTO are business decisions I bring to the product owner with costs attached —
'we can do 15 minutes of data loss or 5, but 5 costs a geo-replica per region, and that's a monthly number you need to
approve'. What I never do is assume the default."*

Also: **a backup you haven't restored isn't a backup.** Say that you'd schedule a restore drill and record the actual
time, because the estimate is always optimistic.

### Q95. How do you do a schema change on a 500 GB production table?

The specific answer they're listening for: **no blocking DDL**.

- Adding a **nullable** column with no default: metadata-only in modern SQL Server (instant) — but a non-null default on
  an old version rewrites the table (pre-2012 behaviour; from 2012 onwards adding `NOT NULL` with a constant default is
  metadata-only).
- Adding an **index**: `ONLINE = ON` (Enterprise/Azure) so writes continue; otherwise schedule it in a window with a
  `MAXDOP` and expect log growth.
- **Changing a column type**: usually a table rewrite + lock. Do it as add-new-column → dual-write → backfill in batches →
  switch → drop old.
- **Backfill**: batched with a `WHERE` predicate and a `TOP (n)` loop, logged, resumable, off-peak.
- **Sequence:** expand (non-breaking, deployed and verified) → backfill (separate, monitored) → contract (next release).

### Q96. EF Core vs a separate read store (CQRS) — when do you split?

Split when the read model has genuinely different needs: different shape (dashboards), different store (search index),
different scale (1000 reads per write), or different latency requirements.

**Cost:** eventual consistency, a projection/relay to build and monitor, and a second thing that can break.

> **Say:** "I'd stay with one database and projections until the read path is measurably hurting the write path. CQRS
> is a tool for a measured asymmetry, not a default architecture — and the first version of the read model can be a
> materialised table rebuilt on a schedule, which costs a fraction of an event-sourced projection."

### Q97. Relational or Cosmos DB — how do you decide?

| Choose SQL (Azure SQL) | Choose Cosmos DB |
|---|---|
| Relationships, joins, transactions are core | Single-container access patterns, huge write throughput |
| Reporting/BI (SQL, Power BI) is needed | Global distribution with low write latency |
| Schema-first, integrity matters (money, inventory) | Very large scale-out with a stable partition key |
| Team knows SQL | Key-based access is dominant and you accept eventual consistency |

**The honest counterpoint to give:** *"Cosmos is not 'SQL but faster' — it's a different data model with RU costs, a
partition key you'd better get right on day one (you can't change it), and weaker query capability. Most systems I've
seen that chose it for 'scale' needed 500 rows per second and would have been fine on a $50 Azure SQL tier."*

### Q98. What's your non-negotiable data standard for a new service?

Have five, so it sounds like a standard and not a preference:

1. **Every table has a clustered primary key** on a narrow, ever-increasing key; `rowversion` where concurrency matters.
2. **Migrations are versioned, reviewed and applied by the pipeline** — never at startup.
3. **The app identity is least-privilege**; no `db_owner`; managed identity in Azure.
4. **Every foreign key is indexed**, and every index is justified by a named query.
5. **UTC everywhere** (`DATETIME2`/`DATETIMEOFFSET`), `decimal` for money, no floats.

If asked "why these" — say the failure they prevent. Each one has a story behind it, which is exactly what a lead
sounds like.

---

## 11. Rapid-fire drill (12 one-liners)

Read the prompt, answer out loud in one sentence. Ninety seconds total. Do this every day for a week.

| Prompt | Model answer |
|---|---|
| Clustered index? | The table itself, in key order, one per table. |
| Covering index? | Every column the query needs is in the index, so the base table is never touched. |
| `NOT IN` with NULL? | Returns no rows — use `NOT EXISTS`. |
| `UNION` vs `ALL`? | `UNION` de-duplicates and pays for a sort; `ALL` concatenates. |
| Seek vs scan? | Seek navigates the B-tree; scan reads everything — a scan is wrong only when it returns far fewer rows than it reads. |
| Deep `OFFSET`? | Costs O(offset) — keyset pagination instead. |
| Parameter sniffing? | Cached plan compiled for the first parameter value; fix with `RECOMPILE`, `OPTIMIZE FOR UNKNOWN`, or a plan split. |
| Implicit conversion? | Type mismatch converts the *column*, so the index can't be seeked — look for `CONVERT_IMPLICIT` in the plan. |
| `NOLOCK`? | Dirty reads, missing and duplicated rows — use RCSI instead. |
| Deadlock fix? | Consistent access order + shorter transactions; retry only covers the remainder. |
| Lost update? | Two read-modify-writes; fix with `rowversion` + `DbUpdateConcurrencyException` → 409. |
| N+1? | One query per parent; fix with projection or `Include`/`AsSplitQuery` + a command-count test. |

---

## 12. The whiteboard script: "the `/api/products` list endpoint is slow at 2M rows"

Six minutes. This is the single most likely data question in a fullstack lead loop because it spans front end, API, EF
Core and SQL — exactly the role. **Rehearse it end to end, out loud.**

### Minute 1 — quantify before touching anything

> "Before I change code I want numbers: p50 and p95 for the endpoint, whether it's all users or one tenant, and when it
> started. If p95 is 8 seconds and p50 is 200 ms, that's a tail problem — usually a specific parameter combination or a
> plan that flips — and the fix is different from a uniformly slow query. I'd also check whether the front end is
> paging or asking for everything: a React table doing client-side pagination over 2M rows is a front-end bug that no
> index will fix."

### Minute 2 — locate it

> "APM gives me the span breakdown: HTTP 8.2 s, SQL 7.9 s, serialisation 0.2 s. So it's the database. If SQL were 300 ms
> I'd be looking at N+1 in the API — counting `DbCommand` executions per request."

### Minute 3 — the query

> "Our query is `WHERE Price > @min ORDER BY Name` with `Skip/Take`. Two problems visible from the shape alone: there's
> no index on `(Price, Name)` so it's a scan + sort, and the `Skip/Take` is a deep `OFFSET`, which reads and discards
> the skipped rows. At 2M rows and page 200 that's 5,000 rows discarded per request."

### Minute 4 — the fix, in order

> "First, `CREATE INDEX IX_Products_Price_Name ON Products (Price, Name) INCLUDE (Description)` — that turns the scan
> and the sort into a seek with ordered output. Second, keyset pagination: `WHERE (Name, Id) > (@lastName, @lastId)` in
> the keyset sense, so cost is constant. I'd keep `OFFSET` only if the UI genuinely needs numbered pages, and then I'd
> cap the depth."

### Minute 5 — the API and front-end side

> "Server-side: project to a DTO with only the four columns the table needs, `AsNoTracking()`, and return `nextCursor` in
> the response envelope instead of a page number. That's a contract change, so it's versioned or additive. Front end:
> Angular's `@for` with `track` on the id, and `OnPush` so a page append doesn't re-render 200 rows; React's
> `useOptimistic`/infinite scroll keyed on the cursor. Both sides need to stop asking for more than a page."

### Minute 6 — verify, guard, cost

> "Verification is logical reads and plan shape before/after, plus p95 in staging. The guardrail is a Query Store review
> after deploy plus an integration test asserting command count and a read budget for the endpoint. The cost is one more
> index — ~2 extra index pages per insert, negligible at our write rate, and I'd confirm with the usage DMV in a month."

**Then stop talking.** The most common way this answer fails is by continuing into a lecture on indexes. Six minutes,
structured, with numbers, then hand it back.

---

## 13. Self-test — 25 questions, 40 minutes, no notes

Score yourself 1 point each, out loud, recorded. Anything you stumble on, go back to the section number.

| # | Question | § |
|---|---|---|
| 1 | Why is a random-GUID clustered key a problem? | D Q46 |
| 2 | Rewrite `WHERE YEAR(CreatedAt) = 2024` so it can seek. | B Q21 |
| 3 | What does a `Key Lookup` in a plan tell you? | B Q19 |
| 4 | Estimated 1 row, actual 400,000. Name three causes. | B Q23/24 |
| 5 | What's the left-prefix rule, and what breaks it? | B Q22 |
| 6 | Why does `NOT IN` return nothing when the subquery has a NULL? | A Q6 |
| 7 | Give the isolation level that stops phantoms, and its cost. | C Q33 |
| 8 | What does RCSI change, and what does it cost? | C Q34 |
| 9 | Implement optimistic concurrency in EF Core, end to end. | C Q35 |
| 10 | Why is a deadlock an access-order problem? | C Q36 |
| 11 | What's wrong with `NOLOCK`, in two sentences? | C Q37 |
| 12 | Name the three phases of a zero-downtime schema change. | D Q53 |
| 13 | `decimal` vs `float` for money — why, and what's the EF Core default scale? | D Q47 |
| 14 | How do you store timestamps across regions, and how does .NET map them? | D Q48 |
| 15 | What SQL does `Where(p => p.Price > 100).Take(20)` generate? | E Q55 |
| 16 | Find N+1 without a profiler. | E Q56 |
| 17 | Why is `Include` + collection dangerous on a list endpoint? | E Q57 |
| 18 | Biggest EF Core performance win, and why? | E Q58 |
| 19 | Make `EnableRetryOnFailure` dangerous. | E Q63 |
| 20 | How do you correlate a slow HTTP request to its SQL? | E Q64 |
| 21 | Ship a migration to production — the sequence. | E Q66 |
| 22 | Three ways SQL injection happens in .NET. | G Q81 |
| 23 | Name the three DB identities in a least-privilege setup. | G Q83 |
| 24 | Reporting is slowing checkout. Your escalation ladder. | H Q87 |
| 25 | Five non-negotiables for a new service's data layer. | H Q98 |

**Target: 20/25 before the interview.** Under 15 and you're not ready for the data section — do §B and §C again.

---

## 14. Study plan and the 30-second version

### Four-day plan (2 hours/day) — run this in the week before the loop

| Day | Focus | Deliverable |
|---|---|---|
| 1 | §A (Q1–Q18) + §B (Q19–Q32) | Answer the 12 rapid-fire drills (§11) in one breath |
| 2 | §C (Q33–Q44) + §D (Q45–Q54) | Write the `rowversion` concurrency code from memory |
| 3 | §E (Q55–Q72) | Recite §E Q55, Q56, Q58, Q66 without notes — these are the three most likely |
| 4 | §F + §G + §H, then §12 the whiteboard script | Full 6-minute rehearsal recorded, twice |

### Cross-links (don't re-read these, they're already covered)

- `05B` §7 — the one-page instinct table (the summary of this chapter's §A/§B).
- `05C` §9 — P34–P36, the three SQL coding problems you may have to write live.
- `05` — EF Core in the context of the API: DI scopes, `DbContext` lifetime bugs, the outbox, saga.
- `06` — the "SQL / data" rows of the interview question index.
- `09` — where data ownership and one-database-per-service get decided (ADR-shaped).
- `11` — Azure SQL hosting, managed identity, cost levers.

### 30-second version

> **"Default to EF Core for correctness and migrations, and drop to parameterised SQL for measured hot paths. Diagnose
> with the plan and Query Store, not with guesses — reads-to-rows ratio first, then estimated-vs-actual rows. Indexes
> cost writes, `NOLOCK` costs correctness, and caching is a staleness decision. Never ship a schema change and a code
> change in the same release."**

If you can say that paragraph calmly, with one concrete story behind each clause, you are ahead of most candidates in
a Microsoft fullstack lead loop.

---

## 30-second version of this chapter

> **SQL is where six years of .NET doing shows.** Answer tier-1 questions in one sentence and save your depth for tier-2
> ("this got slow — go") and tier-3 ("design the data layer for two consumers"). Every answer: name the mechanism, say
> where it bites in *our* code, state the cost, land on the risk. Read plans right-to-left, compare estimated to actual
> rows, quote logical reads. And have one real incident story with a guardrail at the end.

