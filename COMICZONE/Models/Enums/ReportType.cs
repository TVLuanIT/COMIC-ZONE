using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Enums
{
    public enum ReportType
    {
        [Display(Name = "Đánh giá")]
        Review = 1,

        [Display(Name = "Phản hồi")]
        Reply = 2
    }
}