using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Core.Entities;

namespace UrlShortener.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ShortenedUrl> ShortenedUrls => Set<ShortenedUrl>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ShortenedUrl>(e =>
        {
            e.HasIndex(x => x.OriginalUrl).IsUnique();
            e.HasIndex(x => x.ShortCode).IsUnique();

            e.HasOne(x => x.CreatedBy)
             .WithMany(u => u.ShortenedUrls)
             .HasForeignKey(x => x.CreatedById);
        });
    }
}