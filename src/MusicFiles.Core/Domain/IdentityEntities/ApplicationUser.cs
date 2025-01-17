using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MusicFiles.Core.Domain.IdentityEntities;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid PublicUserId { get; private set; }
    [MaxLength(50)]
    public string? FirstName { get; set; }
    [MaxLength(50)]
    public string? LastName { get; set; }
    
    // satisfy EF Core
    public ApplicationUser()
    {
        PublicUserId = Guid.NewGuid();
    }
    
    // I don't necessarily need this constructor
    // I was just using an object initializer with the base class before
    // public ApplicationUser(Guid publicUserId, string firstName, string lastName, string userName)
    // {
    //     PublicUserId = publicUserId;
    //     FirstName = firstName;
    //     LastName = lastName;
    //     base.UserName = userName;
    // }
}