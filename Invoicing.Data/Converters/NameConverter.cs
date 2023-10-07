using Invoicing.Core.Primitives;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Invoicing.Data.Converters;

public class NameConverter : ValueConverter<Name?, string?>
{
    public NameConverter() : base(n => n == null ? null : n.Value, v => v == null ? null : new Name(v))
    {
    }
}
