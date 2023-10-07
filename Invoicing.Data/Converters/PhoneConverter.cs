using Invoicing.Core.Primitives;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Invoicing.Data.Converters;

public class PhoneConverter : ValueConverter<Phone?, string?>
{
    public PhoneConverter() : base(p => p == null ? null : p.Value, v => v == null ? null : new Phone(v))
    {
    }
}
