using System;
using System.Linq;
using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;

namespace COMICZONE.Extensions
{
    public static class MarketplaceQueryExtensions
    {
        // ==================== POSTS ====================
        public static IQueryable<MarketplacePost> ApplyMarketplacePostSearch(this IQueryable<MarketplacePost> query, MarketplacePostSearchModel request)
        {
            if (request == null) return query;

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                string kw = request.Keyword.Trim();
                query = query.Where(p => p.Title.Contains(kw) ||
                                         (p.Description != null && p.Description.Contains(kw)) ||
                                         p.Id.ToString() == kw ||
                                         (p.Seller != null && p.Seller.Username.Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(p => p.Status == request.Status);

            if (!string.IsNullOrWhiteSpace(request.Category))
                query = query.Where(p => p.Category == request.Category);

            if (!string.IsNullOrWhiteSpace(request.Condition))
                query = query.Where(p => p.Condition == request.Condition);

            if (!string.IsNullOrWhiteSpace(request.SellerUsername))
                query = query.Where(p => p.Seller != null && p.Seller.Username.Contains(request.SellerUsername));

            if (request.SellerId.HasValue)
                query = query.Where(p => p.Sellerid == request.SellerId.Value);

            if (request.PriceFrom.HasValue)
                query = query.Where(p => p.Price >= request.PriceFrom.Value);

            if (request.PriceTo.HasValue)
                query = query.Where(p => p.Price <= request.PriceTo.Value);

            if (request.CreatedFrom.HasValue)
                query = query.Where(p => p.Createdat >= request.CreatedFrom.Value);

            if (request.CreatedTo.HasValue)
                query = query.Where(p => p.Createdat <= request.CreatedTo.Value);

            if (request.IsDeleted.HasValue)
            {
                bool showDeleted = request.IsDeleted.Value;
                query = query.Where(p => (p.Isdeleted ?? false) == showDeleted);
            }

            return query;
        }

        public static IQueryable<MarketplacePost> ApplyMarketplacePostSort(this IQueryable<MarketplacePost> query, string? sortColumn, bool isAscending)
        {
            if (string.IsNullOrWhiteSpace(sortColumn))
                return isAscending ? query.OrderBy(p => p.Createdat) : query.OrderByDescending(p => p.Createdat);

            return sortColumn.ToLower() switch
            {
                "id" => isAscending ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id),
                "title" => isAscending ? query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title),
                "price" => isAscending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                "seller" => isAscending ? query.OrderBy(p => p.Seller.Username) : query.OrderByDescending(p => p.Seller.Username),
                "status" => isAscending ? query.OrderBy(p => p.Status) : query.OrderByDescending(p => p.Status),
                "category" => isAscending ? query.OrderBy(p => p.Category) : query.OrderByDescending(p => p.Category),
                "condition" => isAscending ? query.OrderBy(p => p.Condition) : query.OrderByDescending(p => p.Condition),
                "createdat" => isAscending ? query.OrderBy(p => p.Createdat) : query.OrderByDescending(p => p.Createdat),
                _ => isAscending ? query.OrderBy(p => p.Createdat) : query.OrderByDescending(p => p.Createdat)
            };
        }

    }
}
