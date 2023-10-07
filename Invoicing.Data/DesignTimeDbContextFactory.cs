using Invoicing.Data.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Invoicing.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<InvoicingDbContext>
{
    public InvoicingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InvoicingDbContext>();

        optionsBuilder.UseSqlServer();
        optionsBuilder.UseOpenIddict<Application, Authorization, Scope, Token, int>();

        return new InvoicingDbContext(optionsBuilder.Options);
    }
}
