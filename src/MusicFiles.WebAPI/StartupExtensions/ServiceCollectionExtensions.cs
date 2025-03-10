using System.Text;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MusicFiles.Core.Domain.IdentityEntities;
using MusicFiles.Core.RepositoryContracts;
using MusicFiles.Core.ServiceContracts;
using MusicFiles.Core.Services;
using MusicFiles.Infrastructure.Data;
using MusicFiles.Infrastructure.Repositories;
using MusicFiles.Infrastructure.Services;

namespace MusicFiles.WebAPI.StartupExtensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var credentials = LoadAwsCredentials(configuration
                 .GetRequiredSection("AWS")
                 .GetValue<string>("ProfileName") 
                 ?? throw new InvalidOperationException("Profile name must be configured."));
        
        // File storage
        services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(credentials));
        // S3Service (Infra Service; Singleton) because it depends on IAmazonS3 (Singleton)
        services.AddSingleton<IS3Service>(sp =>
            new S3Service(
                sp.GetRequiredService<IAmazonS3>(),
                configuration.GetRequiredSection("AWS").GetValue<string>("BucketName") 
                ?? throw new InvalidOperationException("Bucket name must be configured.")
            ));
        // FileUploadService (Core Service; Scoped) because it may perform request-specific logic in the future
        services.AddScoped<IFileUploadService, FileUploadService>();
        
        // Data persistence: the entire chain of services is scoped to guarantee thread safety and avoid lifecycle conflicts
        // DbContext (Infra) => Repository (Infra) => DataService (Core)
        services.AddDbContext<MusicFilesDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
                if (environment.IsDevelopment())
                {
                    // useful for troubleshooting the postgres connection.
                    options.EnableSensitiveDataLogging()
                        .EnableDetailedErrors();
                }
            }
        );
        
        // Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                //  The default ASP.NET Identity database migration already applies a unique constraint on the UserName column.
                options.User.RequireUniqueEmail = true;
                // RequiredUniqueChars is the only non-default option here, but I want to be explicit/self-documenting. 
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredUniqueChars = 4;
            })
            .AddEntityFrameworkStores<MusicFilesDbContext>()
            .AddDefaultTokenProviders()
            .AddUserStore<UserStore<ApplicationUser, ApplicationRole, MusicFilesDbContext, Guid>>()
            .AddRoleStore<RoleStore<ApplicationRole, MusicFilesDbContext, Guid>>();
        
        // JWT
        services.AddTransient<IJwtService, JwtService>();

        services.AddScoped<IMusicFilesRepository, MusicFilesRepository>();
        services.AddScoped<IMusicDataService, MusicDataService>();

        services.AddAuthorization(options =>
        {
            // Endpoints will require authentication unless overridden with [AllowAnonymous]
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
        
        // Auth Context
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing");
        var jwtAudience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is missing");
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(5) 
                // ^^ leeway window that accounts for minor differences between clocks on
                // different machines (the server that issues the token vs. the server that validates it)
            };
        });
        
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
        
        return services;
    }
    
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
}

