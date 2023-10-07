using Invoicing.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoicing.Data.Invoicing;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable(TableNames.Invoicing.Invoice, TableNames.Invoicing.Schema);

        builder.HasKey(x => x.InvoiceId).IsClustered();
        builder.Property(x => x.InvoiceId).UseIdentityColumn();

        builder.HasOne<Customer>()
               .WithMany()
               .HasForeignKey(x => x.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(x => x.LineItems, line =>
        {
            line.ToTable(TableNames.Invoicing.InvoiceLineItem, TableNames.Invoicing.Schema);

            line.WithOwner().HasForeignKey(nameof(Invoice.InvoiceId));
            line.Property(x => x.UnitPrice).HasColumnType("decimal(19,4)");

            string idColumn = $"{TableNames.Invoicing.InvoiceLineItem}Id";

            line.Property<int>(idColumn).UseIdentityColumn();
            line.HasKey(idColumn);
        });

        builder.Property(x => x.TotalAmount).HasColumnType("decimal(19,4)");

        builder.Property<int>("AddressId").IsRequired();

        builder.HasOne(x => x.Address)
               .WithMany()
               .HasForeignKey("AddressId");

        builder.Navigation(x => x.Address).AutoInclude();
        builder.Navigation(x => x.LineItems).AutoInclude();
    }
}
