using Microsoft.AspNetCore.Identity;

namespace MusicFiles.Core.Domain.IdentityEntities;

public class ApplicationRole : IdentityRole<Guid>
{
    // satisfy EF Core
    public ApplicationRole(){}
    
}