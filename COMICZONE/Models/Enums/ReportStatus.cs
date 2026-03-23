using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Enums
{
    public enum ReportStatus
    {
        [Display(Name = "Chờ xử lý")]
        Pending = 1,

        [Display(Name = "Đã duyệt")]
        Approved = 2,

        [Display(Name = "Từ chối")]
        Rejected = 3
    }
}
