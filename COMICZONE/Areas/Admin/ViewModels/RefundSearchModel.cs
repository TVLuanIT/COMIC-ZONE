namespace COMICZONE.Areas.Admin.ViewModels
{
    public class RefundSearchModel : AdminSearchModel
    {
        public int? Id { get; set; }
        public int? PaymentId { get; set; }

        public List<string>? Statuses { get; set; }

        public decimal? AmountMin { get; set; }
        public decimal? AmountMax { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        // Payment info
        public string? Transactionid { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }

        // Order info
        public int? OrderId { get; set; }
        public string? OrderStatus { get; set; }
        public string? OrderPhoneNumber { get; set; }
        public string? OrderUsername { get; set; }

        public string? Reason { get; set; }

        public bool? PendingOnly { get; set; }
        public bool? CompletedOnly { get; set; }

        public bool? LargeRefundsOnly { get; set; }
    }
}
