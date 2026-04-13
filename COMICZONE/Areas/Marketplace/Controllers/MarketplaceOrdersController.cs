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

        public async Task<IActionResult> MyOrders(int pPost = 1, int pSell = 1, int pBuy = 1)
        {
            if (!IsLoggedIn()) return RedirectToAction("Index", "Home", new { area = "" });

            var userId = int.Parse(CurrentUserId());
            const int pageSize = 10;

            var (myPosts, totalPosts) = await _marketplaceService.GetPostsBySellerAsync(userId, pPost, pageSize);
            var (sellingOrders, totalSelling) = await _marketplaceService.GetOrdersBySellerAsync(userId, pSell, pageSize);
            var (buyingOrders, totalBuying) = await _marketplaceService.GetOrdersByBuyerAsync(userId, pBuy, pageSize);

            ViewBag.MyPosts = myPosts;
            ViewBag.SellingOrders = sellingOrders;
            ViewBag.BuyingOrders = buyingOrders;

            ViewBag.PaginationPost = new PaginationModel
            {
                CurrentPage = pPost,
                TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize),
                Controller = "MarketplaceOrders",
                Action = "MyOrders",
                Area = "Marketplace",
                PageParam = "pPost",
                ExtraParams = new Dictionary<string, string> { { "pSell", pSell.ToString() }, { "pBuy", pBuy.ToString() }, { "tab", "posts" } }
            };

            ViewBag.PaginationSell = new PaginationModel
            {
                CurrentPage = pSell,
                TotalPages = (int)Math.Ceiling(totalSelling / (double)pageSize),
                Controller = "MarketplaceOrders",
                Action = "MyOrders",
                Area = "Marketplace",
                PageParam = "pSell",
                ExtraParams = new Dictionary<string, string> { { "pPost", pPost.ToString() }, { "pBuy", pBuy.ToString() }, { "tab", "sell" } }
            };

            ViewBag.PaginationBuy = new PaginationModel
            {
                CurrentPage = pBuy,
                TotalPages = (int)Math.Ceiling(totalBuying / (double)pageSize),
                Controller = "MarketplaceOrders",
                Action = "MyOrders",
                Area = "Marketplace",
                PageParam = "pBuy",
                ExtraParams = new Dictionary<string, string> { { "pPost", pPost.ToString() }, { "pSell", pSell.ToString() }, { "tab", "buy" } }
            };

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
