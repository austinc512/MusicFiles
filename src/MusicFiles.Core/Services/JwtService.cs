using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MusicFiles.Core.Domain.IdentityEntities;
using MusicFiles.Core.DTOs.Response;
using MusicFiles.Core.ServiceContracts;

namespace MusicFiles.Core.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    // general notes:
    // you can trust the Token for authentication purposes assuming the front end is secure
    // Store JWTs in secure, HTTP-only, same-site cookies
    // enforce HTTPS, HSTS, short-lived access and refresh tokens,
    // device/session tracking (log out user is token is used from unexpected location)
    // long/random Jwt:Key

    // For performance, we want to avoid checking the PublicUserId in the JWT matching against
    // the user's ID in the DB.

    // Hybrid approach (a feature much later down the pipeline): 
    // Store PublicUserId in Redis (or an in-memory cache) when the user logs in.
    // validate JWT token claim against cached value
    // in the event of a cache miss, fall back to DB query
    public AuthenticationResponse CreateJwtToken(ApplicationUser user, List<string> roles)
    {
        
        var expiration = DateTimeOffset.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:EXPIRATION_MINUTES"]));

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.PublicUserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Exp, expiration.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException("JWT secret key is missing in the configuration.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration.UtcDateTime,
            SigningCredentials = credentials,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Optional could go here, something like:
        // "JWT token generated for user {UserId} with expiration at {Expiration}"

        return new AuthenticationResponse
        {
            Token = tokenString,
            Expiration = expiration,
            RefreshToken = GenerateRefreshToken(),
            RefreshTokenExpiration =
                DateTimeOffset.UtcNow.AddMinutes(
                    Convert.ToDouble(_configuration["RefreshToken:EXPIRATION_MINUTES"]))
        };
    }

    public ClaimsPrincipal? GetPrincipalFromJwtToken(string? token)
    {
        var tokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = true,
            ValidAudience = _configuration["Jwt:Audience"],
            ValidateIssuer = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ??
                                                                               throw new InvalidOperationException(
                                                                                   "JWT secret key is missing in the configuration."))),
            ValidateLifetime = false // expect the token to be expired in this context
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    private string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        var randomNumber = RandomNumberGenerator.Create();
        randomNumber.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}