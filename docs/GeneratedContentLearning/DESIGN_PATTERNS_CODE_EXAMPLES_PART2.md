# Design Patterns - Complete Code Examples (Part 2/3)

Runnable C# code examples for Behavioral patterns.

---

## BEHAVIORAL PATTERNS (10)

### 11. Observer Pattern

**Purpose:** Objects notify multiple listeners of state changes

```csharp
public interface IObserver
{
    void Update(string message);
}

public class EventPublisher
{
    private readonly List<IObserver> _observers = new();
    
    public void Attach(IObserver observer) => _observers.Add(observer);
    public void Detach(IObserver observer) => _observers.Remove(observer);
    
    public void NotifyObservers(string message)
    {
        foreach (var observer in _observers)
            observer.Update(message);
    }
}

public class EmailSubscriber : IObserver
{
    private readonly string _email;
    public EmailSubscriber(string email) => _email = email;
    
    public void Update(string message) => 
        Console.WriteLine($"Email sent to {_email}: {message}");
}

public class SlackSubscriber : IObserver
{
    private readonly string _channel;
    public SlackSubscriber(string channel) => _channel = channel;
    
    public void Update(string message) => 
        Console.WriteLine($"Slack message in {_channel}: {message}");
}

// Usage
var publisher = new EventPublisher();
publisher.Attach(new EmailSubscriber("user@example.com"));
publisher.Attach(new SlackSubscriber("#notifications"));

publisher.NotifyObservers("New order received!");
// Email sent to user@example.com: New order received!
// Slack message in #notifications: New order received!
```

**Real-World: Stock Price Alerts**

```csharp
public class Stock
{
    private decimal _price;
    private readonly List<IPriceObserver> _observers = new();
    
    public string Symbol { get; }
    public decimal Price
    {
        get => _price;
        set
        {
            _price = value;
            NotifyObservers();  // Notify when price changes
        }
    }
    
    public Stock(string symbol, decimal initialPrice)
    {
        Symbol = symbol;
        _price = initialPrice;
    }
    
    public void Subscribe(IPriceObserver observer) => _observers.Add(observer);
    public void Unsubscribe(IPriceObserver observer) => _observers.Remove(observer);
    
    private void NotifyObservers()
    {
        foreach (var observer in _observers)
            observer.OnPriceChanged(Symbol, _price);
    }
}

public interface IPriceObserver
{
    void OnPriceChanged(string symbol, decimal newPrice);
}

public class Trader : IPriceObserver
{
    private readonly string _name;
    public Trader(string name) => _name = name;
    
    public void OnPriceChanged(string symbol, decimal newPrice)
    {
        Console.WriteLine($"{_name}: {symbol} is now ${newPrice}");
    }
}

// Usage
var apple = new Stock("AAPL", 150m);
apple.Subscribe(new Trader("John"));
apple.Subscribe(new Trader("Jane"));

apple.Price = 152m;  // All traders notified
// John: AAPL is now $152
// Jane: AAPL is now $152
```

---

### 12. Strategy Pattern

**Purpose:** Encapsulate interchangeable algorithms

```csharp
public interface IPaymentStrategy
{
    void Pay(decimal amount);
}

public class CreditCardStrategy : IPaymentStrategy
{
    private readonly string _cardNumber;
    public CreditCardStrategy(string cardNumber) => _cardNumber = cardNumber;
    
    public void Pay(decimal amount) => 
        Console.WriteLine($"Paid ${amount} using Credit Card {_cardNumber}");
}

public class PayPalStrategy : IPaymentStrategy
{
    private readonly string _email;
    public PayPalStrategy(string email) => _email = email;
    
    public void Pay(decimal amount) => 
        Console.WriteLine($"Paid ${amount} using PayPal {_email}");
}

public class BitcoinStrategy : IPaymentStrategy
{
    private readonly string _walletAddress;
    public BitcoinStrategy(string walletAddress) => _walletAddress = walletAddress;
    
    public void Pay(decimal amount) => 
        Console.WriteLine($"Paid ${amount} in Bitcoin to {_walletAddress}");
}

public class ShoppingCart
{
    private IPaymentStrategy _strategy;
    private decimal _total;
    
    public void SetPaymentStrategy(IPaymentStrategy strategy) => _strategy = strategy;
    public void AddItem(decimal price) => _total += price;
    
    public void Checkout()
    {
        if (_strategy == null)
            throw new InvalidOperationException("Payment strategy not set");
        
        _strategy.Pay(_total);
    }
}

// Usage: Choose strategy at runtime
var cart = new ShoppingCart();
cart.AddItem(29.99m);
cart.AddItem(49.99m);

cart.SetPaymentStrategy(new CreditCardStrategy("4111111111111111"));
cart.Checkout();  // Paid $79.98 using Credit Card

cart.SetPaymentStrategy(new BitcoinStrategy("1A1z7agoat8Bt8ShwQW9Thoga"));
cart.Checkout();  // Paid $79.98 in Bitcoin
```

**Real-World: Sorting Strategy**

```csharp
public interface ISortingStrategy
{
    List<int> Sort(List<int> items);
}

public class BubbleSortStrategy : ISortingStrategy
{
    public List<int> Sort(List<int> items)
    {
        var arr = new List<int>(items);
        for (int i = 0; i < arr.Count; i++)
            for (int j = 0; j < arr.Count - 1; j++)
                if (arr[j] > arr[j + 1])
                    (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
        return arr;
    }
}

public class QuickSortStrategy : ISortingStrategy
{
    public List<int> Sort(List<int> items) => items.OrderBy(x => x).ToList();
}

public class DataProcessor
{
    private ISortingStrategy _sortStrategy;
    
    public void SetSortStrategy(ISortingStrategy strategy) => _sortStrategy = strategy;
    
    public void ProcessData(List<int> data)
    {
        var sorted = _sortStrategy.Sort(data);
        Console.WriteLine($"Sorted: {string.Join(", ", sorted)}");
    }
}

// Usage
var processor = new DataProcessor();
processor.SetSortStrategy(new QuickSortStrategy());
processor.ProcessData(new List<int> { 5, 2, 8, 1, 9 });  // Sorted: 1, 2, 5, 8, 9
```

---

### 13. Command Pattern

**Purpose:** Encapsulate requests as objects

```csharp
public interface ICommand
{
    void Execute();
    void Undo();
}

public class Light
{
    private bool _isOn = false;
    
    public void TurnOn()
    {
        _isOn = true;
        Console.WriteLine("Light is ON");
    }
    
    public void TurnOff()
    {
        _isOn = false;
        Console.WriteLine("Light is OFF");
    }
}

public class LightOnCommand : ICommand
{
    private readonly Light _light;
    
    public LightOnCommand(Light light) => _light = light;
    
    public void Execute() => _light.TurnOn();
    public void Undo() => _light.TurnOff();
}

public class LightOffCommand : ICommand
{
    private readonly Light _light;
    
    public LightOffCommand(Light light) => _light = light;
    
    public void Execute() => _light.TurnOff();
    public void Undo() => _light.TurnOn();
}

public class RemoteControl
{
    private readonly Stack<ICommand> _history = new();
    
    public void PressButton(ICommand command)
    {
        command.Execute();
        _history.Push(command);
    }
    
    public void PressUndo()
    {
        if (_history.Pop() is ICommand command)
            command.Undo();
    }
}

// Usage
var light = new Light();
var remote = new RemoteControl();

remote.PressButton(new LightOnCommand(light));   // Light is ON
remote.PressButton(new LightOffCommand(light));  // Light is OFF
remote.PressUndo();                               // Light is ON
```

**Real-World: Transaction Queue**

```csharp
public interface ITransaction : ICommand { }

public class Database
{
    private decimal _balance = 1000m;
    
    public void Deposit(decimal amount)
    {
        _balance += amount;
        Console.WriteLine($"Deposited ${amount}. Balance: ${_balance}");
    }
    
    public void Withdraw(decimal amount)
    {
        _balance -= amount;
        Console.WriteLine($"Withdrew ${amount}. Balance: ${_balance}");
    }
}

public class DepositCommand : ITransaction
{
    private readonly Database _db;
    private readonly decimal _amount;
    
    public DepositCommand(Database db, decimal amount)
    {
        _db = db;
        _amount = amount;
    }
    
    public void Execute() => _db.Deposit(_amount);
    public void Undo() => _db.Withdraw(_amount);
}

public class TransactionQueue
{
    private readonly Queue<ICommand> _queue = new();
    
    public void Enqueue(ICommand command) => _queue.Enqueue(command);
    
    public void ExecuteAll()
    {
        while (_queue.Count > 0)
            _queue.Dequeue().Execute();
    }
}
```

---

### 14. Chain of Responsibility Pattern

**Purpose:** Pass request through chain of handlers

```csharp
public interface IHandler
{
    void SetNext(IHandler next);
    void Handle(Request request);
}

public class Request
{
    public int Level { get; set; }
    public string Message { get; set; }
}

public abstract class AbstractHandler : IHandler
{
    protected IHandler _next;
    
    public virtual void SetNext(IHandler next) => _next = next;
    
    public virtual void Handle(Request request)
    {
        if (CanHandle(request))
            Process(request);
        else if (_next != null)
            _next.Handle(request);
        else
            Console.WriteLine("Request could not be handled");
    }
    
    protected abstract bool CanHandle(Request request);
    protected abstract void Process(Request request);
}

public class Level1Handler : AbstractHandler
{
    protected override bool CanHandle(Request request) => request.Level <= 1;
    protected override void Process(Request request) => 
        Console.WriteLine($"Level 1 handled: {request.Message}");
}

public class Level2Handler : AbstractHandler
{
    protected override bool CanHandle(Request request) => request.Level <= 2;
    protected override void Process(Request request) => 
        Console.WriteLine($"Level 2 handled: {request.Message}");
}

public class Level3Handler : AbstractHandler
{
    protected override bool CanHandle(Request request) => request.Level <= 3;
    protected override void Process(Request request) => 
        Console.WriteLine($"Level 3 handled: {request.Message}");
}

// Usage
var level1 = new Level1Handler();
var level2 = new Level2Handler();
var level3 = new Level3Handler();

level1.SetNext(level2);
level2.SetNext(level3);

level1.Handle(new Request { Level = 1, Message = "Simple" });    // Level 1 handled
level1.Handle(new Request { Level = 2, Message = "Complex" });   // Level 2 handled
level1.Handle(new Request { Level = 3, Message = "Very Complex" }); // Level 3 handled
```

**Real-World: Approval Workflow**

```csharp
public abstract class Approver
{
    protected Approver _nextApprover;
    protected decimal _approvalLimit;
    
    public void SetNext(Approver next) => _nextApprover = next;
    
    public virtual void Approve(ExpenseRequest request)
    {
        if (request.Amount <= _approvalLimit)
            Console.WriteLine($"{GetType().Name} approved ${request.Amount}");
        else if (_nextApprover != null)
            _nextApprover.Approve(request);
        else
            Console.WriteLine($"Request for ${request.Amount} rejected - exceeds all limits");
    }
}

public class Manager : Approver
{
    public Manager() => _approvalLimit = 1000m;
}

public class Director : Approver
{
    public Director() => _approvalLimit = 10000m;
}

public class CEO : Approver
{
    public CEO() => _approvalLimit = decimal.MaxValue;
}

public class ExpenseRequest
{
    public decimal Amount { get; set; }
}

// Usage
var manager = new Manager();
var director = new Director();
var ceo = new CEO();

manager.SetNext(director);
director.SetNext(ceo);

manager.Approve(new ExpenseRequest { Amount = 500m });    // Manager approved
manager.Approve(new ExpenseRequest { Amount = 5000m });   // Director approved
manager.Approve(new ExpenseRequest { Amount = 50000m });  // CEO approved
```

---

### 15. State Pattern

**Purpose:** Allow object behavior to change based on state

```csharp
public interface IState
{
    void Handle(Context context);
}

public class Context
{
    private IState _state;
    
    public Context(IState initialState) => _state = initialState;
    
    public void SetState(IState state) => _state = state;
    public void Request() => _state.Handle(this);
}

public class ConcreteStateA : IState
{
    public void Handle(Context context)
    {
        Console.WriteLine("Handling in State A, transitioning to State B");
        context.SetState(new ConcreteStateB());
    }
}

public class ConcreteStateB : IState
{
    public void Handle(Context context)
    {
        Console.WriteLine("Handling in State B, transitioning to State A");
        context.SetState(new ConcreteStateA());
    }
}

// Usage
var context = new Context(new ConcreteStateA());
context.Request();  // Handling in State A, transitioning to State B
context.Request();  // Handling in State B, transitioning to State A
```

**Real-World: Order Status**

```csharp
public interface IOrderState
{
    void Process(Order order);
}

public class PendingState : IOrderState
{
    public void Process(Order order)
    {
        Console.WriteLine("Order is pending, confirming...");
        order.SetState(new ConfirmedState());
    }
}

public class ConfirmedState : IOrderState
{
    public void Process(Order order)
    {
        Console.WriteLine("Order confirmed, shipping...");
        order.SetState(new ShippedState());
    }
}

public class ShippedState : IOrderState
{
    public void Process(Order order)
    {
        Console.WriteLine("Order shipped, delivering...");
        order.SetState(new DeliveredState());
    }
}

public class DeliveredState : IOrderState
{
    public void Process(Order order)
    {
        Console.WriteLine("Order delivered!");
    }
}

public class Order
{
    private IOrderState _state;
    
    public Order() => _state = new PendingState();
    
    public void SetState(IOrderState state) => _state = state;
    public void Process() => _state.Process(this);
}

// Usage
var order = new Order();
order.Process();  // Order is pending, confirming...
order.Process();  // Order confirmed, shipping...
order.Process();  // Order shipped, delivering...
order.Process();  // Order delivered!
```

---

### 16. Template Method Pattern

**Purpose:** Define algorithm skeleton, let subclasses override steps

```csharp
public abstract class DataProcessor
{
    // Template method - final algorithm skeleton
    public final void Process()
    {
        OpenFile();
        ExtractData();
        TransformData();
        ValidateData();
        SaveData();
    }
    
    protected abstract void OpenFile();
    protected abstract void ExtractData();
    protected abstract void TransformData();
    protected abstract void ValidateData();
    protected abstract void SaveData();
}

// Note: C# uses 'sealed' or 'virtual sealed' instead of 'final'
public abstract class DataProcessorCS
{
    public void Process()  // sealed behavior
    {
        OpenFile();
        ExtractData();
        TransformData();
        ValidateData();
        SaveData();
    }
    
    protected abstract void OpenFile();
    protected abstract void ExtractData();
    protected abstract void TransformData();
    protected abstract void ValidateData();
    protected abstract void SaveData();
}

public class CsvDataProcessor : DataProcessorCS
{
    protected override void OpenFile() => 
        Console.WriteLine("Opening CSV file");
    
    protected override void ExtractData() => 
        Console.WriteLine("Extracting data from CSV");
    
    protected override void TransformData() => 
        Console.WriteLine("Transforming CSV data");
    
    protected override void ValidateData() => 
        Console.WriteLine("Validating CSV data");
    
    protected override void SaveData() => 
        Console.WriteLine("Saving to database");
}

public class JsonDataProcessor : DataProcessorCS
{
    protected override void OpenFile() => 
        Console.WriteLine("Opening JSON file");
    
    protected override void ExtractData() => 
        Console.WriteLine("Extracting data from JSON");
    
    protected override void TransformData() => 
        Console.WriteLine("Transforming JSON data");
    
    protected override void ValidateData() => 
        Console.WriteLine("Validating JSON data");
    
    protected override void SaveData() => 
        Console.WriteLine("Saving to database");
}

// Usage
DataProcessorCS csv = new CsvDataProcessor();
csv.Process();  // All steps executed in order

DataProcessorCS json = new JsonDataProcessor();
json.Process();  // Same steps, different implementations
```

---

### 17. Memento Pattern

**Purpose:** Capture and restore object state without violating encapsulation

```csharp
public class Memento
{
    public string State { get; }
    
    public Memento(string state) => State = state;
}

public class Editor
{
    private string _content = "";
    
    public void Type(string text)
    {
        _content += text;
        Console.WriteLine($"Content: {_content}");
    }
    
    public Memento SaveState() => new Memento(_content);
    
    public void RestoreState(Memento memento)
    {
        _content = memento.State;
        Console.WriteLine($"Restored to: {_content}");
    }
}

public class EditorHistory
{
    private readonly Stack<Memento> _history = new();
    
    public void Save(Editor editor) => _history.Push(editor.SaveState());
    
    public void Undo(Editor editor)
    {
        if (_history.Pop() is Memento memento)
            editor.RestoreState(memento);
    }
}

// Usage
var editor = new Editor();
var history = new EditorHistory();

editor.Type("Hello ");      // Content: Hello
history.Save(editor);
editor.Type("World");       // Content: Hello World
history.Save(editor);
editor.Type("!");           // Content: Hello World!

history.Undo(editor);       // Restored to: Hello World
history.Undo(editor);       // Restored to: Hello
```

---

### 18. Iterator Pattern

**Purpose:** Access elements of collection sequentially without exposing structure

```csharp
public interface IIterator<T>
{
    T Next();
    bool HasNext();
}

public interface IIterable<T>
{
    IIterator<T> CreateIterator();
}

public class ArrayIterator<T> : IIterator<T>
{
    private readonly T[] _array;
    private int _index = 0;
    
    public ArrayIterator(T[] array) => _array = array;
    
    public T Next() => _array[_index++];
    public bool HasNext() => _index < _array.Length;
}

public class Collection<T> : IIterable<T>
{
    private readonly T[] _items;
    
    public Collection(T[] items) => _items = items;
    
    public IIterator<T> CreateIterator() => new ArrayIterator<T>(_items);
}

// Usage
var collection = new Collection<string>(new[] { "Apple", "Banana", "Orange" });
var iterator = collection.CreateIterator();

while (iterator.HasNext())
    Console.WriteLine(iterator.Next());  // Apple, Banana, Orange
```

---

### 19. Visitor Pattern

**Purpose:** Represent operations on elements of object structure

```csharp
public interface IElement
{
    void Accept(IVisitor visitor);
}

public interface IVisitor
{
    void Visit(Circle circle);
    void Visit(Rectangle rectangle);
}

public class Circle : IElement
{
    public double Radius { get; set; }
    
    public void Accept(IVisitor visitor) => visitor.Visit(this);
}

public class Rectangle : IElement
{
    public double Width { get; set; }
    public double Height { get; set; }
    
    public void Accept(IVisitor visitor) => visitor.Visit(this);
}

public class AreaCalculator : IVisitor
{
    public void Visit(Circle circle)
    {
        var area = Math.PI * circle.Radius * circle.Radius;
        Console.WriteLine($"Circle area: {area:F2}");
    }
    
    public void Visit(Rectangle rectangle)
    {
        var area = rectangle.Width * rectangle.Height;
        Console.WriteLine($"Rectangle area: {area:F2}");
    }
}

public class PerimeterCalculator : IVisitor
{
    public void Visit(Circle circle)
    {
        var perimeter = 2 * Math.PI * circle.Radius;
        Console.WriteLine($"Circle perimeter: {perimeter:F2}");
    }
    
    public void Visit(Rectangle rectangle)
    {
        var perimeter = 2 * (rectangle.Width + rectangle.Height);
        Console.WriteLine($"Rectangle perimeter: {perimeter:F2}");
    }
}

// Usage
var shapes = new IElement[]
{
    new Circle { Radius = 5 },
    new Rectangle { Width = 4, Height = 6 }
};

var areaCalc = new AreaCalculator();
var perimeterCalc = new PerimeterCalculator();

foreach (var shape in shapes)
{
    shape.Accept(areaCalc);
    shape.Accept(perimeterCalc);
}
```

---

### 20. Interpreter Pattern

**Purpose:** Define grammar and interpreter for a language

```csharp
public interface IExpression
{
    int Interpret();
}

public class Number : IExpression
{
    private readonly int _value;
    
    public Number(int value) => _value = value;
    
    public int Interpret() => _value;
}

public class Add : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;
    
    public Add(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }
    
    public int Interpret() => _left.Interpret() + _right.Interpret();
}

public class Subtract : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;
    
    public Subtract(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }
    
    public int Interpret() => _left.Interpret() - _right.Interpret();
}

// Usage: (5 + 3) - 2 = 6
IExpression expression = new Subtract(
    new Add(new Number(5), new Number(3)),
    new Number(2)
);

Console.WriteLine(expression.Interpret());  // 6
```

---

## Summary of Part 2

**Created:** 10 behavioral design patterns with complete runnable code
- 11. Observer Pattern
- 12. Strategy Pattern
- 13. Command Pattern
- 14. Chain of Responsibility Pattern
- 15. State Pattern
- 16. Template Method Pattern
- 17. Memento Pattern
- 18. Iterator Pattern
- 19. Visitor Pattern
- 20. Interpreter Pattern

Each includes:
- Complete class implementations
- Usage examples with output
- Real-world applications

Continue to Part 3 for Architectural & Advanced patterns (21-30)
