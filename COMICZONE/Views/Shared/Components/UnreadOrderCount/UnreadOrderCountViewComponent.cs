using Microsoft.AspNetCore.Mvc;
using COMICZONE.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace COMICZONE.ViewComponents
{
    public class UnreadOrderCountViewComponent : ViewComponent
    {
        private readonly ComiczoneContext _context;

        public UnreadOrderCountViewComponent(ComiczoneContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            int count = 0;

            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                // Đếm các thông báo chưa đọc liên quan đến đơn hàng
                count = await _context.Notifications
                    .Where(n => n.UserId == userId && 
                           (n.IsRead == false || n.IsRead == null) && 
                           !n.Isdeleted && 
                           (n.Link.Contains("MyOrders") || n.Link.Contains("OrderDetails")))
                    .CountAsync();
            }

            return View(count);
        }
    }
}
