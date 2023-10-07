using Invoicing.Core;

namespace Invoicing.Api.Contracts;

public record InvoiceEditModel(int CustomerId, AddressModel BillingAddress, DateTime InvoiceDate, DateTime? DueDate, List<InvoiceLineItemModel> LineItems)
{
    public Invoice ToInvoice()
    {
        var invoice = new Invoice
        {
            CustomerId = CustomerId,
            InvoiceDate = InvoiceDate,
            DueDate = DueDate,
            Address = BillingAddress?.ToAddress() ?? Address.Default()
        };

        invoice.UpdateLineItems(LineItems.Select(x => x.ToLineItem()));
        return invoice;
    }
}
