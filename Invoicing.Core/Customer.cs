using Invoicing.Core.Primitives;

namespace Invoicing.Core;

public class Customer : IValidatable
{
    public int CustomerId { get; init; }
    public required Name FirstName { get; set; }
    public required Name LastName { get; set; }
    public Phone? Phone { get; set; }
    public Email? Email { get; set; }
    public required Address BillingAddress { get; set; }

    public ObjectValidationResult Validate()
    {
        var errors = new List<IValidatable?> { FirstName, LastName, Phone, Email, BillingAddress }
            .Select(x => x?.Validate() ?? ObjectValidationResult.Succeed())
            .Where(x => !x.IsValid)
            .Select(x => x.Error!)
            .ToList();

        return ObjectValidationResult.ConvertFrom(errors);
    }
}
