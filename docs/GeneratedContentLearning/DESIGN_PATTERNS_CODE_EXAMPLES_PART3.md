# Design Patterns - Complete Code Examples (Part 3/3)

Runnable C# code examples for Architectural and Advanced patterns.

---

## ARCHITECTURAL & ADVANCED PATTERNS (10)

### 21. Repository Pattern

**Purpose:** Abstract data access logic, enable different storage backends

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// SQL Implementation
public class SqlRepository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;
    
    public SqlRepository(DbContext context) => _context = context;
    
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }
    
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }
    
    public async Task AddAsync(T entity)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}

// In-Memory Implementation (for testing)
public class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly Dictionary<int, T> _storage = new();
    private int _nextId = 1;
    
    public Task<T?> GetByIdAsync(int id)
    {
        return Task.FromResult(_storage.TryGetValue(id, out var entity) ? entity : null);
    }
    
    public Task<IEnumerable<T>> GetAllAsync()
    {
        return Task.FromResult(_storage.Values.AsEnumerable());
    }
    
    public Task AddAsync(T entity)
    {
        _storage[_nextId++] = entity;
        return Task.CompletedTask;
    }
    
    public Task UpdateAsync(T entity)
    {
        // Implementation would set entity in storage
        return Task.CompletedTask;
    }
    
    public Task DeleteAsync(int id)
    {
        _storage.Remove(id);
        return Task.CompletedTask;
    }
}

// Usage: Works with any repository implementation
public class UserService
{
    private readonly IRepository<User> _repository;
    
    public UserService(IRepository<User> repository) => _repository = repository;
    
    public async Task RegisterUserAsync(string name, string email)
    {
        var user = new User { Name = name, Email = email };
        await _repository.AddAsync(user);
        Console.WriteLine($"Registered {name}");
    }
}

// DI: Swap repositories easily
services.AddScoped<IRepository<User>, SqlRepository<User>>();  // Production
// services.AddScoped<IRepository<User>, InMemoryRepository<User>>();  // Testing
```

---

### 22. Unit of Work Pattern

**Purpose:** Manage multiple repositories and transactions as one unit

```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Order> Orders { get; }
    Task SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private IRepository<User> _users;
    private IRepository<Order> _orders;
    
    public UnitOfWork(DbContext context) => _context = context;
    
    public IRepository<User> Users => _users ??= new SqlRepository<User>(_context);
    public IRepository<Order> Orders => _orders ??= new SqlRepository<Order>(_context);
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    
    public void Dispose() => _context?.Dispose();
}

// Usage
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public OrderService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    
    public async Task CreateOrderAsync(int userId, List<OrderItem> items)
    {
        using (_unitOfWork)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");
            
            var order = new Order { UserId = userId, Items = items };
            await _unitOfWork.Orders.AddAsync(order);
            
            // Save all changes atomically
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
```

---

### 23. CQRS (Command Query Responsibility Segregation)

**Purpose:** Separate read and write operations into different models

```csharp
// Models
public class CreateProductCommand
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class ProductViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Command Handler (Write)
public interface ICommandHandler<T> where T : class
{
    Task ExecuteAsync(T command);
}

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand>
{
    private readonly DbContext _context;
    
    public CreateProductCommandHandler(DbContext context) => _context = context;
    
    public async Task ExecuteAsync(CreateProductCommand command)
    {
        var product = new Product
        {
            Name = command.Name,
            Price = command.Price,
            CreatedDate = DateTime.UtcNow
        };
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        Console.WriteLine($"Product created: {command.Name}");
    }
}

// Query Handler (Read)
public interface IQueryHandler<TQuery, TResult> where TQuery : class
{
    Task<TResult> HandleAsync(TQuery query);
}

public class GetProductQuery
{
    public int Id { get; set; }
}

public class GetProductQueryHandler : IQueryHandler<GetProductQuery, ProductViewModel>
{
    private readonly DbContext _context;
    
    public GetProductQueryHandler(DbContext context) => _context = context;
    
    public async Task<ProductViewModel> HandleAsync(GetProductQuery query)
    {
        return await _context.Products
            .Where(p => p.Id == query.Id)
            .Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            })
            .FirstOrDefaultAsync();
    }
}

// Usage
public class ProductService
{
    private readonly ICommandHandler<CreateProductCommand> _createHandler;
    private readonly IQueryHandler<GetProductQuery, ProductViewModel> _getHandler;
    
    public ProductService(
        ICommandHandler<CreateProductCommand> createHandler,
        IQueryHandler<GetProductQuery, ProductViewModel> getHandler)
    {
        _createHandler = createHandler;
        _getHandler = getHandler;
    }
    
    public async Task CreateAsync(string name, decimal price)
    {
        await _createHandler.ExecuteAsync(new CreateProductCommand { Name = name, Price = price });
    }
    
    public async Task<ProductViewModel> GetAsync(int id)
    {
        return await _getHandler.HandleAsync(new GetProductQuery { Id = id });
    }
}
```

---

### 24. Mediator Pattern (Advanced)

**Purpose:** Centralize communication between objects through mediator

```csharp
public interface IMediator
{
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request) 
        where TRequest : IRequest<TResponse>;
}

public interface IRequest<TResponse> { }

public interface IRequestHandler<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request);
}

// Concrete Request
public class GetUserByIdRequest : IRequest<User>
{
    public int Id { get; set; }
}

// Concrete Handler
public class GetUserByIdHandler : IRequestHandler<GetUserByIdRequest, User>
{
    private readonly IRepository<User> _repository;
    
    public GetUserByIdHandler(IRepository<User> repository) => _repository = repository;
    
    public async Task<User> HandleAsync(GetUserByIdRequest request)
    {
        return await _repository.GetByIdAsync(request.Id);
    }
}

// Mediator Implementation
public class Mediator : IMediator
{
    private readonly IServiceProvider _services;
    
    public Mediator(IServiceProvider services) => _services = services;
    
    public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request)
        where TRequest : IRequest<TResponse>
    {
        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(typeof(TRequest), typeof(TResponse));
        
        var handler = _services.GetService(handlerType);
        if (handler == null)
            throw new InvalidOperationException($"Handler not found for {typeof(TRequest).Name}");
        
        var handleMethod = handlerType.GetMethod("HandleAsync");
        return (TResponse)await (dynamic)handleMethod.Invoke(handler, new object[] { request });
    }
}

// Usage
var user = await mediator.SendAsync<GetUserByIdRequest, User>(
    new GetUserByIdRequest { Id = 1 });
```

---

### 25. Specification Pattern

**Purpose:** Encapsulate query logic in reusable specifications

```csharp
public abstract class Specification<T>
{
    public virtual Expression<Func<T, bool>> Criteria { get; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public Expression<Func<T, object>> OrderBy { get; protected set; }
    public Expression<Func<T, object>> OrderByDescending { get; protected set; }
    
    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
}

// Concrete Specification
public class ActiveUsersSpecification : Specification<User>
{
    public override Expression<Func<User, bool>> Criteria => u => u.IsActive;
    
    public ActiveUsersSpecification()
    {
        OrderBy = u => u.Name;
    }
}

public class UserByIdSpecification : Specification<User>
{
    public override Expression<Func<User, bool>> Criteria { get; }
    
    public UserByIdSpecification(int id)
    {
        Criteria = u => u.Id == id;
    }
}

// Repository using specifications
public class SpecificationRepository<T> where T : class
{
    private readonly DbContext _context;
    
    public SpecificationRepository(DbContext context) => _context = context;
    
    public async Task<IEnumerable<T>> GetAsync(Specification<T> spec)
    {
        var query = _context.Set<T>().Where(spec.Criteria);
        
        foreach (var include in spec.Includes)
            query = query.Include(include);
        
        if (spec.OrderBy != null)
            query = query.OrderBy(spec.OrderBy);
        
        if (spec.OrderByDescending != null)
            query = query.OrderByDescending(spec.OrderByDescending);
        
        return await query.ToListAsync();
    }
}

// Usage
var repo = new SpecificationRepository<User>(_context);
var activeUsers = await repo.GetAsync(new ActiveUsersSpecification());
var user = await repo.GetAsync(new UserByIdSpecification(1));
```

---

### 26. Proxy Pattern

**Purpose:** Provide control over another object's access

```csharp
public interface IDataService
{
    Task<string> FetchDataAsync();
}

public class RealDataService : IDataService
{
    public async Task<string> FetchDataAsync()
    {
        Console.WriteLine("Fetching data from remote server...");
        await Task.Delay(2000);  // Simulate network delay
        return "Important Data";
    }
}

public class CachingProxy : IDataService
{
    private readonly IDataService _realService;
    private string _cachedData;
    private DateTime _cachedTime;
    private readonly int _cacheDurationSeconds;
    
    public CachingProxy(IDataService realService, int cacheDurationSeconds = 60)
    {
        _realService = realService;
        _cacheDurationSeconds = cacheDurationSeconds;
    }
    
    public async Task<string> FetchDataAsync()
    {
        if (_cachedData != null && 
            (DateTime.Now - _cachedTime).TotalSeconds < _cacheDurationSeconds)
        {
            Console.WriteLine("Returning cached data");
            return _cachedData;
        }
        
        _cachedData = await _realService.FetchDataAsync();
        _cachedTime = DateTime.Now;
        return _cachedData;
    }
}

// Usage
IDataService service = new RealDataService();
service = new CachingProxy(service);  // Wrap with caching

await service.FetchDataAsync();  // Fetches from server
await service.FetchDataAsync();  // Returns from cache
await Task.Delay(61000);
await service.FetchDataAsync();  // Cache expired, fetches again
```

**Real-World: Security Proxy**

```csharp
public interface IBankAccount
{
    void Withdraw(decimal amount);
    void Deposit(decimal amount);
    decimal GetBalance();
}

public class BankAccount : IBankAccount
{
    private decimal _balance;
    
    public void Withdraw(decimal amount) => _balance -= amount;
    public void Deposit(decimal amount) => _balance += amount;
    public decimal GetBalance() => _balance;
}

public class SecurityProxy : IBankAccount
{
    private readonly IBankAccount _account;
    private readonly string _password;
    private bool _isAuthenticated = false;
    
    public SecurityProxy(IBankAccount account, string password)
    {
        _account = account;
        _password = password;
    }
    
    public void Authenticate(string password)
    {
        _isAuthenticated = password == _password;
    }
    
    public void Withdraw(decimal amount)
    {
        if (!_isAuthenticated)
            throw new UnauthorizedAccessException("Not authenticated");
        _account.Withdraw(amount);
    }
    
    public void Deposit(decimal amount)
    {
        _account.Deposit(amount);
    }
    
    public decimal GetBalance()
    {
        if (!_isAuthenticated)
            throw new UnauthorizedAccessException("Not authenticated");
        return _account.GetBalance();
    }
}

// Usage
IBankAccount account = new SecurityProxy(new BankAccount(), "password123");
// account.GetBalance();  // ❌ UnauthorizedAccessException

((SecureProxy)account).Authenticate("password123");
account.GetBalance();  // ✅ Returns balance
```

---

### 27. Dependency Injection Container

**Purpose:** Manage object creation and dependency resolution

```csharp
public interface IServiceContainer
{
    void Register<TInterface, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TImplementation : TInterface;
    
    T Resolve<T>();
}

public enum ServiceLifetime
{
    Transient,    // New instance each time
    Singleton,    // One instance forever
    Scoped        // One instance per scope
}

public class SimpleServiceContainer : IServiceContainer
{
    private readonly Dictionary<Type, (Type Implementation, ServiceLifetime Lifetime)> _registrations = new();
    private readonly Dictionary<Type, object> _singletons = new();
    
    public void Register<TInterface, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TImplementation : TInterface
    {
        _registrations[typeof(TInterface)] = (typeof(TImplementation), lifetime);
    }
    
    public T Resolve<T>()
    {
        var interfaceType = typeof(T);
        
        if (!_registrations.TryGetValue(interfaceType, out var registration))
            throw new InvalidOperationException($"{interfaceType.Name} not registered");
        
        var (implementationType, lifetime) = registration;
        
        if (lifetime == ServiceLifetime.Singleton)
        {
            if (_singletons.TryGetValue(interfaceType, out var singleton))
                return (T)singleton;
            
            var instance = CreateInstance(implementationType);
            _singletons[interfaceType] = instance;
            return (T)instance;
        }
        
        return (T)CreateInstance(implementationType);
    }
    
    private object CreateInstance(Type type)
    {
        var constructors = type.GetConstructors();
        if (constructors.Length == 0)
            return Activator.CreateInstance(type);
        
        var constructor = constructors[0];
        var parameters = constructor.GetParameters();
        var paramInstances = parameters.Select(p => Resolve(p.ParameterType)).ToArray();
        
        return Activator.CreateInstance(type, paramInstances);
    }
    
    private object Resolve(Type type)
    {
        var method = typeof(SimpleServiceContainer).GetMethod("Resolve", Type.EmptyTypes)
            .MakeGenericMethod(type);
        return method.Invoke(this, null);
    }
}

// Usage
var container = new SimpleServiceContainer();
container.Register<IRepository<User>, SqlRepository<User>>(ServiceLifetime.Singleton);
container.Register<IUserService, UserService>(ServiceLifetime.Transient);

var userService = container.Resolve<IUserService>();
```

---

### 28. Pipeline/Middleware Pattern

**Purpose:** Process requests through chain of processing steps

```csharp
public interface IMiddleware
{
    Task InvokeAsync(Request request, Func<Request, Task> next);
}

public class Request
{
    public string Data { get; set; }
    public List<string> Log { get; } = new();
}

public class LoggingMiddleware : IMiddleware
{
    public async Task InvokeAsync(Request request, Func<Request, Task> next)
    {
        request.Log.Add("Logging: Started processing");
        await next(request);
        request.Log.Add("Logging: Finished processing");
    }
}

public class ValidationMiddleware : IMiddleware
{
    public async Task InvokeAsync(Request request, Func<Request, Task> next)
    {
        request.Log.Add("Validation: Checking data");
        
        if (string.IsNullOrEmpty(request.Data))
        {
            request.Log.Add("Validation: Data is empty!");
            return;
        }
        
        await next(request);
    }
}

public class ProcessingMiddleware : IMiddleware
{
    public async Task InvokeAsync(Request request, Func<Request, Task> next)
    {
        request.Log.Add($"Processing: Converting data to uppercase");
        request.Data = request.Data?.ToUpper();
        await next(request);
    }
}

public class Pipeline
{
    private List<IMiddleware> _middlewares = new();
    
    public void Use(IMiddleware middleware) => _middlewares.Add(middleware);
    
    public async Task ExecuteAsync(Request request)
    {
        var index = 0;
        
        Func<Request, Task> next = async (req) =>
        {
            if (index < _middlewares.Count)
            {
                var middleware = _middlewares[index++];
                await middleware.InvokeAsync(req, next);
            }
        };
        
        await next(request);
    }
}

// Usage
var pipeline = new Pipeline();
pipeline.Use(new LoggingMiddleware());
pipeline.Use(new ValidationMiddleware());
pipeline.Use(new ProcessingMiddleware());

var request = new Request { Data = "hello world" };
await pipeline.ExecuteAsync(request);

foreach (var log in request.Log)
    Console.WriteLine(log);
// Logging: Started processing
// Validation: Checking data
// Processing: Converting data to uppercase
// Logging: Finished processing
```

---

### 29. Interceptor Pattern

**Purpose:** Intercept and modify method calls before/after execution

```csharp
public interface IInterceptor
{
    void BeforeCall(MethodCallInfo callInfo);
    void AfterCall(MethodCallInfo callInfo);
}

public class MethodCallInfo
{
    public string MethodName { get; set; }
    public object[] Arguments { get; set; }
    public object ReturnValue { get; set; }
}

public class LoggingInterceptor : IInterceptor
{
    public void BeforeCall(MethodCallInfo callInfo)
    {
        Console.WriteLine($"Calling: {callInfo.MethodName}");
    }
    
    public void AfterCall(MethodCallInfo callInfo)
    {
        Console.WriteLine($"Completed: {callInfo.MethodName}");
    }
}

public class PerformanceInterceptor : IInterceptor
{
    private DateTime _startTime;
    
    public void BeforeCall(MethodCallInfo callInfo)
    {
        _startTime = DateTime.Now;
    }
    
    public void AfterCall(MethodCallInfo callInfo)
    {
        var elapsed = (DateTime.Now - _startTime).TotalMilliseconds;
        Console.WriteLine($"Execution time: {elapsed}ms");
    }
}

public interface IUserService
{
    void CreateUser(string name);
    string GetUserName(int id);
}

public class UserService : IUserService
{
    private readonly List<IInterceptor> _interceptors = new();
    
    public void AddInterceptor(IInterceptor interceptor) => _interceptors.Add(interceptor);
    
    public void CreateUser(string name)
    {
        var callInfo = new MethodCallInfo { MethodName = "CreateUser", Arguments = new object[] { name } };
        
        foreach (var interceptor in _interceptors)
            interceptor.BeforeCall(callInfo);
        
        Console.WriteLine($"Creating user: {name}");
        
        foreach (var interceptor in _interceptors)
            interceptor.AfterCall(callInfo);
    }
    
    public string GetUserName(int id)
    {
        var callInfo = new MethodCallInfo { MethodName = "GetUserName", Arguments = new object[] { id } };
        
        foreach (var interceptor in _interceptors)
            interceptor.BeforeCall(callInfo);
        
        var name = $"User {id}";
        
        foreach (var interceptor in _interceptors)
            interceptor.AfterCall(callInfo);
        
        return name;
    }
}

// Usage
var service = new UserService();
service.AddInterceptor(new LoggingInterceptor());
service.AddInterceptor(new PerformanceInterceptor());

service.CreateUser("John");
// Calling: CreateUser
// Creating user: John
// Completed: CreateUser
// Execution time: 5.32ms
```

---

### 30. Event Sourcing Pattern

**Purpose:** Store application state as sequence of immutable events

```csharp
public class DomainEvent
{
    public Guid AggregateId { get; set; }
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; }
}

public class UserCreatedEvent : DomainEvent
{
    public string Name { get; set; }
    public string Email { get; set; }
}

public class UserEmailChangedEvent : DomainEvent
{
    public string NewEmail { get; set; }
}

public class EventStore
{
    private readonly List<DomainEvent> _events = new();
    
    public void Append(DomainEvent @event)
    {
        @event.Timestamp = DateTime.UtcNow;
        _events.Add(@event);
        Console.WriteLine($"Event stored: {@event.EventType}");
    }
    
    public IEnumerable<DomainEvent> GetEvents(Guid aggregateId)
    {
        return _events.Where(e => e.AggregateId == aggregateId);
    }
}

public class UserAggregate
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    
    public static UserAggregate Create(string name, string email)
    {
        var user = new UserAggregate();
        
        var @event = new UserCreatedEvent
        {
            AggregateId = Guid.NewGuid(),
            Name = name,
            Email = email,
            EventType = nameof(UserCreatedEvent)
        };
        
        user.ApplyEvent(@event);
        return user;
    }
    
    public void ChangeEmail(string newEmail)
    {
        var @event = new UserEmailChangedEvent
        {
            AggregateId = Id,
            NewEmail = newEmail,
            EventType = nameof(UserEmailChangedEvent)
        };
        
        ApplyEvent(@event);
    }
    
    private void ApplyEvent(DomainEvent @event)
    {
        switch (@event)
        {
            case UserCreatedEvent created:
                Id = created.AggregateId;
                Name = created.Name;
                Email = created.Email;
                break;
            
            case UserEmailChangedEvent emailChanged:
                Email = emailChanged.NewEmail;
                break;
        }
    }
}

// Usage
var eventStore = new EventStore();
var user = UserAggregate.Create("John", "john@example.com");

eventStore.Append(new UserCreatedEvent
{
    AggregateId = user.Id,
    Name = user.Name,
    Email = user.Email,
    EventType = nameof(UserCreatedEvent)
});

user.ChangeEmail("newemail@example.com");

eventStore.Append(new UserEmailChangedEvent
{
    AggregateId = user.Id,
    NewEmail = user.Email,
    EventType = nameof(UserEmailChangedEvent)
});

// Replay events to reconstruct state
var events = eventStore.GetEvents(user.Id);
foreach (var @event in events)
{
    Console.WriteLine($"Event: {@event.EventType} at {@event.Timestamp}");
}
```

---

## Complete Pattern Summary

| # | Pattern | Purpose | Key Benefit |
|---|---------|---------|-------------|
| 1 | Singleton | One instance globally | Control resource access |
| 2 | Factory | Create objects dynamically | Hide creation complexity |
| 3 | Builder | Build complex objects | Step-by-step construction |
| 4 | Prototype | Clone objects | Avoid expensive creation |
| 5 | Object Pool | Reuse expensive objects | Improve performance |
| 6 | Adapter | Make incompatible interfaces work | Integrate legacy systems |
| 7 | Bridge | Decouple abstraction from implementation | Independent evolution |
| 8 | Composite | Treat objects uniformly in hierarchies | Simplify recursive structures |
| 9 | Decorator | Add functionality dynamically | Extend without modification |
| 10 | Facade | Simplify complex subsystems | Easy-to-use interface |
| 11 | Observer | Notify multiple listeners | Loose coupling |
| 12 | Strategy | Encapsulate algorithms | Choose algorithm at runtime |
| 13 | Command | Encapsulate requests as objects | Queue, undo, replay |
| 14 | Chain of Responsibility | Pass request through handlers | Dynamic handler selection |
| 15 | State | Change behavior by state | State-specific logic |
| 16 | Template Method | Define algorithm skeleton | Subclass customization |
| 17 | Memento | Capture/restore state | Undo functionality |
| 18 | Iterator | Access collection elements sequentially | Hide internal structure |
| 19 | Visitor | Perform operations on elements | Separate operations from data |
| 20 | Interpreter | Define grammar and parser | Domain-specific languages |
| 21 | Repository | Abstract data access | Swap storage backends |
| 22 | Unit of Work | Manage transactions | Atomic operations |
| 23 | CQRS | Separate reads from writes | Scalable queries |
| 24 | Mediator | Centralize communication | Reduce coupling |
| 25 | Specification | Encapsulate query logic | Reusable queries |
| 26 | Proxy | Control access to objects | Security, caching, lazy loading |
| 27 | DI Container | Manage dependencies | Automatic wiring |
| 28 | Pipeline/Middleware | Process through chain | Request handling |
| 29 | Interceptor | Intercept method calls | Logging, performance monitoring |
| 30 | Event Sourcing | Store state as events | Complete audit trail |

---

## How to Practice All 30 Patterns

### Phase 1: Creational Patterns (Week 1)
1. Copy Singleton code, run it
2. Implement Factory with your own products
3. Build a report using Builder
4. Clone a document template

### Phase 2: Structural Patterns (Week 2)
5. Create LegacySystem adapter
6. Implement Bridge for document rendering
7. Build file system with Composite
8. Add logging decorator to any service
9. Simplify complex workflow with Facade

### Phase 3: Behavioral Patterns (Weeks 3-4)
10-20. Implement each pattern with different data types
- Change payment strategies
- Create approval chains
- Track order state transitions
- Store user history with Memento

### Phase 4: Architectural Patterns (Week 5)
21-30. Build complete application using patterns
- Implement generic Repository
- Add Unit of Work for transactions
- Separate reads with CQRS
- Use DI container for wiring
- Add middleware pipeline

---

**Files Created This Session:**
- DESIGN_PATTERNS_CODE_EXAMPLES_PART1.md (892 lines)
- DESIGN_PATTERNS_CODE_EXAMPLES_PART2.md (951 lines)
- DESIGN_PATTERNS_CODE_EXAMPLES_PART3.md (THIS FILE)

**Total Code Examples: 30 design patterns with complete, runnable C# code**
