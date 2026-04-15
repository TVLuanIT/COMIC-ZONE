using System;
using System.Linq;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Extensions;
using COMICZONE.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using COMICZONE.Services;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MarketplacePostsController : AdminBaseController
    {
        private readonly ComiczoneContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMarketplaceService _marketplaceService;
        private readonly INotificationService _notificationService;

        public MarketplacePostsController(ComiczoneContext context, IWebHostEnvironment webHostEnvironment, IMarketplaceService marketplaceService, INotificationService notificationService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _marketplaceService = marketplaceService;
            _notificationService = notificationService;
        }

        // ==================== INDEX ====================
        public async Task<IActionResult> Index(MarketplacePostSearchModel search)
        {
            search ??= new MarketplacePostSearchModel();

            var query = _context.MarketplacePosts
                .Include(p => p.Seller)
                .Include(p => p.MarketplacePostImages)
                .Include(p => p.MarketplacePostPromotions)
                .AsQueryable();

            query = query.ApplyMarketplacePostSearch(search);
            search.TotalItems = await query.CountAsync();
            query = query.ApplyMarketplacePostSort(search.SortColumn, search.IsAscending);

            var posts = await query.ApplyPagination(search.Page, search.PageSize).ToListAsync();

            ViewBag.Posts = posts;
            return View(search);
        }

        // ==================== DETAILS ====================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.MarketplacePosts
                .Include(p => p.Seller)
                .Include(p => p.MarketplacePostImages)
                .Include(p => p.MarketplaceFavorites)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            return View(post);
        }

        // ==================== EDIT ====================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.MarketplacePosts
                .Include(p => p.Seller)
                .Include(p => p.MarketplacePostImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MarketplacePost model)
        {
            if (id != model.Id) return NotFound();

            var post = await _context.MarketplacePosts.FindAsync(id);
            if (post == null) return NotFound();

            var oldStatus = post.Status;
            post.Status = model.Status;
            post.Isdeleted = model.Isdeleted;
            post.Updatedat = DateTime.Now;

            await _context.SaveChangesAsync();

            // Send notification if status changed to Approved or Rejected
            if (oldStatus != post.Status)
            {
                var adminIdStr = HttpContext.Session.GetString("UserId");
                int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

                if (post.Status == "Approved")
                {
                    await _notificationService.SendNotificationAsync(post.Sellerid, adminId, "Bài đăng được duyệt", $"Bài đăng \"{post.Title}\" của bạn đã được duyệt.", $"/Marketplace/MarketplacePosts/Details/{post.Id}");
                }
                else if (post.Status == "Rejected")
                {
                    await _notificationService.SendNotificationAsync(post.Sellerid, adminId, "Bài đăng bị từ chối", $"Bài đăng \"{post.Title}\" của bạn không được duyệt.", "/Marketplace/MarketplacePosts/MyPosts");
                }
            }

            TempData["Success"] = $"Đã cập nhật bài đăng #{post.Id} thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ==================== DELETE ====================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.MarketplacePosts
                .Include(p => p.Seller)
                .Include(p => p.MarketplacePostImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            return View(post);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.MarketplacePosts
                .Include(p => p.MarketplacePostImages)
                .Include(p => p.MarketplaceFavorites)
                .Include(p => p.MarketplaceMessages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            // 1. Xóa các tệp ảnh vật lý trên đĩa
            var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "marketplace");
            foreach (var img in post.MarketplacePostImages)
            {
                var filePath = Path.Combine(folderPath, img.Filename);
                if (System.IO.File.Exists(filePath))
                {
                    try { System.IO.File.Delete(filePath); } catch { /* Ignore file system errors */ }
                }
            }

            // 3. Xóa các bản ghi liên quan trong DB
            _context.MarketplacePostImages.RemoveRange(post.MarketplacePostImages);
            _context.MarketplaceFavorites.RemoveRange(post.MarketplaceFavorites);
            _context.MarketplaceMessages.RemoveRange(post.MarketplaceMessages);

            // 4. Xóa bài đăng chính
            _context.MarketplacePosts.Remove(post);

            await _context.SaveChangesAsync();
            
            TempData["Success"] = $"Đã xóa bài đăng #{id} vĩnh viễn cùng tất cả dữ liệu liên quan.";
            return RedirectToAction(nameof(Index));
        }

        // ==================== AJAX: APPROVE ====================
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var post = await _context.MarketplacePosts.FindAsync(id);
            if (post == null) return Json(new { success = false });

            post.Status = "Approved";
            post.Updatedat = DateTime.Now;

            var adminIdStr = HttpContext.Session.GetString("UserId");
            int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

            await _notificationService.SendNotificationAsync(post.Sellerid, adminId, "Bài đăng được duyệt", $"Bài đăng \"{post.Title}\" của bạn đã được duyệt.", $"/Marketplace/MarketplacePosts/Details/{post.Id}");

            return Json(new { success = true, status = "Approved" });
        }

        // ==================== AJAX: REJECT ====================
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var post = await _context.MarketplacePosts.FindAsync(id);
            if (post == null) return Json(new { success = false });

            post.Status = "Rejected";
            post.Updatedat = DateTime.Now;

            var adminIdStr = HttpContext.Session.GetString("UserId");
            int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

            await _notificationService.SendNotificationAsync(post.Sellerid, adminId, "Bài đăng bị từ chối", $"Bài đăng \"{post.Title}\" của bạn không được duyệt.", "/Marketplace/MarketplacePosts/MyPosts");

            return Json(new { success = true, status = "Rejected" });
        }

        // ==================== AJAX: TOGGLE DELETE ====================
        [HttpPost]
        public async Task<IActionResult> ToggleDelete(int id)
        {
            var post = await _context.MarketplacePosts.FindAsync(id);
            if (post == null) return Json(new { success = false });

            post.Isdeleted = !(post.Isdeleted ?? false);
            post.Updatedat = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isDeleted = post.Isdeleted });
        }

        // ==================== AJAX: PROMOTE (GIFT) ====================
        [HttpPost]
        public async Task<IActionResult> Promote(int id, int days)
        {
            var post = await _context.MarketplacePosts.FindAsync(id);
            if (post == null) return Json(new { success = false, message = "Không tìm thấy bài đăng." });

            if (days <= 0) days = 1;

            try
            {
                // Create a promotion with 0 amount and special method
                var promotion = await _marketplaceService.PromotePostAsync(id, post.Sellerid, days, 0, "ADMIN_GIFT");
                
                // Immediately activate it
                await _marketplaceService.ActivatePromotionAsync(promotion.Id);

                // Notify the user
                var adminIdStr = HttpContext.Session.GetString("UserId");
                int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

                await _notificationService.SendNotificationAsync(post.Sellerid, adminId, "Bài đăng được tặng quảng cáo", $"Bài đăng \"{post.Title}\" của bạn đã được Admin tặng gói quảng cáo nổi bật trong {days} ngày.", $"/Marketplace/MarketplacePosts/Details/{post.Id}");

                return Json(new { success = true, message = $"Đã tặng quảng cáo {days} ngày thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
