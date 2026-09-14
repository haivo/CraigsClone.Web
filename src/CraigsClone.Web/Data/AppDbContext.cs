using CraigsClone.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CraigsClone.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<City> Cities => Set<City>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Listing> Listings => Set<Listing>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<City>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(60).IsRequired();
            e.HasIndex(x => x.Slug).IsUnique();
        });

        b.Entity<Category>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(60).IsRequired();
            // Stored as text ("ForSale") so it's readable in psql.
            e.Property(x => x.Group).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.Slug).IsUnique();
        });

        b.Entity<Listing>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(120).IsRequired();
            e.Property(x => x.Description).IsRequired();
            e.Property(x => x.Price).HasPrecision(10, 2);
            e.Property(x => x.Neighborhood).HasMaxLength(80);
            e.Property(x => x.ContactEmail).HasMaxLength(254).IsRequired();
            // The browse page's query: this city, this category, newest first.
            e.HasIndex(x => new { x.CityId, x.CategoryId, x.CreatedAt }).IsDescending(false, false, true);
            // A city or category with listings can't be deleted.
            e.HasOne(x => x.City).WithMany().OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany().OnDelete(DeleteBehavior.Restrict);
        });
    }
}
