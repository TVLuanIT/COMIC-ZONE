using Microsoft.AspNetCore.Mvc;
using COMICZONE.Data;
using COMICZONE.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace COMICZONE.ViewComponents
{
    public class UnreadTotalCountViewComponent : ViewComponent
    {
        private readonly ComiczoneContext _context;
        private readonly IChatService _chatService;

        public UnreadTotalCountViewComponent(ComiczoneContext context, IChatService chatService)
        {
            _context = context;
            _chatService = chatService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            int totalCount = 0;

            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                // 1. Đếm thông báo hệ thống
                int notificationCount = await _context.Notifications
                    .Where(n => n.UserId == userId && 
                           (n.IsRead == false || n.IsRead == null) && 
                           !n.Isdeleted)
                    .CountAsync();

                // 2. Đếm tin nhắn chat
                int chatCount = await _chatService.GetTotalUnreadCountAsync(userId);

                totalCount = notificationCount + chatCount;
            }

            return View(totalCount);
        }
    }
}
