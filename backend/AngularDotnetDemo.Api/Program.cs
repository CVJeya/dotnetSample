using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AngularDotnetDemo.Api.Data;
using AngularDotnetDemo.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ── Config (all secrets come from environment variables in production) ──
var jwtKey      = builder.Configuration["Jwt:Key"]      ?? throw new InvalidOperationException("Jwt__Key is not set");
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]   ?? "AngularDotnetDemo";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AngularDotnetDemoClient";
if (jwtKey.Length < 32) throw new InvalidOperationException("Jwt__Key must be at least 32 characters");

// DATABASE_URL may be a Neon URI (postgresql://user:pass@host/db?sslmode=require) or a normal Npgsql string
var dbUrl = builder.Configuration["DATABASE_URL"] ?? throw new InvalidOperationException("DATABASE_URL is not set");
string ToNpgsql(string s)
{
    if (!s.StartsWith("postgres")) return s;
    var u = new Uri(s);
    var parts = u.UserInfo.Split(':', 2);
    return $"Host={u.Host};Port={(u.Port > 0 ? u.Port : 5432)};Database={u.AbsolutePath.TrimStart('/')};" +
           $"Username={Uri.UnescapeDataString(parts[0])};Password={Uri.UnescapeDataString(parts[1])};" +
           "SSL Mode=Require;Trust Server Certificate=true";
}
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(ToNpgsql(dbUrl)));

builder.Services.AddControllers();

// CORS: only the origins you list in ALLOWED_ORIGINS (comma separated). Not needed if Netlify proxies /api.
var origins = (builder.Configuration["ALLOWED_ORIGINS"] ?? "http://localhost:5173")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});

var app = builder.Build();

// ── Create/upgrade schema and seed demo data on startup ──
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new User { Name = "Alice Johnson", Email = "alice@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), CreatedAt = DateTime.UtcNow },
            new User { Name = "Bob Smith",     Email = "bob@example.com",   PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), CreatedAt = DateTime.UtcNow });
        db.Products.AddRange(
            new Product { Name = "Laptop", Description = "High performance laptop", Price = 1200.00m, CreatedBy = "Alice Johnson", CreatedAt = DateTime.UtcNow },
            new Product { Name = "Mouse",  Description = "Wireless mouse",          Price = 25.50m,   CreatedBy = "Bob Smith",     CreatedAt = DateTime.UtcNow });
        db.SaveChanges();
    }
}

app.UseForwardedHeaders();
if (app.Environment.IsDevelopment()) app.MapOpenApi();   // HTTPS redirect removed: the host terminates TLS
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ──────────── Validation helper (DTO attributes) ────────────
static Dictionary<string, string[]>? Validate<T>(T dto) where T : notnull
{
    var results = new List<ValidationResult>();
    if (Validator.TryValidateObject(dto, new ValidationContext(dto), results, validateAllProperties: true)) return null;
    return results.GroupBy(r => r.MemberNames.FirstOrDefault() ?? "").ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage ?? "Invalid").ToArray());
}

// ──────────── Auth (public) ────────────
app.MapPost("/api/auth/register", async (RegisterRequest req, AppDbContext db) =>
{
    if (Validate(req) is { } errs) return Results.ValidationProblem(errs);
    var email = req.Email.Trim().ToLowerInvariant();
    if (await db.Users.AnyAsync(u => u.Email == email))
        return Results.BadRequest(new { message = "Email already registered" });

    var user = new User { Name = req.Name.Trim(), Email = email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password), CreatedAt = DateTime.UtcNow };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(new { token = GenerateJwt(user), user = new { user.Id, user.Name, user.Email } });
});

app.MapPost("/api/auth/login", async (LoginRequest req, AppDbContext db) =>
{
    if (Validate(req) is { } errs) return Results.ValidationProblem(errs);
    var email = req.Email.Trim().ToLowerInvariant();
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);   // parameterized by EF Core
    if (user is null || !Verify(req.Password, user.PasswordHash)) return Results.Unauthorized();
    return Results.Ok(new { token = GenerateJwt(user), user = new { user.Id, user.Name, user.Email } });
});

// ──────────── Users (now protected) ────────────
var users = app.MapGroup("/api/users").RequireAuthorization();

users.MapGet("/", async (AppDbContext db) =>
    Results.Ok(await db.Users.AsNoTracking().Select(u => new { u.Id, u.Name, u.Email, u.CreatedAt }).ToListAsync()));

users.MapGet("/me", async Task<IResult> (ClaimsPrincipal principal, AppDbContext db) =>
{
    if (!int.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        return Results.Unauthorized();

    var user = await db.Users.AsNoTracking()
        .Where(u => u.Id == userId)
        .Select(u => new { u.Id, u.Name, u.Email, u.CreatedAt })
        .FirstOrDefaultAsync();

    return user is null ? Results.NotFound() : Results.Ok(user);
});

users.MapGet("/{id:int}", async (int id, AppDbContext db) =>
    await db.Users.AsNoTracking().Where(u => u.Id == id).Select(u => new { u.Id, u.Name, u.Email, u.CreatedAt }).FirstOrDefaultAsync()
        is { } u ? Results.Ok(u) : Results.NotFound());

users.MapPost("/", async (UserRequest req, AppDbContext db) =>
{
    if (Validate(req) is { } errs) return Results.ValidationProblem(errs);
    var email = req.Email.Trim().ToLowerInvariant();
    if (await db.Users.AnyAsync(u => u.Email == email)) return Results.BadRequest(new { message = "Email already registered" });
    // No password given here: this account cannot log in until a password flow is added
    var user = new User { Name = req.Name.Trim(), Email = email, PasswordHash = "", CreatedAt = DateTime.UtcNow };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/api/users/{user.Id}", new { user.Id, user.Name, user.Email, user.CreatedAt });
});

users.MapPut("/{id:int}", async (int id, UserRequest req, AppDbContext db) =>
{
    if (Validate(req) is { } errs) return Results.ValidationProblem(errs);
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();
    var email = req.Email.Trim().ToLowerInvariant();
    if (await db.Users.AnyAsync(u => u.Email == email && u.Id != id)) return Results.BadRequest(new { message = "Email already registered" });
    user.Name = req.Name.Trim();
    user.Email = email;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

users.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();
    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ──────────── Products (protected) ────────────
var products = app.MapGroup("/api/products").RequireAuthorization();

products.MapGet("/", async (AppDbContext db) => Results.Ok(await db.Products.AsNoTracking().ToListAsync()));

products.MapGet("/{id:int}", async (int id, AppDbContext db) =>
    await db.Products.FindAsync(id) is { } p ? Results.Ok(p) : Results.NotFound());

products.MapPost("/", async (HttpContext ctx, ProductRequest req, AppDbContext db) =>
{
    if (Validate(req) is { } errs) return Results.ValidationProblem(errs);
    var p = new Product
    {
        Name = req.Name.Trim(), Description = req.Description?.Trim() ?? "", Price = req.Price,
        CreatedBy = ctx.User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown", CreatedAt = DateTime.UtcNow
    };
    db.Products.Add(p);
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{p.Id}", p);
});

products.MapPut("/{id:int}", async (int id, ProductRequest req, AppDbContext db) =>
{
    if (Validate(req) is { } errs) return Results.ValidationProblem(errs);
    var p = await db.Products.FindAsync(id);
    if (p is null) return Results.NotFound();
    p.Name = req.Name.Trim(); p.Description = req.Description?.Trim() ?? ""; p.Price = req.Price;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

products.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var p = await db.Products.FindAsync(id);
    if (p is null) return Results.NotFound();
    db.Products.Remove(p);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

// ──────────── Helpers ────────────
string GenerateJwt(User user)
{
    var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), SecurityAlgorithms.HmacSha256);
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Email, user.Email)
    };
    var token = new JwtSecurityToken(jwtIssuer, jwtAudience, claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: creds);
    return new JwtSecurityTokenHandler().WriteToken(token);
}

static bool Verify(string password, string hash)
{
    try { return !string.IsNullOrEmpty(hash) && BCrypt.Net.BCrypt.Verify(password, hash); }
    catch { return false; }
}

// ──────────── Request DTOs (only these fields can be set by clients) ────────────
record RegisterRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Name,
    [property: Required, EmailAddress, StringLength(200)] string Email,
    [property: Required, StringLength(100, MinimumLength = 8)] string Password);

record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password);

record UserRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Name,
    [property: Required, EmailAddress, StringLength(200)] string Email);

record ProductRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Name,
    [property: StringLength(1000)] string? Description,
    [property: Range(0, 1000000000)] decimal Price);
