namespace COMICZONE.Areas.Admin.ViewModels
{
    public class MarketplaceOrderSearchModel : AdminSearchModel
    {
        public string? Status { get; set; }
        public string? BuyerUsername { get; set; }
        public string? SellerUsername { get; set; }
        public int? BuyerId { get; set; }
        public int? SellerId { get; set; }
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}
