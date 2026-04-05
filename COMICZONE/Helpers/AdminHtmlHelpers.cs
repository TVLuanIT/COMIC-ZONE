using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace COMICZONE.Helpers
{
    public static class AdminHtmlHelpers
    {
        public static IHtmlContent SortHeader(this IHtmlHelper html, string displayName, string columnName, string currentSort, bool isAscending, 
            string? keyword = null, string? statusFilter = null, string? roleFilter = null, string? typeFilter = null)
        {
            var isCurrent = string.Equals(columnName, currentSort, StringComparison.OrdinalIgnoreCase);
            var nextDirection = isCurrent ? !isAscending : true;
            var iconClass = "ti-arrows-sort";

            if (isCurrent)
            {
                iconClass = isAscending ? "ti-arrow-narrow-up text-primary fw-bold" : "ti-arrow-narrow-down text-primary fw-bold";
            }

            var url = $"?keyword={keyword}&statusFilter={statusFilter}&roleFilter={roleFilter}&typeFilter={typeFilter}" +
                      $"&sortColumn={columnName}&isAscending={nextDirection.ToString().ToLower()}&page=1";
            
            return new HtmlString($"""
                <a href='{url}' class='text-decoration-none text-uppercase small text-secondary d-flex align-items-center gap-1 {(isCurrent ? "fw-bold" : "")}'>
                    {displayName}
                    <i class='ti {iconClass}'></i>
                </a>
                """);
        }
    }
}
