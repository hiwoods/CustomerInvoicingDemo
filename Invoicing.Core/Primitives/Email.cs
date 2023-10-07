using System.Text.RegularExpressions;

namespace Invoicing.Core.Primitives;

public partial record Email : IValidatable
{
    public const int MaxLength = 256;

    public string Value { get; }

    public Email(string value)
    {
        Value = value?.Trim() ?? "";
    }

    public ObjectValidationResult Validate()
    {
        var errors = new List<Error>();

        if (string.IsNullOrEmpty(Value)) errors.Add(ExpectedErrors.Email.NullOrEmpty);
        if (Value.Length > MaxLength) errors.Add(ExpectedErrors.Email.TooLong);
        if (!EmailRegex().IsMatch(Value)) errors.Add(ExpectedErrors.Email.InvalidFormat);

        return ObjectValidationResult.ConvertFrom(errors);
    }

    [GeneratedRegex("^(?!\\.)(\"([^\"\\r\\\\]|\\\\[\"\\r\\\\])*\"|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\\.)\\.)*)(?<!\\.)@[a-z0-9][\\w\\.-]*[a-z0-9]\\.[a-z][a-z\\.]*[a-z]$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex EmailRegex();
}
