using Microsoft.AspNetCore.Mvc;
using COMICZONE.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace COMICZONE.ViewComponents
{
    public class UnreadNotificationCountViewComponent : ViewComponent
    {
        private readonly ComiczoneContext _context;

        public UnreadNotificationCountViewComponent(ComiczoneContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            int count = 0;

            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                // Đếm tất cả các thông báo chưa đọc của user
                count = await _context.Notifications
                    .Where(n => n.UserId == userId && 
                           (n.IsRead == false || n.IsRead == null) && 
                           !n.Isdeleted)
                    .CountAsync();
            }

            return View(count);
        }
    }
}
