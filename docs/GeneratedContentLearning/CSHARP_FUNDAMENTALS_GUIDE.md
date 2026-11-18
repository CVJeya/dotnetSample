# C# Fundamentals: Variables, Data Types, Collections, and Statements

A complete guide to C# basics with practical, runnable examples for beginners.

---

## Table of Contents
1. Variables
2. Data Types
3. Collections
4. Statements
5. Complete Examples

---

## 1. VARIABLES 📦

### What is a Variable?

A variable is a named container that stores a value in memory.

**Syntax:**
```csharp
dataType variableName = value;
```

### Variable Declaration

```csharp
// ✅ GOOD: Explicitly declare type
int age = 25;
string name = "John";
bool isActive = true;

// ✅ GOOD: Use 'var' - compiler infers type
var city = "New York";  // Compiler knows this is string
var score = 95;         // Compiler knows this is int

// ❌ BAD: Uninitialized variable (not allowed in C#)
// int age;  // Error: must be initialized
// age = 25; // Now it's initialized
```

### Variable Naming Rules

```csharp
// ✅ GOOD: Clear, descriptive names
int studentAge = 20;
string firstName = "Alice";
bool isVerified = false;

// ❌ BAD: Unclear, single-letter (except loop counters)
int a = 20;           // What is 'a'?
string x = "Alice";   // Meaningless name

// ❌ BAD: Reserved keywords
// int class = 5;      // 'class' is reserved
// int int = 10;       // 'int' is reserved

// ✅ GOOD: PascalCase for constants
const double PI = 3.14159;
const string COMPANY = "Acme Corp";
```

### Variable Scope

```csharp
public class ScopeExample
{
    // Class-level variable (accessible anywhere in class)
    private int classVariable = 100;
    
    public void MethodExample()
    {
        // Method-level variable (accessible only in this method)
        int methodVariable = 50;
        
        if (methodVariable > 0)
        {
            // Block-level variable (accessible only in this block)
            int blockVariable = 25;
            Console.WriteLine(blockVariable);  // ✅ OK
        }
        // Console.WriteLine(blockVariable);  // ❌ Error: out of scope
        
        Console.WriteLine(methodVariable);   // ✅ OK
    }
}
```

### const vs readonly

```csharp
public class ConstVsReadonly
{
    // const: Compile-time constant
    // - Must be initialized when declared
    // - Value cannot change
    // - Value known at compile time
    public const double PI = 3.14159;
    
    // readonly: Runtime constant
    // - Can be initialized in constructor
    // - Value cannot change after initialization
    // - Value known at runtime
    private readonly string _databaseConnectionString;
    
    public ConstVsReadonly(string connectionString)
    {
        _databaseConnectionString = connectionString;  // ✅ Can initialize here
    }
    
    public void Example()
    {
        // PI = 3.14;  // ❌ Error: cannot assign to const
        // _databaseConnectionString = "new";  // ❌ Error: cannot reassign readonly
    }
}
```

---

## 2. DATA TYPES 🔢

### Value Types vs Reference Types

```csharp
// VALUE TYPES: Stored on stack, value is stored directly
int age = 25;           // int: stored as 25
double price = 19.99;   // double: stored as 19.99
bool isActive = true;   // bool: stored as true

// REFERENCE TYPES: Stored on heap, variable holds reference
string name = "John";   // string: variable holds address to string
int[] numbers = { 1, 2, 3 };  // array: variable holds address

var copy1 = age;     // Copies VALUE (25)
var copy2 = name;    // Copies REFERENCE (address)
```

### Built-in Data Types

```csharp
// NUMERIC TYPES
byte smallNum = 255;              // 0-255
short mediumNum = 32767;          // -32768 to 32767
int age = 25;                     // -2,147,483,648 to 2,147,483,647
long largeNum = 9223372036854775807;  // Very large numbers

float price = 19.99f;             // 32-bit decimal (add 'f' suffix)
double pi = 3.14159265;           // 64-bit decimal (more precision)
decimal money = 199.99m;          // 128-bit decimal (financial, most precise)

// LOGICAL
bool isActive = true;
bool isExpired = false;

// CHARACTER
char grade = 'A';                 // Single character
char symbol = '@';

// STRING
string name = "John Doe";
string empty = "";
string multiLine = @"Line 1
Line 2";  // Verbatim string

// DATE & TIME
DateTime today = DateTime.Now;
DateTime birthDate = new DateTime(1990, 5, 15);
TimeSpan duration = TimeSpan.FromHours(2);

// NULLABLE TYPES: Can hold null value
int? optionalAge = null;          // Can be int or null
string? optionalName = null;      // Can be string or null

if (optionalAge.HasValue)
    Console.WriteLine(optionalAge.Value);
```

### Type Conversion

```csharp
// IMPLICIT CONVERSION: No data loss, safe
int num = 100;
double price = num;  // ✅ int to double (safe, no loss)

// EXPLICIT CONVERSION: Potential data loss, must cast
double value = 19.99;
int rounded = (int)value;  // ✅ 19.99 becomes 19 (data loss)

// CONVERT CLASS: Safer with checks
string ageStr = "25";
int age = int.Parse(ageStr);           // ✅ Throws if invalid
if (int.TryParse(ageStr, out var age2)) // ✅ Returns false if invalid
    Console.WriteLine(age2);

// STRING CONVERSION
int num2 = 42;
string text = num2.ToString();  // "42"
string formatted = num2.ToString("D4");  // "0042" (4 digits, padded)
```

### Nullable Reference Types (C# 8+)

```csharp
public class NullableExample
{
    // Non-nullable string - cannot be null
    string name = "John";  // Required to initialize
    
    // Nullable string - can be null
    string? description = null;  // OK to be null
    
    public void PrintName()
    {
        Console.WriteLine(name);  // ✅ Safe - never null
        
        if (description != null)
            Console.WriteLine(description);  // ✅ Safe - checked for null
        
        // Console.WriteLine(description.Length);  // ❌ Error: might be null
    }
}
```

---

## 3. COLLECTIONS 📚

### Arrays

```csharp
// ARRAY: Fixed size, ordered collection

// Declare and initialize with size
int[] numbers = new int[5];  // Array of 5 integers, all default (0)

// Declare and initialize with values
int[] scores = new int[] { 85, 90, 78, 92 };
int[] moreScores = { 85, 90, 78 };  // Shorthand

// Access elements
Console.WriteLine(scores[0]);  // 85 (first element)
Console.WriteLine(scores[2]);  // 78 (third element)

// Modify elements
scores[0] = 95;  // Change first element to 95

// Array length
Console.WriteLine(scores.Length);  // 4

// Multi-dimensional arrays
int[,] matrix = new int[3, 3];  // 3x3 grid
matrix[0, 0] = 1;
matrix[1, 2] = 5;

// Jagged array (array of arrays)
int[][] jagged = new int[3][];  // 3 rows, each can have different size
jagged[0] = new int[4];
jagged[1] = new int[2];

// Array operations
Array.Sort(scores);              // Sort in place
int maxScore = scores.Max();     // Get max value
int minScore = scores.Min();     // Get min value
int sum = scores.Sum();          // Sum all elements
```

### List<T> - Dynamic Array

```csharp
// LIST: Dynamic size, ordered collection

var numbers = new List<int>();  // Empty list
numbers.Add(10);     // Add element
numbers.Add(20);
numbers.Add(30);

Console.WriteLine(numbers[0]);  // 10
Console.WriteLine(numbers.Count);  // 3

// Remove elements
numbers.Remove(20);  // Remove by value
numbers.RemoveAt(0);  // Remove by index

// Check if contains
if (numbers.Contains(30))
    Console.WriteLine("Found 30");

// Iterate
foreach (var num in numbers)
    Console.WriteLine(num);

// Convert to array
int[] array = numbers.ToArray();

// Clear
numbers.Clear();  // Remove all elements

// List of objects
var users = new List<User>();
users.Add(new User { Id = 1, Name = "John" });
users.Add(new User { Id = 2, Name = "Jane" });
```

### Dictionary<TKey, TValue> - Key-Value Pairs

```csharp
// DICTIONARY: Key-value collection

var person = new Dictionary<string, string>();
person["Name"] = "John";
person["City"] = "New York";
person["Age"] = "25";

// Access by key
Console.WriteLine(person["Name"]);  // "John"

// Add method
person.Add("Country", "USA");

// Check if key exists
if (person.ContainsKey("City"))
    Console.WriteLine(person["City"]);

// Get value safely
if (person.TryGetValue("Age", out var age))
    Console.WriteLine($"Age: {age}");

// Iterate
foreach (var kvp in person)
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");

// Keys and values
var keys = person.Keys;    // "Name", "City", "Age", "Country"
var values = person.Values;  // "John", "New York", "25", "USA"

// Remove
person.Remove("Country");

// Dictionary with object values
var employees = new Dictionary<int, Employee>();
employees[1] = new Employee { Name = "John", Salary = 50000 };
employees[2] = new Employee { Name = "Jane", Salary = 60000 };
```

### HashSet<T> - Unique Values

```csharp
// HASHSET: Unordered, unique values

var uniqueNumbers = new HashSet<int>();
uniqueNumbers.Add(10);
uniqueNumbers.Add(20);
uniqueNumbers.Add(10);  // Duplicate, won't be added

Console.WriteLine(uniqueNumbers.Count);  // 2 (not 3)

// Check membership (fast)
if (uniqueNumbers.Contains(20))
    Console.WriteLine("Found");

// Set operations
var set1 = new HashSet<int> { 1, 2, 3, 4 };
var set2 = new HashSet<int> { 3, 4, 5, 6 };

set1.UnionWith(set2);      // {1, 2, 3, 4, 5, 6}
set1.IntersectWith(set2);  // {3, 4}
set1.ExceptWith(set2);     // Elements in set1 but not set2
```

### Queue<T> - First-In-First-Out

```csharp
// QUEUE: FIFO (First In, First Out)

var queue = new Queue<string>();
queue.Enqueue("First");
queue.Enqueue("Second");
queue.Enqueue("Third");

// Remove from front
var first = queue.Dequeue();  // "First"
Console.WriteLine(first);

// Peek without removing
var next = queue.Peek();  // "Second"

Console.WriteLine(queue.Count);  // 2
```

### Stack<T> - Last-In-First-Out

```csharp
// STACK: LIFO (Last In, First Out)

var stack = new Stack<int>();
stack.Push(10);   // Add to top
stack.Push(20);
stack.Push(30);

var top = stack.Pop();  // 30 (Last in, first out)
Console.WriteLine(top);

// Peek without removing
var next = stack.Peek();  // 20

Console.WriteLine(stack.Count);  // 2

// Use case: Undo/Redo functionality
var undoStack = new Stack<string>();
undoStack.Push("Action 1");
undoStack.Push("Action 2");
var lastAction = undoStack.Pop();  // Undo last action
```

### LINQ - Query Collections

```csharp
// LINQ: Language Integrated Query

var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// Filter
var evenNumbers = numbers.Where(n => n % 2 == 0);  // 2, 4, 6, 8, 10

// Transform
var doubled = numbers.Select(n => n * 2);  // 2, 4, 6, 8, 10, 12, 14, 16, 18, 20

// Combine filter and transform
var result = numbers
    .Where(n => n > 5)       // 6, 7, 8, 9, 10
    .Select(n => n * 2)      // 12, 14, 16, 18, 20
    .OrderByDescending(n => n);  // 20, 18, 16, 14, 12

// Aggregate operations
int sum = numbers.Sum();      // 55
int count = numbers.Count();  // 10
int max = numbers.Max();      // 10
int min = numbers.Min();      // 1
double average = numbers.Average();  // 5.5

// First/Last
int first = numbers.First();  // 1
int last = numbers.Last();    // 10
int firstEven = numbers.FirstOrDefault(n => n % 2 == 0);  // 2

// Any/All
bool hasEven = numbers.Any(n => n % 2 == 0);  // true
bool allPositive = numbers.All(n => n > 0);   // true

// Distinct
var duplicates = new List<int> { 1, 2, 2, 3, 3, 3 };
var unique = duplicates.Distinct();  // 1, 2, 3

// Take/Skip
var first3 = numbers.Take(3);    // 1, 2, 3
var skipFirst3 = numbers.Skip(3);  // 4, 5, 6, 7, 8, 9, 10
```

---

## 4. STATEMENTS ⚙️

### Conditional Statements

```csharp
// IF-ELSE
int age = 18;

if (age >= 18)
    Console.WriteLine("Adult");
else
    Console.WriteLine("Minor");

// IF-ELSE IF-ELSE
if (age < 13)
    Console.WriteLine("Child");
else if (age < 18)
    Console.WriteLine("Teen");
else if (age < 65)
    Console.WriteLine("Adult");
else
    Console.WriteLine("Senior");

// TERNARY OPERATOR
string status = age >= 18 ? "Adult" : "Minor";

// SWITCH
int day = 3;
switch (day)
{
    case 1:
        Console.WriteLine("Monday");
        break;
    case 2:
        Console.WriteLine("Tuesday");
        break;
    case 3:
    case 4:
    case 5:
        Console.WriteLine("Weekday");
        break;
    default:
        Console.WriteLine("Unknown");
        break;
}

// SWITCH EXPRESSION (C# 8+)
string dayName = day switch
{
    1 => "Monday",
    2 => "Tuesday",
    3 => "Wednesday",
    _ => "Unknown"
};

// SWITCH WITH PATTERNS (C# 8+)
string GetCategory(int score) => score switch
{
    >= 90 => "A",
    >= 80 => "B",
    >= 70 => "C",
    >= 60 => "D",
    _ => "F"
};
```

### Loop Statements

```csharp
// FOR LOOP
for (int i = 0; i < 5; i++)
    Console.WriteLine(i);  // 0, 1, 2, 3, 4

// FOR LOOP WITH STEP
for (int i = 0; i <= 10; i += 2)
    Console.WriteLine(i);  // 0, 2, 4, 6, 8, 10

// WHILE LOOP
int count = 0;
while (count < 3)
{
    Console.WriteLine(count);  // 0, 1, 2
    count++;
}

// DO-WHILE LOOP (executes at least once)
int num = 0;
do
{
    Console.WriteLine(num);
    num++;
} while (num < 3);

// FOREACH LOOP (for collections)
var fruits = new[] { "Apple", "Banana", "Orange" };
foreach (var fruit in fruits)
    Console.WriteLine(fruit);

// FOREACH WITH INDEX (C# 7+)
var items = new List<string> { "A", "B", "C" };
foreach (var (item, index) in items.Select((x, i) => (x, i)))
    Console.WriteLine($"{index}: {item}");

// BREAK AND CONTINUE
for (int i = 0; i < 5; i++)
{
    if (i == 2)
        continue;  // Skip to next iteration
    if (i == 4)
        break;     // Exit loop
    Console.WriteLine(i);  // 0, 1, 3
}
```

### Exception Handling

```csharp
// TRY-CATCH
try
{
    int result = 10 / int.Parse("0");  // Throws DivideByZeroException
}
catch (DivideByZeroException ex)
{
    Console.WriteLine($"Cannot divide by zero: {ex.Message}");
}
catch (FormatException ex)
{
    Console.WriteLine($"Invalid number format: {ex.Message}");
}
catch (Exception ex)  // Catch all exceptions
{
    Console.WriteLine($"Error: {ex.Message}");
}

// TRY-CATCH-FINALLY
try
{
    var file = File.OpenText("nonexistent.txt");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.FileName}");
}
finally
{
    // Always executes, even if exception occurs
    Console.WriteLine("Cleanup code");
}

// THROWING EXCEPTIONS
public void ValidateAge(int age)
{
    if (age < 0)
        throw new ArgumentException("Age cannot be negative");
    if (age > 150)
        throw new ArgumentOutOfRangeException(nameof(age), "Age unrealistic");
}
```

### Using Statement (Resource Management)

```csharp
// OLD WAY: Manual try-finally
StreamReader reader = new StreamReader("file.txt");
try
{
    string line = reader.ReadLine();
}
finally
{
    reader?.Dispose();
}

// MODERN WAY: Using statement
using (var reader = new StreamReader("file.txt"))
{
    string line = reader.ReadLine();
}  // reader automatically disposed

// MODERN WAY: Using declaration (C# 8+)
using var reader2 = new StreamReader("file.txt");
string line2 = reader2.ReadLine();
// reader2 automatically disposed at end of method
```

---

## 5. COMPLETE PRACTICAL EXAMPLES 💡

### Example 1: Student Grade Tracker

```csharp
public class StudentGradeTracker
{
    private Dictionary<string, List<int>> studentGrades = new();
    
    public void AddGrade(string studentName, int grade)
    {
        if (!studentGrades.ContainsKey(studentName))
            studentGrades[studentName] = new List<int>();
        
        studentGrades[studentName].Add(grade);
        Console.WriteLine($"Added grade {grade} for {studentName}");
    }
    
    public double GetAverage(string studentName)
    {
        if (!studentGrades.ContainsKey(studentName))
            return 0;
        
        var grades = studentGrades[studentName];
        return grades.Average();
    }
    
    public string GetGrade(double average) => average switch
    {
        >= 90 => "A",
        >= 80 => "B",
        >= 70 => "C",
        >= 60 => "D",
        _ => "F"
    };
    
    public void PrintReport()
    {
        foreach (var student in studentGrades)
        {
            double average = student.Value.Average();
            string grade = GetGrade(average);
            Console.WriteLine($"{student.Key}: Average = {average:F2}, Grade = {grade}");
        }
    }
}

// USAGE
var tracker = new StudentGradeTracker();
tracker.AddGrade("John", 85);
tracker.AddGrade("John", 90);
tracker.AddGrade("Jane", 92);
tracker.AddGrade("Jane", 88);
tracker.PrintReport();
// Output:
// John: Average = 87.50, Grade = B
// Jane: Average = 90.00, Grade = A
```

### Example 2: Shopping Cart with Collections

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class ShoppingCart
{
    private Dictionary<int, (Product product, int quantity)> items = new();
    
    public void AddItem(Product product, int quantity)
    {
        if (items.ContainsKey(product.Id))
        {
            var (p, q) = items[product.Id];
            items[product.Id] = (p, q + quantity);
        }
        else
        {
            items[product.Id] = (product, quantity);
        }
    }
    
    public void RemoveItem(int productId)
    {
        items.Remove(productId);
    }
    
    public decimal GetTotal()
    {
        return items.Values.Sum(x => x.product.Price * x.quantity);
    }
    
    public void DisplayCart()
    {
        Console.WriteLine("=== Shopping Cart ===");
        foreach (var item in items.Values)
        {
            decimal lineTotal = item.product.Price * item.quantity;
            Console.WriteLine($"{item.product.Name} x{item.quantity} = ${lineTotal:F2}");
        }
        Console.WriteLine($"Total: ${GetTotal():F2}");
    }
}

// USAGE
var cart = new ShoppingCart();
cart.AddItem(new Product { Id = 1, Name = "Apple", Price = 1.50m }, 5);
cart.AddItem(new Product { Id = 2, Name = "Banana", Price = 0.75m }, 3);
cart.DisplayCart();
// Output:
// === Shopping Cart ===
// Apple x5 = $7.50
// Banana x3 = $2.25
// Total: $9.75
```

### Example 3: Data Processing Pipeline

```csharp
public class DataProcessor
{
    private List<int> data = new();
    
    public void LoadData(int[] initialData)
    {
        data = initialData.ToList();
        Console.WriteLine($"Loaded {data.Count} items");
    }
    
    public void ProcessAndDisplay()
    {
        try
        {
            if (data.Count == 0)
                throw new InvalidOperationException("No data loaded");
            
            // Filter and transform
            var result = data
                .Where(x => x > 0)           // Only positive
                .Select(x => x * 2)          // Double each
                .OrderByDescending(x => x)   // Sort descending
                .Take(5)                     // First 5
                .ToList();
            
            Console.WriteLine($"Processed {result.Count} items:");
            foreach (var item in result)
                Console.WriteLine($"  {item}");
            
            // Statistics
            Console.WriteLine($"Average: {result.Average():F2}");
            Console.WriteLine($"Sum: {result.Sum()}");
            Console.WriteLine($"Max: {result.Max()}");
            Console.WriteLine($"Min: {result.Min()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

// USAGE
var processor = new DataProcessor();
processor.LoadData(new[] { -5, 10, 3, -2, 8, 15, 1, 20 });
processor.ProcessAndDisplay();
// Output:
// Loaded 8 items
// Processed 5 items:
//   40
//   30
//   20
//   16
//   6
// Average: 22.40
// Sum: 112
// Max: 40
// Min: 6
```

---

## Quick Reference Table

| Concept | Example | Purpose |
|---------|---------|---------|
| **Variable** | `int age = 25;` | Store a value |
| **Constant** | `const double PI = 3.14;` | Value that never changes |
| **Nullable** | `int? age = null;` | Variable that can be null |
| **Array** | `int[] nums = {1,2,3};` | Fixed-size collection |
| **List** | `List<int> nums = new();` | Dynamic-size collection |
| **Dictionary** | `Dictionary<string, int>` | Key-value pairs |
| **HashSet** | `HashSet<int>` | Unique values only |
| **Queue** | `Queue<T>` | FIFO - First In First Out |
| **Stack** | `Stack<T>` | LIFO - Last In First Out |
| **If-Else** | `if (x > 5) { }` | Conditional execution |
| **Switch** | `switch (day) { }` | Multiple conditions |
| **For Loop** | `for (int i = 0; i < 10; i++)` | Repeat fixed times |
| **While Loop** | `while (x > 0) { }` | Repeat while condition true |
| **ForEach** | `foreach (var item in list)` | Iterate collection |
| **Try-Catch** | `try { } catch { }` | Handle exceptions |
| **LINQ** | `.Where().Select()` | Query collections |

---

## Next Steps

1. **Practice Variables:** Create variables of different types and assign values
2. **Learn Collections:** Use List, Dictionary, and HashSet in your projects
3. **Master Loops:** Understand when to use for, while, and foreach
4. **Handle Exceptions:** Learn to catch and handle errors gracefully
5. **Use LINQ:** Query and transform collections efficiently

**Created:** January 21, 2025
**Target Audience:** C# beginners
**Scope:** Fundamentals with practical examples
