using Bogus;
using Invoicing.Api.Contracts;

namespace Invoicing.Api.Test.Fakers;

public class FakeAddressModel : Faker<AddressModel>
{
    public FakeAddressModel()
    {
        CustomInstantiator(f => new AddressModel(f.Address.StreetAddress(),
                                                 f.Address.SecondaryAddress(),
                                                 f.Address.City(),
                                                 f.Address.StateAbbr(),
                                                 f.Address.ZipCode("#####")));
    }
}
