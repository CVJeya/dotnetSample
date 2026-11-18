# Design Patterns Explained in Simple Language

This document explains the design patterns used in the quota enforcement solution using **everyday analogies** that even non-programmers can understand.

---

## 1. THE INTERFACE PATTERN (The Restaurant Menu)

### Real-World Analogy

Imagine a restaurant where you can order food:
- **The Menu (Interface)** = A list of dishes the restaurant can make
  - "We serve: Pizza, Pasta, Salad"
  - It tells you WHAT's available but NOT how it's made
  
- **The Cook (Implementation)** = The person who actually makes the food
  - One cook makes pizza by hand
  - Another cook makes pasta in a factory kitchen
  - But the menu looks the same to you!

### In Our Quota Code

```csharp
// The Menu (Interface)
public interface IQuotaService
{
    Task<QuotaCheckResult> CheckAndRecordAsync(string tenantId, QuotaResource resource, long amount);
}

// Cook #1: Makes food using memory (in-home kitchen)
public class InMemoryQuotaService : IQuotaService
{
    public async Task<QuotaCheckResult> CheckAndRecordAsync(...)
    {
        // Store data in computer's memory
    }
}

// Cook #2: Makes food using Redis (professional kitchen)
public class RedisQuotaService : IQuotaService
{
    public async Task<QuotaCheckResult> CheckAndRecordAsync(...)
    {
        // Store data in Redis database
    }
}
```

### Why This Matters

✅ **You don't care HOW quotas are checked** — just that they ARE checked
✅ **Easy to switch implementations** — swap "in-memory" for "Redis" in config
✅ **Easy to test** — create a fake implementation for testing without Redis

---

## 2. THE DEPENDENCY INJECTION PATTERN (The Personal Assistant)

### Real-World Analogy

Imagine you're a busy executive:
- Instead of going to the store yourself, you have an **assistant**
- You tell your assistant: "I need a coffee machine"
- Your assistant **brings it to you** and sets it up
- You just use it; you don't care where it came from

### In Our Quota Code

```csharp
// You need a quota service, so you ask for it
public class QuotaEnforcementMiddleware
{
    // Instead of creating it yourself:
    // var service = new InMemoryQuotaService();  ❌ BAD
    
    // You ask your assistant (dependency injection):
    public QuotaEnforcementMiddleware(IQuotaService quotaService)  ✅ GOOD
    {
        _quotaService = quotaService;  // Assistant gave it to you
    }
}

// In Program.cs, your assistant is configured:
services.AddScoped<IQuotaService, InMemoryQuotaService>();
// "When someone asks for IQuotaService, give them InMemoryQuotaService"
```

### Why This Matters

✅ **You don't create objects yourself** — cleaner code
✅ **Easy to change implementations** — change one line in Program.cs
✅ **Easy to test** — inject a fake service for testing
✅ **Objects are reused efficiently** — your assistant keeps track

---

## 3. THE STRATEGY PATTERN (The Weather Outfit)

### Real-World Analogy

Imagine getting dressed for the day:
- **The Decision**: "What should I wear based on the weather?"
- **Strategy 1 (Sunny)**: Wear light clothes, sunscreen
- **Strategy 2 (Rainy)**: Wear raincoat, take umbrella
- **Strategy 3 (Snowy)**: Wear heavy coat, boots

You choose the strategy based on the weather, but your body stays the same!

### In Our Quota Code

```csharp
// The Decision: "How should I store quota counters?"
// Strategy 1 (Development): Use computer memory
public class InMemoryQuotaService : IQuotaService { ... }

// Strategy 2 (Production): Use Redis database
public class RedisQuotaService : IQuotaService { ... }

// The config decides which strategy to use:
if (!string.IsNullOrWhiteSpace(quotaOptions.Redis))
{
    // Sunny day → wear light clothes (use Redis)
    services.AddScoped<IQuotaService, RedisQuotaService>();
}
else
{
    // Rainy day → wear raincoat (use in-memory)
    services.AddScoped<IQuotaService, InMemoryQuotaService>();
}
```

### Why This Matters

✅ **Different behaviors** based on environment (dev vs. production)
✅ **Easy to switch** between strategies
✅ **Clean code** — no messy if/else scattered everywhere

---

## 4. THE MIDDLEWARE PATTERN (The Security Guard)

### Real-World Analogy

Imagine entering a shopping mall:
1. **Security Guard** checks your bags at the entrance
2. **Security Guard** decides if you can enter or must leave
3. **Security Guard** either lets you pass OR stops you
4. If allowed, you proceed to the shops

Each layer checks something and decides what happens next.

### In Our Quota Code

```csharp
public class QuotaEnforcementMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Security guard checks: "Is this request within quota?"
        var result = await _quotaService.CheckAndRecordAsync(tenantId, QuotaResource.ApiCalls, 1);
        
        if (result.Allowed)
        {
            // ✅ Allowed → pass to next guard/shop
            await next(context);
        }
        else
        {
            // ❌ Blocked → return HTTP 429 (go home)
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            // Send error message
        }
    }
}
```

### The Middleware Pipeline (In Order)

```
User Request
    ↓
[1. Authentication Guard] — "Who are you?"
    ↓
[2. Quota Guard] ← HERE IS OUR MIDDLEWARE — "Are you within quota?"
    ↓
[3. Authorization Guard] — "Do you have permission?"
    ↓
[4. Business Logic] — "Process the request"
    ↓
Response
```

### Why This Matters

✅ **Separation of concerns** — each guard does one job
✅ **Reusable** — same guard checks every request
✅ **Ordered** — auth happens before quota
✅ **Easy to test** — can test guards independently

---

## 5. THE RESOLVER PATTERN (The Accountant)

### Real-World Analogy

Imagine a company with different pay scales:
- **Junior Employee**: Gets $30/hour
- **Senior Employee**: Gets $60/hour
- **Manager**: Gets $100/hour + bonus

Your **accountant** resolves this:
1. Looks at your job title
2. Looks at special pay overrides (got a raise? take that)
3. Calculates your actual pay

### In Our Quota Code

```csharp
public class QuotaPlanResolver
{
    public (long limit, bool exempt) ResolveLimit(string tenantId, QuotaResource resource)
    {
        // Is this the special VIP tenant? (exempt)
        if (tenantId == "root")
            return (long.MaxValue, true);

        // Get tenant's plan: "free", "pro", "enterprise"
        var plan = tenant.Plan ?? "free";

        // 1st Priority: Does this tenant have a special override?
        if (tenant.QuotaLimits.TryGetValue(resource, out var limit))
            return (limit, false);  // Use the override

        // 2nd Priority: Use their plan's limit
        var planLimit = plans[plan][resource];  // e.g., 1000 for free, 50000 for pro
        return (planLimit, false);

        // If nothing found → unlimited
        return (long.MaxValue, false);
    }
}
```

**Priority Order (like a filing system):**
1. Special overrides (top folder)
2. Plan defaults (middle folder)
3. Unlimited (default fallback)

### Why This Matters

✅ **Centralized logic** — one place to change quota rules
✅ **Priority system** — handles all cases (overrides, plans, defaults)
✅ **Reusable** — same resolver for all resources
✅ **Testable** — easy to test different scenarios

---

## 6. THE CONFIGURATION PATTERN (The Recipe Book)

### Real-World Analogy

Imagine a baker's recipe book:
- **The Recipe**: Ingredients and instructions
- **The Environment**: Your kitchen (oven temperature varies)
- **Adaptation**: Same recipe, but bake at 350°F in Denver, 375°F in NYC

You don't change the code (recipe), you change the config (temperature).

### In Our Quota Code

```csharp
// In appsettings.json (your recipe book)
{
  "QuotaOptions": {
    "Enabled": true,
    "Redis": "localhost:6379",           // ← Development
    "DefaultPlan": "free",
    "Plans": {
      "free": { "ApiCalls": 1000 },
      "pro": { "ApiCalls": 50000 },
      "enterprise": { "ApiCalls": 999999 }
    }
  }
}

// In appsettings.Production.json (different kitchen)
{
  "QuotaOptions": {
    "Redis": "redis.aws.com:6379",      // ← Production (cloud)
    "Plans": { ... }  // Same logic, different values
  }
}
```

### Why This Matters

✅ **No code changes** — just change config files
✅ **Different environments** — dev uses localhost, prod uses cloud
✅ **Easy to deploy** — same code, different configs per environment
✅ **Safe for secrets** — don't commit production passwords

---

## 7. THE RECORD PATTERN (The Data Container)

### Real-World Analogy

Imagine a receipt from a store:
- **Date**: 2024-01-15
- **Total**: $42.50
- **Items**: 3
- **Receipt is FINAL** — you can't change it

You can read the receipt, but you can't alter the past!

### In Our Quota Code

```csharp
// A record: immutable data container
public sealed record QuotaCheckResult(
    bool Allowed,              // Can proceed?
    QuotaResource Resource,    // What was checked?
    long CurrentUsage,         // Current usage (read-only)
    long Limit,                // Limit (read-only)
    DateTimeOffset? ResetAtUtc // When resets (read-only)
);

// Use it:
var result = new QuotaCheckResult(true, ApiCalls, 50, 1000, nextMonth);

// You can READ the values:
if (result.Allowed) { ... }
Console.WriteLine(result.CurrentUsage);  // 50

// But you CAN'T CHANGE them:
result.Allowed = false;  // ❌ COMPILER ERROR

// The compiler prevents accidental bugs!
```

### Why This Matters

✅ **Immutable** — once created, can't be changed (prevents bugs)
✅ **Simple** — just data, no logic
✅ **Readable** — shows exactly what info is in a result
✅ **Safe** — compiler catches mistakes

---

## 8. THE ABSTRACTION PATTERN (The Dashboard)

### Real-World Analogy

Imagine a car dashboard:
- **What you see**: Speedometer, fuel gauge, temperature
- **What's hidden**: Complex engine, transmission, suspension
- You don't need to understand the engine to drive!

The dashboard **hides complexity**.

### In Our Quota Code

```csharp
// What the middleware SEES (simple dashboard):
public async Task InvokeAsync(HttpContext context, RequestDelegate next)
{
    var result = await _quotaService.CheckAndRecordAsync(
        tenantId,
        QuotaResource.ApiCalls,
        1);
    
    if (result.Allowed)
        await next(context);
    else
        // Reject
}

// What's HIDDEN (complex engine):
// - Redis atomic operations (INCRBY)
// - TTL expiration logic
// - Plan resolution priorities
// - Tenant database lookups
// - All hidden behind the simple interface!
```

### Why This Matters

✅ **Middleware doesn't care HOW quotas work** — just calls the service
✅ **Complex logic is encapsulated** — not spread everywhere
✅ **Easy to understand** — high-level overview without details
✅ **Easy to change** — swap implementations without touching middleware

---

## 9. THE SINGLETON vs SCOPED PATTERN (Coffee Machines)

### Real-World Analogy

Imagine office coffee machines:

**Singleton** (buy ONE machine for everyone):
- One big coffee machine in the break room
- Everyone shares it
- Save money, but gets busy
- Perfect for: Configuration, Database Connections

**Scoped** (one machine per person per day):
- Each person gets fresh coffee each morning
- Machines last all day, then get replaced
- Perfect for: Per-request services

### In Our Quota Code

```csharp
// Singleton: Create ONCE, reuse forever
services.AddSingleton<QuotaPlanResolver>();
services.AddSingleton<QuotaOptions>();
services.AddSingleton<IConnectionMultiplexer>();  // Redis connection

// Scoped: Create NEW for each HTTP request, reuse during that request
services.AddScoped<IQuotaService, RedisQuotaService>();

// Transient: Create NEW every single time (rare)
services.AddTransient<QuotaEnforcementMiddleware>();
```

**Why Different Lifetimes?**

| Type | Lifetime | Why |
|------|----------|-----|
| **QuotaOptions** | Singleton | Configuration doesn't change during request |
| **QuotaPlanResolver** | Singleton | Pure logic, no state changes |
| **IQuotaService** | Scoped | Tracks state during request, reset per request |
| **Middleware** | Transient | Created for each request handling |

### Why This Matters

✅ **Memory efficiency** — don't create objects unnecessarily
✅ **Thread safety** — request-specific state doesn't leak between requests
✅ **Performance** — reuse expensive connections (Redis)

---

## 10. THE DECORATOR/WRAPPER PATTERN (Gift Wrapping)

### Real-World Analogy

Imagine wrapping a gift:
- **Core item**: T-shirt
- **Wrapper 1**: Put in box
- **Wrapper 2**: Wrap in paper
- **Wrapper 3**: Add bow
- You can unwrap layer by layer!

Same idea in code — each layer adds responsibility.

### In Our Quota Code

```csharp
// The middleware is like a wrapper
public class QuotaEnforcementMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // LAYER 1: Check if exemption path
        if (IsExempt(context.Request.Path))
        {
            await next(context);  // Skip this wrapper
            return;
        }

        // LAYER 2: Get tenant ID
        var tenantId = _tenantAccessor.GetCurrentTenantId();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            await next(context);  // Skip this wrapper
            return;
        }

        // LAYER 3: Check quota
        var result = await _quotaService.CheckAndRecordAsync(...);
        
        if (result.Allowed)
            await next(context);  // Continue to next layer
        else
            await RejectAsync(context, result);  // Stop here
    }
}
```

### The Middleware Stack (Like Layers of Wrapping)

```
[Original Request]
    ↓
[Middleware 1: CORS wrapper] — "Allow cross-origin?"
    ↓
[Middleware 2: Auth wrapper] — "Who are you?"
    ↓
[Middleware 3: Quota wrapper] ← Our code — "Within quota?"
    ↓
[Middleware 4: Business Logic] — "Process the request"
    ↓
[Response]
```

Each layer can say "stop, don't continue" or "continue to next layer".

### Why This Matters

✅ **Separation** — each concern is separate layer
✅ **Reusable** — same middleware applied to all requests
✅ **Composable** — combine multiple middlewares in any order
✅ **Breakable** — can short-circuit at any layer

---

## 11. THE ATOMIC OPERATION PATTERN (All or Nothing)

### Real-World Analogy

Imagine transferring money from your bank account:
- **BAD**: Subtract from Account A, pause, add to Account B
  - If the system crashes in the middle, money disappears!
  
- **GOOD**: Atomic operation
  - Either BOTH happen, or NEITHER happens
  - No in-between state

### In Our Quota Code

```csharp
// BAD: Two separate operations (race condition)
var currentUsage = await GetCurrentAsync(tenantId, resource);
if (currentUsage + amount <= limit)
{
    await RecordAsync(tenantId, resource, amount);  // What if another request runs here?
}

// GOOD: One atomic operation
public async Task<QuotaCheckResult> CheckAndRecordAsync(...)
{
    // Check AND record in ONE step
    // No other request can squeeze in between!
    
    var current = await db.StringGetAsync(key);
    if (current + amount <= limit)
    {
        await db.StringIncrementAsync(key, amount);  // In Redis, this is atomic
    }
}
```

### Why This Matters

✅ **Data consistency** — prevents "dirty" states
✅ **Race condition prevention** — two requests don't interfere
✅ **Correctness** — quota is always accurate

---

## 12. THE ENUM PATTERN (Categorization)

### Real-World Analogy

Imagine a library organizing books:
- **Fiction**: Novels, short stories
- **Non-Fiction**: History, science
- **Reference**: Dictionaries, encyclopedias

Books go in ONE category. You can't put a book in two places.

### In Our Quota Code

```csharp
// Each quota resource is ONE category
public enum QuotaResource
{
    ApiCalls,       // Number of API requests
    StorageBytes,   // Storage space
    Users           // User accounts
}

// You can check a specific resource:
await _quotaService.CheckAndRecordAsync(
    tenantId,
    QuotaResource.ApiCalls,  // ← Only this ONE category
    1);

// The system can have different limits per resource:
Plans["pro"][QuotaResource.ApiCalls] = 50000;      // 50,000 API calls
Plans["pro"][QuotaResource.StorageBytes] = 1000000;  // 1 MB storage
Plans["pro"][QuotaResource.Users] = 10;            // 10 users
```

### Why This Matters

✅ **Type-safe** — compiler prevents wrong values
✅ **Clear** — limited choices (no typos)
✅ **Self-documenting** — shows all possible resources
✅ **Exhaustive** — forces you to handle all cases

---

## Summary: Patterns Work Together

```
┌─────────────────────────────────────────────────────────────┐
│  INTERFACE PATTERN (IQuotaService)                          │
│  ↓                                                           │
│  DEPENDENCY INJECTION (inject into middleware)              │
│  ↓                                                           │
│  STRATEGY PATTERN (InMemory or Redis implementation)        │
│  ↓                                                           │
│  RESOLVER PATTERN (figure out limits)                       │
│  ↓                                                           │
│  ATOMIC OPERATION (check + record together)                 │
│  ↓                                                           │
│  MIDDLEWARE PATTERN (wrap each request)                     │
│  ↓                                                           │
│  RECORD PATTERN (return result)                             │
│  ↓                                                           │
│  CONFIGURATION PATTERN (everything configurable)            │
└─────────────────────────────────────────────────────────────┘
```

Each pattern solves a specific problem. Together, they create **clean, maintainable, testable code**.

---

## Real-World Analogy: Restaurant System

**How is this quota system like a restaurant?**

1. **Interface Pattern**: "I'm a server" (IQuotaService)
   - Customers don't care if you're good or slow
   - Just that you serve them

2. **Dependency Injection**: Hiring
   - Boss hires servers and assigns tables
   - Servers just work; they don't hire themselves

3. **Strategy Pattern**: Different kitchen setups
   - Fast-casual (in-memory): Small kitchen, fast
   - High-end (Redis): Big kitchen, handles crowds

4. **Middleware Pattern**: Multiple checkpoints
   - Hostess checks reservation
   - Server checks seating availability  
   - Chef checks inventory

5. **Resolver Pattern**: Pricing tiers
   - Free plan: Appetizers only
   - Pro plan: Full menu
   - Enterprise: Custom orders

6. **Configuration Pattern**: Menu changes seasonally
   - Winter menu vs. Summer menu
   - Same restaurant, different configs

7. **Atomic Operation**: Payment processing
   - Order AND charge in one go
   - Never partial transactions

---

## Key Takeaway

These patterns are **universal**. You'll see them in:
- Web apps
- Mobile apps
- Desktop software
- Game development
- Enterprise systems

**Understanding these patterns = understanding software design**. The quota system is just one practical application!
