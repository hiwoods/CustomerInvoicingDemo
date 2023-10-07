namespace Invoicing.Api.Test;

[CollectionDefinition(Name)]
public class InvoicingTestCollection : ICollectionFixture<TestServerFixture>
{
    public const string Name = "InvoicingTestCollection";
}
