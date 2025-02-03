using MusicFiles.Core.Domain.IdentityEntities;
using MusicFiles.Core.DTOs.Response;

namespace MusicFiles.Core.ServiceContracts;

public interface IJwtService
{
    AuthenticationResponse CreateJwtToken(ApplicationUser user);
}