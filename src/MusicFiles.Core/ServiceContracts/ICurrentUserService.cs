using System.Security.Claims;

namespace MusicFiles.Core.ServiceContracts;

public interface ICurrentUserService
{
    string? PublicUserId { get; }
    // If other user-related data is added to the token later on, add that data here.
    /// <summary>
    /// Claims in this context can be used for debugging
    /// </summary>
    public IEnumerable<Claim>? Claims { get; }
    bool IsAuthenticated { get; }
}