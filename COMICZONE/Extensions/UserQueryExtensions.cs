using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Extensions
{
    public static class UserQueryExtensions
    {
        public static IQueryable<User> ApplyUserFilters(this IQueryable<User> query, UserSearchModel search)
        {
            if (search == null) return query;

            // 1. Keyword search (Username, Email, ID, FullName)
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.ToLower();
                query = query.Where(u => 
                    u.Id.ToString() == keyword ||
                    u.Username.ToLower().Contains(keyword) ||
                    (u.Email != null && u.Email.ToLower().Contains(keyword)) ||
                    (u.Customer != null && u.Customer.Fullname != null && u.Customer.Fullname.ToLower().Contains(keyword))
                );
            }

            // 2. Exact Field Matches
            if (search.Id.HasValue)
                query = query.Where(u => u.Id == search.Id.Value);

            if (!string.IsNullOrWhiteSpace(search.Username))
                query = query.Where(u => u.Username.Contains(search.Username));

            if (!string.IsNullOrWhiteSpace(search.Email))
                query = query.Where(u => u.Email != null && u.Email.Contains(search.Email));

            // 3. Roles & Statuses
            if (search.Roles != null && search.Roles.Any())
                query = query.Where(u => search.Roles.Contains(u.Role));

            if (search.IsActive.HasValue)
                query = query.Where(u => u.Isactive == search.IsActive.Value);

            if (search.IsDeleted.HasValue)
                query = query.Where(u => u.Isdeleted == search.IsDeleted.Value);

            // 4. Creation Timeline
            if (search.CreatedFrom.HasValue)
                query = query.Where(u => u.Createdat >= search.CreatedFrom.Value);

            if (search.CreatedTo.HasValue)
            {
                var toDate = search.CreatedTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(u => u.Createdat <= toDate);
            }

            // 5. Customer Profile Info
            if (!string.IsNullOrWhiteSpace(search.CustomerFullName))
                query = query.Where(u => u.Customer != null && u.Customer.Fullname != null && u.Customer.Fullname.Contains(search.CustomerFullName));

            if (!string.IsNullOrWhiteSpace(search.CustomerPhoneNumber))
                query = query.Where(u => u.Customer != null && u.Customer.Phone != null && u.Customer.Phone.Contains(search.CustomerPhoneNumber));

            // 6. Activity Counts (Using GroupBy/Counts might be slow on large datasets, but for standard admin it's fine)
            // Note: EF might translate .Count() into subqueries which is standard.

            if (search.HasOrders == true)
                query = query.Where(u => u.Orders.Any());
            else if (search.HasOrders == false)
                query = query.Where(u => !u.Orders.Any());

            if (search.OrderCountMin.HasValue)
                query = query.Where(u => u.Orders.Count() >= search.OrderCountMin.Value);

            if (search.OrderCountMax.HasValue)
                query = query.Where(u => u.Orders.Count() <= search.OrderCountMax.Value);

            if (search.HasReviews == true)
                query = query.Where(u => u.ProductReviews.Any());

            if (search.ReviewCountMin.HasValue)
                query = query.Where(u => u.ProductReviews.Count() >= search.ReviewCountMin.Value);

            if (search.ReviewCountMax.HasValue)
                query = query.Where(u => u.ProductReviews.Count() <= search.ReviewCountMax.Value);

            if (search.HasBlogs == true)
                query = query.Where(u => u.Blogs.Any());

            if (search.BlogCountMin.HasValue)
                query = query.Where(u => u.Blogs.Count() >= search.BlogCountMin.Value);

            if (search.BlogCountMax.HasValue)
                query = query.Where(u => u.Blogs.Count() <= search.BlogCountMax.Value);

            // 7. System / Security / Meta
            if (search.HasNotifications == true)
                query = query.Where(u => u.NotificationUsers.Any());

            if (search.HasViolations == true)
                query = query.Where(u => u.ViolationReports.Any());

            if (search.ViolationCountMin.HasValue)
                query = query.Where(u => u.ViolationReports.Count() >= search.ViolationCountMin.Value);

            if (search.HasResetToken == true)
                query = query.Where(u => !string.IsNullOrEmpty(u.ResetToken));

            if (search.ResetTokenExpireFrom.HasValue)
                query = query.Where(u => u.ResetTokenExpire >= search.ResetTokenExpireFrom.Value);

            if (search.ResetTokenExpireTo.HasValue)
                query = query.Where(u => u.ResetTokenExpire <= search.ResetTokenExpireTo.Value);

            if (search.HasAvatar == true)
                query = query.Where(u => !string.IsNullOrEmpty(u.Avatar));
            else if (search.HasAvatar == false)
                query = query.Where(u => string.IsNullOrEmpty(u.Avatar));

            return query;
        }
    }
}
