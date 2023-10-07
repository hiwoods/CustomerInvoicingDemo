namespace Invoicing.Core.Primitives;

public interface IValidatable
{
    ObjectValidationResult Validate();
}
