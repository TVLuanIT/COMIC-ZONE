using Microsoft.AspNetCore.Mvc;

namespace COMICZONE.Helpers
{
    public class UserAvatarViewComponent : ViewComponent
    {
        // Truyền trực tiếp avatarUrl và size
        public IViewComponentResult Invoke(string? avatarUrl, int size = 32)
        {
            ViewData["AvatarUrl"] = avatarUrl;
            ViewData["Size"] = size;
            return View();
        }
    }
}