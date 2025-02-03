namespace MusicFiles.Core.DTOs.Response;

public class AuthenticationResponse
{
    public string? PersonName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Token { get; set; } = string.Empty;
    public DateTimeOffset Expiration { get; set; }
    public string? RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset RefreshTokenExpirationDateTime { get; set; }
}