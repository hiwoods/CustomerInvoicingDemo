using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Invoicing.Api.Test;

[Collection(InvoicingTestCollection.Name)]
public class AuthenticationApiTest
{
    private readonly TestServerFixture _serverFixture;

    public AuthenticationApiTest(TestServerFixture serverFixture) => _serverFixture = serverFixture;

    [Fact(DisplayName = "Get Token")]
    public async Task Access_token_should_be_retrieved_without_error()
    {
        //arrange
        string clientId = Guid.NewGuid().ToString();
        string clientSecret = Guid.NewGuid().ToString();

        //register an application that can authenticate
        await CreateApplicationClient(clientId, clientSecret);

        //create a httpClient that will hit our Web App
        using var httpClient = _serverFixture.WebApp.CreateClient();

        var parmeters = new Dictionary<string, string>
        {
            {"grant_type", "client_credentials" },
            {"client_id", clientId },
            {"client_secret", clientSecret }
        };

        using var requestContent = new FormUrlEncodedContent(parmeters);

        //act
        var responseMessage = await httpClient.PostAsync("/api/token", requestContent);

        //assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.OK, because: "Token should be retrieved");
        var content = await responseMessage.Content.ReadAsStringAsync();

        var deserialized = JsonSerializer.Deserialize<OAuthResponse>(content);
        Assert.NotNull(deserialized);

        var responseWithoutToken = await httpClient.GetAsync("/api/customers/1");
        responseWithoutToken.StatusCode.Should().Be(HttpStatusCode.Unauthorized, because: "request doesn't have token");

        //token should allow access
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(deserialized.TokenType, deserialized.AccessToken);

        var responseWithToken = await httpClient.GetAsync("/api/customers/1");
        responseWithToken.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized, because: "token should allow access");
    }

    [Fact(DisplayName = "Invalid Credentials")]
    public async Task Invaid_credentials_should_return_400()
    {
        var parmeters = new Dictionary<string, string>
        {
            {"grant_type", "client_credentials" },
            {"client_id", Guid.NewGuid().ToString() },
            {"client_secret",  Guid.NewGuid().ToString() }
        };

        using var client = _serverFixture.WebApp.CreateClient();
        using var requestContent = new FormUrlEncodedContent(parmeters);

        var responseMessage = await client.PostAsync("/api/token", requestContent);

        responseMessage.IsSuccessStatusCode.Should().BeFalse();
    }

    private async Task CreateApplicationClient(string clientId, string clientSecret)
    {
        await using var scope = _serverFixture.ServiceProvider.CreateAsyncScope();

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        var application = await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            DisplayName = "AuthenticationTest Client",
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.ClientCredentials
            }
        });
    }
}
