using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace Invoicing.Data.Identity;

public class Authorization : OpenIddictEntityFrameworkCoreAuthorization<int, Application, Token>
{
}

public class AuthorizationConfiguration : IEntityTypeConfiguration<Authorization>
{
    public void Configure(EntityTypeBuilder<Authorization> builder)
    {
        // Warning: optional foreign keys MUST NOT be added as CLR properties because
        // Entity Framework would throw an exception due to the TKey generic parameter
        // being non-nullable when using value types like short, int, long or Guid.

        builder.ToTable(TableNames.Identity.Authorization, TableNames.Identity.Schema);

        builder.HasKey(authorization => authorization.Id);
        builder.Property(authorization => authorization.Id).UseIdentityColumn();

        builder.HasIndex(
            nameof(OpenIddictEntityFrameworkCoreAuthorization.Application) + nameof(OpenIddictEntityFrameworkCoreApplication.Id),
            nameof(OpenIddictEntityFrameworkCoreAuthorization.Status),
            nameof(OpenIddictEntityFrameworkCoreAuthorization.Subject),
            nameof(OpenIddictEntityFrameworkCoreAuthorization.Type));

        builder.Property(authorization => authorization.ConcurrencyToken)
               .HasMaxLength(50)
               .IsConcurrencyToken();

        builder.Property(authorization => authorization.Status)
               .HasMaxLength(50);

        builder.Property(authorization => authorization.Subject)
               .HasMaxLength(400);

        builder.Property(authorization => authorization.Type)
               .HasMaxLength(50);

        builder.HasMany(authorization => authorization.Tokens)
               .WithOne(token => token.Authorization!)
               .HasForeignKey(nameof(OpenIddictEntityFrameworkCoreToken.Authorization) +
                              nameof(OpenIddictEntityFrameworkCoreAuthorization.Id))
               .IsRequired(required: false);
    }
}

