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
    public class MarketplaceReviewsController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public MarketplaceReviewsController(ComiczoneContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(MarketplaceReviewSearchModel search)
        {
            search ??= new MarketplaceReviewSearchModel();

            var query = _context.MarketplaceReviews
                .Include(r => r.Reviewer)
                .Include(r => r.Order).ThenInclude(o => o.Post)
                .Include(r => r.Order).ThenInclude(o => o.Seller)
                .AsQueryable();

            query = query.ApplyMarketplaceReviewSearch(search);
            search.TotalItems = await query.CountAsync();
            query = query.ApplyMarketplaceReviewSort(search.SortColumn, search.IsAscending);

            var reviews = await query.ApplyPagination(search.Page, search.PageSize).ToListAsync();

            ViewBag.Reviews = reviews;
            return View(search);
        }
    }
}
