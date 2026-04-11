using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using COMICZONE.Services;
using COMICZONE.Models;
using System;

namespace COMICZONE.Areas.Marketplace.Controllers
{
    [Area("Marketplace")]
    public class MarketplaceOrdersController : COMICZONE.Controllers.BaseController
    {
        private readonly IMarketplaceService _marketplaceService;

        public MarketplaceOrdersController(IMarketplaceService marketplaceService)
        {
            _marketplaceService = marketplaceService;
        }

        public async Task<IActionResult> MyOrders()
        {
            if (!IsLoggedIn()) return RedirectToAction("Index", "Home", new { area = "" });

            var userId = int.Parse(CurrentUserId());
            var buyingOrders = await _marketplaceService.GetOrdersByBuyerAsync(userId);
            var sellingOrders = await _marketplaceService.GetOrdersBySellerAsync(userId);

            ViewBag.BuyingOrders = buyingOrders;
            ViewBag.SellingOrders = sellingOrders;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(int postId)
        {
            if (!IsLoggedIn()) return RedirectToAction("Index", "Home", new { area = "" });

            var post = await _marketplaceService.GetPostByIdAsync(postId);
            if (post == null || post.Status != "Approved") return NotFound();

            var order = new MarketplaceOrder
            {
                Postid = post.Id,
                Buyerid = int.Parse(CurrentUserId()),
                Sellerid = post.Sellerid,
                Price = post.Price,
                Status = "Pending"
            };

            await _marketplaceService.PlaceOrderAsync(order);
            TempData["Success"] = "Đặt mua khu vực Marketplace thành công!";
            return RedirectToAction("MyOrders");
        }
    }
}
