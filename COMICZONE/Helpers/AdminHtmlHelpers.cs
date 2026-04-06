using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace COMICZONE.Helpers
{
    public static class AdminHtmlHelpers
    {
        public static IHtmlContent SortHeader(this IHtmlHelper html, string displayName, string columnName, string currentSort, bool isAscending, string? prefix = null)
        {
            var isCurrent = string.Equals(columnName, currentSort, StringComparison.OrdinalIgnoreCase);
            var nextDirection = isCurrent ? !isAscending : true;
            var iconClass = "ti-arrows-sort text-muted opacity-50";

            if (isCurrent)
            {
                iconClass = isAscending ? "ti-arrow-narrow-up text-primary fw-bold" : "ti-arrow-narrow-down text-primary fw-bold";
            }

            // Get current query parameters to preserve them
            var query = html.ViewContext.HttpContext.Request.Query;
            var routeValues = new Dictionary<string, string>();

            foreach (var key in query.Keys)
            {
                routeValues[key] = query[key]!;
            }

            // Override sort parameters
            var sortPrefix = string.IsNullOrEmpty(prefix) ? "" : prefix + ".";
            routeValues[sortPrefix + "SortColumn"] = columnName;
            routeValues[sortPrefix + "IsAscending"] = nextDirection.ToString().ToLower();
            routeValues[sortPrefix + "Page"] = "1"; // Reset to page 1 on sort

            // Rebuild URL
            var url = "?" + string.Join("&", routeValues.Select(r => $"{r.Key}={Uri.EscapeDataString(r.Value)}"));

            return new HtmlString($"""
                <a href='{url}' class='text-decoration-none text-uppercase small text-secondary d-flex align-items-center gap-1 {(isCurrent ? "fw-bold text-dark" : "")}'>
                    {displayName}
                    <i class='ti {iconClass}'></i>
                </a>
                """);
        }

        // Backward compatibility overload
        public static IHtmlContent SortHeader(this IHtmlHelper html, string displayName, string columnName, string currentSort, bool isAscending, 
            string? keyword, string? statusFilter, string? roleFilter, string? typeFilter)
        {
            // Simply call the new generic version. 
            // The new version will automatically pick up filters from the current Request.Query.
            return SortHeader(html, displayName, columnName, currentSort, isAscending, null);
        }
    }
}
