using Invoicing.Core.Primitives;
using Invoicing.Core.Utilities;

namespace Invoicing.Core;

public class Invoice : IValidatable
{
    public int InvoiceId { get; private set; }
    public required int CustomerId { get; init; }
    public required DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }

    public required Address Address { get; set; }
    public DateTime? PaidDate {get; set; }
    public decimal TotalAmount { get; private set; }

    public IReadOnlyList<InvoiceLineItem> LineItems { get; private set; } = new List<InvoiceLineItem>(0);

    public void UpdateLineItems(IEnumerable<InvoiceLineItem> lineItems)
    {
        if (PaidDate.HasValue)
        {
            throw new InvoicingErrorException(ExpectedErrors.Invoice.AlreadyPaid);
        }

        LineItems = lineItems.ToList();
        TotalAmount = LineItems.Sum(x => x.UnitPrice * x.Quantity);
    }

    public ObjectValidationResult Validate()
    {
        var errors = new List<Error>();

        if (CustomerId <= 0) errors.Add(ExpectedErrors.Invoice.InvalidCustomerId);
        if (!DateTimeUtilities.IsValidSqlDateTime(InvoiceDate)) errors.Add(ExpectedErrors.Invoice.InvalidDate);

        return ObjectValidationResult.ConvertFrom(errors);
    }
}
