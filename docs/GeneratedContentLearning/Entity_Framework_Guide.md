# Entity Framework in ASP.NET - Complete Guide

A comprehensive guide to Entity Framework (EF Core and EF6) in ASP.NET with practical examples for SQL Server, MySQL, PostgreSQL, and Oracle backends.

---

## Table of Contents

1. [Setup & Configuration](#setup--configuration)
2. [Database Context](#database-context)
3. [Models & Relationships](#models--relationships)
4. [Querying Data (LINQ)](#querying-data-linq)
5. [CRUD Operations](#crud-operations)
6. [Advanced Queries](#advanced-queries)
7. [Relationships & Navigation](#relationships--navigation)
8. [Performance Optimization](#performance-optimization)
9. [Migrations](#migrations)
10. [Testing](#testing)

---

## Setup & Configuration

### Project Structure

```
MyProject/
├── Models/                    # Entity classes
│   ├── Employee.cs
│   ├── Department.cs
│   └── Project.cs
├── Data/
│   ├── ApplicationDbContext.cs  # DbContext class
│   └── Configurations/        # Fluent API configurations
│       ├── EmployeeConfiguration.cs
│       └── DepartmentConfiguration.cs
├── Services/                  # Business logic
│   ├── EmployeeService.cs
│   └── DepartmentService.cs
├── Controllers/               # API controllers
├── Migrations/                # Database migrations
└── appsettings.json
```

### Package Installation

#### For SQL Server (Default)

```bash
# NuGet Package Manager
Install-Package Microsoft.EntityFrameworkCore
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools

# Or via .NET CLI
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

#### For MySQL

```bash
# NuGet Package Manager
Install-Package Pomelo.EntityFrameworkCore.MySql

# Or via .NET CLI
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

#### For PostgreSQL

```bash
# NuGet Package Manager
Install-Package Npgsql.EntityFrameworkCore.PostgreSQL

# Or via .NET CLI
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

#### For Oracle

```bash
# NuGet Package Manager
Install-Package Oracle.EntityFrameworkCore

# Or via .NET CLI
dotnet add package Oracle.EntityFrameworkCore
```

### appsettings.json Configuration

#### SQL Server
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MyDatabase;Trusted_Connection=true;Encrypt=true;TrustServerCertificate=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  }
}
```

#### MySQL
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=mydatabase;Uid=root;Pwd=password;"
  }
}
```

#### PostgreSQL
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=mydatabase;Username=postgres;Password=password"
  }
}
```

#### Oracle
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "User Id=system;Password=password;Data Source=localhost:1521/orcl;"
  }
}
```

### Dependency Injection Setup (Program.cs / Startup.cs)

#### For SQL Server
```csharp
// Program.cs (.NET 6+)
var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
app.Run();
```

#### For MySQL
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));
```

#### For PostgreSQL
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

#### For Oracle
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));
```

---

## Database Context

### Basic DbContext Implementation

```csharp
using Microsoft.EntityFrameworkCore;
using MyProject.Models;

namespace MyProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets represent tables
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entities
            modelBuilder.Entity<Employee>()
                .ToTable("employees");

            modelBuilder.Entity<Department>()
                .ToTable("departments");

            // Add seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed departments
            modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentId = 1, Name = "Engineering", Location = "New York" },
                new Department { DepartmentId = 2, Name = "Marketing", Location = "Boston" },
                new Department { DepartmentId = 3, Name = "Sales", Location = "Chicago" }
            );

            // Seed employees
            modelBuilder.Entity<Employee>().HasData(
                new Employee { EmployeeId = 1, FirstName = "John", LastName = "Doe", DepartmentId = 1, Salary = 85000, HireDate = new DateTime(2020, 1, 15) },
                new Employee { EmployeeId = 2, FirstName = "Jane", LastName = "Smith", DepartmentId = 1, Salary = 92000, HireDate = new DateTime(2019, 3, 20) },
                new Employee { EmployeeId = 3, FirstName = "Bob", LastName = "Johnson", DepartmentId = 2, Salary = 75000, HireDate = new DateTime(2021, 6, 10) }
            );
        }
    }
}
```

### DbContext with Entity Configurations

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Apply fluent configurations from separate files
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
}
```

#### EmployeeConfiguration.cs
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyProject.Models;

namespace MyProject.Data.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("employees");

            builder.HasKey(e => e.EmployeeId);

            builder.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Email)
                .HasMaxLength(100);

            builder.Property(e => e.Salary)
                .HasColumnType("decimal(10,2)");

            builder.Property(e => e.HireDate)
                .HasColumnType("datetime");

            // Relationships
            builder.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId);

            builder.HasMany(e => e.Projects)
                .WithMany(p => p.Employees)
                .UsingEntity("EmployeeProject");
        }
    }
}
```

---

## Models & Relationships

### Entity Models

#### Employee Model
```csharp
namespace MyProject.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Foreign key
        public int DepartmentId { get; set; }

        // Navigation properties
        public Department Department { get; set; }
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<EmployeeReview> Reviews { get; set; } = new List<EmployeeReview>();

        // Computed property
        public string FullName => $"{FirstName} {LastName}";
    }
}
```

#### Department Model
```csharp
namespace MyProject.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
```

#### Project Model
```csharp
namespace MyProject.Models
{
    public class Project
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Budget { get; set; }
        public ProjectStatus Status { get; set; }

        // Navigation property (many-to-many)
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }

    public enum ProjectStatus
    {
        Planning,
        InProgress,
        OnHold,
        Completed,
        Cancelled
    }
}
```

#### EmployeeReview Model (One-to-Many)
```csharp
namespace MyProject.Models
{
    public class EmployeeReview
    {
        public int ReviewId { get; set; }
        public int EmployeeId { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
        public string ReviewedBy { get; set; }

        // Navigation property
        public Employee Employee { get; set; }
    }
}
```

### Relationship Types

#### One-to-Many (Employee -> Department)
```csharp
// In Employee model
public int DepartmentId { get; set; }
public Department Department { get; set; }

// In Department model
public ICollection<Employee> Employees { get; set; } = new List<Employee>();

// Configuration
modelBuilder.Entity<Employee>()
    .HasOne(e => e.Department)
    .WithMany(d => d.Employees)
    .HasForeignKey(e => e.DepartmentId)
    .OnDelete(DeleteBehavior.Cascade);
```

#### Many-to-Many (Employee <-> Project)
```csharp
// Method 1: Using navigation properties only
modelBuilder.Entity<Employee>()
    .HasMany(e => e.Projects)
    .WithMany(p => p.Employees)
    .UsingEntity("EmployeeProject");

// Method 2: Using explicit join entity
public class EmployeeProject
{
    public int EmployeeId { get; set; }
    public int ProjectId { get; set; }
    public DateTime AssignedDate { get; set; }
    public int HoursAllocated { get; set; }

    public Employee Employee { get; set; }
    public Project Project { get; set; }
}

// Configuration
modelBuilder.Entity<EmployeeProject>()
    .HasKey(ep => new { ep.EmployeeId, ep.ProjectId });

modelBuilder.Entity<EmployeeProject>()
    .HasOne(ep => ep.Employee)
    .WithMany(e => e.EmployeeProjects)
    .HasForeignKey(ep => ep.EmployeeId);

modelBuilder.Entity<EmployeeProject>()
    .HasOne(ep => ep.Project)
    .WithMany(p => p.EmployeeProjects)
    .HasForeignKey(ep => ep.ProjectId);
```

---

## Querying Data (LINQ)

### Basic LINQ Queries

#### Simple SELECT
```csharp
using var context = new ApplicationDbContext();

// Get all employees
var allEmployees = context.Employees.ToList();

// Get with ordering
var employeesByName = context.Employees
    .OrderBy(e => e.FirstName)
    .ThenBy(e => e.LastName)
    .ToList();

// Get specific columns
var employeeNames = context.Employees
    .Select(e => new { e.EmployeeId, e.FirstName, e.LastName })
    .ToList();

// Get with filter
var activeEmployees = context.Employees
    .Where(e => e.IsActive)
    .ToList();

// Get single record
var employee = context.Employees
    .FirstOrDefault(e => e.EmployeeId == 1);

// Check existence
bool exists = context.Employees
    .Any(e => e.Email == "john.doe@company.com");
```

#### LINQ Operators

```csharp
// WHERE - Filtering
var highSalaryEmployees = context.Employees
    .Where(e => e.Salary > 80000)
    .ToList();

// Multiple WHERE conditions
var filtered = context.Employees
    .Where(e => e.IsActive && e.Salary > 50000 && e.DepartmentId == 1)
    .ToList();

// ORDER BY
var sorted = context.Employees
    .OrderByDescending(e => e.Salary)
    .ThenBy(e => e.FirstName)
    .ToList();

// SELECT (projection)
var dto = context.Employees
    .Select(e => new EmployeeDto 
    { 
        Id = e.EmployeeId,
        FullName = e.FullName,
        Email = e.Email
    })
    .ToList();

// DISTINCT
var departments = context.Employees
    .Select(e => e.DepartmentId)
    .Distinct()
    .ToList();

// SKIP and TAKE (pagination)
var page = context.Employees
    .OrderBy(e => e.EmployeeId)
    .Skip(10)
    .Take(20)
    .ToList();

// COUNT, SUM, AVG, MIN, MAX
int totalEmployees = context.Employees.Count();
int activeCount = context.Employees.Count(e => e.IsActive);
decimal totalSalary = context.Employees.Sum(e => e.Salary);
decimal averageSalary = context.Employees.Average(e => e.Salary);
decimal minSalary = context.Employees.Min(e => e.Salary);
decimal maxSalary = context.Employees.Max(e => e.Salary);

// GROUP BY
var groupedBySalary = context.Employees
    .GroupBy(e => e.DepartmentId)
    .Select(g => new
    {
        DepartmentId = g.Key,
        EmployeeCount = g.Count(),
        AverageSalary = g.Average(e => e.Salary),
        TotalSalary = g.Sum(e => e.Salary)
    })
    .ToList();

// DISTINCT with multiple columns
var distinctEmails = context.Employees
    .Select(e => new { e.Email })
    .Distinct()
    .ToList();
```

### Complex Filtering

```csharp
// String operations
var startsWithJohn = context.Employees
    .Where(e => e.FirstName.StartsWith("John"))
    .ToList();

var containsDoe = context.Employees
    .Where(e => e.LastName.Contains("Doe"))
    .ToList();

var emailEndsWithCom = context.Employees
    .Where(e => e.Email.EndsWith(".com"))
    .ToList();

// Date range filtering
var hiredInLastYear = context.Employees
    .Where(e => e.HireDate >= DateTime.Now.AddYears(-1))
    .ToList();

// Using IN operator
var targetIds = new[] { 1, 2, 3 };
var selectedEmployees = context.Employees
    .Where(e => targetIds.Contains(e.EmployeeId))
    .ToList();

// Using NOT operator
var inactiveDepts = context.Employees
    .Where(e => !e.IsActive)
    .ToList();

// BETWEEN equivalent
var midRangeSalaries = context.Employees
    .Where(e => e.Salary >= 50000 && e.Salary <= 80000)
    .ToList();

// NULL checks
var employeesWithoutEmail = context.Employees
    .Where(e => string.IsNullOrEmpty(e.Email))
    .ToList();

// OR condition
var orQuery = context.Employees
    .Where(e => e.DepartmentId == 1 || e.DepartmentId == 2)
    .ToList();
```

---

## CRUD Operations

### Create (INSERT)

```csharp
public class EmployeeService
{
    private readonly ApplicationDbContext _context;

    public EmployeeService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Create single employee
    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    // Create with validation
    public async Task<Employee> CreateEmployeeWithValidationAsync(Employee employee)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(employee.FirstName))
            throw new ArgumentException("First name is required");

        if (employee.Salary <= 0)
            throw new ArgumentException("Salary must be greater than zero");

        // Check if department exists
        var departmentExists = await _context.Departments
            .AnyAsync(d => d.DepartmentId == employee.DepartmentId);

        if (!departmentExists)
            throw new InvalidOperationException("Department not found");

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    // Bulk insert
    public async Task<int> BulkInsertEmployeesAsync(List<Employee> employees)
    {
        _context.Employees.AddRange(employees);
        return await _context.SaveChangesAsync();
    }

    // Add with related data
    public async Task<Employee> CreateEmployeeWithProjectsAsync(Employee employee, List<int> projectIds)
    {
        var projects = await _context.Projects
            .Where(p => projectIds.Contains(p.ProjectId))
            .ToListAsync();

        employee.Projects = projects;
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }
}
```

### Read (SELECT)

```csharp
public class EmployeeService
{
    // Get by ID
    public async Task<Employee> GetEmployeeByIdAsync(int id)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == id);
    }

    // Get with related data
    public async Task<Employee> GetEmployeeWithDepartmentAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.EmployeeId == id);
    }

    // Get with multiple includes
    public async Task<Employee> GetEmployeeFullDataAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Projects)
            .Include(e => e.Reviews)
            .FirstOrDefaultAsync(e => e.EmployeeId == id);
    }

    // Get all with pagination
    public async Task<PaginatedList<Employee>> GetEmployeesAsync(int pageNumber, int pageSize)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .OrderBy(e => e.EmployeeId);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedList<Employee>(items, totalCount, pageNumber, pageSize);
    }

    // Search
    public async Task<List<Employee>> SearchEmployeesAsync(string searchTerm)
    {
        return await _context.Employees
            .Where(e => e.FirstName.Contains(searchTerm) ||
                        e.LastName.Contains(searchTerm) ||
                        e.Email.Contains(searchTerm))
            .ToListAsync();
    }

    // Get with related data (filtered)
    public async Task<List<Employee>> GetDepartmentEmployeesAsync(int departmentId)
    {
        return await _context.Employees
            .Where(e => e.DepartmentId == departmentId)
            .Include(e => e.Projects)
            .OrderBy(e => e.FirstName)
            .ToListAsync();
    }
}
```

### Update (UPDATE)

```csharp
// Simple update
public async Task<Employee> UpdateEmployeeAsync(Employee employee)
{
    var existingEmployee = await _context.Employees
        .FirstOrDefaultAsync(e => e.EmployeeId == employee.EmployeeId);

    if (existingEmployee == null)
        throw new InvalidOperationException("Employee not found");

    // Update properties
    existingEmployee.FirstName = employee.FirstName;
    existingEmployee.LastName = employee.LastName;
    existingEmployee.Email = employee.Email;
    existingEmployee.Salary = employee.Salary;

    await _context.SaveChangesAsync();
    return existingEmployee;
}

// Update specific columns
public async Task<bool> UpdateEmployeeSalaryAsync(int employeeId, decimal newSalary)
{
    var employee = await _context.Employees
        .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

    if (employee == null)
        return false;

    employee.Salary = newSalary;
    await _context.SaveChangesAsync();
    return true;
}

// Bulk update
public async Task<int> IncreaseSalariesAsync(int departmentId, decimal percentage)
{
    var employees = await _context.Employees
        .Where(e => e.DepartmentId == departmentId)
        .ToListAsync();

    foreach (var employee in employees)
    {
        employee.Salary = employee.Salary * (1 + percentage / 100);
    }

    return await _context.SaveChangesAsync();
}

// Update with ExecuteUpdate (EF Core 7+)
public async Task<int> UpdateSalaryDirectAsync(int employeeId, decimal newSalary)
{
    return await _context.Employees
        .Where(e => e.EmployeeId == employeeId)
        .ExecuteUpdateAsync(s => s.SetProperty(e => e.Salary, newSalary));
}

// Update related data
public async Task<bool> UpdateEmployeeProjectsAsync(int employeeId, List<int> projectIds)
{
    var employee = await _context.Employees
        .Include(e => e.Projects)
        .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

    if (employee == null)
        return false;

    // Clear current projects
    employee.Projects.Clear();

    // Add new projects
    var projects = await _context.Projects
        .Where(p => projectIds.Contains(p.ProjectId))
        .ToListAsync();

    employee.Projects = projects;

    await _context.SaveChangesAsync();
    return true;
}
```

### Delete (DELETE)

```csharp
// Simple delete
public async Task<bool> DeleteEmployeeAsync(int employeeId)
{
    var employee = await _context.Employees
        .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

    if (employee == null)
        return false;

    _context.Employees.Remove(employee);
    await _context.SaveChangesAsync();
    return true;
}

// Bulk delete
public async Task<int> DeleteEmployeesByDepartmentAsync(int departmentId)
{
    var employees = await _context.Employees
        .Where(e => e.DepartmentId == departmentId)
        .ToListAsync();

    _context.Employees.RemoveRange(employees);
    return await _context.SaveChangesAsync();
}

// Delete with ExecuteDelete (EF Core 7+)
public async Task<int> DeleteEmployeeDirectAsync(int employeeId)
{
    return await _context.Employees
        .Where(e => e.EmployeeId == employeeId)
        .ExecuteDeleteAsync();
}

// Soft delete (logical delete)
public async Task<bool> SoftDeleteEmployeeAsync(int employeeId)
{
    var employee = await _context.Employees
        .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

    if (employee == null)
        return false;

    employee.IsActive = false;
    await _context.SaveChangesAsync();
    return true;
}
```

---

## Advanced Queries

### Joins

```csharp
// INNER JOIN
var employeesWithDept = context.Employees
    .Join(context.Departments,
        e => e.DepartmentId,
        d => d.DepartmentId,
        (e, d) => new
        {
            e.EmployeeId,
            e.FirstName,
            e.LastName,
            DepartmentName = d.Name
        })
    .ToList();

// Using Include (automatic join)
var employeesWithDeptInclude = context.Employees
    .Include(e => e.Department)
    .Select(e => new
    {
        e.EmployeeId,
        e.FirstName,
        e.LastName,
        DepartmentName = e.Department.Name
    })
    .ToList();

// Multiple joins
var complexJoin = context.Employees
    .Join(context.Departments, e => e.DepartmentId, d => d.DepartmentId, (e, d) => new { e, d })
    .Join(context.Projects, ed => ed.e.EmployeeId, p => p.ProjectId, (ed, p) => new { ed.e, ed.d, p })
    .Select(x => new
    {
        x.e.EmployeeId,
        x.e.FirstName,
        DepartmentName = x.d.Name,
        ProjectName = x.p.Name
    })
    .ToList();

// LEFT JOIN (using DefaultIfEmpty)
var leftJoin = context.Employees
    .GroupJoin(context.Departments,
        e => e.DepartmentId,
        d => d.DepartmentId,
        (e, d) => new
        {
            Employee = e,
            Department = d.FirstOrDefault()
        })
    .ToList();
```

### Subqueries

```csharp
// Subquery in WHERE clause
var highEarners = context.Employees
    .Where(e => e.Salary > context.Employees.Average(x => x.Salary))
    .ToList();

// Subquery in SELECT
var employeeWithAvgDeptSalary = context.Employees
    .Select(e => new
    {
        e.EmployeeId,
        e.FirstName,
        e.Salary,
        AverageDepartmentSalary = context.Employees
            .Where(x => x.DepartmentId == e.DepartmentId)
            .Average(x => x.Salary)
    })
    .ToList();

// Using Any() in subquery
var employeesInActiveProjects = context.Employees
    .Where(e => context.Projects
        .Where(p => p.Status == ProjectStatus.InProgress)
        .SelectMany(p => p.Employees)
        .Any(emp => emp.EmployeeId == e.EmployeeId))
    .ToList();

// Using Count() in subquery
var employeeProjectCount = context.Employees
    .Select(e => new
    {
        e.EmployeeId,
        e.FirstName,
        ProjectCount = context.Projects
            .Where(p => p.Employees.Any(emp => emp.EmployeeId == e.EmployeeId))
            .Count()
    })
    .ToList();
```

### Grouping and Aggregation

```csharp
// Simple GROUP BY
var groupedByDept = context.Employees
    .GroupBy(e => e.DepartmentId)
    .Select(g => new
    {
        DepartmentId = g.Key,
        EmployeeCount = g.Count(),
        AverageSalary = g.Average(e => e.Salary),
        TotalSalary = g.Sum(e => e.Salary),
        MinSalary = g.Min(e => e.Salary),
        MaxSalary = g.Max(e => e.Salary)
    })
    .ToList();

// GROUP BY with HAVING
var departmentsWithHighAvgSalary = context.Employees
    .GroupBy(e => e.DepartmentId)
    .Where(g => g.Average(e => e.Salary) > 75000)
    .Select(g => new
    {
        DepartmentId = g.Key,
        EmployeeCount = g.Count(),
        AverageSalary = g.Average(e => e.Salary)
    })
    .ToList();

// GROUP BY multiple columns
var groupedByDeptAndStatus = context.Employees
    .GroupBy(e => new { e.DepartmentId, e.IsActive })
    .Select(g => new
    {
        g.Key.DepartmentId,
        g.Key.IsActive,
        EmployeeCount = g.Count(),
        AverageSalary = g.Average(e => e.Salary)
    })
    .ToList();

// GROUP BY with Department include
var deptStats = context.Departments
    .Select(d => new
    {
        DepartmentName = d.Name,
        EmployeeCount = d.Employees.Count,
        AverageSalary = d.Employees.Average(e => e.Salary)
    })
    .ToList();
```

### Window Functions (EF Core 7.0+)

```csharp
// ROW_NUMBER
var withRowNumber = context.Employees
    .OrderBy(e => e.DepartmentId)
    .ThenBy(e => e.Salary)
    .Select((e, index) => new
    {
        e.EmployeeId,
        e.FirstName,
        e.Salary,
        RowNumber = index + 1
    })
    .ToList();

// RANK within department (by salary)
var rankBySalary = context.Employees
    .GroupBy(e => e.DepartmentId)
    .SelectMany(g => g
        .OrderByDescending(e => e.Salary)
        .Select((e, index) => new
        {
            e.EmployeeId,
            e.FirstName,
            e.Salary,
            Rank = index + 1,
            DepartmentId = g.Key
        }))
    .ToList();

// Running total
var runningTotal = context.Employees
    .OrderBy(e => e.EmployeeId)
    .Select(e => new
    {
        e.EmployeeId,
        e.FirstName,
        e.Salary,
        RunningTotal = context.Employees
            .Where(x => x.EmployeeId <= e.EmployeeId)
            .Sum(x => x.Salary)
    })
    .ToList();

// LAG equivalent (previous value)
var withPrevious = context.Employees
    .OrderBy(e => e.EmployeeId)
    .Select((e, idx) => new
    {
        e.EmployeeId,
        e.FirstName,
        e.Salary,
        PreviousSalary = context.Employees
            .Where(x => x.EmployeeId < e.EmployeeId)
            .OrderByDescending(x => x.EmployeeId)
            .Select(x => (decimal?)x.Salary)
            .FirstOrDefault()
    })
    .ToList();
```

### Union and Intersect

```csharp
// UNION
var allEmployeeNames = context.Employees
    .Select(e => e.FirstName)
    .Union(context.Employees.Select(e => e.LastName))
    .ToList();

// UNION ALL
var combinedList = context.Employees
    .Select(e => new { e.EmployeeId, e.FirstName })
    .Concat(context.Employees
        .Select(e => new { EmployeeId = e.DepartmentId, FirstName = "Dept: " + e.FirstName }))
    .ToList();

// INTERSECT
var commonIds = context.Employees
    .Select(e => e.DepartmentId)
    .Intersect(context.Departments.Select(d => d.DepartmentId))
    .ToList();
```

---

## Relationships & Navigation

### Include and ThenInclude

```csharp
// Single level include
var employee = context.Employees
    .Include(e => e.Department)
    .FirstOrDefault(e => e.EmployeeId == 1);

// Multiple includes
var employeeWithAll = context.Employees
    .Include(e => e.Department)
    .Include(e => e.Projects)
    .Include(e => e.Reviews)
    .FirstOrDefault(e => e.EmployeeId == 1);

// Nested includes (ThenInclude)
var departmentWithEmployees = context.Departments
    .Include(d => d.Employees)
        .ThenInclude(e => e.Projects)
    .FirstOrDefault(d => d.DepartmentId == 1);

// Filtered include (EF Core 5+)
var employeeWithActiveProjects = context.Employees
    .Include(e => e.Projects.Where(p => p.Status == ProjectStatus.InProgress))
    .FirstOrDefault(e => e.EmployeeId == 1);

// Include with ordering
var departmentWithOrderedEmployees = context.Departments
    .Include(d => d.Employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName))
    .FirstOrDefault(d => d.DepartmentId == 1);
```

### Eager vs Lazy vs Explicit Loading

```csharp
// Eager loading (default recommended)
var eager = context.Employees
    .Include(e => e.Department)
    .Include(e => e.Projects)
    .ToList();

// Lazy loading (requires proxy)
// Not recommended for performance reasons
var lazy = context.Employees.FirstOrDefault();
var dept = lazy.Department; // Loads on access (lazy)

// Explicit loading
var employee = context.Employees.FirstOrDefault(e => e.EmployeeId == 1);
context.Entry(employee)
    .Reference(e => e.Department)
    .Load();

context.Entry(employee)
    .Collection(e => e.Projects)
    .Load();
```

### Projection (Select with Related Data)

```csharp
// Projection with related data
var employeeDto = context.Employees
    .Select(e => new EmployeeDto
    {
        EmployeeId = e.EmployeeId,
        FirstName = e.FirstName,
        LastName = e.LastName,
        DepartmentName = e.Department.Name,
        ProjectNames = e.Projects.Select(p => p.Name).ToList(),
        ReviewCount = e.Reviews.Count
    })
    .ToList();

// Flattened projection
var flat = context.Employees
    .SelectMany(e => e.Projects, (e, p) => new
    {
        EmployeeName = e.FirstName + " " + e.LastName,
        ProjectName = p.Name
    })
    .ToList();
```

---

## Performance Optimization

### Query Optimization

#### AsNoTracking for Read-Only Queries
```csharp
// For queries that don't need updates
var employees = context.Employees
    .AsNoTracking()
    .ToList();

// Faster for large result sets
var allEmployees = context.Employees
    .AsNoTracking()
    .Include(e => e.Department)
    .ToList();
```

#### Split Queries (EF Core 5+)
```csharp
// Helps with N+1 problems
var employees = context.Employees
    .AsSplitQuery()
    .Include(e => e.Department)
    .Include(e => e.Projects)
    .ToList();
```

#### SqlInterpolatedString for Complex Queries
```csharp
// Using raw SQL when LINQ is insufficient
var minSalary = 50000;
var employees = context.Employees
    .FromSqlInterpolated($"SELECT * FROM employees WHERE salary > {minSalary}")
    .ToList();

// With parameters
var deptId = 1;
var employeesInDept = context.Employees
    .FromSqlInterpolated($"SELECT * FROM employees WHERE department_id = {deptId}")
    .ToList();
```

### Caching

```csharp
public class CachedEmployeeService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private const string EMPLOYEES_CACHE_KEY = "employees_all";

    public CachedEmployeeService(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<Employee>> GetEmployeesAsync()
    {
        if (_cache.TryGetValue(EMPLOYEES_CACHE_KEY, out List<Employee> employees))
            return employees;

        employees = await _context.Employees
            .AsNoTracking()
            .ToListAsync();

        _cache.Set(EMPLOYEES_CACHE_KEY, employees, TimeSpan.FromMinutes(30));
        return employees;
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Invalidate cache
        _cache.Remove(EMPLOYEES_CACHE_KEY);

        return employee;
    }
}
```

### Batch Operations

```csharp
// Bulk insert (more efficient)
public async Task BulkInsertAsync(List<Employee> employees)
{
    _context.Employees.AddRange(employees);
    await _context.SaveChangesAsync();
}

// Batch update
public async Task BatchUpdateSalariesAsync(List<(int EmployeeId, decimal NewSalary)> updates)
{
    foreach (var (empId, newSalary) in updates)
    {
        var employee = new Employee { EmployeeId = empId, Salary = newSalary };
        _context.Employees.Attach(employee);
        _context.Entry(employee).Property(e => e.Salary).IsModified = true;
    }
    await _context.SaveChangesAsync();
}
```

### Indexing Strategy

```csharp
// In OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Index on frequently queried columns
    modelBuilder.Entity<Employee>()
        .HasIndex(e => e.Email)
        .IsUnique();

    modelBuilder.Entity<Employee>()
        .HasIndex(e => e.DepartmentId);

    // Composite index
    modelBuilder.Entity<Employee>()
        .HasIndex(e => new { e.DepartmentId, e.IsActive });

    modelBuilder.Entity<Employee>()
        .HasIndex(e => e.LastName)
        .HasDatabaseName("idx_employee_lastname");
}
```

---

## Migrations

### Creating and Running Migrations

#### Initial Migration
```bash
# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration to database
dotnet ef database update

# Create and update in one step
dotnet ef database update --migration InitialCreate
```

#### Subsequent Migrations
```bash
# Create new migration for changes
dotnet ef migrations add AddEmployeeEmail

# List all migrations
dotnet ef migrations list

# Get specific migration info
dotnet ef migrations info

# Revert to previous migration
dotnet ef database update PreviousMigrationName

# Remove latest migration (before applying)
dotnet ef migrations remove
```

### Migration File Example

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.Data.Migrations
{
    public partial class AddEmployeeEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_employee_email",
                table: "employees",
                column: "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_employee_email",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "employees");
        }
    }
}
```

### Custom Migration Operations

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Create table
    migrationBuilder.CreateTable(
        name: "employees",
        columns: table => new
        {
            id = table.Column<int>(nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
            first_name = table.Column<string>(maxLength: 50, nullable: false),
            last_name = table.Column<string>(maxLength: 50, nullable: false),
            salary = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
            department_id = table.Column<int>(nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_employees", x => x.id);
            table.ForeignKey(
                name: "FK_employees_departments",
                column: x => x.department_id,
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        });

    // Add index
    migrationBuilder.CreateIndex(
        name: "idx_salary",
        table: "employees",
        column: "salary");

    // Add data
    migrationBuilder.InsertData(
        table: "employees",
        columns: new[] { "first_name", "last_name", "salary", "department_id" },
        values: new object[] { "John", "Doe", 85000m, 1 });
}
```

---

## Testing

### Unit Tests with InMemory Database

```csharp
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MyProject.Tests
{
    public class EmployeeServiceTests
    {
        private ApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CreateEmployee_WithValidData_ShouldAddEmployee()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = new EmployeeService(context);

            var employee = new Employee
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Salary = 50000,
                DepartmentId = 1
            };

            // Act
            var result = await service.CreateEmployeeAsync(employee);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.EmployeeId > 0);
            var savedEmployee = await context.Employees.FindAsync(result.EmployeeId);
            Assert.Equal("John", savedEmployee.FirstName);
        }

        [Fact]
        public async Task GetEmployee_WithValidId_ShouldReturnEmployee()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Employees.Add(new Employee
            {
                EmployeeId = 1,
                FirstName = "Jane",
                LastName = "Smith",
                Salary = 60000,
                DepartmentId = 1
            });
            await context.SaveChangesAsync();

            var service = new EmployeeService(context);

            // Act
            var result = await service.GetEmployeeByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Jane", result.FirstName);
        }

        [Fact]
        public async Task UpdateEmployee_ShouldUpdateProperties()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var employee = new Employee
            {
                EmployeeId = 1,
                FirstName = "John",
                LastName = "Doe",
                Salary = 50000,
                DepartmentId = 1
            };
            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            var service = new EmployeeService(context);

            // Act
            employee.Salary = 55000;
            await service.UpdateEmployeeAsync(employee);

            // Assert
            var updated = await context.Employees.FindAsync(1);
            Assert.Equal(55000, updated.Salary);
        }

        [Fact]
        public async Task DeleteEmployee_ShouldRemoveEmployee()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Employees.Add(new Employee
            {
                EmployeeId = 1,
                FirstName = "John",
                LastName = "Doe",
                Salary = 50000,
                DepartmentId = 1
            });
            await context.SaveChangesAsync();

            var service = new EmployeeService(context);

            // Act
            var result = await service.DeleteEmployeeAsync(1);

            // Assert
            Assert.True(result);
            var deleted = await context.Employees.FindAsync(1);
            Assert.Null(deleted);
        }
    }
}
```

### Integration Tests with SQLite

```csharp
public class EmployeeServiceIntegrationTests
{
    private ApplicationDbContext CreateSqliteContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetEmployeesWithProjects_ShouldIncludeRelatedData()
    {
        // Arrange
        using var context = CreateSqliteContext();
        
        var department = new Department { DepartmentId = 1, Name = "Engineering" };
        var project = new Project { ProjectId = 1, Name = "Project A", Status = ProjectStatus.InProgress };
        var employee = new Employee
        {
            EmployeeId = 1,
            FirstName = "John",
            LastName = "Doe",
            Salary = 50000,
            DepartmentId = 1,
            Department = department,
            Projects = new List<Project> { project }
        };

        context.Add(employee);
        await context.SaveChangesAsync();

        // Act
        var result = await context.Employees
            .Include(e => e.Department)
            .Include(e => e.Projects)
            .FirstOrDefaultAsync(e => e.EmployeeId == 1);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Department);
        Assert.NotEmpty(result.Projects);
        Assert.Equal("Engineering", result.Department.Name);
    }
}
```

### Test Data Factory

```csharp
public class TestDataFactory
{
    public static Employee CreateTestEmployee(
        int id = 1,
        string firstName = "John",
        string lastName = "Doe",
        decimal salary = 50000,
        int departmentId = 1)
    {
        return new Employee
        {
            EmployeeId = id,
            FirstName = firstName,
            LastName = lastName,
            Salary = salary,
            DepartmentId = departmentId,
            HireDate = DateTime.Now
        };
    }

    public static Department CreateTestDepartment(int id = 1, string name = "Engineering")
    {
        return new Department
        {
            DepartmentId = id,
            Name = name,
            Location = "New York"
        };
    }

    public static Project CreateTestProject(int id = 1, string name = "Project A")
    {
        return new Project
        {
            ProjectId = id,
            Name = name,
            Status = ProjectStatus.InProgress,
            Budget = 100000
        };
    }
}
```

---

## Best Practices

### 1. Always Use Async/Await
```csharp
// Good
public async Task<List<Employee>> GetEmployeesAsync()
{
    return await _context.Employees.ToListAsync();
}

// Avoid
public List<Employee> GetEmployees()
{
    return _context.Employees.ToList();
}
```

### 2. Use AsNoTracking for Read-Only Queries
```csharp
// Good for reads
var employees = await _context.Employees
    .AsNoTracking()
    .ToListAsync();

// Keep tracking for updates
var employee = await _context.Employees
    .FirstOrDefaultAsync(e => e.EmployeeId == 1);
```

### 3. Avoid N+1 Queries with Include
```csharp
// Bad - causes N+1 queries
var employees = _context.Employees.ToList();
foreach (var emp in employees)
{
    var dept = _context.Departments.Find(emp.DepartmentId); // Extra query per employee
}

// Good - single query with join
var employees = await _context.Employees
    .Include(e => e.Department)
    .ToListAsync();
```

### 4. Use Projection to Reduce Data Transfer
```csharp
// Good - only fetch needed columns
var employeeNames = await _context.Employees
    .Select(e => new { e.EmployeeId, e.FirstName, e.LastName })
    .ToListAsync();

// Avoid - fetches all columns
var allEmployees = await _context.Employees.ToListAsync();
```

### 5. Always Validate Before Saving
```csharp
public async Task<Employee> CreateEmployeeAsync(Employee employee)
{
    // Validate
    if (string.IsNullOrWhiteSpace(employee.FirstName))
        throw new ArgumentException("First name is required");

    if (employee.Salary <= 0)
        throw new ArgumentException("Salary must be positive");

    _context.Employees.Add(employee);
    await _context.SaveChangesAsync();
    return employee;
}
```

### 6. Handle Concurrency
```csharp
public class Employee
{
    public int EmployeeId { get; set; }
    // ... other properties ...

    [Timestamp]  // SQL Server
    public byte[] RowVersion { get; set; }
}

// In OnModelCreating
modelBuilder.Entity<Employee>()
    .Property(e => e.RowVersion)
    .IsRowVersion();

// In update
try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException)
{
    // Handle concurrent modification
}
```

### 7. Use Repository Pattern for Abstraction
```csharp
public interface IEmployeeRepository
{
    Task<Employee> GetByIdAsync(int id);
    Task<List<Employee>> GetAllAsync();
    Task<Employee> CreateAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(int id);
}

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Employee> GetByIdAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.EmployeeId == id);
    }

    // ... implement other methods ...
}
```

---

## Common Mistakes and Solutions

### Mistake 1: Filtering in Memory Instead of Database

```csharp
// BAD - loads all data to memory first
var highSalaryEmployees = _context.Employees
    .ToList()  // PROBLEM: Loads everything
    .Where(e => e.Salary > 80000)
    .ToList();

// GOOD - filters in database
var highSalaryEmployees = await _context.Employees
    .Where(e => e.Salary > 80000)
    .ToListAsync();
```

### Mistake 2: Using Contains with LINQ to Entities on Strings

```csharp
// BAD - may not translate to SQL properly
var searchTerm = "John";
var employees = await _context.Employees
    .Where(e => e.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
    .ToListAsync();

// GOOD - use EF.Functions.Like
var employees = await _context.Employees
    .Where(e => EF.Functions.Like(e.FirstName, $"%{searchTerm}%"))
    .ToListAsync();
```

### Mistake 3: Not Disposing DbContext

```csharp
// BAD
public void GetEmployee(int id)
{
    var context = new ApplicationDbContext();
    var employee = context.Employees.Find(id);
    // context never disposed
}

// GOOD - using dependency injection
public class EmployeeService
{
    private readonly ApplicationDbContext _context;

    public EmployeeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Employee GetEmployee(int id)
    {
        return _context.Employees.Find(id);
    }
    // context disposed by DI container
}

// Or using using statement
public Employee GetEmployee(int id)
{
    using (var context = new ApplicationDbContext())
    {
        return context.Employees.Find(id);
    }
    // context disposed
}
```

### Mistake 4: Lazy Loading Causing N+1 Problem

```csharp
// BAD - causes N+1 queries
var employees = _context.Employees.ToList();
foreach (var emp in employees)
{
    Console.WriteLine($"{emp.FirstName} - {emp.Department.Name}"); // Lazy loads each
}

// GOOD - explicit loading
var employees = await _context.Employees
    .Include(e => e.Department)
    .ToListAsync();

foreach (var emp in employees)
{
    Console.WriteLine($"{emp.FirstName} - {emp.Department.Name}");
}
```

---

## Quick Reference

### Connection Strings

| Database | Connection String |
|----------|-------------------|
| **SQL Server** | `Server=.;Database=MyDb;Trusted_Connection=true;Encrypt=true;` |
| **MySQL** | `Server=localhost;Database=mydb;Uid=root;Pwd=password;` |
| **PostgreSQL** | `Host=localhost;Database=mydb;Username=postgres;Password=password` |
| **Oracle** | `User Id=system;Password=password;Data Source=localhost:1521/orcl;` |

### Common LINQ Methods

| Method | Purpose |
|--------|---------|
| `Where()` | Filter records |
| `Select()` | Project/transform records |
| `OrderBy()` / `OrderByDescending()` | Sort records |
| `GroupBy()` | Group records |
| `Join()` | Combine datasets |
| `Include()` | Load related data |
| `Take()` | Limit result count |
| `Skip()` | Skip records (pagination) |
| `FirstOrDefault()` | Get first record or null |
| `SingleOrDefault()` | Get single record or null |
| `Count()` | Count matching records |
| `Any()` | Check if records exist |
| `All()` | Check if all records match condition |
| `Distinct()` | Remove duplicates |
| `Union()` | Combine two queries |

