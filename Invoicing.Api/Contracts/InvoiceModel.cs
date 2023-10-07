using Invoicing.Core;

namespace Invoicing.Api.Contracts;

public record InvoiceModel(int InvoiceId, int CustomerId, AddressModel Address, DateTime InvoiceDate, DateTime? DueDate, DateTime? PaidDate, decimal TotalAmount, List<InvoiceLineItemModel> LineItems)
{
    public static InvoiceModel From(Invoice invoice)
        => new(invoice.InvoiceId,
               invoice.CustomerId,
               AddressModel.From(invoice.Address),
               invoice.InvoiceDate,
               invoice.DueDate,
               invoice.PaidDate,
               invoice.TotalAmount,
               invoice.LineItems.Select(InvoiceLineItemModel.From).ToList());
}
