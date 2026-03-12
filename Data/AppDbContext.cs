using Microsoft.EntityFrameworkCore;
using UrlShortener.Models;

namespace UrlShortener.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShortUrl>()
                .HasIndex(x => x.ShortCode)
                .IsUnique();

            modelBuilder.Entity<ShortUrl>()
                .Property(x => x.OriginalUrl)
                .IsRequired();

            modelBuilder.Entity<ShortUrl>()
                .Property(x => x.ShortCode)
                .IsRequired();
        }
    }
}