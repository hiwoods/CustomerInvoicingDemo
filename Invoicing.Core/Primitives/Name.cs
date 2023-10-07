using System.Diagnostics.CodeAnalysis;

namespace Invoicing.Core.Primitives;

public sealed record Name : IValidatable
{
    public const int MaxLength = 50;
    public string Value { get; }

    public Name(string value)
    {
        Value = value?.Trim() ?? "";
    }

    public ObjectValidationResult Validate()
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(Value)) errors.Add(ExpectedErrors.Name.NullOrEmpty);
        if (Value?.Length > MaxLength) errors.Add(ExpectedErrors.Name.TooLong);

        return ObjectValidationResult.ConvertFrom(errors);
    }

    public override string ToString() => Value;
}
