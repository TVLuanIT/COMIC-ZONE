namespace COMICZONE.Models
{
    public class ChatbotIntent
    {
        public string Intent { get; set; }

        public string Keyword { get; set; }

        public decimal? MaxPrice { get; set; }

        public decimal? MinPrice { get; set; }

        public string Category { get; set; }

        public string OrderId { get; set; }
    }
}
