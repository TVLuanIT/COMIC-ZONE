namespace COMICZONE.Areas.Admin.ViewModels
{
    public class MarketplacePostSearchModel : AdminSearchModel
    {
        // Filters
        public string? Status { get; set; }
        public string? Category { get; set; }
        public string? Condition { get; set; }
        public string? SellerUsername { get; set; }
        public int? SellerId { get; set; }
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
