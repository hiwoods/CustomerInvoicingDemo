using Microsoft.AspNetCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Invoicing.Api.Endpoints;

public static class AuthenticationApi
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost(Routes.Authentication.Token, ExchangeToken)
               .AllowAnonymous()
               .WithOpenApi(c => new OpenApiOperation(c)
               {
                   OperationId = "Token",
                   Tags = new[] { new OpenApiTag { Name = "Authentication" } },
                   RequestBody = new OpenApiRequestBody
                   {
                       Content =
                       {
                           ["application/x-www-form-urlencoded"] = new OpenApiMediaType
                           {
                               Schema = new OpenApiSchema
                               {
                                   Type = "object",
                                   Properties =
                                   {
                                       ["client_id"] = new OpenApiSchema { Type = "string", },
                                       ["client_secret"] = new OpenApiSchema { Type = "string" },
                                       ["grant_type"] = new OpenApiSchema { Type = "string", Default = new OpenApiString("client_credentials") }
                                   },
                                   Required = new HashSet<string> { "client_id", "client_secret", "grant_type" }
                               }
                           }
                       }
                   },
                   Responses = new OpenApiResponses
                   {
                       ["200"] = new OpenApiResponse
                       {
                           Description = "Successful authentication",
                           Content =
                           {
                               ["application/json"] = new OpenApiMediaType
                               {
                                   Schema = new OpenApiSchema
                                   {
                                       Type = "object",
                                       Properties =
                                       {
                                           ["access_token"] = new OpenApiSchema { Type = "string" },
                                           ["token_type"] = new OpenApiSchema { Type = "string" },
                                           ["expires_in"] = new OpenApiSchema { Type = "number", Format = "int32" }
                                       }
                                   }
                               }
                           }
                       }
                   }
               });

        return builder;
    }

    public static async Task<IResult> ExchangeToken(HttpContext context, IOpenIddictApplicationManager applicationManager)
    {
        var request = context.GetOpenIddictServerRequest();

        if (request is null || !request.IsClientCredentialsGrantType())
        {
            throw new NotImplementedException("The specified grant is not implemented.");
        }

        var application = await applicationManager.FindByClientIdAsync(request.ClientId!) ??
            throw new InvalidOperationException("The application cannot be found.");

        var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

        // Use the client_id as the subject identifier.
        identity.SetClaim(Claims.Subject, await applicationManager.GetClientIdAsync(application));
        identity.SetClaim(Claims.Name, await applicationManager.GetDisplayNameAsync(application));

        identity.SetDestinations(static claim => claim.Type switch
        {
            Claims.Name when claim.Subject!.HasScope(Scopes.Profile)
                => new[] { Destinations.AccessToken, Destinations.IdentityToken },
            _ => new[] { Destinations.AccessToken }
        });

        return Results.SignIn(new ClaimsPrincipal(identity), authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
