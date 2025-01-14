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

// Configure middleware pipeline
app.UseRouting(); // Match request URL to a route.
app.UseAuthentication(); // Validate JWT and set HttpContext.User
app.UseAuthorization(); // Enforce access control rules
app.MapControllers(); // Handle request using appropriate controller/action

app.Run();
