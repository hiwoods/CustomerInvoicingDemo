using Bogus;
using Invoicing.Api.Contracts;

namespace Invoicing.Api.Test.Fakers;

public class FakeInvoiceEditModel : Faker<InvoiceEditModel>
{
    public FakeInvoiceEditModel(int customerId)
    {
        GenerateAddress();
        CustomInstantiator(f => new InvoiceEditModel(customerId,
                                                     GenerateAddress(),
                                                     f.Date.Recent(-30),
                                                     f.Date.Soon(30),
                                                     GenerateLineItems(f.Random.Number(1, 5))));
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

    private List<InvoiceLineItemModel> GenerateLineItems(int count)
    {
        var items = new FakeInvoiceLineItemModel();
        if (localSeed.HasValue)
        {
            items.UseSeed(localSeed.Value);
        }

        return items.Generate(count);
    }
}
