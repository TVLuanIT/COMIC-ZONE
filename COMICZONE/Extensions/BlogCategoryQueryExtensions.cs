using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;

namespace COMICZONE.Extensions
{
    public static class BlogCategoryQueryExtensions
    {
        public static IQueryable<BlogCategory> ApplyBlogCategoryFilters(this IQueryable<BlogCategory> query, BlogCategorySearchModel search)
        {
            if (search == null) return query;

            if (search.IsDeleted.HasValue)
            {
                query = query.Where(c => c.Isdeleted == search.IsDeleted.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.ToLower();
                query = query.Where(c => 
                    (c.Name != null && c.Name.ToLower().Contains(keyword)) ||
                    (c.Slug != null && c.Slug.ToLower().Contains(keyword))
                );
            }

            return query;
        }
    }
}
