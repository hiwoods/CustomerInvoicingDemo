using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace Invoicing.Data.Identity;

public class Application : OpenIddictEntityFrameworkCoreApplication<int, Authorization, Token>
{
}

public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable(TableNames.Identity.Application, TableNames.Identity.Schema);

        builder.HasKey(application => application.Id);

        builder.Property(application => application.Id)
               .UseIdentityColumn();

        builder.HasIndex(u => u.ClientId)
               .HasDatabaseName("IX_Application_ClientId")
               .IsUnique();

        builder.Property(application => application.ClientId)
               .HasMaxLength(100);

        builder.Property(application => application.ConcurrencyToken)
               .HasMaxLength(50)
               .IsConcurrencyToken();

        builder.Property(application => application.ConsentType)
               .HasMaxLength(50);

        builder.Property(application => application.Type)
               .HasMaxLength(50);

        builder.HasMany(application => application.Authorizations)
               .WithOne(authorization => authorization.Application!)
               .HasForeignKey(nameof(OpenIddictEntityFrameworkCoreAuthorization.Application) +
                              nameof(OpenIddictEntityFrameworkCoreApplication.Id))
               .IsRequired(required: false);

        builder.HasMany(application => application.Tokens)
               .WithOne(token => token.Application!)
               .HasForeignKey(nameof(OpenIddictEntityFrameworkCoreToken.Application) + nameof(OpenIddictEntityFrameworkCoreApplication.Id))
               .IsRequired(required: false);
    }
}
