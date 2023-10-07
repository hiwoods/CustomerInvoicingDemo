using Invoicing.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoicing.Data.Invoicing;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable(TableNames.Invoicing.Customer, TableNames.Invoicing.Schema);

        builder.HasKey(x => x.CustomerId).IsClustered();

        builder.Property(x => x.CustomerId).UseIdentityColumn();

        builder.Property(x => x.FirstName).IsRequired();
        builder.Property(x => x.LastName).IsRequired();

        builder.Property<int>("BillingAddressId").IsRequired();

        builder.Navigation(x => x.BillingAddress).AutoInclude();

        builder.HasOne(x => x.BillingAddress)
               .WithMany()
               .HasForeignKey("BillingAddressId");
    }
}
