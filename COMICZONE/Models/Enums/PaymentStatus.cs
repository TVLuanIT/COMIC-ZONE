using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Enums
{
    public enum PaymentStatus
    {
        [Display(Name = "Chờ thanh toán")]
        PENDING = 1,

        [Display(Name = "Thanh toán thành công")]
        SUCCESS = 2,

        [Display(Name = "Thanh toán thất bại")]
        FAILED = 3,

        [Display(Name = "Đã hủy thanh toán")]
        CANCELLED = 4,

        [Display(Name = "Chờ hoàn tiền")]
        REFUND_PENDING = 5,

        [Display(Name = "Đã hoàn tiền")]
        REFUNDED = 6
    }
}