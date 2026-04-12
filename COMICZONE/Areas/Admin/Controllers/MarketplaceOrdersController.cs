using System.Linq;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Extensions;
using COMICZONE.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MarketplaceOrdersController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public MarketplaceOrdersController(ComiczoneContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(MarketplaceOrderSearchModel search)
        {
            search ??= new MarketplaceOrderSearchModel();

            var query = _context.MarketplaceOrders
                .Include(o => o.Post).ThenInclude(p => p.MarketplacePostImages)
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .AsQueryable();

            query = query.ApplyMarketplaceOrderSearch(search);
            search.TotalItems = await query.CountAsync();
            query = query.ApplyMarketplaceOrderSort(search.SortColumn, search.IsAscending);

            var orders = await query.ApplyPagination(search.Page, search.PageSize).ToListAsync();

            ViewBag.Orders = orders;
            return View(search);
        }
    }
}
