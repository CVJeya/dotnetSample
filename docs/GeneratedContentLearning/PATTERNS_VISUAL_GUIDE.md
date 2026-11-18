# Design Patterns - Visual & Interactive Guide

## Quick Visual Reference

### 1. INTERFACE PATTERN 🍽️
```
┌──────────────────────────────────────────┐
│         IQuotaService (Menu)             │
│  ✓ CheckAndRecordAsync()                 │
│  ✓ CheckAsync()                          │
│  ✓ RecordAsync()                         │
│  ✓ GetCurrentAsync()                     │
└──────────────────────────────────────────┘
         △           △
         │           │
    ┌────┴──┐    ┌───┴───┐
    │        │    │       │
    │        │    │       │
┌─────────────┐ ┌──────────────────┐
│ In-Memory   │ │ Redis Service    │
│ Service     │ │ Implementation   │
│ (Dev)       │ │ (Production)     │
└─────────────┘ └──────────────────┘

User says: "I need to check quota"
         ↓
System gives them EITHER in-memory OR redis
      (They look the same to user!)
```

---

### 2. DEPENDENCY INJECTION 👔
```
WITHOUT DI:                    WITH DI:
┌──────────────┐              ┌──────────────┐
│ Middleware   │              │ Middleware   │
│              │              │              │
│ new Redis()  │              │ (receives    │
│ new Resolver │              │  service)    │
│ new Logger   │              │              │
│ new Options  │              │              │
│              │              │              │
│ (Creates     │              │ (Assistant   │
│  everything) │              │  gives it)   │
└──────────────┘              └──────────────┘

❌ TIGHT COUPLING              ✅ LOOSE COUPLING
  Hard to test                  Easy to test
  Hard to change                Easy to swap
```

---

### 3. STRATEGY PATTERN 👕
```
CONFIGURATION:
┌────────────────────────────────────┐
│ appsettings.json                   │
│ Redis: "localhost:6379"            │
└────────────────────────────────────┘
         │
         ↓
    ENVIRONMENT CHECK
         │
    ┌────┴────┐
    │          │
    ▼          ▼
DEVELOPMENT  PRODUCTION
    │          │
    ▼          ▼
┌─────────┐ ┌──────────┐
│ In-Mem  │ │  Redis   │
└─────────┘ └──────────┘

Same request path → Different implementation based on config
```

---

### 4. MIDDLEWARE PATTERN 🛡️
```
REQUEST LIFECYCLE:

User Request
    ↓
┌──────────────────┐
│ CORS Middleware  │──→ Check origin
└──────────────────┘
    ↓
┌──────────────────┐
│ Auth Middleware  │──→ Verify token
└──────────────────┘
    ↓
┌──────────────────┐
│ QUOTA ←HERE      │──→ Check limits
└──────────────────┘        │
    ↓                       ├─ Allowed? Continue ↓
┌──────────────────┐        └─ Denied? Return 429 ↗
│ Route Handler    │
└──────────────────┘
    ↓
Response (200 OK or error)

EACH MIDDLEWARE:
- Can see the request
- Can allow it (call next()) OR block it
- Can modify response
```

---

### 5. RESOLVER PATTERN 📊
```
QUESTION: "What's the limit for this tenant?"

ANSWER PRIORITY (Try in order):
┌─────────────────────────────────┐
│ 1. Tenant Override?             │
│    QuotaLimits["free"] = 999    │  ← Highest priority
└─────────────────────────────────┘
         │ (not found)
         ↓
┌─────────────────────────────────┐
│ 2. Plan Default?                │
│    Plans["free"] = 1000         │  ← Medium priority
└─────────────────────────────────┘
         │ (not found)
         ↓
┌─────────────────────────────────┐
│ 3. Unlimited (Default)          │
│    long.MaxValue                │  ← Fallback
└─────────────────────────────────┘

Returns: (limit=999, exempt=false)
         ↓
Middleware uses this to decide: Allow or Reject?
```

---

### 6. CONFIGURATION PATTERN 📖
```
appsettings.Development.json      appsettings.Production.json
┌─────────────────────────┐      ┌─────────────────────────┐
│ QuotaOptions            │      │ QuotaOptions            │
│ {                       │      │ {                       │
│   Enabled: true         │      │   Enabled: true         │
│   Redis: null           │      │   Redis: "redis.aws..." │
│   DefaultPlan: "free"   │      │   DefaultPlan: "free"   │
│   Plans: {...}          │      │   Plans: {...}          │
│ }                       │      │ }                       │
└─────────────────────────┘      └─────────────────────────┘
         │                                   │
         ↓                                   ↓
    In-Memory Service                   Redis Service
   (Single process)                  (Cluster-wide)
         │                                   │
         ↓                                   ↓
    Laptop Dev Env                    Cloud Production

SAME CODE, DIFFERENT BEHAVIOR
```

---

### 7. RECORD PATTERN 🧾
```
IMMUTABLE DATA CONTAINER:

var result = new QuotaCheckResult(
    Allowed: true,           ← Can READ
    Resource: ApiCalls,      ← Can READ
    CurrentUsage: 50,        ← Can READ
    Limit: 1000,            ← Can READ
    ResetAtUtc: Feb 1       ← Can READ
);

result.Allowed = false;  ❌ COMPILER ERROR
                         Can't change once created!

BENEFITS:
✓ Can't accidentally break data
✓ Compiler catches bugs
✓ Thread-safe (no synchronization needed)
✓ Clear intent (this is just data)
```

---

### 8. ABSTRACTION PATTERN 🚗
```
WHAT USER SEES:
┌────────────────────────────────────┐
│    DASHBOARD (Interface)           │
│                                    │
│  await quotaService.Check...()    │
│                                    │
└────────────────────────────────────┘
         │
         ↓
WHAT'S HIDDEN (Implementation):
┌────────────────────────────────────┐
│    ENGINE (Complex Logic)          │
│                                    │
│  - Redis connections               │
│  - Atomic operations               │
│  - TTL management                  │
│  - Plan resolution                 │
│  - Tenant database lookups         │
│  - Concurrency control             │
│                                    │
└────────────────────────────────────┘

User: "I just want to check quota"
  ↓
Driver: "Look at speedometer"
  ↓
YOU DON'T CARE about transmission,
  engine, suspension, etc.
```

---

### 9. SINGLETON vs SCOPED ☕
```
SINGLETON (Create ONCE, reuse forever):
┌──────────────────────────────────┐
│  SHARED COFFEE MACHINE           │
│  (Office break room)             │
├──────────────────────────────────┤
│  Request 1: Use machine → return │
│  Request 2: Use same machine     │
│  Request 3: Use same machine     │
│  ...                             │
│  Request 1000: Still same object │
└──────────────────────────────────┘

Perfect for: Configuration, Connections
Problem: Must be thread-safe


SCOPED (Create NEW per request):
┌──────────────────────────────────┐
│  FRESH COFFEE PER PERSON         │
├──────────────────────────────────┤
│  Request 1: Get new service      │
│  Request 2: Get different new    │
│  Request 3: Get different new    │
│  ...                             │
│  Request 1000: Get new again     │
└──────────────────────────────────┘

Perfect for: Per-request state
Benefit: Thread-safe automatically


TRANSIENT (Create EVERY time):
┌──────────────────────────────────┐
│  NEW CUP FOR EACH SIP            │
├──────────────────────────────────┤
│  quotaService.Check() → new obj  │
│  quotaService.Check() → new obj  │
│  quotaService.Check() → new obj  │
└──────────────────────────────────┘

Rare, usually for lightweight objects
```

---

### 10. DECORATOR/WRAPPER PATTERN 🎁
```
MIDDLEWARE STACK (Wrapped Request):

┌───────────────────────────────┐
│    Raw Request                │
│    GET /api/products          │
│    tenant: acme               │
│    claims: {...}              │
└───────────────────────────────┘
         │
         ↓ Wrapped by CORS Middleware
┌───────────────────────────────┐
│    With CORS Headers          │
│    Access-Control-Allow-...   │
└───────────────────────────────┘
         │
         ↓ Wrapped by Auth Middleware
┌───────────────────────────────┐
│    With User Identity         │
│    User authenticated: yes    │
└───────────────────────────────┘
         │
         ↓ Wrapped by Quota Middleware
┌───────────────────────────────┐
│    With Quota Tracking        │
│    Usage: 1, Limit: 1000      │
└───────────────────────────────┘
         │
         ↓ Wrapped by Handler
┌───────────────────────────────┐
│    With Response              │
│    [{"id": 1, "name": "..."}] │
└───────────────────────────────┘

Each layer can:
• Pass through to next layer
• Modify the request
• Stop and return error
```

---

### 11. ATOMIC OPERATION PATTERN 🏦
```
SCENARIO: Check & Record Quota

BAD (Not Atomic):
┌─────────────────────────┐
│ Step 1: Read current    │ ← Got 999
├─────────────────────────┤
│ Step 2: Check limit     │ ← 999 < 1000? Yes
├─────────────────────────┤
│ Step 3: Increment       │ ← Becomes 1000
│         OOPS! Another   │   But wait...
│         request runs    │   [OTHER REQUEST RUNS HERE]
│         between 2 & 3!  │   It also checks 999!
│         Both think ok!  │
└─────────────────────────┘

RESULT: Quota exceeded but both passed!


GOOD (Atomic):
┌─────────────────────────┐
│ ATOMIC OPERATION        │
│ {                       │
│   Read current: 999     │
│   Check limit: ok       │
│   Increment: 1000       │
│ }                       │
│ ← No other request can  │
│   run during this!      │
└─────────────────────────┘

RESULT: Only one increments, others wait
```

---

### 12. ENUM PATTERN 📚
```
LIBRARY ORGANIZATION:

What NOT to do:
┌─────────────────────┐
│ Books               │
│ - "Harry Potter"    │
│ - "Dune"            │
│ - 12.5              │  ← What is this?
│ - "maybe fiction"   │  ← Unclear
│ - null              │  ← Data missing
└─────────────────────┘


What TO do (Enum):
┌─────────────────────┐
│ QuotaResource       │
│ • ApiCalls          │ ← Type-safe
│ • StorageBytes      │ ← Clear choices
│ • Users             │ ← Compiler checks
│                     │
│ (Can't choose any-  │
│  thing else!)       │
└─────────────────────┘

CODE:
await quotaService.CheckAndRecordAsync(
    tenantId,
    QuotaResource.ApiCalls,  ← Must be one of the enum values
    1
);

quotaService.CheckAndRecordAsync(
    tenantId,
    "ApiCalls",  ← ❌ STRING - typo could happen
    1
);

quotaService.CheckAndRecordAsync(
    tenantId,
    999,  ← ❌ NUMBER - compiler doesn't know what this is
    1
);
```

---

## Pattern Interaction Diagram

```
┌─────────────────────────────────────────────────────────────┐
│  USER SENDS REQUEST                                         │
└─────────────────────────────────────────────────────────────┘
         │
         ↓
┌─────────────────────────────────────────────────────────────┐
│  CONFIGURATION PATTERN (Read from appsettings.json)         │
│  - Determine: Use Redis or In-Memory?                       │
│  - Load: Plan definitions, defaults, exemptions             │
└─────────────────────────────────────────────────────────────┘
         │
         ↓
┌─────────────────────────────────────────────────────────────┐
│  DEPENDENCY INJECTION (Program.cs)                          │
│  - Create QuotaPlanResolver (singleton)                     │
│  - Create IQuotaService (Redis or In-Memory) (scoped)       │
│  - Create Middleware (transient)                            │
└─────────────────────────────────────────────────────────────┘
         │
         ↓
┌─────────────────────────────────────────────────────────────┐
│  MIDDLEWARE PATTERN (Request arrives)                       │
│  - Can this request proceed?                                │
└─────────────────────────────────────────────────────────────┘
         │
         ↓
┌─────────────────────────────────────────────────────────────┐
│  INTERFACE PATTERN (Call IQuotaService)                     │
│  - Middleware doesn't care HOW it works                     │
│  - Just calls CheckAndRecordAsync()                         │
└─────────────────────────────────────────────────────────────┘
         │
         ↓
┌─────────────────────────────────────────────────────────────┐
│  STRATEGY PATTERN (Dispatch to implementation)              │
│  - If Redis configured → RedisQuotaService.Check...()      │
│  - If not → InMemoryQuotaService.Check...()                │
└─────────────────────────────────────────────────────────────┘
         │
         ↓
┌─────────────────────────────────────────────────────────────┐
│  RESOLVER PATTERN (Figure out limit)                        │
│  - Check tenant override (priority 1)                       │
│  - Check plan limit (priority 2)                            │
│  - Default to unlimited (priority 3)                        │
│  - Return (limit, exempt)                                   │
└─────────────────────────────────────────────────────────────┘
         │
         ↓
┌─────────────────────────────────────────────────────────────┐
│  ENUM PATTERN (Which resource?)                             │
│  - QuotaResource.ApiCalls ← Type-safe choice               │
└─────────────────────────────────────────────────────────────┘
         │
         ↓
┌─────────────────────────────────────────────────────────────┐
│  ATOMIC OPERATION PATTERN (Check & increment)               │
│  - No race conditions                                       │
│  - All-or-nothing guarantee                                 │
└─────────────────────────────────────────────────────────────┘
         │
         ↓
┌─────────────────────────────────────────────────────────────┐
│  RECORD PATTERN (Return result)                             │
│  - QuotaCheckResult (immutable)                             │
│  - Contains: Allowed, CurrentUsage, Limit, ResetTime       │
└─────────────────────────────────────────────────────────────┘
         │
         ↓
┌─────────────────────────────────────────────────────────────┐
│  MIDDLEWARE PATTERN (Decide)                                │
│  - result.Allowed == true? Continue to handler              │
│  - result.Allowed == false? Return HTTP 429                 │
└─────────────────────────────────────────────────────────────┘
         │
         ↓
┌─────────────────────────────────────────────────────────────┐
│  ABSTRACTION PATTERN (User doesn't know complexity)         │
│  - Just sees: Request allowed or rejected                   │
│  - Doesn't see: Redis, locking, TTL, resolution            │
└─────────────────────────────────────────────────────────────┘
         │
         ↓
┌─────────────────────────────────────────────────────────────┐
│  RESPONSE                                                   │
│  - 200 OK + business logic result                           │
│  - OR 429 Too Many Requests + ProblemDetails error          │
└─────────────────────────────────────────────────────────────┘
```

---

## Decision Tree: Which Pattern?

```
PROBLEM: How do I organize my code?
│
├─ "Multiple ways to do same thing"
│  └─ Use INTERFACE + STRATEGY patterns
│     Example: Redis vs In-Memory storage
│
├─ "Need flexibility to swap implementations"
│  └─ Use DEPENDENCY INJECTION pattern
│     Example: Inject IQuotaService instead of creating it
│
├─ "Different behavior per environment"
│  └─ Use CONFIGURATION pattern
│     Example: appsettings.json changes behavior
│
├─ "Need to check things before processing"
│  └─ Use MIDDLEWARE pattern
│     Example: Auth before quota before business logic
│
├─ "Complex logic to figure out answer"
│  └─ Use RESOLVER pattern
│     Example: QuotaPlanResolver figures out limits
│
├─ "Should never change once created"
│  └─ Use RECORD pattern
│     Example: QuotaCheckResult is immutable
│
├─ "Need to hide implementation details"
│  └─ Use ABSTRACTION pattern
│     Example: User calls interface, not implementation
│
├─ "Adding behavior to requests"
│  └─ Use DECORATOR/WRAPPER pattern
│     Example: Middleware wraps request
│
├─ "Need all-or-nothing execution"
│  └─ Use ATOMIC OPERATION pattern
│     Example: Check and increment together
│
└─ "Limited set of valid choices"
   └─ Use ENUM pattern
      Example: QuotaResource can only be specific values
```

---

## Cheat Sheet: When to Use Each Pattern

| Pattern | Problem | Solution | Example |
|---------|---------|----------|---------|
| **Interface** | Multiple implementations | Define contract | IQuotaService |
| **DI** | Tight coupling | Inject dependencies | Constructor param |
| **Strategy** | Different algorithms | Pick at runtime | Redis vs In-Memory |
| **Middleware** | Request processing | Chain of handlers | Auth → Quota → Handler |
| **Resolver** | Complex decisions | Centralized logic | Figure out quota limit |
| **Config** | Hardcoded values | Externalize settings | appsettings.json |
| **Record** | Prevent mutations | Immutable data | QuotaCheckResult |
| **Abstraction** | Implementation details | Hide complexity | Don't expose Redis |
| **DI Lifetime** | Object creation | Control lifespan | Singleton/Scoped |
| **Decorator** | Add behavior | Wrap and compose | Middleware stacking |
| **Atomic Op** | Race conditions | All-or-nothing | Check + record |
| **Enum** | Type safety | Limited choices | QuotaResource values |

---

## Anti-Patterns (Don't Do This!)

```csharp
❌ ANTI-PATTERN: Tight Coupling
public class Middleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var service = new RedisQuotaService(new RedisClient(...));
        // Can't change to In-Memory
        // Hard to test (real Redis required)
        // Middleware creates its own dependencies
    }
}

✅ CORRECT: Loose Coupling with DI
public class Middleware
{
    private readonly IQuotaService _service;  // Injected
    
    public Middleware(IQuotaService service)
    {
        _service = service;  // Can be any implementation
    }
}


❌ ANTI-PATTERN: Hardcoded Configuration
var limit = 1000;  // Change requires recompile!
var redis = "localhost";  // Different in production

✅ CORRECT: Configuration-Driven
var limit = options.Plans[plan][resource];  // From config
var redis = configuration["Redis"];  // From appsettings.json


❌ ANTI-PATTERN: Mutable Data
var result = new QuotaCheckResult(...);
result.Allowed = false;  // Can change anytime!

✅ CORRECT: Immutable Data
public record QuotaCheckResult(bool Allowed, ...);
result.Allowed = false;  // ❌ Compiler error!


❌ ANTI-PATTERN: Scattered Logic
// Quota check logic in middleware
// Plan resolution in handler
// Limit calculation in repository
// (Hard to maintain, duplicate code)

✅ CORRECT: Centralized Logic
// All quota logic in QuotaService
// All plan logic in QuotaPlanResolver
// (Single place to change)
```

---

## The Pattern Learning Journey

```
DAY 1: What is an Interface?
└─ "A contract that says 'I have these methods'"

DAY 5: Why use Interfaces?
└─ "So I can swap implementations without changing code"

DAY 10: How do Interfaces + DI work together?
└─ "DI creates the right implementation, interface abstracts it"

DAY 20: I see patterns in everything!
└─ "Oh, that's Strategy pattern!"
└─ "Oh, that's Resolver pattern!"

DAY 60: I can recognize patterns and choose when to use them
└─ "This problem needs Decorator pattern"
└─ "That problem needs Resolver pattern"

DAY 100: I can explain patterns in simple terms
└─ "Interface is like a restaurant menu"
└─ "Middleware is like security guards"

DAY 1000: I design systems using patterns
└─ "Build modular, testable, scalable code"
```

---

## Key Insight: Patterns Are About Communication

**Software development = Problem Solving + Communication**

When you say:
- "We use the Strategy pattern here" → Developer understands immediately
- "We use the Middleware pattern" → Developer knows the pipeline structure
- "This is a record" → Developer knows it's immutable

**Patterns = Shorthand that experienced developers understand**

This is why they're valuable:
1. **Knowledge transfer** — New team members understand architecture faster
2. **Decision making** — Patterns guide architectural choices
3. **Code organization** — Clear structure, easy to navigate
4. **Problem solving** — Proven solutions to common problems

---

## Final Checklist: Do You Understand?

- [ ] I can explain Interface pattern using restaurant analogy
- [ ] I can explain DI pattern using personal assistant analogy
- [ ] I can explain Strategy pattern using weather outfit analogy
- [ ] I can explain Middleware pattern using security guard analogy
- [ ] I can explain Resolver pattern using accountant analogy
- [ ] I can explain Configuration pattern using recipe book analogy
- [ ] I can explain Record pattern using receipt analogy
- [ ] I can explain Abstraction pattern using car dashboard analogy
- [ ] I can explain DI Lifetimes using coffee machine analogy
- [ ] I can explain Decorator pattern using gift wrapping analogy
- [ ] I can explain Atomic Operations using bank transfer analogy
- [ ] I can explain Enum pattern using library categories analogy

If you checked most boxes — **You understand the patterns!** 🎉

Now go build something with them.
