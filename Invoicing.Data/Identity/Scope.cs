using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace Invoicing.Data.Identity;

public class Scope : OpenIddictEntityFrameworkCoreScope<int>
{
}

public class ScopeConfiguration : IEntityTypeConfiguration<Scope>
{
    public void Configure(EntityTypeBuilder<Scope> builder)
    {
        // Warning: optional foreign keys MUST NOT be added as CLR properties because
        // Entity Framework would throw an exception due to the TKey generic parameter
        // being non-nullable when using value types like short, int, long or Guid.

        builder.HasKey(scope => scope.Id);
        builder.Property(scope => scope.Id).UseIdentityColumn();

        builder.HasIndex(scope => scope.Name).HasDatabaseName("IX_Scope_Name").IsUnique();

        builder.Property(scope => scope.ConcurrencyToken)
               .HasMaxLength(50)
               .IsConcurrencyToken();

        builder.Property(scope => scope.Name)
               .HasMaxLength(200);

        builder.ToTable(TableNames.Identity.Scope, TableNames.Identity.Schema);
    }
}
