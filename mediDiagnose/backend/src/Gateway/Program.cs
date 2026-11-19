var builder = WebApplication.CreateBuilder(args);
var serviceUrls = new[]
{
    ("identity", "ServiceUrls:Identity"),
    ("directory", "ServiceUrls:Directory"),
    ("appointments", "ServiceUrls:Appointments")
};
var proxyConfiguration = new Dictionary<string, string?>();
foreach (var (cluster, settingKey) in serviceUrls)
{
    var serviceUrl = builder.Configuration[settingKey]
        ?? throw new InvalidOperationException($"{settingKey.Replace(":", "__")} is not set.");
    if (!Uri.TryCreate(serviceUrl, UriKind.Absolute, out var serviceUri)
        || serviceUri.Scheme is not ("http" or "https"))
    {
        throw new InvalidOperationException($"{settingKey.Replace(":", "__")} must be an absolute HTTP or HTTPS URL.");
    }

    proxyConfiguration[$"ReverseProxy:Clusters:{cluster}:Destinations:primary:Address"] =
        serviceUri.AbsoluteUri.EndsWith('/') ? serviceUri.AbsoluteUri : serviceUri.AbsoluteUri + "/";
}

builder.Configuration.AddInMemoryCollection(proxyConfiguration);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "gateway" }));
app.MapReverseProxy();
app.Run();

public partial class Program;
