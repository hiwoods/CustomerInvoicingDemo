namespace Invoicing.Data;

internal static class TableNames
{
    public static class Identity
    {
        public const string Schema = "identity";

        public const string Application = "Application";
        public const string Authorization = "Authorization";
        public const string Token = "Token";
        public const string Scope = "Scope";
    }

    public static class Invoicing
    {
        public const string Schema = "invoicing";

        public const string Customer = "Customer";
        public const string Address = "Address";
        public const string Invoice = "Invoice";
        public const string InvoiceLineItem = "InvoiceLineItem";
    }
}