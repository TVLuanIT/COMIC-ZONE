namespace COMICZONE.Areas.Admin.ViewModels
{
    public class CartSearchModel : AdminSearchModel
    {
        public int? CartId { get; set; }
        public int? UserId { get; set; }

        public string? Username { get; set; }
        public string? UserEmail { get; set; }
        public string? CustomerPhoneNumber { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public int? ProductId { get; set; }
        public string? ProductName { get; set; }

        public int? ItemCountMin { get; set; }
        public int? ItemCountMax { get; set; }

        public bool? HasItems { get; set; }
        public bool? EmptyCartOnly { get; set; }

        public bool? AbandonedOnly { get; set; }

        public decimal? TotalValueMin { get; set; }
        public decimal? TotalValueMax { get; set; }
    }
}
