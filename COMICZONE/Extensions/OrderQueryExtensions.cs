using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Extensions
{
    public static class OrderQueryExtensions
    {
        public static IQueryable<Order> ApplyOrderFilters(this IQueryable<Order> query, OrderSearchModel search)
        {
            if (search == null) return query;

            // 1. Keyword search (OrderId, User Info, Address, Phone)
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.ToLower();
                query = query.Where(o => 
                    o.OrderId.ToString() == keyword ||
                    (o.User.Username != null && o.User.Username.ToLower().Contains(keyword)) ||
                    (o.User.Customer != null && o.User.Customer.Fullname != null && o.User.Customer.Fullname.ToLower().Contains(keyword)) ||
                    (o.User.Email != null && o.User.Email.ToLower().Contains(keyword)) ||
                    (o.PhoneNumber != null && o.PhoneNumber.Contains(keyword)) ||
                    (o.ShippingAddress != null && o.ShippingAddress.ToLower().Contains(keyword))
                );
            }

            // 2. Identification
            if (search.OrderId.HasValue)
                query = query.Where(o => o.OrderId == search.OrderId.Value);

            if (search.UserId.HasValue)
                query = query.Where(o => o.UserId == search.UserId.Value);

            if (!string.IsNullOrWhiteSpace(search.Username))
                query = query.Where(o => o.User.Username != null && o.User.Username.Contains(search.Username));

            if (!string.IsNullOrWhiteSpace(search.UserEmail))
                query = query.Where(o => o.User.Email != null && o.User.Email.Contains(search.UserEmail));

            // 3. Status
            if (!string.IsNullOrWhiteSpace(search.Status))
            {
                query = query.Where(o => o.Status == search.Status);
            }
            else if (search.Statuses != null && search.Statuses.Any())
            {
                query = query.Where(o => o.Status != null && search.Statuses.Contains(o.Status));
            }

            // 4. Contact & Address
            if (!string.IsNullOrWhiteSpace(search.PhoneNumber))
                query = query.Where(o => o.PhoneNumber != null && o.PhoneNumber.Contains(search.PhoneNumber));

            if (!string.IsNullOrWhiteSpace(search.ShippingAddress))
                query = query.Where(o => o.ShippingAddress != null && o.ShippingAddress.Contains(search.ShippingAddress));

            if (!string.IsNullOrWhiteSpace(search.Note))
                query = query.Where(o => o.Note != null && o.Note.Contains(search.Note));

            // 5. Financials
            if (search.TotalAmountMin.HasValue)
                query = query.Where(o => o.TotalAmount >= search.TotalAmountMin.Value);

            if (search.TotalAmountMax.HasValue)
                query = query.Where(o => o.TotalAmount <= search.TotalAmountMax.Value);

            // 6. Dates
            if (search.OrderDateFrom.HasValue)
                query = query.Where(o => o.OrderDate >= search.OrderDateFrom.Value);

            if (search.OrderDateTo.HasValue)
            {
                var toDate = search.OrderDateTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.OrderDate <= toDate);
            }

            if (search.CreatedFrom.HasValue)
                query = query.Where(o => o.CreatedAt >= search.CreatedFrom.Value);

            if (search.CreatedTo.HasValue)
            {
                var toDate = search.CreatedTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.CreatedAt <= toDate);
            }

            // 7. Product inclusions
            if (search.ProductId.HasValue)
                query = query.Where(o => o.OrderItems.Any(oi => oi.ProductId == search.ProductId.Value));

            if (!string.IsNullOrWhiteSpace(search.ProductName))
                query = query.Where(o => o.OrderItems.Any(oi => oi.Product != null && oi.Product.Name != null && oi.Product.Name.Contains(search.ProductName)));

            // 8. Payment info
            if (search.HasPayment.HasValue)
            {
                if (search.HasPayment.Value) query = query.Where(o => o.Payments.Any());
                else query = query.Where(o => !o.Payments.Any());
            }

            if (!string.IsNullOrWhiteSpace(search.PaymentMethod))
                query = query.Where(o => o.Payments.Any(p => p.Paymentmethod != null && p.Paymentmethod == search.PaymentMethod));

            if (!string.IsNullOrWhiteSpace(search.PaymentStatus))
                query = query.Where(o => o.Payments.Any(p => p.Paymentstatus != null && p.Paymentstatus == search.PaymentStatus));

            if (!string.IsNullOrWhiteSpace(search.TransactionId))
                query = query.Where(o => o.Payments.Any(p => p.Transactionid != null && p.Transactionid.Contains(search.TransactionId)));

            if (search.PaidOnly == true)
                query = query.Where(o => o.Payments.Any(p => p.Paymentstatus == "SUCCESS"));

            if (search.UnpaidOnly == true)
                query = query.Where(o => !o.Payments.Any(p => p.Paymentstatus == "SUCCESS"));

            // 9. Invoice details
            if (search.HasInvoice.HasValue)
            {
                if (search.HasInvoice.Value) query = query.Where(o => o.Invoices.Any());
                else query = query.Where(o => !o.Invoices.Any());
            }

            if (!string.IsNullOrWhiteSpace(search.InvoiceNumber))
                query = query.Where(o => o.Invoices.Any(inv => inv.Id.ToString() == search.InvoiceNumber));

            // 10. Items count (Sum of quantities)
            if (search.ItemCountMin.HasValue)
                query = query.Where(o => o.OrderItems.Sum(oi => oi.Quantity) >= search.ItemCountMin.Value);

            if (search.ItemCountMax.HasValue)
                query = query.Where(o => o.OrderItems.Sum(oi => oi.Quantity) <= search.ItemCountMax.Value);

            // 11. Status
            if (search.IsDeleted.HasValue)
                query = query.Where(o => o.Isdeleted == search.IsDeleted.Value);

            return query;
        }
    }
}
