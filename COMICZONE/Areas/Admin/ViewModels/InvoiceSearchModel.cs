namespace COMICZONE.Areas.Admin.ViewModels
{
    public class InvoiceSearchModel : AdminSearchModel
    {
        public int? Id { get; set; }
        public int? OrderId { get; set; }
        public string? CustomerName { get; set; }

        // Order context
        public string? OrderStatus { get; set; }
        public string? OrderPhoneNumber { get; set; }
        public DateTime? OrderDateFrom { get; set; }
        public DateTime? OrderDateTo { get; set; }

        // User info
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? UserEmail { get; set; }

        // Amounts
        public decimal? TotalAmountMin { get; set; }
        public decimal? TotalAmountMax { get; set; }

        // Timeline
        public DateTime? IssueDateFrom { get; set; }
        public DateTime? IssueDateTo { get; set; }

        // Payment context
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? Transactionid { get; set; }

        // Logic flags
        public bool? PaidOnly { get; set; }
        public bool? UnpaidOnly { get; set; }

        public bool? IsDeleted { get; set; }
    }
}
