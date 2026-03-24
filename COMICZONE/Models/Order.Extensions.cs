using System.ComponentModel.DataAnnotations.Schema;

namespace COMICZONE.Models
{
    public partial class Order
    {
        [NotMapped]
        public string? TransactionId { get; set; }

        [NotMapped]
        public string? PaymentMethod { get; set; }
    }
}
