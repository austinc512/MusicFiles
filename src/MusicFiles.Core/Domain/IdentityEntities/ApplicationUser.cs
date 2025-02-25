using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MusicFiles.Core.Domain.IdentityEntities;
[Index(nameof(PublicUserId), IsUnique = true)]
public class ApplicationUser : IdentityUser<Guid>
{
    
    public Guid PublicUserId { get; private set; }
    [MaxLength(50)]
    public string? FirstName { get; set; }
    [MaxLength(50)]
    public string? LastName { get; set; }
    [MaxLength(88)]
    public string? RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset RefreshTokenExpiration { get; set; }
    
    // satisfy EF Core
    public ApplicationUser()
    {
        PublicUserId = Guid.NewGuid();
    }
}