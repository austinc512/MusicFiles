using MusicFiles.WebAPI.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);

// startup extensions:
builder.Services.ConfigureServices(builder.Configuration, builder.Environment);


// Register services for Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configure middleware pipeline
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

app.Run();
