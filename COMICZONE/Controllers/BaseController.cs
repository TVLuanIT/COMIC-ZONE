using Microsoft.AspNetCore.Mvc;

namespace COMICZONE.Controllers
{
    public class BaseController : Controller
    {
        protected bool IsLoggedIn()
        {
            return !string.IsNullOrWhiteSpace(CurrentUserId());
        }

        protected string? CurrentUserId()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrWhiteSpace(userIdStr))
                return null;

            return userIdStr;
        }
    }
}
