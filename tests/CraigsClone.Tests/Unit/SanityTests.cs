namespace CraigsClone.Tests.Unit;

[Trait("Category", "Unit")]
public class SanityTests
{
    [Fact]
    public void Runner_Works() => Assert.Equal(2, 1 + 1);
}
