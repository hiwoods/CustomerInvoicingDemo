namespace Invoicing.Core.Externals;

public interface IPaymentGateway
{
    Task MakePayment(int customerId, decimal amount, CancellationToken ct = default);
}
