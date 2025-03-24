using System.Text.Json.Serialization;
using MusicFiles.Observability;
using MusicFiles.WebAPI.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);

// startup extensions:
builder.Services.ConfigureServices(builder.Configuration, builder.Environment);

// Register services for Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // allows enum values to be passed via API as their string representations
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });;

// CORS localhost
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
        policyBuilder.WithOrigins(origins!) // React App port number
            .WithHeaders("Authorization", "origin", "accept", "content-type") // allow additional headers
            .WithMethods("GET", "POST", "PUT", "DELETE");
    });
});

// Configure logging
builder.Logging.ConfigureLogging();

// Configure OpenTelemetry tracing
builder.Services.ConfigureTracing();

var app = builder.Build();

// Enforce HTTPS redirection and HSTS.
// HSTS can be disabled in Development to avoid caching of HSTS settings on localhost
// if (!app.Environment.IsDevelopment())
// {
//     app.UseHsts(); // Add HSTS for production environments
// }

// I think I'm just going to keep this in development too.
// once I get to the front end, maybe I just to all my testing in incognito to avoid caching, use cmd + shit + r to refresh the page.
// I'll worry about that later.
app.UseHsts();
app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS

// Configure middleware pipeline
app.UseRouting(); // Match request URL to a route.
app.UseCors();
app.UseAuthentication(); // Validate JWT and set HttpContext.User
app.UseAuthorization(); // Enforce access control rules
app.MapControllers(); // Handle request using appropriate controller/action

app.Run();
