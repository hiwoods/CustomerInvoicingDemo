using Invoicing.Core;

namespace Invoicing.Api.Contracts;

public record CustomerModel(int CustomerId, string FirstName, string LastName, string? Phone, string? Email, AddressModel? BillingAddress)
{
    public static CustomerModel From(Customer customer)
        => new(customer.CustomerId,
               customer.FirstName.Value,
               customer.LastName.Value,
               customer.Phone?.Value,
               customer.Email?.Value,
               AddressModel.From(customer.BillingAddress));
}
