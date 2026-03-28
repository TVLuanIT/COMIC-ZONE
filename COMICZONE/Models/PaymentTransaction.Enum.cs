using System.ComponentModel.DataAnnotations.Schema;
using COMICZONE.Helpers;
using COMICZONE.Models.Enums;

namespace COMICZONE.Models
{
    public partial class PaymentTransaction
    {
        [NotMapped]
        public PaymentTransactionStatus PaymentTransactionStatusEnum
        {
            get => EnumHelper.ParseOrThrow<PaymentTransactionStatus>(Status);

            set => Status = value.ToString();
        }
    }
}
