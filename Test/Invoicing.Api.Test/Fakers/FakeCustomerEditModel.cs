using Bogus;
using Invoicing.Api.Contracts;

namespace Invoicing.Api.Test.Fakers;

public class FakeCustomerEditModel : Faker<CustomerEditModel>
{
    public FakeCustomerEditModel()
    {
        CustomInstantiator(f => new CustomerEditModel(f.Name.FirstName(),
                                                      f.Name.LastName(),
                                                      f.Phone.PhoneNumber("##########"),
                                                      f.Internet.Email(),
                                                      GenerateAddress()));
    }

    private AddressModel GenerateAddress()
    {
        var addressModel = new FakeAddressModel();
        if (localSeed.HasValue)
        {
            addressModel.UseSeed(localSeed.Value);
        }

        return addressModel.Generate();
    }
}
