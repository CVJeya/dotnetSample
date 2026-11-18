# Full Stack Engineering Learning Guide

A comprehensive reference for **C#/.NET Core**, **ASP.NET Web API**, **Entity Framework**, **Dapper**, **Microservices**, **React.js**, **Redux**, **Context API**, and **Frontend Technologies** for senior full-stack engineers.

---

## Table of Contents

1. [C# & .NET Core Fundamentals](#c--net-core-fundamentals)
2. [ASP.NET Web API](#aspnet-web-api)
3. [Entity Framework Core](#entity-framework-core)
4. [Dapper](#dapper)
5. [Microservices Architecture](#microservices-architecture)
6. [React.js Hooks & Functional Components](#reactjs-hooks--functional-components)
7. [Redux](#redux)
8. [Context API](#context-api)
9. [HTML5](#html5)
10. [CSS3](#css3)
11. [Bootstrap](#bootstrap)
12. [Tailwind CSS](#tailwind-css)
13. [Material UI](#material-ui)

---

# PART 1: C# & .NET CORE FUNDAMENTALS

## 1.1 C# Language Basics

### Syntax & Data Types

**Definition:** C# is a statically-typed, object-oriented language designed for .NET runtime. Code compiles to IL (Intermediate Language) which the CLR (Common Language Runtime) executes.

**Basic Data Types:**

```csharp
// Value types (stack-allocated)
int age = 25;
double salary = 50000.50;
bool isActive = true;
char grade = 'A';
decimal price = 99.99m;

// Reference types (heap-allocated)
string name = "John Doe";
object obj = new object();
byte[] data = new byte[10];

// Nullable types
int? nullableInt = null;
string? nullableString = null;
```

**Key Differences from JavaScript:**
- Strongly typed at compile time (vs. dynamic in JavaScript)
- Requires explicit type declarations (vs. inferred in many cases)
- Case-sensitive (like JavaScript)

---

### Classes & Objects

**Definition:** The foundation of OOP in C#. A class is a blueprint for creating objects with properties, methods, and constructors.

**Syntax:**

```csharp
public class Person
{
    // Auto-property with backing field
    public string Name { get; set; }
    public int Age { get; set; }
    
    // Property with custom logic
    private decimal _salary;
    public decimal Salary
    {
        get { return _salary; }
        set { _salary = value > 0 ? value : 0; }
    }

    // Constructor
    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }

    // Method
    public void PrintInfo()
    {
        Console.WriteLine($"{Name} is {Age} years old");
    }
}

// Usage
var person = new Person("John", 30);
person.PrintInfo(); // Output: John is 30 years old
```

**Property Initialization Styles:**

```csharp
// Style 1: Auto-property (C# 3.0+)
public string Name { get; set; }

// Style 2: Init-only property (C# 9.0+) — immutable after creation
public string Name { get; init; }

// Style 3: Field initializer (C# 3.0+)
public string Name { get; set; } = "Default";

// Style 4: Record type (C# 9.0+) — immutable by default
public record PersonRecord(string Name, int Age);
```

---

### Inheritance & Polymorphism

**Definition:** Inheritance allows a class to inherit members from a base class. Polymorphism enables treating objects of different types uniformly via a common base type or interface.

**Syntax:**

```csharp
// Base class
public abstract class Animal
{
    public string Name { get; set; }
    
    public abstract void Speak();
    
    public virtual void Move()
    {
        Console.WriteLine("Moving...");
    }
}

// Derived class
public class Dog : Animal
{
    // Override abstract method (must implement)
    public override void Speak()
    {
        Console.WriteLine($"{Name} barks!");
    }
    
    // Override virtual method (optional)
    public override void Move()
    {
        Console.WriteLine($"{Name} runs...");
        base.Move(); // Call base implementation
    }
}

// Usage (polymorphism)
Animal dog = new Dog { Name = "Buddy" };
dog.Speak(); // Output: Buddy barks!
dog.Move();  // Output: Buddy runs... Moving...
```

---

### Interfaces

**Definition:** A contract that classes must implement. Defines method signatures without implementation (abstraction without a default base).

**Syntax:**

```csharp
// Define interface
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

// Implement interface
public class UserRepository : IRepository<User>
{
    public async Task<User?> GetByIdAsync(int id)
    {
        // Implementation
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        // Implementation
    }

    // ... other methods
}

// Usage (dependency injection)
IRepository<User> repo = new UserRepository();
var user = await repo.GetByIdAsync(1);
```

**Key Differences from Abstract Classes:**
- Interface: multiple inheritance allowed, no state, no constructors
- Abstract class: single inheritance, can have state and constructors

---

### LINQ (Language Integrated Query)

**Definition:** A set of methods and query syntax for filtering, transforming, and aggregating data from collections, databases, and APIs.

**Syntax:**

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5, 6 };

// Method syntax
var evens = numbers
    .Where(n => n % 2 == 0)
    .Select(n => n * 2)
    .OrderBy(n => n)
    .ToList();
// Result: [4, 8, 12]

// Query syntax (SQL-like)
var evens2 = (from n in numbers
              where n % 2 == 0
              select n * 2
              orderby n
              select n).ToList();

// Common operations
var first = numbers.FirstOrDefault();
var count = numbers.Count(n => n > 3);
var sum = numbers.Sum();
var grouped = numbers.GroupBy(n => n % 2).ToDictionary(g => g.Key, g => g.Count());
```

**LINQ with Collections:**

```csharp
var users = new List<User>
{
    new User { Id = 1, Name = "Alice", Age = 30 },
    new User { Id = 2, Name = "Bob", Age = 25 },
    new User { Id = 3, Name = "Charlie", Age = 35 }
};

// Filter
var adults = users.Where(u => u.Age >= 30);

// Project
var names = users.Select(u => u.Name);

// Sort
var byAge = users.OrderBy(u => u.Age);

// Aggregate
var totalAge = users.Sum(u => u.Age);
var avgAge = users.Average(u => u.Age);

// Group
var byAgeGroup = users
    .GroupBy(u => u.Age / 10 * 10)
    .Select(g => new { AgeRange = g.Key, Users = g.ToList() });
```

---

### Async/Await

**Definition:** Non-blocking asynchronous programming pattern. `async` marks a method as asynchronous; `await` pauses execution until a Task completes without blocking threads.

**Syntax:**

```csharp
// Synchronous method (blocks thread)
public string FetchData()
{
    var data = GetDataFromApi(); // Blocks here
    return data;
}

// Asynchronous method (non-blocking)
public async Task<string> FetchDataAsync()
{
    var data = await GetDataFromApiAsync(); // Pauses, doesn't block thread
    return data;
}

// Method returning Task (fire and forget, but still trackable)
public async Task DoWorkAsync()
{
    await Task.Delay(1000);
    Console.WriteLine("Work done");
}

// Method returning Task<T> (async with return value)
public async Task<int> GetCountAsync()
{
    await Task.Delay(100);
    return 42;
}

// Usage
var result = await FetchDataAsync();

// Multiple async operations
var task1 = FetchDataAsync();
var task2 = GetCountAsync();
await Task.WhenAll(task1, task2); // Wait for both

// Async enumerable (C# 8.0+)
public async IAsyncEnumerable<Item> GetItemsAsync()
{
    for (int i = 0; i < 10; i++)
    {
        await Task.Delay(100);
        yield return new Item { Id = i };
    }
}

// Iterate async enumerable
await foreach (var item in GetItemsAsync())
{
    Console.WriteLine(item.Id);
}
```

---

## 1.2 .NET Core Overview

### What is .NET Core?

**Definition:** A free, open-source, cross-platform runtime for executing managed code. Executes IL code via the CLR (Common Language Runtime).

**Key Characteristics:**
- **Cross-platform:** Runs on Windows, Linux, macOS
- **Unified framework:** Single codebase for desktop, web, mobile, IoT
- **High performance:** Optimized JIT compilation, minimal allocations
- **Open source:** MIT license, community-driven

**Project Structure:**

```
MyProject/
├── MyProject.csproj          # Project file (defines dependencies, targets)
├── Program.cs                # Entry point, DI registration
├── appsettings.json          # Configuration (database, logging)
├── src/
│   ├── Controllers/          # API endpoints
│   ├── Services/             # Business logic
│   ├── Data/
│   │   ├── Entities/         # Database models
│   │   └── DbContext.cs      # EF Core context
│   └── Models/               # DTOs, ViewModels
├── Tests/                    # Unit tests
└── bin/, obj/                # Build outputs
```

---

### Dependency Injection (DI)

**Definition:** A design pattern where objects declare their dependencies instead of creating them. The DI container instantiates and injects dependencies.

**Syntax (Program.cs):**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<ICache, MemoryCache>();
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

// Build the app
var app = builder.Build();

app.MapControllers();
app.Run();
```

**Lifetimes:**

| Lifetime | Behavior | Use Case |
|----------|----------|----------|
| **Transient** | New instance every request | Stateless services |
| **Scoped** | One instance per HTTP request | DbContext, repositories |
| **Singleton** | One instance for app lifetime | Caches, configuration |

**Usage in Controllers:**

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    // Dependency injected via constructor
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _userService.GetUserAsync(id);
        return Ok(user);
    }
}
```

---

### Configuration

**Definition:** Application settings loaded from `appsettings.json`, environment variables, or command-line args. Accessed via `IConfiguration` interface.

**appsettings.json:**

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=MyDb;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "Jwt": {
    "Secret": "your-secret-key-here",
    "ExpiryMinutes": 60
  },
  "AppSettings": {
    "MaxFileSize": 10485760
  }
}
```

**Usage:**

```csharp
public class AppSettings
{
    public string Secret { get; set; }
    public int ExpiryMinutes { get; set; }
}

// In Program.cs
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("Jwt")
);

// In a service
public class JwtService
{
    private readonly AppSettings _settings;

    public JwtService(IOptions<AppSettings> options)
    {
        _settings = options.Value;
    }

    public string GenerateToken(User user)
    {
        var secret = _settings.Secret;
        // ... token generation using secret
    }
}
```

---

### Middleware & Pipeline

**Definition:** A sequence of components that handle HTTP requests and responses. Each middleware can inspect, modify, or short-circuit the request.

**Syntax:**

```csharp
var app = builder.Build();

// Middleware order matters — executed top-to-bottom on request, bottom-to-top on response

// Exception handling
app.UseExceptionHandler("/error");

// HTTPS redirection
app.UseHttpsRedirection();

// Static files (CSS, JS, images)
app.UseStaticFiles();

// Routing
app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Custom middleware
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Path}");
    await next(); // Call next middleware
    Console.WriteLine($"Response: {context.Response.StatusCode}");
});

// Endpoint routing
app.MapControllers();
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();
```

**Custom Middleware Class:**

```csharp
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
        _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
        await _next(context);
        _logger.LogInformation($"Response: {context.Response.StatusCode}");
    }
}

// Register in Program.cs
app.UseMiddleware<LoggingMiddleware>();
```

---

## Summary of C# & .NET Core Key Concepts

| Concept | Purpose | Example |
|---------|---------|---------|
| Classes & OOP | Object-oriented design | `public class User { }` |
| Interfaces | Define contracts | `public interface IRepository<T>` |
| LINQ | Query data fluently | `users.Where(u => u.Age > 30)` |
| Async/Await | Non-blocking I/O | `await GetDataAsync()` |
| Dependency Injection | Loosely coupled code | `services.AddScoped<IService, Service>()` |
| Configuration | Settings management | `appsettings.json` |
| Middleware | HTTP request pipeline | `app.Use(...)` |

---

# PART 2: ASP.NET WEB API

## 2.1 Controllers & Routing

**Definition:** Controllers handle HTTP requests and return responses. Routes map URL paths to controller actions.

**Basic Controller:**

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var products = await _service.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _service.GetByIdAsync(id);
        if (product == null)
            return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductDto dto)
    {
        var product = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateProductDto dto)
    {
        var success = await _service.UpdateAsync(id, dto);
        if (!success)
            return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success)
            return NotFound();
        return NoContent();
    }
}
```

**Route Patterns:**

```csharp
// Default (token replacement)
[Route("api/[controller]")] // → /api/products
public class ProductsController { }

// Custom route
[Route("api/v1/products")]
public class ProductsController { }

// Action-level route
[HttpGet("search")] // → /api/products/search
public async Task<ActionResult> Search(string query) { }

// Route with parameters
[HttpGet("{id:int}")] // → /api/products/123
public async Task<ActionResult> GetById(int id) { }

[HttpPost("{id}/reviews/{reviewId}")] // → /api/products/1/reviews/2
public async Task<ActionResult> AddReview(int id, int reviewId) { }

// Optional parameter
[HttpGet("{id:int?}")] // → /api/products or /api/products/5
public async Task<ActionResult> Get(int? id) { }
```

---

## 2.2 Request/Response Models

**Definition:** DTOs (Data Transfer Objects) for transferring data between client and server. Separate from domain entities to control API contract.

**Syntax:**

```csharp
// Request model (input)
public class CreateProductRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }

    [Required]
    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}

// Response model (output)
public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Pagination response
public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}

// API response wrapper
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public IEnumerable<string>? Errors { get; set; }
}
```

---

## 2.3 HTTP Methods & Status Codes

**Definition:** HTTP verbs (GET, POST, PUT, DELETE, PATCH) map to CRUD operations. Status codes indicate request outcome.

**Common Patterns:**

```csharp
[HttpGet] // Read
public async Task<ActionResult> GetAll() => Ok(data);

[HttpGet("{id}")]
public async Task<ActionResult> GetById(int id) =>
    product == null ? NotFound() : Ok(product);

[HttpPost] // Create
public async Task<ActionResult> Create(CreateDto dto) =>
    CreatedAtAction(nameof(GetById), new { id = product.Id }, product);

[HttpPut("{id}")] // Update (replace entire resource)
public async Task<ActionResult> Update(int id, UpdateDto dto) =>
    NoContent();

[HttpPatch("{id}")] // Update (partial)
public async Task<ActionResult> PartialUpdate(int id, JsonPatchDocument<UpdateDto> patch) =>
    NoContent();

[HttpDelete("{id}")] // Delete
public async Task<ActionResult> Delete(int id) =>
    NoContent();
```

**Common Status Codes:**

| Code | Name | Use Case |
|------|------|----------|
| 200 | OK | Successful GET, PUT, PATCH |
| 201 | Created | Successful POST |
| 204 | No Content | Successful DELETE, PUT, PATCH (no response body) |
| 400 | Bad Request | Invalid input validation |
| 401 | Unauthorized | Missing/invalid authentication |
| 403 | Forbidden | Authenticated but insufficient permissions |
| 404 | Not Found | Resource doesn't exist |
| 500 | Internal Server Error | Server error |

---

## 2.4 Validation

**Definition:** Data validation ensures input meets requirements before processing.

**Syntax:**

```csharp
// Data annotations
public class CreateUserRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Range(18, 120)]
    public int Age { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    [RegularExpression(@"^[A-Z][a-z]+$")]
    public string FirstName { get; set; }
}

// Validation in controller
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateUserRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState); // 400 with validation errors

    var user = await _service.CreateAsync(request);
    return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
}

// Automatic validation middleware (Program.cs)
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
            return new BadRequestObjectResult(new ApiResponse<object>
            {
                Success = false,
                Message = "Validation failed",
                Errors = errors
            });
        };
    });

// Custom validation attribute
public class ValidEmailDomainAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is string email && email.EndsWith("@company.com"))
            return ValidationResult.Success;
        return new ValidationResult("Email must be from company domain");
    }
}

// FluentValidation (alternative, more powerful)
public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(3, 100).WithMessage("Name must be 3-100 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Age)
            .InclusiveBetween(18, 120);
    }
}
```

---

## Summary of ASP.NET Web API

| Concept | Purpose | Example |
|---------|---------|---------|
| Controllers | Handle HTTP requests | `[ApiController] public class UsersController` |
| Routing | Map URLs to actions | `[Route("api/[controller]")]` |
| HTTP Methods | CRUD operations | `[HttpGet]`, `[HttpPost]`, etc. |
| Status Codes | Response outcomes | `Ok(200)`, `NotFound(404)` |
| Request/Response Models | Data transfer | `CreateUserRequest`, `UserResponse` |
| Validation | Input verification | `[Required]`, `[Range]`, FluentValidation |

---

# PART 3: ENTITY FRAMEWORK CORE

## 3.1 DbContext & Models

**Definition:** Entity Framework is an ORM (Object-Relational Mapper) that maps C# classes to database tables. DbContext manages entity instances and database operations.

**Model Definition:**

```csharp
// Domain entity
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Order> Orders { get; } = new List<Order>();
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
    
    // Foreign key navigation
    public User User { get; set; } = null!;
    public ICollection<OrderItem> Items { get; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
```

**DbContext:**

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure User
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);
        
        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        // Configure Order
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure OrderItem
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed initial data
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop", Price = 999.99m, StockQuantity = 10 },
            new Product { Id = 2, Name = "Mouse", Price = 29.99m, StockQuantity = 50 }
        );
    }
}

// Register in Program.cs
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);
```

---

## 3.2 CRUD Operations

**Definition:** Create, Read, Update, Delete — fundamental database operations using EF Core.

**Syntax:**

```csharp
public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    // CREATE
    public async Task<User> CreateUserAsync(CreateUserDto dto)
    {
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync(); // Execute INSERT
        return user;
    }

    // READ
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetUserWithOrdersAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Orders)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    // UPDATE
    public async Task<bool> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;

        user.Name = dto.Name;
        user.Email = dto.Email;

        _context.Users.Update(user);
        await _context.SaveChangesAsync(); // Execute UPDATE
        return true;
    }

    // DELETE
    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(); // Execute DELETE
        return true;
    }
}
```

---

## 3.3 Relationships & Navigation

**Definition:** Define how entities relate to each other (one-to-many, many-to-many, etc.) via foreign keys and navigation properties.

**Syntax:**

```csharp
// One-to-Many (User has many Orders)
public class User
{
    public int Id { get; set; }
    public ICollection<Order> Orders { get; } = new List<Order>();
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}

// Many-to-Many (Student enrolls in Course)
public class Student
{
    public int Id { get; set; }
    public ICollection<Enrollment> Enrollments { get; } = new List<Enrollment>();
}

public class Course
{
    public int Id { get; set; }
    public ICollection<Enrollment> Enrollments { get; } = new List<Enrollment>();
}

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrolledAt { get; set; }
    
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}

// Configure many-to-many (C# 5.0+)
modelBuilder.Entity<StudentCourse>()
    .HasKey(sc => new { sc.StudentId, sc.CourseId });

modelBuilder.Entity<StudentCourse>()
    .HasOne(sc => sc.Student)
    .WithMany(s => s.StudentCourses)
    .HasForeignKey(sc => sc.StudentId);

modelBuilder.Entity<StudentCourse>()
    .HasOne(sc => sc.Course)
    .WithMany(c => c.StudentCourses)
    .HasForeignKey(sc => sc.CourseId);

// Query with relationships
var students = await _context.Students
    .Include(s => s.Enrollments)
    .ThenInclude(e => e.Course)
    .ToListAsync();
```

---

## 3.4 Migrations

**Definition:** Database versioning system. Create migrations to evolve schema over time while preserving data.

**Syntax:**

```bash
# Create migration (generates migration files)
dotnet ef migrations add InitialCreate

# Create migration with specific context
dotnet ef migrations add AddUserTable -c AppDbContext

# Apply migrations to database
dotnet ef database update

# Rollback to previous migration
dotnet ef database update PreviousMigrationName

# Remove last migration (before applying)
dotnet ef migrations remove

# List all migrations
dotnet ef migrations list
```

**Generated Migration File:**

```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(maxLength: 100, nullable: false),
                Email = table.Column<string>(maxLength: 256, nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
                table.UniqueConstraint("AK_Users_Email", x => x.Email);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Users");
    }
}
```

---

## Summary of Entity Framework Core

| Concept | Purpose | Example |
|---------|---------|---------|
| DbContext | ORM container | `AppDbContext` with `DbSet<T>` properties |
| Entity Models | Map to tables | `public class User { }` |
| Navigation Properties | Relationships | `public ICollection<Order> Orders { get; }` |
| LINQ Queries | Query data | `_context.Users.Where(u => u.Age > 30)` |
| Migrations | Schema versioning | `dotnet ef migrations add` |
| Include/ThenInclude | Eager loading | `.Include(u => u.Orders)` |
| SaveChangesAsync | Persist changes | `await _context.SaveChangesAsync()` |

---

# PART 4: DAPPER

## 4.1 Dapper Basics

**Definition:** A lightweight ORM providing a minimal abstraction over ADO.NET. Fast, simple SQL execution with parameter binding.

**Setup:**

```csharp
using Dapper;
using System.Data;
using System.Data.SqlClient;

public class DapperRepository<T> where T : class
{
    private readonly string _connectionString;

    public DapperRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Default");
    }

    protected IDbConnection Connection => new SqlConnection(_connectionString);
}
```

---

## 4.2 CRUD with Dapper

**Syntax:**

```csharp
public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // CREATE
    public async Task<int> InsertAsync(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = @"
            INSERT INTO Users (Name, Email, CreatedAt)
            VALUES (@Name, @Email, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        var id = await connection.ExecuteScalarAsync<int>(sql, user);
        return id;
    }

    // READ
    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = "SELECT * FROM Users WHERE Id = @Id";
        
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = "SELECT * FROM Users";
        
        return await connection.QueryAsync<User>(sql);
    }

    public async Task<IEnumerable<User>> SearchAsync(string searchTerm)
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = "SELECT * FROM Users WHERE Name LIKE @SearchTerm";
        
        return await connection.QueryAsync<User>(sql, new { SearchTerm = $"%{searchTerm}%" });
    }

    // UPDATE
    public async Task<bool> UpdateAsync(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = @"
            UPDATE Users 
            SET Name = @Name, Email = @Email 
            WHERE Id = @Id";

        var affectedRows = await connection.ExecuteAsync(sql, user);
        return affectedRows > 0;
    }

    // DELETE
    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = "DELETE FROM Users WHERE Id = @Id";
        
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        return affectedRows > 0;
    }
}
```

---

## 4.3 Query Mapping

**Definition:** Map SQL results to C# objects automatically.

**Syntax:**

```csharp
// Simple mapping
public async Task<User?> GetUserAsync(int id)
{
    using var connection = new SqlConnection(_connectionString);
    var sql = "SELECT Id, Name, Email FROM Users WHERE Id = @Id";
    
    return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
}

// Multiple result sets (QueryMultiple)
public async Task<(User user, IEnumerable<Order> orders)> GetUserWithOrdersAsync(int id)
{
    using var connection = new SqlConnection(_connectionString);
    var sql = @"
        SELECT * FROM Users WHERE Id = @Id;
        SELECT * FROM Orders WHERE UserId = @Id;";

    using var reader = await connection.QueryMultipleAsync(sql, new { Id = id });
    var user = await reader.ReadFirstOrDefaultAsync<User>();
    var orders = await reader.ReadAsync<Order>();
    
    return (user, orders);
}

// Dynamic mapping
public async Task<dynamic?> GetDynamicAsync(int id)
{
    using var connection = new SqlConnection(_connectionString);
    var sql = "SELECT * FROM Users WHERE Id = @Id";
    
    return await connection.QueryFirstOrDefaultAsync(sql, new { Id = id });
}

// Custom mapping with DapperSqlBuilder
public async Task<PagedResult<User>> GetPagedAsync(int pageNumber, int pageSize, string? search = null)
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = @"
        SELECT * FROM Users
        WHERE (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Id
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY;
        
        SELECT COUNT(*) FROM Users
        WHERE (@Search IS NULL OR Name LIKE '%' + @Search + '%');";

    var parameters = new
    {
        Search = search,
        Offset = (pageNumber - 1) * pageSize,
        PageSize = pageSize
    };

    using var reader = await connection.QueryMultipleAsync(sql, parameters);
    var items = await reader.ReadAsync<User>();
    var totalCount = await reader.ReadFirstAsync<int>();

    return new PagedResult<User>
    {
        Items = items,
        TotalCount = totalCount,
        PageNumber = pageNumber,
        PageSize = pageSize
    };
}
```

---

## 4.4 Transactions

**Definition:** Ensure multiple database operations succeed or fail atomically.

**Syntax:**

```csharp
public async Task<bool> TransferFundsAsync(int fromAccountId, int toAccountId, decimal amount)
{
    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();
    
    using var transaction = connection.BeginTransaction();
    try
    {
        // Debit from account
        var sql1 = "UPDATE Accounts SET Balance = Balance - @Amount WHERE Id = @Id";
        await connection.ExecuteAsync(sql1, new { Amount = amount, Id = fromAccountId }, transaction);

        // Credit to account
        var sql2 = "UPDATE Accounts SET Balance = Balance + @Amount WHERE Id = @Id";
        await connection.ExecuteAsync(sql2, new { Amount = amount, Id = toAccountId }, transaction);

        transaction.Commit();
        return true;
    }
    catch
    {
        transaction.Rollback();
        return false;
    }
}
```

---

## Dapper vs EF Core Comparison

| Feature | Dapper | EF Core |
|---------|--------|---------|
| **Learning Curve** | Easier, just SQL | Steeper, complex abstraction |
| **Performance** | Fastest, minimal overhead | Good, some overhead |
| **Query Control** | Full SQL control | LINQ abstraction |
| **Change Tracking** | Manual | Automatic |
| **Migrations** | Manual scripts | Built-in versioning |
| **Best For** | High-performance, complex SQL | Standard CRUD apps |

---

---

# PART 5: MICROSERVICES ARCHITECTURE

## 5.1 Microservices Fundamentals

**Definition:** An architectural approach where a large application is decomposed into small, independent services. Each service owns its database, business logic, and API.

**Key Principles:**

1. **Single Responsibility** — Each service does one thing well
2. **Loose Coupling** — Services are independent, minimal dependencies
3. **High Cohesion** — Related functionality grouped together
4. **Decentralized** — Services manage their own data and state
5. **API-Driven** — Services communicate via APIs (REST, gRPC, messaging)

**Monolith vs Microservices:**

```
MONOLITH                          MICROSERVICES
┌──────────────────────┐         ┌──────────────┐
│  User Service        │         │  User API    │
│  Product Service     │    →    │  Order API   │
│  Order Service       │         │  Product API │
│  Shared Database     │         │ + 3 Databases│
└──────────────────────┘         └──────────────┘
  Single deployment        Independent deployment
  Tight coupling           Loose coupling
  Shared schema            Own schema per service
```

---

## 5.2 Service Architecture Pattern

**Definition:** How individual microservices are structured internally.

**Layered Architecture Within a Service:**

```csharp
// Project Structure
UserService/
├── UserService.Api/              # API Layer
│   ├── Controllers/
│   │   └── UsersController.cs
│   └── Program.cs
├── UserService.Application/      # Business Logic
│   ├── Services/
│   │   └── UserService.cs
│   ├── DTOs/
│   │   ├── CreateUserDto.cs
│   │   └── UserResponse.cs
│   └── Interfaces/
│       └── IUserService.cs
├── UserService.Domain/           # Domain Models
│   ├── Entities/
│   │   └── User.cs
│   └── ValueObjects/
├── UserService.Infrastructure/   # Data Access
│   ├── Data/
│   │   └── UserDbContext.cs
│   ├── Repositories/
│   │   └── UserRepository.cs
│   └── ExternalServices/
└── UserService.Tests/            # Tests
```

**Service Template:**

```csharp
// Domain Entity
namespace UserService.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

// Application Service
namespace UserService.Application.Services
{
    public interface IUserService
    {
        Task<UserResponse?> GetUserAsync(int id);
        Task<IEnumerable<UserResponse>> GetAllUsersAsync();
        Task<UserResponse> CreateUserAsync(CreateUserDto dto);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<UserResponse?> GetUserAsync(int id)
        {
            var user = await _repository.GetByIdAsync(id);
            return _mapper.Map<UserResponse>(user);
        }

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            var users = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserResponse>>(users);
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserDto dto)
        {
            var user = new User { Name = dto.Name, Email = dto.Email, CreatedAt = DateTime.UtcNow };
            await _repository.AddAsync(user);
            return _mapper.Map<UserResponse>(user);
        }
    }
}

// API Controller
namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetUser(int id)
        {
            var user = await _service.GetUserAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserResponse>> CreateUser(CreateUserDto dto)
        {
            var user = await _service.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
    }
}

// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);
builder.Services.AddAutoMapper(typeof(Program));
var app = builder.Build();
app.MapControllers();
app.Run();
```

---

## 5.3 Service-to-Service Communication

**Definition:** How microservices talk to each other.

**REST/HTTP Communication:**

```csharp
// OrderService calling UserService
public class OrderService
{
    private readonly HttpClient _httpClient;
    private readonly string _userServiceUrl;

    public OrderService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _userServiceUrl = config["Services:UserService"];
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderDto dto)
    {
        // Get user from UserService
        var userResponse = await _httpClient.GetAsync($"{_userServiceUrl}/api/users/{dto.UserId}");
        if (!userResponse.IsSuccessStatusCode)
            throw new Exception("User not found");

        var user = await userResponse.Content.ReadAsAsync<UserResponse>();

        var order = new Order { UserId = dto.UserId, Total = dto.Total };
        // Save order...
        return new OrderResponse { Id = order.Id, User = user };
    }
}

// Register HttpClient with Polly (retry/circuit breaker)
builder.Services.AddHttpClient<OrderService>()
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2)))
    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
```

**Message Queue Communication (RabbitMQ/Azure Service Bus):**

```csharp
// OrderService publishes OrderCreated event
public class OrderService
{
    private readonly IMessagePublisher _publisher;

    public OrderService(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
    {
        var order = new Order { UserId = dto.UserId, Total = dto.Total };
        // Save order...

        // Publish event for other services to listen
        await _publisher.PublishAsync(new OrderCreatedEvent
        {
            OrderId = order.Id,
            UserId = order.UserId,
            Total = order.Total
        });

        return order;
    }
}

// NotificationService listens for OrderCreated
public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;

    public OrderCreatedEventHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        await _emailService.SendOrderConfirmationAsync(@event.UserId, @event.OrderId);
    }
}

// Register message handlers
builder.Services.AddScoped<IEventHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();
builder.Services.AddRabbitMqEventBus(builder.Configuration);
```

---

## 5.4 API Gateway

**Definition:** A single entry point for clients. Routes requests to appropriate microservices, handles authentication, rate limiting, and aggregation.

**Syntax:**

```csharp
// Using Ocelot (API Gateway library)
// appsettings.json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/users/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5001
        }
      ],
      "UpstreamPathTemplate": "/users/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ]
    },
    {
      "DownstreamPathTemplate": "/api/orders/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5002
        }
      ],
      "UpstreamPathTemplate": "/orders/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ]
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "http://localhost:5000"
  }
}

// Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);
builder.Configuration.AddJsonFile("ocelot.json");
builder.Services.AddOcelot();
var app = builder.Build();
await app.UseOcelot();
app.Run();
```

**Custom Gateway Implementation:**

```csharp
[ApiController]
[Route("api")]
public class GatewayController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authService;

    public GatewayController(HttpClient httpClient, IAuthenticationService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    [HttpGet("users/{id}")]
    public async Task<ActionResult> GetUser(int id)
    {
        // Authentication & Authorization
        var claims = User.Claims;
        if (claims == null)
            return Unauthorized();

        // Route to UserService
        var response = await _httpClient.GetAsync($"http://user-service:5001/api/users/{id}");
        var content = await response.Content.ReadAsStringAsync();
        return Ok(JsonSerializer.Deserialize<object>(content));
    }

    [HttpPost("orders")]
    public async Task<ActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var token = HttpContext.Request.Headers["Authorization"];

        // Forward to OrderService with auth
        var request = new HttpRequestMessage(HttpMethod.Post, "http://order-service:5002/api/orders");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(dto);

        var response = await _httpClient.SendAsync(request);
        return Ok(await response.Content.ReadAsStringAsync());
    }
}
```

---

## 5.5 Service Discovery

**Definition:** Mechanism for services to find each other dynamically (instead of hard-coding URLs).

**Consul Example:**

```csharp
// Register service with Consul
builder.Services.AddConsul(builder.Configuration);

// In appsettings.json
{
  "Consul": {
    "Host": "localhost",
    "Port": 8500,
    "ServiceName": "UserService",
    "ServiceId": "UserService-1",
    "ServiceAddress": "localhost",
    "ServicePort": 5001,
    "HealthCheckUrl": "http://localhost:5001/health"
  }
}

// Discover service
public class OrderService
{
    private readonly IConsulClient _consul;
    private readonly HttpClient _httpClient;

    public OrderService(IConsulClient consul, HttpClient httpClient)
    {
        _consul = consul;
        _httpClient = httpClient;
    }

    public async Task<UserResponse> GetUserAsync(int userId)
    {
        // Query Consul for UserService instance
        var services = await _consul.Catalog.Service("UserService");
        var service = services.Response.FirstOrDefault();
        
        if (service == null)
            throw new Exception("UserService not available");

        var url = $"http://{service.ServiceAddress}:{service.ServicePort}/api/users/{userId}";
        var response = await _httpClient.GetAsync(url);
        return await response.Content.ReadAsAsync<UserResponse>();
    }
}
```

---

## 5.6 Distributed Tracing & Monitoring

**Definition:** Track requests across multiple services for debugging and performance monitoring.

**Syntax (OpenTelemetry):**

```csharp
// Program.cs
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddJaegerExporter(options =>
        {
            options.AgentHost = "localhost";
            options.AgentPort = 6831;
        })
    );

// Usage (automatic tracing for HTTP calls and DB queries)
public class OrderService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderService> _logger;

    public OrderService(HttpClient httpClient, ILogger<OrderService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderDto dto)
    {
        // This HTTP call is automatically traced
        var userResponse = await _httpClient.GetAsync($"http://user-service/api/users/{dto.UserId}");
        
        _logger.LogInformation("Order created for user {UserId}", dto.UserId);
        // All logs include trace ID for correlation
        
        return new OrderResponse { Id = 1 };
    }
}

// View traces in Jaeger UI (http://localhost:16686)
```

---

## Microservices Checklist

| Aspect | Consideration |
|--------|---------------|
| **Service Boundary** | Define by business capability (Users, Orders, Products) |
| **Data Ownership** | Each service has its own database |
| **Communication** | REST (sync) or Messaging (async) |
| **Resilience** | Timeouts, retries, circuit breakers |
| **API Gateway** | Single entry point (Ocelot, Kong, AWS API Gateway) |
| **Service Discovery** | Dynamic registration (Consul, Eureka, Kubernetes DNS) |
| **Monitoring** | Centralized logging, tracing (ELK, Jaeger, Datadog) |
| **Deployment** | Containerization (Docker) + Orchestration (Kubernetes) |

---

# PART 6: REACT.JS HOOKS & FUNCTIONAL COMPONENTS

## 6.1 React Fundamentals Review

**Definition:** React is a JavaScript library for building UI with components (reusable pieces). Functional components with hooks are the modern standard.

**React Component Basics:**

```jsx
// Simple functional component
function Greeting({ name }) {
  return <h1>Hello, {name}!</h1>;
}

// Usage
<Greeting name="John" />

// Component with children
function Container({ children, className }) {
  return <div className={className}>{children}</div>;
}

<Container className="card">
  <Greeting name="Alice" />
</Container>
```

---

## 6.2 Hooks: useState

**Definition:** Hook that lets functional components have local state. Returns `[state, setState]` tuple.

**Syntax:**

```jsx
import { useState } from 'react';

function Counter() {
  const [count, setCount] = useState(0);
  const [name, setName] = useState('John');

  return (
    <div>
      <p>Count: {count}</p>
      <button onClick={() => setCount(count + 1)}>Increment</button>
      
      <input 
        value={name} 
        onChange={(e) => setName(e.target.value)} 
      />
      <p>Name: {name}</p>
    </div>
  );
}
```

**Lazy Initialization:**

```jsx
// Expensive computation runs only once
function Component() {
  const [data, setData] = useState(() => {
    return expensiveComputation(); // Runs only on mount
  });
  return <div>{data}</div>;
}
```

---

## 6.3 Hooks: useEffect

**Definition:** Hook for side effects (API calls, subscriptions, DOM changes). Runs after render. Dependency array controls when.

**Syntax:**

```jsx
import { useEffect, useState } from 'react';

function UserProfile({ userId }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Run once on mount
  useEffect(() => {
    const fetchUser = async () => {
      try {
        const response = await fetch(`/api/users/${userId}`);
        const data = await response.json();
        setUser(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchUser();
  }, []); // Empty dependency array = run once

  // Run when userId changes
  useEffect(() => {
    const fetchUser = async () => {
      setLoading(true);
      try {
        const response = await fetch(`/api/users/${userId}`);
        const data = await response.json();
        setUser(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchUser();
  }, [userId]); // Re-run when userId changes

  // Cleanup (unsubscribe, clear timers)
  useEffect(() => {
    const timer = setInterval(() => {
      console.log('Tick');
    }, 1000);

    return () => clearInterval(timer); // Cleanup function
  }, []);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;
  return <div>{user.name}</div>;
}
```

---

## 6.4 Hooks: useContext

**Definition:** Hook to access values from Context without prop drilling.

**Syntax:**

```jsx
import { createContext, useContext } from 'react';

// Create context
const ThemeContext = createContext();

// Provider component
function ThemeProvider({ children }) {
  const [theme, setTheme] = useState('light');

  const toggleTheme = () => {
    setTheme(t => t === 'light' ? 'dark' : 'light');
  };

  return (
    <ThemeContext.Provider value={{ theme, toggleTheme }}>
      {children}
    </ThemeContext.Provider>
  );
}

// Custom hook for consuming context
function useTheme() {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within ThemeProvider');
  }
  return context;
}

// Usage
function App() {
  return (
    <ThemeProvider>
      <Header />
      <Content />
    </ThemeProvider>
  );
}

function Header() {
  const { theme, toggleTheme } = useTheme();
  return (
    <header style={{ background: theme === 'light' ? '#fff' : '#333' }}>
      <button onClick={toggleTheme}>Toggle Theme</button>
    </header>
  );
}
```

---

## 6.5 Hooks: useReducer

**Definition:** Hook for managing complex state with multiple related values. Similar to Redux for component-level state.

**Syntax:**

```jsx
import { useReducer } from 'react';

const initialState = { count: 0, step: 1 };

function reducer(state, action) {
  switch (action.type) {
    case 'INCREMENT':
      return { ...state, count: state.count + state.step };
    case 'DECREMENT':
      return { ...state, count: state.count - state.step };
    case 'SET_STEP':
      return { ...state, step: action.payload };
    case 'RESET':
      return initialState;
    default:
      return state;
  }
}

function Counter() {
  const [state, dispatch] = useReducer(reducer, initialState);

  return (
    <div>
      <p>Count: {state.count}</p>
      <p>Step: {state.step}</p>
      
      <button onClick={() => dispatch({ type: 'INCREMENT' })}>
        Increment
      </button>
      <button onClick={() => dispatch({ type: 'DECREMENT' })}>
        Decrement
      </button>
      <input 
        value={state.step}
        onChange={(e) => dispatch({ type: 'SET_STEP', payload: parseInt(e.target.value) })}
      />
      <button onClick={() => dispatch({ type: 'RESET' })}>
        Reset
      </button>
    </div>
  );
}
```

---

## 6.6 Hooks: useCallback & useMemo

**Definition:** Performance optimization hooks. `useCallback` memoizes functions; `useMemo` memoizes values.

**Syntax:**

```jsx
import { useCallback, useMemo } from 'react';

function ProductList({ products, onSelect }) {
  // Memoize callback (prevents re-creating on every render)
  const handleClick = useCallback((productId) => {
    onSelect(productId);
  }, [onSelect]);

  // Memoize expensive computation
  const sortedProducts = useMemo(() => {
    return [...products].sort((a, b) => a.price - b.price);
  }, [products]);

  return (
    <div>
      {sortedProducts.map(product => (
        <button key={product.id} onClick={() => handleClick(product.id)}>
          {product.name}
        </button>
      ))}
    </div>
  );
}
```

---

## 6.7 Hooks: useRef

**Definition:** Hook for accessing DOM elements directly or storing mutable values that don't trigger re-renders.

**Syntax:**

```jsx
import { useRef } from 'react';

function TextInput() {
  const inputRef = useRef(null);

  const focusInput = () => {
    inputRef.current.focus();
  };

  return (
    <div>
      <input ref={inputRef} type="text" />
      <button onClick={focusInput}>Focus</button>
    </div>
  );
}

// Store mutable value (doesn't trigger re-render)
function Timer() {
  const intervalRef = useRef(null);
  const [isRunning, setIsRunning] = useState(false);

  const start = () => {
    setIsRunning(true);
    intervalRef.current = setInterval(() => {
      // Timer logic
    }, 1000);
  };

  const stop = () => {
    clearInterval(intervalRef.current);
    setIsRunning(false);
  };

  return (
    <div>
      <button onClick={isRunning ? stop : start}>
        {isRunning ? 'Stop' : 'Start'}
      </button>
    </div>
  );
}
```

---

## 6.8 Custom Hooks

**Definition:** User-defined hooks that encapsulate reusable logic.

**Syntax:**

```jsx
import { useState, useEffect } from 'react';

// Custom hook: useWindowSize
function useWindowSize() {
  const [size, setSize] = useState({
    width: window.innerWidth,
    height: window.innerHeight
  });

  useEffect(() => {
    const handleResize = () => {
      setSize({
        width: window.innerWidth,
        height: window.innerHeight
      });
    };

    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  return size;
}

// Custom hook: useFetch
function useFetch(url) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch(url);
        const json = await response.json();
        setData(json);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [url]);

  return { data, loading, error };
}

// Usage
function App() {
  const { width, height } = useWindowSize();
  const { data, loading, error } = useFetch('/api/products');

  return (
    <div>
      <p>Window: {width}x{height}</p>
      {loading && <p>Loading...</p>}
      {error && <p>Error: {error}</p>}
      {data && <p>Data: {JSON.stringify(data)}</p>}
    </div>
  );
}
```

---

## Summary of React Hooks

| Hook | Purpose | Example |
|------|---------|---------|
| `useState` | Local state | `const [count, setCount] = useState(0)` |
| `useEffect` | Side effects | `useEffect(() => {}, [])` |
| `useContext` | Access context values | `const value = useContext(MyContext)` |
| `useReducer` | Complex state logic | `const [state, dispatch] = useReducer(reducer, initial)` |
| `useCallback` | Memoize functions | `const fn = useCallback(() => {}, [deps])` |
| `useMemo` | Memoize values | `const val = useMemo(() => expensive(), [deps])` |
| `useRef` | DOM access, mutable values | `const ref = useRef(null)` |
| Custom Hooks | Reusable logic | `function useFetch(url) {}` |

---

---

# PART 7: REDUX

## 7.1 Redux Fundamentals

**Definition:** Redux is a predictable state management library for JavaScript. It centralizes application state in a single store, making state changes traceable and debuggable.

**Core Concepts:**

```
┌─────────────┐
│   Store     │ ← Single source of truth
└─────────────┘
      ↑
      │ dispatch(action)
      │
┌─────────────┐
│  Reducers   │ ← Pure functions: (state, action) → newState
└─────────────┘
      ↑
      │
┌─────────────┐
│  Actions    │ ← Objects describing what happened
└─────────────┘
      ↑
      │
┌─────────────┐
│ Components  │ ← Dispatch actions, subscribe to state
└─────────────┘
```

---

## 7.2 Store, Actions, Reducers

**Definition:**

- **Store:** Container for the entire application state
- **Action:** Plain object describing an event (type + optional payload)
- **Reducer:** Pure function that takes current state + action, returns new state

**Syntax:**

```javascript
import { createStore } from 'redux';

// 1. Define initial state
const initialState = {
  users: [],
  loading: false,
  error: null,
  selectedUserId: null
};

// 2. Define action types (constants prevent typos)
const FETCH_USERS_REQUEST = 'FETCH_USERS_REQUEST';
const FETCH_USERS_SUCCESS = 'FETCH_USERS_SUCCESS';
const FETCH_USERS_FAILURE = 'FETCH_USERS_FAILURE';
const SELECT_USER = 'SELECT_USER';
const DELETE_USER = 'DELETE_USER';

// 3. Define action creators (functions that return actions)
function fetchUsersRequest() {
  return { type: FETCH_USERS_REQUEST };
}

function fetchUsersSuccess(users) {
  return { type: FETCH_USERS_SUCCESS, payload: users };
}

function fetchUsersFailure(error) {
  return { type: FETCH_USERS_FAILURE, payload: error };
}

function selectUser(userId) {
  return { type: SELECT_USER, payload: userId };
}

function deleteUser(userId) {
  return { type: DELETE_USER, payload: userId };
}

// 4. Define reducer (pure function)
function usersReducer(state = initialState, action) {
  switch (action.type) {
    case FETCH_USERS_REQUEST:
      return {
        ...state,
        loading: true,
        error: null
      };

    case FETCH_USERS_SUCCESS:
      return {
        ...state,
        loading: false,
        users: action.payload,
        error: null
      };

    case FETCH_USERS_FAILURE:
      return {
        ...state,
        loading: false,
        error: action.payload
      };

    case SELECT_USER:
      return {
        ...state,
        selectedUserId: action.payload
      };

    case DELETE_USER:
      return {
        ...state,
        users: state.users.filter(u => u.id !== action.payload)
      };

    default:
      return state;
  }
}

// 5. Create store
const store = createStore(usersReducer);

// 6. Subscribe to store changes
store.subscribe(() => {
  console.log('State changed:', store.getState());
});

// 7. Dispatch actions
store.dispatch(fetchUsersRequest());
store.dispatch(fetchUsersSuccess([
  { id: 1, name: 'Alice' },
  { id: 2, name: 'Bob' }
]));
store.dispatch(selectUser(1));
store.dispatch(deleteUser(2));
```

---

## 7.3 Redux with React (react-redux)

**Definition:** Bindings that connect Redux store to React components via Provider and hooks.

**Syntax:**

```javascript
import React from 'react';
import { Provider, useDispatch, useSelector } from 'react-redux';
import { createStore } from 'redux';

// ... (reducer and actions from above)

// 1. Wrap app with Provider
function App() {
  return (
    <Provider store={store}>
      <UserList />
      <UserDetail />
    </Provider>
  );
}

// 2. Component: Select state with useSelector
function UserList() {
  const dispatch = useDispatch();
  const { users, loading, error } = useSelector(state => state);

  React.useEffect(() => {
    // Fetch users on mount
    dispatch(fetchUsersRequest());
    
    fetch('/api/users')
      .then(res => res.json())
      .then(data => dispatch(fetchUsersSuccess(data)))
      .catch(err => dispatch(fetchUsersFailure(err.message)));
  }, [dispatch]);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <ul>
      {users.map(user => (
        <li key={user.id} onClick={() => dispatch(selectUser(user.id))}>
          {user.name}
          <button onClick={() => dispatch(deleteUser(user.id))}>Delete</button>
        </li>
      ))}
    </ul>
  );
}

// 3. Component: Display selected user
function UserDetail() {
  const selectedUserId = useSelector(state => state.selectedUserId);
  const users = useSelector(state => state.users);
  const user = users.find(u => u.id === selectedUserId);

  if (!user) return <div>No user selected</div>;

  return (
    <div>
      <h2>{user.name}</h2>
      <p>ID: {user.id}</p>
    </div>
  );
}
```

---

## 7.4 Redux Middleware & Thunks

**Definition:** Middleware intercepts actions before they reach reducers. `redux-thunk` allows async operations in actions.

**Syntax:**

```javascript
import { createStore, applyMiddleware } from 'redux';
import thunk from 'redux-thunk';

// Redux Thunk: action creator returns function instead of object
function fetchUsers() {
  return async (dispatch) => {
    dispatch(fetchUsersRequest());
    try {
      const response = await fetch('/api/users');
      const data = await response.json();
      dispatch(fetchUsersSuccess(data));
    } catch (error) {
      dispatch(fetchUsersFailure(error.message));
    }
  };
}

// Apply middleware
const store = createStore(usersReducer, applyMiddleware(thunk));

// Usage
dispatch(fetchUsers()); // Automatically handles async

// Custom middleware (logging)
function loggingMiddleware(store) {
  return (next) => (action) => {
    console.log('Dispatching:', action);
    const result = next(action);
    console.log('New state:', store.getState());
    return result;
  };
}

const store = createStore(
  usersReducer,
  applyMiddleware(thunk, loggingMiddleware)
);
```

---

## 7.5 Redux Toolkit (Modern Approach)

**Definition:** Opinionated, batteries-included Redux library. Simplifies store setup, reducers, and middleware configuration.

**Syntax:**

```javascript
import { createSlice, configureStore } from '@reduxjs/toolkit';

// Define slice (reducer + actions)
const usersSlice = createSlice({
  name: 'users',
  initialState: {
    items: [],
    loading: false,
    error: null,
    selectedId: null
  },
  reducers: {
    // Synchronous actions
    selectUser: (state, action) => {
      state.selectedId = action.payload;
    },
    deleteUser: (state, action) => {
      state.items = state.items.filter(u => u.id !== action.payload);
    }
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchUsers.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchUsers.fulfilled, (state, action) => {
        state.loading = false;
        state.items = action.payload;
      })
      .addCase(fetchUsers.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  }
});

// Create async thunk (handles pending/fulfilled/rejected)
import { createAsyncThunk } from '@reduxjs/toolkit';

export const fetchUsers = createAsyncThunk(
  'users/fetchUsers',
  async (_, { rejectWithValue }) => {
    try {
      const response = await fetch('/api/users');
      return await response.json();
    } catch (error) {
      return rejectWithValue(error.message);
    }
  }
);

// Export actions
export const { selectUser, deleteUser } = usersSlice.actions;

// Create store
const store = configureStore({
  reducer: {
    users: usersSlice.reducer
  }
});

export default store;
```

**Usage in Components:**

```javascript
import { useDispatch, useSelector } from 'react-redux';
import { fetchUsers, selectUser, deleteUser } from './usersSlice';

function UserList() {
  const dispatch = useDispatch();
  const { items, loading, error, selectedId } = useSelector(state => state.users);

  React.useEffect(() => {
    dispatch(fetchUsers());
  }, [dispatch]);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div>
      {items.map(user => (
        <div key={user.id} className={selectedId === user.id ? 'selected' : ''}>
          <span onClick={() => dispatch(selectUser(user.id))}>{user.name}</span>
          <button onClick={() => dispatch(deleteUser(user.id))}>Delete</button>
        </div>
      ))}
    </div>
  );
}
```

---

## 7.6 Redux DevTools

**Definition:** Browser extension for debugging Redux state changes and time-traveling through actions.

**Setup:**

```javascript
import { configureStore } from '@reduxjs/toolkit';

const store = configureStore({
  reducer: {
    users: usersSlice.reducer
  },
  devTools: process.env.NODE_ENV !== 'production'
});

// Open Redux DevTools browser extension to:
// - View state
// - See dispatched actions
// - Time-travel (replay actions)
// - Diff state changes
// - Export/import state
```

---

## Redux vs Context API Comparison

| Aspect | Redux | Context API |
|--------|-------|------------|
| **Complexity** | Verbose, boilerplate | Simple, minimal |
| **DevTools** | Excellent debugging | None |
| **Performance** | Optimized | Can cause unnecessary re-renders |
| **Scalability** | Large apps | Small to medium apps |
| **Learning Curve** | Steep | Shallow |
| **Best For** | Complex state, many updates | Simple state, theme, auth |

---

# PART 8: CONTEXT API (Detailed)

## 8.1 Context API Fundamentals

**Definition:** Built-in React API for sharing state across components without prop drilling.

**Syntax:**

```javascript
import { createContext, useContext, useState } from 'react';

// 1. Create context
const AuthContext = createContext();

// 2. Create provider component
function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  const login = async (email, password) => {
    setLoading(true);
    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password })
      });
      const data = await response.json();
      setUser(data);
      localStorage.setItem('token', data.token);
    } catch (error) {
      console.error('Login failed:', error);
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem('token');
  };

  const value = { user, login, logout, loading };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

// 3. Custom hook to consume context
function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}

// 4. Usage in App
function App() {
  return (
    <AuthProvider>
      <Routes />
    </AuthProvider>
  );
}

function Dashboard() {
  const { user, logout } = useAuth();

  if (!user) return <Navigate to="/login" />;

  return (
    <div>
      <h1>Welcome, {user.name}</h1>
      <button onClick={logout}>Logout</button>
    </div>
  );
}
```

---

## 8.2 Multiple Contexts

**Definition:** Combining multiple context providers for different state domains.

**Syntax:**

```javascript
// Theme Context
const ThemeContext = createContext();

function ThemeProvider({ children }) {
  const [theme, setTheme] = useState('light');

  const toggleTheme = () => {
    setTheme(t => t === 'light' ? 'dark' : 'light');
  };

  return (
    <ThemeContext.Provider value={{ theme, toggleTheme }}>
      {children}
    </ThemeContext.Provider>
  );
}

function useTheme() {
  return useContext(ThemeContext);
}

// Notification Context
const NotificationContext = createContext();

function NotificationProvider({ children }) {
  const [notifications, setNotifications] = useState([]);

  const addNotification = (message, type = 'info') => {
    const id = Date.now();
    setNotifications(prev => [...prev, { id, message, type }]);
    setTimeout(() => removeNotification(id), 3000);
  };

  const removeNotification = (id) => {
    setNotifications(prev => prev.filter(n => n.id !== id));
  };

  return (
    <NotificationContext.Provider value={{ notifications, addNotification }}>
      {children}
    </NotificationContext.Provider>
  );
}

function useNotification() {
  return useContext(NotificationContext);
}

// App with multiple providers
function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <NotificationProvider>
          <MainApp />
        </NotificationProvider>
      </AuthProvider>
    </ThemeProvider>
  );
}

// Component using multiple contexts
function Header() {
  const { user, logout } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const { addNotification } = useNotification();

  const handleLogout = () => {
    logout();
    addNotification('Logged out successfully', 'success');
  };

  return (
    <header style={{ background: theme === 'light' ? '#fff' : '#333' }}>
      <span>{user.name}</span>
      <button onClick={toggleTheme}>Toggle Theme</button>
      <button onClick={handleLogout}>Logout</button>
    </header>
  );
}
```

---

## 8.3 useReducer with Context

**Definition:** Combine useReducer with Context for more structured state management (Redux-like but simpler).

**Syntax:**

```javascript
import { createContext, useReducer, useContext } from 'react';

const TodoContext = createContext();

// Action types
const ADD_TODO = 'ADD_TODO';
const DELETE_TODO = 'DELETE_TODO';
const TOGGLE_TODO = 'TOGGLE_TODO';

// Reducer function
function todoReducer(state, action) {
  switch (action.type) {
    case ADD_TODO:
      return {
        ...state,
        todos: [...state.todos, { id: Date.now(), text: action.payload, completed: false }]
      };
    case DELETE_TODO:
      return {
        ...state,
        todos: state.todos.filter(t => t.id !== action.payload)
      };
    case TOGGLE_TODO:
      return {
        ...state,
        todos: state.todos.map(t =>
          t.id === action.payload ? { ...t, completed: !t.completed } : t
        )
      };
    default:
      return state;
  }
}

// Provider
function TodoProvider({ children }) {
  const [state, dispatch] = useReducer(todoReducer, { todos: [] });

  return (
    <TodoContext.Provider value={{ state, dispatch }}>
      {children}
    </TodoContext.Provider>
  );
}

function useTodo() {
  const context = useContext(TodoContext);
  if (!context) {
    throw new Error('useTodo must be used within TodoProvider');
  }
  return context;
}

// Usage
function TodoList() {
  const { state, dispatch } = useTodo();
  const [input, setInput] = useState('');

  const handleAdd = () => {
    dispatch({ type: ADD_TODO, payload: input });
    setInput('');
  };

  return (
    <div>
      <input value={input} onChange={(e) => setInput(e.target.value)} />
      <button onClick={handleAdd}>Add</button>

      <ul>
        {state.todos.map(todo => (
          <li key={todo.id}>
            <input
              type="checkbox"
              checked={todo.completed}
              onChange={() => dispatch({ type: TOGGLE_TODO, payload: todo.id })}
            />
            <span style={{ textDecoration: todo.completed ? 'line-through' : 'none' }}>
              {todo.text}
            </span>
            <button onClick={() => dispatch({ type: DELETE_TODO, payload: todo.id })}>
              Delete
            </button>
          </li>
        ))}
      </ul>
    </div>
  );
}

function App() {
  return (
    <TodoProvider>
      <TodoList />
    </TodoProvider>
  );
}
```

---

## Context API Performance Optimization

**Definition:** Prevent unnecessary re-renders when Context value changes.

**Problem (causes all consumers to re-render):**

```javascript
function MyProvider({ children }) {
  const [user, setUser] = useState(null);
  const [theme, setTheme] = useState('light');

  // This object is recreated every render, causing all consumers to re-render
  const value = { user, theme };

  return <MyContext.Provider value={value}>{children}</MyContext.Provider>;
}
```

**Solution (memoize context value):**

```javascript
import { useMemo } from 'react';

function MyProvider({ children }) {
  const [user, setUser] = useState(null);
  const [theme, setTheme] = useState('light');

  // Memoize value to prevent unnecessary re-renders
  const value = useMemo(() => ({ user, theme }), [user, theme]);

  return <MyContext.Provider value={value}>{children}</MyContext.Provider>;
}
```

**Split Contexts (separate read/write):**

```javascript
const UserStateContext = createContext(); // For reading
const UserDispatchContext = createContext(); // For dispatching

function UserProvider({ children }) {
  const [user, setUser] = useState(null);

  return (
    <UserStateContext.Provider value={user}>
      <UserDispatchContext.Provider value={setUser}>
        {children}
      </UserDispatchContext.Provider>
    </UserStateContext.Provider>
  );
}

// Only re-render if that specific context changes
function UserDisplay() {
  const user = useContext(UserStateContext);
  return <div>{user?.name}</div>;
}

function LoginForm() {
  const setUser = useContext(UserDispatchContext);
  const handleLogin = () => setUser({ name: 'John' });
  return <button onClick={handleLogin}>Login</button>;
}
```

---

## Summary: Redux vs Context API vs useReducer

| Use Case | Recommendation |
|----------|---|
| Simple state (theme, auth) | Context API |
| Form state within component | useState |
| Complex state, many dispatches | useReducer + Context or Redux Toolkit |
| Large app, DevTools debugging needed | Redux Toolkit |
| Predictable state management | Redux Toolkit |
| Avoiding prop drilling | Context API |

---

---

# PART 9: FRONTEND TECHNOLOGIES

## 9.1 HTML5

**Definition:** Markup language for structuring web content. HTML5 adds semantic elements and native media support.

**Semantic HTML:**

```html
<!-- Good: Semantic structure -->
<article>
  <header>
    <h1>Article Title</h1>
    <time datetime="2024-01-15">January 15, 2024</time>
  </header>
  
  <nav>
    <ul>
      <li><a href="/">Home</a></li>
      <li><a href="/about">About</a></li>
      <li><a href="/contact">Contact</a></li>
    </ul>
  </nav>
  
  <main>
    <section>
      <h2>Introduction</h2>
      <p>Content here...</p>
    </section>
    
    <aside>
      <h3>Related</h3>
      <p>Side content...</p>
    </aside>
  </main>
  
  <footer>
    <p>&copy; 2024 My Site</p>
  </footer>
</article>

<!-- Bad: Non-semantic divs (don't use) -->
<div class="article">
  <div class="header">
    <h1>Article Title</h1>
  </div>
  <div class="content">...</div>
</div>
```

**Key Semantic Elements:**

| Element | Purpose |
|---------|---------|
| `<header>` | Introductory content, logo, navigation |
| `<nav>` | Navigation links |
| `<main>` | Primary content (one per page) |
| `<article>` | Self-contained content (blog post, comment) |
| `<section>` | Thematic grouping |
| `<aside>` | Sidebar, related links |
| `<footer>` | Footer, copyright, links |
| `<figure>` | Self-contained media |
| `<figcaption>` | Caption for figure |

**Form Elements:**

```html
<form>
  <!-- Text input -->
  <label for="name">Name:</label>
  <input type="text" id="name" name="name" required />

  <!-- Email input (built-in validation) -->
  <label for="email">Email:</label>
  <input type="email" id="email" name="email" />

  <!-- Password input -->
  <label for="password">Password:</label>
  <input type="password" id="password" name="password" />

  <!-- Number input -->
  <label for="age">Age:</label>
  <input type="number" id="age" name="age" min="18" max="120" />

  <!-- Date input -->
  <label for="dob">Date of Birth:</label>
  <input type="date" id="dob" name="dob" />

  <!-- Select dropdown -->
  <label for="country">Country:</label>
  <select id="country" name="country">
    <option value="">Select...</option>
    <option value="us">United States</option>
    <option value="uk">United Kingdom</option>
  </select>

  <!-- Checkbox -->
  <label>
    <input type="checkbox" name="agree" required />
    I agree to terms
  </label>

  <!-- Radio button -->
  <fieldset>
    <legend>Gender:</legend>
    <label>
      <input type="radio" name="gender" value="male" /> Male
    </label>
    <label>
      <input type="radio" name="gender" value="female" /> Female
    </label>
  </fieldset>

  <!-- Textarea -->
  <label for="message">Message:</label>
  <textarea id="message" name="message" rows="5"></textarea>

  <!-- Submit button -->
  <button type="submit">Submit</button>
</form>
```

**Accessibility (a11y):**

```html
<!-- Use proper headings hierarchy -->
<h1>Main Title</h1>
<h2>Subsection</h2>
<h3>Minor section</h3>

<!-- Alt text for images -->
<img src="logo.png" alt="Company logo" />

<!-- ARIA labels for complex elements -->
<button aria-label="Close menu">×</button>

<!-- Skip to main content link -->
<a href="#main" class="sr-only">Skip to main content</a>
<main id="main">...</main>

<!-- Use semantic HTML for screen readers -->
<article role="article">...</article>
<button type="button">Click me</button> <!-- Better than <div onClick> -->
```

---

## 9.2 CSS3

**Definition:** Cascading Style Sheets for styling HTML. CSS3 adds flexbox, grid, animations, and advanced selectors.

**Selectors:**

```css
/* Element selector */
p { color: blue; }

/* Class selector */
.highlight { background: yellow; }

/* ID selector (use sparingly) */
#header { font-size: 24px; }

/* Attribute selector */
input[type="email"] { border: 1px solid blue; }

/* Pseudo-class */
a:hover { color: red; }
input:focus { outline: 2px solid blue; }
li:first-child { font-weight: bold; }
li:nth-child(odd) { background: #f5f5f5; }

/* Pseudo-element */
p::first-line { text-transform: uppercase; }
p::before { content: ">> "; }

/* Combinators */
div > p { margin: 0; } /* Direct child */
div p { margin: 0; }  /* Any descendant */
h2 + p { margin-top: 0; } /* Adjacent sibling */
h2 ~ p { color: gray; } /* General sibling */

/* Multiple selectors */
h1, h2, h3 { font-family: Arial; }
```

**Flexbox (1D Layout):**

```css
.container {
  display: flex;
  justify-content: center;      /* Horizontal alignment */
  align-items: center;          /* Vertical alignment */
  gap: 16px;                    /* Space between items */
  flex-wrap: wrap;              /* Wrap to next line */
}

.item {
  flex: 1;                      /* Grow equally */
  flex-basis: 200px;            /* Base size */
}

.item-grow { flex: 2; }        /* Grow 2x faster */
.item-no-shrink { flex-shrink: 0; }
```

**Grid (2D Layout):**

```css
.grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);  /* 3 equal columns */
  grid-template-rows: auto 1fr auto;      /* 3 rows */
  gap: 20px;
}

.grid-item {
  grid-column: 1 / 3;  /* Span columns 1-3 */
  grid-row: 1 / 2;
}

/* Responsive grid */
@media (max-width: 768px) {
  .grid {
    grid-template-columns: 1fr;
  }
}
```

**Responsive Design:**

```css
/* Mobile-first approach */
.container {
  width: 100%;
  padding: 10px;
}

/* Tablet */
@media (min-width: 768px) {
  .container {
    width: 750px;
    padding: 20px;
  }
}

/* Desktop */
@media (min-width: 1024px) {
  .container {
    width: 960px;
    padding: 30px;
  }
}

/* Print */
@media print {
  body { font-size: 12pt; }
  .no-print { display: none; }
}
```

**Animations:**

```css
/* Define keyframes */
@keyframes slide-in {
  from {
    transform: translateX(-100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

/* Apply animation */
.sidebar {
  animation: slide-in 0.3s ease-in-out forwards;
}

/* Transition (smooth property change) */
.button {
  background: blue;
  transition: background 0.3s ease;
}

.button:hover {
  background: darkblue;
}

/* Transform */
.card {
  transform: rotate(5deg) scale(1.1);
  transition: transform 0.2s;
}

.card:hover {
  transform: rotate(0deg) scale(1);
}
```

**Variables (CSS Custom Properties):**

```css
:root {
  --primary-color: #0066cc;
  --secondary-color: #f0f0f0;
  --spacing-unit: 8px;
  --border-radius: 4px;
}

.button {
  background: var(--primary-color);
  padding: var(--spacing-unit) calc(var(--spacing-unit) * 2);
  border-radius: var(--border-radius);
}

/* Dark theme override */
@media (prefers-color-scheme: dark) {
  :root {
    --primary-color: #0099ff;
    --secondary-color: #1a1a1a;
  }
}
```

---

## 9.3 Bootstrap

**Definition:** Popular CSS framework with pre-built components and responsive grid system.

**Installation & Setup:**

```html
<!-- CDN -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>

<!-- npm -->
npm install bootstrap
```

**Grid System:**

```html
<div class="container">
  <div class="row">
    <!-- 12-column grid, responsive -->
    <div class="col-12 col-md-6 col-lg-4">Column 1</div>
    <div class="col-12 col-md-6 col-lg-4">Column 2</div>
    <div class="col-12 col-md-12 col-lg-4">Column 3</div>
  </div>
</div>

<!-- Container variations -->
<div class="container">Full width, max 1200px</div>
<div class="container-fluid">Full viewport width</div>
```

**Common Components:**

```html
<!-- Button -->
<button class="btn btn-primary">Primary</button>
<button class="btn btn-secondary">Secondary</button>
<button class="btn btn-danger">Danger</button>
<button class="btn btn-sm">Small</button>
<button class="btn btn-lg">Large</button>

<!-- Alert -->
<div class="alert alert-success">Success!</div>
<div class="alert alert-danger">Error occurred</div>

<!-- Card -->
<div class="card">
  <div class="card-header">Header</div>
  <div class="card-body">
    <h5 class="card-title">Title</h5>
    <p class="card-text">Content</p>
  </div>
</div>

<!-- Navigation bar -->
<nav class="navbar navbar-expand-lg navbar-light bg-light">
  <div class="container-fluid">
    <a class="navbar-brand" href="#">Site</a>
    <button class="navbar-toggler" data-bs-toggle="collapse" data-bs-target="#navmenu">
      <span class="navbar-toggler-icon"></span>
    </button>
    <div id="navmenu" class="collapse navbar-collapse">
      <ul class="navbar-nav ms-auto">
        <li class="nav-item"><a class="nav-link" href="/">Home</a></li>
        <li class="nav-item"><a class="nav-link" href="/about">About</a></li>
      </ul>
    </div>
  </div>
</nav>

<!-- Modal -->
<button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#myModal">Open</button>
<div id="myModal" class="modal fade">
  <div class="modal-dialog">
    <div class="modal-content">
      <div class="modal-header"><h5>Title</h5></div>
      <div class="modal-body">Content</div>
      <div class="modal-footer">
        <button class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
      </div>
    </div>
  </div>
</div>

<!-- Form -->
<form>
  <div class="mb-3">
    <label for="email" class="form-label">Email:</label>
    <input type="email" id="email" class="form-control" />
  </div>
  <div class="mb-3">
    <label for="password" class="form-label">Password:</label>
    <input type="password" id="password" class="form-control" />
  </div>
  <button type="submit" class="btn btn-primary">Login</button>
</form>
```

**Spacing Utilities:**

```html
<!-- Padding: pt-3 (padding-top), px-4 (padding-left+right) -->
<div class="p-3">Padding all sides</div>
<div class="px-4 py-2">Padding horizontal/vertical</div>

<!-- Margin: mt-5, mb-2, mx-auto (center) -->
<div class="mt-5 mb-2">Margin top/bottom</div>
<div class="mx-auto">Center horizontally</div>

<!-- Gap between flex items -->
<div class="d-flex gap-3">
  <div>Item 1</div>
  <div>Item 2</div>
</div>
```

---

## 9.4 Tailwind CSS

**Definition:** Utility-first CSS framework. Build layouts using small, composable classes instead of pre-built components.

**Installation & Setup:**

```bash
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
```

**Configuration (tailwind.config.js):**

```javascript
export default {
  content: ['./index.html', './src/**/*.{js,jsx,ts,tsx}'],
  theme: {
    extend: {
      colors: {
        primary: '#0066cc',
        secondary: '#f0f0f0'
      },
      spacing: {
        '128': '32rem'
      }
    }
  },
  plugins: []
};
```

**Utility Classes:**

```html
<!-- Layout -->
<div class="flex items-center justify-between gap-4 p-6">
  <h1 class="text-3xl font-bold">Title</h1>
  <button>Menu</button>
</div>

<!-- Colors -->
<div class="bg-blue-500 text-white">Blue background, white text</div>
<div class="border-2 border-red-300">Red border</div>

<!-- Typography -->
<h1 class="text-4xl font-bold leading-tight">Heading</h1>
<p class="text-base font-normal text-gray-600">Paragraph</p>

<!-- Spacing -->
<div class="mt-8 mb-4 px-6 py-3">Margin/padding</div>

<!-- Sizing -->
<div class="w-full h-screen">Full width and height</div>
<div class="w-1/2 h-64">Half width, fixed height</div>

<!-- Responsive -->
<div class="text-sm md:text-base lg:text-lg">
  Small on mobile, larger on tablet/desktop
</div>

<!-- Flexbox -->
<div class="flex flex-col gap-4 md:flex-row">
  <div class="flex-1">Item 1</div>
  <div class="flex-1">Item 2</div>
  <div class="flex-1">Item 3</div>
</div>

<!-- Grid -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
  <div>Card 1</div>
  <div>Card 2</div>
  <div>Card 3</div>
</div>

<!-- Shadow & Rounded corners -->
<div class="rounded-lg shadow-md hover:shadow-xl transition-shadow">Card</div>

<!-- State variants -->
<button class="bg-blue-500 hover:bg-blue-600 focus:ring-4 disabled:opacity-50">
  Button
</button>

<!-- Dark mode -->
<div class="bg-white dark:bg-gray-900 text-black dark:text-white">
  Content
</div>
```

**Creating Components with @apply:**

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer components {
  .btn-primary {
    @apply px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors;
  }

  .card {
    @apply bg-white rounded-lg shadow-md p-6;
  }

  .input {
    @apply w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent;
  }
}
```

**Usage in React:**

```jsx
// Use Tailwind classes directly in JSX
function Button({ children, variant = 'primary' }) {
  const variants = {
    primary: 'bg-blue-500 hover:bg-blue-600 text-white',
    secondary: 'bg-gray-300 hover:bg-gray-400 text-black',
    danger: 'bg-red-500 hover:bg-red-600 text-white'
  };

  return (
    <button className={`px-4 py-2 rounded-lg transition-colors ${variants[variant]}`}>
      {children}
    </button>
  );
}

function App() {
  return (
    <div className="min-h-screen bg-gray-50 p-4">
      <div className="max-w-4xl mx-auto">
        <h1 className="text-4xl font-bold text-gray-900 mb-8">Dashboard</h1>
        
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {[1, 2, 3].map(i => (
            <div key={i} className="bg-white rounded-lg shadow-md p-6">
              <h2 className="text-lg font-semibold mb-2">Card {i}</h2>
              <p className="text-gray-600 mb-4">Content goes here</p>
              <Button variant="primary">Action</Button>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
```

---

## 9.5 Material UI (MUI)

**Definition:** React component library implementing Google's Material Design. Pre-built, accessible, themeable components.

**Installation & Setup:**

```bash
npm install @mui/material @emotion/react @emotion/styled
npm install @mui/icons-material  # Optional: icon library
```

**Basic Usage:**

```jsx
import { Button, TextField, Card, CardContent, Typography, Container, Box } from '@mui/material';
import { ThemeProvider, createTheme } from '@mui/material/styles';

// Create custom theme
const theme = createTheme({
  palette: {
    primary: {
      main: '#0066cc',
    },
    secondary: {
      main: '#f50057',
    },
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
  },
});

function App() {
  return (
    <ThemeProvider theme={theme}>
      <Container maxWidth="md">
        <Box sx={{ py: 4 }}>
          <Typography variant="h4" component="h1" gutterBottom>
            Welcome
          </Typography>
          <Typography variant="body1" color="textSecondary">
            This is a Material UI example
          </Typography>
        </Box>
      </Container>
    </ThemeProvider>
  );
}
```

**Common Components:**

```jsx
import {
  Button,
  TextField,
  Card,
  CardHeader,
  CardContent,
  CardActions,
  AppBar,
  Toolbar,
  Drawer,
  List,
  ListItem,
  ListItemText,
  Grid,
  Paper,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Snackbar,
  Alert,
  LinearProgress,
  Table,
  TableContainer,
  TableHead,
  TableBody,
  TableRow,
  TableCell,
  Pagination,
  Chip,
  Avatar,
  Rating
} from '@mui/material';

// Button variations
<Button variant="contained" color="primary">Contained</Button>
<Button variant="outlined" color="secondary">Outlined</Button>
<Button variant="text">Text</Button>
<Button size="small">Small</Button>
<Button size="large" fullWidth>Large Full Width</Button>

// Form inputs
<TextField
  label="Email"
  type="email"
  variant="outlined"
  fullWidth
  margin="normal"
  error={!!errors.email}
  helperText={errors.email?.message}
/>

<TextField
  select
  label="Choose an option"
  value={selected}
  onChange={(e) => setSelected(e.target.value)}
>
  <MenuItem value="option1">Option 1</MenuItem>
  <MenuItem value="option2">Option 2</MenuItem>
</TextField>

// Card
<Card>
  <CardHeader
    avatar={<Avatar>R</Avatar>}
    title="Card Title"
    subheader="September 14, 2016"
  />
  <CardContent>
    <Typography variant="body2" color="textSecondary">
      This is the card content
    </Typography>
  </CardContent>
  <CardActions>
    <Button size="small">Learn More</Button>
  </CardActions>
</Card>

// App Bar & Navigation
<AppBar position="static">
  <Toolbar>
    <Typography variant="h6" sx={{ flexGrow: 1 }}>
      My App
    </Typography>
    <Button color="inherit">Login</Button>
  </Toolbar>
</AppBar>

// Modal Dialog
<Dialog open={open} onClose={handleClose}>
  <DialogTitle>Confirm Action</DialogTitle>
  <DialogContent>
    <Typography>Are you sure?</Typography>
  </DialogContent>
  <DialogActions>
    <Button onClick={handleClose}>Cancel</Button>
    <Button onClick={handleConfirm} variant="contained">
      Confirm
    </Button>
  </DialogActions>
</Dialog>

// Table
<TableContainer component={Paper}>
  <Table>
    <TableHead>
      <TableRow sx={{ backgroundColor: '#f5f5f5' }}>
        <TableCell>Name</TableCell>
        <TableCell>Email</TableCell>
        <TableCell align="right">Actions</TableCell>
      </TableRow>
    </TableHead>
    <TableBody>
      {users.map((user) => (
        <TableRow key={user.id}>
          <TableCell>{user.name}</TableCell>
          <TableCell>{user.email}</TableCell>
          <TableCell align="right">
            <Button size="small">Edit</Button>
          </TableCell>
        </TableRow>
      ))}
    </TableBody>
  </Table>
</TableContainer>

// Notification
<Snackbar open={open} autoHideDuration={6000} onClose={handleClose}>
  <Alert onClose={handleClose} severity="success">
    Operation completed successfully!
  </Alert>
</Snackbar>

// Grid layout
<Grid container spacing={2}>
  <Grid item xs={12} sm={6} md={4}>
    <Paper>Content 1</Paper>
  </Grid>
  <Grid item xs={12} sm={6} md={4}>
    <Paper>Content 2</Paper>
  </Grid>
  <Grid item xs={12} sm={12} md={4}>
    <Paper>Content 3</Paper>
  </Grid>
</Grid>

// Chip (tag/label)
<Chip label="React" color="primary" variant="outlined" />

// Rating
<Rating value={rating} onChange={(e, value) => setRating(value)} />

// Progress
<LinearProgress variant="determinate" value={progress} />
```

**Styling with sx Prop:**

```jsx
import { Box, Typography } from '@mui/material';

function StyledComponent() {
  return (
    <Box
      sx={{
        backgroundColor: '#f5f5f5',
        p: 3,                    // padding
        borderRadius: 2,
        boxShadow: 1,
        '&:hover': {
          boxShadow: 3,
          backgroundColor: '#e0e0e0'
        },
        '@media (max-width: 600px)': {
          p: 1
        }
      }}
    >
      <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
        Styled Box
      </Typography>
    </Box>
  );
}
```

---

## Frontend Frameworks Comparison

| Framework | Strengths | Best For |
|-----------|-----------|----------|
| **Bootstrap** | Pre-built components, easy grid, widely used | Quick prototypes, traditional layouts |
| **Tailwind CSS** | Flexible, lightweight, customizable, responsive | Custom designs, modern apps |
| **Material UI** | Rich component library, Material Design, accessible | Enterprise apps, complex UIs |
| **Vanilla CSS** | Full control, no dependencies | Minimal projects, performance-critical |

---

## End of Full Stack Learning Guide

**Complete Coverage:**
✅ C# & .NET Core
✅ ASP.NET Web API
✅ Entity Framework Core
✅ Dapper
✅ Microservices Architecture
✅ React.js Hooks & Functional Components
✅ Redux
✅ Context API
✅ HTML5
✅ CSS3
✅ Bootstrap
✅ Tailwind CSS
✅ Material UI

---

### Quick Reference Checklists

**Backend Stack:**
- [ ] Understand C# OOP (classes, interfaces, inheritance)
- [ ] Master async/await patterns
- [ ] Use LINQ for data queries
- [ ] Configure dependency injection
- [ ] Build RESTful APIs with ASP.NET
- [ ] Design database schemas with EF Core
- [ ] Implement migrations for schema versioning
- [ ] Use Dapper for complex/performant queries
- [ ] Decompose monolith into microservices
- [ ] Implement inter-service communication
- [ ] Set up API gateway

**Frontend Stack:**
- [ ] Write semantic HTML5
- [ ] Master CSS3 (flexbox, grid, animations)
- [ ] Use Bootstrap for rapid UI development
- [ ] Leverage Tailwind utilities for custom designs
- [ ] Build component libraries with Material UI
- [ ] Use React hooks (useState, useEffect, custom)
- [ ] Manage state with Redux Toolkit
- [ ] Simplify state with Context API
- [ ] Optimize performance (memoization, code splitting)
- [ ] Implement authentication/authorization

**Full Stack Patterns:**
- [ ] API-driven architecture
- [ ] Separation of concerns
- [ ] Error handling & validation
- [ ] Logging & monitoring
- [ ] Security (CORS, JWT, encryption)
- [ ] Testing (unit, integration, E2E)
- [ ] CI/CD pipelines
- [ ] Docker containerization
- [ ] Performance optimization

---

**Document completed.** Save this as a reference for senior full-stack engineers.
