# Chapter 5A — C# Language & OOP Fundamentals (the gate questions)

> **Read this before `06-INTERVIEW-QUESTIONS.md` §6.** Chapter 5 covers ASP.NET Core *depth*. This chapter covers the
> language-level questions that come **before** the framework questions — and they are asked even at lead level,
> because they are the cheapest way for an interviewer to test whether "5 years of .NET" is real.

---

## 0. Why a lead still gets asked "what is boxing?"

There are three tiers of technical question in a Microsoft fullstack lead loop:

| Tier | Example | What it's really testing | Where it's covered |
|---|---|---|---|
| **Gate** | "struct vs class", "boxing", "`==` vs `Equals`", "abstract vs interface" | Is the 5 years real? Are you a copy-paste developer? | **This chapter** |
| **Depth** | DI lifetime bug, `IQueryable` vs `IEnumerable`, `SaveChanges` + outbox | Do you understand the runtime, not just the syntax? | `05-ASPNET-CORE-SENIOR.md` |
| **Judgement** | "how would you migrate this", "how do you decide" | Can you lead? | `08`–`13` |

You cannot skip tier 1 on the way to tier 3. **A lead who fumbles `record` vs `class` loses the room in 20 seconds**,
because the interviewer then questions every claim in your stories.

### The answer shape for every question in this chapter

```
1. Direct answer — one sentence, the definition.
2. Concrete example — preferably from YOUR repo (this chapter gives you one per topic).
3. The trade-off or the trap — this is the part that separates 5 years from 1 year repeated 5 times.
```

Example, complete answer to "struct vs class":

> "A class is a reference type allocated on the heap with reference semantics; a struct is a value type copied on
> assignment, and it's for small, immutable, short-lived data. In our food-delivery API, `Order` and `OrderItem` are
> classes because they have identity and are tracked by EF Core — copying them on every assignment would be wrong.
> A `Money` or `Quantity` struct would be reasonable. The trap is a struct larger than ~16 bytes or one with mutable
> fields: you start copying megabytes and get bugs where a change to a copy silently doesn't propagate."

That's 45 seconds and it wins the question. Use the same shape everywhere.

---

## 1. Value vs reference types (C1–C8)

### C1. What's the difference between a value type and a reference type?

| | Value type (`struct`, `int`, `decimal`, `DateTime`, `enum`, `bool`) | Reference type (`class`, `string`, `array`, `delegate`, `record` class) |
|---|---|---|
| Storage | Inline where declared (stack for locals, **inside the object** for fields) | Object on the heap, variable holds a reference |
| Assignment | Copies the whole value | Copies the reference (both point at one object) |
| Equality default | Field-by-field value equality | Reference identity |
| Can be null | Only as `Nullable<T>` / `T?` | Yes |
| Inheritance | None (can implement interfaces) | Yes |
| GC | Not separately collected (unless boxed) | Collected |
| Typical use | Small immutable data, `Money`, `Point`, `Result` | Entities, services, anything with identity or behaviour |

**The one-liner:** "Value types are copied, reference types are aliased."

### C2. "Value types live on the stack, reference types on the heap" — true?

**False as a rule**, and saying it confidently is a common way to lose credibility. The truth:

- A **local** value type in a method is on the stack.
- A value-type **field of a class** lives **inside that object on the heap** — `Order.TotalAmount` (a `decimal`) lives on the heap with the `Order`.
- A value type **captured in a closure** is hoisted into a compiler-generated class → heap.
- A value type **boxed** is copied onto the heap.
- An **`async` method's** locals are lifted into a state machine struct/class → may be heap.
- **Array of structs** — the structs are inline in the array, on the heap.

**Say this instead:** "The stack/heap split is an implementation detail. What matters is copy semantics vs reference semantics. Storage location follows from where the value is declared, not from its type."

### C3. When would you choose a `struct` over a `class`?

Choose a struct when **all** of these hold:
1. Small — rule of thumb ≤ 16–24 bytes (a `decimal` is 16 bytes by itself).
2. Immutable — `readonly struct` with `init`/get-only members.
3. Short-lived / not boxed often.
4. No inheritance needed.
5. Value equality is genuinely the semantics you want.

```csharp
// Reasonable struct: 16 bytes, immutable, value semantics
public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0m, currency);
    public Money Add(Money other) =>
        other.Currency == Currency
            ? this with { Amount = Amount + other.Amount }
            : throw new InvalidOperationException($"Cannot add {other.Currency} to {Currency}.");
}

// Bad struct: 40+ bytes, mutable, gets copied on every call
public struct OrderDraft
{
    public Guid Id;
    public string CustomerId;
    public List<OrderItem> Items;   // a reference field in a struct — smell
    public decimal Total;
}
```

**Trap to mention:** a mutable struct in a `List<T>` gives you a **copy** on `list[0].Total = 5` — it compiles and does nothing. That single sentence proves you have debugged real code.

### C4. What is boxing and why is it a performance problem?

Boxing = copying a value type into a heap object (`object`, `interface`) to satisfy a reference-typed slot. Unboxing = the cast back, which includes a type check and a copy.

```csharp
int n = 42;
object boxed = n;        // allocation + copy on the heap
int back = (int)boxed;   // unbox + copy
```

```csharp
// Compiles. 1000 allocations. A classic review finding.
var list = new ArrayList();
for (int i = 0; i < 1000; i++) list.Add(i);

// Generic collections are specialised by the JIT — no boxing.
var list2 = new List<int>();
for (int i = 0; i < 1000; i++) list2.Add(i);
```

**Where boxing bites in real code:**
- Non-generic collections (`ArrayList`, `Hashtable`) — banned in modern code.
- Formatting: `string.Format("{0}", 5)` / `$"{5}"` — historically boxed; `DefaultInterpolatedStringHandler` (C# 10+) removed most of it.
- Enum as dictionary key via non-generic APIs, `Enum.HasFlag` (boxed `Enum`), `Enum.Parse`.
- Interfaces on structs — `struct` implementing an interface and used via the **interface** boxes; calling through the **constrained generic** does not.
- `params object[]` on logging calls.

**Interview-grade closing line:** "The fix is generics — `List<T>`, `Dictionary<TKey,TValue>`, and constrained generic methods let the JIT emit specialised code with no boxing. If I see `ArrayList` or `object[]` in a code review, I ask why."

### C5. `class` vs `record` vs `struct` vs `record struct`

| | `class` | `record` (class) | `struct` | `record struct` |
|---|---|---|---|---|
| Kind | Reference | Reference | Value | Value |
| Equality | Reference | **Value** (all members) | Value (opt-in best practice) | Value |
| `ToString` | Type name | **Generated, readable** | Type name | Generated |
| `with` expression | No | **Yes** | No | **Yes** |
| `Deconstruct` | No | **Yes** (positional) | No | Yes (positional) |
| Mutable | Yes | Positional = init-only | Yes | Yes (or `readonly record struct`) |
| Use for | Entities, services, anything with identity | **DTOs, messages, events, value objects, API contracts** | Small value data | Small value data needing value equality |

```csharp
public record OrderPlaced(Guid OrderId, string CustomerId, decimal Total, DateTimeOffset OccurredAt);
public record struct Money(decimal Amount, string Currency);

var original = new OrderPlaced(orderId, "cust-1", 42.50m, DateTimeOffset.UtcNow);
var corrected = original with { Total = 40.00m };   // copy with one change — ideal for domain events
```

**Why this matters in your repo:** the delivery app publishes integration events. Those events are *values* — if two
`OrderPlaced` messages have identical content they are interchangeable, and `record` gives you correct equality,
readable logs, and cheap `with`-copies for versioning.

**The caveat (say it):** `record` equality is **shallow** — two records holding the same `List<OrderItem>` reference are equal if the list reference matches; mutate the list inside and both records' "value" changed. Records are for immutable data; a record holding a mutable collection is a half-truth.

### C6. `const` vs `readonly` vs `static readonly` vs `init`

```csharp
public class Order
{
    public const decimal VatRate = 0.20m;              // compile-time constant, baked into callers (incl. other assemblies)
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);  // runtime, one per type
    public readonly Guid Id;                            // set in constructor only
    public string Status { get; init; } = "Pending";    // settable at construction / object initialiser only
    public required string CustomerId { get; init; }    // C# 11+: compiler forces the initialiser
}
```

| | Bound when | Scope | Inter-assembly versioning |
|---|---|---|---|
| `const` | Compile time | Static | **Value is copied into the consumer's IL** — changing it needs a rebuild of *consumers*. Never `const` a value that could change (prices, URLs). |
| `static readonly` | Static ctor / type load | Static | Read at runtime — safe to change |
| `readonly` (instance field) | Constructor | Per instance | — |
| `init` | Object initialiser / constructor | Per instance | Best of both for immutable-ish DTOs |

**The killer follow-up you should pre-empt:** "`const` across assembly boundaries is a versioning bug — if
`AssemblyA` defines `public const int Max = 10` and `AssemblyB` uses it, `10` is inlined into `B`'s IL. Ship a new `A`
with `Max = 20` and `B` still uses 10 until you rebuild it. `static readonly` avoids that."

### C7. `ref`, `out`, `in` — and when not to use them

```csharp
void Swap(ref int a, ref int b) => (a, b) = (b, a);          // read + write
bool TryParse(string s, out int value) { ... }               // write-only, must be assigned
void Log(in ReadOnlySpan<char> message) { ... }              // read-only reference, no copy
```

- `ref` — caller's variable is the parameter. Use for genuine mutation of the caller's state.
- `out` — must be assigned before return; the `TryXxx` pattern. Use when "success" and "the value" share one call.
- `in` — passes a large struct by reference without allowing mutation; avoids the copy. Only a win for structs; irrelevant for classes.

**When not to:** in public/domain APIs they make the call site noisier and break fluent composition. Prefer returning a result type (`record struct ParseResult(bool Ok, int Value)` or `TryParse`). Reserve `ref`/`in` for hot paths you have profiled — `Span<T>` APIs are the legitimate modern use.

**Note:** `in` parameters are not enforced const-correctness — a method can copy the `in` parameter and mutate the copy. It's a performance hint, not a guarantee.

### C8. `==` vs `Equals` vs `ReferenceEquals` vs `IEquatable<T>`

```csharp
object a = "hello", b = "hello";
a == b                // true — string overloads == to do value comparison
object.ReferenceEquals(a, b)  // usually true here only because of string interning

var p1 = new Product { Id = 1 };
var p2 = new Product { Id = 1 };
p1 == p2              // false — default reference comparison
p1.Equals(p2)         // false — Product doesn't override Equals
```

Rules:
- `==` is an **operator** — overloadable, resolved at **compile time** by static type. For `string` it's value comparison (special case). For `object`-typed variables it's reference comparison — a genuine bug source.
- `Equals(object)` is **virtual** — override it together with `GetHashCode()` (equal objects must have equal hash codes, or `Dictionary`/`HashSet` silently break).
- `IEquatable<T>.Equals(T)` avoids boxing and is what `Dictionary<K,V>` uses when available. **Always implement it when you override `Equals`.**
- `ReferenceEquals` — identity, cannot be overridden.
- The combined advice: **for anything used as a dictionary key or in a `HashSet`, implement `IEquatable<T>` + `Equals` + `GetHashCode` — or use a `record`/`record struct`, which generates all three correctly.**

```csharp
public record ProductKey(int Id, string Region);      // free correct equality + hash
var cache = new Dictionary<ProductKey, Product>();
```

**The classic trap:** `HashSet<OrderItem>` where `OrderItem` doesn't override equality → you "de-duplicate" and nothing happens, because every instance is distinct. And: `GetHashCode()` based on a **mutable** field → the object gets lost in the dictionary after mutation.

---

## 2. Strings (C9–C14)

### C9. Why are strings immutable, and what does that cost?

Every `string` operation creates a new string. Consequences:
- `s += x` in a loop is **O(n²)** — it copies the whole accumulated string every iteration.
- You can cache hash codes (`string` caches its hash), so strings are safe as dictionary keys.
- Thread-safe without locks.
- Concatenation of a **fixed** set of literals is folded by the compiler; `a + b + c` on ≤4 strings compiles to a single `string.Concat` call — that's *not* the problem. The problem is the **loop**.

```csharp
// BAD — allocates 1+2+3+...+n chars
var report = "";
foreach (var order in orders)
    report += $"{order.Id}:{order.TotalAmount}\n";

// GOOD — amortised O(n), one buffer
var sb = new StringBuilder(orders.Count * 40);   // capacity hint avoids regrowth
foreach (var order in orders)
    sb.Append(order.Id).Append(':').Append(order.TotalAmount).AppendLine();
var report = sb.ToString();
```

### C10. When do you *not* use `StringBuilder`?

- Single concatenation, or a handful of pieces: `string.Concat` / interpolation is faster — `StringBuilder` allocates its own object and buffer.
- Building with a known, small number of parts in an expression: `$"{a}-{b}"`.
- Hot path building into a rented buffer: `string.Create` or `ValueStringBuilder` (internal) / `Span<char>` with `StringBuilder` avoided entirely.
- **This is the trade-off sentence:** "`StringBuilder` pays off from roughly 5–10 concatenations or any loop; below that it's pure overhead."

### C11. Ways to parse/manipulate strings cheaply?

```csharp
// Allocation-free slicing — no substring copies
ReadOnlySpan<char> path = "/api/orders/42";
var idPart = path[path.LastIndexOf('/')..];        // ReadOnlySpan<char>

// Split without allocating strings
foreach (var range in path.Split('/')) { ... }      // .NET 8+ Split into ranges / Span

string.Join(',', ids);                              // better than manual separators
decimal.TryParse(span, CultureInfo.InvariantCulture, out var price);   // Span overloads exist
```

Mention `Span<T>` only if asked — it's the answer to "how do you make this faster without changing the design", and it belongs in a senior answer, not a junior one.

### C12. `IsNullOrEmpty` vs `IsNullOrWhiteSpace` vs `null` vs `""` vs `string.Empty`

- `string.Empty` and `""` are the **same interned object** — pick one by style; `string.Empty` communicates intent.
- `IsNullOrEmpty` — null or zero length.
- `IsNullOrWhiteSpace` — also rejects `"   "`. **Use this for user input**, because `" "` is not a valid customer name.
- With nullable reference types on, prefer `string?` for "may be absent" and `string` for "never null", so most `IsNullOrEmpty` checks disappear from the domain.

**Your repo speaks to this:** `Product.Name` is declared `public string Name { get; set; } = string.Empty;` — that `= string.Empty` exists *because* nullable reference types are on and the model binder can leave the property null. The **stronger** fix for a request DTO is `required` + `[Required]` validation, which is `05` §6 territory.

### C13. `ToLower()` vs `StringComparison.OrdinalIgnoreCase` — why does it matter?

```csharp
// BAD: allocates a lowercase copy of both strings, culture-sensitive (Turkish 'i' problem)
if (name.ToLower() == "admin") { }

// GOOD: culture-independent, allocation-free, shows intent
if (string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase)) { }

// For user-facing sort/compare, be explicit about culture:
string.Compare(a, b, StringComparison.CurrentCulture);
```

- **Ordinal** = byte/char by char. Correct for identifiers, keys, protocol values, `StartsWith`/`EndsWith` on paths.
- **CurrentCulture** = correct for display sorting.
- The `StartsWith(string)` overload is **culture-sensitive by default**, which is a known performance sink and a correctness bug on Linux vs Windows. **Always pass the `StringComparison`.**
- In EF Core, `ToLower()` in a `Where` kills the index. Use a **case-insensitive collation** (`SQL_Latin1_General_CP1_CI_AS`, the default) or a normalized column. This is a real query-performance answer.

### C14. Interpolation vs `+` vs `string.Format` — what does `$""` compile to?

Since C# 10, `$"..."` compiles to a `DefaultInterpolatedStringHandler` — in most cases it becomes a single `string.Concat`/`TryFormat` path with **no `object[]` allocation and no boxing of value types**. So:

- Interpolation is the default choice — readable **and** fast.
- `string.Format` with a format string stored in a variable (localisation) still needs it.
- `+` is fine for two or three parts and for compile-time constants.
- The only real anti-pattern is `+=` in a loop (C9) and logging arguments that are always evaluated.

```csharp
_logger.LogDebug("Order {OrderId} total {Total}", order.Id, order.TotalAmount);  // structured, no boxing
_logger.LogDebug($"Order {order.Id}");                                          // pays interpolation cost even when Debug is off
```

**That logging line is a great "senior" detail** — it's a real finding in code review.

---

## 3. Nullability & modern syntax (C15–C22)

### C15. What do nullable reference types (NRT) actually do?

They are a **compile-time analysis only**. No runtime change, no null checks emitted (except `!` suppression removing warnings, and `required`/`[NotNull]` metadata).

```csharp
public string Name { get; set; } = null!;   // "I promise this is set by EF Core / the binder"
public string? Nickname { get; set; }        // may legitimately be null
```

Warnings you should name: `CS8618` (non-nullable not initialised), `CS8602` (possible null dereference), `CS8600` (converting null literal to non-nullable).

**The mature answer:** "NRT turns null bugs into compile-time warnings in *my* code, but they don't protect me at the boundaries — model binding, JSON deserialisation, and the database can all still hand me null. So NRT is the first line; validation (`[Required]`, FluentValidation) is the boundary; and `??`/`?.` at the edges is the third. I never treat a green build with NRT as proof of non-null."

### C16. `?.`, `??`, `??=`, `!`

```csharp
var city = order?.Customer?.Address?.City ?? "unknown";   // null-conditional + null-coalescing
_cache ??= new Dictionary<string, Product>();              // assign if null (lazy init)
var total = items?.Sum(i => i.Price) ?? 0m;
string.IsNullOrEmpty(name) ? "anonymous" : name;           // ternary for display
var safe = maybeNull!;                                     // suppress — use only at a boundary you control
```

**The trap:** `order?.Customer.GetName()` — if `order` is null the whole expression short-circuits, but if `Customer` is null it throws. The `?.` only protects the *one* link it follows. And `a?.B.C` vs `a?.B?.C` is the classic review comment.

### C17. `required` and `init` — what problem do they solve?

```csharp
public class CreateOrderRequest
{
    public required string CustomerId { get; init; }   // compiler error if not set at construction
    public required List<CreateOrderItem> Items { get; init; }
    public string? Notes { get; init; }
}
```

- `required` (C# 11) moves the "must be set" contract **into the type**, not a comment or a runtime check.
- `init` makes the property settable during construction/object-initialiser only → immutable afterwards → safe for records, DTOs, and domain events that get passed between threads.
- Together they replace: ctor + 8 parameters, or a mutable bag of `{ get; set; }` that callers can corrupt later.

**Trade-off:** `required` + `init` fights `System.Text.Json` deserialisation only in edge cases (needs a ctor or `JsonConstructor`), and EF Core materialisation ignores `init` (it uses the backing field) — which is exactly why EF entities usually stay `{ get; set; }` while DTOs are `init`-only.

### C18. Show me pattern matching.

```csharp
// Type + property pattern on an order state
var fee = order switch
{
    { Status: "Completed", TotalAmount: > 100m } => 0m,
    { Status: "Failed" }                          => 0m,
    { Items.Count: 0 }                            => throw new InvalidOperationException("Empty order"),
    { TotalAmount: < 10m }                        => 2.50m,
    _                                             => 1.00m
};

// Relational + logical patterns
string Classify(decimal total) => total switch
{
    < 0 => "invalid",
    >= 0 and < 20 => "small",
    >= 20 and < 100 => "medium",
    _ => "large"
};

// List patterns (.NET 7+)
if (parts is [var head, .. var rest]) { ... }
if (path is [_, "orders", var id]) { ... }
```

**The high-value sentence:** "`switch` *expressions* over a sealed hierarchy or a value enum are how I keep decision logic in one place and let the compiler warn me when a new case appears. `order.Status` being a **string** in our delivery app is where that breaks — see §9."

### C19. Primary constructors and collection expressions (C# 12)

```csharp
// Primary constructor on a class (C# 12) — parameters are in scope for the whole body
public class OrderService(AppDbContext db, IPublishEndpoint bus, ILogger<OrderService> log)
{
    public async Task<Order> CreateAsync(CreateOrderRequest req, CancellationToken ct)
    {
        log.LogInformation("Creating order for {Customer}", req.CustomerId);
        ...
    }
}

// Collection expressions — the repo already uses this
public List<OrderItem> Items { get; set; } = [];
int[] numbers = [1, 2, 3];
ReadOnlySpan<byte> header = [0x89, 0x50, 0x4E, 0x47];
```

**Note the interview nuance:** primary-constructor parameters are **not** properties and are **not** injected by the DI container—the container still resolves the constructor you declare. Mixing primary constructors with `record` semantics confuses people; state the difference plainly.

### C20. `var` vs `dynamic` vs `object`

| | Type checking | Use |
|---|---|---|
| `var` | Compile-time, fully static (inferred) | Default for obvious right-hand sides |
| `object` | Compile-time, needs cast | Legacy interop, non-generic APIs |
| `dynamic` | **Runtime** (DLR), no IntelliSense, no compile check | COM interop, `ExpandoObject` JSON edge cases — **never in a normal domain** |

`var` is not "untyped". `dynamic` is a runtime binder — it throws `RuntimeBinderException` in production where a compile error belonged. If you say "we don't use `dynamic`" you get credit.

### C21. File-scoped namespaces, global usings, top-level statements

```csharp
// FullOrderModels.cs — file-scoped namespace, no braces, one less indent level
namespace FoodDelivery.Api.Models;

public class Order { ... }
```

- Global usings (`<ImplicitUsings>enable</ImplicitUsings>`) inject `System`, `System.Linq`, `System.Collections.Generic`, `System.Threading.Tasks` etc. — that's why `Program.cs` files start without them.
- Top-level statements: `Program.cs` is a script with implicit `Main`; the compiler generates the class. **Consequence worth naming:** top-level statements make `WebApplicationFactory<Program>` tests need `public partial class Program { }` at the bottom — a real gotcha in integration testing.

### C22. `nameof`, `default`, `checked`/`unchecked`

```csharp
throw new ArgumentNullException(nameof(request));     // rename-safe, no magic strings
int x = default;                                       // 0
Order? o = default;                                    // null
if (o is null) { }                                     // preferred over == null (no operator overload surprises)

checked { var big = int.MaxValue + 1; }                // throws OverflowException
var wrapped = unchecked(int.MaxValue + 1);             // default behaviour: wraps
decimal.Multiply(a, b);                                // decimal always throws on overflow regardless of checked
```

`nameof` in `ArgumentException`/`PropertyChanged`/log templates is free correctness — mention it as a review standard.

---

## 4. Generics & collection interfaces (C23–C28)

### C23. Generics: what do constraints and variance mean?

```csharp
public T Load<T>(string key) where T : class, new()      // reference type + parameterless ctor
TResult Project<T, TResult>(T source) where T : IEntity
where TResult : notnull

public interface IRepository<T> where T : class, IEntity
{
    Task<T?> GetAsync(Guid id, CancellationToken ct);
    void Add(T entity);
}
```

- Constraints let you call members of the constraint type and give the JIT the info to avoid boxing (`where T : struct` → specialised, no boxing).
- `new()` constraint ⇒ `Activator.CreateInstance` avoided.
- **Variance**: `IEnumerable<out T>` is covariant — `IEnumerable<Order>` is assignable to `IEnumerable<object>`. `Action<in T>` is contravariant. `IList<T>` is **invariant** because it's read-write; that's why `IReadOnlyList<out T>` exists and why you should expose it.

**The practical payoff to state:** "`IReadOnlyList<Order>` on a return type means the caller can't `Add` to my internal list. That's covariance *and* encapsulation in one signature change — it's a one-line PR that prevents a class of bug."

### C24. `IEnumerable` vs `ICollection` vs `IList` vs `List<T>` — what should a method return?

| Interface | Gives caller | Return it when |
|---|---|---|
| `IEnumerable<T>` | foreach only, lazy | You're a stream/pipeline; caller shouldn't count or index |
| `IReadOnlyCollection<T>` | foreach + `Count` | Read-only set with a known size — **good default for API results** |
| `IReadOnlyList<T>` | + indexer | Read-only ordered collection — **best default for "a list of things"** |
| `ICollection<T>` | + `Add`/`Remove` | Caller genuinely needs to mutate |
| `IList<T>` | + index set/Insert | Rare outside builders |
| `List<T>` (concrete) | everything | Local variables, fields, internal implementations |

**Rule:** accept the **most general** (`IEnumerable<T>`), return the **most restrictive useful** (`IReadOnlyList<T>`), store the **concrete** (`List<T>`).

**Trap to mention:** returning `IEnumerable<T>` from a method that has a `using`/`DbContext` scope inside means the enumeration happens **after the context is disposed** → `ObjectDisposedException` or a lazy query against a closed connection. That's the #1 deferred-execution bug.

### C25. `IEnumerable<T>` vs `IQueryable<T>` — the most important LINQ distinction

| | `IEnumerable<T>` | `IQueryable<T>` |
|---|---|---|
| Where the work runs | **In memory**, in your process | **At the provider** (SQL Server) |
| What's passed | Delegates (`Func<T,bool>`) | **Expression tree** (`Expression<Func<T,bool>>`) — inspected and translated |
| Cost of `.Where(...)` | Iterates objects | Adds to the SQL `WHERE` |
| Risk | Pulling a whole table then filtering | Untranslatable expression → exception, or silent client evaluation |

```csharp
// GOOD: one query, filter + projection + paging all in SQL
var page = await db.Orders
    .AsNoTracking()
    .Where(o => o.CustomerId == customerId && o.Status == "Pending")
    .OrderByDescending(o => o.CreatedAt)
    .Skip((pageNumber - 1) * pageSize).Take(pageSize)
    .Select(o => new OrderSummary(o.Id, o.TotalAmount, o.CreatedAt))
    .ToListAsync(ct);

// BAD: loads the whole table into memory, then filters there
var all = await db.Orders.ToListAsync(ct);              // ← AsEnumerable() equivalent
var page2 = all.Where(o => o.CustomerId == customerId).ToList();
```

**The three sentences that make this a senior answer:**
1. "`ToList()`/`AsEnumerable()` is the boundary — everything after it runs in memory."
2. "`AsNoTracking()` on read-only queries halves the tracking overhead; I use it as a rule, not an exception."
3. "Projection with `Select` is the single biggest win — entities carry every column, DTOs carry five."

See `05-ASPNET-CORE-SENIOR.md` §7 for the N+1 and tracking depth.

### C26. Deferred execution and the double-enumeration bug

```csharp
IEnumerable<Order> GetPending() => db.Orders.Where(o => o.Status == "Pending");   // no SQL yet

var q = GetPending();              // nothing has run
if (q.Any()) { ... }               // SQL #1
foreach (var o in q) { ... }       // SQL #2 — same data, second roundtrip
var n = q.Count();                 // SQL #3
```

Rules:
- LINQ-to-objects: **enumerating twice re-runs the delegate**. If the source is a `Random`, a clock, or a consumed stream, the two runs **differ**.
- LINQ-to-EF: **enumerating twice re-queries the database**.
- Fix by materialising once: `var list = q.ToList();` — and say "I materialise at the boundary I control, not inside every helper."

**Follow-up you should be ready for:** "is `Count() > 0` or `Any()` better?" → `Any()` — it can stop after one row (SQL `EXISTS` / `TOP 1`), whereas `Count()` scans. On `IQueryable` both are SQL, but `EXISTS` is still plan-cheaper. And on an `IList`, `Count()` is O(1) while `Any()` is also O(1) — the real argument is **`Any()` communicates intent and never scans**.

### C27. How does `yield return` work, and when do you use it?

```csharp
public static IEnumerable<Order> InBatches(IEnumerable<Order> source, int size)
{
    var batch = new List<Order>(size);
    foreach (var o in source)
    {
        batch.Add(o);
        if (batch.Count == size) { yield return batch; batch = new List<Order>(size); }
    }
    if (batch.Count > 0) yield return batch;
}
```

- The compiler turns the method into a **state machine**; nothing runs until the consumer calls `MoveNext()`.
- **Pros:** O(1) memory, streaming, composable — ideal for large exports, file parsing, paging APIs.
- **Cons (say these):** the body runs on the **consumer's** thread and time — an exception surfaces at `MoveNext`, not at call time; you cannot `try/catch` around the call and expect to catch it; an iterator that touches a `DbContext` must be consumed inside its scope; you can't use `yield` in a method with `try/catch` around a `yield` (only `try/finally` is allowed).
- `IAsyncEnumerable<T>` + `await foreach` is the async version — see C64.

### C28. What problem does `Span<T>` / `Memory<T>` solve?

A `Span<T>` is a **ref struct** — a pointer + length over existing memory (array, stack, or unmanaged), *not an allocation*. It lets you slice without copying and keep the type safety.

```csharp
static bool TryGetOrderId(ReadOnlySpan<char> path, out Guid id)
{
    id = default;
    var last = path.LastIndexOf('/');
    return last >= 0 && Guid.TryParse(path[(last + 1)..], out id);   // zero allocations
}
```

Limits (state these — it's the tell): ref structs can't be a field of a class, can't be boxed, can't cross an `await` boundary, can't be used in `async` methods or lambdas. `Memory<T>` is the heap-friendly sibling for async.

**Where a lead says this matters:** high-throughput string parsing in a middleware hot path (route parsing, log parsing) — not in CRUD controllers. Don't optimise the wrong thing; mention that you'd profile first.

---

## 5. OOP pillars (C29–C40)

### C29. The four pillars in one answer, using your own code

| Pillar | What it means concretely | In your repo |
|---|---|---|
| **Encapsulation** | Hide state, expose behaviour that preserves invariants | `AppDbContext` — you never touch `DbConnection`; `SaveChangesAsync` is the only way to commit |
| **Abstraction** | Expose *what*, hide *how* | `IPublishEndpoint.Publish<T>()` — MassTransit decides RabbitMQ/Kafka/in-memory |
| **Inheritance** | Reuse + substitutability | `ControllerBase` → `OrderController`; `BackgroundService` → your consumer host |
| **Polymorphism** | One call site, many implementations, chosen at runtime | `IPaymentProcessor` resolved from DI; `IEntityTypeConfiguration<T>` applied by EF Core |

**The senior add-on:** "The pillar that matters most in production is encapsulation — because it is the only one that *prevents* bugs rather than organising code. Inheritance is the one I use least."

### C30. What does encapsulation really mean? (It is not getters and setters)

Real encapsulation = **the object can only exist in valid states**, enforced by the object itself.

```csharp
// Not encapsulated — the invariants live in the caller (or nowhere)
public class Order
{
    public decimal TotalAmount { get; set; }     // anyone can set -5
    public List<OrderItem> Items { get; set; } = [];
}

// Encapsulated — the invariant travels with the data
public class Order
{
    private readonly List<OrderItem> _items = [];
    public IReadOnlyList<OrderItem> Items => _items;
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify a submitted order.");
        _items.Add(item);
        RecalculateTotal();
    }

    public void MarkPaid()
    {
        if (Status != OrderStatus.Pending) throw new InvalidOperationException($"Invalid transition {Status} → Paid");
        Status = OrderStatus.Paid;
    }

    private void RecalculateTotal() => TotalAmount = _items.Sum(i => i.UnitPrice * i.Quantity);
}
```

**Your repo's current state (be honest about it — it scores well):** `Order` in `DeliverFoodApp/src/FoodDelivery.Api/Models/DomainModels.cs` has
`public List<OrderItem> Items { get; set; } = [];`, `public decimal TotalAmount { get; set; }`, and
`public string Status { get; set; } = "Pending";`. Every invariant is public and unchecked — the controller sets
`TotalAmount` and `Status` directly. That's fine for a demo of messaging, but it's the first thing I'd change in a real
system, and being able to say *why* is worth more than having written perfect code.

### C31. Inheritance vs composition — the rule

**Prefer composition.** Use inheritance only for a genuine **is-a** with **substitutable** behaviour (LSP-safe).

```csharp
// Inheritance misuse: a hierarchy for something that varies on one axis
abstract class OrderProcessor { public abstract void Process(Order o); }
class StandardOrderProcessor : OrderProcessor { ... }
class DiscountedOrderProcessor : OrderProcessor { ... }
class GiftOrderProcessor : OrderProcessor { ... }
// -> discounted + gift? The tree explodes combinatorially.

// Composition: behaviour as a collaborator
public interface IDiscountPolicy
{
    decimal Apply(decimal total);
    bool AppliesTo(Order order);
}
public class OrderPricing(IEnumerable<IDiscountPolicy> policies)
{
    public decimal Price(Order order) =>
        policies.Where(p => p.AppliesTo(order))
                .Aggregate(order.TotalAmount, (total, p) => p.Apply(total));
}
```

**The smell test list:** hierarchy depth > 2; a base class with 30 members where subclasses use 3; `virtual` members overriding `virtual` members; an "abstract" class with no abstract members; a child that throws `NotSupportedException` for an inherited member (⇒ LSP violated).

**Stack Overflow's canonical answer, if you want the recognised line:** "inheritance is a *white-box*, compile-time relationship — the subclass sees internals of the base; composition is *black-box*, runtime, and swappable."

### C32. Abstract class vs interface — the decision table

| | Abstract class | Interface |
|---|---|---|
| State (fields) | **Yes** | No (C# 8 default interface methods can have bodies but no instance fields) |
| Constructors | Yes | No |
| Multiple inheritance | No (single base) | **Yes** |
| Access modifiers | Any | Public (pre-C# 8) |
| Shared implementation | Yes, with `virtual`/concrete members | Only via default implementations (use sparingly) |
| Versioning | **Breaking** to add a member | Non-breaking with a *default* implementation; breaking otherwise |
| Represents | "is a", partial implementation, a template | "can do / is capable of", a contract |

**Decision rule:** "Interface by default — for DI seams, for testability, and because a type can only have one base class, so reserving it is expensive. Abstract class when subclasses must share **state and partial implementation** (e.g. `BackgroundService`, `ControllerBase`, `DbContext`). And if I need both, an interface plus an optional abstract base (`IOrderRepository` + `OrderRepositoryBase`) gives callers the seam and implementers the shared code."

**Follow-up: default interface methods.** "C# 8 lets an interface carry a body. It's a versioning tool — add a method without breaking implementers. But I use it rarely: it's not virtual dispatch for state and it encourages hidden behaviour. If it needs logic and state, it's an abstract class."

**Follow-up: explicit interface implementation.** `void IDisposable.Dispose()` — used when a member should be callable only through the interface, e.g. `Dispose()` not polluting your public API.

### C33. `virtual`, `override`, `new`, `sealed`

```csharp
public class Base { public virtual string Name() => "base"; }
public class Override : Base { public override string Name() => "override"; }
public class Hider  : Base { public new string Name() => "hider"; }

Base b1 = new Override(); Console.WriteLine(b1.Name());   // "override"  — runtime dispatch
Base b2 = new Hider();    Console.WriteLine(b2.Name());   // "base"      — static binding! the new member is invisible
```

**The sentence to have ready:** "`new` hides, it does not override. Through a base reference you still get the base behaviour — so `new` is almost always an accident. If I see `new` on a method in review, I assume a design problem. `sealed override` is the opposite and is useful: it stops further overriding and lets the JIT devirtualise the call."

### C34. Give me a Liskov substitution violation.

"LSP says a subtype must be usable anywhere the base type is expected **without the caller knowing**."

```csharp
public class OrderRepository
{
    public virtual void Add(Order order) { ... }
    public virtual Order Get(Guid id) { ... }
}

// Violation: promises the base contract but breaks it at runtime
public class ReadOnlyOrderArchive : OrderRepository
{
    public override void Add(Order order) =>
        throw new NotSupportedException("Archive is read-only.");
}

void Import(OrderRepository repo, IEnumerable<Order> orders)
{
    foreach (var o in orders) repo.Add(o);   // blows up depending on which subtype arrived
}
```

**The fix:** don't inherit write capability — split the contracts (`IOrderReader` / `IOrderWriter`), so `ReadOnlyOrderArchive : IOrderReader` and the compiler enforces the truth at the call site.

**Second classic:** `Square : Rectangle` where setting `Width` also sets `Height`, so `Assert(r.Width == 5)` fails after `r.Height = 10`. Both are worth having.

### C35. Compile-time vs runtime polymorphism

| | Compile-time (static) | Runtime (dynamic) |
|---|---|---|
| Mechanisms | Method **overloads**, generic constraints, `static` dispatch | `virtual`/`override`, interface dispatch, `dynamic` |
| Resolved by | The **static types** at the call site | The **actual runtime type** |

```csharp
void Send(Order o) { }
void Send(OrderSummary s) { }        // overload — chosen at compile time by the variable's declared type

IOrderHandler h = factory.Create();  // runtime — chosen by the object's real type
h.Handle(order);
```

**Interview follow-up:** "which is faster?" — virtual dispatch costs a vtable lookup and blocks inlining; modern JITs do **guarded devirtualisation** when the call site is monomorphic, so in practice the difference is negligible. Choose for design, then measure.

### C36. Coupling and cohesion — how do you assess them quickly?

- **Cohesion** — does everything in this class/assembly change for the same reason? Low cohesion looks like a `Helpers` class or a 900-line `OrderService` with pricing, email, PDF and SFTP in it.
- **Coupling** — how many things break when this changes? Measure it cheaply: count the `using` directives and the constructor parameters.

**The heuristic you can quote:** "I look at the constructor. More than ~4 collaborators means the class is doing too much, or it's a coordinator that should delegate. And I look at the `using` list — if a single class references EF Core, `HttpClient`, `ILogger`, `IMapper` *and* a PDF library, it's coupled to five reasons to change."

### C37. Anemic vs rich domain model — which do you use?

- **Anemic**: entities are property bags; logic lives in services. Fast to start, matches EF nicely, and it's what most .NET codebases are.
- **Rich**: entities enforce their own invariants; services orchestrate. More code, fewer invalid states.

**The answer that works in an interview:** "Neither is a religion. I use a rich model **only where there are real invariants** — money, state transitions, quantities, pricing — and anemic everywhere else. Our delivery `Order` should be rich: the `Status` transitions (`Pending → PaymentProcessed → InventoryReserved → Completed/Failed`) are exactly the kind of rule that must not be settable from outside, otherwise two code paths will eventually disagree. A `Product` with a name and a price can stay a property bag forever."

**Follow-up: "isn't that over-engineering?"** → "Then the state machine lives in the controller or a service — and it will be duplicated. The test is: can a new developer put an order into an impossible state? If yes, the model is too weak."

### C38. Tell, don't ask

```csharp
// ASK — caller reads state, decides, writes state (race + duplication)
if (order.Status == "Pending" && order.Items.Count > 0) { order.Status = "PaymentProcessed"; }

// TELL — the object owns the decision
order.MarkPaymentProcessed();     // throws if the transition is illegal
```

The `if` above is where double-payment bugs live: two requests both read `Pending`, both proceed. Moving the check inside the entity, plus a concurrency token (see `05` §8), is the fix.

### C39. SOLID — with an example for each and the honest caveat

| | Principle | One-line meaning | Where it shows up in your code / what to say |
|---|---|---|---|
| **S** | Single Responsibility | One reason to change | `Program.cs` at 250 lines is the classic SRP failure — it has composition root *and* endpoint definitions. I'd split into `Program.cs` + endpoint mapping extensions per feature. |
| **O** | Open/Closed | Add behaviour without editing existing code | `switch (order.Status)` in three controllers: adding a status means editing all three. Replace with a `IOrderStateMachine` or a strategy keyed by status. |
| **L** | Liskov | Subtype usable as base | The `NotSupportedException` override in C34. |
| **I** | Interface Segregation | No client should depend on members it doesn't use | A 15-method `IRepository<T>` where a controller needs one. Split into `IRead`/`IWrite` or, better in modern .NET, **skip the interface and use the `DbContext` directly** where appropriate. |
| **D** | Dependency Inversion | Depend on abstractions | `IPublishEndpoint`, `ILogger<T>`, `TimeProvider` — the app depends on interfaces, the composition root picks implementations. |

**The honest caveat (this is the lead-level bit):** "SOLID is a set of heuristics, not laws. Applied mechanically it produces `IOrderService`, `IOrderServiceFactory`, `IOrderServiceProvider` and an unreadable codebase. My rule: **Dependency Inversion and Single Responsibility earn their keep almost always; Interface Segregation and Open/Closed only where the variation is real.** I'd rather have a 200-line `OrderService` with a clear boundary than seven one-method classes."

### C40. DRY, KISS, YAGNI, and the rule of three

- **DRY** — "every piece of knowledge has one authoritative representation". **Not** "no two lines look alike": two pieces of code that *look* the same but change for different reasons are **not** duplication — extracting them creates coupling. Say: "I look for duplication of *knowledge*, not of syntax. A premature abstraction is more expensive than a duplicated 6-line block."
- **Rule of three** — abstract the third time you write it, not the second.
- **KISS** — the simplest thing that satisfies the current, stated requirement.
- **YAGNI** — don't build the plugin system for the second client that doesn't exist yet.

**Lead-level framing:** "YAGNI is not an excuse for bad structure. You can't YAGNI away an architectural boundary that is expensive to add later — but you can YAGNI away the generic abstraction nobody asked for."

---

## 6. Design patterns that actually come up (C41–C52)

You will not be asked to implement GoF patterns from memory. You **will** be asked "which patterns do you use and why".
Answer with the problem, not the name. Here are the ones worth knowing with a repo hook.

### C41. Strategy
**Problem:** several interchangeable algorithms chosen at runtime.
```csharp
public interface IPaymentProcessor { Task<PaymentResult> ChargeAsync(Order o, CancellationToken ct); }
public class StripeProcessor : IPaymentProcessor { ... }
public class InvoiceProcessor : IPaymentProcessor { ... }

// Composition root picks by configuration; the caller never switches
services.AddKeyedScoped<IPaymentProcessor, StripeProcessor>("card");
```
**In your repo:** the consumers are effectively strategies — different reactions to different message types.
**Trade-off:** one class per strategy; don't use it when there are two cases and no sign of a third.

### C42. Factory (Method / Abstract / DI-registered factory)
**Problem:** the caller shouldn't know which concrete type to construct — often because the choice depends on data.
```csharp
public interface IMessagePublisherFactory { IMessagePublisher Create(Transport t); }
// or the lighter, DI-friendly form: a Func<> registered in the container
services.AddSingleton<Func<Transport, IMessagePublisher>>(sp => t => t switch
{
    Transport.RabbitMq => sp.GetRequiredService<RabbitMqPublisher>(),
    Transport.Kafka    => sp.GetRequiredService<KafkaPublisher>(),
    _ => throw new ArgumentOutOfRangeException(nameof(t))
});
```
**Note:** with DI, a `Func<>` or `AddKeyedScoped` usually replaces a hand-written factory class — say that and you sound current.

### C43. Builder
**Problem:** an object with many optional parts, constructed step by step and validated on build.
```csharp
var query = new OrderQueryBuilder()
    .ForCustomer(customerId)
    .WithStatus(OrderStatus.Pending)
    .CreatedAfter(DateTimeOffset.UtcNow.AddDays(-7))
    .Page(1, 25)
    .Build();
```
**Where you already use it:** `WebApplicationBuilder`, `Host.CreateDefaultBuilder`, EF's `ModelBuilder`, MassTransit's `AddMassTransit(x => x.UsingRabbitMq(...))`. Recognising that **the framework itself is built on these patterns** is the answer that reads as experienced.

### C44. Decorator
**Problem:** add cross-cutting behaviour around an existing implementation without touching it.
```csharp
public class CachedOrderReader(IOrderReader inner, IMemoryCache cache, TimeProvider clock) : IOrderReader
{
    public async Task<Order?> GetAsync(Guid id, CancellationToken ct) =>
        await cache.GetOrCreateAsync($"order:{id}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
            return await inner.GetAsync(id, ct);
        });
}
// registration: layer it, don't replace it
services.AddScoped<IOrderReader>(sp => new CachedOrderReader(
    sp.GetRequiredService<SqlOrderReader>(), sp.GetRequiredService<IMemoryCache>(), TimeProvider.System));
```
**Your repo already uses decoration:** ASP.NET Core middleware is a chain of decorators around the request delegate.
**Trade-off:** layering is invisible at the call site (debuggers are confusing) and ordering matters. Prefer this over modifying the inner class — but if you need 4 decorators, consider a pipeline abstraction.

### C45. Adapter
**Problem:** your domain depends on your own interface; a third-party API has a different shape.
```csharp
public interface IPaymentGateway { Task<PaymentResult> ChargeAsync(Money amount, string token, CancellationToken ct); }

public class StripeGatewayAdapter(StripeClient client) : IPaymentGateway
{
    public async Task<PaymentResult> ChargeAsync(Money amount, string token, CancellationToken ct)
    {
        var resp = await client.ChargeAsync(amount.Amount, amount.Currency, token, ct);
        return resp.Paid ? PaymentResult.Success(resp.Id) : PaymentResult.Declined(resp.FailureReason);
    }
}
```
**The lead point:** "the adapter is where the vendor's SDK stops. Nothing above it knows Stripe exists — which is what makes the vendor replaceable and the tests fake-able."

### C46. Facade
**Problem:** one call from the caller, five subsystems underneath.
**In your repo:** `IPublishEndpoint.Publish(...)` is a facade over serialisation, topology, retries, and transport. `SaveChangesAsync` is a facade over unit-of-work commit, transaction, and change tracking.
**Distinguish from Adapter:** adapter changes **shape** for one interface; facade **simplifies** a set of subsystems.

### C47. Repository + Unit of Work — the .NET-specific controversy
```csharp
public interface IUnitOfWork { Task<int> SaveChangesAsync(CancellationToken ct); }

public class AppDbContext(DbContextOptions<AppDbContext> o) : DbContext(o), IUnitOfWork
{
    public DbSet<Order> Orders => Set<Order>();
}
```
**The answer that scores:** "`DbContext` **is** a repository and a unit of work. Adding `IOrderRepository` on top is usually ceremony that hides `IQueryable`, forces you to re-implement `Include`/`ThenInclude`, and breaks the best feature of EF (composable queries). I use a repository only when (a) I need to unit-test domain logic without a database, or (b) I genuinely might swap the store — and I keep the interface **query-shaped** (`IQueryable<Order> Query()`) rather than `GetAll()`-shaped. Otherwise a thin abstraction over `DbContext` per feature slice is enough."

If the interviewer is dogmatic about repositories, don't fight — state the trade-off, then say what you'd do given their constraints. That is the lead behaviour.

### C48. Options pattern
**Problem:** strongly typed configuration, validated at startup instead of `configuration["..."]` string lookups everywhere.
```csharp
public sealed class JwtOptions
{
    public const string Section = "Jwt";
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string SigningKey { get; init; }
    public int ExpiryMinutes { get; init; } = 60;
}

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.Section))
    .ValidateDataAnnotations()
    .Validate(o => o.SigningKey.Length >= 32, "Signing key must be at least 32 chars")
    .ValidateOnStart();                     // fail fast at boot, not on first request
```
**In your repo:** `Program.cs` reads `jwtIssuer`, `jwtAudience` and a signing key from configuration inline. Wrapping them in `JwtOptions` + `ValidateOnStart()` is a 20-line PR that turns a production failure into a startup failure. Have that ready as your improvement suggestion — it's concrete and uncontroversial.

### C49. Chain of Responsibility
**Problem:** a request passes through an ordered set of handlers, each may act or pass on.
**In your repo:** the ASP.NET Core middleware pipeline, and MassTransit's consumer/middleware pipeline (`UseMessageRetry`, `UseOutbox`).
**Say:** "the container's equivalent in the front end is Angular's HTTP interceptor chain — same pattern, different language. Being able to name the same pattern in both stacks is a strong fullstack signal."

### C50. Observer / publish–subscribe
**Problem:** one event, many independent reactions, no compile-time coupling.
**In your repo:** `OrderPlaced` published via MassTransit; `PaymentConsumer`, `InventoryConsumer` react independently; SignalR pushes to the client; Angular/React subscribe to the UI stream.
**Lead point:** "pub/sub buys loose coupling and *costs* you a distributed transaction problem — which is exactly why the outbox pattern (see `05` §11) exists. Naming both the benefit and the tax is the answer."

### C51. Singleton — and why it's usually an anti-pattern
"It's a design pattern only when you need exactly one instance **and** global access **and** you can't use DI. In .NET with DI we get the single instance for free via `AddSingleton`, without the static state, without the test-hostility. A hand-written `public static Instance` is a code smell in a DI app because it makes lifetime invisible and can capture dependencies of the wrong lifetime — that's the captive-dependency bug in `05` §2."

### C52. Mediator / CQRS (and when not to)
"In-process mediator (`MediatR`) turns a controller into `_mediator.Send(new GetOrdersQuery(...))` with one handler per request. The win is a consistent pipeline for validation/logging/caching and tiny units of code. The cost is indirection — you can no longer "find usages" of a method, and a stack trace becomes five frames of framework. I use it in a large codebase with many contributors or when I need pipeline behaviours; for a 6-controller API I'd use plain services. For CQRS, I separate the *read* model from the *write* model but I keep one database until read load actually justifies a projection store — see `09` §9."

---

## 7. Exceptions & resource handling (C53–C58)

### C53. When do you throw, and when do you return a result?

- **Throw** for *exceptional* — a broken invariant, a missing required resource, a programming error, an infrastructure failure you cannot handle locally.
- **Return** (`bool TryX` / `Result<T>`) for *expected* outcomes — validation failures, "not found" on a lookup, a payment declined. Exceptions are expensive (stack capture) and are invisible in a signature.
- **Never** throw for normal control flow in a loop.

```csharp
// custom exception carrying the info the handler needs
public sealed class OrderStateException(Guid orderId, string from, string to)
    : Exception($"Invalid transition for order {orderId}: {from} → {to}")
{
    public Guid OrderId { get; } = orderId;
}

// mapped once, centrally, to a ProblemDetails response
app.UseExceptionHandler(err => err.Run(async ctx =>
{
    var feature = ctx.Features.Get<IExceptionHandlerFeature>();
    var (status, title) = feature?.Error switch
    {
        OrderStateException => (StatusCodes.Status409Conflict, "Invalid order state"),
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
        ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
        _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
    };
    ctx.Response.StatusCode = status;
    await ctx.Response.WriteAsJsonAsync(new ProblemDetails { Title = title, Status = status });
}));
```
**The lead point:** "exception handling is a cross-cutting concern and belongs in exactly one place — the pipeline. Controller `try/catch` around every action is the smell. I want my controllers to be 6 lines."

### C54. `throw;` vs `throw ex;` — and why

```csharp
try { await CallAsync(); }
catch (HttpRequestException ex)
{
    _log.LogError(ex, "Upstream call failed");
    throw;            // ✅ original stack trace preserved
    // throw ex;      // ❌ resets the stack trace to this line — the real origin is lost
    // throw new AppException("Upstream failed", ex);  // ✅ wrapping keeps the inner exception
}
```
**Bonus points:** use `ExceptionDispatchInfo.Capture(ex).Throw()` if you must rethrow outside a `catch` block. And mention that `$"..."` interpolation in the log call is fine for `LogError` because the level is enabled.

### C55. Exception filters and `AggregateException`

```csharp
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
{
    await Task.Delay(backoff, ct);
    // retry
}
```
- Filters run **before** the stack unwinds — cheaper than catching and rethrowing, and they keep the trace clean.
- They're how you write a "catch for logging only, then let it propagate" clause: `catch (Exception ex) when (Log(ex))` — still frowned on, mention it as a curiosity.
- `AggregateException` comes from `Task.WaitAll`/`Task.Wait` (blocking). With `await`, the **first** exception is thrown and the rest are attached to it (`await task` unwraps). If you need all of them: `Task.WhenAll` + inspect `task.Exception.InnerExceptions` after awaiting in a `try`.

### C56. `IDisposable` vs finalizer vs `IAsyncDisposable`

| | Deterministic? | Use for |
|---|---|---|
| `IDisposable` / `using` | **Yes** — disposed at scope end | Managed resources, `DbContext`, `HttpClient` handles, streams |
| Finalizer (`~Ctor`) | No — runs at GC, at all | **Only** unmanaged memory, and only via `SafeHandle` |
| `IAsyncDisposable` / `await using` | Yes | Disposing something that must await (`DisposeAsync` on a channel, a stream) |

```csharp
await using var scope = _scopeFactory.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
```
**The full-dispose idiom (only if you hold unmanaged state):**
```csharp
public class Handle : IDisposable
{
    private SafeHandle _handle = ...;
    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _handle?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
```
**What a lead says:** "in application code I never write finalizers. The pattern I insist on is `using` on anything that owns a resource, `await using` for async, and never depending on the GC for a connection. `HttpClient` is the classic exception — don't dispose it per call; inject `IHttpClientFactory`."

### C57. Is `Dispose` idempotent? Does `using` catch exceptions?

- `Dispose()` must be safe to call twice — implement the `_disposed` flag.
- `using` is a `try/finally`, so **`Dispose` runs even when the body throws**; the body's exception wins unless `Dispose` throws (which then replaces it — a reason to never throw from `Dispose`).
- `using` does **not** swallow exceptions.

### C58. Why `int.TryParse` instead of `try { int.Parse }`?

`TryParse` expresses "this may not parse" in the signature, costs no exception overhead, and reads better in validation code. Exceptions for expected input failure is a real performance problem at API scale — a malformed query string should be a 400, not a thrown-and-caught `FormatException` thousands of times a minute. This is also the rationale for `[ApiController]`'s automatic model validation.

---

## 8. Concurrency language features (C59–C64)

Full async depth is in `05-ASPNET-CORE-SENIOR.md` §5; these are the language-level questions.

### C59. `async`/`await` in one paragraph
"`async` marks a method as resumable; `await` yields the thread back to the pool until the awaited task completes. It's not a new thread — it's a state machine. The payoff is **scalability** (no blocked threads), not raw speed, and it's a lie if the work is CPU-bound — for that you need `Task.Run` with a very good reason."

### C60. `lock` vs `SemaphoreSlim` vs `Interlocked` vs `ConcurrentDictionary`

| Tool | Use |
|---|---|
| `lock` | Short, synchronous, in-process critical section. **Cannot `await` inside.** |
| `SemaphoreSlim` / `.WaitAsync()` | Async-compatible mutual exclusion; also throttling (`new SemaphoreSlim(10)`) |
| `Interlocked.Increment/Add/CompareExchange` | Single-variable atomic ops — counters, lock-free compare-and-swap |
| `ConcurrentDictionary<TKey,TValue>` | Concurrent map with `GetOrAdd`/`AddOrUpdate` |
| `AsyncLocal<T>` | Flow context (correlation id, tenant) across awaits |
| `Channel<T>` | Producer/consumer queue between async components |
| `volatile` | Only for a simple flag; it's a memory barrier hint, not mutual exclusion |

```csharp
// Thread-safe counter
private static int _requests;
public static int Requests => Interlocked.Increment(ref _requests) is var n ? n : n;

// Idempotent cache that can't be added twice
var value = _cache.GetOrAdd(key, k => Load(k));   // factory may run more than once — know that
```
**The sentence that proves depth:** "`ConcurrentDictionary.GetOrAdd` may invoke your factory more than once under contention — the value you actually get is a single winner, but you don't control how many times `Load` ran. So the factory must be side-effect-free or you need `Lazy<T>` as the value type."

### C61. What's wrong here? (closure capture)

```csharp
var tasks = new List<Task>();
for (int i = 0; i < 5; i++)
    tasks.Add(Task.Run(() => Console.WriteLine(i)));   // ⚠️ all may print 5

for (int i = 0; i < 5; i++)
{
    int copy = i;                                     // ✅ capture a per-iteration copy
    tasks.Add(Task.Run(() => Console.WriteLine(copy)));
}

foreach (var i in Enumerable.Range(0, 5))
    tasks.Add(Task.Run(() => Console.WriteLine(i)));  // ✅ foreach has captured per-iteration since C# 5
```
**Say:** "`foreach` was fixed in C# 5, `for` was not. This is the most common concurrency bug in code that uses `Task.Run` in a loop, and it also shows up as the last-item-wins bug in event handler registration."

### C62. `async void`, and sequential vs parallel awaits

```csharp
// ❌ async void: cannot be awaited, exceptions crash the process, scope is lost
public async void Send(Order o) { await _bus.Publish(o); }

// ✅ async Task
public async Task SendAsync(Order o) => await _bus.Publish(o);

// ❌ sequential — N round trips, N × latency
foreach (var id in ids) results.Add(await _client.GetAsync(id, ct));

// ✅ parallel — bounded, one round trip of time
var results = await Task.WhenAll(ids.Select(id => _client.GetAsync(id, ct)));

// ✅ parallel bounded — don't open 1000 sockets
using var throttle = new SemaphoreSlim(8);
var bounded = await Task.WhenAll(ids.Select(async id =>
{
    await throttle.WaitAsync(ct);
    try { return await _client.GetAsync(id, ct); }
    finally { throttle.Release(); }
}));
```
**Note the trap:** `Task.WhenAll` throws the **first** exception; use it with `try/catch` and inspect `whenAllTask.Exception?.InnerExceptions` if you need all failures. And always bound the parallelism — `Parallel.ForEachAsync` with `MaxDegreeOfParallelism` is the modern form.

`async void` is only legitimate for event handlers — it's the Angular/React equivalent of `setTimeout(...)` with no `await` and no error channel.

### C63. What does `volatile` do? Do you need it?

`volatile` guarantees reads/writes aren't cached in a register and prevents reordering **for that field only**. It is **not** thread safety. If you need more than one field to be consistent, use `lock` or `Interlocked`. Modern advice: "I've never needed `volatile` in application code — `lock`, `Interlocked`, `Concurrent*` and immutable messages cover everything."

### C64. `IAsyncEnumerable<T>` and `await foreach`

```csharp
public async IAsyncEnumerable<Order> StreamAsync(
    [EnumeratorCancellation] CancellationToken ct = default)
{
    await foreach (var order in db.Orders.AsAsyncEnumerable().WithCancellation(ct))
        yield return order;
}

// consumer: constant memory over millions of rows
await foreach (var order in service.StreamAsync(ct)) Process(order);
```
**Why it matters:** it's the answer to "how do you export 5 million rows without `OutOfMemoryException`", and it's the async counterpart to C27. Note that EF Core's `AsAsyncEnumerable()` streams server-side, so the connection must stay open for the duration — so no returning it from a `using`-scoped method.

---

## 9. The four modelling decisions every lead gets asked

These are the short questions with the longest tails, all of them visible in your repo.

### Money: `decimal` or `double`?

**`decimal`, always, for money.** `double` is binary floating point: `0.1 + 0.2 != 0.3`, and `0.1` has no exact representation, so cents drift. `decimal` is 128-bit base-10 with 28–29 significant digits.
```csharp
public decimal TotalAmount { get; set; }     // ✅ your repo does this correctly
```
Follow-ups to be ready for: **rounding policy** — decide and document (`MidpointRounding.ToEven` for accounting, `AwayFromZero` for user-facing totals), round **once** at the boundary and store the rounded value; **currency** — an amount without a currency is a bug waiting to happen, so a `Money` type; **`float`** never.

The two-line version: "`decimal` for money, `double` for scientific/geometric computation, and never mix them in one expression — `decimal + double` doesn't even compile without a cast, which is a feature."

### Time: `DateTime`, `DateTimeOffset`, or `TimeProvider`?

- `DateTime` has a **`Kind`** that is easy to lose through JSON round-trips → "is this UTC or local?" bugs.
- `DateTimeOffset` carries the offset — compare and store it, sort it, no ambiguity.
- `TimeProvider` (.NET 8+) is the **testability** fix: inject it instead of calling `DateTime.UtcNow`, and `FakeTimeProvider` makes time-dependent tests deterministic.

```csharp
public DateTime UtcNow { get; set; } = DateTime.UtcNow;  // repo pattern — works, but untestable and Kind can be Lost
public DateTimeOffset CreatedAt { get; init; }           // prefer this
public class OrderService(TimeProvider clock)            // and this for anything with SLAs/expiry/retries
{
    private DateTimeOffset Now => clock.GetUtcNow();
}
```
**This is a strong "improvement I'd make" answer** because it is small, uncontroversial, and it's a real weakness in the current code.

### Identity: `int`, `long`, or `Guid`?

| | Wins | Costs |
|---|---|---|
| `int` identity | 4 bytes, sequential → clustered-index friendly, human-usable | Guessable (enumeration attack), leaks volume, sharding pain, migration pain when you exceed 2.1B |
| `long` | Same, more headroom | Same (slightly less) |
| `Guid` | Client-generated (no round trip to get an id), unguessable, mergeable across systems, shardable | 16 bytes, **random GUIDs fragment the clustered index** → page splits, slower inserts, fatter non-clustered indexes |
| Sequential GUID | Best of both | Ordering is only "good enough" |

**Your repo uses both** — `Product.Id` is `int`, `Order.Id` is `Guid`. That is a legitimate design and a great thing to be able to explain: "the catalogue is internal and bounded → `int` is fine; an order is created by a client, appears in messages, and must be globally unique across services → `Guid`." Then add the modern improvement: "on .NET 9+ I'd use `Guid.CreateVersion7()` — time-ordered, so it's index-friendly and still unguessable. On .NET 8 with SQL Server, `NEWSEQUENTIALID()` or a `Guid` column with a sequential default gets you most of the way."

### Status: `string`, `enum`, or a state machine?

**Your repo has `public string Status { get; set; } = "Pending";` with the allowed values in a comment.** That's the question to pre-empt:

> "A string status is a magic string: `'pending'` vs `'Pending'` is a silent bug, adding a value means hunting three `switch` statements, and nothing stops an impossible transition. I'd move to an `enum` stored as a string via `HasConversion<string>()` — so the database stays readable and the C# is compile-checked — and put the transitions in a small state machine on the entity:
> ```
> Pending → PaymentProcessed → InventoryReserved → Completed
>   ↘ Failed (from any pre-terminal state)
> ```
> That's Open/Closed in practice: a new state is one enum member plus one transition rule, and the compiler finds my `switch` expressions for me."

Storage note: `enum` as `int` is fragile (renumbering the enum changes meaning) — so store **as string** or keep explicit values. That detail is what makes the answer senior.

---

## 10. Rapid-fire bank (50 one-liners)

| # | Question | Answer |
|---|---|---|
| 1 | `string` immutable? | Yes — hence `StringBuilder`, and hash caching. |
| 2 | `string` vs `String`? | Same type; `string` is the C# alias. |
| 3 | `int` vs `Int32`? | Same; alias. |
| 4 | `sealed`? | Cannot be inherited / cannot be overridden further (`sealed override`). |
| 5 | `static class`? | All members static, no instances, cannot inherit (extension-method host). |
| 6 | `partial class`? | One type across files — used by source generators and EF migrations. |
| 7 | Boxing? | Value type → heap object, allocation + copy. |
| 8 | Unboxing? | Cast back, with a runtime type check. |
| 9 | `Nullable<T>`? | `struct` wrapper; `T?` for value types, different meaning than `T?` on references. |
| 10 | `default(T)`? | Zero/`null` default of the type. |
| 11 | `var`? | Statically typed, inferred at compile time. |
| 12 | `dynamic`? | Runtime dispatch via DLR — avoid in domain code. |
| 13 | `object`? | The root of all types (`System.Object`). |
| 14 | `nameof`? | Compile-time name string — rename-safe. |
| 15 | `using` declaration? | `using var x = ...;` disposes at end of scope. |
| 16 | `try/finally` vs `using`? | `using` *is* `try/finally` for `IDisposable`. |
| 17 | `throw;` vs `throw ex;` | `throw;` preserves the stack trace. |
| 18 | `catch when`? | Exception filter — runs before unwind. |
| 19 | `AggregateException`? | Multiple failures from `WhenAll`/`Wait`. |
| 20 | `async void`? | Only for event handlers; unroutable exceptions. |
| 21 | `ConfigureAwait(false)`? | Skip context capture — for libraries, not ASP.NET Core. |
| 22 | `.Result` / `.Wait()`? | Blocking → deadlocks + thread-pool starvation. Never. |
| 23 | `ValueTask`? | Avoid allocation when the result is usually synchronous (5 §5). |
| 24 | `Task` vs `Thread`? | `Task` is a promise on the pool, not a thread. |
| 25 | `Parallel.ForEachAsync`? | Bounded async parallel work. |
| 26 | `lock` on what? | A **private static readonly object**, never `this`, never a string. |
| 27 | `Interlocked`? | Atomic increment/CAS on a single variable. |
| 28 | `AsyncLocal<T>`? | Flows context across `await`. |
| 29 | `IEnumerable` vs `IQueryable`? | Memory vs provider (SQL) — 05 §7. |
| 30 | Deferred execution? | Nothing runs until enumerated. |
| 31 | `First` vs `FirstOrDefault`? | `First` throws on empty; `Single` throws on >1. |
| 32 | `Any` vs `Count() > 0`? | `Any` short-circuits. |
| 33 | `Select` vs `SelectMany`? | Map vs map + flatten. |
| 34 | `GroupBy` deferred? | Yes, and it materialises on first enumeration. |
| 35 | `AsNoTracking`? | Don't track results — read-only queries. |
| 36 | `AsSplitQuery`? | Avoids cartesian explosion on multiple `Include`s. |
| 37 | `Include` vs projection? | `Select` into a DTO is usually cheaper. |
| 38 | `record` vs `class`? | Value equality + `with` + readable `ToString`. |
| 39 | `record struct`? | Value type with value semantics. |
| 40 | `init`? | Set at construction only. |
| 41 | `required`? | Compiler enforces initialisation (C# 11). |
| 42 | `const` danger? | Inlined into consumers' IL → versioning bug. |
| 43 | `readonly` field? | Assignment only in the constructor. |
| 44 | Struct size guidance? | ≤ 16–24 bytes, immutable, no inheritance need. |
| 45 | `ref`/`out`/`in`? | Reference to the caller's variable / write-out / read-only ref. |
| 46 | `Span<T>`? | Non-allocating view; ref struct; not for `async`. |
| 47 | `yield`? | Lazy state machine; caller's timing and thread. |
| 48 | `IAsyncEnumerable`? | Stream with `await foreach`. |
| 49 | `nameof` in `ArgumentNullException`? | Free correctness in logs and messages. |
| 50 | `Guid` vs `int` id? | Unguessable + client-generated vs index-friendly. |

---

## 11. Self-test — answer these out loud, recorded (30 min)

1. Value type vs reference type, and where each is stored. Give the "not always the stack" caveat.
2. When is a struct the right choice, and what is the mutable-struct-in-a-list bug?
3. What is boxing — give two places it happens in code you have actually written.
4. `record` vs `class`: what do you get for free, and what is the shallow-equality caveat?
5. `const` vs `static readonly` — and the versioning bug.
6. Why is `s += x` in a loop O(n²), and when is `StringBuilder` *not* worth it?
7. `ToLower() == "admin"` — name three problems with that line.
8. What do nullable reference types actually do at runtime? (Trick: almost nothing.)
9. `var` vs `dynamic` vs `object`.
10. `IEnumerable<T>` vs `IQueryable<T>` — where does the query execute, and what breaks if you get it wrong?
11. Explain the double-enumeration bug and how you avoid it.
12. `IReadOnlyList<T>` on a return type — what does it buy you?
13. Abstract class vs interface — decision rule, plus default interface methods.
14. `virtual` vs `new` — demonstrate the difference through a base reference.
15. Give a LSP violation you have actually seen, and the fix.
16. Encapsulation is not getters and setters — prove it with an example from your repo.
17. Anemic vs rich domain model — where is the line in your delivery app?
18. SOLID: one example each, and the honest caveat about over-applying it.
19. Name four patterns you use and the problem each solves.
20. Is `DbContext` a repository and unit of work? Argue both sides for 60 seconds.
21. `throw;` vs `throw ex;`, and where exception handling belongs in an ASP.NET Core app.
22. `decimal` vs `double` for money — plus your rounding policy.
23. `Guid` vs `int` keys — argue for the design your repo already has.
24. `string` status vs `enum` vs a state machine — what would you change and why.
25. `GetOrAdd` on `ConcurrentDictionary` — what's the trap?

If you can answer 20 of those in under 90 seconds each, tier 1 is closed and you can spend your remaining prep time on `05`–`13`.

---

## 30-second version

Tier-1 questions are a **credibility gate**, not a knowledge test — a lead who fumbles `record` vs `class` has every story silently discounted. Answer in three beats: definition, your repo's example, the trap. The highest-yield items are **value vs reference semantics, boxing, string immutability, deferred execution, `IEnumerable` vs `IQueryable`, abstract vs interface, LSP, `throw;` vs `throw ex;`, and the four modelling decisions (money, time, identity, status)** — three of which are visible weaknesses in this repo that you can name and fix on the whiteboard.
