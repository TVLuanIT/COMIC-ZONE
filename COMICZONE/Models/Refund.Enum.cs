using System.ComponentModel.DataAnnotations.Schema;
using COMICZONE.Helpers;
using COMICZONE.Models.Enums;

namespace COMICZONE.Models
{
    public partial class Refund
    {
        [NotMapped]
        public RefundStatus RefundStatusEnum
        {
            get
            {
                if (string.IsNullOrEmpty(Status)) return RefundStatus.Pending;
                return EnumHelper.ParseOrThrow<RefundStatus>(Status);
            }
            set => Status = value.ToString();
        }
    }
}
