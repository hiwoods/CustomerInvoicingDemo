using Invoicing.Core.Primitives;

namespace Invoicing.Core;

[Serializable]
public class InvoicingErrorException : Exception
{
    public Error Error { get; }

    public InvoicingErrorException(Error error) : base(error.ToString()) => Error = error;
}
