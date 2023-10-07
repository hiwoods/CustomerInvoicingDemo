using Invoicing.Api.Contracts;
using Invoicing.Api.Endpoints;
using Invoicing.Api.Test.Fakers;
using Invoicing.Core;
using Invoicing.Core.Externals;
using Invoicing.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Net.Http.Json;
using System.Text.Json;

namespace Invoicing.Api.Test;

[Collection(InvoicingTestCollection.Name)]
[UsesVerify]
public class InvoicesApiTest
{
    private static readonly int LocalSeed = 1453234;

    private readonly TestServerFixture _serverFixture;

    public InvoicesApiTest(TestServerFixture serverFixture) => _serverFixture = serverFixture;

    [Fact(DisplayName = "Create Invoice")]
    public async Task CreateInvoice()
    {
        //arrange
        var customer = await CreateCustomer();

        using var client = await _serverFixture.CreateAuthenticatedClient();
        var createRequest = new FakeInvoiceEditModel(customer.CustomerId).UseSeed(LocalSeed).Generate();

        //act
        var response = await client.PostAsJsonAsync("/api/invoices", createRequest);

        var content = await response.Content.ReadAsStringAsync();

        //assert
        response.EnsureSuccessStatusCode();

        var model = JsonSerializer.Deserialize<InvoiceModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        await Verify(model);

        using (var scope = _serverFixture.ServiceProvider.CreateScope())
        using (var dbContext = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>())
        {
            var persisted = await dbContext.Invoices.FindAsync(model!.InvoiceId);
            persisted.Should().NotBeNull();
        }
    }

    [Fact(DisplayName = "Update Invoice")]
    public async Task UpdateInvoice()
    {
        //arrange
        var customer = await CreateCustomer();

        var invoiceFaker = new FakeInvoiceEditModel(customer.CustomerId).UseSeed(LocalSeed);

        var originalInvoice = invoiceFaker.Generate().ToInvoice();

        using (var scope = _serverFixture.ServiceProvider.CreateScope())
        using (var dbContext = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>())
        {
            dbContext.Invoices.Add(originalInvoice);
            await dbContext.SaveChangesAsync();
        }

        var editRequest = invoiceFaker.Generate();

        using var client = await _serverFixture.CreateAuthenticatedClient();

        //act
        var response = await client.PutAsJsonAsync($"/api/invoices/{originalInvoice.InvoiceId}", editRequest);

        //assert
        response.EnsureSuccessStatusCode();

        using (var scope = _serverFixture.ServiceProvider.CreateScope())
        using (var dbContext = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>())
        {
            var persisted = await dbContext.Invoices.FindAsync(originalInvoice!.InvoiceId);
            await Verify(persisted);
        }
    }

    [Fact(DisplayName = "Delete Invoice")]
    public async Task DeleteInvoice()
    {
        //arrange
        var customer = await CreateCustomer();

        var invoiceFaker = new FakeInvoiceEditModel(customer.CustomerId).UseSeed(LocalSeed);

        var originalInvoice = invoiceFaker.Generate().ToInvoice();

        using (var scope = _serverFixture.ServiceProvider.CreateScope())
        using (var dbContext = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>())
        {
            dbContext.Invoices.Add(originalInvoice);
            dbContext.SaveChanges();
        }

        using var client = await _serverFixture.CreateAuthenticatedClient();

        //act
        var response = await client.DeleteAsync($"/api/invoices/{originalInvoice.InvoiceId}");

        //assert
        response.EnsureSuccessStatusCode();

        using (var scope = _serverFixture.ServiceProvider.CreateScope())
        using (var dbContext = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>())
        {
            var persisted = await dbContext.Invoices.FindAsync(originalInvoice!.InvoiceId);
            persisted.Should().BeNull(because: "invoice should have been deleted");
        }
    }

    [Fact(DisplayName = "Get Invoice")]
    public async Task GetInvoice()
    {
        //arrange
        var customer = await CreateCustomer();

        var invoiceFaker = new FakeInvoiceEditModel(customer.CustomerId).UseSeed(LocalSeed);

        var originalInvoice = invoiceFaker.Generate().ToInvoice();

        using (var scope = _serverFixture.ServiceProvider.CreateScope())
        using (var dbContext = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>())
        {
            dbContext.Invoices.Add(originalInvoice);
            dbContext.SaveChanges();
        }

        using var client = await _serverFixture.CreateAuthenticatedClient();

        //act
        var response = await client.GetAsync($"/api/invoices/{originalInvoice.InvoiceId}");

        //assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        await VerifyJson(content).ScrubMembers("customerId", "invoiceId");
    }

    [Fact(DisplayName = "Make Payment")]
    public async Task MakePaymentTest()
    {
        //arrange
        var customer = await CreateCustomer();

        var originalInvoice = new FakeInvoiceEditModel(customer.CustomerId).UseSeed(LocalSeed).Generate().ToInvoice();

        using (var scope = _serverFixture.ServiceProvider.CreateScope())
        using (var dbContext = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>())
        {
            dbContext.Invoices.Add(originalInvoice);
            dbContext.SaveChanges();
        }

        //act
        using (var scope = _serverFixture.ServiceProvider.CreateScope())
        using (var dbContext = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>())
        {
            var paymentGateway = Substitute.For<IPaymentGateway>();

            var result = await InvoicesApi.MakePayment(originalInvoice.InvoiceId, paymentGateway, dbContext, CancellationToken.None);

            //assert
            result.Result.Should().Be(Results.NoContent());

            await paymentGateway.Received(1).MakePayment(originalInvoice.CustomerId, originalInvoice.TotalAmount, Arg.Any<CancellationToken>());
            paymentGateway.ReceivedCalls().Should().HaveCount(1);
        }
    }

    private async Task<Customer> CreateCustomer()
    {
        var customer = new FakeCustomer().UseSeed(LocalSeed).Generate();

        using var scope = _serverFixture.ServiceProvider.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();

        return customer;
    }
}
