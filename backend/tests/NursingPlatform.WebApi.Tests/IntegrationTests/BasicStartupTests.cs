namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class BasicStartupTests
{
    private readonly WebApiTestFactory _factory;

    public BasicStartupTests(WebApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Factory_CanCreateClient()
    {
        using var client = _factory.CreateClient();
        Assert.NotNull(client);
    }
}
