using Invoicing.Core;

namespace Invoicing.Api.Contracts;

public record AddressModel(string Street1, string? Street2, string City, string State, string Zip)
{
    public static AddressModel From(Address address)
        => new(address.Street1, address.Street2, address.City, address.State, address.Zip);

    public Address ToAddress() => new()
    {
        Street1 = Street1?.Trim() ?? "",
        Street2 = Street2?.Trim(),
        City = City?.Trim() ?? "",
        State = State?.Trim() ?? "",
        Zip = Zip?.Trim() ?? ""
    };
}