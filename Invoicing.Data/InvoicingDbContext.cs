using Invoicing.Core;
using Invoicing.Core.Primitives;
using Invoicing.Data.Converters;
using Microsoft.EntityFrameworkCore;

namespace Invoicing.Data;

public class InvoicingDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    public InvoicingDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("invoicing");
        builder.ApplyConfigurationsFromAssembly(typeof(InvoicingDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Name>().HaveMaxLength(Name.MaxLength).HaveConversion<NameConverter>();
        configurationBuilder.Properties<Phone>().HaveMaxLength(Phone.MaxLength).HaveConversion<PhoneConverter>();
        configurationBuilder.Properties<Email>().HaveMaxLength(Email.MaxLength).HaveConversion<EmailConverter>();
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ValidateModifiedEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateModifiedEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    private void ValidateModifiedEntities()
    {
        var validatables = ChangeTracker
                    .Entries()
                    .Where(entity => entity.State == EntityState.Added || entity.State == EntityState.Modified)
                    .Select(entity => entity.Entity)
                    .OfType<IValidatable>()
                    .ToList();

        foreach (var validatable in validatables)
        {
            var validation = validatable.Validate();

            if (!validation.IsValid)
            {
                throw new InvoicingErrorException(validation.Error);
            }
        }
    }
}
