namespace COMICZONE.Models
{
public class PaginationModel
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }

    public required string Action { get; set; }
    public required string Controller { get; set; }

    // Tên param page (page / reviewPage / replyPage)
    public string PageParam { get; set; } = "page";

    // Các query cần giữ lại
    public required Dictionary<string, string> ExtraParams { get; set; }
}
}