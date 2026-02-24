using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> ToggleDislike(int reviewId)
        {
            string? userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return Json(new { success = false, message = "Chưa đăng nhập" });

            int userId = int.Parse(userIdStr);

            var existing = await _context.ProductReviewLikes
                .FirstOrDefaultAsync(x => x.Reviewid == reviewId && x.Userid == userId);

            bool isDisliked;

            if (existing == null)
            {
                var newDislike = new ProductReviewLike
                {
                    Reviewid = reviewId,
                    Userid = userId,
                    Createdat = DateTime.Now,
                    IsLike = false
                };
                _context.ProductReviewLikes.Add(newDislike);
                isDisliked = true;
            }
            else
            {
                // nếu trước đó like thì đổi sang dislike, nếu dislike thì bỏ dislike
                isDisliked = existing.IsLike == false ? false : true;
                existing.IsLike = isDisliked ? false : (bool?)null;
            }

            await _context.SaveChangesAsync();

            // Trả lại số lượng like
            var likeCount = await _context.ProductReviewLikes
                .CountAsync(x => x.Reviewid == reviewId && x.IsLike == true);

            return Json(new { success = true, likeCount = likeCount, isDisliked = isDisliked });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike(int reviewId)
        {
            string? userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
                return Json(new { success = false });

            int userId = int.Parse(userIdStr);

            var existing = await _context.ProductReviewLikes
                .FirstOrDefaultAsync(x => x.Reviewid == reviewId && x.Userid == userId);

            bool isLiked;

            if (existing == null)
            {
                _context.ProductReviewLikes.Add(new ProductReviewLike
                {
                    Reviewid = reviewId,
                    Userid = userId,
                    Createdat = DateTime.Now,
                    IsLike = true
                });

                isLiked = true;
            }
            else
            {
                if (existing.IsLike == true)
                {
                    // Nếu đã like → bỏ like (xóa luôn)
                    _context.ProductReviewLikes.Remove(existing);
                    isLiked = false;
                }
                else
                {
                    // Nếu đang dislike → chuyển sang like
                    existing.IsLike = true;
                    isLiked = true;
                }
            }

            await _context.SaveChangesAsync();

            var likeCount = await _context.ProductReviewLikes
                .CountAsync(x => x.Reviewid == reviewId && x.IsLike == true);

            return Json(new
            {
                success = true,
                likeCount = likeCount,
                isLiked = isLiked
            });
        }

        public async Task<IActionResult> Reviews(int productId, int page = 1, int pageSize = 5)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            int userId = string.IsNullOrEmpty(userIdStr) ? 0 : int.Parse(userIdStr);

            var query = _context.ProductReviews
                .Where(r => r.Productid == productId);

            var totalCount = await query.CountAsync();

            var reviews = await query
                .Include(r => r.User)
                .OrderByDescending(r => r.Createdat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            var reviewsWithStatus = reviews.Select(r => new ProductReview
            {
                Reviewid = r.Reviewid,
                Productid = r.Productid,
                Reviewcontent = r.Reviewcontent,
                Rating = r.Rating,
                Createdat = r.Createdat,
                Userid = r.Userid,
                User = r.User,
                LikeCount = _context.ProductReviewLikes
                    .Count(l => l.Reviewid == r.Reviewid && (l.IsLike ?? false)),
                IsLikedByUser = _context.ProductReviewLikes
                    .Any(l => l.Reviewid == r.Reviewid && l.Userid == userId && (l.IsLike ?? false)),
                IsDislikedByUser = _context.ProductReviewLikes
                    .Any(l => l.Reviewid == r.Reviewid && l.Userid == userId && (l.IsLike.HasValue && !l.IsLike.Value))
            }).ToList();

            ViewBag.ProductId = productId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return PartialView("_ProductReviewList", reviewsWithStatus);
        }

        // POST: ProductReviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductReview review)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
            {
                string returnUrl = Url.Action("Detail", "Products",
                    new { id = review.Productid, tab = "comment" }) ?? "/";

                return RedirectToAction("Login", "Authentication", new { returnUrl });
            }

            int userId = int.Parse(userIdStr);

            //  KIỂM TRA ĐÃ REVIEW CHƯA
            var existingReview = await _context.ProductReviews
                .FirstOrDefaultAsync(r =>
                    r.Productid == review.Productid &&
                    r.Userid == userId);

            if (existingReview != null)
            {
                //  Nếu đã có thì UPDATE thay vì thêm mới
                existingReview.Reviewcontent = review.Reviewcontent;
                existingReview.Rating = review.Rating;   // nếu có cột Rating
                existingReview.Createdat = DateTime.Now;

                await _context.SaveChangesAsync();
            }
            else
            {
                // 👉 Nếu chưa có thì thêm mới
                review.Userid = userId;
                review.Createdat = DateTime.Now;

                _context.ProductReviews.Add(review);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Detail", "Products",
                new { id = review.Productid, tab = "comment" });
        }
    }
}
