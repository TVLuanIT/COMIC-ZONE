using Microsoft.EntityFrameworkCore;
using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;

namespace COMICZONE.Extensions
{
    public static class NotificationQueryExtensions
    {
        public static IQueryable<Notification> ApplySearch(this IQueryable<Notification> query, NotificationSearchRequest request)
        {
            if (request == null) return query;

            // 1. Keyword search (Across Multiple Fields)
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(n => n.Title.Contains(keyword) ||
                                         n.Message.Contains(keyword) ||
                                         n.User.Username.Contains(keyword) ||
                                         n.User.Email.Contains(keyword) ||
                                         (n.CreatedByNavigation != null && n.CreatedByNavigation.Username.Contains(keyword)));
            }

            // 2. Specific Field Filters
            if (request.NotificationId.HasValue)
            {
                query = query.Where(n => n.NotificationId == request.NotificationId.Value);
            }

            // User (Recipient) Filters
            if (request.UserId.HasValue)
            {
                query = query.Where(n => n.UserId == request.UserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                query = query.Where(n => n.User.Username.Contains(request.Username.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(request.UserEmail))
            {
                query = query.Where(n => n.User.Email.Contains(request.UserEmail.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(request.CustomerPhoneNumber))
            {
                query = query.Where(n => n.User.Customer != null && n.User.Customer.Phone != null && n.User.Customer.Phone.Contains(request.CustomerPhoneNumber.Trim()));
            }

            // CreatedBy (Sender) Filters
            if (request.CreatedById.HasValue)
            {
                query = query.Where(n => n.CreatedBy == request.CreatedById.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.CreatedByUsername))
            {
                query = query.Where(n => n.CreatedByNavigation != null && n.CreatedByNavigation.Username.Contains(request.CreatedByUsername.Trim()));
            }

            // Status Filters
            if (request.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == request.IsRead.Value);
            }

            if (request.UnreadOnly)
            {
                query = query.Where(n => n.IsRead == false || n.IsRead == null);
            }

            if (request.ReadOnly)
            {
                query = query.Where(n => n.IsRead == true);
            }

            // Content Filters
            if (!string.IsNullOrWhiteSpace(request.TitleKeyword))
            {
                query = query.Where(n => n.Title.Contains(request.TitleKeyword.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(request.MessageKeyword))
            {
                query = query.Where(n => n.Message.Contains(request.MessageKeyword.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(request.LinkKeyword))
            {
                query = query.Where(n => n.Link != null && n.Link.Contains(request.LinkKeyword.Trim()));
            }

            // Date Filters
            if (request.CreatedFrom.HasValue)
            {
                query = query.Where(n => n.CreatedAt >= request.CreatedFrom.Value);
            }

            if (request.CreatedTo.HasValue)
            {
                // To the end of the day
                var endOfDay = request.CreatedTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(n => n.CreatedAt <= endOfDay);
            }

            // Soft Delete Filter
            query = query.Where(n => n.Isdeleted == request.IsDeleted);

            // Type Filters
            if (request.SystemOnly)
            {
                query = query.Where(n => n.CreatedBy == null);
            }

            if (request.ManualOnly)
            {
                query = query.Where(n => n.CreatedBy != null);
            }

            if (request.UnreadByUserOnly)
            {
                query = query.Where(n => n.UserId != null && (n.IsRead == false || n.IsRead == null));
            }

            return query;
        }
    }
}
