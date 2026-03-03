namespace Library.Domain
{
    public class InvoiceLine
    {
        public int Id { get; set; }

        // FK to Invoice
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        // FK to Product
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // snapshot price at time of invoice
    }
}