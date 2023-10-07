using Invoicing.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Invoicing.Api.Workers;

[ExcludeFromCodeCoverage(Justification = "Development Env only")]
public class MigrationWorkder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationWorkder> _logger;

    public MigrationWorkder(IServiceProvider serviceProvider, ILogger<MigrationWorkder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Database migration started");

        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        _logger.LogInformation("Database migration completed");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
