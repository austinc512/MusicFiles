namespace MusicFiles.Core.DTOs.Response;

public class AuthenticationResponse
{
    // these should be relegated to a second API request that occurs once a user is already authentication,
    // not in first authentication response after login:
    // public string? PersonName { get; set; } = string.Empty;
    // public string? Email { get; set; } = string.Empty;
    public string? Token { get; set; } = string.Empty;
    public DateTimeOffset Expiration { get; set; }
    // still need to implement
    public string? RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset RefreshTokenExpiration { get; set; }
}