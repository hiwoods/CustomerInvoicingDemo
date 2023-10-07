using Invoicing.Api.Contracts;
using Invoicing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Invoicing.Api.Endpoints;

public static class CustomersApi
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/")
            .WithTags(nameof(Routes.Customers))
            .RequireAuthorization();

        group.MapGet(Routes.Customers.GetCustomerById, GetCustomerById).WithName(nameof(Routes.Customers.GetCustomerById));
        group.MapPost(Routes.Customers.CreateCustomer, CreateCustomer).WithName(nameof(Routes.Customers.CreateCustomer));
        group.MapPut(Routes.Customers.UpdateCustomer, UpdateCustomer).WithName(nameof(Routes.Customers.UpdateCustomer));

        return builder;
    }

    [ProducesResponseType(typeof(CustomerModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<Results<Ok<CustomerModel>, NotFound>> GetCustomerById([FromRoute] int customerId,
                                                                                   InvoicingDbContext dbContext,
                                                                                   CancellationToken ct)
    {
        var customer = await dbContext.Customers
            .Include(x => x.BillingAddress)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CustomerId == customerId, ct);

        if (customer is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(CustomerModel.From(customer));
    }

    [ProducesResponseType(typeof(CustomerModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<CreatedAtRoute<CustomerModel>> CreateCustomer([FromBody] CustomerEditModel customerEdit,
                                                                           InvoicingDbContext dbContext,
                                                                           CancellationToken ct)
    {
        var customer = customerEdit.ToCustomer();

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(ct);

        return TypedResults.CreatedAtRoute(CustomerModel.From(customer),
                                           nameof(Routes.Customers.GetCustomerById),
                                           new { customerId = customer.CustomerId });
    }

    [Authorize]
    [ProducesResponseType(typeof(CustomerModel), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> UpdateCustomer([FromRoute] int customerId,
                                                     [FromBody] CustomerEditModel customerEdit,
                                                     InvoicingDbContext dbContext,
                                                     CancellationToken ct)
    {
        var customer = await dbContext.Customers.FindAsync(new object?[] { customerId }, cancellationToken: ct);

        if (customer is null)
        {
            return Results.NotFound();
        }

        var mappedCustomer = customerEdit.ToCustomer();

        customer.FirstName = mappedCustomer.FirstName;
        customer.LastName = mappedCustomer.LastName;
        customer.Email = mappedCustomer.Email;
        customer.Phone = mappedCustomer.Phone;
        customer.BillingAddress = mappedCustomer.BillingAddress;

        await dbContext.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
