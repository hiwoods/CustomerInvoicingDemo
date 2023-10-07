using Bogus;
using Invoicing.Api.Contracts;
using Invoicing.Api.Endpoints;
using Invoicing.Core;
using Invoicing.Core.Externals;
using Invoicing.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Testcontainers.MsSql;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Invoicing.Api.Test;

public class TestServerFixture : IAsyncLifetime
{
    private MsSqlContainer? _msSqlContainer;
    private WebApplicationFactory<Program>? _webApp;

    public string ClientId { get; private set; } = Guid.NewGuid().ToString();
    public string ClientSecret { get; private set; } = Guid.NewGuid().ToString();
    private string? CachedAccessToken { get; set; }

    public async Task InitializeAsync()
    {
        Randomizer.Seed = new Random(8675309);

        //db container
        _msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithCleanUp(true)
            .WithAutoRemove(true)
            .Build();

        await _msSqlContainer.StartAsync();

        //create app
        _webApp = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("FunctionalTest");

            var configuration =
                new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        { "ConnectionStrings:DefaultConnection", _msSqlContainer.GetConnectionString() },
                    })
                    .Build();

            builder.UseConfiguration(configuration);

            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(sp => NSubstitute.Substitute.For<IPaymentGateway>());
            });
        });

        //set to https
        _webApp.Server.BaseAddress = new Uri("https://localhost/");
        _webApp.ClientOptions.BaseAddress = new Uri("https://localhost/");

        //migrate
        await using var scope = _webApp.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
        await dbContext.Database.MigrateAsync();

        //create default client

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        var application = await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            DisplayName = "AuthenticationTest Client",
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.ClientCredentials
            }
        });
    }

    public Task DisposeAsync()
    {
        var disposeWebApp = _webApp?.DisposeAsync().AsTask() ?? Task.CompletedTask;
        var disposeContainer = _msSqlContainer?.DisposeAsync().AsTask() ?? Task.CompletedTask;
        return Task.WhenAll(disposeWebApp, disposeContainer);
    }

    public WebApplicationFactory<Program> WebApp => _webApp ?? throw new InvalidOperationException("Server was not set up correctly");

    public IServiceProvider ServiceProvider => _webApp?.Services ?? throw new InvalidOperationException("Server was not set up correctly");

    public async Task<HttpClient> CreateAuthenticatedClient()
    {
        var client = WebApp.CreateClient();

        if (CachedAccessToken is null)
        {
            var parmeters = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials" },
                {"client_id", ClientId },
                {"client_secret", ClientSecret }
            };

            using var requestContent = new FormUrlEncodedContent(parmeters);

            var responseMessage = await client.PostAsync(Routes.Authentication.Token, requestContent);

            var content = await responseMessage.Content.ReadAsStringAsync();
            var deserialized = JsonSerializer.Deserialize<OAuthResponse>(content);
            Assert.NotNull(deserialized);
            CachedAccessToken = deserialized.AccessToken;
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CachedAccessToken);

        return client;
    }

    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.ScrubMembers<InvoiceModel>(x => x.InvoiceId, x => x.CustomerId);
        VerifierSettings.ScrubMembers<Invoice>(x => x.InvoiceId, x => x.CustomerId);
        VerifierSettings.ScrubMembers<Address>(x => x.AddressId);
        VerifierSettings.ScrubMembers<Customer>(x => x.CustomerId);
    }
}
