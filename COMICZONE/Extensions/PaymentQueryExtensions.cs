using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Extensions
{
    public static class PaymentQueryExtensions
    {
        public static IQueryable<Payment> ApplyPaymentFilters(this IQueryable<Payment> query, PaymentSearchModel search)
        {
            if (search == null) return query;

            // 1. Keyword search (PaymentId, OrderId, TransactionId, OrderUsername)
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.ToLower();
                query = query.Where(p => 
                    p.Paymentid.ToString() == keyword ||
                    p.Orderid.ToString() == keyword ||
                    (p.Transactionid != null && p.Transactionid.ToLower().Contains(keyword)) ||
                    (p.Order.User.Username != null && p.Order.User.Username.ToLower().Contains(keyword))
                );
            }

            // 2. Exact Field Matches
            if (search.Paymentid.HasValue)
                query = query.Where(p => p.Paymentid == search.Paymentid.Value);

            if (search.Orderid.HasValue)
                query = query.Where(p => p.Orderid == search.Orderid.Value);

            if (!string.IsNullOrWhiteSpace(search.Transactionid))
                query = query.Where(p => p.Transactionid != null && p.Transactionid.Contains(search.Transactionid));

            if (!string.IsNullOrWhiteSpace(search.OrderUsername))
                query = query.Where(p => p.Order.User.Username != null && p.Order.User.Username.Contains(search.OrderUsername));

            // 3. Statuses & Methods
            if (search.Paymentstatuses != null && search.Paymentstatuses.Any())
                query = query.Where(p => p.Paymentstatus != null && search.Paymentstatuses.Contains(p.Paymentstatus));

            if (search.Paymentmethods != null && search.Paymentmethods.Any())
                query = query.Where(p => p.Paymentmethod != null && search.Paymentmethods.Contains(p.Paymentmethod));

            // 4. Amount Range
            if (search.AmountMin.HasValue)
                query = query.Where(p => p.Amount >= search.AmountMin.Value);

            if (search.AmountMax.HasValue)
                query = query.Where(p => p.Amount <= search.AmountMax.Value);

            // 5. Date Range (PaidAt)
            if (search.PaidFrom.HasValue)
                query = query.Where(p => p.Paidat >= search.PaidFrom.Value);

            if (search.PaidTo.HasValue)
            {
                var toDate = search.PaidTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.Paidat <= toDate);
            }

            // 6. Flags
            if (search.PaidOnly == true)
                query = query.Where(p => p.Paymentstatus == "SUCCESS");

            if (search.HasRefund == true)
                query = query.Where(p => p.Refunds.Any());

            return query;
        }
    }
}
