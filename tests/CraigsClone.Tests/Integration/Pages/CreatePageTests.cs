using System.Net;
using System.Text.RegularExpressions;
using CraigsClone.Tests.E2E;

namespace CraigsClone.Tests.Integration.Pages;

[Trait("Category", "Integration")]
[Collection("app")]
public class CreatePageTests(WebAppFixture fixture)
{
    // The <select> for one field, including its <option>s.
    private static string Select(string body, string name) =>
        Regex.Match(body, $"<select[^>]*name=\"{name}\"[^>]*>(.*?)</select>", RegexOptions.Singleline).Groups[1].Value;

    private static int OptionCount(string select) => Regex.Matches(select, "<option").Count - 1;   // minus the placeholder

    [Fact]
    public async Task GetPost_HasTokenAndBothDropdowns()
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync("/post");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("__RequestVerificationToken", body);
        Assert.Equal(6, OptionCount(Select(body, "CityId")));
        Assert.Equal(22, OptionCount(Select(body, "CategoryId")));
    }

    [Fact]
    public async Task GetPost_WithCity_PreselectsIt()
    {
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync("/post?city=austin");

        var citySelect = Select(body, "CityId");
        Assert.Matches("<option selected=\"selected\" value=\"\\d+\">Austin</option>", citySelect);
    }

    [Fact]
    public async Task GetPost_WithUnknownCity_IsFineWithNothingSelected()
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync("/post?city=nowhere");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("selected", Select(body, "CityId"));
    }
}
