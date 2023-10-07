using Invoicing.Core.Primitives;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Invoicing.Data.Converters;

public class EmailConverter : ValueConverter<Email?, string?>
{
    public EmailConverter() : base(n => n == null ? null : n.Value, v => v == null ? null : new Email(v))
    {
    }
}
