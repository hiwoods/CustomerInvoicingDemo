using Invoicing.Core.Primitives;

namespace Invoicing.Core;

public static class ExpectedErrors
{
    private const string ErrorCodeFormat = $"{{0}}.{{1}}";
    private static Error Error(string area, string code, string errorMsg) => new(string.Format(ErrorCodeFormat, area, code), errorMsg);

    public static class Name
    {
        public static readonly Error NullOrEmpty = Error(nameof(Name), nameof(NullOrEmpty), "Name is required");
        public static readonly Error TooLong = Error(nameof(Name), nameof(TooLong), $"Name exceeded {Primitives.Name.MaxLength} characters");
    }

    public static class Email
    {
        public static readonly Error NullOrEmpty = Error(nameof(Email), nameof(NullOrEmpty), "Email is required");
        public static readonly Error TooLong = Error(nameof(Email), nameof(TooLong), $"Email exceeded {Primitives.Email.MaxLength} character limit");
        public static readonly Error InvalidFormat = Error(nameof(Email), nameof(InvalidFormat), "Email has invalid format");
    }

    public static class Phone
    {
        public static readonly Error NullOrEmpty = Error(nameof(Phone), nameof(NullOrEmpty), "Phone is required");
        public static readonly Error InvalidFormat = Error(nameof(Phone), nameof(InvalidFormat), "Phone has invalid format");
    }

    public static class Address
    {
        public static readonly Error MissingStreet1 = Error(nameof(Address), nameof(MissingStreet1), "Street1 is required");
        public static readonly Error MissingCity = Error(nameof(Address), nameof(MissingCity), "City is required");

        public static readonly Error MissingState = Error(nameof(Address), nameof(MissingState), "State is required");
        public static readonly Error InvalidState = Error(nameof(Address), nameof(InvalidState), "State name should be 2-letter abbreviation");

        public static readonly Error MissingZip = Error(nameof(Address), nameof(MissingZip), "Zip is required");
        public static readonly Error InvalidZip = Error(nameof(Address), nameof(MissingZip), "Zip should have NNNNN format");
    }

    public static class Invoice
    {
        public static readonly Error InvalidDate = Error(nameof(Invoice), nameof(InvalidDate), "Invoice date is invalid");
        public static readonly Error AlreadyPaid = Error(nameof(Invoice), nameof(AlreadyPaid), "Invoice has already paid and can no longer be modified");
        public static readonly Error InvalidCustomerId = Error(nameof(Invoice), nameof(InvalidCustomerId), "Invoice msut belong to a customer");
        public static readonly Error InvalidLineItemQuantity = Error(nameof(Invoice), nameof(InvalidLineItemQuantity), "Line item quantity must be positive");
    }
}