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

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MarketplacePostsController : AdminBaseController
    {
        private readonly ComiczoneContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MarketplacePostsController(ComiczoneContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==================== INDEX ====================
        public async Task<IActionResult> Index(MarketplacePostSearchModel search)
        {
            search ??= new MarketplacePostSearchModel();

            var query = _context.MarketplacePosts
                .Include(p => p.Seller)
                .Include(p => p.MarketplacePostImages)
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
                .Include(p => p.MarketplaceOrders).ThenInclude(o => o.Buyer)
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

            post.Status = model.Status;
            post.Isdeleted = model.Isdeleted;
            post.Updatedat = DateTime.Now;

            await _context.SaveChangesAsync();

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
                .Include(p => p.MarketplaceOrders)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            // 1. Kiểm tra đơn hàng - Nếu đã có đơn hàng thì không cho phép xóa vĩnh viễn
            if (post.MarketplaceOrders.Any())
            {
                TempData["Error"] = "Không thể xóa vĩnh viễn bài đăng này vì đã có đơn hàng liên quan. Vui lòng sử dụng tính năng 'Ẩn bài đăng' thay thế.";
                return RedirectToAction(nameof(Index));
            }

            // 2. Xóa các tệp ảnh vật lý trên đĩa
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

            _context.Notifications.Add(new Notification
            {
                UserId = post.Sellerid,
                Title = "Bài đăng được duyệt",
                Message = $"Bài đăng \"{post.Title}\" của bạn đã được duyệt.",
                CreatedBy = adminId,
                CreatedAt = DateTime.Now,
                IsRead = false,
                Link = $"/Marketplace/MarketplacePosts/Details/{post.Id}"
            });

            await _context.SaveChangesAsync();
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

            _context.Notifications.Add(new Notification
            {
                UserId = post.Sellerid,
                Title = "Bài đăng bị từ chối",
                Message = $"Bài đăng \"{post.Title}\" của bạn không được duyệt.",
                CreatedBy = adminId,
                CreatedAt = DateTime.Now,
                IsRead = false,
                Link = "/Marketplace/MarketplacePosts/Index"
            });

            await _context.SaveChangesAsync();
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
    }
}
