using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MusicFiles.Core.ServiceContracts;

namespace MusicFiles.Core.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IEnumerable<Claim>? Claims => _httpContextAccessor.HttpContext?.User.Claims;
    

    public string? PublicUserId => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // If other user-related data is added to the token later on, add that data here.

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}