namespace COMICZONE.Models
{
    public class PaginationModel
    {
        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public required string Action { get; set; }

        public required string Controller { get; set; }
    }
}