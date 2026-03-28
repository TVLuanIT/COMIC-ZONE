namespace COMICZONE.Models.Requests
{
    public class CreateOrderConRequest
    {
        public int UserId { get; set; }

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public string? Note { get; set; }

        public string? PaymentMethod { get; set; }

        public bool IsPaid { get; set; }

        public string? TransactionId { get; set; }
    }
}
