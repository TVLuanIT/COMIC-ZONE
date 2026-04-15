using System;

namespace COMICZONE.Areas.Admin.ViewModels
{
    public class MarketplacePostPromotionSearchModel : AdminSearchModel
    {
        // Filters
        public string? Status { get; set; }
        public string? PromotionType { get; set; }
        public string? Username { get; set; }
        public string? PostTitle { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
