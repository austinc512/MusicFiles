using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicFiles.Core.RepositoryContracts;
using MusicFiles.Core.ServiceContracts;
using MusicFiles.Core.Services;
using MusicFiles.Infrastructure.Data;
using MusicFiles.Infrastructure.Services;
using MusicFiles.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

var credentials = LoadAwsCredentials(
    builder.Configuration
        .GetRequiredSection("AWS")
        .GetValue<string>("ProfileName") 
    ?? throw new InvalidOperationException("Profile name must be configured.")
);

// File storage
builder.Services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(credentials));
// S3Service (Infra Service; Singleton) because it depends on IAmazonS3 (Singleton)
builder.Services.AddSingleton<IS3Service>(sp =>
    new S3Service(
        sp.GetRequiredService<IAmazonS3>(),
        builder.Configuration.GetRequiredSection("AWS").GetValue<string>("BucketName") 
        ?? throw new InvalidOperationException("Bucket name must be configured.")
    ));
// FileUploadService (Core Service; Scoped) because it may perform request-specific logic in the future
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// check environment
var environment = builder.Environment;

// Data persistence
// DbContext's ServiceLifetime is Scoped by default
// DbContext (Infra) => Repository (Infra) => DataService (Core)
// Making all of these scoped guarantees thread safety and avoids lifecycle conflicts
builder.Services.AddDbContext<MusicFilesDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        if (environment.IsDevelopment())
        {
            // useful for troubleshooting the postgres connection.
            options.EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        }
    }
    );

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<MusicFilesDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<IMusicFilesRepository, MusicFilesRepository>();
builder.Services.AddScoped<IMusicDataService, MusicDataService>();

builder.Services.AddAuthorization(options =>
{
// Endpoints will require authentication unless overridden with [AllowAnonymous]
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});


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

// Helper method to load AWS credentials
static AWSCredentials LoadAwsCredentials(string profileName)
{
    var credentialsFile = new SharedCredentialsFile();
    if (credentialsFile.TryGetProfile(profileName, out var profile))
    {
        AWSCredentialsFactory.TryGetAWSCredentials(profile, credentialsFile, out var credentials);
        return credentials;
    }
    else
    {
        throw new Exception($"Profile {profileName} not found in credentials file.");
    }
}