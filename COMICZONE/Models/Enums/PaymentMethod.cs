using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Enums
{
    public enum PaymentMethod
    {
        [Display(Name = "Thanh toán khi nhận hàng")]
        COD = 1,

        [Display(Name = "VNPay")]
        VNPAY = 2,

        [Display(Name = "MoMo")]
        MOMO = 3,
    }
}