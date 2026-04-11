using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using COMICZONE.Services;
using COMICZONE.Models;
using System;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MarketplacePostsController : COMICZONE.Controllers.BaseController
    {
        private readonly IMarketplaceService _marketplaceService;
        private readonly INotificationService _notificationService;

        public MarketplacePostsController(IMarketplaceService marketplaceService, INotificationService notificationService)
        {
            _marketplaceService = marketplaceService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _marketplaceService.GetAllPostsAsync("Pending");
            return View(posts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Authentication", new { area = "Account" });

            var result = await _marketplaceService.UpdatePostStatusAsync(id, "Approved");
            if (result)
            {
                var post = await _marketplaceService.GetPostByIdAsync(id);
                if (post != null)
                {
                    await _notificationService.SendNotificationAsync(
                        userId: post.Sellerid,
                        createdBy: int.Parse(CurrentUserId()),
                        title: "Bài đăng được duyệt",
                        message: $"Bài đăng '{post.Title}' của bạn đã được duyệt.",
                        link: $"/Marketplace/MarketplacePosts/Details/{post.Id}"
                    );
                }
                TempData["SuccessMessage"] = "Đã duyệt bài đăng thành công.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Authentication", new { area = "Account" });

            var result = await _marketplaceService.UpdatePostStatusAsync(id, "Rejected");
            if (result)
            {
                var post = await _marketplaceService.GetPostByIdAsync(id);
                if (post != null)
                {
                    await _notificationService.SendNotificationAsync(
                        userId: post.Sellerid,
                        createdBy: int.Parse(CurrentUserId()),
                        title: "Bài đăng bị từ chối",
                        message: $"Bài đăng '{post.Title}' của bạn không được duyệt. Vui lòng kiểm tra lại.",
                        link: $"/Marketplace/MarketplacePosts/Index"
                    );
                }
                TempData["SuccessMessage"] = "Đã từ chối bài đăng.";
            }
            return RedirectToAction("Index");
        }
    }
}
