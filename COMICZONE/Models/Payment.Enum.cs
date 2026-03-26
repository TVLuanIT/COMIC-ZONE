using System.ComponentModel.DataAnnotations.Schema;
using COMICZONE.Helpers;
using COMICZONE.Models.Enums;

namespace COMICZONE.Models
{
    public partial class Payment
    {
        [NotMapped]
        public PaymentMethod PaymentMethodEnum
        {
            get => EnumHelper.ParseOrThrow<PaymentMethod>(Paymentmethod);

            set => Paymentmethod = value.ToString();
        }

        [NotMapped]
        public PaymentStatus PaymentStatusEnum
        {
            get => EnumHelper.ParseOrThrow<PaymentStatus>(Paymentstatus);

            set => Paymentstatus = value.ToString();
        }
    }
}