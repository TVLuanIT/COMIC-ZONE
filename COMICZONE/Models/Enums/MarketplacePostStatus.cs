using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Enums
{
    public enum MarketplacePostStatus
    {
        [Display(Name = "Chờ duyệt")]
        Pending,

        [Display(Name = "Đã duyệt")]
        Approved,

        [Display(Name = "Từ chối")]
        Rejected,

        [Display(Name = "Đã bán")]
        Sold
    }
}
