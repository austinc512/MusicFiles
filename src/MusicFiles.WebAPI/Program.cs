using System.Text.Json.Serialization;
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

var app = builder.Build();

// Enforce HTTPS redirection and HSTS.
// HSTS is disabled in Development to avoid caching of HSTS settings on localhost
if (!app.Environment.IsDevelopment())
{
    app.UseHsts(); // Add HSTS for production environments
}
app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS

// Configure middleware pipeline
app.UseRouting(); // Match request URL to a route.
app.UseCors();
app.UseAuthentication(); // Validate JWT and set HttpContext.User
app.UseAuthorization(); // Enforce access control rules
app.MapControllers(); // Handle request using appropriate controller/action

app.Run();
