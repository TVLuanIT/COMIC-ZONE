using Microsoft.AspNetCore.Mvc;
using COMICZONE.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace COMICZONE.ViewComponents
{
    public class UnreadChatCountViewComponent : ViewComponent
    {
        private readonly IChatService _chatService;

        public UnreadChatCountViewComponent(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            int count = 0;

            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                count = await _chatService.GetTotalUnreadCountAsync(userId);
            }

            return View(count);
        }
    }
}
