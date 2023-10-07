using Bogus;
using Invoicing.Core;
using Invoicing.Core.Primitives;

namespace Invoicing.Api.Test.Fakers;

public class FakeCustomer : Faker<Customer>
{
    public FakeCustomer()
    {
        RuleFor(c => c.FirstName, f => new Name(f.Name.FirstName()));
        RuleFor(c => c.LastName, f => new Name(f.Name.LastName()));
        RuleFor(c => c.Phone, f => new Phone(f.Phone.PhoneNumber("##########")));
        RuleFor(c => c.Email, f => new Email(f.Internet.Email()));
        RuleFor(c => c.BillingAddress, f => GenerateAddress());
    }

    private Address GenerateAddress()
    {
        var address = new FakeAddress();
        if (localSeed.HasValue)
        {
            address.UseSeed(localSeed.Value);
        }

        return address.Generate();
    }
}
