using COMICZONE.Models;

namespace COMICZONE.Services
{
    public interface IInvoiceService
    {
        /// <summary>
        /// Tạo và lưu hóa đơn cho một đơn hàng cụ thể.
        /// </summary>
        /// <param name="orderId">Mã đơn hàng</param>
        /// <returns>Đối tượng Invoice vừa tạo, hoặc null nếu đơn hàng không tồn tại.</returns>
        Task<Invoice?> CreateInvoiceAsync(int orderId);
        
        /// <summary>
        /// Kiểm tra xem đơn hàng đã có hóa đơn chưa.
        /// </summary>
        /// <param name="orderId">Mã đơn hàng</param>
        /// <returns>True nếu đã có hóa đơn.</returns>
        Task<bool> HasInvoiceAsync(int orderId);
    }
}
