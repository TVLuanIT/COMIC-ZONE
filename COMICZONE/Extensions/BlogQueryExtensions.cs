using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;

namespace COMICZONE.Extensions
{
    public static class BlogQueryExtensions
    {
        public static IQueryable<Blog> ApplyBlogFilters(this IQueryable<Blog> query, BlogSearchModel search)
        {
            if (search == null) return query;

            if (search.IsDeleted.HasValue)
            {
                query = query.Where(b => b.Isdeleted == search.IsDeleted.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.ToLower();
                query = query.Where(b => 
                    (b.Title != null && b.Title.ToLower().Contains(keyword)) ||
                    (b.Slug != null && b.Slug.ToLower().Contains(keyword)) ||
                    (b.Shortdescription != null && b.Shortdescription.ToLower().Contains(keyword)) ||
                    (b.Content != null && b.Content.ToLower().Contains(keyword))
                );
            }

            if (search.CategoryId.HasValue)
            {
                query = query.Where(b => b.Categories.Any(c => c.Id == search.CategoryId.Value));
            }

            if (!string.IsNullOrWhiteSpace(search.StatusFilter))
            {
                query = query.Where(b => b.Status == search.StatusFilter);
            }

            return query;
        }
    }
}
