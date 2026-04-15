using System;
using System.Linq;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Extensions;
using COMICZONE.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Services;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MarketplacePostPromotionsController : AdminBaseController
    {
        private readonly ComiczoneContext _context;
        private readonly IMarketplaceService _marketplaceService;

        public MarketplacePostPromotionsController(ComiczoneContext context, IMarketplaceService marketplaceService)
        {
            _context = context;
            _marketplaceService = marketplaceService;
        }

        // ==================== INDEX ====================
        public async Task<IActionResult> Index(MarketplacePostPromotionSearchModel search)
        {
            search ??= new MarketplacePostPromotionSearchModel();

            var query = _context.MarketplacePostPromotions
                .Include(p => p.Post)
                .Include(p => p.User)
                .AsQueryable();

            query = query.ApplyPromotionSearch(search);
            search.TotalItems = await query.CountAsync();
            query = query.ApplyPromotionSort(search.SortColumn, search.IsAscending);

            var promotions = await query.ApplyPagination(search.Page, search.PageSize).ToListAsync();

            ViewBag.Promotions = promotions;
            return View(search);
        }

        // ==================== AJAX: CANCEL PROMOTION ====================
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var promotion = await _context.MarketplacePostPromotions
                .Include(p => p.Post)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promotion == null) return Json(new { success = false, message = "Không tìm thấy quảng cáo." });

            var success = await _marketplaceService.CancelPromotionAsync(id);
            if (!success) return Json(new { success = false, message = "Không tìm thấy quảng cáo hoặc đã xảy ra lỗi." });

            // Notify User
            var adminIdStr = HttpContext.Session.GetString("UserId");
            int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

            _context.Notifications.Add(new Notification
            {
                UserId = promotion.Userid,
                Title = "Quảng cáo bị hủy bởi Admin",
                Message = $"Gói quảng cáo cho bài đăng \"{promotion.Post?.Title}\" đã bị Admin hủy bỏ.",
                CreatedBy = adminId,
                CreatedAt = DateTime.Now,
                IsRead = false,
                Link = "/Marketplace/MarketplacePosts/Index"
            });

            await _context.SaveChangesAsync();
            
            return Json(new { success = true, message = "Đã huỷ gói quảng cáo thành công." });
        }

        // ==================== AJAX: RESTORE PROMOTION ====================
        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var promotion = await _context.MarketplacePostPromotions
                .Include(p => p.Post)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (promotion == null) return Json(new { success = false, message = "Không tìm thấy quảng cáo." });

            var success = await _marketplaceService.RestorePromotionAsync(id);
            if (!success) return Json(new { success = false, message = "Không thể khôi phục gói này (có thể đã hết hạn hoặc không phải trạng thái Cancelled)." });

            // Notify User
            var adminIdStr = HttpContext.Session.GetString("UserId");
            int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

            string notificationTitle = "Gói quảng cáo được khôi phục";
            string notificationMessage = $"Gói quảng cáo cho bài đăng \"{promotion.Post?.Title}\" đã được Admin khôi phục lại.";

            if (promotion.Status == "Completed")
            {
                notificationTitle = "Gói quảng cáo đã hết hạn";
                notificationMessage = $"Gói quảng cáo cho bài đăng \"{promotion.Post?.Title}\" đã được khôi phục nhưng hiện đã hết hạn.";
            }

            _context.Notifications.Add(new Notification
            {
                UserId = promotion.Userid,
                Title = notificationTitle,
                Message = notificationMessage,
                CreatedBy = adminId,
                CreatedAt = DateTime.Now,
                IsRead = false,
                Link = promotion.Status == "Active" 
                    ? $"/Marketplace/MarketplacePosts/Details/{promotion.Postid}" 
                    : "/Marketplace/MarketplacePosts/MyPosts"
            });

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã khôi phục gói quảng cáo thành công." });
        }

        // ==================== AJAX: DELETE LOG ====================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var promotion = await _context.MarketplacePostPromotions.FindAsync(id);
            if (promotion == null) return Json(new { success = false, message = "Không tìm thấy giao dịch này." });

            _context.MarketplacePostPromotions.Remove(promotion);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa lịch sử quảng cáo vĩnh viễn." });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleDelete(int id)
        {
            var promotion = await _context.MarketplacePostPromotions.FindAsync(id);
            if (promotion == null) return Json(new { success = false });

            promotion.Isdeleted = !promotion.Isdeleted;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isDeleted = promotion.Isdeleted });
        }
    }
}
