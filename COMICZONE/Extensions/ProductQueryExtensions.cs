using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Extensions
{
    public static class ProductQueryExtensions
    {
        public static IQueryable<Product> ApplyProductFilters(this IQueryable<Product> query, ProductSearchModel search)
        {
            if (search == null) return query;

            // 1. Keyword (Matches across multiple fields)
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.ToLower();
                query = query.Where(p => 
                    (p.Name != null && p.Name.ToLower().Contains(keyword)) ||
                    (p.Author != null && p.Author.ToLower().Contains(keyword)) ||
                    (p.Distributor != null && p.Distributor.ToLower().Contains(keyword)) ||
                    (p.Series != null && p.Series.ToLower().Contains(keyword)) ||
                    (p.Description != null && p.Description.ToLower().Contains(keyword)) ||
                    (p.Translator != null && p.Translator.ToLower().Contains(keyword)) ||
                    (p.Publisher != null && p.Publisher.ToLower().Contains(keyword))
                );
            }

            // 2. Exact Field Matches
            if (search.Id.HasValue) 
                query = query.Where(p => p.Id == search.Id.Value);

            if (!string.IsNullOrWhiteSpace(search.Name))
                query = query.Where(p => p.Name != null && p.Name.Contains(search.Name));

            if (!string.IsNullOrWhiteSpace(search.Author))
                query = query.Where(p => p.Author != null && p.Author.Contains(search.Author));

            if (!string.IsNullOrWhiteSpace(search.Translator))
                query = query.Where(p => p.Translator != null && p.Translator.Contains(search.Translator));

            if (!string.IsNullOrWhiteSpace(search.Series))
                query = query.Where(p => p.Series != null && p.Series.Contains(search.Series));

            if (!string.IsNullOrWhiteSpace(search.Publisher))
                query = query.Where(p => p.Publisher != null && p.Publisher.Contains(search.Publisher));

            if (!string.IsNullOrWhiteSpace(search.Distributor))
                query = query.Where(p => p.Distributor != null && p.Distributor.Contains(search.Distributor));

            if (!string.IsNullOrWhiteSpace(search.Description))
                query = query.Where(p => p.Description != null && p.Description.Contains(search.Description));

            // 3. Format & Specs
            if (!string.IsNullOrWhiteSpace(search.Format))
                query = query.Where(p => p.Format == search.Format);

            if (!string.IsNullOrWhiteSpace(search.Size))
                query = query.Where(p => p.Size != null && p.Size.Contains(search.Size));

            if (!string.IsNullOrWhiteSpace(search.Weight))
                query = query.Where(p => p.Weight != null && p.Weight.Contains(search.Weight));

            if (search.Pages.HasValue)
                query = query.Where(p => p.Pages == search.Pages.Value);

            if (!string.IsNullOrWhiteSpace(search.IllustrationType))
                query = query.Where(p => p.IllustrationType == search.IllustrationType);

            if (!string.IsNullOrWhiteSpace(search.AgeGroup))
                query = query.Where(p => p.AgeGroup == search.AgeGroup);

            // 4. Price & Stock Ranges
            if (search.PriceMin.HasValue)
                query = query.Where(p => p.Price >= search.PriceMin.Value);

            if (search.PriceMax.HasValue)
                query = query.Where(p => p.Price <= search.PriceMax.Value);

            if (search.StockQuantityMin.HasValue)
                query = query.Where(p => p.StockQuantity >= search.StockQuantityMin.Value);

            if (search.StockQuantityMax.HasValue)
                query = query.Where(p => p.StockQuantity <= search.StockQuantityMax.Value);

            // 5. Stock Status
            if (!string.IsNullOrWhiteSpace(search.StockStatus))
            {
                switch (search.StockStatus.ToLower())
                {
                    case "available":
                        query = query.Where(p => p.StockQuantity > 0);
                        break;
                    case "outofstock":
                        query = query.Where(p => p.StockQuantity <= 0);
                        break;
                    case "lowstock":
                        query = query.Where(p => p.StockQuantity > 0 && p.StockQuantity < 10);
                        break;
                }
            }

            // 6. Release Date Range
            if (search.ReleaseDateFrom.HasValue)
            {
                var fromDate = DateOnly.FromDateTime(search.ReleaseDateFrom.Value);
                query = query.Where(p => p.ReleaseDate >= fromDate);
            }

            if (search.ReleaseDateTo.HasValue)
            {
                var toDate = DateOnly.FromDateTime(search.ReleaseDateTo.Value);
                query = query.Where(p => p.ReleaseDate <= toDate);
            }

            // 7. Collections (Tags & Artists)
            if (search.TagIds != null && search.TagIds.Any())
                query = query.Where(p => p.Tags.Any(t => search.TagIds.Contains(t.Id)));

            if (!string.IsNullOrWhiteSpace(search.TagNames))
            {
                var tags = search.TagNames.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim().ToLower());
                foreach (var tag in tags)
                {
                    query = query.Where(p => p.Tags.Any(t => t.Name != null && t.Name.ToLower().Contains(tag)));
                }
            }

            if (search.ArtistIds != null && search.ArtistIds.Any())
                query = query.Where(p => p.Artists.Any(a => search.ArtistIds.Contains(a.Id)));

            if (!string.IsNullOrWhiteSpace(search.ArtistNames))
            {
                var artists = search.ArtistNames.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim().ToLower());
                foreach (var artist in artists)
                {
                    query = query.Where(p => p.Artists.Any(a => a.Name != null && a.Name.ToLower().Contains(artist)));
                }
            }

            // 8. Existence checks
            if (search.HasReviews.HasValue)
            {
                if (search.HasReviews.Value) query = query.Where(p => p.ProductReviews.Any());
                else query = query.Where(p => !p.ProductReviews.Any());
            }

            if (search.HasPictures.HasValue)
            {
                if (search.HasPictures.Value) query = query.Where(p => p.Pictures.Any());
                else query = query.Where(p => !p.Pictures.Any());
            }

            if (search.HasOrders.HasValue)
            {
                if (search.HasOrders.Value) query = query.Where(p => p.OrderItems.Any());
                else query = query.Where(p => !p.OrderItems.Any());
            }

            // 9. Status
            if (search.IsDeleted.HasValue)
                query = query.Where(p => p.Isdeleted == search.IsDeleted.Value);

            return query;
        }
    }
}
