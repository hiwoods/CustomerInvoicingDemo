using Invoicing.Core;

namespace Invoicing.Api.Contracts;

public record InvoiceLineItemModel(string LineItem, int Quantity, decimal UnitPrice)
{
    public static InvoiceLineItemModel From(InvoiceLineItem lineItem)
        => new(lineItem.LineItem, lineItem.Quantity, lineItem.UnitPrice);

    public InvoiceLineItem ToLineItem() => new()
    {
        LineItem = LineItem,
        Quantity = Quantity,
        UnitPrice = UnitPrice
    };
}