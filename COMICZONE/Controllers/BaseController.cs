using Microsoft.AspNetCore.Mvc;

namespace COMICZONE.Controllers
{
    public class BaseController : Controller
    {
        protected bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("UserId") != null;
        }
    }
}
