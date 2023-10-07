using Bogus;
using Invoicing.Api.Contracts;

namespace Invoicing.Api.Test.Fakers;

public class FakeInvoiceLineItemModel : Faker<InvoiceLineItemModel>
{ 
    public FakeInvoiceLineItemModel()
    {
        CustomInstantiator(f => new InvoiceLineItemModel(f.Lorem.Sentence(5), f.Random.Number(1, 20), f.Finance.Amount()));
    }
}
