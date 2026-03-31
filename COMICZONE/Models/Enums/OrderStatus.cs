using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Enums
{
    public enum OrderStatus
    {
        [Display(Name = "Chờ xử lý")]
        Pending = 1,

        [Display(Name = "Đang xử lý")]
        Processing = 4,

        [Display(Name = "Đang giao hàng")]
        Shipping = 5,

        [Display(Name = "Hoàn thành")]
        Completed = 2,

        [Display(Name = "Đã hủy")]
        Cancelled = 3
    }
}
