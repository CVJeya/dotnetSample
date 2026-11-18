using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TaskManagement.API.Data;
using TaskManagement.API.Mappings;
using TaskManagement.API.Middleware;
using TaskManagement.API.Repositories;
using TaskManagement.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Management API",
        Version = "v1",
        Description = "REST API for enterprise task management with CRUD and search.",
        Contact = new OpenApiContact { Name = "Engineering", Email = "engineering@example.com" }
    });

    var xmlName = $"{typeof(Program).Assembly.GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlName);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
        sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));

builder.Services.AddAutoMapper(typeof(TaskMappingProfile).Assembly);

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

var corsSection = "AngularDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsSection, policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:4200" })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

await ApplyEfMigrationsAsync(app);

app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(corsSection);
app.MapControllers();

app.Run();

static async Task ApplyEfMigrationsAsync(WebApplication application)
{
    await using var scope = application.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Database");
    try
    {
        await db.Database.MigrateAsync();
        logger.LogInformation("EF Core migrations applied (database/schema ready).");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex,
            "Database migration failed. Ensure SQL Server is running, " +
            "the connection string Server= name is correct (e.g. (localdb)\\\\mssqllocaldb), " +
            "and your login can create/use database 'taskmanagementdb'.");
        throw;
    }
}
