using Microsoft.AspNetCore.Mvc;
using COMICZONE.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace COMICZONE.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly ComiczoneContext _context;

        public CartCountViewComponent(ComiczoneContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            int count = 0;

            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (cart != null)
                {
                    count = cart.CartItems.Sum(ci => ci.Quantity);
                }
            }

            return View(count);
        }
    }
}
