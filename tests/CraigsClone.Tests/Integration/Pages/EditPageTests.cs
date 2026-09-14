using System.Net;
using System.Text.RegularExpressions;
using CraigsClone.Tests.E2E;

namespace CraigsClone.Tests.Integration.Pages;

[Trait("Category", "Integration")]
[Collection("app")]
public class EditPageTests(WebAppFixture fixture)
{
    [Fact]
    public async Task GetEdit_PrefillsTitleAndCity()
    {
        var listing = await TestData.AddListingAsync(fixture.Services, "chicago", "free");
        using var client = fixture.CreateClient();

        var response = await client.GetAsync($"/listing/{listing.Id}/edit");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"value=\"{listing.Title}\"", body);
        var citySelect = Regex.Match(body, "<select[^>]*name=\"CityId\"[^>]*>(.*?)</select>", RegexOptions.Singleline).Groups[1].Value;
        Assert.Matches("<option selected=\"selected\" value=\"\\d+\">Chicago</option>", citySelect);
    }

    [Fact]
    public async Task GetEdit_UnknownId_Is404()
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync("/listing/999999/edit");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Details_HasEditLink()
    {
        var listing = await TestData.AddListingAsync(fixture.Services);
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync($"/listing/{listing.Id}");

        Assert.Contains($"href=\"/listing/{listing.Id}/edit\"", body);
    }
}
