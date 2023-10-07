using Bogus;
using Invoicing.Core;

namespace Invoicing.Api.Test.Fakers;

public class FakeAddress : Faker<Address>
{
    public FakeAddress()
    {
        RuleFor(a => a.Street1, f => f.Address.StreetAddress());
        RuleFor(a => a.Street2, f => f.Address.SecondaryAddress());
        RuleFor(a => a.City, f => f.Address.City());
        RuleFor(a => a.State, f => f.Address.StateAbbr());
        RuleFor(a => a.Zip, f => f.Address.ZipCode("#####"));
    }
}