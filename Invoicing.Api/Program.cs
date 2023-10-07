using Invoicing;
using Invoicing.Api.Endpoints;
using Invoicing.Api.Middleware;
using Invoicing.Api.Workers;
using Invoicing.Core.Externals;
using Invoicing.Core.Externals.SuperRealPaymentGateway;
using Invoicing.Data;
using Invoicing.Data.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<InvoicingDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseOpenIddict<Application, Authorization, Scope, Token, int>();
});

builder.Services.AddOpenIddictServer(builder.Configuration);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddScoped<IPaymentGateway, SuperRealPaymentGateway>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<MigrationWorkder>();
    builder.Services.AddHostedService<DefaultClientSeedWorker>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseInvoicingExceptionHandler(app.Environment);

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthenticationEndpoints()
   .MapCustomerEndpoints()
   .MapInvoicesEndpoints();

app.Run();

namespace Invoicing.Api
{
    public partial class Program { }
}