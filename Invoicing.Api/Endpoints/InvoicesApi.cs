using Invoicing.Api.Contracts;
using Invoicing.Core;
using Invoicing.Core.Externals;
using Invoicing.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Invoicing.Api.Endpoints;

public static class InvoicesApi
{
    public static IEndpointRouteBuilder MapInvoicesEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/")
            .WithTags(nameof(Routes.Invoices))
            .RequireAuthorization();

        group.MapGet(Routes.Invoices.GetInvoiceById, GetInvoiceById).WithName(nameof(Routes.Invoices.GetInvoiceById));
        group.MapPost(Routes.Invoices.CreateInvoice, CreateInvoice);
        group.MapPut(Routes.Invoices.UpdateInvoice, UpdateInvoice);
        group.MapDelete(Routes.Invoices.DeleteInvoice, DeleteInvoice);
        group.MapPost(Routes.Invoices.MakePayment, MakePayment);

        return builder;
    }

    [ProducesResponseType(typeof(InvoiceModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<CreatedAtRoute<InvoiceModel>> CreateInvoice([FromBody] InvoiceEditModel invoiceEdit, InvoicingDbContext context, CancellationToken ct)
    {
        var invoice = invoiceEdit.ToInvoice();

        context.Invoices.Add(invoice);
        await context.SaveChangesAsync(ct);

        return TypedResults.CreatedAtRoute(InvoiceModel.From(invoice), nameof(Routes.Invoices.GetInvoiceById), new { invoiceId = invoice.InvoiceId });
    }

    [ProducesResponseType(typeof(InvoiceModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<Results<Ok<InvoiceModel>, NotFound>> GetInvoiceById([FromRoute] int invoiceId, InvoicingDbContext context, CancellationToken ct)
    {
        var invoice = await context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InvoiceId == invoiceId, ct);

        if (invoice is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(InvoiceModel.From(invoice));
    }

    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Results<NoContent, NotFound>> UpdateInvoice([FromRoute] int invoiceId, [FromBody] InvoiceEditModel invoiceEdit, InvoicingDbContext context, CancellationToken ct)
    {
        var invoice = await context.Invoices.FindAsync(new object?[] { invoiceId }, ct);

        if (invoice is null)
        {
            return TypedResults.NotFound();
        }

        if (invoice.PaidDate.HasValue)
        {
            throw new InvoicingErrorException(ExpectedErrors.Invoice.AlreadyPaid);
        }

        invoice.Address = invoiceEdit.BillingAddress.ToAddress();
        invoice.DueDate = invoiceEdit.DueDate;
        invoice.InvoiceDate = invoiceEdit.InvoiceDate;
        invoice.UpdateLineItems(invoiceEdit.LineItems.Select(x => x.ToLineItem()));

        await context.SaveChangesAsync(ct);

        return TypedResults.NoContent();
    }

    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Results<NoContent, NotFound>> DeleteInvoice([FromRoute] int invoiceId, InvoicingDbContext dbContext, CancellationToken ct)
    {
        var invoice = await dbContext.Invoices.FindAsync(new object?[] { invoiceId }, ct);

        if (invoice is null)
        {
            return TypedResults.NotFound();
        }

        if (invoice.PaidDate.HasValue)
        {
            throw new InvoicingErrorException(ExpectedErrors.Invoice.AlreadyPaid);
        }

        dbContext.Invoices.Remove(invoice);
        await dbContext.SaveChangesAsync(ct);

        return TypedResults.NoContent();
    }

    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Results<NoContent, NotFound>> MakePayment([FromRoute] int invoiceId,
                                                                       IPaymentGateway paymentGateway,
                                                                       InvoicingDbContext context,
                                                                       CancellationToken ct)
    {
        using var transaction = context.Database.BeginTransaction();

        var invoice = await context.Invoices.FindAsync(new object?[] { invoiceId }, ct);

        if (invoice is null)
        {
            return TypedResults.NotFound();
        }

        if (invoice.PaidDate.HasValue)
        {
            throw new InvoicingErrorException(ExpectedErrors.Invoice.AlreadyPaid);
        }

        invoice.PaidDate = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        await paymentGateway.MakePayment(invoice.CustomerId, invoice.TotalAmount, ct);

        await transaction.CommitAsync();
        
        return TypedResults.NoContent();
    }
}
