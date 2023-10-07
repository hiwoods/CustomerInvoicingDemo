using Invoicing.Core;
using Invoicing.Core.Primitives;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Invoicing.Api.Middleware;

public static class ExceptionHandlerMiddleware
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static IApplicationBuilder UseInvoicingExceptionHandler(this IApplicationBuilder app, IHostEnvironment environment)
    {
        return app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (InvoicingErrorException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/problem+json";

                var problemDetails = new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = StatusCodes.Status400BadRequest,
                    Extensions =
                    {
                        ["errors"] = ex.Error is ManyErrors errors ? errors.Errors : new [] { ex.Error },
                    }
                };

                if (environment.IsDevelopment())
                {
                    problemDetails.Extensions.Add("exception", ex.Message);
                    problemDetails.Extensions.Add("stackTrace", ex.StackTrace);
                }

                var result = JsonSerializer.Serialize(problemDetails, _serializerOptions);
                await context.Response.WriteAsync(result);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var problemDetails = new ProblemDetails
                {
                    Title = "An unexpected error occurred",
                    Status = StatusCodes.Status500InternalServerError,
                };

                if (environment.IsDevelopment())
                {
                    problemDetails.Extensions.Add("exception", ex.Message);
                    problemDetails.Extensions.Add("stackTrace", ex.StackTrace);
                }

                var result = JsonSerializer.Serialize(problemDetails, _serializerOptions);
                await context.Response.WriteAsync(result);
            }
        });
    }
}
