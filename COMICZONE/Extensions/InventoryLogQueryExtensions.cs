using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Extensions
{
    public static class InventoryLogQueryExtensions
    {
        public static IQueryable<InventoryLog> ApplyInventoryLogFilters(this IQueryable<InventoryLog> query, InventoryLogSearchModel search)
        {
            if (search == null) return query;

            // 1. Keyword search (Across multiple fields)
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.ToLower();
                query = query.Where(i => 
                    (i.Product.Name != null && i.Product.Name.ToLower().Contains(keyword)) ||
                    (i.Type != null && i.Type.ToLower().Contains(keyword)) ||
                    (i.Id.ToString() == keyword) ||
                    (i.ProductId.ToString() == keyword)
                );
            }

            // 2. Exact Field Matches
            if (search.Id.HasValue)
                query = query.Where(i => i.Id == search.Id.Value);

            if (search.ProductId.HasValue)
                query = query.Where(i => i.ProductId == search.ProductId.Value);

            if (!string.IsNullOrWhiteSpace(search.ProductName))
                query = query.Where(i => i.Product.Name != null && i.Product.Name.Contains(search.ProductName));

            // 3. Transaction Types
            if (!string.IsNullOrWhiteSpace(search.Type))
            {
                query = query.Where(i => i.Type == search.Type);
            }
            else if (search.Types != null && search.Types.Any())
            {
                query = query.Where(i => i.Type != null && search.Types.Contains(i.Type));
            }

            // 4. Change Amount & Directional Filters
            if (search.ChangeAmountMin.HasValue)
                query = query.Where(i => i.ChangeAmount >= search.ChangeAmountMin.Value);

            if (search.ChangeAmountMax.HasValue)
                query = query.Where(i => i.ChangeAmount <= search.ChangeAmountMax.Value);

            if (search.IncreaseOnly == true)
                query = query.Where(i => i.ChangeAmount > 0);

            if (search.DecreaseOnly == true)
                query = query.Where(i => i.ChangeAmount < 0);

            // 5. Date Range
            if (search.CreatedFrom.HasValue)
                query = query.Where(i => i.CreatedAt >= search.CreatedFrom.Value);

            if (search.CreatedTo.HasValue)
            {
                // Set to end of day to include all transactions on that day
                var toDate = search.CreatedTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(i => i.CreatedAt <= toDate);
            }

            // 6. Magnitude Filter
            if (search.LargeChangesOnly == true)
                query = query.Where(i => i.ChangeAmount >= 50 || i.ChangeAmount <= -50);

            return query;
        }
    }
}
