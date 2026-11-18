# Object-Oriented Programming (OOP) Concepts with Real Examples

This guide explains the 4 core OOP concepts with examples from the FSH codebase and real-world analogies.

---

## Table of Contents
1. Encapsulation
2. Abstraction
3. Inheritance
4. Polymorphism
5. Bonus: SOLID Principles

---

## 1. ENCAPSULATION 🔒

### What is Encapsulation?

**Definition:** Bundling data (properties) and behavior (methods) together, hiding internal details from the outside world.

**Real-World Analogy:**
- **A car:** You don't see the engine, transmission, or wiring inside. You just use the steering wheel, pedals, and gear shift. The car **encapsulates** complex mechanisms.
- **A bank ATM:** You can't see the vault or server. You just enter your PIN and request money. Internal details are hidden.

### Code Example 1: Simple Encapsulation

```csharp
// ❌ BAD: Exposed internal details
public class BadProduct
{
    public decimal Price;  // Direct access (dangerous!)
    public int Stock;      // Anyone can modify
}

var product = new BadProduct();
product.Price = -1000;  // ❌ Negative price allowed! Bug!
product.Stock = -50;    // ❌ Negative stock! Bug!

// ✅ GOOD: Encapsulated with validation
public class GoodProduct
{
    private decimal _price;  // Hidden from outside
    private int _stock;      // Hidden from outside
    
    // Public property with validation
    public decimal Price
    {
        get => _price;
        set => _price = value > 0 ? value : throw new ArgumentException("Price must be positive");
    }
    
    public int Stock
    {
        get => _stock;
        set => _stock = value >= 0 ? value : throw new ArgumentException("Stock must be non-negative");
    }
}

var product = new GoodProduct();
product.Price = -1000;  // ✅ Throws exception - prevents bug!
```

### Code Example 2: FSH Real Example

**From:** `src/BuildingBlocks/Core/Domain/BaseEntity.cs`

```csharp
public abstract class BaseEntity<TId>
{
    // Private field - hidden from outside
    private readonly List<IDomainEvent> _domainEvents = [];
    
    // Public property - read-only (can only read, not modify)
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;
    
    // Protected method - only subclasses can call
    protected void AddDomainEvent(IDomainEvent @event)
        => _domainEvents.Add(@event);
    
    // Public method - anyone can call
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

**Why Encapsulation Here:**
- ✅ Nobody can accidentally modify `_domainEvents` directly
- ✅ Domain events can only be added through `AddDomainEvent()` (controlled)
- ✅ External code can READ events via `DomainEvents` property (safe)
- ✅ Internal logic is protected (only subclasses can add events)

### Access Modifiers (Levels of Encapsulation)

```csharp
public class Example
{
    // PUBLIC: Everyone can access
    public string Name { get; set; }
    
    // PRIVATE: Only this class can access (default for fields)
    private string _internalSecret;
    
    // PROTECTED: Only this class and subclasses can access
    protected virtual void InternalBehavior() { }
    
    // INTERNAL: Only this assembly/project can access
    internal void ProjectOnlyMethod() { }
    
    // PRIVATE PROTECTED: Only subclasses in this assembly
    private protected void SubclassInternalOnly() { }
}
```

### Real-World Example: User Password

```csharp
// ❌ BAD: Password visible to everyone
public class BadUser
{
    public string Password;  // Anyone can read/modify!
}

// ✅ GOOD: Password hidden and hashed
public class GoodUser
{
    private string _passwordHash;  // Hidden - nobody sees the password!
    
    public void SetPassword(string plainTextPassword)
    {
        // Validate
        if (plainTextPassword.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters");
        
        // Hash and store (never store plain text!)
        _passwordHash = BCrypt.Net.BCrypt.HashPassword(plainTextPassword);
    }
    
    public bool VerifyPassword(string plainTextPassword)
    {
        return BCrypt.Net.BCrypt.Verify(plainTextPassword, _passwordHash);
    }
}

var user = new GoodUser();
user.SetPassword("MySecurePass123");  // ✅ Password is hashed and stored securely
var isCorrect = user.VerifyPassword("MySecurePass123");  // ✅ Returns true
var isWrong = user.VerifyPassword("WrongPassword");      // ✅ Returns false
```

### Key Takeaway on Encapsulation
**Encapsulation = Control Access + Hide Complexity**
- Use `private` for internal details
- Use `public` properties for controlled access
- Validate data before storing
- Hide implementation, expose interface

---

## 2. ABSTRACTION 🎭

### What is Abstraction?

**Definition:** Hiding unnecessary complexity and showing only what's important. Focusing on WHAT an object does, not HOW it does it.

**Real-World Analogy:**
- **A Netflix app:** You don't need to know HOW Netflix streams video, compresses data, or manages servers. You just press Play and watch.
- **A calculator:** You don't care about circuit boards or transistors. You just press buttons and get results.

### Code Example 1: Simple Abstraction

```csharp
// ❌ CONCRETE: Shows how email is sent (implementation details)
public class EmailService
{
    public void SendEmail()
    {
        // Connect to SMTP server
        var client = new SmtpClient("smtp.gmail.com");
        client.Port = 587;
        client.EnableSsl = true;
        client.Credentials = new NetworkCredential("user", "pass");
        
        // Create message
        var message = new MailMessage("from@example.com", "to@example.com");
        message.Subject = "Hello";
        message.Body = "Test email";
        
        // Send
        client.Send(message);
        client.Dispose();
    }
}

// ✅ ABSTRACTION: Hide complexity behind simple interface
public interface IMailService
{
    Task SendAsync(MailRequest request);
}

public class EmailService : IMailService
{
    public async Task SendAsync(MailRequest request)
    {
        // All complexity hidden inside
        // Caller doesn't need to know SMTP, ports, SSL, credentials
    }
}

// Usage: Simple and clean
public class UserRegistrationHandler
{
    private readonly IMailService _mailService;
    
    public async Task Handle(RegisterUserCommand command)
    {
        var user = User.Create(command.Email);
        await _repository.AddAsync(user);
        
        // Send welcome email - no complexity visible!
        await _mailService.SendAsync(new MailRequest
        {
            To = user.Email,
            Subject = "Welcome!",
            Body = "Thank you for registering"
        });
    }
}
```

### Code Example 2: FSH Real Example

**From:** `src/BuildingBlocks/Storage/Services/IStorageService.cs`

```csharp
// ABSTRACTION: Simple interface, hides all complexity
public interface IStorageService
{
    // Caller doesn't care about S3, MinIO, local filesystem
    // Just upload a file
    Task<string> UploadAsync<T>(FileUploadRequest request, FileType fileType);
    
    // Just download a file
    Task<FileDownloadResponse?> DownloadAsync(string path);
    
    // Check if file exists
    Task<bool> ExistsAsync(string path);
    
    // Delete a file
    Task RemoveAsync(string path);
}

// Different implementations - same interface
public class S3StorageService : IStorageService
{
    // Implementation for AWS S3
    public async Task<string> UploadAsync<T>(FileUploadRequest request, FileType fileType)
    {
        // S3 specific code: buckets, regions, access keys
        // All hidden from caller
    }
}

public class LocalStorageService : IStorageService
{
    // Implementation for local filesystem
    public async Task<string> UploadAsync<T>(FileUploadRequest request, FileType fileType)
    {
        // Local filesystem code: disk I/O, paths
        // All hidden from caller
    }
}

// Usage: No knowledge of storage backend needed
public class FileUploadHandler
{
    private readonly IStorageService _storage;
    
    public async Task Handle(UploadFileCommand command)
    {
        // Works with S3, local storage, or any implementation
        // Abstraction hides the details!
        var path = await _storage.UploadAsync<Document>(command.File);
    }
}
```

### Real-World Example: Bank ATM

```csharp
// ❌ CONCRETE: Exposes banking system complexity
public class AtmMachine
{
    public void WithdrawMoney(string accountNumber, string bankCode, string routingNumber, decimal amount)
    {
        // Connect to core banking system
        // Validate account credentials
        // Check transaction limits
        // Update ledger
        // Manage reserve funds
        // Generate audit logs
        // Update regulatory reports
        // ... 100 more details
    }
}

// ✅ ABSTRACTION: Simple interface
public interface IBank
{
    Task<bool> WithdrawAsync(string cardNumber, decimal amount);
}

public class AtmMachine
{
    private readonly IBank _bank;
    
    public async Task WithdrawMoney(string cardNumber, decimal amount)
    {
        // Simple! All banking complexity is hidden
        var success = await _bank.WithdrawAsync(cardNumber, amount);
        if (success)
            DispenseCash(amount);
    }
}
```

### Key Takeaway on Abstraction
**Abstraction = Hide Details + Show Interface**
- Use `interface` to define what something does
- Hide implementation behind the interface
- Change implementation without breaking callers
- Focus on WHAT, not HOW

---

## 3. INHERITANCE 👨‍👦

### What is Inheritance?

**Definition:** A child class inherits (gets) properties and methods from a parent class. Reuse code and establish hierarchies.

**Real-World Analogy:**
- **Animals:** 
  - Animal (parent) has: `Move()`, `Eat()`
  - Dog (child) inherits `Move()`, `Eat()` and adds: `Bark()`
  - Cat (child) inherits `Move()`, `Eat()` and adds: `Meow()`

### Code Example 1: Simple Inheritance

```csharp
// Parent class
public class Vehicle
{
    public string Color { get; set; }
    public int Speed { get; set; }
    
    public virtual void Drive()
    {
        Console.WriteLine($"Vehicle driving at {Speed} mph");
    }
}

// Child class 1
public class Car : Vehicle
{
    public int Doors { get; set; }
    
    public override void Drive()
    {
        Console.WriteLine($"Car driving at {Speed} mph with {Doors} doors");
    }
}

// Child class 2
public class Motorcycle : Vehicle
{
    public bool HasSidecar { get; set; }
    
    public override void Drive()
    {
        Console.WriteLine($"Motorcycle driving at {Speed} mph");
    }
}

// Usage
var car = new Car { Color = "Red", Speed = 60, Doors = 4 };
car.Drive();  // "Car driving at 60 mph with 4 doors"

var bike = new Motorcycle { Color = "Black", Speed = 80, HasSidecar = false };
bike.Drive();  // "Motorcycle driving at 80 mph"
```

### Code Example 2: FSH Real Example

**From:** `src/BuildingBlocks/Core/Domain/BaseEntity.cs`

```csharp
// Parent class - base for all entities
public abstract class BaseEntity<TId> : IEntity<TId>, IHasDomainEvents
{
    public TId Id { get; protected set; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;
    
    protected void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

// Child class - aggregate root (special entity)
public abstract class AggregateRoot<TId> : BaseEntity<TId>
{
    // Inherits Id, DomainEvents, AddDomainEvent, etc.
    // Can add aggregate-specific behavior
}

// Domain-specific child classes
public class User : AggregateRoot<string>
{
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    
    // Inherited: Id, DomainEvents, AddDomainEvent()
    
    public static User Register(string email, string password)
    {
        var user = new User { Id = Guid.NewGuid().ToString(), Email = email };
        user.PasswordHash = HashPassword(password);
        user.AddDomainEvent(new UserRegisteredEvent { UserId = user.Id });
        return user;
    }
}

public class Product : AggregateRoot<string>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    
    // Inherited: Id, DomainEvents, AddDomainEvent()
    
    public void UpdatePrice(decimal newPrice)
    {
        Price = newPrice;
        AddDomainEvent(new PriceChangedEvent { ProductId = Id, NewPrice = newPrice });
    }
}

// All entities can be treated as BaseEntity
List<BaseEntity<string>> entities = new()
{
    new User { Email = "user@example.com" },
    new Product { Name = "Widget", Price = 19.99 }
};

// All have domain events!
foreach (var entity in entities)
{
    var events = entity.DomainEvents;
}
```

### Hierarchy Example: Role-Based Access

```csharp
// Parent class
public abstract class User
{
    public string Name { get; set; }
    public string Email { get; set; }
    
    // Virtual method - can be overridden
    public virtual void ViewDashboard()
    {
        Console.WriteLine("Viewing basic dashboard");
    }
}

// Child class 1: Regular user
public class RegularUser : User
{
    public override void ViewDashboard()
    {
        Console.WriteLine("Viewing user dashboard - can see own data only");
    }
}

// Child class 2: Admin user
public class AdminUser : User
{
    public override void ViewDashboard()
    {
        Console.WriteLine("Viewing admin dashboard - can see all data");
    }
    
    public void ManageUsers()
    {
        Console.WriteLine("Managing all users");
    }
}

// Usage
User user1 = new RegularUser { Name = "John" };
User user2 = new AdminUser { Name = "Admin" };

user1.ViewDashboard();  // "Viewing user dashboard - can see own data only"
user2.ViewDashboard();  // "Viewing admin dashboard - can see all data"

((AdminUser)user2).ManageUsers();  // "Managing all users"
```

### Key Takeaway on Inheritance
**Inheritance = Reuse + Hierarchy**
- Child class inherits parent's code
- Use `virtual/override` for customization
- Establish IS-A relationships
- Avoid deep hierarchies (max 3 levels)

---

## 4. POLYMORPHISM 🎪

### What is Polymorphism?

**Definition:** Many forms. One interface, many implementations. Treat different objects uniformly using the same code.

**Real-World Analogy:**
- **Restaurant menu:** One "dish" name, many implementations:
  - Order "pasta" → could get spaghetti, penne, fettuccine
  - You don't need to know which one you'll get
  - The chef provides the right type

### Code Example 1: Simple Polymorphism

```csharp
// Define interface (one form, many implementations)
public interface IAnimal
{
    void MakeSound();
}

// Implementation 1
public class Dog : IAnimal
{
    public void MakeSound() => Console.WriteLine("Woof!");
}

// Implementation 2
public class Cat : IAnimal
{
    public void MakeSound() => Console.WriteLine("Meow!");
}

// Implementation 3
public class Bird : IAnimal
{
    public void MakeSound() => Console.WriteLine("Tweet!");
}

// Polymorphism: Same code, different behavior
List<IAnimal> animals = new()
{
    new Dog(),
    new Cat(),
    new Bird(),
    new Dog()
};

// Same method call, different results!
foreach (var animal in animals)
{
    animal.MakeSound();  // Each animal makes its own sound
}

// Output:
// Woof!
// Meow!
// Tweet!
// Woof!
```

### Code Example 2: FSH Real Example - QuotaService

**From:** `src/BuildingBlocks/Quota/`

```csharp
// Interface (contract)
public interface IQuotaService
{
    Task<QuotaCheckResult> CheckAndRecordAsync(string tenantId, QuotaResource resource, long amount);
    Task<long> GetCurrentAsync(string tenantId, QuotaResource resource);
}

// Implementation 1: Redis (production)
public class RedisQuotaService : IQuotaService
{
    private readonly IConnectionMultiplexer _redis;
    
    public async Task<QuotaCheckResult> CheckAndRecordAsync(string tenantId, QuotaResource resource, long amount)
    {
        // Uses Redis atomic operations
        var db = _redis.GetDatabase();
        var key = $"quota:{tenantId}:{resource}:{DateTime.UtcNow:yyyyMM}";
        var newValue = await db.StringIncrementAsync(key, amount);
        // ... validation logic
    }
}

// Implementation 2: In-Memory (development)
public class InMemoryQuotaService : IQuotaService
{
    private readonly Dictionary<string, long> _counters = new();
    
    public async Task<QuotaCheckResult> CheckAndRecordAsync(string tenantId, QuotaResource resource, long amount)
    {
        // Uses dictionary
        lock (_lock)
        {
            var key = $"{tenantId}:{resource}:{DateTime.UtcNow:yyyyMM}";
            if (!_counters.TryGetValue(key, out var current))
                current = 0;
            _counters[key] = current + amount;
        }
    }
}

// Implementation 3: NoOp (testing)
public class NoopQuotaService : IQuotaService
{
    public async Task<QuotaCheckResult> CheckAndRecordAsync(string tenantId, QuotaResource resource, long amount)
    {
        // Does nothing - useful for tests
        return QuotaCheckResult.Unlimited(resource, 0);
    }
}

// Usage: Same code, different storage backend
public class QuotaEnforcementMiddleware
{
    private readonly IQuotaService _quotaService;  // Works with ANY implementation
    
    public QuotaEnforcementMiddleware(IQuotaService quotaService)
    {
        _quotaService = quotaService;  // Can be Redis, In-Memory, or NoOp
    }
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var result = await _quotaService.CheckAndRecordAsync(tenantId, QuotaResource.ApiCalls, 1);
        
        if (result.Allowed)
            await next(context);
        else
            await RejectAsync(context);
    }
}

// In Program.cs:
if (isDevelopment)
    services.AddScoped<IQuotaService, InMemoryQuotaService>();  // Dev: use in-memory
else
    services.AddScoped<IQuotaService, RedisQuotaService>();     // Prod: use Redis

// Same middleware works with both!
```

### Code Example 3: Storage Service Polymorphism

```csharp
// Interface (contract)
public interface IStorageService
{
    Task<string> UploadAsync<T>(FileUploadRequest request, FileType fileType);
    Task<FileDownloadResponse?> DownloadAsync(string path);
    Task RemoveAsync(string path);
}

// Implementation 1: AWS S3
public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    
    public async Task<string> UploadAsync<T>(FileUploadRequest request, FileType fileType)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = "my-bucket",
            Key = Guid.NewGuid().ToString(),
            InputStream = new MemoryStream(request.Data)
        };
        await _s3Client.PutObjectAsync(putRequest);
        return putRequest.Key;
    }
}

// Implementation 2: Local Filesystem
public class LocalStorageService : IStorageService
{
    private readonly string _basePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
    
    public async Task<string> UploadAsync<T>(FileUploadRequest request, FileType fileType)
    {
        var fileName = Guid.NewGuid().ToString();
        var filePath = Path.Combine(_basePath, fileName);
        await File.WriteAllBytesAsync(filePath, request.Data);
        return fileName;
    }
}

// Implementation 3: MinIO (S3-compatible)
public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _minioClient;
    
    public async Task<string> UploadAsync<T>(FileUploadRequest request, FileType fileType)
    {
        var objectName = Guid.NewGuid().ToString();
        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket("uploads")
            .WithObject(objectName)
            .WithStreamData(new MemoryStream(request.Data))
            .WithObjectSize(request.Data.Length));
        return objectName;
    }
}

// Usage: Polymorphism in action
public class FileUploadHandler
{
    private readonly IStorageService _storage;  // Works with S3, Local, or MinIO
    
    public async Task Handle(UploadFileCommand command)
    {
        // Same code, different storage backend
        var path = await _storage.UploadAsync<Document>(command.File);
        return path;
    }
}

// In Program.cs:
if (useS3)
    services.AddScoped<IStorageService, S3StorageService>();
else if (useLocal)
    services.AddScoped<IStorageService, LocalStorageService>();
else
    services.AddScoped<IStorageService, MinioStorageService>();

// Handler works with all!
```

### Key Takeaway on Polymorphism
**Polymorphism = One Interface, Many Implementations**
- Define interface with contract
- Create multiple implementations
- Use interface in code (not specific class)
- Swap implementations without changing code

---

## 5. How OOP Concepts Work Together 🎯

### Complete Example: User Registration

```csharp
// ENCAPSULATION: Hide password
public class User : AggregateRoot<string>  // INHERITANCE
{
    private string _passwordHash;  // Encapsulated
    
    public string Email { get; set; }
    public string Name { get; set; }
    
    public void SetPassword(string plainText)
    {
        _passwordHash = HashPassword(plainText);
    }
    
    public bool VerifyPassword(string plainText)
    {
        return VerifyHash(plainText, _passwordHash);
    }
    
    public static User Create(string email, string name)
    {
        var user = new User { Id = Guid.NewGuid().ToString(), Email = email, Name = name };
        user.AddDomainEvent(new UserCreatedEvent { UserId = user.Id });
        return user;
    }
}

// ABSTRACTION: Hide database details
public interface IUserRepository  // ABSTRACTION
{
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}

// Different repository implementations - POLYMORPHISM
public class SqlUserRepository : IUserRepository
{
    private readonly DbContext _context;
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}

public class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<string, User> _users = new();
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        return _users.Values.FirstOrDefault(u => u.Email == email);
    }
}

// ABSTRACTION: Hide email service details
public interface IMailService  // ABSTRACTION
{
    Task SendWelcomeAsync(string email, string name);
}

public class EmailService : IMailService  // POLYMORPHISM: Can swap implementation
{
    public async Task SendWelcomeAsync(string email, string name)
    {
        // Hide SMTP details
    }
}

// Handler uses all 4 concepts!
public class RegisterUserHandler
{
    private readonly IUserRepository _userRepository;  // ABSTRACTION
    private readonly IMailService _mailService;        // ABSTRACTION
    
    public async Task Handle(RegisterUserCommand command)
    {
        // Check if user exists (ABSTRACTION: don't know if SQL or in-memory)
        var existing = await _userRepository.GetByEmailAsync(command.Email);
        if (existing != null)
            throw new UserAlreadyExistsException();
        
        // Create user (ENCAPSULATION: password is hashed and hidden)
        var user = User.Create(command.Email, command.Name);
        user.SetPassword(command.Password);  // ✅ Password never exposed
        
        // Save user (ABSTRACTION: don't know storage backend)
        await _userRepository.AddAsync(user);
        
        // Send email (ABSTRACTION + POLYMORPHISM: could use SMTP, SendGrid, etc.)
        await _mailService.SendWelcomeAsync(user.Email, user.Name);
        
        // Publish domain event (happens automatically in FSH)
        // user.DomainEvents has UserCreatedEvent
    }
}
```

---

## 6. SOLID Principles (Advanced OOP)

### S - Single Responsibility Principle

**Rule:** One class should have ONE reason to change

```csharp
// ❌ BAD: Too many responsibilities
public class UserService
{
    public void Register(string email, string password)
    {
        // Validate user
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException("Email required");
        
        // Send email
        var smtpClient = new SmtpClient();
        smtpClient.Send(email, "Welcome", "...");
        
        // Save to database
        var connection = new SqlConnection();
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Users...";
        command.ExecuteNonQuery();
        
        // Log activity
        File.AppendAllText("log.txt", "User registered: " + email);
    }
}

// ✅ GOOD: Each class has ONE responsibility
public class UserValidator
{
    public void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException("Email required");
    }
}

public class UserRepository
{
    private readonly DbContext _context;
    
    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
}

public class MailService
{
    private readonly SmtpClient _smtpClient;
    
    public async Task SendWelcomeAsync(string email)
    {
        await _smtpClient.SendAsync(email, "Welcome", "...");
    }
}

public class Logger
{
    public void Log(string message)
    {
        File.AppendAllText("log.txt", message);
    }
}

// Clean, focused handler
public class RegisterUserHandler
{
    private readonly UserValidator _validator;
    private readonly UserRepository _repository;
    private readonly MailService _mailService;
    private readonly Logger _logger;
    
    public async Task Handle(RegisterUserCommand command)
    {
        _validator.ValidateEmail(command.Email);
        var user = User.Create(command.Email, command.Name);
        user.SetPassword(command.Password);
        await _repository.AddAsync(user);
        await _mailService.SendWelcomeAsync(user.Email);
        _logger.Log($"User registered: {user.Email}");
    }
}
```

### O - Open/Closed Principle

**Rule:** Open for extension, closed for modification

```csharp
// ❌ BAD: Need to modify code to add new notification type
public class NotificationService
{
    public void SendNotification(string type, string message)
    {
        if (type == "email")
        {
            // Send email
        }
        else if (type == "sms")
        {
            // Send SMS
        }
        else if (type == "slack")  // ❌ Need to modify this class!
        {
            // Send Slack message
        }
    }
}

// ✅ GOOD: Extend with new types without modifying existing code
public interface INotificationChannel
{
    Task SendAsync(string message);
}

public class EmailNotificationChannel : INotificationChannel
{
    public async Task SendAsync(string message)
    {
        // Send email
    }
}

public class SmsNotificationChannel : INotificationChannel
{
    public async Task SendAsync(string message)
    {
        // Send SMS
    }
}

public class SlackNotificationChannel : INotificationChannel  // ✅ New without modifying others!
{
    public async Task SendAsync(string message)
    {
        // Send Slack
    }
}

public class NotificationService
{
    private readonly INotificationChannel _channel;
    
    public NotificationService(INotificationChannel channel)
    {
        _channel = channel;
    }
    
    public async Task SendAsync(string message)
    {
        await _channel.SendAsync(message);
    }
}

// Usage: Add new channels without modifying NotificationService!
```

### L - Liskov Substitution Principle

**Rule:** Subtypes must be substitutable for their base types

```csharp
// ❌ BAD: Subtypes break the contract
public class Employee
{
    public virtual decimal GetSalary()
    {
        return 50000;
    }
}

public class Contractor : Employee
{
    public override decimal GetSalary()
    {
        // ❌ Violates contract: contractors don't have salary!
        throw new NotImplementedException("Contractors don't have salary");
    }
}

// ✅ GOOD: Different types, same contract
public interface ICompensation
{
    decimal GetCompensation();
}

public class Employee : ICompensation
{
    public decimal GetCompensation()
    {
        return 50000;  // Annual salary
    }
}

public class Contractor : ICompensation
{
    public decimal GetCompensation()
    {
        return 100;  // Hourly rate (can be multiplied by hours)
    }
}

// Both can be used the same way
ICompensation emp = new Employee();
ICompensation contractor = new Contractor();

decimal totalCost = emp.GetCompensation() + contractor.GetCompensation();  // ✅ Works!
```

### I - Interface Segregation Principle

**Rule:** Many specific interfaces > one fat interface

```csharp
// ❌ BAD: Fat interface
public interface IWorker
{
    void Work();
    void Eat();  // ❌ Not all workers eat!
    void Sleep(); // ❌ Not all workers sleep!
    void Code();  // ❌ Not all workers code!
}

public class Robot : IWorker
{
    public void Work() { /* robots work */ }
    public void Eat() { throw new NotImplementedException(); }  // ❌ Robots don't eat!
    public void Sleep() { throw new NotImplementedException(); }  // ❌ Robots don't sleep!
    public void Code() { /* robots code */ }
}

// ✅ GOOD: Specific interfaces
public interface IWorker
{
    void Work();
}

public interface IHuman
{
    void Eat();
    void Sleep();
}

public interface IProgrammer
{
    void Code();
}

public class Developer : IWorker, IHuman, IProgrammer
{
    public void Work() { /* work */ }
    public void Eat() { /* eat */ }
    public void Sleep() { /* sleep */ }
    public void Code() { /* code */ }
}

public class Robot : IWorker, IProgrammer
{
    public void Work() { /* work */ }
    public void Code() { /* code */ }
    // ✅ No forced implementation of Eat/Sleep
}
```

### D - Dependency Inversion Principle

**Rule:** Depend on abstractions, not concretions

```csharp
// ❌ BAD: Depends on concrete class
public class OrderService
{
    private readonly SqlOrderRepository _repository = new();  // Concrete dependency
    
    public void CreateOrder(Order order)
    {
        _repository.Save(order);  // Tightly coupled to SQL!
    }
}

// ✅ GOOD: Depends on abstraction
public class OrderService
{
    private readonly IOrderRepository _repository;  // Abstract dependency
    
    public OrderService(IOrderRepository repository)
    {
        _repository = repository;  // Injected
    }
    
    public void CreateOrder(Order order)
    {
        _repository.Save(order);  // Works with ANY repository!
    }
}

// Can swap implementations
services.AddScoped<IOrderRepository, SqlOrderRepository>();      // SQL
// or
services.AddScoped<IOrderRepository, NoSqlOrderRepository>();    // NoSQL
// or
services.AddScoped<IOrderRepository, InMemoryOrderRepository>();  // In-memory
```

---

## Summary: OOP Concepts at a Glance

| Concept | What | Why | How |
|---------|------|-----|-----|
| **Encapsulation** | Bundle data + behavior, hide details | Control access, prevent bugs | `private` fields, `public` properties |
| **Abstraction** | Hide complexity, show interface | Simplify usage, change implementation | `interface`, `abstract class` |
| **Inheritance** | Child inherits from parent | Reuse code, establish hierarchy | `class Child : Parent` |
| **Polymorphism** | Many forms, one interface | Swap implementations, use uniform code | `interface IService` + multiple implementations |
| **SOLID** | 5 best practices for OOP | Write maintainable, flexible code | Single responsibility, open/closed, etc. |

---

## Practice Exercises

### Exercise 1: Bank Account (Encapsulation)
Create a `BankAccount` class that:
- Has private `_balance` field
- Has public `Deposit()` method (add money)
- Has public `Withdraw()` method (remove money)
- Validates that balance never goes negative

### Exercise 2: Payment Processing (Abstraction & Polymorphism)
Create:
- `IPaymentProcessor` interface with `ProcessAsync(amount)` method
- `CreditCardProcessor` implementation
- `PayPalProcessor` implementation
- `BankTransferProcessor` implementation
- Use all three interchangeably

### Exercise 3: Vehicle Hierarchy (Inheritance)
Create:
- `Vehicle` parent class with `Drive()`, `Refuel()`
- `Car` child class
- `Truck` child class
- `Motorcycle` child class
- Each overrides `Drive()` with specific behavior

### Exercise 4: SOLID Principles
Refactor a monolithic class into multiple focused classes following SOLID principles.

---

**Created:** January 21, 2025
**Target Audience:** Beginners learning OOP concepts
**Scope:** 4 OOP pillars + SOLID principles with real examples
