using Invoicing.Core;
using Invoicing.Core.Primitives;

namespace Invoicing.Api.Contracts;

public record CustomerEditModel(string? FirstName, string? LastName, string? Phone, string? Email, AddressModel? BillingAddress)
{
    public Customer ToCustomer()
        => new()
        {
            FirstName = new Name(FirstName?.Trim() ?? ""),
            LastName = new Name(LastName?.Trim() ?? ""),
            Phone = string.IsNullOrEmpty(Phone) ? null : new Phone(Phone.Trim() ?? ""),
            Email = string.IsNullOrEmpty(Email) ? null : new Email(Email?.Trim() ?? ""),
            BillingAddress = BillingAddress?.ToAddress() ?? new Address
            {
                Street1 = "",
                City = "",
                State = "",
                Zip = ""
            }
        };
}

