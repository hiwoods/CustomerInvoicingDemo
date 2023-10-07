namespace Invoicing.Api.Endpoints;

public static class Routes
{
    public static class Authentication
    {
        public const string Token = "/api/token";
    }

    public static class Customers
    {
        public const string GetCustomerById = "/api/customers/{customerId:int}";
        public const string CreateCustomer = "/api/customers";
        public const string UpdateCustomer = "/api/customers/{customerId:int}";
    }

    public static class Invoices
    {
        public const string GetInvoiceById = "/api/invoices/{invoiceId:int}";
        public const string CreateInvoice = "/api/invoices";
        public const string UpdateInvoice = "/api/invoices/{invoiceId:int}";
        public const string DeleteInvoice = "/api/invoices/{invoiceId:int}";
        public const string MakePayment = "/api/invoices/{invoiceId:int}/makePayment";
    }
}
