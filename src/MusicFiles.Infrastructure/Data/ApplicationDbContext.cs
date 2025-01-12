using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MusicFiles.Core.Domain.Entities;
using MusicFiles.Core.Domain.IdentityEntities;

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
    }
}
