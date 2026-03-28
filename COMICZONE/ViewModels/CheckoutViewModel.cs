using System.ComponentModel.DataAnnotations;
using COMICZONE.Models.Enums;

namespace COMICZONE.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        public string Fullname { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string Address { get; set; }

        public string? Note { get; set; }

        [Required(ErrorMessage = "Chọn phương thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; }

        // Transaction ID do PayPal trả về (nếu phương thức là PayPal)
        public string? PaypalTransactionId { get; set; }
    }
}
