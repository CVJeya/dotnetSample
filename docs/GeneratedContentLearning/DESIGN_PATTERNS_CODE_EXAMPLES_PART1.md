# Design Patterns - Complete Code Examples (Part 1/3)

Runnable C# code examples for Creational & Structural patterns.

---

## CREATIONAL PATTERNS (5)

### 1. Singleton Pattern

**Purpose:** Ensure only one instance exists globally

```csharp
// ❌ BAD: Thread-unsafe
public class BadSingleton
{
    private static BadSingleton _instance;
    
    public static BadSingleton Instance
    {
        get
        {
            if (_instance == null)
                _instance = new BadSingleton();  // Not thread-safe!
            return _instance;
        }
    }
}

// ✅ GOOD: Thread-safe lazy initialization
public class GoodSingleton
{
    private static readonly Lazy<GoodSingleton> _instance = 
        new Lazy<GoodSingleton>(() => new GoodSingleton());
    
    public static GoodSingleton Instance => _instance.Value;
    
    private GoodSingleton() { }  // Private constructor
}

// Usage
var singleton1 = GoodSingleton.Instance;
var singleton2 = GoodSingleton.Instance;
Console.WriteLine(ReferenceEquals(singleton1, singleton2));  // true - same instance
```

**Real-World Example:** Configuration, Logger, Database Connection Pool

```csharp
public class DatabaseConnection
{
    private static readonly Lazy<DatabaseConnection> _instance =
        new Lazy<DatabaseConnection>(() => new DatabaseConnection());
    
    public static DatabaseConnection Instance => _instance.Value;
    
    private readonly string _connectionString;
    
    private DatabaseConnection()
    {
        _connectionString = "Server=localhost;Database=AppDb";
        Console.WriteLine("Database connection initialized");
    }
    
    public void ExecuteQuery(string query)
    {
        Console.WriteLine($"Executing: {query}");
    }
}

// All code uses same connection instance
DatabaseConnection.Instance.ExecuteQuery("SELECT * FROM Users");
DatabaseConnection.Instance.ExecuteQuery("UPDATE Users SET Active=1");
```

---

### 2. Factory Pattern

**Purpose:** Create objects without specifying exact classes

```csharp
// Product interface
public interface IPaymentGateway
{
    void ProcessPayment(decimal amount);
}

public class CreditCardGateway : IPaymentGateway
{
    public void ProcessPayment(decimal amount) => 
        Console.WriteLine($"Processing ${amount} via Credit Card");
}

public class PayPalGateway : IPaymentGateway
{
    public void ProcessPayment(decimal amount) => 
        Console.WriteLine($"Processing ${amount} via PayPal");
}

public class ApplePayGateway : IPaymentGateway
{
    public void ProcessPayment(decimal amount) => 
        Console.WriteLine($"Processing ${amount} via Apple Pay");
}

// Factory
public class PaymentGatewayFactory
{
    public static IPaymentGateway Create(string type) =>
        type.ToLower() switch
        {
            "creditcard" => new CreditCardGateway(),
            "paypal" => new PayPalGateway(),
            "applepay" => new ApplePayGateway(),
            _ => throw new ArgumentException($"Unknown gateway: {type}")
        };
}

// Usage
var gateway = PaymentGatewayFactory.Create("paypal");
gateway.ProcessPayment(99.99m);  // "Processing $99.99 via PayPal"

gateway = PaymentGatewayFactory.Create("creditcard");
gateway.ProcessPayment(49.99m);  // "Processing $49.99 via Credit Card"
```

**Variation: Abstract Factory**

```csharp
public interface INotificationFactory
{
    IEmailSender CreateEmailSender();
    ISmsSender CreateSmsSender();
}

public class GmailNotificationFactory : INotificationFactory
{
    public IEmailSender CreateEmailSender() => new GmailSender();
    public ISmsSender CreateSmsSender() => new TwilioSmsSender();
}

public class AzureNotificationFactory : INotificationFactory
{
    public IEmailSender CreateEmailSender() => new AzureEmailSender();
    public ISmsSender CreateSmsSender() => new AzureSmsSender();
}

// Usage: Create entire family of objects
INotificationFactory factory = new GmailNotificationFactory();
var emailSender = factory.CreateEmailSender();
var smsSender = factory.CreateSmsSender();
```

---

### 3. Builder Pattern

**Purpose:** Build complex objects step-by-step

```csharp
// Complex object
public class ReportBuilder
{
    private string _title = "";
    private List<string> _sections = new();
    private List<string> _charts = new();
    private bool _includeFooter = false;
    
    public ReportBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }
    
    public ReportBuilder AddSection(string section)
    {
        _sections.Add(section);
        return this;
    }
    
    public ReportBuilder AddChart(string chartName)
    {
        _charts.Add(chartName);
        return this;
    }
    
    public ReportBuilder WithFooter(bool include = true)
    {
        _includeFooter = include;
        return this;
    }
    
    public Report Build()
    {
        return new Report
        {
            Title = _title,
            Sections = _sections,
            Charts = _charts,
            IncludeFooter = _includeFooter
        };
    }
}

public class Report
{
    public string Title { get; set; }
    public List<string> Sections { get; set; }
    public List<string> Charts { get; set; }
    public bool IncludeFooter { get; set; }
}

// Usage: Fluent, readable
var report = new ReportBuilder()
    .WithTitle("Q4 Sales Report")
    .AddSection("Executive Summary")
    .AddSection("Revenue Analysis")
    .AddChart("Revenue Trend")
    .AddChart("Regional Breakdown")
    .WithFooter()
    .Build();
```

**Real-World: SQL Query Builder**

```csharp
public class SqlQueryBuilder
{
    private string _select = "*";
    private string _from = "";
    private List<string> _wheres = new();
    private List<string> _joins = new();
    private string _orderBy = "";
    
    public SqlQueryBuilder Select(string columns)
    {
        _select = columns;
        return this;
    }
    
    public SqlQueryBuilder From(string table)
    {
        _from = table;
        return this;
    }
    
    public SqlQueryBuilder Where(string condition)
    {
        _wheres.Add(condition);
        return this;
    }
    
    public SqlQueryBuilder Join(string joinClause)
    {
        _joins.Add(joinClause);
        return this;
    }
    
    public SqlQueryBuilder OrderBy(string column, string direction = "ASC")
    {
        _orderBy = $"{column} {direction}";
        return this;
    }
    
    public string Build()
    {
        var query = $"SELECT {_select} FROM {_from}";
        
        if (_joins.Count > 0)
            query += " " + string.Join(" ", _joins);
        
        if (_wheres.Count > 0)
            query += " WHERE " + string.Join(" AND ", _wheres);
        
        if (!string.IsNullOrEmpty(_orderBy))
            query += $" ORDER BY {_orderBy}";
        
        return query;
    }
}

// Usage
var sql = new SqlQueryBuilder()
    .Select("ID, Name, Email")
    .From("Users")
    .Join("INNER JOIN Orders ON Users.ID = Orders.UserID")
    .Where("Orders.Status = 'Pending'")
    .Where("Users.Active = 1")
    .OrderBy("Orders.CreatedDate", "DESC")
    .Build();

Console.WriteLine(sql);
// SELECT ID, Name, Email FROM Users 
// INNER JOIN Orders ON Users.ID = Orders.UserID 
// WHERE Orders.Status = 'Pending' AND Users.Active = 1 
// ORDER BY Orders.CreatedDate DESC
```

---

### 4. Prototype Pattern

**Purpose:** Clone objects instead of creating new ones

```csharp
public interface ICloneable<T>
{
    T Clone();
}

public class User : ICloneable<User>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; } = new();
    
    public User Clone()
    {
        // Shallow copy - only copies references
        return new User
        {
            Id = this.Id,
            Name = this.Name,
            Email = this.Email,
            Roles = new List<string>(this.Roles)  // Deep copy of list
        };
    }
}

// Usage
var originalUser = new User 
{ 
    Id = 1, 
    Name = "John", 
    Email = "john@example.com",
    Roles = new() { "Admin", "User" }
};

var clonedUser = originalUser.Clone();
clonedUser.Name = "Jane";
clonedUser.Roles.Add("Manager");

Console.WriteLine(originalUser.Name);  // "John" - unchanged
Console.WriteLine(clonedUser.Name);    // "Jane"
Console.WriteLine(originalUser.Roles.Count);  // 2
Console.WriteLine(clonedUser.Roles.Count);    // 3
```

**Real-World: Document Template**

```csharp
public class DocumentTemplate : ICloneable<DocumentTemplate>
{
    public string Title { get; set; }
    public string Content { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    
    public DocumentTemplate Clone()
    {
        return new DocumentTemplate
        {
            Title = this.Title,
            Content = this.Content,
            Metadata = new Dictionary<string, string>(this.Metadata)
        };
    }
}

// Usage: Create many similar documents from template
var contractTemplate = new DocumentTemplate 
{ 
    Title = "Service Agreement",
    Content = "Standard contract terms..."
};
contractTemplate.Metadata["Version"] = "1.0";
contractTemplate.Metadata["CreatedBy"] = "Legal";

var contract1 = contractTemplate.Clone();
contract1.Metadata["ClientName"] = "Acme Corp";

var contract2 = contractTemplate.Clone();
contract2.Metadata["ClientName"] = "TechStart Inc";

// Both are independent copies
```

---

### 5. Object Pool Pattern

**Purpose:** Reuse expensive objects instead of creating/destroying repeatedly

```csharp
public interface IPoolable
{
    void Reset();
}

public class DatabaseConnection : IPoolable
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public DateTime LastUsed { get; set; }
    
    public void Execute(string query)
    {
        Console.WriteLine($"Connection {Id}: Executing {query}");
        LastUsed = DateTime.Now;
    }
    
    public void Reset()
    {
        Console.WriteLine($"Connection {Id}: Reset");
    }
}

public class ObjectPool<T> where T : IPoolable, new()
{
    private readonly Stack<T> _available = new();
    private readonly HashSet<T> _inUse = new();
    private readonly int _maxSize;
    
    public ObjectPool(int maxSize)
    {
        _maxSize = maxSize;
        for (int i = 0; i < maxSize; i++)
            _available.Push(new T());
    }
    
    public T Acquire()
    {
        lock (_available)
        {
            if (_available.Count == 0)
                throw new InvalidOperationException("No objects available in pool");
            
            var obj = _available.Pop();
            _inUse.Add(obj);
            Console.WriteLine($"Acquired object. Available: {_available.Count}, InUse: {_inUse.Count}");
            return obj;
        }
    }
    
    public void Release(T obj)
    {
        lock (_available)
        {
            if (_inUse.Remove(obj))
            {
                obj.Reset();
                _available.Push(obj);
                Console.WriteLine($"Released object. Available: {_available.Count}, InUse: {_inUse.Count}");
            }
        }
    }
}

// Usage
var pool = new ObjectPool<DatabaseConnection>(3);

var conn1 = pool.Acquire();
var conn2 = pool.Acquire();

conn1.Execute("SELECT * FROM Users");

pool.Release(conn1);  // Returns to pool

var conn1Again = pool.Acquire();  // Same instance reused!
Console.WriteLine(conn1Again.Id == conn1.Id);  // true
```

---

## STRUCTURAL PATTERNS (5)

### 6. Adapter Pattern

**Purpose:** Make incompatible interfaces work together

```csharp
// Old system (incompatible interface)
public class LegacyPaymentSystem
{
    public decimal GetBalance(string accountId)
    {
        return 1000m;
    }
    
    public bool TransferFunds(string from, string to, decimal amount)
    {
        Console.WriteLine($"Legacy: Transferred {amount} from {from} to {to}");
        return true;
    }
}

// New system (expected interface)
public interface IModernPaymentGateway
{
    Task<decimal> GetBalanceAsync(string accountId);
    Task<bool> SendMoneyAsync(string from, string to, decimal amount);
}

// Adapter: Makes old system compatible with new interface
public class LegacyPaymentAdapter : IModernPaymentGateway
{
    private readonly LegacyPaymentSystem _legacy;
    
    public LegacyPaymentAdapter(LegacyPaymentSystem legacy)
    {
        _legacy = legacy;
    }
    
    public Task<decimal> GetBalanceAsync(string accountId)
    {
        return Task.FromResult(_legacy.GetBalance(accountId));
    }
    
    public Task<bool> SendMoneyAsync(string from, string to, decimal amount)
    {
        return Task.FromResult(_legacy.TransferFunds(from, to, amount));
    }
}

// Usage: New code uses adapter without knowing about legacy system
IModernPaymentGateway gateway = new LegacyPaymentAdapter(new LegacyPaymentSystem());
await gateway.SendMoneyAsync("ACC001", "ACC002", 500m);
var balance = await gateway.GetBalanceAsync("ACC001");
```

---

### 7. Bridge Pattern

**Purpose:** Decouple abstraction from implementation

```csharp
// Implementation
public interface IRenderer
{
    void Render(string content);
}

public class ConsoleRenderer : IRenderer
{
    public void Render(string content) => Console.WriteLine($"Console: {content}");
}

public class FileRenderer : IRenderer
{
    private readonly string _filename;
    
    public FileRenderer(string filename) => _filename = filename;
    
    public void Render(string content) => 
        File.WriteAllText(_filename, content);
}

// Abstraction (independent of renderer)
public abstract class Document
{
    protected IRenderer _renderer;
    
    public Document(IRenderer renderer) => _renderer = renderer;
    
    public abstract void Display();
}

public class PdfDocument : Document
{
    private readonly string _content;
    
    public PdfDocument(string content, IRenderer renderer) : base(renderer)
    {
        _content = content;
    }
    
    public override void Display() => _renderer.Render($"PDF: {_content}");
}

public class WordDocument : Document
{
    private readonly string _content;
    
    public WordDocument(string content, IRenderer renderer) : base(renderer)
    {
        _content = content;
    }
    
    public override void Display() => _renderer.Render($"Word: {_content}");
}

// Usage: Mix and match
Document pdfToConsole = new PdfDocument("Report", new ConsoleRenderer());
pdfToConsole.Display();  // Console: PDF: Report

Document pdfToFile = new PdfDocument("Report", new FileRenderer("output.txt"));
pdfToFile.Display();  // Writes to file

Document wordToConsole = new WordDocument("Letter", new ConsoleRenderer());
wordToConsole.Display();  // Console: Word: Letter
```

---

### 8. Composite Pattern

**Purpose:** Treat individual objects and compositions uniformly

```csharp
public interface IFileSystemItem
{
    void Display(int indent = 0);
    long GetSize();
}

public class File : IFileSystemItem
{
    public string Name { get; }
    public long Size { get; }
    
    public File(string name, long size)
    {
        Name = name;
        Size = size;
    }
    
    public void Display(int indent = 0)
    {
        Console.WriteLine(new string(' ', indent) + Name + $" ({Size} bytes)");
    }
    
    public long GetSize() => Size;
}

public class Directory : IFileSystemItem
{
    public string Name { get; }
    private readonly List<IFileSystemItem> _items = new();
    
    public Directory(string name) => Name = name;
    
    public void Add(IFileSystemItem item) => _items.Add(item);
    
    public void Display(int indent = 0)
    {
        Console.WriteLine(new string(' ', indent) + Name + "/");
        foreach (var item in _items)
            item.Display(indent + 2);
    }
    
    public long GetSize() => _items.Sum(i => i.GetSize());
}

// Usage: Same interface for files and directories
var root = new Directory("root");
root.Add(new File("readme.txt", 1024));
root.Add(new File("config.json", 512));

var documents = new Directory("Documents");
documents.Add(new File("report.pdf", 2048));
documents.Add(new File("budget.xlsx", 1536));

root.Add(documents);

root.Display();
// root/
//   readme.txt (1024 bytes)
//   config.json (512 bytes)
//   Documents/
//     report.pdf (2048 bytes)
//     budget.xlsx (1536 bytes)

Console.WriteLine($"Total size: {root.GetSize()} bytes");  // 5120
```

---

### 9. Decorator Pattern

**Purpose:** Add functionality to objects dynamically

```csharp
public interface ICoffee
{
    decimal GetCost();
    string GetDescription();
}

public class SimpleCoffee : ICoffee
{
    public decimal GetCost() => 2.0m;
    public string GetDescription() => "Simple Coffee";
}

// Decorators add functionality
public abstract class CoffeeDecorator : ICoffee
{
    protected ICoffee _coffee;
    
    public CoffeeDecorator(ICoffee coffee) => _coffee = coffee;
    
    public virtual decimal GetCost() => _coffee.GetCost();
    public virtual string GetDescription() => _coffee.GetDescription();
}

public class MilkDecorator : CoffeeDecorator
{
    public MilkDecorator(ICoffee coffee) : base(coffee) { }
    
    public override decimal GetCost() => _coffee.GetCost() + 0.5m;
    public override string GetDescription() => _coffee.GetDescription() + ", Milk";
}

public class ChocolateDecorator : CoffeeDecorator
{
    public ChocolateDecorator(ICoffee coffee) : base(coffee) { }
    
    public override decimal GetCost() => _coffee.GetCost() + 0.75m;
    public override string GetDescription() => _coffee.GetDescription() + ", Chocolate";
}

public class WhippedCreamDecorator : CoffeeDecorator
{
    public WhippedCreamDecorator(ICoffee coffee) : base(coffee) { }
    
    public override decimal GetCost() => _coffee.GetCost() + 0.6m;
    public override string GetDescription() => _coffee.GetDescription() + ", Whipped Cream";
}

// Usage: Stack decorators
ICoffee coffee = new SimpleCoffee();
Console.WriteLine($"{coffee.GetDescription()}: ${coffee.GetCost()}");
// Simple Coffee: $2.0

coffee = new MilkDecorator(coffee);
Console.WriteLine($"{coffee.GetDescription()}: ${coffee.GetCost()}");
// Simple Coffee, Milk: $2.5

coffee = new ChocolateDecorator(coffee);
Console.WriteLine($"{coffee.GetDescription()}: ${coffee.GetCost()}");
// Simple Coffee, Milk, Chocolate: $3.25

coffee = new WhippedCreamDecorator(coffee);
Console.WriteLine($"{coffee.GetDescription()}: ${coffee.GetCost()}");
// Simple Coffee, Milk, Chocolate, Whipped Cream: $3.85
```

**Real-World: Caching Decorator**

```csharp
public interface IDataRepository
{
    Task<User> GetUserAsync(int id);
}

public class SqlDataRepository : IDataRepository
{
    public async Task<User> GetUserAsync(int id)
    {
        await Task.Delay(100);  // Simulate DB query
        return new User { Id = id, Name = "John" };
    }
}

public class CachingDecorator : IDataRepository
{
    private readonly IDataRepository _inner;
    private readonly Dictionary<int, User> _cache = new();
    
    public CachingDecorator(IDataRepository inner) => _inner = inner;
    
    public async Task<User> GetUserAsync(int id)
    {
        if (_cache.TryGetValue(id, out var user))
        {
            Console.WriteLine($"Cache hit for user {id}");
            return user;
        }
        
        Console.WriteLine($"Cache miss for user {id}, querying database");
        user = await _inner.GetUserAsync(id);
        _cache[id] = user;
        return user;
    }
}

// Usage
IDataRepository repo = new SqlDataRepository();
repo = new CachingDecorator(repo);  // Wrap with caching

await repo.GetUserAsync(1);  // "Cache miss...", queries DB
await repo.GetUserAsync(1);  // "Cache hit...", returns from cache
```

---

### 10. Facade Pattern

**Purpose:** Simplify complex subsystem with single interface

```csharp
// Complex subsystem
public class PaymentProcessor
{
    public bool ValidateCard(string cardNumber) 
    { 
        Console.WriteLine("Validating card...");
        return cardNumber.Length == 16; 
    }
}

public class FraudDetectionService
{
    public bool CheckForFraud(string cardNumber, decimal amount)
    {
        Console.WriteLine($"Checking fraud for ${amount}...");
        return false;  // No fraud
    }
}

public class PaymentGateway
{
    public bool ProcessPayment(string cardNumber, decimal amount)
    {
        Console.WriteLine($"Processing ${amount}...");
        return true;
    }
}

public class NotificationService
{
    public void SendConfirmation(string email, decimal amount)
    {
        Console.WriteLine($"Sent confirmation to {email} for ${amount}");
    }
}

// Facade: Simplifies complex operations
public class PaymentFacade
{
    private readonly PaymentProcessor _processor;
    private readonly FraudDetectionService _fraudService;
    private readonly PaymentGateway _gateway;
    private readonly NotificationService _notificationService;
    
    public PaymentFacade()
    {
        _processor = new PaymentProcessor();
        _fraudService = new FraudDetectionService();
        _gateway = new PaymentGateway();
        _notificationService = new NotificationService();
    }
    
    public bool ProcessPayment(string cardNumber, string email, decimal amount)
    {
        if (!_processor.ValidateCard(cardNumber))
            return false;
        
        if (_fraudService.CheckForFraud(cardNumber, amount))
            return false;
        
        if (!_gateway.ProcessPayment(cardNumber, amount))
            return false;
        
        _notificationService.SendConfirmation(email, amount);
        return true;
    }
}

// Usage: One simple method instead of coordinating 4 services
var facade = new PaymentFacade();
bool success = facade.ProcessPayment("4111111111111111", "user@example.com", 99.99m);
// Internally orchestrates: validation, fraud check, payment, notification
```

---

## Summary of Part 1

**Created:** 10 design patterns with complete runnable code
- 5 Creational: Singleton, Factory, Builder, Prototype, Object Pool
- 5 Structural: Adapter, Bridge, Composite, Decorator, Facade

Each includes:
- ❌ BAD pattern (common mistakes)
- ✅ GOOD pattern (best practice)
- 📝 Usage examples
- 💡 Real-world applications

Continue to Part 2 for Behavioral patterns (11-20)
