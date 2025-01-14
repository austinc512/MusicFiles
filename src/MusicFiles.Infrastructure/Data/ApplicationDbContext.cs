using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MusicFiles.Core.Domain.Entities;
using MusicFiles.Core.Domain.IdentityEntities;
using MusicFiles.Core.Enums;

namespace MusicFiles.Infrastructure.Data;

public class MusicFilesDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public DbSet<MusicFileEntity> MusicFiles { get; set; }

    public MusicFilesDbContext(DbContextOptions<MusicFilesDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Troubleshooting
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<MusicFileEntity>(entity =>
        {
            entity.ToTable("MusicFiles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserPublicId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.S3Key).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.MediaType).HasConversion<int>().IsRequired();
            entity.Property(e => e.LastModified).IsRequired();
        });
        
        // Seed roles using ApplicationRole and UserTypeOptions enum
        modelBuilder.Entity<ApplicationRole>().HasData(
            new ApplicationRole
            {
                // https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes#pending-model-changes
                // using Guid.Parse() instead of Guid.NewGuid()
                // alternatively, you could use new seeding pattern:
                // https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/whatsnew#improved-data-seeding
                Id = Guid.Parse("809b96c3-0336-451f-ba31-0281e3d9a441"), // Generate a new GUID for each role
                Name = UserTypeOptions.Admin.ToString(),
                NormalizedName = UserTypeOptions.Admin.ToString().ToUpper()
            },
            new ApplicationRole
            {
                Id = Guid.Parse("f7a45b47-a364-4e8e-9c00-e6ffe194b262"), 
                Name = UserTypeOptions.Customer.ToString(),
                NormalizedName = UserTypeOptions.Customer.ToString().ToUpper()
            },
            new ApplicationRole
            {
                Id = Guid.Parse("93ca2452-0682-4052-b179-f0bd5b1cc918"), 
                Name = UserTypeOptions.Publisher.ToString(),
                NormalizedName = UserTypeOptions.Publisher.ToString().ToUpper()
            }
        );
    }
}
