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

        public class ReplyRequest
        {
            public int ReviewId { get; set; }
            public string Content { get; set; } = "";
            public int? ReplyToUserId { get; set; } // THÊM thuộc tính reply tới ai
        }

        [HttpPost]
        public async Task<IActionResult> ToggleReplyLike(int replyId)
        {
            string? userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return Json(new { success = false, message = "Chưa đăng nhập" });

            int userId = int.Parse(userIdStr);

            var existing = await _context.ProductReviewReplyLikes
                .FirstOrDefaultAsync(x => x.Replyid == replyId && x.Userid == userId);

            bool isLiked;

            if (existing == null)
            {
                _context.ProductReviewReplyLikes.Add(new ProductReviewReplyLike
                {
                    Replyid = replyId,
                    Userid = userId,
                    Createdat = DateTime.Now,
                    Islike = true
                });
                isLiked = true;
            }
            else
            {
                if (existing.Islike == true)
                {
                    // Nếu đã like → bỏ like
                    _context.ProductReviewReplyLikes.Remove(existing);
                    isLiked = false;
                }
                else
                {
                    // Nếu đang dislike → chuyển sang like
                    existing.Islike = true;
                    isLiked = true;
                }
            }

            await _context.SaveChangesAsync();

            var likeCount = await _context.ProductReviewReplyLikes
                .CountAsync(x => x.Replyid == replyId && x.Islike == true);

            return Json(new
            {
                success = true,
                likeCount = likeCount,
                isLiked = isLiked
            });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleReplyDislike(int replyId)
        {
            string? userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return Json(new { success = false, message = "Chưa đăng nhập" });

            int userId = int.Parse(userIdStr);

            var existing = await _context.ProductReviewReplyLikes
                .FirstOrDefaultAsync(x => x.Replyid == replyId && x.Userid == userId);

            bool isDisliked;

            if (existing == null)
            {
                _context.ProductReviewReplyLikes.Add(new ProductReviewReplyLike
                {
                    Replyid = replyId,
                    Userid = userId,
                    Createdat = DateTime.Now,
                    Islike = false
                });
                isDisliked = true;
            }
            else
            {
                // nếu trước đó like thì đổi sang dislike, nếu dislike thì bỏ dislike
                isDisliked = existing.Islike == false ? false : true;
                existing.Islike = isDisliked ? false : (bool?)null;
            }

            await _context.SaveChangesAsync();

            var likeCount = await _context.ProductReviewReplyLikes
                .CountAsync(x => x.Replyid == replyId && x.Islike == true);

            return Json(new { success = true, likeCount = likeCount, isDisliked = isDisliked });
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

        [HttpPost]
        public async Task<IActionResult> Report(int? reviewId, int? replyId, string reason)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            // Kiểm tra đã báo cáo chưa
            bool exists = await _context.ProductReviewReports
                .AnyAsync(r => r.Reviewid == reviewId && r.Replyid == replyId && r.Userid == userId);
            if (exists)
                return Json(new { success = false, message = "Bạn đã báo cáo trước đó." });

            var report = new ProductReviewReport
            {
                Reviewid = reviewId,
                Replyid = replyId,
                Userid = userId,
                Reason = reason,
                Status = "PENDING",
                Createdat = DateTime.Now
            };

            _context.ProductReviewReports.Add(report);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Báo cáo thành công!" });
        }

        // GET: /ProductReviews/Replies?reviewId=19
        public async Task<IActionResult> Replies(int reviewId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            int currentUserId = string.IsNullOrEmpty(userIdStr) ? 0 : int.Parse(userIdStr);

            var replies = await _context.ProductReviewReplies
                .Where(r => r.Reviewid == reviewId)
                .Include(r => r.User)
                .Include(r => r.Replytouser)
                .OrderBy(r => r.Createdat)
                .AsNoTracking()
                .ToListAsync();

            // Thêm các thuộc tính like/dislike
            var repliesWithStatus = replies.Select(r => new ProductReviewReply
            {
                Replyid = r.Replyid,
                Reviewid = r.Reviewid,
                Replycontent = r.Replycontent,
                Createdat = r.Createdat,
                Userid = r.Userid,
                User = r.User,
                Replytouserid = r.Replytouserid,
                Replytouser = r.Replytouser,
                LikeCount = _context.ProductReviewReplyLikes.Count(l => l.Replyid == r.Replyid && l.Islike == true),
                IsLikedByUser = _context.ProductReviewReplyLikes.Any(l => l.Replyid == r.Replyid && l.Userid == currentUserId && l.Islike == true),
                IsDislikedByUser = _context.ProductReviewReplyLikes.Any(l => l.Replyid == r.Replyid && l.Userid == currentUserId && l.Islike == false)
            }).ToList();

            return PartialView("_ProductReviewReplies", repliesWithStatus);
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
                .Include(r => r.ProductReviewReplies)               // ← thêm dòng này
                    .ThenInclude(reply => reply.User)
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
                ProductReviewReplies = r.ProductReviewReplies.Select(reply => new ProductReviewReply
                {
                    Replyid = reply.Replyid,
                    Reviewid = reply.Reviewid,
                    Replycontent = reply.Replycontent,
                    Createdat = reply.Createdat,
                    Userid = reply.Userid,
                    User = reply.User,
                    Replytouserid = reply.Replytouserid,
                    Replytouser = reply.Replytouser,

                    LikeCount = _context.ProductReviewReplyLikes
                        .Count(l => l.Replyid == reply.Replyid && l.Islike == true),

                                    IsLikedByUser = _context.ProductReviewReplyLikes
                        .Any(l => l.Replyid == reply.Replyid
                               && l.Userid == userId
                               && l.Islike == true),

                                    IsDislikedByUser = _context.ProductReviewReplyLikes
                        .Any(l => l.Replyid == reply.Replyid
                               && l.Userid == userId
                               && l.Islike == false)
                                }).ToList(),
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

        [HttpPost]
        public async Task<IActionResult> AddReply([FromBody] ReplyRequest model)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var review = await _context.ProductReviews.FindAsync(model.ReviewId);
            if (review == null) return NotFound();

            var reply = new ProductReviewReply
            {
                Reviewid = model.ReviewId,
                Userid = userId,
                Replycontent = model.Content,
                Createdat = DateTime.Now,
                Replytouserid = model.ReplyToUserId  // gán ID người được reply
            };

            _context.ProductReviewReplies.Add(reply);
            await _context.SaveChangesAsync();

            // Lấy thông tin user gửi và user được reply (nếu có)
            var user = await _context.Users.FindAsync(userId);
            User? replyToUser = null;
            if (model.ReplyToUserId.HasValue)
            {
                replyToUser = reply.Replytouserid.HasValue
                    ? await _context.Users.FindAsync(reply.Replytouserid.Value)
                    : null;
            }

            return Json(new
            {
                success = true,
                username = user?.Username ?? "Người dùng ẩn danh",
                content = reply.Replycontent,
                replytouserUsername = replyToUser?.Username // trả về để JS hiển thị @username
            });
        }

        // POST: ProductReviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(ProductReview review)
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
                //  Nếu chưa có thì thêm mới
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
