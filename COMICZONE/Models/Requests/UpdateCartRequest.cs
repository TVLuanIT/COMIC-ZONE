namespace COMICZONE.Models.Requests
{
    public class UpdateCartRequest
    {
        public int CartItemId { get; set; }

        public int Quantity { get; set; }
    }
}
