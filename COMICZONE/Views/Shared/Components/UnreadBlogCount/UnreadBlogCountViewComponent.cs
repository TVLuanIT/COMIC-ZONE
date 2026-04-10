using Microsoft.AspNetCore.Mvc;
using COMICZONE.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace COMICZONE.ViewComponents
{
    public class UnreadBlogCountViewComponent : ViewComponent
    {
        private readonly ComiczoneContext _context;

        public UnreadBlogCountViewComponent(ComiczoneContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            int count = 0;

            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                // Đếm các thông báo chưa đọc liên quan đến bài viết của tôi
                // Thông báo bài viết thường có Link chứa /Blogs/Blogs/Details
                count = await _context.Notifications
                    .Where(n => n.UserId == userId && 
                           (n.IsRead == false || n.IsRead == null) && 
                           !n.Isdeleted && 
                           n.Link.Contains("/Blogs/Blogs/Details/"))
                    .CountAsync();
            }

            return View(count);
        }
    }
}
