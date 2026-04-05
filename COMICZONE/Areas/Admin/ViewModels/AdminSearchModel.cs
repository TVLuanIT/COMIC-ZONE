namespace COMICZONE.Areas.Admin.ViewModels
{
    public class AdminSearchModel
    {
        public string? Keyword { get; set; }
        public string? SortColumn { get; set; }
        public bool IsAscending { get; set; } = true;

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Backward compatibility
        public int PageNumber { get => Page; set => Page = value; }
        public int TotalItems { get => TotalCount; set => TotalCount = value; }

        // Advanced Filters
        public string? StatusFilter { get; set; }
        public string? RoleFilter { get; set; }
        public string? TypeFilter { get; set; }

        public string GetSortDirection() => IsAscending ? "asc" : "desc";
    }
}
