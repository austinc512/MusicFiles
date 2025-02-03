using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
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
    public AuthenticationResponse CreateJwtToken(ApplicationUser user)
    {
        var expiration =
            DateTimeOffset.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:EXPIRATION_MINUTES"]));
        var claims = new Claim[]
        {
            // subject -- unique value of user
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            // JWT ID
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // Issued At (date/time of token generation)
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                
            // OPTIONALS:
            // Unique name identifier of the user (Email)
            // this one isn't being recognized when we call it in the Controller
            new Claim(ClaimTypes.NameIdentifier, user.Email),
            // name of the user
            // new Claim(ClaimTypes.Name, user.PersonName),
            // so we create this one instead
            new Claim(ClaimTypes.Email, user.Email.ToString()),
        };
        throw new NotImplementedException();
    }
}