namespace COMICZONE.Areas.Admin.ViewModels
{
    public class MarketplaceReviewSearchModel : AdminSearchModel
    {
        public int? Rating { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public string? ReviewerUsername { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}
