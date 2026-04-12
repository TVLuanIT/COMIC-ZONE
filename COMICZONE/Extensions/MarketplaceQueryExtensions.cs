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

        // ==================== ORDERS ====================
        public static IQueryable<MarketplaceOrder> ApplyMarketplaceOrderSearch(this IQueryable<MarketplaceOrder> query, MarketplaceOrderSearchModel request)
        {
            if (request == null) return query;

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                string kw = request.Keyword.Trim();
                query = query.Where(o => o.Id.ToString() == kw ||
                                         (o.Post != null && o.Post.Title.Contains(kw)) ||
                                         (o.Buyer != null && o.Buyer.Username.Contains(kw)) ||
                                         (o.Seller != null && o.Seller.Username.Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(o => o.Status == request.Status);

            if (!string.IsNullOrWhiteSpace(request.BuyerUsername))
                query = query.Where(o => o.Buyer != null && o.Buyer.Username.Contains(request.BuyerUsername));

            if (!string.IsNullOrWhiteSpace(request.SellerUsername))
                query = query.Where(o => o.Seller != null && o.Seller.Username.Contains(request.SellerUsername));

            if (request.BuyerId.HasValue)
                query = query.Where(o => o.Buyerid == request.BuyerId.Value);

            if (request.SellerId.HasValue)
                query = query.Where(o => o.Sellerid == request.SellerId.Value);

            if (request.PriceFrom.HasValue)
                query = query.Where(o => o.Price >= request.PriceFrom.Value);

            if (request.PriceTo.HasValue)
                query = query.Where(o => o.Price <= request.PriceTo.Value);

            if (request.CreatedFrom.HasValue)
                query = query.Where(o => o.Createdat >= request.CreatedFrom.Value);

            if (request.CreatedTo.HasValue)
                query = query.Where(o => o.Createdat <= request.CreatedTo.Value);

            return query;
        }

        public static IQueryable<MarketplaceOrder> ApplyMarketplaceOrderSort(this IQueryable<MarketplaceOrder> query, string? sortColumn, bool isAscending)
        {
            if (string.IsNullOrWhiteSpace(sortColumn))
                return isAscending ? query.OrderBy(o => o.Createdat) : query.OrderByDescending(o => o.Createdat);

            return sortColumn.ToLower() switch
            {
                "id" => isAscending ? query.OrderBy(o => o.Id) : query.OrderByDescending(o => o.Id),
                "buyer" => isAscending ? query.OrderBy(o => o.Buyer.Username) : query.OrderByDescending(o => o.Buyer.Username),
                "seller" => isAscending ? query.OrderBy(o => o.Seller.Username) : query.OrderByDescending(o => o.Seller.Username),
                "price" => isAscending ? query.OrderBy(o => o.Price) : query.OrderByDescending(o => o.Price),
                "status" => isAscending ? query.OrderBy(o => o.Status) : query.OrderByDescending(o => o.Status),
                "createdat" => isAscending ? query.OrderBy(o => o.Createdat) : query.OrderByDescending(o => o.Createdat),
                _ => isAscending ? query.OrderBy(o => o.Createdat) : query.OrderByDescending(o => o.Createdat)
            };
        }

        // ==================== REVIEWS ====================
        public static IQueryable<MarketplaceReview> ApplyMarketplaceReviewSearch(this IQueryable<MarketplaceReview> query, MarketplaceReviewSearchModel request)
        {
            if (request == null) return query;

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                string kw = request.Keyword.Trim();
                query = query.Where(r => (r.Comment != null && r.Comment.Contains(kw)) ||
                                         r.Id.ToString() == kw ||
                                         (r.Reviewer != null && r.Reviewer.Username.Contains(kw)));
            }

            if (request.Rating.HasValue)
                query = query.Where(r => r.Rating == request.Rating.Value);

            if (request.MinRating.HasValue)
                query = query.Where(r => r.Rating >= request.MinRating.Value);

            if (request.MaxRating.HasValue)
                query = query.Where(r => r.Rating <= request.MaxRating.Value);

            if (!string.IsNullOrWhiteSpace(request.ReviewerUsername))
                query = query.Where(r => r.Reviewer != null && r.Reviewer.Username.Contains(request.ReviewerUsername));

            if (request.CreatedFrom.HasValue)
                query = query.Where(r => r.Createdat >= request.CreatedFrom.Value);

            if (request.CreatedTo.HasValue)
                query = query.Where(r => r.Createdat <= request.CreatedTo.Value);

            return query;
        }

        public static IQueryable<MarketplaceReview> ApplyMarketplaceReviewSort(this IQueryable<MarketplaceReview> query, string? sortColumn, bool isAscending)
        {
            if (string.IsNullOrWhiteSpace(sortColumn))
                return isAscending ? query.OrderBy(r => r.Createdat) : query.OrderByDescending(r => r.Createdat);

            return sortColumn.ToLower() switch
            {
                "id" => isAscending ? query.OrderBy(r => r.Id) : query.OrderByDescending(r => r.Id),
                "rating" => isAscending ? query.OrderBy(r => r.Rating) : query.OrderByDescending(r => r.Rating),
                "reviewer" => isAscending ? query.OrderBy(r => r.Reviewer.Username) : query.OrderByDescending(r => r.Reviewer.Username),
                "createdat" => isAscending ? query.OrderBy(r => r.Createdat) : query.OrderByDescending(r => r.Createdat),
                _ => isAscending ? query.OrderBy(r => r.Createdat) : query.OrderByDescending(r => r.Createdat)
            };
        }
    }
}
