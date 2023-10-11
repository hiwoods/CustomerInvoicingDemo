using Invoicing.Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Invoicing.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet]
    [Route("{invoiceId}")]
    public async Task<ActionResult<InvoiceModel>> GetById(int invoiceId, CancellationToken ct)
    {
        var invoices = await _invoiceService.GetInvoiceByIdAsync(invoiceId, ct);

        return Ok(invoices);
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult<InvoiceModel>> Create([FromBody] InvoiceEditModel invoiceEdit, CancellationToken ct)
    {
        var invoice = await _invoiceService.CreateInvoiceAsync(invoiceEdit, ct);
        return CreatedAtAction(nameof(GetById), new { invoiceId = invoice.InvoiceId }, invoice);
    }

    [HttpPut]
    [Route("{invoiceId}")]
    public async Task<ActionResult<InvoiceModel>> Update(int invoiceId, [FromBody] InvoiceEditModel invoiceEdit, CancellationToken ct)
    {
        var invoice = await _invoiceService.UpdateInvoiceAsync(invoiceId, invoiceEdit, ct);
        return Ok(invoice);
    }

    [HttpDelete]
    [Route("{invoiceId}")]
    public async Task<ActionResult> Delete(int invoiceId, CancellationToken ct)
    {
        await _invoiceService.DeleteInvoiceAsync(invoiceId, ct);
        return NoContent();
    }

    [HttpPost]
    [Route("{invoiceId}/makepayment")]
    public async Task<ActionResult> MakePayment(int invoiceId, CancellationToken ct)
    {
        await _invoiceService.MakePaymentAsync(invoiceId, ct);
        return NoContent();
    }
}
