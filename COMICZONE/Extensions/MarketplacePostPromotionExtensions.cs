using System.Linq;
using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;

namespace COMICZONE.Extensions
{
    public static class MarketplacePostPromotionExtensions
    {
        public static IQueryable<MarketplacePostPromotion> ApplyPromotionSearch(this IQueryable<MarketplacePostPromotion> query, MarketplacePostPromotionSearchModel search)
        {
            if (!string.IsNullOrEmpty(search.Keyword))
            {
                var lowerKeyword = search.Keyword.ToLower();
                query = query.Where(p => 
                    (p.Post != null && p.Post.Title != null && p.Post.Title.ToLower().Contains(lowerKeyword)) ||
                    (p.User != null && p.User.Username != null && p.User.Username.ToLower().Contains(lowerKeyword)) ||
                    (p.PromotionType != null && p.PromotionType.ToLower().Contains(lowerKeyword))
                );
            }

            if (!string.IsNullOrEmpty(search.Status))
            {
                query = query.Where(p => p.Status == search.Status);
            }

            if (!string.IsNullOrEmpty(search.PromotionType))
            {
                query = query.Where(p => p.PromotionType == search.PromotionType);
            }

            if (search.CreatedFrom.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= search.CreatedFrom.Value);
            }

            if (search.CreatedTo.HasValue)
            {
                // Include the whole day for CreatedTo
                var endOfDay = search.CreatedTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.CreatedAt <= endOfDay);
            }

            if (search.IsDeleted.HasValue)
            {
                query = query.Where(p => p.Isdeleted == search.IsDeleted.Value);
            }

            return query;
        }

        public static IQueryable<MarketplacePostPromotion> ApplyPromotionSort(this IQueryable<MarketplacePostPromotion> query, string? sortColumn, bool isAscending)
        {
            query = (sortColumn?.ToLower()) switch
            {
                "id" => isAscending ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id),
                "post" => isAscending ? query.OrderBy(p => p.Post.Title) : query.OrderByDescending(p => p.Post.Title),
                "user" => isAscending ? query.OrderBy(p => p.User.Username) : query.OrderByDescending(p => p.User.Username),
                "type" => isAscending ? query.OrderBy(p => p.PromotionType) : query.OrderByDescending(p => p.PromotionType),
                "price" => isAscending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                "status" => isAscending ? query.OrderBy(p => p.Status) : query.OrderByDescending(p => p.Status),
                "startdate" => isAscending ? query.OrderBy(p => p.StartDate) : query.OrderByDescending(p => p.StartDate),
                "enddate" => isAscending ? query.OrderBy(p => p.EndDate) : query.OrderByDescending(p => p.EndDate),
                "createdat" => isAscending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            return query;
        }
    }
}
