using Invoicing.Data;
using Invoicing.Data.Identity;
using Invoicing.Api.Endpoints;

namespace Invoicing;

public static class OpenIddictServerConfiguration
{
    public static IServiceCollection AddOpenIddictServer(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<InvoicingDbContext>()
                       .ReplaceDefaultEntities<Application, Authorization, Scope, Token, int>();
            })
            .AddServer(options =>
            {
                // Enable the token endpoint.
                options.SetTokenEndpointUris(Routes.Authentication.Token);
                // Enable the code flow.
                options.AllowClientCredentialsFlow();
                // Register the signing and encryption credentials.
                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                // Register the ASP.NET Core host and configure the ASP.NET Core options.
                options.UseAspNetCore()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableTokenEndpointPassthrough();

                options.DisableAccessTokenEncryption();
            })
            // Register the OpenIddict validation components.
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();

            });

        return services;
    }
}
