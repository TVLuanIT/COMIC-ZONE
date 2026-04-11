using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using COMICZONE.Services;
using COMICZONE.Models;

namespace COMICZONE.Areas.Marketplace.Controllers
{
    [Area("Marketplace")]
    public class MarketplaceReviewsController : COMICZONE.Controllers.BaseController
    {
        private readonly IMarketplaceService _marketplaceService;

        public MarketplaceReviewsController(IMarketplaceService marketplaceService)
        {
            _marketplaceService = marketplaceService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewSeller(MarketplaceReview review)
        {
            if (!IsLoggedIn()) return RedirectToAction("Index", "Home", new { area = "" });

            ModelState.Remove("Order");
            ModelState.Remove("Reviewer");

            if (ModelState.IsValid)
            {
                review.Reviewerid = int.Parse(CurrentUserId());
                await _marketplaceService.AddReviewAsync(review);
                TempData["Success"] = "Cảm ơn bạn đã đánh giá người bán.";
                return RedirectToAction("MyOrders", "MarketplaceOrders");
            }

            TempData["Error"] = "Vui lòng nhập đánh giá hợp lệ.";
            return RedirectToAction("MyOrders", "MarketplaceOrders");
        }
    }
}
