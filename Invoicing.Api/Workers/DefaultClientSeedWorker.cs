using OpenIddict.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace Invoicing.Api.Workers;

[ExcludeFromCodeCoverage(Justification = "Development Env only")]
public class DefaultClientSeedWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DefaultClientSeedWorker> _logger;

    public DefaultClientSeedWorker(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<DefaultClientSeedWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        var defaultClients = _configuration.GetSection("DefaultClients").Get<DefaultClient[]>() ?? Array.Empty<DefaultClient>();

        foreach (var client in defaultClients)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation("Creating default client {DisplayName}", client.DisplayName);

            var persistedClient = await applicationManager.FindByClientIdAsync(client.ClientId, cancellationToken);

            if (persistedClient is not null)
            {
                await applicationManager.DeleteAsync(persistedClient);
            }

            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = client.ClientId,
                ClientSecret = client.ClientSecret,
                DisplayName = client.DisplayName,
            };

            descriptor.Permissions.UnionWith(client.Permissions ?? Array.Empty<string>());
            descriptor.Requirements.UnionWith(client.Requirements ?? Array.Empty<string>());

            await applicationManager.CreateAsync(descriptor);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private record DefaultClient(string ClientId, string ClientSecret, string DisplayName)
    {
        public string[]? Permissions { get; init; }
        public string[]? Requirements { get; init; }
    }
}

