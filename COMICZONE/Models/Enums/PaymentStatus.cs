using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Enums
{
    public enum PaymentStatus
    {
        [Display(Name = "Chờ thanh toán")]
        PENDING = 1,

        [Display(Name = "Thành công")]
        SUCCESS = 2,

        [Display(Name = "Thất bại")]
        FAILED = 3,

        [Display(Name = "Đã hủy")]
        CANCELLED = 4
    }
}