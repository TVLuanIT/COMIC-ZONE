using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Extensions
{
    public static class ArtistQueryExtensions
    {
        public static IQueryable<Artist> ApplyArtistFilters(this IQueryable<Artist> query, ArtistSearchModel search)
        {
            if (search == null) return query;

            // 1. Keyword search
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.ToLower();
                query = query.Where(a => 
                    (a.Name != null && a.Name.ToLower().Contains(keyword)) ||
                    (a.Id.ToString() == keyword)
                );
            }

            // 2. Exact Field Matches
            if (search.Id.HasValue)
                query = query.Where(a => a.Id == search.Id.Value);

            if (!string.IsNullOrWhiteSpace(search.Name))
                query = query.Where(a => a.Name != null && a.Name.Contains(search.Name));

            // 3. Product associations
            if (search.ProductIds != null && search.ProductIds.Any())
                query = query.Where(a => a.Products.Any(p => search.ProductIds.Contains(p.Id)));

            if (!string.IsNullOrWhiteSpace(search.ProductNames))
            {
                var products = search.ProductNames.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim().ToLower());
                foreach (var product in products)
                {
                    query = query.Where(a => a.Products.Any(p => p.Name != null && p.Name.ToLower().Contains(product)));
                }
            }

            // 4. Metrics & Flags
            if (search.HasProducts.HasValue)
            {
                if (search.HasProducts.Value) query = query.Where(a => a.Products.Any());
                else query = query.Where(a => !a.Products.Any());
            }

            if (search.UnusedArtistsOnly == true)
                query = query.Where(a => !a.Products.Any());

            if (search.ProductCountMin.HasValue)
                query = query.Where(a => a.Products.Count >= search.ProductCountMin.Value);

            if (search.ProductCountMax.HasValue)
                query = query.Where(a => a.Products.Count <= search.ProductCountMax.Value);

            // 5. Status
            if (search.IsDeleted.HasValue)
                query = query.Where(a => a.Isdeleted == search.IsDeleted.Value);

            return query;
        }
    }
}
