using Invoicing.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoicing.Data.Invoicing;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable(TableNames.Invoicing.Address, TableNames.Invoicing.Schema);

        builder.HasKey(x => x.AddressId).IsClustered();

        builder.Property(x => x.AddressId).UseIdentityColumn();
        builder.Property(x => x.Street1).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Street2).HasMaxLength(250).IsRequired(false);
        builder.Property(x => x.City).HasMaxLength(250).IsRequired();
        builder.Property(x => x.State).HasMaxLength(2).IsRequired();
        builder.Property(x => x.Zip).HasMaxLength(9).IsRequired();
    }
}
