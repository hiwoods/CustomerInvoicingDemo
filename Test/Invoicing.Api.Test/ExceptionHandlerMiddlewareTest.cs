using Invoicing.Api.Middleware;
using Invoicing.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Invoicing.Api.Test;

[UsesVerify]
public class ExceptionHandlerMiddlewareTest
{
    [Fact]
    public async Task HandleExpectedException()
    {
        using var webApp = CreateWebApp();

        using var client = webApp.CreateDefaultClient();

        var response = await client.GetAsync("/invoicingException");
        var content = await response.Content.ReadAsStringAsync();
        await VerifyJson(content);
    }

    [Fact]
    public async Task HandleUnexpectedException()
    {
        using var webApp = CreateWebApp();

        using var client = webApp.CreateDefaultClient();

        var response = await client.GetAsync("/unknownexception");
        var content = await response.Content.ReadAsStringAsync();
        await VerifyJson(content);
    }

    private static WebApplicationFactory<Program> CreateWebApp()
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(buidler =>
        {
            buidler.UseEnvironment("FunctionalTest");

            buidler.Configure((context, app) =>
            {
                app.UseInvoicingExceptionHandler(context.HostingEnvironment);

                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/invoicingException", c => throw new InvoicingErrorException(new Core.Primitives.Error("TestCode", "invoicingException endpoint is bad!")));
                    endpoints.MapGet("/unknownexception", c => throw new InvalidOperationException("Unexpected error occured"));
                });

            });
        });
    }
}
