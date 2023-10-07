using Invoicing.Core.Primitives;

namespace Invoicing.Core;

public class InvoiceLineItem : IValidatable
{
    public required string LineItem { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }

    public ObjectValidationResult Validate()
    {
        var errors = new List<Error>();

        if (Quantity <= 0) errors.Add(ExpectedErrors.Invoice.InvalidLineItemQuantity);

        return ObjectValidationResult.ConvertFrom(errors);
    }
}
