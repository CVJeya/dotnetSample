# OOP & SOLID Principles - Practice Exercises

This document contains **20 practical coding exercises** to master OOP concepts and SOLID principles.

Each exercise includes:
- ❌ **BAD code** (what NOT to do)
- ✅ **GOOD code** (best practices)
- 📝 **Explanation** (why this matters)
- 💡 **Variations** (extensions to try)

---

## Part 1: Encapsulation Exercises

### Exercise 1.1: Bank Account (Basic Encapsulation)

**Goal:** Protect account balance from invalid operations

#### ❌ BAD: Exposed internal state
```csharp
public class BadBankAccount
{
    public decimal Balance;  // ❌ Anyone can modify!
    public string AccountNumber;
    
    public void Deposit(decimal amount)
    {
        Balance += amount;
    }
    
    public void Withdraw(decimal amount)
    {
        Balance -= amount;  // ❌ No validation!
    }
}

// Usage (dangerous):
var account = new BadBankAccount { Balance = 1000 };
account.Balance = -5000;  // ❌ BUG: negative balance!
account.Withdraw(999999);  // ❌ BUG: overdraw!
```

#### ✅ GOOD: Encapsulation with validation
```csharp
public class GoodBankAccount
{
    private decimal _balance;
    private readonly string _accountNumber;
    private readonly decimal _overdraftLimit;
    
    public decimal Balance => _balance;  // Read-only access
    public string AccountNumber => _accountNumber;
    
    public GoodBankAccount(string accountNumber, decimal initialBalance = 0, decimal overdraftLimit = 500)
    {
        if (initialBalance < 0)
            throw new ArgumentException("Initial balance cannot be negative");
        
        _accountNumber = accountNumber ?? throw new ArgumentNullException(nameof(accountNumber));
        _balance = initialBalance;
        _overdraftLimit = overdraftLimit;
    }
    
    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive");
        
        _balance += amount;
        Console.WriteLine($"Deposited {amount:C}. New balance: {_balance:C}");
    }
    
    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive");
        
        if (_balance - amount < -_overdraftLimit)
            throw new InvalidOperationException($"Insufficient funds. Balance: {_balance:C}, Limit: {_overdraftLimit:C}");
        
        _balance -= amount;
        Console.WriteLine($"Withdrew {amount:C}. New balance: {_balance:C}");
    }
    
    public decimal GetBalance() => _balance;
}

// Usage (safe):
var account = new GoodBankAccount("ACC123", 1000);
account.Deposit(500);      // ✅ Deposited $500. New balance: $1500
account.Withdraw(200);     // ✅ Withdrew $200. New balance: $1300
account.Withdraw(2000);    // ✅ Exception: Insufficient funds
```

#### 📝 Explanation
- `_balance` is private — nobody can modify it directly
- Properties provide controlled read-only access
- Methods validate input before modifying state
- Constructor ensures valid initial state

#### 💡 Variations to Try
1. Add `TransactionHistory` list to track deposits/withdrawals
2. Add `InterestRate` property and `ApplyInterest()` method
3. Add transaction fee logic to `Withdraw()`

---

### Exercise 1.2: Employee Salary (Encapsulation with Validation)

**Goal:** Prevent invalid salary modifications

#### ❌ BAD: No validation
```csharp
public class BadEmployee
{
    public string Name;
    public decimal Salary;
    
    public void GiveRaise(decimal amount)
    {
        Salary += amount;  // ❌ Could be negative!
    }
}

var emp = new BadEmployee { Name = "John", Salary = 50000 };
emp.GiveRaise(-100000);  // ❌ Salary becomes -50000!
```

#### ✅ GOOD: Proper encapsulation
```csharp
public class GoodEmployee
{
    private string _name;
    private decimal _salary;
    private readonly DateTime _hireDate;
    
    public string Name
    {
        get => _name;
        set => _name = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    public decimal Salary => _salary;
    public DateTime HireDate => _hireDate;
    
    public GoodEmployee(string name, decimal initialSalary)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name required");
        if (initialSalary < 15000)
            throw new ArgumentException("Salary must meet minimum wage");
        
        _name = name;
        _salary = initialSalary;
        _hireDate = DateTime.Now;
    }
    
    public void GiveRaise(decimal percentage)
    {
        if (percentage <= 0 || percentage > 50)
            throw new ArgumentException("Raise must be between 0% and 50%");
        
        decimal raiseAmount = _salary * (percentage / 100);
        _salary += raiseAmount;
        Console.WriteLine($"Given raise of {percentage}%. New salary: {_salary:C}");
    }
    
    public void SetSalary(decimal newSalary)
    {
        if (newSalary < 15000)
            throw new ArgumentException("Salary must meet minimum wage");
        if (newSalary < _salary)
            throw new InvalidOperationException("Salary can only increase or stay same");
        
        _salary = newSalary;
    }
}

// Usage:
var emp = new GoodEmployee("John", 50000);
emp.GiveRaise(10);    // ✅ Salary increased 10%
emp.GiveRaise(100);   // ✅ Exception: Raise > 50%
```

#### 💡 Variations
1. Add department and manager fields
2. Add `PromoteToManager()` method that validates years of service
3. Track salary history

---

## Part 2: Abstraction Exercises

### Exercise 2.1: Payment Processing (Hide Complexity)

**Goal:** Hide payment gateway complexity behind simple interface

#### ❌ BAD: Exposes all payment complexity
```csharp
public class BadPaymentProcessor
{
    public void ProcessPayment(string cardNumber, string cvv, decimal amount, string cardholderName)
    {
        // Validate card number
        if (cardNumber.Length != 16)
            throw new ArgumentException("Invalid card number");
        
        // Connect to payment gateway
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.paymentgateway.com/charge");
        request.Headers.Add("Authorization", "Bearer sk_live_12345");
        
        var body = new
        {
            amount = (int)(amount * 100),  // Convert to cents
            currency = "usd",
            card = new { number = cardNumber, cvc = cvv },
            description = cardholderName
        };
        
        var response = client.SendAsync(request).Result;
        
        if (!response.IsSuccessStatusCode)
            throw new Exception("Payment failed");
        
        Console.WriteLine("Payment processed");
    }
}

// Usage (messy):
var processor = new BadPaymentProcessor();
processor.ProcessPayment("4111111111111111", "123", 99.99m, "John Doe");
```

#### ✅ GOOD: Abstraction hides complexity
```csharp
// Abstract the concept
public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(PaymentRequest request);
}

public class PaymentRequest
{
    public string CardNumber { get; set; }
    public string Cvv { get; set; }
    public string CardholderName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}

public class PaymentResult
{
    public bool IsSuccessful { get; set; }
    public string TransactionId { get; set; }
    public string ErrorMessage { get; set; }
}

// Implementation 1: Stripe
public class StripePaymentGateway : IPaymentGateway
{
    private readonly string _apiKey;
    
    public StripePaymentGateway(string apiKey)
    {
        _apiKey = apiKey;
    }
    
    public async Task<PaymentResult> ChargeAsync(PaymentRequest request)
    {
        ValidateCard(request);
        
        // Stripe-specific code
        var client = new HttpClient();
        var stripeRequest = new
        {
            amount = (int)(request.Amount * 100),
            currency = request.Currency,
            card = new { number = request.CardNumber, cvc = request.Cvv }
        };
        
        var response = await client.PostAsJsonAsync("https://api.stripe.com/v1/charges", stripeRequest);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsAsync<dynamic>();
            return new PaymentResult
            {
                IsSuccessful = true,
                TransactionId = content.id
            };
        }
        
        return new PaymentResult
        {
            IsSuccessful = false,
            ErrorMessage = "Payment failed"
        };
    }
    
    private void ValidateCard(PaymentRequest request)
    {
        if (string.IsNullOrEmpty(request.CardNumber) || request.CardNumber.Length != 16)
            throw new ArgumentException("Invalid card number");
        if (string.IsNullOrEmpty(request.Cvv) || request.Cvv.Length != 3)
            throw new ArgumentException("Invalid CVV");
    }
}

// Implementation 2: PayPal
public class PayPalPaymentGateway : IPaymentGateway
{
    private readonly string _clientId;
    
    public PayPalPaymentGateway(string clientId)
    {
        _clientId = clientId;
    }
    
    public async Task<PaymentResult> ChargeAsync(PaymentRequest request)
    {
        // PayPal-specific code
        var client = new HttpClient();
        var paypalRequest = new
        {
            intent = "sale",
            payer = new { payment_method = "credit_card" },
            transactions = new[] { new { amount = new { total = request.Amount } } }
        };
        
        var response = await client.PostAsJsonAsync("https://api.paypal.com/v1/payments/payment", paypalRequest);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsAsync<dynamic>();
            return new PaymentResult
            {
                IsSuccessful = true,
                TransactionId = content.id
            };
        }
        
        return new PaymentResult { IsSuccessful = false };
    }
}

// Clean usage - no knowledge of payment gateway details!
public class OrderService
{
    private readonly IPaymentGateway _paymentGateway;
    
    public OrderService(IPaymentGateway paymentGateway)
    {
        _paymentGateway = paymentGateway;
    }
    
    public async Task<bool> ProcessOrderAsync(Order order, string cardNumber, string cvv)
    {
        var request = new PaymentRequest
        {
            CardNumber = cardNumber,
            Cvv = cvv,
            Amount = order.Total,
            Currency = "usd"
        };
        
        var result = await _paymentGateway.ChargeAsync(request);
        return result.IsSuccessful;
    }
}

// Usage (clean):
IPaymentGateway gateway = new StripePaymentGateway("sk_live_12345");
var service = new OrderService(gateway);
await service.ProcessOrderAsync(order, "4111111111111111", "123");

// Switch to PayPal without changing OrderService!
gateway = new PayPalPaymentGateway("client_id");
```

#### 💡 Variations
1. Add `IPaymentValidator` interface to validate cards
2. Add `IPaymentLogger` to log transactions
3. Implement ApplePayGateway

---

### Exercise 2.2: Notification Service (Multiple Implementations)

**Goal:** Send notifications via multiple channels without exposing complexity

#### ✅ GOOD: Abstraction pattern
```csharp
public interface INotificationService
{
    Task SendAsync(string recipient, string message);
}

public class EmailNotificationService : INotificationService
{
    private readonly SmtpClient _smtpClient;
    
    public async Task SendAsync(string recipient, string message)
    {
        var mail = new MailMessage("noreply@example.com", recipient)
        {
            Subject = "Notification",
            Body = message
        };
        
        await _smtpClient.SendMailAsync(mail);
    }
}

public class SmsNotificationService : INotificationService
{
    private readonly TwilioClient _twilioClient;
    
    public async Task SendAsync(string recipient, string message)
    {
        await _twilioClient.SendSmsAsync(recipient, message);
    }
}

public class SlackNotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;
    
    public async Task SendAsync(string recipient, string message)
    {
        var payload = new { text = $"{recipient}: {message}" };
        await _httpClient.PostAsJsonAsync(_webhookUrl, payload);
    }
}

// Clean usage:
INotificationService notifier = new EmailNotificationService();
await notifier.SendAsync("user@example.com", "Order confirmed!");

// Switch channel without changing calling code:
notifier = new SmsNotificationService();
await notifier.SendAsync("+1234567890", "Order confirmed!");
```

---

## Part 3: Inheritance Exercises

### Exercise 3.1: Vehicle Hierarchy

**Goal:** Share common code between vehicle types

#### ❌ BAD: Duplicate code
```csharp
public class BadCar
{
    public string Make { get; set; }
    public string Model { get; set; }
    public decimal Speed { get; set; }
    
    public void Drive() => Console.WriteLine($"Car driving at {Speed} mph");
    public void Accelerate() => Speed += 10;
    public void Brake() => Speed -= 10;
}

public class BadTruck
{
    public string Make { get; set; }
    public string Model { get; set; }
    public decimal Speed { get; set; }
    
    public void Drive() => Console.WriteLine($"Truck driving at {Speed} mph");  // ❌ Duplicate!
    public void Accelerate() => Speed += 10;  // ❌ Duplicate!
    public void Brake() => Speed -= 10;  // ❌ Duplicate!
}

public class BadMotorcycle
{
    public string Make { get; set; }
    public string Model { get; set; }
    public decimal Speed { get; set; }
    
    public void Drive() => Console.WriteLine($"Motorcycle driving at {Speed} mph");  // ❌ Duplicate!
    public void Accelerate() => Speed += 10;  // ❌ Duplicate!
    public void Brake() => Speed -= 10;  // ❌ Duplicate!
}
```

#### ✅ GOOD: Inheritance eliminates duplication
```csharp
// Parent class with shared code
public abstract class Vehicle
{
    public string Make { get; set; }
    public string Model { get; set; }
    protected decimal _speed;
    
    public decimal Speed
    {
        get => _speed;
        protected set => _speed = Math.Max(0, value);  // Never negative
    }
    
    public virtual void Drive()
    {
        Console.WriteLine($"Vehicle driving at {Speed} mph");
    }
    
    public virtual void Accelerate(decimal amount)
    {
        Speed += amount;
        Console.WriteLine($"Accelerated to {Speed} mph");
    }
    
    public virtual void Brake(decimal amount)
    {
        Speed -= amount;
        Console.WriteLine($"Braked to {Speed} mph");
    }
}

// Child class 1: Car
public class Car : Vehicle
{
    public int Doors { get; set; }
    
    public override void Drive()
    {
        Console.WriteLine($"Car ({Doors} doors) driving at {Speed} mph");
    }
    
    public override void Accelerate(decimal amount)
    {
        base.Accelerate(amount * 1.2m);  // Cars accelerate slightly faster
    }
}

// Child class 2: Truck
public class Truck : Vehicle
{
    public decimal CargoWeight { get; set; }
    
    public override void Drive()
    {
        Console.WriteLine($"Truck (cargo: {CargoWeight}kg) driving at {Speed} mph");
    }
    
    public override void Accelerate(decimal amount)
    {
        base.Accelerate(amount * 0.8m);  // Trucks accelerate slower when loaded
    }
}

// Child class 3: Motorcycle
public class Motorcycle : Vehicle
{
    public bool HasSidecar { get; set; }
    
    public override void Drive()
    {
        Console.WriteLine($"Motorcycle{(HasSidecar ? " with sidecar" : "")} driving at {Speed} mph");
    }
    
    public override void Accelerate(decimal amount)
    {
        base.Accelerate(amount * 1.5m);  // Motorcycles accelerate fastest
    }
}

// Usage:
var vehicles = new Vehicle[]
{
    new Car { Make = "Honda", Model = "Civic", Doors = 4 },
    new Truck { Make = "Ford", Model = "F-150", CargoWeight = 5000 },
    new Motorcycle { Make = "Harley", Model = "Street 750", HasSidecar = false }
};

foreach (var vehicle in vehicles)
{
    vehicle.Drive();       // Each outputs differently
    vehicle.Accelerate(10);  // Each accelerates differently
}
```

#### 📝 Explanation
- Shared code (Make, Model, Speed) in parent class
- Shared behavior (Accelerate, Brake) in parent class with virtual methods
- Each child class customizes behavior via `override`
- Child classes inherit all parent functionality

#### 💡 Variations
1. Add `Refuel()` method to parent
2. Add different fuel types (Fuel for Car, Diesel for Truck, etc.)
3. Add `GetMaxSpeed()` abstract method
4. Create ElectricCar that overrides refuel

---

### Exercise 3.2: Employee Hierarchy (Abstract Base Class)

**Goal:** Different employee types share common structure

#### ✅ GOOD: Abstract class with inheritance
```csharp
public abstract class Employee
{
    public string Name { get; set; }
    public string EmployeeId { get; set; }
    public decimal BaseSalary { get; set; }
    
    public Employee(string name, string id, decimal baseSalary)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        EmployeeId = id ?? throw new ArgumentNullException(nameof(id));
        BaseSalary = baseSalary > 0 ? baseSalary : throw new ArgumentException("Salary must be positive");
    }
    
    // Abstract method - all employees must implement
    public abstract decimal CalculateSalary();
    
    // Virtual method - can be overridden
    public virtual void PrintDetails()
    {
        Console.WriteLine($"Employee: {Name} (ID: {EmployeeId})");
        Console.WriteLine($"Salary: {CalculateSalary():C}");
    }
}

public class FullTimeEmployee : Employee
{
    public decimal Bonus { get; set; }
    
    public override decimal CalculateSalary()
    {
        return BaseSalary + Bonus;
    }
}

public class PartTimeEmployee : Employee
{
    public decimal HourlyRate { get; set; }
    public int HoursWorked { get; set; }
    
    public override decimal CalculateSalary()
    {
        return HourlyRate * HoursWorked;
    }
    
    public override void PrintDetails()
    {
        base.PrintDetails();
        Console.WriteLine($"Hours worked: {HoursWorked}");
    }
}

public class ContractorEmployee : Employee
{
    public decimal ProjectRate { get; set; }
    public int ProjectsCompleted { get; set; }
    
    public override decimal CalculateSalary()
    {
        return ProjectRate * ProjectsCompleted;
    }
}

// Usage:
var employees = new Employee[]
{
    new FullTimeEmployee("John", "FT001", 50000) { Bonus = 5000 },
    new PartTimeEmployee("Jane", "PT001", 0) { HourlyRate = 20, HoursWorked = 80 },
    new ContractorEmployee("Bob", "C001", 0) { ProjectRate = 1000, ProjectsCompleted = 5 }
};

foreach (var emp in employees)
{
    emp.PrintDetails();
    Console.WriteLine("---");
}
```

---

## Part 4: Polymorphism Exercises

### Exercise 4.1: Shape Area Calculation

**Goal:** Calculate area for different shapes using same interface

#### ✅ GOOD: Polymorphism with shapes
```csharp
public interface IShape
{
    double GetArea();
    double GetPerimeter();
    void PrintInfo();
}

public class Circle : IShape
{
    public double Radius { get; set; }
    
    public Circle(double radius)
    {
        if (radius <= 0)
            throw new ArgumentException("Radius must be positive");
        Radius = radius;
    }
    
    public double GetArea() => Math.PI * Radius * Radius;
    public double GetPerimeter() => 2 * Math.PI * Radius;
    public void PrintInfo()
    {
        Console.WriteLine($"Circle (r={Radius})");
        Console.WriteLine($"  Area: {GetArea():F2}");
        Console.WriteLine($"  Perimeter: {GetPerimeter():F2}");
    }
}

public class Rectangle : IShape
{
    public double Width { get; set; }
    public double Height { get; set; }
    
    public Rectangle(double width, double height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("Dimensions must be positive");
        Width = width;
        Height = height;
    }
    
    public double GetArea() => Width * Height;
    public double GetPerimeter() => 2 * (Width + Height);
    public void PrintInfo()
    {
        Console.WriteLine($"Rectangle ({Width}x{Height})");
        Console.WriteLine($"  Area: {GetArea():F2}");
        Console.WriteLine($"  Perimeter: {GetPerimeter():F2}");
    }
}

public class Triangle : IShape
{
    public double SideA { get; set; }
    public double SideB { get; set; }
    public double SideC { get; set; }
    
    public Triangle(double a, double b, double c)
    {
        if (!IsValidTriangle(a, b, c))
            throw new ArgumentException("Invalid triangle dimensions");
        SideA = a;
        SideB = b;
        SideC = c;
    }
    
    private bool IsValidTriangle(double a, double b, double c)
    {
        return a + b > c && b + c > a && c + a > b;
    }
    
    public double GetArea()
    {
        double s = (SideA + SideB + SideC) / 2;  // Semi-perimeter
        return Math.Sqrt(s * (s - SideA) * (s - SideB) * (s - SideC));  // Heron's formula
    }
    
    public double GetPerimeter() => SideA + SideB + SideC;
    public void PrintInfo()
    {
        Console.WriteLine($"Triangle ({SideA}, {SideB}, {SideC})");
        Console.WriteLine($"  Area: {GetArea():F2}");
        Console.WriteLine($"  Perimeter: {GetPerimeter():F2}");
    }
}

// Polymorphic usage - same code, different shapes!
public class GeometryCalculator
{
    public void AnalyzeShapes(IShape[] shapes)
    {
        double totalArea = 0;
        
        foreach (var shape in shapes)
        {
            shape.PrintInfo();
            totalArea += shape.GetArea();
            Console.WriteLine();
        }
        
        Console.WriteLine($"Total area of all shapes: {totalArea:F2}");
    }
}

// Usage:
var shapes = new IShape[]
{
    new Circle(5),
    new Rectangle(4, 6),
    new Triangle(3, 4, 5)
};

var calculator = new GeometryCalculator();
calculator.AnalyzeShapes(shapes);
```

#### 📝 Explanation
- `IShape` interface defines contract (GetArea, GetPerimeter, PrintInfo)
- Each shape implements the interface differently
- `AnalyzeShapes` works with ANY shape - doesn't know specific types
- Polymorphism: same method call, different behavior per shape

---

### Exercise 4.2: Database Repository Pattern

**Goal:** Same code works with different databases

#### ✅ GOOD: Repository polymorphism
```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<List<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// Implementation 1: SQL Server
public class SqlServerUserRepository : IUserRepository
{
    private readonly string _connectionString;
    
    public SqlServerUserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task<User?> GetByIdAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT * FROM Users WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            
            var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = (int)reader["Id"],
                    Name = (string)reader["Name"],
                    Email = (string)reader["Email"]
                };
            }
            return null;
        }
    }
    
    public async Task<List<User>> GetAllAsync()
    {
        var users = new List<User>();
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT * FROM Users", connection);
            var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = (int)reader["Id"],
                    Name = (string)reader["Name"],
                    Email = (string)reader["Email"]
                });
            }
        }
        return users;
    }
    
    public async Task AddAsync(User user)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = new SqlCommand(
                "INSERT INTO Users (Name, Email) VALUES (@name, @email)",
                connection);
            command.Parameters.AddWithValue("@name", user.Name);
            command.Parameters.AddWithValue("@email", user.Email);
            
            await command.ExecuteNonQueryAsync();
        }
    }
    
    public async Task UpdateAsync(User user)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = new SqlCommand(
                "UPDATE Users SET Name = @name, Email = @email WHERE Id = @id",
                connection);
            command.Parameters.AddWithValue("@name", user.Name);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@id", user.Id);
            
            await command.ExecuteNonQueryAsync();
        }
    }
    
    public async Task DeleteAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = new SqlCommand("DELETE FROM Users WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            
            await command.ExecuteNonQueryAsync();
        }
    }
}

// Implementation 2: In-Memory (testing)
public class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<int, User> _users = new();
    private int _nextId = 1;
    
    public Task<User?> GetByIdAsync(int id)
    {
        return Task.FromResult(_users.TryGetValue(id, out var user) ? user : null);
    }
    
    public Task<List<User>> GetAllAsync()
    {
        return Task.FromResult(_users.Values.ToList());
    }
    
    public Task AddAsync(User user)
    {
        user.Id = _nextId++;
        _users[user.Id] = user;
        return Task.CompletedTask;
    }
    
    public Task UpdateAsync(User user)
    {
        if (_users.ContainsKey(user.Id))
            _users[user.Id] = user;
        return Task.CompletedTask;
    }
    
    public Task DeleteAsync(int id)
    {
        _users.Remove(id);
        return Task.CompletedTask;
    }
}

// Clean usage - works with ANY repository!
public class UserService
{
    private readonly IUserRepository _repository;
    
    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<User> RegisterUserAsync(string name, string email)
    {
        var user = new User { Name = name, Email = email };
        await _repository.AddAsync(user);
        return user;
    }
    
    public async Task<User?> GetUserAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}

// Usage:
// Production: SQL Server
IUserRepository repo = new SqlServerUserRepository("Server=...;Database=...;");
var service = new UserService(repo);

// Testing: In-Memory
repo = new InMemoryUserRepository();
service = new UserService(repo);  // Same service, different storage!
```

---

## Part 5: SOLID Principles Exercises

### Exercise 5.1: Single Responsibility Principle (SRP)

**Goal:** Each class has ONE reason to change

#### ❌ BAD: Multiple responsibilities
```csharp
public class UserManager
{
    // RESPONSIBILITY 1: User operations
    public void CreateUser(string name, string email) { }
    public void DeleteUser(int id) { }
    
    // RESPONSIBILITY 2: Email operations
    public void SendWelcomeEmail(string email) { }
    
    // RESPONSIBILITY 3: Logging
    public void LogUserAction(string action) { }
    
    // RESPONSIBILITY 4: Database operations
    public void SaveToDatabase(User user) { }
}

// Too many reasons to change!
// - If email format changes
// - If logging framework changes
// - If database changes
// - If user validation changes
```

#### ✅ GOOD: Each class has ONE responsibility
```csharp
// Responsibility: User operations
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;
    
    public UserService(IUserRepository repository, IEmailService emailService, ILogger logger)
    {
        _repository = repository;
        _emailService = emailService;
        _logger = logger;
    }
    
    public async Task<User> CreateUserAsync(string name, string email)
    {
        var user = new User { Name = name, Email = email };
        await _repository.AddAsync(user);
        _logger.Log($"User created: {email}");
        await _emailService.SendWelcomeAsync(email);
        return user;
    }
}

// Responsibility: Sending emails
public interface IEmailService
{
    Task SendWelcomeAsync(string email);
    Task SendPasswordResetAsync(string email, string resetLink);
}

public class EmailService : IEmailService
{
    private readonly SmtpClient _smtpClient;
    
    public async Task SendWelcomeAsync(string email)
    {
        var message = new MailMessage("noreply@example.com", email)
        {
            Subject = "Welcome!",
            Body = "Thank you for joining!"
        };
        await _smtpClient.SendMailAsync(message);
    }
    
    public async Task SendPasswordResetAsync(string email, string resetLink)
    {
        var message = new MailMessage("noreply@example.com", email)
        {
            Subject = "Reset Password",
            Body = $"Click here: {resetLink}"
        };
        await _smtpClient.SendMailAsync(message);
    }
}

// Responsibility: Logging
public interface ILogger
{
    void Log(string message);
    void LogError(string message, Exception ex);
}

public class ConsoleLogger : ILogger
{
    public void Log(string message) => Console.WriteLine($"[LOG] {message}");
    public void LogError(string message, Exception ex) => Console.WriteLine($"[ERROR] {message}: {ex.Message}");
}

// Responsibility: Data persistence
public interface IUserRepository
{
    Task<User> GetByIdAsync(int id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}
```

#### 📝 Explanation
- UserService: Only manages users
- EmailService: Only sends emails
- ConsoleLogger: Only handles logging
- Each class changes for ONE reason

---

### Exercise 5.2: Open/Closed Principle (OCP)

**Goal:** Open for extension, closed for modification

#### ❌ BAD: Must modify existing code to add features
```csharp
public class ReportGenerator
{
    public void GenerateReport(string type)
    {
        if (type == "pdf")
        {
            // PDF-specific code
        }
        else if (type == "excel")
        {
            // Excel-specific code
        }
        else if (type == "word")  // ❌ Need to modify this!
        {
            // Word-specific code
        }
    }
}

// Every new format requires modifying ReportGenerator!
```

#### ✅ GOOD: Extend without modifying
```csharp
public interface IReportFormatter
{
    void Format(Report report);
    void Save(string filename);
}

public class PdfReportFormatter : IReportFormatter
{
    public void Format(Report report)
    {
        // PDF formatting logic
    }
    
    public void Save(string filename)
    {
        // Save as PDF
    }
}

public class ExcelReportFormatter : IReportFormatter
{
    public void Format(Report report)
    {
        // Excel formatting logic
    }
    
    public void Save(string filename)
    {
        // Save as Excel
    }
}

public class WordReportFormatter : IReportFormatter
{
    public void Format(Report report)
    {
        // Word formatting logic
    }
    
    public void Save(string filename)
    {
        // Save as Word
    }
}

// Clean generator - no modifications needed!
public class ReportGenerator
{
    private readonly IReportFormatter _formatter;
    
    public ReportGenerator(IReportFormatter formatter)
    {
        _formatter = formatter;
    }
    
    public void GenerateReport(Report report, string filename)
    {
        _formatter.Format(report);
        _formatter.Save(filename);
    }
}

// Add PowerPoint support WITHOUT modifying ReportGenerator!
public class PowerPointReportFormatter : IReportFormatter
{
    public void Format(Report report) { /* ... */ }
    public void Save(string filename) { /* ... */ }
}
```

#### 💡 Variations
1. Add JsonReportFormatter
2. Add CsvReportFormatter
3. Create a ReportFormatterFactory to choose formatter

---

### Exercise 5.3: Dependency Inversion Principle (DIP)

**Goal:** Depend on abstractions, not concrete classes

#### ❌ BAD: Tightly coupled to concrete classes
```csharp
public class UserRegistrationService
{
    private readonly SqlUserRepository _repository = new();  // ❌ Concrete dependency!
    private readonly SmtpEmailService _emailService = new();  // ❌ Concrete dependency!
    private readonly ConsoleLogger _logger = new();  // ❌ Concrete dependency!
    
    public void RegisterUser(string name, string email)
    {
        _logger.Log("Registering user...");
        var user = new User { Name = name, Email = email };
        _repository.Add(user);
        _emailService.SendWelcome(email);
    }
}

// Hard to test - can't swap implementations!
```

#### ✅ GOOD: Depend on abstractions
```csharp
public interface IUserRepository
{
    void Add(User user);
    User GetById(int id);
}

public interface IEmailService
{
    void SendWelcome(string email);
}

public interface ILogger
{
    void Log(string message);
}

public class UserRegistrationService
{
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;
    
    // Dependencies injected - can swap any implementation!
    public UserRegistrationService(
        IUserRepository repository,
        IEmailService emailService,
        ILogger logger)
    {
        _repository = repository;
        _emailService = emailService;
        _logger = logger;
    }
    
    public void RegisterUser(string name, string email)
    {
        _logger.Log("Registering user...");
        var user = new User { Name = name, Email = email };
        _repository.Add(user);
        _emailService.SendWelcome(email);
    }
}

// Easy to test - inject test doubles!
public class UserRegistrationServiceTests
{
    [Fact]
    public void RegisterUser_SendsWelcomeEmail()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger>();
        
        var service = new UserRegistrationService(mockRepository.Object, mockEmailService.Object, mockLogger.Object);
        
        // Act
        service.RegisterUser("John", "john@example.com");
        
        // Assert
        mockEmailService.Verify(x => x.SendWelcome("john@example.com"), Times.Once);
    }
}
```

---

## Part 6: Combined Exercises

### Exercise 6.1: E-Commerce Order System

**Goal:** Apply all OOP and SOLID principles together

#### ✅ GOOD: Complete implementation
```csharp
// Models
public class Order
{
    public int Id { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
}

public class OrderItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public enum OrderStatus { Pending, Confirmed, Shipped, Delivered }

// Abstractions (SRP + DIP)
public interface IOrderRepository
{
    Task<Order> GetByIdAsync(int id);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
}

public interface IPaymentProcessor
{
    Task<bool> ProcessPaymentAsync(decimal amount, string paymentMethod);
}

public interface INotificationService
{
    Task SendOrderConfirmationAsync(Order order, string email);
    Task SendShippingNotificationAsync(Order order, string email);
}

public interface IInventoryService
{
    Task<bool> ReserveItemsAsync(List<OrderItem> items);
    Task ReleaseItemsAsync(List<OrderItem> items);
}

// Implementation
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly INotificationService _notificationService;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger _logger;
    
    public OrderService(
        IOrderRepository repository,
        IPaymentProcessor paymentProcessor,
        INotificationService notificationService,
        IInventoryService inventoryService,
        ILogger logger)
    {
        _repository = repository;
        _paymentProcessor = paymentProcessor;
        _notificationService = notificationService;
        _inventoryService = inventoryService;
        _logger = logger;
    }
    
    public async Task<Order> CreateOrderAsync(Order order, string paymentMethod, string customerEmail)
    {
        try
        {
            _logger.Log($"Creating order with {order.Items.Count} items");
            
            // Reserve inventory
            bool reserved = await _inventoryService.ReserveItemsAsync(order.Items);
            if (!reserved)
            {
                throw new InvalidOperationException("Items out of stock");
            }
            
            // Process payment
            bool paid = await _paymentProcessor.ProcessPaymentAsync(order.Total, paymentMethod);
            if (!paid)
            {
                await _inventoryService.ReleaseItemsAsync(order.Items);
                throw new InvalidOperationException("Payment failed");
            }
            
            // Save order
            order.Status = OrderStatus.Confirmed;
            await _repository.AddAsync(order);
            
            // Send confirmation
            await _notificationService.SendOrderConfirmationAsync(order, customerEmail);
            
            _logger.Log($"Order created: {order.Id}");
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create order", ex);
            throw;
        }
    }
    
    public async Task ShipOrderAsync(int orderId, string customerEmail)
    {
        var order = await _repository.GetByIdAsync(orderId);
        order.Status = OrderStatus.Shipped;
        await _repository.UpdateAsync(order);
        await _notificationService.SendShippingNotificationAsync(order, customerEmail);
    }
}

// Multiple payment implementations (OCP + Polymorphism)
public class CreditCardPaymentProcessor : IPaymentProcessor
{
    public async Task<bool> ProcessPaymentAsync(decimal amount, string cardNumber)
    {
        // Stripe integration
        return true;
    }
}

public class PayPalPaymentProcessor : IPaymentProcessor
{
    public async Task<bool> ProcessPaymentAsync(decimal amount, string paypalEmail)
    {
        // PayPal integration
        return true;
    }
}

// DI Configuration
services.AddScoped<IOrderRepository, SqlOrderRepository>();
services.AddScoped<IPaymentProcessor, CreditCardPaymentProcessor>();
services.AddScoped<INotificationService, EmailNotificationService>();
services.AddScoped<IInventoryService, InventoryService>();
services.AddScoped<ILogger, ConsoleLogger>();
services.AddScoped<OrderService>();
```

---

## Summary Table

| Principle | What | How | Example |
|-----------|------|-----|---------|
| **Encapsulation** | Hide internal state | Private fields, public properties | BankAccount with balance validation |
| **Abstraction** | Hide complexity | Interfaces, abstract classes | PaymentGateway interface hides Stripe/PayPal |
| **Inheritance** | Reuse code | Base class shared by children | Vehicle parent, Car/Truck children |
| **Polymorphism** | One interface, many implementations | Interface + multiple classes | IShape, Circle, Rectangle, Triangle |
| **SRP** | One responsibility per class | Split into focused classes | UserService, EmailService, Logger separate |
| **OCP** | Extend without modifying | Use interfaces + new implementations | ReportFormatter for PDF/Excel/Word |
| **LSP** | Subtypes substitute base types | Implement contract correctly | All IShape implementations work the same |
| **ISP** | Specific interfaces over fat ones | Split large interfaces | IWorker, IHuman, IProgrammer separate |
| **DIP** | Depend on abstractions | Inject interfaces | OrderService depends on IPaymentProcessor |

---

## How to Practice

1. **Start with Exercise 1.1** (Bank Account) — Simple encapsulation
2. **Move to Exercise 2.1** (Payment Processing) — Learn abstraction
3. **Try Exercise 3.1** (Vehicle Hierarchy) — Understand inheritance
4. **Practice Exercise 4.1** (Shapes) — Master polymorphism
5. **Deep dive Exercise 5.1-5.3** — SOLID principles one at a time
6. **Combine with Exercise 6.1** — Build complete system

Each exercise can be run in a Console application. Copy the code, run it, modify it, understand it.

---

**Created:** January 21, 2025
**Target Audience:** Developers learning OOP and SOLID principles
**Total Examples:** 20+ complete, runnable exercises
