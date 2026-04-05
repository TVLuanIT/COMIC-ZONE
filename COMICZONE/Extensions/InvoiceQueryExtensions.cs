using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Extensions
{
    public static class InvoiceQueryExtensions
    {
        public static IQueryable<Invoice> ApplyInvoiceFilters(this IQueryable<Invoice> query, InvoiceSearchModel search)
        {
            if (search == null) return query;

            // 1. Keyword search (InvoiceID, OrderID, CustomerName, Username)
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.ToLower();
                query = query.Where(i => 
                    i.Id.ToString() == keyword ||
                    i.OrderId.ToString() == keyword ||
                    (i.CustomerName != null && i.CustomerName.ToLower().Contains(keyword)) ||
                    (i.Order.User.Username != null && i.Order.User.Username.ToLower().Contains(keyword))
                );
            }

            // 2. Exact Field Matches
            if (search.Id.HasValue)
                query = query.Where(i => i.Id == search.Id.Value);

            if (search.OrderId.HasValue)
                query = query.Where(i => i.OrderId == search.OrderId.Value);

            if (!string.IsNullOrWhiteSpace(search.CustomerName))
                query = query.Where(i => i.CustomerName != null && i.CustomerName.Contains(search.CustomerName));

            // 3. User Info
            if (search.UserId.HasValue)
                query = query.Where(i => i.Order.UserId == search.UserId.Value);

            if (!string.IsNullOrWhiteSpace(search.Username))
                query = query.Where(i => i.Order.User.Username.Contains(search.Username));

            if (!string.IsNullOrWhiteSpace(search.UserEmail))
                query = query.Where(i => i.Order.User.Email != null && i.Order.User.Email.Contains(search.UserEmail));

            // 4. Order Info
            if (!string.IsNullOrWhiteSpace(search.OrderStatus))
                query = query.Where(i => i.Order.Status == search.OrderStatus);

            if (!string.IsNullOrWhiteSpace(search.OrderPhoneNumber))
                query = query.Where(i => i.Order.PhoneNumber != null && i.Order.PhoneNumber.Contains(search.OrderPhoneNumber));

            if (search.OrderDateFrom.HasValue)
                query = query.Where(i => i.Order.OrderDate >= search.OrderDateFrom.Value);

            if (search.OrderDateTo.HasValue)
            {
                var toDate = search.OrderDateTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(i => i.Order.OrderDate <= toDate);
            }

            // 5. Invoice Timeline & Amounts
            if (search.TotalAmountMin.HasValue)
                query = query.Where(i => i.TotalAmount >= search.TotalAmountMin.Value);

            if (search.TotalAmountMax.HasValue)
                query = query.Where(i => i.TotalAmount <= search.TotalAmountMax.Value);

            if (search.IssueDateFrom.HasValue)
                query = query.Where(i => i.IssueDate >= search.IssueDateFrom.Value);

            if (search.IssueDateTo.HasValue)
            {
                var toDate = search.IssueDateTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(i => i.IssueDate <= toDate);
            }

            // 6. Payment Context (Checking associated order's payments)
            if (!string.IsNullOrWhiteSpace(search.PaymentMethod))
                query = query.Where(i => i.Order.Payments.Any(p => p.Paymentmethod == search.PaymentMethod));

            if (!string.IsNullOrWhiteSpace(search.PaymentStatus))
                query = query.Where(i => i.Order.Payments.Any(p => p.Paymentstatus == search.PaymentStatus));

            if (!string.IsNullOrWhiteSpace(search.Transactionid))
                query = query.Where(i => i.Order.Payments.Any(p => p.Transactionid != null && p.Transactionid.Contains(search.Transactionid)));

            if (search.PaidOnly == true)
                query = query.Where(i => i.Order.Payments.Any(p => p.Paymentstatus == "SUCCESS"));

            if (search.UnpaidOnly == true)
                query = query.Where(i => !i.Order.Payments.Any(p => p.Paymentstatus == "SUCCESS"));

            // 7. Is Deleted
            if (search.IsDeleted.HasValue)
                query = query.Where(i => i.Isdeleted == search.IsDeleted.Value);

            return query;
        }
    }
}
