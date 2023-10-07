using Invoicing.Api.Contracts;
using Invoicing.Api.Test.Fakers;
using Invoicing.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;

namespace Invoicing.Api.Test;

[Collection(InvoicingTestCollection.Name)]
[UsesVerify]
public class CustomersApiTest
{
    private static readonly int LocalSeed = 324324;
    private readonly TestServerFixture _serverFixture;

    public CustomersApiTest(TestServerFixture serverFixture) => _serverFixture = serverFixture;

    [Fact(DisplayName = "Create Customer")]
    public async Task CreateCustomer()
    {
        //arrange
        using var client = await _serverFixture.CreateAuthenticatedClient();
        var createRequest = new FakeCustomerEditModel().UseSeed(LocalSeed).Generate();

        //act
        var response = await client.PostAsJsonAsync("/api/customers", createRequest);

        var content = await response.Content.ReadAsStringAsync();

        //assert
        response.EnsureSuccessStatusCode();

        var model = JsonSerializer.Deserialize<CustomerModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        await Verify(model).ScrubMember<CustomerModel>(x => x.CustomerId);

        using (var scope = _serverFixture.ServiceProvider.CreateScope())
        using (var dbContext = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>())
        {
            var persisted = await dbContext.Customers.FindAsync(model!.CustomerId);
            persisted.Should().NotBeNull();
        }
    }

    [Fact(DisplayName = "Update Customer")]
    public async Task UpdateCustomer()
    {
        //arrange
        var customer = new FakeCustomer().UseSeed(LocalSeed).Generate();
        var editRequest = new FakeCustomerEditModel().UseSeed(LocalSeed).Generate();

        using (var scope = _serverFixture.ServiceProvider.CreateScope())
        using (var dbContext = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>())
        {
            dbContext.Customers.Add(customer);
            await dbContext.SaveChangesAsync();
        }

        //act
        using var client = await _serverFixture.CreateAuthenticatedClient();

        var response = await client.PutAsJsonAsync($"/api/customers/{customer.CustomerId}", editRequest);

        //assert
        response.EnsureSuccessStatusCode();

        using (var scope = _serverFixture.ServiceProvider.CreateScope())
        using (var dbContext = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>())
        {
            var persisted = await dbContext.Customers.FindAsync(customer.CustomerId);
            await Verify(persisted);
        }
    }

    [Fact(DisplayName = "Get Customer")]
    public async Task GetCustomer()
    {
        //arrange
        var customer = new FakeCustomer().UseSeed(LocalSeed).Generate();

        using (var scope = _serverFixture.ServiceProvider.CreateScope())
        using (var dbContext = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>())
        {
            dbContext.Customers.Add(customer);
            await dbContext.SaveChangesAsync();
        }

        //act
        using var client = await _serverFixture.CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/customers/{customer.CustomerId}");

        //assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        await VerifyJson(content).ScrubMembers("customerId", "addressId");
    }
}
