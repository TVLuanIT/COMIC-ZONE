using COMICZONE.Data;
using COMICZONE.Models;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ComiczoneContext _context;

        public InvoiceService(ComiczoneContext context)
        {
            _context = context;
        }

        public async Task<Invoice?> CreateInvoiceAsync(int orderId)
        {
            // 1. Kiểm tra xem đơn hàng có tồn tại không
            var order = await _context.Orders
                .Include(o => o.User)
                    .ThenInclude(u => u.Customer)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return null;

            // 2. Kiểm tra xem đã có hóa đơn cho đơn hàng này chưa (tránh tạo trùng)
            bool exists = await HasInvoiceAsync(orderId);
            if (exists)
            {
                // Nếu đã có, trả về hóa đơn đầu tiên tìm thấy
                return await _context.Invoices.FirstOrDefaultAsync(i => i.OrderId == orderId);
            }

            // 3. Tạo hóa đơn mới
            var invoice = new Invoice
            {
                OrderId = orderId,
                TotalAmount = order.TotalAmount,
                IssueDate = DateTime.Now,
                // Ưu tiên lấy FullName từ Customer, nếu không thì lấy Username
                CustomerName = order.User?.Customer?.Fullname ?? order.User?.Username ?? "Khách hàng vãng lai",
                Isdeleted = false
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return invoice;
        }

        public async Task<bool> HasInvoiceAsync(int orderId)
        {
            return await _context.Invoices.AnyAsync(i => i.OrderId == orderId && !i.Isdeleted);
        }
    }
}
