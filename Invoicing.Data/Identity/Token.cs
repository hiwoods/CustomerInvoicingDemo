using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace Invoicing.Data.Identity;

public class Token : OpenIddictEntityFrameworkCoreToken<int, Application, Authorization>
{
}

public class TokenConfiguration : IEntityTypeConfiguration<Token>
{
    public void Configure(EntityTypeBuilder<Token> builder)
    {
        builder.ToTable(TableNames.Identity.Token, TableNames.Identity.Schema);

        builder.HasKey(token => token.Id);
        builder.Property(token => token.Id).UseIdentityColumn();

        builder.HasIndex(token => token.ReferenceId)
               .HasDatabaseName("IX_Token_ReferenceId")
               .IsUnique();

        builder.HasIndex(
            nameof(OpenIddictEntityFrameworkCoreToken.Application) + nameof(OpenIddictEntityFrameworkCoreApplication.Id),
            nameof(OpenIddictEntityFrameworkCoreToken.Status),
            nameof(OpenIddictEntityFrameworkCoreToken.Subject),
            nameof(OpenIddictEntityFrameworkCoreToken.Type));

        builder.Property(token => token.ConcurrencyToken)
               .HasMaxLength(50)
               .IsConcurrencyToken();

        builder.Property(token => token.ReferenceId)
               .HasMaxLength(100);

        builder.Property(token => token.Status)
               .HasMaxLength(50);

        builder.Property(token => token.Subject)
               .HasMaxLength(400);

        builder.Property(token => token.Type)
               .HasMaxLength(50);

    }
}