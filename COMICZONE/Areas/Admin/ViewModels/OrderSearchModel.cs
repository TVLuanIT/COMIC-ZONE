namespace COMICZONE.Areas.Admin.ViewModels
{
    public class OrderSearchModel : AdminSearchModel
    {
        public int? OrderId { get; set; }

        // User info
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? UserEmail { get; set; }

        // Shipping & Contact
        public string? PhoneNumber { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Note { get; set; }

        // Status
        public string? Status { get; set; }
        public List<string>? Statuses { get; set; }

        // Amounts
        public decimal? TotalAmountMin { get; set; }
        public decimal? TotalAmountMax { get; set; }

        // Dates
        public DateTime? OrderDateFrom { get; set; }
        public DateTime? OrderDateTo { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        // Product content
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }

        // Payment
        public bool? HasPayment { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TransactionId { get; set; }
        public bool? PaidOnly { get; set; }
        public bool? UnpaidOnly { get; set; }

        // Invoice
        public bool? HasInvoice { get; set; }
        public string? InvoiceNumber { get; set; }

        // Items count
        public int? ItemCountMin { get; set; }
        public int? ItemCountMax { get; set; }

        public bool? IsDeleted { get; set; }
    }
}
