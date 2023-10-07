using Invoicing.Core.Primitives;
using System.Text.RegularExpressions;

namespace Invoicing.Core;

public partial class Address : IValidatable
{
    public int AddressId { get; init; }
    public required string Street1 { get; set; }
    public string? Street2 { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string Zip { get; set; }

    public ObjectValidationResult Validate()
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(Street1)) errors.Add(ExpectedErrors.Address.MissingStreet1);
        if (string.IsNullOrWhiteSpace(City)) errors.Add(ExpectedErrors.Address.MissingCity);

        if (string.IsNullOrWhiteSpace(State)) errors.Add(ExpectedErrors.Address.MissingState);
        else if (State.Length != 2) errors.Add(ExpectedErrors.Address.InvalidState);

        if (string.IsNullOrWhiteSpace(Zip)) errors.Add(ExpectedErrors.Address.MissingZip);
        else if (!ZipCodeRegex().IsMatch(Zip)) errors.Add(ExpectedErrors.Address.InvalidZip);

        return ObjectValidationResult.ConvertFrom(errors);
    }

    public static Address Default() => new Address { Street1 = "", City = "", State = "", Zip = "" };

    [GeneratedRegex("\\d{5}")]
    private static partial Regex ZipCodeRegex();
}