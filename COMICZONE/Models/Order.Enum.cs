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
            get => EnumHelper.ParseOrThrow<OrderStatus>(Status);

            set => Status = value.ToString();
        }
    }
}
