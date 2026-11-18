# Chapter 5B — Data Structures, Big-O & Algorithm Patterns

> For a **lead** interview this is rarely "invert a binary tree". It is: can you pick the right collection, can you spot
> the O(n²), can you explain why a query got slow, and can you think out loud while you do it. The coding round itself
> (with model solutions) is `05C-CODING-QUESTION-BANK.md`.

---

## 0. What is actually being tested

| Skill | How it appears | The tell |
|---|---|---|
| **Complexity literacy** | "What's the complexity of that?" after you write anything | Can you say it *and* justify it in one sentence |
| **Right collection** | "How would you store this?" | `List.Contains` in a loop = instant fail |
| **Reading real code** | "Find the bug in this method" | Do you see the N+1 / O(n²) / double enumeration |
| **Data modelling** | "How would you model indexing for this" | Do you think in access patterns, not in tables |
| **Thinking out loud** | Live coding | Silence for 3 minutes is the failure mode, not wrong code |

**Lead-level calibration:** you are not expected to be faster than a 25-year-old at writing a red-black tree. You *are*
expected to know when a `HashSet` replaces a nested loop, why a dictionary became the bottleneck, and how to keep a
junior from shipping the quadratic version.

---

## 1. Big-O without the maths

Big-O answers: **"as the input grows, how does the work grow?"** It is not a stopwatch; it is a shape.

| Notation | Name | Grows like | n = 1,000 | Typical cause |
|---|---|---|---|---|
| O(1) | constant | nothing | 1 op | dictionary/hash lookup, array index |
| O(log n) | logarithmic | halving | ~10 ops | binary search, balanced tree, B-tree index seek |
| O(n) | linear | proportional | 1,000 | single pass, full table scan |
| O(n log n) | linearithmic | sort-ish | ~10,000 | `OrderBy`, index build, merge sort |
| O(n²) | quadratic | squared | **1,000,000** | nested loop over the same collection |
| O(n³) | cubic | cubed | 10⁹ — hangs | triple loop (3-way join done badly) |
| O(2ⁿ) | exponential | doubles per item | astronomically | naive recursive Fibonacci, subset enumeration |
| O(n!) | factorial | explodes | never | naive TSP, permutations |

### The three sentences that make you sound senior

1. **"Quadratic is where systems die."** n = 10,000 in a nested loop is 100 million operations — invisible in a unit test with 10 items, a production incident with 10,000.
2. **"Constants matter more than the class at small n."** An O(n²) loop over 20 items beats a hash lookup with allocation. Don't trade readability for asymptotics you don't need.
3. **"The real complexity is often the network, not the CPU."** 500 ms × 100 sequential HTTP calls is 50 seconds regardless of your algorithm. That's the actual bottleneck in most .NET code, and naming it is the lead answer.

### Amortised cost — the phrase you should be able to explain

`List<T>.Add` is **O(1) amortised**: the backing array doubles when full, which is an O(n) copy, but spread over n adds it averages out to O(1). Hence:

```csharp
var list = new List<Order>(expectedCount);   // pre-size when you know it — avoids log(n) reallocations
```
Same idea for `StringBuilder` capacity and `Dictionary` capacity (`new Dictionary<K,V>(capacity)`).

### Space complexity

Also say the memory: `List<T>` = 2× worst-case buffer bloat during growth; `Dictionary` = ~2× entries for buckets + entries; string concatenation in a loop = O(n²) memory as well as time. **The most common real-world memory bug is materialising a query you only needed to stream** (`ToList()` on a million rows).

---

## 2. The .NET collection decision table (memorise this)

| Type | Ordering | Lookup | Insert | Remove | Use when |
|---|---|---|---|---|---|
| `T[]` | index | **O(1)** by index | fixed size | fixed size | Fixed, known size; hot loops; `Span` source |
| `List<T>` | index | O(1) by index, **O(n) by value** | **O(1)** amortised append, O(n) insert | O(n) | **The default ordered collection** |
| `Dictionary<TKey,TValue>` | none (insertion order in practice, not guaranteed) | **O(1)** avg | O(1) avg | O(1) avg | Key → value lookup; **de-dupe with `HashSet<T>`** |
| `HashSet<T>` | none | **O(1)** avg `Contains` | O(1) avg | O(1) avg | Membership, de-duplication, set ops (`Union`, `Intersect`, `Except`) |
| `SortedDictionary<K,V>` | **sorted by key** | O(log n) | O(log n) | O(log n) | Ordered iteration **and** frequent mutation (red-black tree) |
| `SortedList<K,V>` | sorted | O(log n) (binary search over array) | **O(n)** (array shift) | O(n) | Sorted + mostly-immutable + memory-sensitive (arrays, not nodes) |
| `SortedSet<T>` | sorted | O(log n) | O(log n) | O(log n) | Ordered uniqueness, range queries in memory |
| `LinkedList<T>` | links | **O(n)** | O(1) at a known node | O(1) at a known node | Rare — LRU lists, queues needing removal from the middle |
| `Queue<T>` | FIFO | O(1) front peek | O(1) enqueue | O(1) dequeue | Work queues, BFS, buffering |
| `Stack<T>` | LIFO | O(1) peek | O(1) push | O(1) pop | Undo, DFS, bracket matching, recursion replacement |
| `PriorityQueue<TElement,TPriority>` (.NET 6+) | by priority | O(1) peek | **O(log n)** | O(log n) | Top-K, Dijkstra, schedulers, retry backoff |
| `ConcurrentDictionary<K,V>` | none | O(1) avg, lock-free reads | O(1) avg | O(1) avg | Multi-threaded maps |
| `ConcurrentQueue<T>` / `ConcurrentBag<T>` | FIFO / none | — | O(1) | O(1) | Producer/consumer without a channel |
| `Channel<T>` | FIFO, bounded/unbounded | — | O(1) async | O(1) async | **Async producer/consumer — the modern default** |
| `BlockingCollection<T>` | FIFO, bounded | — | blocking | blocking | Legacy in-process queue (prefer `Channel<T>`) |
| `ImmutableArray<T>` | index | O(1) | O(n) copy | O(n) copy | Shared read-only state, safe publication |
| `FrozenDictionary` (.NET 8) | none | **fastest** reads | once, then frozen | never | Build once at startup, read-only forever |
| `IReadOnlyList<T>` | — | — | — | — | **Return type**, not a storage choice |

### The seven comparisons that actually get asked

**1. `List<T>.Contains` vs `HashSet<T>.Contains`** — this is *the* question.
```csharp
// O(n × m) — the classic quadratic. 10,000 × 10,000 = 100M comparisons
var known = new List<string> { "cust-1", "cust-2", ... };
foreach (var id in incoming) if (known.Contains(id)) { ... }

// O(n + m) — one build, constant lookups
var knownSet = new HashSet<string>(known, StringComparer.OrdinalIgnoreCase);
foreach (var id in incoming) if (knownSet.Contains(id)) { ... }
```
Say the magic words: **"`List.Contains` is a linear scan; anything inside a loop means quadratic. `HashSet` turns it into a hash lookup."**

**2. `Dictionary` vs `SortedDictionary` vs `SortedList`**
- Need O(1) lookup and don't care about order → `Dictionary`.
- Need sorted iteration with frequent inserts → `SortedDictionary` (O(log n) everywhere).
- Need sorted, mostly read-only, and memory matters → `SortedList` (arrays; iterate fast, insert slow).
- Need to iterate sorted once from an unordered result → **just `Dictionary` + `OrderBy` at the end** (O(n log n) once beats O(log n) on every insert).

**3. Array vs `List<T>`** — array when the size is fixed and you care about allocations/`Span` access; `List` almost everywhere else. `List` wraps an array, so indexing cost is identical.

**4. `HashSet` vs `Dictionary`** — if you never ask "what's the value?", you want a `HashSet`. "I see `Dictionary<string, bool>` used as a set" is a legitimate review comment.

**5. `Queue` vs `Stack`** — BFS vs DFS; fair processing vs last-in-first. Obvious, but they'll ask you to justify it for a real case: "retry queue" → `Queue`; "undo history" / "navigate back" → `Stack`.

**6. What collection for an LRU cache?** → `Dictionary<key, LinkedListNode<...>>` + `LinkedList<...>`. O(1) `Get` and O(1) "move to front" because the dictionary holds the node reference. This is the question most likely to appear at lead level because it has a real production use case. Full code in §6.

**7. What about thread safety?** — `Dictionary` is not thread-safe for concurrent writes; `ConcurrentDictionary` is (lock-free reads, fine-grained locks on writes). **But**: a read-modify-write across two operations is still a race even on `ConcurrentDictionary` — you need `AddOrUpdate`/`GetOrAdd` or an external lock. And a `List<T>` being *read* by many threads is fine only if nobody writes.

---

## 3. The eight complexity questions about code you will actually write

### Q1. Is this method O(n²)?

```csharp
// You will be shown this shape in a code review question
foreach (var order in orders)                             // n
    foreach (var item in order.Items)                     // m — usually fine, it's a total
        ...
```
Nested loops are only quadratic when both levels iterate **the same collection**. `orders × itemsPerOrder` is **O(total items)** — a legitimate linear pass. **Saying this distinction out loud is a strong senior signal**, because junior candidates call every nested loop O(n²).

The real quadratic shapes to spot:
```csharp
foreach (var o in orders) if (allOrders.Contains(o))       // List.Contains in a loop  → O(n²)
var query = orders.Where(o => db.Orders.Any(x => x.Id == o.Id));   // N+1 → O(n) round trips
orders.Select(o => orders.Count(x => x.CustomerId == o.CustomerId)); // O(n²) grouping
list.Insert(0, item);   // in a loop → O(n²) — every insert shifts the array
result = result + next; // string in a loop → O(n²)
```

### Q2. Where's the N+1 in this EF Core code?

```csharp
// ❌ 1 + N queries
var orders = await db.Orders.ToListAsync(ct);
foreach (var o in orders)
    Console.WriteLine(o.Customer.Name);     // lazy-loads per order

// ✅ 1 query (or 2 with AsSplitQuery)
var orders = await db.Orders
    .Include(o => o.Customer)
    .AsNoTracking()
    .ToListAsync(ct);

// ✅✅ usually best: one projection, only the columns you need
var rows = await db.Orders
    .Select(o => new { o.Id, CustomerName = o.Customer.Name, o.TotalAmount })
    .ToListAsync(ct);
```
This is the single most likely "find the performance bug" question in a .NET interview, and it is a **data-structure** question wearing a framework costume: the fix is "stop doing an O(n) lookup per item; do one set-based query".

### Q3. Why did this endpoint become slow when the table grew?

Almost always one of:
1. **Missing index** → the DB is doing an O(n) scan inside an O(n) loop.
2. **N+1** → O(n) round trips, each with latency.
3. **Client evaluation** → `ToList()` before `Where` → pulling O(n) rows over the wire.
4. **Deep paging** → `Skip(100000)` makes SQL Server read and discard 100,000 rows.
5. **Tracking** → change tracking on 100,000 read-only entities (memory + CPU).
6. **Cartesian explosion** → three `Include`s on a collection, `SELECT` returns n×m×k rows → `AsSplitQuery()`.

**Say the diagnostic method, not just the answer:** "I'd get the actual SQL (`LogTo(Console.WriteLine)`) and the execution plan first. Guessing at EF performance without the plan is how people 'optimise' the wrong query."

### Q4. `OrderBy` vs `Sort` — and why does stability matter?

- `List<T>.Sort` / `Array.Sort` → **introsort**, O(n log n), **unstable** (equal elements may swap), and **in place**.
- `Enumerable.OrderBy` → **stable** (equal elements keep source order), O(n log n), allocates a new sequence.
- `OrderBy(...).ThenBy(...)` is a stable multi-key sort. `list.Sort(cmp)` with a multi-key comparator is faster but unstable.

**Why it matters in a UI:** paging over an **unstable** sort with ties gives duplicated/missing rows across pages. That's a real production bug and a great answer: "always sort by something unique — `OrderByDescending(o => o.CreatedAt).ThenBy(o => o.Id)` — or SQL paging will skip and repeat rows when timestamps collide."

### Q5. `First` vs `FirstOrDefault` vs `Single` vs `SingleOrDefault`?

| Method | Empty source | More than one match |
|---|---|---|
| `First` | throws | returns first |
| `FirstOrDefault` | default | returns first |
| `Single` | throws | **throws** |
| `SingleOrDefault` | default | **throws** |
| `ElementAt` | throws | — |

`Single` is an **assertion** — use it when duplicates would be a data bug (lookup by unique key). In an API, `FirstOrDefault` + `null` check → 404 is the standard. And note: `Single` on `IQueryable` emits `TOP 2` so the database helps you detect the violation.

### Q6. `Count()` vs `Any()` vs `Count` property vs `LongCount()`

- `list.Count` (property) — O(1).
- `enumerable.Count()` — O(n) on a plain `IEnumerable` (iterates!), SQL `COUNT(*)` on `IQueryable`.
- `Any()` — O(1) short-circuit, SQL `EXISTS`.
- `LongCount()` — for >2.1B rows (a real consideration for log/audit tables).

**The trap:** `if (query.Count() > 0)` on an `IQueryable` is a `COUNT(*)` over the whole filtered set instead of `EXISTS` with `TOP 1`.

### Q7. `GroupBy` — what's the complexity, and where does it hurt?

O(n) building a hash-based grouping, then O(k log k) if you sort each group. Two issues:
- **Deferred** — nothing happens until enumerated, and it **materialises** internally (it is not streaming group-by in LINQ-to-Objects).
- On `IQueryable`, `GroupBy` translates to SQL `GROUP BY` **only** if your projection is aggregate-only. Anything else and EF throws or falls back to client evaluation. Project aggregates before leaving the server:
```csharp
var byCustomer = await db.Orders
    .GroupBy(o => o.CustomerId)
    .Select(g => new { CustomerId = g.Key, Count = g.Count(), Total = g.Sum(o => o.TotalAmount) })
    .ToListAsync(ct);
```

### Q8. How do you page efficiently?

```csharp
// ❌ offset paging — O(offset) work on the server, and it skips/duplicates rows if data changes mid-scroll
.Skip((page - 1) * size).Take(size)

// ✅ keyset / cursor paging — O(log n) with an index on the sort key, stable under inserts
.Where(o => o.CreatedAt < cursor.CreatedAt || (o.CreatedAt == cursor.CreatedAt && o.Id < cursor.Id))
.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id)
.Take(size)
```
**Say:** "offset paging is fine for page 1–10 in an admin grid. For infinite scroll or an API consumed by an app, I use keyset pagination — the deep-page cost of `OFFSET 100000` is the same problem as an O(n) lookup inside a loop."

---

## 4. The algorithm patterns — a trigger table

You do not need to memorise algorithms. You need to recognise **which pattern a problem wants**, because that
recognition is what produces a solution in 20 minutes instead of 60.

| If the problem says… | Reach for | Why | Complexity |
|---|---|---|---|
| "pair / sum / count that matches" | **Hash map** | Turn the inner loop into an O(1) lookup | O(n) |
| "duplicates / unique / seen before" | **`HashSet`** | Membership in O(1) | O(n) |
| "sorted array, find target" | **Binary search / two pointers** | Order lets you discard half / move inward | O(log n) / O(n) |
| "contiguous subarray / substring of size k" | **Sliding window** | Reuse the previous window's work | O(n) |
| "top K / kth largest / most frequent" | **Heap (`PriorityQueue`)** or `OrderBy().Take(k)` | Partial sort beats a full sort | O(n log k) |
| "sort then greedy" | **Sort first** | Sorted input makes the greedy choice provable | O(n log n) |
| "shortest path / minimum steps" | **BFS** with `Queue` | Level-by-level = fewest edges | O(V+E) |
| "all paths / connected components / cycle" | **DFS** with `Stack` or recursion + visited set | Exhaustive exploration | O(V+E) |
| "count the ways / min cost with overlapping subproblems" | **DP + memo table** | Avoid re-computation | O(states × transitions) |
| "sum over a range, repeatedly" | **Prefix sums** | O(1) range query after O(n) precompute | O(n) + O(1) |
| "next greater / matching bracket / nesting" | **`Stack`** | LIFO matches the nesting structure | O(n) |
| "intervals / merge ranges" | **Sort by start, then sweep** | Overlap becomes adjacent comparison | O(n log n) |
| "coarse-grained time window / rate" | **Token bucket + `TimeProvider`** | Real production shape of the above | O(1) |
| "design a cache that evicts" | **`Dictionary` + `LinkedList`** | O(1) get and O(1) move-to-front | O(1) |

### The 20-second pattern-recognition drill

Cover the right column, read the left, say the pattern out loud. Ten minutes a day for three days and you will not freeze in the coding round.

---

## 5. The data-structure questions a lead actually gets

These are *design* questions dressed as data-structure questions. Answer with access patterns first.

### Q. "Design the in-memory index for a product catalogue search."

Work through it out loud:

1. **Clarify the access pattern** — by id? by name prefix? by category? full-text? how many writes vs reads?
2. **Primary store:** `Dictionary<Guid, Product>` → O(1) by id.
3. **Name search:** sort-free scan is O(n) per keystroke — unacceptable. So either a `SortedList<string, List<Guid>>` for prefix ranges (`O(log n)` seek + range), or push it to the database's full-text index. **Say explicitly:** "in-memory prefix search is the wrong place to solve this at scale — SQL Server full-text or Azure AI Search is the right tool; I'd only keep an in-memory index for a small, hot subset."
4. **Category filter:** `Dictionary<string, HashSet<Guid>>` postings lists, then intersect (`HashSet.IntersectWith`).
5. **Freshness:** entries need eviction → `IMemoryCache` with absolute + sliding expiration, plus an explicit invalidation on `ProductChanged` message.
6. **Thread safety / scale-out:** an in-memory index is per-instance. With 3 pods you get 3 divergent caches — that's `IMemoryCache` vs `Redis` (`IDistributedCache`), which is a `09`/`11` decision.
7. **Cost:** memory ≈ entries × (object overhead + string). Say a number and how you'd measure it.

**That structure — clarify, primary store, secondary index, freshness, concurrency, cost — is the whole answer format for any "design a data structure" question at this level.**

### Q. "How do you keep a per-customer rate limit across three instances?"

In-memory token bucket per instance is wrong (3× the intended limit). Options:
- **Distributed counter:** Redis `INCR` + `EXPIRE` (O(1), atomic) — the standard answer.
- **Sliding window log** in Redis (sorted set of timestamps) — more accurate, more memory.
- **Fixed window** — simplest, allows a burst at the window boundary.
- **Token bucket** — smooths bursts, needs a stored token count + last-refill timestamp.

Then: "at the API gateway (Azure API Management / Front Door) is cheaper than implementing it in every service." That last sentence is the lead answer — buy the platform feature before writing the distributed algorithm.

### Q. "How would you implement an LRU cache?" (see §6 — full code)

### Q. "What data structure for the outbox/idempotency table?"

"An `Idempotency-Key` unique index in the database. The **unique constraint is the data structure** — it's what makes the operation idempotent under concurrency, because the second insert fails atomically. In memory you'd use a `ConcurrentDictionary` with `TryAdd`, but durability requires the database." This connects DS&A to the distributed-systems material in `05` §11 — a strong cross-topic link.

### Q. "Why is my `Dictionary` slow?"

Possible real answers:
- Bad `GetHashCode()` — every key collides → O(n) per lookup, all the way to O(n²). **Classic bug: a mutable field in the hash code, or a hash based on `ToString()` of a huge object.**
- Wrong capacity → repeated resizing and rehashing.
- `Dictionary` used as an ordered structure (`OrderBy` on every read).
- The comparison is expensive (`StringComparer.InvariantCulture` on 10k strings) — use `Ordinal`.
- It's not the dictionary — it's the allocation of a large value type per entry, or boxing.

**Say:** "the first thing I'd check is the hash function and the capacity; the second is whether we're enumerating the whole dictionary per request."

---

## 6. Worked examples (the four most likely in-memory designs)

### 6.1 LRU cache — O(1) get and put

```csharp
public sealed class LruCache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _map;
    private readonly LinkedList<(TKey Key, TValue Value)> _order = new();   // head = most recent
    private readonly object _sync = new();

    public LruCache(int capacity)
    {
        _capacity = capacity > 0 ? capacity : throw new ArgumentOutOfRangeException(nameof(capacity));
        _map = new Dictionary<TKey, LinkedListNode<(TKey, TValue)>>(capacity);
    }

    public bool TryGet(TKey key, out TValue? value)
    {
        lock (_sync)
        {
            if (!_map.TryGetValue(key, out var node))
            {
                value = default;
                return false;
            }
            _order.Remove(node);          // O(1) — we hold the node, not the value
            _order.AddFirst(node);
            value = node.Value.Value;
            return true;
        }
    }

    public void Put(TKey key, TValue value)
    {
        lock (_sync)
        {
            if (_map.TryGetValue(key, out var existing))
            {
                _order.Remove(existing);
                _order.AddFirst(existing);
                existing.Value = (key, value);
                return;
            }

            if (_map.Count == _capacity)   // evict least recently used
            {
                var lru = _order.Last!;
                _order.RemoveLast();
                _map.Remove(lru.Value.Key);
            }

            var node = _order.AddFirst((key, value));
            _map[key] = node;
        }
    }
}
```
**The point to narrate:** "`LinkedList` alone can't find a key in O(1); `Dictionary` alone can't reorder in O(1). The combination works because the dictionary stores the **node reference**, so removal is pointer surgery, not a search. That's the 'why' interviewers are listening for."

**Follow-ups:** "make it thread-safe without one global lock" → `ConcurrentDictionary` + a lock only for the order list, or shard by key, or just use `Microsoft.Extensions.Caching.Memory` (`IMemoryCache` has size-limited eviction built in) — "in production I use `IMemoryCache` with `SetSize`/`SizeLimit`, not my own LRU". **Saying that is the right answer**; the exercise is to prove you can reason, not that you should ship it.

### 6.2 Top-K frequent items with a heap

```csharp
public static IReadOnlyList<(string Item, int Count)> TopK(IEnumerable<string> items, int k)
{
    var counts = new Dictionary<string, int>(StringComparer.Ordinal);
    foreach (var item in items) counts[item] = counts.GetValueOrDefault(item) + 1;   // O(n)

    // Min-heap of size k: the smallest of the current top-k is on top, so it's the one to evict
    var heap = new PriorityQueue<string, int>();
    foreach (var (item, count) in counts)                                            // O(distinct log k)
    {
        if (heap.Count < k) heap.Enqueue(item, count);
        else if (count > heap.PeekPriority())
        {
            heap.Dequeue();
            heap.Enqueue(item, count);
        }
    }
    return heap.UnorderedItems.Select(x => (x.Element, x.Priority)).OrderByDescending(x => x.Item2).ToList();
}
```
Complexity: **O(n + d log k)** where d = distinct, versus **O(n log n)** for `OrderByDescending().Take(k)`. At 10M rows with k = 10 and d = 1M, that's the difference between a second and many seconds.
**Say:** "for k small and n large, a bounded min-heap is the win. If k is large or the data is in a database, `ORDER BY COUNT(*) DESC` with an index is the right answer and I wouldn't write this at all."

### 6.3 Sliding window (max sum of k consecutive)

```csharp
public static decimal MaxWindowSum(IReadOnlyList<decimal> values, int k)
{
    if (k <= 0 || values.Count < k) throw new ArgumentException("k out of range");

    decimal window = 0;
    for (int i = 0; i < k; i++) window += values[i];      // first window  O(k)
    decimal best = window;

    for (int i = k; i < values.Count; i++)                 // slide           O(n-k)
    {
        window += values[i] - values[i - k];
        if (window > best) best = window;
    }
    return best;
}
```
**O(n)** instead of the naive O(n·k). The insight to narrate: "each slide adds one and removes one — recomputing the whole window is the wasted work."

### 6.4 Bracket/validation with a stack — and its real-world use

```csharp
public static bool IsBalanced(string s)
{
    var pairs = new Dictionary<char, char> { [')'] = '(', [']'] = '[', ['}'] = '{' };
    var stack = new Stack<char>();
    foreach (var c in s)
    {
        if (c is '(' or '[' or '{') stack.Push(c);
        else if (pairs.TryGetValue(c, out var open))
        {
            if (stack.Count == 0 || stack.Pop() != open) return false;
        }
    }
    return stack.Count == 0;
}
```
Real-world framing that lands better than the toy: "the same stack discipline is how JSON readers, Razor/HTML tag matching, and expression parsers work. When I see a manual depth counter in a parser, I look for the bug the stack would have prevented."

---

## 7. SQL / data questions (a fullstack lead gets these too)

The database is a data structure with a query planner. Interviewers test the same instinct.

> **This is the one-page summary only.** The full question bank — 98 Q&A on indexing and plans, transactions and
> concurrency, schema design, EF Core ↔ SQL, security and scaling — is **`05D-SQL-AND-DATA-ACCESS-FOR-DOTNET.md`**.

| Question | Answer core |
|---|---|
| Why is this query slow after 2M rows? | Missing index → scan; N+1; client evaluation; deep `OFFSET`; tracking. **Get the plan first.** |
| What's a **clustered** index? | The table *is* the index — rows stored in key order. One per table. Default = primary key. That's why **random GUID keys fragment it**. |
| Clustered vs non-clustered? | Non-clustered = separate structure with key + row locator; a **covering** index includes all needed columns so no key lookup is needed. |
| What's a covering index? | All columns in `SELECT`/`WHERE`/`ORDER BY` are in the index (as keys or `INCLUDE`) → index-only read. |
| `EXISTS` vs `IN` vs `JOIN`? | `EXISTS` short-circuits on the first match; `IN` with a subquery can force a full evaluation; a `JOIN` multiplies rows. For "has any", use `EXISTS`. |
| `LEFT JOIN` filter trap? | Putting a right-table predicate in `WHERE` silently converts it to an inner join — put it in the `ON` clause or filter for `NULL`. **Very common bug.** |
| `UNION` vs `UNION ALL`? | `UNION` deduplicates (sort/hash → expensive); `UNION ALL` doesn't. Use `ALL` unless you need dedupe. |
| Isolation levels? | Read committed (default) allows non-repeatable reads; `SNAPSHOT`/`RCSI` gives readers a consistent version without blocking writers — **the usual fix for "the report blocks the app"**. |
| `NOLOCK`? | Dirty reads. Almost never the right answer; say "I'd use RCSI instead." |
| Deadlock cause? | Two transactions taking locks in **opposite order**. Fix: consistent ordering, shorter transactions, narrower locks. |
| Optimistic concurrency? | A `rowversion`/`RowVersion` token; `UPDATE ... WHERE RowVersion = @v`; 0 rows → `DbUpdateConcurrencyException` → 409. (Also in `05` §8.) |
| Pagination in SQL? | `OFFSET/FETCH` is offset paging (`OFFSET 100000 ROWS` reads and discards). Keyset: `WHERE (CreatedAt, Id) < (@c, @id) ORDER BY CreatedAt DESC, Id DESC`. |
| Index selectivity? | An index on a column with 3 distinct values over 10M rows won't be used. Index the columns you filter and sort by, in that order; put the most selective first. |
| Why not index every column? | Every index costs write throughput and storage; the planner can also pick wrong with too many. |

**The lead addition:** "all index decisions come from the query workload, not from the schema. I'd ask for the top 5 queries by frequency and by total duration, then index for those — and I would not add an index without a plan showing me what it will help."

---

## 8. How to answer a complexity question in the room

Never answer with a bare "O(n log n)". Use this three-beat template:

> **"This is O(n log n) — the sort dominates. The scan before it is O(n), and the final projection allocates a new
> list, so memory is O(n). If n were large enough that the sort mattered, I'd use a bounded heap and get O(n log k).
> At our current volume — about 50k orders a day — the sort is irrelevant and I'd keep the simpler code."**

That last clause — **"and at our volume it doesn't matter"** — is what distinguishes an engineer who understands cost
from one who recites notation. Interviewers remember it.

### The five follow-ups you should expect after writing any algorithm

1. "What if the input is empty / null / has one element?" → handle at the top, say the edge cases first.
2. "What if it doesn't fit in memory?" → streaming (`IAsyncEnumerable`), chunking, external sort, or push to the database.
3. "What if it's called concurrently?" → immutability, `ConcurrentDictionary`, or a per-request instance. State the contract.
4. "How would you test it?" → table-driven xUnit `[Theory]` with the boundary cases: empty, single, duplicates, negatives, max.
5. "How would you make it faster?" → profile first, then: fewer allocations, `Span`, avoid boxing, pre-size collections, cache the hash, or move it to the database. **Never "use `Parallel.ForEach`" as the first answer.**

---

## 9. Data structures in the front end (the fullstack bonus)

You are interviewing as a **fullstack** lead, so connect the same instincts to Angular and React:

| Backend instinct | Front-end equivalent |
|---|---|
| `Dictionary` for O(1) lookup | `Map<string, Product>` instead of `array.find()` inside a template — `find` in a loop is the same quadratic bug |
| `HashSet` for membership | A `Set<number>` of selected ids, not `array.includes()` per row |
| Immutable data | React state updates must create new references, or `memo`/`OnPush` won't see the change |
| Don't rebuild the collection per request | Don't rebuild the list per render — memoise (`useMemo`, `computed`) |
| N+1 queries | N+1 HTTP calls in a component; or N+1 re-renders because a `key` is missing |
| Index on the sort key | `track` in `@for` / `key` in `.map()` — the reconciler's identity map is a data structure; get it wrong and it's O(n²) DOM work |

That table is the single best "I'm a fullstack lead, not a backend lead who tolerates the UI" answer available to you. Rehearse it.

---

## 10. Self-test (20 minutes, out loud)

1. Name the complexity and the cause for: dictionary lookup, binary search, `List.Contains`, `OrderBy`, nested loop over the same list.
2. When is a nested loop **not** O(n²)?
3. `List.Contains` inside a loop — what's the fix and what's the new complexity?
4. `Dictionary` vs `SortedDictionary` vs `SortedList` — one use case each.
5. `HashSet` vs `Dictionary<string,bool>` — which is right and why?
6. `Queue` + `Stack` — name a real production use for each.
7. Which two collections combine for an O(1) LRU cache, and why does the dictionary store a *node*?
8. Where is the N+1 in this EF query, and what are two different fixes?
9. Why does `Skip(100000).Take(20)` get slower as the page number grows, and what's the alternative?
10. `First` vs `Single` vs `FirstOrDefault` — and which one asserts a data invariant?
11. `Any()` vs `Count() > 0` — what does the SQL look like for each?
12. Why is `OrderBy` being *stable* a correctness issue for paging?
13. Clustered vs non-clustered index; what does "covering" add?
14. Why do random GUID primary keys hurt insert performance, and what do you do about it?
15. `LEFT JOIN` filter bug — show it and fix it.
16. `UNION` vs `UNION ALL`.
17. How do you rate-limit across three instances, and where should it live?
18. Top-K on 10M rows — which structure and which complexity?
19. What's the complexity of `GetOrAdd` on a `ConcurrentDictionary`, and what's the catch?
20. Give the front-end equivalent of `List.Contains` in a loop.

If 15+ land cleanly, the DS&A portion is closed. Then go do the drills in `05C-CODING-QUESTION-BANK.md`.

---

## 30-second version

Big-O is a shape, not a stopwatch: **O(1) hash lookup, O(log n) index seek, O(n) scan, O(n log n) sort, and O(n²) is where production dies.** The three questions you will actually be graded on are *"which collection"*, *"where's the quadratic / N+1"*, and *"what's the complexity of what you just wrote"* — and the winning habit is answering with the access pattern and the volume ("at 50k rows a day the sort is irrelevant") rather than the notation alone. Learn the trigger table in §4 and the collection table in §2 and you cover 90% of what a fullstack lead is asked.
