using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Enums
{
    public enum RefundStatus
    {
        [Display(Name = "Chờ duyệt")]
        Pending,

        [Display(Name = "Đang xử lý")]
        Processing,

        [Display(Name = "Thành công")]
        Success,

        [Display(Name = "Hoàn tất")]
        Completed,

        [Display(Name = "Bị từ chối")]
        Rejected
    }
}
