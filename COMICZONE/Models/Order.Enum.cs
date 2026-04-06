using System.ComponentModel.DataAnnotations.Schema;
using COMICZONE.Helpers;
using COMICZONE.Models.Enums;

namespace COMICZONE.Models
{
    public partial class Order
    {
        [NotMapped]
        public OrderStatus OrderStatusEnum
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Status))
                    return OrderStatus.Pending;

                try
                {
                    return EnumHelper.ParseOrThrow<OrderStatus>(Status);
                }
                catch
                {
                    return OrderStatus.Pending;
                }
            }

            set => Status = value.ToString();
        }
    }
}
