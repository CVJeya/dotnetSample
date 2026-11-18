# Advanced C# & .NET Core Concepts - Complete Guide

Missing concepts not covered in fundamentals. Build on your foundation to become expert.

---

## Table of Contents
1. Object-Oriented Programming Advanced
2. Async/Await & Task-Based Programming
3. Generics & Constraints
4. LINQ Deep Dive
5. Delegates, Events & Lambda
6. Reflection & Attributes
7. Extension Methods & Operators
8. Nullability & Pattern Matching
9. Dependency Injection & IoC
10. Entity Framework Core
11. ASP.NET Core Basics
12. Middleware & Pipelines

---

## 1. OBJECT-ORIENTED PROGRAMMING ADVANCED 🏗️

### Abstract Classes vs Interfaces

```csharp
// ABSTRACT CLASS: Share code and state
public abstract class Animal
{
    // Concrete method - has implementation
    public void Sleep()
    {
        Console.WriteLine("Zzz...");
    }
    
    // Abstract method - no implementation (must override)
    public abstract void MakeSound();
    
    // Property
    public string Name { get; set; }
    
    // Constructor
    protected Animal(string name)
    {
        Name = name;
    }
}

public class Dog : Animal
{
    public Dog(string name) : base(name) { }
    
    public override void MakeSound()
    {
        Console.WriteLine("Woof!");
    }
}

// INTERFACE: Contract without implementation or state
public interface IFlyable
{
    void Fly();
    int MaxAltitude { get; }
}

public interface ISwimmable
{
    void Swim();
}

public class Duck : Animal, IFlyable, ISwimmable
{
    public int MaxAltitude { get; } = 10000;
    
    public Duck(string name) : base(name) { }
    
    public override void MakeSound() => Console.WriteLine("Quack!");
    public void Fly() => Console.WriteLine("Flying high!");
    public void Swim() => Console.WriteLine("Swimming in water");
}

// Usage
Animal dog = new Dog("Buddy");
dog.MakeSound();    // Woof!

Duck duck = new Duck("Donald");
duck.MakeSound();   // Quack!
duck.Fly();         // Flying high!
duck.Swim();        // Swimming in water
```

### Virtual Methods & Polymorphism

```csharp
public class Vehicle
{
    // Virtual method - can be overridden
    public virtual void Drive()
    {
        Console.WriteLine("Vehicle driving");
    }
    
    // Non-virtual method - cannot be overridden
    public void Refuel()
    {
        Console.WriteLine("Refueling...");
    }
}

public class Car : Vehicle
{
    // Override virtual method
    public override void Drive()
    {
        Console.WriteLine("Car driving on road");
    }
}

public class Boat : Vehicle
{
    // Override virtual method
    public override void Drive()
    {
        Console.WriteLine("Boat driving on water");
    }
}

// Polymorphic behavior
Vehicle v1 = new Car();
Vehicle v2 = new Boat();

v1.Drive();  // Car driving on road
v2.Drive();  // Boat driving on water
v1.Refuel(); // Refueling...

// Same reference type, different behavior!
```

### Sealed Classes & Methods

```csharp
// SEALED CLASS: Cannot be inherited
public sealed class FinalClass
{
    public void DoSomething() { }
}

// ❌ Error: Cannot inherit from sealed class
// public class Derived : FinalClass { }

// SEALED METHOD: Cannot be overridden in descendants
public class Base
{
    public virtual void Method1() { }
    
    public virtual void Method2() { }
}

public class Child : Base
{
    public override void Method1() { }
    
    // Seal this method - no further overrides
    public sealed override void Method2() { }
}

// ❌ Error: Cannot override sealed method
// public class GrandChild : Child {
//     public override void Method2() { }  // Error!
// }
```

---

## 2. ASYNC/AWAIT & TASK-BASED PROGRAMMING ⚡

### Understanding Async/Await

```csharp
// SYNCHRONOUS: Blocks thread until complete
public string FetchDataSync()
{
    System.Threading.Thread.Sleep(2000);  // Blocks 2 seconds
    return "Data";
}

// ASYNCHRONOUS: Non-blocking, returns immediately
public async Task<string> FetchDataAsync()
{
    // await pauses method, resumes when Task completes
    await Task.Delay(2000);  // Doesn't block thread
    return "Data";
}

// Usage
string result1 = FetchDataSync();  // Waits 2 seconds

string result2 = await FetchDataAsync();  // Non-blocking wait

// Multiple async operations
public async Task ProcessMultipleAsync()
{
    // Sequential - waits for each
    var data1 = await FetchDataAsync();
    var data2 = await FetchDataAsync();
    // Total time: ~4 seconds
    
    // Parallel - runs together
    var task1 = FetchDataAsync();
    var task2 = FetchDataAsync();
    await Task.WhenAll(task1, task2);  // Total time: ~2 seconds
}
```

### Task & Task<T>

```csharp
// TASK: Represents async operation with no return
public async Task DownloadFileAsync(string url)
{
    await Task.Delay(1000);
    Console.WriteLine($"Downloaded: {url}");
}

// TASK<T>: Represents async operation with return value
public async Task<int> FetchCountAsync()
{
    await Task.Delay(1000);
    return 42;
}

// Task.Run: Run sync code async
public void SyncMethod()
{
    // This blocks
    System.Threading.Thread.Sleep(2000);
}

public async Task<int> RunSyncAsync()
{
    // Run sync code on thread pool
    return await Task.Run(() =>
    {
        SyncMethod();
        return 123;
    });
}

// Fire and forget (not recommended)
public async void BadPractice()
{
    await FetchDataAsync();  // Can't catch exceptions properly
}

// Better: Return Task
public async Task GoodPractice()
{
    await FetchDataAsync();
}
```

### Exception Handling in Async

```csharp
public async Task FetchWithErrorHandling()
{
    try
    {
        // Exceptions inside await are thrown here
        string data = await FetchDataAsync();
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Network error: {ex.Message}");
    }
    catch (TaskCanceledException ex)
    {
        Console.WriteLine($"Request timeout: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"General error: {ex.Message}");
    }
    finally
    {
        Console.WriteLine("Cleanup");
    }
}

// Multiple tasks - AggregateException
public async Task HandleMultipleExceptions()
{
    var task1 = Task.FromException(new Exception("Error 1"));
    var task2 = Task.FromException(new Exception("Error 2"));
    
    try
    {
        await Task.WhenAll(task1, task2);
    }
    catch (Exception ex)
    {
        // Single exception thrown
        Console.WriteLine(ex.Message);  // "Error 1"
    }
}
```

### Cancellation Token

```csharp
// CANCELLATIONTAKEN: Stop long-running operations
public async Task FetchWithCancellationAsync(CancellationToken cancellationToken)
{
    try
    {
        for (int i = 0; i < 10; i++)
        {
            // Check if cancellation requested
            cancellationToken.ThrowIfCancellationRequested();
            
            await Task.Delay(1000, cancellationToken);
            Console.WriteLine($"Step {i}");
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Operation cancelled");
    }
}

// Usage
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(3));  // Cancel after 3 seconds

await FetchWithCancellationAsync(cts.Token);
```

---

## 3. GENERICS & CONSTRAINTS 🔍

### Generic Classes & Methods

```csharp
// GENERIC CLASS: Works with any type
public class Repository<T>
{
    private List<T> _items = new();
    
    public void Add(T item) => _items.Add(item);
    public List<T> GetAll() => _items;
    public T GetById(int index) => _items[index];
}

// Usage with different types
var userRepo = new Repository<User>();
userRepo.Add(new User { Id = 1, Name = "John" });

var productRepo = new Repository<Product>();
productRepo.Add(new Product { Id = 1, Name = "Laptop" });

// GENERIC METHOD: Type parameter in method
public T Max<T>(T a, T b) where T : IComparable<T>
{
    return a.CompareTo(b) > 0 ? a : b;
}

Console.WriteLine(Max(10, 20));      // 20
Console.WriteLine(Max("Apple", "Zebra"));  // Zebra
```

### Generic Constraints

```csharp
// CONSTRAINT: where T : BaseClass
public class AnimalRepository<T> where T : Animal
{
    private List<T> _animals = new();
    
    public void Add(T animal) => _animals.Add(animal);
    
    public void MakeAllSounds()
    {
        foreach (var animal in _animals)
            animal.MakeSound();
    }
}

// CONSTRAINT: where T : IInterface
public class ServiceFactory<T> where T : IService
{
    public T Create() => Activator.CreateInstance<T>();
}

// CONSTRAINT: where T : new()
// Type must have parameterless constructor
public class Factory<T> where T : new()
{
    public T Create() => new T();
}

// CONSTRAINT: where T : class
// Type must be reference type
public class Reference<T> where T : class
{
    public void Process(T item) { }
}

// CONSTRAINT: where T : struct
// Type must be value type
public class Struct<T> where T : struct
{
    public void Process(T value) { }
}

// MULTIPLE CONSTRAINTS
public class Complex<T> where T : class, IService, new()
{
    // T must be: reference type AND implement IService AND have parameterless constructor
}
```

---

## 4. LINQ DEEP DIVE 🔎

### Method vs Query Syntax

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// METHOD SYNTAX
var methodResult = numbers
    .Where(x => x % 2 == 0)
    .Select(x => x * 2)
    .OrderByDescending(x => x);

// QUERY SYNTAX (SQL-like)
var queryResult = from n in numbers
                  where n % 2 == 0
                  select n * 2
                  orderby n descending;

// Both produce same result
// Method syntax is more common in modern C#
```

### Complex LINQ Operations

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
}

var products = new List<Product>
{
    new() { Id = 1, Name = "Laptop", Price = 1000, Category = "Electronics" },
    new() { Id = 2, Name = "Mouse", Price = 50, Category = "Electronics" },
    new() { Id = 3, Name = "Desk", Price = 300, Category = "Furniture" },
    new() { Id = 4, Name = "Chair", Price = 200, Category = "Furniture" },
};

// GROUP BY
var byCategory = products
    .GroupBy(p => p.Category)
    .Select(g => new
    {
        Category = g.Key,
        Count = g.Count(),
        TotalPrice = g.Sum(p => p.Price),
        Average = g.Average(p => p.Price)
    });

// JOIN
var electronics = new List<string> { "Laptop", "Mouse" };
var joined = products
    .Join(electronics,
          p => p.Name,
          e => e,
          (p, e) => new { p.Id, p.Name, p.Price })
    .ToList();

// DISTINCT
var categories = products
    .Select(p => p.Category)
    .Distinct();

// TAKE/SKIP (pagination)
var page = products
    .OrderBy(p => p.Name)
    .Skip(2)  // Skip first 2
    .Take(2)  // Take next 2
    .ToList();

// AGGREGATE
decimal sum = products.Sum(p => p.Price);
decimal avg = products.Average(p => p.Price);
int count = products.Count();

// CUSTOM AGGREGATE
string names = products
    .Aggregate(
        "",
        (acc, p) => acc + p.Name + ", ",
        acc => acc.TrimEnd(',', ' ')
    );
```

### Deferred vs Immediate Execution

```csharp
// DEFERRED: Executed when enumerated
IEnumerable<int> deferred = numbers
    .Where(x => x > 5)
    .Select(x => x * 2);

// Query not executed yet - just defined
// Executes when you iterate:
foreach (var num in deferred)
    Console.WriteLine(num);

// IMMEDIATE: Executed immediately
List<int> immediate = numbers
    .Where(x => x > 5)
    .Select(x => x * 2)
    .ToList();  // ToList() forces execution

// IEnumerable vs IQueryable
// IEnumerable: LINQ to Objects (in-memory)
IEnumerable<int> inMemory = new List<int> { 1, 2, 3 }
    .Where(x => x > 1);

// IQueryable: LINQ to SQL, LINQ to Entity Framework
IQueryable<User> fromDatabase = dbContext.Users
    .Where(u => u.IsActive);  // Executes at database
```

---

## 5. DELEGATES, EVENTS & LAMBDA 📢

### Delegates

```csharp
// DELEGATE: Type-safe function pointer
public delegate void NotifyDelegate(string message);
public delegate int CalculateDelegate(int a, int b);

// Old way: Named methods
public void SendEmail(string message)
{
    Console.WriteLine($"Email: {message}");
}

public void SendSms(string message)
{
    Console.WriteLine($"SMS: {message}");
}

NotifyDelegate notifier = SendEmail;
notifier("Hello");  // Email: Hello

notifier = SendSms;
notifier("Hello");  // SMS: Hello

// Modern way: Lambda expressions
CalculateDelegate add = (a, b) => a + b;
CalculateDelegate multiply = (a, b) => a * b;

Console.WriteLine(add(5, 3));      // 8
Console.WriteLine(multiply(5, 3)); // 15
```

### Events

```csharp
// EVENT: Delegates used for notifications
public class Button
{
    // Define delegate
    public delegate void ClickEventHandler(string buttonName);
    
    // Define event
    public event ClickEventHandler OnClick;
    
    public void Click()
    {
        Console.WriteLine("Button clicked!");
        // Raise event (notify subscribers)
        OnClick?.Invoke("MyButton");
    }
}

public class EventsExample
{
    public void Subscribe()
    {
        var button = new Button();
        
        // Subscribe to event
        button.OnClick += (name) => Console.WriteLine($"Handler 1: {name}");
        button.OnClick += (name) => Console.WriteLine($"Handler 2: {name}");
        
        button.Click();
        // Output:
        // Button clicked!
        // Handler 1: MyButton
        // Handler 2: MyButton
    }
}

// Modern EventHandler pattern
public class Button2
{
    public event EventHandler OnClick;
    
    public void Click()
    {
        OnClick?.Invoke(this, EventArgs.Empty);
    }
}

public class CustomEventArgs : EventArgs
{
    public string Message { get; set; }
}

public class Button3
{
    public event EventHandler<CustomEventArgs> OnClick;
    
    public void Click()
    {
        OnClick?.Invoke(this, new CustomEventArgs { Message = "Clicked!" });
    }
}
```

### Lambda Expressions

```csharp
// LAMBDA: Anonymous function
// Syntax: (parameters) => expression

// Single parameter
Func<int, int> square = x => x * x;
Console.WriteLine(square(5));  // 25

// Multiple parameters
Func<int, int, int> add = (a, b) => a + b;
Console.WriteLine(add(3, 4));  // 7

// Multiple statements
Func<int, string> describe = x =>
{
    if (x > 0) return "Positive";
    if (x < 0) return "Negative";
    return "Zero";
};

// With LINQ
var numbers = new List<int> { 1, 2, 3, 4, 5 };
var evens = numbers.Where(n => n % 2 == 0);
var doubled = numbers.Select(n => n * 2);

// Expression-bodied members (similar to lambda)
public class Calculator
{
    public int Add(int a, int b) => a + b;  // Single expression method
    public string Name => "Calculator";      // Property expression
}
```

---

## 6. REFLECTION & ATTRIBUTES 🔬

### Basic Reflection

```csharp
// REFLECTION: Inspect types at runtime
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// Get type information
Type userType = typeof(User);
Console.WriteLine($"Type: {userType.Name}");

// Get properties
PropertyInfo[] properties = userType.GetProperties();
foreach (var prop in properties)
{
    Console.WriteLine($"Property: {prop.Name} ({prop.PropertyType.Name})");
}

// Get methods
MethodInfo[] methods = userType.GetMethods();
foreach (var method in methods)
{
    Console.WriteLine($"Method: {method.Name}");
}

// Dynamically create instance
object instance = Activator.CreateInstance(userType);

// Set property value
PropertyInfo nameProp = userType.GetProperty("Name");
nameProp.SetValue(instance, "John");

// Get property value
var name = nameProp.GetValue(instance);
Console.WriteLine(name);  // John
```

### Attributes

```csharp
// ATTRIBUTE: Metadata attached to code
[Serializable]
[System.Obsolete("Use UserV2 instead")]
public class User
{
    [Required]
    public string Name { get; set; }
    
    [Range(0, 150)]
    public int Age { get; set; }
    
    [EmailAddress]
    public string Email { get; set; }
}

// Custom attribute
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorAttribute : Attribute
{
    public string Name { get; set; }
    public string Date { get; set; }
    
    public AuthorAttribute(string name, string date)
    {
        Name = name;
        Date = date;
    }
}

// Using custom attribute
[Author("John", "2024-01-15")]
public class MyClass
{
    [Author("Jane", "2024-01-16")]
    public void MyMethod() { }
}

// Reading attributes via reflection
var classType = typeof(MyClass);
var authorAttr = classType.GetCustomAttribute<AuthorAttribute>();
Console.WriteLine($"Author: {authorAttr.Name}, Date: {authorAttr.Date}");

// Data annotation attributes (common in ASP.NET)
public class Product
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100)]
    public string Name { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
}
```

---

## 7. EXTENSION METHODS & OPERATORS ➕

### Extension Methods

```csharp
// EXTENSION METHOD: Add methods to existing types without inheritance
public static class StringExtensions
{
    // Extends string class
    public static string Capitalize(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        
        return char.ToUpper(text[0]) + text.Substring(1);
    }
    
    public static string Reverse(this string text)
    {
        return new string(text.Reverse().ToArray());
    }
    
    public static bool IsPalindrome(this string text)
    {
        string cleaned = new string(text.Where(char.IsLetterOrDigit).ToArray()).ToLower();
        return cleaned == new string(cleaned.Reverse().ToArray());
    }
}

// Usage
string name = "john";
Console.WriteLine(name.Capitalize());      // John
Console.WriteLine(name.Reverse());         // nhoj
Console.WriteLine("racecar".IsPalindrome()); // true

// Collection extensions
public static class EnumerableExtensions
{
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }
    
    public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        return source.Where(x => !predicate(x));
    }
}

// Usage
var numbers = new List<int> { 1, 2, 3, 4, 5 };
numbers
    .WhereNot(n => n % 2 == 0)  // Get odd numbers
    .ForEach(n => Console.WriteLine(n));
```

### Operator Overloading

```csharp
public class Money
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    
    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    // Overload + operator
    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new ArgumentException("Cannot add different currencies");
        
        return new Money(a.Amount + b.Amount, a.Currency);
    }
    
    // Overload - operator
    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new ArgumentException("Cannot subtract different currencies");
        
        return new Money(a.Amount - b.Amount, a.Currency);
    }
    
    // Overload * operator
    public static Money operator *(Money m, decimal multiplier)
    {
        return new Money(m.Amount * multiplier, m.Currency);
    }
    
    // Overload == operator
    public static bool operator ==(Money a, Money b)
    {
        return a.Amount == b.Amount && a.Currency == b.Currency;
    }
    
    public static bool operator !=(Money a, Money b)
    {
        return !(a == b);
    }
    
    public override string ToString() => $"{Amount} {Currency}";
}

// Usage
var money1 = new Money(100, "USD");
var money2 = new Money(50, "USD");

Console.WriteLine(money1 + money2);  // 150 USD
Console.WriteLine(money1 - money2);  // 50 USD
Console.WriteLine(money1 * 2);       // 200 USD
Console.WriteLine(money1 == money2); // false
```

---

## 8. NULLABILITY & PATTERN MATCHING 🎯

### Nullable Reference Types

```csharp
// Enable nullable reference types in .csproj:
// <PropertyGroup>
//   <Nullable>enable</Nullable>
// </PropertyGroup>

public class Person
{
    // Non-nullable string - must be initialized
    public string FirstName { get; set; }
    
    // Nullable string - can be null
    public string? LastName { get; set; }
    
    // Non-nullable int - value type, always has value
    public int Age { get; set; }
    
    // Nullable int - can be null
    public int? OptionalAge { get; set; }
}

// Usage
var person = new Person { FirstName = "John" };

// ✅ OK - checking null before use
if (person.LastName != null)
{
    Console.WriteLine(person.LastName.Length);
}

// ❌ Warning: LastName might be null
// Console.WriteLine(person.LastName.Length);

// Null-coalescing operator
string name = person.LastName ?? "Unknown";

// Null-conditional operator
int? length = person.LastName?.Length;

// Null-coalescing assignment
person.LastName ??= "DefaultLast";
```

### Pattern Matching

```csharp
public class PatternExample
{
    // TYPE PATTERN
    public string CheckType(object obj) => obj switch
    {
        string s => $"String: {s}",
        int i => $"Integer: {i}",
        bool b => $"Boolean: {b}",
        _ => "Unknown type"
    };
    
    // PROPERTY PATTERN
    public string CheckPerson(Person p) => p switch
    {
        { FirstName: "John" } => "It's John!",
        { Age: > 18 } => "Adult",
        { Age: < 18 } => "Minor",
        null => "No person",
        _ => "Someone else"
    };
    
    // RELATIONAL PATTERN
    public string CheckAge(int age) => age switch
    {
        < 0 => "Invalid",
        >= 0 and < 13 => "Child",
        >= 13 and < 18 => "Teen",
        >= 18 => "Adult",
        _ => "Unknown"
    };
    
    // LIST PATTERN (C# 11+)
    public string CheckArray(int[] numbers) => numbers switch
    {
        [] => "Empty",
        [var first] => $"Single: {first}",
        [var first, .. var rest] => $"First: {first}, others: {rest.Length}",
        _ => "Multiple"
    };
    
    // WHEN CLAUSE
    public string CheckValue(int value) => value switch
    {
        > 0 when value % 2 == 0 => "Positive even",
        > 0 when value % 2 != 0 => "Positive odd",
        < 0 => "Negative",
        _ => "Zero"
    };
}
```

---

## 9. DEPENDENCY INJECTION & IOC 🔧

### Service Registration

```csharp
// DEPENDENCY INJECTION: Provide dependencies instead of creating them

public interface ILogger
{
    void Log(string message);
}

public class ConsoleLogger : ILogger
{
    public void Log(string message) => Console.WriteLine($"[LOG] {message}");
}

public interface IEmailService
{
    Task SendAsync(string to, string message);
}

public class EmailService : IEmailService
{
    private readonly ILogger _logger;
    
    // Dependency injected via constructor
    public EmailService(ILogger logger)
    {
        _logger = logger;
    }
    
    public async Task SendAsync(string to, string message)
    {
        _logger.Log($"Sending email to {to}");
        await Task.Delay(100);  // Simulate sending
        _logger.Log("Email sent");
    }
}

// ASP.NET Core: Register in Program.cs
var builder = WebApplication.CreateBuilder(args);

// Transient: New instance every time
builder.Services.AddTransient<ILogger, ConsoleLogger>();

// Scoped: One instance per HTTP request
builder.Services.AddScoped<IEmailService, EmailService>();

// Singleton: One instance forever
// builder.Services.AddSingleton<ICache, MemoryCache>();

var app = builder.Build();

// Resolve from DI container
var emailService = app.Services.GetRequiredService<IEmailService>();
await emailService.SendAsync("user@example.com", "Hello!");
```

### Service Lifetimes

```csharp
// TRANSIENT: New instance each time
builder.Services.AddTransient<IService, MyService>();

var s1 = app.Services.GetRequiredService<IService>();
var s2 = app.Services.GetRequiredService<IService>();
// s1 != s2 (different instances)

// SCOPED: One per HTTP request
builder.Services.AddScoped<IRepository, SqlRepository>();

// In HTTP request handler:
public class OrderController
{
    private readonly IRepository _repo1;
    private readonly IRepository _repo2;
    
    public OrderController(IRepository repo1, IRepository repo2)
    {
        _repo1 = repo1;
        _repo2 = repo2;
    }
    
    public void Example()
    {
        // _repo1 == _repo2 (same instance in same request)
    }
}

// SINGLETON: One instance forever
builder.Services.AddSingleton<ICache, MemoryCache>();

var cache1 = app.Services.GetRequiredService<ICache>();
var cache2 = app.Services.GetRequiredService<ICache>();
// cache1 == cache2 (same instance always)
```

---

## 10. ENTITY FRAMEWORK CORE 🗄️

### DbContext & Models

```csharp
// MODEL: Represents database table
public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [EmailAddress]
    public string Email { get; set; }
    
    // Foreign key
    public int DepartmentId { get; set; }
    
    // Navigation property
    public Department Department { get; set; }
    
    // Collection navigation property
    public ICollection<Order> Orders { get; set; }
}

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public ICollection<User> Users { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public decimal Total { get; set; }
    public int UserId { get; set; }
    
    public User User { get; set; }
}

// DBCONTEXT: Database connection & operations
public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Order> Orders { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("connection-string");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships
        modelBuilder.Entity<User>()
            .HasOne(u => u.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DepartmentId);
    }
}
```

### CRUD Operations

```csharp
public class UserRepository
{
    private readonly ApplicationDbContext _context;
    
    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // CREATE
    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    // READ
    public async Task<User> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Department)  // Load related data
            .Include(u => u.Orders)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
    
    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }
    
    // UPDATE
    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    // DELETE
    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
    
    // ADVANCED QUERIES
    public async Task<List<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }
    
    public async Task<decimal> GetTotalOrderAmountAsync(int userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .SumAsync(o => o.Total);
    }
}
```

---

## 11. ASP.NET CORE BASICS 🌐

### Controllers & Routing

```csharp
// CONTROLLER: Handles HTTP requests
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    // Dependency injection via constructor
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAllAsync()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }
    
    // GET: api/users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetByIdAsync(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();
        
        return Ok(user);
    }
    
    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateAsync([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateAsync(request);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = user.Id }, user);
    }
    
    // PUT: api/users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userService.UpdateAsync(id, request);
        if (user == null)
            return NotFound();
        
        return Ok(user);
    }
    
    // DELETE: api/users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }
}

// REQUEST/RESPONSE MODELS
public class CreateUserRequest
{
    [Required]
    public string Name { get; set; }
    
    [EmailAddress]
    public string Email { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

### Services & Configuration

```csharp
// SERVICE: Business logic layer
public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(CreateUserRequest request);
    Task<UserDto> UpdateAsync(int id, UpdateUserRequest request);
    Task DeleteAsync(int id);
}

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    
    public UserService(IUserRepository repository, IMapper mapper, ILogger<UserService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();
        return _mapper.Map<List<UserDto>>(users);
    }
    
    public async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        _logger.LogInformation($"Creating user: {request.Name}");
        
        var user = new User
        {
            Name = request.Name,
            Email = request.Email
        };
        
        var created = await _repository.CreateAsync(user);
        return _mapper.Map<UserDto>(created);
    }
}

// PROGRAM.CS: Configure services
var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Add services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Build and run
var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
```

---

## 12. MIDDLEWARE & PIPELINES 🔀

### Middleware

```csharp
// MIDDLEWARE: Component that handles HTTP request/response

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;
    
    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        
        _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
        
        try
        {
            // Call next middleware
            await _next(context);
        }
        finally
        {
            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogInformation($"Response: {context.Response.StatusCode} ({elapsed.TotalMilliseconds}ms)");
        }
    }
}

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                message = "An error occurred",
                error = ex.Message
            };
            
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}

// REGISTER IN PROGRAM.CS
var app = builder.Build();

// Order matters!
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.MapControllers();

app.Run();
```

### Custom Pipeline

```csharp
// CUSTOM REQUEST PROCESSING
app.Use(async (context, next) =>
{
    // Before request
    Console.WriteLine("1. Before");
    
    await next();
    
    // After response
    Console.WriteLine("3. After");
});

app.Use(async (context, next) =>
{
    Console.WriteLine("2. Before");
    await next();
    Console.WriteLine("4. After");
});

app.Run(async context =>
{
    Console.WriteLine("5. Endpoint");
    await context.Response.WriteAsync("Response");
});

// Output order:
// 1. Before
// 2. Before
// 5. Endpoint
// 4. After
// 3. After
```

---

## Summary: Learning Path

### **Beginner → Intermediate → Advanced**

1. **Week 1-2:** OOP Advanced (Abstract, Virtual, Sealed)
2. **Week 3-4:** Async/Await & Task
3. **Week 5:** Generics & Constraints
4. **Week 6:** LINQ Deep Dive
5. **Week 7:** Delegates, Events, Lambda
6. **Week 8:** Reflection & Attributes
7. **Week 9:** Extension Methods & Operators
8. **Week 10:** Nullability & Pattern Matching
9. **Week 11-12:** Dependency Injection
10. **Week 13-14:** Entity Framework Core
11. **Week 15-16:** ASP.NET Core & Middleware

---

**Created:** January 21, 2025
**Target:** Advanced C# & .NET Core developers
**Scope:** Missing concepts from fundamentals documents
