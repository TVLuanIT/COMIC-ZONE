using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Models.Enums;
using COMICZONE.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
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
                return Json(new { success = false });

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
                if (existing.Islike == false)
                {
                    // đã dislike → bỏ dislike
                    _context.ProductReviewReplyLikes.Remove(existing);
                    isDisliked = false;
                }
                else
                {
                    // đang like → chuyển sang dislike
                    existing.Islike = false;
                    isDisliked = true;
                }
            }

            await _context.SaveChangesAsync();

            var likeCount = await _context.ProductReviewReplyLikes
                .CountAsync(x => x.Replyid == replyId && x.Islike == true);

            return Json(new
            {
                success = true,
                likeCount,
                isDisliked
            });
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

        private async Task UpdateProductReviewSummary(int productId)
        {
            var reviewsQuery = _context.ProductReviews
                .Where(r => r.Productid == productId);

            var total = await reviewsQuery.CountAsync();

            decimal average = 0;

            if (total > 0)
            {
                average = await reviewsQuery
                    .AverageAsync(r => (decimal)r.Rating);
            }

            var summary = await _context.ProductReviewSummaries
                .FirstOrDefaultAsync(s => s.Productid == productId);

            if (summary == null)
            {
                summary = new ProductReviewSummary
                {
                    Productid = productId,
                    Totalreview = total,
                    Averagerating = average,
                    Lastupdated = DateTime.Now
                };

                _context.ProductReviewSummaries.Add(summary);
            }
            else
            {
                summary.Totalreview = total;
                summary.Averagerating = average;
                summary.Lastupdated = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Report(ReportProductReviewRequest request)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return Json(new { success = false, message = "Bạn cần đăng nhập." });

            int userId = int.Parse(userIdStr);

            if (string.IsNullOrWhiteSpace(request.Reason))
                return Json(new { success = false, message = "Vui lòng nhập lý do." });

            if ((request.ReviewId == null && request.ReplyId == null) ||
                (request.ReviewId != null && request.ReplyId != null))
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            // Xác định loại report
            int reportType;
            int targetId;

            if (request.ReviewId.HasValue)
            {
                reportType = (int)ReportType.Review;
                targetId = request.ReviewId.Value;
            }
            else if (request.ReplyId.HasValue)
            {
                reportType = (int)ReportType.Reply;
                targetId = request.ReplyId.Value;
            }
            else
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            // Check đã report chưa
            bool alreadyReported = _context.ViolationReports.Any(r =>
                r.Userid == userId &&
                r.Reporttype == reportType &&
                r.Targetid == targetId
            );

            if (alreadyReported)
                return Json(new { success = false, message = "Bạn đã báo cáo nội dung này rồi." });

            // Lưu vào bảng chuẩn
            var report = new ViolationReport
            {
                Userid = userId,
                Reporttype = reportType,
                Targetid = targetId,
                Reason = request.Reason,
                Createdat = DateTime.Now,
                Status = (int)ReportStatus.Pending,
                Isdeleted = false
            };

            _context.ViolationReports.Add(report);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReview(int reviewId, string reviewcontent)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
                return Json(new { success = false, message = "Bạn chưa đăng nhập." });

            int userId = int.Parse(userIdStr);

            var review = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.Reviewid == reviewId);

            if (review == null)
                return Json(new { success = false, message = "Không tìm thấy đánh giá." });

            if (review.Userid != userId)
                return Json(new { success = false, message = "Bạn không có quyền sửa." });

            review.Reviewcontent = reviewcontent;
            review.Updatedat = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                updatedAt = review.Updatedat?.ToString("dd/MM/yyyy HH:mm")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReply(int replyId, string content)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
                return Json(new { success = false, message = "Bạn chưa đăng nhập." });

            int userId = int.Parse(userIdStr);

            var reply = await _context.ProductReviewReplies
                .FirstOrDefaultAsync(r => r.Replyid == replyId);

            if (reply == null)
                return Json(new { success = false, message = "Không tìm thấy phản hồi." });

            if (reply.Userid != userId)
                return Json(new { success = false, message = "Bạn không có quyền sửa." });

            reply.Replycontent = content;
            reply.Updatedat = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                updatedAt = reply.Updatedat?.ToString("dd/MM/yyyy HH:mm")
            });
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
                Updatedat = r.Updatedat,
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
                Updatedat = r.Updatedat,
                Userid = r.Userid,
                User = r.User,
                ProductReviewReplies = r.ProductReviewReplies.Select(reply => new ProductReviewReply
                {
                    Replyid = reply.Replyid,
                    Reviewid = reply.Reviewid,
                    Replycontent = reply.Replycontent,
                    Createdat = reply.Createdat,
                    Updatedat = reply.Updatedat,
                    Userid = reply.Userid,
                    User = reply.User,
                    Replytouserid = reply.Replytouserid,
                    Replytouser = reply.Replytouser,

                    LikeCount = _context.ProductReviewReplyLikes.Count(l => l.Replyid == reply.Replyid && l.Islike == true),
                    IsLikedByUser = _context.ProductReviewReplyLikes.Any(l => l.Replyid == reply.Replyid && l.Userid == userId && l.Islike == true),
                    IsDislikedByUser = _context.ProductReviewReplyLikes.Any(l => l.Replyid == reply.Replyid && l.Userid == userId && l.Islike == false),

                }).ToList(),

                LikeCount = _context.ProductReviewLikes.Count(l => l.Reviewid == r.Reviewid && (l.IsLike ?? false)),
                IsLikedByUser = _context.ProductReviewLikes.Any(l => l.Reviewid == r.Reviewid && l.Userid == userId && (l.IsLike ?? false)),
                IsDislikedByUser = _context.ProductReviewLikes.Any(l => l.Reviewid == r.Reviewid && l.Userid == userId && (l.IsLike.HasValue && !l.IsLike.Value)),

            }).ToList();

            ViewBag.ProductId = productId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return PartialView("_ProductReviewList", reviewsWithStatus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReply(int replyId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole") ?? "User";
            int currentUserId = string.IsNullOrEmpty(userIdStr) ? 0 : int.Parse(userIdStr);

            var reply = await _context.ProductReviewReplies
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Replyid == replyId);

            if (reply == null)
                return Json(new { success = false, message = "Không tìm thấy phản hồi" });

            if (reply.Userid != currentUserId && userRole != "Admin")
                return Json(new { success = false, message = "Bạn không có quyền xóa phản hồi này" });

            try
            {
                // Xóa like/dislike liên quan
                var likes = _context.ProductReviewReplyLikes.Where(l => l.Replyid == replyId);
                _context.ProductReviewReplyLikes.RemoveRange(likes);

                // Xóa báo cáo liên quan
                // Xóa report liên quan đến reply
                var reports = _context.ViolationReports
                    .Where(r =>
                        r.Reporttype == (int)ReportType.Reply &&
                        r.Targetid == replyId
                    );

                _context.ViolationReports.RemoveRange(reports);

                // Cuối cùng xóa reply
                _context.ProductReviewReplies.Remove(reply);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.Reviewid == id);

            if (review == null)
                return Json(new { success = false });

            // Lưu lại productId trước khi xóa
            int productId = review.Productid;

            try
            {
                // Xóa report của reply
                var replyReports = _context.ViolationReports
                    .Where(r =>
                        r.Reporttype == (int)ReportType.Reply &&
                        _context.ProductReviewReplies
                            .Where(rep => rep.Reviewid == id)
                            .Select(rep => rep.Replyid)
                            .Contains(r.Targetid)
                    );

                _context.ViolationReports.RemoveRange(replyReports);

                // Xóa like của reply
                var replyLikes = _context.ProductReviewReplyLikes
                    .Where(l => l.Reply.Reviewid == id);

                _context.ProductReviewReplyLikes.RemoveRange(replyLikes);

                // Xóa reply
                var replies = _context.ProductReviewReplies
                    .Where(r => r.Reviewid == id);

                _context.ProductReviewReplies.RemoveRange(replies);

                // Xóa report của review
                var reviewReports = _context.ViolationReports
                    .Where(r =>
                        r.Reporttype == (int)ReportType.Review &&
                        r.Targetid == id
                    );

                _context.ViolationReports.RemoveRange(reviewReports);

                // Xóa like của review
                var reviewLikes = _context.ProductReviewLikes
                    .Where(l => l.Reviewid == id);

                _context.ProductReviewLikes.RemoveRange(reviewLikes);

                // Xóa review
                _context.ProductReviews.Remove(review);

                await _context.SaveChangesAsync();

                // Cập nhật summary SAU khi xóa xong
                await UpdateProductReviewSummary(productId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddReply([FromBody] ReplyRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

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
                Replytouserid = model.ReplyToUserId,  // gán ID người được reply
                Parentreplyid = model.ParentReplyId
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
                replytouserUsername = replyToUser?.Username, // trả về để JS hiển thị @username
                reviewId = reply.Reviewid
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
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Bạn chưa đăng nhập." });
                }

                string returnUrl = Url.Action("Detail", "Products",
                    new { id = review.Productid, tab = "comment" }) ?? "/";

                return RedirectToAction("Login", "Authentication", new { returnUrl });
            }

            int userId = int.Parse(userIdStr);

            // Trim nội dung trước
            if (review.Reviewcontent != null)
            {
                review.Reviewcontent = review.Reviewcontent.Trim();
            }

            if (string.IsNullOrWhiteSpace(review.Reviewcontent))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Nội dung đánh giá không được để trống." });
                }

                TempData["ReviewError"] = "Nội dung đánh giá không được để trống.";
                return RedirectToAction("Detail", "Products",
                    new { id = review.Productid, tab = "comment" });
            }

            // Kiểm tra đã review chưa
            var existingReview = await _context.ProductReviews
                .FirstOrDefaultAsync(r =>
                    r.Productid == review.Productid &&
                    r.Userid == userId);

            if (existingReview != null)
            {
                existingReview.Reviewcontent = review.Reviewcontent;

                if (review.Rating >= 1 && review.Rating <= 5)
                    existingReview.Rating = review.Rating;

                // Không thay Createdat
                existingReview.Updatedat = DateTime.Now; // <-- cập nhật thời gian chỉnh sửa

                await _context.SaveChangesAsync();
            }
            else
            {
                review.Userid = userId;
                review.Createdat = DateTime.Now;
                review.Updatedat = null; // review mới thì chưa chỉnh sửa

                _context.ProductReviews.Add(review);
                await _context.SaveChangesAsync();
            }

            // UPDATE SUMMARY SAU KHI DB ĐÃ THAY ĐỔI
            await UpdateProductReviewSummary(review.Productid);

            // Nếu AJAX request → trả JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, updatedContent = review.Reviewcontent });
            }

            return RedirectToAction("Detail", "Products",
                new { id = review.Productid, tab = "comment" });
        }
    }
}
