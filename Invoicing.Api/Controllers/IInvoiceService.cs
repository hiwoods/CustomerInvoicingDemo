using Invoicing.Api.Contracts;
using Invoicing.Core;
using Invoicing.Core.Externals;
using Invoicing.Data;
using Microsoft.EntityFrameworkCore;

namespace Invoicing.Api.Controllers;

public interface IInvoiceService
{
    Task<InvoiceModel> CreateInvoiceAsync(InvoiceEditModel invoiceEdit, CancellationToken ct);
    Task<InvoiceModel?> GetInvoiceByIdAsync(int invoiceId, CancellationToken ct);
    Task<InvoiceModel?> UpdateInvoiceAsync(int invoiceId, InvoiceEditModel invoiceEdit, CancellationToken ct);
    Task DeleteInvoiceAsync(int invoiceId, CancellationToken ct);
    Task MakePaymentAsync(int invoiceId, CancellationToken ct);
}

public class InvoiceService : IInvoiceService
{
    private readonly InvoicingDbContext _dbContext;
    private readonly IPaymentGateway _paymentGateway;

    public InvoiceService(InvoicingDbContext dbContext, IPaymentGateway paymentGateway)
    {
        _dbContext = dbContext;
        _paymentGateway = paymentGateway;
    }

    public async Task<InvoiceModel> CreateInvoiceAsync(InvoiceEditModel invoiceEdit, CancellationToken ct)
    {
        var invoice = invoiceEdit.ToInvoice();

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync(ct);

        return InvoiceModel.From(invoice);
    }

    public async Task DeleteInvoiceAsync(int invoiceId, CancellationToken ct)
    {
        var invoice = await _dbContext.Invoices
            .FirstOrDefaultAsync(x => x.InvoiceId == invoiceId, ct) 
            ?? throw new InvoicingErrorException(new Core.Primitives.Error("NotFound", "Invoice not found"));

        _dbContext.Invoices.Remove(invoice);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<InvoiceModel?> GetInvoiceByIdAsync(int invoiceId, CancellationToken ct)
    {
        var invoice = await _dbContext.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InvoiceId == invoiceId, ct);

        if (invoice is null)
        {
            return null;
        }

        return InvoiceModel.From(invoice);
    }

    public async Task<InvoiceModel?> UpdateInvoiceAsync(int invoiceId, InvoiceEditModel invoiceEdit, CancellationToken ct)
    {
        var invoice = await _dbContext.Invoices
            .FirstOrDefaultAsync(x => x.InvoiceId == invoiceId, ct);

        if (invoice is null)
        {
            return null;
        }

        invoice.Address = invoiceEdit.BillingAddress.ToAddress();
        invoice.DueDate = invoiceEdit.DueDate;
        invoice.InvoiceDate = invoiceEdit.InvoiceDate;
        invoice.UpdateLineItems(invoiceEdit.LineItems.Select(x => x.ToLineItem()));

        await _dbContext.SaveChangesAsync(ct);

        return InvoiceModel.From(invoice);
    }

    public async Task MakePaymentAsync(int invoiceId, CancellationToken ct)
    {
        var invoice = await _dbContext.Invoices
            .FirstOrDefaultAsync(x => x.InvoiceId == invoiceId, ct)
            ?? throw new InvoicingErrorException(new Core.Primitives.Error("NotFound", "Invoice not found"));

        if (invoice.PaidDate.HasValue)
        {
            throw new InvoicingErrorException(ExpectedErrors.Invoice.AlreadyPaid);
        }

        await _paymentGateway.MakePayment(invoice.CustomerId, invoice.TotalAmount, ct);

        invoice.PaidDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);
    }
}