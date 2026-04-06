using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Enums
{
    public enum BlogStatus
    {
        [Display(Name = "Bản nháp")]
        Draft = 1,

        [Display(Name = "Chờ xử lý")]
        Pending = 2,

        [Display(Name = "Chấp nhận")]
        Approved = 3,

        [Display(Name = "Từ chối")]
        Rejected = 4
    }
}
