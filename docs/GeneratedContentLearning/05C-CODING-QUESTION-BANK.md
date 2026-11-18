# Chapter 5C — Coding Question Bank & Live-Coding Playbook

> **This is the round most senior candidates lose.** Not because the problems are hard — they aren't — but because
> they go quiet, start typing immediately, or produce code with no tests and no complexity statement.
> 32 problems with model C# solutions, in the shape a Microsoft-style loop asks them.

---

## 0. How the coding round is actually scored

Almost every structured loop scores four things separately. Code that compiles but scores 2s on three of them loses to
slightly imperfect code that scores 4s on all four.

| Dimension | Score 4 looks like | Score 2 looks like |
|---|---|---|
| **Problem solving** | Restates the problem, asks 2–3 clarifying questions, names a brute force **before** optimising, states complexity | Starts typing, discovers the problem mid-code |
| **Code quality** | Small named methods, meaningful names, no dead code, handles the boundary | One 60-line method, `x`, `temp`, `data` |
| **Testing / correctness** | Walks a hand-worked example, states edge cases, suggests a `[Theory]` | "It should work" |
| **Communication** | Narrates the plan and trade-offs while typing | Silence, or narration of syntax ("now I write for, open paren") |

**The single highest-leverage habit:** say the plan out loud for 60 seconds **before** touching the keyboard, then keep a
low, steady commentary. Interviewers grade what they heard, not what you were thinking.

### The 8-step protocol (run this on every problem, even easy ones)

```
1. Restate the problem in one sentence, in your own words.
2. Ask 2–3 clarifying questions: input size, duplicates, nulls, empty, expected output shape.
3. Write 2–3 hand-worked examples — including a boundary one. Put them on the board/screen.
4. State the brute force + its complexity. ("The obvious way is nested loops, O(n²) — that's not good enough
   if n is large.")
5. Name the insight that improves it, in one sentence.
6. Write the code, top-down: signature first, then helpers.
7. Walk your examples through the code you just wrote. Find at least one bug yourself.
8. State final complexity (time + space) and what you'd do differently at 100× the size.
```

Practise the protocol until it is automatic. Then the problem content barely matters.

### The questions to always ask

| Domain | Ask |
|---|---|
| Input | "Can it be null?" · "Empty?" · "Sorted?" · "Duplicates?" · "How big — thousands or millions?" |
| Values | "Can numbers be negative?" · "Zero?" · "Overflow risk?" |
| Output | "Return the value, all of them, or just a count?" · "Does order matter?" · "What on no-result — `null`, empty, or throw?" |
| Contract | "Is the input mutable? May I sort it?" · "Is this called concurrently?" |
| Money | "`decimal` or double?" · "What rounding, and where?" — *always ask this in a .NET money problem* |
| Culture | "Should string comparison be ordinal or culture-aware?" — a small detail that reads as experienced |

### What to say when you're stuck (memorise one of these)

- **"Let me work the small case by hand."** → then generalise. This is the most reliable unblock.
- **"The brute force is nested loops; let me see if I can trade memory for time."** → usually leads to a hash map.
- **"If I sort first, does the problem get easier?"** → unlocks two pointers, greedy, interval merge.
- **"I'm going to write the correct-but-slow version, then optimise — better than nothing on the board."** → shows judgement; graders score partial credit generously.
- **Never** say nothing for 60+ seconds. Say "I'm thinking about the data structure" — that's information, silence is not.

---

## 1. Warm-ups (P1–P5)

These appear as the "we start easy" opener. They are **not** testing difficulty; they are testing whether you narrate and handle boundaries.

### P1. Reverse the words in a sentence

> "Given `'the quick brown fox'` return `'fox brown quick the'`. Words separated by single spaces."

**Ask:** multiple spaces? leading/trailing whitespace? punctuation attached to words?

```csharp
// ✅ Idiomatic and readable — what you should write first
public static string ReverseWords(string sentence)
{
    ArgumentNullException.ThrowIfNull(sentence);
    return string.Join(' ', sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Reverse());
}

// ✅ If they push on allocations: in-place on a char array, reverse whole then reverse each word
public static string ReverseWordsInPlace(string sentence)
{
    var chars = sentence.Trim().ToCharArray();
    Array.Reverse(chars);                              // "xof nworb ... eht"
    int start = 0;
    for (int i = 0; i <= chars.Length; i++)
    {
        if (i == chars.Length || chars[i] == ' ')
        {
            Array.Reverse(chars, start, i - start);    // reverse each word back
            start = i + 1;
        }
    }
    return new string(chars);
}
```
**Complexity:** O(n) time, O(n) space (first version allocates the split array).
**Follow-ups:** "one line?" → the LINQ version. "without splitting?" → the in-place version. "how do you test it?" → `[Theory]` with `""`, `" "`, `"a"`, `"a b"`, double spaces.
**The trap to name:** `sentence.Split(' ')` with consecutive spaces yields empty strings — `RemoveEmptyEntries` is the fix, and mentioning it is a correctness point.

### P2. FizzBuzz — but answer it like a senior

```csharp
public static IEnumerable<string> FizzBuzz(int n)
{
    ArgumentOutOfRangeException.ThrowIfNegative(n);
    for (int i = 1; i <= n; i++)
    {
        yield return (i % 3, i % 5) switch
        {
            (0, 0) => "FizzBuzz",
            (0, _) => "Fizz",
            (_, 0) => "Buzz",
            _      => i.ToString(CultureInfo.InvariantCulture)
        };
    }
}
```
**Why this scores:** single branch, no `if/else if` ladder, `yield` for laziness, culture-invariant formatting, guard clause.
**Say:** "the classic bug is `if (i % 3 == 0) ... else if (i % 5 == 0) ...` — the divisible-by-15 case is unreachable. Testing 15 first is the fix." **That is the whole point of the question.**

### P3. Palindrome, ignoring case, spaces and punctuation

```csharp
public static bool IsPalindrome(string s)
{
    ArgumentNullException.ThrowIfNull(s);
    int left = 0, right = s.Length - 1;
    while (left < right)
    {
        while (left < right && !char.IsLetterOrDigit(s[left])) left++;
        while (left < right && !char.IsLetterOrDigit(s[right])) right--;
        if (char.ToUpperInvariant(s[left]) != char.ToUpperInvariant(s[right])) return false;
        left++; right--;
    }
    return true;
}
```
**Complexity:** O(n) time, **O(1) space** — that's why this version beats `s.Where(char.IsLetterOrDigit).Reverse()`, which allocates.
**Say:** "`ToLower()` allocates; `ToUpperInvariant`/`OrdinalIgnoreCase` doesn't, and it isn't culture-sensitive. For Unicode-correct comparison I'd note that 'ß' vs 'ss' needs normalisation — and that I'd ask whether we need it rather than assume."

### P4. Round money safely

> "Given order line totals, compute the grand total to 2 decimal places. Prices are `decimal`."

```csharp
public static decimal Total(IEnumerable<decimal> lineTotals)
{
    // Round per line, THEN sum — the order matters and it's a business rule, not a detail
    return lineTotals
        .Select(t => Math.Round(t, 2, MidpointRounding.ToEven))
        .Sum();
}
```
**The answer they want:** "`decimal`, never `double` — `0.1 + 0.2 != 0.3` in binary floating point. Rounding is a **business rule**: round each line then sum, or sum then round, gives different answers, and accounting conventions prefer per-line (`MidpointRounding.ToEven`, banker's rounding). I'd get that in writing from the business, and I'd store the rounded value, not recompute it in three places."
**Follow-up:** "why `ToEven`?" → reduces aggregate bias; `AwayFromZero` is what most people expect; document the choice.
**This problem is a gift in a .NET interview** — it separates you from candidates who reach for `double` because it's shorter to type.

### P5. Count character frequencies

```csharp
public static IReadOnlyDictionary<char, int> Frequencies(string s)
{
    ArgumentNullException.ThrowIfNull(s);
    var counts = new Dictionary<char, int>(s.Length);
    foreach (var c in s) counts[c] = counts.GetValueOrDefault(c) + 1;
    return counts;
}
```
**Complexity:** O(n). **Say:** "`counts[c]++` would throw a `KeyNotFoundException` for a new key — `GetValueOrDefault` or `TryGetValue` is the correct idiom. And I'd reserve capacity, since resizing a dictionary is the main avoidable cost here."
**Follow-up:** "sorted output?" → `SortedDictionary` or `OrderBy`. "top 3?" → `PriorityQueue` (see `05B` §6.2).

---

## 2. Arrays & strings (P6–P11)

### P6. Find the duplicate(s) in an array of ints

> "Given an array of ints, return the first repeated value. Values are 1..n-1."

**Three levels — offer all three, then pick.** This is what a senior answer looks like:

```csharp
// Level 1 — brute force. Say it exists, then reject it.
// O(n²) time, O(1) space: for each i, scan j > i.

// Level 2 — hash set. The default answer.
public static int? FirstDuplicateHash(int[] numbers)
{
    var seen = new HashSet<int>(numbers.Length);
    foreach (var n in numbers)
        if (!seen.Add(n)) return n;      // Add returns false if already present
    return null;
}
// O(n) time, O(n) space

// Level 3 — Floyd's cycle detection (values are bounded 1..n-1, so the array IS a linked list)
// O(n) time, O(1) space — the "can you do it without extra memory" answer
public static int? FirstDuplicateNoSpace(int[] numbers)
{
    int slow = numbers[0], fast = numbers[0];
    do { slow = numbers[slow]; fast = numbers[numbers[fast]]; }
    while (slow != fast);

    slow = numbers[0];
    while (slow != fast) { slow = numbers[slow]; fast = numbers[fast]; }
    return slow;
}
```
**Say:** "level 2 is what I'd ship. Level 3 is the answer to the follow-up, and I'd only write it in production if memory were the actual constraint — it's harder to read and the correctness argument is non-obvious."
**That ranking is the lead signal.** Juniors write level 3 badly to impress; seniors ship level 2 and know level 3 exists.

### P7. Two Sum

```csharp
public static (int Left, int Right)? TwoSum(int[] numbers, int target)
{
    var seen = new Dictionary<int, int>(numbers.Length);      // value -> index
    for (int i = 0; i < numbers.Length; i++)
    {
        var need = target - numbers[i];
        if (seen.TryGetValue(need, out var j)) return (j, i);
        seen[numbers[i]] = i;
    }
    return null;
}
```
**O(n)** vs the O(n²) nested loop. **Say:** "the insight is to invert the question — instead of 'find two numbers that add to target', ask 'given this number, what's its complement, and have I already seen it?'"
**Follow-ups:** sorted input → two pointers, O(n) time O(1) space. Return all pairs → group + dedupe by value, careful with identical values. Overflow → "with ints, `target - numbers[i]` can overflow; with `long` it can't — I'd check the domain and use `long` arithmetic if the inputs are unbounded."

### P8. Group anagrams / group equivalent menu items

```csharp
public static IReadOnlyCollection<IReadOnlyList<string>> GroupAnagrams(IEnumerable<string> words)
{
    var buckets = new Dictionary<string, List<string>>(StringComparer.Ordinal);
    foreach (var word in words)
    {
        var key = string.Concat(word.OrderBy(c => c, StringComparer.Ordinal));   // canonical form
        if (!buckets.TryGetValue(key, out var list)) buckets[key] = list = [];
        list.Add(word);
    }
    return buckets.Values.Cast<IReadOnlyList<string>>().ToList();
}
```
**O(n · k log k)** where k = word length. **Say:** "the trick is a **canonical key** — reduce each word to something that's identical for all anagrams. Sorted characters is the usual choice; a count array (26 ints as a key) is O(n·k) and avoids the sort, which matters if the words are long."
**Follow-up — this is the good one:** "what's a stable string key for a 26-int count array?" → "a `ValueTuple` of counts won't hash well beyond 8 elements... in practice `string.Create` or a `record struct` with a custom `GetHashCode`. But honestly if the words are short, sorting is fine and I'd measure."

### P9. Apply a discount policy to order lines

> "Each line has quantity, unit price, and a category. Food items get 10% off; totals over 100 get a further 5%. Return the payable total."

```csharp
public record OrderLine(string Sku, string Category, int Quantity, decimal UnitPrice);

public static decimal Payable(IReadOnlyList<OrderLine> lines)
{
    if (lines is null || lines.Count == 0) return 0m;

    var subtotal = lines.Sum(l => Math.Round(l.UnitPrice * l.Quantity, 2, MidpointRounding.ToEven));

    var foodDiscount = lines
        .Where(l => string.Equals(l.Category, "Food", StringComparison.OrdinalIgnoreCase))
        .Sum(l => Math.Round(l.UnitPrice * l.Quantity * 0.10m, 2, MidpointRounding.ToEven));

    var afterFood = subtotal - foodDiscount;
    var orderDiscount = afterFood > 100m
        ? Math.Round(afterFood * 0.05m, 2, MidpointRounding.ToEven)
        : 0m;

    return afterFood - orderDiscount;
}
```
**What they're grading:** decimals not doubles, guard clauses, rounding at a stated point, `OrdinalIgnoreCase` for a category comparison, single pass per rule.
**The discussion that earns the offer:** "this is where I'd push back on the ticket. The rules are **data**, not code — a new campaign means a deploy. I'd model `IDiscountPolicy { bool AppliesTo(line/total); decimal Apply(...) }` and compose them, per `05A` C31. And the arithmetic — is the 5% on the discounted or original total? — is a question for the business, not a guess I make in code."

### P10. Remove duplicates in place from a sorted array

```csharp
// Sorted input, return the new length; the first k elements must hold the unique values
public static int RemoveDuplicates(int[] numbers)
{
    ArgumentNullException.ThrowIfNull(numbers);
    if (numbers.Length == 0) return 0;

    int write = 1;
    for (int read = 1; read < numbers.Length; read++)
        if (numbers[read] != numbers[write - 1]) numbers[write++] = numbers[read];

    return write;
}
```
**O(n) time, O(1) space** — the two-pointer write/read pattern.
**Say:** "the generic version is `numbers.Distinct().ToArray()`, O(n) time O(n) space. The in-place version exists because 'in place' is the constraint — and it only works because the array is sorted. **I'd say that out loud**: sortedness is what makes one pass sufficient."

### P11. Longest common prefix (menu code prefixes)

```csharp
public static string LongestCommonPrefix(IReadOnlyList<string> values)
{
    if (values is null || values.Count == 0) return string.Empty;

    var prefix = values[0];
    foreach (var value in values.Skip(1))
    {
        int i = 0;
        while (i < prefix.Length && i < value.Length &&
               prefix[i] == value[i]) i++;
        prefix = prefix[..i];
        if (prefix.Length == 0) return string.Empty;      // early exit
    }
    return prefix;
}
```
**O(S)** where S = total characters. **Say:** "the alternative is sort-then-compare-first-vs-last, O(n log n · k) — cleverer, but worse unless the strings are long. I'd write the simple one and mention the other."
**Follow-up:** "case-insensitive?" → compare with `char.ToUpperInvariant` or `string.Compare(..., OrdinalIgnoreCase)`; "culture-dependent?" → prefix logic should be ordinal, always.

---

## 3. Hashing & dictionaries (P12–P15)

### P12. First non-repeating character

```csharp
public static char? FirstUnique(string s)
{
    ArgumentNullException.ThrowIfNull(s);

    var counts = new Dictionary<char, int>(s.Length);
    foreach (var c in s) counts[c] = counts.GetValueOrDefault(c) + 1;
    foreach (var c in s) if (counts[c] == 1) return c;
    return null;
}
```
**O(n) time, O(k) space.** **Say:** "two passes — one to count, one to find the first with count 1. Preserving input order is why I return the character from the string rather than from the dictionary: `Dictionary` enumeration order is an implementation detail and I shouldn't rely on it."

### P13. Top N most frequent, from a large stream

```csharp
public static IReadOnlyList<KeyValuePair<string, int>> TopN(IEnumerable<string> stream, int n)
{
    var counts = new Dictionary<string, int>(StringComparer.Ordinal);
    foreach (var item in stream) counts[item] = counts.GetValueOrDefault(item) + 1;

    return counts
        .OrderByDescending(kv => kv.Value)
        .ThenBy(kv => kv.Key, StringComparer.Ordinal)     // deterministic tie-break — always add one
        .Take(n)
        .ToList();
}
```
**Say:** "if the distinct count is small, `OrderBy` is fine — O(d log d). If d is huge and n is small, a bounded min-heap is O(d log n) — I'd write that; the code is in my notes." **Then mention the deterministic tie-break** — that's a real correctness point for paginated results and it is a *detail nobody volunteers*.

### P14. Set operations on two id lists

```csharp
public static (IReadOnlyList<Guid> OnlyA, IReadOnlyList<Guid> Both, IReadOnlyList<Guid> OnlyB) Compare(
    IEnumerable<Guid> a, IEnumerable<Guid> b)
{
    var setA = new HashSet<Guid>(a);
    var setB = new HashSet<Guid>(b);

    var onlyA = new List<Guid>();
    var both  = new List<Guid>();
    foreach (var id in setA) (setB.Contains(id) ? both : onlyA).Add(id);

    var onlyB = setB.Where(id => !setA.Contains(id)).ToList();
    return (onlyA, both, onlyB);
}
```
**O(n + m)** — the whole point. **Say:** "`listA.Intersect(listB)` is already a set operation and is fine; `listA.Where(x => listB.Contains(x))` is the O(n·m) version and is the bug I actually see in code reviews. `HashSet` + `Contains` is the fix, and `HashSet<Guid>` also removes duplicates for free."
**Follow-up:** "what about string ids and case?" → "`new HashSet<string>(StringComparer.OrdinalIgnoreCase)` — the comparer must match the domain rule, and it must also match the database collation, or you get different answers in memory and in SQL. That mismatch is a genuinely nasty production bug."

### P15. Merge two collections by key

```csharp
public static IReadOnlyList<OrderSummary> Merge(
    IReadOnlyList<Order> orders, IReadOnlyDictionary<Guid, Customer> customers)
{
    var result = new List<OrderSummary>(orders.Count);
    foreach (var order in orders)
    {
        var name = customers.TryGetValue(order.CustomerId, out var c) ? c.Name : "unknown";
        result.Add(new OrderSummary(order.Id, name, order.TotalAmount));
    }
    return result;
}
```
**O(n + m)**; the naive version is a `FirstOrDefault` inside the loop → **O(n·m)**, which is exactly the *N+1 / missing join* problem from `05B` §3 in memory form.
**Say:** "the dictionary is a hash join. Same shape as the SQL join the database does for me — which is why the architectural fix at scale is to let the database do it, not to hand-roll it in C#."

---

## 4. Two pointers & sliding window (P16–P19)

### P16. Maximum sum of k consecutive orders

Full solution in `05B-DATA-STRUCTURES-AND-ALGORITHMS.md` §6.3. In the interview: write the O(n·k) version, say it's O(n·k), then slide.

```csharp
decimal window = 0;
for (int i = 0; i < k; i++) window += values[i];
decimal best = window;
for (int i = k; i < values.Count; i++)
{
    window += values[i] - values[i - k];
    if (window > best) best = window;
}
```

### P17. Longest substring without repeating characters

```csharp
public static int LongestUniqueSubstring(string s)
{
    ArgumentNullException.ThrowIfNull(s);

    var lastSeen = new Dictionary<char, int>(s.Length);
    int start = 0, best = 0;

    for (int end = 0; end < s.Length; end++)
    {
        if (lastSeen.TryGetValue(s[end], out var previous) && previous >= start)
            start = previous + 1;                 // jump the window past the duplicate

        lastSeen[s[end]] = end;
        best = Math.Max(best, end - start + 1);
    }
    return best;
}
```
**O(n) time, O(k) space.** **The bug to point out:** "if you reset `start = previous + 1` unconditionally, you move the window *backwards* for a duplicate that's outside it — hence the `previous >= start` check. That off-by-one is the whole difficulty of this problem."
**Follow-up:** "return the substring, not the length?" → track `bestStart` as well. "ASCII only?" → a `int[128]` beats the dictionary (no hashing, no allocation).

### P18. Container with most water (two pointers)

```csharp
public static int MaxArea(IReadOnlyList<int> heights)
{
    ArgumentNullException.ThrowIfNull(heights);
    int left = 0, right = heights.Count - 1, best = 0;

    while (left < right)
    {
        var area = Math.Min(heights[left], heights[right]) * (right - left);
        if (area > best) best = area;
        if (heights[left] < heights[right]) left++; else right--;   // move the shorter wall
    }
    return best;
}
```
**O(n)** vs O(n²) brute force. **The proof to say in one sentence:** "the area is limited by the shorter wall, so moving the taller one can never help — you can only improve by moving the shorter one, which means each step eliminates n candidate pairs."
**That one-sentence justification is what separates 'memorised the solution' from 'can reason'.** If you can't justify a greedy step, don't claim it.

### P19. Merge two sorted lists

```csharp
public static List<T> MergeSorted<T>(IReadOnlyList<T> a, IReadOnlyList<T> b, IComparer<T> comparer)
{
    var result = new List<T>(a.Count + b.Count);
    int i = 0, j = 0;

    while (i < a.Count && j < b.Count)
        result.Add(comparer.Compare(a[i], b[j]) <= 0 ? a[i++] : b[j++]);

    while (i < a.Count) result.Add(a[i++]);      // drain the remainder — don't forget this
    while (j < b.Count) result.Add(b[j++]);
    return result;
}
```
**O(n + m)** — which is why it beats `a.Concat(b).OrderBy(...)` (O((n+m) log(n+m))).
**Say:** "the property that makes this work is that both inputs are already sorted. The bug everyone writes is forgetting to drain the tail."

---

## 5. Sorting & searching (P20–P22)

### P20. Binary search — including the boundary variant

```csharp
// Find any index of target in a sorted array
public static int IndexOf(int[] sorted, int target)
{
    int low = 0, high = sorted.Length - 1;
    while (low <= high)
    {
        int mid = low + (high - low) / 2;        // NOT (low + high) / 2 — overflow
        if (sorted[mid] == target) return mid;
        if (sorted[mid] < target) low = mid + 1; else high = mid - 1;
    }
    return -1;
}

// Lower bound: first index where value >= target — the version that's actually useful
public static int LowerBound(IReadOnlyList<int> sorted, int target)
{
    int low = 0, high = sorted.Count;            // note: high is exclusive
    while (low < high)
    {
        int mid = low + (high - low) / 2;
        if (sorted[mid] < target) low = mid + 1; else high = mid;
    }
    return low;
}
```
**O(log n).** **The three things they listen for:**
1. "`low <= high` vs `low < high` is the classic off-by-one — the lower-bound form uses an exclusive `high` and `<`."
2. "`(low + high) / 2` overflows for large arrays — `low + (high - low) / 2` doesn't." *(Yes, it's an ancient bug — and mentioning it proves you read the reason, not just the code.)*
3. "In .NET I'd use `Array.BinarySearch` or `List.BinarySearch` — but they require the array to be sorted and they return the **bitwise complement** of the insertion point on failure, which surprises people. For a range query I'd use `lower_bound`-style code because 'first index ≥ target' is what callers actually need."

### P21. Merge overlapping delivery slots

```csharp
public record Slot(DateTimeOffset Start, DateTimeOffset End);

public static IReadOnlyList<Slot> Merge(IEnumerable<Slot> slots)
{
    ArgumentNullException.ThrowIfNull(slots);

    var ordered = slots.OrderBy(s => s.Start).ToList();     // O(n log n) — the key step
    if (ordered.Count == 0) return [];

    var merged = new List<Slot> { ordered[0] };
    foreach (var slot in ordered.Skip(1))
    {
        var last = merged[^1];
        if (slot.Start <= last.End)                                          // overlap (or touching)
            merged[^1] = last with { End = slot.End > last.End ? slot.End : last.End };
        else
            merged.Add(slot);
    }
    return merged;
}
```
**O(n log n)** dominated by the sort. **Say:** "sorting by start turns a pairwise overlap problem into an adjacent comparison — that's the whole trick, and it's why the sort is the algorithm, not a preliminary step."
**Follow-ups:** "touching slots — merge or not?" → a **business question**; answer with "depends whether `End` is inclusive — I'd ask." "n is a billion?" → "this belongs in the database or in a sweep over a sorted stream, not in memory."
**Edge case a lead mentions:** "with `DateTimeOffset`, slots in different offsets compare correctly by instant — with `DateTime` they don't. That's a real bug source in scheduling code."

### P22. Sort by multiple keys (the realistic one)

```csharp
public static IReadOnlyList<Order> SortForDisplay(IEnumerable<Order> orders) =>
    orders
        .OrderBy(o => o.Status, StatusOrderComparer.Instance)   // business priority, not alphabetical
        .ThenByDescending(o => o.CreatedAt)
        .ThenBy(o => o.Id)                                     // unique final key → stable paging
        .ToList();

public sealed class StatusOrderComparer : IComparer<string>
{
    private static readonly string[] Rank = ["Pending", "PaymentProcessed", "InventoryReserved", "Completed", "Failed"];
    public static readonly StatusOrderComparer Instance = new();

    public int Compare(string? x, string? y)
    {
        var ix = Array.IndexOf(Rank, x);
        var iy = Array.IndexOf(Rank, y);
        if (ix < 0) ix = int.MaxValue;      // unknown statuses sort last, never throw
        if (iy < 0) iy = int.MaxValue;
        return ix.CompareTo(iy);
    }
}
```
**Why this is a strong answer:** it handles a **business ordering that isn't alphabetical**, it never throws on unknown data (production reality), and the final `.ThenBy(o => o.Id)` makes paging correct. Say: "the last key is what stops `Skip`/`Take` from duplicating rows across pages when timestamps tie."

---

## 6. Stack, queue, heap (P23–P25)

### P23. Valid brackets

Full solution in `05B` §6.4. Narrate the stack discipline and mention the real uses (JSON/HTML/expression parsing).

### P24. Next greater element

```csharp
// For each element, the next element to its right that is greater (or -1)
public static int[] NextGreater(int[] values)
{
    var result = new int[values.Length];
    Array.Fill(result, -1);
    var stack = new Stack<int>();                  // holds INDICES, not values — monotonic decreasing

    for (int i = 0; i < values.Length; i++)
    {
        while (stack.Count > 0 && values[stack.Peek()] < values[i])
            result[stack.Pop()] = values[i];
        stack.Push(i);
    }
    return result;
}
```
**O(n)** — each index is pushed and popped at most once. **Say:** "the naive version is O(n²). The stack version works because each element's answer is determined the moment a larger one arrives, and the stack holds exactly the unresolved ones. Storing indices rather than values is what lets me write the answer back into the right slot."
**Real-world hook:** "this is the same shape as a stock-price 'days until a higher price' query, and it's asked because it tests whether you can hold an invariant in your head."

### P25. Min-stack / max-tracked structure

```csharp
public sealed class MinStack<T> where T : IComparable<T>
{
    private readonly Stack<(T Value, T Min)> _stack = new();

    public void Push(T value) =>
        _stack.Push((value, _stack.Count == 0 || value.CompareTo(_stack.Peek().Min) < 0
            ? value
            : _stack.Peek().Min));

    public T Pop() => _stack.Pop().Value;

    public T Min => _stack.Count > 0
        ? _stack.Peek().Min
        : throw new InvalidOperationException("Stack is empty.");
}
```
**O(1) for all operations** — the trick is that the minimum is **carried alongside** each element, so popping restores the previous minimum automatically. **Say:** "the alternative is a second stack, which is the same idea with more moving parts."
**Follow-up:** "when is this useful in real code?" → "sliding-window minimum for rate limiting or moving averages; and it's the shape of the `try/finally`-style state restoration pattern."

---

## 7. LINQ & EF Core problems (P26–P30) — the most likely section for you

This is where your 5 years of .NET pays off. These are the questions where you should be **clearly stronger than a front-end-led candidate**.

### P26. Group orders by customer and return the top spenders

```csharp
public record CustomerSpend(string CustomerId, int OrderCount, decimal Total, decimal Average);

public static IReadOnlyList<CustomerSpend> TopSpenders(IEnumerable<Order> orders, int take) =>
    orders
        .GroupBy(o => o.CustomerId)
        .Select(g => new CustomerSpend(
            g.Key,
            g.Count(),
            g.Sum(o => o.TotalAmount),
            g.Average(o => o.TotalAmount)))
        .OrderByDescending(c => c.Total)
        .ThenBy(c => c.CustomerId, StringComparer.Ordinal)
        .Take(take)
        .ToList();
```
**O(n + k log k).** Now the important follow-up: **"do the same in SQL, on the server."**

```csharp
var rows = await db.Orders
    .GroupBy(o => o.CustomerId)
    .Select(g => new CustomerSpend(
        g.Key,
        g.Count(),
        g.Sum(o => o.TotalAmount),
        g.Average(o => o.TotalAmount)))
    .OrderByDescending(c => c.Total)
    .Take(take)
    .ToListAsync(ct);
```
**Say:** "the same `GroupBy` on `IQueryable` becomes SQL `GROUP BY`, and only the aggregates cross the wire. The failure mode is calling `.ToListAsync()` **before** `GroupBy` — then all the rows come to the app and the grouping happens in memory. The rule: the `IQueryable` boundary is where performance is won or lost."
**And:** "`Average` on an empty group throws in LINQ-to-Objects; SQL returns `NULL`. That difference is a real bug when you move a query between the two."

### P27. Find the customer with the highest total spend

```csharp
// Readable
var top = orders
    .GroupBy(o => o.CustomerId)
    .Select(g => new { CustomerId = g.Key, Total = g.Sum(o => o.TotalAmount) })
    .MaxBy(x => x.Total);                       // .NET 6+

// One pass, no grouping materialisation — the "n is huge" answer
public static (string? CustomerId, decimal Total) HighestSpend(IEnumerable<Order> orders)
{
    var totals = new Dictionary<string, decimal>(StringComparer.Ordinal);
    foreach (var o in orders)
        totals[o.CustomerId] = totals.GetValueOrDefault(o.CustomerId) + o.TotalAmount;

    string? best = null;
    decimal max = decimal.MinValue;
    foreach (var (id, total) in totals)
        if (total > max || (total == max && string.CompareOrdinal(id, best) < 0))
            (best, max) = (id, total);

    return (best, best is null ? 0m : max);
}
```
**Why the second version exists:** `MaxBy` is O(n log n)-ish through the grouping plus a second pass; the dictionary version is **O(n)** and single-pass. **Say:** "I'd write `MaxBy` — it's clearer. I'd only hand-roll the second one if profiling said the query ran millions of times a day. **Clarity is the default; optimisation needs a measurement.**"
**Note the tie-break** — with equal totals the winner must be deterministic. That's the detail that makes the answer senior.

### P28. Distinct + paged + stably ordered

```csharp
public static IReadOnlyList<MenuItem> PageDistinct(
    IQueryable<MenuItem> source, int page, int size)
{
    ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
    ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(size, 100);   // cap the page size — always

    return source
        .Where(m => m.IsActive)
        .Select(m => new MenuItem(m.Id, m.Name, m.Price))         // project before Distinct
        .Distinct()
        .OrderBy(m => m.Name, StringComparer.Ordinal)              // ordinal, not culture
        .ThenBy(m => m.Id)                                        // unique tie-break
        .Skip((page - 1) * size)
        .Take(size)
        .ToList();
}
```
**The four things they're grading:** validated page/size, page size cap (a DoS guard), projection before `Distinct` (smaller rows), and a unique final sort key (correct paging). **Say all four.**
**Follow-up:** "`Distinct` on `IQueryable` and a projection — does it work?" → "`Distinct` on a projection of scalars translates to SQL `DISTINCT`. On a projection to a custom type it needs value equality to be meaningful; with SQL it depends what EF can push down. This is one of the cases I'd verify with `ToQueryString()` **before** shipping, not after."

### P29. Fix the slow query (given broken code)

```csharp
// ❌ The version they hand you
public async Task<List<OrderDto>> GetOrdersAsync(int page)
{
    var orders = await _db.Orders.ToListAsync();                                   // 1. whole table
    var customers = await _db.Customers.ToListAsync();                              // 2. whole table
    return orders
        .Where(o => o.Status == "Pending")                                          // 3. filter after materialising
        .Skip(page * 20).Take(20)
        .Select(o => new OrderDto(o.Id, customers.First(c => c.Id == o.CustomerId).Name, o.TotalAmount))  // 4. O(n·m)
        .ToList();
}
```

**Name the five defects in order** — that ordering is the answer:

1. **Two full table scans** — `ToListAsync()` before any filter. At 2M orders this is the whole problem.
2. **Filtering and paging in memory** — `Skip/Take` after materialisation means the database returns everything and the app throws it away.
3. **O(n·m) lookup** — `customers.First(...)` inside `Select` is a linear scan per order.
4. **No projection pushdown** — it materialises full entities to use 3 fields.
5. **No cancellation token, no `AsNoTracking`, no stable ordering** — `Skip/Take` without an `OrderBy` gives a **non-deterministic** page.

```csharp
// ✅ The fix
public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(int page, int size, CancellationToken ct)
{
    return await _db.Orders
        .AsNoTracking()
        .Where(o => o.Status == OrderStatus.Pending)
        .OrderByDescending(o => o.CreatedAt).ThenBy(o => o.Id)     // stable paging
        .Skip((page - 1) * size).Take(size)
        .Select(o => new OrderDto(o.Id, o.Customer.Name, o.TotalAmount))   // one SQL JOIN, 3 columns
        .ToListAsync(ct);
}
```
**Then say:** "and I'd confirm with `ToQueryString()` that it's one `SELECT ... JOIN ... ORDER BY ... OFFSET/FETCH`, and check the plan has an index on `(Status, CreatedAt)`."
**That closing line is the difference between "I know EF" and "I have owned a production database".**

### P30. Idempotent POST

> "The client retries. We must not create two orders."

```csharp
public async Task<IActionResult> CreateOrderAsync(
    CreateOrderRequest request,
    [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
    CancellationToken ct)
{
    if (string.IsNullOrWhiteSpace(idempotencyKey))
        return BadRequest("Idempotency-Key header is required.");

    // 1. Fast path: have we seen this key?
    var existing = await _db.IdempotencyRecords
        .AsNoTracking()
        .FirstOrDefaultAsync(r => r.Key == idempotencyKey, ct);
    if (existing is not null)
        return Ok(existing.Response);                 // replay the original result

    var order = Order.Create(request.CustomerId, request.Items);

    // 2. The unique index on Key is what actually makes this correct under concurrency —
    //    the fast path is an optimisation, not the guarantee.
    _db.Orders.Add(order);
    _db.IdempotencyRecords.Add(IdempotencyRecord.For(idempotencyKey, order.Id, response));
    try
    {
        await _db.SaveChangesAsync(ct);
    }
    catch (DbUpdateException ex) when (ex.IsUniqueViolation())
    {
        // Another request won the race — return its result
        var winner = await _db.IdempotencyRecords.AsNoTracking()
            .FirstAsync(r => r.Key == idempotencyKey, ct);
        return Ok(winner.Response);
    }

    return CreatedAtRoute("GetOrder", new { id = order.Id }, response);
}
```
**The three sentences that carry this answer:**
1. "**The database unique constraint is the correctness mechanism** — an in-memory check is a race by construction, and with three pods it's three separate check-then-act windows."
2. "The key must be **scoped to the operation and the customer**, and stored with a TTL — unbounded idempotency keys become a table that grows forever."
3. "The response is stored, not recomputed, so a replay returns the identical body and status — which is what 'idempotent' means to an HTTP client."

`IsUniqueViolation()` maps SQL Server error 2601/2627 (and Postgres `23505`) — say the numbers if you know them; if not, say "the provider-specific unique-constraint error code, isolated in one extension method so it isn't scattered around the codebase."

---

## 8. Async & concurrency problems (P31–P33)

### P31. Call N downstream services in parallel, bounded

```csharp
public async Task<IReadOnlyList<PriceResult>> GetPricesAsync(
    IReadOnlyList<Guid> productIds, CancellationToken ct)
{
    using var throttle = new SemaphoreSlim(8);            // never fan out unbounded — you'll get 429s / socket exhaustion

    var tasks = productIds.Select(async id =>
    {
        await throttle.WaitAsync(ct);
        try
        {
            return await _pricing.GetPriceAsync(id, ct);
        }
        finally
        {
            throttle.Release();                            // always release, even on exception
        }
    });

    return await Task.WhenAll(tasks);
}
```
**The four points to make:**
- **Sequential `await` in a loop is O(n) round trips** — 100 × 80 ms = 8 s where 800 ms would do.
- **Unbounded parallelism is worse** — socket exhaustion, throttling, and a downstream outage you caused.
- **`Task.WhenAll` surfaces only the first exception** when awaited; if you need all failures, inspect `whenAllTask.Exception?.InnerExceptions`.
- **`Parallel.ForEachAsync(..., new ParallelOptions { MaxDegreeOfParallelism = 8 })`** is the modern one-liner for the same thing.

**Follow-up:** "one of them fails — do you fail the batch?" → "depends on the contract. For a price display, partial results with a `null`/error marker beat a 500. I'd make that explicit in the return type, not in a silent catch."

### P32. Thread-safe cache with the `GetOrAdd` trap

```csharp
public sealed class PriceCache(IPricingClient client)
{
    private readonly ConcurrentDictionary<Guid, Lazy<Task<decimal>>> _cache = new();

    public Task<decimal> GetAsync(Guid productId, CancellationToken ct) =>
        _cache.GetOrAdd(productId,
            id => new Lazy<Task<decimal>>(() => client.GetPriceAsync(id, ct))).Value;
}
```
**Why `Lazy<Task<T>>`:** "`GetOrAdd` may invoke the factory more than once under contention. A plain `Task<decimal>` factory means the HTTP call can fire twice for the same key — a real problem for a paid or rate-limited API. `Lazy` makes the factory run once and the losing thread gets the winner's task."
**The caveats a lead adds:** "the cache is per-instance — three pods means three caches; if that's wrong, it belongs in Redis. And entries never expire here, so in production I'd use `IMemoryCache` with `SizeLimit` and absolute expiration instead of hand-rolling this."
**Saying "in production I'd use the framework, here's the mechanism questions they're testing" is a perfect answer shape** — it demonstrates both depth and pragmatism.

### P33. Retry with exponential backoff and jitter

```csharp
public static async Task<T> RetryAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    int maxAttempts = 4,
    TimeProvider? clock = null,
    CancellationToken ct = default)
{
    clock ??= TimeProvider.System;
    var delay = TimeSpan.FromMilliseconds(200);

    for (int attempt = 1; ; attempt++)
    {
        try
        {
            return await operation(ct);
        }
        catch (Exception ex) when (attempt < maxAttempts && IsTransient(ex))
        {
            // Full jitter — prevents a thundering herd of retries after an outage
            var jittered = TimeSpan.FromMilliseconds(Random.Shared.NextDouble() * delay.TotalMilliseconds);
            await Task.Delay(jittered, clock, ct);
            delay *= 2;
        }
    }
}

private static bool IsTransient(Exception ex) => ex switch
{
    HttpRequestException { StatusCode: HttpStatusCode.TooManyRequests or
                                        HttpStatusCode.ServiceUnavailable or
                                        HttpStatusCode.GatewayTimeout } => true,
    TaskCanceledException => false,     // caller cancelled — never retry
    TimeoutException => true,
    _ => false
};
```
**What they're grading:**
1. **Only retry transient failures** — retrying a 400 or a `NullReferenceException` amplifies the failure.
2. **Do not retry a cancelled operation** — `OperationCanceledException` must propagate.
3. **Jitter** — without it, every client retries in lock-step and re-DDoSes the recovering service. **This is the point of the question.**
4. **Bounded attempts with a cap.**
5. **`TimeProvider`** so the delay is testable without real waiting.

**Then the lead answer:** "in production I wouldn't write this — I'd use Polly via `Microsoft.Extensions.Http.Resilience` (`AddStandardResilienceHandler()` gives retry + circuit breaker + timeout + bulkhead with sane defaults), because a hand-rolled retry is usually missing jitter, a circuit breaker, or `Retry-After` handling. I'd write this version only to show I know what the library is doing."
**Bonus:** "honour the `Retry-After` header when the server sends one — a blind backoff that's shorter than the server's instruction makes things worse."

---

## 9. SQL problems (P34–P36)

> Three live-coding SQL problems. The **question-and-answer** bank — fundamentals, indexes and execution plans,
> transactions, EF Core ↔ SQL, security, scaling — is **`05D-SQL-AND-DATA-ACCESS-FOR-DOTNET.md`**.

### P34. Top 5 customers by revenue in the last 30 days

```sql
SELECT TOP (5)
       o.CustomerId,
       COUNT(*)        AS OrderCount,
       SUM(o.TotalAmount) AS Revenue
FROM   dbo.Orders AS o
WHERE  o.Status = 'Completed'
  AND  o.CreatedAt >= DATEADD(DAY, -30, SYSUTCDATETIME())
GROUP BY o.CustomerId
ORDER BY Revenue DESC, o.CustomerId ASC;      -- deterministic tie-break
```
**Supporting index:** `CREATE INDEX IX_Orders_Status_CreatedAt ON dbo.Orders (Status, CreatedAt) INCLUDE (CustomerId, TotalAmount);`
**Say:** "the index is chosen from the query — equality column first (`Status`), range column second (`CreatedAt`), and `CustomerId`/`TotalAmount` as `INCLUDE` so the index covers the query and no key lookup is needed."
**Follow-up:** "the numbers must match the finance report exactly" → "then the definition of revenue lives in a view or a published data model, not in each query. Two queries that disagree about refunds and rounding is a classic month-end bug."

### P35. Fix this slow query (deep paging)

```sql
-- ❌ page 5000: SQL reads and discards 100,000 rows
SELECT Id, CustomerId, TotalAmount, CreatedAt
FROM   dbo.Orders
WHERE  Status = 'Completed'
ORDER BY CreatedAt DESC
OFFSET 100000 ROWS FETCH NEXT 20 ROWS ONLY;

-- ✅ keyset: O(log n) seek, stable under inserts
SELECT TOP (20) Id, CustomerId, TotalAmount, CreatedAt
FROM   dbo.Orders
WHERE  Status = 'Completed'
  AND (CreatedAt < @cursorCreatedAt
       OR (CreatedAt = @cursorCreatedAt AND Id < @cursorId))
ORDER BY CreatedAt DESC, Id DESC;
```
**Say:** "`OFFSET` cost grows with the page number — the engine has no choice but to produce and discard the rows. Keyset paging is a seek on the index, so page 5000 costs the same as page 1. The trade-off is no random page access — which is fine for infinite scroll and wrong for a page-numbered admin grid."
**The index that makes it work:** `(Status, CreatedAt DESC, Id DESC) INCLUDE (CustomerId, TotalAmount)`.

### P36. `LEFT JOIN` that silently became an inner join

```sql
-- ❌ the WHERE clause filters out customers with no orders
SELECT c.Id, c.Name, COUNT(o.Id) AS Orders
FROM   dbo.Customers c
LEFT JOIN dbo.Orders o ON o.CustomerId = c.Id
WHERE  o.Status = 'Completed'          -- NULLs from the LEFT JOIN fail this predicate
GROUP BY c.Id, c.Name;

-- ✅ move the predicate into the ON clause
SELECT c.Id, c.Name, COUNT(o.Id) AS Orders
FROM   dbo.Customers c
LEFT JOIN dbo.Orders o ON o.CustomerId = c.Id AND o.Status = 'Completed'
GROUP BY c.Id, c.Name;

-- ✅ or filter for the NULLs explicitly when you genuinely want "customers with no completed orders"
-- ... WHERE o.Id IS NULL
```
**Say:** "a `WHERE` predicate on the right-hand table of a `LEFT JOIN` eliminates the unmatched rows, because `NULL = 'Completed'` is unknown, not true. So the join degenerates to an inner join. The predicate belongs in `ON`."
**This single question distinguishes people who have debugged SQL from people who have written SQL.** Have it ready.

---

## 10. The 10 things interviewers flag in live code

Avoid these visibly and you gain a full point:

1. **`double` for money** — use `decimal`.
2. **`ToList()` before `Where`** on an `IQueryable`.
3. **`foreach` with `List.Contains`** inside — quadratic.
4. **No guard clauses** — null/empty handled 20 lines deep instead of at the top.
5. **Magic numbers and strings** — `if (status == "Pending")` repeated ten times.
6. **A 50-line method** — extract named helpers; the interviewer scores readability, not line count.
7. **No edge-case discussion** — empty, single element, duplicates, null, max.
8. **Silent `catch { }`** — always either handle, log and rethrow, or don't catch.
9. **`async void` / `.Result` / `.Wait()`** — instant red flags in a .NET loop.
10. **Ignoring the `CancellationToken`** in an async signature — you were given `ct`; use it.

### And the five habits that earn points

1. Say "let me restate that" before you start.
2. Say the brute force and its complexity **before** optimising.
3. Name your data structure out loud: "I'll use a HashSet because membership is O(1)".
4. Write one test case per boundary in a `[Theory]` and say it out loud.
5. Finish with: "the complexity is X, and at 100× the data I'd move this to the database / use keyset paging / add an index."

---

## 11. The 45-minute assessment round — a plan

| Minutes | Do this | Never do this |
|---|---|---|
| 0–4 | Restate, ask 3 questions, write 2 examples + 1 edge case | Start typing |
| 4–7 | Brute force + complexity, out loud | Apologise for the brute force |
| 7–10 | Name the optimisation and its complexity | Claim O(n) you can't justify |
| 10–28 | Write it, top-down, narrating decisions | Go silent for a stretch |
| 28–35 | Walk the examples through the code, fix your own bug | Say "looks right" |
| 35–42 | Edge cases + a couple of `[Theory]` rows + complexity recap | Add speculative features |
| 42–45 | "What I'd do differently at scale / with concurrency / for testability" | Trail off |

**If you finish early:** add tests, then *ask what they'd change*. "Is there a constraint I've missed?" is scored as collaboration.

---

## 12. Drill schedule (works with `13-FIRST-90-DAYS.md`)

| Week | Daily 45 min |
|---|---|
| 1 | P1–P11 out loud, recorded. Focus on narration, not speed. |
| 2 | P12–P25, plus the `05B` §4 trigger drill (10 min). |
| 3 | P26–P36 — **your strongest section**, so aim for flawless. |
| 4 | Two full timed mocks: pick 2 unseen problems from §2–§8, run the §11 plan on camera, then grade yourself against the §10 list. |

**The grading rubric to apply to your own recording:**

- Did I ask clarifying questions? (yes/no)
- Did I state the brute force before optimising? (yes/no)
- Did I go more than 45 seconds without speaking? (yes/no)
- Did I name the complexity unprompted? (yes/no)
- Did I find at least one of my own bugs? (yes/no)
- Would I approve this in a PR? (honest answer)

Six yeses = you pass the round. Anything less, re-record the same problem tomorrow. **Re-recording the same problem is more valuable than doing a new one** — it's the narration you're training, not the solutions.

---

## 30-second version

Coding rounds are scored on **problem solving, code quality, testing and communication** — not on getting a memorised optimal solution. Run the 8-step protocol every time: restate, ask, example, brute force + complexity, insight, code, walk-through, complexity recap. Your differential as a .NET lead is section 7 (LINQ/EF) and section 9 (SQL) — be flawless there and the algorithm problems can be merely good. Avoid the ten flagged habits in §10, and finish every answer with a sentence about what you'd change at 100× the scale.
