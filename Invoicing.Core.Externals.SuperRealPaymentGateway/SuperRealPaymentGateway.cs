namespace Invoicing.Core.Externals.SuperRealPaymentGateway;

public class SuperRealPaymentGateway : IPaymentGateway
{
    public Task MakePayment(int customerId, decimal amount, CancellationToken ct = default)
    {
        throw new NotImplementedException("SuperReal payment gateway has not been implemented yet :(");
    }
}
