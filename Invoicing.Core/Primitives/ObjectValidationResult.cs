using System.Diagnostics.CodeAnalysis;

namespace Invoicing.Core.Primitives;

public sealed class ObjectValidationResult
{
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsValid { get; }

    public Error? Error { get; }

    private ObjectValidationResult(bool isValid, Error? error)
    {
        IsValid = isValid;
        Error = error;
    }

    public static ObjectValidationResult Succeed() => new(true, null);
    public static ObjectValidationResult Fail(Error error) => new(false, error);

    public static ObjectValidationResult ConvertFrom(ICollection<Error>? errors)
        => errors is null || errors.Count == 0
            ? Succeed()
            : errors.Count == 1
                ? Fail(errors.Single())
                : Fail(new ManyErrors(errors));
}