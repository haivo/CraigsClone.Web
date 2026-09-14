using CraigsClone.Web.Data;

namespace CraigsClone.Tests.Unit.Data;

// Checks the static sample list only. No database.
[Trait("Category", "Unit")]
public class DevSeederDataTests
{
    [Fact]
    public void HasAtLeastThirtySamples()
    {
        Assert.True(DevSeeder.Samples.Count >= 30, $"only {DevSeeder.Samples.Count} samples");
    }

    // A typo here would be a KeyNotFoundException at startup. Catch it in a millisecond instead.
    [Fact]
    public void EveryCityAndCategorySlug_ExistsInSeedData()
    {
        var cities = DbSeeder.Cities.Select(c => c.Slug).ToHashSet();
        var categories = DbSeeder.Categories.Select(c => c.Slug).ToHashSet();

        Assert.All(DevSeeder.Samples, s =>
        {
            Assert.Contains(s.City, cities);
            Assert.Contains(s.Category, categories);
        });
    }

    // Both display paths (price and "—") get exercised by the sample data.
    [Fact]
    public void HasPricedAndUnpricedSamples()
    {
        Assert.Contains(DevSeeder.Samples, s => s.Price is null);
        Assert.Contains(DevSeeder.Samples, s => s.Price is not null);
    }

    [Fact]
    public void Titles_FitTheColumn()
    {
        Assert.All(DevSeeder.Samples, s => Assert.InRange(s.Title.Length, 1, 120));
    }

    [Fact]
    public void Titles_AreUnique()
    {
        var titles = DevSeeder.Samples.Select(s => s.Title).ToList();
        Assert.Equal(titles.Count, titles.Distinct().Count());
    }
}
