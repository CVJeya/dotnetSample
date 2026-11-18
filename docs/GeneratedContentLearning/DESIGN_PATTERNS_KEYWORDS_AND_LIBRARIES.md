# Design Patterns: Keywords, Libraries & Learning Path

This document provides:
1. **Keywords** used across design patterns
2. **Libraries** that enable design patterns in .NET
3. **Learning progression** from basics to advanced

---

## Part 1: Design Pattern Keywords

### A. Core OOP Keywords

#### **Interface** 
- **Keyword:** `interface`
- **Purpose:** Contract definition
- **Used in:** Interface Pattern, Adapter Pattern, Bridge Pattern
- **Example:**
```csharp
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(string id);
}
```

#### **Abstract**
- **Keyword:** `abstract`
- **Purpose:** Define shared behavior in base class
- **Used in:** Template Method Pattern, Abstract Factory
- **Example:**
```csharp
public abstract class Specification<T>
{
    protected void Where(Expression<Func<T, bool>> expression) { }
}
```

#### **Virtual & Override**
- **Keywords:** `virtual`, `override`
- **Purpose:** Polymorphism and method override
- **Used in:** Strategy Pattern, Decorator Pattern, Template Method
- **Example:**
```csharp
public virtual string BuildKey() { }
public override string BuildKey() { return "custom"; }
```

#### **Sealed**
- **Keyword:** `sealed`
- **Purpose:** Prevent inheritance (security/performance)
- **Used in:** Proxy Pattern, Decorator Pattern (prevent unintended subclassing)
- **Example:**
```csharp
public sealed class RedisQuotaService : IQuotaService { }
```

#### **Internal**
- **Keyword:** `internal`
- **Purpose:** Hide implementation details
- **Used in:** Adapter Pattern, Facade Pattern
- **Example:**
```csharp
internal sealed class S3StorageService : IStorageService { }
```

---

### B. Generic & Constraint Keywords

#### **Generic<T>**
- **Keyword:** `<T>`, `<TKey, TValue>`
- **Purpose:** Type-safe abstraction
- **Used in:** Repository Pattern, Specification Pattern, Factory Pattern
- **Example:**
```csharp
public interface IRepository<T> where T : class { }
public class Specification<T> where T : class { }
```

#### **Where (Constraint)**
- **Keyword:** `where`
- **Purpose:** Constrain generic type parameter
- **Used in:** Most patterns with generics
- **Examples:**
```csharp
where T : class              // Must be reference type
where T : IEntity           // Must implement interface
where T : new()             // Must have parameterless constructor
where TKey : IComparable    // Must implement IComparable
```

---

### C. Async & Concurrency Keywords

#### **Async & Await**
- **Keywords:** `async`, `await`
- **Purpose:** Non-blocking asynchronous operations
- **Used in:** Observer Pattern, Pipeline Behavior, Event Handlers
- **Example:**
```csharp
public async Task<QuotaCheckResult> CheckAndRecordAsync(...)
{
    var result = await _service.CheckAsync(...);
    return result;
}
```

#### **Task & ValueTask**
- **Keywords:** `Task`, `Task<T>`, `ValueTask<T>`
- **Purpose:** Represent asynchronous operations
- **Used in:** All async patterns (CQRS, Mediator, Pipeline)
- **Example:**
```csharp
public async ValueTask<TResponse> Handle(TMessage message, ...)
{
    return await next(message, cancellationToken);
}
```

#### **CancellationToken**
- **Keyword:** `CancellationToken`
- **Purpose:** Graceful cancellation of async operations
- **Used in:** Repository Pattern, Service calls
- **Example:**
```csharp
public async Task<T?> GetByIdAsync(string id, CancellationToken ct = default)
{
    return await context.Set<T>().FindAsync([id], cancellationToken: ct);
}
```

---

### D. Functional Programming Keywords

#### **Func & Action**
- **Keywords:** `Func<T, TResult>`, `Action<T>`
- **Purpose:** Delegate types for functional composition
- **Used in:** Strategy Pattern, Resolver Pattern, Pipeline
- **Example:**
```csharp
public Func<T, object>? CreateProjection { get; set; }
public Action<T>? BeforeFilter { get; set; }
```

#### **Expression Trees**
- **Keyword:** `Expression<Func<T, TResult>>`
- **Purpose:** Represent code as data (runtime compilation)
- **Used in:** Specification Pattern, LINQ queries
- **Example:**
```csharp
public Expression<Func<T, bool>> Criteria { get; }
protected void Where(Expression<Func<T, bool>> expression) { }
```

#### **Lambda Expressions**
- **Keyword:** `=>`
- **Purpose:** Concise anonymous functions
- **Used in:** Specifications, Resolvers, Factories
- **Example:**
```csharp
products.Where(p => p.Price > 100)
products.OrderBy(p => p.Name)
_products.Add(p => p.Category.Id == categoryId)
```

---

### E. Pattern-Specific Keywords

#### **Delegate**
- **Keyword:** `delegate`
- **Purpose:** Type-safe function pointer
- **Used in:** Observer Pattern, Event Handlers
- **Example:**
```csharp
public delegate void DomainEventHandler(IDomainEvent @event);
```

#### **Event**
- **Keyword:** `event`
- **Purpose:** Encapsulated publish-subscribe
- **Used in:** Observer Pattern, Domain Events
- **Example:**
```csharp
public event EventHandler<DomainEventArgs>? DomainEventPublished;
```

#### **Params**
- **Keyword:** `params`
- **Purpose:** Variable-length argument list
- **Used in:** Factory Pattern, Builder Pattern
- **Example:**
```csharp
builder.AddModules(params Assembly[] assemblies)
```

#### **Out & Ref**
- **Keywords:** `out`, `ref`
- **Purpose:** Pass by reference
- **Used in:** Factory Pattern (out parameters), Resolver Pattern
- **Example:**
```csharp
public bool TryResolveLimit(string tenantId, out long limit) { }
```

#### **Null Coalescing & Null Conditional**
- **Keywords:** `??`, `?.`
- **Purpose:** Null-safe operations
- **Used in:** All patterns (defensive coding)
- **Example:**
```csharp
var plan = tenant.Plan ?? _options.DefaultPlan;
var result = context?.User?.FindFirst("sub");
```

---

### F. Metadata Keywords

#### **Attribute**
- **Keyword:** `[Attribute]`
- **Purpose:** Add metadata to code
- **Used in:** Marker Interfaces, AOP, Authorization
- **Example:**
```csharp
[FshModule(Name = "Identity", Order = 1)]
public class IdentityModule : IModule { }

[RequiredPermission("users.read")]
public class GetUsersQuery { }
```

#### **Reflection Keywords**
- **Keyword:** `typeof()`, `.GetType()`, `reflection`
- **Purpose:** Inspect code at runtime
- **Used in:** Factory Pattern, Module Loader, DI Container
- **Example:**
```csharp
var assemblies = AppDomain.CurrentDomain.GetAssemblies();
var moduleTypes = assemblies.SelectMany(a => a.GetCustomAttributes<FshModuleAttribute>());
```

---

### G. SOLID Principle Keywords

#### **Single Responsibility**
- **Keywords:** `interface`, `sealed class`, `internal`
- **Pattern:** One class = one reason to change
- **Example:** `ValidationBehavior` only handles validation

#### **Open/Closed**
- **Keywords:** `abstract`, `interface`, `virtual`
- **Pattern:** Open for extension (inheritance), closed for modification
- **Example:** `IQuotaService` interface allows new implementations

#### **Liskov Substitution**
- **Keyword:** `interface`, `base class`
- **Pattern:** Subtype must substitute base type
- **Example:** `RedisQuotaService` and `InMemoryQuotaService` are interchangeable

#### **Interface Segregation**
- **Keyword:** `interface` (specific, not fat)
- **Pattern:** Many specific interfaces > one fat interface
- **Example:** `IRepository<T>` vs `IDataAccessService`

#### **Dependency Inversion**
- **Keywords:** `interface`, `dependency injection`
- **Pattern:** Depend on abstractions, not concretions
- **Example:** Depend on `IQuotaService`, not concrete implementation

---

## Part 2: Libraries Used in FSH Design Patterns

### A. Core Framework Libraries

| Library | Purpose | Pattern Used |
|---------|---------|--------------|
| **Microsoft.EntityFrameworkCore** | ORM, Database abstraction | Repository, Specification, Unit of Work |
| **Microsoft.AspNetCore** | Web framework | Middleware, Pipeline Behavior |
| **Microsoft.Extensions.DependencyInjection** | Dependency Injection container | Dependency Injection, Factory |
| **Microsoft.Extensions.Configuration** | Configuration management | Configuration Pattern, Strategy |
| **Microsoft.Extensions.Caching.Hybrid** | Hybrid caching (L1 + L2) | Decorator, Strategy |

### B. Cross-Cutting Concern Libraries

| Library | Purpose | Pattern Used |
|---------|---------|--------------|
| **Serilog** | Structured logging | Observer, Pipeline Behavior |
| **FluentValidation** | Input validation | Pipeline Behavior, Specification |
| **Mediator** | CQRS mediator | Mediator, Command, CQRS |
| **Finbuckle.MultiTenant** | Multi-tenancy support | Proxy, Resolver |
| **OpenTelemetry** | Observability (tracing) | Observer, Interceptor |

### C. Storage & Caching Libraries

| Library | Purpose | Pattern Used |
|---------|---------|--------------|
| **StackExchange.Redis** | Redis client | Strategy, Decorator |
| **AWSSDK.S3** | AWS S3 storage | Adapter, Strategy, Factory |
| **Npgsql** | PostgreSQL driver | Repository, Unit of Work |

### D. Background Job Libraries

| Library | Purpose | Pattern Used |
|---------|---------|--------------|
| **Hangfire** | Background job scheduler | Observer, Command, Factory |
| **Hangfire.PostgreSql** | PostgreSQL storage for Hangfire | Strategy |

### E. Testing Libraries

| Library | Purpose | Pattern Used |
|---------|---------|--------------|
| **xUnit** | Unit testing framework | Specification (test patterns) |
| **NSubstitute** | Mocking library | Proxy, Decorator (in tests) |
| **Shouldly** | Fluent assertions | (Test pattern) |
| **AutoFixture** | Test data generation | Factory (test data) |
| **Testcontainers** | Docker containers for tests | Strategy (DB per test) |
| **NetArchTest** | Architecture rule testing | (Architecture validation) |

### F. Authentication & Security Libraries

| Library | Purpose | Pattern Used |
|---------|---------|--------------|
| **Microsoft.AspNetCore.Identity** | User management | Repository, Factory |
| **System.IdentityModel.Tokens.Jwt** | JWT handling | Adapter, Command |
| **Microsoft.AspNetCore.DataProtection** | Data encryption | Strategy, Decorator |

### G. Utility Libraries

| Library | Purpose | Pattern Used |
|---------|---------|--------------|
| **AutoMapper** | Object mapping | Adapter, Decorator |
| **MimeKit** | Email handling | Adapter, Factory |
| **Spectre.Console** | CLI output formatting | Decorator, Builder |

---

## Part 3: Learning Path - Basics to Advanced

### Phase 1: Fundamentals (Week 1)

#### Goal
Understand basic OOP concepts and simple patterns

#### Keywords to Know
- `interface`, `class`, `abstract`
- `public`, `private`, `internal`
- `virtual`, `override`, `sealed`
- `new` (keyword and operator)

#### Libraries to Install
```bash
dotnet add package Microsoft.Extensions.DependencyInjection
```

#### Patterns to Learn
1. **Interface Pattern**
   - What: Contracts
   - Why: Abstraction
   - How: `interface IService { }`

2. **Dependency Injection**
   - What: Invert object creation
   - Why: Loose coupling
   - How: `services.AddScoped<IService, ServiceImpl>()`

3. **Strategy Pattern**
   - What: Swap algorithms
   - Why: Runtime behavior change
   - How: `if (condition) return strategyA else strategyB`

#### Exercises
```csharp
// Exercise 1: Create an interface and two implementations
public interface ICalculator
{
    int Calculate(int a, int b);
}

public class Adder : ICalculator
{
    public int Calculate(int a, int b) => a + b;
}

public class Multiplier : ICalculator
{
    public int Calculate(int a, int b) => a * b;
}

// Exercise 2: Use DI to inject strategy
public class MathService
{
    private readonly ICalculator _calculator;
    public MathService(ICalculator calculator) => _calculator = calculator;
    public int Compute(int a, int b) => _calculator.Calculate(a, b);
}

// Exercise 3: Register and use
services.AddScoped<ICalculator, Adder>();
var service = serviceProvider.GetRequiredService<MathService>();
```

---

### Phase 2: Intermediate (Week 2-3)

#### Goal
Understand async, generics, and repository pattern

#### Keywords to Learn
- `async`, `await`, `Task<T>`
- `<T>`, `where T : class`
- `Expression<Func<T, bool>>`
- `lambda`, `=>`, `?.`, `??`

#### Libraries to Install
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package FluentValidation
```

#### Patterns to Learn
4. **Repository Pattern**
   - What: Abstract data access
   - Why: Swap database implementations
   - How: `public interface IRepository<T>`

5. **Specification Pattern**
   - What: Encapsulate query logic
   - Why: Reuse complex queries
   - How: `public class UsersByAgeSpec : Specification<User>`

6. **CQRS (Command Query Responsibility Segregation)**
   - What: Separate read/write operations
   - Why: Optimize independently
   - How: `Command` and `Query` classes

#### Exercises
```csharp
// Exercise 1: Generic Repository
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IList<T>> GetListAsync(ISpecification<T> spec, CancellationToken ct);
    Task AddAsync(T entity, CancellationToken ct);
}

// Exercise 2: Specification with Expression Trees
public class UsersByAgeSpecification : Specification<User>
{
    public UsersByAgeSpecification(int minAge)
    {
        Where(u => u.Age >= minAge);
        OrderBy(u => u.Name);
    }
}

// Exercise 3: CQRS Commands and Queries
public class CreateUserCommand : ICommand<string>
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class GetUserQuery : IQuery<UserDto>
{
    public int UserId { get; set; }
}
```

---

### Phase 3: Advanced Architecture (Week 4-5)

#### Goal
Understand middleware, events, and interceptors

#### Keywords to Learn
- `event`, `delegate`
- `IMiddleware`, `RequestDelegate`
- `[Attribute]`, `reflection`
- `Interceptor`

#### Libraries to Install
```bash
dotnet add package Mediator
dotnet add package Serilog
```

#### Patterns to Learn
7. **Middleware Pattern**
   - What: Chain of request handlers
   - Why: Separation of concerns
   - How: `public class AuthMiddleware : IMiddleware`

8. **Pipeline Behavior (AOP)**
   - What: Cross-cutting concerns
   - Why: Apply logic to all requests
   - How: `IPipelineBehavior<TRequest, TResponse>`

9. **Domain Event Pattern**
   - What: In-process events
   - Why: Async, decoupled side effects
   - How: `entity.AddDomainEvent(new UserCreatedEvent())`

10. **Interceptor Pattern**
    - What: Hook into ORM lifecycle
    - Why: Automatic behavior (audit, soft delete)
    - How: `SaveChangesInterceptor`

#### Exercises
```csharp
// Exercise 1: Custom Middleware
public class LoggingMiddleware : IMiddleware
{
    private readonly ILogger _logger;
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
        await next(context);
        _logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
    }
}

// Exercise 2: Pipeline Behavior
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IValidator<TRequest> _validator;
    
    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);
        
        return await next(request, cancellationToken);
    }
}

// Exercise 3: Domain Events
public class User : BaseEntity<int>
{
    public string Email { get; set; }
    
    public static User Create(string email)
    {
        var user = new User { Email = email };
        user.AddDomainEvent(new UserCreatedEvent { UserId = user.Id });
        return user;
    }
}

// Exercise 4: EF Core Interceptor
public class AuditInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
```

---

### Phase 4: Enterprise Patterns (Week 6-7)

#### Goal
Master composition and real-world scenarios

#### Keywords to Learn
- `sealed`, `internal`
- `[Attribute]` for metadata
- Reflection patterns
- `params`, `out`

#### Libraries to Install
```bash
dotnet add package Finbuckle.MultiTenant
dotnet add package OpenTelemetry
```

#### Patterns to Learn
11. **Multi-Tenancy Pattern**
    - What: Tenant isolation at all layers
    - Why: SaaS architecture
    - How: Finbuckle + EF Core IsMultiTenant()

12. **Decorator Pattern (Advanced)**
    - What: Wrap objects to add behavior
    - Why: Composition > inheritance
    - How: Caching decorator over repository

13. **Factory Pattern**
    - What: Create objects without `new`
    - Why: Encapsulate creation logic
    - How: `Activator.CreateInstance()`, reflection

14. **Observer Pattern (Advanced)**
    - What: Publish-subscribe events
    - Why: Loose coupling, async processing
    - How: `IEventBus`, domain events

#### Exercises
```csharp
// Exercise 1: Multi-Tenancy
public class TenantAwareRepository<T> : IRepository<T> where T : class, IHasTenant
{
    private readonly IRepository<T> _inner;
    private readonly ITenantContext _tenantContext;
    
    public async Task<T?> GetByIdAsync(int id, CancellationToken ct)
    {
        // Automatically filter by tenant
        var tenantId = _tenantContext.GetTenantId();
        var spec = new Specification<T>();
        spec.Where(x => x.TenantId == tenantId && x.Id == id);
        return await _inner.GetAsync(spec, ct);
    }
}

// Exercise 2: Decorator for Caching
public class CachedRepository<T> : IRepository<T>
{
    private readonly IRepository<T> _inner;
    private readonly IDistributedCache _cache;
    
    public async Task<T?> GetByIdAsync(int id, CancellationToken ct)
    {
        var key = $"repo:{typeof(T).Name}:{id}";
        var cached = await _cache.GetStringAsync(key, ct);
        if (cached != null)
            return JsonSerializer.Deserialize<T>(cached);
        
        var result = await _inner.GetByIdAsync(id, ct);
        if (result != null)
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(result), ct: ct);
        
        return result;
    }
}

// Exercise 3: Factory with Reflection
public class RepositoryFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public IRepository<T> CreateRepository<T>() where T : class
    {
        var innerType = typeof(Repository<T>);
        var inner = (IRepository<T>)ActivatorUtilities.CreateInstance(_serviceProvider, innerType);
        
        var cacheType = typeof(CachedRepository<T>);
        return (IRepository<T>)ActivatorUtilities.CreateInstance(_serviceProvider, cacheType, inner);
    }
}

// Exercise 4: Event Bus with Observer
public interface IEventHandler<T> where T : IDomainEvent
{
    Task HandleAsync(T @event, CancellationToken ct);
}

public class EventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();
    
    public void Subscribe<T>(Func<T, Task> handler) where T : IDomainEvent
    {
        var type = typeof(T);
        if (!_handlers.ContainsKey(type))
            _handlers[type] = new List<Delegate>();
        _handlers[type].Add(handler);
    }
    
    public async Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IDomainEvent
    {
        var type = typeof(T);
        if (!_handlers.TryGetValue(type, out var handlers))
            return;
        
        foreach (var handler in handlers.Cast<Func<T, Task>>())
            await handler(@event);
    }
}
```

---

### Phase 5: Mastery & System Design (Week 8+)

#### Goal
Design complete systems using pattern combinations

#### Advanced Topics
- Pattern composition (3+ patterns working together)
- Anti-patterns (what NOT to do)
- Performance optimization
- Testability and mockability
- Security by design

#### Real-World Scenarios

**Scenario 1: User Registration Flow**
```
Command (CreateUserCommand)
  ↓ Mediator
ValidationBehavior (validates input)
  ↓ LoggingBehavior (logs operation)
CommandHandler
  ↓ Factory (creates User aggregate)
User.Create() raises UserCreatedEvent
  ↓ SaveChangesAsync (with interceptor)
AuditInterceptor (sets CreatedBy, CreatedAt)
  ↓ DomainEventsInterceptor
PublishAsync (publishes UserCreatedEvent)
  ↓ Observer Pattern
SendWelcomeEmailHandler.HandleAsync()
  ↓ Email sent
```

**Scenario 2: Data Query with Caching**
```
Query (GetProductsQuery)
  ↓ Mediator
CachingBehavior (check L1/L2 cache)
  ↓ QueryHandler
CachedRepository<Product> (decorator)
  ↓ Repository<Product> (abstract data access)
SpecificationEvaluator
  ↓ ProductsByPriceRangeSpecification
EF Core Query
  ↓ Database
Result cached (L1 + L2)
```

**Scenario 3: Multi-Tenant API Request**
```
HTTP Request
  ↓ TenantResolutionMiddleware (Finbuckle)
CurrentTenant = resolved
  ↓ AuthMiddleware
User authenticated
  ↓ QuotaEnforcementMiddleware
CheckAndRecord (atomic)
  ↓ QuotaPlanResolver
Limit determined
  ↓ Handler/Endpoint
TenantAwareRepository<T>
  ↓ Automatic WHERE tenant_id = current_tenant
Response
```

---

## Part 4: Pattern Keywords Quick Reference

### By Frequency of Use

| Rank | Keyword | Pattern | Context |
|------|---------|---------|---------|
| 1 | `interface` | Interface | ALL |
| 2 | `async/await` | CQRS, Pipeline | ALL |
| 3 | `<T>` (generic) | Repository, Specification | Most |
| 4 | `abstract` | Template Method, Factory | Common |
| 5 | `virtual/override` | Strategy, Decorator | Common |
| 6 | `Expression<Func>` | Specification | Query |
| 7 | `where (constraint)` | Generic patterns | Most |
| 8 | `Task<T>` | Async patterns | ALL |
| 9 | `event/delegate` | Observer, Domain Events | Async |
| 10 | `sealed` | Security, Performance | Advanced |
| 11 | `internal` | Encapsulation | Advanced |
| 12 | `[Attribute]` | Metadata, AOP | Advanced |
| 13 | `reflection` | Factory, Module Loader | Advanced |
| 14 | `Func/Action` | Strategy, Pipeline | Functional |
| 15 | `lambda (=>)` | Query, Builder | ALL |

---

## Part 5: Library Installation Checklist

### Essential (Week 1)
```bash
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.AspNetCore
```

### Core (Week 2-3)
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package FluentValidation
dotnet add package Mediator
```

### Advanced (Week 4-5)
```bash
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
dotnet add package OpenTelemetry
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
```

### Enterprise (Week 6+)
```bash
dotnet add package Finbuckle.MultiTenant
dotnet add package Finbuckle.MultiTenant.EntityFrameworkCore
dotnet add package Hangfire
dotnet add package AutoMapper
```

### Testing (Throughout)
```bash
dotnet add package xunit
dotnet add package NSubstitute
dotnet add package Shouldly
dotnet add package AutoFixture
dotnet add package Testcontainers.PostgreSql
dotnet add package NetArchTest.Rules
```

---

## Part 6: Common Pattern Keywords by Category

### Object Creation
| Keyword | Pattern |
|---------|---------|
| `new` | Factory, Builder |
| `Activator.CreateInstance()` | Reflection Factory |
| `<T> where new()` | Generic Factory |
| `ServiceProvider.GetRequiredService<T>()` | DI Factory |

### Behavior Abstraction
| Keyword | Pattern |
|---------|---------|
| `interface` | Strategy, Adapter, Bridge |
| `abstract class` | Template Method |
| `virtual/override` | Method Override |
| `delegate` | Observer, Callback |

### Data Access
| Keyword | Pattern |
|---------|---------|
| `IQueryable<T>` | Specification, LINQ |
| `Expression<Func<T, bool>>` | Query filter |
| `DbSet<T>` | Repository |
| `DbContext` | Unit of Work |

### Async & Events
| Keyword | Pattern |
|---------|---------|
| `async Task` | Async operations |
| `await` | Async wait |
| `event` | Observer, Pub/Sub |
| `TaskCompletionSource<T>` | Promise pattern |

### Configuration & Metadata
| Keyword | Pattern |
|---------|---------|
| `[Attribute]` | Configuration |
| `GetCustomAttribute()` | Reflection |
| `IOptions<T>` | Configuration |
| `ConfigureServices()` | DI setup |

---

## Part 7: Learning Progression Summary

```
Week 1: BASICS
  Keywords: interface, class, virtual, abstract
  Patterns: Interface, DI, Strategy
  Goal: Understand abstraction
  
Week 2-3: INTERMEDIATE
  Keywords: async/await, <T>, Expression<Func<T>>
  Patterns: Repository, Specification, CQRS
  Goal: Build query systems
  
Week 4-5: ADVANCED
  Keywords: event, delegate, IMiddleware
  Patterns: Middleware, Pipeline, Domain Events, Interceptor
  Goal: Handle cross-cutting concerns
  
Week 6-7: ENTERPRISE
  Keywords: sealed, internal, [Attribute], reflection
  Patterns: Multi-Tenancy, Decorator, Factory, Observer
  Goal: System design with constraints
  
Week 8+: MASTERY
  Pattern composition, anti-patterns, optimization
  Real-world scenarios, security, performance
  Goal: Design enterprise systems
```

---

## Part 8: Practice Projects by Level

### Level 1: Keyword Practice
Create a calculator with DI:
- Use `interface ICalculator`
- Create two implementations
- Use `async Task<int> CalculateAsync()`
- Register with DI container

### Level 2: Repository Practice
Build a product repository:
- Implement `IRepository<Product>`
- Create `ProductsByPriceSpec`
- Use `Expression<Func<Product, bool>>` for filtering
- Add async methods

### Level 3: Middleware Practice
Create logging + validation middleware:
- Implement `IMiddleware`
- Add `async Task InvokeAsync()`
- Chain multiple middlewares
- Test with unit tests

### Level 4: CQRS Practice
Implement user workflow:
- `CreateUserCommand` + handler
- `GetUserQuery` + handler
- Add `ValidationBehavior`
- Add `LoggingBehavior`

### Level 5: Enterprise System
Build multi-tenant feature:
- Use Finbuckle for tenant resolution
- Implement `TenantAwareRepository<T>`
- Add domain events
- EF Core interceptors for audit
- Caching decorator
- Integration tests

---

## Conclusion

Design patterns use a rich vocabulary of keywords and rely on specific libraries. Understanding them requires:

1. **Keywords First** — Grasp C# syntax and semantics
2. **Libraries Second** — Know what tools are available
3. **Patterns Third** — See keywords in action
4. **Composition Fourth** — Combine patterns
5. **Mastery Fifth** — Design complete systems

The learning path from basics to advanced typically takes **8-12 weeks** with dedicated practice.

---

**Created:** January 21, 2025
**Target Audience:** Beginners to Intermediate .NET developers
**Scope:** Keywords, Libraries, Learning progression
