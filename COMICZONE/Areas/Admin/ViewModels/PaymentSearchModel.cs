namespace COMICZONE.Areas.Admin.ViewModels
{
    public class PaymentSearchModel : AdminSearchModel
    {
        public int? Paymentid { get; set; }
        public int? Orderid { get; set; }
        public string? Transactionid { get; set; }

        public List<string>? Paymentstatuses { get; set; }
        public List<string>? Paymentmethods { get; set; }

        public decimal? AmountMin { get; set; }
        public decimal? AmountMax { get; set; }

        public DateTime? PaidFrom { get; set; }
        public DateTime? PaidTo { get; set; }

        public string? OrderUsername { get; set; }

        public bool? PaidOnly { get; set; }
        public bool? HasRefund { get; set; }
    }
}
