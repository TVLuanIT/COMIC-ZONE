using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Extensions
{
    public static class RefundQueryExtensions
    {
        public static IQueryable<Refund> ApplyRefundFilters(this IQueryable<Refund> query, RefundSearchModel search)
        {
            if (search == null) return query;

            if (search.IsDeleted.HasValue)
            {
                query = query.Where(r => r.Isdeleted == search.IsDeleted.Value);
            }

            // 1. Keyword search (RefundId, PaymentId, Reason, Username)
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.ToLower();
                query = query.Where(r => 
                    r.Id.ToString() == keyword ||
                    r.PaymentId.ToString() == keyword ||
                    (r.Reason != null && r.Reason.ToLower().Contains(keyword)) ||
                    (r.Payment.Order.User.Username != null && r.Payment.Order.User.Username.ToLower().Contains(keyword))
                );
            }

            // 2. Exact Field Matches
            if (search.Id.HasValue)
                query = query.Where(r => r.Id == search.Id.Value);

            if (search.PaymentId.HasValue)
                query = query.Where(r => r.PaymentId == search.PaymentId.Value);

            if (search.OrderId.HasValue)
                query = query.Where(r => r.Payment.Orderid == search.OrderId.Value);

            // 3. Statuses & Methods
            if (search.Statuses != null && search.Statuses.Any())
                query = query.Where(r => r.Status != null && search.Statuses.Contains(r.Status));

            if (!string.IsNullOrWhiteSpace(search.PaymentStatus))
                query = query.Where(r => r.Payment.Paymentstatus == search.PaymentStatus);

            if (!string.IsNullOrWhiteSpace(search.PaymentMethod))
                query = query.Where(r => r.Payment.Paymentmethod == search.PaymentMethod);

            // 4. Amount Range
            if (search.AmountMin.HasValue)
                query = query.Where(r => r.Amount >= search.AmountMin.Value);

            if (search.AmountMax.HasValue)
                query = query.Where(r => r.Amount <= search.AmountMax.Value);

            // 5. Date Range
            if (search.CreatedFrom.HasValue)
                query = query.Where(r => r.CreatedAt >= search.CreatedFrom.Value);

            if (search.CreatedTo.HasValue)
            {
                var toDate = search.CreatedTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(r => r.CreatedAt <= toDate);
            }

            // 6. Order / User related
            if (!string.IsNullOrWhiteSpace(search.OrderUsername))
                query = query.Where(r => r.Payment.Order.User.Username.Contains(search.OrderUsername));

            if (!string.IsNullOrWhiteSpace(search.OrderPhoneNumber))
                query = query.Where(r => r.Payment.Order.PhoneNumber != null && r.Payment.Order.PhoneNumber.Contains(search.OrderPhoneNumber));

            if (!string.IsNullOrWhiteSpace(search.OrderStatus))
                query = query.Where(r => r.Payment.Order.Status == search.OrderStatus);

            if (!string.IsNullOrWhiteSpace(search.Transactionid))
                query = query.Where(r => r.Payment.Transactionid != null && r.Payment.Transactionid.Contains(search.Transactionid));

            if (!string.IsNullOrWhiteSpace(search.Reason))
                query = query.Where(r => r.Reason != null && r.Reason.Contains(search.Reason));

            // 7. Flags
            if (search.PendingOnly == true)
                query = query.Where(r => r.Status == "PENDING" || r.Status == "PROCESSING");

            if (search.CompletedOnly == true)
                query = query.Where(r => r.Status == "SUCCESS" || r.Status == "COMPLETED" || r.Status == "APPROVED");

            if (search.LargeRefundsOnly == true)
                query = query.Where(r => r.Amount >= 500000);

            return query;
        }
    }
}
