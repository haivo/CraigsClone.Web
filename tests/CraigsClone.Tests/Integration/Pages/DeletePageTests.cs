using System.Net;
using CraigsClone.Tests.E2E;

namespace CraigsClone.Tests.Integration.Pages;

[Trait("Category", "Integration")]
[Collection("app")]
public class DeletePageTests(WebAppFixture fixture)
{
    // Pins that delete is a POST form with a confirm, not a link.
    [Fact]
    public async Task Details_HasDeleteForm_WithConfirm()
    {
        var listing = await TestData.AddListingAsync(fixture.Services);
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync($"/listing/{listing.Id}");

        Assert.Contains($"<form method=\"post\" action=\"/listing/{listing.Id}/delete\"", body);
        Assert.Contains("confirm(", body);
        Assert.DoesNotContain($"<a href=\"/listing/{listing.Id}/delete\"", body);

        // The token must be INSIDE the delete form, or the global filter rejects the POST with 400.
        var deleteForm = System.Text.RegularExpressions.Regex.Match(body,
            $"<form method=\"post\" action=\"/listing/{listing.Id}/delete\".*?</form>",
            System.Text.RegularExpressions.RegexOptions.Singleline).Value;
        Assert.Contains("__RequestVerificationToken", deleteForm);
    }

    // A link (GET) can't delete anything.
    [Fact]
    public async Task GetDelete_IsNotAllowed()
    {
        var listing = await TestData.AddListingAsync(fixture.Services);
        using var client = fixture.CreateClient();

        var response = await client.GetAsync($"/listing/{listing.Id}/delete");

        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed });
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/listing/{listing.Id}")).StatusCode);   // still there
    }
}
