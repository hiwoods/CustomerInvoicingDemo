using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Invoicing.Core.Primitives;

public partial record Phone : IValidatable
{
    public const int MaxLength = 10;

    public string Value { get; }

    public Phone(string value)
    {
        Value = SpecialCharRegex().Replace(value, string.Empty);
    }

    public ObjectValidationResult Validate()
    {
        var errors = new List<Error>();

        if (string.IsNullOrEmpty(Value)) errors.Add(ExpectedErrors.Phone.NullOrEmpty);
        if (Value.Length != 10) errors.Add(ExpectedErrors.Phone.InvalidFormat);

        return ObjectValidationResult.ConvertFrom(errors);
    }

    [GeneratedRegex("[^0-9A-Za-z]")]
    private static partial Regex SpecialCharRegex();
}