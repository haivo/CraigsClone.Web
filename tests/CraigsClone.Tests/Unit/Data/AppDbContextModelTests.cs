using CraigsClone.Web.Data;
using CraigsClone.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CraigsClone.Tests.Unit.Data;

// Inspects the EF model only. The context is never opened, so no database is needed.
[Trait("Category", "Unit")]
public class AppDbContextModelTests
{
    // The design-time model keeps details (like index direction) that the
    // read-optimized runtime model in db.Model leaves out.
    private static IModel BuildModel()
    {
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=unused").Options);
        return db.GetService<IDesignTimeModel>().Model;
    }

    private static IEntityType Entity<T>(IModel model) =>
        model.FindEntityType(typeof(T)) ?? throw new Xunit.Sdk.XunitException($"{typeof(T).Name} not in model");

    [Fact]
    public void ListingPrice_IsNumeric10_2()
    {
        var price = Entity<Listing>(BuildModel()).FindProperty(nameof(Listing.Price))!;

        Assert.Equal(10, price.GetPrecision());
        Assert.Equal(2, price.GetScale());
    }

    [Fact]
    public void ListingTitle_MaxLength120()
    {
        var title = Entity<Listing>(BuildModel()).FindProperty(nameof(Listing.Title))!;

        Assert.Equal(120, title.GetMaxLength());
    }

    [Theory]
    [InlineData(typeof(City))]
    [InlineData(typeof(Category))]
    public void Slug_HasUniqueIndex(Type entityType)
    {
        var entity = BuildModel().FindEntityType(entityType)!;

        var index = entity.GetIndexes().Single(i => i.Properties.Select(p => p.Name).SequenceEqual(["Slug"]));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Listing_HasBrowseIndex()
    {
        var entity = Entity<Listing>(BuildModel());

        var index = entity.GetIndexes().SingleOrDefault(i =>
            i.Properties.Select(p => p.Name).SequenceEqual(["CityId", "CategoryId", "CreatedAt"]));

        Assert.NotNull(index);
        Assert.Equal([false, false, true], index.IsDescending);
    }

    [Fact]
    public void CategoryGroup_IsStoredAsText()
    {
        var group = Entity<Category>(BuildModel()).FindProperty(nameof(Category.Group))!;

        Assert.Equal(typeof(string), group.GetProviderClrType());
        Assert.StartsWith("character varying", group.GetColumnType());
    }
}
