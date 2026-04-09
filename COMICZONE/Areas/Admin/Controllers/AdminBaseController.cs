using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class AdminBaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var role = context.HttpContext.Session.GetString("UserRole");

        if (string.IsNullOrEmpty(role) || role != "Admin")
        {
            context.Result = new RedirectToActionResult("Login", "Authentication", new { area = "Account" });
        }

        base.OnActionExecuting(context);
    }
}