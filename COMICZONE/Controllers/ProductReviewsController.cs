using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace COMICZONE.Controllers
{
    public class ProductReviewsController : Controller
    {
        private readonly ComiczoneContext _context;

        public ProductReviewsController(ComiczoneContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike(int reviewId)
        {
            string? userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
            {
                // Người dùng chưa đăng nhập → trả về lỗi để JS chỉ toggle màu
                return Json(new { success = false, message = "Bạn cần đăng nhập để like" });
            }

            int userId = int.Parse(userIdStr);

            // Kiểm tra xem người dùng đã like chưa
            var existingLike = await _context.ProductReviewLikes
                .FirstOrDefaultAsync(l => l.Reviewid == reviewId && l.Userid == userId);

            if (existingLike != null)
            {
                _context.ProductReviewLikes.Remove(existingLike);
            }
            else
            {
                _context.ProductReviewLikes.Add(new ProductReviewLike
                {
                    Reviewid = reviewId,
                    Userid = userId,
                    Createdat = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            // Lấy lại số lượt like
            var likeCount = await _context.ProductReviewLikes
                .CountAsync(l => l.Reviewid == reviewId);

            return Json(new { success = true, likeCount });
        }

        public async Task<IActionResult> Reviews(int productId, int page = 1, int pageSize = 5)
        {
            var reviews = await _context.ProductReviews
                .Include(r => r.User)
                .Where(r => r.Productid == productId)
                .OrderByDescending(r => r.Createdat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.ProductReviews
                .Where(r => r.Productid == productId)
                .CountAsync();

            ViewBag.ProductId = productId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Lấy UserId và UserRole từ session
            var userIdStr = HttpContext.Session.GetString("UserId");
            ViewBag.UserId = string.IsNullOrEmpty(userIdStr) ? 0 : int.Parse(userIdStr);

            var roleStr = HttpContext.Session.GetString("UserRole");
            ViewBag.UserRole = string.IsNullOrEmpty(roleStr) ? "User" : roleStr;

            return PartialView("_ProductReviewList", reviews);
        }

        // POST: ProductReviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductReview review)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                // Chuyển tới login, kèm ReturnUrl
                string returnUrl = Url.Action("Detail", "Products", new { id = review.Productid, tab = "comment" }) ?? "/";
                return RedirectToAction("Login", "Authentication", new { returnUrl });
            }

            // Gán UserId
            review.Userid = int.Parse(userId);
            review.Createdat = DateTime.Now;

            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", "Products", new { id = review.Productid, tab = "comment" });
        }
    }
}
