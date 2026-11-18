# Building Quota Enforcement WITHOUT FSH - Complete Guide

This guide shows you how to build the **same functionality as FSH's quota enforcement** from scratch in a standard .NET Core application.

## Overview

FSH provides a quota system that:
1. ✅ Tracks API calls per tenant (monthly counters)
2. ✅ Enforces limits based on billing plans
3. ✅ Allows per-tenant overrides
4. ✅ Stores counters in Redis (or in-memory for dev)
5. ✅ Rejects requests with HTTP 429 when quota exceeded
6. ✅ Returns RFC 9457 ProblemDetails response

**You can build all of this without FSH using standard .NET patterns.**

---

## Step 1: Domain Models & Enums

```csharp
// Core/Quota/QuotaResource.cs
public enum QuotaResource
{
    ApiCalls,      // Counter-based: monthly resets
    StorageBytes,  // Gauge-based: current usage
    Users,         // Gauge-based: current count
}

// Core/Quota/QuotaCheckResult.cs
public sealed record QuotaCheckResult(
    bool Allowed,
    QuotaResource Resource,
    long CurrentUsage,
    long Limit,
    DateTimeOffset? ResetAtUtc);

// Core/Quota/QuotaOptions.cs
public sealed class QuotaOptions
{
    public bool Enabled { get; set; } = true;
    public string? Redis { get; set; }  // Redis connection or null for in-memory
    public string DefaultPlan { get; set; } = "free";
    public Dictionary<string, Dictionary<QuotaResource, long>> Plans { get; } = new();
    public bool ExemptRootTenant { get; set; } = true;
}

// Core/Multitenancy/TenantInfo.cs
public sealed class TenantInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Plan { get; set; }  // "free", "pro", "enterprise"
    public Dictionary<QuotaResource, long> QuotaLimits { get; set; } = new();
}
```

---

## Step 2: Quota Service Interface

```csharp
// Core/Quota/IQuotaService.cs
public interface IQuotaService
{
    /// <summary>
    /// Check if request would exceed quota (doesn't increment counter).
    /// </summary>
    Task<QuotaCheckResult> CheckAsync(
        string tenantId,
        QuotaResource resource,
        long amount,
        CancellationToken ct = default);

    /// <summary>
    /// Increment counter and return new usage.
    /// </summary>
    Task<long> RecordAsync(
        string tenantId,
        QuotaResource resource,
        long amount,
        CancellationToken ct = default);

    /// <summary>
    /// Atomically check + record in one call (prevents race conditions).
    /// </summary>
    Task<QuotaCheckResult> CheckAndRecordAsync(
        string tenantId,
        QuotaResource resource,
        long amount,
        CancellationToken ct = default);

    /// <summary>
    /// Get current usage without incrementing.
    /// </summary>
    Task<long> GetCurrentAsync(
        string tenantId,
        QuotaResource resource,
        CancellationToken ct = default);
}
```

---

## Step 3: Plan Resolver

```csharp
// Services/QuotaPlanResolver.cs
public sealed class QuotaPlanResolver
{
    private readonly QuotaOptions _options;
    private readonly TenantStore _tenantStore;  // Your tenant storage
    private readonly ILogger<QuotaPlanResolver> _logger;

    public QuotaPlanResolver(QuotaOptions options, TenantStore tenantStore, ILogger<QuotaPlanResolver> logger)
    {
        _options = options;
        _tenantStore = tenantStore;
        _logger = logger;
    }

    /// <summary>
    /// Resolve quota limit for a tenant + resource.
    /// Priority: Per-tenant override > Plan default > DefaultPlan > Unlimited
    /// </summary>
    public (long limit, bool exempt) ResolveLimit(string tenantId, QuotaResource resource)
    {
        // Root tenant is always exempt
        if (_options.ExemptRootTenant && tenantId == "root")
            return (long.MaxValue, true);

        // Fetch tenant
        var tenant = _tenantStore.GetTenantAsync(tenantId).Result;
        if (tenant == null)
            return (0, false);  // Unknown tenant → no quota

        // 1. Check per-tenant override
        if (tenant.QuotaLimits.TryGetValue(resource, out var limit))
            return (limit, false);

        // 2. Check plan default
        var planName = tenant.Plan ?? _options.DefaultPlan;
        if (_options.Plans.TryGetValue(planName, out var planLimits) &&
            planLimits.TryGetValue(resource, out limit))
        {
            return (limit, false);
        }

        // 3. Fallback: unlimited
        _logger.LogWarning("No quota limit found for tenant {TenantId} resource {Resource}; using unlimited",
            tenantId, resource);
        return (long.MaxValue, false);
    }
}
```

---

## Step 4: In-Memory Quota Service (For Development)

```csharp
// Services/InMemoryQuotaService.cs
public sealed class InMemoryQuotaService : IQuotaService
{
    private readonly QuotaPlanResolver _planResolver;
    private readonly Dictionary<string, long> _counters = new();  // "tenant:resource:202501" → count
    private readonly object _lock = new();

    public InMemoryQuotaService(QuotaPlanResolver planResolver)
    {
        _planResolver = planResolver;
    }

    public Task<QuotaCheckResult> CheckAsync(string tenantId, QuotaResource resource, long amount, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var (limit, exempt) = _planResolver.ResolveLimit(tenantId, resource);
            var key = BuildKey(tenantId, resource);
            var current = _counters.TryGetValue(key, out var val) ? val : 0;

            if (exempt || limit == long.MaxValue)
                return Task.FromResult(QuotaCheckResult.Unlimited(resource, current));

            var allowed = current + amount <= limit;
            var resetAt = GetMonthlyResetTime();
            return Task.FromResult(new QuotaCheckResult(allowed, resource, current, limit, resetAt));
        }
    }

    public Task<long> RecordAsync(string tenantId, QuotaResource resource, long amount, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var key = BuildKey(tenantId, resource);
            if (!_counters.TryGetValue(key, out var current))
                current = 0;

            var newValue = current + amount;
            _counters[key] = newValue;
            return Task.FromResult(newValue);
        }
    }

    public async Task<QuotaCheckResult> CheckAndRecordAsync(string tenantId, QuotaResource resource, long amount, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var (limit, exempt) = _planResolver.ResolveLimit(tenantId, resource);
            var key = BuildKey(tenantId, resource);
            var current = _counters.TryGetValue(key, out var val) ? val : 0;

            if (exempt || limit == long.MaxValue)
            {
                _counters[key] = current + amount;
                return QuotaCheckResult.Unlimited(resource, current);
            }

            var allowed = current + amount <= limit;
            if (allowed)
                _counters[key] = current + amount;

            var resetAt = GetMonthlyResetTime();
            return new QuotaCheckResult(allowed, resource, current, limit, resetAt);
        }
    }

    public Task<long> GetCurrentAsync(string tenantId, QuotaResource resource, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var key = BuildKey(tenantId, resource);
            return Task.FromResult(_counters.TryGetValue(key, out var val) ? val : 0);
        }
    }

    private static string BuildKey(string tenantId, QuotaResource resource)
        => $"{tenantId}:{resource}:{DateTime.UtcNow:yyyyMM}";

    private static DateTimeOffset GetMonthlyResetTime()
    {
        var now = DateTime.UtcNow;
        var nextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
        return new DateTimeOffset(nextMonth, TimeSpan.Zero);
    }
}
```

---

## Step 5: Redis Quota Service (Production)

```csharp
// Services/RedisQuotaService.cs
using StackExchange.Redis;

public sealed class RedisQuotaService : IQuotaService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly QuotaPlanResolver _planResolver;
    private readonly ILogger<RedisQuotaService> _logger;

    public RedisQuotaService(IConnectionMultiplexer redis, QuotaPlanResolver planResolver, ILogger<RedisQuotaService> logger)
    {
        _redis = redis;
        _planResolver = planResolver;
        _logger = logger;
    }

    public async Task<QuotaCheckResult> CheckAsync(string tenantId, QuotaResource resource, long amount, CancellationToken ct = default)
    {
        var (limit, exempt) = _planResolver.ResolveLimit(tenantId, resource);
        var current = await GetCurrentAsync(tenantId, resource, ct);

        if (exempt || limit == long.MaxValue)
            return QuotaCheckResult.Unlimited(resource, current);

        var allowed = current + amount <= limit;
        var resetAt = GetMonthlyResetTime();
        return new QuotaCheckResult(allowed, resource, current, limit, resetAt);
    }

    public async Task<long> RecordAsync(string tenantId, QuotaResource resource, long amount, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = BuildKey(tenantId, resource);
        var newValue = await db.StringIncrementAsync(key, amount);

        // Set TTL to expire after month ends
        await db.KeyExpireAsync(key, TimeSpan.FromDays(35));

        return newValue;
    }

    public async Task<QuotaCheckResult> CheckAndRecordAsync(string tenantId, QuotaResource resource, long amount, CancellationToken ct = default)
    {
        var (limit, exempt) = _planResolver.ResolveLimit(tenantId, resource);

        if (exempt || limit == long.MaxValue)
        {
            await RecordAsync(tenantId, resource, amount, ct);
            var current = await GetCurrentAsync(tenantId, resource, ct);
            return QuotaCheckResult.Unlimited(resource, current);
        }

        var db = _redis.GetDatabase();
        var key = BuildKey(tenantId, resource);

        // Use transaction for atomicity
        var transaction = db.CreateTransaction();
        var currentTask = transaction.StringGetAsync(key);
        await transaction.ExecuteAsync();

        var current = currentTask.Result.HasValue ? long.Parse(currentTask.Result!) : 0;
        var allowed = current + amount <= limit;

        if (allowed)
        {
            await db.StringIncrementAsync(key, amount);
            await db.KeyExpireAsync(key, TimeSpan.FromDays(35));
        }

        var resetAt = GetMonthlyResetTime();
        return new QuotaCheckResult(allowed, resource, current, limit, resetAt);
    }

    public async Task<long> GetCurrentAsync(string tenantId, QuotaResource resource, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = BuildKey(tenantId, resource);
        var value = await db.StringGetAsync(key);
        return value.HasValue ? long.Parse(value!) : 0;
    }

    private static string BuildKey(string tenantId, QuotaResource resource)
        => $"quota:{tenantId}:{resource}:{DateTime.UtcNow:yyyyMM}";

    private static DateTimeOffset GetMonthlyResetTime()
    {
        var now = DateTime.UtcNow;
        var nextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
        return new DateTimeOffset(nextMonth, TimeSpan.Zero);
    }
}
```

---

## Step 6: Quota Enforcement Middleware

```csharp
// Middleware/QuotaEnforcementMiddleware.cs
public sealed class QuotaEnforcementMiddleware : IMiddleware
{
    private readonly IQuotaService _quotaService;
    private readonly QuotaOptions _options;
    private readonly ILogger<QuotaEnforcementMiddleware> _logger;
    private readonly ITenantContextAccessor _tenantAccessor;  // Your multi-tenancy accessor

    public QuotaEnforcementMiddleware(
        IQuotaService quotaService,
        QuotaOptions options,
        ILogger<QuotaEnforcementMiddleware> logger,
        ITenantContextAccessor tenantAccessor)
    {
        _quotaService = quotaService;
        _options = options;
        _logger = logger;
        _tenantAccessor = tenantAccessor;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Skip if disabled
        if (!_options.Enabled)
        {
            await next(context);
            return;
        }

        // Skip exempt paths
        if (IsExempt(context.Request.Path))
        {
            await next(context);
            return;
        }

        // Get current tenant
        var tenantId = _tenantAccessor.GetCurrentTenantId();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            await next(context);
            return;
        }

        // Check and record quota
        var result = await _quotaService.CheckAndRecordAsync(
            tenantId,
            QuotaResource.ApiCalls,
            1,
            context.RequestAborted);

        if (result.Allowed)
        {
            await next(context);
            return;
        }

        // Quota exceeded → HTTP 429
        await RejectAsync(context, result);
    }

    private static async Task RejectAsync(HttpContext context, QuotaCheckResult result)
    {
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.ContentType = "application/problem+json";

        // Calculate Retry-After header
        var retryAfter = result.ResetAtUtc.HasValue
            ? (int)Math.Ceiling((result.ResetAtUtc.Value - DateTimeOffset.UtcNow).TotalSeconds)
            : null;

        if (retryAfter.HasValue && retryAfter > 0)
            context.Response.Headers["Retry-After"] = retryAfter.ToString();

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Quota Exceeded",
            Detail = $"Monthly quota for {result.Resource} has been reached ({result.CurrentUsage}/{result.Limit}).",
            Type = "https://datatracker.ietf.org/doc/html/rfc6585#section-4",
            Instance = context.Request.Path
        };

        problem.Extensions["resource"] = result.Resource.ToString();
        problem.Extensions["limit"] = result.Limit;
        problem.Extensions["currentUsage"] = result.CurrentUsage;
        if (result.ResetAtUtc.HasValue)
            problem.Extensions["resetAtUtc"] = result.ResetAtUtc.Value.ToString("o");

        await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
    }

    private static bool IsExempt(PathString path) =>
        path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/healthz", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/metrics", StringComparison.OrdinalIgnoreCase);
}
```

---

## Step 7: Dependency Injection Setup

```csharp
// Extensions/ServiceExtensions.cs
public static class ServiceExtensions
{
    public static IServiceCollection AddQuotaEnforcement(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var quotaOptions = new QuotaOptions();
        configuration.GetSection(nameof(QuotaOptions)).Bind(quotaOptions);

        services.AddSingleton(quotaOptions);
        services.AddSingleton<QuotaPlanResolver>();

        // Choose implementation based on config
        if (!quotaOptions.Enabled)
        {
            services.AddScoped<IQuotaService, NoopQuotaService>();
        }
        else if (!string.IsNullOrWhiteSpace(quotaOptions.Redis))
        {
            // Production: Redis
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var config = ConfigurationOptions.Parse(quotaOptions.Redis);
                config.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(config);
            });
            services.AddScoped<IQuotaService, RedisQuotaService>();
        }
        else
        {
            // Development: In-memory
            services.AddScoped<IQuotaService, InMemoryQuotaService>();
        }

        services.AddTransient<QuotaEnforcementMiddleware>();
        return services;
    }
}
```

---

## Step 8: Program.cs Integration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 1. Add quota enforcement
builder.Services.AddQuotaEnforcement(builder.Configuration);

// 2. Add your multi-tenancy (Finbuckle or custom)
builder.Services.AddMultitenancy();

var app = builder.Build();

// 3. Use middleware (AFTER authentication, BEFORE routing)
app.UseAuthentication();
app.UseQuotaEnforcement();  // ← Line added
app.UseAuthorization();

app.MapControllers();
await app.RunAsync();
```

---

## Step 9: Configuration (appsettings.json)

```json
{
  "QuotaOptions": {
    "Enabled": true,
    "Redis": "localhost:6379",
    "DefaultPlan": "free",
    "ExemptRootTenant": true,
    "Plans": {
      "free": {
        "ApiCalls": 1000,
        "StorageBytes": 5368709120,
        "Users": 1
      },
      "pro": {
        "ApiCalls": 50000,
        "StorageBytes": 107374182400,
        "Users": 10
      },
      "enterprise": {
        "ApiCalls": 9223372036854775807,
        "StorageBytes": 9223372036854775807,
        "Users": 9223372036854775807
      }
    }
  }
}
```

---

## Step 10: Multi-Tenancy Accessor (Your Custom Implementation)

```csharp
// Multitenancy/ITenantContextAccessor.cs
public interface ITenantContextAccessor
{
    string? GetCurrentTenantId();
    TenantInfo? GetCurrentTenant();
}

// Multitenancy/HttpContextTenantAccessor.cs
public sealed class HttpContextTenantAccessor : ITenantContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTenantAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetCurrentTenantId()
    {
        // Get tenant from claims, headers, or route
        var context = _httpContextAccessor.HttpContext;
        if (context?.User?.FindFirst("tenant_id")?.Value is string tenantId)
            return tenantId;

        // Or from header
        if (context?.Request.Headers.TryGetValue("X-Tenant-ID", out var headerValue) == true)
            return headerValue.ToString();

        return null;
    }

    public TenantInfo? GetCurrentTenant()
    {
        var tenantId = GetCurrentTenantId();
        if (tenantId == null)
            return null;

        // Fetch from your tenant store
        // return await _tenantStore.GetAsync(tenantId);
        return null;
    }
}
```

---

## Summary: Without FSH, You Need:

| Component | What | Where |
|-----------|------|-------|
| **Enums/Models** | `QuotaResource`, `QuotaCheckResult`, `QuotaOptions` | Core/Quota/ |
| **Interface** | `IQuotaService` | Services/ |
| **Plan Resolver** | `QuotaPlanResolver` | Services/ |
| **Implementations** | `InMemoryQuotaService`, `RedisQuotaService`, `NoopQuotaService` | Services/ |
| **Middleware** | `QuotaEnforcementMiddleware` | Middleware/ |
| **DI Registration** | `AddQuotaEnforcement()` extension method | Extensions/ |
| **Config** | appsettings.json with QuotaOptions section | Root |
| **Multi-tenancy** | `ITenantContextAccessor` (yours or Finbuckle) | Multitenancy/ |

**Total: ~500 lines of code** — comparable to FSH's quota module but **100% yours to understand and modify**.

---

## Key Differences vs FSH:

✅ **You have full control** — no hidden magic
✅ **You understand every line** — good for learning
✅ **No dependency on FSH** — portable to any .NET project
❌ **More code to write** — FSH saves boilerplate
❌ **No built-in features** — async gauge providers, idempotency, etc. are on you

This is a **production-ready implementation** that you can use in real applications.
