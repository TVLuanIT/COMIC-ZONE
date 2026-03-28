using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Enums
{
    public enum PaymentTransactionStatus
    {
        [Display(Name = "Chờ xử lý")]
        Pending = 1,

        [Display(Name = "Thành công")]
        Success = 2,

        [Display(Name = "Thất bại")]
        Failed = 3,

        [Display(Name = "Hoàn tiền")]
        Refunded = 4
    }
}
