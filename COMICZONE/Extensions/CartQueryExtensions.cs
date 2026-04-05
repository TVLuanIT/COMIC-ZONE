using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Extensions
{
    public static class CartQueryExtensions
    {
        public static IQueryable<Cart> ApplyCartFilters(this IQueryable<Cart> query, CartSearchModel search)
        {
            if (search == null) return query;

            // 1. Keyword search (CartID, Username, Product Name)
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.ToLower();
                query = query.Where(c => 
                    c.CartId.ToString() == keyword ||
                    c.User.Username.ToLower().Contains(keyword) ||
                    (c.User.Email != null && c.User.Email.ToLower().Contains(keyword)) ||
                    c.CartItems.Any(ci => ci.Product.Name != null && ci.Product.Name.ToLower().Contains(keyword))
                );
            }

            // 2. Exact Field Matches
            if (search.CartId.HasValue)
                query = query.Where(c => c.CartId == search.CartId.Value);

            if (search.UserId.HasValue)
                query = query.Where(c => c.UserId == search.UserId.Value);

            // 3. User & Customer Context
            if (!string.IsNullOrWhiteSpace(search.Username))
                query = query.Where(c => c.User.Username.Contains(search.Username));

            if (!string.IsNullOrWhiteSpace(search.UserEmail))
                query = query.Where(c => c.User.Email != null && c.User.Email.Contains(search.UserEmail));

            if (!string.IsNullOrWhiteSpace(search.CustomerPhoneNumber))
                query = query.Where(c => c.User.Customer != null && c.User.Customer.Phone != null && c.User.Customer.Phone.Contains(search.CustomerPhoneNumber));

            // 4. Timeline
            if (search.CreatedFrom.HasValue)
                query = query.Where(c => c.CreatedAt >= search.CreatedFrom.Value);

            if (search.CreatedTo.HasValue)
            {
                var toDate = search.CreatedTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(c => c.CreatedAt <= toDate);
            }

            // 5. Product Logic
            if (search.ProductId.HasValue)
                query = query.Where(c => c.CartItems.Any(ci => ci.ProductId == search.ProductId.Value));

            if (!string.IsNullOrWhiteSpace(search.ProductName))
                query = query.Where(c => c.CartItems.Any(ci => ci.Product.Name != null && ci.Product.Name.Contains(search.ProductName)));

            // 6. Quantitative Logic
            if (search.HasItems == true)
                query = query.Where(c => c.CartItems.Any());
            else if (search.HasItems == false)
                query = query.Where(c => !c.CartItems.Any());

            if (search.EmptyCartOnly == true)
                query = query.Where(c => !c.CartItems.Any());

            if (search.ItemCountMin.HasValue)
                query = query.Where(c => c.CartItems.Count() >= search.ItemCountMin.Value);

            if (search.ItemCountMax.HasValue)
                query = query.Where(c => c.CartItems.Count() <= search.ItemCountMax.Value);

            // 7. Abandoned Cart Logic (Standard: > 24h old and still has items)
            if (search.AbandonedOnly == true)
            {
                var threshold = DateTime.Now.AddHours(-24);
                query = query.Where(c => c.CreatedAt < threshold && c.CartItems.Any());
            }

            // 8. Total Value Filtering
            // Since Total Value is calculated as Sum(Quantity * Price), we use subqueries.
            if (search.TotalValueMin.HasValue)
                query = query.Where(c => c.CartItems.Sum(ci => ci.Quantity * (ci.Product.Price ?? 0)) >= (double)search.TotalValueMin.Value);

            if (search.TotalValueMax.HasValue)
                query = query.Where(c => c.CartItems.Sum(ci => ci.Quantity * (ci.Product.Price ?? 0)) <= (double)search.TotalValueMax.Value);

            return query;
        }
    }
}
