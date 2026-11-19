using BuildingBlocks.Configuration;

var builder = WebApplication.CreateBuilder(args);
DatabaseConnectionSettings.RequireDbUrl(builder.Configuration);

var app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "appointments" }));
app.Run();

public partial class Program;
