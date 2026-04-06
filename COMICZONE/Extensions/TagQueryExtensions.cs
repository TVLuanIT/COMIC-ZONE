using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Extensions
{
    public static class TagQueryExtensions
    {
        public static IQueryable<Tag> ApplyTagFilters(this IQueryable<Tag> query, TagSearchModel search)
        {
            if (search == null) return query;

            // 1. Keyword search
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.ToLower();
                query = query.Where(t => 
                    (t.Name != null && t.Name.ToLower().Contains(keyword)) ||
                    (t.Id.ToString() == keyword)
                );
            }

            // 2. Exact Field Matches
            if (search.Id.HasValue)
                query = query.Where(t => t.Id == search.Id.Value);

            if (!string.IsNullOrWhiteSpace(search.Name))
                query = query.Where(t => t.Name != null && t.Name.Contains(search.Name));

            // 3. Product associations
            if (search.ProductIds != null && search.ProductIds.Any())
                query = query.Where(t => t.Products.Any(p => search.ProductIds.Contains(p.Id)));

            if (!string.IsNullOrWhiteSpace(search.ProductNames))
            {
                var products = search.ProductNames.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim().ToLower());
                foreach (var product in products)
                {
                    query = query.Where(t => t.Products.Any(p => p.Name != null && p.Name.ToLower().Contains(product)));
                }
            }

            // 4. Metrics & Flags
            if (search.HasProducts.HasValue)
            {
                if (search.HasProducts.Value) query = query.Where(t => t.Products.Any());
                else query = query.Where(t => !t.Products.Any());
            }

            if (search.ProductCountMin.HasValue)
                query = query.Where(t => t.Products.Count >= search.ProductCountMin.Value);

            if (search.ProductCountMax.HasValue)
                query = query.Where(t => t.Products.Count <= search.ProductCountMax.Value);

            if (search.UnusedTagsOnly == true)
                query = query.Where(t => !t.Products.Any());

            if (search.PopularTagsOnly == true)
                query = query.Where(t => t.Products.Count > 10);

            // 5. Status
            if (search.IsDeleted.HasValue)
                query = query.Where(t => t.Isdeleted == search.IsDeleted.Value);

            return query;
        }
    }
}
